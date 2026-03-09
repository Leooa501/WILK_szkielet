using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using WILK.Presenters;
using WILK.Services;

namespace WILK.Views.Tabs
{
    public class ComponentsTab : BaseTab, IComponentsView
    {
        private ComponentsPresenter _presenter;
        
        // Controls
        private DataGridView _grid;
        private TextBox _txtSearchId;
        private TextBox _txtSearchName;
        private Button _btnAdd;
        private Button _btnEdit;

        public event EventHandler SearchRequested;
        public event EventHandler AddRequested;
        public event EventHandler EditRequested;

        public override string TabName => "Komponenty";

        public ComponentsTab(IEnterpriseDatabase db, IMainView mainView) : base(db, mainView)
        {
            _presenter = new ComponentsPresenter(this, db);
        }

        public string SearchId => _txtSearchId.Text;
        public string SearchName => _txtSearchName.Text;

        protected override void CreateTabPage()
        {
            TabPage = new TabPage(TabName) { UseVisualStyleBackColor = true };
        }

        protected override void SetupControls()
        {
            var mainPanel = new Panel { Dock = DockStyle.Fill, Parent = TabPage };

            var topPanel = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.WhiteSmoke };
            int topY = 12;

            new Label { Text = "ID:", Location = new Point(10, topY + 4), AutoSize = true, Parent = topPanel };
            _txtSearchId = new TextBox { Location = new Point(35, topY), Width = 100, Parent = topPanel, MaxLength = 5 };

            new Label { Text = "Nazwa:", Location = new Point(150, topY + 4), AutoSize = true, Parent = topPanel };
            _txtSearchName = new TextBox { Location = new Point(200, topY), Width = 250, Parent = topPanel };


            // bottom panel
            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = Color.WhiteSmoke };
            int botY = 15;

            // add button
            _btnAdd = new Button 
            { 
                Text = "Dodaj", 
                Location = new Point(30, botY), 
                Size = new Size(120, 35), 
                Parent = bottomPanel, 
                UseVisualStyleBackColor = true,
                BackColor = Color.White 
            };

            // edit button
            _btnEdit = new Button 
            { 
                Text = "Edytuj", 
                Location = new Point(170, botY), 
                Size = new Size(120, 35), 
                Parent = bottomPanel, 
                UseVisualStyleBackColor = true,
                BackColor = Color.White 
            };


            // GRID
            _grid = new DataGridView
            {
                Dock = DockStyle.Fill, 
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                
                
                BackgroundColor = Color.White, 
                BorderStyle = BorderStyle.FixedSingle,
                CellBorderStyle = DataGridViewCellBorderStyle.Single,
                EnableHeadersVisualStyles = false, 
                AllowUserToResizeRows = false,
                AllowUserToResizeColumns = false
            };

            

