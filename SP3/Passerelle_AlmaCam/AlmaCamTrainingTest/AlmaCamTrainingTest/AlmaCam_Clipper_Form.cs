using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Wpm.Implement.Manager;
using Wpm.Implement.Processor;
using Wpm.Implement.ComponentEditor;  // ouverture de fenetres de selection almacam
using Wpm.Schema.Kernel;

//using System.ComponentModel;
//using Alma.BaseUI.Utils;





//dll personnalisées
using AF_Clipper_Dll;
using AF_ImportTools;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;

//suppression de wpm.schema.component.dll et remplacement par wpm.schema.componenteditor.dll 

namespace AlmaCamTrainingTest
{




    public partial class AlmaCam_Clipper_Form : Form
    {

        //initialisation des listes
        IContext _Context = null;
        
        string DbName = Alma_RegitryInfos.GetLastDataBase();

        public AlmaCam_Clipper_Form()
        {
            try
            {

                InitializeComponent();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

        }
        /// <summary>
        /// icone notification,
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 

            
        private void Form1_Resize(object sender, EventArgs e)
        {

        }


        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon.Visible = false;
        }

        /// <summary>
        /// form load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {


            

            //creation du model repository
            IModelsRepository modelsRepository = new ModelsRepository();
            _Context = modelsRepository.GetModelContext(DbName);  //nom de la base;
            int i = _Context.ModelsRepository.ModelList.Count();
            string infosPasserelle;
            
            infosPasserelle= DbName + "-P." + AF_Clipper_Dll.Clipper_Param.getClipperDllVersion() + "-CAM." + AF_Clipper_Dll.Clipper_Param.getAlmaCAMCompatibleVerion();
            this.Text = this.Name;
            this.InfosLabel.Text = infosPasserelle;
            this.Text = "Passerelle Clipper V8 validée pour : " + infosPasserelle;
        }

     
        private void button1_Click(object sender, EventArgs e)
        {//purge stock
            IEntityList stocks = _Context.EntityManager.GetEntityList("_STOCK");
            stocks.Fill(false);
            DialogResult res = MessageBox.Show("Do you really want to destroy all sheets from the stock?", "Warnig", MessageBoxButtons.OKCancel);
            if (res == DialogResult.OK)
            {
                foreach (IEntity stock in stocks)
                {
                    stock.Delete();
                }


                IEntityList formats = _Context.EntityManager.GetEntityList("_SHEET");
                formats.Fill(false);

                foreach (IEntity format in formats)
                {
                    format.Delete();
                }

            }
        }
    
