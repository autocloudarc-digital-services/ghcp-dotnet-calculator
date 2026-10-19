#nullable enable

using System.Globalization;
using calculator.web.Services;

namespace calculator.tests;

/// <summary>Verifies calculator input, arithmetic transitions, and circuit-scoped UI behavior.</summary>
public class CalculatorServiceTests
{
    /// <summary>Checks that keypad sequences produce correctly formatted results.</summary>
    /// <param name="keys">The sequence of calculator keys.</param>
    /// <param name="expected">The expected display text.</param>
    [Theory]
    [InlineData("7+3=", "10")]
    [InlineData("2-5=", "-3")]
    [InlineData("2*3=", "6")]
    [InlineData("5/2=", "2.5")]
    [InlineData("7%3=", "1")]
    [InlineData("9^.5=", "3")]
    [InlineData("2+3*4=", "20")]
    [InlineData("2+*3=", "6")]
    [InlineData("000.5..+0.25=", "0.75")]
    [InlineData("7+3==", "10")]
    [InlineData("7+=", "7")]
    [InlineData("7+3=2", "2")]
    public void Input_KeySequence_ProducesExpectedDisplay(string keys, string expected)
    {
        var calculator = new CalculatorService();
        Enter(calculator, keys);
        Assert.Equal(expected, calculator.Display);
        Assert.Null(calculator.ErrorMessage);
    }

    /// <summary>Checks that failed operations do not enter history and new input recovers.</summary>
    /// <param name="keys">The expression that cannot produce a finite real result.</param>
    /// <param name="expectedHistoryCount">The successful calculations before the error.</param>
    [Theory]
    [InlineData("6/0=", 0)]
    [InlineData("6%0=", 0)]
    [InlineData("9^999=", 0)]
    [InlineData("0-1=^.5=", 1)]
    public void Input_InvalidArithmetic_ReportsErrorAndRecovers(string keys, int expectedHistoryCount)
    {
        var calculator = new CalculatorService();
        var records = new List<CalculationRecord>();
        calculator.CalculationCompleted += records.Add;
        Enter(calculator, keys);
        Assert.Equal(expectedHistoryCount, records.Count);
        Assert.Equal("Error", calculator.Display);
        Assert.False(string.IsNullOrWhiteSpace(calculator.ErrorMessage));

        Enter(calculator, "2+3=");

        Assert.Equal("5", calculator.Display);
        Assert.Null(calculator.ErrorMessage);
        Assert.Equal(expectedHistoryCount + 1, records.Count);
        Assert.DoesNotContain(records, record => !double.IsFinite(record.Result));
    }

    /// <summary>Checks that out-of-range typed numbers produce an error rather than an infinite result.</summary>
    [Fact]
    public void Input_OutOfRangeEntry_ReportsErrorWithoutRecordingCalculation()
    {
        var calculator = new CalculatorService();
        var records = new List<CalculationRecord>();
        calculator.CalculationCompleted += records.Add;
        Enter(calculator, new string('9', 400) + "+");
        Assert.Equal("Error", calculator.Display);
        Assert.Contains("finite number", calculator.ErrorMessage);
        Assert.Empty(records);
        Enter(calculator, "1");
        Assert.Equal("1", calculator.Display);
        Assert.Null(calculator.ErrorMessage);
    }

    /// <summary>Checks precision, invariant formatting, and history replay after a pending operation.</summary>
    [Fact]
    public void Replay_Result_PreservesPrecisionAndResetsPendingOperation()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            var calculator = new CalculatorService();
            CalculationRecord? record = null;
            calculator.CalculationCompleted += calculation => record = calculation;
            Enter(calculator, "1/3=");
            Assert.NotNull(record);
            Enter(calculator, "9+");

            calculator.Replay(record);
            Enter(calculator, "*3=");

            Assert.Equal("1", calculator.Display);
            Assert.Equal(1d, record.Result);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    /// <summary>Checks that clear abandons pending work and never removes past history.</summary>
    [Fact]
    public void Clear_PendingOperation_ResetsCalculatorButPreservesHistory()
    {
        var calculator = new CalculatorService();
        var history = new HistoryService();
        calculator.CalculationCompleted += history.Add;
        Enter(calculator, "7+3=9+");
        calculator.Clear();
        Assert.Equal("0", calculator.Display);
        Assert.Empty(calculator.Expression);
        Enter(calculator, "2=");
        Assert.Equal("2", calculator.Display);
        Assert.Equal(10d, Assert.Single(history.Entries).Result);
        Assert.Throws<ArgumentException>(() => calculator.Input("unsupported"));
    }

    /// <summary>Checks newest-first retention and isolation between sessions.</summary>
    [Fact]
    public void History_MoreThanFiftyCalculations_RetainsNewestSessionEntries()
    {
        var history = new HistoryService();
        for (var index = 0; index < 55; index++)
            history.Add(new CalculationRecord(index, "+", 1, index + 1));

        Assert.Equal(50, history.Entries.Count);
        Assert.Equal(55d, history.Entries[0].Result);
        Assert.Equal(6d, history.Entries[^1].Result);
        Assert.Empty(new HistoryService().Entries);
        history.Clear();
        Assert.Empty(history.Entries);
    }

    /// <summary>Checks that theme changes affect only the current session and can be reversed.</summary>
    [Fact]
    public void Theme_Toggle_ChangesOnlyCurrentSession()
    {
        var theme = new ThemeService();
        var initialClass = theme.CssClass;
        theme.Toggle();
        Assert.True(theme.IsDark);
        Assert.NotEqual(initialClass, theme.CssClass);
        Assert.False(new ThemeService().IsDark);
        theme.Toggle();
        Assert.Equal(initialClass, theme.CssClass);
    }

    private static void Enter(CalculatorService calculator, string keys)
    {
        foreach (var key in keys) calculator.Input(key.ToString());
    }
}
