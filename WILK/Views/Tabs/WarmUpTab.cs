using System.Data;
using WILK.Presenters;
using WILK.Services;

namespace WILK.Views.Tabs
{
    public class WarmUpTab : BaseTab, IWarmUpView
    {
        private Panel? _mainPanel;
        private DataGridView? _dataGridWarmUp;
        private TextBox? _textBoxSearchField;
        private Label? _searchLabel;
        private Button? _buttonAddNewWarmUp;
        private Button? _buttonDeleteWarmUp;
        private DataTable? _currentDataTable;
        private WarmUpPresenter? _presenter;
        
        private string? _warmUpGridFilter;

        public event EventHandler<WarmUpDeleteEventArgs>? WarmUpDeleteRequested;
        public event EventHandler<EventArgs>? WarmUpTabSelected;

        public override string TabName => "Do wygrzania";

        public WarmUpTab(IEnterpriseDatabase enterpriseDatabase, IMainView mainView)
            : base(enterpriseDatabase, mainView)
        {
            _presenter = new WarmUpPresenter(this, enterpriseDatabase);
        }

        protected override void CreateTabPage()
        {
            TabPage = new TabPage("Do wygrzania")
            {
                UseVisualStyleBackColor = true
            };
        }

        protected override void SetupControls()
        {
            _mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Name = "panel9"
            };

            _textBoxSearchField = new TextBox
            {
                Location = new Point(94, 11),
                Name = "textBoxSearchField2",
                Size = new Size(182, 27),
                TabIndex = 6
            };

            _searchLabel = new Label
            {
                AutoSize = true,
                Location = new Point(15, 14),
                Name = "label10",
                Size = new Size(73, 20),
                TabIndex = 5,
                Text = "Wyszukaj:"
            };

            _buttonDeleteWarmUp = new Button
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Name = "ButtonDeleteWarmUp",
                Size = new Size(119, 52),
                TabIndex = 8,
                Text = "Usuń",
                UseVisualStyleBackColor = true
            };

