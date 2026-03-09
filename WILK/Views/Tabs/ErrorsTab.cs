using System.Data;
using DocumentFormat.OpenXml.Math;
using WILK.Presenters;
using WILK.Services;

namespace WILK.Views.Tabs
{
    public class ErrorsTab : BaseTab, IErrorsView
    {
        private ComboBox? _comboBoxType;
        private TextBox? _textBoxReelId;
        private TextBox? _textBoxCorrectOrder;
        private ComboBox? _comboBoxCause;
        private TextBox? _textBoxCorrectAmount;
        private TextBox? _textBoxCorrectBox;
        private TextBox? _textBoxDescription;
        private Button? _buttonAddErrors;
        private DataGridView? _historyGrid;
        private ComboBox? _comboBoxAuthor;
        private readonly ErrorsPresenter _presenter;
        public override string TabName => "Błędy";

        public event EventHandler<ErrorsEventArgs>? ErrorsAdded;
        public event EventHandler<EventArgs>? ErrorsTabSelected; 

        public ErrorsTab(IEnterpriseDatabase enterpriseDatabase, IMainView mainView)
            : base(enterpriseDatabase, mainView)
        {
            _presenter = new ErrorsPresenter(this, enterpriseDatabase);
        }

        protected override void CreateTabPage()
        {
            TabPage = new TabPage("Błędy")
            {
                UseVisualStyleBackColor = true
            };
        }

