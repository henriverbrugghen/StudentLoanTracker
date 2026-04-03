using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentLoanTracker.Core;
using StudentLoanTracker.App.Services;

namespace StudentLoanTracker.App.ViewModels;

/// <summary>
/// Presents every saved loan in a grid and routes navigation to detail, schedule, and charts screens.
/// The list is loaded from <see cref="ILoanRepository"/>; commands that need a row use <see cref="SelectedLoan"/>.
/// </summary>
public partial class LoanListViewModel : ViewModelBase
{
    private readonly INavigationService _navigation;
    private readonly ILoanRepository _loanRepository;
    private readonly ILoanCalculator _loanCalculator;

    public ObservableCollection<Loan> Loans { get; } = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditLoanCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteLoanCommand))]
    [NotifyCanExecuteChangedFor(nameof(ViewScheduleCommand))]
    [NotifyCanExecuteChangedFor(nameof(ViewChartsCommand))]
    private Loan? _selectedLoan;

    /// <summary>User-facing status text (errors, or short confirmations like after delete).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasStatusMessage))]
    private string? _statusMessage;

    /// <summary>True when at least one loan exists — controls DataGrid vs empty-state panel.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowEmptyState))]
    private bool _hasLoans;

    /// <summary>Inverse of <see cref="HasLoans"/> for binding the welcome card without a value converter.</summary>
    public bool ShowEmptyState => !HasLoans;

    /// <summary>Whether to show the status line under the grid.</summary>
    public bool HasStatusMessage => !string.IsNullOrWhiteSpace(StatusMessage);

    public LoanListViewModel(INavigationService navigation, ILoanRepository loanRepository, ILoanCalculator loanCalculator)
    {
        _navigation = navigation;
        _loanRepository = loanRepository;
        _loanCalculator = loanCalculator;
        _ = LoadLoansAsync();
    }

    [RelayCommand(CanExecute = nameof(HasSelectedLoan))]
    private void EditLoan()
    {
        if (SelectedLoan == null) return;
        _navigation.NavigateTo(new LoanDetailViewModel(_navigation, _loanRepository, SelectedLoan));
    }

    [RelayCommand(CanExecute = nameof(HasSelectedLoan))]
    private async Task DeleteLoanAsync()
    {
        if (SelectedLoan == null) return;
        await _loanRepository.DeleteAsync(SelectedLoan.Id).ConfigureAwait(true);
        StatusMessage = "That loan was removed from your list.";
        await LoadLoansAsync().ConfigureAwait(true);
    }

    [RelayCommand]
    private void AddLoan()
    {
        var newLoan = new Loan
        {
            Id = Guid.NewGuid(),
            StartDate = DateTime.Today,
            TermMonths = 120,
            Compounding = "Monthly"
        };
        _navigation.NavigateTo(new LoanDetailViewModel(_navigation, _loanRepository, newLoan));
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadLoansAsync().ConfigureAwait(true);
    }

    [RelayCommand(CanExecute = nameof(HasSelectedLoan))]
    private void ViewSchedule()
    {
        if (SelectedLoan == null) return;
        _navigation.NavigateTo(new ScheduleViewModel(_navigation, _loanCalculator, SelectedLoan));
    }

    [RelayCommand(CanExecute = nameof(HasSelectedLoan))]
    private void ViewCharts()
    {
        if (SelectedLoan == null) return;
        _navigation.NavigateTo(new ChartsViewModel(_navigation, _loanCalculator, SelectedLoan));
    }

    private bool HasSelectedLoan() => SelectedLoan != null;

    /// <summary>Reloads all loans from SQLite and refreshes <see cref="HasLoans"/> for the UI.</summary>
    public async Task LoadLoansAsync()
    {
        try
        {
            var list = await _loanRepository.GetAllAsync().ConfigureAwait(true);
            Loans.Clear();
            foreach (var loan in list)
                Loans.Add(loan);
            HasLoans = Loans.Count > 0;
            StatusMessage = null;
        }
        catch (Exception ex)
        {
            HasLoans = Loans.Count > 0;
            StatusMessage = "We couldn't load your loans. " + ex.Message;
        }
    }
}
