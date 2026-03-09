using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using WILK.Presenters;
using WILK.Services;

namespace WILK.Views.Tabs
{
    public class AlternativesTab : BaseTab, IAlternativesView
    {
        private Panel? _mainPanel;
        private DataGridView? _dataGridViewAlts;
        private TextBox? _textBoxSearchField;
        private Label? _searchLabel;
        private Button? _buttonAddNewAlternative;
        private Button? _buttonDeleteAlternative;
        private DataTable? _currentDataTable;
        private AlternativesPresenter? _presenter;
        private string? _alternativesGridFilter;
        
        public event EventHandler<AlternativeDeleteEventArgs>? AlternativeDeleteRequested;
        public event EventHandler<EventArgs>? AlternativesTabSelected; 

        public override string TabName => "Zamienniki";

        public AlternativesTab(IEnterpriseDatabase enterpriseDatabase, IMainView mainView)
            : base(enterpriseDatabase, mainView)
        {
            _presenter = new AlternativesPresenter(this, enterpriseDatabase);
        }

        protected override void CreateTabPage()
        {
            TabPage = new TabPage("Zamienniki")
            {
                UseVisualStyleBackColor = true
            };
        }

        protected override void SetupControls()
        {
            _mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Name = "panel8",
                Padding = new Padding(10)
            };
            _searchLabel = new Label
            {
                AutoSize = true,
                Location = new System.Drawing.Point(10, 14),
                Name = "label9",
                TabIndex = 3,
                Text = "Wyszukaj:"
            };

            _textBoxSearchField = new TextBox
            {
                Location = new System.Drawing.Point(90, 11),
                Name = "textBoxSearchField1",
                Size = new System.Drawing.Size(200, 27),
                TabIndex = 4,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            _buttonAddNewAlternative = new Button
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Name = "ButtonAddNewAlternative",
                Size = new System.Drawing.Size(120, 45),
                TabIndex = 1,
                Text = "Dodaj nowy",
                UseVisualStyleBackColor = true
            };

