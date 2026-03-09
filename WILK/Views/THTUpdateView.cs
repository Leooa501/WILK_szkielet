using System.Data;
using System.Linq;

using WILK.Services;

namespace WILK.Views
{
    public partial class THTUpdateView : Form
    {
        private Panel? _topPanel;
        private DataGridView? _dataGridView;
        private DataTable? _dataTable;
        private DataTable? _draftTable;
        private TextBox? _textBox;
        private Label? _label;
        private Button? _buttonSave;
        private Button? _buttonCheck;
        private Button? _buttonSaveDraft;
        private Panel? _bottomPanel;
        private Label? _statusLabel;
        private int _start;
        private int _done;
        private ToolTip? _toolTip;
        private readonly IEnterpriseDatabase _enterpriseDatabase;
        private readonly int _listId;
        private readonly Dictionary<int, Button> _floatingAddButtons = new();

        public THTUpdateView(int listId, IEnterpriseDatabase enterpriseDatabase)
        {
            _listId = listId;
            _enterpriseDatabase = enterpriseDatabase ?? throw new ArgumentNullException(nameof(enterpriseDatabase));

            this.AutoScaleMode = AutoScaleMode.Font;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(700, 420);

            InitializeCustomComponents();

            var progress = _enterpriseDatabase.GetReservationProgressTHTAsync(_listId).GetAwaiter().GetResult().Data;
            if (progress != null)
            {
                _done = progress.Value.done;
                _start = progress.Value.start;
            }
            else
            {
                _done = 0;
                _start = 0;
            }

            var (realizeFlag, assignedPerson) = _enterpriseDatabase.GetMetadata(_listId).GetAwaiter().GetResult().Data;
            if (realizeFlag)
                _statusLabel!.Text += " REALIZOWANA" + (string.IsNullOrWhiteSpace(assignedPerson) ? "" : $" przez {assignedPerson}");

            _textBox!.Text = _start.ToString();

            _dataTable = _enterpriseDatabase.LoadReservationTHTAsync(_listId).GetAwaiter().GetResult().Data;
            _draftTable = _enterpriseDatabase.GetDraft(_listId).GetAwaiter().GetResult().Data;

            var dt = new DataTable();
            dt.Columns.Add(new DataColumn { ColumnName = "reel_id", Caption = "ID szpuli", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn { ColumnName = "box", Caption = "Box", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn { ColumnName = "used_quantity", Caption = "Użyto", DataType = typeof(int) });

            // populate from draft if present, otherwise fall back to the reservation data; always assign DataSource so the grid initializes
            if (_draftTable != null && _draftTable.Rows.Count > 0)
            {
                foreach (DataRow dr in _draftTable.Rows)
                {
                    var newRow = dt.NewRow();
                    newRow["reel_id"] = dr["id_reel"];
                    newRow["box"] = dr["box"];
                    newRow["used_quantity"] = dr["quantity"];
                    dt.Rows.Add(newRow);
                }
            }

            _dataGridView!.DataSource = dt;
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Aktualizacja THT";
            this.Icon = new Icon("Resources/Icons/wolf_256x256.ico");

            _topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                Padding = new Padding(10, 6, 10, 6)
            };

            var topLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Margin = Padding.Empty
            };
            topLayout.ColumnStyles.Clear();
            topLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            topLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            topLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            _label = new Label
            {
                Text = "Ilość płytek:",
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 6, 8, 6)
            };

            _statusLabel = new Label
            {
                Text = "",
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 6, 8, 6),
                ForeColor = Color.Red
            };

