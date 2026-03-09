using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MongoDB.Driver.Linq;
using WILK.Services;

namespace WILK.Views
{
    public partial class EditReservationList : Form
    {
        private long _reservationId;
        private IEnterpriseDatabase _enterpriseDatabase;
        bool skipAble;
        private bool _wasSkipped = false;
        private bool _topFileAdded = false;
        private bool _botFileAdded = false;
        private string _topFileName = "";
        private string _botFileName = "";

        private List<(int r_id, int quantity)> topData;
        private List<(int r_id, int quantity)> botData;
        
        public bool WasSkipped => _wasSkipped;
        public bool BothFilesAdded => _topFileAdded && _botFileAdded;
        
        public EditReservationList(IEnterpriseDatabase enterpriseDatabase, long reservationId, bool skipAble = false)
        {
            _enterpriseDatabase = enterpriseDatabase;
            _reservationId = reservationId;
            this.skipAble = skipAble;
            InitializeComponent();
            
            // Keyboard shortcuts
            this.KeyPreview = true;
            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    Close();
                }
                else if (e.KeyCode == Keys.Enter)
                {
                    if (skipAble && ButtonSkip.Visible)
                    {
                        ButtonSkip.PerformClick();
                    }
                    else if (ButtonOk.Visible)
                    {
                        ButtonOk.PerformClick();
                    }
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            };

            LoadReservationDetails();

            ButtonEditFileTop.Click += OnButtonEditFileTopClick;
            ButtonEditFileBot.Click += OnButtonEditFileBotClick;
            ButtonOk.Click += OnButtonOkClick;

            if (skipAble)
            {
                ButtonSkip.Visible = true;
                ButtonSkip.Click += (s, e) => 
                { 
                    _wasSkipped = true;
                    this.Close(); 
                };
                ButtonOk.Visible = true;
                ButtonOk.Enabled = false;
            }else
            {
                ButtonSkip.Visible = false;
                ButtonOk.Visible = true;
                ButtonOk.Enabled = false;
            }
        }

        private void LoadReservationDetails()
        {
            var topFileResult = _enterpriseDatabase.GetSideFileNameAsync(_reservationId, "TOP").Result;
            LabelTop.Text = "TOP: " + (topFileResult.IsSuccess && !String.IsNullOrEmpty(topFileResult.Data) ? topFileResult.Data : "Brak pliku");
            var botFileResult = _enterpriseDatabase.GetSideFileNameAsync(_reservationId, "BOT").Result;
            LabelBot.Text = "BOT: " + (botFileResult.IsSuccess && !String.IsNullOrEmpty(botFileResult.Data) ? botFileResult.Data : "Brak pliku");
        }

        private void OnButtonEditFileTopClick(object? sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "Excel files|*.xlsx;*.xls";

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            var data = processData(ofd.FileName);
            topData = data;
            if (data.Count == 0)
            {   
                lblLoading.Visible = false;
                ButtonEditFileTop.Enabled = true;
                ButtonEditFileBot.Enabled = true;
                return;
            }
            _topFileName = ofd.FileName;
            LabelTop.Text = "TOP: " + System.IO.Path.GetFileName(_topFileName);

            _topFileAdded = true;
            //LoadReservationDetails();

            
            lblLoading.Visible = false;
            ButtonEditFileTop.Enabled = true;
            ButtonEditFileBot.Enabled = true;
            if(_topFileAdded && _botFileAdded)
            {
                ButtonOk.Enabled = true;
            }
        }

