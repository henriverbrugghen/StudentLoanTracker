using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using StudentLoanTracker.App.ViewModels;

namespace StudentLoanTracker.App;

/// <summary>
/// Avalonia data template used by the main window's <c>ContentControl</c>. For any
/// <see cref="ViewModelBase"/>, replaces "ViewModel" with "View" in the CLR type name and instantiates
/// that view type from the same assembly (e.g. <c>LoanListViewModel</c> → <c>LoanListView</c>).
/// </summary>
[RequiresUnreferencedCode(
    "Default implementation of ViewLocator involves reflection which may be trimmed away.",
    Url = "https://docs.avaloniaui.net/docs/concepts/view-locator")]
public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        var vmType = param.GetType();
        var viewName = vmType.FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
        var viewType = Type.GetType(viewName)
            ?? vmType.Assembly.GetType(viewName);

        if (viewType != null)
        {
            return (Control)Activator.CreateInstance(viewType)!;
        }

        return new TextBlock { Text = "Not Found: " + viewName };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
