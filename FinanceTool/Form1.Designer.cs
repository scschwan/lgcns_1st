namespace FinanceTool
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            menuStrip1 = new MenuStrip();
            fileLoadToolStripMenuItem = new ToolStripMenuItem();
            dataPreprocessingToolStripMenuItem = new ToolStripMenuItem();
            dataAnalToolStripMenuItem = new ToolStripMenuItem();
            classificationToolStripMenuItem = new ToolStripMenuItem();
            exportToolStripMenuItem = new ToolStripMenuItem();
            mainPanel = new Panel();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileLoadToolStripMenuItem, dataPreprocessingToolStripMenuItem, dataAnalToolStripMenuItem, classificationToolStripMenuItem, exportToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1904, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            menuStrip1.Dock = DockStyle.Top;
            // 
            // fileLoadToolStripMenuItem
            // 
            fileLoadToolStripMenuItem.Name = "fileLoadToolStripMenuItem";
            fileLoadToolStripMenuItem.Size = new Size(67, 20);
            fileLoadToolStripMenuItem.Text = "File Load";
            fileLoadToolStripMenuItem.Click += fileLoadToolStripMenuItem_Click;
            // 
            // dataPreprocessingToolStripMenuItem
            // 
            dataPreprocessingToolStripMenuItem.Name = "dataPreprocessingToolStripMenuItem";
            dataPreprocessingToolStripMenuItem.Size = new Size(122, 20);
            dataPreprocessingToolStripMenuItem.Text = "Data Preprocessing";
            dataPreprocessingToolStripMenuItem.Click += dataPreprocessingToolStripMenuItem_Click;
            // 
            // dataAnalToolStripMenuItem
            // 
            dataAnalToolStripMenuItem.Name = "dataAnalToolStripMenuItem";
            dataAnalToolStripMenuItem.Size = new Size(102, 20);
            dataAnalToolStripMenuItem.Text = "Data Transform";
            dataAnalToolStripMenuItem.Click += dataAnalToolStripMenuItem_Click;
            // 
            // classificationToolStripMenuItem
            // 
            classificationToolStripMenuItem.Name = "classificationToolStripMenuItem";
            classificationToolStripMenuItem.Size = new Size(73, 20);
            classificationToolStripMenuItem.Text = "Clustering";
            classificationToolStripMenuItem.Click += classificationToolStripMenuItem_Click;
            // 
            // exportToolStripMenuItem
            // 
            exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            exportToolStripMenuItem.Size = new Size(53, 20);
            exportToolStripMenuItem.Text = "Export";
            exportToolStripMenuItem.Click += exportToolStripMenuItem_Click;
            // 
            // mainPanel
            // 
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Location = new Point(0, 24);
            mainPanel.Name = "mainPanel";
            mainPanel.Size = new Size(1904, 1017);
            mainPanel.TabIndex = 1;
            mainPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            mainPanel.Padding = new Padding(5);
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            MinimumSize = new Size(800, 600);
            ClientSize = new Size(1904, 1041);
            Controls.Add(mainPanel);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "Form1";
            Text = "Finance Tool";
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private Panel mainPanel;
        private ToolStripMenuItem fileLoadToolStripMenuItem;
        private ToolStripMenuItem dataPreprocessingToolStripMenuItem;
        private ToolStripMenuItem dataAnalToolStripMenuItem;
        private ToolStripMenuItem classificationToolStripMenuItem;
        private ToolStripMenuItem exportToolStripMenuItem;
    }
}
