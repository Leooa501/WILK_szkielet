using System.Data;
using System.Xml;
using DocumentFormat.OpenXml.InkML;
using WILK.Presenters;
using WILK.Services;

namespace WILK.Views.Tabs
{
    public class CompletedTab : BaseTab, ICompletedView
    {
        public event EventHandler<EventArgs>? CompletedTabSelected;
        public event EventHandler<ListsSelectedEventArgs>? GeneneratedReportClicked;
        public event EventHandler<ListsSelectedEventArgs>? CloseListClicked;
        public event EventHandler<UpdateTransferredStatusEventArgs>? UpdateTransferredStatusClicked;

        public override string TabName => "Wydane listy THT";

        private DataGridView? _dataGridView;
        private Panel? _mainPanel;        
        private Label? _headerLabel;        
        private Button? _closeButton;
        private Button? _generateReportButton;
        private TextBox? _searchTextBox;
        private Label? _seachLabel;

        private CompletedPresenter? _presenter;
        private bool _disposed = false;

        public CompletedTab(IEnterpriseDatabase enterpriseDatabase, IMainView mainView)
            : base(enterpriseDatabase, mainView)
        {
            _presenter = new CompletedPresenter(this, enterpriseDatabase);
        }

        protected override void CreateTabPage()
        {
            TabPage = new TabPage(TabName)
            {
                UseVisualStyleBackColor = true
            };
        }

        public override void OnTabSelected()
        {
            CompletedTabSelected?.Invoke(this, EventArgs.Empty);
        }

        protected override void SetupControls()
        {
            _mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            _headerLabel = new Label
            {
                AutoSize = true,
                Location = new System.Drawing.Point(10, 14),
                Name = "labelChooseList",
                TabIndex = 0,
                Text = "Wybierz listę"
            };

            _dataGridView = new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AllowUserToResizeColumns = false,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoGenerateColumns = true,
                ReadOnly = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Location = new System.Drawing.Point(10, 40)
            };

            _dataGridView.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(250, 250, 250);
            _dataGridView.ShowCellToolTips = true;

            _closeButton = new Button
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Name = "buttonCompletedGenerate",
                Size = new System.Drawing.Size(120, 40),
                TabIndex = 1,
                Text = "Zakończ",
                UseVisualStyleBackColor = true
            };

