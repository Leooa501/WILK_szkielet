namespace WILK.Views
{
    partial class CreateReservationList
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
            ButtonImportExcelList = new Button();
            lblListName = new Label();
            label5 = new Label();
            button1 = new Button();
            label1 = new Label();
            textBox1 = new TextBox();
            lblLoading = new Label();
            statusStrip1 = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            statusStrip1.SuspendLayout();
            radioDoubleSided = new RadioButton();
            radioSingleSided = new RadioButton();
            //debug = new Button();
            SuspendLayout();
            //
            // debug
            //
            //debug.Location = new Point(313, 32);
            //debug.Name = "debug";
            //debug.Size = new Size(126, 29);
            //debug.TabIndex = 20;
            // 
            // ButtonImportExcelList
            // 
            ButtonImportExcelList.Location = new Point(12, 32);
            ButtonImportExcelList.Name = "ButtonImportExcelList";
            ButtonImportExcelList.Size = new Size(133, 60);
            ButtonImportExcelList.TabIndex = 13;
            ButtonImportExcelList.Text = "Wgraj plik z lista";
            ButtonImportExcelList.UseVisualStyleBackColor = true;
            // 
            // lblListName
            // 
            lblListName.AutoSize = true;
            lblListName.Location = new Point(12, 95);
            lblListName.MaximumSize = new Size(350, 0);
            lblListName.Name = "lblListName";
            lblListName.Size = new Size(129, 20);
            lblListName.TabIndex = 14;
            lblListName.Text = "Wgrany plik: pusty";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(12, 9);
            label5.Name = "label5";
            label5.Size = new Size(116, 20);
            label5.TabIndex = 15;
            label5.Text = "Zarezerwuj listę:";
            // 
            // button1
            // 
            button1.Location = new Point(313, 156);
            button1.Name = "button1";
            button1.Size = new Size(126, 49);
            button1.TabIndex = 11;
            button1.Text = "Zapisz";
            button1.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(20, 147);
            label1.Name = "label1";
            label1.Size = new Size(42, 20);
            label1.TabIndex = 9;
            label1.Text = "Ilość:";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(20, 170);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(125, 27);
            textBox1.TabIndex = 13;
            // 
            // lblLoading
            // 
            lblLoading.AutoSize = true;
            lblLoading.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblLoading.ForeColor = Color.DarkBlue;
            lblLoading.Location = new Point(160, 175);
            lblLoading.Name = "lblLoading";
            lblLoading.Size = new Size(130, 23);
            lblLoading.TabIndex = 16;
            lblLoading.Text = "Ładowanie...";
            lblLoading.Visible = false;
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new Size(20, 20);
            statusStrip1.Items.AddRange(new ToolStripItem[] { statusLabel });
            statusStrip1.Location = new Point(0, 217);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(451, 25);
            statusStrip1.TabIndex = 17;
            statusStrip1.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(60, 20);
            statusLabel.Text = "Gotowy";
            //
            // radioDoubleSided
            //
            radioDoubleSided.AutoSize = true;
            radioDoubleSided.Location = new Point(217, 17);
            radioDoubleSided.Name = "radioDoubleSided";
            radioDoubleSided.Size = new Size(123, 24);
            radioDoubleSided.TabIndex = 18;
            radioDoubleSided.TabStop = true;
            radioDoubleSided.Text = "Dwustronna";
            radioDoubleSided.UseVisualStyleBackColor = true;
            //
            // radioSingleSided
            //
            radioSingleSided.AutoSize = true;
            radioSingleSided.Location = new Point(217, 47);
            radioSingleSided.Name = "radioSingleSided";
            radioSingleSided.Size = new Size(112, 24);
            radioSingleSided.TabIndex = 19;
            radioSingleSided.TabStop = true;
            radioSingleSided.Text = "Jednostronna";
            radioSingleSided.UseVisualStyleBackColor = true;
            // 
            // CreateReservationList
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(451, 242);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Controls.Add(textBox1);
            Controls.Add(ButtonImportExcelList);
            Controls.Add(label1);
            Controls.Add(lblListName);
            Controls.Add(button1);
            Controls.Add(label5);
            Controls.Add(lblLoading);
            Controls.Add(statusStrip1);
            Controls.Add(radioDoubleSided);
            Controls.Add(radioSingleSided);
            //Controls.Add(debug);
            Name = "CreateReservationList";
            Text = "CreateReservationList";
            Icon = new Icon("Resources/Icons/wolf_256x256.ico");
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button ButtonImportExcelList;
        private Label lblListName;
        private Label label5;
        private Button button1;
        private Label label1;
        private TextBox textBox1;
        private Label lblLoading;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel statusLabel;
        private RadioButton radioDoubleSided;
        private RadioButton radioSingleSided;
        //private Button debug;
    }
}