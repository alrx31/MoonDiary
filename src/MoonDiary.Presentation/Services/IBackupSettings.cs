namespace MoonDiary.Presentation.Services;

public interface IBackupSettings
{
    bool IsAutoBackupSupported { get; }

    void OpenBackupSettings();
}
