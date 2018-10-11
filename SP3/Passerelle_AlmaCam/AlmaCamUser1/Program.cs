//system
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
//psecific windows
using System.IO;
using Microsoft.Win32;

//dll almacam
using Wpm.Implement.Manager;
using Wpm.Implement.Processor;
using Wpm.Implement.ComponentEditor;  // ouverture de fenetres de selection almacam
using Wpm.Schema.Kernel;
using Actcut.ActcutModelManager;
using Actcut.NestingManager;
using Actcut.ResourceManager;
using Actcut.ResourceModel;
//dll personnalisées
using AF_Clipper_Dll;
using AF_ImportTools;

using System.Windows.Forms;


namespace AlmaCamUser1
{
    class Program
    {

        //initialisation des listes
        /// <summary>
        /// aucun log dans ce programme, seulement des messages informels
        /// Si pas de fichier detecté   , alors on annule l'import
        /// Si pas de type detecté      , alors on annule l'import
        /// </summary>
        /// <param name="args">arg  0 : type d'import, arg 1 chemin vers le fichier d'import
        /// il n'y a pas d'obligation a envoyer le chemin car ce dernier peut etre fourni par l'application almacam
        /// </param>
        static void Main(string[] args)
        {
               
            IContext _clipper_Context = null;
            
            string TypeImport = "";
            //string fulpathname = "";            
            string DbName = Alma_RegitryInfos.GetLastDataBase();
            Alma_Log.Create_Log();
            
            


            using (EventLog eventLog = new EventLog("Application"))
            {


                            

                ModelsRepository clipper_modelsRepository = new ModelsRepository();
                string csvImportPath = null;
                _clipper_Context = clipper_modelsRepository.GetModelContext(DbName);  //nom de la base;
                int i = _clipper_Context.ModelsRepository.ModelList.Count();
                Clipper_Param.GetlistParam(_clipper_Context);
                if (args.Length==0)  { 

                    /* dans ce cas on recupere les arguments de la base directement*/
                    /* on force l'import of*/
                    TypeImport = "OF";
                    /**/

                }
                else {//sinon on recupere le paramètre du type d'import
                    TypeImport = args[0].ToUpper().ToString();}
               

                 {
                    switch (TypeImport)
                    {
                        //fullpath name
                        case "STOCK":
                            //import stock
                            csvImportPath = null;
                            //
                            if (args.Length == 0 || args.Length == 1)
                            {
                                csvImportPath = Clipper_Param.GetPath("IMPORT_DM");
                            }
                            else
                            {
                                csvImportPath = args[1];
                                
                            }

                            Alma_Log.Write_Log(" chemin du fichier d'import" + csvImportPath);
                            
                            string dataModelstring = Clipper_Param.GetModelDM();
                            using (Clipper_Stock Stock = new Clipper_Stock())
                            {
                                //Stock.Import(_clipper_Context, csvImportPath, dataModelstring);
                                Stock.Import(_clipper_Context);//, csvImportPath);
                            }
                            clipper_modelsRepository = null;

                            break;

                        case "STOCK_PURGE":
                            //puge de tous les elements du stock


                            IEntityList stocks = _clipper_Context.EntityManager.GetEntityList("_STOCK");
                            stocks.Fill(false);

                            foreach (IEntity stock in stocks)
                            {
                                stock.Delete();
                            }


                            IEntityList formats = _clipper_Context.EntityManager.GetEntityList("_SHEET");
                            formats.Fill(false);

                            foreach (IEntity format in formats)
                            {
                                format.Delete();
                            }
                            clipper_modelsRepository = null;
                            break;

                        case "OF":
                         
                            clipper_modelsRepository = new ModelsRepository();
                            //import of                          
                            

                            if (args.Length==0 || args.Length == 1)
                            {
                                csvImportPath = Clipper_Param.GetPath("IMPORT_CDA");
                            }
                            else
                            {
                                csvImportPath = args[1].ToUpper().ToString();

                            }
                            
                            string of_dataModelstring = Clipper_Param.GetModelCA();

                            //chargement des paramètres
                            bool SansDt = false;
                           
                            //MessageBox.Show(csvImportPath);

                            if (csvImportPath.Contains("SANSDT") | csvImportPath.Contains("SANS_DT"))
                            {
                                SansDt = true;
                            }


                            using (Clipper_OF CahierAffaire = new Clipper_OF())
                            {
                                
                                CahierAffaire.Import(_clipper_Context, csvImportPath, of_dataModelstring, SansDt);
                                //CahierAffaire.Import(_clipper_Context, csvImportSandDt, of_dataModelstring, true);
                                       
                            }


                         
                            break;

                    }



                }
            }
            


            }
        }
    }