            _generateReportButton = new Button
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Name = "buttonCompletedAction",
                Size = new System.Drawing.Size(120, 40),
                TabIndex = 2,
                Text = "Generuj raport",
                UseVisualStyleBackColor = true
            };

            _searchTextBox = new TextBox
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Name = "searchTextBox",
                Size = new System.Drawing.Size(200, 22),
                TabIndex = 3,
                PlaceholderText = "Szukaj..."
            };

            _seachLabel = new Label
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                AutoSize = true,
                Name = "searchLabel",
                TabIndex = 4,
                Text = "Wyszukaj:"
            };

            _mainPanel.SizeChanged += (s, e) =>
            {
                if (_mainPanel == null) return;

                int buttonMargin = 10;
                int maxButtonHeight = Math.Max(_generateReportButton!.Height, _closeButton!.Height);
                int buttonY = _mainPanel.Height - maxButtonHeight - buttonMargin;

                _closeButton.Location = new System.Drawing.Point(buttonMargin, buttonY);
                _generateReportButton.Location = new System.Drawing.Point(_mainPanel.Width - _generateReportButton.Width - buttonMargin, buttonY);
                
                int searchBoxTop = 10;
                _seachLabel.Location = new System.Drawing.Point(buttonMargin, searchBoxTop + 3);
                _searchTextBox.Location = new System.Drawing.Point(buttonMargin + _seachLabel.Width + 10, searchBoxTop);

                int headerTop = 24 + searchBoxTop + 10;
                _headerLabel!.Location = new System.Drawing.Point(10, headerTop);
                int gridTop = _headerLabel.Bottom + 8;
                _dataGridView.Location = new System.Drawing.Point(10, gridTop);

                int gridWidth = Math.Max(100, _mainPanel.Width - 20);
                int gridHeight = Math.Max(100, _mainPanel.Height - maxButtonHeight - gridTop - 20);
                _dataGridView.Size = new System.Drawing.Size(gridWidth, gridHeight);
            };

            _mainPanel.Controls.Add(_headerLabel);
            _mainPanel.Controls.Add(_dataGridView);
            _mainPanel.Controls.Add(_closeButton);
            _mainPanel.Controls.Add(_generateReportButton);
            _mainPanel.Controls.Add(_seachLabel);
            _mainPanel.Controls.Add(_searchTextBox);
            TabPage.Controls.Add(_mainPanel);
        }

        protected override void AttachEventHandlers()
        {
            if (_closeButton != null)
                _closeButton.Click += CloseButton_Click;

            if (_generateReportButton != null)
                _generateReportButton.Click += GenerateReportButton_Click;

            if (_searchTextBox != null)
            {
                _searchTextBox.TextChanged += (s, e) =>
                {
                    string filterText = _searchTextBox.Text.Trim().ToLower();
                    if (_dataGridView != null && _dataGridView.DataSource is DataTable dt)
                    {
                        if (string.IsNullOrEmpty(filterText))
                        {
                            dt.DefaultView.RowFilter = string.Empty;
                        }
                        else
                        {
                            dt.DefaultView.RowFilter = $"name LIKE '%{filterText.Replace("'", "''")}%'";
                        }
                    }
                };
            }
        }

        public void BindDataGridView(DataTable dt)
        {
            if(_dataGridView == null)
                return;

            _dataGridView.DataSource = dt;
            _dataGridView.ShowCellToolTips = true;
            _dataGridView.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(250, 250, 250);

            if (_dataGridView.Columns.Contains("id") && _dataGridView.Columns["id"] != null)
            {
                _dataGridView.Columns["id"].Visible = false;
            }

            if (_dataGridView.Columns.Contains("name") && _dataGridView.Columns["name"] != null)
            {
                _dataGridView.Columns["name"].HeaderText = "Nazwa rezerwacji";
                _dataGridView.Columns["name"].ReadOnly = true;
            }

            if (_dataGridView.Columns.Contains("updatedAt") && _dataGridView.Columns["updatedAt"] != null)
            {
                _dataGridView.Columns["updatedAt"].HeaderText = "Data zakończenia";
                _dataGridView.Columns["updatedAt"].ReadOnly = true;
            }

            if (_dataGridView.Columns.Contains("start") && _dataGridView.Columns["start"] != null)
            {
                _dataGridView.Columns["start"].HeaderText = "Ilość";
                _dataGridView.Columns["start"].ReadOnly = true;
            }

            if (_dataGridView.Columns.Contains("createdAt") && _dataGridView.Columns["createdAt"] != null)
            {
                _dataGridView.Columns["createdAt"].HeaderText = "Data utworzenia";
                _dataGridView.Columns["createdAt"].ReadOnly = true;
            }

            if (_dataGridView.Columns.Contains("assignedPerson") && _dataGridView.Columns["assignedPerson"] != null)
            {
                _dataGridView.Columns["assignedPerson"].HeaderText = "Przydzielona osoba";
                _dataGridView.Columns["assignedPerson"].ReadOnly = true;
            }

            if (_dataGridView.Columns.Contains("destination") && _dataGridView.Columns["destination"] != null)
            {
                _dataGridView.Columns["destination"].HeaderText = "Miejsce docelowe";
                _dataGridView.Columns["destination"].ReadOnly = true;
            }

            if (_dataGridView.Columns.Contains("maxEndDate") && _dataGridView.Columns["maxEndDate"] != null)
            {
                _dataGridView.Columns["maxEndDate"].HeaderText = "Maksymalna data zakończenia";
                _dataGridView.Columns["maxEndDate"].ReadOnly = true;
                _dataGridView.Columns["maxEndDate"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }

            if (dt.Columns.Contains("transferred") && _dataGridView.Columns["transferred"] != null)
            {
                int idx = _dataGridView.Columns["transferred"].Index;

                _dataGridView.Columns.Remove("transferred");
                var chk = new DataGridViewCheckBoxColumn
                {
                    Name = "transferred",
                    HeaderText = "Przekazano",
                    DataPropertyName = "transferred",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader,
                    ReadOnly = false
                };
                _dataGridView.Columns.Insert(idx, chk);
                _dataGridView.Columns["transferred"].ReadOnly = false;
            }

            // Kolejność kolumn
            _dataGridView.Columns["name"].DisplayIndex = 0;
            _dataGridView.Columns["start"].DisplayIndex = 1;
            _dataGridView.Columns["createdAt"].DisplayIndex = 2;
            _dataGridView.Columns["updatedAt"].DisplayIndex = 3;

            _dataGridView.CellValueChanged -= DataGridView_CellValueChanged;
            _dataGridView.CurrentCellDirtyStateChanged -= DataGridView_CurrentCellDirtyStateChanged;
            _dataGridView.CellValueChanged += DataGridView_CellValueChanged;
            _dataGridView.CurrentCellDirtyStateChanged += DataGridView_CurrentCellDirtyStateChanged;
        }

        private void GenerateReportButton_Click(object? sender, EventArgs e)
        {
            if (_dataGridView == null || _dataGridView.SelectedCells.Count == 0)
            {
                ShowError("Błąd", "Wybierz wiersz z listy, aby zobaczyć szczegóły.");
                return;
            }

            GeneneratedReportClicked?.Invoke(this, new ListsSelectedEventArgs(int.TryParse(_dataGridView.SelectedRows[0].Cells["id"].Value?.ToString(), out int listId) ? listId : -1,
                                                                             _dataGridView.SelectedRows[0].Cells["name"].Value?.ToString()));
        }

        private void CloseButton_Click(object? sender, EventArgs e)
        {
            if (_dataGridView == null || _dataGridView.SelectedCells.Count == 0)
            {
                ShowError("Błąd", "Wybierz wiersz z listy, aby zobaczyć szczegóły.");
                return;
            }

            var result = MessageBox.Show("Czy na pewno chcesz zakończyć tę rezerwację? Operacji tej nie można cofnąć.",
                                         "Potwierdzenie", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.No)
                return;

            CloseListClicked?.Invoke(this, new ListsSelectedEventArgs(int.TryParse(_dataGridView.SelectedRows[0].Cells["id"].Value?.ToString(), out int listId) ? listId : -1));
        }

        private void DataGridView_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
        {
            if (_dataGridView == null || _dataGridView.IsCurrentCellDirty)
            {
                _dataGridView?.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void DataGridView_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (_dataGridView == null || e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            var column = _dataGridView.Columns[e.ColumnIndex];
            if (column.Name == "transferred")
            {
                var idCell = _dataGridView.Rows[e.RowIndex].Cells["id"];
                if (idCell.Value != null && int.TryParse(idCell.Value.ToString(), out int listId))
                {
                    bool transferred = Convert.ToBoolean(_dataGridView.Rows[e.RowIndex].Cells["transferred"].Value);
                    UpdateTransferredStatusClicked?.Invoke(this, new UpdateTransferredStatusEventArgs(listId, transferred));
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                if (_closeButton != null)
                {
                    _closeButton.Click -= CloseButton_Click;
                }

                if (_generateReportButton != null)
                {
                    _generateReportButton.Click -= GenerateReportButton_Click;
                }

                if (_dataGridView != null)
                {
                    _dataGridView.CellValueChanged -= DataGridView_CellValueChanged;
                    _dataGridView.CurrentCellDirtyStateChanged -= DataGridView_CurrentCellDirtyStateChanged;
                }

                _mainPanel?.Dispose();                
                _headerLabel?.Dispose();                
                _closeButton?.Dispose();
                _generateReportButton?.Dispose();
                _dataGridView?.Dispose();
                _presenter?.Dispose();

                _disposed = true;
            }

            base.Dispose(disposing);
        }

    }
}