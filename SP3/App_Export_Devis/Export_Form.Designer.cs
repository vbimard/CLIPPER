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
            // Export_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(398, 109);
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
    }
}