        protected override void SetupControls()
        {
            //miejsce na dodanie przycisków i DataGridView do wyświetlania błędów
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
                ColumnCount = 3,
                RowCount = 4,
                AutoSize = true,
            };
            topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
            topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            topPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            topPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            topPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            topPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var labelType = new Label
            {
                Text = "Typ błędu",
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
                Margin = new Padding(10, 10, 10, 0),
                Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular)
            };
            var labelOrder = new Label
            {
                Text = "Nr Zamówienia",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Margin = new Padding(10, 6, 10, 0),
                Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular)
            };
            var labelCause = new Label
            {
                Text = "Powód",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Margin = new Padding(10, 6, 10, 0),
                Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular)
            };
            var labelAmount = new Label
            {
                Text = "Poprawna ilość",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Margin = new Padding(10, 6, 10, 0),
                Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular)
            };
            var labelBox = new Label
            {
                Text = "Pojemnik",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Margin = new Padding(10, 6, 10, 0),
                Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular)
            };
            _comboBoxType = new ComboBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(10, 3, 10, 6),
                DropDownStyle = ComboBoxStyle.DropDownList,
                AutoCompleteSource = AutoCompleteSource.ListItems
            };
            _comboBoxType.Items.AddRange(new object[]//pełna lista typów błędów (PD-28)
            {
                "Błędy lokalizacji",
                "Błędy przyjęcia",
                "Błędy oznaczeń",
                "Inne"
            });
            _textBoxReelId = new TextBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(10, 3, 10, 6),
                PlaceholderText = "Reel ID"

            };
            _textBoxCorrectOrder = new TextBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(10, 3, 10, 6),
                PlaceholderText = "Nr Zamówienia"

            };
            _comboBoxCause = new ComboBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(10, 3, 10, 6),
                DropDownStyle = ComboBoxStyle.DropDownList,
                AutoCompleteSource = AutoCompleteSource.ListItems
            };
            _comboBoxCause.Items.AddRange(new object[]//pełna lista powodów błędów (PD-28)
            {
                "Źle odłożony komponent",
                "Komponent w złej strefie (THT/SMD)",
                "Brak pojemnika na danej lokalizacji",
                "Błędny numer ID komponentu",
                "Błędny numer zamówienia",
                "Zła ilość przyjęta",
                "Nieczytelna etykieta",
                "Brak etykiety",
                "Inne"
            });
            _textBoxCorrectAmount = new TextBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(10, 3, 10, 6),
                PlaceholderText = "Poprawna ilość"
            };
            _textBoxCorrectBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(10, 3, 10, 6),
                PlaceholderText = "Poprawny pojemnik"

            };
            //_textBoxReelId nie jest sanityzowany żeby móc przyjąć dane ze skanera - odfiltrowane przy dodaniu błędu
            _textBoxCorrectOrder.TextChanged += (s, e) =>
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
            // Sanityzowanie wejścia: tylko cyfry
            _textBoxCorrectAmount.TextChanged += (s, e) =>
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
            // Sanityzowanie wejścia: tylko cyfry
            _textBoxCorrectBox.TextChanged += (s, e) =>
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

            topPanel.Controls.Add(labelType, 0, 0);
            topPanel.Controls.Add(labelReelId, 1, 0);
            topPanel.Controls.Add(labelOrder, 2, 0);
            topPanel.Controls.Add(_comboBoxType, 0, 1);
            topPanel.Controls.Add(_textBoxReelId, 1, 1);
            topPanel.Controls.Add(_textBoxCorrectOrder, 2, 1);
            topPanel.Controls.Add(labelCause, 0, 2);
            topPanel.Controls.Add(labelAmount, 1, 2);
            topPanel.Controls.Add(labelBox, 2, 2);
            topPanel.Controls.Add(_comboBoxCause, 0, 3);
            topPanel.Controls.Add(_textBoxCorrectAmount, 1, 3);
            topPanel.Controls.Add(_textBoxCorrectBox, 2, 3);

            _textBoxDescription = new TextBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(10),
                Multiline = true,
                AcceptsReturn = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new System.Drawing.Font("Segoe UI", 10F),
                PlaceholderText = "Opis"
            };
            // Ogranicz wysokość do 3 linii
            int descriptionLineHeight = (int)Math.Ceiling(_textBoxDescription.Font.GetHeight());
            int descriptionMaxLines = 3;
            int desiredDescriptionHeight = descriptionLineHeight * descriptionMaxLines + 8;
            _textBoxDescription.MinimumSize = new System.Drawing.Size(0, desiredDescriptionHeight);
            _textBoxDescription.MaximumSize = new System.Drawing.Size(0, desiredDescriptionHeight);
            _textBoxDescription.TextChanged += TextBoxDescription_TextChanged;

            _buttonAddErrors = new Button
            {
                Text = "Dodaj",
                Dock = DockStyle.Right,
                Margin = new Padding(10),
                MinimumSize = new System.Drawing.Size(100, 30)
            };

            _comboBoxAuthor = new ComboBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(10, 3, 10, 6),
                DropDownStyle = ComboBoxStyle.DropDownList,
                AutoCompleteSource = AutoCompleteSource.ListItems
            };
            _comboBoxAuthor.Items.AddRange(new object[]
            {
                "Pracownik 1",
                "Pracownik 2"
            });
            _comboBoxAuthor.SelectedIndex = 0;

            layout.Controls.Add(topPanel, 0, 0);
            var descriptionPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                AutoSize = false
            };
            descriptionPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            descriptionPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 80F));
            var labelDescription = new Label
            {
                Text = "Opis",
                Dock = DockStyle.Top,
                Margin = new Padding(10, 10, 10, 0),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            descriptionPanel.Controls.Add(labelDescription, 0, 0);
            descriptionPanel.Controls.Add(_textBoxDescription, 0, 1);

            layout.RowCount = 5;
            layout.RowStyles.Clear();
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.Controls.Add(descriptionPanel, 0, 1);

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
            _buttonAddErrors.Anchor = AnchorStyles.Right;
            buttonsPanel.Controls.Add(_buttonAddErrors, 0, 0);
            buttonsPanel.Controls.Add(_comboBoxAuthor, 1, 0);

            layout.Controls.Add(buttonsPanel, 0, 2);
            var historyLabel = new Label
            {
                Text = "Historia błędów",
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
                AllowUserToResizeRows = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, 
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                RowHeadersVisible = false,
                Margin = new Padding(10)
            };
            int historyGridLineHeight = (int)Math.Ceiling(_historyGrid.Font.GetHeight());
            _historyGrid.RowTemplate.MinimumHeight = historyGridLineHeight + 8;

            layout.Controls.Add(historyLabel, 0, 3);
            layout.Controls.Add(_historyGrid, 0, 4);

            TabPage.Controls.Add(layout);
        }

        protected override void AttachEventHandlers()
        {
            if (_buttonAddErrors != null)
            {
                _buttonAddErrors.Click += ButtonAddErrors_Click;
            }
            if (_comboBoxType != null)
            {
                _comboBoxType.SelectedIndexChanged += ComboBoxType_SelectedIndexChanged;
            }
            if (_comboBoxCause != null)
            {
                _comboBoxCause.SelectedIndexChanged += ComboBoxCause_SelectedIndexChanged;
            }
            if (_historyGrid != null)
            {
                _historyGrid.CellDoubleClick += HistoryGrid_CellDoubleClick;
            }
        }

        public override void OnTabSelected()
        {
            ErrorsTabSelected?.Invoke(this, EventArgs.Empty);
        }
        private void ButtonAddErrors_Click(object? sender, EventArgs e)
        {
            string type = _comboBoxType.Text.Trim();
            if (string.IsNullOrEmpty(type)) // brak wybranego typu
            {
                MessageBox.Show("Wprowadź typ błędu.", "Błąd wejścia", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string cause = _comboBoxCause.Text.Trim();
            if (string.IsNullOrEmpty(cause)) // brak wybranego powodu
            {
                MessageBox.Show("Wprowadź powód błędu.", "Błąd wejścia", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string reelId = _textBoxReelId.Text.Trim();
            long id;
            var tab = reelId.Split("[>");                
            if (long.TryParse(tab[0], out id))//wykrycie inputu ze skanera, konwersja na 14 znakow
            {
                reelId = Convert.ToString(id);
            }
            bool reelIdBool = reelId.Length != 14;//warunek 14 znaków, jeśli * lub nie-puste
            if((_textBoxReelId.PlaceholderText == "*" && reelIdBool) || (!string.IsNullOrEmpty(reelId) && reelIdBool)){
                MessageBox.Show(reelId, "Błąd wejścia", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string correctAmount = _textBoxCorrectAmount.Text.Trim();
            bool correctAmountBool = !int.TryParse(correctAmount, out int amount) || amount <= 0;//warunek liczby większej od 0
            if((_textBoxCorrectAmount.PlaceholderText == "*" && correctAmountBool) || (!string.IsNullOrEmpty(correctAmount) && correctAmountBool)){
                MessageBox.Show("Wprowadź poprawną wartość ilości.", "Błąd wejścia", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string correctOrder = _textBoxCorrectOrder.Text.Trim();
            bool correctOrderBool = !int.TryParse(correctOrder, out int order) || order <= 0;//warunek liczby większej od 0
            if((_textBoxCorrectOrder.PlaceholderText == "*" && correctOrderBool) || (!string.IsNullOrEmpty(correctOrder) && correctOrderBool)){
                MessageBox.Show("Wprowadź poprawny numer zamówienia.", "Błąd wejścia", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string correctBox = _textBoxCorrectBox.Text.Trim();
            bool correctBoxBool = correctBox.Length != 7;//warunek 7 znaków, jeśli * lub nie-puste
            if((_textBoxCorrectBox.PlaceholderText == "*" && correctBoxBool) || (!string.IsNullOrEmpty(correctBox) && correctBoxBool)){
                MessageBox.Show("Wprowadź poprawny, 7-znakowy numer pojemnika", "Błąd wejścia", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string description = _textBoxDescription.Text.Trim();
            if(_textBoxDescription.PlaceholderText == "*" && string.IsNullOrEmpty(description)){
                MessageBox.Show("Wprowadź opis błędu", "Błąd wejścia", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string author = _comboBoxAuthor.Text.Trim();

            //zmienne nieprzyjmujące nulli dostają wartości "N/A" lub "0", można to zmienić w celu dostosowania się do standardów w tabeli danych
            //chcąc wprowadzić użycie zmiennych przyjmujących nulle, należałoby dostosować odpowiednio logikę powyższych pól przy ButtonErrorsAdd_Click
            type = string.IsNullOrEmpty(type) ? "N/A" : type;
            cause = string.IsNullOrEmpty(cause) ? "N/A" : cause;
            reelId = string.IsNullOrEmpty(reelId) ? "N/A" : reelId;
            correctAmount = string.IsNullOrEmpty(correctAmount) ? "0" : correctAmount;
            correctOrder = string.IsNullOrEmpty(correctOrder) ? "0" : correctOrder;
            correctBox = string.IsNullOrEmpty(correctBox) ? "N/A" : correctBox;
            description = string.IsNullOrEmpty(description) ? "N/A" : description;
            author = string.IsNullOrEmpty(author) ? "N/A" : author;

                        // Wywołaj zdarzenie dodania błędu
            ErrorsAdded?.Invoke(this, new ErrorsEventArgs(type, cause, reelId, int.Parse(correctAmount), int.Parse(correctOrder), correctBox, description, author));
            _comboBoxType.SelectedIndex = -1;
            _textBoxReelId?.Clear();
            _textBoxCorrectOrder?.Clear();
            _comboBoxCause.SelectedIndex = -1;
            _textBoxCorrectAmount?.Clear();
            _textBoxCorrectBox?.Clear();
            _textBoxDescription?.Clear();
            _comboBoxAuthor.SelectedIndex = 0;
        }
        private void ComboBoxType_SelectedIndexChanged(object? sender, EventArgs e)
        {
            switch (_comboBoxType?.SelectedIndex)//dostosowanie listy powodów do wybranego typu błędu, gdyby coś miało się psuć to przez różnice SelectedIndex a SelectedValue
            {
                case 0: // Błędy lokalizacji
                    _comboBoxCause.Items.Clear();
                    _comboBoxCause.Items.AddRange(new object[]
                    {
                        "Źle odłożony komponent",
                        "Komponent w złej strefie (THT/SMD)",
                        "Brak pojemnika na danej lokalizacji",
                    });
                    break;
                case 1: // Błędy przyjęcia
                    _comboBoxCause.Items.Clear();
                    _comboBoxCause.Items.AddRange(new object[]
                    {
                        "Błędny numer ID komponentu",
                        "Błędny numer zamówienia",
                        "Zła ilość przyjęta",
                    });
                    break;
                case 2: // Błędy oznaczeń
                    _comboBoxCause.Items.Clear();
                    _comboBoxCause.Items.AddRange(new object[]
                    {
                        "Nieczytelna etykieta",
                        "Brak etykiety",
                    });
                    break;
                case 3: // Inne
                    _comboBoxCause.Items.Clear();
                    _comboBoxCause.Items.Add("Inne");
                    _comboBoxCause.Text = "Inne";
                    break;
                default:
                    _comboBoxCause.Items.Clear();
                    _comboBoxCause.Items.AddRange(new object[]
                    {
                        "Źle odłożony komponent",
                        "Komponent w złej strefie (THT/SMD)",
                        "Brak pojemnika na danej lokalizacji",
                        "Błędny numer ID komponentu",
                        "Błędny numer zamówienia",
                        "Zła ilość przyjęta",
                        "Nieczytelna etykieta",
                        "Brak etykiety",
                        "Inne"
                    });
                    break;
            }
        }
        private void ComboBoxCause_SelectedIndexChanged(object? sender, EventArgs e)
        {
            switch (_comboBoxCause?.Text)//zaznaczenie obowiązkowych pól - logika faktycznie oczekuje "*" tam gdzie oczekujemy wartości nie-pustej
            {
                case "Źle odłożony komponent":
                    _comboBoxType.SelectedValue = 0; // Błędy lokalizacji
                    _textBoxReelId.PlaceholderText = "*";
                    _textBoxCorrectOrder.PlaceholderText = "Nr Zamówienia";
                    _textBoxCorrectAmount.PlaceholderText = "Poprawna ilość";
                    _textBoxCorrectBox.PlaceholderText = "*";
                    _textBoxDescription.PlaceholderText = "Opis";
                    break;
                case "Komponent w złej strefie (THT/SMD)":
                    _comboBoxType.SelectedValue = 0; // Błędy lokalizacji
                    _textBoxReelId.PlaceholderText = "*";
                    _textBoxCorrectOrder.PlaceholderText = "Nr Zamówienia";
                    _textBoxCorrectAmount.PlaceholderText = "Poprawna ilość";
                    _textBoxCorrectBox.PlaceholderText = "Poprawny pojemnik";
                    _textBoxDescription.PlaceholderText = "Opis";
                    break;
                case "Brak pojemnika na danej lokalizacji":
                    _comboBoxType.SelectedValue = 0; // Błędy lokalizacji
                    _textBoxReelId.PlaceholderText = "*";
                    _textBoxCorrectOrder.PlaceholderText = "Nr Zamówienia";
                    _textBoxCorrectAmount.PlaceholderText = "Poprawna ilość";
                    _textBoxCorrectBox.PlaceholderText = "*";
                    _textBoxDescription.PlaceholderText = "Opis";
                    break;
                case "Błędny numer ID komponentu":
                    _comboBoxType.SelectedValue = 1; // Błędy przyjęcia
                    _textBoxReelId.PlaceholderText = "*";
                    _textBoxCorrectOrder.PlaceholderText = "Nr Zamówienia";
                    _textBoxCorrectAmount.PlaceholderText = "Poprawna ilość";
                    _textBoxCorrectBox.PlaceholderText = "Poprawny pojemnik";
                    _textBoxDescription.PlaceholderText = "Opis";
                    break;
                case "Błędny numer zamówienia":
                    _comboBoxType.SelectedValue = 1; // Błędy przyjęcia
                    _textBoxReelId.PlaceholderText = "*";
                    _textBoxCorrectOrder.PlaceholderText = "*";
                    _textBoxCorrectAmount.PlaceholderText = "Poprawna ilość";
                    _textBoxCorrectBox.PlaceholderText = "Poprawny pojemnik";
                    _textBoxDescription.PlaceholderText = "Opis";
                    break;
                case "Zła ilość przyjęta":
                    _comboBoxType.SelectedValue = 1; // Błędy przyjęcia
                    _textBoxReelId.PlaceholderText = "*";
                    _textBoxCorrectOrder.PlaceholderText = "Nr Zamówienia";
                    _textBoxCorrectAmount.PlaceholderText = "*";
                    _textBoxCorrectBox.PlaceholderText = "Poprawny pojemnik";
                    _textBoxDescription.PlaceholderText = "Opis";
                    break;
                case "Nieczytelna etykieta":
                    _comboBoxType.SelectedValue = 2; // Błędy oznaczeń
                    _textBoxReelId.PlaceholderText = "*";
                    _textBoxCorrectOrder.PlaceholderText = "Nr Zamówienia";
                    _textBoxCorrectAmount.PlaceholderText = "Poprawna ilość";
                    _textBoxCorrectBox.PlaceholderText = "Poprawny pojemnik";
                    _textBoxDescription.PlaceholderText = "Opis";
                    break;
                case "Brak etykiety":
                    _comboBoxType.SelectedValue = 2; // Błędy oznaczeń
                    _textBoxReelId.PlaceholderText = "*";
                    _textBoxCorrectOrder.PlaceholderText = "Nr Zamówienia";
                    _textBoxCorrectAmount.PlaceholderText = "*";
                    _textBoxCorrectBox.PlaceholderText = "Poprawny pojemnik";
                    _textBoxDescription.PlaceholderText = "Opis";
                    break;
                case "Inne": // Inne
                    _comboBoxType.SelectedValue = 3; // Inne
                    _textBoxReelId.PlaceholderText = "Reel ID";
                    _textBoxCorrectOrder.PlaceholderText = "Nr Zamówienia";
                    _textBoxCorrectAmount.PlaceholderText = "Poprawna ilość";
                    _textBoxCorrectBox.PlaceholderText = "Poprawny pojemnik";
                    _textBoxDescription.PlaceholderText = "*";
                    break;
                default:
                    _comboBoxType.SelectedValue = -1; //wyświetlenie wszystkich typów błędu
                    _textBoxReelId.PlaceholderText = "Reel ID";
                    _textBoxCorrectOrder.PlaceholderText = "Nr Zamówienia";
                    _textBoxCorrectAmount.PlaceholderText = "Poprawna ilość";
                    _textBoxCorrectBox.PlaceholderText = "Poprawny pojemnik";
                    _textBoxDescription.PlaceholderText = "Opis";
                    break;
            }//
        }
        private void TextBoxDescription_TextChanged(object? sender, EventArgs e)
        {
            if (_textBoxDescription == null) return;
            var lines = _textBoxDescription.Lines;
            if (lines.Length <= 3) return;
            // Ogranicz do pierwszych 3 linii
            var allowed = lines.Take(3).ToArray();
            int selStart = _textBoxDescription.SelectionStart;
            _textBoxDescription.Text = string.Join(Environment.NewLine, allowed);
            _textBoxDescription.SelectionStart = Math.Min(selStart, _textBoxDescription.Text.Length);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _comboBoxType?.Dispose();
                _textBoxReelId?.Dispose();
                _textBoxCorrectOrder?.Dispose();
                _comboBoxCause?.Dispose();
                _textBoxCorrectAmount?.Dispose();
                _textBoxCorrectBox?.Dispose();
                _textBoxDescription?.Dispose();
                _comboBoxAuthor?.Dispose();
                _buttonAddErrors?.Dispose();
                _historyGrid?.Dispose();
                _presenter.Dispose();
            }
            base.Dispose(disposing);
        }

        public void BindErrorsHistoryGrid(System.Data.DataTable table)
        {
            if (_historyGrid == null) return;

            // Związanie danych
            _historyGrid.DataSource = table;

            if (_historyGrid == null) return;

            // Konfiguracja kolumn

            if (_historyGrid.Columns.Contains("type") && _historyGrid.Columns["type"] != null)
            {
                _historyGrid.Columns["type"].HeaderText = "Typ błędu";
                _historyGrid.Columns["type"].DisplayIndex = 0;
                _historyGrid.Columns["type"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
            if (_historyGrid.Columns.Contains("cause") && _historyGrid.Columns["cause"] != null)
            {
                _historyGrid.Columns["cause"].HeaderText = "Przyczyna błędu";
                _historyGrid.Columns["cause"].DisplayIndex = 1;
                _historyGrid.Columns["cause"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
            if (_historyGrid.Columns.Contains("id_reel") && _historyGrid.Columns["id_reel"] != null)
            {
                _historyGrid.Columns["id_reel"].HeaderText = "Reel ID";
                _historyGrid.Columns["id_reel"].DisplayIndex = 2;
                _historyGrid.Columns["id_reel"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
            if (_historyGrid.Columns.Contains("correct_amount") && _historyGrid.Columns["correct_amount"] != null)
            {
                _historyGrid.Columns["correct_amount"].HeaderText = "Poprawna ilość";
                _historyGrid.Columns["correct_amount"].DisplayIndex = 3;
                _historyGrid.Columns["correct_amount"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                _historyGrid.Columns["correct_amount"].MinimumWidth = 100;
            }
            if (_historyGrid.Columns.Contains("correct_order") && _historyGrid.Columns["correct_order"] != null)
            {
                _historyGrid.Columns["correct_order"].HeaderText = "Nr Zamówienia";
                _historyGrid.Columns["correct_order"].DisplayIndex = 4;
                _historyGrid.Columns["correct_order"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                _historyGrid.Columns["correct_order"].MinimumWidth = 150;
            }
            if (_historyGrid.Columns.Contains("correct_box") && _historyGrid.Columns["correct_box"] != null)
            {
                _historyGrid.Columns["correct_box"].HeaderText = "Poprawny pojemnik";
                _historyGrid.Columns["correct_box"].DisplayIndex = 5;
                _historyGrid.Columns["correct_box"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
            if (_historyGrid.Columns.Contains("description") && _historyGrid.Columns["description"] != null)
            {
                _historyGrid.Columns["description"].HeaderText = "Opis";
                _historyGrid.Columns["description"].DisplayIndex = 6;
                _historyGrid.Columns["description"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill ;
                _historyGrid.Columns["description"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                _historyGrid.Columns["description"].MinimumWidth = 200;
            }
            if (_historyGrid.Columns.Contains("author") && _historyGrid.Columns["author"] != null)
            {
                _historyGrid.Columns["author"].HeaderText = "Autor wpisu";
                _historyGrid.Columns["author"].DisplayIndex = 7;
                _historyGrid.Columns["author"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
            if (_historyGrid.Columns.Contains("created_at") && _historyGrid.Columns["created_at"] != null)
            {
                _historyGrid.Columns["created_at"].HeaderText = "Data utworzenia wpisu";
                _historyGrid.Columns["created_at"].DisplayIndex = 8;
                _historyGrid.Columns["created_at"].DefaultCellStyle.Format = "yyyy-MM-dd";
                _historyGrid.Columns["created_at"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                _historyGrid.Columns["created_at"].MinimumWidth = 120;
            }

            _historyGrid.AutoResizeColumns();
            _historyGrid.ClearSelection();
        }
        private void HistoryGrid_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)//podwójne kliknięcie w komórkę DGV powoduje dostosowanie wysokości wiersza do zawartości najdłuższej komórki - opisu
        {   
            if (e.RowIndex < 0) return; // ignore header
            var row = _historyGrid.Rows[e.RowIndex];
            //row.AutoSizeMode = DataGridViewAutoSizeRowMode.AllCells;
            row.Height = row.GetPreferredHeight(e.RowIndex, DataGridViewAutoSizeRowMode.AllCells, true);
        }

    }
}