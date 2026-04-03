using CommunityToolkit.Mvvm.ComponentModel;

namespace StudentLoanTracker.App.ViewModels;

/// <summary>Marker base type for MVVM view models; <see cref="ViewLocator"/> only matches this hierarchy.</summary>
public abstract class ViewModelBase : ObservableObject
{
}
