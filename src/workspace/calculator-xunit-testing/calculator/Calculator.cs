Console.WriteLine("Calculator");

var continueCalculating = true;

while (continueCalculating)
{
	Console.Write("Enter the first number: ");
	var firstInput = Console.ReadLine();

	if (!double.TryParse(firstInput, out var firstOperand))
	{
		Console.WriteLine("Please enter a valid number.");
		continue;
	}

	Console.Write("Enter an operator (+, -, *, /, %, ^): ");
	var operatorInput = Console.ReadLine();

	Console.Write("Enter the second number: ");
	var secondInput = Console.ReadLine();

	if (!double.TryParse(secondInput, out var secondOperand))
	{
		Console.WriteLine("Please enter a valid number.");
		continue;
	}

	try
	{
		var result = operatorInput?.Trim() switch
		{
			"+" => CalculatorOperations.Add(firstOperand, secondOperand),
			"-" => CalculatorOperations.Subtract(firstOperand, secondOperand),
			"*" => CalculatorOperations.Multiply(firstOperand, secondOperand),
			"/" => CalculatorOperations.Divide(firstOperand, secondOperand),
			"%" => CalculatorOperations.Modulo(firstOperand, secondOperand),
			"^" => CalculatorOperations.Power(firstOperand, secondOperand),
			_ => throw new ArgumentException("Please enter a supported operator.")
		};

		Console.WriteLine($"Result: {result}");
	}
	catch (ArgumentException exception)
	{
		Console.WriteLine(exception.Message);
	}
	catch (DivideByZeroException)
	{
		Console.WriteLine("The second number cannot be zero for this operation.");
	}

	Console.Write("Would you like to perform another calculation? (y/n): ");
	var continueInput = Console.ReadLine();
	continueCalculating = string.Equals(continueInput?.Trim(), "y", StringComparison.OrdinalIgnoreCase);
}

Console.WriteLine("Goodbye!");
