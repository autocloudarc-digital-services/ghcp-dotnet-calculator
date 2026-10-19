#nullable enable

/// <summary>Provides deterministic arithmetic shared by the console and web calculators.</summary>
public static class CalculatorOperations
{
    /// <summary>Adds two operands.</summary>
    /// <param name="firstOperand">The first value.</param>
    /// <param name="secondOperand">The second value.</param>
    /// <returns>The sum.</returns>
    public static double Add(double firstOperand, double secondOperand)
    {
        return firstOperand + secondOperand;
    }

    /// <summary>Subtracts the second operand from the first.</summary>
    /// <param name="firstOperand">The value to subtract from.</param>
    /// <param name="secondOperand">The value to subtract.</param>
    /// <returns>The difference.</returns>
    public static double Subtract(double firstOperand, double secondOperand)
    {
        return firstOperand - secondOperand;
    }

    /// <summary>Multiplies two operands.</summary>
    /// <param name="firstOperand">The first factor.</param>
    /// <param name="secondOperand">The second factor.</param>
    /// <returns>The product.</returns>
    public static double Multiply(double firstOperand, double secondOperand)
    {
        return firstOperand * secondOperand;
    }

    /// <summary>Divides the first operand by the second.</summary>
    /// <param name="firstOperand">The dividend.</param>
    /// <param name="secondOperand">The divisor.</param>
    /// <returns>The quotient.</returns>
    /// <exception cref="DivideByZeroException">The divisor is zero.</exception>
    public static double Divide(double firstOperand, double secondOperand)
    {
        if (secondOperand == 0)
        {
            throw new DivideByZeroException();
        }

        return firstOperand / secondOperand;
    }

    /// <summary>Calculates the remainder of division.</summary>
    /// <param name="firstOperand">The dividend.</param>
    /// <param name="secondOperand">The divisor.</param>
    /// <returns>The remainder.</returns>
    /// <exception cref="DivideByZeroException">The divisor is zero.</exception>
    public static double Modulo(double firstOperand, double secondOperand)
    {
        if (secondOperand == 0)
        {
            throw new DivideByZeroException();
        }

        return firstOperand % secondOperand;
    }

    /// <summary>Raises the first operand to the second operand's power.</summary>
    /// <param name="firstOperand">The base.</param>
    /// <param name="secondOperand">The exponent.</param>
    /// <returns>The exponentiation result.</returns>
    public static double Power(double firstOperand, double secondOperand)
    {
        return Math.Pow(firstOperand, secondOperand);
    }
}