using System.Data;
using WILK.Services;
using WILK.Views;
using WILK.Views.Tabs;

namespace WILK.Presenters
{
    public interface IExcessiveView
    {
        event EventHandler<ExcessiveUsageEventArgs>? ExcessiveUsageAdded;
        event EventHandler<EventArgs>? ExcessiveTabSelected; 
        event EventHandler<ExcessiveDeleteEventArgs>? DeleteExcessive;

        void ShowError(string title, string message);
        void ShowInfo(string title, string message);
        void BindExcessiveHistoryGrid(System.Data.DataTable table);
    }

    public class ExcessivePresenter : BaseTabPresenter
    {
        private readonly IExcessiveView _view;

        public ExcessivePresenter(IExcessiveView view, IEnterpriseDatabase enterpriseDatabase)
            : base(enterpriseDatabase)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));

            _view.ExcessiveUsageAdded += OnExcessiveUsageAdded;
            _view.ExcessiveTabSelected += (s, e) => OnTabActivated();
            _view.DeleteExcessive += DeleteExcessive;
        }

        private void LoadHistory()
        {
            try
            {
                // Wczytaj historię ponadnormatywnego użycia z ostatnich 30 dni
                var res = _enterpriseDatabase.GetExcessiveUsage(DateTime.UtcNow.AddDays(-30)).Result.Data;
                if (res == null)
                {
                    return;
                }
                _view.BindExcessiveHistoryGrid(res);
            }
            catch (Exception ex)
            {
                _view.ShowError("Błąd ładowania historii", ex.Message);
            }
        }

        public override void Initialize()
        {
            
        }

        public override void OnTabActivated()
        {
            LoadHistory();
        }

        private async void OnExcessiveUsageAdded(object? sender, ExcessiveUsageEventArgs e)
        {
            try
            {
                var checkId = await _enterpriseDatabase.GetComponentIdByRIdAsync(e.RId);
                if (checkId.Data == null) // Sprawdź, czy komponent istnieje
                {
                    _view.ShowError("Błąd dodawania ponadnormatywnego", "Nie znaleziono komponentu o podanym Id.");
                    return;
                }
                var result = _enterpriseDatabase.AddExcessiveUsageAsync(e.RId, e.Quantity, e.Reason, e.ReelId == null ? string.Empty : e.ReelId).Result;

                if (!result.IsSuccess) // Sprawdź, czy dodawanie się powiodło
                {
                    _view.ShowError("Błąd dodawania ponadnormatywnego", result.ErrorMessage ?? "Nieznany błąd");
                }
                else // Sukces
                {
                    _view.ShowInfo("Sukces", "Ponadnormatywne użycie zostało dodane pomyślnie.");
                    LoadHistory();
                }
            }
            catch (Exception ex)
            {
                _view.ShowError("Błąd dodawania ponadnormatywnego", ex.Message);
            }
        }

        public void DeleteExcessive(object? sender, ExcessiveDeleteEventArgs e)
        {
            try
            {
                var result = _enterpriseDatabase.DeleteExcessiveUsageAsync(e.Id).Result;
                if (!result.IsSuccess) // Sprawdź, czy usuwanie się powiodło
                {
                    _view.ShowError("Błąd usuwania ponadnormatywnego", result.ErrorMessage ?? "Nieznany błąd");
                }
                else // Sukces
                {
                    _view.ShowInfo("Sukces", "Ponadnormatywne użycie zostało usunięte pomyślnie.");
                    LoadHistory();
                }
            }
            catch (Exception ex)
            {
                _view.ShowError("Błąd usuwania ponadnormatywnego", ex.Message);
            }
        }
    }
}