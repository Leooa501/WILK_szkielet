using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WILK.Services;

namespace WILK.Views
{
    public partial class CreateReservationSingle : Form
    {
        private readonly IEnterpriseDatabase _enterpriseDatabase;
        private bool _isComponentValid = false;
        private bool _isQuantityValid = false;
        
        public CreateReservationSingle(IEnterpriseDatabase enterpriseDatabase)
        {
            _enterpriseDatabase = enterpriseDatabase;
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
            
            // Disable button initially
            button1.Enabled = false;

            button1.Click += ButtonAddReservation_Click;

            textBox1.TextChanged += async (s, e) =>
            {
                if (textBox1.Text.Length == 5)
                {
                    if (int.TryParse(textBox1.Text, out int warmUpId))
                    {
                        var result = await _enterpriseDatabase.GetComponentNameByRIdAsync(warmUpId);
                        labelComponentName.Text = result.IsSuccess ? result.Data ?? String.Empty : String.Empty;
                        _isComponentValid = result.IsSuccess && !string.IsNullOrEmpty(result.Data);
                        
                        // Visual feedback
                        textBox1.BackColor = _isComponentValid ? Color.LightGreen : Color.LightPink;
                    }
                    else
                    {
                        labelComponentName.Text = String.Empty;
                        _isComponentValid = false;
                        textBox1.BackColor = Color.LightPink;
                    }
                }
                else
                {
                    labelComponentName.Text = String.Empty;
                    _isComponentValid = false;
                    textBox1.BackColor = string.IsNullOrEmpty(textBox1.Text) ? SystemColors.Window : Color.LightYellow;
                }
                UpdateButtonState();
            };

            textBox2.TextChanged += (s, e) =>
            {
                if (int.TryParse(textBox2.Text, out int qty) && qty > 0)
                {
                    _isQuantityValid = true;
                    textBox2.BackColor = Color.LightGreen;
                }
                else
                {
                    _isQuantityValid = false;
                    textBox2.BackColor = string.IsNullOrEmpty(textBox2.Text) ? SystemColors.Window : Color.LightPink;
                }
                UpdateButtonState();
            };
        }

        private async void ButtonAddReservation_Click(object? sender, EventArgs e)
        {
            if (!int.TryParse(textBox1.Text, out int r_id) || !int.TryParse(textBox2.Text, out int qty))
            {
                statusLabel.Text = "Błąd: Niepoprawne dane rezerwacji";
                statusLabel.ForeColor = Color.Red;
                return;
            }

            if (qty <= 0)
            {
                statusLabel.Text = "Błąd: Ilość musi być większa od zera";
                statusLabel.ForeColor = Color.Red;
                return;
            }

            if (string.IsNullOrEmpty(labelComponentName.Text))
            {
                statusLabel.Text = "Błąd: Niepoprawne ID komponentu";
                statusLabel.ForeColor = Color.Red;
                return;
            }

            lblLoading.Visible = true;
            button1.Enabled = false;
            Application.DoEvents();

            try
            {
                var result = await _enterpriseDatabase.AddReservationComponentAsync(r_id, qty);
                
                lblLoading.Visible = false;
                
                if (result.IsSuccess)
                {
                    statusLabel.Text = "Zarezerwowano pomyślnie";
                    statusLabel.ForeColor = Color.Green;
                }
                else
                {
                    button1.Enabled = true;
                    statusLabel.Text = $"Błąd: {result.ErrorMessage ?? "Unknown error"}";
                    statusLabel.ForeColor = Color.Red;
                }
            }
            catch (Exception ex) 
            { 
                lblLoading.Visible = false;
                button1.Enabled = true;
                statusLabel.Text = $"Błąd: {ex.Message}";
                statusLabel.ForeColor = Color.Red;
            }

            Close();
        }

        private void UpdateButtonState()
        {
            button1.Enabled = _isComponentValid && _isQuantityValid;
        }
    }
}
