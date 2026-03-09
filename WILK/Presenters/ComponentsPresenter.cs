using System;
using System.Data;
using System.Windows.Forms;
using WILK.Services;

namespace WILK.Presenters
{
    public interface IComponentsView
    {
        event EventHandler SearchRequested;
        event EventHandler AddRequested;
        event EventHandler EditRequested;

        string SearchId { get; }
        string SearchName { get; }
        
        void SetGridData(DataTable dt);
        void ShowError(string title, string message);
        void ShowInfo(string title, string message);

        bool ShowComponentDialog(bool isEdit, ref string id, ref string name, ref string type);

        // Metoda do pobierania zaznaczonego wiersza
        (int id, string name, string type)? GetSelectedComponent();
    }
    
    public class ComponentsPresenter : BaseTabPresenter
    {
        private readonly IComponentsView _view;

        public ComponentsPresenter(IComponentsView view, IEnterpriseDatabase db) : base(db)
        {
            _view = view;
            _view.SearchRequested += OnSearch;
            _view.AddRequested += OnAdd;
            _view.EditRequested += OnEdit;
        }

        public override void Initialize()
        {
            OnSearch(this,EventArgs.Empty);
        }

        private async void OnSearch(object sender, EventArgs e)
        {
            try
            {
                string id = string.IsNullOrWhiteSpace(_view.SearchId) ? null : _view.SearchId;
                string name = string.IsNullOrWhiteSpace(_view.SearchName) ? null : _view.SearchName;

                var result = await _enterpriseDatabase.SearchComponentsAsync(id, name);

                if (result.IsSuccess)
                {
                    _view.SetGridData(result.Data);
                }
                else
                {
                    _view.ShowError("Błąd bazy", result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _view.ShowError("Błąd", ex.Message);
            }
        }

        private async void OnAdd(object sender, EventArgs e)
        {
            string id = "";
            string name = "";
            string type = "";

            if (_view.ShowComponentDialog(false, ref id, ref name, ref type))
            {
                if (!int.TryParse(id, out int idInt)) 
                {
                    _view.ShowError("Błąd", "ID musi być liczbą!");
                    return;
                }

                var result = await _enterpriseDatabase.AddComponentAsync(idInt, name, type);

                if (result.IsSuccess)
                {
                    _view.ShowInfo("Sukces", "Dodano nowy komponent!");
                    OnSearch(this, EventArgs.Empty); 
                }
                else
                {
                    _view.ShowError("Błąd zapisu", result.ErrorMessage);
                }
            }
        }

        private async void OnEdit(object sender, EventArgs e)
        {
            var selected = _view.GetSelectedComponent();
            if (selected == null)
            {
                _view.ShowInfo("Info", "Zaznacz coś, żeby edytować.");
                return;
            }

            string id = selected.Value.id.ToString();
            string name = selected.Value.name;
            string type = selected.Value.type;

            if (_view.ShowComponentDialog(true, ref id, ref name, ref type))
            {

                var result = await _enterpriseDatabase.UpdateComponentAsync(selected.Value.id, name, type);

                if (result.IsSuccess)
                {
                    _view.ShowInfo("Sukces", "Zaktualizowano dane.");
                    OnSearch(this, EventArgs.Empty); 
                }
                else
                {
                    _view.ShowError("Błąd edycji", result.ErrorMessage);
                }
            }
        }
    }
}