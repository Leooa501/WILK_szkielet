using System.Data;
using WILK.Services;
using WILK.Presenters;

namespace WILK.Views.Tabs
{
    public class RealAltsTab : BaseTab, IRealAltsView
    {
        private DataGridView? _dataGridView1;   
        private DataGridView? _dataGridView2;   
        private DataRow? _selectedListRow;
        private int? _savedListId; // snapshot of list id to restore after binding
        private DataRow? _selectedAlternativeRow; // selected component row (in grid2)
        private int? _savedComponentId; // snapshot of component (r_id) to restore after binding
        private DataRow? _selectedReplacementRow; // selected alternative row (in grid3)
        private TextBox? _searchBox1;
        private TextBox? _searchBox2;
        private DataGridView? _dataGridView3;
        private DataTable? _dataTable1;

        private Button? _buttonAddAlternative;
        private Button? _buttonRemoveAlternative;

        private TableLayoutPanel? _layoutPanel;
        private Panel? _topPanel;
        private Panel? buttonsPanel;
        private Label? _labelList;
        private Label? _labelTopRight;
        private Label? _labelComps;
     
        public event EventHandler<ListsSelectedEventArgs>? ListSelected;
        public event EventHandler<AddAlternativeEventArgs>? AddAlternativeRequested;
        public event EventHandler<EditAlternativeEventArgs>? EditAlternativeRequested;
        public event EventHandler<RemoveAlternativeEventArgs>? RemoveAlternativeRequested;
        public event EventHandler<EventArgs>? RealAltsTabSelected;

        private RealAltsPresenter? _presenter;
        public RealAltsTab(IEnterpriseDatabase enterpriseDatabase, IMainView mainView)
            : base(enterpriseDatabase, mainView)
        {
            _presenter = new RealAltsPresenter(this, enterpriseDatabase);
        }

        public override string TabName => "Zamienniki";

        protected override void CreateTabPage()
        {
            TabPage = new TabPage("Zamienniki na listach") 
            { 
                UseVisualStyleBackColor = true 
            };
        }

        protected override void AttachEventHandlers()
        {
            if (_dataGridView2 != null)
            {
                _dataGridView2.SelectionChanged += BindAlternativesGrid;
                // Zapamiętanie zaznaczonego wiersza
                _dataGridView2.SelectionChanged += (s, e) =>
                {
                    if (_dataGridView2 == null || _dataGridView2.SelectedRows.Count == 0) { _selectedAlternativeRow = null; return; }
                    var sel = _dataGridView2.SelectedRows[0];
                    _selectedAlternativeRow = (sel.DataBoundItem as DataRowView)?.Row;
                };
            }
            if (_buttonAddAlternative != null)
                _buttonAddAlternative.Click += ButtonAddAlternative_Click;
            if (_buttonRemoveAlternative != null)
                _buttonRemoveAlternative.Click += ButtonRemoveAlternative_Click;

            if (_dataGridView1 != null)
                _dataGridView1.SelectionChanged += (s, e) =>
                {
                    if (_dataGridView1 == null || _dataGridView1.SelectedRows.Count == 0) { _selectedListRow = null; return; }

                    var selectedRow = _dataGridView1.SelectedRows[0];
                    _selectedListRow = (selectedRow.DataBoundItem as DataRowView)?.Row;
                    if (selectedRow.Cells["id"].Value != null)
                    {
                        int listId = Convert.ToInt32(selectedRow.Cells["id"].Value);
                        ListSelected?.Invoke(this, new ListsSelectedEventArgs(listId));
                    }
                };

            if ( _dataGridView3 != null )
            {
                _dataGridView3.SelectionChanged += (s, e) =>
                {
                    bool hasSelection = _dataGridView3 != null && _dataGridView3.SelectedRows.Count > 0;
                    if (_buttonRemoveAlternative != null) _buttonRemoveAlternative.Enabled = hasSelection;
                };

                // Zapamiętanie zaznaczonego wiersza
                _dataGridView3.SelectionChanged += (s, e) =>
                {
                    if (_dataGridView3 == null || _dataGridView3.SelectedRows.Count == 0) { _selectedReplacementRow = null; return; }
                    var sel = _dataGridView3.SelectedRows[0];
                    _selectedReplacementRow = (sel.DataBoundItem as DataRowView)?.Row;
                };
            }

            if (_topPanel != null)
                _topPanel.Resize +=  ResizeTopPanelButtons;
        }

