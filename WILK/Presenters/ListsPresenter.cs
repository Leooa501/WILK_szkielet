using System.Data;
using WILK.Models;
using WILK.Services;
using WILK.Views;
using ClosedXML.Excel;
using WILK.Services.Repositories;
using Mysqlx.Crud;
using System.Text.RegularExpressions;
using ZstdSharp.Unsafe;

namespace WILK.Presenters
{
    public interface IListsView
    {
        event EventHandler<DeleteReservationEventArgs>? DeleteReservationRequested;
        event EventHandler<UpdateListDoneEventArgs>? UpdateListDoneRequested;
        event EventHandler<ReverseLastUpdateEventArgs>? ReverseLastUpdateRequested;
        event EventHandler<GenerateListEventArgs>? GenerateListRequested;
        event EventHandler<GenerateListTHTEventArgs>? GenerateListTHTRequested;
        event EventHandler<EventArgs>? TabListsSelected;
        event EventHandler<EventArgs>? TabListsTHTSelected;
        event EventHandler<EventArgs>? TabItemsSelected;
        event EventHandler<EventArgs>? TabAdditionalMaterialsSelected;
        event EventHandler<ListsSelectedEventArgs>? UpdateAdditionalMaterialsRequested;
        event EventHandler<ListsSelectedEventArgs>? DeleteAdditionalMaterialRequested;
        event EventHandler<ReverseLastUpdateTHTEventArgs>? ReverseLastUpdateTHTRequested;


        void BindReservationItems(DataTable items);
        void BindReservationsGrid(DataTable dt);
        void BindReservationsTHTGrid(DataTable dt);
        void BindAdditionalMaterialsGrid(DataTable dt);
        void ShowInfo(string title, string message);
        void ShowError(string title, string message);
    }

    public class ListsPresenter : BaseTabPresenter
    {
        private readonly IListsView _view;

        public ListsPresenter(IListsView view, IEnterpriseDatabase enterpriseDatabase) : base(enterpriseDatabase)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            
            SubscribeToViewEvents();
        }

        private void SubscribeToViewEvents()
        {
            _view.DeleteReservationRequested += OnDeleteReservationRequested;
            _view.UpdateListDoneRequested += OnUpdateListDoneRequested;
            _view.ReverseLastUpdateRequested += OnReverseLastUpdateRequested;
            _view.GenerateListRequested += OnGenerateListRequested;
            _view.GenerateListTHTRequested += OnGenerateListTHTRequested;
            _view.TabListsSelected += (s, e) => OnTabActivated();
            _view.TabListsTHTSelected += (s, e) => RefreshReservationsTHT();
            _view.TabItemsSelected += (s, e) => RefreshReservations();
            _view.TabAdditionalMaterialsSelected += RefreshAdditionalMaterials;
            _view.UpdateAdditionalMaterialsRequested += UpdateAdditionalMaterials;
            _view.DeleteAdditionalMaterialRequested += DeleteAdditionalMaterial;
            _view.ReverseLastUpdateTHTRequested += OnReverseLastUpdateTHTRequested;

        }

        public override void Initialize()
        {
            
        }

        public override void OnTabActivated()
        {
            RefreshReservations();
        }

