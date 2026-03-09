using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using ExcelDataReader;
using MySql.Data.MySqlClient;
using WILK.Services;

namespace WILK.Views
{
    public partial class CreateReservationList : Form
    {
        protected readonly IEnterpriseDatabase _enterpriseDatabase;
        private readonly IFileProcessingService _fileProcessingService;

        protected string _excelName = string.Empty;
        protected List<(string Kol1, string Kol2, string Kol3)> _excelData = new();
        private bool _isFileValid = false;
        private bool _isQuantityValid = false;

        public CreateReservationList(IEnterpriseDatabase enterpriseDatabase, IFileProcessingService fileProcessingService)
        {
            _enterpriseDatabase = enterpriseDatabase;
            _fileProcessingService = fileProcessingService;
            InitializeComponent();

            // Keyboard shortcuts
            this.KeyPreview = true;
            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    Close();
                }
                else if (e.KeyCode == Keys.Enter && button1.Enabled)
                {
                    button1.PerformClick();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            };
            
            // Auto-focus first field
            this.Shown += (s, e) => textBox1.Focus();

            ButtonImportExcelList.Click += OnButtonImportExcelListClick;
            button1.Click += OnButtonSaveListClick;
            textBox1.TextChanged += OnQuantityTextChanged;
            
            // Initial state - disable save button until valid data is loaded
            button1.Enabled = false;
            radioDoubleSided.Checked = true;
            radioSingleSided.Checked = false;

            //debug.Click += (s, e) =>
            //{
                /*var ofd = new OpenFileDialog();
                ofd.Filter = "Excel files|*.xlsx;*.xls";
                if (ofd.ShowDialog() != DialogResult.OK)
                    return;
                
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                using var stream = File.Open(ofd.FileName, FileMode.Open, FileAccess.Read);
                using var reader = ExcelReaderFactory.CreateReader(stream);


                var comps =  new List<( int id, int quantity)>();

                reader.Read();
                var name = reader.GetValue(0)?.ToString() ?? string.Empty;
                reader.Read();
                while (reader.Read())
                {
                    var a = reader.GetValue(5)?.ToString() ?? string.Empty;
                    var b = reader.GetValue(6)?.ToString() ?? string.Empty;

                    if (string.IsNullOrWhiteSpace(a) && string.IsNullOrWhiteSpace(b))
                        break;
                    comps.Add((int.TryParse(a, out int id) ? id : 0, int.TryParse(b, out int qty) ? qty : 0));
                }

                comps = comps.Where(c => _enterpriseDatabase.GetComponentTypeAsync(_enterpriseDatabase.GetComponentIdByRIdAsync(c.id).Result.Data).Result.Data == "THT").ToList();
                if ( comps.Count == 0)
                {
                    MessageBox.Show("Brak komponentów THT w liście", "Debug", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                
                var connection = new MySqlConnection("server=vm-pcb-sql.mikronika.com.pl;user=pcb1;password=quaih8eiW2;database=pcb1;");
                connection.Open();
                var sql1 = @"INSERT INTO ListOfReservationsTHT (name, createdAt, start, status) VALUES (@name, NOW(), @start, @status); SELECT LAST_INSERT_ID();";
                using var command1 = new MySqlCommand(sql1, connection);
                command1.Parameters.AddWithValue("@name", name + "_THT");
                command1.Parameters.AddWithValue("@start", 56);
                command1.Parameters.AddWithValue("@status", "Zarezerwowane");
                var listId = Convert.ToInt64(command1.ExecuteScalar());

                foreach (var comp in comps)
                {
                    var sql = @"INSERT INTO ReservationsTHT (components_id, quantity, list_id, have_alternative) VALUES (@r_id, @quantity, @list_id, @have_alternative);";
                    using var command = new MySqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@r_id", comp.id);
                    command.Parameters.AddWithValue("@quantity", comp.quantity * 56);
                    command.Parameters.AddWithValue("@list_id", listId);
                    command.Parameters.AddWithValue("@have_alternative", false);
                    command.ExecuteNonQuery();
                }*/
                
           // };
        }

        protected void HideRadio()
        {
            radioDoubleSided.Visible = false;
            radioSingleSided.Visible = false;
        }

        protected int GetQuantity()
        {
            if (int.TryParse(textBox1.Text.Trim(), out int qty))
                return qty;
            return -1;
        }

        private void OnQuantityTextChanged(object? sender, EventArgs e)
        {
            _isQuantityValid = int.TryParse(textBox1.Text.Trim(), out int qty) && qty > 0;
            
            // Visual feedback
            if (textBox1 != null)
            {
                if (_isQuantityValid)
                    textBox1.BackColor = Color.LightGreen;
                else
                    textBox1.BackColor = string.IsNullOrEmpty(textBox1.Text) ? SystemColors.Window : Color.LightPink;
            }
            
            UpdateSaveButtonState();
        }

        private void UpdateSaveButtonState()
        {
            button1.Enabled = _isFileValid && _isQuantityValid;
        }

