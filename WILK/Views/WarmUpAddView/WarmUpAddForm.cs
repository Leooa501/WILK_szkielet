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
    public partial class WarmUpAddForm : Form
    {
        private IEnterpriseDatabase _enterpriseDatabase;
        private bool _isWarmUpComponentValid = false;

        public WarmUpAddForm(IEnterpriseDatabase enterpriseDatabase)
        {
            InitializeComponent();
            _enterpriseDatabase = enterpriseDatabase;
            
            // Keyboard shortcuts
            this.KeyPreview = true;
            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    Close();
                }
                else if (e.KeyCode == Keys.Enter && ButtonSave.Enabled)
                {
                    ButtonSave.PerformClick();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            };
            
            // Auto-focus first field
            this.Shown += (s, e) => textBoxWarmUpComponent.Focus();
            
            // Disable save button initially
            ButtonSave.Enabled = false;

            textBoxWarmUpComponent.TextChanged += async (s, e) =>
            {
                if (textBoxWarmUpComponent.Text.Length == 5)
                {
                    if (int.TryParse(textBoxWarmUpComponent.Text, out int warmUpId))
                    {
                        var result = await _enterpriseDatabase.GetComponentNameByRIdAsync(warmUpId);
                        WarmUpComponentName.Text = result.IsSuccess ? result.Data ?? String.Empty : String.Empty;
                        _isWarmUpComponentValid = result.IsSuccess && !string.IsNullOrEmpty(result.Data);
                        
                        // Visual feedback
                        textBoxWarmUpComponent.BackColor = _isWarmUpComponentValid ? Color.LightGreen : Color.LightPink;
                    }
                    else
                    {
                        WarmUpComponentName.Text = String.Empty;
                        _isWarmUpComponentValid = false;
                        textBoxWarmUpComponent.BackColor = Color.LightPink;
                    }
                }
                else
                {
                    WarmUpComponentName.Text = String.Empty;
                    _isWarmUpComponentValid = false;
                    textBoxWarmUpComponent.BackColor = string.IsNullOrEmpty(textBoxWarmUpComponent.Text) ? SystemColors.Window : Color.LightYellow;
                }
                UpdateSaveButtonState();
            };

            ButtonCancel.Click += (s, e) => Close();

            ButtonSave.Click += async (s, e) =>
            {
                try
                {
                    if (int.TryParse(textBoxWarmUpComponent.Text, out int warmUpId))
                    {
                        if (string.IsNullOrEmpty(WarmUpComponentName.Text))
                        {
                            statusLabel.Text = "Błąd: Niepoprawny ID komponentu";
                            statusLabel.ForeColor = Color.Red;
                            return;
                        }
                        
                        lblLoading.Visible = true;
                        ButtonSave.Enabled = false;
                        ButtonCancel.Enabled = false;
                        Application.DoEvents();

                        var result = await _enterpriseDatabase.AddWarmUpComponentAsync(warmUpId);
                        
                        lblLoading.Visible = false;
                        ButtonCancel.Enabled = true;
                        
                        if (result.IsSuccess)
                        {
                            statusLabel.Text = "Komponent dodany do wygrzania";
                            statusLabel.ForeColor = Color.Green;
                            Close();
                        }
                        else
                        {
                            ButtonSave.Enabled = true;
                            statusLabel.Text = $"Błąd: {result.ErrorMessage}";
                            statusLabel.ForeColor = Color.Red;
                        }
                    }
                    else
                    {
                        statusLabel.Text = "Błąd: Nieprawidłowy identyfikator komponentu";
                        statusLabel.ForeColor = Color.Red;
                    }
                }
                catch (Exception ex)
                {
                    lblLoading.Visible = false;
                    ButtonSave.Enabled = true;
                    ButtonCancel.Enabled = true;
                    statusLabel.Text = $"Błąd: {ex.Message}";
                    statusLabel.ForeColor = Color.Red;
                }
            };
        }

        private void UpdateSaveButtonState()
        {
            ButtonSave.Enabled = _isWarmUpComponentValid;
        }
    }
}
