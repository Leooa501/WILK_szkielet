using System;
using System.Data;
using WILK.Models;
using WILK.Services;
using WILK.Views;

namespace WILK.Presenters
{
    public interface IAlternativesView
    {
        // Events
        event EventHandler<AlternativeDeleteEventArgs>? AlternativeDeleteRequested;
        event EventHandler<EventArgs>? AlternativesTabSelected; 

        // Funkcje widoku
        void BindAlternativesGrid(DataTable dt);
        void ShowInfo(string title, string message);
        void ShowError(string title, string message);
    }

    public class AlternativesPresenter : BaseTabPresenter
    {
        private readonly IAlternativesView _view;

        public AlternativesPresenter(IAlternativesView view, IEnterpriseDatabase enterpriseDatabase) : base(enterpriseDatabase)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            
            SubscribeToViewEvents();
        }

        private void SubscribeToViewEvents()
        {
            _view.AlternativeDeleteRequested += OnAlternativeDeleteRequested;
            _view.AlternativesTabSelected += (s, e) => OnTabActivated();
        }

        public override void Initialize()
        {
            
        }

        public override void OnTabActivated()
        {
            LoadAlternatives();
        }

        private void OnAlternativeDeleteRequested(object? sender, AlternativeDeleteEventArgs e)
        {
            try
            {
                var MessageBoxResult = MessageBox.Show("Czy na pewno chcesz usunąć ten zamiennik?", "Potwierdzenie usunięcia", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (MessageBoxResult != DialogResult.Yes)
                {
                    return;
                }
                
                _enterpriseDatabase.DeleteAlternativeComponentAsync(e.AlternativeId).Wait();
                LoadAlternatives();
                _view.ShowInfo("Sukces", "Zamiennik został usunięty.");
            }
            catch (Exception ex)
            {
                _view.ShowError("Błąd", $"Błąd podczas usuwania zamiennika: {ex.Message}");
            }
        }

        private void LoadAlternatives()
        {
            try
            {
                var result = _enterpriseDatabase.GetAlternativeComponentsTableAsync().Result;
                if (!result.IsSuccess)
                {
                    _view.ShowError("Błąd", $"Błąd podczas ładowania alternatyw: {result.ErrorMessage}");
                    return;
                }
                
                if (result.Data != null)
                {
                    _view.BindAlternativesGrid(result.Data);
                }
            }
            catch (Exception ex)
            {
                _view.ShowError("Błąd", $"Błąd podczas ładowania alternatyw: {ex.Message}");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _view.AlternativeDeleteRequested -= OnAlternativeDeleteRequested;
            }
            
            base.Dispose(disposing);
        }
    }
}