        private void OnButtonImportExcelListClick(object? s, EventArgs e)
        {
            using var ofd = new OpenFileDialog();
            ofd.Filter = "Excel files|*.xlsx;*.xls";
            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            lblLoading.Text = "Ładowanie pliku...";
            lblLoading.Visible = true;
            ButtonImportExcelList.Enabled = false;
            button1.Enabled = false;
            Application.DoEvents();

            try
            {
                (_excelName, _excelData) = _fileProcessingService.LoadList(ofd.FileName);
                
                // Validate data integrity
                if (string.IsNullOrWhiteSpace(_excelName))
                {
                    statusLabel.Text = "Błąd: Nazwa pliku jest pusta";
                    statusLabel.ForeColor = Color.Red;
                    _excelData.Clear();
                    return;
                }

                if (_excelData.Count == 0)
                {
                    statusLabel.Text = "Błąd: Plik nie zawiera danych";
                    statusLabel.ForeColor = Color.Red;
                    return;
                }

                // Validate each row
                var invalidRows = new List<string>();
                for (int i = 0; i < _excelData.Count; i++)
                {
                    var row = _excelData[i];
                    var rowNum = i + 1;

                    // Check if any field is empty
                    if (string.IsNullOrWhiteSpace(row.Kol1))
                    {
                        invalidRows.Add($"Wiersz {rowNum}: Pusta nazwa komponentu");
                    }

                    if (string.IsNullOrWhiteSpace(row.Kol2))
                    {
                        invalidRows.Add($"Wiersz {rowNum}: Puste ID komponentu");
                    }
                    else if (!int.TryParse(row.Kol2, out int componentId) || componentId <= 0)
                    {
                        invalidRows.Add($"Wiersz {rowNum}: Nieprawidłowe ID komponentu '{row.Kol2}' (musi być liczbą całkowitą > 0)");
                    }
                }

                if (invalidRows.Count > 0)
                {
                    statusLabel.Text = $"Błąd walidacji: {invalidRows.Count} błędów";
                    statusLabel.ForeColor = Color.Red;
                    _excelData.Clear();
                    _isFileValid = false;
                    UpdateSaveButtonState();
                    return;
                }

                lblListName.Text = $"Wgrany plik: {_excelName} ({_excelData.Count} komponentów)";
                statusLabel.Text = $"Wczytano {_excelData.Count} komponentów";
                statusLabel.ForeColor = Color.Green;
                _isFileValid = true;
                UpdateSaveButtonState();
            }
            catch (Exception ex) 
            { 
                statusLabel.Text = $"Błąd wczytywania: {ex.Message}";
                statusLabel.ForeColor = Color.Red;
                _isFileValid = false;
                UpdateSaveButtonState();
                return;
            }
            finally
            {
                lblLoading.Visible = false;
                ButtonImportExcelList.Enabled = true;
            }
        }

