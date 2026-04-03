using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentLoanTracker.Core;
using StudentLoanTracker.App.Services;

namespace StudentLoanTracker.App.ViewModels;

/// <summary>
/// Binds the add/edit form to a single <see cref="Loan"/> instance. On save, fields are copied back into
/// that entity, validated with <see cref="LoanValidator"/>, then persisted via <see cref="ILoanRepository"/>.
/// New loans are detected when name is empty and principal is zero (see constructor).
/// </summary>
public partial class LoanDetailViewModel : ViewModelBase
{
    private readonly INavigationService _navigation;
    private readonly ILoanRepository _loanRepository;
    private readonly Loan _loan;
    private readonly bool _isNew;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _servicer = string.Empty;

    [ObservableProperty]
    private decimal _principal;

    [ObservableProperty]
    private decimal _interestRate;

    [ObservableProperty]
    private string _compounding = "Monthly";

    [ObservableProperty]
    private decimal _minimumPayment;

    [ObservableProperty]
    private DateTime _startDate;

    [ObservableProperty]
    private int _termMonths;

    [ObservableProperty]
    private bool _isFederal;

    [ObservableProperty]
    private string? _notes;

    [ObservableProperty]
    private string? _validationMessage;

    public string Title => _isNew ? "Add Loan" : "Edit Loan";

    public LoanDetailViewModel(INavigationService navigation, ILoanRepository loanRepository, Loan loan)
    {
        _navigation = navigation;
        _loanRepository = loanRepository;
        _loan = loan;
        _isNew = loan.Name == string.Empty && loan.Principal == 0;
        Name = loan.Name;
        Servicer = loan.Servicer;
        Principal = loan.Principal;
        InterestRate = loan.InterestRate;
        Compounding = loan.Compounding;
        MinimumPayment = loan.MinimumPayment;
        StartDate = loan.StartDate;
        TermMonths = loan.TermMonths;
        IsFederal = loan.IsFederal;
        Notes = loan.Notes;
    }

    /// <summary>Writes bound fields into the loan, validates, then INSERT or UPDATE in SQLite.</summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        ValidationMessage = null;
        _loan.Name = Name;
        _loan.Servicer = Servicer;
        _loan.Principal = Principal;
        _loan.InterestRate = InterestRate;
        _loan.Compounding = Compounding;
        _loan.MinimumPayment = MinimumPayment;
        _loan.StartDate = StartDate;
        _loan.TermMonths = TermMonths;
        _loan.IsFederal = IsFederal;
        _loan.Notes = Notes;

        if (!LoanValidator.IsValid(_loan, out var errors))
        {
            ValidationMessage = string.Join(" ", errors);
            return;
        }

        try
        {
            if (_isNew)
                await _loanRepository.AddAsync(_loan).ConfigureAwait(true);
            else
                await _loanRepository.UpdateAsync(_loan).ConfigureAwait(true);
            _navigation.NavigateToLoanList();
        }
        catch (Exception ex)
        {
            ValidationMessage = "Error saving: " + ex.Message;
        }
    }

    /// <summary>Discards in-memory edits and returns to the list (no database write).</summary>
    [RelayCommand]
    private void Cancel()
    {
        _navigation.NavigateToLoanList();
    }
}
