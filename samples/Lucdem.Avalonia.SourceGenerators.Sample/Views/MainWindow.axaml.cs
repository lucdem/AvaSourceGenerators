using Avalonia.Controls;
using Avalonia.Input;

namespace Lucdem.Avalonia.SourceGenerators.Sample.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            TBox.AddHandler(TextBox.PointerPressedEvent, ClearBoxOnce, handledEventsToo: true);
            //TBox.PointerPressed += ClearBoxOnce;
        }

        private void ClearBoxOnce(object? sender, PointerPressedEventArgs args)
        {
            TBox.Text = string.Empty;
            TBox.RemoveHandler(TextBox.PointerPressedEvent, ClearBoxOnce);
        }
    }
}