        private void OnButtonEditFileBotClick(object? sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "Excel files|*.xlsx;*.xls";

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            lblLoading.Text = "Ładowanie pliku...";
            lblLoading.Visible = true;
            ButtonEditFileTop.Enabled = false;
            ButtonEditFileBot.Enabled = false;
            Application.DoEvents();

            var data = processData(ofd.FileName);
            botData = data;
            if (data.Count == 0)
            {
                lblLoading.Visible = false;
                ButtonEditFileTop.Enabled = true;
                ButtonEditFileBot.Enabled = true;
                return;
            }

            _botFileName = ofd.FileName;
            LabelBot.Text = "BOT: " + System.IO.Path.GetFileName(_botFileName);
            _botFileAdded = true;
            //LoadReservationDetails();
            
            lblLoading.Visible = false;
            ButtonEditFileTop.Enabled = true;
            ButtonEditFileBot.Enabled = true;
            if(_topFileAdded && _botFileAdded)
            {
                ButtonOk.Enabled = true;
            }
        }
        private List<(int r_id, int quantity)> processData(string path)
        {
            var result = new List<(int r_id, int quantity)>();

            try
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                using var stream = System.IO.File.Open(path, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                using var reader = ExcelDataReader.ExcelReaderFactory.CreateReader(stream);

                while (reader.Read())
                {
                    var idStr = reader.GetValue(6)?.ToString();
                    if (string.IsNullOrEmpty(idStr))
                        continue;

                    if (!int.TryParse(idStr, out var id))
                        continue;

                    var idx = result.FindIndex(x => x.r_id == id);
                    if (idx >= 0)
                    {
                        var existing = result[idx];
                        result[idx] = (existing.r_id, existing.quantity + 1);
                    }
                    else
                    {
                        result.Add((id, 1));
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return result;
        }

        private void OnButtonOkClick(object? sender, EventArgs e)
        {
            lblLoading.Text = "Aktualizowanie rezerwacji...";
            lblLoading.Visible = true;
            ButtonOk.Enabled = false;
            Application.DoEvents();

            if(topData == null) topData = new List<(int r_id, int quantity)>();
            var dataForTop = topData.Select(x => 
                {
                    var name = _enterpriseDatabase.GetComponentNameByRIdAsync(x.r_id).Result.Data ?? "";
                    return (name, x.r_id.ToString(), x.quantity.ToString()); 
                }
            );

            if(botData == null) botData = new List<(int r_id, int quantity)>();
            var dataForBot = botData.Select(x => 
                {
                    var name = _enterpriseDatabase.GetComponentNameByRIdAsync(x.r_id).Result.Data ?? "";
                    return (name, x.r_id.ToString(), x.quantity.ToString()); 
                }
            );

            topData = topData.Where(t => t.r_id != -1 && t.r_id != 65189 && t.r_id != 65190 && t.r_id != 73708 && t.r_id != 56107
 && t.r_id != 59717 && t.r_id != 63708).ToList();
            botData = botData.Where(b => b.r_id != -1 && b.r_id != 65189 && b.r_id != 65190 && b.r_id != 73708 && b.r_id != 56107
 && b.r_id != 59717 && b.r_id != 63708).ToList();

            if(botData.Count == 0 && topData.Count == 0)
            {
                MessageBox.Show("Nie dodano żadnych danych z plików. Proszę dodać pliki przed zatwierdzeniem.", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                lblLoading.Visible = false;
                ButtonOk.Enabled = true;
                return;
            }
            if(!skipAble)
            {
                var comps = _enterpriseDatabase.GetListDataAsync((int)_reservationId).Result.Data;
                var listData = _enterpriseDatabase.GetReservationProgressAsync((int)_reservationId).Result.Data;
                
                // Components in reservation but NOT in new files (missing from files)
                var missingFromFiles = comps.AsEnumerable().Where(c => 
                    !topData.Any(t => t.r_id == (int)c["elementId"]) && 
                    !botData.Any(b => b.r_id == (int)c["elementId"])
                );
                // Exclude components whose type is PCB
                missingFromFiles = missingFromFiles.Where(r => {
                    var typeObj = r["type"];
                    if (typeObj == null || typeObj == DBNull.Value) return true; // keep if type unknown
                    var typeStr = typeObj.ToString().Trim();
                    return !string.Equals(typeStr, "PCB", StringComparison.OrdinalIgnoreCase);
                });

                // Components in new files but NOT in current reservation (being added)
                var existingIds = comps.AsEnumerable().Select(c => (int)c["elementId"]).ToHashSet();
                var allNewIds = topData.Select(t => t.r_id).Union(botData.Select(b => b.r_id)).Distinct();
                var addedToFiles = allNewIds.Where(id => !existingIds.Contains(id)).ToList();

                var quantityMismatches = new List<string>();
                foreach (var comp in comps.AsEnumerable())
                {
                    var elementId = (int)comp["elementId"];
                    var reservedQty = (int)comp["quantity"];
                    var typeObj = comp["type"];
                    if (typeObj?.ToString().Trim().Equals("PCB", StringComparison.OrdinalIgnoreCase) == true)
                        continue;
                    var topQty = topData.FirstOrDefault(t => t.r_id == elementId).quantity * listData.Value.start;
                    var botQty = botData.FirstOrDefault(b => b.r_id == elementId).quantity * listData.Value.start;
                    var totalQty = topQty + botQty;
                    if (reservedQty != totalQty)
                    {
                        var name = comp["name"]?.ToString() ?? "";
                        quantityMismatches.Add($"R_Id: {elementId}, Nazwa: {name}, Ilość w rezerwacji: {reservedQty}, Ilość w plikach: {totalQty}");
                    }
                }

                if (missingFromFiles.Any() || addedToFiles.Any() || quantityMismatches.Any())
                {
                    var msg = new StringBuilder();
                    
                    if (missingFromFiles.Any())
                    {
                        msg.AppendLine("USUNIĘTE - Komponenty z rezerwacji nieuwzględnione w nowych plikach:");
                        foreach (var row in missingFromFiles)
                        {
                            msg.AppendLine($"  R_Id: {row["elementId"]}, Nazwa: {row["name"]}, Ilość w rezerwacji: {row["quantity"]}");
                        }
                        msg.AppendLine();
                    }
                    
                    if (addedToFiles.Any())
                    {
                        msg.AppendLine("DODANE - Komponenty w nowych plikach, których nie ma w rezerwacji:");
                        foreach (var id in addedToFiles)
                        {
                            var name = _enterpriseDatabase.GetComponentNameByRIdAsync(id).Result.Data ?? "";
                            var topQty = topData.FirstOrDefault(t => t.r_id == id).quantity;
                            var botQty = botData.FirstOrDefault(b => b.r_id == id).quantity;
                            var totalQty = topQty + botQty;
                            msg.AppendLine($"  R_Id: {id}, Nazwa: {name}, Ilość w plikach: {totalQty}");
                        }
                        msg.AppendLine();
                    }

                    if (quantityMismatches.Any())
                    {
                        msg.AppendLine("NIEZGODNOŚCI ILOŚCI - Komponenty, których ilość w rezerwacji różni się od ilości w nowych plikach:");
                        foreach (var line in quantityMismatches)
                        {
                            msg.AppendLine("  " + line);
                        }
                        msg.AppendLine();
                    }
                    
                    msg.AppendLine("Czy na pewno chcesz kontynuować?");
                    var dr = MessageBox.Show(msg.ToString(), "Potwierdzenie", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (dr != DialogResult.Yes)
                    {
                        lblLoading.Visible = false;
                        ButtonOk.Enabled = true;
                        
                        return;
                    }
                }

                var clearedReservationsResult = _enterpriseDatabase.ClearReservationsAsync(_reservationId).Result;

                _enterpriseDatabase.AddPnPDataAsync(topData, _reservationId, _topFileName, "TOP").Wait();
                _enterpriseDatabase.AddPnPDataAsync(botData, _reservationId, _botFileName, "BOT").Wait();

                dataForBot = dataForBot.Select(d => (d.name, d.Item2, (int.Parse(d.Item3) * listData.Value.start).ToString()));
                dataForTop = dataForTop.Select(d => (d.name, d.Item2, (int.Parse(d.Item3) * listData.Value.start).ToString()));

                _enterpriseDatabase.UpdateReservationListAsyncList(dataForBot.ToList(), _reservationId, "BOT").Wait();
                _enterpriseDatabase.UpdateReservationListAsyncList(dataForTop.ToList(), _reservationId, "TOP").Wait();

                if(clearedReservationsResult.IsSuccess && clearedReservationsResult.Data != null && clearedReservationsResult.Data.Count > 0)
                {
                     _enterpriseDatabase.MoveRealReservationData(clearedReservationsResult.Data, _reservationId).Wait();   
                }

            }else
            {
                _enterpriseDatabase.AddPnPDataAsync(topData, _reservationId, _topFileName, "TOP").Wait();
                _enterpriseDatabase.AddPnPDataAsync(botData, _reservationId, _botFileName, "BOT").Wait();
            }
            this.Close();
        }
    }
}
