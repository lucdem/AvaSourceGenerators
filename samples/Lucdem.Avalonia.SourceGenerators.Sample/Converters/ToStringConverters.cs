using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Lucdem.Avalonia.SourceGenerators.Sample.Converters;

public static class ToStringConverters
{
    public static FuncValueConverter<double, string> DoubleConverter =>
        new FuncValueConverter<double, string>(x => $"{x:g3}");
}