            _textBox = new TextBox
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                Margin = new Padding(0, 6, 0, 6)
            };

            _toolTip = new ToolTip();
            _textBox.KeyPress += NumericTextBox_KeyPress;
            _textBox.TextChanged += ListSizeTextBox_TextChanged;

            topLayout.Controls.Add(_label, 0, 0);
            topLayout.Controls.Add(_textBox, 1, 0);
            topLayout.Controls.Add(_statusLabel, 2, 0);
            _topPanel.Controls.Add(topLayout);

            _dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(10, 6, 10, 10),
                AutoGenerateColumns = false,
                AllowUserToAddRows = true,
                AllowUserToDeleteRows = false,
                ReadOnly = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.CellSelect,
                MultiSelect = false
            };

            _dataGridView.Columns.Clear();
            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "reel_id", HeaderText = "ID szpuli", DataPropertyName = "reel_id", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "box", HeaderText = "Box", DataPropertyName = "box", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "used_quantity", HeaderText = "Użyto", DataPropertyName = "used_quantity", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });

            // spacer dlaprzycisków (+)
            if (!_dataGridView.Columns.Contains("spacer"))
            {
                var spacer = new DataGridViewTextBoxColumn
                {
                    Name = "spacer",
                    HeaderText = string.Empty,
                    ReadOnly = true,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    Width = 40,
                    DefaultCellStyle = new DataGridViewCellStyle { BackColor = SystemColors.Control, SelectionForeColor = SystemColors.GrayText }
                };
                _dataGridView.Columns.Add(spacer);
            }

            _dataGridView.CellContentClick -= DataGridView_CellContentClick;
            _dataGridView.CellContentClick += DataGridView_CellContentClick;

            _dataGridView.CellFormatting -= DataGridView_CellFormatting;
            _dataGridView.CellFormatting += DataGridView_CellFormatting;

            _dataGridView.Scroll -= DataGridView_Scroll;
            _dataGridView.Scroll += DataGridView_Scroll;

            _dataGridView.RowsAdded -= DataGridView_RowsChanged;
            _dataGridView.RowsAdded += DataGridView_RowsChanged;

            _dataGridView.RowsRemoved -= DataGridView_RowsChanged;
            _dataGridView.RowsRemoved += DataGridView_RowsChanged;

            _dataGridView.CellValueChanged -= DataGridView_CellValueChanged;
            _dataGridView.CellValueChanged += DataGridView_CellValueChanged;

            _dataGridView.CurrentCellDirtyStateChanged -= DataGridView_CurrentCellDirtyStateChanged;
            _dataGridView.CurrentCellDirtyStateChanged += DataGridView_CurrentCellDirtyStateChanged;

            _dataGridView.EditingControlShowing -= DataGridView_EditingControlShowing;
            _dataGridView.EditingControlShowing += DataGridView_EditingControlShowing;

            _bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                Padding = new Padding(10)
            };

            _buttonSave = new Button
            {
                Text = "Wydaj",
                AutoSize = true,
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                Enabled = true
            };

            _buttonCheck = new Button
            {
                Text = "Sprawdź",
                AutoSize = true,
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                Enabled = true
            };

            _buttonSaveDraft = new Button
            {
                Text = "Zapisz szkic",
                AutoSize = true,
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                Enabled = true
            };

            _buttonCheck.Click += ButtonCheck_Click;
            _buttonSaveDraft.Click += ButtonSaveDraft_Click;
            _buttonSave.Click += ButtonSave_Click;

            var fl = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = Padding.Empty
            };
            fl.Controls.Add(_buttonSaveDraft);
            fl.Controls.Add(_buttonSave);
            fl.Controls.Add(_buttonCheck);
            _bottomPanel.Controls.Add(fl);

            Controls.Add(_dataGridView);
            Controls.Add(_topPanel);
            Controls.Add(_bottomPanel);

            this.FormClosed += THTUpdateView_FormClosed;
        }


        private void ButtonCheck_Click(object? sender, EventArgs e)
        {
            if (_dataGridView == null || _buttonSave == null) return;
            try { _dataGridView.EndEdit(); } catch { }

            RecomputeDuplicateReelIds();

            // validuj wybierana ilość płytek
            if (!ValidateListSizeTextBox(out var listSize, out var listMsg))
            {
                if (_textBox != null)
                {
                    _textBox.Focus();
                    _textBox.SelectAll();
                    _textBox.BackColor = Color.LightCoral;
                    _toolTip?.SetToolTip(_textBox, listMsg ?? string.Empty);
                }
                MessageBox.Show(listMsg ?? "Nieprawidłowa wartość pola 'Ilość płytek'.", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var rows = new List<(string reelId, string box, int used)>();
            var errors = new List<(int rowIndex, string columnName, string message)>();

            for (int i = 0; i < _dataGridView.Rows.Count; i++)
            {
                var r = _dataGridView.Rows[i];
                if (r.IsNewRow) continue;

                var reelCell = r.Cells["reel_id"];
                var boxCell = r.Cells["box"];
                var usedCell = r.Cells["used_quantity"];

                var reel = SafeString(reelCell.Value).Trim();
                var box = SafeString(boxCell.Value).Trim();
                var usedText = SafeString(usedCell.Value).Trim();

                ClearValidationError(reelCell);
                ClearValidationError(usedCell);
                ClearValidationError(boxCell);

                if (string.IsNullOrWhiteSpace(reel)) // reel_id puste
                {
                    errors.Add((i, "reel_id", "Pole wymagane"));
                }
                else
                {
                    var digits = new string(reel.Where(char.IsDigit).ToArray());
                    if (PerCellValidationFails(reelCell, digits)) // reel_id nie przeszło walidacji
                        errors.Add((i, "reel_id", "Błędne ID"));
                    else if (!string.IsNullOrEmpty(reelCell.ToolTipText ?? string.Empty) && (reelCell.ToolTipText.Contains("(duplikat)") || reelCell.ToolTipText.Contains("duplikat")))
                        errors.Add((i, "reel_id", "Duplikat")); 
                }

                int usedParsed = 0;
                if (string.IsNullOrWhiteSpace(usedText)) // used_quantity puste
                {
                    errors.Add((i, "used_quantity", "Pole wymagane"));
                }
                else if (!int.TryParse(usedText, out usedParsed)) // used_quantity nie jest liczbą
                {
                    errors.Add((i, "used_quantity", "Nieprawidłowa liczba"));
                }


                if (string.IsNullOrWhiteSpace(box)) // box puste
                {
                    errors.Add((i, "box", "Pole wymagane"));
                }

                if (!errors.Any(x => x.rowIndex == i)) // jeśli nie ma błędów w tym wierszu, dodaj do listy zapisu
                    rows.Add((reel, box, usedParsed));
            }

            // sprawdź zgodność ilości w grupach (na podstawie prefiksów reel_id)
            var groupProblems = RecomputeGroupQuantityValidation();
            foreach (var gp in groupProblems)
            {
                if (gp.rowIndexes != null && gp.rowIndexes.Count > 0)
                {
                    foreach (var ri in gp.rowIndexes) // oznacz wiersze z niezgodną ilością
                        errors.Add((ri, "reel_id", $"Niezgodna ilość (oczekiwano: {gp.expected}, podano: {gp.actual})"));
                }
                else
                {
                    // brak pasujących wierszy dla oczekiwanego ID
                    errors.Add((-1, "reel_id", $"Brak wpisów dla prefiksu {gp.prefix} (oczekiwano: {gp.expected}, podano: {gp.actual})"));
                }
            }

            if (errors.Count > 0)
            {
                // oznacz błędne komórki
                foreach (var err in errors)
                {
                    if (err.rowIndex >= 0)
                    {
                        var cell = _dataGridView.Rows[err.rowIndex].Cells[err.columnName];
                        MarkValidationError(cell, err.message);
                    }
                }

                var first = errors.OrderBy(x => x.rowIndex).ThenBy(x => x.columnName).First();
                if (first.rowIndex >= 0)
                {
                    var firstCell = _dataGridView.Rows[first.rowIndex].Cells[first.columnName];
                    _dataGridView.CurrentCell = firstCell;
                    _dataGridView.BeginEdit(true);
                }

                // podsumowanie błędów grupowych (ilości)
                if (groupProblems != null && groupProblems.Count > 0)
                {
                    var lines = new List<string>();
                    foreach (var gp in groupProblems)
                    {
                        var matched = new List<string>();
                        if (_dataTable != null)
                        {
                            foreach (DataRow dr in _dataTable.Rows)
                            {
                                var rid = (dr.Table.Columns.Contains("r_id") ? dr["r_id"]?.ToString() : null) ?? string.Empty;
                                if (string.IsNullOrWhiteSpace(rid)) continue;
                                if (!rid.StartsWith(gp.prefix, StringComparison.Ordinal)) continue;

                                int q = 0;
                                if (dr.Table.Columns.Contains("quantity")) int.TryParse((dr["quantity"] ?? "0").ToString(), out q);
                                if (q == gp.expected) matched.Add(rid);
                                else matched.Add(rid);
                            }
                        }

                        if (matched.Count == 0)
                        {
                            // nie znaleziono pasujących ID
                            lines.Add($"{gp.prefix} - oczekiwana: {gp.expected}, podana: {gp.actual}");
                        }
                        else
                        {
                            foreach (var rid in matched.Distinct())
                                lines.Add($"{rid} - oczekiwana: {gp.expected}, podana: {gp.actual}");
                        }
                    }

                    var msg = new System.Text.StringBuilder();
                    msg.AppendLine("Niezgodności ilości:");
                    foreach (var l in lines.Distinct()) msg.AppendLine(l);

                    Task.Run(() => MessageBox.Show(msg.ToString().Trim(), "Niezgodności ilości", MessageBoxButtons.OK, MessageBoxIcon.Warning));
                    RecomputeDuplicateReelIds();
                    return;
                }

                // podsumowanie błędów walidacji
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"Znaleziono {errors.Count} błąd(ów). Popraw poniższe pola:");
                foreach (var g in errors.Take(20))
                {
                    if (g.rowIndex >= 0)
                        sb.AppendLine($"Wiersz {g.rowIndex + 1}: {g.columnName} — {g.message}");
                    else
                        sb.AppendLine($"Ogólny: {g.columnName} — {g.message}");
                }
                if (errors.Count > 20) sb.AppendLine("...więcej błędów nie wyświetlono");

                Task.Run(() => MessageBox.Show(sb.ToString().Trim(), "Błędy walidacji", MessageBoxButtons.OK, MessageBoxIcon.Warning));
                RecomputeDuplicateReelIds();
                return;
            }
        }

        private void ButtonSave_Click(object? sender, EventArgs e)
        {
            if (_dataGridView == null || _buttonSave == null) return;
            try { _dataGridView.EndEdit(); } catch { }

            RecomputeDuplicateReelIds();

            var rows = new List<(string reelId, string box, int used)>();
            var errors = new List<(int rowIndex, string columnName, string message)>();

            for (int i = 0; i < _dataGridView.Rows.Count; i++)
            {
                var r = _dataGridView.Rows[i];
                if (r.IsNewRow) continue;

                var reelCell = r.Cells["reel_id"];
                var boxCell = r.Cells["box"];
                var usedCell = r.Cells["used_quantity"];

                var reel = SafeString(reelCell.Value).Trim();
                var box = SafeString(boxCell.Value).Trim();
                var usedText = SafeString(usedCell.Value).Trim();

                ClearValidationError(reelCell);
                ClearValidationError(usedCell);
                ClearValidationError(boxCell);

                if (string.IsNullOrWhiteSpace(reel)) // reel_id puste
                {
                    errors.Add((i, "reel_id", "Pole wymagane"));
                }
                else
                {
                    var digits = new string(reel.Where(char.IsDigit).ToArray());
                    if (PerCellValidationFails(reelCell, digits)) // reel_id nie przeszło walidacji
                        errors.Add((i, "reel_id", "Błędne ID"));
                    else if (!string.IsNullOrEmpty(reelCell.ToolTipText ?? string.Empty) && (reelCell.ToolTipText.Contains("(duplikat)") || reelCell.ToolTipText.Contains("duplikat")))
                        errors.Add((i, "reel_id", "Duplikat")); 
                }

                int usedParsed = 0;
                if (string.IsNullOrWhiteSpace(usedText)) // used_quantity puste
                {
                    errors.Add((i, "used_quantity", "Pole wymagane"));
                }
                else if (!int.TryParse(usedText, out usedParsed)) // used_quantity nie jest liczbą
                {
                    errors.Add((i, "used_quantity", "Nieprawidłowa liczba"));
                }


                if (string.IsNullOrWhiteSpace(box)) // box puste
                {
                    errors.Add((i, "box", "Pole wymagane"));
                }

                if (!errors.Any(x => x.rowIndex == i)) // jeśli nie ma błędów w tym wierszu, dodaj do listy zapisu
                    rows.Add((reel, box, usedParsed));
            }

            if (errors.Count > 0)
            {
                // oznacz błędne komórki
                foreach (var err in errors)
                {
                    if (err.rowIndex >= 0)
                    {
                        var cell = _dataGridView.Rows[err.rowIndex].Cells[err.columnName];
                        MarkValidationError(cell, err.message);
                    }
                }

                var first = errors.OrderBy(x => x.rowIndex).ThenBy(x => x.columnName).First();
                if (first.rowIndex >= 0)
                {
                    var firstCell = _dataGridView.Rows[first.rowIndex].Cells[first.columnName];
                    _dataGridView.CurrentCell = firstCell;
                    _dataGridView.BeginEdit(true);
                }

                // podsumowanie błędów walidacji
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"Znaleziono {errors.Count} błąd(ów). Popraw poniższe pola:");
                foreach (var g in errors.Take(20))
                {
                    if (g.rowIndex >= 0)
                        sb.AppendLine($"Wiersz {g.rowIndex + 1}: {g.columnName} — {g.message}");
                    else
                        sb.AppendLine($"Ogólny: {g.columnName} — {g.message}");
                }
                if (errors.Count > 20) sb.AppendLine("...więcej błędów nie wyświetlono");

                Task.Run(() => MessageBox.Show(sb.ToString().Trim(), "Błędy walidacji", MessageBoxButtons.OK, MessageBoxIcon.Warning));
                RecomputeDuplicateReelIds();
                return;
            }

            var problems = new List<(string prefix, int expected, int actual, List<int> rowIndexes)>();

            var prefixSums = new Dictionary<string, int>();
            var prefixRows = new Dictionary<string, List<int>>();

            for (int ri = 0; ri < _dataGridView.Rows.Count; ri++)
            {
                var row = _dataGridView.Rows[ri];
                if (row.IsNewRow) continue;
                var reelVal = SafeString(row.Cells["reel_id"].Value);
                var digits = Digits(reelVal);
                if (digits.Length < 5) continue;

                var prefix = digits.Substring(0, 5);
                int used = 0;
                var usedText = SafeString(row.Cells["used_quantity"].Value).Trim();
                int.TryParse(usedText, out used);

                if (!prefixSums.TryGetValue(prefix, out var cur)) cur = 0;
                prefixSums[prefix] = cur + used;
                if (!prefixRows.TryGetValue(prefix, out var list))
                {
                    list = new List<int>();
                    prefixRows[prefix] = list;
                }
                list.Add(ri);
            }

            foreach (DataRow dr in _dataTable.Rows)
            {
                var rid = (dr.Table.Columns.Contains("r_id") ? dr["r_id"]?.ToString() : null) ?? string.Empty;
                if (string.IsNullOrWhiteSpace(rid) || rid.Length < 5) continue;
                var prefix = rid.Substring(0, 5);

                int expected = 0;
                if (dr.Table.Columns.Contains("quantity"))
                {
                    int.TryParse((dr["quantity"] ?? "0").ToString(), out expected);

                    int listSize;
                    if (!int.TryParse(_textBox?.Text ?? string.Empty, out listSize) || listSize <= 0)
                        listSize = 1;
                }

                prefixSums.TryGetValue(prefix, out var actual);

                if (actual > expected)
                {
                    prefixRows.TryGetValue(prefix, out var rowsWithPrefix);
                    problems.Add((prefix, expected, actual, rowsWithPrefix ?? new List<int>()));

                    if (rowsWithPrefix != null)
                    {
                        foreach (var ri in rowsWithPrefix)
                        {
                            var c = _dataGridView.Rows[ri].Cells["reel_id"];
                            var existing = (c.ToolTipText ?? string.Empty).Trim();
                            var msg = $"Niezgodna ilość (maksymalna: {expected}, podano: {actual})";
                            if (string.IsNullOrEmpty(existing)) c.ToolTipText = msg;
                            else if (!existing.Contains("Niezgodna ilość")) c.ToolTipText = existing + " — " + msg;
                            c.Style.BackColor = Color.LightCoral;
                        }
                    }
                }
            }

            if (problems.Count > 0)
            {
                var msg = new System.Text.StringBuilder();
                msg.AppendLine("Nie można zapisać danych z powodu niezgodności ilości:");
                foreach (var p in problems)
                {
                    msg.AppendLine($"{p.prefix} - maksymalna: {p.expected}, podana: {p.actual}");
                }
                Task.Run(() => MessageBox.Show(msg.ToString().Trim(), "Niezgodności ilości", MessageBoxButtons.OK, MessageBoxIcon.Error));
                return;
            }

            // wszystko OK — zapisz dane
            var result = _enterpriseDatabase.UpdateTraceTHT(rows, _listId).GetAwaiter().GetResult();
            try
            {
                var reservation = _enterpriseDatabase.GetReservationProgressTHTAsync(_listId).Result.Data;
                if (reservation.HasValue)
                {
                    int newDone;
                    newDone = reservation.Value.done + int.Parse(_textBox!.Text.Trim());
                    _enterpriseDatabase.UpdateListReservationTHTAsync(_listId, newDone).Wait();
                }
                else
                {
                    MessageBox.Show("Nie można zaktualizować postępu zamówienia: brak danych rezerwacji.", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas aktualizacji: {ex.Message}", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            if (result.IsSuccess)
            {
                MessageBox.Show("Dane zostały pomyślnie zapisane.", "Sukces", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            else
            {
                MessageBox.Show("Wystąpił błąd podczas zapisywania danych.", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DataGridView_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (_dataGridView == null || _buttonSave == null || e.RowIndex < 0 || e.ColumnIndex < 0) return;
            var col = _dataGridView.Columns[e.ColumnIndex];
            if (col == null) return;

            var cell = _dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];

            if (col.Name == "reel_id")
            {
                ValidateReelIdCell(cell);

                return;
            }

            // Jeśli zmieniła się wartość 'box', dostosuj pozycje guzików '+' i odśwież wiersze
            if (col.Name == "box")
            {
                int idx = e.RowIndex;
                // odśwież edytowany, poprzedni i następny wiersz (jeśli istnieją)
                if (idx >= 0 && idx < _dataGridView.Rows.Count) _dataGridView.InvalidateRow(idx);
                if (idx - 1 >= 0) _dataGridView.InvalidateRow(idx - 1);
                if (idx + 1 < _dataGridView.Rows.Count) _dataGridView.InvalidateRow(idx + 1);

                UpdateFloatingAddButtons();

                // wyczyść błąd, jeśli box nie jest już pusty
                var boxCell = _dataGridView.Rows[e.RowIndex].Cells["box"];
                if (!string.IsNullOrWhiteSpace((boxCell.Value ?? string.Empty).ToString()))
                    ClearValidationError(boxCell);
                else
                    MarkValidationError(boxCell, "Pole wymagane");
            }

            // Jeśli zmieniła się wartość 'used_quantity', zweryfikuj czy jest liczbą
            if (col.Name == "used_quantity")
            {
                var usedCell = _dataGridView.Rows[e.RowIndex].Cells["used_quantity"];
                var usedText = SafeString(usedCell.Value).Trim();
                if (int.TryParse(usedText, out _))
                {
                    ClearValidationError(usedCell);
                    // wyczyść komunikaty o niezgodnej ilości
                    var reel = SafeString(_dataGridView.Rows[e.RowIndex].Cells["reel_id"]?.Value);
                    var digits = Digits(reel);
                    if (digits.Length >= 5) ClearGroupQuantityMessagesForPrefix(digits.Substring(0, 5));
                }
            }

            RecomputeDuplicateReelIds();
        }

        private void DataGridView_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
        {
            if (_dataGridView == null) return;
            if (_dataGridView.IsCurrentCellDirty)
                _dataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void DataGridView_EditingControlShowing(object? sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (_dataGridView == null || _dataGridView.CurrentCell == null) return;

            var col = _dataGridView.CurrentCell.OwningColumn;
            if (col == null) return;

            bool isNumericColumn = col.ValueType == typeof(int) || col.ValueType == typeof(long)
                                   || col.ValueType == typeof(decimal) || col.Name == "used_quantity" || col.Name == "reel_id";

            if (e.Control is TextBox tb)
            {
                tb.KeyPress -= NumericTextBox_KeyPress;
                tb.TextChanged -= NumericTextBox_TextChanged;

                if (isNumericColumn)
                {
                    tb.KeyPress += NumericTextBox_KeyPress;
                    tb.TextChanged += NumericTextBox_TextChanged;
                }

                // ograniczenie reel_id do 14 znaków
                if (col.Name == "reel_id")
                {
                    tb.MaxLength = 14;
                    try
                    {
                        var cleaned = Digits(tb.Text);
                        ValidateReelIdCell(_dataGridView.CurrentCell, cleaned);
                        RecomputeDuplicateReelIds(_dataGridView.CurrentCell.OwningRow?.Index, cleaned);
                    }
                    catch {  }
                }
                else if (col.Name == "box")
                {
                    tb.MaxLength = 6;
                }
                else if (col.Name == "used_quantity")
                {
                    tb.MaxLength = 9;
                }
                else
                {
                    tb.MaxLength = 0;
                }
            }
        }

        private void NumericTextBox_KeyPress(object? sender, KeyPressEventArgs e)
        {
            // pozwól tylko na cyfry i klawisze sterujące
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        private void NumericTextBox_TextChanged(object sender, EventArgs e)
        {
            if (sender is not TextBox tb) return;

            try
            {
                int selStart = Math.Max(0, Math.Min(tb.SelectionStart, tb.Text?.Length ?? 0));
                string cleaned = new string((tb.Text ?? string.Empty).Where(char.IsDigit).ToArray());
                if (cleaned != tb.Text)
                {
                    int originalLength = tb.Text?.Length ?? 0;
                    tb.Text = cleaned;
                    int delta = originalLength - cleaned.Length;
                    int newPos = Math.Max(0, selStart - Math.Max(0, delta));
                    tb.SelectionStart = Math.Min(newPos, tb.Text.Length);
                }

                var currentCol = _dataGridView?.CurrentCell?.OwningColumn?.Name;
                if (currentCol == "reel_id")
                {
                    ValidateReelIdCell(_dataGridView.CurrentCell, cleaned);
                    RecomputeDuplicateReelIds(_dataGridView.CurrentCell.OwningRow?.Index, cleaned);
                }
                else if (currentCol == "used_quantity")
                {
                    // used_quantity musi być liczbą
                    var cell = _dataGridView?.CurrentCell;
                    if (cell != null)
                    {
                        if (int.TryParse(cleaned, out _))
                        {
                            ClearValidationError(cell);
                        }
                    }

                }
                else if (currentCol == "box")
                {
                    // box moze zostac pusty, chyba ze byl wczesniej oznaczony jako błąd
                    var cell = _dataGridView?.CurrentCell;
                    if (cell != null && !string.IsNullOrWhiteSpace(cleaned))
                    {
                        ClearValidationError(cell);
                    }
                }
            }
            catch {  }
        }
        private bool ValidateListSizeTextBox(out int value, out string? message)
        {
            value = 0;
            message = null;
            if (_textBox == null) return true;

            if (!int.TryParse(_textBox.Text, out value) || value <= 0)
            {
                message = "Wartość musi być dodatnią liczbą całkowitą.";
                return false;
            }

            if (value > _start)
            {
                message = $"Wartość nie może być większa niż {_start}.";
                return false;
            }

            var remaining = _start - _done;
            if (value > remaining)
            {
                message = $"Wartość nie może być większa niż {remaining} (pozostało).";
                return false;
            }

            return true;
        }

        private void ListSizeTextBox_TextChanged(object? sender, EventArgs e)
        {
            if (_textBox == null) return;

            string cleaned = new string((_textBox.Text ?? string.Empty).Where(char.IsDigit).ToArray());
            if (cleaned != _textBox.Text)
            {
                int sel = Math.Max(0, Math.Min(_textBox.SelectionStart, cleaned.Length));
                _textBox.Text = cleaned;
                _textBox.SelectionStart = sel;
            }

            var ok = ValidateListSizeTextBox(out _, out var msg);
            _textBox.BackColor = ok ? SystemColors.Window : Color.LightCoral;
            _toolTip?.SetToolTip(_textBox, ok ? string.Empty : msg ?? string.Empty);
            if (_buttonSave != null) _buttonSave.Enabled = ok;
        }

        private void ButtonAddRow_Click(object? sender, EventArgs e)
        {
            if (_dataGridView == null) return;

            string lastBox = string.Empty;
            for (int i = _dataGridView.Rows.Count - 1; i >= 0; i--)
            {
                var r = _dataGridView.Rows[i];
                if (r.IsNewRow) continue;
                lastBox = SafeString(r.Cells["box"]?.Value);
                break;
            }

            var idx = _dataGridView.Rows.Add();
            _dataGridView.Rows[idx].Cells["reel_id"].Value = string.Empty;
            _dataGridView.Rows[idx].Cells["box"].Value = lastBox;
            _dataGridView.Rows[idx].Cells["used_quantity"].Value = 0;

            UpdateFloatingAddButtons();
            _dataGridView.Refresh();

            _dataGridView.CurrentCell = _dataGridView.Rows[idx].Cells["reel_id"];
        }

        private void MarkValidationError(DataGridViewCell cell, string message)
        {
            if (cell == null) return;
            cell.Style.BackColor = Color.LightCoral;
            cell.ToolTipText = message ?? string.Empty;
        }

        private void ClearValidationError(DataGridViewCell cell)
        {
            if (cell == null) return;
            var colName = cell.OwningColumn?.Name;
            cell.Style.BackColor = (colName == "reel_id" || colName == "used_quantity" || colName == "box") ? Color.White : SystemColors.ControlLight;
            cell.ToolTipText = string.Empty;
        }

        private static string SafeString(object? o) => (o ?? string.Empty).ToString();
        private static string Digits(string? s) => new string((s ?? string.Empty).Where(char.IsDigit).ToArray());

        private void ValidateReelIdCell(DataGridViewCell cell, string? editingText = null)
        {
            if (_dataGridView == null || cell == null) return;

            var raw = editingText ?? SafeString(cell.Value);
            var digits = Digits(raw);

            ClearValidationError(cell);

            // reel_id musi mieć dokładnie 14 cyfr
            if (digits.Length != 14)
            {
                MarkValidationError(cell, "Błędne ID");
                RecomputeDuplicateReelIds(cell.OwningRow?.Index, digits);
                return;
            }

            // wyciagnij id z reel_id
            if (digits.Length >= 5)
            {
                var prefix = digits.Substring(0, 5);
                bool found = false;
                if (_dataTable != null)
                {
                    foreach (DataRow dr in _dataTable.Rows)
                    {
                        var rid = (dr.Table.Columns.Contains("r_id") ? dr["r_id"]?.ToString() : null) ?? string.Empty;
                        if (!string.IsNullOrEmpty(rid) && rid.StartsWith(prefix, StringComparison.Ordinal))
                        {
                            found = true;
                            break;
                        }
                        var id = _enterpriseDatabase.GetComponentIdByRIdAsync(int.Parse(rid)).GetAwaiter().GetResult().Data;
                        if (!string.IsNullOrEmpty(rid) && _enterpriseDatabase.GetAlternativeComponentsAsync(id).GetAwaiter().GetResult().Data.Any(ac => ac.RId == int.Parse(prefix)))
                        {
                            found = true;
                            break;
                        }
                    }
                }

                if (!found)
                {
                    MarkValidationError(cell, "Błędne ID");
                    RecomputeDuplicateReelIds(cell.OwningRow?.Index, digits);
                    return;
                }
            }

            RecomputeDuplicateReelIds(cell.OwningRow?.Index, digits);
        }

        private bool PerCellValidationFails(DataGridViewCell cell, string? digitsOverride = null)
        {
            if (_dataGridView == null || cell == null) return false;
            var raw = digitsOverride ?? SafeString(cell.Value);
            var digits = Digits(raw);

            if (string.IsNullOrWhiteSpace(digits)) return false;

            if (digits.Length != 14) return true;

            if (_dataTable != null)
            {
                var prefix = digits.Substring(0, 5);
                foreach (DataRow dr in _dataTable.Rows)
                {
                    var rid = (dr.Table.Columns.Contains("r_id") ? dr["r_id"]?.ToString() : null) ?? string.Empty;
                    if (!string.IsNullOrEmpty(rid) && rid.StartsWith(prefix, StringComparison.Ordinal))
                        return false;
                }
                return true;
            }

            return false;
        }

        private void RecomputeDuplicateReelIds(int? editingRowIndex = null, string? editingDigits = null)
        {
            if (_dataGridView == null) return;

            // zbuduj mapę wystąpień każdej wartości reel_id
            var map = new Dictionary<string, List<DataGridViewCell>>(StringComparer.Ordinal);
            for (int ri = 0; ri < _dataGridView.Rows.Count; ri++)
            {
                var r = _dataGridView.Rows[ri];
                var cell = r.Cells["reel_id"];

                // pomiń wiersz placeholder
                if (r.IsNewRow)
                {
                    ClearValidationError(cell);
                    continue;
                }

                string digits;
                if (editingRowIndex.HasValue && editingRowIndex.Value == ri && !string.IsNullOrEmpty(editingDigits))
                {
                    // użyj przekazanej wartości podczas edycji
                    digits = editingDigits;
                }
                else
                {
                    var v = SafeString(cell.Value);
                    digits = Digits(v);
                }

                // pomiń puste wartości
                if (string.IsNullOrWhiteSpace(digits))
                {
                    ClearValidationError(cell);
                    continue;
                }

                if (!map.TryGetValue(digits, out var list))
                {
                    list = new List<DataGridViewCell>();
                    map[digits] = list;
                }
                list.Add(cell);
            }

            // oznacz duplikaty
            const string dupTag = "(duplikat)";
            foreach (var kv in map)
            {
                if (kv.Value.Count <= 1)
                {
                    // pojedyncze wystąpienie - usuń oznaczenia duplikatów
                    foreach (var c in kv.Value)
                    {
                        // usuń znacznik duplikatu z tooltipa, jeśli obecny
                        if (!string.IsNullOrEmpty(c.ToolTipText) && c.ToolTipText.Contains(dupTag))
                        {
                            c.ToolTipText = c.ToolTipText.Replace($" {dupTag}", string.Empty).Replace(dupTag, string.Empty).Trim();
                        }

                        if (!PerCellValidationFails(c)) // jeśli komórka nie ma innych błędów, wyczyść kolor
                            ClearValidationError(c);
                        else // inaczej, zachowaj kolor błędu
                            c.Style.BackColor = Color.LightCoral; 
                    }
                    continue;
                }

                // wiele wystąpień - oznacz jako duplikaty
                foreach (var c in kv.Value)
                {
                    // dodaj znacznik duplikatu do tooltipa, jeśli jeszcze go nie ma
                    var existing = (c.ToolTipText ?? string.Empty).Trim();
                    if (existing.Contains(dupTag))
                        continue;

                    if (string.IsNullOrEmpty(existing))
                    {
                        // jeśli komórka jest poprawna, pokaż tylko znacznik duplikatu; w przeciwnym razie zachowaj komunikat dla komorki
                        if (PerCellValidationFails(c))
                            c.ToolTipText = $"Błędne ID {dupTag}";
                        else
                            c.ToolTipText = $"{dupTag}";
                    }
                    else
                    {
                        c.ToolTipText = existing + $" {dupTag}";
                    }

                    c.Style.BackColor = Color.LightCoral;
                }
            }

            // przejrzyj wszystkie komórki i usuń oznaczenia duplikatów tam, gdzie już nie występują
            foreach (DataGridViewRow r in _dataGridView.Rows)
            {
                var c = r.Cells["reel_id"];
                var v = (c.Value ?? string.Empty).ToString();
                var digits = new string((v ?? string.Empty).Where(char.IsDigit).ToArray());

                if (string.IsNullOrWhiteSpace(digits))
                {
                    ClearValidationError(c);
                    continue;
                }

                // jeśli nie ma duplikatu i nie ma innych błędów, wyczyść oznaczenie
                if ((!map.TryGetValue(digits, out var list) || list.Count <= 1) && !PerCellValidationFails(c))
                {
                    ClearValidationError(c);
                    continue;
                }

                if (map.TryGetValue(digits, out var finalList) && finalList.Count > 1)
                {
                    var existing = (c.ToolTipText ?? string.Empty).Trim();
                    if (!existing.Contains(dupTag))
                    {
                        if (PerCellValidationFails(c)) c.ToolTipText = $"Błędne ID {dupTag}";
                        else c.ToolTipText = $"{dupTag}";
                        c.Style.BackColor = Color.LightCoral;
                    }
                }
            }

        }

        private List<(string prefix, int expected, int actual, List<int> rowIndexes)> RecomputeGroupQuantityValidation()
        {
            var problems = new List<(string prefix, int expected, int actual, List<int> rowIndexes)>();
            if (_dataGridView == null) return problems;
            if (_dataTable == null || !_dataTable.Rows.Cast<DataRow>().Any())
            {
                return problems;
            }

            var prefixSums = new Dictionary<string, int>(StringComparer.Ordinal);
            var prefixRows = new Dictionary<string, List<int>>(StringComparer.Ordinal);

            for (int ri = 0; ri < _dataGridView.Rows.Count; ri++)
            {
                var row = _dataGridView.Rows[ri];
                if (row.IsNewRow) continue;
                var reelVal = SafeString(row.Cells["reel_id"].Value);
                var digits = Digits(reelVal);
                if (digits.Length < 5) continue;

                var prefix = digits.Substring(0, 5);
                int used = 0;
                var usedText = SafeString(row.Cells["used_quantity"].Value).Trim();
                int.TryParse(usedText, out used);

                if (!prefixSums.TryGetValue(prefix, out var cur)) cur = 0;
                prefixSums[prefix] = cur + used;
                if (!prefixRows.TryGetValue(prefix, out var list)) { list = new List<int>(); prefixRows[prefix] = list; }
                list.Add(ri);
            }

            foreach (DataRow dr in _dataTable.Rows)
            {
                var rid = (dr.Table.Columns.Contains("r_id") ? dr["r_id"]?.ToString() : null) ?? string.Empty;
                if (string.IsNullOrWhiteSpace(rid) || rid.Length < 5) continue;
                var prefix = rid.Substring(0, 5);

                int expected = 0;
                if (dr.Table.Columns.Contains("quantity"))
                {
                    int.TryParse((dr["quantity"] ?? "0").ToString(), out expected);

                    int listSize;
                    if (!int.TryParse(_textBox?.Text ?? string.Empty, out listSize) || listSize <= 0)
                        listSize = 1;
                    expected = (int)Math.Round(expected * ((double)listSize / _start ));
                }

                prefixSums.TryGetValue(prefix, out var actual);

                if (actual != expected)
                {
                    prefixRows.TryGetValue(prefix, out var rowsWithPrefix);
                    problems.Add((prefix, expected, actual, rowsWithPrefix ?? new List<int>()));

                    if (rowsWithPrefix != null)
                    {
                        foreach (var ri in rowsWithPrefix)
                        {
                            var c = _dataGridView.Rows[ri].Cells["reel_id"];
                            var existing = (c.ToolTipText ?? string.Empty).Trim();
                            var msg = $"Niezgodna ilość (oczekiwano: {expected}, podano: {actual})";
                            if (string.IsNullOrEmpty(existing)) c.ToolTipText = msg;
                            else if (!existing.Contains("Niezgodna ilość")) c.ToolTipText = existing + " — " + msg;
                            c.Style.BackColor = Color.LightCoral;
                        }
                    }
                }
                else
                {
                    if (prefixRows.TryGetValue(prefix, out var okRows))
                    {
                        foreach (var ri in okRows)
                        {
                            var c = _dataGridView.Rows[ri].Cells["reel_id"];
                            var tt = c.ToolTipText ?? string.Empty;
                            if (tt.Contains("Niezgodna ilość"))
                            {
                                tt = tt.Replace("Niezgodna ilość", string.Empty).Replace("--", "-").Trim();
                                if (string.IsNullOrWhiteSpace(tt)) ClearValidationError(c);
                                else c.ToolTipText = tt;
                            }
                        }
                    }
                }
            }

            return problems;
        }

        private void ClearGroupQuantityMessagesForPrefix(string prefix)
        {
            if (_dataGridView == null || string.IsNullOrWhiteSpace(prefix) || prefix.Length < 5) return;
            for (int ri = 0; ri < _dataGridView.Rows.Count; ri++)
            {
                var row = _dataGridView.Rows[ri];
                if (row.IsNewRow) continue;
                var c = row.Cells["reel_id"];
                var v = SafeString(c.Value);
                var digits = Digits(v);
                if (digits.Length < 5) continue;
                if (digits.StartsWith(prefix, StringComparison.Ordinal))
                {
                    var tt = (c.ToolTipText ?? string.Empty);
                    if (tt.Contains("Niezgodna ilość"))
                    {
                        tt = tt.Replace("Niezgodna ilość", string.Empty).Replace("--", "-").Trim();
                        if (string.IsNullOrWhiteSpace(tt)) ClearValidationError(c);
                        else c.ToolTipText = tt;
                    }
                }
            }
        }

        private void UpdateFloatingAddButtons()
        {
            if (_dataGridView == null) return;

            var actionable = new HashSet<int>();
            int lastIndex = _dataGridView.Rows.Count - 1;
            for (int i = 0; i <= lastIndex; i++)
            {
                var row = _dataGridView.Rows[i];
                if (row.IsNewRow) continue;
                var curBox = row.Cells["box"]?.Value?.ToString() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(curBox)) continue;

                int nx = i + 1; DataGridViewRow? next = null;
                while (nx < _dataGridView.Rows.Count)
                {
                    if (!_dataGridView.Rows[nx].IsNewRow) { next = _dataGridView.Rows[nx]; break; }
                    nx++;
                }

                bool endOfBoxGroup = next == null || !string.Equals(curBox, next.Cells["box"]?.Value?.ToString() ?? string.Empty, StringComparison.Ordinal);
                if (i == lastIndex || endOfBoxGroup) actionable.Add(i);
            }

            var remove = _floatingAddButtons.Keys.Except(actionable).ToList();
            foreach (var k in remove)
            {
                if (_floatingAddButtons.TryGetValue(k, out var btn))
                {
                    btn.Click -= FloatingAddButton_Click;
                    _dataGridView.Controls.Remove(btn);
                    btn.Dispose();
                    _floatingAddButtons.Remove(k);
                }
            }

            foreach (var rowIdx in actionable)
            {
                var row = _dataGridView.Rows[rowIdx];
                var anchorColName = _dataGridView.Columns.Contains("spacer") ? "spacer" : "used_quantity";
                var rect = _dataGridView.GetCellDisplayRectangle(_dataGridView.Columns[anchorColName].Index, rowIdx, true);
                if (rect.Height <= 0 || rect.Width <= 0) 
                {
                    if (_floatingAddButtons.TryGetValue(rowIdx, out var hiddenBtn)) hiddenBtn.Visible = false;
                    continue;
                }

                int btnW = Math.Min(28, rect.Width - 4);
                int btnH = Math.Max(20, rect.Height - 4);
                int btnX = rect.Left + Math.Max(2, (rect.Width - btnW) / 2);
                int btnY = rect.Top + (rect.Height - btnH) / 2;

                if (!_floatingAddButtons.TryGetValue(rowIdx, out var btn))
                {
                    btn = new Button
                    {
                        Text = "+",
                        Width = btnW,
                        Height = btnH,
                        FlatStyle = FlatStyle.System,
                        BackColor = System.Drawing.Color.White,
                        Tag = rowIdx,
                        TabStop = false
                    };
                    btn.Click += FloatingAddButton_Click;
                    _floatingAddButtons[rowIdx] = btn;
                    _dataGridView.Controls.Add(btn);
                }

                // reposition & show
                btn.Tag = rowIdx;
                btn.Location = new Point(btnX, btnY);
                btn.Visible = true;
                btn.BringToFront();
            }
        }

        private void DataGridView_Scroll(object? sender, ScrollEventArgs e) => UpdateFloatingAddButtons();
        private void DataGridView_SizeChanged(object? sender, EventArgs e) => UpdateFloatingAddButtons();
        private void DataGridView_RowsChanged(object? sender, EventArgs e)
        {
            UpdateFloatingAddButtons();
            RecomputeDuplicateReelIds();
        }

        private void DataGridView_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (_dataGridView == null || e.RowIndex < 0 || e.ColumnIndex < 0) return;
            var col = _dataGridView.Columns[e.ColumnIndex];
            if (col == null) return;

            if (col.Name == "add_row")
            {
                e.Value = string.Empty;
                e.CellStyle.BackColor = SystemColors.Control;
                return;
            }

            if (col.Name == "reel_id")
            {
                var cell = _dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
                if (!string.IsNullOrEmpty(cell.ToolTipText))
                {
                    e.CellStyle.BackColor = Color.LightCoral;
                    e.CellStyle.ForeColor = SystemColors.ControlText;
                }
                else
                {
                    e.CellStyle.BackColor = Color.White;
                    e.CellStyle.ForeColor = SystemColors.ControlText;
                }
            }
        }

        private void DataGridView_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (_dataGridView == null || e.RowIndex < 0 || e.ColumnIndex < 0) return;
            var col = _dataGridView.Columns[e.ColumnIndex];
            if (col == null || col.Name != "add_row") return;

            int lastIndex = _dataGridView.Rows.Count - 1;
            if (e.RowIndex < 0 || e.RowIndex > lastIndex) return;

            var curRow = _dataGridView.Rows[e.RowIndex];
            if (curRow.IsNewRow) return;

            var curBox = curRow.Cells["box"]?.Value?.ToString() ?? string.Empty;

            bool isLast = e.RowIndex == lastIndex;
            bool nextBoxDifferent = false;
            if (!isLast)
            {
                int nx = e.RowIndex + 1;
                DataGridViewRow? next = null;
                while (nx < _dataGridView.Rows.Count)
                {
                    if (!_dataGridView.Rows[nx].IsNewRow) { next = _dataGridView.Rows[nx]; break; }
                    nx++;
                }

                var nextBox = next?.Cells["box"]?.Value?.ToString() ?? string.Empty;
                nextBoxDifferent = !string.Equals(curBox, nextBox, StringComparison.Ordinal);
            }

            if (string.IsNullOrWhiteSpace(curBox))
                return;

            if (!isLast && !nextBoxDifferent)
                return;

            int insertIndex = InsertRowAfter(e.RowIndex, curBox);
            if (insertIndex >= 0)
            {
                _dataGridView.CurrentCell = _dataGridView.Rows[insertIndex].Cells["reel_id"];
                _dataGridView.FirstDisplayedScrollingRowIndex = Math.Max(0, insertIndex - 3);
            }
        }

        private int InsertRowAfter(int rowIndex, string? boxValue)
        {
            if (_dataGridView == null) return -1;
            int lastIndex = _dataGridView.Rows.Count - 1;
            bool isLast = rowIndex >= lastIndex - 1;
            int insertIndex = -1;

            DataTable? boundTable = null;
            if (_dataGridView.DataSource is DataTable dtSource) boundTable = dtSource;
            else if (_dataGridView.DataSource is BindingSource bs && bs.DataSource is DataTable dtBs) boundTable = dtBs;

            if (boundTable != null)
            {
                var newRow = boundTable.NewRow();
                newRow["reel_id"] = string.Empty;
                newRow["box"] = boxValue ?? string.Empty;
                newRow["used_quantity"] = 0;

                if (isLast)
                {
                    boundTable.Rows.Add(newRow);
                    insertIndex = boundTable.Rows.Count - 1;
                }
                else
                {
                    int dtInsertPos = Math.Min(boundTable.Rows.Count, rowIndex + 1);
                    boundTable.Rows.InsertAt(newRow, dtInsertPos);
                    insertIndex = dtInsertPos;
                }
            }
            else
            {
                if (isLast)
                {
                    insertIndex = _dataGridView.Rows.Add();
                    _dataGridView.Rows[insertIndex].Cells["reel_id"].Value = string.Empty;
                    _dataGridView.Rows[insertIndex].Cells["box"].Value = boxValue ?? string.Empty;
                    _dataGridView.Rows[insertIndex].Cells["used_quantity"].Value = 0;
                }
                else
                {
                    insertIndex = rowIndex + 1;
                    _dataGridView.Rows.Insert(insertIndex, new object[] { string.Empty, boxValue ?? string.Empty, 0, string.Empty });
                }
            }

            UpdateFloatingAddButtons();
            RecomputeDuplicateReelIds();
            _dataGridView.Refresh();

            return insertIndex;
        }

        private void FloatingAddButton_Click(object? sender, EventArgs e)
        {
            if (sender is not Button b) return;
            if (b.Tag is not int rowIndex) return;
            _dataGridView?.EndEdit();
            var box = SafeString(_dataGridView?.Rows[rowIndex].Cells["box"]?.Value);
            var newIndex = InsertRowAfter(rowIndex, box);
            if (newIndex >= 0)
            {
                _dataGridView.CurrentCell = _dataGridView.Rows[newIndex].Cells["reel_id"];
                _dataGridView.FirstDisplayedScrollingRowIndex = Math.Max(0, newIndex - 3);
            }
        }

        private void ButtonSaveDraft_Click(object? sender, EventArgs e)
        {
            if (_dataGridView?.DataSource is DataTable dt)
            {
                var filtered = dt.Clone();
                foreach (DataRow row in dt.Rows)
                {
                    var reelId = (row.Table.Columns.Contains("reel_id") ? row["reel_id"]?.ToString() : null) ?? string.Empty;
                    var box = (row.Table.Columns.Contains("box") ? row["box"]?.ToString() : null) ?? string.Empty;
                    var quantity = (row.Table.Columns.Contains("used_quantity") ? row["used_quantity"]?.ToString() : null) ?? string.Empty;
                    bool emptyReel = string.IsNullOrWhiteSpace(reelId);
                    bool emptyBox = string.IsNullOrWhiteSpace(box);
                    bool emptyQty = string.IsNullOrWhiteSpace(quantity) || quantity == "0";
                    if (!(emptyReel && emptyBox && emptyQty))
                        filtered.ImportRow(row);
                }
                _enterpriseDatabase.SaveDraft(_listId, filtered);
            }
        }

        private void THTUpdateView_FormClosed(object? sender, FormClosedEventArgs e)
        {
            this.FormClosed -= THTUpdateView_FormClosed;
            if (_buttonCheck != null) _buttonCheck.Click -= ButtonCheck_Click;
            if (_buttonSaveDraft != null) _buttonSaveDraft.Click -= ButtonSaveDraft_Click;
            if (_buttonSave != null) _buttonSave.Click -= ButtonSave_Click;
            if (_dataGridView != null)
            {
                _dataGridView.CellContentClick -= DataGridView_CellContentClick;
                _dataGridView.CellFormatting -= DataGridView_CellFormatting;
                _dataGridView.Scroll -= DataGridView_Scroll;
                _dataGridView.RowsAdded -= DataGridView_RowsChanged;
                _dataGridView.RowsRemoved -= DataGridView_RowsChanged;
                _dataGridView.CellValueChanged -= DataGridView_CellValueChanged;
                _dataGridView.CurrentCellDirtyStateChanged -= DataGridView_CurrentCellDirtyStateChanged;
                _dataGridView.EditingControlShowing -= DataGridView_EditingControlShowing;

                foreach (var btn in _floatingAddButtons.Values.ToList())
                {
                    btn.Click -= FloatingAddButton_Click;
                    _dataGridView.Controls.Remove(btn);
                    btn.Dispose();
                }
                _floatingAddButtons.Clear();
            }

            if (_textBox != null)
            {
                _textBox.KeyPress -= NumericTextBox_KeyPress;
                _textBox.TextChanged -= ListSizeTextBox_TextChanged;
            }
            if (_toolTip != null)
            {
                _toolTip.Dispose();
                _toolTip = null;
            }
        }
    }
}