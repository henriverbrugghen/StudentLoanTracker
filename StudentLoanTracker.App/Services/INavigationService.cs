namespace StudentLoanTracker.App.Services;

/// <summary>
/// Abstraction for moving between full-screen views. Implemented by <see cref="MainWindowViewModel"/>,
/// which updates the window's content and (for the list) reloads data from storage.
/// </summary>
public interface INavigationService
{
    /// <summary>Display the given view model; the view locator resolves it to a control.</summary>
    void NavigateTo(object viewModel);

    /// <summary>Navigate to the loan list and refresh it from the database.</summary>
    void NavigateToLoanList();
}