        private void OnDeleteReservationRequested(object? sender, DeleteReservationEventArgs e)
        {
            try
            {
                var MessageBoxResult = MessageBox.Show("Czy na pewno chcesz usunąć tę rezerwację?", "Potwierdzenie usunięcia", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (MessageBoxResult != DialogResult.Yes)
                {
                    return;
                }
                var item = new ReservationItemDto 
                { 
                    Id = e.Id, 
                    IsList = e.IsList 
                };
                
                _enterpriseDatabase.DeleteReservationAsync(item).Wait();
                RefreshReservations();
                _view.ShowInfo("Sukces", "Pozycja została usunięta.");
            }
            catch (Exception ex)
            {
                _view.ShowError("Błąd", $"Błąd podczas usuwania: {ex.Message}");
            }
        }

        private void OnUpdateListDoneRequested(object? sender, UpdateListDoneEventArgs e)
        {
            try
            {
                var reservation = _enterpriseDatabase.GetReservationProgressAsync((int)e.ReservationId).Result.Data;
                if (reservation.HasValue) // Sprawdź, czy rezerwacja istnieje
                {
                    int newDone;
                    if (e.Side == IReservationRepository.Side.TOP) // Aktualizacja dla strony TOP
                    {
                        newDone = reservation.Value.done_top + e.AddValue;
                        if (newDone > reservation.Value.start) // Sprawdzanie przekroczenia wartości startowej
                        {
                            MessageBox.Show("Ilość nie może przekroczyć wartość startową.", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                    else if(e.Side == IReservationRepository.Side.BOT) // Aktualizacja dla strony BOT
                    {
                        newDone = reservation.Value.done_bot + e.AddValue;
                        if (newDone > reservation.Value.start) // Sprawdzanie przekroczenia wartości startowej
                        {
                            MessageBox.Show("Ilość nie może przekroczyć wartość startową.", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }else // Aktualizacja dla strony SINGLE
                    {
                        newDone = reservation.Value.done_top + e.AddValue;
                        var newDoneTop = newDone;
                        var newDoneBot = reservation.Value.done_bot + e.AddValue;
                        if (newDoneTop > reservation.Value.start || newDoneBot > reservation.Value.start) // Sprawdzanie przekroczenia wartości startowej
                        {
                            MessageBox.Show("Ilość nie może przekroczyć wartość startową.", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        _enterpriseDatabase.UpdateLogsAsync((int)e.ReservationId, newDoneBot, IReservationRepository.Side.SINGLE).Wait();
                        _enterpriseDatabase.UpdateListReservationAsync((int)e.ReservationId, newDoneTop, IReservationRepository.Side.TOP).Wait();
                        _enterpriseDatabase.UpdateListReservationAsync((int)e.ReservationId, newDoneBot, IReservationRepository.Side.BOT).Wait();
                        RefreshReservations();
                        _view.ShowInfo("Sukces", "Lista została zaktualizowana.");

                        return;
                    }
                    _enterpriseDatabase.UpdateListReservationAsync((int)e.ReservationId, newDone, e.Side).Wait();
                    _enterpriseDatabase.UpdateLogsAsync((int)e.ReservationId, newDone, e.Side).Wait();
                    RefreshReservations();
                    _view.ShowInfo("Sukces", "Lista została zaktualizowana.");
                }
                else // Rezerwacja nie znaleziona
                {
                    _view.ShowError("Błąd", "Nie znaleziono rezerwacji.");
                }
            }
            catch (Exception ex)
            {
                _view.ShowError("Błąd", $"Błąd podczas aktualizacji: {ex.Message}");
            }
        }

        private void OnReverseLastUpdateRequested(object? sender, ReverseLastUpdateEventArgs e)
        {
            try
            {
                _enterpriseDatabase.ReverseLastUpdateAsync((int)e.ReservationId, e.Side).Wait();
                RefreshReservations();
                _view.ShowInfo("Sukces", "Ostatnia aktualizacja została cofnięta.");
            }
            catch (Exception ex)
            {
                _view.ShowError("Błąd", $"Błąd podczas cofania aktualizacji: {ex.Message}");
            }
        }

        private void OnGenerateListRequested(object? sender, GenerateListEventArgs e)
        {
            try
            {
                var isOneSided = e.oneSided;

                GenerateExcelList(e.filePath, e.id, isOneSided, e.quantity);
                _view.ShowInfo("Sukces", "Lista została wygenerowana.");
            }
            catch (Exception ex)
            {
                _view.ShowError("Błąd", $"Błąd podczas generowania listy: {ex.Message}");
            }
        }

        private void OnGenerateListTHTRequested(object? sender, GenerateListTHTEventArgs e)
        {
            try
            {
        
                var wb = new XLWorkbook();
                if (wb.Worksheets.Contains("Raport"))
                {
                    wb.Worksheet("Raport").Delete();
                }
                var ws = wb.Worksheets.Add("Raport");

                // Nagłówki
                ws.Cell(2, 1).Value = "Nazwa";
                ws.Cell(2, 2).Value = "ID elementu";
                ws.Cell(2, 3).Value = "Ilość";
                ws.Cell(2, 4).Value = "Pojemnik";
                ws.Cell(2, 5).Value = "Zamiennik";
                ws.Cell(2, 6).Value = "Zgodność";

                var data = _enterpriseDatabase.GetListDataTHTAsync((int)e.id).Result.Data;
                if (data?.Rows == null) return;
                
                var dane = new List<(string name, int elementId, int quantity, List<string> locations)>();
                var start = _enterpriseDatabase.GetReservationTHTProgressAsync((int)e.id).Result.Data?.start ?? 0;


                string [] ids = new string[data.Rows.Count];

                // Pobierz lokalizacje dla wszystkich elementów
                foreach (DataRow r in data.Rows)
                {
                    int elementId = int.Parse(r["elementId"].ToString() ?? "0");
                    ids[data.Rows.IndexOf(r)] = elementId.ToString();
                    dane.Add((
                        name: r["name"].ToString() ?? "",
                        elementId: elementId,
                        quantity: (int)(int.Parse(r["quantity"].ToString() ?? "0") * (double)(e.quantity != null ? int.Parse(e.quantity) : 1) / (start != 0 ? start : (e.quantity != null ? int.Parse(e.quantity) : 1))),
                        locations: new List<string>()
                    ));
                }
                _enterpriseDatabase.UpdateComponentsAsync(ids).Wait();
                var locationsResult = _enterpriseDatabase.GetLocationsAsync(ids).Result;

                dane = dane.Select(d =>
                {
                    var locs = locationsResult.IsSuccess
                        ? locationsResult.Data.Where(l => l.Type == d.elementId.ToString()).Select(l => l.ContainerID).Where(loc => !string.IsNullOrEmpty(loc) && char.IsDigit(loc[0])).ToList()
                        : new List<string>();
                    return (d.name, d.elementId, d.quantity, locs);
                }).ToList();

                // Grupowanie komponentów według pojemnika
                // Elementy pogrupowane według najczęściej występującego pojemnika
                var sortedData = new List<(string name, int elementId, int quantity, string container)>();

                while (dane.Count > 0)
                {
                    // Zlicz wystąpienia pojemników
                    var licznik = new Dictionary<string, int>();
                    foreach (var rekord in dane)
                    {
                        foreach (var container in rekord.locations)
                        {
                            if (!licznik.ContainsKey(container))
                                licznik[container] = 0;
                            licznik[container]++;
                        }
                    }

                    // Jeżeli brak pojemników, zakończ grupowanie
                    if (licznik.Count == 0)
                    {
                        foreach (var rekord in dane)
                        {
                            sortedData.Add((rekord.name, rekord.elementId, rekord.quantity, string.Empty));
                        }
                        break;
                    }
                    var maxPara = licznik.OrderByDescending(x => x.Value).First();

                    // Jeżeli pojemnik używany tylko raz, nie grupuj dalej
                    if (maxPara.Value <= 1)
                    {
                        foreach (var rekord in dane)
                        {
                            sortedData.Add((rekord.name, rekord.elementId, rekord.quantity, string.Empty));
                        }
                        break;
                    }

                    // Pogrupuj komponenty według najczęściej używanego pojemnika
                    string najczestszaLiczba = maxPara.Key;
                    var pasujaceRekordy = dane
                        .Where(x => x.locations.Contains(najczestszaLiczba))
                        .ToList();
                    foreach (var rekord in pasujaceRekordy)
                    {
                        sortedData.Add((rekord.name, rekord.elementId, rekord.quantity, najczestszaLiczba));
                    }
                    // Usuń pogrupowane komponenty z listy do przetworzenia
                    dane = dane.Except(pasujaceRekordy).ToList();
                }

                // Zlicz użycie pojemników; jeśli pojemnik występuje tylko raz, wyczyść go z pozycji
                var initialContainerCounts = sortedData
                    .Where(d => !string.IsNullOrEmpty(d.container))
                    .GroupBy(d => d.container)
                    .ToDictionary(g => g.Key, g => g.Count());

                // usuń (ustaw na pusty) kontenery, które występują tylko raz
                sortedData = sortedData
                    .Select(d => initialContainerCounts.TryGetValue(d.container, out var __cnt) && __cnt > 1
                                ? d
                                : (d.name, d.elementId, d.quantity, string.Empty))
                    .ToList();

                // przelicz zliczenia po wyczyszczeniu pojedynczych wystąpień
                var containerUsageCount = sortedData
                    .Where(d => !string.IsNullOrEmpty(d.container))
                    .GroupBy(d => d.container)
                    .ToDictionary(g => g.Key, g => g.Count());
                
                sortedData = sortedData
                        .OrderBy(d => string.IsNullOrEmpty(d.container) ? 1 : 0)
                        .OrderByDescending(d => initialContainerCounts.TryGetValue(d.container, out var __occ) ? __occ : 0)
                        .ThenBy(d => int.TryParse(d.container, out var __n) ? __n : int.MaxValue)
                        .ThenByDescending(d => d.quantity)
                        .ToList();
                
                // Pobierz dane o brakach dla tej listy
                var componentAmountResult = _enterpriseDatabase.GetComponentAmountAsync(sortedData.AsEnumerable().Select(r => new Models.ExcelRow
                {
                    Name = r.name.ToString() ?? "",
                    Id = r.elementId.ToString() ?? "",
                    Quantity = r.quantity.ToString() ?? ""
                }).ToList()).Result;
                var componentAmounts = componentAmountResult.IsSuccess && componentAmountResult.Data != null ? componentAmountResult.Data : new List<(int componentId, string componentName, int Quantity, int rId)>();

                var braki = new List<(int componentId, string componentName, int brakQuantity, int rId)>();
                braki = componentAmounts
                    .Select(ca =>
                    {
                        var requiredQty = sortedData.FirstOrDefault(d => d.elementId == ca.rId).quantity;
                        var brakQty = ca.Quantity < requiredQty ? requiredQty - ca.Quantity : 0;
                        return (ca.componentId, ca.componentName, brakQty, ca.rId);
                    })
                    .Where(b => b.brakQty > 0)
                    .ToList();

                // Zapisz wiersze komponentów z analizą braków i alternatyw
                int row = 3;
                foreach (var dr in sortedData)
                {
                    ws.Cell(row, 1).Value = dr.name.ToString();
                    ws.Cell(row, 2).Value = dr.elementId.ToString();
                    ws.Cell(row, 3).Value = dr.quantity.ToString();

                    int brakQty = 0;
                    string altId = "";
                    
                    if (braki.Any(b => b.rId.ToString() == dr.elementId.ToString())) // Sprawdź, czy są braki dla tego komponentu
                    {
                        brakQty = braki.First(b => b.rId.ToString() == dr.elementId.ToString()).brakQuantity;
                        if(brakQty < 0) // Nigdy nie powinno się zdarzyć xd
                            continue;
                        
                        if (brakQty >= int.Parse(dr.quantity.ToString() ?? "0")) // Jeśli brak wiekszy niz potrzebna ilosc cappuj
                        {
                            brakQty = int.Parse(dr.quantity.ToString() ?? "0");
                        }
                        var componentIdResult = _enterpriseDatabase.GetComponentIdByRIdAsync(int.Parse(dr.elementId.ToString() ?? "0")).Result;
                        var alts = componentIdResult.IsSuccess ? _enterpriseDatabase.GetAlternativeComponentsAsync(componentIdResult.Data).Result.Data : null;
                        
                        // Poszukiwanie zamienników
                        if (alts != null)
                        {
                            // 1. Pojedynczy zamiennik pokrywający cały brak
                            foreach (var a in alts)
                            {
                                if (a.Quantity >= brakQty)
                                {
                                    brakQty = 0;
                                    altId += a.RId.ToString() + " ";
                                    break;
                                }
                            }
                            
                            // 2. Kilka zamienników pokrywających cały brak
                            if (brakQty > 0)
                            {
                                foreach (var a in alts)
                                {
                                    brakQty -= a.Quantity;
                                    altId += a.RId.ToString() + " ";
                                    if (brakQty <= 0)
                                        break;
                                }
                            }
                        }
                        
                    }
                    
                    // Zapisz informacje o brakach i zamiennikach
                    ws.Cell(row, 6).Value = brakQty == 0 ? "" : $"Brakuje {brakQty}";
                    ws.Cell(row, 5).Value = altId.Trim();
                    
                    // Zapisz pojemnik tylko jeśli jest używany przez więcej niż jeden komponent
                    string containerValue = "";
                    if (!string.IsNullOrEmpty(dr.container) && containerUsageCount.TryGetValue(dr.container, out int count) && count > 1)
                    {
                        containerValue = dr.container;
                    }
                    ws.Cell(row, 4).Value = containerValue;
                    
                    // Oznaczenie elementów do wygrzania
                    if (_enterpriseDatabase.IsInWarmUpAsync(int.Parse(dr.elementId.ToString() ?? "0")).Result.Data)
                    {
                        ws.Cell(row, 6).Value = ws.Cell(row, 6).Value.ToString() + " (Do wygrzania)";
                    }
                    row++;
                }
                _view.ShowInfo("Sukces", "Lista THT została wygenerowana.");
                wb.SaveAs(e.filePath);
            }
            catch (Exception ex)
            {
                _view.ShowError("Błąd", $"Błąd podczas generowania listy THT: {ex.Message}");
            }
        }

        private void RefreshReservations()
        {
            try
            {
                var reservations = _enterpriseDatabase.GetReservationsTableAsync().Result.Data;
                if (reservations != null)
                {
                    _view.BindReservationsGrid(reservations);
                }
                
                var items = _enterpriseDatabase.LoadReservationsCompsAsync().Result.Data;
                if (items != null) 
                {
                    _view.BindReservationItems(items);
                }
            }
            catch (Exception ex)
            {
                _view.ShowError("Błąd", $"Błąd podczas odświeżania danych: {ex.Message}");
            }
        }

        private void GenerateExcelList(string filePath, int listId, bool isOneSided, int? multiplier) // Duplikat tylko dla SMD
        {
            var wb = new XLWorkbook();
            if (wb.Worksheets.Contains("Raport"))
            {
                wb.Worksheet("Raport").Delete();
            }
            var ws = wb.Worksheets.Add("Raport");

            // Nagłówki
            ws.Cell(2, 1).Value = "Nazwa";
            ws.Cell(2, 2).Value = "ID elementu";
            ws.Cell(2, 3).Value = "Ilość";
            ws.Cell(2, 4).Value = "Pojemnik";
            ws.Cell(2, 5).Value = "Zamiennik";
            ws.Cell(2, 6).Value = "Zgodność";

            var data = _enterpriseDatabase.GetListDataAsync((int)listId).Result.Data;
            if (data?.Rows == null) return;
            
            // Pobierz lokalizacje dla wszystkich elementów
            var dane = new List<(string name, int elementId, int quantity, List<string> locations)>();
            var start = _enterpriseDatabase.GetReservationProgressAsync((int)listId).Result.Data?.start ?? 0;

            string [] ids = new string[data.Rows.Count];
            foreach (DataRow r in data.Rows)
            {
                int elementId = int.Parse(r["elementId"].ToString() ?? "0");
                ids[data.Rows.IndexOf(r)] = elementId.ToString();
                dane.Add((
                    name: r["name"].ToString() ?? "",
                    elementId: elementId,
                    quantity: (int)(int.Parse(r["quantity"].ToString() ?? "0") * (double)(multiplier != null ? multiplier.Value : 1) / (start != 0 ? start : (multiplier != null ? multiplier.Value : 1))),
                    locations: new List<string>()
                ));
            }

            _enterpriseDatabase.UpdateComponentsAsync(ids).Wait();
            var locationsResult = _enterpriseDatabase.GetLocationsAsync(ids).Result;

            dane = dane.Select(d =>
            {
                var locs = locationsResult.IsSuccess
                    ? locationsResult.Data.Where(l => l.Type == d.elementId.ToString()).Select(l => l.ContainerID).Where(loc => !string.IsNullOrEmpty(loc) && char.IsDigit(loc[0])).ToList()
                    : new List<string>();
                return (d.name, d.elementId, d.quantity, locs);
            }).ToList();

            // Grupuj komponenty według pojemnika
            // Elementy pogrupowane według najczęściej występującego pojemnika
            var sortedData = new List<(string name, int elementId, int quantity, string container)>();

            while (dane.Count > 0)
            {
                // Zlicz wystąpienia pojemników
                var licznik = new Dictionary<string, int>();
                foreach (var rekord in dane)
                {
                    foreach (var container in rekord.locations)
                    {
                        if (!licznik.ContainsKey(container))
                            licznik[container] = 0;
                        licznik[container]++;
                    }
                }

                // Jeżeli brak pojemników, zakończ grupowanie
                if (licznik.Count == 0)
                {
                    foreach (var rekord in dane)
                    {
                        sortedData.Add((rekord.name, rekord.elementId, rekord.quantity, string.Empty));
                    }
                    break;
                }
                var maxPara = licznik.OrderByDescending(x => x.Value).First();

                // Jeżeli pojemnik używany tylko raz, nie grupuj dalej
                if (maxPara.Value <= 1)
                {
                    foreach (var rekord in dane)
                    {
                        sortedData.Add((rekord.name, rekord.elementId, rekord.quantity, string.Empty));
                    }
                    break;
                }

                // Grupuj komponenty według najczęściej używanego pojemnika
                string najczestszaLiczba = maxPara.Key;
                var pasujaceRekordy = dane
                    .Where(x => x.locations.Contains(najczestszaLiczba))
                    .ToList();
                foreach (var rekord in pasujaceRekordy)
                {
                    sortedData.Add((rekord.name, rekord.elementId, rekord.quantity, najczestszaLiczba));
                }
                // Usuń pogrupowane komponenty z listy do przetworzenia
                dane = dane.Except(pasujaceRekordy).ToList();
            }
            
            // Zlicz użycie pojemników; jeśli pojemnik występuje tylko raz, wyczyść go z pozycji
            var initialContainerCounts = sortedData
                .Where(d => !string.IsNullOrEmpty(d.container))
                .GroupBy(d => d.container)
                .ToDictionary(g => g.Key, g => g.Count());

            // usuń (ustaw na pusty) kontenery, które występują tylko raz
            sortedData = sortedData
                .Select(d => initialContainerCounts.TryGetValue(d.container, out var __cnt) && __cnt > 1
                             ? d
                             : (d.name, d.elementId, d.quantity, string.Empty))
                .ToList();

            // przelicz zliczenia po wyczyszczeniu pojedynczych wystąpień
            var containerUsageCount = sortedData
                .Where(d => !string.IsNullOrEmpty(d.container))
                .GroupBy(d => d.container)
                .ToDictionary(g => g.Key, g => g.Count());
            
            sortedData = sortedData
                    .OrderBy(d => string.IsNullOrEmpty(d.container) ? 1 : 0)
                    .OrderByDescending(d => initialContainerCounts.TryGetValue(d.container, out var __occ) ? __occ : 0)
                    .ThenBy(d => int.TryParse(d.container, out var __n) ? __n : int.MaxValue)
                    .ThenByDescending(d => d.quantity)
                    .ToList();
            
            // Pobierz dane o brakach dla tej listy
            var groupedData = sortedData
                .GroupBy(d => d.elementId)
                .Select(g => (
                    name: g.First().name,
                    elementId: g.Key,
                    quantity: g.Sum(x => x.quantity),
                    container: g.First().container
                ))
                .ToList();
            
            var componentAmountResult = _enterpriseDatabase.GetComponentAmountAsync(groupedData.Select(r => new Models.ExcelRow
            {
                Name = r.name.ToString() ?? "",
                Id = r.elementId.ToString() ?? "",
                Quantity = r.quantity.ToString() ?? ""
            }).ToList()).Result;
            var componentAmounts = componentAmountResult.IsSuccess && componentAmountResult.Data != null ? componentAmountResult.Data : new List<(int componentId, string componentName, int Quantity, int rId)>();

            var braki = new List<(int componentId, string componentName, int brakQuantity, int rId)>();
            braki = componentAmounts
                .Select(ca =>
                {
                    var requiredQty = sortedData.FirstOrDefault(d => d.elementId == ca.rId).quantity;
                    var brakQty = ca.Quantity < requiredQty ? requiredQty - ca.Quantity : 0;
                    return (ca.componentId, ca.componentName, brakQty, ca.rId);
                })
                .Where(b => b.brakQty > 0)
                .ToList();

            // Zapisz wiersze komponentów z analizą braków i zamienników
            int row = 3;
            foreach (var dr in sortedData)
            {
                ws.Cell(row, 1).Value = dr.name.ToString();
                ws.Cell(row, 2).Value = dr.elementId.ToString();
                ws.Cell(row, 3).Value = dr.quantity.ToString();

                int brakQty = 0;
                string altId = "";
                
                // Sprawdź, czy są braki dla tego komponentu
                if (braki.Any(b => b.rId.ToString() == dr.elementId.ToString()))
                {
                    brakQty = braki.First(b => b.rId.ToString() == dr.elementId.ToString()).brakQuantity;
                    if(brakQty < 0)
                        continue;
                    
                    // Cappuj braki do wymaganej ilości
                    if (brakQty >= int.Parse(dr.quantity.ToString() ?? "0"))
                    {
                        brakQty = int.Parse(dr.quantity.ToString() ?? "0");
                    }
                    brakQty = int.Parse(dr.quantity.ToString() ?? "0");
                    var componentIdResult = _enterpriseDatabase.GetComponentIdByRIdAsync(int.Parse(dr.elementId.ToString() ?? "0")).Result;
                    var alts = componentIdResult.IsSuccess ? _enterpriseDatabase.GetAlternativeComponentsAsync(componentIdResult.Data).Result.Data : null;
                    
                    // Poszukiwanie zamienników
                    if (alts != null)
                    {
                        // 1. Pojedynczy zamiennik pokrywający cały brak
                        foreach (var a in alts)
                        {
                            if (a.Quantity >= brakQty)
                            {
                                brakQty = 0;
                                altId += a.RId.ToString() + " ";
                                break;
                            }
                        }
                        
                        // 2. Kilka zamienników pokrywających cały brak
                        if (brakQty > 0)
                        {
                            foreach (var a in alts)
                            {
                                brakQty -= a.Quantity;
                                altId += a.RId.ToString() + ": " + a.Quantity + " ";
                                if (brakQty <= 0)
                                    break;
                            }
                        }
                    }
                }
                
                // Zapisz informacje o brakach i zamiennikach
                ws.Cell(row, 6).Value = brakQty == 0 ? "" : $"Brakuje {brakQty}";
                ws.Cell(row, 5).Value = altId.Trim();
                
                // Zapisz pojemnik tylko jeśli jest używany przez więcej niż jeden komponent
                string containerValue = "";
                if (!string.IsNullOrEmpty(dr.container) && containerUsageCount.TryGetValue(dr.container, out int count) && count > 1)
                {
                    containerValue = dr.container;
                }
                ws.Cell(row, 4).Value = containerValue;
                
                // Oznaczenie elementów do wygrzania
                if (_enterpriseDatabase.IsInWarmUpAsync(int.Parse(dr.elementId.ToString() ?? "0")).Result.Data)
                {
                    ws.Cell(row, 6).Value = ws.Cell(row, 6).Value.ToString() + " (Do wygrzania)";
                }

                // Określenie strony
                if(isOneSided) // Jednostronna lista SMD - brak oznaczeń stron
                {
                    ws.Cell(row, 7).Value = "";
                    row++;
                    continue;
                }
                var sides = _enterpriseDatabase.GetComponentSideAsync(int.Parse(dr.elementId.ToString() ?? "0"), listId).Result.Data;
                if (sides == null || sides.Length <= 0) // Brak oznaczeń stron
                {
                    ws.Cell(row, 7).Value = "";
                }
                else if (sides.Any(s => s == "TOP")) // Zawiera stronę TOP
                {
                    if (sides.Any(s => s == "BOT")) // Zawiera również stronę BOT
                    {
                        ws.Cell(row, 7).Value = "TOP/BOT"; // Komponent dwustronny
                    }
                    else // Tylko strona TOP
                    {
                        ws.Cell(row, 7).Value = "TOP";
                    }
                }
                else if (sides.Any(s => s == "BOT")) // Tylko strona BOT
                {
                    ws.Cell(row, 7).Value = "BOT"; // Komponent jednostronny BOT
                }

                row++;
            }
            wb.SaveAs(filePath);
        }

        private void RefreshReservationsTHT(object? s = null, EventArgs? e = null)
        {
            var reervations = _enterpriseDatabase.GetReservationsTHTAsync();

            if (reervations.Result.IsSuccess && reervations.Result.Data != null)
            {
                _view.BindReservationsTHTGrid(reervations.Result.Data);
            }
        }

        private void RefreshAdditionalMaterials(object? s = null, EventArgs? e = null)
        {
            try
            {
                var additionalMaterials = _enterpriseDatabase.GetAdditionalMaterialsListAsync().Result.Data;
                if (additionalMaterials != null) 
                {
                    _view.BindAdditionalMaterialsGrid(additionalMaterials);
                }
            }
            catch (Exception ex)
            {
                _view.ShowError("Błąd", $"Błąd podczas odświeżania danych: {ex.Message}");
            }
        }

        private void UpdateAdditionalMaterials(object? s, ListsSelectedEventArgs e)
        {
            var window = new AdditionalMaterialView();
            var listId = e.listId;
            window.SaveClicked += (s, ev) =>
            {
                try
                {
                    var data = window.outputDataTables;
                    if (data != null)
                    {
                        foreach (var dt in data)
                        {   
                            var dtValue = dt.Value;
                            var reel_id = dtValue.Rows[0]["reel"].ToString() ?? "";
                            var box = dtValue.Rows[0]["box"].ToString() ?? "";
                            var quantity = int.Parse(dtValue.Rows[0]["quantity"].ToString() ?? "0");
                            _enterpriseDatabase.UpdateTraceAdditionalMaterialsAsync(listId, (reel_id, box, quantity)).Wait();
                        }

                        foreach (var dt in data)
                        {
                            int componentId = dt.Key;
                            int totalQuantity = dt.Value.Rows.Cast<DataRow>()
                                .Sum(r => int.Parse(r["quantity"].ToString() ?? "0"));
                            _enterpriseDatabase.UpdateReservationsAdditionalMaterialsAsync(listId, (componentId, totalQuantity)).Wait();
                        }

                        RefreshAdditionalMaterials();
                        
                        if (window.AreAllElementsComplete())
                        {
                            _enterpriseDatabase.CompleteAdditionalMaterialsListAsync(listId).Wait();
                            _view.ShowInfo("Sukces", "Wszystkie elementy zostały w pełni wydane.");
                            RefreshAdditionalMaterials();
                        }
                        else
                        {
                            _view.ShowInfo("Sukces", "Dane zostały zapisane.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _view.ShowError("Błąd", $"Błąd podczas zapisywania danych: {ex.Message}");
                }
            };

            window.CancelClicked += (s, ev) =>
            {
                window.Close();
            };

            var idsData = _enterpriseDatabase.GetDataAdditionalMaterialsListAsync(listId).Result.Data;
            if (idsData == null) return;
            foreach (DataRow r in idsData.Rows)
            {
                var idl = int.Parse(r["r_id"].ToString() ?? "0");
                var quantityl = int.Parse(r["quantity"].ToString() ?? "0");
                var name = r["name"].ToString() ?? "";
                window.AddElement(idl, name, 0, quantityl);

                var dt = new DataTable();
                dt.Columns.Add("reel", typeof(string));
                dt.Columns.Add("box", typeof(string));
                dt.Columns.Add("quantity", typeof(int));
                var data = _enterpriseDatabase.GetTraceDataAdditionalMaterialsAsync(listId).Result.Data;
                foreach(DataRow d in data!.Rows)
                {
                    string id = d["reel_id"].ToString() ?? "";
                    var m = Regex.Match(id, "\\d+");
                    if (!m.Success || !m.Value.StartsWith(idl.ToString(), StringComparison.Ordinal)) continue;
                    string box = d["box"].ToString() ?? "";
                    int quantity = int.Parse(d["quantity"].ToString() ?? "0");
                    dt.Rows.Add(id, box, quantity);
                }
                window.SetElementReels(idl, dt);
            }

            window.ShowDialog();
            window.SaveClicked -= null;
            window.Dispose();
        }

        private void DeleteAdditionalMaterial(object? s, ListsSelectedEventArgs e)
        {
            try
            {
                _enterpriseDatabase.DeleteAdditionalMaterialsListAsync(e.listId).Wait();
                RefreshAdditionalMaterials();
                _view.ShowInfo("Sukces", "Lista dodatkowych materiałów została usunięta.");
            }
            catch (Exception ex)
            {
                _view.ShowError("Błąd", $"Błąd podczas usuwania listy: {ex.Message}");
            }
        }

        private void OnReverseLastUpdateTHTRequested(object? sender, ReverseLastUpdateTHTEventArgs e)
        {
            try
            {
                _enterpriseDatabase.ReverseLastUpdateTHTAsync((int)e.ReservationId).Wait();
                RefreshReservations();
                _view.ShowInfo("Sukces", "Ostatnia aktualizacja została cofnięta.");
            }
            catch (Exception ex)
            {
                _view.ShowError("Błąd", $"Błąd podczas cofania aktualizacji: {ex.Message}");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _view.DeleteReservationRequested -= OnDeleteReservationRequested;
                _view.UpdateListDoneRequested -= OnUpdateListDoneRequested;
                _view.ReverseLastUpdateRequested -= OnReverseLastUpdateRequested;
                _view.GenerateListRequested -= OnGenerateListRequested;
            }
            
            base.Dispose(disposing);
        }
    }
}