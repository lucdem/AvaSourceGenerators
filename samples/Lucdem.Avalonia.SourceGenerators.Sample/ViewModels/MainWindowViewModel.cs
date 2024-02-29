using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Lucdem.Avalonia.SourceGenerators.Sample.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private string? bindedText = "I'm binded! Type in the text box to change me";

    [ObservableProperty]
    public Thickness buttonLabelMargin;

    [ObservableProperty]
    public int buttonLabelMarginSize;

    partial void OnButtonLabelMarginSizeChanged(int value)
    {
        ButtonLabelMargin = new Thickness(value);
    }
}
