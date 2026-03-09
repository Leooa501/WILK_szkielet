using MySql.Data.MySqlClient;
using WILK.Models;

namespace WILK.Services.Repositories
{
    /// <summary>
    /// Repository for data import/export and metadata tracking
    /// </summary>
    public interface IExcessiveUsageRepository
    {
        Task<DatabaseResult<bool>> AddExcessiveUsageAsync(int productId, int quantity, string reason, string reelId);
        Task<DatabaseResult<System.Data.DataTable>> GetExcessiveUsageAsync(DateTime since);
        Task<DatabaseResult<bool>> DeleteExcessiveUsageAsync(int id);
    }

    public class ExcessiveUsageRepository : IExcessiveUsageRepository
    {
        private readonly string _connectionString;

        public ExcessiveUsageRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        private MySqlConnection CreateConnection()
        {
            var connection = new MySqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        public Task<DatabaseResult<bool>> AddExcessiveUsageAsync(int productId, int quantity, string reason, string reelId)
        {
            return Task.Run(async () =>
            {
                try
                {
                    using var connection = CreateConnection();
                    using var command = new MySqlCommand("INSERT INTO ExcessiveUsage (component_id, quantity, reason, reel_id, created_at) VALUES (@ProductId, @Quantity, @Reason, @ReelId, @DateAdded)", connection);
                    command.Parameters.AddWithValue("@ProductId", productId);
                    command.Parameters.AddWithValue("@Quantity", quantity);
                    command.Parameters.AddWithValue("@Reason", reason);
                    command.Parameters.AddWithValue("@ReelId", reelId);
                    command.Parameters.AddWithValue("@DateAdded", DateTime.UtcNow);

                    command.ExecuteNonQuery();

                    return DatabaseResult<bool>.Success(true);
                }
                catch (Exception ex)
                {
                    return DatabaseResult<bool>.Failure(ex.Message);
                }
            });

        }

        public Task<DatabaseResult<System.Data.DataTable>> GetExcessiveUsageAsync(DateTime since)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    string sql = @"SELECT id, component_id, quantity, reason, reel_id, created_at FROM ExcessiveUsage WHERE created_at >= @since ORDER BY created_at DESC LIMIT 1000;";
                    using var cmd = new MySqlCommand(sql, connection);
                    cmd.Parameters.AddWithValue("@since", since);
                    using var reader = cmd.ExecuteReader();
                    var dt = new System.Data.DataTable();
                    dt.Columns.Add("id", typeof(int));
                    dt.Columns.Add("component_id", typeof(int));
                    dt.Columns.Add("quantity", typeof(int));
                    dt.Columns.Add("reason", typeof(string));
                    dt.Columns.Add("reel_id", typeof(string));
                    dt.Columns.Add("created_at", typeof(DateTime));
                    while (reader.Read())
                    {
                        dt.Rows.Add(
                            reader.GetInt32("id"),
                            reader.GetInt32("component_id"),
                            reader.GetInt32("quantity"),
                            reader.IsDBNull(reader.GetOrdinal("reason")) ? string.Empty : reader.GetString("reason"),
                            reader.IsDBNull(reader.GetOrdinal("reel_id")) ? string.Empty : reader.GetString("reel_id"),
                            reader.GetDateTime("created_at")
                        );
                    }

                    return DatabaseResult<System.Data.DataTable>.Success(dt);
                }
                catch (Exception ex)
                {
                    return DatabaseResult<System.Data.DataTable>.Failure(ex.Message, ex);
                }
            });
        }

        public Task<DatabaseResult<bool>> DeleteExcessiveUsageAsync(int id)
        {
            return Task.Run(async () =>
            {
                try
                {
                    using var connection = CreateConnection();
                    using var command = new MySqlCommand("DELETE FROM ExcessiveUsage WHERE id = @Id", connection);
                    command.Parameters.AddWithValue("@Id", id);

                    command.ExecuteNonQuery();

                    return DatabaseResult<bool>.Success(true);
                }
                catch (Exception ex)
                {
                    return DatabaseResult<bool>.Failure(ex.Message);
                }
            });
        }
    }
}