using CommunityToolkit.Mvvm.ComponentModel;

namespace Lucdem.Avalonia.SourceGenerators.Sample.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private string? bindedText = "I'm binded! Type here to change me";
}
