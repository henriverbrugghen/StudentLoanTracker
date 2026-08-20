================================================================================
  STUDENT LOAN TRACKER — INTRODUCTION AND PROJECT GUIDE
================================================================================

Welcome. This file is a first look at the Student Loan Tracker: what it is,
how it is built, how to clone and run it, and what each source file is for.


--------------------------------------------------------------------------------
WHAT THIS APP DOES
--------------------------------------------------------------------------------

Student Loan Tracker is a desktop app for keeping student loans in one place
and seeing how they pay down over time. You add loans (name, servicer,
principal, interest rate, term, dates, notes), then open a month-by-month
payment schedule or charts. You can try an extra monthly payment and see how
that changes payoff date and total interest.

It is a local Avalonia UI app on .NET 8. The window talks to view models
(MVVM). View models load and save loans through a repository. The Data
project stores them in a SQLite file on your machine. Schedules and charts
come from amortization math in LoanCalculator, not from the database.


--------------------------------------------------------------------------------
HOW TO CLONE AND RUN
--------------------------------------------------------------------------------

  Requirements
    • Git
    • .NET 8 SDK (https://dotnet.microsoft.com/download)
    • Windows is the usual target; Visual Studio 2022 (or later) is optional
      if you prefer an IDE over the command line.

  Clone
    git clone https://github.com/henriverbrugghen/StudentLoanTracker.git
    cd StudentLoanTracker

  Run from the command line
    dotnet restore
    dotnet run --project StudentLoanTracker.App

  Run from Visual Studio
    Open StudentLoanTracker.sln, set StudentLoanTracker.App as the startup
    project, then press F5 (or Ctrl+F5 to run without debugging).

  After it starts, loans are stored in a SQLite database under your user
  Application Data folder (created on first run by DatabaseInitializer).
  There is no web server and no sign-in.


--------------------------------------------------------------------------------
SOLUTION LAYOUT (THREE PROJECTS)
--------------------------------------------------------------------------------

  StudentLoanTracker.Core
    Domain types, validation, and the amortization calculator. No UI and no
    database code. The App and Data projects both depend on this library.

  StudentLoanTracker.Data
    SQLite access. Implements Core’s ILoanRepository so the UI never talks
    to SQL directly.

  StudentLoanTracker.App
    Avalonia UI (XAML views), view models, navigation, dependency injection,
    and the executable you actually run.

  StudentLoanTracker.sln
    Visual Studio / dotnet solution that ties the three projects together.


--------------------------------------------------------------------------------
QUICK “WHERE DO I LOOK?”
--------------------------------------------------------------------------------

  Change window title, header text, default size
    → StudentLoanTracker.App\Views\MainWindow.axaml

  Change loan list screen (buttons, grid columns, empty state)
    → Views\LoanListView.axaml
    → ViewModels\LoanListViewModel.cs (commands, loading, HasLoans, etc.)

  Change add/edit loan form (labels, fields, layout)
    → Views\LoanDetailView.axaml
    → ViewModels\LoanDetailViewModel.cs (save/cancel, binding properties)

  Change payment schedule table or “extra payment” behavior on that screen
    → Views\ScheduleView.axaml
    → ViewModels\ScheduleViewModel.cs
    → Core\LoanCalculator.cs (actual month-by-month math)

  Change charts (labels, layout) or how series are built
    → Views\ChartsView.axaml
    → ViewModels\ChartsViewModel.cs
    → Core\LoanCalculator.cs (same schedule data as the grid)

  Change validation rules (allowed rates, term limits, etc.)
    → Core\LoanValidator.cs

  Change how loans are stored or add columns / migrations
    → Data\DatabaseInitializer.cs (schema)
    → Data\SqliteLoanRepository.cs (SQL + mapping)
    → Core\Loan.cs (properties must match what you persist)

  Change how screens switch without opening the window type
    → ViewModels\MainWindowViewModel.cs (implements INavigationService)
    → Services\INavigationService.cs (interface)

  Register services or set database path at startup
    → App.axaml.cs

  Change fonts / global theme / ViewLocator registration
    → App.axaml

  Change how a ViewModel type finds its View (.axaml)
    → ViewLocator.cs


--------------------------------------------------------------------------------
FILE-BY-FILE REFERENCE
--------------------------------------------------------------------------------

ROOT
----
  StudentLoanTracker.sln
    Solution file that lists the three projects and their build configurations.
    Open this in Visual Studio, or pass it to `dotnet build` / `dotnet run`.

  READ_ME.md
    This introduction and map of the codebase.

  .gitignore / .gitattributes (if present)
    Git ignore rules and line-ending settings. They are not used at runtime.


STUDENTLOANTRACKER.CORE  (class library — business rules & types)
-----------------------------------------------------------------
  StudentLoanTracker.Core.csproj
    Project file for the Core library. It targets .NET 8 and enables nullable
    reference types. Other projects reference this file, not individual .cs files.

  Loan.cs
    Data model for one loan (name, servicer, principal, rate, term, dates,
    notes, and so on). UI, calculator, and SQLite all share this type so a
    loan looks the same everywhere.

  PaymentScheduleEntry.cs
    One row of an amortization schedule: payment number, date, principal,
    interest, extra payment, and remaining balance. The calculator produces
    a list of these; the schedule grid and charts consume them.

  LoanValidator.cs
    Static checks for principal, interest rate, term, and minimum payment.
    The detail screen and calculator call it so invalid loans are rejected
    before they are saved or scheduled.

  LoanCalculator.cs
    Amortization engine. It builds a payment schedule, payoff month count,
    and total interest using level-payment (PMT-style) math plus an optional
    extra monthly payment.

  ILoanCalculator.cs
    Interface implemented by LoanCalculator. The app injects this so tests
    (or a future replacement) can swap the calculator without changing views.

  ILoanRepository.cs
    Interface for load, save, and delete. The Data project implements it;
    view models depend on the interface, not on SQLite types.


STUDENTLOANTRACKER.DATA  (class library — SQLite)
-------------------------------------------------
  StudentLoanTracker.Data.csproj
    Project file for the Data library. It references Core and
    Microsoft.Data.Sqlite so SQL stays out of the UI and Core layers.

  DatabaseInitializer.cs
    Creates the app data folder under the user’s ApplicationData directory
    and ensures the Loans table exists. First launch runs this so SQLite is
    ready before any screen loads.

  SqliteLoanRepository.cs
    Implements ILoanRepository with SELECT/INSERT/UPDATE/DELETE. It maps
    table rows to Loan objects (Guid and DateTime are stored as strings).


STUDENTLOANTRACKER.APP  (executable Avalonia application)
---------------------------------------------------------
  StudentLoanTracker.App.csproj
    Project file for the desktop app: Avalonia, LiveCharts, CommunityToolkit.Mvvm,
    DI, and references to Core and Data. `dotnet run --project` this file.

  app.manifest
    Windows application manifest (compatibility and DPI defaults). Leave it
    alone unless you have a specific Windows packaging need.

  Program.cs
    Process entry point. It builds the Avalonia application and starts the
    classic desktop lifetime so a normal windowed app appears.

  App.axaml
    Application-level XAML: Fluent theme, DataGrid theme, and ViewLocator
    in DataTemplates so a ContentControl can show the matching view.

  App.axaml.cs
    Startup code: optional validation plugin removal, SQLite initialization,
    and Microsoft.Extensions.DependencyInjection (repository, calculator,
    MainWindowViewModel). It assigns DataContext on MainWindow.

  ViewLocator.cs
    IDataTemplate that, given a ViewModelBase, loads the matching *View*
    type by replacing “ViewModel” with “View” in the type name. That is how
    navigation can swap screens without constructing windows by hand.

  Services\INavigationService.cs
    Interface with NavigateTo(viewModel) and NavigateToLoanList(). Child
    view models use it to change the current screen without knowing the
    window type.


  ViewModels\ViewModelBase.cs
    Empty base class inheriting ObservableObject. ViewLocator only matches
    types derived from this, so every screen view model inherits it.

  ViewModels\MainWindowViewModel.cs
    Holds CurrentViewModel (the active screen) and implements INavigationService.
    It creates or reuses LoanListViewModel when you go back to the list.

  ViewModels\LoanListViewModel.cs
    ObservableCollection of loans, selected row, status text, and HasLoans /
    ShowEmptyState. Commands add, edit, delete, refresh, open schedule, and
    open charts. Data comes from ILoanRepository.

  ViewModels\LoanDetailViewModel.cs
    Two-way fields for one Loan. Save copies values onto the entity, validates,
    then Add or Update in the repository and returns to the list. Cancel goes
    back without saving. The title switches between add and edit.

  ViewModels\ScheduleViewModel.cs
    Wraps one loan and ILoanCalculator. Changing ExtraMonthlyPayment rebuilds
    the schedule. Totals and payoff date are bound on the schedule view.

  ViewModels\ChartsViewModel.cs
    Same loan, calculator, and extra-payment idea as the schedule screen.
    It builds LiveCharts series for a remaining-balance line and a
    principal-vs-interest pie.


  Views\MainWindow.axaml
    Root window: title, size, header strip, and a ContentControl bound to
    CurrentViewModel. Everything you see after launch is hosted here.

  Views\MainWindow.axaml.cs
    Code-behind for the window. It typically only calls InitializeComponent.

  Views\LoanListView.axaml
    Loan list UI: toolbar, DataGrid, empty-state panel, and status line.
    Bindings point at LoanListViewModel.

  Views\LoanListView.axaml.cs
    Code-behind for the list user control (usually minimal).

  Views\LoanDetailView.axaml
    Add/edit form: text boxes, numeric inputs, date picker, notes,
    validation messages, save and cancel. Layout lives here; save logic
    lives in the view model.

  Views\LoanDetailView.axaml.cs
    Code-behind for the form user control (usually minimal).

  Views\ScheduleView.axaml
    Back button, loan name, extra-payment controls, summary numbers, and a
    read-only schedule DataGrid bound to ScheduleViewModel.

  Views\ScheduleView.axaml.cs
    Code-behind for the schedule user control (usually minimal).

  Views\ChartsView.axaml
    Back button, loan name, extra payment, totals, CartesianChart and
    PieChart bound to ChartsViewModel series.

  Views\ChartsView.axaml.cs
    Code-behind for the charts user control (usually minimal).


GENERATED / BUILD ARTIFACTS (DO NOT EDIT BY HAND)
--------------------------------------------------------------------------------

  **/obj/** and **/bin/**
    Compiler output, including CommunityToolkit.Mvvm generated *.g.cs files
    that implement [ObservableProperty] and [RelayCommand] for your view models.

  Editing those generated files would be overwritten on the next build.


VIEWMODELS AND AUTO-GENERATED CODE
--------------------------------------------------------------------------------

  ViewModels use CommunityToolkit.Mvvm attributes. At compile time, the toolkit
  generates partial class members (properties, commands). You edit the partial
  class file in ViewModels\; the generator fills in the repetitive
  INotifyPropertyChanged and ICommand wiring.

================================================================================
  END OF READ_ME
================================================================================
