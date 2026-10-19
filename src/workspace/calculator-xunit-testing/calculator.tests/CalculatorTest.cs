#nullable enable

using System.Globalization;
using Npgsql;

namespace calculator.tests;

public class CalculatorOperationsTests : IClassFixture<PostgreSqlTestData>
{
    private readonly PostgreSqlTestData testData;

    /// <summary>Uses the isolated PostgreSQL dataset initialized by xUnit.</summary>
    /// <param name="testData">The database fixture shared by this test class.</param>
    public CalculatorOperationsTests(PostgreSqlTestData testData)
    {
        this.testData = testData;
    }

    /// <summary>Loads addition cases from PostgreSQL.</summary>
    /// <returns>The operands and expected result for each addition case.</returns>
    public static IEnumerable<object[]> GetAddTestCases() => GetTestCases("Add");

    /// <summary>Loads subtraction cases from PostgreSQL.</summary>
    /// <returns>The operands and expected result for each subtraction case.</returns>
    public static IEnumerable<object[]> GetSubtractTestCases() => GetTestCases("Subtract");

    /// <summary>Loads multiplication cases from PostgreSQL.</summary>
    /// <returns>The operands and expected result for each multiplication case.</returns>
    public static IEnumerable<object[]> GetMultiplyTestCases() => GetTestCases("Multiply");

    /// <summary>Loads division cases from PostgreSQL.</summary>
    /// <returns>The operands and expected result for each division case.</returns>
    public static IEnumerable<object[]> GetDivideTestCases() => GetTestCases("Divide");

    /// <summary>Loads modulo cases from PostgreSQL.</summary>
    /// <returns>The operands and expected result for each modulo case.</returns>
    public static IEnumerable<object[]> GetModuloTestCases() => GetTestCases("Modulo");

    /// <summary>Loads exponentiation cases from PostgreSQL.</summary>
    /// <returns>The operands and expected result for each power case.</returns>
    public static IEnumerable<object[]> GetPowerTestCases() => GetTestCases("Exponent");

    private static IEnumerable<object[]> GetTestCases(string operation)
    {
        // xUnit v2 MemberData is synchronous; the fixture performs asynchronous database I/O.
        return PostgreSqlTestData.Current.ReadTestCasesAsync(operation).GetAwaiter().GetResult();
    }

    [Theory]
    [MemberData(nameof(GetAddTestCases), DisableDiscoveryEnumeration = true)]
    public void GivenNumbers_Add_ReturnsSum(double firstOperand, double secondOperand, double expected)
    {
        var actual = global::CalculatorOperations.Add(firstOperand, secondOperand);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(GetSubtractTestCases), DisableDiscoveryEnumeration = true)]
    public void GivenNumbers_Subtract_ReturnsDifference(double firstOperand, double secondOperand, double expected)
    {
        var actual = global::CalculatorOperations.Subtract(firstOperand, secondOperand);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(GetMultiplyTestCases), DisableDiscoveryEnumeration = true)]
    public void GivenNumbers_Multiply_ReturnsProduct(double firstOperand, double secondOperand, double expected)
    {
        var actual = global::CalculatorOperations.Multiply(firstOperand, secondOperand);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(GetDivideTestCases), DisableDiscoveryEnumeration = true)]
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
    [MemberData(nameof(GetModuloTestCases), DisableDiscoveryEnumeration = true)]
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
    [MemberData(nameof(GetPowerTestCases), DisableDiscoveryEnumeration = true)]
    public void GivenNumbers_Power_ReturnsExponentResult(double firstOperand, double secondOperand, double expected)
    {
        var actual = global::CalculatorOperations.Power(firstOperand, secondOperand);

        Assert.Equal(expected, actual, 5);
    }

    /// <summary>Checks invariant numeric parsing and skips malformed database rows.</summary>
    /// <returns>A task representing the database-backed assertion.</returns>
    [Fact]
    public async Task ReadTestCases_MalformedNumericRows_SkipsInvalidRows()
    {
        await using var connection = new NpgsqlConnection(testData.ConnectionString);
        await connection.OpenAsync();
        const string operation = "ParsingRegression";
        await using var insert = new NpgsqlCommand("""
            INSERT INTO test_data ("Operand1", "Operand2", "Operation", "ExpectedResult")
            VALUES ('1.5', '2.25', @operation, '3.75'),
                   ('invalid', '2', @operation, '3'),
                   ('1', NULL, @operation, '3'),
                   ('1', '2', @operation, 'NaN');
            """, connection);
        insert.Parameters.AddWithValue("operation", operation);
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            await insert.ExecuteNonQueryAsync();
            var cases = await testData.ReadTestCasesAsync(operation);
            Assert.Equal(new object[] { 1.5, 2.25, 3.75 }, Assert.Single(cases));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            await using var delete = new NpgsqlCommand(
                """DELETE FROM test_data WHERE "Operation" = @operation""", connection);
            delete.Parameters.AddWithValue("operation", operation);
            await delete.ExecuteNonQueryAsync();
        }
    }

    /// <summary>Prevents missing operation data from silently passing without assertions.</summary>
    /// <returns>A task representing the database-backed assertion.</returns>
    [Fact]
    public async Task ReadTestCases_UnknownOperation_ThrowsClearError()
    {
        var exception = await Assert.ThrowsAsync<InvalidDataException>(
            () => testData.ReadTestCasesAsync("MissingOperation"));
        Assert.Contains("no valid cases for operation 'MissingOperation'", exception.Message);
    }
}
