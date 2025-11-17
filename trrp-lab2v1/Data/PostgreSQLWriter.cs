using Npgsql;
using TVShowsTransfer.Models;

namespace TVShowsTransfer.Data
{
    public class PostgreSQLWriter
    {
        private readonly string _connectionString;

        public PostgreSQLWriter(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task SaveNormalizedDataAsync(DataNormalizer normalizer)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                // Your existing PostgreSQL save logic here
                // Copy from your working code

                await transaction.CommitAsync();
                Console.WriteLine("Data saved to PostgreSQL");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Save failed: {ex.Message}");
                throw;
            }
        }
    }
}