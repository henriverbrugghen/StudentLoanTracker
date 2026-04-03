using CommunityToolkit.Mvvm.ComponentModel;
using StudentLoanTracker.Core;
using StudentLoanTracker.App.Services;

namespace StudentLoanTracker.App.ViewModels;

/// <summary>
/// Root view model for the main window. The window hosts a single <see cref="ContentControl"/> whose
/// <c>Content</c> is whichever screen (view model) is current. This class also implements
/// <see cref="INavigationService"/> so child view models can switch screens without referencing the window.
/// <see cref="ViewLocator"/> maps each view model type to its matching Avalonia view (same name, "ViewModel" → "View").
/// </summary>
public partial class MainWindowViewModel : ViewModelBase, INavigationService
{
    /// <summary>The active screen; must be a <see cref="ViewModelBase"/> for the view locator.</summary>
    [ObservableProperty]
    private object? _currentViewModel;

    private readonly ILoanRepository _loanRepository;
    private readonly ILoanCalculator _loanCalculator;

    /// <summary>Reused list screen so navigating back does not lose scroll/selection unnecessarily.</summary>
    private LoanListViewModel? _loanListViewModel;

    public MainWindowViewModel(ILoanRepository loanRepository, ILoanCalculator loanCalculator)
    {
        _loanRepository = loanRepository;
        _loanCalculator = loanCalculator;
        _loanListViewModel = new LoanListViewModel(this, loanRepository, loanCalculator);
        CurrentViewModel = _loanListViewModel;
    }

    /// <summary>Show any view model (detail form, schedule, charts, etc.).</summary>
    public void NavigateTo(object viewModel)
    {
        CurrentViewModel = viewModel;
    }

    /// <summary>Return to the loan grid and refresh data from the database.</summary>
    public void NavigateToLoanList()
    {
        if (_loanListViewModel == null)
            _loanListViewModel = new LoanListViewModel(this, _loanRepository, _loanCalculator);
        CurrentViewModel = _loanListViewModel;
        _ = _loanListViewModel.LoadLoansAsync();
    }
}