        private void button3_Click(object sender, EventArgs e)
        {
            
            using (Clipper_Stock Stock = new Clipper_Stock())
            {
                //Stock.Import(_Context, csvImportPath, dataModelstring);
                Stock.Import(_Context);//, csvImportPath);
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {

            //import of
            //chargement de sparamètres
            // bool SansDt=false;

            Clipper_Param.GetlistParam(_Context);
            string csvImportPath = Clipper_Param.GetPath("IMPORT_CDA");
            //recuperation du nom de fichier
            string csvFileName = Path.GetFileNameWithoutExtension(csvImportPath);
            string csvDirectory = Path.GetDirectoryName(csvImportPath);
            string csvImportSandDt = csvDirectory + "\\" + csvFileName + "_SANSDT.csv";
          

            string dataModelstring = Clipper_Param.GetModelCA();


            using (Clipper_OF CahierAffaire = new Clipper_OF())
            {
                CahierAffaire.Import(_Context, csvImportPath, dataModelstring, false);
                CahierAffaire.Import(_Context, csvImportSandDt, dataModelstring, true);
                //}

            }


        }


        /// <summary>
        /// import of
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {

            /*
            //private void button8_Click(object sender, EventArgs e)

            Clipper_Dll.Clipper_DoOnAction_AfterSendToWorkshop doonaction = new Clipper_DoOnAction_AfterSendToWorkshop();
            doonaction.execute(_Context);
            //doonaction.OnAfterSendToWorkshopEvent(_Context ,"" );
            */


        }

        private void button6_Click(object sender, EventArgs e)
        {
            ///for test
            ///
            ///
            ///Pour la passerelle « ALMACAM », je vous fournirai un executable pour l’import automatique du stock et du cahier d’affaire en suivant la meme logique.
            ///Chemin de l’executable paramétable et paramétré par defaut dans
            ///

            Clipper_Param.GetlistParam(_Context);
            string csvImportPath = Clipper_Param.GetPath("IMPORT_DM");
            ProcessStartInfo start_dm = new ProcessStartInfo();
            start_dm.Arguments = "stock " + csvImportPath;
            //start.FileName =  @"C:\AlmaCAM\Bin\AlmaCamUser1.exe";
            start_dm.FileName = Clipper_Param.get_application1();
            //start.WindowStyle = ProcessWindowStyle.Normal;
            start_dm.CreateNoWindow = true;
            start_dm.UseShellExecute = true;
            System.Diagnostics.Process.Start(start_dm);


        }

        private void button7_Click(object sender, EventArgs e)
        {
            Clipper_Param.GetlistParam(_Context);

            string csvImportPath = Clipper_Param.GetPath("IMPORT_CDA");
            ProcessStartInfo start = new ProcessStartInfo();
            ProcessStartInfo start_ca = new ProcessStartInfo();
            start_ca.Arguments = "OF " + csvImportPath;
            //start.FileName = @"C:\AlmaCAM\Bin\Clipper_Import.exe";
            start_ca.FileName = Clipper_Param.get_application1();
            //start.WindowStyle = ProcessWindowStyle.Normal;
            start_ca.CreateNoWindow = false;
            start_ca.UseShellExecute = true;
            string exename = Clipper_Param.get_application1();
            Process p = Process.Start(exename, "OF " + csvImportPath);


        }

        private void button8_Click(object sender, EventArgs e)
        {
            /*
            Clipper_Dll.Clipper_DoOnAction_BeforeSendToWorkshop doonaction = new Clipper_DoOnAction_BeforeSendToWorkshop();
            doonaction.OnBeforeSendToWorkshopEvent(_Context ,null );
            */


        }

        private void button9_Click(object sender, EventArgs e)
        {

            AF_Clipper_Dll.Clipper_Export_DT Export_dt = new AF_Clipper_Dll.Clipper_Export_DT();
            Export_dt.Execute();
            /*
            Clipper_Dll.Clipper_RemonteeDt Remontee_Dt = new Clipper_Dll.Clipper_RemonteeDt();
            Remontee_Dt.Export_Piece_To_File(_Context);*/
        }




        private void button10_Click(object sender, EventArgs e)
        {///creation devis
            //Clipper_Dll.Test_quote testquote = new Clipper_Dll.Test_quote();
            //testquote.createnewquote(_Context);
            IEntity mpart = null;
            IEntityList mparts = _Context.EntityManager.GetEntityList("_MACHINABLE_PART");
            mparts.Fill(false);
            mpart = mparts.FirstOrDefault();
            Topologie topo = new Topologie();
            topo.GetTopologie(ref mpart, ref _Context);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            long decalage;
            decalage = _Context.ParameterSetManager.GetParameterValue("_EXPORT", "_CLIPPER_QUOTE_NUMBER_OFFSET").GetValueAsLong();

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void button12_Click(object sender, EventArgs e)
        {
            //appel de la lib d'export des besoins ici
            using (Clipper_Sheet_Requirement Sheet_requirement = new Clipper_Sheet_Requirement())
            {
                Sheet_requirement.Export(_Context);//), csvImportPath);
            }

        }

        private void cAToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void sheetRequiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //appel de la lib d'export des besoins ici
            using (Clipper_Sheet_Requirement Sheet_requirement = new Clipper_Sheet_Requirement())
            {
                Sheet_requirement.Export(_Context);//), csvImportPath);
            }

        }

        private void donnéesTechniquesToolStripMenuItem_Click(object sender, EventArgs e)
        {

            AF_Clipper_Dll.Clipper_Export_DT Export_dt = new AF_Clipper_Dll.Clipper_Export_DT();
            Export_dt.Execute();

        }

        private void purgerLeStockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //purge stock
            IEntityList stocks = _Context.EntityManager.GetEntityList("_STOCK");
            stocks.Fill(false);
            DialogResult res = MessageBox.Show("Do you really want to destroy all sheets from the stock?", "Warnig", MessageBoxButtons.OKCancel);
            if (res == DialogResult.OK)
            {
                foreach (IEntity stock in stocks)
                {
                    stock.Delete();
                }


                IEntityList formats = _Context.EntityManager.GetEntityList("_SHEET");
                formats.Fill(false);

                foreach (IEntity format in formats)
                {
                    format.Delete();
                }

            }
        }

        private void stockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //import_stock
            //chargement de sparamètres
            //Clipper_Param.GetlistParam(_Context);
            //string csvImportPath = Clipper_Param.GetPath("IMPORT_DM");
            //string dataModelstring = Clipper_Param.GetModelDM();
            //"DISPO_MAT.csv"
            using (Clipper_Stock Stock = new Clipper_Stock())
            {
                //Stock.Import(_Context, csvImportPath, dataModelstring);
                Stock.Import(_Context);//, csvImportPath);
            }
        }

        private void retourPlacementToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*

            //private void button8_Click(object sender, EventArgs e)

