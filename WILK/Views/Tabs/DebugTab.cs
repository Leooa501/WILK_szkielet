using System;
using System.Windows.Forms;
using WILK.Services;

namespace WILK.Views.Tabs
{
    /// <summary>
    /// Debug tab for development and testing operations
    /// </summary>
    public class DebugTab : BaseTab
    {
        private Button? _buttonUpdateAlts;
        private Button? _buttonWsad;
        private readonly IFileProcessingService _fileProcessingService;

        public override string TabName => "Debug";

        public event EventHandler? UpdateAlternativesRequested;
        public event EventHandler<ExcelListEventArgs>? WsadImport;

        public DebugTab(IEnterpriseDatabase enterpriseDatabase, IFileProcessingService fileProcessingService, IMainView mainView)
            : base(enterpriseDatabase, mainView)
        {
            _fileProcessingService = fileProcessingService ?? throw new ArgumentNullException(nameof(fileProcessingService));
        }

        protected override void CreateTabPage()
        {
            TabPage = new TabPage("Debug")
            {
                UseVisualStyleBackColor = true
            };
        }

        protected override void SetupControls()
        {
            _buttonUpdateAlts = new Button
            {
                Text = "Aktualizacja listy zamiennikow",
                Size = new System.Drawing.Size(142, 58),
                Location = new System.Drawing.Point(59, 62)
            };

            _buttonWsad = new Button
            {
                Text = "Wsad wszystkich list .xlsx",
                Size = new System.Drawing.Size(188, 61),
                Location = new System.Drawing.Point(220, 62)
            };

            TabPage.Controls.Add(_buttonUpdateAlts);
            TabPage.Controls.Add(_buttonWsad);
        }

        protected override void AttachEventHandlers()
        {
            if (_buttonUpdateAlts != null)
            {
                _buttonUpdateAlts.Click += ButtonUpdateAlts_Click;
            }

            if (_buttonWsad != null)
            {
                _buttonWsad.Click += ButtonWsad_Click;
            }
        }

        private void ButtonUpdateAlts_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog();
            ofd.Filter = "Excel files|*.xlsx;*.xls";
            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                // Trigger event that can be handled by the presenter or main form
                UpdateAlternativesRequested?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating alternatives: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ButtonWsad_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog();
            ofd.Filter = "Excel files|*.xlsx;*.xls";

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            WsadImport?.Invoke(this, new ExcelListEventArgs(ofd.FileName));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _buttonUpdateAlts?.Dispose();
                _buttonWsad?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
