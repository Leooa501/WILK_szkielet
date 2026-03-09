using WILK.Services;
using WILK.Services.Configuration;
using WILK.Views.Tabs;

namespace WILK.Views
{
    public partial class MainForm : Form, IMainView
    {
        private IEnterpriseDatabase _enterpriseDatabase;
        private IFileProcessingService _fileProcessingService;
        private TabManager? _tabManager;
        private AppSettings _appSettings;


        public MainForm(IEnterpriseDatabase enterpriseDatabase, IFileProcessingService fileProcessingService, AppSettings appSettings)
        {
            InitializeComponent();
            
            _enterpriseDatabase = enterpriseDatabase ?? throw new ArgumentNullException(nameof(enterpriseDatabase));
            _fileProcessingService = fileProcessingService ?? throw new ArgumentNullException(nameof(fileProcessingService));
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            
            // Set version in status bar
            versionLabel.Text = $"Wersja: {_appSettings.Version}";
            
            InitializeTabManager();

            base.Load += (s, e) => Load?.Invoke(this, EventArgs.Empty);
        }

        private void InitializeTabManager()
        {
            // Create the main TabControl that TabManager will manage
            var mainTabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Name = "mainTabControl"
            };
            
            this.Controls.Add(mainTabControl);
            this.Controls.SetChildIndex(mainTabControl, 0);
            
            // Initialize TabManager with the main TabControl
            _tabManager = new TabManager(mainTabControl, _enterpriseDatabase, _fileProcessingService, this);

            // Register top-level tabs
            _tabManager.RegisterTab("Aktualizacje", () => new UpdatesTab(_enterpriseDatabase, _fileProcessingService, this));
            _tabManager.RegisterTab("Shortage", () => new ShortageTab(_enterpriseDatabase, _fileProcessingService, this));
            _tabManager.RegisterTab("Excessive", () => new ExcessiveTab(_enterpriseDatabase, this));
            _tabManager.RegisterTab("Completed", () => new CompletedTab(_enterpriseDatabase, this));
            _tabManager.RegisterTab("Errors", () => new ErrorsTab(_enterpriseDatabase, this));
            //_tabManager.RegisterTab("Debug", () => new DebugTab(_enterpriseDatabase, _fileProcessingService, this));

            // Initialize all top-level tabs
            _tabManager.InitializeAllTabs();
            
            // Defer SelectInitialTab until the control is fully rendered
            mainTabControl.HandleCreated += (s, e) => 
                mainTabControl.BeginInvoke(() => _tabManager.SelectInitialTab("Aktualizacje"));
        }

        // ///////////////////////////////////////////////
        // IMainView events (presenter subscribes)
        public new event EventHandler? Load;

        public void ShowInfo(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void ShowError(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void ShowMessage(string message)
        {
            MessageBox.Show(message);
        }
    }
}
