using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace WILK.Views
{
    public partial class AdditionalMaterialView : Form
    {
        public Dictionary<int, DataTable>? outputDataTables = null;
        private Panel _elementsPanel = null!;
        private Button _buttonSave = null!;
        private Button _buttonCancel = null!;

        private Dictionary<int, (Panel container, Panel headerPanel, Label nameLabel, Label progressLabel, Label totalLabel, Button toggleButton, DataGridView grid)> _elementControls = new Dictionary<int, (Panel, Panel, Label, Label, Label, Button, DataGridView)>();

        public event EventHandler? SaveClicked;
        public event EventHandler? CancelClicked;

        public AdditionalMaterialView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Dodatkowy materiał";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            _elementsPanel = new Panel
            {
                AutoSize = false,
                Location = new Point(10, 48),
                Size = new Size(this.ClientSize.Width - 20, this.ClientSize.Height - 100),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AutoScroll = true,
                BackColor = Color.LightGray,
                Padding = new Padding(0, 0, 0, 0) 
            };

            _buttonSave = new Button
            {
                Text = "Zapisz",
                Size = new Size(120, 36),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };

            _buttonCancel = new Button
            {
                Text = "Anuluj",
                Size = new Size(120, 36),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };

            _buttonSave.Location = new Point(this.ClientSize.Width - _buttonSave.Width - 10, this.ClientSize.Height - _buttonSave.Height - 10);
            _buttonCancel.Location = new Point(10, this.ClientSize.Height - _buttonCancel.Height - 10);

            this.Resize += (s, e) =>
            {
                _elementsPanel.Size = new Size(this.ClientSize.Width - 20, Math.Max(100, this.ClientSize.Height - 100));

                _buttonSave.Location = new Point(this.ClientSize.Width - _buttonSave.Width - 10, this.ClientSize.Height - _buttonSave.Height - 10);
                _buttonCancel.Location = new Point(10, this.ClientSize.Height - _buttonCancel.Height - 10);
            };

            _buttonSave.Click += (s, e) =>
            {
                // przy zapisie kończymy edycję wszystkich komorek
                EndEditAllGrids();
                var data = GetUninitializedRows();
                if (data == null) return;
                outputDataTables = data;
                SaveClicked?.Invoke(this, EventArgs.Empty);
            };
            _buttonCancel.Click += (s, e) => { CancelClicked?.Invoke(this, EventArgs.Empty); this.Close(); };

            this.Controls.Add(_elementsPanel);
            this.Controls.Add(_buttonSave);
            this.Controls.Add(_buttonCancel);
            Icon = new Icon("Resources/Icons/wolf_256x256.ico");
        }

        public bool AddElement(int elementId, string name, int current = 0, int total = 0)
        {
            if (_elementControls.ContainsKey(elementId))
                return false;

            var container = new Panel
            {
                BorderStyle = BorderStyle.FixedSingle,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowOnly,
                Margin = new Padding(0, 3, 3, 0), 
                Dock = DockStyle.Top
            };

            var nameLabel = new Label
            {
                Text = name,
                AutoSize = false,
                Location = new Point(2, 4),
                Size = new Size(200, 24),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var progressLabel = new Label
            {
                Text = "Wydane: 0",
                AutoSize = false,
                Size = new Size(140, 24),
                TextAlign = ContentAlignment.MiddleRight,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                ForeColor = Color.DarkBlue,
                Font = new Font(this.Font.FontFamily, 9, FontStyle.Bold),
                Visible = true
            };

            var totalLabel = new Label
            {
                Text = total > 0 ? $"Do wydania: {total}" : "Do wydania: 0",
                AutoSize = false,
                Size = new Size(110, 24),
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = Color.DarkGreen,
                Visible = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Font = new Font(this.Font.FontFamily, 9, FontStyle.Bold)
            };
            totalLabel.Tag = total;

            var toggleButton = new Button
            {
                Text = "▼",
                Size = new Size(36, 24),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.LightCoral,
                FlatStyle = FlatStyle.Standard,
                Visible = true
            };

            container.Controls.Add(nameLabel);
            container.Controls.Add(progressLabel);
            container.Controls.Add(toggleButton);
            container.Controls.Add(totalLabel);

            var grid = new DataGridView
            {
                Height = 0,
                Margin = new Padding(6, 0, 6, 3),
                AllowUserToResizeColumns = false,
                AllowUserToResizeRows = false,
                AllowUserToOrderColumns = false,
                AllowUserToAddRows = true,
                AllowUserToDeleteRows = true,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                EditMode = DataGridViewEditMode.EditOnEnter,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                Visible = false
            };

            // ensure we adjust layout when rows are added/removed by user or code
            grid.RowsAdded -= Grid_RowsAdded;
            grid.RowsAdded += Grid_RowsAdded;
            grid.UserAddedRow -= Grid_UserAddedRow;
            grid.UserAddedRow += Grid_UserAddedRow;
            grid.RowsRemoved -= Grid_RowsRemoved;
            grid.RowsRemoved += Grid_RowsRemoved;

            toggleButton.Click += (s, e) =>
            {
                ToggleElement(elementId);
            };

            int initialPanelWidth = _elementsPanel.ClientSize.Width > 0 ? _elementsPanel.ClientSize.Width - 30 : Math.Max(100, this.ClientSize.Width - 50);
            container.Width = Math.Max(100, initialPanelWidth);
            container.Height = 32;
            container.Padding = new Padding(4);
            container.BackColor = SystemColors.ControlLight;
            container.BorderStyle = BorderStyle.FixedSingle;
            nameLabel.Size = new Size(Math.Max(50, container.Width - 190), 24);
            nameLabel.ForeColor = Color.Black;
            nameLabel.Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold);
            nameLabel.Text = $"{name} [{elementId}]";
            progressLabel.Location = new Point(Math.Max(100, container.Width - 170), 6);
            progressLabel.Font = new Font(this.Font.FontFamily, 9, FontStyle.Bold);
            toggleButton.Location = new Point(Math.Max(34, container.Width - 44), 6);

            grid.Margin = new Padding(6, 0, 6, 3);
            grid.Visible = false;
            grid.BorderStyle = BorderStyle.FixedSingle;

            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 32,
                Padding = new Padding(4)
            };

            if (container.Controls.Contains(nameLabel)) container.Controls.Remove(nameLabel);
            if (container.Controls.Contains(progressLabel)) container.Controls.Remove(progressLabel);
            if (container.Controls.Contains(toggleButton)) container.Controls.Remove(toggleButton);

            headerPanel.Controls.Add(nameLabel);
            headerPanel.Controls.Add(progressLabel);
            headerPanel.Controls.Add(totalLabel);
            headerPanel.Controls.Add(toggleButton);
            try
            {
                headerPanel.Controls.SetChildIndex(toggleButton, 0);
                headerPanel.Controls.SetChildIndex(totalLabel, 1);
                headerPanel.Controls.SetChildIndex(progressLabel, 2);
                headerPanel.Controls.SetChildIndex(nameLabel, 3);
            }
            catch { }

            headerPanel.SizeChanged += (s, e) =>
            {
                try
                {
                    int padding = 6;
                    int h = headerPanel.ClientSize.Height;
                    int w = headerPanel.ClientSize.Width;

                    int reservedForToggle = toggleButton.Width + padding + 4;
                    int reservedForLabels = progressLabel.Width + totalLabel.Width + 16;
                    int availableForName = Math.Max(50, w - reservedForToggle - reservedForLabels - nameLabel.Left - 8);
                    nameLabel.Size = new Size(availableForName, 24);

                    int labelsY = nameLabel.Top;
                    int labelsX = nameLabel.Right + 8;
                    progressLabel.Location = new Point(labelsX, labelsY);
                    totalLabel.Location = new Point(progressLabel.Right + 8, labelsY);

                    int toggleX = Math.Max(w - toggleButton.Width - padding, totalLabel.Right + 8);
                    toggleButton.Location = new Point(toggleX, Math.Max(0, (h - toggleButton.Height) / 2));

                    try { toggleButton.BringToFront(); totalLabel.BringToFront(); progressLabel.BringToFront(); } catch { }
                }
                catch { }
            };

            try { headerPanel.PerformLayout(); headerPanel.Invalidate(); headerPanel.Update(); } catch { }
            progressLabel.Visible = true;
            totalLabel.Visible = true;

            container.Controls.Add(headerPanel);

            grid.Location = new Point(4, headerPanel.Bottom + 2);
            grid.Width = Math.Max(100, container.ClientSize.Width - 8);
            grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            container.Controls.Add(grid);
            headerPanel.BringToFront();

            _elementControls[elementId] = (container, headerPanel, nameLabel, progressLabel, totalLabel, toggleButton, grid);

            _elementsPanel.Controls.Add(container);

            container.Visible = true;

            container.BringToFront();
            _elementsPanel.ScrollControlIntoView(container);
            _elementsPanel.PerformLayout();
            _elementsPanel.Refresh();
            this.Refresh();
            Application.DoEvents();

            return true;
        }
        public bool SetElementReels(int elementId, DataTable reelsTable)
        {
            if (!_elementControls.TryGetValue(elementId, out var tuple))
                return false;

            var grid = tuple.grid;
            var progressLabel = tuple.progressLabel;
            var toggleButton = tuple.toggleButton;

            if (reelsTable != null)
            {
                if (!reelsTable.Columns.Contains("__initialized"))
                    reelsTable.Columns.Add(new DataColumn("__initialized", typeof(bool)) { DefaultValue = false });

                // oznakuj istniejące wiersze jako zainicjalizowane
                try
                {
                    foreach (DataRow dr in reelsTable.Rows)
                    {
                        dr["__initialized"] = true;
                    }
                }
                catch { }

            }

            grid.DataSource = reelsTable;

            try // oblicz sumę wydanych
            {
                long sum = 0;
                if (reelsTable != null)
                {
                    foreach (DataRow rr in reelsTable.Rows)
                    {
                        var qobj = rr["quantity"];
                        if (qobj == null || qobj == DBNull.Value) continue;
                        if (long.TryParse(qobj.ToString(), out long qv)) sum += qv;
                    }
                }
                    try { tuple.progressLabel!.Text = $"Wydane: {sum}"; tuple.progressLabel!.Tag = sum; } catch { }
                    try
                    {
                        int reservationTotal = 0;
                        try { reservationTotal = Convert.ToInt32(tuple.totalLabel?.Tag ?? tuple.progressLabel?.Tag ?? 0); } catch { reservationTotal = 0; }
                        tuple.totalLabel!.Tag = reservationTotal;
                        tuple.totalLabel!.Text = reservationTotal > 0 ? $"Do wydania: {reservationTotal}" : "Do wydania: 0";
                    }
                    catch { }

                try
                {
                    var header = tuple.headerPanel;
                    int topY = header != null ? header.Bottom : tuple.nameLabel.Bottom;
                    grid.Location = new Point(4, topY + 4);
                    grid.Width = Math.Max(100, tuple.container.ClientSize.Width - 8);
                }
                catch { }
            }
            catch { }

            grid.Visible = false;
            toggleButton.Text = "▼";

            try
            {
                int rowHeight = grid.RowTemplate?.Height > 0 ? grid.RowTemplate.Height : 22;
                int rows = reelsTable?.Rows?.Count ?? 0;
                int targetHeight = Math.Min(300, Math.Max(80, rows * (rowHeight + 2) + 30));
                grid.Height = targetHeight;

                var header = tuple.headerPanel;
                int topPosition = header != null ? header.Bottom : tuple.nameLabel.Bottom;
                grid.Location = new Point(4, topPosition + 4);
                grid.Width = Math.Max(100, tuple.container.ClientSize.Width - 8);
            }
            catch { }

            grid.DataBindingComplete -= Grid_DataBindingComplete;
            grid.DataBindingComplete += Grid_DataBindingComplete;

            grid.CellValidating -= Grid_CellValidating;
            grid.CellValidating += Grid_CellValidating;
            grid.CellEndEdit -= Grid_CellEndEdit;
            grid.CellEndEdit += Grid_CellEndEdit;
            grid.CellValueChanged -= Grid_CellValueChanged;
            grid.CellValueChanged += Grid_CellValueChanged;
            grid.RowsRemoved -= Grid_RowsRemoved;
            grid.RowsRemoved += Grid_RowsRemoved;
            grid.RowsAdded -= Grid_RowsAdded;
            grid.RowsAdded += Grid_RowsAdded;
            grid.UserAddedRow -= Grid_UserAddedRow;
            grid.UserAddedRow += Grid_UserAddedRow; 

            _elementsPanel.PerformLayout();
            _elementsPanel.Refresh();

            return true;
        }

        private void Grid_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (sender is DataGridView g)
            {
                try
                {
                    g.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);

                    // zablokuj edycję wierszy oznaczonych jako zainicjalizowane
                    foreach (DataGridViewRow r in g.Rows)
                    {
                        bool isUserAdded = false;
                        if (r.DataBoundItem is DataRowView drv && drv.Row.Table.Columns.Contains("__initialized"))
                        {
                            try { isUserAdded = Convert.ToBoolean(drv["__initialized"]); } catch { isUserAdded = false; }
                        }
                        if (isUserAdded) // oznaczone jako zainicjalizowane
                        {
                            g.Rows[r.Index].ReadOnly = true;
                            g.Rows[r.Index].DefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
                        }else // nieoznaczone jako zainicjalizowane
                        {
                            g.Rows[r.Index].ReadOnly = false;
                        }
                    }
                }
                catch { }

                if (g.Columns.Contains("__initialized"))
                {
                    g.Columns["__initialized"].Visible = false;   
                }

                if (g.Columns.Contains("reel") && g.Columns["reel"] != null)
                {
                    g.Columns["reel"].HeaderText = "ID rolki";
                    g.Columns["reel"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }

                if (g.Columns.Contains("quantity") && g.Columns["quantity"] != null)
                {
                    g.Columns["quantity"].HeaderText = "Ilość";
                    g.Columns["quantity"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                }

                if (g.Columns.Contains("box") && g.Columns["box"] != null)
                {
                    g.Columns["box"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    g.Columns["box"].HeaderText = "Box";
                }
            }
        }

        private void EndEditAllGrids()
        {
            foreach (var kv in _elementControls)
            {
                var grid = kv.Value.grid;
                try
                {
                    grid.EndEdit();
                    grid.CommitEdit(DataGridViewDataErrorContexts.Commit);

                    if (grid.DataSource is DataTable dt)
                    {
                        try
                        {
                            var bm = this.BindingContext?[dt];
                            bm?.EndCurrentEdit();
                        }
                        catch { }
                    }
                }
                catch { }
            }
        }
        public Dictionary<int, DataTable>? GetUninitializedRows()
        {
            var result = new Dictionary<int, DataTable>();
            foreach (var kv in _elementControls)
            {
                int elementId = kv.Key;
                var tuple = kv.Value;
                var grid = tuple.grid;
                var dt = grid.DataSource as DataTable;
                if (dt == null) continue;

                try { grid.EndEdit(); grid.CommitEdit(DataGridViewDataErrorContexts.Commit); var bm = this.BindingContext?[dt]; bm?.EndCurrentEdit(); } catch { }

                var rows = dt.AsEnumerable().Where(r => !r.Table.Columns.Contains("__initialized") || !Convert.ToBoolean(r["__initialized"]))
                                            .ToList();
                if (rows.Count == 0) continue;

                var rowsToCopy = new List<DataRow>();
                foreach (var r in rows)
                {
                    bool pass = true;
                    if (dt.Columns.Contains("reel"))
                    {
                        try
                        {
                            var rv = r["reel"]?.ToString() ?? string.Empty;
                            var m = Regex.Match(rv, "\\d+");
                            if (!m.Success || m.Value.Length != 14) pass = false;
                            else pass = string.Equals(m.Value.Substring(0, 5), elementId.ToString(), StringComparison.Ordinal);
                        }
                        catch { pass = false; }
                    }

                    if (!pass)
                    {
                        MessageBox.Show(this, $"Rolka '{r["reel"]}' nie jest poprawna.", "Błąd walidacji", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        try // ustaw fokus na błędnej rolce
                        {
                            var gridToFocus = tuple.grid;
                            if (!gridToFocus.Visible) ToggleElement(elementId);
                            int colIndex = -1;
                            for (int ci = 0; ci < gridToFocus.Columns.Count; ci++)
                            {
                                var name = (gridToFocus.Columns[ci].DataPropertyName ?? gridToFocus.Columns[ci].Name) ?? string.Empty;
                                if (string.Equals(name, "reel", StringComparison.OrdinalIgnoreCase)) { colIndex = ci; break; }
                            }
                            if (colIndex == -1 && gridToFocus.Columns.Count > 0) colIndex = 0;
                            string reelVal = r["reel"]?.ToString() ?? string.Empty;
                            int foundRow = -1;
                            for (int ri = 0; ri < gridToFocus.Rows.Count; ri++)
                            {
                                var cellVal = gridToFocus.Rows[ri].Cells[colIndex].Value?.ToString() ?? string.Empty;
                                if (string.Equals(cellVal, reelVal, StringComparison.Ordinal)) { foundRow = ri; break; }
                            }
                            gridToFocus.ClearSelection();
                            if (foundRow >= 0)
                            {
                                var row = gridToFocus.Rows[foundRow];
                                row.Selected = true;
                                gridToFocus.CurrentCell = row.Cells[colIndex];
                                try { gridToFocus.FirstDisplayedScrollingRowIndex = Math.Max(0, foundRow - 2); } catch { }
                                gridToFocus.Focus();
                                gridToFocus.BeginEdit(true);
                            }
                            else { gridToFocus.Focus(); }
                        }
                        catch { }

                        return null;
                    }

                    rowsToCopy.Add(r);
                }

                // validuj sumę ilosci - nie przekracza rezerwacji
                if (dt.Columns.Contains("quantity"))
                {
                    try
                    {
                        long sum = 0;
                        foreach (DataRow rr in dt.Rows) // podlicz sume ilosci
                        {
                            var qobj = rr["quantity"];
                            if (qobj == null || qobj == DBNull.Value)
                            {
                                MessageBox.Show(this, $"Ilość w rolce '{rr["reel"]}' musi być liczbą całkowitą.", "Błąd walidacji", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                try { var gridToFocus = tuple.grid; if (!gridToFocus.Visible) ToggleElement(elementId); gridToFocus.Focus(); } catch { }
                                return null;
                            }
                            if (!long.TryParse(qobj.ToString(), out long qv))
                            {
                                MessageBox.Show(this, $"Ilość w rolce '{rr["reel"]}' musi być liczbą całkowitą.", "Błąd walidacji", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                try { var gridToFocus = tuple.grid; if (!gridToFocus.Visible) ToggleElement(elementId); gridToFocus.Focus(); } catch { }
                                return null;
                            }
                            sum += qv;
                        }

                        int reservationTotal = 0;
                        try
                        {
                            var txt = tuple.totalLabel?.Text ?? string.Empty;
                            if (txt.Contains("Do wydania:"))
                            {
                                var parts = txt.Split(':');
                                if (parts.Length > 1) reservationTotal = Convert.ToInt32(parts[1].Trim());
                            }
                            else if (txt.Contains('/'))
                            {
                                reservationTotal = Convert.ToInt32(txt.Split('/').Last());
                            }
                            else if (!string.IsNullOrWhiteSpace(txt))
                            {
                                reservationTotal = Convert.ToInt32(txt);
                            }
                        }
                        catch { reservationTotal = 0; }

                        if (sum > reservationTotal)
                        {
                            MessageBox.Show(this, $"Suma ilości ({sum}) przekracza ilość rezerwacji ({reservationTotal}) dla elementu {elementId}.", "Błąd walidacji", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            try
                            {
                                var gridToFocus = tuple.grid;
                                if (!gridToFocus.Visible) ToggleElement(elementId);
                                gridToFocus.ClearSelection();
                                if (rowsToCopy.Count > 0) // ustaw fokus na pierwszym wierszu do skopiowania
                                {
                                    int qcol = -1;
                                    for (int ci = 0; ci < gridToFocus.Columns.Count; ci++)
                                    {
                                        var name = (gridToFocus.Columns[ci].DataPropertyName ?? gridToFocus.Columns[ci].Name) ?? string.Empty;
                                        if (string.Equals(name, "quantity", StringComparison.OrdinalIgnoreCase)) { qcol = ci; break; }
                                    }
                                    if (qcol == -1 && gridToFocus.Columns.Count > 0) qcol = 0;
                                    string firstReel = rowsToCopy[0]["reel"]?.ToString() ?? string.Empty;
                                    int foundRow = -1;
                                    for (int ri = 0; ri < gridToFocus.Rows.Count; ri++)
                                    {
                                        var cellVal = gridToFocus.Rows[ri].Cells[0].Value?.ToString() ?? string.Empty;
                                        if (string.Equals(cellVal, firstReel, StringComparison.Ordinal)) { foundRow = ri; break; }
                                    }
                                    if (foundRow >= 0)
                                    {
                                        var row = gridToFocus.Rows[foundRow];
                                        row.Selected = true;
                                        gridToFocus.CurrentCell = row.Cells[qcol];
                                        try { gridToFocus.FirstDisplayedScrollingRowIndex = Math.Max(0, foundRow - 2); } catch { }
                                        gridToFocus.Focus();
                                        gridToFocus.BeginEdit(true);
                                    }
                                    else { gridToFocus.Focus(); }
                                }
                                else { gridToFocus.Focus(); }
                            }
                            catch { }

                            return null;
                        }
                    }
                    catch { }
                }

                // skopiuj wiersze do nowej tabeli
                var copy = dt.Clone();
                foreach (var r in rowsToCopy)
                {
                    var nr = copy.NewRow();
                    foreach (DataColumn c in dt.Columns) nr[c.ColumnName] = r[c.ColumnName];
                    copy.Rows.Add(nr);
                    try { r["__initialized"] = true; } catch { }
                }

                result[elementId] = copy;
            }
            return result;
        }

        private void Grid_CellValidating(object? sender, DataGridViewCellValidatingEventArgs e)
        {
            try
            {
                var g = sender as DataGridView;
                if (g == null) return;
                if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
                var col = g.Columns[e.ColumnIndex];
                var prop = !string.IsNullOrEmpty(col.DataPropertyName) ? col.DataPropertyName : col.Name;

                if (string.Equals(prop, "box", StringComparison.OrdinalIgnoreCase))
                {
                    if (e.FormattedValue != null)
                    {
                        var newBox = e.FormattedValue.ToString() ?? string.Empty;
                        if (newBox.Length > 10)
                        {
                            MessageBox.Show(this, "Nazwa boxa nie może przekraczać 10 znaków.", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            var cell = g.Rows[e.RowIndex].Cells[e.ColumnIndex];
                            cell.Value = cell.Tag == null ? DBNull.Value : cell.Tag;

                            try
                            {
                                g.ClearSelection();
                                var row = g.Rows[e.RowIndex];
                                row.Selected = true;
                                g.CurrentCell = row.Cells[e.ColumnIndex];
                                try { g.FirstDisplayedScrollingRowIndex = Math.Max(0, e.RowIndex - 2); } catch { }
                                g.Focus();
                                g.BeginEdit(true);
                            }
                            catch { }

                            e.Cancel = true;
                        }
                    }
                }

                if (!string.Equals(prop, "quantity", StringComparison.OrdinalIgnoreCase))
                    return;

                var newText = e.FormattedValue?.ToString() ?? string.Empty;
                if (!int.TryParse(newText, out _) && newText != string.Empty) // sprawdz czy quantity jest liczbą całkowitą
                {
                    MessageBox.Show(this, "Wartość ilości musi być liczbą całkowitą.", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    var cell = g.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    cell.Value = cell.Tag == null ? DBNull.Value : cell.Tag;

                    try
                    {
                        g.ClearSelection();
                        var row = g.Rows[e.RowIndex];
                        row.Selected = true;
                        g.CurrentCell = row.Cells[e.ColumnIndex];
                        try { g.FirstDisplayedScrollingRowIndex = Math.Max(0, e.RowIndex - 2); } catch { }
                        g.Focus();
                        g.BeginEdit(true);
                    }
                    catch { }

                    e.Cancel = true;
                }

                if (int.TryParse(newText, out int newVal) && newVal <= 0) // sprawdz czy quantity jest wieksze od 0
                {
                    MessageBox.Show(this, "Wartość ilości musi być liczbą całkowitą.", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    var cell = g.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    cell.Value = cell.Tag == null ? DBNull.Value : cell.Tag; 

                    try
                    {
                        g.ClearSelection();
                        var row = g.Rows[e.RowIndex];
                        row.Selected = true;
                        g.CurrentCell = row.Cells[e.ColumnIndex];
                        try { g.FirstDisplayedScrollingRowIndex = Math.Max(0, e.RowIndex - 2); } catch { }
                        g.Focus();
                        g.BeginEdit(true);
                    }
                    catch { }

                    e.Cancel = true;
                }
            }
            catch { }
        }

        private void Grid_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            try
            {
                var g = sender as DataGridView;
                if (g == null) return;
                if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
                var cell = g.Rows[e.RowIndex].Cells[e.ColumnIndex];
                cell.Tag = cell.Value;
                RecalculateTotalForGrid(g);
            }
            catch { }
        }

        private void Grid_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            try
            {
                var g = sender as DataGridView;
                if (g == null) return;
                RecalculateTotalForGrid(g);
            }
            catch { }
        }

        private void Grid_RowsRemoved(object? sender, DataGridViewRowsRemovedEventArgs e)
        {
            try
            {
                var g = sender as DataGridView;
                if (g == null) return;
                RecalculateTotalForGrid(g);
                AdjustGridLayout(g);
            }
            catch { }
        }

        private void Grid_RowsAdded(object? sender, DataGridViewRowsAddedEventArgs e)
        {
            try
            {
                var g = sender as DataGridView;
                if (g == null) return;
                RecalculateTotalForGrid(g);
                AdjustGridLayout(g);
            }
            catch { }
        }

        private void Grid_UserAddedRow(object? sender, DataGridViewRowEventArgs e)
        {
            try
            {
                var g = sender as DataGridView;
                if (g == null) return;
                RecalculateTotalForGrid(g);
                AdjustGridLayout(g);
            }
            catch { }
        }

        private void AdjustGridLayout(DataGridView? grid)
        {
            try
            {
                if (grid == null) return;

                int rows = 0;
                try { rows = (grid.DataSource as DataTable)?.Rows?.Count ?? grid.Rows.Count; } catch { rows = grid.Rows.Count; }
                rows+=2; // account for the new-row placeholder
                int rowHeight = grid.RowTemplate?.Height > 0 ? grid.RowTemplate.Height : 22;
                int targetHeight = Math.Min(300, Math.Max(80, rows * (rowHeight + 2) + 30));
                grid.Height = targetHeight;

                // find container/header for this grid and adjust width/location
                foreach (var kv in _elementControls)
                {
                    if (kv.Value.grid == grid)
                    {
                        try
                        {
                            var header = kv.Value.headerPanel;
                            int topPosition = header != null ? header.Bottom : kv.Value.nameLabel.Bottom;
                            grid.Location = new Point(4, topPosition + 4);
                            grid.Width = Math.Max(100, kv.Value.container.ClientSize.Width - 8);
                            kv.Value.container.PerformLayout();
                        }
                        catch { }
                        break;
                    }
                }

                _elementsPanel.PerformLayout();
                _elementsPanel.Refresh();
            }
            catch { }
        }

        private void RecalculateTotalForGrid(DataGridView? grid)
        {
            try
            {
                if (grid == null) return;
                long sum = 0;
                if (grid.DataSource is DataTable dt)
                {
                    foreach (DataRow r in dt.Rows)
                    {
                        var qobj = r["quantity"];
                        if (qobj == null || qobj == DBNull.Value) continue;
                        if (long.TryParse(qobj.ToString(), out long qv)) sum += qv;
                    }
                }

                foreach (var kv in _elementControls)
                {
                    if (kv.Value.grid == grid)
                    {
                        try { kv.Value.progressLabel!.Text = $"Wydane: {sum}"; kv.Value.progressLabel!.Tag = sum; } catch { }
                        try
                        {
                            int reservationTotal = 0;
                            try { reservationTotal = Convert.ToInt32(kv.Value.totalLabel?.Tag ?? kv.Value.progressLabel?.Tag ?? 0); } catch { reservationTotal = 0; }
                            kv.Value.totalLabel!.Tag = reservationTotal;
                            kv.Value.totalLabel!.Text = reservationTotal > 0 ? $"Do wydania: {reservationTotal}" : "Do wydania: 0";
                        }
                        catch { }
                        break;
                    }
                }
            }
            catch { }
        }

        public void ToggleElement(int elementId)
        {
            if (!_elementControls.TryGetValue(elementId, out var tuple))
                return;

            var grid = tuple.grid;
            var btn = tuple.toggleButton;
            grid.Visible = !grid.Visible;
            btn.Text = grid.Visible ? "▲" : "▼";

            if (grid.Visible) // jeśli rozwinięty
            {
                // oblicz rozmiary
                var header = tuple.headerPanel;
                int topY = header != null ? header.Bottom : tuple.nameLabel.Bottom;

                grid.Location = new Point(6, topY + 4);
                grid.Width = Math.Max(100, tuple.container.ClientSize.Width - 18);

                int rows = 0;
                try { rows = (grid.DataSource as DataTable)?.Rows?.Count ?? grid.Rows.Count; } catch { rows = grid.Rows.Count; }
                rows++; // uwzględnij pusty wiersz do dodania
                int rowHeight = grid.RowTemplate?.Height > 0 ? grid.RowTemplate.Height : 22;
                int targetHeight = Math.Min(300, Math.Max(80, rows * (rowHeight + 2) + 30));
                grid.Height = targetHeight;

                RecalculateTotalForGrid(grid);

                // wymus odświeżenie layoutu
                tuple.container.PerformLayout();
                _elementsPanel.PerformLayout();
                _elementsPanel.Refresh();
            }
            else // ukryj
            {
                grid.Height = 0;
                grid.Visible = false;
                tuple.container.PerformLayout();
                _elementsPanel.PerformLayout();
                _elementsPanel.Refresh();
            }

        }

        public bool AreAllElementsComplete()
        {
            foreach (var kv in _elementControls)
            {
                var progressLabel = kv.Value.progressLabel;
                var totalLabel = kv.Value.totalLabel;
                
                try
                {
                    int current = Convert.ToInt32(progressLabel?.Tag ?? 0);
                    int total = Convert.ToInt32(totalLabel?.Tag ?? 0);
                    
                    if (current != total)
                        return false;
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _elementsPanel?.Dispose();
                foreach (var kv in _elementControls.Values)
                {
                    kv.grid?.Dispose();
                    kv.container?.Dispose();
                }
                _buttonSave?.Dispose();
                _buttonCancel?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
