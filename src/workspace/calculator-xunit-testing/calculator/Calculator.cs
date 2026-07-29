#nullable enable

Console.WriteLine("Basic Calculator");

var continueCalculating = true;

while (continueCalculating)
{
    if (!TryReadOperand("Enter the first number: ", out var firstOperand)
        || !TryReadOperand("Enter the second number: ", out var secondOperand))
    {
        Console.WriteLine("Input ended. Exiting calculator.");
        break;
    }

    Console.Write("Enter an operator (+, -, *, /, %, ^): ");
    var operation = Console.ReadLine()?.Trim();

    if (operation is null)
    {
        Console.WriteLine("Input ended. Exiting calculator.");
        break;
    }

    try
    {
        var result = operation switch
        {
            "+" => CalculatorOperations.Add(firstOperand, secondOperand),
            "-" => CalculatorOperations.Subtract(firstOperand, secondOperand),
            "*" => CalculatorOperations.Multiply(firstOperand, secondOperand),
            "/" => CalculatorOperations.Divide(firstOperand, secondOperand),
            "%" => CalculatorOperations.Modulo(firstOperand, secondOperand),
            "^" => CalculatorOperations.Power(firstOperand, secondOperand),
            _ => throw new InvalidOperationException(
                "Please enter one of these operators: +, -, *, /, %, ^.")
        };

        Console.WriteLine($"Result: {result}");
    }
    catch (DivideByZeroException exception)
    {
        Console.WriteLine($"{exception.Message} Please try another calculation.");
    }
    catch (InvalidOperationException exception)
    {
        Console.WriteLine(exception.Message);
    }

    continueCalculating = ReadContinueResponse();
}

static bool TryReadOperand(string prompt, out double operand)
{
    while (true)
    {
        Console.Write(prompt);
        var input = Console.ReadLine()?.Trim();

        if (input is null)
        {
            operand = default;
            return false;
        }

        if (double.TryParse(input, out operand))
        {
            return true;
        }

        Console.WriteLine("Please enter a valid number.");
    }
}

static bool ReadContinueResponse()
{
    while (true)
    {
        Console.Write("Would you like to perform another calculation? (y/n): ");
        var response = Console.ReadLine()?.Trim();

        if (response is null)
        {
            return false;
        }

        if (response.Equals("y", StringComparison.OrdinalIgnoreCase)
            || response.Equals("yes", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (response.Equals("n", StringComparison.OrdinalIgnoreCase)
            || response.Equals("no", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        Console.WriteLine("Please enter y or n.");
    }
}
