using System;
using System.Data;
using WILK.Services;
using WILK.Views;

namespace WILK.Presenters
{
    public interface IWarmUpView
    {
        event EventHandler<WarmUpDeleteEventArgs>? WarmUpDeleteRequested;
        event EventHandler<EventArgs>? WarmUpTabSelected;

        void BindWarmUpGrid(DataTable dt);
        void ShowInfo(string title, string message);
        void ShowError(string title, string message);
    }

    public class WarmUpPresenter : BaseTabPresenter
    {
        private readonly IWarmUpView _view;

        public WarmUpPresenter(IWarmUpView view, IEnterpriseDatabase enterpriseDatabase) : base(enterpriseDatabase)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));

            SubscribeToViewEvents();
        }

        private void SubscribeToViewEvents()
        {
            _view.WarmUpDeleteRequested += OnWarmUpDeleteRequested;
            _view.WarmUpTabSelected += (s, e) => OnTabActivated();
        }

        public override void Initialize()
        {
            
        }

        public override void OnTabActivated()
        {
            LoadWarmUpComponents();
        }

        private void OnWarmUpDeleteRequested(object? sender, WarmUpDeleteEventArgs e)
        {
            try
            {
                var MessageBoxResult = MessageBox.Show("Czy na pewno chcesz usunąć ten komponent z listy rozgrzewania?", "Potwierdzenie usunięcia", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (MessageBoxResult != DialogResult.Yes)
                {
                    return;
                }
                _enterpriseDatabase.DeleteWarmUpComponentAsync(e.WarmUpId).Wait();
                LoadWarmUpComponents();
                _view.ShowInfo("Sukces", "Komponent został usunięty z listy rozgrzewania.");
            }
            catch (Exception ex)
            {
                _view.ShowError("Błąd", $"Błąd podczas usuwania komponentu: {ex.Message}");
            }
        }

        private void LoadWarmUpComponents()
        {
            try
            {
                var warmUpComponents = _enterpriseDatabase.GetWarmUpComponentsTableAsync().Result.Data;
                if (warmUpComponents != null)
                {
                    _view.BindWarmUpGrid(warmUpComponents);
                }
            }
            catch (Exception ex)
            {
                _view.ShowError("Błąd", $"Błąd podczas ładowania komponentów rozgrzewania: {ex.Message}");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _view.WarmUpDeleteRequested -= OnWarmUpDeleteRequested;
                _view.WarmUpTabSelected -= (s, e) => OnTabActivated();
            }
            
            base.Dispose(disposing);
        }
    }
}