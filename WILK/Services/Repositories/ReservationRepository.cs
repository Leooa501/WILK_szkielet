using Microsoft.VisualBasic;
using MySql.Data.MySqlClient;
using WILK.Models;
using WILK.Services.Constants;
using System.Data;

namespace WILK.Services.Repositories
{

    public interface IReservationRepository
    {
        enum Side { SINGLE, TOP, BOT, THT }

        Task<DatabaseResult<DataTable>> LoadReservationsCompsAsync();
        Task<DatabaseResult<DataTable>> GetReservationsTableAsync();
        Task<DatabaseResult<(int done_top, int done_bot, int start)?>> GetReservationProgressAsync(int reservationId);
        Task<DatabaseResult<bool>> UpdateListReservationAsync(int reservationId, int newDone, Side side);
        Task<DatabaseResult<long>> AddListOfComponentsAsync(string name, int start, bool isSingleSided = false, bool isTHT = false, long existingListId = 0);
        Task<DatabaseResult<bool>> AddReservationComponentAsync(int r_id, int quantity);
        Task<DatabaseResult<bool>> DeleteReservationAsync(ReservationItemDto selected);
        Task<DatabaseResult<bool>> AddListOfComponentsFromExcelAsync(List<(string listName, long componentId, int componentQuantity, int listStart)> excelData);
        Task<DatabaseResult<bool>> ReverseLastUpdateAsync(int reservationId, Side side);
        Task<DatabaseResult<DataTable>> GetListDataAsync(int listId);
        Task<DatabaseResult<bool>> UpdateReservationListAsyncList(List<(string kolName, string kolId, string kolQuantity)> dane, long listId, string Side);
        Task<DatabaseResult<List<(long, string)>>> ClearReservationsAsync(long listId);
        Task<DatabaseResult<bool>> UpdateReservationTHTAsyncList(List<(string kolName, string kolId, string kolQuantity)> dane, long listId);
        Task<DatabaseResult<DataTable>> GetRealReservationAsync(long listId);
        Task<DatabaseResult<bool>> EditRealReservationAsync(int id, int quantity);
        Task<DatabaseResult<bool>> RemoveRealAlternativeAsync(int reservationId, int alternativeId);
        Task<DatabaseResult<bool>> AddRealAlternativeComponentAsync(int reservation_id, int originalRId, int substituteRId, int quantity);
        Task<DatabaseResult<DataTable>> GetDailyUsage(DateTime fromDate);
        Task<DatabaseResult<bool>> UpdateLogsAsync(int reservationId, int newDone, Side side);
        Task<DatabaseResult<DateTime>> GetLastReportDateAsync();
        Task<DatabaseResult<bool>> MoveRealReservationData(List<(long, string)> componentsToUpdate, long listID);
        Task<DatabaseResult<DataTable>> GetReservationsTHTAsync();
        Task<DatabaseResult<bool>> UpdateReservationTHTAsync(int reservationId, int newQuantity);
        Task<DatabaseResult<DataTable>> GetListDataTHTAsync(int listId);
        Task<DatabaseResult<(int done, int start)?>> GetReservationTHTProgressAsync(int reservationId);
        Task<DatabaseResult<DataTable>> GetCompletedReservationsTableTHTAsync();
        Task<DatabaseResult<DataTable>> GetTraceDataTHT(int list_id);
        Task<DatabaseResult<bool>> CloseReservationTHTAsync(int reservationId);
        Task<DatabaseResult<DataTable>> GetAdditionalMaterialsListAsync();
        Task<DatabaseResult<bool>> UpdateTraceAdditionalMaterialsAsync(int listId, (string reel_id, string box, int quantity) data);
        Task<DatabaseResult<DataTable>> GetTraceDataAdditionalMaterialsAsync(int list_id);
        Task<DatabaseResult<DataTable>> GetDataAdditionalMaterialsListAsync(int listId);
        Task<DatabaseResult<bool>> UpdateReservationsAdditionalMaterialsAsync(int listId, (long componentId, int quantity) data);
        Task<DatabaseResult<bool>> CompleteAdditionalMaterialsListAsync(int listId);
        Task<DatabaseResult<bool>> UpdateTransferredStatus(int listId, bool transferred);
        Task<DatabaseResult<int>> AddListOfAdditionalMaterialsAsync(string name, int start);
        Task<DatabaseResult<bool>> AddAdditionalMaterialReservation(List<(int componentId, int quantity)> materials, int listId);
        Task<DatabaseResult<bool>> DeleteAdditionalMaterialsListAsync(int listId);
        Task<DatabaseResult<(bool realizeFlag, string assignedPerson)>> GetMetadata(int listId);
        Task<DatabaseResult<(int done, int start)?>> GetReservationProgressTHTAsync(int reservationId);
        Task<DatabaseResult<DataTable>> LoadReservationTHTAsync(int listId);
        Task<DatabaseResult<DataTable>> GetDraft(int listId);
        Task<DatabaseResult<bool>> UpdateTraceTHT(List<(string reelId, string box, int used)> data, int listId);
        Task<DatabaseResult<bool>> UpdateListReservationTHTAsync(int reservationId, int newDone);
        Task<DatabaseResult<bool>> SaveDraft(int listId, DataTable draftTable);
        Task<DatabaseResult<bool>> UpdateReservationMetadataAsync(int reservationId, bool? realizeFlag = null, DateTime? maxEndDate = null, string? assignedPerson = null, string? destination = null, string? package = null);
        Task<DatabaseResult<bool>> ReverseLastUpdateTHTAsync(int reservationId);
    }

