using ClosedXML.Excel;
using Org.BouncyCastle.Crypto.Prng;
using WILK.Services;
using WILK.Views;

namespace WILK.Presenters
{
    public interface ICompletedView
    {
        event EventHandler<EventArgs>? CompletedTabSelected;
        event EventHandler<ListsSelectedEventArgs>? GeneneratedReportClicked;
        event EventHandler<ListsSelectedEventArgs>? CloseListClicked;
        event EventHandler<UpdateTransferredStatusEventArgs>? UpdateTransferredStatusClicked;

        void ShowError(string title, string message);
        void ShowInfo(string title, string message);
        void BindDataGridView(System.Data.DataTable table);
    }

    public class CompletedPresenter : BaseTabPresenter
    {
        private readonly ICompletedView _view;

        public CompletedPresenter(ICompletedView view, IEnterpriseDatabase enterpriseDatabase)
            : base(enterpriseDatabase)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));

            _view.CompletedTabSelected += (s, e) => OnTabActivated();
            _view.GeneneratedReportClicked += GenerateReportClicked;
            _view.CloseListClicked += OnCloseListClicked;
            _view.UpdateTransferredStatusClicked += OnUpdateTransferredStatusClicked;
        }

        public override void Initialize()
        {

        }

        public override void OnTabActivated()
        {
            OnRefreshCompletedData(null, EventArgs.Empty);
        }   

        public void OnRefreshCompletedData(object? sender, EventArgs e)
        {
            try
            {
                var res = _enterpriseDatabase.GetCompletedReservationsTableTHTAsync().Result.Data;
                if (res != null)
                {
                    _view.BindDataGridView(res);
                }
            }
            catch (Exception ex)
            {
                _view.ShowError("Błąd ładowania zakończonych rezerwacji", ex.Message);
            }
        }    

        public void GenerateReportClicked(object? sender, ListsSelectedEventArgs e)
        {
            try
            {
                var ofd = new SaveFileDialog
                {
                    Filter = "Pliki Excel (*.xlsx)|*.xlsx",
                    Title = "Zapisz raport jako",
                    FileName = e.listName + ".xlsx"
                };
                if (ofd.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                var data = _enterpriseDatabase.GetTraceDataTHT(e.listId).Result.Data;
                if (data == null)
                {
                    _view.ShowError("Błąd generowania raportu", "Nie udało się pobrać danych do raportu.");
                    return;
                }

                var wb = new XLWorkbook();
                if (wb.Worksheets.Contains("Raport"))
                {
                    wb.Worksheet("Raport").Delete();
                }
                var ws = wb.Worksheets.Add("Raport");

                // Tytuł
                ws.Cell(1, 1).Value = e.listName ?? "Raport";

                // Nagłówki
                ws.Cell(2, 1).Value = "ID rolki";
                ws.Cell(2, 2).Value = "Ilość";
                ws.Cell(2, 3).Value = "Box";

                // Wypełnianie danych
                int currentRow = 3;
                foreach (System.Data.DataRow row in data.Rows)
                {
                    ws.Cell(currentRow, 1).Value = row["reel_id"]?.ToString();
                    ws.Cell(currentRow, 2).Value = row["quantity"]?.ToString();
                    ws.Cell(currentRow, 3).Value = row["box"]?.ToString();
                    currentRow++;
                }
                wb.SaveAs(ofd.FileName);

                _view.ShowInfo("Generowanie raportu", "Raport został wygenerowany pomyślnie.");
            }
            catch (Exception ex)
            {
                _view.ShowError("Błąd generowania raportu", ex.Message);
            }
        }

        public void OnCloseListClicked(object? sender, ListsSelectedEventArgs e)
        {
            try
            {
                var result = _enterpriseDatabase.CloseReservationTHTAsync(e.listId).Result;

                if (result.IsSuccess)
                {
                    _view.ShowInfo("Sukces", "Rezerwacja została zamknięta.");
                    OnRefreshCompletedData(null, EventArgs.Empty);
                }
                else
                {
                    _view.ShowError("Błąd", $"Nie udało się zamknąć rezerwacji: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                _view.ShowError("Błąd", $"Błąd podczas zamykania rezerwacji: {ex.Message}");
            }
        }

        public void OnUpdateTransferredStatusClicked(object? sender, UpdateTransferredStatusEventArgs e)
        {
            try
            {
                var result = _enterpriseDatabase.UpdateTransferredStatus(e.ListId, e.Transferred).Result;
                if (result.IsSuccess)
                {
                    _view.ShowInfo("Sukces", "Status został zaktualizowany.");
                    OnRefreshCompletedData(null, EventArgs.Empty);
                }
                else
                {
                    _view.ShowError("Błąd", $"Nie udało się zaktualizować statusu: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                _view.ShowError("Błąd", $"Błąd podczas aktualizacji statusu: {ex.Message}");
            }
        }
    }
}