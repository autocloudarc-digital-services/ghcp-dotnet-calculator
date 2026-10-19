#nullable enable

using System.Globalization;

namespace calculator.web.Services;

/// <summary>Maintains immediate-execution calculator state for one Blazor circuit.</summary>
public sealed class CalculatorService
{
    private double? accumulator;
    private string? pendingOperation;
    private bool startNewEntry = true;

    /// <summary>Gets the current entry, result, or error indicator.</summary>
    public string Display { get; private set; } = "0";

    /// <summary>Gets the pending or completed expression.</summary>
    public string Expression { get; private set; } = "";

    /// <summary>Gets an actionable error message when input or arithmetic fails.</summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>Occurs only after a successful calculation.</summary>
    public event Action<CalculationRecord>? CalculationCompleted;

    /// <summary>Processes one keypad or supported keyboard action.</summary>
    /// <param name="key">A digit, decimal, operator, equals, or clear key.</param>
    /// <exception cref="ArgumentException">The key is unsupported.</exception>
    public void Input(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (key.Length == 1 && key[0] is >= '0' and <= '9')
        {
            BeginEntry();
            Display = Display == "0" ? key : Display + key;
            return;
        }

        switch (key)
        {
            case ".":
                BeginEntry();
                if (!Display.Contains('.')) Display += ".";
                break;
            case "+" or "-" or "*" or "/" or "%" or "^":
                SelectOperation(key);
                break;
            case "=" or "Enter":
                if (pendingOperation is not null && !startNewEntry) Calculate();
                break;
            case "C" or "c" or "Backspace" or "Delete":
                Clear();
                break;
            default:
                throw new ArgumentException("Unsupported calculator key.", nameof(key));
        }
    }

    /// <summary>Resets the current calculation without changing history.</summary>
    public void Clear()
    {
        Display = "0";
        Expression = "";
        ErrorMessage = null;
        accumulator = null;
        pendingOperation = null;
        startNewEntry = true;
    }

    /// <summary>Loads a history result as the starting value of a new calculation.</summary>
    /// <param name="record">The completed calculation to replay.</param>
    /// <exception cref="ArgumentException">The record has a non-finite result.</exception>
    public void Replay(CalculationRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        if (!double.IsFinite(record.Result))
            throw new ArgumentException("A replay result must be finite.", nameof(record));
        Clear();
        accumulator = record.Result;
        Display = record.ResultText;
    }

    private void BeginEntry()
    {
        if (ErrorMessage is not null) Clear();
        if (!startNewEntry) return;
        if (pendingOperation is null)
        {
            accumulator = null;
            Expression = "";
        }

        Display = "0";
        startNewEntry = false;
    }

    private void SelectOperation(string operation)
    {
        if (ErrorMessage is not null) return;
        if (pendingOperation is not null && !startNewEntry)
        {
            if (!Calculate()) return;
        }
        else if (accumulator is null || !startNewEntry)
        {
            if (!TryReadOperand(out var operand)) return;
            accumulator = operand;
        }

        var firstOperand = accumulator
            ?? throw new InvalidOperationException("An operand is required before selecting an operation.");
        pendingOperation = operation;
        Expression = $"{CalculationRecord.Format(firstOperand)} {operation}";
        startNewEntry = true;
    }

    private bool Calculate()
    {
        if (!TryReadOperand(out var secondOperand)) return false;
        if (accumulator is not { } firstOperand || pendingOperation is not { } operation)
            throw new InvalidOperationException("Two operands and an operation are required to calculate.");
        double result;
        try
        {
            result = operation switch
            {
                "+" => CalculatorOperations.Add(firstOperand, secondOperand),
                "-" => CalculatorOperations.Subtract(firstOperand, secondOperand),
                "*" => CalculatorOperations.Multiply(firstOperand, secondOperand),
                "/" => CalculatorOperations.Divide(firstOperand, secondOperand),
                "%" => CalculatorOperations.Modulo(firstOperand, secondOperand),
                "^" => CalculatorOperations.Power(firstOperand, secondOperand),
                _ => throw new InvalidOperationException("No supported operation is pending.")
            };
        }
        catch (DivideByZeroException)
        {
            return SetError("Cannot divide or take a remainder by zero. Clear or enter a number to continue.");
        }

        if (!double.IsFinite(result))
            return SetError("The result is outside the supported real-number range. Clear or enter a number to continue.");

        var record = new CalculationRecord(firstOperand, operation, secondOperand, result);
        accumulator = result;
        pendingOperation = null;
        startNewEntry = true;
        Display = record.ResultText;
        Expression = $"{record.Expression} =";
        CalculationCompleted?.Invoke(record);
        return true;
    }

    private bool TryReadOperand(out double operand)
    {
        if (double.TryParse(Display, NumberStyles.Float, CultureInfo.InvariantCulture, out operand) &&
            double.IsFinite(operand)) return true;
        return SetError("Enter a finite number. Clear or enter a number to continue.");
    }

    private bool SetError(string message)
    {
        Clear();
        Display = "Error";
        ErrorMessage = message;
        return false;
    }
}
