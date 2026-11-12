using System.Globalization;
using MoonDiary.Presentation.Services;

namespace MoonDiary.Services;

public sealed class ErrorLog : IErrorLog
{
    private const long MaxSize = 256 * 1024;

    private readonly TimeProvider timeProvider;
    private readonly Lock gate = new();

    public ErrorLog(TimeProvider timeProvider)
    {
        this.timeProvider = timeProvider;
    }

    public static string FilePath => Path.Combine(FileSystem.AppDataDirectory, "errors.log");

    public void Write(string source, Exception exception)
    {
        try
        {
            lock (gate)
            {
                var file = new FileInfo(FilePath);

                if (file.Exists && file.Length > MaxSize)
                {
                    file.Delete();
                }

                var stamp = timeProvider.GetUtcNow().ToString("O", CultureInfo.InvariantCulture);
                File.AppendAllText(FilePath, $"{stamp} {source}: {exception}{Environment.NewLine}");
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }

        System.Diagnostics.Debug.WriteLine($"{source}: {exception}");
    }
}
