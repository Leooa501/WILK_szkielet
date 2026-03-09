namespace WILK.Views
{
    partial class EditReservationList
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
            LabelTop = new Label();
            LabelBot = new Label();
            ButtonEditFileTop = new Button();
            ButtonEditFileBot = new Button();
            ButtonSkip = new Button();
            ButtonOk = new Button();
            lblLoading = new Label();
            // 
            // LabelTop
            // 
            LabelTop.AutoSize = true;
            LabelTop.Location = new Point(12, 10);
            LabelTop.MaximumSize = new Size(550, 0);
            LabelTop.AutoSize = true;
            LabelTop.Name = "LabelTop";
            LabelTop.Size = new Size(50, 20);
            LabelTop.TabIndex = 0;
            LabelTop.Text = "label1";
            // 
            // LabelBot
            // 
            LabelBot.AutoSize = true;
            LabelBot.Location = new Point(12, 100);
            LabelBot.MaximumSize = new Size(550, 0);
            LabelBot.AutoSize = true;
            LabelBot.Name = "LabelBot";
            LabelBot.Size = new Size(50, 20);
            LabelBot.TabIndex = 1;
            LabelBot.Text = "label2";
            // 
            // ButtonEditFileTop
            // 
            ButtonEditFileTop.Location = new Point(12, 55);
            ButtonEditFileTop.Name = "ButtonEditFileTop";
            ButtonEditFileTop.Size = new Size(150, 35);
            ButtonEditFileTop.TabIndex = 2;
            ButtonEditFileTop.Text = "Edytuj plik TOP";
            ButtonEditFileTop.UseVisualStyleBackColor = true;
            // 
            // ButtonEditFileBot
            // 
            ButtonEditFileBot.Location = new Point(12, 145);
            ButtonEditFileBot.Name = "ButtonEditFileBot";
            ButtonEditFileBot.Size = new Size(150, 35);
            ButtonEditFileBot.TabIndex = 3;
            ButtonEditFileBot.Text = "Edytuj plik BOT";
            ButtonEditFileBot.UseVisualStyleBackColor = true;
            // 
            // ButtonSkip
            // 
            ButtonSkip.Location = new Point(12, 200);
            ButtonSkip.Name = "ButtonSkip";
            ButtonSkip.Size = new Size(150, 40);
            ButtonSkip.TabIndex = 4;
            ButtonSkip.Text = "Dodaj później";
            ButtonSkip.UseVisualStyleBackColor = true;
            ButtonSkip.Visible = false;
            // 
            // ButtonOk
            // 
            ButtonOk.Location = new Point(412, 200);
            ButtonOk.Name = "ButtonOk";
            ButtonOk.Size = new Size(150, 40);
            ButtonOk.TabIndex = 5;
            ButtonOk.Text = "OK";
            ButtonOk.UseVisualStyleBackColor = true;
            // 
            // lblLoading
            // 
            lblLoading.AutoSize = true;
            lblLoading.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblLoading.ForeColor = Color.DarkBlue;
            lblLoading.Location = new Point(180, 210);
            lblLoading.Name = "lblLoading";
            lblLoading.Size = new Size(130, 23);
            lblLoading.TabIndex = 6;
            lblLoading.Text = "Ładowanie...";
            lblLoading.Visible = false;
            // 
            // EditReservationList
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            ClientSize = new Size(574, 252);
            Controls.Add(ButtonOk);
            Controls.Add(ButtonSkip);
            Controls.Add(ButtonEditFileBot);
            Controls.Add(ButtonEditFileTop);
            Controls.Add(LabelBot);
            Controls.Add(LabelTop);
            Controls.Add(lblLoading);
            Name = "EditReservationList";
            Text = "Edycja Rezerwacji";
            Icon = new Icon("Resources/Icons/wolf_256x256.ico");
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label LabelTop;
        private Label LabelBot;
        private Button ButtonEditFileTop;
        private Button ButtonEditFileBot;
        private Button ButtonSkip;
        private Button ButtonOk;
        private Label lblLoading;
    }
}