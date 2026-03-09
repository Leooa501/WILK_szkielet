using WILK.Services;

namespace WILK.Views.Tabs
{
    public class ShortageTab : BaseTab
    {
        private TabControl? _nestedTabControl;
        private TabManager? _nestedTabManager;
        private readonly IFileProcessingService _fileProcessingService;

        public override string TabName => "Listy wybraniowe";

        public ShortageTab(IEnterpriseDatabase enterpriseDatabase, IFileProcessingService fileProcessingService, IMainView mainView)
            : base(enterpriseDatabase, mainView)
        {
            _fileProcessingService = fileProcessingService ?? throw new ArgumentNullException(nameof(fileProcessingService));
        }

        protected override void CreateTabPage()
        {
            TabPage = new TabPage("Braki")
            {
                UseVisualStyleBackColor = true
            };
        }

        protected override void SetupControls()
        {
            _nestedTabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Name = "shortageNestedTabControl",
                Padding = new Point(0, 0)
            };
            TabPage.Controls.Add(_nestedTabControl);

            _nestedTabManager = new TabManager(_nestedTabControl, _enterpriseDatabase, _fileProcessingService, _mainView);

            _nestedTabManager.RegisterTab("Reports", () => new ReportTab(_enterpriseDatabase, _fileProcessingService, _mainView));

            _nestedTabManager.InitializeAllTabs();
        }

        protected override void AttachEventHandlers()
        {
            _nestedTabControl!.HandleCreated += (s, e) =>
                _nestedTabControl.BeginInvoke(() => _nestedTabManager!.SelectInitialTab("Reports"));
            
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
