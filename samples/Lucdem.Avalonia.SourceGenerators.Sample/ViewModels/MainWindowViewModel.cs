using System;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Lucdem.Avalonia.SourceGenerators.Sample.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private string? _bindedText = "I'm binded! Type in the text box to change me";

    [ObservableProperty]
    private Thickness _buttonLabelMargin;

    [ObservableProperty]
    private double _buttonLabelMarginSize;

    partial void OnButtonLabelMarginSizeChanged(double value)
    {
        ButtonLabelMargin = new Thickness(value);
    }
}
