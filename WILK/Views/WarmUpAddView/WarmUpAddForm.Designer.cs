namespace WILK.Views
{
    partial class WarmUpAddForm
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
            ButtonCancel = new Button();
            ButtonSave = new Button();
            this.WarmUpComponentName = new Label();
            this.textBoxWarmUpComponent = new TextBox();
            WarmUpCompnent = new Label();
            lblLoading = new Label();
            statusStrip1 = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // ButtonCancel
            // 
            ButtonCancel.Location = new Point(275, 94);
            ButtonCancel.Name = "ButtonCancel";
            ButtonCancel.Size = new Size(94, 29);
            ButtonCancel.TabIndex = 15;
            ButtonCancel.Text = "Anuluj";
            ButtonCancel.UseVisualStyleBackColor = true;
            // 
            // ButtonSave
            // 
            ButtonSave.Location = new Point(161, 94);
            ButtonSave.Name = "ButtonSave";
            ButtonSave.Size = new Size(94, 29);
            ButtonSave.TabIndex = 14;
            ButtonSave.Text = "Zapisz";
            ButtonSave.UseVisualStyleBackColor = true;
            // 
            // WarmUpComponentName
            // 
            this.WarmUpComponentName.AutoSize = true;
            this.WarmUpComponentName.Location = new Point(143, 49);
            this.WarmUpComponentName.Name = "WarmUpComponentName";
            this.WarmUpComponentName.Size = new Size(0, 20);
            this.WarmUpComponentName.TabIndex = 10;
            // 
            // textBoxWarmUpComponent
            // 
            this.textBoxWarmUpComponent.Location = new Point(12, 46);
            this.textBoxWarmUpComponent.Name = "textBoxWarmUpComponent";
            this.textBoxWarmUpComponent.Size = new Size(125, 27);
            this.textBoxWarmUpComponent.TabIndex = 9;
            // 
            // WarmUpCompnent
            // 
            WarmUpCompnent.AutoSize = true;
            WarmUpCompnent.Location = new Point(12, 9);
            WarmUpCompnent.Name = "WarmUpCompnent";
            WarmUpCompnent.Size = new Size(181, 20);
            WarmUpCompnent.TabIndex = 8;
            WarmUpCompnent.Text = "Id elementu do wygrzania";
            // 
            // lblLoading
            // 
            lblLoading.AutoSize = true;
            lblLoading.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblLoading.ForeColor = Color.DarkBlue;
            lblLoading.Location = new Point(12, 80);
            lblLoading.Name = "lblLoading";
            lblLoading.Size = new Size(130, 23);
            lblLoading.TabIndex = 16;
            lblLoading.Text = "Zapisywanie...";
            lblLoading.Visible = false;
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new Size(20, 20);
            statusStrip1.Items.AddRange(new ToolStripItem[] { statusLabel });
            statusStrip1.Location = new Point(0, 135);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(381, 25);
            statusStrip1.TabIndex = 17;
            statusStrip1.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(60, 20);
            statusLabel.Text = "Gotowy";
            // 
            // WarmUpAddForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(381, 160);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Controls.Add(ButtonCancel);
            Controls.Add(ButtonSave);
            Controls.Add(this.WarmUpComponentName);
            Controls.Add(this.textBoxWarmUpComponent);
            Controls.Add(WarmUpCompnent);
            Controls.Add(lblLoading);
            Controls.Add(statusStrip1);
            Name = "WarmUpAddForm";
            Text = "WarmUpAddForm";
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            Icon = new Icon("Resources/Icons/wolf_256x256.ico");
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button ButtonCancel;
        private Button ButtonSave;
        private TextBox textBoxWarmUpComponent;
        private Label WarmUpComponentName;
        private Label WarmUpCompnent;
        private Label lblLoading;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel statusLabel;
    }
}