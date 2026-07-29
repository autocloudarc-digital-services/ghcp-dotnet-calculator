#nullable enable

/// <summary>
/// Provides arithmetic operations for the calculator.
/// </summary>
public static class CalculatorOperations
{
    /// <summary>
    /// Adds two operands.
    /// </summary>
    /// <param name="firstOperand">The first operand.</param>
    /// <param name="secondOperand">The second operand.</param>
    /// <returns>The sum of the operands.</returns>
    public static double Add(double firstOperand, double secondOperand) =>
        firstOperand + secondOperand;

    /// <summary>
    /// Subtracts the second operand from the first operand.
    /// </summary>
    /// <param name="firstOperand">The first operand.</param>
    /// <param name="secondOperand">The second operand.</param>
    /// <returns>The difference between the operands.</returns>
    public static double Subtract(double firstOperand, double secondOperand) =>
        firstOperand - secondOperand;

    /// <summary>
    /// Multiplies two operands.
    /// </summary>
    /// <param name="firstOperand">The first operand.</param>
    /// <param name="secondOperand">The second operand.</param>
    /// <returns>The product of the operands.</returns>
    public static double Multiply(double firstOperand, double secondOperand) =>
        firstOperand * secondOperand;

    /// <summary>
    /// Divides the first operand by the second operand.
    /// </summary>
    /// <param name="firstOperand">The first operand.</param>
    /// <param name="secondOperand">The second operand.</param>
    /// <returns>The quotient of the operands.</returns>
    /// <exception cref="DivideByZeroException">
    /// Thrown when <paramref name="secondOperand"/> is zero.
    /// </exception>
    public static double Divide(double firstOperand, double secondOperand)
    {
        if (secondOperand == 0)
        {
            throw new DivideByZeroException("Cannot divide by zero.");
        }

        return firstOperand / secondOperand;
    }

    /// <summary>
    /// Returns the remainder after dividing the first operand by the second operand.
    /// </summary>
    /// <param name="firstOperand">The first operand.</param>
    /// <param name="secondOperand">The second operand.</param>
    /// <returns>The remainder after division.</returns>
    /// <exception cref="DivideByZeroException">
    /// Thrown when <paramref name="secondOperand"/> is zero.
    /// </exception>
    public static double Modulo(double firstOperand, double secondOperand)
    {
        if (secondOperand == 0)
        {
            throw new DivideByZeroException("Cannot calculate modulo with zero.");
        }

        return firstOperand % secondOperand;
    }

    /// <summary>
    /// Raises the first operand to the power of the second operand.
    /// </summary>
    /// <param name="firstOperand">The base value.</param>
    /// <param name="secondOperand">The exponent value.</param>
    /// <returns>The power result.</returns>
    public static double Power(double firstOperand, double secondOperand) =>
        Math.Pow(firstOperand, secondOperand);
}
