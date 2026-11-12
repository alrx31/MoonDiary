namespace MoonDiary.Presentation.Services;

public interface IFileService
{
    Task<bool> SaveTextAsync(string fileName, string content);

    Task<string?> PickTextAsync();

    Task<string> SaveBackupAsync(string fileName, string content);
}
