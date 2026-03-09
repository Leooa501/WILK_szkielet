using Google.Protobuf.Collections;

namespace WILK.Views
{
    partial class MainForm
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
            // Dispose TabManager from the main partial class
            if (disposing && _tabManager != null)
            {
                _tabManager.Dispose();
                _tabManager = null;
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
            SuspendLayout();
            // 
            // statusStrip
            // 
            statusStrip = new StatusStrip();
            versionLabel = new ToolStripStatusLabel();
            statusStrip.Items.Add(versionLabel);
            statusStrip.Dock = DockStyle.Bottom;
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(1214, 22);
            statusStrip.TabIndex = 0;
            // 
            // versionLabel
            // 
            versionLabel.Name = "versionLabel";
            versionLabel.Text = "Wersja: ";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1214, 676);
            Controls.Add(statusStrip);
            MinimumSize = new Size(1300, 800);
            Name = "MainForm";
            Text = "WILK";
            Icon = new Icon("Resources/Icons/wolf_256x256.ico");
            ResumeLayout(false);
        }

        #endregion

        private StatusStrip statusStrip;
        private ToolStripStatusLabel versionLabel;
#if THT
        private TabControl hiddenTabControl;
#endif
    }
}
