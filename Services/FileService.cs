using System.Text;
using MoonDiary.Presentation.Services;

namespace MoonDiary.Services;

public sealed class FileService : IFileService
{
    private static readonly FilePickerFileType JsonFiles = new(new Dictionary<DevicePlatform, IEnumerable<string>>
    {
        [DevicePlatform.Android] = ["application/json", "text/plain", "application/octet-stream"],
        [DevicePlatform.MacCatalyst] = ["public.json", "public.text"],
        [DevicePlatform.iOS] = ["public.json", "public.text"],
        [DevicePlatform.WinUI] = [".json"]
    });

    public async Task<bool> SaveTextAsync(string fileName, string content)
    {
#if ANDROID
        return await DocumentSaver.SaveAsync(fileName, content);
#else
        var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        await File.WriteAllTextAsync(Path.Combine(folder, fileName), content, Encoding.UTF8);

        return true;
#endif
    }

    public async Task<string?> PickTextAsync()
    {
        var file = await MainThread.InvokeOnMainThreadAsync(() =>
            FilePicker.Default.PickAsync(new PickOptions { FileTypes = JsonFiles }));

        if (file is null)
        {
            return null;
        }

        await using var stream = await file.OpenReadAsync();
        using var reader = new StreamReader(stream, Encoding.UTF8);

        return await reader.ReadToEndAsync();
    }

    public async Task<string> SaveBackupAsync(string fileName, string content)
    {
        var folder = Path.Combine(FileSystem.AppDataDirectory, "backups");
        var path = Path.Combine(folder, fileName);

        Directory.CreateDirectory(folder);
        await File.WriteAllTextAsync(path, content, Encoding.UTF8);

        return path;
    }
}
