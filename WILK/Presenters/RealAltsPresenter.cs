using System.Data;
using WILK.Services;
using WILK.Views;

namespace WILK.Presenters
{
    public interface IRealAltsView 
    {
        event EventHandler<ListsSelectedEventArgs>? ListSelected;
        event EventHandler<AddAlternativeEventArgs>? AddAlternativeRequested;
        event EventHandler<EditAlternativeEventArgs>? EditAlternativeRequested;
        event EventHandler<RemoveAlternativeEventArgs>? RemoveAlternativeRequested;
        event EventHandler<EventArgs>? RealAltsTabSelected;


        void ShowError(string title, string message);
        void ShowInfo(string title, string message);
        void BindListGrid(DataTable items);
        void BindComponentGrid(DataTable dt);
    }

    public class RealAltsPresenter : BaseTabPresenter
    {
        private readonly IRealAltsView _view;

        public RealAltsPresenter(IRealAltsView view, IEnterpriseDatabase enterpriseDatabase)
            : base(enterpriseDatabase)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));

            SubscribeToViewEvents();
        }

        private void SubscribeToViewEvents()
        {
            _view.ListSelected += OnListSelected;
            _view.RemoveAlternativeRequested += OnRemoveAlternative;
            _view.EditAlternativeRequested += OnEditAlternative;
            _view.RealAltsTabSelected += (s, e) => OnTabActivated();
            _view.AddAlternativeRequested += async (s, e) => await AddAlternativeAsync(s, e);
        }

        public override void Initialize()
        {
            
        }

        public override void OnTabActivated()
        {
            RefreshItems();
        }

        private void RefreshItems()
        {
            try
            {
                var reservations = _enterpriseDatabase.GetReservationsTableAsync().Result.Data;
                if (reservations != null)
                {
                    _view.BindListGrid(reservations);
                }
                
            }
            catch (Exception ex)
            {
                _view.ShowError("Błąd", $"Błąd podczas odświeżania danych: {ex.Message}");
            }
        }

        public void OnListSelected(object? sender, ListsSelectedEventArgs e)
        {
            var components = _enterpriseDatabase.GetRealReservationsAsync(e.listId).Result;
            if (components != null && components.Data != null)
            {
                _view.BindComponentGrid(components.Data);
            }
        }

        public void OnRemoveAlternative(object? sender, RemoveAlternativeEventArgs e)
        {
            try
            {
                var result = _enterpriseDatabase.RemoveRealAlternativeAsync(e.reservationId, e.alternativeId).Result;

                if (result.IsSuccess)
                {
                    _view.ShowInfo("Sukces", "Alternatywa została usunięta.");
                    RefreshItems();
                }
                else
                {
                    _view.ShowError("Błąd", $"Nie udało się usunąć alternatywy: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                _view.ShowError("Błąd", $"Błąd podczas usuwania alternatywy: {ex.Message}");
            }
        }

        public void OnEditAlternative(object? sender, EditAlternativeEventArgs e)
        {
            try
            {
                var result = _enterpriseDatabase.EditRealReservationAsync(e.reservationId, e.newQuantity).Result;
                if (result.IsSuccess)
                {
                    _view.ShowInfo("Sukces", "Ilość alternatywy została zaktualizowana.");
                    RefreshItems();
                }
                else
                {
                    _view.ShowError("Błąd", $"Nie udało się zaktualizować ilości alternatywy: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                _view.ShowError("Błąd", $"Błąd podczas aktualizacji ilości alternatywy: {ex.Message}");
            }
        }

        public async Task<Models.DatabaseResult<bool>> AddAlternativeAsync(object? sender, AddAlternativeEventArgs e)
        {
            try
            {
                var result = await _enterpriseDatabase.AddRealAlternativeComponentAsync(e.reservationId, e.originalRId, e.substituteRId, e.quantity);
                if (result.IsSuccess)
                {
                    _view.ShowInfo("Sukces", "Alternatywa została dodana.");
                    OnTabActivated();
                }
                else
                {
                    _view.ShowError("Błąd", $"Nie udało się dodać alternatywy: {result.ErrorMessage}");
                }

                return result;
            }
            catch (Exception ex)
            {
                _view.ShowError("Błąd", $"Błąd podczas dodawania alternatywy: {ex.Message}");
                return Models.DatabaseResult<bool>.Failure(ex.Message, ex);
            }
        }
    }
}