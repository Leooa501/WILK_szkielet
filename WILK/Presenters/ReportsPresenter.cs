using System.Data;
using ClosedXML.Excel;
using WILK.Models;
using WILK.Services;
using WILK.Views;

namespace WILK.Presenters
{
    public interface IReportView
    {
        event EventHandler<EventArgs>? ExcelListImported;
        event EventHandler<EventArgs>? CheckBrakiRequested;
        event EventHandler<ExportBrakiEventArgs>? ExportBrakiRequested;
        event EventHandler<DailyUsageEventArgs>? DailyButtonClicked;
        event EventHandler<EventArgs>? ReportsTabSelected;

        void ShowError(string title, string message);
        void ShowInfo(string title, string message);
        void SetImportedFileName(string fileName);
        void SetLastReportDate(DateTime? date);
    }
    public class ReportsPresenter : BaseTabPresenter
    {
        private readonly IReportView _view;
        private readonly IFileProcessingService _fileProcessingService;
        private List<(string Kol1, string Kol2, string Kol3)> _excelData;
        private string _excelName;

        public ReportsPresenter(IReportView view, IEnterpriseDatabase enterpriseDatabase, IFileProcessingService fileProcessingService)
            : base(enterpriseDatabase)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _fileProcessingService = fileProcessingService ?? throw new ArgumentNullException(nameof(fileProcessingService));

            _excelData = new List<(string Kol1, string Kol2, string Kol3)>();
            _excelName = string.Empty;

            _excelData = new List<(string Kol1, string Kol2, string Kol3)>();
            _excelName = string.Empty;

            SubscribeToViewEvents();
        }

        private void SubscribeToViewEvents()
        {
            _view.ExcelListImported += OnExcelListImported;
            _view.CheckBrakiRequested += OnCheckBrakiRequested;
            _view.ExportBrakiRequested += OnExportBrakiRequested;
            _view.DailyButtonClicked += OnDailyButtonClicked;
            _view.ReportsTabSelected += (s, e) => OnTabActivated();
        }

        public override void Initialize()
        {
            LoadLastReportDate();
        }

        public override void OnTabActivated()
        {
            LoadLastReportDate();
        }

        private async void LoadLastReportDate()
        {
            try
            {
                var result = await _enterpriseDatabase.GetLastReportDateAsync();
                if (result != null && result.IsSuccess)
                {
                    _view.SetLastReportDate(result.Data);
                }
                else
                {
                    _view.SetLastReportDate(null);
                }
            }
            catch
            {
                _view.SetLastReportDate(null);
            }
        }

        private void OnExcelListImported(object? sender, EventArgs e)
        {
            if( e.GetType () == typeof(ExcelListEventArgs) )
            {
                try
                {
                    var excelListEventArgs = (ExcelListEventArgs)e;
                    _excelName = Path.GetFileName(excelListEventArgs.FileName);
                    (_excelName, _excelData) = _fileProcessingService.LoadList(excelListEventArgs.FileName);
                    
                    _view.SetImportedFileName(_excelName);
                    _view.ShowInfo("Sukces", $"Zaimportowano {_excelData.Count} elementów z pliku {_excelName}");
                }
                catch (Exception ex)
                {
                    _view.ShowError("Błąd importu", $"Nie udało się zaimportować pliku: {ex.Message}");
                }
            }
            else if (e.GetType() == typeof(MultipleFilesEventArgs))
            {
                var multipleFilesEventArgs = (MultipleFilesEventArgs)e;
                _excelData.Clear();
                var accumulated = new List<(string Kol1, string Kol2, string Kol3)>();
                foreach (var file in multipleFilesEventArgs.Files)
                {
                    try
                    {
                        var (name, data) = _fileProcessingService.LoadList(file.FileName);
                        accumulated.AddRange(data);
                    }
                    catch (Exception ex)
                    {
                        _view.ShowError("Błąd importu", $"Nie udało się zaimportować pliku {file.FileName}: {ex.Message}");
                    }
                }
                _excelData = accumulated
                    .GroupBy(x => (x.Kol1, x.Kol2))
                    .Select(g =>
                    {
                        var first = g.First();
                        int total = 0;
                        foreach (var item in g)
                        {
                            if (int.TryParse(item.Kol3, out var v))
                                total += v;
                        }
                        return (first.Kol1, first.Kol2, total.ToString());
                    })
                    .ToList();
                _excelName = "Wiele plików";
                _view.SetImportedFileName(_excelName);
                _view.ShowInfo("Sukces", $"Zaimportowano {_excelData.Count} elementów z wielu plików");
            }else 
            {
                _view.ShowError("Błąd importu", "Nieznany typ zdarzenia importu");
            }
        }

