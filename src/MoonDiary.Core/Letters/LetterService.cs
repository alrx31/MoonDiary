using MoonDiary.Core.Abstractions;
using MoonDiary.Core.Domain;
using MoonDiary.Core.Entries;
using MoonDiary.Core.Time;

namespace MoonDiary.Core.Letters;

public sealed class LetterService
{
    public const int PreviewLength = 40;

    public static readonly TimeOnly DefaultOpenTime = new(10, 0);

    private readonly ILetterRepository letters;
    private readonly DayClock clock;

    public LetterService(ILetterRepository letters, DayClock clock)
    {
        this.letters = letters;
        this.clock = clock;
    }

    public DateOnly MinOpenDate => clock.CurrentDayKey.AddDays(1);

    public async Task<Letter> CreateAsync(string text, DateOnly openDate, TimeOnly? openTime = null)
    {
        var trimmed = (text ?? string.Empty).Trim();

        if (trimmed.Length == 0)
        {
            throw new DomainException("Напишите текст письма");
        }

        if (trimmed.Length > TextLimits.LetterText)
        {
            throw new DomainException($"Письмо не длиннее {TextLimits.LetterText} символов");
        }

        if (openDate < MinOpenDate)
        {
            throw new DomainException("Дата открытия — не раньше завтрашнего дня");
        }

        var letter = new Letter
        {
            Id = Guid.NewGuid(),
            Text = trimmed,
            CreatedAtUtc = clock.UtcNow,
            OpenDate = openDate,
            OpenTime = openTime ?? DefaultOpenTime
        };

        await letters.SaveAsync(letter);

        return letter;
    }

    public async Task<IReadOnlyList<Letter>> OpenDueAsync()
    {
        var now = clock.LocalNow.DateTime;
        var opened = new List<Letter>();

        foreach (var letter in await letters.GetAllAsync())
        {
            if (letter.OpenedAtUtc is null && IsOpen(letter, now))
            {
                letter.OpenedAtUtc = clock.UtcNow;
                await letters.SaveAsync(letter);
                opened.Add(letter);
            }
        }

        return opened;
    }

    public async Task<IReadOnlyList<LetterListItem>> GetListAsync()
    {
        var now = clock.LocalNow.DateTime;
        var all = (await letters.GetAllAsync())
            .Select(letter => (Letter: letter, IsOpen: IsOpen(letter, now)))
            .ToList();
        var closed = all.Where(item => !item.IsOpen).OrderBy(item => OpenMoment(item.Letter));
        var open = all.Where(item => item.IsOpen).OrderByDescending(item => OpenMoment(item.Letter));

        return closed.Concat(open)
            .Select(item => new LetterListItem(
                item.Letter.Id,
                item.Letter.OpenDate,
                item.Letter.OpenTime,
                item.IsOpen,
                item.IsOpen ? Preview(item.Letter.Text) : null))
            .ToList();
    }

    public async Task<Letter> ReadAsync(Guid id)
    {
        var letter = await letters.GetByIdAsync(id) ?? throw new DomainException("Письмо не найдено");

        if (!IsOpen(letter, clock.LocalNow.DateTime))
        {
            throw new DomainException("Письмо ещё закрыто");
        }

        if (letter.OpenedAtUtc is null)
        {
            letter.OpenedAtUtc = clock.UtcNow;
            await letters.SaveAsync(letter);
        }

        return letter;
    }

    public Task DeleteAsync(Guid id) => letters.DeleteAsync(id);

    public async Task<IReadOnlyList<LetterNotification>> GetPendingNotificationsAsync()
    {
        var zone = clock.LocalNow.Offset;

        return (await letters.GetAllAsync())
            .Where(letter => !letter.Notified)
            .Select(letter => new LetterNotification(
                letter.Id,
                new DateTimeOffset(OpenMoment(letter), zone)))
            .ToList();
    }

    public async Task MarkNotifiedAsync(Guid id)
    {
        var letter = await letters.GetByIdAsync(id);

        if (letter is null || letter.Notified)
        {
            return;
        }

        letter.Notified = true;
        await letters.SaveAsync(letter);
    }

    private static DateTime OpenMoment(Letter letter) => letter.OpenDate.ToDateTime(letter.OpenTime);

    private static bool IsOpen(Letter letter, DateTime localNow) => OpenMoment(letter) <= localNow;

    private static string Preview(string text)
    {
        var line = text.ReplaceLineEndings(" ");

        return line.Length <= PreviewLength ? line : line[..PreviewLength];
    }
}