            _buttonDeleteAlternative = new Button
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Name = "ButtonDeleteAlternative",
                Size = new System.Drawing.Size(120, 45),
                TabIndex = 2,
                Text = "Usuń",
                UseVisualStyleBackColor = true
            };
            _dataGridViewAlts = new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                Location = new System.Drawing.Point(10, 50),
                MultiSelect = false,
                Name = "dataGridViewAlts",
                ReadOnly = true,
                RowHeadersVisible = false,
                RowHeadersWidth = 51,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                TabIndex = 0
            };
            _dataGridViewAlts.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
            // Automatyczne dostosowywanie położenia kontrolek przy resize
            _mainPanel.SizeChanged += (s, e) =>
            {
                if (_mainPanel.Height > 100 && _mainPanel.Width > 200)
                {
                    int buttonY = _mainPanel.Height - 55;                    
                    int buttonMargin = 10;
                    
                    if (_buttonAddNewAlternative != null)
                    {
                        _buttonAddNewAlternative.Location = new System.Drawing.Point(buttonMargin, buttonY);
                    }
                    
                    if (_buttonDeleteAlternative != null)
                    {
                        _buttonDeleteAlternative.Location = new System.Drawing.Point(_mainPanel.Width - 120 - buttonMargin, buttonY);
                    }
                    if (_dataGridViewAlts != null)
                    {
                        int gridWidth = _mainPanel.Width - 20;                        
                        int gridHeight = _mainPanel.Height - 50 - 55 - 10;
                        _dataGridViewAlts.Size = new System.Drawing.Size(gridWidth, Math.Max(gridHeight, 100));
                    }
                }
            };

            _mainPanel.Controls.Add(_searchLabel);
            _mainPanel.Controls.Add(_textBoxSearchField);
            _mainPanel.Controls.Add(_dataGridViewAlts);
            _mainPanel.Controls.Add(_buttonAddNewAlternative);
            _mainPanel.Controls.Add(_buttonDeleteAlternative);
            TabPage.Controls.Add(_mainPanel);
        }

        protected override void AttachEventHandlers()
        {
            if (_buttonAddNewAlternative != null)
                _buttonAddNewAlternative.Click += ButtonAddNewAlternative_Click;
            if (_buttonDeleteAlternative != null)
                _buttonDeleteAlternative.Click += ButtonDeleteAlternative_Click;
        }

        public override void OnTabSelected()
        {
            // Zresetuj filtr wyszukiwania
            _alternativesGridFilter = null;
            if (_textBoxSearchField != null)
                _textBoxSearchField.Text = "";
            
            AlternativesTabSelected?.Invoke(this, EventArgs.Empty);
        }

        public void BindAlternativesGrid(DataTable dt)
        {
            if (_dataGridViewAlts == null) return;
            
            SaveCurrentAlternativesFilter();
            
            _currentDataTable = dt;
            
            // Zastosuj zapisany filtr
            if (!string.IsNullOrEmpty(_alternativesGridFilter))
            {
                try
                {
                    var view = dt.DefaultView;
                    view.RowFilter = _alternativesGridFilter;
                }
                catch
                {
                    _alternativesGridFilter = null;
                }
            }
            
            _dataGridViewAlts.DataSource = dt;
            if (_dataGridViewAlts == null) return;
            
            _currentDataTable = dt;
            _dataGridViewAlts.DataSource = dt;
            _dataGridViewAlts.ShowCellToolTips = true;

            // Konfiguracja kolumn
            if (dt.Columns.Contains("id"))
                _dataGridViewAlts.Columns["id"].Visible = false;

            if (dt.Columns.Contains("o_id"))
            {
                _dataGridViewAlts.Columns["o_id"].HeaderText = "ID komponentu";
                _dataGridViewAlts.Columns["o_id"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }

            if (dt.Columns.Contains("o_name"))
            {
                _dataGridViewAlts.Columns["o_name"].HeaderText = "Nazwa komponentu";
                _dataGridViewAlts.Columns["o_name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }

            if (dt.Columns.Contains("z_id"))
            {
                _dataGridViewAlts.Columns["z_id"].HeaderText = "ID alternatywy";
                _dataGridViewAlts.Columns["z_id"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }

            if (dt.Columns.Contains("z_name"))
            {
                _dataGridViewAlts.Columns["z_name"].HeaderText = "Nazwa alternatywy";
                _dataGridViewAlts.Columns["z_name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
            
            _dataGridViewAlts.CellToolTipTextNeeded -= DataGridViewAlts_CellToolTipTextNeeded;
            _dataGridViewAlts.CellToolTipTextNeeded += DataGridViewAlts_CellToolTipTextNeeded;
            if (_textBoxSearchField != null)
            {
                _textBoxSearchField.TextChanged -= TextBoxSearchField_TextChanged;
                _textBoxSearchField.TextChanged += TextBoxSearchField_TextChanged;
            }
        }

        private void DataGridViewAlts_CellToolTipTextNeeded(object? sender, DataGridViewCellToolTipTextNeededEventArgs e)
        {
            if (e.RowIndex >= 0 && (e.ColumnIndex == 2 || e.ColumnIndex == 4) && _dataGridViewAlts != null)
            {
                var cellValue = _dataGridViewAlts.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                e.ToolTipText = cellValue?.ToString();
            }
        }

        private void TextBoxSearchField_TextChanged(object? sender, EventArgs e)
        {
            // Logika filtrowania
            if (_currentDataTable == null || _textBoxSearchField == null) return;

            string search = _textBoxSearchField.Text.Trim().Replace("'", "''").ToLower();

            if (string.IsNullOrEmpty(search))
            {
                _currentDataTable.DefaultView.RowFilter = "";
                _alternativesGridFilter = "";
                return;
            }

            var filters = new List<string>();
            foreach (DataColumn col in _currentDataTable.Columns)
            {
                filters.Add($"Convert([{col.ColumnName}], 'System.String') LIKE '%{search}%'");
            }

            var filterString = string.Join(" OR ", filters);
            _currentDataTable.DefaultView.RowFilter = filterString;
            _alternativesGridFilter = filterString;
        }

        private void SaveCurrentAlternativesFilter()
        {
            if (_dataGridViewAlts?.DataSource is DataTable dt && dt.DefaultView != null && !string.IsNullOrEmpty(dt.DefaultView.RowFilter))
            {
                _alternativesGridFilter = dt.DefaultView.RowFilter;
            }
        }

        private void ButtonAddNewAlternative_Click(object? sender, EventArgs e)
        {
            var addAlternativeForm = new AlternativeAddForm(_enterpriseDatabase);
            addAlternativeForm.ShowDialog();
            OnTabSelected(); // Odśwież listę po dodaniu nowej alternatywy
        }

        private void ButtonDeleteAlternative_Click(object? sender, EventArgs e)
        {
            if (_dataGridViewAlts?.SelectedCells.Count == 0)
            {
                ShowError("Błąd", "Wybierz zamiennik do usunięcia!");
                return;
            }

            if (_dataGridViewAlts != null && _dataGridViewAlts.SelectedCells.Count > 0)
            {
                int alternativeId = Convert.ToInt32(_dataGridViewAlts.Rows[_dataGridViewAlts.SelectedCells[0].RowIndex].Cells["id"].Value);
                AlternativeDeleteRequested?.Invoke(this, new AlternativeDeleteEventArgs(alternativeId));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                if (_dataGridViewAlts != null)
                {
                    _dataGridViewAlts.CellToolTipTextNeeded -= DataGridViewAlts_CellToolTipTextNeeded;
                }
                if (_textBoxSearchField != null)
                {
                    _textBoxSearchField.TextChanged -= TextBoxSearchField_TextChanged;
                }
                if (_buttonAddNewAlternative != null)
                {
                    _buttonAddNewAlternative.Click -= ButtonAddNewAlternative_Click;
                }
                if (_buttonDeleteAlternative != null)
                {
                    _buttonDeleteAlternative.Click -= ButtonDeleteAlternative_Click;
                }
                _mainPanel?.Dispose();
                _dataGridViewAlts?.Dispose();
                _textBoxSearchField?.Dispose();
                _searchLabel?.Dispose();
                _buttonAddNewAlternative?.Dispose();
                _buttonDeleteAlternative?.Dispose();
                _currentDataTable = null;
                _presenter?.Dispose();
            }

            base.Dispose(disposing);
        }

        private bool _disposed = false;
    }
}