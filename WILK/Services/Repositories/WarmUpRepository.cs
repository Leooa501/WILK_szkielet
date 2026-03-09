using System;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using WILK.Models;

namespace WILK.Services.Repositories
{
    /// <summary>
    /// Repository for component warm-up management
    /// </summary>
    public interface IWarmUpRepository
    {
        Task<DatabaseResult<DataTable>> GetWarmUpComponentsTableAsync();
        Task<DatabaseResult<bool>> AddWarmUpComponentAsync(int rId);
        Task<DatabaseResult<bool>> DeleteWarmUpComponentAsync(int warmUpId);
        Task<DatabaseResult<bool>> IsInWarmUpAsync(int rId);
    }

    public class WarmUpRepository : IWarmUpRepository
    {
        private readonly string _connectionString;
        public WarmUpRepository(string connectionString)
        {
            _connectionString = connectionString;        }

        private MySqlConnection CreateConnection()
        {
            var connection = new MySqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        public Task<DatabaseResult<DataTable>> GetWarmUpComponentsTableAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    const string sql = @"
                        SELECT w.id, c.r_id, c.name 
                        FROM WarmUp w 
                        LEFT JOIN Components c ON c.id = w.c_id
                        ORDER BY w.id DESC;";
                    
                    using var command = new MySqlCommand(sql, connection);
                    using var adapter = new MySqlDataAdapter(command);
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    
                    return DatabaseResult<DataTable>.Success(dt);
                }
                catch (Exception ex)
                {                    return DatabaseResult<DataTable>.Failure($"Error getting warm-up components table: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<bool>> AddWarmUpComponentAsync(int rId)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    const string sql = @"
                        INSERT INTO WarmUp (c_id)
                        VALUES ((SELECT id FROM Components WHERE r_id = @rId LIMIT 1));";
                    
                    using var command = new MySqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@rId", rId);
                    
                    command.ExecuteNonQuery();                    return DatabaseResult<bool>.Success(true);
                }
                catch (Exception ex)
                {                    return DatabaseResult<bool>.Failure($"Error adding warm-up component: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<bool>> DeleteWarmUpComponentAsync(int warmUpId)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    const string sql = "DELETE FROM WarmUp WHERE id = @id;";
                    
                    using var command = new MySqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@id", warmUpId);
                    
                    command.ExecuteNonQuery();                    return DatabaseResult<bool>.Success(true);
                }
                catch (Exception ex)
                {                    return DatabaseResult<bool>.Failure($"Error deleting warm-up component: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<bool>> IsInWarmUpAsync(int rId)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    const string sql = @"
                        SELECT COUNT(*) 
                        FROM WarmUp 
                        WHERE c_id = (SELECT id FROM Components WHERE r_id = @rId LIMIT 1);";
                    
                    using var command = new MySqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@rId", rId);
                    
                    var count = Convert.ToInt32(command.ExecuteScalar());
                    return DatabaseResult<bool>.Success(count > 0);
                }
                catch (Exception ex)
                {                    return DatabaseResult<bool>.Failure($"Error checking warm-up status: {ex.Message}", ex);
                }
            });
        }
    }
}