            Clipper_Dll.Clipper_DoOnAction_AfterSendToWorkshop doonaction = new Clipper_DoOnAction_AfterSendToWorkshop();
            doonaction.execute(_Context);
            //doonaction.OnAfterSendToWorkshopEvent(_Context ,"" );
            */

        }

        private void bt_BeforeClose_Click(object sender, EventArgs e)
        {
            /*
            Clipper_DoOnAction_Before_Cutting_End doonaction = new Clipper_DoOnAction_Before_Cutting_End();
            doonaction.execute(_Context);
            ///*/

        }

        private void button14_Click(object sender, EventArgs e)
        {
            /*
            Clipper_Dll.Clipper_DoOnAction_From_WorkShop doonaction = new Clipper_DoOnAction_From_WorkShop();
            doonaction.Export(_Context);*/
        }

        private void AfterClose_Click(object sender, EventArgs e)
        {
      
            AF_Clipper_Dll.Clipper_DoOnAction_After_Cutting_end doonaction = new Clipper_DoOnAction_After_Cutting_end();

             //string stage = "_CLOSED_NESTING";
            string stage = "_CUT_SHEET";
            //creation du fichier de sortie
            //recupere les path
            Clipper_Param.GetlistParam(_Context);
            IEntitySelector Entityselector = null;

            Entityselector = new EntitySelector();

            //entity type pointe sur la list d'objet du model
            Entityselector.Init(_Context, _Context.Kernel.GetEntityType(stage));
            Entityselector.MultiSelect = true;

            if (Entityselector.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
             
                    doonaction.execute(Entityselector.SelectedEntity);
                

              
            }
            

            

        }


        private IEntity SelectEntity(string SelectionType)
        {
            IEntity selectedentity = null;
            IEntitySelector xselector = null;
            xselector = new EntitySelector();

            xselector.Init(_Context, _Context.Kernel.GetEntityType(SelectionType));//"_TO_CUT_NESTING"
            xselector.MultiSelect = true;

            if (xselector.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (IEntity xentity in xselector.SelectedEntity)
                {
                    selectedentity = xentity;

                }

            }

            return selectedentity;

        }
        private IEntity SelectEntity_If_Not_Exported(string entitystage)
        {
            IEntity selectedentity = null;
            IEntitySelector xselector = null;
            xselector = new EntitySelector();



            //GPAO_Exported
            //_Context.EntityManager.GetEntityList("_CLOSED_NESTING"entitystage, "GPAO_Exported", ConditionOperator.Equal, null);
            IEntityList GPAO_Exported_filter = _Context.EntityManager.GetEntityList(entitystage, "GPAO_Exported", ConditionOperator.Equal, null);
            GPAO_Exported_filter.Fill(false);

            _Context.EntityManager.GetExtendedEntityList(entitystage, GPAO_Exported_filter);
            IExtendedEntityList exported_nestings = _Context.EntityManager.GetExtendedEntityList(entitystage, GPAO_Exported_filter);
            exported_nestings.Fill(false);

            if (exported_nestings.Count == 0)
            {
                //_Context.EntityManager.GetEntityList("_CLOSED_NESTING"entitystage, "GPAO_Exported", ConditionOperator.Equal, false);
                GPAO_Exported_filter = _Context.EntityManager.GetEntityList(entitystage, "GPAO_Exported", ConditionOperator.Equal, false);
                GPAO_Exported_filter.Fill(false);
               // _Context.EntityManager.GetExtendedEntityList("_CLOSED_NESTING"entitystage, GPAO_Exported_filter);
                exported_nestings = _Context.EntityManager.GetExtendedEntityList(entitystage, GPAO_Exported_filter);
                exported_nestings.Fill(false);
            }

            xselector.Init(_Context, exported_nestings);//"_TO_CUT_NESTING"
            xselector.MultiSelect = true;

            if (xselector.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (IEntity xentity in xselector.SelectedEntity)
                {
                    selectedentity = xentity;

                }

            }

            return selectedentity;
        
    
              /*
            else
            {
                xselector.Init(_Context, _Context.Kernel.GetEntityType(entitytype));//"_TO_CUT_NESTING"
                xselector.MultiSelect = true;

            }*/

          

            

        }

        private void button13_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
           
        }

       

        private void quitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            
        }

        private void quitToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void AfterSendToWorkshop_Click(object sender, EventArgs e)
        {
            
            //IEntity TO_CUT_nesting;

            AF_Clipper_Dll.Clipper_DoOnAction_AfterSendToWorkshop doonaction= new Clipper_DoOnAction_AfterSendToWorkshop();
            string stage = "_TO_CUT_NESTING";

            //creation du fichier de sortie
            //recupere les path
            Clipper_Param.GetlistParam(_Context);
            IEntitySelector nestingselector = null;

            nestingselector = new EntitySelector();

            //entity type pointe sur la list d'objet du model
            nestingselector.Init(_Context, _Context.Kernel.GetEntityType(stage));
            nestingselector.MultiSelect = true;

            
            if (nestingselector.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (IEntity nesting in nestingselector.SelectedEntity)
                {
                    doonaction.execute( nesting);

                }
            }

            
           // doonaction.execute(_Context, TO_CUT_nesting);

          
        }

        private void purgerStockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IEntityList stocks = _Context.EntityManager.GetEntityList("_STOCK");
            stocks.Fill(false);
            DialogResult res = MessageBox.Show("Do you really want to destroy all sheets from the stock?", "Warnig", MessageBoxButtons.OKCancel);
            if (res == DialogResult.OK)
            {
                foreach (IEntity stock in stocks)
                {
                    stock.Delete();
                }


                IEntityList formats = _Context.EntityManager.GetEntityList("_SHEET");
                formats.Fill(false);

                foreach (IEntity format in formats)
                {
                    format.Delete();
                }

            }
        }
    }



}