            //hearders style
            
            
            _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240); 
            _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            _grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(240, 240, 240);
            _grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            _grid.ColumnHeadersHeight = 30;

            // row styles
            _grid.DefaultCellStyle.BackColor = Color.White;
            _grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            _grid.DefaultCellStyle.BackColor = Color.White;
            _grid.DefaultCellStyle.ForeColor = Color.Black;
            _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215); 
            _grid.DefaultCellStyle.SelectionForeColor = Color.White;

            // Adding to screen
            mainPanel.Controls.Add(_grid);
            mainPanel.Controls.Add(bottomPanel);
            mainPanel.Controls.Add(topPanel);

            SearchRequested?.Invoke(this, EventArgs.Empty);
        }

        protected override void AttachEventHandlers()
        {
            _txtSearchName.TextChanged += (s, e) => SearchRequested?.Invoke(this, EventArgs.Empty);
            _txtSearchId.TextChanged += (s, e) => SearchRequested?.Invoke(this, EventArgs.Empty);

            _txtSearchId.KeyPress += (s, ev) => 
            {
                if (!char.IsControl(ev.KeyChar) && !char.IsDigit(ev.KeyChar))
                {
                    ev.Handled = true;
                }
            };

            _btnAdd.Click += (s, e) => AddRequested?.Invoke(this, EventArgs.Empty);
            _btnEdit.Click += (s, e) => EditRequested?.Invoke(this, EventArgs.Empty);
        }

        public void SetGridData(DataTable dt)
        {
            _grid.DataSource = dt;
            if (_grid.Columns.Contains("r_id")) _grid.Columns["r_id"].HeaderText = "ID";
            if (_grid.Columns.Contains("name")) _grid.Columns["name"].HeaderText = "Nazwa";
            if (_grid.Columns.Contains("quantity")) _grid.Columns["quantity"].HeaderText = "Ilość";
            if (_grid.Columns.Contains("type")) _grid.Columns["type"].HeaderText = "Typ";
        }

        public (int id, string name, string type)? GetSelectedComponent()
        {
            if (_grid.SelectedRows.Count == 0) return null;
            var row = _grid.SelectedRows[0];
            int rId = row.Cells["r_id"].Value != DBNull.Value ? Convert.ToInt32(row.Cells["r_id"].Value) : 0;
            string name = row.Cells["name"].Value?.ToString() ?? "";
            string type = row.Cells["type"].Value?.ToString() ?? "";
            return (rId, name, type);
        }

        public bool ShowComponentDialog(bool isEdit, ref string id, ref string name, ref string type)
        {
            using (var form = new Form())
            {
                form.Text = isEdit ? "Edytuj Komponent" : "Dodaj Komponent";
                form.Size = new Size(400, 300);
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;

                var txtId = new TextBox { Text = id, Left = 20, Top = 40, Width = 340, Enabled = !isEdit,MaxLength = 5 };
                txtId.KeyPress += (s, ev) => 
                {
                    if (!char.IsControl(ev.KeyChar) && !char.IsDigit(ev.KeyChar))
                    {
                        ev.Handled = true; 
                    }
                };
                var txtName = new TextBox { Text = name, Left = 20, Top = 100, Width = 340, MaxLength = 100};
                var cmbType = new ComboBox 
                { 
                    Left = 20, Top = 160, Width = 340, 
                    DropDownStyle = ComboBoxStyle.DropDownList 
                };
                cmbType.Items.AddRange(new string[] { "SMD", "THT","PCB"});
                
                if (!string.IsNullOrEmpty(type) && cmbType.Items.Contains(type))
                    cmbType.SelectedItem = type;
                else
                    cmbType.SelectedIndex = 0; 

                var btnOk = new Button { Text = "Zapisz", Left = 180, Top = 210, Width = 80, Height = 30 };
                var btnCancel = new Button { Text = "Anuluj", Left = 270, Top = 210, Width = 80, Height = 30, DialogResult = DialogResult.Cancel };


                btnOk.Click += (s, ev) => 
                {
                    if (txtId.Text.Trim().Length != 5)
                    {
                        MessageBox.Show("ID musi składać się dokładnie z 5 cyfr!", "Błąd walidacji", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return; 
                    }

                    if (string.IsNullOrWhiteSpace(txtName.Text))
                    {
                        MessageBox.Show("Nazwa nie może być pusta!", "Błąd walidacji", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (txtName.Text.Length > 100)
                    {
                        MessageBox.Show("Nazwa nie może być dłuższa niż 100 znaków!", "Błąd walidacji", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    form.DialogResult = DialogResult.OK; 
                };

                form.Controls.Add(new Label { Text = "ID (5 cyfr):", Left = 20, Top = 20, AutoSize = true });
                form.Controls.Add(txtId);
                form.Controls.Add(new Label { Text = "Nazwa:", Left = 20, Top = 80, AutoSize = true });
                form.Controls.Add(txtName);
                form.Controls.Add(new Label { Text = "Typ:", Left = 20, Top = 140, AutoSize = true });
                form.Controls.Add(cmbType); 
                form.Controls.Add(btnOk);
                form.Controls.Add(btnCancel);
                form.AcceptButton = btnOk;
                form.CancelButton = btnCancel;

                if (form.ShowDialog() == DialogResult.OK)
                {
                    id = txtId.Text;
                    name = txtName.Text;
                    type = cmbType.SelectedItem?.ToString() ?? ""; 
                    return true;
                }
            }
            return false;
        }

        public new void ShowError(string title, string message) => MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        public new void ShowInfo(string title, string message) => MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}