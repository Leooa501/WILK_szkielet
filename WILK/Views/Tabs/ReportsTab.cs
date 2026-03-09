using WILK.Controls;
using WILK.Presenters;
using WILK.Services;

namespace WILK.Views.Tabs
{
    public class ReportTab : BaseTab, IReportView
    {
        private Panel? _mainPanel;
        private GroupBox? _groupBoxListShortage;
        private GroupBox? _groupBoxGlobalShortage;
        private GroupBox? _groupBoxDaily;
        private Label? _labelGenerateFrom;
        private DateTimePicker? _datePicker;
        private Label? _labelLastReport;
        private Button? _buttonDaily;
        private DropDownButton? _buttonImportExcelList;
        private Label? _labelImportedFile;
        private Button? _buttonCheckListBraki;
        private Button? _buttonExportBraki;
        private CheckBox? _checkBoxTHT;
        private ContextMenuStrip? _ImportExcelListMenu;


        public event EventHandler<EventArgs>? ExcelListImported;
        public event EventHandler<EventArgs>? CheckBrakiRequested;
        public event EventHandler<ExportBrakiEventArgs>? ExportBrakiRequested;
        public event EventHandler<DailyUsageEventArgs>? DailyButtonClicked;
        public event EventHandler<EventArgs>? ReportsTabSelected;
        
        private ReportsPresenter? _presenter;

        public ReportTab(IEnterpriseDatabase enterpriseDatabase, IFileProcessingService fileProcessingService, IMainView mainView)
            : base(enterpriseDatabase, mainView)
        {
            _presenter = new ReportsPresenter(this, enterpriseDatabase, fileProcessingService);
            _presenter.Initialize();
        }

        public override string TabName => "Raporty";

        protected override void CreateTabPage()
        {
            TabPage = new TabPage(TabName) { UseVisualStyleBackColor = true };
        }

        protected override void SetupControls()
        {
            _mainPanel = new Panel { Dock = DockStyle.Fill };

            _groupBoxGlobalShortage = new GroupBox
            {
                Text = "Całkowite Braki",
                Size = new Size(250, 185),
                Anchor = AnchorStyles.None
            };

            _checkBoxTHT = new CheckBox
            {
                Text = "Uwzględnij THT",
                AutoSize = true,
                Location = new Point(16, 38)
            };

            _buttonExportBraki = new Button
            {
                Text = "Zapisz braki",
                Size = new Size(132, 64),
                Location = new Point(6, 67)
            };

            _groupBoxGlobalShortage.Controls.Add(_checkBoxTHT);
            _groupBoxGlobalShortage.Controls.Add(_buttonExportBraki);

            _groupBoxListShortage = new GroupBox
            {
                Text = "Braki Listy",
                Size = new Size(429, 185),
                Anchor = AnchorStyles.None
            };

            _buttonImportExcelList = new DropDownButton
            {
                Size = new Size(164, 47),
                Location = new Point(6, 26)
            };
            _buttonImportExcelList.SetText("Lista wybraniowa");

            _labelImportedFile = new Label
            {
                Text = "Wgrany plik: pusty",
                AutoSize = true,
                Location = new Point(6, 76)
            };

            _buttonCheckListBraki = new Button
            {
                Text = "Sprawdz braki list",
                Size = new Size(164, 63),
                Location = new Point(6, 113)
            };

            _ImportExcelListMenu = new ContextMenuStrip();
            _ImportExcelListMenu.Items.Add("Sprawdź dla wielu", null, (s, e) => 
            {
                MultipleListsView multipleListsView = new MultipleListsView();
                multipleListsView.ShowDialog();
                if (multipleListsView.DialogResult == DialogResult.OK)
                {
                    var selectedFiles = multipleListsView._files.Select(f => f.FileName).ToList();
                    if (selectedFiles != null && selectedFiles.Count > 0)
                    {
                        ExcelListImported?.Invoke(this, new MultipleFilesEventArgs(multipleListsView._files));
                    }
                }
            });
            _buttonImportExcelList.SetContextMenu(_ImportExcelListMenu);

            _groupBoxListShortage.Controls.Add(_buttonImportExcelList);
            _groupBoxListShortage.Controls.Add(_labelImportedFile);
            _groupBoxListShortage.Controls.Add(_buttonCheckListBraki);

            _groupBoxDaily = new GroupBox
            {
                Text = "Dzienne Zużycie",
                Size = new Size(250, 185),
                Anchor = AnchorStyles.None
            };

            _labelGenerateFrom = new Label
            {
                Text = "Generuj od:",
                AutoSize = true,
                Location = new Point(15, 20)
            };

            _datePicker = new DateTimePicker
            {
                Location = new Point(15, 40),
                Size = new Size(220, 25),
                Format = DateTimePickerFormat.Short
            };

            _labelLastReport = new Label
            {
                Text = "Ostatni raport: -",
                AutoSize = true,
                Location = new Point(15, 120)
            };

            _buttonDaily = new Button
            {
                Text = "Generuj raporty",
                Size = new Size(140, 35),
                Location = new Point(55, 70)
            };
            _groupBoxDaily.Controls.Add(_labelGenerateFrom);
            _groupBoxDaily.Controls.Add(_datePicker);
            _groupBoxDaily.Controls.Add(_labelLastReport);
            _groupBoxDaily.Controls.Add(_buttonDaily);

            _mainPanel.Controls.Add(_groupBoxGlobalShortage);
            _mainPanel.Controls.Add(_groupBoxListShortage);
            _mainPanel.Controls.Add(_groupBoxDaily);

            _mainPanel.Resize += (s, e) =>
            {
                if (_groupBoxGlobalShortage != null && _groupBoxListShortage != null && _groupBoxDaily != null)
                {
                    int centerY = _mainPanel.Height / 2;
                    int spacing = 10;
                    int totalWidth = _groupBoxGlobalShortage.Width + spacing + _groupBoxListShortage.Width + spacing + _groupBoxDaily.Width;
                    int startX = (_mainPanel.Width - totalWidth) / 2;

                    _groupBoxGlobalShortage.Location = new Point(startX, centerY - _groupBoxGlobalShortage.Height / 2);
                    _groupBoxListShortage.Location = new Point(startX + _groupBoxGlobalShortage.Width + spacing, centerY - _groupBoxListShortage.Height / 2);
                    _groupBoxDaily.Location = new Point(startX + _groupBoxGlobalShortage.Width + spacing + _groupBoxListShortage.Width + spacing, centerY - _groupBoxDaily.Height / 2);
                }
            };

            TabPage.Controls.Add(_mainPanel);
        }