        virtual protected async void OnButtonSaveListClick(object? sender, EventArgs e)
        {
            if(_excelData.Count == 0)
            {
                MessageBox.Show("Brak wgranej listy do zapisu.", "Błąd zapisu listy Excel", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if(int.TryParse(textBox1.Text.Trim(), out int start) == false)
            {
                MessageBox.Show("Ilość nie może być pusta.", "Błąd zapisu listy Excel", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            lblLoading.Text = "Zapisywanie...";
            lblLoading.Visible = true;
            button1.Enabled = false;
            ButtonImportExcelList.Enabled = false;
            Application.DoEvents();

            var SMTdata = new List<(string Kol1, string Kol2, string Kol3)>();
            var THTdata = new List<(string Kol1, string Kol2, string Kol3)>();
            long listId;
            try
            {
                foreach(var row in _excelData)
                {
                    if(!int.TryParse(row.Kol2, out var r_id) || !int.TryParse(row.Kol3, out var qty))
                    {
                        MessageBox.Show($"Niepoprawne dane w liście: {row.Kol1}, {row.Kol2}, {row.Kol3}", "Błąd zapisu listy Excel", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    var compIdResult = await _enterpriseDatabase.GetComponentIdByRIdAsync(r_id);
                    if(!compIdResult.IsSuccess || compIdResult.Data == -1)
                    {
                        MessageBox.Show($"Brak elementu w bazie danych id: {row.Kol2}", "Błąd zapisu listy Excel", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        lblLoading.Visible = false;
                        button1.Enabled = true;
                        ButtonImportExcelList.Enabled = true;
                        return;
                    }
                }
                var componentTasks = _excelData.Select(async x => 
                {
                    var rId = int.Parse(x.Kol2);
                    var compIdResult = await _enterpriseDatabase.GetComponentIdByRIdAsync(rId);
                    if (!compIdResult.IsSuccess) return (x, type: "UNKNOWN");
                    
                    var typeResult = await _enterpriseDatabase.GetComponentTypeAsync(compIdResult.Data);
                    return (x, type: typeResult.IsSuccess ? typeResult.Data ?? "UNKNOWN" : "UNKNOWN");
                });

                var componentResults = await Task.WhenAll(componentTasks);

                var excludedIds = new HashSet<string> { "65189", "65190", "73708", "56107", "59717", "63708" };
                componentResults = componentResults.Where(r => !excludedIds.Contains(r.x.Kol2)).ToArray();

                if (componentResults.Any(r => r.type == "UNKNOWN" || r.type == null || r.type == ""))
                {
                    var missingComps = componentResults.Where(r => r.type == "UNKNOWN" || r.type == null || r.type == "").Select(r => r.x.Kol2);
                    MessageBox.Show($"Brak typu dla ID: {string.Join(", ", missingComps)}. Uzupełnij typ.", "Błąd zapisu listy Excel", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    lblLoading.Visible = false;
                    button1.Enabled = true;
                    ButtonImportExcelList.Enabled = true;
                    return;
                }
                
                SMTdata = componentResults.Where(r => r.type != "THT").Select(r => r.x).ToList();
                THTdata = componentResults.Where(r => r.type == "THT").Select(r => r.x).ToList();

                // Podiana ID listw THT


                var smtResult = await _enterpriseDatabase.AddListOfComponentsAsync(_excelName, start, radioSingleSided.Checked ,false);
                if (!smtResult.IsSuccess)
                {
                    MessageBox.Show($"Błąd zapisywania listy SMT: {smtResult.ErrorMessage}", "Błąd zapisu listy Excel", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                listId = smtResult.Data;
                
                var thtResult = await _enterpriseDatabase.AddListOfComponentsAsync(_excelName + "_THT", start, radioSingleSided.Checked, true, listId);
                if (!thtResult.IsSuccess)
                {
                    MessageBox.Show($"Błąd zapisywania listy THT: {thtResult.ErrorMessage}", "Błąd zapisu listy Excel", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                var updateResult = await _enterpriseDatabase.UpdateReservationTHTAsyncList(THTdata, thtResult.Data);
                if (!updateResult.IsSuccess)
                {
                    MessageBox.Show($"Błąd zapisywania rezerwacji THT: {updateResult.ErrorMessage}", "Błąd zapisu listy Excel", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                MessageBox.Show("Sukces", "Lista zapisana w bazie.", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) 
            { 
                MessageBox.Show(ex.Message, "Błąd importu listy Excel", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblLoading.Visible = false;
                button1.Enabled = true;
                ButtonImportExcelList.Enabled = true;
                return;
            }

            if (listId == -1)
            {
                MessageBox.Show("Błąd zapisu listy w bazie danych.", "Błąd zapisu listy Excel", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblLoading.Visible = false;
                button1.Enabled = true;
                ButtonImportExcelList.Enabled = true;
                return;
            }

            if (radioSingleSided.Checked)
            {
                _enterpriseDatabase.UpdateReservationListAsyncList(SMTdata, (int)listId, "SINGLE").Wait();
                Close();
                return;;
            }

            var editReservationListForm = new EditReservationList(_enterpriseDatabase, listId, true);
            editReservationListForm.ShowDialog();
            if (editReservationListForm.WasSkipped)
            {
                _enterpriseDatabase.UpdateReservationListAsyncList(SMTdata, (int)listId, "NONE").Wait();            
            } 
            else if (editReservationListForm.BothFilesAdded)
            {
                
                var smdData = _excelData.Where(row => 
                {
                    if(!int.TryParse(row.Kol2, out var r_id))
                        return false;
                    var compIdResult = _enterpriseDatabase.GetComponentIdByRIdAsync(r_id).Result;
                    if(!compIdResult.IsSuccess || compIdResult.Data == -1)
                        return false;
                    var typeResult = _enterpriseDatabase.GetComponentTypeAsync(compIdResult.Data).Result;
                    return typeResult.IsSuccess && typeResult.Data != "THT";
                }).ToList();

                var topData = new List<(string Name, string Id, string Quantity)>();
                var botData = new List<(string Name, string Id, string Quantity)>();
                foreach (var row in smdData)
                {
                    var name = row.Kol1;
                    var id = row.Kol2;
                    var sideAndQty = _enterpriseDatabase.GetComponentSideAndQtyAsync(int.Parse(id), (int)listId).Result.Data;
                    foreach (var (type, qty) in sideAndQty)
                    {
                        if (type == "TOP")
                            topData.Add((name, id, qty.ToString()));
                        else if (type == "BOT")
                            botData.Add((name, id, qty.ToString()));
                    }
                }

                topData = topData.Select(x => (x.Name, x.Id, (int.Parse(x.Quantity) * start).ToString())).ToList();
                botData = botData.Select(x => (x.Name, x.Id, (int.Parse(x.Quantity) * start).ToString())).ToList();
                _enterpriseDatabase.UpdateReservationListAsyncList(topData, (int)listId, "TOP").Wait();
                _enterpriseDatabase.UpdateReservationListAsyncList(botData, (int)listId, "BOT").Wait();
            }

            Close();
        }
    }
}
