using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoonDiary.Core.Abstractions;
using MoonDiary.Core.Domain;
using MoonDiary.Core.Transfer;
using MoonDiary.Presentation.Services;

namespace MoonDiary.Presentation.ViewModels;

public sealed partial class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsRepository settings;
    private readonly DataTransferService transfer;
    private readonly IThemeService theme;
    private readonly INotificationService notifications;
    private readonly IFileService files;
    private readonly IBackupSettings backup;
    private readonly ReminderScheduler scheduler;
    private bool loading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLight), nameof(IsDark), nameof(IsAuto))]
    private ThemeMode themeMode = ThemeMode.Dark;

    [ObservableProperty]
    private bool reminderEnabled;

    [ObservableProperty]
    private TimeSpan? reminderTime;

    [ObservableProperty]
    private bool adaptiveReminder;

    [ObservableProperty]
    private bool notificationsDenied;

    public SettingsViewModel(
        ViewModelContext context,
        ISettingsRepository settings,
        DataTransferService transfer,
        IThemeService theme,
        INotificationService notifications,
        IFileService files,
        IBackupSettings backup,
        ReminderScheduler scheduler)
        : base(context)
    {
        this.settings = settings;
        this.transfer = transfer;
        this.theme = theme;
        this.notifications = notifications;
        this.files = files;
        this.backup = backup;
        this.scheduler = scheduler;
    }

    public bool IsLight => ThemeMode == ThemeMode.Light;

    public bool IsDark => ThemeMode == ThemeMode.Dark;

    public bool IsAuto => ThemeMode == ThemeMode.Auto;

    public string BackupStatus => backup.IsAutoBackupSupported ? Strings.Settings_BackupOn : Strings.Settings_BackupOff;

    public override async Task RefreshAsync()
    {
        try
        {
            var current = await settings.GetAsync();

            loading = true;
            ThemeMode = current.Theme;
            ReminderEnabled = current.ReminderEnabled;
            ReminderTime = current.ReminderTime.ToTimeSpan();
            AdaptiveReminder = current.AdaptiveReminder;
            loading = false;
            NotificationsDenied = !await notifications.IsPermissionGrantedAsync();
        }
#pragma warning disable CA1031
        catch (Exception exception)
#pragma warning restore CA1031
        {
            loading = false;
            ErrorLog.Write(nameof(SettingsViewModel), exception);
        }
    }

    [RelayCommand]
    private void SelectTheme(string mode) => ThemeMode = Enum.Parse<ThemeMode>(mode);

    [RelayCommand]
    private void OpenNotificationSettings() => notifications.OpenNotificationSettings();

    [RelayCommand]
    private void OpenBackupSettings() => backup.OpenBackupSettings();

    [RelayCommand]
    private async Task ExportAsync()
    {
        var saved = false;

        await RunAsync(async () =>
            saved = await files.SaveTextAsync(transfer.SuggestFileName(), await transfer.ExportJsonAsync()));

        if (saved)
        {
            await Dialogs.AlertAsync(Strings.Settings_Export, Strings.Settings_ExportDone);
        }
    }

    [RelayCommand]
    private async Task ImportAsync()
    {
        if (!await Dialogs.ConfirmAsync(
                Strings.Settings_Import,
                Strings.Settings_ImportConfirm,
                Strings.Settings_ImportButton,
                Strings.Common_Cancel))
        {
            return;
        }

        string? backupPath = null;
        var imported = await RunAsync(async () =>
        {
            var json = await files.PickTextAsync();

            if (json is null)
            {
                return;
            }

            await transfer.ImportJsonAsync(json, async (name, content) =>
                backupPath = await files.SaveBackupAsync(name, content));
        });

        if (!imported || backupPath is null)
        {
            return;
        }

        await RefreshAsync();
        theme.Apply(ThemeMode);
        await scheduler.RefreshAsync();
        await Dialogs.AlertAsync(
            Strings.Settings_Import,
            string.Format(CultureInfo.InvariantCulture, Strings.Settings_ImportDone, backupPath));
    }

    [RelayCommand]
    private Task BackAsync() => Navigation.GoBackAsync();

    partial void OnThemeModeChanged(ThemeMode value)
    {
        if (loading)
        {
            return;
        }

        theme.Apply(value);
        _ = SaveAsync();
    }

    partial void OnReminderEnabledChanged(bool value) => _ = SaveAsync();

    partial void OnReminderTimeChanged(TimeSpan? value) => _ = SaveAsync();

    partial void OnAdaptiveReminderChanged(bool value) => _ = SaveAsync();

    private async Task SaveAsync()
    {
        if (loading)
        {
            return;
        }

        try
        {
            var current = await settings.GetAsync();

            current.Theme = ThemeMode;
            current.ReminderEnabled = ReminderEnabled;
            current.ReminderTime = ReminderTime is { } time ? TimeOnly.FromTimeSpan(time) : current.ReminderTime;
            current.AdaptiveReminder = AdaptiveReminder;
            await settings.SaveAsync(current);
            await scheduler.RefreshAsync();
        }
#pragma warning disable CA1031
        catch (Exception exception)
#pragma warning restore CA1031
        {
            ErrorLog.Write(nameof(SettingsViewModel), exception);
            await Dialogs.AlertAsync(Strings.Common_Error, Strings.Common_SaveFailed);
        }
    }
}
