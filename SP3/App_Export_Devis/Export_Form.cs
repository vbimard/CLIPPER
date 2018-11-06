using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AF_Export_Devis_Clipper;
using Wpm.Implement.Manager;

using AF_ImportTools;
using Actcut.QuoteModelManager;
using Wpm.Schema.Kernel;
using System.Drawing.Imaging;
using System.IO;

//namespace WindowsFormsApp1
    namespace AF_Export_Devis_Clipper
{     
    public partial class Export_Form : Form
    {
        IContext contextlocal;

        public Export_Form()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
          
            //IContext contextlocal;
            IEntity quote=null;
                       

           
            string database;
            database = comboDataBaseList.Text;///txt_Database.Text;

            status.Text = "Connecting database";

            contextlocal = AF_ImportTools.DataBase.Connect(ref database);
            status.Text = "Connected";
            

            quote = contextlocal.EntityManager.GetEntity(Convert.ToInt64(txtQuoteid.Text), "_QUOTE_REQUEST"); //AF_ImportTools.SimplifiedMethods.GetFirtOfList(quotes);
            ITransaction transaction = contextlocal.CreateTransaction();
            IQuote iquote = new Quote(transaction, quote);


            status.Text = "Generating Quot...";
            if (iquote.QuotePartList.Count()>0) { 
            AF_Export_Devis_Clipper.ExportQuote.ExportQuoteRequest(contextlocal, iquote);
            linkemf.Text = quote.Context.ParameterSetManager.GetParameterValue("_EXPORT", "_ACTCUT_DPR_DIRECTORY").GetValueAsString();
            linkquote.Text = quote.Context.ParameterSetManager.GetParameterValue("_EXPORT", "_EXPORT_GP_DIRECTORY").GetValueAsString();

                status.Text = "Done";

            }
            else
            {
                status.Text = txtQuoteid.Text + " n'existe pas";
            }

            
            iquote = null;
            contextlocal = null;

        }

        private void Export_Form_Load(object sender, EventArgs e)
        {
            
            //IEntity quote = null;
            List<IModel> listModel = new List<IModel> ();
          
            
            listModel = AF_ImportTools.DataBase.GetDataBaseList();
           
            foreach (IModel model in listModel)
            {
                comboDataBaseList.Items.Add(model.DatabaseName);
            } 

            
        }

        private void linkquote_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe " ,linkquote.Text);
        }

        private void linkemf_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe ", linkemf.Text);
        }




        //startProcess

        /* //export d'un devis
    //recuperation de la base
    string dbname;
    dbname =  txt_Database.Text;
    IContext Context = ImportTools.DataBase.Connect(ref dbname);*/


    }
}
