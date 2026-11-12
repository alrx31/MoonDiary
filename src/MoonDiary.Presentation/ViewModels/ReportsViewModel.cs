using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoonDiary.Core.Reports;
using MoonDiary.Presentation.Navigation;

namespace MoonDiary.Presentation.ViewModels;

public sealed partial class ReportsViewModel : ViewModelBase
{
    private readonly WeeklyReportService reports;

    [ObservableProperty]
    private IReadOnlyList<ReportRowViewModel> items = [];

    [ObservableProperty]
    private bool isEmpty;

    public ReportsViewModel(ViewModelContext context, WeeklyReportService reports)
        : base(context)
    {
        this.reports = reports;
    }

    public override async Task RefreshAsync()
    {
        Items = (await reports.GetHistoryAsync()).Select(report => new ReportRowViewModel(report)).ToList();
        IsEmpty = Items.Count == 0;
    }

    [RelayCommand]
    private Task OpenAsync(ReportRowViewModel row) =>
        Navigation.PushAsync(Routes.Report, NavParams.Of((NavParams.ReportId, row.Id)));

    [RelayCommand]
    private Task BackAsync() => Navigation.GoBackAsync();
}
