using System.ComponentModel;
using System.Data;
using System.Xml;
using WILK.Controls;
using WILK.Presenters;
using WILK.Services;
using WILK.Services.Repositories;

namespace WILK.Views.Tabs
{
    public class ListsTab : BaseTab, IListsView
    {
        private Panel? _bottomPanel;
        private readonly IFileProcessingService _fileProcessingService;

        private Button? _buttonEditReservation;
        private DropDownButton? _buttonGenerateList;
        private ContextMenuStrip? _generateListMenu;
        private TextBox? _textBoxSearchField;
        private Label? _searchLabel;
        private Button? _buttonCreateReservation;
        private TabControl? _tabControl3;
        private TabPage? _listyTabPage;
        private TabPage? _listyTHTTabPage;
        private TabPage? _singleCompsTabPage;
        private TabPage? _AdditionalMaterialsTabPage;
        private DataGridView? _dataGridView1;        
        private DataGridView? _dataGridView2;
        private DataGridView? _dataGridView3;
        private DataGridView? _dataGridView4;
        private Button? _buttonDeleteReservation;
        private Button? _buttonUpdateListDone;
        private TextBox? _textBox3;
        private ComboBox? _comboBoxSide;
        private Label? _labelSide;
        private DataTable? _currentListsDataTable;
        private string? _lastSortedColumn;
        private ListSortDirection _lastSortDirection = ListSortDirection.Ascending;
        private DataRowView? _lastSelectedListRow;
        private List<string>? _assignedPersonOptions;
        private List<string>? _destinationOptions;
        
        private string? _listsGridFilter;
        private string? _listsTHTGridFilter;
        private string? _itemsGridFilter;
        private string? _additionalMaterialsGridFilter;
        private EventHandler? _reservationCreatedHandler;
        private ListsPresenter? _presenter;
        private EventHandler? _currentUpdateReservationHandler;

        public event EventHandler<DeleteReservationEventArgs>? DeleteReservationRequested;
        public event EventHandler<UpdateListDoneEventArgs>? UpdateListDoneRequested;
        public event EventHandler<ReverseLastUpdateEventArgs>? ReverseLastUpdateRequested;
        public event EventHandler<GenerateListEventArgs>? GenerateListRequested;
        public event EventHandler<GenerateListTHTEventArgs>? GenerateListTHTRequested;
        public event EventHandler<EventArgs>? TabListsSelected;
        public event EventHandler<EventArgs>? TabListsTHTSelected;
        public event EventHandler<EventArgs>? TabItemsSelected;
        public event EventHandler<EventArgs>? TabAdditionalMaterialsSelected;
        public event EventHandler<ListsSelectedEventArgs>? UpdateAdditionalMaterialsRequested;
        public event EventHandler<ListsSelectedEventArgs>? DeleteAdditionalMaterialRequested;
        public event EventHandler<ReverseLastUpdateTHTEventArgs>? ReverseLastUpdateTHTRequested;

        public override string TabName => "Listy";

        public ListsTab(IEnterpriseDatabase enterpriseDatabase, IFileProcessingService fileProcessingService, IMainView mainView)
            : base(enterpriseDatabase, mainView)
        {
            _fileProcessingService = fileProcessingService ?? throw new ArgumentNullException(nameof(fileProcessingService));
            _presenter = new ListsPresenter(this, enterpriseDatabase);

            _assignedPersonOptions = new List<string> { "Artur", "Norbert", "Robert", "Mateusz", "Julia" };
            _destinationOptions = new List<string> { "Marcin Krzosek - Selektyw", "Magda - Budynek P", "Monika - Budynek B" };
        }

        protected override void CreateTabPage()
        {
            TabPage = new TabPage("Listy")
            {
                UseVisualStyleBackColor = true
            };
        }

        protected override void SetupControls()
        {
            SetupBottomPanel();
            if (_bottomPanel != null)
                TabPage.Controls.Add(_bottomPanel);
            TabPage.Resize += TabPage_Resize;

            PositionButtons();
        }

        private void SetupBottomPanel()
        {
            _bottomPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Name = "panel2"
            };
            _textBoxSearchField = new TextBox
            {
                Location = new Point(87, 11),
                Name = "textBoxSearchField3",
                Size = new Size(182, 27),
                TabIndex = 26
            };

            _searchLabel = new Label
            {
                AutoSize = true,
                Location = new Point(8, 14),
                Name = "label3",
                Size = new Size(73, 20),
                TabIndex = 25,
                Text = "Wyszukaj:"
            };
            SetupButtons();
            SetupTabControl();
            _bottomPanel.Controls.Add(_textBoxSearchField);
            _bottomPanel.Controls.Add(_searchLabel);
            _bottomPanel.Controls.Add(_tabControl3);
            if (_buttonEditReservation != null)
                _bottomPanel.Controls.Add(_buttonEditReservation);
            if (_buttonGenerateList != null)
                _bottomPanel.Controls.Add(_buttonGenerateList);
            if (_buttonCreateReservation != null)
                _bottomPanel.Controls.Add(_buttonCreateReservation);
            if (_buttonDeleteReservation != null)
                _bottomPanel.Controls.Add(_buttonDeleteReservation);
            if (_buttonUpdateListDone != null)
                _bottomPanel.Controls.Add(_buttonUpdateListDone);
            if (_textBox3 != null)
                _bottomPanel.Controls.Add(_textBox3);
            if (_labelSide != null)
                _bottomPanel.Controls.Add(_labelSide);
            if (_comboBoxSide != null)
                _bottomPanel.Controls.Add(_comboBoxSide);
        }

        private void SetupButtons()
        {
            int buttonHeight = 45;
            
            _textBox3 = new TextBox
            {
                Name = "textBox3",
                Size = new Size(85, 27),
                TabIndex = 18,
                PlaceholderText = "Ilość"
            };

            _labelSide = new Label
            {
                Name = "labelSide",
                Text = "Strona:",
                AutoSize = true,
                TabIndex = 17
            };

            _comboBoxSide = new ComboBox
            {
                Name = "comboBoxSide",
                Size = new Size(80, 27),
                TabIndex = 18,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _comboBoxSide.Items.AddRange(["BOT", "TOP", "OBIE"]);
            _comboBoxSide.SelectedIndex = 2;

            _buttonUpdateListDone = new Button
            {
                Name = "ButtonUpdateListDone",
                Size = new Size(122, buttonHeight),
                TabIndex = 19,
                Text = "Aktualizuj",
                UseVisualStyleBackColor = true
            };
            
            // Enter - aktualizuj
            _textBox3.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter && _buttonUpdateListDone != null && _buttonUpdateListDone.Enabled)
                {
                    _buttonUpdateListDone.PerformClick();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            };
            
            // Validacja ilości
            _textBox3.TextChanged += (s, e) =>
            {
                if (_textBox3 != null)
                {
                    if (int.TryParse(_textBox3.Text.Trim(), out int val) && val > 0)
                        _textBox3.BackColor = Color.LightGreen;
                    else
                        _textBox3.BackColor = string.IsNullOrEmpty(_textBox3.Text) ? SystemColors.Window : Color.LightPink;
                }
            };

            _buttonCreateReservation = new Button
            {
                Name = "ButtonCreateReservation",
                Size = new Size(119, buttonHeight),
                TabIndex = 24,
                Text = "Dodaj",
                UseVisualStyleBackColor = true
            };

            _buttonDeleteReservation = new Button
            {
                Name = "ButtonDeleteReservation",
                Size = new Size(118, buttonHeight),
                TabIndex = 22,
                Text = "Usuń",
                UseVisualStyleBackColor = true
            };

            _buttonEditReservation = new Button
            {
                Name = "ButtonEditReservation",
                Size = new Size(118, buttonHeight),
                TabIndex = 28,
                Text = "Edytuj",
                UseVisualStyleBackColor = true
            };

            _generateListMenu = new ContextMenuStrip();
            _generateListMenu.Items.Add("Generuj dla części", null, (s, e) => 
            {
                using var inputForm = new Form
                {
                    Text = "Podaj ilość",
                    Size = new Size(300, 150),
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false
                };

                var label = new Label
                {
                    Text = "Ilość:",
                    Location = new Point(20, 20),
                    AutoSize = true
                };

                var textBox = new TextBox
                {
                    Location = new Point(20, 45),
                    Size = new Size(240, 27),
                    Text = "1"
                };

                var okButton = new Button
                {
                    Text = "OK",
                    DialogResult = DialogResult.OK,
                    Location = new Point(105, 80),
                    Size = new Size(75, 30)
                };

                var cancelButton = new Button
                {
                    Text = "Anuluj",
                    DialogResult = DialogResult.Cancel,
                    Location = new Point(185, 80),
                    Size = new Size(75, 30)
                };

                inputForm.Controls.AddRange(new Control[] { label, textBox, okButton, cancelButton });
                inputForm.AcceptButton = okButton;
                inputForm.CancelButton = cancelButton;

                if (inputForm.ShowDialog() != DialogResult.OK)
                    return;

                if (!int.TryParse(textBox.Text.Trim(), out int multiplier) || multiplier <= 0)
                {
                    ShowError("Błąd", "Podaj poprawną liczbę!");
                    return;
                }

                if (_dataGridView1?.SelectedRows.Count > 0)
                {
                    var startValue = _dataGridView1.SelectedRows[0].Cells["start"].Value;
                    if (startValue != null && multiplier > Convert.ToInt32(startValue))
                    {
                        ShowError("Błąd", "Podaj poprawną liczbę!");
                        return;
                    }
                }
                else if (_dataGridView3?.SelectedRows.Count > 0)
                {
                    var startValue = _dataGridView3.SelectedRows[0].Cells["start"].Value;
                    if (startValue != null && multiplier > Convert.ToInt32(startValue))
                    {
                        ShowError("Błąd", "Podaj poprawną liczbę!");
                        return;
                    }
                }
                else
                {
                    ShowError("Błąd", "Nie wybrano żadnej rezerwacji!");
                    return;
                }

                GenerateListWithMultiplier(multiplier);
            });
            _buttonGenerateList = new DropDownButton
            {
                Name = "ButtonGenerateList",
                Size = new Size(180, buttonHeight),
                TabIndex = 27,
                Text = "",
                UseVisualStyleBackColor = true
            };
            _buttonGenerateList.SetText("Generuj listę wybraniową");
            _buttonGenerateList.SetContextMenu(_generateListMenu);
        }
        private void SetupTabControl()
        {
            _tabControl3 = new TabControl
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Location = new Point(4, 44),
                Name = "tabControl3",
                SelectedIndex = 0,
                Size = new Size(800, 250),
                TabIndex = 23,
                Padding = new Point(0, 0),
                SizeMode = TabSizeMode.Fixed,
                ItemSize = new Size(170, 25)
            };

