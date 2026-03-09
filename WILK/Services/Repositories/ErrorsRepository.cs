using MySql.Data.MySqlClient;
using WILK.Models;

namespace WILK.Services.Repositories
{
    /// <summary>
    /// Repository for data import/export and metadata tracking
    /// </summary>
    public interface IErrorsRepository
    {
        Task<DatabaseResult<bool>> AddErrorsAsync(string type, string cause, string? reelId = null, int? correctAmount = null, int? correctOrder = null, string? correctBox = null, string? description = null, string? author = null);
        Task<DatabaseResult<System.Data.DataTable>> GetErrorsAsync(DateTime since);
    }

    public class ErrorsRepository : IErrorsRepository
    {
        private readonly string _connectionString;

        public ErrorsRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        private MySqlConnection CreateConnection()
        {
            var connection = new MySqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        public Task<DatabaseResult<bool>> AddErrorsAsync(string type, string cause, string? reelId = null, int? correctAmount = null, int? correctOrder = null, string? correctBox = null, string? description = null, string? author = null)
        {
            return Task.Run(async () =>
            {
                try
                {
                    using var connection = CreateConnection();
                    using var command = new MySqlCommand("INSERT INTO KPIErrors (type, cause, created_at, id_reel, correct_amount, correct_order, correct_box, description, author) VALUES (@Type, @Cause, @DateAdded, @ReelId, @CorrectAmount, @CorrectOrder, @CorrectBox, @Description, @Author)", connection);
                    command.Parameters.AddWithValue("@Type", type);
                    command.Parameters.AddWithValue("@Cause", cause);
                    command.Parameters.AddWithValue("@DateAdded", DateTime.UtcNow);
                    command.Parameters.AddWithValue("@ReelId", reelId ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@CorrectAmount", correctAmount ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@CorrectOrder", correctOrder ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@CorrectBox", correctBox ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Description", description ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Author", author ?? (object)DBNull.Value);

                    command.ExecuteNonQuery();

                    return DatabaseResult<bool>.Success(true);
                }
                catch (Exception ex)
                {
                    return DatabaseResult<bool>.Failure(ex.Message);
                }
            });

        }

        public Task<DatabaseResult<System.Data.DataTable>> GetErrorsAsync(DateTime since)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    string sql = @"SELECT type, cause, created_at, id_reel, correct_amount, correct_order, correct_box, description,  author FROM KPIErrors WHERE created_at >= @since ORDER BY created_at DESC LIMIT 1000;";
                    using var cmd = new MySqlCommand(sql, connection);
                    cmd.Parameters.AddWithValue("@since", since);
                    using var reader = cmd.ExecuteReader();
                    var dt = new System.Data.DataTable();
                    dt.Columns.Add("type", typeof(string));
                    dt.Columns.Add("cause", typeof(string));
                    dt.Columns.Add("created_at", typeof(DateTime));
                    dt.Columns.Add("id_reel", typeof(string));
                    dt.Columns.Add("correct_amount", typeof(int));
                    dt.Columns.Add("correct_order", typeof(int));
                    dt.Columns.Add("correct_box", typeof(string));
                    dt.Columns.Add("description", typeof(string));
                    dt.Columns.Add("author", typeof(string));
                    while (reader.Read())
                    {
                        dt.Rows.Add(
                            reader.IsDBNull(reader.GetOrdinal("type")) ? string.Empty : reader.GetString("type"),
                            reader.IsDBNull(reader.GetOrdinal("cause")) ? string.Empty : reader.GetString("cause"),
                            reader.GetDateTime("created_at"),
                            reader.IsDBNull(reader.GetOrdinal("id_reel")) ? string.Empty : reader.GetString("id_reel"),
                            reader.IsDBNull(reader.GetOrdinal("correct_amount")) ? 0 : reader.GetInt32("correct_amount"),
                            reader.IsDBNull(reader.GetOrdinal("correct_order")) ? 0 : reader.GetInt32("correct_order"),
                            reader.IsDBNull(reader.GetOrdinal("correct_box")) ? string.Empty : reader.GetString("correct_box"),
                            reader.IsDBNull(reader.GetOrdinal("description")) ? string.Empty : reader.GetString("description"),
                            reader.IsDBNull(reader.GetOrdinal("author")) ? string.Empty : reader.GetString("author")
                        );
                    }
                    return DatabaseResult<System.Data.DataTable>.Success(dt);
                }
                catch (Exception ex)
                {
                    return DatabaseResult<System.Data.DataTable>.Failure(ex.Message, ex);
                }}
            );
        }
    }
}