using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using StudentLoanTracker.Core;
using StudentLoanTracker.App.Services;

namespace StudentLoanTracker.App.ViewModels;

/// <summary>
/// Feeds LiveCharts from the same schedule as <see cref="ScheduleViewModel"/>: balance line series and
/// a pie of total principal vs interest. When <see cref="ExtraMonthlyPayment"/> changes, series are rebuilt.
/// </summary>
public partial class ChartsViewModel : ViewModelBase
{
    private readonly INavigationService _navigation;
    private readonly ILoanCalculator _loanCalculator;
    private readonly Loan _loan;

    [ObservableProperty]
    private string _loanName = string.Empty;

    [ObservableProperty]
    private decimal _extraMonthlyPayment;

    public ObservableCollection<ISeries> BalanceSeries { get; } = new();
    public ObservableCollection<ISeries> PrincipalInterestSeries { get; } = new();

    public decimal TotalPrincipal { get; private set; }
    public decimal TotalInterest { get; private set; }

    public ChartsViewModel(INavigationService navigation, ILoanCalculator loanCalculator, Loan loan)
    {
        _navigation = navigation;
        _loanCalculator = loanCalculator;
        _loan = loan;
        _loanName = loan.Name;
        BuildCharts();
    }

    [RelayCommand]
    private void Back()
    {
        _navigation.NavigateToLoanList();
    }

    partial void OnExtraMonthlyPaymentChanged(decimal value) => BuildCharts();

    /// <summary>
    /// Runs the calculator, then fills <see cref="BalanceSeries"/> (line) and <see cref="PrincipalInterestSeries"/> (pie).
    /// </summary>
    private void BuildCharts()
    {
        var schedule = _loanCalculator.BuildSchedule(_loan, ExtraMonthlyPayment > 0 ? ExtraMonthlyPayment : null).ToList();
        if (schedule.Count == 0)
            return;

        TotalPrincipal = schedule.Sum(e => e.PrincipalPortion);
        TotalInterest = schedule.Sum(e => e.InterestPortion);
        OnPropertyChanged(nameof(TotalPrincipal));
        OnPropertyChanged(nameof(TotalInterest));

        BalanceSeries.Clear();
        BalanceSeries.Add(new LineSeries<double>
        {
            Name = "Remaining balance",
            Values = schedule.Select(e => (double)e.RemainingBalance).ToArray(),
            Fill = null
        });

        PrincipalInterestSeries.Clear();
        PrincipalInterestSeries.Add(new PieSeries<double>
        {
            Name = "Principal",
            Values = new[] { (double)TotalPrincipal }
        });
        PrincipalInterestSeries.Add(new PieSeries<double>
        {
            Name = "Interest",
            Values = new[] { (double)TotalInterest }
        });
    }
}
