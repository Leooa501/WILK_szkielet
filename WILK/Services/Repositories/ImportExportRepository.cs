using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using WILK.Models;

using MProductionStorageLib.DB;
using MProductionStorageLib.Model;
using System.Security.Policy;

namespace WILK.Services.Repositories
{
    /// <summary>
    /// Repository for data import/export and metadata tracking
    /// </summary>
    public interface IImportExportRepository
    {
        Task<DatabaseResult<bool>> SetLastStanMagazynowyImportDateAsync(string fileName);
        Task<DatabaseResult<(string FileName, DateTime Date)?>> GetLastStanMagazynowyImportDateAsync();
        Task<DatabaseResult<bool>> AddPnPDataAsync(List<(int r_id, int quantity)> data, long listId, string fileName, string type);
        Task<DatabaseResult<string>> GetSideFileNameAsync(long listId, string type);
        Task<DatabaseResult<string[]>> GetComponentSideAsync(int rId, long listId);
        Task<DatabaseResult<(string type, int qty)[]>> GetComponentSideAndQtyAsync(int rId, long listId);
    }

    public class ImportExportRepository : IImportExportRepository
    {
        private readonly string _connectionString;
        public ImportExportRepository(string connectionString)
        {
            _connectionString = connectionString;        }

        private MySqlConnection CreateConnection()
        {
            var connection = new MySqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        public Task<DatabaseResult<bool>> SetLastStanMagazynowyImportDateAsync(string fileName)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    var value = $"{fileName}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                    
                    const string sql = @"
                        INSERT INTO GlobalSettings (setting_key, setting_value)
                        VALUES ('StanMagazynowy', @value)
                        ON DUPLICATE KEY UPDATE setting_value = @value;";
                    
                    using var command = new MySqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@value", value);
                    
                    command.ExecuteNonQuery();                    return DatabaseResult<bool>.Success(true);
                }
                catch (Exception ex)
                {                    return DatabaseResult<bool>.Failure($"Error setting import date: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<(string FileName, DateTime Date)?>> GetLastStanMagazynowyImportDateAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    const string sql = @"
                        SELECT setting_value 
                        FROM GlobalSettings 
                        WHERE setting_key = 'StanMagazynowy' 
                        LIMIT 1;";
                    
                    using var command = new MySqlCommand(sql, connection);
                    var res = command.ExecuteScalar();
                    
                    if (res == null || res == DBNull.Value)
                        return DatabaseResult<(string FileName, DateTime Date)?>.Success(null);
                    var parts = res.ToString()?.Split('|');
                    if (parts == null || parts.Length != 2)
                        return DatabaseResult<(string FileName, DateTime Date)?>.Success(null);
                    
                    if (!DateTime.TryParse(parts[1], out var dt))
                        return DatabaseResult<(string FileName, DateTime Date)?>.Success(null);
                    
                    return DatabaseResult<(string FileName, DateTime Date)?>.Success((parts[0], dt));
                }
                catch (Exception ex)
                {                    return DatabaseResult<(string FileName, DateTime Date)?>.Failure($"Error getting import date: {ex.Message}", ex);
                }
            });
        }
        public Task<DatabaseResult<bool>> AddPnPDataAsync(List<(int r_id, int quantity)> data, long listId, string fileName, string type)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    using var transaction = connection.BeginTransaction();
                    
                    try
                    {
                        const string saveFileNameSql = @"
                            INSERT INTO SideFile (file_name, list_id, type)
                            VALUES (@fileName, @listId, @type);";
                        
                        string safeFileName = (fileName ?? string.Empty).Trim();
                        if (safeFileName.Length > 50) safeFileName = safeFileName.Substring(safeFileName.Length - 50);

                        using (var cmd = new MySqlCommand(saveFileNameSql, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@fileName", safeFileName);
                            cmd.Parameters.AddWithValue("@listId", listId);
                            cmd.Parameters.AddWithValue("@type", type);
                            cmd.ExecuteNonQuery();
                        }
                        const string insertDataSql = @"
                            INSERT INTO SidePcb (r_id, type, quantity, list_id)
                            VALUES (@rId, @type, @quantity, @listId);";
                        
                        foreach (var (rId, quantity) in data)
                        {
                            using var command = new MySqlCommand(insertDataSql, connection, transaction);
                            command.Parameters.AddWithValue("@rId", rId);
                            command.Parameters.AddWithValue("@type", type);
                            command.Parameters.AddWithValue("@quantity", quantity);
                            command.Parameters.AddWithValue("@listId", listId);
                            command.ExecuteNonQuery();
                        }
                        
                        transaction.Commit();                        
                        return DatabaseResult<bool>.Success(true);
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
                catch (Exception ex)
                {                    return DatabaseResult<bool>.Failure($"Error adding PnP data: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<string>> GetSideFileNameAsync(long listId, string type)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    const string sql = @"
                        SELECT file_name 
                        FROM SideFile 
                        WHERE list_id = @listId AND type = @type 
                        LIMIT 1;";
                    
                    using var command = new MySqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@listId", listId);
                    command.Parameters.AddWithValue("@type", type);
                    
                    var result = command.ExecuteScalar();
                    var fileName = result == null || result == DBNull.Value ? string.Empty : result.ToString() ?? string.Empty;
                    
                    return DatabaseResult<string>.Success(fileName);
                }
                catch (Exception ex)
                {                    return DatabaseResult<string>.Failure($"Error getting side file name: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<string[]>> GetComponentSideAsync(int rId, long listId)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    const string sql = @"
                        SELECT type 
                        FROM SidePcb 
                        WHERE r_id = @rId 
                            AND list_id = @listId;";
                    
                    using var command = new MySqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@rId", rId);
                    command.Parameters.AddWithValue("@listId", listId);
                    
                    var sides = new List<string>();
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var side = reader.GetString("type");
                        if (!string.IsNullOrEmpty(side))
                            sides.Add(side);
                    }
                    
                    return DatabaseResult<string[]>.Success(sides.ToArray());
                }
                catch (Exception ex)
                {                    return DatabaseResult<string[]>.Failure($"Error getting component side: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<(string type, int qty)[]>> GetComponentSideAndQtyAsync(int rId, long listId)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    const string sql = @"
                        SELECT type, quantity 
                        FROM SidePcb 
                        WHERE r_id = @rId 
                            AND list_id = @listId;";
                    
                    using var command = new MySqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@rId", rId);
                    command.Parameters.AddWithValue("@listId", listId);
                    
                    using var reader = command.ExecuteReader();

                    var results = new List<(string type, int qty)>();
                    while (reader.Read())
                    {
                        var type = reader.GetString("type");
                        var qty = reader.GetInt32("quantity");
                        results.Add((type, qty));
                    }
                    
                    return DatabaseResult<(string type, int qty)[]>.Success(results.ToArray());
                }
                catch (Exception ex)
                {                    
                    return DatabaseResult<(string type, int qty)[]>.Failure($"Error getting component side and qty: {ex.Message}", ex);
                }
            });
        }
    }
}
