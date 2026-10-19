#nullable enable

using System.Globalization;

namespace calculator.web.Services;

/// <summary>A completed calculation that can be displayed or replayed.</summary>
/// <param name="FirstOperand">The left-hand operand.</param>
/// <param name="Operation">The arithmetic operator.</param>
/// <param name="SecondOperand">The right-hand operand.</param>
/// <param name="Result">The unrounded result.</param>
public sealed record CalculationRecord(double FirstOperand, string Operation, double SecondOperand, double Result)
{
    /// <summary>Gets the readable calculation expression.</summary>
    public string Expression => $"{Format(FirstOperand)} {Operation} {Format(SecondOperand)}";

    /// <summary>Gets the formatted result.</summary>
    public string ResultText => Format(Result);

    internal static string Format(double value) => (value == 0 ? 0 : value).ToString("G15", CultureInfo.InvariantCulture);
}
