using MySql.Data.MySqlClient;
using WILK.Models;
using System.Data;

using MProductionStorageLib.DB;
using MProductionStorageLib.Model;

namespace WILK.Services.Repositories
{
    /// <summary>
    /// Repository for component-related database operations
    /// </summary>
    public interface IComponentRepository
    {
        Task<DatabaseResult<string>> GetComponentNameByRIdAsync(int rId);
        Task<DatabaseResult<int>> GetComponentIdByRIdAsync(int rId);
        Task<DatabaseResult<string>> GetComponentTypeAsync(int componentId);
        Task<DatabaseResult<bool>> UpdateComponentsAsync( string [] IDs = null);
        Task<DatabaseResult<DataTable>> SearchComponentsAsync(string? id, string? namePrefix);
        Task<DatabaseResult<bool>> AddComponentAsync(int id, string name, string type);
        Task<DatabaseResult<bool>> UpdateComponentAsync(int id, string name, string type);
    }

    public class ComponentRepository : IComponentRepository
    {
        private readonly string _connectionString;
        public ComponentRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        private MySqlConnection CreateConnection()
        {
            var connection = new MySqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        public Task<DatabaseResult<bool>> UpdateComponentsAsync(string [] IDs = null)
        {
            return Task.Run(() =>
            {
                try
                {
                    List<ElementAmount> componentsCount;
                    if (IDs == null || IDs.Length == 0)
                    {
                        componentsCount = ElementAmount.GetElementAmounts();
                    }
                    else
                    {
                        componentsCount = ElementAmount.GetElementAmounts(IDs);
                    }

                    var components = componentsCount
                        .GroupBy(c => c.Type)
                        .Select(g => (g.Key, g.Sum(x => (decimal)x.Amount)))
                        .ToList();

                        using var connection = CreateConnection();
                        using var transaction = connection.BeginTransaction();

                        try
                        {
                            if (components.Any())
                            {
                                const int batchSize = 500;
                                var batches = components
                                    .Select((comp, index) => new { comp, index })
                                    .GroupBy(x => x.index / batchSize)
                                    .Select(g => g.Select(x => x.comp).ToList());

                                foreach (var batch in batches)
                                {
                                    var valueParams = new List<string>();
                                    using var command = new MySqlCommand { Connection = connection, Transaction = transaction };

                                    for (int i = 0; i < batch.Count; i++)
                                    {
                                        valueParams.Add($"(@r_id{i}, @quantity{i})");
                                        command.Parameters.AddWithValue($"@r_id{i}", batch[i].Key);
                                        command.Parameters.AddWithValue($"@quantity{i}", batch[i].Item2);
                                    }

                                    command.CommandText = $@"e";

                                    command.ExecuteNonQuery();
                                }
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
                    {
                        return DatabaseResult<bool>.Failure($"Error updating components: {ex.Message}", ex);
                    }
            });
        }

        public Task<DatabaseResult<string>> GetComponentNameByRIdAsync(int rId)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    using var command = new MySqlCommand(@"e", connection);

                    command.Parameters.AddWithValue("@r_id", rId);


                    var result = command.ExecuteScalar();
                    var name = result?.ToString() ?? string.Empty;

                    if(string.IsNullOrEmpty(name))
                    {
                        return DatabaseResult<string>.Failure("Component not found.");
                    }
                    return DatabaseResult<string>.Success(name);
                }
                catch (Exception ex)
                {
                    return DatabaseResult<string>.Failure($"Error getting component name: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<int>> GetComponentIdByRIdAsync(int rId)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    using var command = new MySqlCommand(@"e", connection);

                    command.Parameters.AddWithValue("@r_id", rId);


                    var result = command.ExecuteScalar();
                    var id = result != null ? Convert.ToInt32(result) : -1;


                    return DatabaseResult<int>.Success(id);
                }
                catch (Exception ex)
                {
                    return DatabaseResult<int>.Failure($"Error getting component ID: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<string>> GetComponentTypeAsync(int componentId)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    using var command = new MySqlCommand(@"e", connection);


                    command.Parameters.AddWithValue("@id", componentId);


                    var result = command.ExecuteScalar();
                    var type = result?.ToString() ?? string.Empty;


                    return DatabaseResult<string>.Success(type);
                }
                catch (Exception ex)
                {
                    return DatabaseResult<string>.Failure($"Error getting component type: {ex.Message}", ex);
                }
            });
        }
        public Task<DatabaseResult<DataTable>> SearchComponentsAsync(string? id, string? namePrefix)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    
                    string query = "SELECT r_id, name, quantity, type FROM Components";
                    
                    if (!string.IsNullOrEmpty(id) || !string.IsNullOrEmpty(namePrefix))
                    {
                        query += " WHERE ";
                        
                        var conditions = new System.Collections.Generic.List<string>();

                        if (!string.IsNullOrEmpty(id)) conditions.Add("CAST(r_id AS CHAR) LIKE @id");
                        if (!string.IsNullOrEmpty(namePrefix)) conditions.Add("name LIKE @name");
                        query += string.Join(" AND ", conditions);
                    }
     
                    query += " ORDER BY r_id ASC LIMIT 100";

                    using var command = new MySqlCommand(query, connection);
                    
                    if (!string.IsNullOrEmpty(id)) command.Parameters.AddWithValue("@id", id + "%");
                    if (!string.IsNullOrEmpty(namePrefix)) command.Parameters.AddWithValue("@name", namePrefix + "%");

                    var dt = new DataTable();
                    using (var adapter = new MySqlDataAdapter(command))
                    {
                        adapter.Fill(dt);
                    }
                    return DatabaseResult<DataTable>.Success(dt);
                }
                catch (Exception ex)
                {
                    return DatabaseResult<DataTable>.Failure($"Błąd szukania: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<bool>> AddComponentAsync(int id, string name, string type)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    
                    using (var checkCmd = new MySqlCommand("e", connection))
                    {
                        checkCmd.Parameters.AddWithValue("@id", id);
                        if (Convert.ToInt64(checkCmd.ExecuteScalar()) > 0)
                            return DatabaseResult<bool>.Failure("To ID już istnieje w bazie!");
                    }

                    string sql = "INSERT INTO Components (r_id, name, type, quantity) VALUES (@id, @name, @type, 0)";
                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        command.Parameters.AddWithValue("@name", name);
                        command.Parameters.AddWithValue("@type", type);
                        command.ExecuteNonQuery();
                    }
                    return DatabaseResult<bool>.Success(true);
                }
                catch (Exception ex)
                {
                    return DatabaseResult<bool>.Failure($"Błąd dodawania: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<bool>> UpdateComponentAsync(int id, string name, string type)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    string sql = "e";
                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        command.Parameters.AddWithValue("@name", name);
                        command.Parameters.AddWithValue("@type", type);
                        if (command.ExecuteNonQuery() == 0) return DatabaseResult<bool>.Failure("Nie znaleziono rekordu.");
                    }
                    return DatabaseResult<bool>.Success(true);
                }
                catch (Exception ex)
                {
                    return DatabaseResult<bool>.Failure($"Błąd aktualizacji: {ex.Message}", ex);
                }
            });
        }
    }
}
