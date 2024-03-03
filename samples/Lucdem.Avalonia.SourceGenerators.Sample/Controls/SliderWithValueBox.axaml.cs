using Avalonia.Controls.Primitives;
using Lucdem.Avalonia.SourceGenerators.Attributes;

namespace Lucdem.Avalonia.SourceGenerators.Sample.Controls;

public partial class SliderWithValueBox : TemplatedControl
{
#pragma warning disable 0169, 0414 // disable unused field warnings
    [AvaDirectProperty]
    private double _min = 0;
    
    [AvaDirectProperty]
    private double _max = 50;
    
    [AvaDirectProperty]
    private double _value = 25;
#pragma warning restore 0169, 0414
}