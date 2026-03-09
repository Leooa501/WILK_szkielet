using WILK.Services;

namespace WILK.Views
{
    public partial class AlternativeAddForm : Form
    {
        private IEnterpriseDatabase _enterpriseDatabase;
        private readonly Func<int, int, int, int, Task<Models.DatabaseResult<bool>>>? _onSave;
        private int? _reservationId = null;
        private bool _isOriginalComponentValid = false;
        private bool _isAlternativeComponentValid = false;

        public AlternativeAddForm(IEnterpriseDatabase enterpriseDatabase)
        {
            InitializeComponent();
            _enterpriseDatabase = enterpriseDatabase;

            // By default hide quantity controls (shown only when requested by caller)
            if (QuantityLabel != null) QuantityLabel.Visible = false;
            if (numericUpDownQuantity != null) numericUpDownQuantity.Visible = false;
            
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
            this.Shown += (s, e) => textBoxOriginalComponent.Focus();
            
            // Disable save button initially
            ButtonSave.Enabled = false;

            // Update save button when quantity changes
            if (numericUpDownQuantity != null)
            {
                numericUpDownQuantity.ValueChanged += (s, e) => UpdateSaveButtonState();
            }
            
            textBoxOriginalComponent.TextChanged += async (s, e) =>
            {
                if (textBoxOriginalComponent.Text.Length == 5)
                {
                    if (int.TryParse(textBoxOriginalComponent.Text, out int originalId))
                    {
                        var result = await _enterpriseDatabase.GetComponentNameByRIdAsync(originalId);
                        OriginalComponentName.Text = result.IsSuccess ? result.Data ?? String.Empty : String.Empty;
                        _isOriginalComponentValid = result.IsSuccess && !string.IsNullOrEmpty(result.Data);
                        
                        // Visual feedback
                        textBoxOriginalComponent.BackColor = _isOriginalComponentValid ? Color.LightGreen : Color.LightPink;
                    }
                    else
                    {
                        OriginalComponentName.Text = String.Empty;
                        _isOriginalComponentValid = false;
                        textBoxOriginalComponent.BackColor = Color.LightPink;
                    }
                }
                else
                {
                    OriginalComponentName.Text = String.Empty;
                    _isOriginalComponentValid = false;
                    textBoxOriginalComponent.BackColor = string.IsNullOrEmpty(textBoxOriginalComponent.Text) ? SystemColors.Window : Color.LightYellow;
                }
                UpdateSaveButtonState();
            };
            textBoxAlternativeComponent.TextChanged += async (s, e) =>
            {
                if (textBoxAlternativeComponent.Text.Length == 5)
                {
                    if (int.TryParse(textBoxAlternativeComponent.Text, out int alternativeId))
                    {
                        var result = await _enterpriseDatabase.GetComponentNameByRIdAsync(alternativeId);
                        AlternativeComponentName.Text = result.IsSuccess ? result.Data ?? String.Empty : String.Empty;
                        _isAlternativeComponentValid = result.IsSuccess && !string.IsNullOrEmpty(result.Data);
                        
                        // Visual feedback
                        textBoxAlternativeComponent.BackColor = _isAlternativeComponentValid ? Color.LightGreen : Color.LightPink;
                    }
                    else
                    {
                        AlternativeComponentName.Text = String.Empty;
                        _isAlternativeComponentValid = false;
                        textBoxAlternativeComponent.BackColor = Color.LightPink;
                    }
                }
                else
                {
                    AlternativeComponentName.Text = String.Empty;
                    _isAlternativeComponentValid = false;
                    textBoxAlternativeComponent.BackColor = string.IsNullOrEmpty(textBoxAlternativeComponent.Text) ? SystemColors.Window : Color.LightYellow;
                }
                UpdateSaveButtonState();
            };

            // Cancel
            ButtonCancel.Click += (s, e) => Close();

            ButtonSave.Click += async (s, e) =>
            {
                try
                {
                    if (int.TryParse(textBoxOriginalComponent.Text, out int originalId) &&
                        int.TryParse(textBoxAlternativeComponent.Text, out int alternativeId))
                    {
                        if (originalId == alternativeId)
                        {
                            statusLabel.Text = "Błąd: Komponent nie może być swoją alternatywą";
                            statusLabel.ForeColor = Color.Red;
                            return;
                        }
                        if (OriginalComponentName.Text == String.Empty || AlternativeComponentName.Text == String.Empty)
                        {
                            statusLabel.Text = "Błąd: Niepoprawne ID komponentów";
                            statusLabel.ForeColor = Color.Red;
                            return;   
                        }

                        lblLoading.Visible = true;
                        ButtonSave.Enabled = false;
                        ButtonCancel.Enabled = false;
                        Application.DoEvents();

                        WILK.Models.DatabaseResult<bool>? result = null;
                        int qty = 0;
                        if (numericUpDownQuantity != null)
                        {
                            qty = (int)numericUpDownQuantity.Value;
                        }

                        if (_onSave != null)
                        {
                            // pass reservation id (if provided) as the first parameter, otherwise use originalId for backward compatibility
                            int rid = _reservationId ?? originalId;
                            result = await _onSave(rid, originalId, alternativeId, qty);
                        }
                        else
                        {
                            // Direct assignment, as dbResult is already DatabaseResult<bool>
                            var dbResult = await _enterpriseDatabase.AddAlternativeComponentAsync(originalId, alternativeId);
                            result = dbResult;
                        }

                        lblLoading.Visible = false;
                        ButtonCancel.Enabled = true;

                        if (result != null && result.IsSuccess)
                        {
                            statusLabel.Text = "Alternatywa została dodana pomyślnie";
                            statusLabel.ForeColor = Color.Green;
                            Close();
                        }
                        else
                        {
                            ButtonSave.Enabled = true;
                            statusLabel.Text = $"Błąd: {result?.ErrorMessage ?? "Nieznany błąd"}";
                            statusLabel.ForeColor = Color.Red;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Proszę wprowadzić poprawne ID komponentów.", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    lblLoading.Visible = false;
                    ButtonSave.Enabled = true;
                    ButtonCancel.Enabled = true;
                    MessageBox.Show($"Błąd: {ex.Message}", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
        }

       // Convenience constructor to prefill original component id (fast-add scenario)
        public AlternativeAddForm(IEnterpriseDatabase enterpriseDatabase, int originalId, bool lockOriginal = true, bool showQuantity = false, int initialQuantity = 1, Func<int, int, int, int, System.Threading.Tasks.Task<WILK.Models.DatabaseResult<bool>>>? onSave = null, int? reservationId = null)
            : this(enterpriseDatabase)
        {
            _onSave = onSave;
            _reservationId = reservationId;
            try
            {
                textBoxOriginalComponent.Text = originalId.ToString();
                if (lockOriginal)
                {
                    textBoxOriginalComponent.Enabled = false;
                }

                // show or hide quantity controls depending on caller intent
                if (QuantityLabel != null && numericUpDownQuantity != null)
                {
                    QuantityLabel.Visible = showQuantity;
                    numericUpDownQuantity.Visible = showQuantity;

                    // set initial quantity if requested
                    try
                    {
                        int min = (int)numericUpDownQuantity.Minimum;
                        int max = (int)numericUpDownQuantity.Maximum;
                        int val = initialQuantity;
                        if (val < min) val = min;
                        if (val > max) val = max;
                        numericUpDownQuantity.Value = val;
                    }
                    catch
                    {
                        // ignore if value cannot be set
                    }
                }

                // validate original component id asynchronously
                _ = ValidateOriginalIdAsync(originalId);
            }
            catch
            {
                // ignore - leave default state
            }
        }   
        private async System.Threading.Tasks.Task ValidateOriginalIdAsync(int originalId)
        {
            var result = await _enterpriseDatabase.GetComponentNameByRIdAsync(originalId);
            OriginalComponentName.Text = result.IsSuccess ? result.Data ?? String.Empty : String.Empty;
            _isOriginalComponentValid = result.IsSuccess && !string.IsNullOrEmpty(result.Data);
            textBoxOriginalComponent.BackColor = _isOriginalComponentValid ? Color.LightGreen : Color.LightPink;
            UpdateSaveButtonState();
        }

        private void UpdateSaveButtonState()
        {
            bool qtyOk = true;
            if (numericUpDownQuantity != null)
            {
                qtyOk = numericUpDownQuantity.Value >= 0;
            }
            ButtonSave.Enabled = _isOriginalComponentValid && _isAlternativeComponentValid && qtyOk;
        }
    }
}
