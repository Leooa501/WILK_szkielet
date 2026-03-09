using ClosedXML.Excel;
using ExcelDataReader;
using Newtonsoft.Json.Linq;

namespace WILK.Services
{
    /// <summary>
    /// Service implementation for parsing CSV, Excel, and JSON files
    /// </summary>
    public class FileProcessingService : IFileProcessingService
    {
        public List<string> LoadCSV(string path)
        {
            List<string> result = new List<string>();

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            string[] lines = File.ReadAllLines(path, System.Text.Encoding.GetEncoding("windows-1250"));

            int flag = 0; // Track if we've passed the header row
            foreach (string line in lines)
            {
                int flag2 = 0;
                string[] columns = line.Split(';');
                
                // Skip header row
                if (flag == 0)
                {
                    flag = 1;
                    continue;
                }
                
                // Extract columns: r_id (0), name (1), type (2), quantity (4)
                for (int i = 0; i < 5; i += 1)
                {
                    string value = columns[i].Trim();
                    if (flag2 == 0) // r_id
                    {
                        result.Add(value);
                    }
                    if (flag2 == 1) // name
                    {
                        result.Add(value);
                    }
                    if (flag2 == 2) // type
                    {
                        result.Add(value);
                    }
                    if (flag2 == 4) // quantity (skip column 3)
                    {
                        result.Add(value);
                    }
                    flag2++;
                }
            }

            return result;
        }

        public (string LoadedListName, List<(string Kol1, string Kol2, string Kol3)>) LoadList(string path)
        {
            var loadedListName = string.Empty;
            var loadedListStart = 3;

            var result = new List<(string Kol1, string Kol2, string Kol3)>();

            if (Path.GetExtension(path).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                using var wb = new XLWorkbook(path);
                var ws = wb.Worksheets.First();

                // First cell contains list name
                loadedListName = ws.Cell(1, 1).GetString();

                // Start reading component data from row 3
                var row = loadedListStart;
                while (true)
                {
                    var a = ws.Cell(row, 1).GetString();
                    var b = ws.Cell(row, 2).GetString();
                    var c = ws.Cell(row, 3).GetString();
                    
                    // Stop when all cells are empty
                    if (string.IsNullOrWhiteSpace(a) && string.IsNullOrWhiteSpace(b) && string.IsNullOrWhiteSpace(c))
                        break;

                    result.Add((a.Trim(), b.Trim(), c.Trim()));
                    row++;
                }
            }
            else
            {
                // Handle legacy .xls format
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                using var stream = File.Open(path, FileMode.Open, FileAccess.Read);
                using var reader = ExcelReaderFactory.CreateReader(stream);

                int rowIndex = 0;
                while (reader.Read())
                {
                    // Row 0: List name
                    if (rowIndex == 0)
                    {
                        loadedListName = reader.GetValue(0)?.ToString() ?? string.Empty;
                        rowIndex++;
                        continue;
                    }
                    // Row 1: Header row, skip
                    if (rowIndex == 1)
                    {
                        rowIndex++;
                        continue;
                    }

                    // Data rows: columns offset by 1 in legacy format
                    var a = reader.GetValue(1)?.ToString() ?? string.Empty;
                    var b = reader.GetValue(2)?.ToString() ?? string.Empty;
                    var c = reader.GetValue(3)?.ToString() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(a) && string.IsNullOrWhiteSpace(b) && string.IsNullOrWhiteSpace(c))
                    {
                        rowIndex++;
                        continue;
                    }

                    result.Add((a.Trim(), b.Trim(), c.Trim()));
                    rowIndex++;
                }
                loadedListStart = 2;
            }

            return (loadedListName, result);
        }

        public List<(string id, string altId)> LoadAltsList(string path)
        {
            var rows = new List<(string id, string altId)>();
            
            if (Path.GetExtension(path).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                using var wb = new XLWorkbook(path);
                var ws = wb.Worksheets.First();

                var r = 1;
                while (true)
                {
                    // Column 1: original component ID, Column 4: alternative ID
                    var a = ws.Cell(r, 1).GetString();
                    var b = ws.Cell(r, 4).GetString();
                    if (string.IsNullOrWhiteSpace(a) && string.IsNullOrWhiteSpace(b))
                        break;
                    rows.Add((a.Trim(), b.Trim()));
                    r++;
                }
            }
            else
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                using var stream = File.Open(path, FileMode.Open, FileAccess.Read);
                using var reader = ExcelReaderFactory.CreateReader(stream);

                int rowIndex = 0;
                while (reader.Read())
                {
                    var a = reader.GetValue(0)?.ToString() ?? string.Empty;
                    var b = reader.GetValue(3)?.ToString() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(a) && string.IsNullOrWhiteSpace(b))
                    {
                        rowIndex++;
                        continue;
                    }
                    rows.Add((a.Trim(), b.Trim()));
                    rowIndex++;
                }
            }

            return rows.Select(t => (t.Item1, t.Item2)).ToList();
        }

        public List<string> LoadJson(string path)
        {
            List<string> allIds = new List<string>();

            string jsonContent = File.ReadAllText(path);
            JArray outerArray = JArray.Parse(jsonContent);

            foreach (var inner in outerArray)
            {
                if (inner is JArray innerArray)
                {
                    int flag = 0; // Track if we should skip this container's components
                    string? tekst = null;
                    foreach (var obj in innerArray)
                    {
                        // Extract container _id
                        if (obj is JObject jObj2 && jObj2["Name"]?.ToString() == "_id")
                        {
                            var valueArrayId = jObj2["Value"];
                            tekst = valueArrayId?.ToString();

                            // Skip empty containers
                            if (tekst == "")
                            {
                                flag = 1;
                                continue;
                            }
                            else
                            {
                                // Skip containers with specific ID pattern (position 3 == '5')
                                if (tekst != null && tekst.Length > 3 && tekst[3] == '5')
                                {
                                    flag = 1;
                                    continue;
                                }
                            }
                            if (tekst != null)
                                allIds.Add(tekst);
                        }
                        
                        // Extract component IDs from container
                        if (obj is JObject jObj && jObj["Name"]?.ToString() == "ids")
                        {
                            var valueArray = jObj["Value"] as JArray;
                            
                            // Skip if container was flagged
                            if (flag == 1)
                            {
                                flag = 0;
                                continue;
                            }
                            if (valueArray != null)
                            {
                                foreach (var id in valueArray)
                                {
                                    long number = id.ToObject<long>();
                                    string numStr = number.ToString();

                                    allIds.Add(numStr);
                                }
                            }
                        }
                    }
                }
            }

            return allIds;
        }
    }
}