        private void OnCheckBrakiRequested(object? sender, EventArgs e)
        {
            try
            {
                if (_excelData == null || !_excelData.Any())
                {
                    _view.ShowError("Błąd", "Najpierw zaimportuj listę wybraniową");
                    return;
                }

                var components = _excelData;
                var idsToUpdate = components.Select(c => c.Kol2).ToArray();
                _enterpriseDatabase.UpdateComponentsAsync(idsToUpdate).Wait();
                
                var braki = _enterpriseDatabase.GetBrakiForListAsync(
                    components.ConvertAll(r => new ExcelRow 
                    { 
                        Name = r.Kol1, 
                        Id = r.Kol2, 
                        Quantity = r.Kol3 
                    })
                ).Result.Data;

                using var sfd = new SaveFileDialog();
                sfd.Filter = "Excel files|*.xlsx";
                sfd.FileName = _excelName != string.Empty ? $"Braki_{_excelName}" : "Braki.xlsx";
                sfd.Title = "Zapisz braki do pliku Excel";
                if (sfd.ShowDialog() != DialogResult.OK)
                    return;

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Braki");
                    worksheet.Cell(1, 1).Value = "Nazwa";
                    worksheet.Cell(1, 2).Value = "ID";
                    worksheet.Cell(1, 3).Value = "Ilość";
                    worksheet.Cell(1, 4).Value = "Typ";
                    worksheet.Cell(1, 5).Value = "Status";
                    worksheet.Cell(1, 6).Value = "Zamiennik";
                    worksheet.Cell(1, 7).Value = "Brak globalny";
                    int row = 2;
                    foreach (var comp in components)
                    {
                        // Parsowanie rId i ilości z Excela
                        if (!int.TryParse(comp.Kol2, out int rId))
                            continue;
                        int qtyExcel = int.TryParse(comp.Kol3, out int qtyParsed) ? qtyParsed : 0;

                        // Pobranie id komponentu na podstawie r_id
                        var componentIdResult = _enterpriseDatabase.GetComponentIdByRIdAsync(rId).Result;
                        int componentId = componentIdResult.IsSuccess ? componentIdResult.Data : 99999;

                        // Pobranie typu komponentu
                        var typeResult = _enterpriseDatabase.GetComponentTypeAsync(componentId).Result;
                        string type = (typeResult.IsSuccess && typeResult.Data != null) ? typeResult.Data.Trim().ToUpper() : "UNKNOWN";

                        // Wpisanie do Excela
                        worksheet.Cell(row, 1).Value = comp.Kol1?.Trim() ?? "";
                        worksheet.Cell(row, 2).Value = rId;
                        worksheet.Cell(row, 3).Value = qtyExcel;
                        worksheet.Cell(row, 4).Value = type;
                        // Status: Brak / OKa
                        var brak = braki.FirstOrDefault(b => b.rId == rId);
                        if (brak == default)
                        {
                            if(_enterpriseDatabase.GetComponentIdByRIdAsync(rId).Result.IsSuccess && _enterpriseDatabase.GetComponentIdByRIdAsync(rId).Result.Data != -1)
                            {
                                worksheet.Cell(row, 5).Value = "OK";
                            }
                            else
                            {
                                worksheet.Cell(row, 5).Value = "Brak w bazie";
                            }
                            row++;
                            continue;
                        }

                        bool usedAlternative = false;
                        string alternativeId = "";
                        var altCompsResult = _enterpriseDatabase.GetAlternativeComponentsAsync(componentId).Result;
                        if (altCompsResult.IsSuccess && altCompsResult.Data != null)
                        {
                            foreach (var alt in altCompsResult.Data)
                            {
                                int altAvailable = alt.Quantity;
                                if (altAvailable - brak.brakQuantity >= 0)
                                {
                                    usedAlternative = true;
                                    alternativeId = alt.RId.ToString();
                                    break;
                                }
                            }
                            if (!usedAlternative)
                            {
                                int availableAlt = 0;
                                for (int i = 0; i < altCompsResult.Data.Count; i++)
                                {
                                    var alt = altCompsResult.Data[i];
                                    if(alt.Quantity <= 0)
                                        continue;
                                    
                                    int altAvailable = alt.Quantity;
                                    availableAlt += altAvailable;
                                    alternativeId += alt.RId.ToString() + " ";

                                    if(availableAlt >= brak.brakQuantity)
                                        break;
                                }
                                if (brak.brakQuantity - availableAlt <= 0)
                                {
                                    usedAlternative = true;
                                }
                            }
                        }
                        worksheet.Cell(row, 7).Value = brak.brakQuantity;
                        if ( brak.brakQuantity >= qtyExcel)
                        {
                            brak.brakQuantity = qtyExcel;
                        }
                        worksheet.Cell(row, 5).Value = usedAlternative ? "OK" : $"Brakuje {brak.brakQuantity}";

                        if(usedAlternative)
                        {
                            worksheet.Cell(row, 6).Value = alternativeId;
                        }

                        row++;
                    }

                    workbook.SaveAs(sfd.FileName);
                }

                _view.ShowInfo("Sukces", "Braki wyeksportowane do Excela.");
            }
            catch (Exception ex)
            {
                _view.ShowError("Błąd sprawdzania braków", ex.Message);
            }
        }

        private void OnExportBrakiRequested(object? sender, ExportBrakiEventArgs e)
        {
            try
            {
                _enterpriseDatabase.UpdateComponentsAsync().Wait();
                var braki = _enterpriseDatabase.GetBrakiAsync(e.onlySMD).Result.Data;

                if (braki == null || braki.Count == 0)
                {
                    _view.ShowError("Błąd eksportu braków", "Nie ma braków do eksportu.");
                    return;
                }

                _enterpriseDatabase.ExportBrakiToExcelAsync(braki, e.FilePath).Wait();
                _view.ShowInfo("Sukces", "Braki wyeksportowane do Excela.");
            }
            catch (Exception ex)
            {
                _view.ShowError("Błąd eksportu braków", ex.Message);
            }
        }

        private void OnDailyButtonClicked(object? sender, DailyUsageEventArgs e)
        {
            try
            {
                var ofd = new FolderBrowserDialog
                {
                    Description = "Wybierz folder"
                };   
                if (ofd.ShowDialog() != DialogResult.OK)
                    return;
                
                var dbResult = _enterpriseDatabase.GetDailyUsage(e.SelectedDate).Result;
                if (dbResult == null || !dbResult.IsSuccess || dbResult.Data == null)
                {
                    _view.ShowError("Błąd aktualizacji dziennego zużycia", dbResult?.ErrorMessage ?? "Nieznany błąd");
                    return;
                }

                var dt = dbResult.Data;
                var groups = dt.AsEnumerable().GroupBy(r => new 
                { 
                    ListId = r.Field<int>("list_id"),
                    ReportDate = r.Field<DateTime>("report_date")
                });
                int filesCreated = 0;
                foreach (var grp in groups)
                {
                    int listId = grp.Key.ListId;
                    DateTime reportDate = grp.Key.ReportDate;
                    // Użycie nazwy listy do stworzenia pliku
                    var rawListName = grp.First().Field<string>("list_name") ?? $"list_{listId}";
                    var invalid = Path.GetInvalidFileNameChars();
                    var sanitized = string.Concat(rawListName.Select(c => invalid.Contains(c) ? '_' : c)).Trim();
                    if (string.IsNullOrWhiteSpace(sanitized)) sanitized = $"list_{listId}";
                    // Dodanie daty do nazwy pliku
                    string datePart = "";
                    datePart = $"_{reportDate:yyyy-MM-dd}";
                    var filePath = Path.Combine(ofd.SelectedPath, $"{sanitized}_{listId}{datePart}.txt");

                    using (var sw = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
                    {
                        // grupowanie po identyfikatorze komponentu i sumowanie ilości
                        var aggregated = grp
                            .GroupBy(r => r.Field<int>("r_id"))
                            .Select(g => new
                            {
                                Id = g.Key,
                                Quantity = g.Sum(r => r.Field<int>("daily_usage"))
                            })
                            .OrderBy(a => a.Id);

                        foreach (var item in aggregated)
                        {
                            sw.WriteLine($"{item.Id}\t{item.Quantity}");
                        }
                    }
                    filesCreated++;
                }

                _view.ShowInfo("Sukces", filesCreated > 0 ? $"Utworzono {filesCreated} plików w {ofd.SelectedPath}." : "Brak danych do zapisania.");
                _view.SetLastReportDate(_enterpriseDatabase.GetLastReportDateAsync().Result.Data);
            }
            catch (Exception ex)
            {
                _view.ShowError("Błąd aktualizacji dziennego zużycia", ex.Message);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _view.ExcelListImported -= OnExcelListImported;
                _view.CheckBrakiRequested -= OnCheckBrakiRequested;
                _view.ExportBrakiRequested -= OnExportBrakiRequested;
                _view.DailyButtonClicked -= OnDailyButtonClicked;
            }
            
            base.Dispose(disposing);
        }
    }
}
