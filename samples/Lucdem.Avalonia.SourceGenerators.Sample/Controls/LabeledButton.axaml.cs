using Avalonia;
using Avalonia.Controls.Primitives;
using Lucdem.Avalonia.SourceGenerators.Attributes;

namespace Lucdem.Avalonia.SourceGenerators.Sample.Controls;

public partial class LabeledButton : TemplatedControl
{
#pragma warning disable 0169, 0414 // disable unused field warnings
    [AvaStyledProperty]
    private string _labelText = string.Empty;

    [AvaStyledProperty]
    private Thickness _labelMargin;
#pragma warning restore 0169, 0414
}