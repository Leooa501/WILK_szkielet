using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using WILK.Models;

using MProductionStorageLib.DB;
using MProductionStorageLib.Model;

namespace WILK.Services.Repositories
{
    /// <summary>
    /// Repository for alternative component substitution management
    /// </summary>
    public interface IAlternativeRepository
    {
        Task<DatabaseResult<List<ComponentDto>>> GetAlternativeComponentsAsync(int originalComponentId);
        Task<DatabaseResult<DataTable>> GetAlternativeComponentsTableAsync();
        Task<DatabaseResult<bool>> AddAlternativeComponentAsync(int originalRId, int substituteRId);
        Task<DatabaseResult<bool>> DeleteAlternativeComponentAsync(int alternativeId);
        Task<DatabaseResult<bool>> AddListOfAlternativesAsync(List<(string Kol1, string Kol2)> altList);
    }

    public class AlternativeRepository : IAlternativeRepository
    {
        private readonly string _connectionString;
        public AlternativeRepository(string connectionString)
        {
            _connectionString = connectionString;        
        }

        private MySqlConnection CreateConnection()
        {
            var connection = new MySqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        public Task<DatabaseResult<List<ComponentDto>>> GetAlternativeComponentsAsync(int originalComponentId)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    const string sql = @"e";
                    
                    using var command = new MySqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@originalComponentId", originalComponentId);
                    
                    var alternatives = new List<ComponentDto>();
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        alternatives.Add(new ComponentDto
                        {
                            Id = reader.GetInt32("id"),
                            RId = reader.GetInt32("RId"),
                            Name = reader.GetString("Name"),
                            Type = reader.GetString("Type"),
                            Quantity = reader.GetInt32("Quantity"),
                            CreatedAt = DateTime.Now,
                            UpdatedAt = null
                        });
                    }
                    
                    return DatabaseResult<List<ComponentDto>>.Success(alternatives);
                }
                catch (Exception ex)
                {                    return DatabaseResult<List<ComponentDto>>.Failure($"Error getting alternative components: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<DataTable>> GetAlternativeComponentsTableAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    const string sql = @"e";
                    
                    using var command = new MySqlCommand(sql, connection);
                    using var adapter = new MySqlDataAdapter(command);
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    
                    return DatabaseResult<DataTable>.Success(dt);
                }
                catch (Exception ex)
                {                    return DatabaseResult<DataTable>.Failure($"Error getting alternative components table: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<bool>> AddAlternativeComponentAsync(int originalRId, int substituteRId)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    const string sql = @"e";
                    
                    using var command = new MySqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@originalRId", originalRId);
                    command.Parameters.AddWithValue("@substituteRId", substituteRId);
                    
                    command.ExecuteNonQuery();                    return DatabaseResult<bool>.Success(true);
                }
                catch (Exception ex)
                {                    return DatabaseResult<bool>.Failure($"Error adding alternative component: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<bool>> DeleteAlternativeComponentAsync(int alternativeId)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    const string sql = "e";
                    
                    using var command = new MySqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@id", alternativeId);
                    
                    command.ExecuteNonQuery();                    return DatabaseResult<bool>.Success(true);
                }
                catch (Exception ex)
                {                    return DatabaseResult<bool>.Failure($"Error deleting alternative component: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<bool>> AddListOfAlternativesAsync(List<(string Kol1, string Kol2)> altList)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    using var transaction = connection.BeginTransaction();
                    
                    try
                    {
                        const string deleteSql = "e";
                        using (var deleteCmd = new MySqlCommand(deleteSql, connection, transaction))
                        {
                            deleteCmd.ExecuteNonQuery();
                        }
                        
                        const string sql = @"e";
                        
                        foreach (var (kol1, kol2) in altList)
                        {
                            if (!int.TryParse(kol1, out var originalRId)) continue;
                            if (!int.TryParse(kol2, out var substituteRId)) continue;
                            
                            using var command = new MySqlCommand(sql, connection, transaction);
                            command.Parameters.AddWithValue("@originalRId", originalRId);
                            command.Parameters.AddWithValue("@substituteRId", substituteRId);
                            command.ExecuteNonQuery();
                        }
                        
                        transaction.Commit();                        return DatabaseResult<bool>.Success(true);
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
                catch (Exception ex)
                {                    return DatabaseResult<bool>.Failure($"Error adding list of alternatives: {ex.Message}", ex);
                }
            });
        }
    }
}
