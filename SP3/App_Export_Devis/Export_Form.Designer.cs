namespace AF_Export_Devis_Clipper
{
    partial class Export_Form
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.txtQuoteid = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.status = new System.Windows.Forms.Label();
            this.comboDataBaseList = new System.Windows.Forms.ComboBox();
            this.linkquote = new System.Windows.Forms.LinkLabel();
            this.linkemf = new System.Windows.Forms.LinkLabel();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "IdDevis";
            // 
            // txtQuoteid
            // 
            this.txtQuoteid.Location = new System.Drawing.Point(78, 20);
            this.txtQuoteid.Name = "txtQuoteid";
            this.txtQuoteid.Size = new System.Drawing.Size(100, 20);
            this.txtQuoteid.TabIndex = 1;
            this.txtQuoteid.Text = "6";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(219, 20);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(140, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "Export_For Clipper";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // status
            // 
            this.status.AutoSize = true;
            this.status.Location = new System.Drawing.Point(75, 87);
            this.status.Name = "status";
            this.status.Size = new System.Drawing.Size(16, 13);
            this.status.TabIndex = 4;
            this.status.Text = "...";
            // 
            // comboDataBaseList
            // 
            this.comboDataBaseList.FormattingEnabled = true;
            this.comboDataBaseList.Location = new System.Drawing.Point(78, 46);
            this.comboDataBaseList.Name = "comboDataBaseList";
            this.comboDataBaseList.Size = new System.Drawing.Size(281, 21);
            this.comboDataBaseList.TabIndex = 5;
            // 
            // linkquote
            // 
            this.linkquote.AutoSize = true;
            this.linkquote.Location = new System.Drawing.Point(55, 111);
            this.linkquote.Name = "linkquote";
            this.linkquote.Size = new System.Drawing.Size(45, 13);
            this.linkquote.TabIndex = 7;
            this.linkquote.TabStop = true;
            this.linkquote.Text = "C:\\temp";
            this.linkquote.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkquote_LinkClicked);
            // 
            // linkemf
            // 
            this.linkemf.AutoSize = true;
            this.linkemf.Location = new System.Drawing.Point(55, 133);
            this.linkemf.Name = "linkemf";
            this.linkemf.Size = new System.Drawing.Size(45, 13);
            this.linkemf.TabIndex = 8;
            this.linkemf.TabStop = true;
            this.linkemf.Text = "C:\\temp";
            this.linkemf.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkemf_LinkClicked);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 133);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(28, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Emf:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 111);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Devis:";
            // 
            // Export_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(372, 172);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.linkemf);
            this.Controls.Add(this.linkquote);
            this.Controls.Add(this.comboDataBaseList);
            this.Controls.Add(this.status);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.txtQuoteid);
            this.Controls.Add(this.label1);
            this.Name = "Export_Form";
            this.Text = "Export_Devis";
            this.Load += new System.EventHandler(this.Export_Form_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtQuoteid;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label status;
        private System.Windows.Forms.ComboBox comboDataBaseList;
        private System.Windows.Forms.LinkLabel linkquote;
        private System.Windows.Forms.LinkLabel linkemf;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
    }
}

