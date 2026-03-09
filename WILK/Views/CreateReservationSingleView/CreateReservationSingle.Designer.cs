namespace WILK.Views
{
    partial class CreateReservationSingle
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
            button1 = new Button();
            label1 = new Label();
            textBox1 = new TextBox();
            textBox2 = new TextBox();
            label2 = new Label();
            labelComponentName = new Label();
            lblLoading = new Label();
            statusStrip1 = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(231, 149);
            button1.Name = "button1";
            button1.Size = new Size(133, 52);
            button1.TabIndex = 1;
            button1.Text = "Rezerwacja komponentu";
            button1.UseVisualStyleBackColor = true;
            //
            // labelComponentName
            //
            labelComponentName.AutoSize = true;
            labelComponentName.Location = new Point(12, 50);
            labelComponentName.Name = "labelComponentName";
            labelComponentName.Size = new Size(50, 20);
            labelComponentName.TabIndex = 6;
            labelComponentName.Text = "";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 93);
            label1.Name = "label1";
            label1.Size = new Size(39, 20);
            label1.TabIndex = 5;
            label1.Text = "Ilość";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(128, 6);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(125, 27);
            textBox1.TabIndex = 2;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(128, 93);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(125, 27);
            textBox2.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 9);
            label2.Name = "label2";
            label2.Size = new Size(110, 20);
            label2.TabIndex = 4;
            label2.Text = "Id komponentu";
            // 
            // lblLoading
            // 
            lblLoading.AutoSize = true;
            lblLoading.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblLoading.ForeColor = Color.DarkBlue;
            lblLoading.Location = new Point(12, 170);
            lblLoading.Name = "lblLoading";
            lblLoading.Size = new Size(130, 23);
            lblLoading.TabIndex = 7;
            lblLoading.Text = "Zapisywanie...";
            lblLoading.Visible = false;
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new Size(20, 20);
            statusStrip1.Items.AddRange(new ToolStripItem[] { statusLabel });
            statusStrip1.Location = new Point(0, 213);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(376, 25);
            statusStrip1.TabIndex = 8;
            statusStrip1.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(60, 20);
            statusLabel.Text = "Gotowy";
            // 
            // CreateReservationSingle
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(376, 238);
            Controls.Add(button1);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(textBox2);
            Controls.Add(textBox1);
            Controls.Add(labelComponentName);
            Controls.Add(lblLoading);
            Controls.Add(statusStrip1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "CreateReservationSingle";
            Text = "CreateReservationSingle";
            Icon = new Icon("Resources/Icons/wolf_256x256.ico");
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private Label label1;
        private TextBox textBox1;
        private TextBox textBox2;
        private Label label2;
        private Label labelComponentName;
        private Label lblLoading;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel statusLabel;
    }
}