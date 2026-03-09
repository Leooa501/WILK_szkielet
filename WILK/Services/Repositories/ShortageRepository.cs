using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using MySql.Data.MySqlClient;
using WILK.Models;
using WILK.Services.Constants;

namespace WILK.Services.Repositories
{
    /// <summary>
    /// Repository for component shortage calculation and reporting
    /// </summary>
    public interface IShortageRepository
    {
        Task<DatabaseResult<List<(int componentId, string componentName, int brakQuantity, int rId)>>> GetBrakiAsync(bool onlySMD = true);
        Task<DatabaseResult<List<(int componentId, string componentName, int brakQuantity, int rId)>>> GetBrakiForListAsync(List<ExcelRow> listOfComponents);
        Task<DatabaseResult<bool>> ExportBrakiToExcelAsync(List<(int componentId, string componentName, int brakQuantity, int rId)> braki, string filePath);
        Task<DatabaseResult<List<(int componentId, string componentName, int Quantity, int rId)>>> GetComponentAmountAsync(List<ExcelRow> listOfComponents);
    }

    public class ShortageRepository : IShortageRepository
    {
        private readonly string _connectionString;
        public ShortageRepository(string connectionString)
        {
            _connectionString = connectionString;        }

        private MySqlConnection CreateConnection()
        {
            var connection = new MySqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        public Task<DatabaseResult<List<(int componentId, string componentName, int brakQuantity, int rId)>>> GetBrakiAsync(bool onlySMD = true)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    var braki = new List<(int componentId, string componentName, int brakQuantity, int rId)>();
                    
                    // Step 1: Load components with current inventory
                    // id = internal database ID for calculations
                    // r_id = external reference ID for Excel export
                    var components = new Dictionary<int, (string name, int quantity, int rId)>();
                    string componentsSql = onlySMD 
                        ? "SELECT id, r_id, name, quantity FROM Components WHERE Type = 'SMD' OR Type = 'PCB'" 
                        : "SELECT id, r_id, name, quantity FROM Components";
                    
                    using (var cmd = new MySqlCommand(componentsSql, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32("id");
                            int rId = reader.GetInt32("r_id");
                            string name = reader.IsDBNull(reader.GetOrdinal("name")) ? "Unknown" : reader.GetString("name");

                            if(reader.IsDBNull(reader.GetOrdinal("quantity")))
                                continue;
                            int quantity = reader.GetInt32("quantity");
                            components[id] = (name, quantity, rId);
                        }
                    }

                    // Step 2: Sum reservations without production list
                    var reservationsWithoutList = new Dictionary<int, int>();
                    using (var cmd = new MySqlCommand(@"
                    SELECT components_id, SUM(quantity) AS total
                    FROM Reservations
                    WHERE list_id IS NULL
                    GROUP BY components_id", connection))

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int compId = reader.GetInt32("components_id");
                            int total = reader.GetInt32("total");
                            reservationsWithoutList[compId] = total;
                        }
                    }

                    // Step 3: Load reservations with production lists (only active lists)
                    var reservationsWithList = new List<(int componentId, int listStart, int listDone, int resQuantity)>();
                    using (var cmd = new MySqlCommand(@"
                        SELECT r.components_id, r.quantity, l.start, l.done_bot, l.done_top, r.side
                        FROM Reservations r
                        JOIN ListOfReservations l ON r.list_id = l.id
                        WHERE r.list_id IS NOT NULL AND l.status = @status", connection))
                    {
                        cmd.Parameters.AddWithValue("@status", ListStatusConstants.ZAREZERWOWANE);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (reader.IsDBNull(reader.GetOrdinal("components_id")))
                                    continue;
                                
                                int compId = reader.GetInt32("components_id");
                                int quantity = reader.IsDBNull(reader.GetOrdinal("quantity")) ? 0 : reader.GetInt32("quantity");
                                int start = reader.IsDBNull(reader.GetOrdinal("start")) ? 0 : reader.GetInt32("start");

                                int doneBot = reader.IsDBNull(reader.GetOrdinal("done_bot")) ? 0 : reader.GetInt32("done_bot");
                                int doneTop = reader.IsDBNull(reader.GetOrdinal("done_top")) ? 0 : reader.GetInt32("done_top");
                                string side = reader.IsDBNull(reader.GetOrdinal("side")) ? "" : reader.GetString("side");

                                int done = side == "TOP" ? doneTop : doneBot;
                                reservationsWithList.Add((compId, start, done, quantity));
                            }
                        }
                    }

                    var reservationsTHT = new List<(int componentId, int quantity)>();

                    if(!onlySMD)
                    {
                        // Step 3b: Include THT reservations    
                        using (var cmd = new MySqlCommand(@"
                            SELECT c.r_id, l.id, r.quantity, c.name, c.id AS c_id
                            FROM ReservationsTHT r
                            JOIN Components c ON c.id = r.components_id
                            JOIN ListOfReservationsTHT l ON r.list_id = l.id
                            WHERE r.list_id IS NOT NULL AND l.status = @status", connection))
                        {
                            cmd.Parameters.AddWithValue("@status", ListStatusConstants.ZAREZERWOWANE);

                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    if (reader.IsDBNull(reader.GetOrdinal("r_id")))
                                        continue;
                                    
                                    int listId = reader.GetInt32("id");
                                    int quantity = reader.IsDBNull(reader.GetOrdinal("quantity")) ? 0 : reader.GetInt32("quantity");

                                    var sql = "SELECT SUM(quantity) FROM TraceTHT WHERE list_id = @listId AND reel_id LIKE CONCAT(@reel, '%') GROUP BY reel_id";
                                    int resQuantity = 0;
                                    var connection1 = CreateConnection();
                                    using (var resCmd = new MySqlCommand(sql, connection1))
                                    {
                                        resCmd.Parameters.AddWithValue("@listId", listId);
                                        resCmd.Parameters.AddWithValue("@reel", reader.GetInt32("r_id"));
                                        using (var resReader = resCmd.ExecuteReader())
                                        {
                                            while(resReader.Read())
                                                resQuantity += resReader.GetInt32(0);
                                        }
                                    }
                                    reservationsTHT.Add((reader.GetInt32("c_id"), quantity - resQuantity));
                                }
                            }
                        }
                    }

                    // 4. Obliczanie braków
                    foreach (var kvp in components)
                    {
                        int id = kvp.Key;
                        string name = kvp.Value.name;
                        int available = kvp.Value.quantity;
                        int rId = kvp.Value.rId;

                        if (reservationsWithoutList.ContainsKey(id))
                            available -= reservationsWithoutList[id];

                        foreach (var res in reservationsWithList.Where(r => r.componentId == id))
                        {
                            int elementsPerBox = res.resQuantity / res.listStart;
                            int boxesRemaining = res.listStart - res.listDone;

                            int remaining = elementsPerBox * boxesRemaining;

                            available -= remaining;
                        }

                        if (!onlySMD)
                        {
                            foreach (var thtRes in reservationsTHT.Where(r => r.componentId == id))
                            {
                                available -= thtRes.quantity;
                            }
                        }                    

                        if (available < 0)
                            braki.Add((id, name, -available, rId));
                    }
                    
                    return DatabaseResult<List<(int componentId, string componentName, int brakQuantity, int rId)>>.Success(braki);
                }
                catch (Exception ex)
                {                    
                    return DatabaseResult<List<(int componentId, string componentName, int brakQuantity, int rId)>>.Failure($"Error calculating shortages: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<List<(int componentId, string componentName, int brakQuantity, int rId)>>> GetBrakiForListAsync(List<ExcelRow> listOfComponents)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    var braki = new List<(int componentId, string componentName, int brakQuantity, int rId)>();
                    
                    // Zamieniamy ExcelRecord na słownik ID -> ilość
                    var excelDict = listOfComponents.ToDictionary(
                    x =>
                    {
                        int id;
                        if (!int.TryParse(x.Id, out id)) id = 99999;
                        return id;
                    },
                    x =>
                    {
                        int qty;
                        if (!int.TryParse(x.Quantity, out qty)) qty = 0;
                        return qty;
                    }
                    );  

                    {

                        // 1️⃣ Wczytanie komponentów z bazy (tylko te z ID z pliku Excel)
                        var components = new Dictionary<int, (string name, int quantity, int rId, string type)>();
                        using (var cmd = new MySqlCommand("SELECT id, r_id, name, quantity, type FROM Components", connection))
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int rId = reader.GetInt32("r_id");
                                if (!excelDict.ContainsKey(rId)) continue; // pomijamy komponenty spoza Excela

                                int id = reader.GetInt32("id");
                                string name = reader.GetString("name");
                                int quantity = reader.GetInt32("quantity");
                                string type = reader.GetString("type");

                                components[id] = (name, quantity, rId, type);
                            }
                        }

                        // 2️⃣ Rezerwacje bez listy
                        var reservationsWithoutList = new Dictionary<int, int>();
                        using (var cmd = new MySqlCommand(@"
                                                        SELECT components_id, SUM(quantity) AS total
                                                        FROM Reservations
                                                        WHERE list_id IS NULL
                                                        GROUP BY components_id", connection))

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int compId = reader.GetInt32("components_id");
                                if (!components.ContainsKey(compId)) continue; // tylko te z Excela

                                int total = reader.GetInt32("total");
                                reservationsWithoutList[compId] = total;
                            }       
                        }

                        // 3️⃣ Rezerwacje z listą
                        var reservationsWithList = new List<(int componentId, int listStart, int listDone, int resQuantity)>();
                        using (var cmd = new MySqlCommand(@"
                                                        SELECT r.components_id, r.quantity, l.start, l.done_top, l.done_bot, r.side
                                                        FROM Reservations r
                                                        JOIN ListOfReservations l ON r.list_id = l.id
                                                        WHERE   r.list_id IS NOT NULL 
                                                                AND l.status = @status", connection))
                                                                
                        {
                            cmd.Parameters.AddWithValue("@status", ListStatusConstants.ZAREZERWOWANE);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int compId = reader.IsDBNull(reader.GetOrdinal("components_id")) ? 0 : reader.GetInt32("components_id");
                                if (!components.ContainsKey(compId)) continue; // tylko te z Excela

                                int quantity = reader.GetInt32("quantity");
                                int start = reader.GetInt32("start");

                                int doneTop = reader.IsDBNull(reader.GetOrdinal("done_top")) ? 0 : reader.GetInt32("done_top");
                                int doneBot = reader.IsDBNull(reader.GetOrdinal("done_bot")) ? 0 : reader.GetInt32("done_bot");
                                string side = reader.IsDBNull(reader.GetOrdinal("side")) ? "" : reader.GetString("side");

                                int done = side == "TOP" ? doneTop : doneBot;

                                reservationsWithList.Add((compId, start, done, quantity));
                            }
                        }

                        // 3b: Include THT reservations – perform aggregation in SQL
                        var reservationsTHT = new List<(int componentId, int quantity)>();
                        // Build optional component filter to limit results to Excel list
                        string componentFilter = components.Count > 0
                            ? "AND c.id IN (" + string.Join(",", components.Keys) + ")"
                            : string.Empty;
                        using (var cmd1 = new MySqlCommand(@"
                            SELECT c.id AS c_id,
                                   r.quantity AS reserved,
                                   IFNULL(
                                       (SELECT SUM(t.quantity)
                                        FROM TraceTHT t
                                        WHERE t.list_id = l.id
                                          AND t.reel_id LIKE CONCAT(c.r_id, '%'))
                                   ,0) AS used
                            FROM ReservationsTHT r
                            JOIN Components c ON c.id = r.components_id
                            JOIN ListOfReservationsTHT l ON r.list_id = l.id
                            WHERE r.list_id IS NOT NULL
                              AND l.status = @status
                              " + componentFilter, connection))
                        {
                            cmd1.Parameters.AddWithValue("@status", ListStatusConstants.ZAREZERWOWANE);

                            using (var reader = cmd1.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int compId = reader.GetInt32("c_id");
                                    int qty = reader.IsDBNull(reader.GetOrdinal("reserved")) ? 0 : reader.GetInt32("reserved");
                                    int used = reader.IsDBNull(reader.GetOrdinal("used")) ? 0 : reader.GetInt32("used");
                                    reservationsTHT.Add((compId, qty - used));
                                }
                            }
                        }

                        // 4️⃣ Obliczanie braków
                        foreach (var kvp in components)
                        {
                            int id = kvp.Key;
                            string name = kvp.Value.name;
                            int available = kvp.Value.quantity;
                            int rId = kvp.Value.rId;

                            // Odjęcie ilości z rezerwacji bez listy
                            if (reservationsWithoutList.ContainsKey(id))
                                available -= reservationsWithoutList[id];

                            // Odjęcie ilości z rezerwacji z listą
                            foreach (var res in reservationsWithList.Where(r => r.componentId == id))
                            {
                                int elementsPerBox = res.resQuantity / res.listStart;
                                int boxesRemaining = res.listStart - res.listDone;
                                int remaining = elementsPerBox * boxesRemaining;

                                available -= remaining;
                            }

                            // Odjęcie ilości z rezerwacji THT 
                            foreach (var thtRes in reservationsTHT.Where(r => r.componentId == id))
                            {
                                available -= thtRes.quantity;
                            }

                            // Uwzględnienie ilości z Excela
                            int excelQty = excelDict.ContainsKey(rId) ? excelDict[rId] : 0;
                            available -= excelQty;

                                if (available < 0)
                                {
                                    braki.Add((id, name, -available, rId));
                                }
                            }
                        }
                        }
                    
                    return DatabaseResult<List<(int componentId, string componentName, int brakQuantity, int rId)>>.Success(braki);
                }
                catch (Exception ex)
                {                    return DatabaseResult<List<(int componentId, string componentName, int brakQuantity, int rId)>>.Failure($"Error calculating shortages for list: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<bool>> ExportBrakiToExcelAsync(List<(int componentId, string componentName, int brakQuantity, int rId)> braki, string filePath)
        {
            return Task.Run(() =>
            {
                try
                {
                    if (braki == null || braki.Count == 0) 
                        return DatabaseResult<bool>.Success(true);

                    using var connection = CreateConnection();
                    using var wb = new XLWorkbook();
                    var ws = wb.Worksheets.Add("Braki");
                    ws.Cell(1, 1).Value = "ComponentId_DB";
                    ws.Cell(1, 2).Value = "ID";
                    ws.Cell(1, 3).Value = "Nazwa";
                    ws.Cell(1, 4).Value = "Brakująca Ilość";

                    int row = 2;
                    foreach (var b in braki)
                    {
                        ws.Cell(row, 1).Value = b.componentId;
                        ws.Cell(row, 2).Value = b.rId;
                        ws.Cell(row, 3).Value = b.componentName;
                        bool usedAlternative = false;
                        string alternativeId = string.Empty;
                        const string altSql = @"
                            SELECT c.id, c.r_id, c.quantity
                            FROM Substitute a
                            JOIN Components c ON c.id = a.z_id
                            WHERE a.o_id = @originalComponentId;";
                        
                        var altComps = new List<(int id, int rId, int quantity)>();
                        using (var altCmd = new MySqlCommand(altSql, connection))
                        {
                            altCmd.Parameters.AddWithValue("@originalComponentId", b.componentId);
                            using var altReader = altCmd.ExecuteReader();
                            while (altReader.Read())
                            {
                                altComps.Add((
                                    altReader.GetInt32("id"),
                                    altReader.GetInt32("r_id"),
                                    altReader.GetInt32("quantity")
                                ));
                            }
                        }

                        foreach (var alt in altComps)
                        {
                            int altAvailable = alt.quantity;
                            if (altAvailable - b.brakQuantity >= 0)
                            {
                                usedAlternative = true;
                                alternativeId = alt.rId.ToString();
                                break;
                            }
                        }

                        // If an alternative component can cover the shortage, skip this component
                        if (usedAlternative)
                        {
                            continue;
                        }
                        
                        int availableAlt = 0;

                        // If single component CAN'T cover shortage, check combined alternatives
                        for (int i = 0; i < altComps.Count; i++)
                        {
                            var alt = altComps[i];
                            int altAvailable = alt.quantity;
                            availableAlt += altAvailable;
                            alternativeId += alt.rId.ToString();
                        }

                        // If combined alternatives can cover the shortage, skip this component
                        if (b.brakQuantity - availableAlt <= 0)
                        {
                            usedAlternative = true;
                        }

                        if (usedAlternative)
                        {
                            continue;
                        }

                        // Else write shortage to Excel
                        ws.Cell(row, 4).Value = b.brakQuantity - availableAlt;
                        row++;
                    }

                    ws.Range(1, 1, 1, 4).Style.Font.Bold = true;
                    ws.Columns().AdjustToContents();

                    var dir = System.IO.Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) 
                        Directory.CreateDirectory(dir);
                    
                    wb.SaveAs(filePath);                    return DatabaseResult<bool>.Success(true);
                }
                catch (Exception ex)
                {                    return DatabaseResult<bool>.Failure($"Error exporting braki to Excel: {ex.Message}", ex);
                }
            });
        }

        public Task<DatabaseResult<List<(int componentId, string componentName, int Quantity, int rId)>>> GetComponentAmountAsync(List<ExcelRow> listOfComponents)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var connection = CreateConnection();
                    var result = new List<(int componentId, string componentName, int Quantity, int rId)>();

                    var excelDict = listOfComponents.ToDictionary(
                        x =>
                        {
                            int id;
                            if (!int.TryParse(x.Id, out id)) id = 99999;
                            return id;
                        },
                        x =>
                        {
                            int qty;
                            if (!int.TryParse(x.Quantity, out qty)) qty = 0;
                            return qty;
                        }
                    );

                    using (var cmd = new MySqlCommand("SELECT id, r_id, name, quantity FROM Components", connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int rId = reader.GetInt32("r_id");
                            if (!excelDict.ContainsKey(rId)) continue;

                            int id = reader.GetInt32("id");
                            string name = reader.GetString("name");
                            int quantity = reader.GetInt32("quantity");

                            result.Add((id, name, quantity, rId));
                        }
                    }

                    return DatabaseResult<List<(int componentId, string componentName, int Quantity, int rId)>>.Success(result);
                }
                catch (Exception ex)
                {
                    return DatabaseResult<List<(int componentId, string componentName, int Quantity, int rId)>>.Failure($"Error retrieving component amounts: {ex.Message}", ex);
                }
            });
        }
    }
}
