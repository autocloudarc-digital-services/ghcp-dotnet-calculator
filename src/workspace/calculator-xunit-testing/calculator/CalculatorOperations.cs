public static class CalculatorOperations
{
    public static double Add(double firstOperand, double secondOperand)
    {
        return firstOperand + secondOperand;
    }

    public static double Subtract(double firstOperand, double secondOperand)
    {
        return firstOperand - secondOperand;
    }

    public static double Multiply(double firstOperand, double secondOperand)
    {
        return firstOperand * secondOperand;
    }

    public static double Divide(double firstOperand, double secondOperand)
    {
        if (secondOperand == 0)
        {
            throw new DivideByZeroException();
        }

        return firstOperand / secondOperand;
    }

    public static double Modulo(double firstOperand, double secondOperand)
    {
        if (secondOperand == 0)
        {
            throw new DivideByZeroException();
        }

        return firstOperand % secondOperand;
    }

    public static double Power(double firstOperand, double secondOperand)
    {
        return Math.Pow(firstOperand, secondOperand);
    }
}