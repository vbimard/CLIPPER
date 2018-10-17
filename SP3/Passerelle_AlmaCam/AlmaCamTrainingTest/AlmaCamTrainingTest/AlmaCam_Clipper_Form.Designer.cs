namespace AlmaCamTrainingTest
{
    partial class AlmaCam_Clipper_Form
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AlmaCam_Clipper_Form));
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.AfterClose = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cAToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stockToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.donnéesTechniquesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sheetRequiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.retourPlacementToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stockToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.purgerLeStockToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quitToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.fichierToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.DoOnAction_TO_CUT_NESTING = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.quitToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.InfosLabel = new System.Windows.Forms.Label();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.menuStrip1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // button3
            // 
            this.button3.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.button3.BackgroundImage = global::AlmaCamTrainingTest.Properties.Resources.BUTTON;
            this.button3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.button3.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.button3.Location = new System.Drawing.Point(8, 76);
            this.button3.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(212, 47);
            this.button3.TabIndex = 2;
            this.button3.Text = "Import du Stock";
            this.button3.UseVisualStyleBackColor = false;
            this.button3.UseWaitCursor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.button4.BackgroundImage = global::AlmaCamTrainingTest.Properties.Resources.BUTTON;
            this.button4.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.button4.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.button4.Location = new System.Drawing.Point(8, 18);
            this.button4.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(212, 48);
            this.button4.TabIndex = 3;
            this.button4.Text = "Import Cahier Affaire";
            this.button4.UseVisualStyleBackColor = false;
            this.button4.UseWaitCursor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button9
            // 
            this.button9.BackColor = System.Drawing.Color.Cyan;
            this.button9.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("button9.BackgroundImage")));
            this.button9.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.button9.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.button9.Location = new System.Drawing.Point(8, 78);
            this.button9.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(212, 47);
            this.button9.TabIndex = 8;
            this.button9.Text = "Remonté les dossiers Techniques";
            this.button9.UseVisualStyleBackColor = false;
            this.button9.UseWaitCursor = true;
            this.button9.Visible = false;
            this.button9.Click += new System.EventHandler(this.button9_Click);
            // 
            // AfterClose
            // 
            this.AfterClose.BackColor = System.Drawing.Color.LimeGreen;
            this.AfterClose.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("AfterClose.BackgroundImage")));
            this.AfterClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.AfterClose.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.AfterClose.Location = new System.Drawing.Point(8, 24);
            this.AfterClose.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.AfterClose.Name = "AfterClose";
            this.AfterClose.Size = new System.Drawing.Size(212, 47);
            this.AfterClose.TabIndex = 11;
            this.AfterClose.Text = "Retour Placement Clos (DoOnAction_After_Cutting_end)";
            this.AfterClose.UseVisualStyleBackColor = false;
            this.AfterClose.UseWaitCursor = true;
            this.AfterClose.Visible = false;
            this.AfterClose.Click += new System.EventHandler(this.AfterClose_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.Color.Gray;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importToolStripMenuItem,
            this.exportToolStripMenuItem,
            this.stockToolStripMenuItem1,
            this.fileToolStripMenuItem,
            this.fichierToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(252, 24);
            this.menuStrip1.TabIndex = 12;
            this.menuStrip1.Text = "menuStrip1";
            this.menuStrip1.UseWaitCursor = true;
            // 
            // importToolStripMenuItem
            // 
            this.importToolStripMenuItem.BackColor = System.Drawing.Color.Gray;
            this.importToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cAToolStripMenuItem,
            this.stockToolStripMenuItem,
            this.quitToolStripMenuItem});
            this.importToolStripMenuItem.Name = "importToolStripMenuItem";
            this.importToolStripMenuItem.Size = new System.Drawing.Size(55, 20);
            this.importToolStripMenuItem.Text = "Import";
            this.importToolStripMenuItem.Visible = false;
            // 
            // cAToolStripMenuItem
            // 
            this.cAToolStripMenuItem.Name = "cAToolStripMenuItem";
            this.cAToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.cAToolStripMenuItem.Text = "CA";
            this.cAToolStripMenuItem.Click += new System.EventHandler(this.cAToolStripMenuItem_Click);
            // 
            // stockToolStripMenuItem
            // 
            this.stockToolStripMenuItem.Name = "stockToolStripMenuItem";
            this.stockToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.stockToolStripMenuItem.Text = "Stock";
            this.stockToolStripMenuItem.Click += new System.EventHandler(this.stockToolStripMenuItem_Click);
            // 
            // quitToolStripMenuItem
            // 
            this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            this.quitToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.quitToolStripMenuItem.Text = "Quit";
            this.quitToolStripMenuItem.Click += new System.EventHandler(this.quitToolStripMenuItem_Click);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.donnéesTechniquesToolStripMenuItem,
            this.sheetRequiToolStripMenuItem,
            this.retourPlacementToolStripMenuItem});
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.exportToolStripMenuItem.Text = "Export";
            this.exportToolStripMenuItem.Visible = false;
            // 
            // donnéesTechniquesToolStripMenuItem
            // 
            this.donnéesTechniquesToolStripMenuItem.Name = "donnéesTechniquesToolStripMenuItem";
            this.donnéesTechniquesToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.donnéesTechniquesToolStripMenuItem.Text = "Données Techniques";
            this.donnéesTechniquesToolStripMenuItem.Click += new System.EventHandler(this.donnéesTechniquesToolStripMenuItem_Click);
            // 
            // sheetRequiToolStripMenuItem
            // 
            this.sheetRequiToolStripMenuItem.Name = "sheetRequiToolStripMenuItem";
            this.sheetRequiToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.sheetRequiToolStripMenuItem.Text = "Sheet Requi.";
            this.sheetRequiToolStripMenuItem.Click += new System.EventHandler(this.sheetRequiToolStripMenuItem_Click);
            // 
            // retourPlacementToolStripMenuItem
            // 
            this.retourPlacementToolStripMenuItem.Name = "retourPlacementToolStripMenuItem";
            this.retourPlacementToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.retourPlacementToolStripMenuItem.Text = "Retour Placement";
            this.retourPlacementToolStripMenuItem.Click += new System.EventHandler(this.retourPlacementToolStripMenuItem_Click);
            // 
            // stockToolStripMenuItem1
            // 
            this.stockToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.purgerLeStockToolStripMenuItem});
            this.stockToolStripMenuItem1.Name = "stockToolStripMenuItem1";
            this.stockToolStripMenuItem1.Size = new System.Drawing.Size(48, 20);
            this.stockToolStripMenuItem1.Text = "Stock";
            // 
            // purgerLeStockToolStripMenuItem
            // 
            this.purgerLeStockToolStripMenuItem.Name = "purgerLeStockToolStripMenuItem";
            this.purgerLeStockToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.purgerLeStockToolStripMenuItem.Text = "Purger le Stock";
            this.purgerLeStockToolStripMenuItem.Click += new System.EventHandler(this.purgerLeStockToolStripMenuItem_Click);
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.quitToolStripMenuItem2});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // quitToolStripMenuItem2
            // 
            this.quitToolStripMenuItem2.Name = "quitToolStripMenuItem2";
            this.quitToolStripMenuItem2.Size = new System.Drawing.Size(97, 22);
            this.quitToolStripMenuItem2.Text = "Quit";
            this.quitToolStripMenuItem2.Click += new System.EventHandler(this.quitToolStripMenuItem2_Click);
            // 
            // fichierToolStripMenuItem
            // 
            this.fichierToolStripMenuItem.Name = "fichierToolStripMenuItem";
            this.fichierToolStripMenuItem.Size = new System.Drawing.Size(12, 20);
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.Color.Transparent;
            this.groupBox2.Controls.Add(this.DoOnAction_TO_CUT_NESTING);
            this.groupBox2.Controls.Add(this.AfterClose);
            this.groupBox2.Controls.Add(this.button9);
            this.groupBox2.Location = new System.Drawing.Point(9, 181);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(230, 196);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Export";
            this.groupBox2.UseWaitCursor = true;
            // 
            // DoOnAction_TO_CUT_NESTING
            // 
            this.DoOnAction_TO_CUT_NESTING.BackColor = System.Drawing.Color.LimeGreen;
            this.DoOnAction_TO_CUT_NESTING.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("DoOnAction_TO_CUT_NESTING.BackgroundImage")));
            this.DoOnAction_TO_CUT_NESTING.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.DoOnAction_TO_CUT_NESTING.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.DoOnAction_TO_CUT_NESTING.Location = new System.Drawing.Point(8, 127);
            this.DoOnAction_TO_CUT_NESTING.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.DoOnAction_TO_CUT_NESTING.Name = "DoOnAction_TO_CUT_NESTING";
            this.DoOnAction_TO_CUT_NESTING.Size = new System.Drawing.Size(212, 47);
            this.DoOnAction_TO_CUT_NESTING.TabIndex = 12;
            this.DoOnAction_TO_CUT_NESTING.Text = "Retour Placement Clos (DoOnAction_TO_CUT_NESTING)";
            this.DoOnAction_TO_CUT_NESTING.UseVisualStyleBackColor = false;
            this.DoOnAction_TO_CUT_NESTING.UseWaitCursor = true;
            this.DoOnAction_TO_CUT_NESTING.Click += new System.EventHandler(this.AfterSendToWorkshop_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.BackColor = System.Drawing.Color.Transparent;
            this.groupBox3.Controls.Add(this.button4);
            this.groupBox3.Controls.Add(this.button3);
            this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.groupBox3.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.groupBox3.Location = new System.Drawing.Point(9, 38);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(230, 136);
            this.groupBox3.TabIndex = 14;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Import";
            this.groupBox3.UseWaitCursor = true;
            // 
            // quitToolStripMenuItem1
            // 
            this.quitToolStripMenuItem1.Name = "quitToolStripMenuItem1";
            this.quitToolStripMenuItem1.Size = new System.Drawing.Size(152, 22);
            this.quitToolStripMenuItem1.Text = "Quit";
            this.quitToolStripMenuItem1.Click += new System.EventHandler(this.quitToolStripMenuItem1_Click);
            // 
            // InfosLabel
            // 
            this.InfosLabel.AutoSize = true;
            this.InfosLabel.BackColor = System.Drawing.Color.Gray;
            this.InfosLabel.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InfosLabel.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.InfosLabel.Location = new System.Drawing.Point(6, 380);
            this.InfosLabel.Name = "InfosLabel";
            this.InfosLabel.Size = new System.Drawing.Size(42, 15);
            this.InfosLabel.TabIndex = 15;
            this.InfosLabel.Text = "Infos";
            this.InfosLabel.UseWaitCursor = true;
            // 
            // notifyIcon
            // 
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "AlmaCam-Clipper";
            this.notifyIcon.Visible = true;
            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseDoubleClick);
            // 
            // AlmaCam_Clipper_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
            this.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.BackgroundImage = global::AlmaCamTrainingTest.Properties.Resources.FONDExport;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ClientSize = new System.Drawing.Size(252, 435);
            this.Controls.Add(this.InfosLabel);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.menuStrip1);
            this.Cursor = System.Windows.Forms.Cursors.WaitCursor;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Location = new System.Drawing.Point(1200, 400);
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.Name = "AlmaCam_Clipper_Form";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "AlmaCam_Clipper";
            this.UseWaitCursor = true;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem cAToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stockToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem donnéesTechniquesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sheetRequiToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stockToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem purgerLeStockToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem retourPlacementToolStripMenuItem;
        private System.Windows.Forms.Button AfterClose;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fichierToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem2;
        private System.Windows.Forms.Label InfosLabel;
        private System.Windows.Forms.Button DoOnAction_TO_CUT_NESTING;
        private System.Windows.Forms.NotifyIcon notifyIcon;
    }
}

