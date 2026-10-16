namespace calculator.tests;

public class CalculatorOperationsTests
{
    [Theory]
    [InlineData(2, 3, 5)]
    [InlineData(-2, 3, 1)]
    [InlineData(0, 0, 0)]
    public void GivenNumbers_Add_ReturnsSum(double firstOperand, double secondOperand, double expected)
    {
        var actual = global::CalculatorOperations.Add(firstOperand, secondOperand);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(5, 3, 2)]
    [InlineData(-2, 3, -5)]
    [InlineData(0, 0, 0)]
    public void GivenNumbers_Subtract_ReturnsDifference(double firstOperand, double secondOperand, double expected)
    {
        var actual = global::CalculatorOperations.Subtract(firstOperand, secondOperand);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(2, 3, 6)]
    [InlineData(-2, 3, -6)]
    [InlineData(5, 0, 0)]
    public void GivenNumbers_Multiply_ReturnsProduct(double firstOperand, double secondOperand, double expected)
    {
        var actual = global::CalculatorOperations.Multiply(firstOperand, secondOperand);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(6, 3, 2)]
    [InlineData(5, 2, 2.5)]
    [InlineData(-6, 3, -2)]
    public void GivenNonZeroDivisor_Divide_ReturnsQuotient(double firstOperand, double secondOperand, double expected)
    {
        var actual = global::CalculatorOperations.Divide(firstOperand, secondOperand);

        Assert.Equal(expected, actual, 5);
    }

    [Fact]
    public void GivenZeroDivisor_Divide_ThrowsDivideByZeroException()
    {
        Assert.Throws<DivideByZeroException>(() => global::CalculatorOperations.Divide(6, 0));
    }

    [Theory]
    [InlineData(7, 3, 1)]
    [InlineData(8, 4, 0)]
    [InlineData(-7, 3, -1)]
    public void GivenNonZeroDivisor_Modulo_ReturnsRemainder(double firstOperand, double secondOperand, double expected)
    {
        var actual = global::CalculatorOperations.Modulo(firstOperand, secondOperand);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GivenZeroDivisor_Modulo_ThrowsDivideByZeroException()
    {
        Assert.Throws<DivideByZeroException>(() => global::CalculatorOperations.Modulo(6, 0));
    }

    [Theory]
    [InlineData(2, 3, 8)]
    [InlineData(5, 0, 1)]
    [InlineData(9, 0.5, 3)]
    public void GivenNumbers_Power_ReturnsExponentResult(double firstOperand, double secondOperand, double expected)
    {
        var actual = global::CalculatorOperations.Power(firstOperand, secondOperand);

        Assert.Equal(expected, actual, 5);
    }
}
