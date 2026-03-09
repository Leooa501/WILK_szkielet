using System.Data;
using WILK.Services;
using WILK.Views;
using WILK.Views.Tabs;

namespace WILK.Presenters
{
    public interface IErrorsView
    {
        event EventHandler<ErrorsEventArgs>? ErrorsAdded;
        event EventHandler<EventArgs>? ErrorsTabSelected; 

        void ShowError(string title, string message);
        void ShowInfo(string title, string message);
        void BindErrorsHistoryGrid(System.Data.DataTable table);
    }

    public class ErrorsPresenter : BaseTabPresenter
    {
        private readonly IErrorsView _view;

        public ErrorsPresenter(IErrorsView view, IEnterpriseDatabase enterpriseDatabase)
            : base(enterpriseDatabase)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));

            _view.ErrorsAdded += OnErrorsAdded;
            _view.ErrorsTabSelected += (s, e) => OnTabActivated();
        }

        private void LoadHistory()
        {
            try
            {
                // Wczytaj historię błędów z ostatnich 30 dni
                var res = _enterpriseDatabase.GetErrors(DateTime.UtcNow.AddDays(-30)).Result.Data;
                if (res == null)
                {
                    return;
                }
                _view.BindErrorsHistoryGrid(res);
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

        private async void OnErrorsAdded(object? sender, ErrorsEventArgs e)
        {
            try
            {
                var result = _enterpriseDatabase.AddErrorsAsync(e.Type, e.Cause, e.ReelId, e.CorrectAmount, e.CorrectOrder, e.CorrectBox, e.Description, e.Author).Result;

                if (!result.IsSuccess) // Sprawdź, czy dodawanie się powiodło
                {
                    _view.ShowError("Błąd błędu", result.ErrorMessage ?? "Nieznany błąd");
                }
                else // Sukces
                {
                    _view.ShowInfo("Sukces", "Błąd dodany pomyślnie.");
                    LoadHistory();
                }
            }
            catch (Exception ex)
            {
                _view.ShowError("Błąd dodawania błędu", ex.Message); //kurza twarz
            }
        }
    }
}