using MoonDiary.Presentation.Services;

namespace MoonDiary.Services;

public sealed class DefaultBackupSettings : IBackupSettings
{
    public bool IsAutoBackupSupported => false;

    public void OpenBackupSettings()
    {
    }
}
