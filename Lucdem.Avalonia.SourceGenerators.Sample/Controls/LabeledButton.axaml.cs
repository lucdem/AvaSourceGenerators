using Avalonia.Controls.Primitives;
using Lucdem.Avalonia.SourceGenerators.Attributes;

namespace Lucdem.Avalonia.SourceGenerators.Sample.Controls;

public partial class LabeledButton : TemplatedControl
{
    public LabeledButton() { }

    [AvaStyledProperty]
    private string labelText = string.Empty;
}