        protected override void AttachEventHandlers()
        {
            if (_buttonCheckListBraki != null)
                _buttonCheckListBraki.Click += ButtonCheckListBraki_Click;

            if (_buttonExportBraki != null)
                _buttonExportBraki.Click += ButtonExportBraki_Click;

            if (_buttonDaily != null)
                _buttonDaily.Click += ButtonDaily_Click;

            if (_buttonImportExcelList != null)
                _buttonImportExcelList.SetOnClick(ButtonImportExcelList_Click);
        }

        public override void OnTabSelected()
        {
            ReportsTabSelected?.Invoke(this, EventArgs.Empty);
        }

        private void ButtonImportExcelList_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "Excel files|*.xlsx;*.xls",
                Title = "Wybierz plik Excel z listą wybraniową"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                ExcelListImported?.Invoke(this, new ExcelListEventArgs(ofd.FileName));
            }
        }

        private void ButtonCheckListBraki_Click(object? sender, EventArgs e)
        {
            CheckBrakiRequested?.Invoke(this, EventArgs.Empty);
        }

        private void ButtonExportBraki_Click(object? sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog
            {
                Filter = "Excel files|*.xlsx",
                Title = "Zapisz braki do pliku Excel",
                FileName = "Braki.xlsx"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                bool onlySMD = !_checkBoxTHT?.Checked ?? false;
                ExportBrakiRequested?.Invoke(this, new ExportBrakiEventArgs(sfd.FileName, onlySMD));
            }
        }

        private void ButtonDaily_Click(object? sender, EventArgs e)
        {
            DateTime selectedDate = _datePicker?.Value ?? DateTime.Today;
            DailyButtonClicked?.Invoke(this, new DailyUsageEventArgs(selectedDate));
        }

        public void SetImportedFileName(string fileName)
        {
            if (_labelImportedFile != null)
            {
                _labelImportedFile.Text = $"Wgrany plik: {fileName}";
            }
        }

        public void SetLastReportDate(DateTime? lastReportDate)
        {
            if (_labelLastReport != null)
            {
                if (lastReportDate.HasValue)
                {
                    _labelLastReport.Text = $"Ostatni raport: {lastReportDate.Value:dd-MM-yyyy}";
                }
                else
                {
                    _labelLastReport.Text = "Ostatni raport: -";
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _mainPanel?.Dispose();
                _groupBoxListShortage?.Dispose();
                _groupBoxGlobalShortage?.Dispose();
                _groupBoxDaily?.Dispose();
                _presenter?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}