        public override void OnTabSelected()
        {
            RealAltsTabSelected?.Invoke(this, EventArgs.Empty);
        }

        protected override void SetupControls()
        {
            _labelList = new Label
            {
                Text = "Wybierz listę:",
                Location = new Point(8, 8),
                AutoSize = true,
                Padding = new Padding(4, 4, 4, 4)
            };

            _searchBox1 = new TextBox
            {
                Width = 180,
                PlaceholderText = "Szukaj listy...",
                Height = 24
            };

            _dataGridView1 = new DataGridView
            {
                Dock = DockStyle.None,
                Size = new Size(300, 120),
                Location = new Point(8, 60),
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                BackgroundColor = SystemColors.Window,
                BorderStyle = BorderStyle.None,
                EnableHeadersVisualStyles = false,
                GridColor = SystemColors.ControlLight,
                CellBorderStyle = DataGridViewCellBorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 8F),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            _dataGridView3 = new DataGridView
            {
                Dock = DockStyle.None,
                Size = new Size(260, 120),
                Location = new Point(0, 0),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                BackgroundColor = SystemColors.Window,
                BorderStyle = BorderStyle.None,
                EnableHeadersVisualStyles = false,
                GridColor = SystemColors.ControlLight,
                CellBorderStyle = DataGridViewCellBorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 8F),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            _labelTopRight = new Label
            {
                Text = "Zamienniki:",
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(0, 0)
            }; 

            _buttonAddAlternative = new Button
            {
                Text = "Dodaj",
                Size = new Size(80, 26),
                Enabled = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            _buttonRemoveAlternative = new Button
            {
                Text = "Usuń",
                Size = new Size(80, 26),
                Enabled = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            _labelComps = new Label
            {
                Text = "Wybierz komponent:",
                Dock = DockStyle.Top,
                Height = 28,
                Padding = new Padding(4, 4, 4, 4)
            };

            _searchBox2 = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 24,
                Width = 60,
                Margin = new Padding(0, 0, 0, 4),
                PlaceholderText = "Szukaj komponentu..."
            };

            _dataGridView2 = new DataGridView
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 8, 0, 0),
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                BackgroundColor = SystemColors.Window,
                BorderStyle = BorderStyle.FixedSingle,
                EnableHeadersVisualStyles = false,
                GridColor = SystemColors.ControlLight,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 9F),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            _layoutPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            _layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 170F));
            _layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            _topPanel = new Panel { Dock = DockStyle.Fill };
            _topPanel.Controls.Add(_labelList);
            _topPanel.Controls.Add(_searchBox1);
            _topPanel.Controls.Add(_labelTopRight);
            _topPanel.Controls.Add(_dataGridView1);
            _topPanel.Controls.Add(_dataGridView3);

            buttonsPanel = new Panel { Size = new Size(90, 66), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            buttonsPanel.Controls.Add(_buttonAddAlternative);
            buttonsPanel.Controls.Add(_buttonRemoveAlternative);
            _buttonAddAlternative.Location = new Point(5, 5);
            _buttonRemoveAlternative.Location = new Point(5, 36);
            _topPanel.Controls.Add(buttonsPanel);
            _labelList?.BringToFront();
            _labelTopRight?.BringToFront();

            if (_dataGridView3 != null)
            {
                int y = (_labelList?.Bottom ?? 28) + 8;

                _dataGridView1.Location = new Point(8, y);
                _dataGridView1.BringToFront();

                _searchBox1.Location = new Point(_dataGridView1.Right + 8, _dataGridView1.Top);
                _searchBox1.BringToFront();

                int btnX = Math.Max(8, _topPanel.ClientSize.Width - buttonsPanel.Width - 8);
                buttonsPanel.Location = new Point(btnX, y);
                buttonsPanel.BringToFront();

                int initX = Math.Max(8, btnX - 6 - _dataGridView3.Width);
                _dataGridView3.Location = new Point(initX, y);
                _dataGridView3.BringToFront();

                if (_labelTopRight != null)
                {
                    _labelTopRight.Location = new Point(_dataGridView3.Left, Math.Max(4, _dataGridView3.Top - _labelTopRight.Height - 4));
                    _labelTopRight.BringToFront();
                }
            }
            else
            {
                int y = (_labelList?.Bottom ?? 28) + 8;

                _dataGridView1.Location = new Point(8, y);
                _dataGridView1.BringToFront();

                _searchBox1.Location = new Point(_dataGridView1.Right + 8, _dataGridView1.Top);
                _searchBox1.BringToFront();

                int initBtnX = Math.Max(8, _topPanel.ClientSize.Width - buttonsPanel.Width - 8);
                buttonsPanel.Location = new Point(Math.Max(8, initBtnX), y);
                buttonsPanel.BringToFront();
            }

            _layoutPanel.Controls.Add(_topPanel, 0, 0);

            var compPanel = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(8, 6, 8, 8), ColumnCount = 1, RowCount = 3 };
            compPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            compPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            compPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            _labelComps.Dock = DockStyle.Fill;
            _labelComps.Margin = new Padding(0, 0, 0, 2);
            _searchBox2.Margin = new Padding(0, 0, 0, 4);
            _dataGridView2.Dock = DockStyle.Fill;
            _dataGridView2.Margin = new Padding(0);

            compPanel.Controls.Add(_labelComps, 0, 0);
            compPanel.Controls.Add(_searchBox2, 0, 1);
            compPanel.Controls.Add(_dataGridView2, 0, 2);

            _layoutPanel.Controls.Add(compPanel, 0, 1);

            TabPage.Controls.Add(_layoutPanel);
        }

        private void ResizeTopPanelButtons(object? sender, EventArgs? e)
        {
            if (_topPanel == null) return;

            int rightPadding = 8;
            int buttonsGap = 6;

            if (_dataGridView1 != null)
            {
                int y = (_labelList?.Bottom ?? 0) + 8;
                _dataGridView1.Location = new Point(8, y);
                _dataGridView1.BringToFront();

                if (_searchBox1 != null)
                {
                    _searchBox1.Location = new Point(_dataGridView1.Right + 8, _dataGridView1.Top);
                    _searchBox1.BringToFront();
                }
            }

            if (_dataGridView3 != null && buttonsPanel != null && _labelTopRight != null)
            {
                int y = (_labelList?.Bottom ?? 0) + 8;
                int btnX = _topPanel.ClientSize.Width - buttonsPanel.Width - rightPadding;
                if (btnX < 8) btnX = 8;
                buttonsPanel.Location = new Point(btnX, y);
                buttonsPanel.BringToFront();

                int x = Math.Max(8, btnX - buttonsGap - _dataGridView3.Width);
                _dataGridView3.Location = new Point(x, y);
                _dataGridView3.BringToFront();

                _labelTopRight.Location = new Point(_dataGridView3.Left, Math.Max(4, _dataGridView3.Top - _labelTopRight.Height - 4));
                _labelTopRight.BringToFront();
                buttonsPanel.BringToFront();
            }
        }

        private DataTable? _originalListTable;
        public void BindListGrid(DataTable dt)
        {
            if (_dataGridView1 == null) return;

            _originalListTable = dt.Copy();
            // Snapshot current selected id to avoid SelectionChanged during binding overwriting it
            if (_selectedListRow != null && _selectedListRow.Table.Columns.Contains("id") && _selectedListRow["id"] != DBNull.Value)
            {
                try { _savedListId = Convert.ToInt32(_selectedListRow["id"]); } catch { _savedListId = null; }
            }
            else _savedListId = null;

            _dataGridView1.DataSource = dt; 

            // Restore previously selected row (if any) after binding completes to avoid default selection overriding it
            if (_dataGridView1 != null)
            {
                DataGridViewBindingCompleteEventHandler? handler = null;
                handler = (s, e) =>
                {
                    _dataGridView1.DataBindingComplete -= handler;
                    try
                    {
                        _dataGridView1.BeginInvoke((MethodInvoker)(() => RestoreSelectedListRow()));
                    }
                    catch { }
                };
                _dataGridView1.DataBindingComplete += handler;
            }

            _dataGridView1.ShowCellToolTips = true;
            if (_searchBox1 != null)
            {
                _searchBox1.TextChanged -= SearchBox1_TextChanged;
                _searchBox1.TextChanged += SearchBox1_TextChanged;
            }

            if (dt.Columns.Contains("id") && _dataGridView1.Columns["id"] != null)
                _dataGridView1.Columns["id"].Visible = false;

            if (dt.Columns.Contains("is_list") && _dataGridView1.Columns["is_list"] != null)
                _dataGridView1.Columns["is_list"].Visible = false;

            if (dt.Columns.Contains("is_one_sided") && _dataGridView1.Columns["is_one_sided"] != null)
                _dataGridView1.Columns["is_one_sided"].Visible = false;

            if (dt.Columns.Contains("name") && _dataGridView1.Columns["name"] != null)
            {
                _dataGridView1.Columns["name"].HeaderText = "Nazwa";
                _dataGridView1.Columns["name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }

            if (dt.Columns.Contains("created_at") && _dataGridView1.Columns["created_at"] != null)
            {
                _dataGridView1.Columns["created_at"].HeaderText = "Data utworzenia";
                _dataGridView1.Columns["created_at"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }

            if (dt.Columns.Contains("last_update_done_bot") && _dataGridView1.Columns["last_update_done_bot"] != null)
            {
                _dataGridView1.Columns["last_update_done_bot"].Visible = false;
            }

            if (dt.Columns.Contains("last_update_done_top") && _dataGridView1.Columns["last_update_done_top"] != null)
            {
                _dataGridView1.Columns["last_update_done_top"].Visible = false;
            }

            if (dt.Columns.Contains("start") && _dataGridView1.Columns["start"] != null)
            {
                _dataGridView1.Columns["start"].Visible = false;
            }

            if (dt.Columns.Contains("done_top") && _dataGridView1.Columns["done_top"] != null)
            {
                _dataGridView1.Columns["done_top"].Visible = false;
            }

            if (dt.Columns.Contains("done_bot") && _dataGridView1.Columns["done_bot"] != null)
            {
                _dataGridView1.Columns["done_bot"].Visible = false;
            }

            // Ustawienie kolejności kolumn
            if (_dataGridView1.Columns["name"] != null)
                _dataGridView1.Columns["name"].DisplayIndex = 0;
            if (_dataGridView1.Columns["created_at"] != null)
                _dataGridView1.Columns["created_at"].DisplayIndex = 1;

        }

        // Restore the previously selected row in _dataGridView1 (if possible)
        private void RestoreSelectedListRow()
        {
            if (_dataGridView1 == null) return;
            int? selId = null;
            if (_savedListId != null)
            {
                selId = _savedListId;
            }
            else if (_selectedListRow != null && _selectedListRow.Table.Columns.Contains("id") && _selectedListRow["id"] != DBNull.Value)
            {
                try { selId = Convert.ToInt32(_selectedListRow["id"]); } catch { selId = null; }
            }
            if (selId == null) return;

            try
            {
                _dataGridView1.ClearSelection();
                foreach (DataGridViewRow row in _dataGridView1.Rows)
                {
                    if (row.Cells["id"].Value != null && Convert.ToInt32(row.Cells["id"].Value) == selId.Value)
                    {
                        row.Selected = true;
                        _dataGridView1.CurrentCell = row.Cells.Cast<DataGridViewCell>().FirstOrDefault(c => c.Visible) ?? row.Cells[0];
                        _savedListId = null; // clear snapshot after restore
                        return;
                    }
                }
            }
            catch { _savedListId = null; }
        }

        // Restore selected component row (if possible)
        private void RestoreSelectedComponentRow()
        {
            if (_dataGridView2 == null) return;
            int? selRid = null;
            if (_savedComponentId != null)
            {
                selRid = _savedComponentId;
            }
            else if (_selectedAlternativeRow != null && _selectedAlternativeRow.Table.Columns.Contains("r_id") && _selectedAlternativeRow["r_id"] != DBNull.Value)
            {
                try { selRid = Convert.ToInt32(_selectedAlternativeRow["r_id"]); } catch { selRid = null; }
            }
            if (selRid == null) return;

            try
            {
                _dataGridView2.ClearSelection();
                foreach (DataGridViewRow row in _dataGridView2.Rows)
                {
                    if (row.Cells["r_id"].Value != null && Convert.ToInt32(row.Cells["r_id"].Value) == selRid.Value)
                    {
                        row.Selected = true;
                        _dataGridView2.CurrentCell = row.Cells.Cast<DataGridViewCell>().FirstOrDefault(c => c.Visible) ?? row.Cells[0];
                        _savedComponentId = null; // clear snapshot after restore
                        return;
                    }
                }
            }
            catch { _savedComponentId = null; }
        }

        public void BindAlternativesGrid(object? sender, EventArgs e)
        {
            if (_dataGridView2 == null || _dataGridView2.SelectedRows.Count == 0) return;

            var selectedRow = _dataGridView2.SelectedRows[0];
            if (selectedRow.Cells["r_id"].Value != null && _dataTable1 != null)
            {
                // Sprawdzenie czy są zamienniki dla wybranego komponentu
                int rId = Convert.ToInt32(selectedRow.Cells["r_id"].Value);
                var componentRows = _dataTable1.Select($"r_id = {rId} AND is_alternative = true");
                if( componentRows.Length == 0)
                {
                    _labelTopRight!.Text = "Zamienniki: (brak)";
                }
                else
                {
                    _labelTopRight!.Text = $"Zamienniki: ({componentRows.Length})";
                }
                if (_dataGridView3 != null)
            {
                _dataGridView3.DataSource = componentRows.Length > 0 ? componentRows.CopyToDataTable() : null;
                // Przywrócenie wcześniej zaznaczonego wiersza (jeśli istnieje)
                if (_selectedReplacementRow != null && _selectedReplacementRow.Table.Columns.Contains("alternative_id") && _selectedReplacementRow["alternative_id"] != DBNull.Value)
                {
                    try
                    {
                        int selAltId = Convert.ToInt32(_selectedReplacementRow["alternative_id"]);
                        foreach (DataGridViewRow rr in _dataGridView3.Rows)
                        {
                            if (rr.Cells["alternative_id"].Value != null && Convert.ToInt32(rr.Cells["alternative_id"].Value) == selAltId)
                            {
                                rr.Selected = true;
                                _dataGridView3.CurrentCell = rr.Cells.Cast<DataGridViewCell>().FirstOrDefault(c => c.Visible) ?? rr.Cells[0];
                                break;
                            }
                        }
                    }
                    catch { }
                }
            }
            }
            
            // Formatowanie kolumn w gridzie zamienników
            if(_dataGridView3 != null)
            {
                if( _dataGridView3.Columns["components_name"] != null)
                {
                    _dataGridView3.Columns["components_name"].Visible = false;
                }

                if( _dataGridView3.Columns["r_id"] != null)
                {
                    _dataGridView3.Columns["r_id"].Visible = false;
                }

                if( _dataGridView3.Columns["is_alternative"] != null)
                {
                    _dataGridView3.Columns["is_alternative"].Visible = false;
                }
                if( _dataGridView3.Columns["alternative_id"] != null)
                {
                    _dataGridView3.Columns["alternative_id"].Visible = false;
                }
                if( _dataGridView3.Columns["id"] != null)
                {
                    _dataGridView3.Columns["id"].Visible = false;
                }
                if ( _dataGridView3.Columns["using_quantity"] != null)
                {
                    _dataGridView3.Columns["using_quantity"].Visible = false;
                }

                if( _dataGridView3.Columns["alternative_name"] != null)
                {
                    _dataGridView3.Columns["alternative_name"].HeaderText = "Nazwa";
                    _dataGridView3.Columns["alternative_name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
                if( _dataGridView3.Columns["alternative_r_id"] != null)
                {
                    _dataGridView3.Columns["alternative_r_id"].HeaderText = "ID";
                    _dataGridView3.Columns["alternative_r_id"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                }
                if( _dataGridView3.Columns["quantity"] != null)
                {
                    _dataGridView3.Columns["quantity"].HeaderText = "Ilość";
                    _dataGridView3.Columns["quantity"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                }

            }
        }

        private DataTable? _originalComponentTable;
        public void BindComponentGrid(DataTable dt)
        {
            if (_dataGridView2 == null) return;

            _dataTable1 = dt;
            _originalComponentTable = dt.Copy();

            // Snapshot current selected component id to avoid SelectionChanged during binding overwriting it
            if (_selectedAlternativeRow != null && _selectedAlternativeRow.Table.Columns.Contains("r_id") && _selectedAlternativeRow["r_id"] != DBNull.Value)
            {
                try { _savedComponentId = Convert.ToInt32(_selectedAlternativeRow["r_id"]); } catch { _savedComponentId = null; }
            }
            else _savedComponentId = null;

            var rows = dt.Select("is_alternative = false"); // tylko oryginalne komponenty
            DataTable res = rows.Length > 0 ? rows.CopyToDataTable() : dt.Clone();

            _dataGridView2.DataSource = res;
            // Restore previously selected component after binding completes
            if (_dataGridView2 != null)
            {
                DataGridViewBindingCompleteEventHandler? compHandler = null;
                compHandler = (s, e) =>
                {
                    _dataGridView2.DataBindingComplete -= compHandler;
                    try { _dataGridView2.BeginInvoke((MethodInvoker)(() => RestoreSelectedComponentRow())); } catch { }
                };
                _dataGridView2.DataBindingComplete += compHandler;
            }
            _dataGridView2.ShowCellToolTips = true;

            if (_searchBox2 != null)
            {
                _searchBox2.TextChanged -= SearchBox2_TextChanged;
                _searchBox2.TextChanged += SearchBox2_TextChanged;
            }

            if( res.Columns.Contains("components_name") && _dataGridView2.Columns["components_name"] != null)
            {
                _dataGridView2.Columns["components_name"].HeaderText = "Nazwa komponentu";
                _dataGridView2.Columns["components_name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }

            if( res.Columns.Contains("r_id") && _dataGridView2.Columns["r_id"] != null)
            {
                _dataGridView2.Columns["r_id"].HeaderText = "ID";
                _dataGridView2.Columns["r_id"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }

            if( res.Columns.Contains("quantity") && _dataGridView2.Columns["quantity"] != null)
            {
                _dataGridView2.Columns["quantity"].HeaderText = "Ilość";
                _dataGridView2.Columns["quantity"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }

            if( res.Columns.Contains("using_quantity") && _dataGridView2.Columns["using_quantity"] != null)
            {
                _dataGridView2.Columns["using_quantity"].HeaderText = "Używana ilość (ilość z normatywu - ilość zmienników)";
                _dataGridView2.Columns["using_quantity"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }

            if( res.Columns.Contains("is_alternative") && _dataGridView2.Columns["is_alternative"] != null)
            {
                _dataGridView2.Columns["is_alternative"].Visible = false;
            }

            if( res.Columns.Contains("alternative_id") && _dataGridView2.Columns["alternative_id"] != null)
            {
                _dataGridView2.Columns["alternative_id"].Visible = false;
            }

            if( res.Columns.Contains("alternative_name") && _dataGridView2.Columns["alternative_name"] != null)
            {
                _dataGridView2.Columns["alternative_name"].Visible = false;
            }

            if( res.Columns.Contains("alternative_r_id") && _dataGridView2.Columns["alternative_r_id"] != null)
            {
                _dataGridView2.Columns["alternative_r_id"].Visible = false;
            }

            if( res.Columns.Contains("id") && _dataGridView2.Columns["id"] != null)
            {
                _dataGridView2.Columns["id"].Visible = false;
            }
        }

        private void SearchBox1_TextChanged(object? sender, EventArgs e)
        {
            if (_originalListTable == null || _dataGridView1 == null || _searchBox1 == null) return;
            string filter = _searchBox1.Text.Trim();
            if (string.IsNullOrEmpty(filter))
            {
                // Snapshot current selected id to avoid SelectionChanged during binding overwriting it
                if (_selectedListRow != null && _selectedListRow.Table.Columns.Contains("id") && _selectedListRow["id"] != DBNull.Value)
                {
                    try { _savedListId = Convert.ToInt32(_selectedListRow["id"]); } catch { _savedListId = null; }
                }
                else _savedListId = null;

                _dataGridView1.DataSource = _originalListTable;
                // Przywrócenie zaznaczonego wiersza (jeśli istnieje) - delayed to ensure rows are ready
                if (_savedListId != null)
                {
                    _dataGridView1.BeginInvoke((MethodInvoker)(() => RestoreSelectedListRow()));
                }
                return;
            }
            var dt = _originalListTable.Clone();
            foreach (DataRow row in _originalListTable.Rows)
            {
                if (row.Table.Columns.Contains("name") && row["name"].ToString()!.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                    dt.ImportRow(row);
            }
            _dataGridView1.DataSource = dt;
        }

        private void SearchBox2_TextChanged(object? sender, EventArgs e)
        {
            if (_originalComponentTable == null || _dataGridView2 == null || _searchBox2 == null) return;
            string filter = _searchBox2.Text.Trim();
            if (string.IsNullOrEmpty(filter))
            {
                var rows = _originalComponentTable.Select("is_alternative = false");
                DataTable res = rows.Length > 0 ? rows.CopyToDataTable() : _originalComponentTable.Clone();
                // Snapshot current selected component id to avoid SelectionChanged during binding overwriting it
                if (_selectedAlternativeRow != null && _selectedAlternativeRow.Table.Columns.Contains("r_id") && _selectedAlternativeRow["r_id"] != DBNull.Value)
                {
                    try { _savedComponentId = Convert.ToInt32(_selectedAlternativeRow["r_id"]); } catch { _savedComponentId = null; }
                }
                else _savedComponentId = null;

                _dataGridView2.DataSource = res;
                // Przywrócenie zaznaczonego wiersza (jeśli istnieje) - delayed to ensure rows are ready
                if (_savedComponentId != null)
                {
                    _dataGridView2.BeginInvoke((MethodInvoker)(() => RestoreSelectedComponentRow()));
                }
                return;
            }
            var dt = _originalComponentTable.Clone();
            foreach (DataRow row in _originalComponentTable.Rows)
            {
                if (row.Table.Columns.Contains("components_name") && row["components_name"].ToString()!.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 && !(bool)row["is_alternative"])
                    dt.ImportRow(row);
            }
            _dataGridView2.DataSource = dt;
        }

        private void ButtonAddAlternative_Click(object? sender, EventArgs e)
        {
            if (_dataGridView2 == null || _dataGridView2.SelectedRows.Count == 0)
            {
                ShowError("Błąd", "Proszę wybrać komponent, dla którego chcesz dodać alternatywę.");
                return;
            }

            var selectedRow = _dataGridView2.SelectedRows[0];
            if (selectedRow.Cells["r_id"].Value == null)
            {
                ShowError("Błąd", "Nie można odczytać ID wybranego komponentu.");
                return;
            }

            int originalRId = Convert.ToInt32(selectedRow.Cells["r_id"].Value);

            try
            {
                // Pobranie ID rezerwacji oryginalnego komponentu
                int reservationId = selectedRow.Cells["id"].Value != null ? Convert.ToInt32(selectedRow.Cells["id"].Value) : 0;
                int originalQty = selectedRow.Cells["using_quantity"].Value != null && selectedRow.Cells["using_quantity"].Value != DBNull.Value ? Convert.ToInt32(selectedRow.Cells["using_quantity"].Value) : selectedRow.Cells["quantity"].Value != null && selectedRow.Cells["quantity"].Value != DBNull.Value ? Convert.ToInt32(selectedRow.Cells["quantity"].Value) : 0;

                // Przygotowanie funkcji zwrotnej do zapisania alternatywy
                Func<int, int, int, int, Task<Models.DatabaseResult<bool>>>? onSave = null;
                if (_presenter != null)
                {
                    // Jeśli prezenter jest dostępny, użyj go do obsługi zapisu alternatywy
                    onSave = async (r, a, b, q) => // reservation_id, originalRId, substituteRId, quantity
                    {
                        // Dodanie alternatywy
                        var addResult = await _presenter.AddAlternativeAsync(this, new AddAlternativeEventArgs(r, a, b, q));

                        // Jeżeli dodanie się powiodło, zaktualizuj ilość oryginału
                        if (addResult.IsSuccess)
                        {
                            EditAlternativeRequested?.Invoke(this, new EditAlternativeEventArgs(reservationId, originalQty - q));
                        }

                        return addResult;
                    };
                }
                else
                {
                    // Jeśli prezenter nie jest dostępny
                    onSave = (r, a, b, q) =>
                    {
                        AddAlternativeRequested?.Invoke(this, new AddAlternativeEventArgs(r, a, b, q));
                        EditAlternativeRequested?.Invoke(this, new EditAlternativeEventArgs(reservationId, originalQty - q));
                        return Task.FromResult(Models.DatabaseResult<bool>.Success(true));
                    };
                }

                // Ustawienie początkowej ilości w oknie dodawania alternatywy
                int initialQty = 1;
                if (selectedRow.Cells["quantity"].Value != null && int.TryParse(selectedRow.Cells["quantity"].Value.ToString(), out var parsedQty))
                {
                    initialQty = parsedQty;
                }

                // Otwarcie okna dodawania alternatywy
                using (var dlg = new AlternativeAddForm(_enterpriseDatabase, originalRId, lockOriginal: true, showQuantity: true, initialQuantity: initialQty, onSave: onSave, reservationId: reservationId))
                {
                    var dr = dlg.ShowDialog();
                    
                    // Odświeżenie listy po dodaniu alternatywy
                    if (_dataGridView1 != null && _dataGridView1.SelectedRows.Count > 0)
                    {
                        var listId = _dataGridView1.SelectedRows[0].Cells["id"].Value != null ? Convert.ToInt32(_dataGridView1.SelectedRows[0].Cells["id"].Value) : 0;
                        ListSelected?.Invoke(this, new ListsSelectedEventArgs(listId));
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Błąd", $"Błąd podczas dodawania alternatywy: {ex.Message}");
            }
        }

        private void ButtonRemoveAlternative_Click(object? sender, EventArgs e)
        {
            if (_dataGridView2 == null || _dataGridView2.SelectedRows.Count == 0 ||
                _dataGridView3 == null || _dataGridView3.SelectedRows.Count == 0) // brak zaznaczenia zamiennika
            {
                ShowError("Błąd", "Proszę wybrać alternatywę do usunięcia.");
                return;
            }
            var confirmResult = MessageBox.Show("Czy na pewno chcesz usunąć wybrany zamiennik?", 
                                     "Potwierdzenie usunięcia zamiennika", 
                                     MessageBoxButtons.YesNo, 
                                     MessageBoxIcon.Warning);
            if (confirmResult == DialogResult.Yes)
            {
                var id = _dataGridView3.SelectedRows[0].Cells["alternative_id"].Value != null ? 
                    Convert.ToInt32(_dataGridView3.SelectedRows[0].Cells["alternative_id"].Value) : 0;

                var reservationId = _dataGridView2.SelectedRows[0].Cells["id"].Value != null ? 
                    Convert.ToInt32(_dataGridView2.SelectedRows[0].Cells["id"].Value) : 0;

                EditAlternativeRequested?.Invoke(this, 
                    new EditAlternativeEventArgs(reservationId,
                        _dataGridView2.SelectedRows[0].Cells["using_quantity"].Value != null ? 
                        Convert.ToInt32(_dataGridView2.SelectedRows[0].Cells["using_quantity"].Value) + 
                        (_dataGridView3.SelectedRows[0].Cells["quantity"].Value != null ? Convert.ToInt32(_dataGridView3.SelectedRows[0].Cells["quantity"].Value) : 0) : 0));

                RemoveAlternativeRequested?.Invoke(this, new RemoveAlternativeEventArgs(id, reservationId));
            }
        }
    }
}