            _buttonAddNewWarmUp = new Button
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Name = "ButtonAddNewWarmUp",
                Size = new Size(119, 52),
                TabIndex = 7,
                Text = "Dodaj nowy",
                UseVisualStyleBackColor = true
            };

            _dataGridWarmUp = new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                Location = new Point(3, 44),
                MultiSelect = false,
                Name = "dataGridWarmUp",
                ReadOnly = true,
                RowHeadersVisible = false,
                RowHeadersWidth = 51,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                TabIndex = 0
            };
            
            _dataGridWarmUp.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
            _mainPanel.Controls.Add(_buttonDeleteWarmUp);
            _mainPanel.Controls.Add(_buttonAddNewWarmUp);
            _mainPanel.Controls.Add(_textBoxSearchField);
            _mainPanel.Controls.Add(_searchLabel);
            _mainPanel.Controls.Add(_dataGridWarmUp);
            TabPage.Controls.Add(_mainPanel);
            SetupResponsiveLayout();
        }

        private void SetupResponsiveLayout()
        {
            if (_mainPanel == null) return;
            _mainPanel.Resize += (s, e) =>
            {
                if (_buttonDeleteWarmUp != null && _buttonAddNewWarmUp != null && _dataGridWarmUp != null)
                {
                    var panelHeight = _mainPanel.Height;
                    var panelWidth = _mainPanel.Width;
                    var buttonY = panelHeight - 70; 
                    _buttonDeleteWarmUp.Location = new Point(panelWidth - 134, buttonY);
                    _buttonAddNewWarmUp.Location = new Point(15, buttonY);
                    _dataGridWarmUp.Size = new Size(panelWidth - 6, buttonY - 50); 
                }
            };
        }

        protected override void AttachEventHandlers()
        {
            if (_buttonAddNewWarmUp != null)
                _buttonAddNewWarmUp.Click += ButtonAddNewWarmUp_Click;
            if (_buttonDeleteWarmUp != null)
                _buttonDeleteWarmUp.Click += ButtonDeleteWarmUp_Click;
        }

        public override void OnTabSelected()
        {
            _warmUpGridFilter = null;
            if (_textBoxSearchField != null)
                _textBoxSearchField.Text = "";

            WarmUpTabSelected?.Invoke(this, EventArgs.Empty);
        }

        public void BindWarmUpGrid(DataTable dt)
        {
            if (_dataGridWarmUp == null) return;
            
            // Zapisz bieżący filtr przed aktualizacją danych
            SaveCurrentWarmUpFilter();
            
            _currentDataTable = dt;
            
            // Ponowne zastosowanie filtru po aktualizacji danych
            if (!string.IsNullOrEmpty(_warmUpGridFilter))
            {
                try
                {
                    var view = dt.DefaultView;
                    view.RowFilter = _warmUpGridFilter;
                }
                catch
                {
                    _warmUpGridFilter = null; // Jeśli filtr jest nieprawidłowy, zresetuj go
                }
            }
            
            _dataGridWarmUp.DataSource = dt;
            _dataGridWarmUp.ShowCellToolTips = true;

            if (dt.Columns.Contains("id"))
                _dataGridWarmUp.Columns["id"].Visible = false;

            if (dt.Columns.Contains("r_id"))
            {
                _dataGridWarmUp.Columns["r_id"].HeaderText = "ID komponentu";
                _dataGridWarmUp.Columns["r_id"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }

            if (dt.Columns.Contains("name"))
            {
                _dataGridWarmUp.Columns["name"].HeaderText = "Nazwa komponentu";
                _dataGridWarmUp.Columns["name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
            _dataGridWarmUp.CellToolTipTextNeeded -= DataGridWarmUp_CellToolTipTextNeeded;
            _dataGridWarmUp.CellToolTipTextNeeded += DataGridWarmUp_CellToolTipTextNeeded;
            if (_textBoxSearchField != null)
            {
                _textBoxSearchField.TextChanged -= TextBoxSearchField_TextChanged;
                _textBoxSearchField.TextChanged += TextBoxSearchField_TextChanged;
            }
        }

        private void DataGridWarmUp_CellToolTipTextNeeded(object? sender, DataGridViewCellToolTipTextNeededEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 2 && _dataGridWarmUp != null)
            {
                var cellValue = _dataGridWarmUp.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                e.ToolTipText = cellValue?.ToString();
            }
        }

        private void TextBoxSearchField_TextChanged(object? sender, EventArgs e) // Filtrowanie danych w siatce
        {
            if (_currentDataTable == null || _textBoxSearchField == null) return;

            string search = _textBoxSearchField.Text.Trim().Replace("'", "''").ToLower();

            if (string.IsNullOrEmpty(search))
            {
                _currentDataTable.DefaultView.RowFilter = "";
                _warmUpGridFilter = "";
                return;
            }

            var filters = new List<string>();
            foreach (DataColumn col in _currentDataTable.Columns)
            {
                filters.Add($"Convert([{col.ColumnName}], 'System.String') LIKE '%{search}%'");
            }

            var filterString = string.Join(" OR ", filters);
            _currentDataTable.DefaultView.RowFilter = filterString;
            _warmUpGridFilter = filterString;
        }

        private void SaveCurrentWarmUpFilter()
        {
            if (_dataGridWarmUp?.DataSource is DataTable dt && dt.DefaultView != null && !string.IsNullOrEmpty(dt.DefaultView.RowFilter))
            {
                _warmUpGridFilter = dt.DefaultView.RowFilter;
            }
        }

        private void ButtonAddNewWarmUp_Click(object? sender, EventArgs e)
        {
            var addWarmUpForm = new WarmUpAddForm(_enterpriseDatabase);
            addWarmUpForm.ShowDialog();
            OnTabSelected();
        }

        private void ButtonDeleteWarmUp_Click(object? sender, EventArgs e)
        {
            if (_dataGridWarmUp?.SelectedCells.Count == 0)
            {
                ShowError("Błąd", "Wybierz komponent do usunięcia!");
                return;
            }

            if (_dataGridWarmUp != null && _dataGridWarmUp.SelectedCells.Count > 0)
            {
                int warmUpId = Convert.ToInt32(_dataGridWarmUp.Rows[_dataGridWarmUp.SelectedCells[0].RowIndex].Cells["id"].Value);
                WarmUpDeleteRequested?.Invoke(this, new WarmUpDeleteEventArgs(warmUpId));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                if (_dataGridWarmUp != null)
                {
                    _dataGridWarmUp.CellToolTipTextNeeded -= DataGridWarmUp_CellToolTipTextNeeded;
                }
                if (_textBoxSearchField != null)
                {
                    _textBoxSearchField.TextChanged -= TextBoxSearchField_TextChanged;
                }
                if (_buttonAddNewWarmUp != null)
                {
                    _buttonAddNewWarmUp.Click -= ButtonAddNewWarmUp_Click;
                }
                if (_buttonDeleteWarmUp != null)
                {
                    _buttonDeleteWarmUp.Click -= ButtonDeleteWarmUp_Click;
                }
                _mainPanel?.Dispose();
                _dataGridWarmUp?.Dispose();
                _textBoxSearchField?.Dispose();
                _searchLabel?.Dispose();
                _buttonAddNewWarmUp?.Dispose();
                _buttonDeleteWarmUp?.Dispose();

                _currentDataTable = null;
                _presenter?.Dispose();
            }

            base.Dispose(disposing);
        }

        private bool _disposed = false;
    }
}