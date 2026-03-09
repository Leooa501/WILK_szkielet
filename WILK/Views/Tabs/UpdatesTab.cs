using System;
using System.Windows.Forms;
using WILK.Services;

namespace WILK.Views.Tabs
{
    public class UpdatesTab : BaseTab
    {
        private TabControl? _nestedTabControl;
        private TabManager? _nestedTabManager;
        private readonly IFileProcessingService _fileProcessingService;

        public override string TabName => "Aktualizacje";

        public UpdatesTab(IEnterpriseDatabase enterpriseDatabase, IFileProcessingService fileProcessingService, IMainView mainView)
            : base(enterpriseDatabase, mainView)
        {
            _fileProcessingService = fileProcessingService ?? throw new ArgumentNullException(nameof(fileProcessingService));
        }

        protected override void CreateTabPage()
        {
            TabPage = new TabPage("Ogólne")
            {
                UseVisualStyleBackColor = true
            };
        }

        protected override void SetupControls()
        {
            _nestedTabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Name = "nestedTabControl",
                Padding = new Point(0, 0) 
            };

            TabPage.Controls.Add(_nestedTabControl);

            _nestedTabManager = new TabManager(_nestedTabControl, _enterpriseDatabase, _fileProcessingService, _mainView);

            _nestedTabManager.RegisterTab("Lists", () => new ListsTab(_enterpriseDatabase, _fileProcessingService, _mainView));
            _nestedTabManager.RegisterTab("Alternatives", () => new AlternativesTab(_enterpriseDatabase, _mainView));
            _nestedTabManager.RegisterTab("WarmUp", () => new WarmUpTab(_enterpriseDatabase, _mainView));
            _nestedTabManager.RegisterTab("RealAlts", () => new RealAltsTab(_enterpriseDatabase, _mainView));
            _nestedTabManager.RegisterTab("Components", () => new ComponentsTab(_enterpriseDatabase, _mainView));

            _nestedTabManager.InitializeAllTabs();
        }

        protected override void AttachEventHandlers()
        {
            _nestedTabControl!.HandleCreated += (s, e) => 
                _nestedTabControl.BeginInvoke(() => _nestedTabManager!.SelectInitialTab("Lists"));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _nestedTabManager?.Dispose();
                _nestedTabControl?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
