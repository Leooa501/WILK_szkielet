namespace WILK.Views
{
    partial class AlternativeAddForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            OriginalCompnent = new Label();
            textBoxOriginalComponent = new TextBox();
            OriginalComponentName = new Label();
            textBoxAlternativeComponent = new TextBox();
            AlernativeComponent = new Label();
            AlternativeComponentName = new Label();
            ButtonSave = new Button();
            ButtonCancel = new Button();
            lblLoading = new Label();
            statusStrip1 = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // OriginalCompnent
            // 
            OriginalCompnent.AutoSize = true;
            OriginalCompnent.Location = new Point(12, 9);
            OriginalCompnent.Name = "OriginalCompnent";
            OriginalCompnent.Size = new Size(180, 20);
            OriginalCompnent.TabIndex = 0;
            OriginalCompnent.Text = "Id oryginalnego elementu";
            // 
            // textBoxOriginalComponent
            // 
            textBoxOriginalComponent.Location = new Point(12, 46);
            textBoxOriginalComponent.Name = "textBoxOriginalComponent";
            textBoxOriginalComponent.Size = new Size(125, 27);
            textBoxOriginalComponent.TabIndex = 1;
            // 
            // OriginalComponentName
            // 
            OriginalComponentName.AutoSize = true;
            OriginalComponentName.Location = new Point(143, 49);
            OriginalComponentName.Name = "OriginalComponentName";
            OriginalComponentName.Size = new Size(0, 20);
            OriginalComponentName.TabIndex = 2;

            // Instantiate quantity controls
            QuantityLabel = new Label();
            numericUpDownQuantity = new NumericUpDown();

            // 
            // textBoxAlternativeComponent
            // 
            textBoxAlternativeComponent.Location = new Point(12, 149);
            textBoxAlternativeComponent.Name = "textBoxAlternativeComponent";
            textBoxAlternativeComponent.Size = new Size(125, 27);
            textBoxAlternativeComponent.TabIndex = 3;
            // 
            // AlernativeComponent
            // 
            AlernativeComponent.AutoSize = true;
            AlernativeComponent.Location = new Point(12, 114);
            AlernativeComponent.Name = "AlernativeComponent";
            AlernativeComponent.Size = new Size(101, 20);
            AlernativeComponent.TabIndex = 4;
            AlernativeComponent.Text = "Id zamiennika";
            // 
            // QuantityLabel
            // 
            QuantityLabel.AutoSize = true;
            QuantityLabel.Location = new Point(12, 186);
            QuantityLabel.Name = "QuantityLabel";
            QuantityLabel.Size = new Size(34, 20);
            QuantityLabel.TabIndex = 5;
            QuantityLabel.Text = "Ilość";
            // Begin init for numeric
            ((System.ComponentModel.ISupportInitialize)(numericUpDownQuantity)).BeginInit();
            // 
            // numericUpDownQuantity
            // 
            numericUpDownQuantity.Location = new Point(12, 210);
            numericUpDownQuantity.Name = "numericUpDownQuantity";
            numericUpDownQuantity.Size = new Size(125, 27);
            numericUpDownQuantity.TabIndex = 6;
            numericUpDownQuantity.Minimum = 0;
            numericUpDownQuantity.Maximum = 100000;
            numericUpDownQuantity.Value = 1;
            ((System.ComponentModel.ISupportInitialize)(numericUpDownQuantity)).EndInit();
            // 
            // AlternativeComponentName
            // 
            AlternativeComponentName.AutoSize = true;
            AlternativeComponentName.Location = new Point(143, 152);
            AlternativeComponentName.Name = "AlternativeComponentName";
            AlternativeComponentName.Size = new Size(0, 20);
            AlternativeComponentName.TabIndex = 5;
            // 
            // ButtonSave
            // 
            ButtonSave.Location = new Point(221, 252);
            ButtonSave.Name = "ButtonSave";
            ButtonSave.Size = new Size(94, 29);
            ButtonSave.TabIndex = 7;
            ButtonSave.Text = "Zapisz";
            ButtonSave.UseVisualStyleBackColor = true;
            // 
            // ButtonCancel
            // 
            ButtonCancel.Location = new Point(335, 252);
            ButtonCancel.Name = "ButtonCancel";
            ButtonCancel.Size = new Size(94, 29);
            ButtonCancel.TabIndex = 8;
            ButtonCancel.Text = "Anuluj";
            ButtonCancel.UseVisualStyleBackColor = true;
            // 
            // lblLoading
            // 
            lblLoading.AutoSize = true;
            lblLoading.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblLoading.ForeColor = Color.DarkBlue;
            lblLoading.Location = new Point(12, 246);
            lblLoading.Name = "lblLoading";
            lblLoading.Size = new Size(130, 23);
            lblLoading.TabIndex = 9;
            lblLoading.Text = "Zapisywanie...";
            lblLoading.Visible = false;
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new Size(20, 20);
            statusStrip1.Items.AddRange(new ToolStripItem[] { statusLabel });
            statusStrip1.Location = new Point(0, 263);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(441, 25);
            statusStrip1.TabIndex = 9;
            statusStrip1.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(60, 20);
            statusLabel.Text = "Gotowy";
            // 
            // AlternativeAddForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(441, 288);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Controls.Add(ButtonCancel);
            Controls.Add(ButtonSave);
            Controls.Add(numericUpDownQuantity);
            Controls.Add(QuantityLabel);
            Controls.Add(AlternativeComponentName);
            Controls.Add(AlernativeComponent);
            Controls.Add(textBoxAlternativeComponent);
            Controls.Add(OriginalComponentName);
            Controls.Add(textBoxOriginalComponent);
            Controls.Add(OriginalCompnent);
            Controls.Add(lblLoading);
            Controls.Add(statusStrip1);
            Name = "AlternativeAddForm";
            Text = "Dodaj alternatywę";
            Icon = new Icon("Resources/Icons/wolf_256x256.ico");
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label OriginalCompnent;
        private TextBox textBoxOriginalComponent;
        private Label OriginalComponentName;
        private TextBox textBoxAlternativeComponent;
        private Label AlernativeComponent;
        private Label AlternativeComponentName;
        private Label QuantityLabel;
        private NumericUpDown numericUpDownQuantity;
        private Button ButtonSave;
        private Button ButtonCancel;
        private Label lblLoading;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel statusLabel;
    }
}