    public class ReservationRepository : IReservationRepository
    {
        private readonly string _connectionString;
        public ReservationRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));        }

        private MySqlConnection CreateConnection()
        {
            var connection = new MySqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        private async Task<MySqlConnection> CreateConnectionAsync()
        {
            var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }

        public Task<DatabaseResult<DataTable>> LoadReservationsCompsAsync()
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
                {                    
                    return DatabaseResult<DataTable>.Failure($"Error loading reservation components: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<DataTable>> LoadReservationTHTAsync(int listId)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    const string sql = @"e";
                    
                    using var command = new MySqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@listId", listId);
                    using var adapter = new MySqlDataAdapter(command);
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    
                    return DatabaseResult<DataTable>.Success(dt);
                }
                catch (Exception ex)
                {                    
                    return DatabaseResult<DataTable>.Failure($"Error loading reservation components: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<DataTable>> GetReservationsTableAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    const string sql = @"e";
                    
                    using var command = new MySqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@status", ListStatusConstants.ZAREZERWOWANE);
                    using var adapter = new MySqlDataAdapter(command);
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    
                    return DatabaseResult<DataTable>.Success(dt);
                }
                catch (Exception ex)
                {                    return DatabaseResult<DataTable>.Failure($"Error getting reservations table: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<(int done_top, int done_bot, int start)?>> GetReservationProgressAsync(int reservationId)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    const string sql = @"e";

                    using var command = new MySqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@id", reservationId);
                    
                    using var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        var done_top = reader.GetInt32("done_top");
                        var done_bot = reader.GetInt32("done_bot");
                        var start = reader.GetInt32("start");
                        return DatabaseResult<(int done_top, int done_bot, int start)?>.Success((done_top, done_bot, start));
                    }
                    
                    return DatabaseResult<(int done_top, int done_bot, int start)?>.Success(null);
                }
                catch (Exception ex)
                {                    
                    return DatabaseResult<(int done_top, int done_bot, int start)?>.Failure($"Error getting reservation progress: {ex.Message}", ex);
                }
            });
        }
        public Task<DatabaseResult<bool>> UpdateListReservationAsync(int reservationId, int newDone, IReservationRepository.Side side)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    
                    var sideStr = side == IReservationRepository.Side.TOP ? "top" : "bot";
                    // Get current 'done' value to store as last update
                    string selectSql = @"e";

                    using var selectCommand = new MySqlCommand(selectSql, connection);
                    selectCommand.Parameters.AddWithValue("@id", reservationId);
                    var currentDone = selectCommand.ExecuteScalar();

                    // Update progress and auto-complete when done >= start
                    string updateSql = @"e";

                    using var updateCommand = new MySqlCommand(updateSql, connection);
                    updateCommand.Parameters.AddWithValue("@id", reservationId);
                    updateCommand.Parameters.AddWithValue("@newDone", newDone);
                    updateCommand.Parameters.AddWithValue("@lastUpdate", currentDone ?? 0);

                    updateCommand.ExecuteNonQuery();  

                    if (side.Equals(IReservationRepository.Side.SINGLE))
                    {
                        updateSql = @"e";

                        using var updateCommand2 = new MySqlCommand(updateSql, connection);
                        updateCommand2.Parameters.AddWithValue("@id", reservationId);
                        updateCommand2.Parameters.AddWithValue("@newDone", newDone);
                        updateCommand2.Parameters.AddWithValue("@lastUpdate", currentDone ?? 0);

                        updateCommand2.ExecuteNonQuery();
                    }

                    string updateStatusSql = @"e";

                    using var statusCommand = new MySqlCommand(updateStatusSql, connection);
                    statusCommand.Parameters.AddWithValue("@status", ListStatusConstants.ZREALIZOWANESMT);
                    statusCommand.Parameters.AddWithValue("@id", reservationId);
                    statusCommand.ExecuteNonQuery();

                    return DatabaseResult<bool>.Success(true);
                }
                catch (Exception ex)
                {                    
                    return DatabaseResult<bool>.Failure($"Error updating reservation: {ex.Message}", ex);
                }
            });
        }

        public async Task<DatabaseResult<long>> AddListOfComponentsAsync(string name, int start, bool isSingleSided = false, bool isTHT = false, long existingListId = 0)
        {
            try
            {
                using var connection = await CreateConnectionAsync();
                using var transaction = await connection.BeginTransactionAsync();
                
                try
                {
                    // Create production list in appropriate table (SMD or THT)
                    var tableName = isTHT ? "ListOfReservationsTHT" : "ListOfReservations";
                    var createListSql = $@"e";
                    if(isTHT)
                    {
                        createListSql = $@"e";
                    }
                    
                    using var createListCommand = new MySqlCommand(createListSql, connection, transaction);
                    createListCommand.Parameters.AddWithValue("@name", name);
                    createListCommand.Parameters.AddWithValue("@start", start);
                    createListCommand.Parameters.AddWithValue("@status", ListStatusConstants.ZAREZERWOWANE);
                    if (!isTHT)
                    {
                        createListCommand.Parameters.AddWithValue("@isOneSided", isSingleSided);
                    }
                    else
                    {
                        createListCommand.Parameters.AddWithValue("@smdListId", existingListId);
                    }

                    var listId = Convert.ToInt64(await createListCommand.ExecuteScalarAsync());

                    await transaction.CommitAsync();                    
                    return DatabaseResult<long>.Success(listId);

                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {                
                return DatabaseResult<long>.Failure($"Error adding list of components: {ex.Message}", ex);
            }
        }

        public async Task<DatabaseResult<bool>> UpdateReservationListAsyncList(List<(string kolName, string kolId, string kolQuantity)> dane, long listId, string side)
        {
            try
            {
                using var connection = CreateConnection();
                using var transaction = connection.BeginTransaction();
                
                try
                {                    
                    const string selectListSql = @"e";
                    using var selectListCommand = new MySqlCommand(selectListSql, connection, transaction);
                    selectListCommand.Parameters.AddWithValue("@id", listId);
                    var startObj = selectListCommand.ExecuteScalar();
                    if (startObj == null)
                    {
                        throw new Exception("List not found");;
                    }
                    int start = Convert.ToInt32(startObj);

                    // Add component reservations linked to this list
                    const string insertReservationSql = @"e";
                    
                    foreach (var (kolName, kolId, kolQuantity) in dane)
                    {
                        if (!int.TryParse(kolId, out var r_id)) continue;
                        if (!int.TryParse(kolQuantity, out var quantity)) continue;
                        
                        using var command = new MySqlCommand(insertReservationSql, connection, transaction);
                        command.Parameters.AddWithValue("@r_id", r_id);
                        command.Parameters.AddWithValue("@quantity", quantity);
                        command.Parameters.AddWithValue("@listId", listId);
                        command.Parameters.AddWithValue("@side", side);
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
            {                
                return DatabaseResult<bool>.Failure($"Error updating reservation list: {ex.Message}", ex);
            }
        }

        public async Task<DatabaseResult<bool>> UpdateReservationTHTAsyncList(List<(string kolName, string kolId, string kolQuantity)> dane, long listId)
        {
            try
            {
                using var connection = CreateConnection();
                using var transaction = connection.BeginTransaction();
                
                try
                {                    
                    const string selectListSql = @"e";
                    using var selectListCommand = new MySqlCommand(selectListSql, connection, transaction);
                    selectListCommand.Parameters.AddWithValue("@id", listId);
                    var startObj = selectListCommand.ExecuteScalar();
                    if (startObj == null)
                    {
                        throw new Exception("List not found");;
                    }
                    int start = Convert.ToInt32(startObj);

                    // Add component reservations linked to this list
                    const string insertReservationSql = @"e";

                    var componentParts = new Dictionary<int, (int r_id, int partQuantity, int totalQuantity)>();
                    // Pobranie danych z tabeli ComponentParts do słownika dla szybkiego dostępu
                    using (var cpCmd = new MySqlCommand("SELECT part_id, r_id, part_quantity, total_quantity FROM ComponentParts", connection, transaction))
                    using (var reader = cpCmd.ExecuteReader())
                    {
                            while (reader.Read())
                        {
                                componentParts[reader.GetInt32(0)] = (
                                reader.GetInt32(1),
                                reader.GetInt32(2),
                                reader.GetInt32(3)
                            );
                        }
                    }
                    
                    foreach (var (kolName, kolId, kolQuantity) in dane)
                    {
                        if (!int.TryParse(kolId, out var r_id)) continue;
                        if (!int.TryParse(kolQuantity, out var quantity)) continue;

                        // Podmiana r_id z tabeli ComponentParts o pasujących r_id = parts_id wraz z zaokrągloną nową qty (PD-3)
                        if (componentParts.TryGetValue((int)r_id, out var part))
                        {
                            r_id = part.r_id; 
                            decimal newQuantity = (decimal)quantity * part.partQuantity / part.totalQuantity;
                            quantity = (int)Math.Ceiling(newQuantity);
                        }
                        
                        using var command = new MySqlCommand(insertReservationSql, connection, transaction);
                        command.Parameters.AddWithValue("@r_id", r_id);
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
            {                
                return DatabaseResult<bool>.Failure($"Error updating reservation list: {ex.Message}", ex);
            }
        }

        public async Task<DatabaseResult<bool>> AddReservationComponentAsync(int r_id, int quantity)
        {
            try
            {
                using var connection = await CreateConnectionAsync();
                const string sql = @"e";
                
                using var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@r_id", r_id);
                command.Parameters.AddWithValue("@quantity", quantity);
                
                await command.ExecuteNonQueryAsync();                
                return DatabaseResult<bool>.Success(true);
            }
            catch (Exception ex)
            {                return DatabaseResult<bool>.Failure($"Error adding reservation component: {ex.Message}", ex);
            }
        }

        public Task<DatabaseResult<bool>> DeleteReservationAsync(ReservationItemDto selected)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    using var transaction = connection.BeginTransaction();
                    
                    try
                    {
                        if (selected.IsList)
                        {
                            const string deleteReservationsSql = @"e";

                            using var deleteReservationsCommand = new MySqlCommand(deleteReservationsSql, connection, transaction);
                            deleteReservationsCommand.Parameters.AddWithValue("@id", selected.Id);
                            deleteReservationsCommand.ExecuteNonQuery();
                            
                            const string deleteListSql = @"e";

                            using var deleteListCommand = new MySqlCommand(deleteListSql, connection, transaction);
                            deleteListCommand.Parameters.AddWithValue("@id", selected.Id);
                            deleteListCommand.ExecuteNonQuery();
                        }
                        else
                        {
                            const string sql = @"e";

                            using var command = new MySqlCommand(sql, connection, transaction);
                            command.Parameters.AddWithValue("@id", selected.Id);
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
                {                    return DatabaseResult<bool>.Failure($"Error deleting reservation: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<bool>> AddListOfComponentsFromExcelAsync(List<(string listName, long componentId, int componentQuantity, int listStart)> excelData)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    using var transaction = connection.BeginTransaction();
                    
                    try
                    {
                        string lastListName = "";
                        long currentListId = 0;

                        foreach (var row in excelData)
                        {
                            if (currentListId == 0 || row.listName != lastListName)
                            {
                                const string insertListQuery = @"e";

                                using var cmd = new MySqlCommand(insertListQuery, connection, transaction);
                                cmd.Parameters.AddWithValue("@name", row.listName);
                                cmd.Parameters.AddWithValue("@start", row.listStart);
                                cmd.Parameters.AddWithValue("@status", ListStatusConstants.ZAREZERWOWANE);
                                currentListId = Convert.ToInt64(cmd.ExecuteScalar());

                                lastListName = row.listName;
                            }

                            const string insertComponentQuery = @"e";

                            long componentIdInDb;
                            using (var cmd = new MySqlCommand(insertComponentQuery, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@r_id", row.componentId);
                                cmd.Parameters.AddWithValue("@name", row.listName);
                                componentIdInDb = Convert.ToInt64(cmd.ExecuteScalar());
                            }

                            const string insertReservationQuery = @"
                                INSERT INTO Reservations (components_id, quantity, list_id)
                                VALUES (@componentId, @quantity, @listId);";

                            using (var cmd = new MySqlCommand(insertReservationQuery, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@componentId", componentIdInDb);
                                cmd.Parameters.AddWithValue("@quantity", row.componentQuantity);
                                cmd.Parameters.AddWithValue("@listId", currentListId);
                                cmd.ExecuteNonQuery();
                            }
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
                {                    return DatabaseResult<bool>.Failure($"Error adding list from Excel: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<bool>> ReverseLastUpdateAsync(int reservationId, IReservationRepository.Side side)
        {
            return Task.Run(() =>
            {
                try
                {
                    // Cjheck if the list is one-sided
                    const string selectList = @"e";

                    using var connectionCheck = CreateConnection();
                    using var commandCheck = new MySqlCommand(selectList, connectionCheck);
                    commandCheck.Parameters.AddWithValue("@id", reservationId);
                    var isOneSidedObj = commandCheck.ExecuteScalar();
                    if (isOneSidedObj == null)
                    {
                        throw new Exception("List not found");;
                    }
                    bool isOneSided = Convert.ToBoolean(isOneSidedObj);

                    // If one-sided, update both sides
                    if(isOneSided)
                    {
                        using var connectionBoth = CreateConnection();

                        // Insert top-side change log
                        string sqlLogTop = @"e";
                        using var commandLogTop = new MySqlCommand(sqlLogTop, connectionBoth);
                        commandLogTop.Parameters.AddWithValue("@reservationId", reservationId);
                        commandLogTop.ExecuteNonQuery();

                        string sqlBoth = $@"e";
                        using var commandBoth = new MySqlCommand(sqlBoth, connectionBoth);
                        commandBoth.Parameters.AddWithValue("@id", reservationId);
                        commandBoth.ExecuteNonQuery();

                        return DatabaseResult<bool>.Success(true);
                    }

                    // Otherwise, update only the specified side
                    var sideStr = side == IReservationRepository.Side.TOP ? "top" : "bot";

                    using var connection = CreateConnection();

                    string sqlLogsSingle = @"e";
                    using var commandLogsSingle = new MySqlCommand(sqlLogsSingle, connection);
                    commandLogsSingle.Parameters.AddWithValue("@reservationId", reservationId);
                    commandLogsSingle.Parameters.AddWithValue("@side", sideStr);
                    commandLogsSingle.ExecuteNonQuery();

                    string sql = $@"UPDATE ListOfReservations 
                                        SET done_{sideStr} = last_update_done_{sideStr}, updatedAt = NOW()
                                        WHERE id = @id;";
                    
                    using var command = new MySqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@id", reservationId);
                    
                    command.ExecuteNonQuery();                    return DatabaseResult<bool>.Success(true);
                }
                catch (Exception ex)
                {                    
                    return DatabaseResult<bool>.Failure($"Error reversing last update: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<DataTable>> GetListDataAsync(int listId)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    const string sql = @"e";
                    
                    using var command = new MySqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@ListId", listId);
                    
                    var dt = new DataTable();
                    dt.Columns.Add("name", typeof(string));
                    dt.Columns.Add("elementId", typeof(int));
                    dt.Columns.Add("quantity", typeof(int));
                    dt.Columns.Add("type", typeof(string));
                    
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        if (reader.GetString("type") != "SMD" && reader.GetString("type") != "PCB")
                            continue;
                        
                        dt.Rows.Add(
                            reader.GetString("name"),
                            reader.GetInt32("elementId"),
                            reader.GetInt32("quantity"),
                            reader.GetString("type")
                        );
                    }
                    
                    return DatabaseResult<DataTable>.Success(dt);
                }
                catch (Exception ex)
                {                    
                    return DatabaseResult<DataTable>.Failure($"Error getting list data: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<DataTable>> GetListDataTHTAsync(int listId)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    const string sql = @"e";
                    
                    using var command = new MySqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@ListId", listId);
                    
                    var dt = new DataTable();
                    dt.Columns.Add("name", typeof(string));
                    dt.Columns.Add("elementId", typeof(int));
                    dt.Columns.Add("quantity", typeof(int));
                    dt.Columns.Add("type", typeof(string));
                    
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        if (reader.GetString("type") != "THT")
                            continue;
                        
                        dt.Rows.Add(
                            reader.GetString("name"),
                            reader.GetInt32("elementId"),
                            reader.GetInt32("quantity"),
                            reader.GetString("type")
                        );
                    }
                    
                    return DatabaseResult<DataTable>.Success(dt);
                }
                catch (Exception ex)
                {                    
                    return DatabaseResult<DataTable>.Failure($"Error getting list data: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<List<(long, string)>>> ClearReservationsAsync(long listId)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connectionCheck = CreateConnection();
                    const string checkSql = @"e";
                    using var checkCommand = new MySqlCommand(checkSql, connectionCheck);
                    checkCommand.Parameters.AddWithValue("@ListId", listId);
                    using var reader = checkCommand.ExecuteReader();
                    var componentsToUpdate = new List<(long, string)>();
                    while (reader.Read())
                    {
                        var componentId = reader.GetInt64("components_id");
                        var side = reader.GetString("side");
                        componentsToUpdate.Add((componentId, side));
                    }
                    reader.Close();

                    using var connection = CreateConnection();
                    string sql = "e";                    
                    using var command = new MySqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@ListId", listId);

                    command.ExecuteNonQuery();

                    return DatabaseResult<List<(long, string)>>.Success(componentsToUpdate);
                }
                catch (Exception ex)
                {
                    return DatabaseResult<List<(long, string)>>.Failure($"Error clearing reservations: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<DataTable>> GetRealReservationAsync(long listId)
        {
            return Task.Run(() =>
            {
                try
                {
                    var result = new DataTable();
                    result.Columns.Add("id", typeof(int));
                    result.Columns.Add("components_name", typeof(string));
                    result.Columns.Add("r_id", typeof(int));
                    result.Columns.Add("quantity", typeof(int));
                    result.Columns.Add("is_alternative", typeof(bool));
                    result.Columns.Add("alternative_id", typeof(int));
                    result.Columns.Add("alternative_name", typeof(string));
                    result.Columns.Add("alternative_r_id", typeof(int));
                    result.Columns.Add("using_quantity", typeof(int));

                    using var connection = CreateConnection();
                    string sql = "e";
                    
                    using var command = new MySqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@ListId", listId);

                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var have_alternative = reader.IsDBNull("have_alternative") ? (bool?)null : reader.GetBoolean("have_alternative");
                        if(have_alternative.HasValue && have_alternative.Value == true)
                        {
                            // Get alternative component details
                            string altSql = "e";
                            using var altCommand = new MySqlCommand(altSql, CreateConnection());
                            altCommand.Parameters.AddWithValue("@reservationId", reader.GetInt32("id"));
                            using var altReader = altCommand.ExecuteReader();
                            while(altReader.Read())
                            {
                                result.Rows.Add(
                                    reader.GetInt32("id"),
                                    reader.GetString("name"),
                                    reader.GetInt32("r_id"),
                                    altReader.GetInt32("quantity"),
                                    true,
                                    altReader.GetInt32("substitute_id"),
                                    altReader.GetString("name"),
                                    altReader.GetInt32("r_id"),
                                    0
                                );
                            }
                        }
                        result.Rows.Add(
                            reader.GetInt32("id"),
                            reader.GetString("name"),
                            reader.GetInt32("r_id"),
                            reader.GetInt32("quantity"),
                            false,
                            DBNull.Value,
                            DBNull.Value,
                            DBNull.Value,
                            reader.IsDBNull("using_quantity") ? (int?)null : reader.GetInt32("using_quantity")
                        );
                    
                    }

                    return DatabaseResult<DataTable>.Success(result);
                }
                catch (Exception ex)
                {
                    return DatabaseResult<DataTable>.Failure($"Error getting list data: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<bool>> EditRealReservationAsync(int id, int quantity)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    string sql = "e";
                    
                    using var command = new MySqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@quantity", quantity);
                    command.Parameters.AddWithValue("@id", id);
                    
                    command.ExecuteNonQuery();
                    return DatabaseResult<bool>.Success(true);
                }
                catch (Exception ex)
                {
                    return DatabaseResult<bool>.Failure($"Error editing real reservation: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<bool>> RemoveRealAlternativeAsync(int reservationId, int alternativeId)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    string sql = @"e";
                    
                    using var command = new MySqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@id", reservationId);
                    command.Parameters.AddWithValue("@alternativeId", alternativeId);
                    
                    command.ExecuteNonQuery();

                    string checkSql = "e";
                    using var checkCommand = new MySqlCommand(checkSql, connection);
                    checkCommand.Parameters.AddWithValue("@id", reservationId);
                    var countObj = checkCommand.ExecuteScalar();
                    int count = Convert.ToInt32(countObj);
                    if (count > 0)
                    {
                        // Still have alternatives, do not clear flag
                        return DatabaseResult<bool>.Success(true);
                    }

                    string updateSql = "e";
                    using var updateCommand = new MySqlCommand(updateSql, connection);
                    updateCommand.Parameters.AddWithValue("@id", reservationId);
                    updateCommand.ExecuteNonQuery();

                    return DatabaseResult<bool>.Success(true);
                }
                catch (Exception ex)
                {
                    return DatabaseResult<bool>.Failure($"Error removing real alternative: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<bool>> AddRealAlternativeComponentAsync(int reservation_id, int originalRId, int substituteRId, int quantity)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    using var transaction = connection.BeginTransaction();
                    try
                    {
                        // Resolve component internal IDs
                        var getOrigCmd = new MySqlCommand("e", connection, transaction);
                        getOrigCmd.Parameters.AddWithValue("@originalRId", originalRId);
                        var origObj = getOrigCmd.ExecuteScalar();
                        if (origObj == null) throw new Exception("Original component not found");
                        int origCompId = Convert.ToInt32(origObj);

                        var getSubCompCmd = new MySqlCommand("e", connection, transaction);
                        getSubCompCmd.Parameters.AddWithValue("@substituteRId", substituteRId);
                        var subObj = getSubCompCmd.ExecuteScalar();
                        if (subObj == null) throw new Exception("Substitute component not found");
                        int subCompId = Convert.ToInt32(subObj);

                        // Find or create Substitute mapping
                        var getSubstituteIdCmd = new MySqlCommand("e", connection, transaction);
                        getSubstituteIdCmd.Parameters.AddWithValue("@origCompId", origCompId);
                        getSubstituteIdCmd.Parameters.AddWithValue("@subCompId", subCompId);
                        var substituteIdObj = getSubstituteIdCmd.ExecuteScalar();
                        int substituteId;
                        if (substituteIdObj == null)
                        {
                            throw new Exception("Ten element nie jest zdefiniowany jako zamiennik.");
                        }
                        else
                        {
                            substituteId = Convert.ToInt32(substituteIdObj);
                        }

                        // If a ReservationSubstitute for this reservation/substitute already exists, increment quantity; otherwise insert new row
                        var checkCmd = new MySqlCommand("e", connection, transaction);
                        checkCmd.Parameters.AddWithValue("@reservation_id", reservation_id);
                        checkCmd.Parameters.AddWithValue("@substituteId", substituteId);
                        var existing = checkCmd.ExecuteScalar();
                        if (existing != null)
                        {
                            var updateCmd = new MySqlCommand("e", connection, transaction);
                            updateCmd.Parameters.AddWithValue("@quantity", quantity);
                            updateCmd.Parameters.AddWithValue("@reservation_id", reservation_id);
                            updateCmd.Parameters.AddWithValue("@substituteId", substituteId);
                            updateCmd.ExecuteNonQuery();
                        }
                        else
                        {
                            var insertCmd = new MySqlCommand("e", connection, transaction);
                            insertCmd.Parameters.AddWithValue("@substituteId", substituteId);
                            insertCmd.Parameters.AddWithValue("@quantity", quantity);
                            insertCmd.Parameters.AddWithValue("@reservation_id", reservation_id);
                            insertCmd.ExecuteNonQuery();
                        }

                        // Mark reservation as having alternative
                        var updateResCmd = new MySqlCommand("e", connection, transaction);
                        updateResCmd.Parameters.AddWithValue("@reservation_id", reservation_id);
                        updateResCmd.ExecuteNonQuery();

                        transaction.Commit();
                        return DatabaseResult<bool>.Success(true);
                    }
                    catch (Exception exTrans)
                    {
                        try { transaction.Rollback(); } catch { }
                        throw exTrans;
                    }
                }
                catch (Exception ex)
                {
                    return DatabaseResult<bool>.Failure($"Error adding alternative component: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<DataTable>> GetDailyUsage(DateTime fromDate)
        {
            return Task.Run(() =>
            {
                try
                {
                    fromDate = fromDate.Date;

                    var result = new DataTable();
                    result.Columns.Add("r_id", typeof(int));
                    result.Columns.Add("daily_usage", typeof(int));
                    result.Columns.Add("list_name", typeof(string));
                    result.Columns.Add("list_id", typeof(int));
                    result.Columns.Add("report_date", typeof(DateTime));

                    using var connection = CreateConnection();
                    
                    // Get lists that have logs and need reports
                    var listsToProcessSMD = GetListsFromLogs(fromDate, false);
                    var listsToProcessTHT = GetListsFromLogs(fromDate, true);

                    // Process SMD lists
                    foreach(var listId in listsToProcessSMD)
                    {
                        string listData = @"e";
                        using var listCommand = new MySqlCommand(listData, connection);
                        listCommand.Parameters.AddWithValue("@listId", listId);
                        using var listReader = listCommand.ExecuteReader();
                        if(!listReader.Read())
                        {
                            continue;
                        }
                        var listName = listReader.GetString("name");
                        bool isOneSided = listReader.GetBoolean("is_one_sided");
                        int doneTop = listReader.GetInt32("done_top");
                        int doneBot = listReader.GetInt32("done_bot");
                        int start = listReader.GetInt32("start");
                        listReader.Close();

                        var dateUsageDict = new Dictionary<(int r_id, DateTime date), int>();

                        string logData = @"e";
                        using var logCommand = new MySqlCommand(logData, connection);
                        logCommand.Parameters.AddWithValue("@listId", listId);
                        logCommand.Parameters.AddWithValue("@fromDate", fromDate);
                        logCommand.Parameters.AddWithValue("@start", start);
                        using var logReader = logCommand.ExecuteReader();
                        while (logReader.Read())
                        {
                            var side = logReader.GetString("side");
                            var dailyUsage = logReader.GetInt32("daily_usage");
                            var date = logReader.GetDateTime("date").Date;
                            var r_id = logReader.GetInt32("r_id");

                            var key = (r_id, date);
                            if (!dateUsageDict.ContainsKey(key))
                            {
                                dateUsageDict[key] = 0;
                            }
                            dateUsageDict[key] += dailyUsage;
                        }
                        logReader.Close();

                        // Compute usage before fromDate per component (to subtract from alternative usage)
                        var usedBeforeDict = new Dictionary<int, int>();
                        string usedBeforeSql = @"e";
                        using (var usedBeforeCmd = new MySqlCommand(usedBeforeSql, connection))
                        {
                            usedBeforeCmd.Parameters.AddWithValue("@listId", listId);
                            usedBeforeCmd.Parameters.AddWithValue("@fromDate", fromDate);
                            usedBeforeCmd.Parameters.AddWithValue("@start", start);
                            using var usedBeforeReader = usedBeforeCmd.ExecuteReader();
                            while (usedBeforeReader.Read())
                            {
                                var r_id = usedBeforeReader.GetInt32("r_id");
                                var usageBefore = usedBeforeReader.IsDBNull("usage_before") ? 0.0 : Convert.ToDouble(usedBeforeReader["usage_before"]);
                                usedBeforeDict[r_id] = (int)Math.Round(usageBefore, MidpointRounding.AwayFromZero);
                            }
                            usedBeforeReader.Close();
                        }


                        // Build a substitutes map for the whole list: original -> list of (altId, altQty)
                        var substitutesMap = GetSubstitutesForList(listId);

                        // Distribute usage per day, consuming original then alternatives in chronological order
                        var usageByDate = dateUsageDict
                            .GroupBy(kv => kv.Key.date)
                            .OrderBy(g => g.Key)
                            .ToList();

                        // Prepare available pools per original component (account for usedBefore)
                        var originalAvailable = new Dictionary<int, int>();
                        foreach (var originalId in dateUsageDict.Select(kv => kv.Key.r_id).Distinct())
                        {
                            var originalTotal = GetOriginalTotalQuantity(listId, originalId);
                            var usedBefore = usedBeforeDict.TryGetValue(originalId, out var ub) ? ub : 0;
                            originalAvailable[originalId] = Math.Max(0, originalTotal - usedBefore);
                        }

                        // Prepare copy of substitutes availability we can mutate per allocation
                        var substitutesAvailable = substitutesMap.ToDictionary(
                            kv => kv.Key,
                            kv => kv.Value.Select(t => (altId: t.altId, qty: t.qty)).ToList()
                        );

                        foreach (var dateGroup in usageByDate)
                        {
                            var date = dateGroup.Key;
                            var byOriginal = dateGroup.GroupBy(kv => kv.Key.r_id).ToDictionary(g => g.Key, g => g.Sum(kv => kv.Value));

                            foreach (var kv in byOriginal)
                            {
                                var originalId = kv.Key;
                                var remaining = kv.Value;

                                // ensure pools exist
                                if (!originalAvailable.ContainsKey(originalId)) originalAvailable[originalId] = GetOriginalTotalQuantity(listId, originalId) - (usedBeforeDict.TryGetValue(originalId, out var ub2) ? ub2 : 0);
                                if (!substitutesAvailable.ContainsKey(originalId)) substitutesAvailable[originalId] = new List<(int altId, int qty)>();

                                // consume original availability first
                                var origAvail = originalAvailable[originalId];
                                var takeOrig = Math.Min(origAvail, remaining);
                                if (takeOrig > 0)
                                {
                                    result.Rows.Add(originalId, takeOrig, listName, listId, date);
                                    originalAvailable[originalId] = origAvail - takeOrig;
                                    remaining -= takeOrig;
                                }

                                // then consume alternatives in order
                                if (remaining > 0)
                                {
                                    var altList = substitutesAvailable[originalId];
                                    for (int i = 0; i < altList.Count && remaining > 0; i++)
                                    {
                                        var alt = altList[i];
                                        var take = Math.Min(remaining, alt.qty);
                                        if (take > 0)
                                        {
                                            result.Rows.Add(alt.altId, take, listName, listId, date);
                                            remaining -= take;
                                            altList[i] = (alt.altId, alt.qty - take);
                                        }
                                    }
                                }

                                if (remaining > 0)
                                {
                                    // still remaining (more used than available across original+alts) - report as overflow on original
                                    return DatabaseResult<DataTable>.Failure("Usage exceeds available quantity including alternatives");
                                }
                            }
                        }
                        
                    }

                    // Process THT lists (no sides, components type THT)
                    foreach (var listId in listsToProcessTHT)
                    {
                        string listDataTHT = @"e";
                        using var listCommandTHT = new MySqlCommand(listDataTHT, connection);
                        listCommandTHT.Parameters.AddWithValue("@listId", listId);
                        using var listReaderTHT = listCommandTHT.ExecuteReader();
                        if (!listReaderTHT.Read())
                        {
                            continue;
                        }
                        var listNameTHT = listReaderTHT.GetString("name");
                        int startTHT = listReaderTHT.GetInt32("start");
                        int smdListId = listReaderTHT.GetInt32("smd_list_id");
                        listReaderTHT.Close();

                        var dateUsageDictTHT = new Dictionary<(int r_id, DateTime date), int>();

                        string logDataTHT = @"e";
                        using var logCommandTHT = new MySqlCommand(logDataTHT, connection);
                        logCommandTHT.Parameters.AddWithValue("@listId", listId);
                        logCommandTHT.Parameters.AddWithValue("@start", startTHT);
                        logCommandTHT.Parameters.AddWithValue("@fromDate", fromDate);
                        using var logReaderTHT = logCommandTHT.ExecuteReader();
                        while (logReaderTHT.Read())
                        {
                            var dailyUsage = logReaderTHT.GetInt32("daily_usage");
                            var date = logReaderTHT.GetDateTime("date").Date;
                            var r_id = logReaderTHT.GetInt32("r_id");

                            var key = (r_id, date);
                            if (!dateUsageDictTHT.ContainsKey(key)) dateUsageDictTHT[key] = 0;
                            dateUsageDictTHT[key] += dailyUsage;
                        }
                        logReaderTHT.Close();

                        // Compute usage before fromDate per component (to subtract from alternative usage)
                        var usedBeforeDictTHT = new Dictionary<int, int>();
                        string usedBeforeSqlTHT = @"e";
                        using (var usedBeforeCmd = new MySqlCommand(usedBeforeSqlTHT, connection))
                        {
                            usedBeforeCmd.Parameters.AddWithValue("@listId", listId);
                            usedBeforeCmd.Parameters.AddWithValue("@fromDate", fromDate);
                            usedBeforeCmd.Parameters.AddWithValue("@start", startTHT);
                            using var usedBeforeReader = usedBeforeCmd.ExecuteReader();
                            while (usedBeforeReader.Read())
                            {
                                var r_id = usedBeforeReader.GetInt32("r_id");
                                var usageBefore = usedBeforeReader.IsDBNull("usage_before") ? 0.0 : Convert.ToDouble(usedBeforeReader["usage_before"]);
                                usedBeforeDictTHT[r_id] = (int)Math.Round(usageBefore, MidpointRounding.AwayFromZero);
                            }
                            usedBeforeReader.Close();
                        }

                        var substitutesMapTHT = GetSubstitutesForListTHT(listId);

                        // Distribute THT usage per day, consuming original then alternatives in chronological order
                        var usageByDateTHT = dateUsageDictTHT
                            .GroupBy(kv => kv.Key.date)
                            .OrderBy(g => g.Key)
                            .ToList();

                        var originalAvailableTHT = new Dictionary<int, int>();
                        foreach (var originalId in dateUsageDictTHT.Select(kv => kv.Key.r_id).Distinct())
                        {
                            var originalTotal = GetOriginalTotalQuantityTHT(listId, originalId);
                            var usedBefore = usedBeforeDictTHT.TryGetValue(originalId, out var ub) ? ub : 0;
                            originalAvailableTHT[originalId] = Math.Max(0, originalTotal - usedBefore);
                        }

                        var substitutesAvailableTHT = substitutesMapTHT.ToDictionary(
                            kv => kv.Key,
                            kv => kv.Value.Select(t => (altId: t.altId, qty: t.qty)).ToList()
                        );

                        foreach (var dateGroup in usageByDateTHT)
                        {
                            var date = dateGroup.Key;
                            var byOriginal = dateGroup.GroupBy(kv => kv.Key.r_id).ToDictionary(g => g.Key, g => g.Sum(kv => kv.Value));

                            foreach (var kv in byOriginal)
                            {
                                var originalId = kv.Key;
                                var remaining = kv.Value;

                                if (!originalAvailableTHT.ContainsKey(originalId)) originalAvailableTHT[originalId] = GetOriginalTotalQuantityTHT(listId, originalId) - (usedBeforeDictTHT.TryGetValue(originalId, out var ub2) ? ub2 : 0);
                                if (!substitutesAvailableTHT.ContainsKey(originalId)) substitutesAvailableTHT[originalId] = new List<(int altId, int qty)>();

                                var origAvail = originalAvailableTHT[originalId];
                                var takeOrig = Math.Min(origAvail, remaining);
                                if (takeOrig > 0)
                                {
                                    result.Rows.Add(originalId, takeOrig, listNameTHT, smdListId, date);
                                    originalAvailableTHT[originalId] = origAvail - takeOrig;
                                    remaining -= takeOrig;
                                }

                                if (remaining > 0)
                                {
                                    var altList = substitutesAvailableTHT[originalId];
                                    for (int i = 0; i < altList.Count && remaining > 0; i++)
                                    {
                                        var alt = altList[i];
                                        var take = Math.Min(remaining, alt.qty);
                                        if (take > 0)
                                        {
                                            result.Rows.Add(alt.altId, take, listNameTHT, listId, date);
                                            remaining -= take;
                                            altList[i] = (alt.altId, alt.qty - take);
                                        }
                                    }
                                }

                                if (remaining > 0)
                                {
                                    return DatabaseResult<DataTable>.Failure("THT usage exceeds available quantity including alternatives");
                                }
                            }
                        }
                    }

                    // Add excessive usage
                    var excessiveUsage = GetExcessiveUsage(fromDate);
                    if (excessiveUsage.Count > 0)
                    {
                        foreach ((int r_id, int quantity, DateTime createdAt) in excessiveUsage)
                        {
                            result.Rows.Add(r_id, quantity, "Ponadnormatywne", -1, createdAt.Date);
                        }
                    }

                    var sqlUpdate =  @"e";

                    using var updateCommand = new MySqlCommand(sqlUpdate, connection);
                    updateCommand.Parameters.AddWithValue("@lastReportDate", DateTime.Now);
                    updateCommand.ExecuteNonQuery();

                    return DatabaseResult<DataTable>.Success(result);

                }catch (Exception ex)
                {
                    return DatabaseResult<DataTable>.Failure($"Error getting daily usage: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<bool>> UpdateLogsAsync(int reservationId, int newDone, IReservationRepository.Side side)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    string sideStr = side == IReservationRepository.Side.TOP ? "top" : side == IReservationRepository.Side.BOT ? "bot" : side == IReservationRepository.Side.SINGLE ? "SINGLE" : "THT";
                    // Get current done value
                    string sideCheckSql = @"e";

                    using var sideCheckCommand = new MySqlCommand(sideCheckSql, connection);
                    sideCheckCommand.Parameters.AddWithValue("@id", reservationId);
                    var sideCheckReader = sideCheckCommand.ExecuteReader();
                    if (!sideCheckReader.Read())
                    {
                        throw new Exception("List not found");;
                    }
                    var isOneSidedObj = sideCheckReader["is_one_sided"];
                    var currentDone = sideCheckReader["current_done"];
                    sideCheckReader.Close();
                    bool isOneSided = isOneSidedObj != null && Convert.ToBoolean(isOneSidedObj);
                    if(!isOneSided && side == IReservationRepository.Side.SINGLE)
                    {
                        // For one-sided lists, when updating SINGLE, we need to log both sides
                        string logSqlTop = @"e";
                        using var logCommandTop = new MySqlCommand(logSqlTop, connection);
                        logCommandTop.Parameters.AddWithValue("@reservationId", reservationId);
                        logCommandTop.Parameters.AddWithValue("@newDone", newDone);
                        logCommandTop.ExecuteNonQuery();

                        string logSqlBot = @"e";
                        using var logCommandBot = new MySqlCommand(logSqlBot, connection);
                        logCommandBot.Parameters.AddWithValue("@reservationId", reservationId);
                        logCommandBot.Parameters.AddWithValue("@newDone", newDone);
                        logCommandBot.ExecuteNonQuery();

                        return DatabaseResult<bool>.Success(true);
                    }

                    string logSql = @"
                        INSERT INTO ReservationsLogs (`list_id`, `side`, `change`, `date`) VALUES (@reservationId, @side, @change, NOW());";
                    using var logCommand = new MySqlCommand(logSql, connection);
                    logCommand.Parameters.AddWithValue("@reservationId", reservationId);
                    logCommand.Parameters.AddWithValue("@side", isOneSided ? "SINGLE" : sideStr);
                    logCommand.Parameters.AddWithValue("@change", newDone - Convert.ToInt32(currentDone ?? 0));
                    logCommand.ExecuteNonQuery();

                    return DatabaseResult<bool>.Success(true);
                }
                catch (Exception ex)
                {
                    return DatabaseResult<bool>.Failure($"Error updating logs: {ex.Message}", ex);
                }
            });
        }

        private List<int> GetListsFromLogs(DateTime fromDate, bool tht = false)
        {
            using var connection = CreateConnection();
            fromDate = fromDate.Date;

            if(!tht)
            {
                string sqlListsToProcess = @"e";
                    
                using var commandLists = new MySqlCommand(sqlListsToProcess, connection);
                commandLists.Parameters.AddWithValue("@status", ListStatusConstants.ZAREZERWOWANE);
                commandLists.Parameters.AddWithValue("@fromDate", fromDate);
                var listsToProcess = new List<int>();
                using (var readerLists = commandLists.ExecuteReader())
                {
                    while (readerLists.Read())
                    {
                        listsToProcess.Add(readerLists.GetInt32("list_id"));
                    }
                }
                return listsToProcess;
            }else
            {
                string sqlThtListsToProcess = @"e";
                    
                using var commandLists = new MySqlCommand(sqlThtListsToProcess, connection);
                commandLists.Parameters.AddWithValue("@status", ListStatusConstants.ZAREZERWOWANE);
                commandLists.Parameters.AddWithValue("@fromDate", fromDate);
                var listsToProcess = new List<int>();
                using (var readerLists = commandLists.ExecuteReader())
                {
                    while (readerLists.Read())
                    {
                        listsToProcess.Add(readerLists.GetInt32("list_id"));
                    }
                }
                return listsToProcess;
            }
            
        }

        private Dictionary<int, List<(int altId, int qty)>> GetSubstitutesForList(int listId)
        {
            var map = new Dictionary<int, List<(int altId, int qty)>>();
            using var conn = CreateConnection();
            string sql = @"e";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@listId", listId);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                var orig = rdr.GetInt32("original_r_id");
                var alt = rdr.GetInt32("alternative_r_id");
                var qty = rdr.IsDBNull("alt_total") ? 0 : Convert.ToInt32(rdr["alt_total"]);
                if (!map.ContainsKey(orig)) map[orig] = new List<(int, int)>();
                map[orig].Add((alt, qty));
            }
            return map;
        }

        private int GetOriginalTotalQuantity(int listId, int originalRId)
        {
            using var conn = CreateConnection();
            string sql = @"e";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@listId", listId);
            cmd.Parameters.AddWithValue("@r_id", originalRId);
            var obj = cmd.ExecuteScalar();
            return obj == null || obj == DBNull.Value ? 0 : Convert.ToInt32(obj);
        }

        private Dictionary<int, List<(int altId, int qty)>> GetSubstitutesForListTHT(int listId)
        {
            var map = new Dictionary<int, List<(int altId, int qty)>>();
            using var conn = CreateConnection();
            string sql = @"e";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@listId", listId);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                var orig = rdr.GetInt32("original_r_id");
                var alt = rdr.GetInt32("alternative_r_id");
                var qty = rdr.IsDBNull("alt_total") ? 0 : Convert.ToInt32(rdr["alt_total"]);
                if (!map.ContainsKey(orig)) map[orig] = new List<(int, int)>();
                map[orig].Add((alt, qty));
            }
            return map;
        }

        private int GetOriginalTotalQuantityTHT(int listId, int originalRId)
        {
            using var conn = CreateConnection();
            string sql = @"e";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@listId", listId);
            cmd.Parameters.AddWithValue("@r_id", originalRId);
            var obj = cmd.ExecuteScalar();
            return obj == null || obj == DBNull.Value ? 0 : Convert.ToInt32(obj);
        }

        private List<(int, int, DateTime)> GetExcessiveUsage(DateTime fromDate)
        {
            var result = new List<(int, int, DateTime)>();
            using var connection = CreateConnection();
            string sql = @"e";
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@fromDate", fromDate.Date);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var r_id = reader.IsDBNull("r_id") ? 0 : reader.GetInt32("r_id");
                if (r_id == 0) continue; // skip malformed rows
                int totalExcessive = reader.IsDBNull("total_excessive") ? 0 : reader.GetInt32("total_excessive");
                DateTime createdAt = reader.IsDBNull("created_at") ? DateTime.MinValue : reader.GetDateTime("created_at");
                result.Add((r_id, totalExcessive, createdAt));
            }
            return result;
        }

        public Task<DatabaseResult<DateTime>> GetLastReportDateAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    string sql = "e";
                    
                    using var command = new MySqlCommand(sql, connection);
                    var resultObj = command.ExecuteScalar();
                    if (resultObj == DBNull.Value || resultObj == null)
                    {
                        return DatabaseResult<DateTime>.Success(DateTime.MinValue);
                    }
                    DateTime lastReportDate = Convert.ToDateTime(resultObj);
                    return DatabaseResult<DateTime>.Success(lastReportDate);
                }
                catch (Exception ex)
                {
                    return DatabaseResult<DateTime>.Failure($"Error getting last report date: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<bool>> MoveRealReservationData(List<(long, string)> componentsToUpdate, long listID)
        {
            return Task.Run(() =>
            {
                var connection = CreateConnection();
                try
                {
                    var sql = @"e";
                    using var command = new MySqlCommand(sql, connection);
                    foreach (var (componentId, side) in componentsToUpdate)
                    {
                        command.Parameters.Clear();

                        var connectionb = CreateConnection();
                        var sqlb = @"SELECT id FROM Reservations WHERE list_id = @listId AND components_id = @componentId LIMIT 1;";
                        using var commandb = new MySqlCommand(sqlb, connectionb);
                        commandb.Parameters.AddWithValue("@listId", listID);
                        commandb.Parameters.AddWithValue("@componentId", componentId);
                        var reservationIdObj = commandb.ExecuteScalar();
                        if (reservationIdObj == null || reservationIdObj == DBNull.Value)
                        {
                            continue; // skip if no matching reservation found
                        }
                        long reservationId = Convert.ToInt64(reservationIdObj);

                        command.Parameters.AddWithValue("@reservationId", reservationId);
                        command.Parameters.AddWithValue("@componentId", componentId);
                        command.ExecuteNonQuery();

                        var connectionc = CreateConnection();
                        var sqlc = @"UPDATE Reservations SET have_alternative = 1, using_quantity = quantity - (SELECT SUM(quantity) FROM ReservationSubstitute WHERE reservation_id = Reservations.id) WHERE id = @reservationId;";
                        using var commandc = new MySqlCommand(sqlc, connectionc);
                        commandc.Parameters.AddWithValue("@reservationId", reservationId);
                        commandc.ExecuteNonQuery();
                    }

                    return DatabaseResult<bool>.Success(true);
                }
                catch (Exception ex)
                {
                    return DatabaseResult<bool>.Failure($"Error moving real reservation data: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<DataTable>> GetReservationsTHTAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    const string sql = @"e";
                    
                    using var command = new MySqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@status", ListStatusConstants.ZAREZERWOWANE);
                    using var adapter = new MySqlDataAdapter(command);
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    
                    return DatabaseResult<DataTable>.Success(dt);
                }
                catch (Exception ex)
                {                    
                    return DatabaseResult<DataTable>.Failure($"Error getting reservations table: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<bool>> UpdateReservationTHTAsync(int reservationId, int newQuantity)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    string sql = @"e";

                    using var command = new MySqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@newQuantity", newQuantity);
                    command.Parameters.AddWithValue("@reservationId", reservationId);
                    command.ExecuteNonQuery();
                    return DatabaseResult<bool>.Success(true);
                }
                catch (Exception ex)
                {
                    return DatabaseResult<bool>.Failure($"Error updating THT reservation: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<(int done, int start)?>> GetReservationTHTProgressAsync(int reservationId)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    string sql = @"e";

                    using var command = new MySqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@reservationId", reservationId);
                    using var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        int done = reader.IsDBNull("done") ? 0 : reader.GetInt32("done");
                        int start = reader.IsDBNull("start") ? 0 : reader.GetInt32("start");
                        return DatabaseResult<(int done, int start)?>.Success((done, start));
                    }
                    else
                    {
                        return DatabaseResult<(int done, int start)?>.Success(null);
                    }
                }
                catch (Exception ex)
                {
                    return DatabaseResult<(int done, int start)?>.Failure($"Error getting THT reservation progress: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<DataTable>> GetCompletedReservationsTableTHTAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    string sql = @"e";

                    using var command = new MySqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@status", ListStatusConstants.ZREALIZOWANESMT);
                    var dataTable = new DataTable();
                    using var reader = command.ExecuteReader();
                    dataTable.Load(reader);
                    return DatabaseResult<DataTable>.Success(dataTable);
                }
                catch (Exception ex)
                {
                    return DatabaseResult<DataTable>.Failure($"Error getting completed THT reservations: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<DataTable>> GetTraceDataTHT(int list_id)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    string sql = @"e";

                    using var command = new MySqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@listId", list_id);
                    var dataTable = new DataTable();
                    using var reader = command.ExecuteReader();
                    dataTable.Load(reader);
                    return DatabaseResult<DataTable>.Success(dataTable);
                }
                catch (Exception ex)
                {
                    return DatabaseResult<DataTable>.Failure($"Error getting trace data for THT list {list_id}: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<bool>> CloseReservationTHTAsync(int reservationId)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    string sql = @"e";

                    using var command = new MySqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@status", ListStatusConstants.THT_COMPLETED);
                    command.Parameters.AddWithValue("@reservationId", reservationId);
                    command.ExecuteNonQuery();
                    return DatabaseResult<bool>.Success(true);
                }
                catch (Exception ex)
                {
                    return DatabaseResult<bool>.Failure($"Error closing THT reservation: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<DataTable>> GetAdditionalMaterialsListAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    string sql = @"e";

                    using var command = new MySqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@status", ListStatusConstants.ZAREZERWOWANE);
                    var dataTable = new DataTable();
                    using var reader = command.ExecuteReader();
                    dataTable.Load(reader);
                    return DatabaseResult<DataTable>.Success(dataTable);
                }
                catch (Exception ex)
                {
                    return DatabaseResult<DataTable>.Failure($"Error getting additional materials lists: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<bool>> UpdateTraceAdditionalMaterialsAsync(int listId, (string reel_id, string box, int quantity) data)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    string sql = @"e";

                    using var command = new MySqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@listId", listId);
                    command.Parameters.AddWithValue("@reelId", data.reel_id);
                    command.Parameters.AddWithValue("@box", data.box);
                    command.Parameters.AddWithValue("@quantity", data.quantity);
                    command.ExecuteNonQuery();
                    return DatabaseResult<bool>.Success(true);
                }
                catch (Exception ex)
                {
                    return DatabaseResult<bool>.Failure($"Error updating trace for additional materials list {listId}: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<DataTable>> GetTraceDataAdditionalMaterialsAsync(int list_id)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    string sql = @"e";

                    using var command = new MySqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@listId", list_id);
                    var dataTable = new DataTable();
                    using var reader = command.ExecuteReader();
                    dataTable.Load(reader);
                    return DatabaseResult<DataTable>.Success(dataTable);
                }
                catch (Exception ex)
                {
                    return DatabaseResult<DataTable>.Failure($"Error getting trace data for additional materials list {list_id}: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<DataTable>> GetDataAdditionalMaterialsListAsync(int listId)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    string sql = @"e";

                    using var command = new MySqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@listId", listId);
                    var dataTable = new DataTable();
                    using var reader = command.ExecuteReader();
                    dataTable.Load(reader);
                    return DatabaseResult<DataTable>.Success(dataTable);
                }
                catch (Exception ex)
                {
                    return DatabaseResult<DataTable>.Failure($"Error getting data for additional materials list {listId}: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<bool>> UpdateReservationsAdditionalMaterialsAsync(int listId, (long componentId, int quantity) data)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    string sql = @"e";

                    using var command = new MySqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@listId", listId);
                    command.Parameters.AddWithValue("@componentId", data.componentId);
                    command.Parameters.AddWithValue("@quantity", data.quantity);
                    command.ExecuteNonQuery();
                    return DatabaseResult<bool>.Success(true);
                }
                catch (Exception ex)
                {
                    return DatabaseResult<bool>.Failure($"Error updating reservations for additional materials list {listId}: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<bool>> CompleteAdditionalMaterialsListAsync(int listId)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    string sql = @"e";

                    using var command = new MySqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@status", ListStatusConstants.ZREALIZOWANESMT);
                    command.Parameters.AddWithValue("@listId", listId);
                    command.ExecuteNonQuery();
                    return DatabaseResult<bool>.Success(true);
                }
                catch (Exception ex)
                {
                    return DatabaseResult<bool>.Failure($"Error completing additional materials list {listId}: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<bool>> UpdateTransferredStatus(int listId, bool transferred)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    string sql = @"e";

                    using var command = new MySqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@transferred", transferred);
                    command.Parameters.AddWithValue("@listId", listId);
                    command.ExecuteNonQuery();
                    return DatabaseResult<bool>.Success(true);
                }
                catch (Exception ex)
                {
                    return DatabaseResult<bool>.Failure($"Error updating transferred status for THT list {listId}: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<int>> AddListOfAdditionalMaterialsAsync(string name, int start)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    string sql = @"e";

                    using var command = new MySqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@status", ListStatusConstants.ZAREZERWOWANE);
                    command.Parameters.AddWithValue("@start", start);
                    var resultObj = command.ExecuteScalar();
                    int newId = Convert.ToInt32(resultObj);
                    return DatabaseResult<int>.Success(newId);
                }
                catch (Exception ex)
                {
                    return DatabaseResult<int>.Failure($"Error adding new additional materials list: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<bool>> AddAdditionalMaterialReservation(List<(int componentId, int quantity)> materials, int listId)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    string sql = @"e";

                    using var command = new MySqlCommand(sql, connection);
                    foreach (var (componentId, quantity) in materials)
                    {
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@listId", listId);
                        command.Parameters.AddWithValue("@componentId", componentId);
                        command.Parameters.AddWithValue("@quantity", quantity);
                        command.ExecuteNonQuery();
                    }
                    return DatabaseResult<bool>.Success(true);
                }
                catch (Exception ex)
                {
                    return DatabaseResult<bool>.Failure($"Error adding reservations for additional materials list {listId}: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<bool>> DeleteAdditionalMaterialsListAsync(int listId)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    string sqlDeleteReservations = @"e";
                    using var commandDeleteReservations = new MySqlCommand(sqlDeleteReservations, connection);
                    commandDeleteReservations.Parameters.AddWithValue("@listId", listId);
                    commandDeleteReservations.ExecuteNonQuery();

                    string sqlDeleteList = @"e";
                    using var commandDeleteList = new MySqlCommand(sqlDeleteList, connection);
                    commandDeleteList.Parameters.AddWithValue("@listId", listId);
                    commandDeleteList.ExecuteNonQuery();

                    return DatabaseResult<bool>.Success(true);
                }
                catch (Exception ex)
                {
                    return DatabaseResult<bool>.Failure($"Error deleting additional materials list {listId}: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<(bool realizeFlag, string assignedPerson)>> GetMetadata(int listId)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    string sql = "e";
                    
                    using var command = new MySqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@listId", listId);
                    using var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        bool realizeFlag = reader.IsDBNull("realizeFlag") ? false : reader.GetBoolean("realizeFlag");
                        string assignedPerson = reader.IsDBNull("assignedPerson") ? string.Empty : reader.GetString("assignedPerson");
                        return DatabaseResult<(bool realizeFlag, string assignedPerson)>.Success((realizeFlag, assignedPerson));
                    }
                    else
                    {
                        return DatabaseResult<(bool realizeFlag, string assignedPerson)>.Success((false, string.Empty));
                    }
                }
                catch (Exception ex)
                {
                    return DatabaseResult<(bool realizeFlag, string assignedPerson)>.Failure($"Error getting list metadata: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<(int done, int start)?>> GetReservationProgressTHTAsync(int reservationId)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    const string sql = @"e";

                    using var command = new MySqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@id", reservationId);
                    
                    using var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        var done = reader.GetInt32("done");
                        var start = reader.GetInt32("start");
                        return DatabaseResult<(int done, int start)?>.Success((done, start));
                    }
                    
                    return DatabaseResult<(int done, int start)?>.Success(null);
                }
                catch (Exception ex)
                {                    
                    return DatabaseResult<(int done, int start)?>.Failure($"Error getting reservation progress: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<DataTable>> GetDraft(int listId)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    string sql = @"e";
                    using var command = new MySqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@listId", listId);
                    using var adapter = new MySqlDataAdapter(command);
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    
                    return DatabaseResult<DataTable>.Success(dt);
                }
                catch (Exception ex)
                {                    
                    return DatabaseResult<DataTable>.Failure($"Error loading draft components: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<bool>> UpdateTraceTHT(List<(string reelId, string box, int used)> data, int listId)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    using var transaction = connection.BeginTransaction();

                    string sql = @"e";

                    using var command = new MySqlCommand(sql, connection, transaction);
                    foreach (var (reelId, box, used) in data)
                    {
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@reelId", reelId);
                        command.Parameters.AddWithValue("@box", box);
                        command.Parameters.AddWithValue("@used", used);
                        command.Parameters.AddWithValue("@listId", listId);
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    return DatabaseResult<bool>.Success(true);
                }
                catch (Exception ex)
                {
                    return DatabaseResult<bool>.Failure($"Error updating THT trace: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<bool>> UpdateListReservationTHTAsync(int reservationId, int newDone)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    
                    // Get current 'done' value to store as last update
                    string selectSql = @"e";

                    using var selectCommand = new MySqlCommand(selectSql, connection);
                    selectCommand.Parameters.AddWithValue("@id", reservationId);
                    var currentDone = selectCommand.ExecuteScalar();

                    // Update progress and auto-complete when done >= start
                    string updateSql = @"e";

                    using var updateCommand = new MySqlCommand(updateSql, connection);
                    updateCommand.Parameters.AddWithValue("@id", reservationId);
                    updateCommand.Parameters.AddWithValue("@newDone", newDone);
                    updateCommand.Parameters.AddWithValue("@lastUpdate", currentDone ?? 0);
                    updateCommand.Parameters.AddWithValue("@completedStatus", ListStatusConstants.ZREALIZOWANESMT);

                    updateCommand.ExecuteNonQuery();

                    // Use escaped column names and include a timestamp for consistency with other log inserts
                    string log = @"e";
                    using var logCommand = new MySqlCommand(log, connection);
                    logCommand.Parameters.AddWithValue("@id", reservationId);
                    logCommand.Parameters.AddWithValue("@newDone", newDone - int.Parse(currentDone.ToString() ?? "0"));
                    logCommand.Parameters.AddWithValue("@side", "THT");

                    logCommand.ExecuteNonQuery();  

                    return DatabaseResult<bool>.Success(true);
                }
                catch (Exception ex)
                {                    
                    return DatabaseResult<bool>.Failure($"Error updating reservation: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<bool>> SaveDraft(int listId, DataTable draftTable)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    using var transaction = connection.BeginTransaction();

                    string deleteSql = "e";
                    using var deleteCommand = new MySqlCommand(deleteSql, connection, transaction);
                    deleteCommand.Parameters.AddWithValue("@listId", listId);
                    deleteCommand.ExecuteNonQuery();

                    string insertSql = @"e";
                    using var insertCommand = new MySqlCommand(insertSql, connection, transaction);

                    foreach (DataRow row in draftTable.Rows)
                    {
                        insertCommand.Parameters.Clear();
                        insertCommand.Parameters.AddWithValue("@id_reel", row["reel_id"]?.ToString() ?? string.Empty);
                        insertCommand.Parameters.AddWithValue("@box", row["box"]?.ToString() ?? string.Empty);
                        insertCommand.Parameters.AddWithValue("@quantity", row["used_quantity"] ?? 0);
                        insertCommand.Parameters.AddWithValue("@listId", listId);
                        insertCommand.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    return DatabaseResult<bool>.Success(true);
                }
                catch (Exception ex)
                {
                    return DatabaseResult<bool>.Failure($"Error saving draft: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<bool>> UpdateReservationMetadataAsync(int reservationId, bool? realizeFlag = null, DateTime? maxEndDate = null, string? assignedPerson = null, string? destination = null, string? package = null)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    var updates = new List<string>();
                    using var command = new MySqlCommand();
                    command.Connection = connection;

                    if (realizeFlag.HasValue)
                    {
                        updates.Add("realizeFlag = @realizeFlag");
                        command.Parameters.AddWithValue("@realizeFlag", realizeFlag.Value ? 1 : 0);
                    }

                    if (maxEndDate != null)
                    {
                        updates.Add("maxEndDate = @maxEndDate");
                        command.Parameters.AddWithValue("@maxEndDate", maxEndDate.Value.Date);
                    }

                    if (assignedPerson != null)
                    {
                        updates.Add("assignedPerson = @assignedPerson");
                        command.Parameters.AddWithValue("@assignedPerson", assignedPerson);
                    }

                    if (destination != null)
                    {
                        updates.Add("destination = @destination");
                        command.Parameters.AddWithValue("@destination", destination);
                    }

                    if (package != null)
                    {
                        updates.Add("package = @package");
                        command.Parameters.AddWithValue("@package", package);
                    }

                    if (updates.Count == 0)
                        return DatabaseResult<bool>.Success(true);

                    string sql = $"e";
                    command.CommandText = sql;
                    command.Parameters.AddWithValue("@id", reservationId);

                    command.ExecuteNonQuery();

                    return DatabaseResult<bool>.Success(true);
                }
                catch (Exception ex)
                {
                    return DatabaseResult<bool>.Failure($"Error updating reservation metadata: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<bool>> ReverseLastUpdateTHTAsync(int reservationId)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();

                    string sql = $@"e";
                    
                    using var command = new MySqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@id", reservationId);
                    
                    command.ExecuteNonQuery();                    
                    return DatabaseResult<bool>.Success(true);
                }
                catch (Exception ex)
                {                    
                    return DatabaseResult<bool>.Failure($"Error reversing last update: {ex.Message}", ex);
                }
            });
        }
    }
}