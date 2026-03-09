using System;
using System.Data;
using System.Windows.Forms;
using WILK.Services;

namespace WILK.Views.Tabs
{
    public abstract class BaseTab : ITab, IDisposable
    {
        protected readonly IEnterpriseDatabase _enterpriseDatabase;
        protected readonly IMainView _mainView;
        private bool _disposed = false;

        public abstract string TabName { get; }
        public TabPage TabPage { get; protected set; }
        public bool IsInitialized { get; private set; }

        protected BaseTab(IEnterpriseDatabase enterpriseDatabase, IMainView mainView)
        {
            _enterpriseDatabase = enterpriseDatabase ?? throw new ArgumentNullException(nameof(enterpriseDatabase));
            _mainView = mainView ?? throw new ArgumentNullException(nameof(mainView));
        }

        public virtual void Initialize()
        {
            if (IsInitialized)
                return;

            CreateTabPage();
            SetupControls();
            AttachEventHandlers();
            IsInitialized = true;
        }

        protected abstract void CreateTabPage();
        protected abstract void SetupControls();
        protected abstract void AttachEventHandlers();

        public virtual void OnTabSelected()
        {
            // Nadpisz w klasach pochodnych dla specyficznego zachowania
        }

        public virtual void OnTabDeselected()
        {
            // Nadpisz w klasach pochodnych dla specyficznego zachowania
        }

        public virtual void ShowError(string title, string message)
        {
            _mainView.ShowError(title, message);
        }

        public virtual void ShowInfo(string title, string message)
        {
            _mainView.ShowInfo(title, message);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                TabPage?.Dispose();
                _disposed = true;
            }
        }
    }
}