            _listyTabPage = new TabPage
            {
                Location = new Point(4, 29),
                Name = "listy SMD",
                Padding = new Padding(0),
                TabIndex = 0,
                Text = "eSeMDe",
                UseVisualStyleBackColor = true,
            };

            _dataGridView1 = new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None, // Remove border to hide white edge
                EditMode = DataGridViewEditMode.EditProgrammatically,
                MultiSelect = false,
                Name = "dataGridView1",
                ReadOnly = true,
                RowHeadersVisible = false,
                RowHeadersWidth = 51,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                TabIndex = 21
            };
            
            _dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);

            _listyTHTTabPage = new TabPage
            {
                Location = new Point(4, 29),
                Name = "listy THT",
                Padding = new Padding(0),
                TabIndex = 2,
                Text = "TeHaTe",
                UseVisualStyleBackColor = true
            };

            _dataGridView3 = new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                EditMode = DataGridViewEditMode.EditOnEnter,
                MultiSelect = false,
                Name = "dataGridView3",
                ReadOnly = false, 
                RowHeadersVisible = false,
                RowHeadersWidth = 51,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                TabIndex = 21
            };

            _listyTabPage.Controls.Add(_dataGridView1);
            _listyTHTTabPage.Controls.Add(_dataGridView3);

            _singleCompsTabPage = new TabPage
            {
                Location = new Point(4, 29),
                Name = "singleComps",
                Padding = new Padding(0),
                TabIndex = 1,
                Text = "Rezerwacje komponentów",
                UseVisualStyleBackColor = true
            };

            _dataGridView2 = new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                EditMode = DataGridViewEditMode.EditProgrammatically,
                MultiSelect = false,
                Name = "dataGridView2",
                ReadOnly = true,
                RowHeadersVisible = false,
                RowHeadersWidth = 51,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                TabIndex = 22
            };
            
            _dataGridView2.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
            _singleCompsTabPage.Controls.Add(_dataGridView2);

            _AdditionalMaterialsTabPage = new TabPage
            {
                Location = new Point(4, 29),
                Name = "additionalMaterials",
                Padding = new Padding(0),
                TabIndex = 3,
                Text = "materiały dodatkowe",
                UseVisualStyleBackColor = true
            };

            _dataGridView4 = new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                EditMode = DataGridViewEditMode.EditProgrammatically,
                MultiSelect = false,
                Name = "dataGridView4",
                ReadOnly = true,
                RowHeadersVisible = false,
                RowHeadersWidth = 51,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                TabIndex = 24
            };

            _AdditionalMaterialsTabPage.Controls.Add(_dataGridView4);

            _tabControl3.Controls.Add(_listyTabPage);
            _tabControl3.Controls.Add(_listyTHTTabPage);
            _tabControl3.Controls.Add(_singleCompsTabPage);
            _tabControl3.Controls.Add(_AdditionalMaterialsTabPage);
        }  

        private void TabPage_Resize(object? sender, EventArgs e)
        {   // Przy zmianie rozmiaru zakładki, przestaw przyciski
            PositionButtons();
        }

        private void DataGridView1_SelectionChanged(object? sender, EventArgs e)
        {   // Przewaliduj opcje w ComboBoxie po zmianie zaznaczenia
            ValidateComboBoxOptions();
        }

        private void ValidateComboBoxOptions()
        {
            if (_comboBoxSide == null || _dataGridView1 == null || _buttonEditReservation == null) return;
            
            if (_dataGridView1.CurrentRow?.DataBoundItem is DataRowView rowView)
            {
                var row = rowView.Row;
                
                int start = row.Table.Columns.Contains("start") ? Convert.ToInt32(row["start"]) : 0;
                int doneBOT = row.Table.Columns.Contains("done_bot") ? Convert.ToInt32(row["done_bot"]) : 0;
                int doneTOP = row.Table.Columns.Contains("done_top") ? Convert.ToInt32(row["done_top"]) : 0;
                
                bool isOneSided = false;
                if (row.Table.Columns.Contains("is_one_sided") && row["is_one_sided"] != DBNull.Value)
                {
                    isOneSided = Convert.ToBoolean(row["is_one_sided"]);
                }
                
                bool botComplete = doneBOT >= start;
                bool topComplete = doneTOP >= start;
                
                _comboBoxSide.Items.Clear();
                
                if (isOneSided) // Jednostronne - tylko OBIE
                {
                    _comboBoxSide.Items.Add("OBIE");
                    _comboBoxSide.SelectedIndex = 0;
                    _comboBoxSide.Enabled = false; 
                    _buttonEditReservation.Enabled = false; 
                }
                else
                {
                    _comboBoxSide.Enabled = true;
                    
                    if (!botComplete) // Dodaj BOT jeśli nieskonczone
                        _comboBoxSide.Items.Add("BOT");
                    
                    if (!topComplete) // Dodaj TOP jeśli nieskonczone
                        _comboBoxSide.Items.Add("TOP");
                    
                    if (!botComplete && !topComplete) // Obie jeśli oba nieskonczone
                        _comboBoxSide.Items.Add("OBIE");
                    
                    if (_comboBoxSide.Items.Count > 0) // Jeśli są dostępne opcje
                    {
                        // Wybierz domyślnie OBIE jeśli dostępne
                        int obieIndex = _comboBoxSide.Items.IndexOf("OBIE");
                        _comboBoxSide.SelectedIndex = obieIndex >= 0 ? obieIndex : 0;
                    }

                    _buttonEditReservation.Enabled = true; // Jeśli nie jednostronne, można edytować
                }
            }
            else
            {
                // Brak zaznaczenia - zresetuj ComboBox
                _comboBoxSide.Items.Clear();
                _comboBoxSide.Items.AddRange(new object[] { "BOT", "TOP", "OBIE" });
                _comboBoxSide.SelectedIndex = 2;
                _comboBoxSide.Enabled = true;
            }
        }

        private void PositionButtons()
        {
            if (_bottomPanel == null || TabPage == null) return;

            int panelHeight = _bottomPanel.Height;
            int panelWidth = _bottomPanel.Width;
            int buttonHeight = 45;
            int buttonMargin = 10;
            int bottomOffset = buttonMargin;

            if (_tabControl3 != null)
            {
                int buttonAreaHeight = buttonHeight + buttonMargin * 2;
                int newTabControlHeight = panelHeight - _tabControl3.Top - buttonAreaHeight;
                if (newTabControlHeight > 50)
                {
                    _tabControl3.Size = new Size(panelWidth - 8, newTabControlHeight);
                }
            }

            int yPos = panelHeight - buttonHeight - bottomOffset;

            if (_textBox3 != null)
                _textBox3.Location = new Point(buttonMargin, yPos + 9);

            if (_labelSide != null)
                _labelSide.Location = new Point(buttonMargin + 95, yPos + 12);

            if (_comboBoxSide != null)
                _comboBoxSide.Location = new Point(buttonMargin + 150, yPos + 9);

            if (_buttonUpdateListDone != null)
                _buttonUpdateListDone.Location = new Point(buttonMargin + 240, yPos);

            if (_buttonCreateReservation != null)
                _buttonCreateReservation.Location = new Point(buttonMargin + 372, yPos);

            if (_buttonDeleteReservation != null)
                _buttonDeleteReservation.Location = new Point(buttonMargin + 501, yPos);

            if (_buttonEditReservation != null)
                _buttonEditReservation.Location = new Point(buttonMargin + 629, yPos);

            if (_buttonGenerateList != null)
                _buttonGenerateList.Location = new Point(panelWidth - 203 - buttonMargin, yPos);
        }
        

        protected override void AttachEventHandlers()
        {
            if (_buttonEditReservation != null)
                _buttonEditReservation.Click += ButtonEditReservation_Click;

            if (_buttonGenerateList != null)
                _buttonGenerateList.SetOnClick(ButtonGenerateList_Click);

            if (_buttonDeleteReservation != null)
                _buttonDeleteReservation.Click += ButtonDeleteReservation_Click;

            if (_buttonUpdateListDone != null)
                _buttonUpdateListDone.Click += _currentUpdateReservationHandler ??= ButtonUpdateListDone_Click;

            if (_tabControl3 != null)
                _tabControl3.SelectedIndexChanged += TabControl3_SelectedIndexChanged;

            SetupReservationCreatedHandler();
        }

        private void SaveCurrentListsFilter()
        {
            if (_dataGridView1?.DataSource is DataTable dt && dt.DefaultView != null && !string.IsNullOrEmpty(dt.DefaultView.RowFilter))
            {
                _listsGridFilter = dt.DefaultView.RowFilter;
            }
            if (_dataGridView3?.DataSource is DataTable dtTHT && dtTHT.DefaultView != null && !string.IsNullOrEmpty(dtTHT.DefaultView.RowFilter))
            {
                _listsTHTGridFilter = dtTHT.DefaultView.RowFilter;
            }
        }

        private void SaveCurrentItemsFilter()
        {
            if (_dataGridView2?.DataSource is DataTable dt && dt.DefaultView != null && !string.IsNullOrEmpty(dt.DefaultView.RowFilter))
            {
                _itemsGridFilter = dt.DefaultView.RowFilter;
            }
        }

        private void SetupReservationCreatedHandler()
        {   // Ustaw obsługę zdarzenia tworzenia rezerwacji
            if (_buttonCreateReservation == null) return;

            _reservationCreatedHandler = (s, e) =>
            {
                var createReservationListForm = new Views.CreateReservationList(_enterpriseDatabase, _fileProcessingService);
                createReservationListForm.ShowDialog();
                TabListsSelected?.Invoke(this, EventArgs.Empty);
            };

            _buttonCreateReservation.Click += _reservationCreatedHandler;
        }

        public override void OnTabSelected()
        {
            // Wyczysć filtry przy zaznaczeniu zakładki
            _listsGridFilter = null;
            _itemsGridFilter = null;
            _listsTHTGridFilter = null;
            if (_textBoxSearchField != null)
                _textBoxSearchField.Text = "";

            // Wywołaj zdarzenie zaznaczenia zakładki Listy
            TabListsSelected?.Invoke(this, EventArgs.Empty);
        }

        public void BindReservationItems(DataTable items)
        {
            if (_dataGridView2 == null) return;

            // Zapisz aktualny filtr PRZED ponownym powiązaniem
            SaveCurrentItemsFilter();
            
            // Przywróć filtr, jeśli istnieje
            if (!string.IsNullOrEmpty(_itemsGridFilter))
            {
                try
                {
                    var view = items.DefaultView;
                    view.RowFilter = _itemsGridFilter;
                }
                catch
                {
                    _itemsGridFilter = null; // Wyczyść nieprawidłowy filtr
                }
            }

            _dataGridView2.DataSource = items;

            if (items.Columns.Contains("id") && _dataGridView2.Columns["id"] != null)
                _dataGridView2.Columns["id"].Visible = false;

            if (items.Columns.Contains("r_id") && _dataGridView2.Columns["r_id"] != null)
            {
                _dataGridView2.Columns["r_id"].HeaderText = "ID";
                _dataGridView2.Columns["r_id"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }

            if (items.Columns.Contains("name") && _dataGridView2.Columns["name"] != null)
            {
                _dataGridView2.Columns["name"].HeaderText = "Nazwa komponentu";
                _dataGridView2.Columns["name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }

            if (items.Columns.Contains("quantity") && _dataGridView2.Columns["quantity"] != null)
            {
                _dataGridView2.Columns["quantity"].HeaderText = "Ilość rezerwacji";
                _dataGridView2.Columns["quantity"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
        }

        public void BindReservationsGrid(DataTable dt)
        {
            if (_dataGridView1 == null) return;

            _currentListsDataTable = dt;

            foreach (DataRow row in dt.Rows) // Formatowanie daty utworzenia
            {
                if (row["created_at"] != null && row["created_at"] != DBNull.Value)
                {
                    DateTime fullDate = Convert.ToDateTime(row["created_at"]);
                    string onlyDate = fullDate.ToString("dd-MM-yyyy");
                    Console.WriteLine(onlyDate);
                }
            }

            _dataGridView1.DataSource = dt;
            _dataGridView1.ShowCellToolTips = true;

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

            // Dodaj kolumny postępu BOT i TOP (start/done)
            if (dt.Columns.Contains("start") && dt.Columns.Contains("done_bot"))
            {
                if (!dt.Columns.Contains("progress_BOT"))
                {
                    dt.Columns.Add("progress_BOT", typeof(string));
                    foreach (DataRow row in dt.Rows)
                    {
                        int done = Convert.ToInt32(row["done_bot"]);
                        int start = Convert.ToInt32(row["start"]);
                        row["progress_BOT"] = $"{done}/{start}";
                    }
                }

                if (_dataGridView1.Columns["progress_BOT"] != null)
                {
                    _dataGridView1.Columns["progress_BOT"].HeaderText = "Postęp BOT";
                    _dataGridView1.Columns["progress_BOT"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    _dataGridView1.Columns["progress_BOT"].SortMode = DataGridViewColumnSortMode.NotSortable;
                }
                
                if (_dataGridView1.Columns["start"] != null)
                    _dataGridView1.Columns["start"].Visible = false;
                if (_dataGridView1.Columns["done_bot"] != null)
                    _dataGridView1.Columns["done_bot"].Visible = false;
            }

            if (dt.Columns.Contains("last_update_done_bot") && _dataGridView1.Columns["last_update_done_bot"] != null)
            {
                _dataGridView1.Columns["last_update_done_bot"].HeaderText = "Ostatnia wartość postępu BOT";
                _dataGridView1.Columns["last_update_done_bot"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                _dataGridView1.Columns["last_update_done_bot"].SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            if (dt.Columns.Contains("start") && dt.Columns.Contains("done_top"))
            {
                if (!dt.Columns.Contains("progress_TOP"))
                {
                    dt.Columns.Add("progress_TOP", typeof(string));
                    foreach (DataRow row in dt.Rows)
                    {
                        int done = Convert.ToInt32(row["done_top"]);
                        int start = Convert.ToInt32(row["start"]);
                        row["progress_TOP"] = $"{done}/{start}";
                    }
                }

                if (_dataGridView1.Columns["progress_TOP"] != null)
                {
                    _dataGridView1.Columns["progress_TOP"].HeaderText = "Postęp TOP";
                    _dataGridView1.Columns["progress_TOP"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    _dataGridView1.Columns["progress_TOP"].SortMode = DataGridViewColumnSortMode.NotSortable;
                }
                
                if (_dataGridView1.Columns["start"] != null)
                    _dataGridView1.Columns["start"].Visible = false;
                if (_dataGridView1.Columns["done_top"] != null)
                    _dataGridView1.Columns["done_top"].Visible = false;
            }

            if (dt.Columns.Contains("last_update_done_top") && _dataGridView1.Columns["last_update_done_top"] != null)
            {
                _dataGridView1.Columns["last_update_done_top"].HeaderText = "Ostatnia wartość postępu TOP";
                _dataGridView1.Columns["last_update_done_top"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                _dataGridView1.Columns["last_update_done_top"].SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            // Dodaj formatowanie komórek dla wierszy jednostronnych
            _dataGridView1.CellFormatting -= DataGridView1_CellFormatting;
            _dataGridView1.CellFormatting += DataGridView1_CellFormatting;
            
            // Dodaj malowanie komórek dla linków przywracania
            _dataGridView1.CellPainting -= DataGridView1_CellPainting;
            _dataGridView1.CellPainting += DataGridView1_CellPainting;
            
            // Dodaj obsługę kliknięć komórek dla funkcji przywracania
            _dataGridView1.CellClick -= DataGridView1_CellClick;
            _dataGridView1.CellClick += DataGridView1_CellClick;

            if (dt.Columns.Contains("created_at") && _dataGridView1.Columns["created_at"] != null)
            {
                _dataGridView1.Columns["created_at"].HeaderText = "Data utworzenia";
                _dataGridView1.Columns["created_at"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }

            // Ustaw DisplayIndex dla wszystkich kolumn
            if (_dataGridView1.Columns["name"] != null)
                _dataGridView1.Columns["name"].DisplayIndex = 0;
            if (_dataGridView1.Columns["progress_BOT"] != null)
                _dataGridView1.Columns["progress_BOT"].DisplayIndex = 1;
            if (_dataGridView1.Columns["progress_TOP"] != null)
                _dataGridView1.Columns["progress_TOP"].DisplayIndex = 2;
            if (_dataGridView1.Columns["last_update_done_bot"] != null)
                _dataGridView1.Columns["last_update_done_bot"].DisplayIndex = 3;
            if (_dataGridView1.Columns["last_update_done_top"] != null)
                _dataGridView1.Columns["last_update_done_top"].DisplayIndex = 4;
            if (_dataGridView1.Columns["created_at"] != null)
                _dataGridView1.Columns["created_at"].DisplayIndex = 5;

            // Przywróć zapisany filtr PO dodaniu wszystkich kolumn
            if (!string.IsNullOrEmpty(_listsGridFilter))
            {
                try
                {
                    var view = dt.DefaultView;
                    view.RowFilter = _listsGridFilter;
                }
                catch
                {
                    _listsGridFilter = null; // Wyczyść nieprawidłowy filtr
                }
            }

            // Zapisz aktualny filtr PRZED ponownym powiązaniem
            SaveCurrentListsFilter();

            _dataGridView1.Sorted -= DataGridView1_Sorted;
            _dataGridView1.Sorted += DataGridView1_Sorted;
            
            _dataGridView1.SelectionChanged -= DataGridView1_SelectionChanged;
            _dataGridView1.SelectionChanged += DataGridView1_SelectionChanged;

            var sortColumn = _dataGridView1.Columns[_lastSortedColumn ?? "id"];
            if (sortColumn != null)
            {
                _dataGridView1.Sort(sortColumn,
                    _lastSortDirection == ListSortDirection.Ascending
                        ? ListSortDirection.Ascending
                        : ListSortDirection.Descending);
            }
            _dataGridView1.CellToolTipTextNeeded -= DataGridView1_CellToolTipTextNeeded;
            _dataGridView1.CellToolTipTextNeeded += DataGridView1_CellToolTipTextNeeded;
            
            _dataGridView1.CellMouseEnter -= DataGridView1_CellMouseEnter;
            _dataGridView1.CellMouseEnter += DataGridView1_CellMouseEnter;
            
            _dataGridView1.CellMouseLeave -= DataGridView1_CellMouseLeave;
            _dataGridView1.CellMouseLeave += DataGridView1_CellMouseLeave;
            
            if (_textBoxSearchField != null)
            {
                _textBoxSearchField.TextChanged -= TextBoxSearchField_TextChanged;
                _textBoxSearchField.TextChanged += TextBoxSearchField_TextChanged;
            }
        }

        public void BindReservationsTHTGrid(DataTable dt)
        {
            if (_dataGridView3 == null) return;

            SaveCurrentListsFilter();

            _currentListsDataTable = dt;

            if (_lastSelectedListRow != null)
            {
                string lastSelectedId = _lastSelectedListRow["id"].ToString() ?? "";
                _lastSelectedListRow = dt.DefaultView.Cast<DataRowView>().FirstOrDefault(drv => drv["id"] != null && drv["id"].ToString() == lastSelectedId);
            }

            foreach (DataRow row in dt.Rows)
            {
                if (row["created_at"] != null && row["created_at"] != DBNull.Value)
                {
                    DateTime fullDate = Convert.ToDateTime(row["created_at"]);
                    string onlyDate = fullDate.ToString("dd-MM-yyyy");
                    Console.WriteLine(onlyDate);
                }
            }

            _dataGridView3.DataSource = dt;
            _dataGridView3.ShowCellToolTips = true;

            if (dt.Columns.Contains("id") && _dataGridView3.Columns["id"] != null)
                _dataGridView3.Columns["id"].Visible = false;

            if (dt.Columns.Contains("is_list") && _dataGridView3.Columns["is_list"] != null)
                _dataGridView3.Columns["is_list"].Visible = false;

            if (dt.Columns.Contains("name") && _dataGridView3.Columns["name"] != null)
            {
                _dataGridView3.Columns["name"].HeaderText = "nazwa";
                _dataGridView3.Columns["name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                _dataGridView3.Columns["name"].ReadOnly = true;
            }

            if (dt.Columns.Contains("start") && dt.Columns.Contains("done"))
            {
                if (!dt.Columns.Contains("progress"))
                {
                    dt.Columns.Add("progress", typeof(string));
                    foreach (DataRow row in dt.Rows)
                    {
                        int done = Convert.ToInt32(row["done"]);
                        int start = Convert.ToInt32(row["start"]);
                        row["progress"] = $"{done}/{start}";
                    }
                }

                if (_dataGridView3.Columns["progress"] != null)
                {
                    _dataGridView3.Columns["progress"].HeaderText = "postęp";
                    _dataGridView3.Columns["progress"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    _dataGridView3.Columns["progress"].SortMode = DataGridViewColumnSortMode.NotSortable;
                    _dataGridView3.Columns["progress"].ReadOnly = true;
                }
                
                if (_dataGridView3.Columns["start"] != null)
                    _dataGridView3.Columns["start"].Visible = false;
                if (_dataGridView3.Columns["done"] != null)
                    _dataGridView3.Columns["done"].Visible = false;
            }

            if (dt.Columns.Contains("last_update_done") && _dataGridView3.Columns["last_update_done"] != null)
            {
                _dataGridView3.Columns["last_update_done"].HeaderText = "ostatnia wartość postępu";
                _dataGridView3.Columns["last_update_done"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                _dataGridView3.Columns["last_update_done"].SortMode = DataGridViewColumnSortMode.NotSortable;
                _dataGridView3.Columns["last_update_done"].ReadOnly = true;
            }
            
            _dataGridView3.CellClick -= DataGridView3_CellClick;
            _dataGridView3.CellClick += DataGridView3_CellClick;

            if (dt.Columns.Contains("created_at") && _dataGridView3.Columns["created_at"] != null)
            {
                _dataGridView3.Columns["created_at"].HeaderText = "data utworzenia";
                _dataGridView3.Columns["created_at"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                _dataGridView3.Columns["created_at"].ReadOnly = true;
            }

            if (dt.Columns.Contains("realizeFlag") && _dataGridView3.Columns["realizeFlag"] != null)
            {
                int idx = _dataGridView3.Columns["realizeFlag"].Index;

                _dataGridView3.Columns.Remove("realizeFlag");
                var chk = new DataGridViewCheckBoxColumn
                {
                    Name = "realizeFlag",
                    HeaderText = "W realizacji",
                    DataPropertyName = "realizeFlag",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader,
                    ReadOnly = false
                };
                _dataGridView3.Columns.Insert(idx, chk);
            }

            if (dt.Columns.Contains("maxEndDate") && _dataGridView3.Columns["maxEndDate"] != null)
            {
                int idx = _dataGridView3.Columns["maxEndDate"].Index;
                _dataGridView3.Columns.Remove("maxEndDate");
                var dateCol = new DataGridViewCalendarColumn
                {
                    Name = "maxEndDate",
                    HeaderText = "Maksymalna data zakończenia",
                    DataPropertyName = "maxEndDate",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                };
                _dataGridView3.Columns.Insert(idx, dateCol);
                _dataGridView3.Columns["maxEndDate"].SortMode = DataGridViewColumnSortMode.Automatic;
            }

            if (dt.Columns.Contains("assignedPerson") && _dataGridView3.Columns["assignedPerson"] != null)
            {
                int idx = _dataGridView3.Columns["assignedPerson"].Index;
                _dataGridView3.Columns.Remove("assignedPerson");

                var persons = (_assignedPersonOptions != null && _assignedPersonOptions.Count > 0)
                                ? new List<string>(_assignedPersonOptions)
                                : dt.AsEnumerable()
                                    .Select(r => r["assignedPerson"] != DBNull.Value ? r["assignedPerson"].ToString() ?? "" : "")
                                    .Select(s => s.Trim())
                                    .Where(s => !string.IsNullOrEmpty(s))
                                    .Distinct()
                                    .OrderBy(s => s)
                                    .ToList();

                if (!persons.Contains("")) persons.Insert(0, "");
                persons = persons.OrderBy(s => s == "" ? "" : s).ToList();

                var personCombo = new DataGridViewComboBoxColumn
                {
                    Name = "assignedPerson",
                    HeaderText = "Osoba przypisana",
                    DataPropertyName = "assignedPerson",
                    DataSource = persons,
                    DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                    FlatStyle = FlatStyle.Flat
                };
                _dataGridView3.Columns.Insert(idx, personCombo);
            }

            if (dt.Columns.Contains("destination") && _dataGridView3.Columns["destination"] != null)
            {
                int idx = _dataGridView3.Columns["destination"].Index;
                _dataGridView3.Columns.Remove("destination");

                var destinations = (_destinationOptions != null && _destinationOptions.Count > 0)
                                    ? new List<string>(_destinationOptions)
                                    : dt.AsEnumerable()
                                        .Select(r => r["destination"] != DBNull.Value ? r["destination"].ToString() ?? "" : "")
                                        .Select(s => s.Trim())
                                        .Where(s => !string.IsNullOrEmpty(s))
                                        .Distinct()
                                        .OrderBy(s => s)
                                        .ToList();
                if (!destinations.Contains("")) destinations.Insert(0, "");
                destinations = destinations.OrderBy(s => s == "" ? "" : s).ToList();

                var destCombo = new DataGridViewComboBoxColumn
                {
                    Name = "destination",
                    HeaderText = "Miejsce docelowe",
                    DataPropertyName = "destination",
                    DataSource = destinations,
                    DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                    FlatStyle = FlatStyle.Flat
                };
                _dataGridView3.Columns.Insert(idx, destCombo);
                using (var g = _dataGridView3.CreateGraphics())
                {
                    var size = g.MeasureString("Marcin Krzosek - Selektyw", _dataGridView3.Font);
                    _dataGridView3.Columns["destination"].MinimumWidth = size.ToSize().Width + 30;
                }
            }

            if (dt.Columns.Contains("package") && _dataGridView3.Columns["package"] != null)
            {
                int pidx = _dataGridView3.Columns["package"].Index;
                _dataGridView3.Columns.Remove("package");

                var packageCol = new DataGridViewTextBoxColumn
                {
                    Name = "package",
                    HeaderText = "Paczka",
                    DataPropertyName = "package",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                };

                _dataGridView3.Columns.Insert(pidx, packageCol);
                _dataGridView3.Columns["package"].ReadOnly = false;
                _dataGridView3.Columns["package"].SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            if (dt.Columns.Contains("transferred") && _dataGridView3.Columns["transferred"] != null)
            {
                _dataGridView3.Columns["transferred"].Visible = false;
            }

            if (_dataGridView3.Columns["name"] != null)
                _dataGridView3.Columns["name"].DisplayIndex = 0;
            if (_dataGridView3.Columns["progress"] != null)
                _dataGridView3.Columns["progress"].DisplayIndex = 1;
            if (_dataGridView3.Columns["last_update_done"] != null)
                _dataGridView3.Columns["last_update_done"].DisplayIndex = 2;
            if (_dataGridView3.Columns["created_at"] != null)
                _dataGridView3.Columns["created_at"].DisplayIndex = 3;

            if (!string.IsNullOrEmpty(_listsGridFilter))
            {
                try
                {
                    var view = dt.DefaultView;
                    view.RowFilter = _listsGridFilter;
                }
                catch
                {
                    _listsGridFilter = null;
                }
            }

            _dataGridView3.Sorted -= DataGridView3_Sorted;
            _dataGridView3.Sorted += DataGridView3_Sorted;
            
            _dataGridView3.SelectionChanged -= DataGridView3_SelectionChanged;
            _dataGridView3.SelectionChanged += DataGridView3_SelectionChanged;

            
            _dataGridView3.SortCompare -= DataGridView3_SortCompare;
            _dataGridView3.SortCompare += DataGridView3_SortCompare;

            var sortColumn = _dataGridView3.Columns[_lastSortedColumn ?? "maxEndDate"];
            if (sortColumn != null)
            {
                _dataGridView3.Sort(sortColumn,
                    _lastSortDirection == ListSortDirection.Ascending
                        ? ListSortDirection.Ascending
                        : ListSortDirection.Descending);
            }
            
            if (_lastSelectedListRow != null)
            {
                string id = _lastSelectedListRow["id"]?.ToString() ?? "";
                foreach (DataGridViewRow dgRow in _dataGridView3.Rows)
                {
                    if (dgRow.DataBoundItem is DataRowView drv && drv["id"]?.ToString() == id)
                    {
                        dgRow.Selected = true;
                        try { _dataGridView3.FirstDisplayedScrollingRowIndex = Math.Max(0, dgRow.Index - 2); } catch { }
                        break;
                    }
                }
                _lastSelectedListRow = null;
            }

            _dataGridView3.CellMouseEnter -= DataGridView3_CellMouseEnter;
            _dataGridView3.CellMouseEnter += DataGridView3_CellMouseEnter;

            _dataGridView3.CellPainting -= DataGridView3_CellPainting;
            _dataGridView3.CellPainting += DataGridView3_CellPainting;
            
            _dataGridView3.CellMouseLeave -= DataGridView3_CellMouseLeave;
            _dataGridView3.CellMouseLeave += DataGridView3_CellMouseLeave;
            
            if (_textBoxSearchField != null)
            {
                _textBoxSearchField.TextChanged -= TextBoxSearchField_TextChanged;
                _textBoxSearchField.TextChanged += TextBoxSearchField_TextChanged;
            }

            _dataGridView3.CurrentCellDirtyStateChanged -= DataGridView3_CurrentCellDirtyStateChanged;
            _dataGridView3.CurrentCellDirtyStateChanged += DataGridView3_CurrentCellDirtyStateChanged;

            _dataGridView3.CellValueChanged -= DataGridView3_CellValueChanged;
            _dataGridView3.CellValueChanged += DataGridView3_CellValueChanged;
            _dataGridView3.EditingControlShowing -= DataGridView3_EditingControlShowing;
            _dataGridView3.EditingControlShowing += DataGridView3_EditingControlShowing;
        }

        private void DataGridView3_SortCompare(object? sender, DataGridViewSortCompareEventArgs e)
        {
            if (e.Column.Name == "maxEndDate")
            {
                var val1 = e.CellValue1 == DBNull.Value || e.CellValue1 == null ? null : (DateTime?)e.CellValue1;
                var val2 = e.CellValue2 == DBNull.Value || e.CellValue2 == null ? null : (DateTime?)e.CellValue2;
                if (val1 == null && val2 == null)
                {
                    e.SortResult = 0;
                }
                else if (val1 == null)
                {
                    e.SortResult = 1; // nulls last
                }
                else if (val2 == null)
                {
                    e.SortResult = -1; // nulls last
                }
                else
                {
                    e.SortResult = DateTime.Compare(val1.Value, val2.Value);
                }
                e.Handled = true;
            }
        }

        public class DataGridViewCalendarColumn : DataGridViewColumn
        {
            public DataGridViewCalendarColumn() : base(new DataGridViewCalendarCell()) { }

            public override DataGridViewCell CellTemplate
            {
                get => base.CellTemplate;
                set
                {
                    if (value != null && !value.GetType().IsAssignableFrom(typeof(DataGridViewCalendarCell)))
                        throw new InvalidCastException("Must be a DataGridViewCalendarCell");
                    base.CellTemplate = value;
                }
            }
        }

        public class DataGridViewCalendarCell : DataGridViewTextBoxCell
        {
            public DataGridViewCalendarCell() : base()
            {
                this.Style.Format = "dd-MM-yyyy";
            }

            public override Type EditType => typeof(DataGridViewDateTimePickerEditingControl);

            public override Type ValueType => typeof(DateTime);

            public override object DefaultNewRowValue => DateTime.Now;

            public override void InitializeEditingControl(int rowIndex, object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
            {
                base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);
                var ctl = DataGridView.EditingControl as DataGridViewDateTimePickerEditingControl;
                if (ctl != null)
                {
                    DateTime current = DateTime.Now;
                    if (this.Value != null && this.Value != DBNull.Value)
                    {
                        DateTime.TryParse(this.Value.ToString(), out current);
                    }
                    ctl.Value = current;
                }
            }
        }

        public class DataGridViewDateTimePickerEditingControl : DateTimePicker, IDataGridViewEditingControl
        {
            DataGridView? dataGridView;
            private bool valueChanged = false;
            int rowIndex;

            public DataGridViewDateTimePickerEditingControl()
            {
                this.Format = DateTimePickerFormat.Short;
            }

            public object? EditingControlFormattedValue
            {
                get => this.Value.ToString("dd-MM-yyyy");
                set
                {
                    if (value is string s && DateTime.TryParse(s, out DateTime dt))
                        this.Value = dt;
                }
            }

            public object? GetEditingControlFormattedValue(DataGridViewDataErrorContexts context) => EditingControlFormattedValue;

            public void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle)
            {
                this.Font = dataGridViewCellStyle.Font;
            }

            public int EditingControlRowIndex { get => rowIndex; set => rowIndex = value; }

            public bool EditingControlValueChanged { get => valueChanged; set => valueChanged = value; }

            public Cursor EditingPanelCursor => base.Cursor;

            public bool RepositionEditingControlOnValueChange => false;

            public void PrepareEditingControlForEdit(bool selectAll) { }

            public DataGridView EditingControlDataGridView { get => dataGridView!; set => dataGridView = value; }

            public bool EditingControlWantsInputKey(Keys key, bool dataGridViewWantsInputKey)
            {
                return true;
            }

            protected override void OnValueChanged(EventArgs eventargs)
            {
                valueChanged = true;
                this.EditingControlDataGridView?.NotifyCurrentCellDirty(true);
                base.OnValueChanged(eventargs);
            }
        }

        private void DataGridView3_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
        {
            if (_dataGridView3 == null || !_dataGridView3.IsCurrentCellDirty) return;

            var cell = _dataGridView3.CurrentCell;
            if (cell == null) return;

            if (cell is DataGridViewCheckBoxCell )
            {
                _dataGridView3.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void DataGridView3_EditingControlShowing(object? sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (_dataGridView3 == null || _dataGridView3.CurrentCell == null) return;
            var col = _dataGridView3.CurrentCell.OwningColumn;
            if (col == null || col.Name != "package") return;

            if (e.Control is TextBox tb)
            {
                tb.MaxLength = 10;
            }
        }

        private async void DataGridView3_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (_dataGridView3 == null || e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var colName = _dataGridView3.Columns[e.ColumnIndex].Name;
            var row = _dataGridView3.Rows[e.RowIndex];
            var idCell = row.Cells["id"]?.Value;
            if (idCell == null || idCell == DBNull.Value) return;

            int reservationId = Convert.ToInt32(idCell);
            _lastSelectedListRow = _dataGridView3.Rows[e.RowIndex].DataBoundItem as DataRowView;

            try
            {
                if (colName == "realizeFlag")
                {
                    var val = row.Cells[e.ColumnIndex].Value;
                    bool newVal = val != null && val != DBNull.Value && Convert.ToBoolean(val);
                    var res = await _enterpriseDatabase.UpdateReservationMetadataAsync(reservationId, realizeFlag: newVal);
                    if (!res.IsSuccess)
                        ShowError("Błąd", $"Aktualizacja nie powiodła się: {res.ErrorMessage}");
                }
                else if (colName == "maxEndDate")
                {
                    var val = row.Cells[e.ColumnIndex].Value;
                    DateTime? newDate = null;
                    if (val != null && val != DBNull.Value && DateTime.TryParse(val.ToString(), out var dt))
                        newDate = dt;
                    var res = await _enterpriseDatabase.UpdateReservationMetadataAsync(reservationId, maxEndDate: newDate);
                    if (!res.IsSuccess)
                        ShowError("Błąd", $"Aktualizacja nie powiodła się: {res.ErrorMessage}");
                }
                else if (colName == "assignedPerson")
                {
                    var val = row.Cells[e.ColumnIndex].Value?.ToString();
                    var res = await _enterpriseDatabase.UpdateReservationMetadataAsync(reservationId, assignedPerson: val);
                    if (!res.IsSuccess)
                        ShowError("Błąd", $"Aktualizacja nie powiodła się: {res.ErrorMessage}");
                }
                else if (colName == "destination")
                {
                    var val = row.Cells[e.ColumnIndex].Value?.ToString();
                    var res = await _enterpriseDatabase.UpdateReservationMetadataAsync(reservationId, destination: val);
                    if (!res.IsSuccess)
                        ShowError("Błąd", $"Aktualizacja nie powiodła się: {res.ErrorMessage}");
                }else if (colName == "package")
                {
                    var val = row.Cells[e.ColumnIndex].Value?.ToString();
                    var res = await _enterpriseDatabase.UpdateReservationMetadataAsync(reservationId, package: val);
                    if (!res.IsSuccess)
                        ShowError("Błąd", $"Aktualizacja nie powiodła się: {res.ErrorMessage}");
                }

                _presenter?.Initialize();
            }
            catch (Exception ex)
            {
                ShowError("Błąd", ex.Message);
            }
        }

        private void DataGridView3_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (_dataGridView3 == null || e.RowIndex < 0 || e.ColumnIndex < 0) return;

            string colName = _dataGridView3.Columns[e.ColumnIndex].Name;
            
            if (colName != "last_update_done")
                return;

            var cellValue = _dataGridView3.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            if (cellValue == null || cellValue == DBNull.Value || string.IsNullOrEmpty(cellValue.ToString()))
                return;

            var idCell = _dataGridView3.Rows[e.RowIndex].Cells["id"]?.Value;
            if (idCell == null) return;
            
            int reservationId = Convert.ToInt32(idCell);
            string lastValue = cellValue.ToString() ?? "0";

            var result = MessageBox.Show(
                $"Czy na pewno chcesz cofnąć ostatnią aktualizację\n\nWartość {lastValue} zostanie przywrócona.",
                "Potwierdź cofnięcie",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ReverseLastUpdateTHTRequested?.Invoke(this, new ReverseLastUpdateTHTEventArgs(reservationId));
            }
        }

        public void BindAdditionalMaterialsGrid(DataTable dt)
        {
            if (_dataGridView4 == null) return;

            _dataGridView4.DataSource = dt;

            if (dt.Columns.Contains("id") && _dataGridView4.Columns["id"] != null)
                _dataGridView4.Columns["id"].Visible = false;

            if (dt.Columns.Contains("name") && _dataGridView4.Columns["name"] != null)
            {
                _dataGridView4.Columns["name"].HeaderText = "Nazwa";
                _dataGridView4.Columns["name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }

            if (dt.Columns.Contains("start") && _dataGridView4.Columns["start"] != null)
            {
                _dataGridView4.Columns["start"].HeaderText = "Ilość";
                _dataGridView4.Columns["start"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }

            if (dt.Columns.Contains("created_at") && _dataGridView4.Columns["created_at"] != null)
            {
                _dataGridView4.Columns["created_at"].HeaderText = "Data utworzenia";
                _dataGridView4.Columns["created_at"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
        }

        private void DataGridView1_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (_dataGridView1 == null || e.RowIndex < 0) return;

            var row = _dataGridView1.Rows[e.RowIndex];
            var cellValue = row.Cells["is_one_sided"]?.Value;
            
            // Obsłuż wartości DBNull i null
            bool isOneSided = cellValue != null && 
                             cellValue != DBNull.Value && 
                             Convert.ToBoolean(cellValue);

            if (!isOneSided) return;

            string colName = _dataGridView1.Columns[e.ColumnIndex].Name;

            // Formatowanie postępu TOP i BOT dla jednostronnych
            if (colName == "progress_TOP" || colName == "last_update_done_top")
            {
                e.CellStyle.ForeColor = Color.Gray;
                e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Italic);
                
                // Dodatkowo dodaj "= wartość" do ostatniej aktualizacji TOP
                if (e.Value != null && !string.IsNullOrEmpty(e.Value.ToString()))
                {
                    e.Value = $"= {e.Value}";
                    e.FormattingApplied = true;
                }
            }
        }

        private void DataGridView1_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (_dataGridView1 == null || e.RowIndex < 0 || e.ColumnIndex < 0) return;

            string colName = _dataGridView1.Columns[e.ColumnIndex].Name;
            
            // Tylko dla kolumn last_update
            if (colName != "last_update_done_bot" && colName != "last_update_done_top")
                return;

            var cellValue = e.Value;
            if (cellValue == null || cellValue == DBNull.Value || string.IsNullOrEmpty(cellValue.ToString()))
                return;

            // Maluj domyślną komórkę
            e.Paint(e.CellBounds, DataGridViewPaintParts.All);

            // Rysuj ikonę przywracania (↶)
            string restoreIcon = "↶";
            using (Font iconFont = new Font(_dataGridView1.Font.FontFamily, 12, FontStyle.Bold))
            using (Brush iconBrush = new SolidBrush(Color.Blue))
            {
                string cellText = cellValue.ToString() ?? "";
                SizeF textSize = e.Graphics.MeasureString(cellText, _dataGridView1.Font);
                SizeF iconSize = e.Graphics.MeasureString(restoreIcon, iconFont);
                
                // Pozycjonuj ikonę na prawym brzegu komórki
                float iconX = e.CellBounds.Right - iconSize.Width - 5;
                float iconY = e.CellBounds.Y + (e.CellBounds.Height - iconSize.Height) / 2;
                
                e.Graphics.DrawString(restoreIcon, iconFont, iconBrush, iconX, iconY);
            }

            e.Handled = true;
        }

        private void DataGridView1_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (_dataGridView1 == null || e.RowIndex < 0 || e.ColumnIndex < 0) return;

            string colName = _dataGridView1.Columns[e.ColumnIndex].Name;
            
            // Tylko obsługuj kliknięcia w kolumnach last_update
            if (colName != "last_update_done_bot" && colName != "last_update_done_top")
                return;

            var cellValue = _dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            if (cellValue == null || cellValue == DBNull.Value || string.IsNullOrEmpty(cellValue.ToString()))
                return;

            // Pobierz ID rezerwacji
            var idCell = _dataGridView1.Rows[e.RowIndex].Cells["id"]?.Value;
            if (idCell == null) return;
            
            int reservationId = Convert.ToInt32(idCell);
            string side = colName == "last_update_done_bot" ? "BOT" : "TOP";
            string lastValue = cellValue.ToString() ?? "0";

            // Potwierdź cofnięcie
            var result = MessageBox.Show(
                $"Czy na pewno chcesz cofnąć ostatnią aktualizację strony {side}?\n\nWartość {lastValue} zostanie przywrócona.",
                "Potwierdź cofnięcie",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Wywołaj cofnięcie aktualizacji dla konkretnej strony
                ReverseLastUpdateRequested?.Invoke(this, new ReverseLastUpdateEventArgs(reservationId, side == "BOT" ? IReservationRepository.Side.BOT : IReservationRepository.Side.TOP));
            }
        }

        private void DataGridView1_CellMouseEnter(object? sender, DataGridViewCellEventArgs e)
        {
            if (_dataGridView1 == null || e.RowIndex < 0 || e.ColumnIndex < 0) return;

            string colName = _dataGridView1.Columns[e.ColumnIndex].Name;
            
            // Zmień kursor dla kolumn last_update, które mają wartości
            if (colName == "last_update_done_bot" || colName == "last_update_done_top")
            {
                var cellValue = _dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                if (cellValue != null && cellValue != DBNull.Value && !string.IsNullOrEmpty(cellValue.ToString()))
                {
                    _dataGridView1.Cursor = Cursors.Hand;
                }
            }
        }

        private void DataGridView3_CellMouseEnter(object? sender, DataGridViewCellEventArgs e)
        {
            if (_dataGridView3 == null || e.RowIndex < 0 || e.ColumnIndex < 0) return;

            string colName = _dataGridView3.Columns[e.ColumnIndex].Name;
            
            // Change cursor for last_update columns that have values
            if (colName == "last_update_done")
            {
                var cellValue = _dataGridView3.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                if (cellValue != null && cellValue != DBNull.Value && !string.IsNullOrEmpty(cellValue.ToString()))
                {
                    _dataGridView3.Cursor = Cursors.Hand;
                }
            }
        }

        private void DataGridView1_CellMouseLeave(object? sender, DataGridViewCellEventArgs e)
        {   // Przywróć domyślny kursor po opuszczeniu komórki
            if (_dataGridView1 == null) return;
            _dataGridView1.Cursor = Cursors.Default;    
        }

        private void DataGridView3_CellMouseLeave(object? sender, DataGridViewCellEventArgs e)
        {
            if (_dataGridView3 == null) return;
            _dataGridView3.Cursor = Cursors.Default;
        }
        
        private void ButtonEditReservation_Click(object? sender, EventArgs e)
        {
            if (_dataGridView1?.SelectedCells.Count == 0)
            {
                ShowError("Błąd", "Wybierz listę do edycji.");
                return;
            }

            if (_dataGridView1 != null && _dataGridView1.SelectedCells.Count > 0)
            {
                var cellValue = _dataGridView1.Rows[_dataGridView1.SelectedCells[0].RowIndex].Cells["id"].Value;
                if (cellValue != null)
                {  
                    var isOneSidedCell = _dataGridView1.Rows[_dataGridView1.SelectedCells[0].RowIndex].Cells["is_one_sided"].Value;
                    bool isOneSided = isOneSidedCell != DBNull.Value && Convert.ToBoolean(isOneSidedCell);
                    int reservationId = Convert.ToInt32(cellValue);
                    var editReservationListForm = new Views.EditReservationList(_enterpriseDatabase, reservationId, isOneSided);
                    editReservationListForm.ShowDialog();
                }
            }
        }

        private void DataGridView3_SelectionChanged(object? sender, EventArgs e)
        {
            ValidateComboBoxOptions();
        }

        private void DataGridView3_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (_dataGridView3 == null || e.RowIndex < 0 || e.ColumnIndex < 0) return;

            string colName = _dataGridView3.Columns[e.ColumnIndex].Name;
            
            // Only for last_update columns
            if (colName != "last_update_done")
                return;

            var cellValue = e.Value;
            if (cellValue == null || cellValue == DBNull.Value || string.IsNullOrEmpty(cellValue.ToString()))
                return;

            // Paint default cell
            e.Paint(e.CellBounds, DataGridViewPaintParts.All);

            // Draw restore icon (↶)
            string restoreIcon = "↶";
            using (Font iconFont = new Font(_dataGridView3.Font.FontFamily, 12, FontStyle.Bold))
            using (Brush iconBrush = new SolidBrush(Color.Blue))
            {
                // Measure text to position icon at the right
                string cellText = cellValue.ToString() ?? "";
                SizeF textSize = e.Graphics.MeasureString(cellText, _dataGridView3.Font);
                SizeF iconSize = e.Graphics.MeasureString(restoreIcon, iconFont);
                
                // Position icon at the right edge of the cell
                float iconX = e.CellBounds.Right - iconSize.Width - 5;
                float iconY = e.CellBounds.Y + (e.CellBounds.Height - iconSize.Height) / 2;
                
                e.Graphics.DrawString(restoreIcon, iconFont, iconBrush, iconX, iconY);
            }

            e.Handled = true;
        }

        private void ButtonGenerateList_Click(object? sender, EventArgs e)
        {
            if (_tabControl3.SelectedTab == _listyTHTTabPage) // Sprawdź czy jest to zakładka THT
            {
                if (_dataGridView3?.SelectedRows.Count == 0) // Czy wybrana lista
                {
                    ShowError("Błąd", "Wybierz listę!");
                    return;
                }

                using var ofd = new SaveFileDialog();
                ofd.Filter = "Excel files|*.xlsx;*.xls";

                string name = _dataGridView3.SelectedRows[0].Cells["name"].Value?.ToString()?.Replace(' ', '_').Replace('.', '_') ?? "Lista_rezerwacji";
                ofd.FileName = $"Lista_wybraniowa_{name}.xlsx";

                if (ofd.ShowDialog() != DialogResult.OK)
                    return;

                var cellValue = _dataGridView3.SelectedRows[0].Cells["id"].Value;
                if (cellValue != null)
                {
                    var start = _dataGridView3.SelectedRows[0].Cells["start"].Value.ToString();
                    if(start == null) start = "1";
                    int reservationId = Convert.ToInt32(cellValue);
                    GenerateListTHTRequested?.Invoke(this, new GenerateListTHTEventArgs(ofd.FileName, reservationId, start));//wywołanie metody bez zmiennej start powoduje spłycenie listy wybraniowej do 1 płytki
                }
            }else // Standardowa zakładka Listy SMD
            {
                if (_dataGridView1?.SelectedRows.Count == 0)
                {
                    ShowError("Błąd", "Wybierz listę!");
                    return;
                }

                using var ofd = new SaveFileDialog();
                ofd.Filter = "Excel files|*.xlsx;*.xls";

                string name = _dataGridView1.SelectedRows[0].Cells["name"].Value?.ToString()?.Replace(' ', '_').Replace('.', '_') ?? "Lista_rezerwacji";
                ofd.FileName = $"Lista_wybraniowa_{name}.xlsx";

                if (ofd.ShowDialog() != DialogResult.OK)
                    return;

                var cellValue = _dataGridView1.SelectedRows[0].Cells["id"].Value;
                if (cellValue != null)
                {
                    var start = _dataGridView1.SelectedRows[0].Cells["start"].Value;
                    if(start == null) start = 1;
                    int reservationId = Convert.ToInt32(cellValue);
                    GenerateListRequested?.Invoke(this, new GenerateListEventArgs(ofd.FileName, reservationId, _dataGridView1.SelectedRows[0].Cells["is_one_sided"].Value != DBNull.Value && Convert.ToBoolean( _dataGridView1.SelectedRows[0].Cells["is_one_sided"].Value), Convert.ToInt32(start)));
                }
            }
        }

        private void GenerateListWithMultiplier(int multiplier)
        {
            if (_tabControl3.SelectedTab == _listyTHTTabPage) // Sprawdź czy jest to zakładka THT
            {
                if (_dataGridView3?.SelectedRows.Count == 0) // Czy wybrana lista
                {
                    ShowError("Błąd", "Wybierz listę!");
                    return;
                }

                using var ofd = new SaveFileDialog();
                ofd.Filter = "Excel files|*.xlsx;*.xls";

                string name = _dataGridView3.SelectedRows[0].Cells["name"].Value?.ToString()?.Replace(' ', '_').Replace('.', '_') ?? "Lista_rezerwacji";
                ofd.FileName = $"Lista_wybraniowa_{name}.xlsx";

                if (ofd.ShowDialog() != DialogResult.OK)
                    return;

                var cellValue = _dataGridView3.SelectedRows[0].Cells["id"].Value;
                if (cellValue != null)
                {
                    int reservationId = Convert.ToInt32(cellValue);
                    GenerateListTHTRequested?.Invoke(this, new GenerateListTHTEventArgs(ofd.FileName, reservationId, multiplier.ToString()));
                }
            }else // Standardowa zakładka Listy SMD
            {
                if (_dataGridView1?.SelectedRows.Count == 0)
                {
                    ShowError("Błąd", "Wybierz listę!");
                    return;
                }

                using var ofd = new SaveFileDialog();
                ofd.Filter = "Excel files|*.xlsx;*.xls";

                string name = _dataGridView1.SelectedRows[0].Cells["name"].Value?.ToString()?.Replace(' ', '_').Replace('.', '_') ?? "Lista_rezerwacji";
                ofd.FileName = $"Lista_wybraniowa_{name}.xlsx";

                if (ofd.ShowDialog() != DialogResult.OK)
                    return;

                var cellValue = _dataGridView1.SelectedRows[0].Cells["id"].Value;
                if (cellValue != null)
                {
                    int reservationId = Convert.ToInt32(cellValue);
                    GenerateListRequested?.Invoke(this, new GenerateListEventArgs(ofd.FileName, reservationId, _dataGridView1.SelectedRows[0].Cells["is_one_sided"].Value != DBNull.Value && Convert.ToBoolean( _dataGridView1.SelectedRows[0].Cells["is_one_sided"].Value), multiplier));
                }
            }
        }

        private void ButtonDeleteReservation_Click(object? sender, EventArgs e)
        {
            if (_dataGridView1?.SelectedCells.Count > 0) // Lista SMD
            {
                var cellValue = _dataGridView1.Rows[_dataGridView1.SelectedCells[0].RowIndex].Cells["id"].Value;
                if (cellValue != null)
                {
                    var sel = new
                    {
                        Id = Convert.ToInt32(cellValue),
                        IsList = true
                    };
                    DeleteReservationRequested?.Invoke(this, new DeleteReservationEventArgs(sel.Id, sel.IsList));
                }
            }
            else if (_dataGridView2?.SelectedCells.Count > 0) // Pojedynczy komponent
            {
                var cellValue = _dataGridView2.Rows[_dataGridView2.SelectedCells[0].RowIndex].Cells["id"].Value;
                if (cellValue != null)
                {
                    var sel = new
                    {
                        Id = Convert.ToInt32(cellValue),
                        IsList = false
                    };
                    DeleteReservationRequested?.Invoke(this, new DeleteReservationEventArgs(sel.Id, sel.IsList));
                }
            }
            else if (_dataGridView4?.SelectedCells.Count > 0) // Lista THT
            {
                var cellValue = _dataGridView4.Rows[_dataGridView4.SelectedCells[0].RowIndex].Cells["id"].Value;
                if (cellValue != null)
                {
                    var sel = new
                    {
                        Id = Convert.ToInt32(cellValue),
                        IsList = true
                    };
                    DeleteAdditionalMaterialRequested?.Invoke(this, new ListsSelectedEventArgs(sel.Id));
                }
            }
            else // Nic nie wybrano
            {
                ShowError("Błąd", "Wybierz element z listy.");
            }
        }

        private void ButtonUpdateListDone_Click(object? sender, EventArgs e)
        {
            if (_dataGridView1?.SelectedRows.Count == 0) // Sprawdź czy wybrano zamówienie
            {
                ShowError("Błąd", "Wybierz zamówienie!");
                return;
            }
            if (!int.TryParse(_textBox3?.Text.Trim(), out int addValue)) // Sprawdź poprawność liczby
            {
                ShowError("Błąd", "Podaj poprawną liczbę!");
                return;
            }
            if( addValue <= 0) // Sprawdź czy liczba jest większa od zera
            {
                ShowError("Błąd", "Podaj liczbę większą od zera!");
                return;
            }

            if (_dataGridView1 != null && _dataGridView1.SelectedRows.Count > 0)
            {
                var cellValue = _dataGridView1.SelectedRows[0].Cells["id"].Value;
                if (cellValue != null)
                {
                    int reservationId = Convert.ToInt32(cellValue);
                    if (_comboBoxSide == null || _comboBoxSide.SelectedItem == null) // Sprawdź czy wybrano stronę aktualizacji
                    {
                        ShowError("Błąd", "Wybierz stronę aktualizacji!");
                        return;
                    }
                    UpdateListDoneRequested?.Invoke(this, new UpdateListDoneEventArgs(reservationId, addValue, 
                                _comboBoxSide.SelectedItem.ToString() == "OBIE" ? IReservationRepository.Side.SINGLE :
                                _comboBoxSide.SelectedItem.ToString() == "TOP" ? IReservationRepository.Side.TOP :
                                IReservationRepository.Side.BOT));
                    
                    // Wyczyść pole tekstowe po udanej aktualizacji
                    if (_textBox3 != null)
                        _textBox3.Text = "";
                }
                else
                {
                    ShowError("Błąd", "Nieprawidłowe dane w zaznaczonym wierszu!");
                }
            }
        }

        private void TabControl3_SelectedIndexChanged(object? sender, EventArgs e)
        {
            _listsTHTGridFilter = null; // Wyczyść filtr list THT
            _listsGridFilter = null; // Wyczyść filtr list SMD
            _itemsGridFilter = null; // Wyczyść filtr komponentów
            if (_dataGridView1 != null && _dataGridView2 != null)
            {
                _dataGridView1.ClearSelection();
                _dataGridView2.ClearSelection();
            }

            if (_tabControl3?.SelectedTab == _listyTabPage) // Zakładka Listy SMD
            {
                _buttonUpdateListDone.Click -= _currentUpdateReservationHandler;
                _currentUpdateReservationHandler = ButtonUpdateListDone_Click;
                _buttonUpdateListDone.Click += _currentUpdateReservationHandler;
                EnableListButtons(true);
                UpdateReservationHandler(true);
                _comboBoxSide.Enabled = true;
                TabListsSelected?.Invoke(this, EventArgs.Empty);
                // Przywróć referencję do tabeli SMD dla filtra wyszukiwania
                _currentListsDataTable = _dataGridView1?.DataSource as DataTable;
            }
            else if (_tabControl3?.SelectedTab == _singleCompsTabPage) // Zakładka Pojedyncze komponenty
            {
                EnableListButtons(false);
                UpdateReservationHandler(false);
                _buttonEditReservation.Enabled = false;
                _comboBoxSide.Enabled = false;
                _buttonUpdateListDone.Enabled = false;
                TabItemsSelected?.Invoke(this, EventArgs.Empty);
                // Przywróć referencję do tabeli komponentów dla filtra wyszukiwania
                _currentListsDataTable = _dataGridView2?.DataSource as DataTable;
            }else if (_tabControl3?.SelectedTab == _listyTHTTabPage) // Zakładka Listy THT
            {
                _lastSortedColumn = null; // Resetuj informacje o sortowaniu
                _lastSortDirection = ListSortDirection.Descending;

                EnableListButtons(false);
                TabListsTHTSelected?.Invoke(this, EventArgs.Empty);
                UpdateReservationHandler(false);
                _buttonUpdateListDone.Click -= _currentUpdateReservationHandler;
                _currentUpdateReservationHandler = (s, e) =>
                {
                    if (_dataGridView3?.SelectedRows.Count == 0)
                    {
                        ShowError("Błąd", "Wybierz zamówienie!");
                        return;
                    }
                    var a = new THTUpdateView(_dataGridView3.SelectedRows[0].Cells["id"].Value != null ? Convert.ToInt32(_dataGridView3.SelectedRows[0].Cells["id"].Value) : 0, _enterpriseDatabase);
                            a.ShowDialog();
                    OnTabSelected();
                };
                _buttonUpdateListDone.Click += _currentUpdateReservationHandler;
                _comboBoxSide.Enabled = false;
                _buttonDeleteReservation.Enabled = false;
                _buttonGenerateList.Enabled = true;
                _buttonEditReservation.Enabled = false;

                // Przywróć referencję do tabeli THT dla filtra wyszukiwania
                _currentListsDataTable = _dataGridView3?.DataSource as DataTable;
            }else if (_tabControl3?.SelectedTab == _AdditionalMaterialsTabPage)
            {
                TabAdditionalMaterialsSelected?.Invoke(this, EventArgs.Empty);
                _buttonUpdateListDone.Click -= _currentUpdateReservationHandler;
                _currentUpdateReservationHandler = (s, ev) =>
                {
                    UpdateAdditionalMaterialsRequested?.Invoke(this, 
                    new ListsSelectedEventArgs(_dataGridView4!.SelectedRows[0].Cells["id"].Value != null ? 
                                                Convert.ToInt32(_dataGridView4.SelectedRows[0].Cells["id"].Value) : 0));
                };
                _buttonUpdateListDone.Click += _currentUpdateReservationHandler;

                _buttonCreateReservation.Click -= _reservationCreatedHandler;
                _reservationCreatedHandler = (s, ev) =>
                {
                    var createAdditionalMaterialForm = new Views.CreateAdditionalMaterialView(_enterpriseDatabase, _fileProcessingService);
                    createAdditionalMaterialForm.ShowDialog();
                    TabAdditionalMaterialsSelected?.Invoke(this, EventArgs.Empty);

                };
                _buttonCreateReservation.Click += _reservationCreatedHandler;
                EnableListButtons(false);
                _comboBoxSide.Enabled = false;
            }

            _textBoxSearchField.Clear();
        }

        private void EnableListButtons(bool enable) // Helper do włączania/wyłączania przycisków w zakładce Listy
        {
            if (_buttonUpdateListDone != null)
                _buttonUpdateListDone.Enabled = enable;
            if (_textBox3 != null)
                _textBox3.Enabled = enable;
            if (_buttonGenerateList != null)
                _buttonGenerateList.Enabled = enable;
            if (_buttonEditReservation != null)
                _buttonEditReservation.Enabled = enable;
            if (_buttonDeleteReservation != null)
                _buttonDeleteReservation.Enabled = true;
            if (_buttonCreateReservation != null)
                _buttonCreateReservation.Enabled = true;
            if (_buttonUpdateListDone != null)
                _buttonUpdateListDone.Enabled = true;
        }

        private void UpdateReservationHandler(bool isList) // Helper do aktualizacji obsługi zdarzenia tworzenia rezerwacji
        {
            if (_buttonCreateReservation == null || _reservationCreatedHandler == null) return;

            _buttonCreateReservation.Click -= _reservationCreatedHandler;

            if (isList)
            {
                _reservationCreatedHandler = (s, e) =>
                {
                    var createReservationListForm = new Views.CreateReservationList(_enterpriseDatabase, _fileProcessingService);
                    createReservationListForm.ShowDialog();
                    TabListsSelected?.Invoke(this, EventArgs.Empty);
                };
            }
            else 
            {
                _reservationCreatedHandler = (s, e) =>
                {
                    var createReservationForm = new Views.CreateReservationSingle(_enterpriseDatabase);
                    createReservationForm.ShowDialog();
                    TabListsSelected?.Invoke(this, EventArgs.Empty);
                };
            }

            _buttonCreateReservation.Click += _reservationCreatedHandler;
        }

        private void DataGridView1_Sorted(object? sender, EventArgs e)
        {
            if (_dataGridView1?.SortedColumn != null)
            {
                var col = _dataGridView1.SortedColumn;
                _lastSortedColumn = col.Name;
                _lastSortDirection = col.HeaderCell.SortGlyphDirection == SortOrder.Ascending
                    ? ListSortDirection.Ascending
                    : ListSortDirection.Descending;
            }
        }

        private void DataGridView3_Sorted(object? sender, EventArgs e)
        {
            if (_dataGridView3?.SortedColumn != null)
            {
                var col = _dataGridView3.SortedColumn;
                _lastSortedColumn = col.Name;
                _lastSortDirection = col.HeaderCell.SortGlyphDirection == SortOrder.Ascending
                    ? ListSortDirection.Ascending
                    : ListSortDirection.Descending;
            }
        }

        private void DataGridView1_CellToolTipTextNeeded(object? sender, DataGridViewCellToolTipTextNeededEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && _dataGridView1 != null)
            {
                var row = _dataGridView1.Rows[e.RowIndex];
                var colName = _dataGridView1.Columns[e.ColumnIndex].Name;
                var isOneSidedValue = row.Cells["is_one_sided"]?.Value;
                
                bool isOneSided = isOneSidedValue != null && 
                                 isOneSidedValue != DBNull.Value && 
                                 Convert.ToBoolean(isOneSidedValue);

                // Podpowiedź dla cofnięcia aktualizacji
                if (colName == "last_update_done_bot" || colName == "last_update_done_top")
                {
                    var cellValue = row.Cells[e.ColumnIndex].Value;
                    if (cellValue != null && cellValue != DBNull.Value && !string.IsNullOrEmpty(cellValue.ToString()))
                    {
                        string side = colName == "last_update_done_bot" ? "BOT" : "TOP";
                        e.ToolTipText = $"Kliknij ↶ aby cofnąć ostatnią aktualizację strony {side}";
                    }
                }
                else if (isOneSided && (colName == "progress_TOP" || colName == "last_update_done_top"))
                {
                    e.ToolTipText = "Płytka jednostronna - wartość taka sama jak BOT";
                }
                else if (e.ColumnIndex == 1)
                {
                    var cellValue = _dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                    e.ToolTipText = cellValue?.ToString();
                }
            }
        }

        private void TextBoxSearchField_TextChanged(object? sender, EventArgs e)
        {
            if (_currentListsDataTable == null || _textBoxSearchField == null) return;

            string search = _textBoxSearchField.Text.Trim().Replace("'", "''").ToLower();

            if (string.IsNullOrEmpty(search))
            {
                _currentListsDataTable.DefaultView.RowFilter = "";
                return;
            }

            var filters = new List<string>();
            foreach (DataColumn col in _currentListsDataTable.Columns)
            {
                filters.Add($"Convert([{col.ColumnName}], 'System.String') LIKE '%{search}%'");
            }

            _currentListsDataTable.DefaultView.RowFilter = string.Join(" OR ", filters);
        }

        protected override void Dispose(bool disposing)
        {
                if (_dataGridView1 != null)
                {
                    _dataGridView1.Sorted -= DataGridView1_Sorted;
                    _dataGridView1.CellToolTipTextNeeded -= DataGridView1_CellToolTipTextNeeded;
                }

                if (_buttonEditReservation != null)
                    _buttonEditReservation.Click -= ButtonEditReservation_Click;

                if (_generateListMenu != null)
                {
                    _generateListMenu.Dispose();
                    _generateListMenu = null;
                }

                if (_buttonDeleteReservation != null)
                    _buttonDeleteReservation.Click -= ButtonDeleteReservation_Click;

                if (_buttonUpdateListDone != null)
                    _buttonUpdateListDone.Click -= ButtonUpdateListDone_Click;

                if (_tabControl3 != null)
                    _tabControl3.SelectedIndexChanged -= TabControl3_SelectedIndexChanged;

                if (_dataGridView1 != null)
                {
                    _dataGridView1.Sorted -= DataGridView1_Sorted;
                    _dataGridView1.CellToolTipTextNeeded -= DataGridView1_CellToolTipTextNeeded;
                }

                if (_textBoxSearchField != null)
                    _textBoxSearchField.TextChanged -= TextBoxSearchField_TextChanged;

                if (_buttonCreateReservation != null && _reservationCreatedHandler != null)
                    _buttonCreateReservation.Click -= _reservationCreatedHandler;

                if (TabPage != null)
                    TabPage.Resize -= TabPage_Resize;
                _bottomPanel?.Dispose();

                _currentListsDataTable = null;
                _presenter?.Dispose();

            base.Dispose(disposing);
        }
        
    }
}        