using System.Data;
using WILK.Presenters;
using WILK.Services;

namespace WILK.Views.Tabs
{
    public class ExcessiveTab : BaseTab, IExcessiveView
    {
        private readonly ExcessivePresenter _presenter;
        public override string TabName => "Excessive";

        private TextBox? _textBoxID;
        private TextBox? _textBoxQuantity;
        private TextBox? _textBoxReelId;
        private TextBox? _textBoxReason;
        private Button? _buttonAddExcessive;
        private DataGridView? _historyGrid;
        private Button? _buttonDeleteSelected;

        public event EventHandler<ExcessiveUsageEventArgs>? ExcessiveUsageAdded;
        public event EventHandler<EventArgs>? ExcessiveTabSelected; 
        public event EventHandler<ExcessiveDeleteEventArgs>? DeleteExcessive;

        public ExcessiveTab(IEnterpriseDatabase enterpriseDatabase, IMainView mainView)
            : base(enterpriseDatabase, mainView)
        {
            _presenter = new ExcessivePresenter(this, enterpriseDatabase);
        }

        protected override void CreateTabPage()
        {
            TabPage = new TabPage("Ponadnormatywne")
            {
                UseVisualStyleBackColor = true
            };
        }

        protected override void SetupControls()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                AutoSize = false,
            };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var topPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                RowCount = 4,
                AutoSize = true,
            };
            topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            topPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            topPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            topPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            topPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var labelId = new Label
            {
                Text = "ID",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Margin = new Padding(10, 10, 10, 0),
                Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular)
            };
            var labelQuantity = new Label
            {
                Text = "Ilość",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Margin = new Padding(10, 10, 10, 0),
                Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular)
            };
            var labelReelId = new Label
            {
                Text = "Reel ID",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Margin = new Padding(10, 6, 10, 0),
                Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular)
            };
            _textBoxID = new TextBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(10, 3, 10, 6),
                PlaceholderText = "ID"
            };
            // Sanityzowanie wejścia: tylko cyfry dla ID
            _textBoxID.TextChanged += (s, e) =>
            {
                var tb = s as TextBox;
                if (tb == null) return;
                var originalText = tb.Text;
                var filteredText = new string(originalText.Where(char.IsDigit).ToArray());
                if (originalText != filteredText)
                {
                    var selectionStart = tb.SelectionStart - (originalText.Length - filteredText.Length);
                    tb.Text = filteredText;
                    tb.SelectionStart = Math.Max(0, selectionStart);
                }
            };
            _textBoxQuantity = new TextBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(10, 3, 10, 10),
                PlaceholderText = "Ilość"
            };
            // Sanityzowanie wejścia: tylko cyfry dla Ilość
            _textBoxQuantity.TextChanged += (s, e) =>
            {
                var tb = s as TextBox;
                if (tb == null) return;
                var originalText = tb.Text;
                var filteredText = new string(originalText.Where(char.IsDigit).ToArray());
                if (originalText != filteredText)
                {
                    var selectionStart = tb.SelectionStart - (originalText.Length - filteredText.Length);
                    tb.Text = filteredText;
                    tb.SelectionStart = Math.Max(0, selectionStart);
                }
            };
            _textBoxReelId = new TextBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(10, 3, 10, 10),
                PlaceholderText = "Reel ID"
            };
            // Sanityzowanie wejścia: usuń znaki nowej linii dla Reel ID
            _textBoxReelId.TextChanged += (s, e) =>
            {
                var tb = s as TextBox;
                if (tb == null) return;
                var txt = tb.Text;
                if (txt.Contains('\n') || txt.Contains('\r'))
                {
                    tb.Text = txt.Replace("\r", "").Replace("\n", "");
                    tb.SelectionStart = tb.Text.Length;
                }
            };

            topPanel.Controls.Add(labelId, 0, 0);
            topPanel.Controls.Add(labelQuantity, 1, 0);
            topPanel.Controls.Add(_textBoxID, 0, 1);
            topPanel.Controls.Add(_textBoxQuantity, 1, 1);
            topPanel.Controls.Add(labelReelId, 0, 2);
            topPanel.Controls.Add(_textBoxReelId, 0, 3);

            _textBoxReason = new TextBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(10),
                Multiline = true,
                AcceptsReturn = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new System.Drawing.Font("Segoe UI", 10F),
                PlaceholderText = "Powód"
            };
            // Ogranicz wysokość do 3 linii
            int reasonLineHeight = (int)Math.Ceiling(_textBoxReason.Font.GetHeight());
            int reasonMaxLines = 3;
            int desiredReasonHeight = reasonLineHeight * reasonMaxLines + 8;
            _textBoxReason.MinimumSize = new System.Drawing.Size(0, desiredReasonHeight);
            _textBoxReason.MaximumSize = new System.Drawing.Size(0, desiredReasonHeight);
            _textBoxReason.TextChanged += TextBoxReason_TextChanged;

            _buttonAddExcessive = new Button
            {
                Text = "Dodaj ponadnormatywne użycie",
                Dock = DockStyle.Right,
                Margin = new Padding(10)
            };
            _buttonDeleteSelected = new Button
            {
                Text = "Usuń zaznaczone",
                Dock = DockStyle.Left,
                Margin = new Padding(10)
            };

            layout.Controls.Add(topPanel, 0, 0);
            var reasonPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                AutoSize = false
            };
            reasonPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            reasonPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 80F));
            var labelReason = new Label
            {
                Text = "Powód",
                Dock = DockStyle.Top,
                Margin = new Padding(10, 10, 10, 0),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            reasonPanel.Controls.Add(labelReason, 0, 0);
            reasonPanel.Controls.Add(_textBoxReason, 0, 1);

            layout.RowCount = 5;
            layout.RowStyles.Clear();
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // inputy ID i ilość
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // powód
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // przyciski
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // etykieta historii
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // siatka historii
            layout.Controls.Add(reasonPanel, 0, 1);

            // Panel przycisków: umieść przyciski Dodaj i Usuń obok siebie i wyrównaj
            var buttonsPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                AutoSize = true,
            };
            buttonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            buttonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            _buttonAddExcessive.Anchor = AnchorStyles.Right;
            _buttonDeleteSelected.Anchor = AnchorStyles.Left;
            buttonsPanel.Controls.Add(_buttonAddExcessive, 0, 0);
            buttonsPanel.Controls.Add(_buttonDeleteSelected, 1, 0);

            layout.Controls.Add(buttonsPanel, 0, 2);
            var historyLabel = new Label
            {
                Text = "Historia",
                Dock = DockStyle.Top,
                Margin = new Padding(10, 10, 10, 0),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            _historyGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                Margin = new Padding(10)
            };

            layout.Controls.Add(historyLabel, 0, 3);
            layout.Controls.Add(_historyGrid, 0, 4);

            TabPage.Controls.Add(layout);
        }

        protected override void AttachEventHandlers()
        {
            if (_buttonAddExcessive != null)
            {
                _buttonAddExcessive.Click += ButtonAddExcessive_Click;
            }
            if (_buttonDeleteSelected != null)
            {
                _buttonDeleteSelected.Click += ButtonDeleteSelected_Click; 
            }
        }

        public override void OnTabSelected()
        {
            ExcessiveTabSelected?.Invoke(this, EventArgs.Empty);
        }

        private void ButtonAddExcessive_Click(object? sender, EventArgs e)
        {
            if (_textBoxID == null || _textBoxQuantity == null || _textBoxReason == null)
                return;

            string productId = _textBoxID.Text.Trim();
            if (string.IsNullOrEmpty(productId) || productId.Length != 5) // ID nie może być puste, ma mieć 5 znaków
            {
                MessageBox.Show("Wprowadz 5-cyfrowe ID produktu.", "Błąd wejścia", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(_textBoxQuantity.Text.Trim(), out int quantity) || quantity <= 0) // Ilość musi być dodatnią liczbą całkowitą
            {
                MessageBox.Show("Wprowadź poprawną dodatnią liczbę całkowitą dla ilości.", "Błąd wejścia", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string reason = _textBoxReason.Text.Trim();
            if (string.IsNullOrEmpty(reason)) // Powód nie może być pusty
            {
                MessageBox.Show("Wprowadź powód nadmiarowego wpisu.", "Błąd wejścia", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string reelId = _textBoxReelId.Text.Trim();
            if (!string.IsNullOrEmpty(reelId) && reelId.Length != 14) // reelID może być puste, ale ma mieć 14 znaków
            {
                MessageBox.Show("Wprowadz 14-cyfrowe Reel ID produktu.", "Błąd wejścia", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Wywołaj zdarzenie dodania ponadnormatywnego użycia
            ExcessiveUsageAdded?.Invoke(this, new ExcessiveUsageEventArgs(int.Parse(productId), quantity, reason, _textBoxReelId?.Text.Trim()));

            // Wyczyść pola wejściowe po dodaniu
            _textBoxID.Clear();
            _textBoxQuantity.Clear();
            _textBoxReason.Clear();
            _textBoxReelId?.Clear();
        }

        private void ButtonDeleteSelected_Click(object? sender, EventArgs e)
        {
            if (_historyGrid == null) return;
            if (_historyGrid.SelectedRows.Count == 0) // Brak zaznaczenia
            {
                MessageBox.Show("Proszę zaznaczyć wiersz do usunięcia.", "Brak zaznaczenia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var selectedRow = _historyGrid.SelectedRows[0];
            if (selectedRow.Cells["id"].Value == null) return;
            if (int.TryParse(selectedRow.Cells["id"].Value.ToString(), out int id))  // Pobierz ID z zaznaczonego wiersza
            {
                DeleteExcessive?.Invoke(this, new ExcessiveDeleteEventArgs(id));
            }
        }

        private void TextBoxReason_TextChanged(object? sender, EventArgs e)
        {
            if (_textBoxReason == null) return;
            var lines = _textBoxReason.Lines;
            if (lines.Length <= 3) return;
            // Ogranicz do pierwszych 3 linii
            var allowed = lines.Take(3).ToArray();
            int selStart = _textBoxReason.SelectionStart;
            _textBoxReason.Text = string.Join(Environment.NewLine, allowed);
            _textBoxReason.SelectionStart = Math.Min(selStart, _textBoxReason.Text.Length);
        }

        public void BindExcessiveHistoryGrid(System.Data.DataTable table)
        {
            if (_historyGrid == null) return;

            // Związanie danych
            _historyGrid.DataSource = table;

            if (_historyGrid == null) return;

            // Konfiguracja kolumn
            if (_historyGrid.Columns.Contains("id") && _historyGrid.Columns["id"] != null)
            {
                _historyGrid.Columns["id"].Visible = false;
            }

            if (_historyGrid.Columns.Contains("component_id") && _historyGrid.Columns["component_id"] != null)
            {
                _historyGrid.Columns["component_id"].HeaderText = "ID komponentu";
                _historyGrid.Columns["component_id"].DisplayIndex = 0;
                _historyGrid.Columns["component_id"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
            if (_historyGrid.Columns.Contains("quantity") && _historyGrid.Columns["quantity"] != null)
            {
                _historyGrid.Columns["quantity"].HeaderText = "Ilość";
                _historyGrid.Columns["quantity"].DisplayIndex = 1;
                _historyGrid.Columns["quantity"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
            if (_historyGrid.Columns.Contains("reason") && _historyGrid.Columns["reason"] != null)
            {
                _historyGrid.Columns["reason"].HeaderText = "Powód";
                _historyGrid.Columns["reason"].DisplayIndex = 2;
                _historyGrid.Columns["reason"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
            if (_historyGrid.Columns.Contains("created_at") && _historyGrid.Columns["created_at"] != null)
            {
                _historyGrid.Columns["created_at"].HeaderText = "Data";
                _historyGrid.Columns["created_at"].DefaultCellStyle.Format = "yyyy-MM-dd";
                _historyGrid.Columns["created_at"].DisplayIndex = 3;
                _historyGrid.Columns["created_at"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }

            _historyGrid.AutoResizeColumns();
            _historyGrid.ClearSelection();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _textBoxID?.Dispose();
                _textBoxQuantity?.Dispose();
                _textBoxReelId?.Dispose();
                _textBoxReason?.Dispose();
                _buttonAddExcessive?.Dispose();
                _historyGrid?.Dispose();
                _buttonDeleteSelected?.Dispose();
                _presenter.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}