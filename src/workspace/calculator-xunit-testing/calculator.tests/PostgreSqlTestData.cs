#nullable enable

using System.Globalization;
using System.Security.Cryptography;
using Npgsql;
using NpgsqlTypes;
using Testcontainers.PostgreSql;

namespace calculator.tests;

/// <summary>Owns an isolated PostgreSQL container seeded once from the copied CSV dataset.</summary>
public sealed class PostgreSqlTestData : IAsyncLifetime
{
    private PostgreSqlContainer? container;
    private static PostgreSqlTestData? current;

    internal static PostgreSqlTestData Current => current
        ?? throw new InvalidOperationException("PostgreSQL test data must be initialized before MemberData execution.");

    internal string ConnectionString => container?.GetConnectionString()
        ?? throw new InvalidOperationException("The PostgreSQL test container has not been created.");

    /// <summary>Starts PostgreSQL with ephemeral credentials and seeds its test_data table.</summary>
    /// <returns>A task representing container startup and CSV seeding.</returns>
    /// <exception cref="InvalidOperationException">Database initialization fails.</exception>
    public async Task InitializeAsync()
    {
        try
        {
            container = new PostgreSqlBuilder("postgres:15.1")
                .WithUsername($"user_{Guid.NewGuid():N}")
                .WithPassword(Convert.ToHexString(RandomNumberGenerator.GetBytes(32)))
                .WithDatabase($"test_{Guid.NewGuid():N}")
                .Build();
            using var startupTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            await container.StartAsync(startupTimeout.Token);
            await SeedAsync();
            current = this;
        }
        catch (Exception exception)
        {
            await DisposeAsync();
            throw new InvalidOperationException(
                "PostgreSQL test initialization failed. Check that Docker is running (docker info), " +
                "postgres:15.1 can be pulled, and TestCases.csv is copied to the test output directory.",
                exception);
        }
    }

    /// <summary>Removes the test container and its ephemeral database.</summary>
    /// <returns>A task representing asynchronous container cleanup.</returns>
    public async Task DisposeAsync()
    {
        if (ReferenceEquals(current, this))
        {
            current = null;
        }

        if (container is not null)
        {
            await container.DisposeAsync();
            container = null;
        }
    }

    private async Task SeedAsync()
    {
        var csvPath = Path.Combine(AppContext.BaseDirectory, "TestCases.csv");
        using var reader = new StreamReader(csvPath);
        if (await reader.ReadLineAsync() != "Operand1,Operand2,Operation,ExpectedResult")
        {
            throw new InvalidDataException($"Invalid CSV header in {csvPath}.");
        }

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var create = new NpgsqlCommand("""
            CREATE TABLE test_data (
                "Operand1" TEXT, "Operand2" TEXT, "Operation" TEXT, "ExpectedResult" TEXT
            );
            """, connection);
        await create.ExecuteNonQueryAsync();

        await using var importer = await connection.BeginBinaryImportAsync("""
            COPY test_data ("Operand1", "Operand2", "Operation", "ExpectedResult")
            FROM STDIN (FORMAT BINARY)
            """);
        var lineNumber = 1;
        while (await reader.ReadLineAsync() is { } line)
        {
            lineNumber++;
            var columns = line.Split(',');
            if (columns.Length != 4)
            {
                Console.Error.WriteLine($"Skipping CSV row {lineNumber}: expected four columns.");
                continue;
            }

            await importer.StartRowAsync();
            foreach (var column in columns)
            {
                await importer.WriteAsync(column, NpgsqlDbType.Text);
            }
        }

        await importer.CompleteAsync();
    }

    internal async Task<IReadOnlyList<object[]>> ReadTestCasesAsync(string operation)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand("""
            SELECT "Operand1", "Operand2", "ExpectedResult"
            FROM test_data WHERE "Operation" = @operation
            """, connection);
        command.Parameters.AddWithValue("operation", operation);
        await using var reader = await command.ExecuteReaderAsync();
        var cases = new List<object[]>();
        var rowNumber = 0;
        while (await reader.ReadAsync())
        {
            rowNumber++;
            var values = new double[3];
            var valid = true;
            for (var index = 0; index < values.Length; index++)
            {
                if (reader.IsDBNull(index) ||
                    !double.TryParse(reader.GetString(index), NumberStyles.Float,
                        CultureInfo.InvariantCulture, out values[index]) ||
                    !double.IsFinite(values[index]))
                {
                    valid = false;
                    break;
                }
            }

            if (!valid)
            {
                Console.Error.WriteLine($"Skipping PostgreSQL {operation} row {rowNumber}: invalid numeric value.");
                continue;
            }

            cases.Add(new object[] { values[0], values[1], values[2] });
        }

        if (cases.Count == 0)
        {
            throw new InvalidDataException($"PostgreSQL test_data contains no valid cases for operation '{operation}'.");
        }

        return cases;
    }
}
