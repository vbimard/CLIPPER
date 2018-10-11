namespace AlmaCamTrainingTest
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.button10 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.AfterClose = new System.Windows.Forms.Button();
            this.button14 = new System.Windows.Forms.Button();
            this.bt_BeforeClose = new System.Windows.Forms.Button();
            this.button11 = new System.Windows.Forms.Button();
            this.button12 = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cAToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stockToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.donnéesTechniquesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sheetRequiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.retourPlacementToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stockToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.purgerLeStockToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.Red;
            this.button1.Location = new System.Drawing.Point(8, 137);
            this.button1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(300, 25);
            this.button1.TabIndex = 0;
            this.button1.Text = "Purger le stock";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Enabled = false;
            this.button2.Location = new System.Drawing.Point(10, 132);
            this.button2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(298, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "Import_Reference";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Visible = false;
            // 
            // button3
            // 
            this.button3.BackColor = System.Drawing.Color.Fuchsia;
            this.button3.Location = new System.Drawing.Point(12, 28);
            this.button3.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(298, 29);
            this.button3.TabIndex = 2;
            this.button3.Text = "Import du Stock";
            this.button3.UseVisualStyleBackColor = false;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.button4.Location = new System.Drawing.Point(12, 101);
            this.button4.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(298, 23);
            this.button4.TabIndex = 3;
            this.button4.Text = "Import Cahier Affaire";
            this.button4.UseVisualStyleBackColor = false;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.BackColor = System.Drawing.Color.GreenYellow;
            this.button5.Location = new System.Drawing.Point(9, 20);
            this.button5.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(300, 28);
            this.button5.TabIndex = 4;
            this.button5.Text = "Retour PLacement (DoOnAction_AfterSendToWorkshop)";
            this.button5.UseVisualStyleBackColor = false;
            this.button5.Visible = false;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button6
            // 
            this.button6.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.button6.Location = new System.Drawing.Point(9, 170);
            this.button6.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(300, 22);
            this.button6.TabIndex = 5;
            this.button6.Text = "Import_Stock";
            this.button6.UseVisualStyleBackColor = false;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(9, 206);
            this.button7.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(300, 19);
            this.button7.TabIndex = 6;
            this.button7.Text = "Import_auto_ca";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Visible = false;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // button8
            // 
            this.button8.BackColor = System.Drawing.Color.SpringGreen;
            this.button8.Enabled = false;
            this.button8.Location = new System.Drawing.Point(12, 226);
            this.button8.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(298, 22);
            this.button8.TabIndex = 7;
            this.button8.Text = "Do_OnAction_Before";
            this.button8.UseVisualStyleBackColor = false;
            this.button8.Visible = false;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // button9
            // 
            this.button9.BackColor = System.Drawing.Color.Cyan;
            this.button9.Location = new System.Drawing.Point(12, 163);
            this.button9.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(298, 23);
            this.button9.TabIndex = 8;
            this.button9.Text = "Remonté les dossiers Techniques";
            this.button9.UseVisualStyleBackColor = false;
            this.button9.Click += new System.EventHandler(this.button9_Click);
            // 
            // button10
            // 
            this.button10.BackColor = System.Drawing.Color.Cyan;
            this.button10.Location = new System.Drawing.Point(12, 194);
            this.button10.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(298, 24);
            this.button10.TabIndex = 9;
            this.button10.Text = "Test_Topo";
            this.button10.UseVisualStyleBackColor = false;
            this.button10.Visible = false;
            this.button10.Click += new System.EventHandler(this.button10_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.AfterClose);
            this.groupBox1.Controls.Add(this.button14);
            this.groupBox1.Controls.Add(this.bt_BeforeClose);
            this.groupBox1.Controls.Add(this.button11);
            this.groupBox1.Controls.Add(this.button7);
            this.groupBox1.Controls.Add(this.button6);
            this.groupBox1.Controls.Add(this.button5);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Location = new System.Drawing.Point(8, 255);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(315, 276);
            this.groupBox1.TabIndex = 10;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Tools";
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // AfterClose
            // 
            this.AfterClose.BackColor = System.Drawing.Color.LimeGreen;
            this.AfterClose.Location = new System.Drawing.Point(8, 98);
            this.AfterClose.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.AfterClose.Name = "AfterClose";
            this.AfterClose.Size = new System.Drawing.Size(300, 28);
            this.AfterClose.TabIndex = 11;
            this.AfterClose.Text = "Retour PLacement (DoOnAction_After_Cutting_end)";
            this.AfterClose.UseVisualStyleBackColor = false;
            this.AfterClose.Click += new System.EventHandler(this.AfterClose_Click);
            // 
            // button14
            // 
            this.button14.BackColor = System.Drawing.Color.GreenYellow;
            this.button14.Location = new System.Drawing.Point(9, 73);
            this.button14.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button14.Name = "button14";
            this.button14.Size = new System.Drawing.Size(300, 28);
            this.button14.TabIndex = 10;
            this.button14.Text = "Retour PLacement (Do_OnAction_Onorkshop)";
            this.button14.UseVisualStyleBackColor = false;
            this.button14.Visible = false;
            this.button14.Click += new System.EventHandler(this.button14_Click);
            // 
            // bt_BeforeClose
            // 
            this.bt_BeforeClose.BackColor = System.Drawing.Color.LimeGreen;
            this.bt_BeforeClose.Location = new System.Drawing.Point(9, 46);
            this.bt_BeforeClose.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.bt_BeforeClose.Name = "bt_BeforeClose";
            this.bt_BeforeClose.Size = new System.Drawing.Size(300, 28);
            this.bt_BeforeClose.TabIndex = 8;
            this.bt_BeforeClose.Text = "Before closing the Nesting (DoOnAction_Before_Cutting)";
            this.bt_BeforeClose.UseVisualStyleBackColor = false;
            this.bt_BeforeClose.Visible = false;
            this.bt_BeforeClose.Click += new System.EventHandler(this.bt_BeforeClose_Click);
            // 
            // button11
            // 
            this.button11.Location = new System.Drawing.Point(8, 232);
            this.button11.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button11.Name = "button11";
            this.button11.Size = new System.Drawing.Size(300, 19);
            this.button11.TabIndex = 7;
            this.button11.Text = "Test";
            this.button11.UseVisualStyleBackColor = true;
            this.button11.Visible = false;
            this.button11.Click += new System.EventHandler(this.button11_Click);
            // 
            // button12
            // 
            this.button12.BackColor = System.Drawing.Color.Fuchsia;
            this.button12.Location = new System.Drawing.Point(12, 65);
            this.button12.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button12.Name = "button12";
            this.button12.Size = new System.Drawing.Size(298, 28);
            this.button12.TabIndex = 11;
            this.button12.Text = "Sheet_Requirements";
            this.button12.UseVisualStyleBackColor = false;
            this.button12.Click += new System.EventHandler(this.button12_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importToolStripMenuItem,
            this.exportToolStripMenuItem,
            this.stockToolStripMenuItem1});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(328, 24);
            this.menuStrip1.TabIndex = 12;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // importToolStripMenuItem
            // 
            this.importToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cAToolStripMenuItem,
            this.stockToolStripMenuItem});
            this.importToolStripMenuItem.Name = "importToolStripMenuItem";
            this.importToolStripMenuItem.Size = new System.Drawing.Size(55, 20);
            this.importToolStripMenuItem.Text = "Import";
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
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.donnéesTechniquesToolStripMenuItem,
            this.sheetRequiToolStripMenuItem,
            this.retourPlacementToolStripMenuItem});
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.exportToolStripMenuItem.Text = "Export";
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
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(328, 539);
            this.Controls.Add(this.button12);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button10);
            this.Controls.Add(this.button9);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.menuStrip1);
            this.Cursor = System.Windows.Forms.Cursors.Default;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.Button button10;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button11;
        private System.Windows.Forms.Button button12;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cAToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stockToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem donnéesTechniquesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sheetRequiToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stockToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem purgerLeStockToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem retourPlacementToolStripMenuItem;
        private System.Windows.Forms.Button bt_BeforeClose;
        private System.Windows.Forms.Button button14;
        private System.Windows.Forms.Button AfterClose;
    }
}

