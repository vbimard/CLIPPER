
//using Clipper_Dll;
//
using AF_ImportTools;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Drawing;

//almacam
using Wpm.Implement.ComponentEditor;  // ouverture de fenetres de selection almacam
using Wpm.Implement.Manager;
using Wpm.Implement.Processor;
using Wpm.Schema.Kernel;
//actcut
using Actcut.ActcutModelManager;
using Actcut.NestingManager;
using Actcut.ResourceManager;
using Actcut.ResourceModel;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Diagnostics;
using Alma.BaseUI.Utils;
using Actcut.CommonModel;
//




namespace AF_Clipper_Dll
//namespace Import_GP
#region commande_processor

{
    /// <summary>
    /// automatisme BO : outils necessaire pour l'envoie d'infos au service windoxs
    /// </summary>
    public class Automation_Tools : IDisposable

    {

        public void Dispose()
        {
            //Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// OBLIGATION D EXECUTER EN ADMIN
        /// recupere la liste des placements exportes (confition exported)
        /// recupere la liste des fichier dans les dossiers gpao
        /// cloture si le fichier n'est plus present
        /// ce code utilise les log windwos: pour supprimer les log windows
        /// Get-EventLog -List
        /// C:\> Remove-EventLog -LogName "MyLog"
        /// Remove-EventLog -Source "MyApp"
        /// </summary>
        /// <returns>true/false</returns>
        public bool Automatic_Nestings_Close( IContext Contextlocal, string Stage, AF_ImportTools.WindowsLog log)
        {
            string message = ""; //message de suivi pour les log
            try
            {

                Clipper_Param.GetlistParam(Contextlocal);
                bool rst = false;
                //string stage = "_TO_CUT_NESTING";
                IEntityList nestings_list = null;
                IEntity current_nesting = null;
                System.Console.WriteLine("connecté à" + Contextlocal.Connection.DatabaseName + " Pour cloture");
                System.Console.WriteLine("fermeture des placements " + Stage);

                nestings_list = Contextlocal.EntityManager.GetEntityList(Stage, AF_ImportTools.SimplifiedMethods.Get_Marqued_FieldName(Stage), ConditionOperator.Equal, true);
                nestings_list.Fill(false);


                if (nestings_list != null && nestings_list.Count() > 0)
                {
                    foreach (IEntity nesting in nestings_list)
                    {
                        string nesting_name;
                        string technology;
                        string path_to_file;
                        //string msgstart;
                        //On initialise le message
                        message = "";
                        //IEntityList nestings_to_close;
                        IEntity nesting_to_close;


                        nesting_to_close = nesting;

                        //get the nesting name 
                        nesting_name = current_nesting.GetFieldValueAsString("_NAME");
                        //get the technology--> get the folder
                        System.Console.WriteLine("----------------------------------------");
                        System.Console.WriteLine("Placement: " + nesting_name);
                        message += "Clôture de: " + nesting_name;
                        technology = AF_ImportTools.Machine_Info.GetNestingTechnologyName(ref Contextlocal, ref current_nesting);
                        System.Console.WriteLine(nesting_name + ": Technologie: " + technology);
                        message += "techno detected:" + technology + "\n";
                        //technology = "";
                        //get the filename
                        @path_to_file = Clipper_Param.GetPath("Export_GPAO") + "\\" + technology + "\\" + nesting_name + ".txt";
                        message += "gpao file: " + @path_to_file + "\n";

                        if (!File.Exists(@path_to_file))
                        {//on cloture
                         //on reconstruit une liste des placaments                     
                         //nesting_to_close = current_nesting; //nestings_list.Where(x => x.GetFieldValueAsString("_NAME") == nesting_name).FirstOrDefault();
                            message += "fermeture du placement " + nesting_to_close.GetFieldValueAsString("_NAME") + "\n";
                            SimplifiedMethods.CloseNesting(Contextlocal, nesting_to_close);
                            log.WriteLogSuccess("Synthèse :\n " + message + "\n" + message);
                            System.Console.WriteLine("placement  " + nesting_to_close.GetFieldValueAsString("_NAME") + " fermé");

                        }
                        else
                        {

                            message += "le fichier de placement a été detecté -le placement ne sera pas cloturé " + "\n";
                        }
                        //suppresse the file
                        log.WriteLogEvent("Synthèse :\n " + message + "\n");
                    }
                }

                System.Console.Out.Close();
                return rst;

            }
            catch (Exception ie)
            {

                log.WriteLogWarningEvent("Probleme rencontré log de la cloture des placements :\n " + message);
                log.WriteLogWarningEvent("details :\n " + ie.Message);
                //System.Console.WriteLine("Erreur à la fermeture du placement " +ie.Message);
                //System.Console.ReadLine() ;
                return false;
            }
        }



        /// <summary>
        /// recupere la liste des placements exportes (confition exported)
        /// recupere la liste des fichier dans les dossiers gpao
        /// cloture si le fichier n'est plus present
        ///utilise les log standard alma 
        /// C:\> Remove-EventLog -LogName "MyLog"
        /// Remove-EventLog -Source "MyApp"
        /// </summary>
        /// <returns></returns>
        public bool Automatic_Nestings_Close(IContext Contextlocal, string Stage)
        {
            string message = ""; //message de suivi pour les log
            try
            {

                Clipper_Param.GetlistParam(Contextlocal);


                Alma_Log.Write_Log("Parametre recuperés");

                bool rst = false;
                //string stage = "_TO_CUT_NESTING";
                IEntityList nestings_list = null;
                IEntity current_nesting = null;

                Alma_Log.Write_Log("connecté à" + Contextlocal.Connection.DatabaseName + " Pour cloture");
                System.Console.WriteLine("connecté à" + Contextlocal.Connection.DatabaseName + " Pour cloture");
                Alma_Log.Write_Log("fermeture des placement " + Stage);
                System.Console.WriteLine("fermeture des placement " + Stage);

                nestings_list = Contextlocal.EntityManager.GetEntityList(Stage, Stage + "_GPAO_Exported", ConditionOperator.Equal, true);
                nestings_list.Fill(false);

                if (nestings_list != null && nestings_list.Count() > 0)
                {
                    foreach (IEntity nesting in nestings_list)
                    {
                        string nesting_name;
                        string technology;
                        string path_to_file;
                        //string msgstart;
                        //On initialise le message
                        message = "";
                        //IEntityList nestings_to_close;
                        IEntity nesting_to_close;

                        //nesting_to_close= nestings_list.Where(x=>x.GetFieldValueAsString("_NAME")== nesting.GetFieldValueAsString("_NAME"));
                        current_nesting = nesting;

                        //get the nesting name 
                        nesting_name = current_nesting.GetFieldValueAsString("_NAME");
                        //get the technology--> get the folder
                        message += "closing :" + nesting_name;
                        technology = AF_ImportTools.Machine_Info.GetNestingTechnologyName(ref Contextlocal, ref current_nesting);
                        message += "techno detected:" + technology;
                        //technology = "";
                        //get the filename
                        @path_to_file = Clipper_Param.GetPath("Export_GPAO") + "\\" + technology + "\\" + technology + "\\" + nesting_name + ".txt";
                        message += "gapo file: " + @path_to_file;
                        if (!File.Exists(@path_to_file))
                        {//on cloture
                         //on reconstruit une liste des placaments                     
                            nesting_to_close = nestings_list.Where(x => x.GetFieldValueAsString("_NAME") == nesting_name).FirstOrDefault();
                            Alma_Log.Write_Log("fermeture du placement " + nesting_to_close.GetFieldValueAsString("_NAME"));
                            System.Console.WriteLine("fermeture du placement " + nesting_to_close.GetFieldValueAsString("_NAME"));

                        }
                        //suppresse the file

                    }
                }

                System.Console.Out.Close();
                return rst;

            }
            catch (Exception ie)
            {
                Alma_Log.Write_Log("Probleme rencontré log de la cloture des placements :\n " + message);
                Alma_Log.Write_Log("details :\n " + ie.Message);
            
                return false;
            }
        }




        




    }










    public class Clipper_Export_DT : CommandProcessor
    {
        public IContext contextlocal = null;
        public override bool Execute()
        {

            //initialisation des listes
            IContext _Context = null;
            //Param_Clipper.context = Context;
            //IEntityList ie = (IEntityList) Command.WorkOnEntityList;
            string DbName = Alma_RegitryInfos.GetLastDataBase();
            IModelsRepository modelsRepository = new ModelsRepository();
            _Context = modelsRepository.GetModelContext(DbName);
            //rw.Close();


            if (_Context != null)
            {
                AF_Clipper_Dll.Clipper_RemonteeDt Remontee_Dt = new AF_Clipper_Dll.Clipper_RemonteeDt();
                Remontee_Dt.Export_Piece_To_File(_Context, true);


            }
            return base.Execute();
        }
    }


    public class Clipper_Requirement_Processor : CommandProcessor
    {
        public IContext contextlocal = null;
       
       
        //appel de la lib d'export des besoins ici
        public override bool Execute()
        { //initialisation des listes
        IContext _Context = null;
            //string DbName = Alma_RegitryInfos.GetLastDataBase();
            //IModelsRepository modelsRepository = new ModelsRepository();
            contextlocal = Context; // modelsRepository.GetModelContext(DbName);

            using (Clipper_Sheet_Requirement sheet_requirement = new Clipper_Sheet_Requirement())
            {
            
                sheet_requirement.Export(_Context);//), csvImportPath);
            }




            return base.Execute();
        }
    }






    /// <summary>
    /// les commandes processor designent les boutons d'action integerés dans l'interface almaquote 
    /// </summary>
 
/*
    public class Clipper_From_Workshop_Processor : CommandProcessor
    {
        public IContext contextlocal = null;


        //appel de la lib d'export des besoins ici
        public override bool Execute()
        { //initialisation des listes

           
            
            IEntity selectedNesting = Command.WorkOnEntity;
            
           

            using (Clipper_DoOnAction_From_WorkShop Return_File = new Clipper_DoOnAction_From_WorkShop())
            {

                Return_File.Export(Command.Context, selectedNesting);//), csvImportPath);
            }




            return base.Execute();
        }
    }
    */



    /// <summary>
    /// cette classe lance l'application deporter d'import du stock
    /// pour le moment cette application est l'executable suivant
    /// C:\AlmaCAM\Bin\AlmaCamTrainingTest.exe
    /// </summary>

    public class ClipperIE : CommandProcessor
    {
        public IContext contextlocal = null;
        public override bool Execute()
        {
           
            ProcessStartInfo start_test = new ProcessStartInfo();
            start_test.Arguments = "";
          

            start_test.FileName = @"C:\AlmaCAM\Bin\AF_Clipper.exe";
      
            Process.Start(start_test);

            return base.Execute();
        }
    }

    public class ClipperIE_Global :SimpleCommandProcessor
    {
        //public IContext contextlocal = null;
        public override bool Execute()
        {

            ProcessStartInfo start_test = new ProcessStartInfo();
            start_test.Arguments = "";


            start_test.FileName = @"C:\AlmaCAM\Bin\AF_Clipper.exe";

            Process.Start(start_test);

            return base.Execute();
        }
    }




    /// <summary>
    /// les commandes processor designent les boutons d'action integerés dans l'interface almaquote 
    /// </summary>
    /// bouton de commande d'import du stock 
    /// de l'interface clipper
    ///
    ///
    public class Command_Clipper_ImportStock : CommandProcessor
    {
        public IContext contextlocal = null;
        public override bool Execute()
        {

            //recuperation du context
            //string DbName = Alma_RegitryInfos.GetLastDataBase();
            //IModelsRepository modelsRepository = new ModelsRepository();
            contextlocal = Context; // modelsRepository.GetModelContext(DbName);



            //import_stock
            //chargement de sparamètres
          

            using (Clipper_Stock Stock = new Clipper_Stock())
            {
                Stock.Import(contextlocal);//), csvImportPath);
            }


            return base.Execute();
        }
    }


    #endregion


    #region exit clipper dll
    public static class ClipperExit {

        public static void Close()
        {
      
            Alma_Log.Final_Open_Log();
            Application.Exit();

        
        }


    }

    #endregion

    //PARAMETRES
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////IMPORT///////////////OF////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    #region parameters
    /// <summary>
    /// la classe clipper_param recupere les paramètres de almacam dans les options de la passerelle (section piece a produire) 
    /// comme la lecture des paramètres est indispensable, cette classe verifie aussi la presence sdes dossier clipper , le nom de la base ainsi
    /// que la compatibilité  de almacam 
    /// </summary>

    public static class Clipper_Param
    {
        static Dictionary<string, object> Parameters_Dictionnary; // liste des path et des types pour le format du fichier de stock et des of

        // public static IContext context;
        static Clipper_Param()
        {
            Parameters_Dictionnary = new Dictionary<string, object>();


        }

        /// <summary>
        /// recuperation des path clipper+fichier.csv (echange csv) ou autre
        /// </summary>
        /// //exemple "H:\tutu\toto\cahieraffaire.csv"
        /// <param name="context"> contexte </param>
        public static Boolean GetlistParam(IContext context)
        {

            try
            {
                string parametre_name;

                Parameters_Dictionnary.Clear();
                /**/
                //ListPath.Add("EXPORT_GP", context.ParameterSetManager.GetParameterValue("EXPORT_GP", "DIR").GetValueAsString());
                //chemin import cahier affaire
                //chemin import
                //test migration sp3 changement de nom de cle pour clip configuraton
                IParameterValue sp3;
               
                string parametersetkey = "CLIPPER_DLL";
                parametre_name = "IMPORT_CDA";
                context.ParameterSetManager.TryGetParameterValue(parametersetkey, parametre_name, out sp3);
                if (sp3 == null) { parametersetkey = "CLIP_CONFIGURATION"; }
                parametre_name = "IMPORT_CDA";
                Alma_Log.Info("recuperation du parametre "+parametre_name, "GetlistParam");
                Parameters_Dictionnary.Add(parametre_name, context.ParameterSetManager.GetParameterValue(parametersetkey, "IMPORT_CDA").GetValueAsString());
                parametre_name = "IMPORT_DM";
                Alma_Log.Info("recuperation du parametre " + parametre_name, "GetlistParam");
                Parameters_Dictionnary.Add(parametre_name, context.ParameterSetManager.GetParameterValue(parametersetkey, "IMPORT_DM").GetValueAsString());
                parametre_name ="Export_GPAO";
                Alma_Log.Info("recuperation du parametre " + parametre_name, "GetlistParam");
                Parameters_Dictionnary.Add(parametre_name, context.ParameterSetManager.GetParameterValue(parametersetkey, "EXPORT_Rp").GetValueAsString());
                parametre_name = "EXPORT_DT";
                Alma_Log.Info("recuperation du parametre " + parametre_name, "GetlistParam");
                Parameters_Dictionnary.Add(parametre_name, context.ParameterSetManager.GetParameterValue(parametersetkey, "EXPORT_Dt").GetValueAsString());                /**/
                //description import
                parametre_name = "IMPORT_AUTO";
                Alma_Log.Info("recuperation du parametre " + parametre_name, "GetlistParam");
                Parameters_Dictionnary.Add(parametre_name, context.ParameterSetManager.GetParameterValue(parametersetkey, "IMPORT_AUTO").GetValueAsBoolean());                /**/
                parametre_name = "EMF_DIRECTORY";
                Alma_Log.Info("recuperation du parametre " + parametre_name, "GetlistParam");
                Parameters_Dictionnary.Add(parametre_name, context.ParameterSetManager.GetParameterValue(parametersetkey, "EMF_DIRECTORY").GetValueAsString());                /**/
                parametre_name = "MODEL_CA";
                Alma_Log.Info("recuperation du parametre " + parametre_name, "GetlistParam");
                Parameters_Dictionnary.Add(parametre_name, context.ParameterSetManager.GetParameterValue(parametersetkey, "MODEL_CA").GetValueAsString());
                parametre_name = "MODEL_DM";
                Alma_Log.Info("recuperation du parametre " + parametre_name, "GetlistParam");
                Parameters_Dictionnary.Add(parametre_name, context.ParameterSetManager.GetParameterValue(parametersetkey, "MODEL_DM").GetValueAsString());
                parametre_name = "MODEL_PATH";
                Alma_Log.Info("recuperation du parametre " + parametre_name, "GetlistParam");
                Parameters_Dictionnary.Add(parametre_name, context.ParameterSetManager.GetParameterValue(parametersetkey, "MODEL_PATH").GetValueAsString());
                parametre_name = "APPLICATION1";
                Alma_Log.Info("recuperation du parametre " + parametre_name, "GetlistParam");
                Parameters_Dictionnary.Add(parametre_name, context.ParameterSetManager.GetParameterValue(parametersetkey, "APPLICATION1").GetValueAsString());

                parametre_name = "SHEET_REQUIREMENT";
                Alma_Log.Info("recuperation du parametre " + parametre_name, "GetlistParam");
                Parameters_Dictionnary.Add(parametre_name, context.ParameterSetManager.GetParameterValue(parametersetkey, "SHEET_REQUIREMENT").GetValueAsString());


                //log
                parametre_name = "VERBOSE_LOG";
                Alma_Log.Info("recuperation du parametre " + parametre_name, "GetlistParam");
                Parameters_Dictionnary.Add(parametre_name, context.ParameterSetManager.GetParameterValue(parametersetkey, "VERBOSE_LOG").GetValueAsBoolean());
                //nom mahine clipper
                parametre_name = "CLIPPER_MACHINE_CF";
                Alma_Log.Info("recuperation du parametre " + parametre_name, "GetlistParam");
                Parameters_Dictionnary.Add(parametre_name, context.ParameterSetManager.GetParameterValue(parametersetkey, "CLIPPER_MACHINE_CF").GetValueAsString());
                /*parametres de sorties*/
                parametre_name = "STRING_FORMAT_DOUBLE";
                Alma_Log.Info("recuperation du parametre " + parametre_name, "GetlistParam");
                Parameters_Dictionnary.Add(parametre_name, "{0:0.00###}");
                parametre_name = "ALMACAM_EDITOR_NAME";
                Alma_Log.Info("recuperation du parametre " + parametre_name, "GetlistParam");
                Parameters_Dictionnary.Add(parametre_name, context.ParameterSetManager.GetParameterValue(parametersetkey, "ALMACAM_EDITOR_NAME").GetValueAsString());
                //parametre export : chemin de sortie des devis
                parametre_name = "_EXPORT_GP_DIRECTORY";
                Alma_Log.Info("recuperation du parametre " + parametre_name, "GetlistParam");
                Parameters_Dictionnary.Add(parametre_name, context.ParameterSetManager.GetParameterValue("_EXPORT", "_EXPORT_GP_DIRECTORY").GetValueAsString());
                //repertoire de exports dpr
                parametre_name = "_ACTCUT_DPR_DIRECTORY";
                Alma_Log.Info("recuperation du parametre " + parametre_name, "GetlistParam");
                Parameters_Dictionnary.Add(parametre_name, context.ParameterSetManager.GetParameterValue("_EXPORT", "_ACTCUT_DPR_DIRECTORY").GetValueAsString());
                //Champs Spécifique a reporter à partir des information des pieces à produire et de lors de l'import gpao dans les pieces2d (reference almacam)
                //entrez l'information du nom de champs 2d puis le nom du champs du line_dictionnary
                // NOM_CHAMPS|NOM_CHAMPS_DU_LINE_DICTIONNARY
                //CUSTOMER_REFERENCE_INFOS;
                parametre_name = "CUSTOMER_REFERENCE_INFOS";
                Alma_Log.Info("recuperation du parametre " + parametre_name, "GetlistParam");
                ///on entre les infos 
                ///Parameters_Dictionnary.Add(parametre_name, context.ParameterSetManager.GetParameterValue("_EXPORT", "CUSTOMER_REFERENCE_INFOS").GetValueAsString());
                Parameters_Dictionnary.Add(parametre_name, "CUSTOMER|_FIRM");
                ///auteur
                parametre_name = "_AUTHOR";
                Parameters_Dictionnary.Add(parametre_name, context.UserId);

                //option du workshop GlobalClosedSeparated or GlobalCloseOneClic
                WorkShopOptionType WORKSHOP_OPTION = ActcutModelOptions.GetWorkShopOption(context);
                parametre_name = "_WORKSHOP_OPTION ";
                Parameters_Dictionnary.Add(parametre_name, WORKSHOP_OPTION.ToString());


                //verification des chemins


                //verification des path
                Alma_Log.Info("verification de l'existance des path " , "GetlistParam");
                if (checkClipperFolderExists() == false) { throw new System.ApplicationException("Certains chemin d'echanges de la Passerelle AlmaCam-clipper ne sont pas accessibles"); };
                ///string AlmaCAmVersion = Directory.GetCurrentDirectory().ToString() + Parameters_Dictionnary.Values("");///
                if (ckeckCompatibilityVersion() == false) { throw new System.ApplicationException("Version de la Dll clipper n'est pas validée pour cette version d'AlmaCam"); }

                return true;
            }
            catch (KeyNotFoundException ex) {
                Alma_Log.Error(ex, "CETTE BASE NE SEMBLE PAS ETRE CONFIGUREE POUR CLIPPER !!! ");
                Alma_Log.Error(ex, "Veuiller verifier la configuration des paramètres de l'import clipper (nom et id des champs....)");
                MessageBox.Show(Alma_RegitryInfos.GetLastDataBase() + " :CETTE BASE NE SEMBLE PAS ETRE CONFIGUREE POUR CLIPPER !!! \r\n " +
                "Veuillez verifier la base selectionnées pour l'ouverture d'AlmaCam");
                //on sort
                //ClipperExit.Close();
                return false;
            }
            catch (System.ApplicationException exVersion) {
                MessageBox.Show(exVersion.Message);
                Alma_Log.Error(exVersion, "Version incompatible ou mauvaise configuration de la base almacam");
                //ClipperExit.Close();
                return false;
            }

            catch (System.IO.DirectoryNotFoundException exFolder)
            {
                MessageBox.Show(exFolder.Message);
                Alma_Log.Error(exFolder, "L'un des dossier de d'echange n'existe pas");
                return false;
                //ClipperExit.Close();
            }

            //finally { return false; }


            //MessageBox.Show(e.Message+"/r/n Version incompatible ou mauvaise configuration de la base almacam"); }
        }

        /// <summary>
        /// retourn la valeur de la clé recherché dans les paramètres
        /// </summary>
        /// <typeparam name="T">type générique </typeparam>
        /// <param name="context">contexte </param>
        /// <param name="PathVariable">clé a rechercher</param>
        /// <returns></returns>
        public static T GetParam<T>(this IDictionary<string, object> dic, string key)
        {
            if (Parameters_Dictionnary.ContainsKey(key))
            {
                return (T)dic[key];
            }
            else { return default(T); }
        }

        /// <summary>
        /// retourne un chemin windows type string
        /// </summary>
        /// <param name="key">nom de la clé dans le dictionnaire</param>
        /// <returns>chemin windows : type c:\actcut...</returns>
        public static string GetPath(string key)
        {

            //GetlistParam(context);//    Alma_Log.Info("verification de la clé " + key, "GetPath");
            try
            {
               // if (Parameters_Dictionnary.ContainsKey(key))
                {
                    return Parameters_Dictionnary[key].ToString();
                }
            }
             catch (Exception ie) {
                Alma_Log.Info("impossible de trouver la clé  " + key, "GetPath");
                Alma_Log.Info("impossible de trouver la clé  " + ie.Message, "GetPath");
                return "Undef"; }
           

        }
        /// <summary>
        /// recupere la case a cocher d'automation
        /// </summary>
        /// <returns>boolean true/false</returns>
        public static bool IsAutomatiqueImport()
        {
            string key = "IMPORT_AUTO";
            //GetlistParam(context);//
            if (Parameters_Dictionnary.ContainsKey(key)) { return (bool)Parameters_Dictionnary[key]; } else { return false; }

        }

        /// <summary>
        /// recupere
        /// </summary>
        /// <returns>un model de fichier csv sous la forme d'une liste de champs 
        /// champs : numero du champ dans #nom du champs  dans almacam#Type  -> plus tard si besoin numero du champ dans #nom du champs  dans almacam#Type  # taille max
        /// 0#NAME#string;1#AFFAIRE#string;2#THICKNESS#string;3#MATERIAL_CLIPPER#string;4#CENTREFRAIS#string;5#TECHNOLOGIE#string;6#FAMILY#string;7#IDLNROUT#string;8#CENTREFRAISSUIV#string;9#CUSTOMER#string;10#PART_INITIAL_QUANTITY#double;11#QUANTITY#double;12#ECOQTY#double;13#STARTDATE#date;14#ENDDATE#date;15#PLAN#string;....
        /// 
        /// </returns>
        public static string GetModelCA()
        {
            string key = "MODEL_CA";
            //GetlistParam(context);//
            if (Parameters_Dictionnary.ContainsKey(key)) { return (string)Parameters_Dictionnary[key]; } else { return "Undef  model CA"; }

        }

        
        public static string GetModelDM()
        {
            string key = "MODEL_DM";
            //GetlistParam(context);//
            if (Parameters_Dictionnary.ContainsKey(key)) { return (string)Parameters_Dictionnary[key]; } else { return "Undef  model  DM"; }

        }


        public static string GetModelPATH()
        {
            string key = "MODEL_PATH";
            //GetlistParam(context);//
            if (Parameters_Dictionnary.ContainsKey(key)) { return (string)Parameters_Dictionnary[key]; } else { return "Undef model PATH"; }

        }


        public static string get_string_format_double()
        {

            string key = "STRING_FORMAT_DOUBLE";
            //GetlistParam(context);//
            if (Parameters_Dictionnary.ContainsKey(key)) { return (string)Parameters_Dictionnary[key]; } else { return "Undef STRING_FORMAT_DOUBLE"; }

        }

        public static string get_application1()
        {
            string key = "APPLICATION1";
            //GetlistParam(context);//
            if (Parameters_Dictionnary.ContainsKey(key)) { return (string)@Parameters_Dictionnary[key]; } else { return "Undef model PATH"; }
        }

        public static bool getVerbose_Log()
        {
            string key = "VERBOSE_LOG"; //log verbeux
            if (Parameters_Dictionnary.ContainsKey(key)) { return (bool)Parameters_Dictionnary[key]; } else { return true; }
        }

        public static string get_Clipper_Machine_Cf()
        {
            string key = "CLIPPER_MACHINE_CF"; //log verbeux
            //GetlistParam(context);//
            if (Parameters_Dictionnary.ContainsKey(key)) { return (string)Parameters_Dictionnary[key]; } else { return "Undef clipper machine"; }

        }


        public static string get_AlmaCamEditorName()
        {
            string key = "ALMACAM_EDITOR_NAME"; //log verbeux
            //GetlistParam(context);//
            if (Parameters_Dictionnary.ContainsKey(key)) { return (string)Parameters_Dictionnary[key]; } else { return "Undef almacam editor"; }

        }
        /// <summary>
        /// verification de l'existance des dossiers d'echange
        /// </summary>
        /// <returns>true si tous les dossier existent, false si ils n'existent pas</returns>
        public static Boolean checkClipperFolderExists() {
            try
            {

                Alma_Log.Info("checking IMPORT_CDA", "checkClipperFolderExists");
                Directory.GetDirectories(Path.GetDirectoryName(Clipper_Param.GetPath("IMPORT_CDA")));
                Alma_Log.Info("checking IMPORT_DM", "checkClipperFolderExists");
                Directory.GetDirectories(Path.GetDirectoryName(Clipper_Param.GetPath("IMPORT_DM")));
                Alma_Log.Info("checking Export_GPAO", "checkClipperFolderExists");
                Directory.GetDirectories(Path.GetDirectoryName(Clipper_Param.GetPath("Export_GPAO")));
                Alma_Log.Info("checking EXPORT_DT", "checkClipperFolderExists");
                Directory.GetDirectories(Path.GetDirectoryName(Clipper_Param.GetPath("EXPORT_DT")));
                
              
                return true;
            }
            catch (System.IO.IOException ie)   //.DirectoryNotFoundException ie)
            {
                Alma_Log.Error(ie, " les dossiers d'echange sont mal definis verifier : IMPORT_CDA, IMPORT_DM ,Export_GPAO, EXPORT_Dt ,_EXPORT_GP_DIRECTORY");
                return false;
                throw;
            }


        }
        /// <summary>
        /// verification sur le nom de la base de données
        /// car clipper a écrit en dur le nom de la base de données
        /// </summary>
        /// <returns>true si le nom de la base est correcte</returns>
        public static Boolean checkDatabasename(string actualdatabasename)
        {
            try
            { bool res = true;

                string lastopenDbname = Alma_RegitryInfos.GetLastDataBase();
                if (Alma_RegitryInfos.GetLastDataBase().Count() == 0)
                {

                    new Exception("Impossible de lire le nom de la dernière base ouvert dans le registre");
                    { throw new Exception(Properties.Resources.Clipper_Almacam_Database_Name.ToString() + ", la base :" + Properties.Resources.Clipper_Almacam_Database_Name + " est introuvable"); }

                }

                else
                {

                    string working_db = Properties.Resources.Clipper_Almacam_Database_Name.ToString();
                    Alma_Log.Write_Log("derniere base ouverte: " + working_db);
                    if (Clipper_Param.checkDatabasename(working_db) == false) { throw new Exception(Alma_RegitryInfos.GetLastDataBase() + ", le Nom de la base est incorrecte,  elle doit se nommer :" + Properties.Resources.Clipper_Almacam_Database_Name); }


                }
                return res;
            }
            //catch (Exception e)
            catch (Exception ex)
            {
                Alma_Log.Write_Log(ex.Message);
                MessageBox.Show(ex.Message);
                return false;
                throw;
            }




        }
        /// <summary>
        /// retourne la verison de la dll clipperDll.dll
        /// </summary>
        /// <returns></returns>
        public static string getClipperDllVersion()
        {
            return Application.ProductVersion.ToString().Substring(0, 3);
        }

        /// <summary>
        /// retourn la version compatible almacam indiquee dans les ressources almacam
        /// </summary>
        /// <returns></returns>
        public static string getAlmaCAMCompatibleVerion() {
            return Properties.Resources.Almcam_Version.ToString();

        }
        /// <summary>
        /// recupere le numero de version compatible. et le compare a la version de l'executable almacam
        /// la version est bloquée par une infos des ressources de la dll clipper_dll
        /// </summary>
        /// <returns>true si le test est accepté</returns>
        public static Boolean ckeckCompatibilityVersion()
        {
            bool res = false;

            try
            {

                bool compatible = false;
                string versionalmacam;
                string almacameditorfullpath = Directory.GetCurrentDirectory().ToString() + "\\" + get_AlmaCamEditorName();
                string almacamCompatibleversion = Properties.Resources.Almcam_Version.ToString();
                //get version//
                versionalmacam = FileVersionInfo.GetVersionInfo(almacameditorfullpath).ProductVersion.ToString();

               
                foreach (string v in almacamCompatibleversion.Split(';')) {
                    if (versionalmacam.StartsWith(v)) {
                        compatible = true;
                        break; };
                }


                ///

                if (compatible == true)
                {
                    Alma_Log.Write_Log("verion wpm.exe :" + versionalmacam + " version autoriséee pour cette dll " + almacamCompatibleversion);
                    Alma_Log.Write_Log("test ok");
                    res = true;
                }
                else
                {
                    Alma_Log.Write_Log("la librairie clipperdll.dll version " + almacamCompatibleversion + " est  incompatible Almacam  " + versionalmacam);
                }

                return res;

            }
            catch
            {

                Alma_Log.Write_Log(": time tag:  ");
                return res;

            }

        }


    }

    #endregion


    //IMPORT OF
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////IMPORT///////////////OF////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///description des boutons
    ///
    /// <summary>
    /// cette classe lance l'application deporter d'import du stock
    /// pour le moment cette application est l'executable suivant
    /// C:\AlmaCAM\Bin\AlmaCamTrainingTest.exe
    /// </summary>


    /*
     * 
     *  //import of
                //chargement de sparamètres
                // bool SansDt=false;

                Clipper_Param.GetlistParam(_Context);
                string csvImportPath = Clipper_Param.GetPath("IMPORT_CDA");
                //recuperation du nom de fichier
                string csvFileName = Path.GetFileNameWithoutExtension(csvImportPath);
                string csvDirectory = Path.GetDirectoryName(csvImportPath);
                string csvImportSandDt = csvDirectory + "\\" + csvFileName + "_SANSDT.csv";
                /*
                if (csvImportSandDt.Contains("SANSDT")| csvImportSandDt.Contains("SANS_DT"))
                {
                    SansDt = true;
                }
               
                string dataModelstring = Clipper_Param.GetModelCA();


            using (Clipper_OF CahierAffaire = new Clipper_OF())
            {
                CahierAffaire.Import(_Context, csvImportPath, dataModelstring, false);
                CahierAffaire.Import(_Context, csvImportSandDt, dataModelstring, true);
                //}

            } 
            
         */











    /// <summary>
    /// import le cahier d'affaire sans données tech
    /// </summary>
    public class Import_OF_SansDt_Processor : CommandProcessor
    {
        public IContext contextlocal = null;
        public override bool Execute()
        {
            //Param_Clipper.context = Context;
            //recuperation du context
            //string DbName = Alma_RegitryInfos.GetLastDataBase();
            //IModelsRepository modelsRepository = new ModelsRepository();
            //contextlocal = modelsRepository.GetModelContext(DbName);
            contextlocal = Context;
            Clipper_Param.GetlistParam(contextlocal);
            string csvImportPath = Clipper_Param.GetPath("IMPORT_CDA");
            //recuperation du nom de fichier
            string csvFileName = Path.GetFileNameWithoutExtension(csvImportPath);
            string csvDirectory = Path.GetDirectoryName(csvImportPath);
            string csvImportSandDt = csvDirectory + "\\" + csvFileName + "_SANSDT.csv";
            string dataModelstring = Clipper_Param.GetModelCA();


            if (contextlocal != null)
            {
                using (Clipper_OF cahierAffairesansdt= new Clipper_OF())
                {
                    cahierAffairesansdt.Import(contextlocal, csvImportSandDt, dataModelstring, true);//), csvImportPath);
                }
            }
            return base.Execute();
        }
    }

    public class Import_OF_Processor : CommandProcessor
    {
        //public IContext contextlocal = null;
        public override bool Execute()
        {
            //recuperation du context
            //string DbName = Alma_RegitryInfos.GetLastDataBase();
           // IModelsRepository modelsRepository = new ModelsRepository();
            //contextlocal = modelsRepository.GetModelContext(DbName);
            //contextlocal = Context;
            Clipper_Param.GetlistParam(Context);
            string csvImportPath = Clipper_Param.GetPath("IMPORT_CDA");
            //recuperation du nom de fichier
            string csvFileName = Path.GetFileNameWithoutExtension(csvImportPath);
            string dataModelstring = Clipper_Param.GetModelCA();


           // if (contextlocal != null)
            if (Context != null)
                {
                using (Clipper_OF cahierAffaire = new Clipper_OF())
                {
                    cahierAffaire.Import(Context, csvImportPath, dataModelstring, false);//), csvImportPath);
                }
            }
            return base.Execute();
        }
    }






    #region Import_OF
    /// <summary>
    /// recupere les of exportes de clipper
    /// </summary>
    public class Clipper_OF : IDisposable, IImport
{
    string CsvImportPath = null;


    public void Dispose()
    {
        //Dispose(true);
        GC.SuppressFinalize(this);
    }



    /// <summary>
    /// creer une nouvelle reference a produire sous reserve de sans_donnees_technique=true et de centre de frais et is non presente
    /// </summary>
    /// <param name="contextlocal"></param>
    /// <param name="line_dictionnary"></param>
    /// <param name="CentreFrais_Dictionnary">dictionnaire des centres de frais</param>
    /// <param name="reference_to_produce">retourn de la nouvelle reference a produire si besoin</param>
    /// <param name="reference">reference a pointer</param>
    /// <param name="timetag">time tag de l'import : pour groupement </param>
    /// <param name="sans_donnees_technique">si true alors oon ne creer jamais de reference</param>
    /// <returns></returns>
    public bool CreateNewPartToProduce(IContext contextlocal, Dictionary<string, object> line_dictionnary, Dictionary<string, string> CentreFrais_Dictionnary, ref IEntity reference_to_produce, ref IEntity reference, string timetag, bool sans_donnees_technique) {
        bool result = false;

        try {
            //la piece ne contient pas de gamme
            //cas des pieces oranges : Pas de cf, pas de id_piece_cfao, on considere que c'est une piece orange--> on ne creer que la reference. 
            if (Data_Model.ExistsInDictionnary("CENTREFRAIS", ref line_dictionnary) == false || sans_donnees_technique == true)
            {
                return false;
            }
            //string referenceName = null;
            Boolean need_prep = true;
            //int index_extension = 0;  //> 0 si ;emf;dpr detectée
            PartInfo machinable_part_infos = null; //infos de machinabe part

            //creation de la nouvelle reference
            reference_to_produce = contextlocal.EntityManager.CreateEntity("_TO_PRODUCE_REFERENCE");
            //recuperation et assignaton de la machine si elle existe
            string machine_name = "";
            Data_Model.ExistsInDictionnary(line_dictionnary["CENTREFRAIS"].ToString(), ref CentreFrais_Dictionnary);
            //lecture des part infos (optionnel) car le get reference fait le travail                 
            machinable_part_infos = new PartInfo();
            bool fastmode = true;
            //bool result = false;
            machinable_part_infos.GetPartinfos(ref contextlocal, reference);
            //on control que la matiere de la reference correspond -soit bonne sinon on continue a la ligne suivante//
            if (fastmode)
            {

                //try { reference_to_produce.SetFieldValue("NEED_PREP", !part_infos.IsPartDefault_Preparation(contextlocal, reference, CentreFrais_Dictionnary[line_Dictionnary["CENTREFRAIS"].ToString()])); }
                if (Data_Model.ExistsInDictionnary(line_dictionnary["CENTREFRAIS"].ToString(), ref CentreFrais_Dictionnary))
                {
                    try { need_prep = need_prep && machinable_part_infos.IsPartDefault_Preparation(reference, CentreFrais_Dictionnary[line_dictionnary["CENTREFRAIS"].ToString()]); }
                    //reference_to_produce.SetFieldValue("NEED_PREP", !machinable_part_infos.IsPartDefault_Preparation(reference, CentreFrais_Dictionnary[line_Dictionnary["CENTREFRAIS"].ToString()])); }
                    catch (KeyNotFoundException)
                    {
                        Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + "le centre de Frais  ne pointe pas vers une machine connue");
                        MessageBox.Show(string.Format("le centre de Frais {0} ne pointe pas vers une machine connue", CentreFrais_Dictionnary[line_dictionnary["CENTREFRAIS"].ToString()]));
                    }
                }

            }
            else
            //slowmode
            //methode par comparaison d'id
            {
                if (machine_name != "")
                {
                    IEntityList machines = null;
                    IEntity machine = null;

                    machine = AF_ImportTools.SimplifiedMethods.GetFirtOfList(machines);
                    string mm = machinable_part_infos.DefaultMachineName;
                    need_prep = need_prep && true;
                }
                else
                {
                    need_prep = need_prep && true;

                }

            }




            //ecriture du time tag
            reference_to_produce.SetFieldValue("OF_IMPORT_NUMBER", timetag.Replace("_", ""));
            reference_to_produce.SetFieldValue("_REFERENCE", reference.Id32);
            reference_to_produce.SetFieldValue("NEED_PREP", need_prep);


            Update_Part_Item(contextlocal, ref reference_to_produce, ref CentreFrais_Dictionnary, ref line_dictionnary);
            Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + "update infos succeed");
            reference_to_produce.Save();
            line_dictionnary.Clear();

            return result;
        }
        catch { return result; }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="contextlocal"></param>
    /// <param name="line_dictionnary"></param>
    /// <param name="CentreFrais_Dictionnary"></param>
    /// <param name="reference_to_produce"></param>
    /// <param name="reference"></param>
    /// <param name="timetag"></param>
    /// <returns></returns>
    public bool CreateNewPartToProduce(IContext contextlocal, Dictionary<string, object> line_dictionnary, Dictionary<string, string> CentreFrais_Dictionnary, ref IEntity reference_to_produce, ref IEntity reference, string timetag)
    {
        bool result = false;

        try
        {
            //la piece ne contient pas de gamme
            //cas des pieces oranges : Pas de cf, pas de id_piece_cfao, on considere que c'est une piece orange--> on ne creer que la reference. 
            if ((Data_Model.ExistsInDictionnary("CENTREFRAIS") == false) && (Data_Model.ExistsInDictionnary("CENTREFRAIS") == false))
            {
                return false;
            }
            //string referenceName = null;
            Boolean need_prep = false;
            //int index_extension = 0;  //> 0 si ;emf;dpr detectée
            PartInfo machinable_part_infos = null; //infos de machinabe part

            //creation de la nouvelle reference
            reference_to_produce = contextlocal.EntityManager.CreateEntity("_TO_PRODUCE_REFERENCE");
            //recuperation et assignaton de la machine si elle existe
            string machine_name = "";
            //Data_Model.ExistsInDictionnary(line_dictionnary["CENTREFRAIS"].ToString(), ref CentreFrais_Dictionnary);
            //lecture des part infos (optionnel) car le get reference fait le travail                 
            machinable_part_infos = new PartInfo();
            bool fastmode = true;
            //bool result = false;
            machinable_part_infos.GetPartinfos(ref contextlocal, reference);
                //
            //on control que la matiere de la reference correspond -soit bonne sinon on continue a la ligne suivante//
            if (fastmode)
            {

                //try { reference_to_produce.SetFieldValue("NEED_PREP", !part_infos.IsPartDefault_Preparation(contextlocal, reference, CentreFrais_Dictionnary[line_Dictionnary["CENTREFRAIS"].ToString()])); }
                if (Data_Model.ExistsInDictionnary(line_dictionnary["CENTREFRAIS"].ToString(), ref CentreFrais_Dictionnary))
                {
                    try { need_prep = need_prep && machinable_part_infos.IsPartDefault_Preparation(reference, CentreFrais_Dictionnary[line_dictionnary["CENTREFRAIS"].ToString()]); }
                    //reference_to_produce.SetFieldValue("NEED_PREP", !machinable_part_infos.IsPartDefault_Preparation(reference, CentreFrais_Dictionnary[line_Dictionnary["CENTREFRAIS"].ToString()])); }
                    catch (KeyNotFoundException)
                    {
                        Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + "le centre de Frais  ne pointe pas vers une machine connue");
                        MessageBox.Show(string.Format("le centre de Frais {0} ne pointe pas vers une machine connue", CentreFrais_Dictionnary[line_dictionnary["CENTREFRAIS"].ToString()]));
                    }
                }

            }
            else
            //slowmode
            //methode par comparaison d'id
            {
                if (machine_name != "")
                {
                    IEntityList machines = null;
                    IEntity machine = null;

                    machine = AF_ImportTools.SimplifiedMethods.GetFirtOfList(machines);
                    string mm = machinable_part_infos.DefaultMachineName;
                    need_prep = need_prep && true;
                }
                else
                {
                    need_prep = need_prep && true;

                }

            }




            //ecriture du time tag
            reference_to_produce.SetFieldValue("OF_IMPORT_NUMBER", timetag.Replace("_", ""));
            reference_to_produce.SetFieldValue("_REFERENCE", reference.Id32);
            reference_to_produce.SetFieldValue("NEED_PREP", need_prep);


            Update_Part_Item(contextlocal, ref reference_to_produce, ref CentreFrais_Dictionnary, ref line_dictionnary);
            Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + "update infos succeed");
            reference_to_produce.Save();
            line_dictionnary.Clear();

            return result;
        }
        catch { return result; }
    }



    /// <summary>
    /// Recupere la liste de toutes les machines sous la forme litterale "nom machine" "centre de frais"
    /// </summary>
    /// <param name="contextlocal">context local</param>
    /// <param name="Clipper_Machine">entité machine clipper</param>
    /// <param name="Clipper_Centre_Frais">entité centre de frais clipper</param>
    /// <returns></returns>
    public Boolean Get_Clipper_Machine(IContext contextlocal, out IEntity Clipper_Machine, out IEntity Clipper_Centre_Frais, out Dictionary<string, string> CentreFrais_Dictionnary) {



        CentreFrais_Dictionnary = new Dictionary<string, string>();
        IEntityList machine_liste = null;
        //recuperation de la machine clipper et initialisation des listes
        //CentreFrais_Dictionnary = null;
        Clipper_Machine = null;
        Clipper_Centre_Frais = null;
        //CentreFrais_Dictionnary.Clear();
        //verification que toutes les machineS sont conformes pour une intégration clipper
        ///remplissage des machines et verification de la presence du centre de frais demandé par clipper
        machine_liste = contextlocal.EntityManager.GetEntityList("_CUT_MACHINE_TYPE");
        machine_liste.Fill(false);


        foreach (IEntity machine in machine_liste)

        {
            IEntity cf;
            cf = machine.GetFieldValueAsEntity("CENTREFRAIS_MACHINE");

            if (!object.Equals(machine.GetFieldValueAsEntity("CENTREFRAIS_MACHINE"), null))
            {
                cf = machine.GetFieldValueAsEntity("CENTREFRAIS_MACHINE");
            } else {
                cf = null;
                Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + ": Missing  cost center on : " + machine.DefaultValue);
                Alma_Log.Error("centre de frais non defini sur la machine  !!!" + machine.DefaultValue, MethodBase.GetCurrentMethod().Name);

            }

            ///creation du dictionnaire des machines installées   
            if (cf.DefaultValue != "" && machine.DefaultValue != "" && Clipper_Param.get_Clipper_Machine_Cf() != null
                )
            {
                if (CentreFrais_Dictionnary.ContainsKey(cf.DefaultValue) == false) { CentreFrais_Dictionnary.Add(cf.DefaultValue, machine.DefaultValue); }

                if (cf.DefaultValue == Clipper_Param.get_Clipper_Machine_Cf())
                {
                    if (Clipper_Param.get_Clipper_Machine_Cf() != "Undef clipper machine")
                    {
                        Clipper_Centre_Frais = cf;
                        Clipper_Machine = machine;
                    }
                    else
                    {
                        Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + ": Missing  clipper machine !!! ");
                        Alma_Log.Error("IL MANQUE LA MACHINE CLIPPER !!!", MethodBase.GetCurrentMethod().Name);
                        return false;//throw new Exception(machine.DefaultValue + " : Missing  cost center definition"); 
                    }

                }

            }

            else { /*on log on arrete tout */
            Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + ": Missing  cost center definition on a machine !!! ");
                    Alma_Log.Error("IL MANQUE LE CENTRE DE FRAIS SUR L UNE DES MACHINES INSTALLEE !!!", MethodBase.GetCurrentMethod().Name);
                    return false;//throw new Exception(machine.DefaultValue + " : Missing  cost center definition"); 
                }
            }
            return true;


        }

        /*
        /// <summary>
        /// pas enciore utilisée mais servira a creer des preparations à la volée
        /// </summary>
        /// <param name="contextlocal"></param>
        /// <param name="reference"></param>
        /// <param name="machine"></param>
        /// <param name="Sans_Donnees_Technique"></param>
        /// <returns></returns>
        public IEntity Create_New_Machinable_Part(IContext contextlocal, IEntity reference, IEntity machine, bool Sans_Donnees_Technique)
        {
            IEntity machinable_part = null;
            try {
                //pas utile de creer une preparation en mode clipper
                machinable_part = contextlocal.EntityManager.CreateEntity("_MACHINABLE_PART");
           
                return machinable_part;
            }
            catch (Exception ie) {
                Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + ": impossible de creer la preparation !!! " + ie.Message);
                return machinable_part;
            }
        }
        */

        /// <summary>
        /// creation d'une references vide pour creation
        /// </summary>
        /// <param name="contextlocal">context</param>
        /// <param name="line_dictionnary">dictionnaire de ligne</param>
        public IEntity CreateNewReference(IContext contextlocal, Dictionary<string, object> line_dictionnary, ref string NewReferenceName, IEntity clipper_machine, bool Sans_Donnees_Technique)
        {
            try {

                

                IEntity newreference = null;
                IEntity material = null;
                IEntity machine = null;

                Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + ": creation d'une nouvelle piece !! ");
                // string referenceName = null;
                int index_extension = 0;
                //si la machine clipper n'est pas nulle
                //on initialise la machine a la machine clipper
                if (clipper_machine.Id32 != 0) { machine = clipper_machine; }

                if (line_dictionnary.ContainsKey("_MATERIAL") && line_dictionnary.ContainsKey("THICKNESS") && line_dictionnary.ContainsKey("_NAME"))
                {
                    //recuperation de la matiere 
                    material = GetMaterialEntity(contextlocal, ref line_dictionnary);
                    //recupe du nom de la geométrie                 
                    //string referenceName = "undef";    just in case mais inutiel
                    if (NewReferenceName == null) {

                        Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + ": Unfortunate error: NewreferenceName does not existes and new reference has been created !! ");
                        if (Data_Model.ExistsInDictionnary("FILENAME", ref line_dictionnary))
                        {

                            NewReferenceName = line_dictionnary["FILENAME"].ToString();
                            if (NewReferenceName.ToUpper().IndexOf(".DPR.EMF") > 0) { index_extension = 7; }
                            if (NewReferenceName.ToUpper().IndexOf(".DPR") > 0) { index_extension = 4; }
                        }
                        else
                        {
                            NewReferenceName = line_dictionnary["_NAME"].ToString();
                        }

                        NewReferenceName = Path.GetFileNameWithoutExtension(@NewReferenceName.Substring(0, (NewReferenceName.Length) - index_extension));

                    }


                    ///////////////////////////recuperation de la machine envoyé par le cahier d'affaire
                    //verification de la machine sur la ligne courante
                    //affectation de la machiune clipper par defaut
                    if (line_dictionnary.ContainsKey("CENTREFRAIS") == true)
                    {
                        ///remplissage des machines et verification de la presence du centre de frais demandé par clipper
                        IEntityList machine_liste = contextlocal.EntityManager.GetEntityList("_CUT_MACHINE_TYPE");
                        machine_liste.Fill(false);

                        foreach (IEntity m in machine_liste)
                        {
                            IEntity cf = m.GetFieldValueAsEntity("CENTREFRAIS_MACHINE");
                            string cfbase = cf.GetFieldValueAsString("_CODE").ToUpper();
                            string cffile = "";
                            /*** SI CHAMP VIDE***/
                            if (line_dictionnary.ContainsKey("CENTREFRAIS") != true) {
                                //recup de la machine clipper par defaut
                                machine = clipper_machine;
                            }
                            else
                            { cffile = line_dictionnary["CENTREFRAIS"].ToString().ToUpper();
                                if (string.Compare(cfbase, cffile) == 0) { machine = m; break; }
                                else { machine = clipper_machine; }
                                /*on log on arrete tout */ //Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + ": centre de frais inconnu !! "); throw new Exception(machine.DefaultValue + " : Missing  cost center definition");


                            }


                        }
                    }
                    ///si vide alors on recupere ma machine clipper
                    else { if (clipper_machine.Id32 != 0) { machine = clipper_machine; } }


                    //

                    //creation des infos complementaires de reference notamment les données sans dt
                    //creation de l'entité
                    newreference = contextlocal.EntityManager.CreateEntity("_REFERENCE");
                    //remplacement par la machine clipper dont le cf est clip7
                    //avant tou on let la machine clipper par defaut
                    //champs standards

                    newreference.SetFieldValue("_DEFAULT_CUT_MACHINE_TYPE", machine.Id32);
                    newreference.SetFieldValue("_NAME", NewReferenceName);
                    newreference.SetFieldValue("_MATERIAL", material.Id32);

                    if (contextlocal.UserId != -1)
                    {
                        newreference.SetFieldValue("_AUTHOR", contextlocal.UserId);
                    }
                    //newreference.SetFieldValue("_AUTHOR", contextlocal.UserId); 

                    /*
                    //infos liées a l'import cfao
                    //CUSTOMER_REFERENCE_INFOS;
                    Clipper_Param.GetlistParam(contextlocal);
                    string Field_value = Clipper_Param.GetPath("CUSTOMER_REFERENCE_INFOS");
                    newreference.SetFieldValue("CUSTOMER", ""); 
                    */
                    //champs specifiques 

                    //nous retournons un minimum d'infos pour la remontée de données technique
                    if (Sans_Donnees_Technique == true || (line_dictionnary.ContainsKey("ID_PIECE_CFAO") == false))
                    {

                        if (line_dictionnary.ContainsKey("ID_PIECE_CFAO") == false) { Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + ": piece manquante creer a la volée, un retour clip sera necessaire !! "); }
                        else { Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + ": ecriture des données sans dt !! "); }
                        newreference.SetFieldValue("AFFAIRE", AF_ImportTools.SimplifiedMethods.ConvertNullStringToEmptystring("AFFAIRE", ref line_dictionnary));
                        newreference.SetFieldValue("REMONTER_DT", true);
                        newreference.SetFieldValue("_REFERENCE", AF_ImportTools.SimplifiedMethods.ConvertNullStringToEmptystring("_DESCRIPTION", ref line_dictionnary));
                        newreference.SetFieldValue("EN_RANG", AF_ImportTools.SimplifiedMethods.ConvertNullStringToEmptystring("EN_RANG", ref line_dictionnary));
                        newreference.SetFieldValue("EN_PERE_PIECE", AF_ImportTools.SimplifiedMethods.ConvertNullStringToEmptystring("EN_PERE_PIECE", ref line_dictionnary));

                     


                    }

                    //concatenation affaire-nom-id
                    //construction d'une description de piece contenant nom matiere affaire.
                    //cette description peut etre exploitée en id d'import.

                    if (Data_Model.ExistsInDictionnary("AFFAIRE", ref line_dictionnary) && Sans_Donnees_Technique == false)
                    {
                        //concatenation dans le champs description
                        string affaire = line_dictionnary["AFFAIRE"].ToString().ToUpper();
                        string material_name = AF_ImportTools.Material.getMaterial_Name(contextlocal, material.Id32);
                        newreference.SetFieldValue("_REFERENCE", NewReferenceName + "-" + material_name + "-" + affaire);

                    }

                    newreference.Save();
                    //creation de la prepâration associée
                    AF_ImportTools.SimplifiedMethods.CreateMachinablePartFromReference(contextlocal, newreference, machine);

                }



                return newreference;

            }
            catch (Exception ie)
            {
                Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + " : Fails");
                System.Windows.Forms.MessageBox.Show(ie.Message);
                return null;
            }






        }



        /// <summary>
        ///Non utilisée
        /// controle de l'integerite des données du fichier texte
        /// on controle les champs obligatoires pour l'import, et l'existantce du centre de frais avant de continue l'import
        /// </summary>
        /// <param name="line_dictionnary">dictionnaire de ligne interprété par le datamodel</param>
        /// <returns>false ou tuue si integre</returns>

        public Boolean CheckDataIntegerity(IContext contextlocal, Dictionary<string, object> line_dictionnary) { return true; }

        /// <summary>
        /// controle de l'integerite des données du fichier texte
        /// on controle les champs obligatoires pour l'import, et l'existantce du centre de frais avant de continue l'import
        /// </summary>
        /// <param name="line_dictionnary">dictionnaire de ligne interprété par le datamodel</param>
        /// <returns>false ou tuue si integre</returns>
        public Boolean CheckDataIntegerity(IContext contextlocal, Dictionary<string, object> line_dictionnary, Dictionary<string, string> CentreFrais_Dictionnary, bool Sans_Donnees_Technique)
        {
            //
            try
            {
                ///////////////////////////////////////////////////////////////////////////
                ///condition cumulées sur result?                
                Boolean result = true;
                ///////////////////////////////////////////////////////////////////////////
                string currenfieldsname;
                ///matiere
                ///
                currenfieldsname = "_MATERIAL";
                if (line_dictionnary.ContainsKey(currenfieldsname))
                {
                    result = result & true;
                }
                else {
                    //MessageBox.Show(currenfieldsname + " : missing ");
                    Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + currenfieldsname + " : missing ");
                    Alma_Log.Write_Log_Important(MethodBase.GetCurrentMethod().Name + ":" + line_dictionnary["_MATERIAL"] + ": champs obligatoire : matiere non detectée sur la ligne a importée, line ignored"); result = result & false;
                    result = result & false;
                }


                ///epaisseur
                ///
                currenfieldsname = "THICKNESS";
                if (line_dictionnary.ContainsKey(currenfieldsname))
                { result = result & true; }
                else {
                    Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + currenfieldsname + " : missing ");
                    Alma_Log.Write_Log_Important(MethodBase.GetCurrentMethod().Name + ":" + line_dictionnary["_THICKNESS"] + ": champs obligatoire epaisseur non detectée sur la ligne a importée, line ignored"); result = result & false;
                    result = result & false; }


                //L' Affaire existe t elle ?
                currenfieldsname = "AFFAIRE";
                if (line_dictionnary.ContainsKey(currenfieldsname)) {
                    result = result & true;
                }
                else {
                    Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + currenfieldsname + " : missing ");
                    Alma_Log.Write_Log_Important(MethodBase.GetCurrentMethod().Name + ":" + line_dictionnary["_NAME"] + ": champs obligatoire Affaire non detectée sur la ligne a importée, line ignored"); result = result & false;
                    result = result & false; }


                ///////////////////////////////////////////////////////////////////////////
                //les quantités negatives sont interdites
                currenfieldsname = "_QUANTITY";
                if (line_dictionnary.ContainsKey(currenfieldsname))
                {
                    if (int.Parse(line_dictionnary["_QUANTITY"].ToString().Trim()) < 0 || int.Parse(line_dictionnary["_QUANTITY"].ToString().Trim()) == 0)
                    {
                        Alma_Log.Error(line_dictionnary["_NAME"] + ":_QUANTITY negative ou null detecté sur la ligne a importée, line ignored", MethodBase.GetCurrentMethod().Name);
                        Alma_Log.Write_Log_Important(MethodBase.GetCurrentMethod().Name + ":" + line_dictionnary["_NAME"] + ": champs obligatoire :_QUANTITY non detecté sur la ligne a importée, line ignored"); result = false;
                    }
                }
                else {
                    Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + currenfieldsname + " : missing ");
                    result = result & false; }

                ///////////////////////////////////////////////////////////////////////////
                //le nom de la piece à produire doit exister
                currenfieldsname = "_NAME";
                if (line_dictionnary.ContainsKey(currenfieldsname) != true)
                {   Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + currenfieldsname + " : missing ");
                    Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + ": champs obligatoire:  pas de nom de reference trouvée"); result = result & false;
                    Alma_Log.Write_Log_Important(MethodBase.GetCurrentMethod().Name + ":" + line_dictionnary["_NAME"] + ": champs obligatoire: pas de non de piece detecté sur la ligne a importée, line ignored"); result = result & false;
                }
                else { 
                    result = result & true; }

                //////////////////////////////////////////////////////////////////////////
                //la machine (centre de frais... )
                //si la ligne ne possede pas de cf  c'est que c'est une piece sans gamme, cette piece prendre la machine clipper par defaut
                currenfieldsname = "CENTREFRAIS";
                if (line_dictionnary.ContainsKey(currenfieldsname) != true)
                {
                    Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + ": pas de centre de frais --> aucune gamme detectées: piece Orange identifiée");
                    Alma_Log.Write_Log_Important(MethodBase.GetCurrentMethod().Name + ":" + line_dictionnary["_NAME"] + ": piece Orange identifiée : centre de frais non detecté sur la ligne a importée"); result = result & true;
                }
                else {
                    // si la machien envoyée n'existe pas on ne fait rien
                   
                    if (Data_Model.ExistsInDictionnary(line_dictionnary[currenfieldsname].ToString(), ref CentreFrais_Dictionnary) == false) {
                        Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + ": le centre de frais spécifié n'existe pas --> la ligne sera ignorée");
                        Alma_Log.Write_Log_Important(MethodBase.GetCurrentMethod().Name + ":" + line_dictionnary["_NAME"] + ":le centre de frais spécifié n'existe pas --> centre de frais non detecté sur la ligne à importée, la ligne sera ignorée");
                        result = result & false; }

                    result = result & true; }

                ///////////////////////////////////////////////////////////////////////////
                //les matieres sont désormais obligatoires
                //string nuance_name = line_dictionnary["_MATERIAL"].ToString().Replace('§', '*');
                string nuance = null;
                string material_name = null;
                double thickness = 0;

                nuance = line_dictionnary["_MATERIAL"].ToString().Replace('§', '*');
                thickness = Convert.ToDouble(line_dictionnary["THICKNESS"]);
                material_name = AF_ImportTools.Material.getMaterial_Name(contextlocal, nuance, thickness);

                if (material_name == string.Empty)
                { /*on log matiere non existante*/
                    Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + ": matiere non existante :" + nuance + " ep " + thickness);
                    result = result & false; }

                ///////////////////////////////////////////////////////////////////////////
                //les matieres sont désormais obligatoires
                //pour uniquement pourles lignes jaunes ( pas pour les ligne  sans_dt)

                if (line_dictionnary.ContainsKey("IDLNROUT") != true && Sans_Donnees_Technique == false)
                {
                    Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + ": champs obligatoire:  pas de numero de gamme unique indiqué"); result = result & false;
                    Alma_Log.Write_Log_Important(MethodBase.GetCurrentMethod().Name + ":" + line_dictionnary["IDLNROUT"] + ":champs obligatoire:  pas de numero de gamme unique indiqué sur la ligne a importée, line ignored"); result = result & false;
                }
                else { result = result & true; }

                if (line_dictionnary.ContainsKey("IDLNBOM") != true && Sans_Donnees_Technique == false)
                {
                    Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + ": champs obligatoire:  pas d'identification unique de piece trouvée"); result = result & false;
                    Alma_Log.Write_Log_Important(MethodBase.GetCurrentMethod().Name + ":" + line_dictionnary["IDLNBOM"] + ": champs obligatoire:  pas d'identification unique de piece detecté sur la ligne a importée, line ignored"); result = result & false;
                }
                else { result = result & true; }

                




                return result;
            }


            catch (Exception ie)
            {
                Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + ": erreur : "+ie.Message);
               // MessageBox.Show(ie.Message);
                return false;
            }



        }
        /// <summary>
        /// renvoie l'entite matiere a partie de la nuance et de l'epaisseur contenu dans le line dictionnary
        /// </summary>
        /// <param name="contextlocal">ientity context</param>
        /// <param name="material_name">ientity  material</param>
        /// <param name="line_dictionnary">dictionnary <string,object> line_dictionnary</param>
        /// <returns></returns>
        public IEntity GetMaterialEntity(IContext contextlocal, ref Dictionary<string, object> line_dictionnary)
        {
            IEntity material = null;

            try
            {

                //IEntityList materials = null;
                //verification simple par nom nuance*etat epaisseur en rgardnat une structure comme ceci
                //"SPC*BRUT 1.00" //attention pas de control de l'obsolecence pour le moment
                if (line_dictionnary.ContainsKey("_MATERIAL") && line_dictionnary.ContainsKey("THICKNESS"))
                {
                    material = Material.getMaterial_Entity(contextlocal, line_dictionnary["_MATERIAL"].ToString(), Convert.ToDouble(line_dictionnary["THICKNESS"]));
                }/*
                materials = contextlocal.EntityManager.GetEntityList("_MATERIAL", "_NAME", ConditionOperator.Equal, material_name);
                materials.Fill(false);

                if (materials.Count() > 0 && materials.FirstOrDefault().Status.ToString() == "Normal")
                { material = materials.FirstOrDefault(); }
                else { material = null; }*/


                return material;
            }
            catch (Exception ie)
            {
                Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + ": erreur :");
                MessageBox.Show(ie.Message);
                return material;
            }

        }

        /*
        /// <summary> ---> sera bientot supprimer                    
        /// recupere une reference en fonction d'un 
        /// numero d'indice //(remplacé par un indice d'identification piece en position 27)
        /// ce numero d'indice est egale a l'id de la piece dans la table reference sauf si etRef_IdFromQuoteTable est a true.
        /// dans ce cas l'indice est recuperer a partir de la tabloe des pieces quotée.
        /// </summary>
        /// <param name="contextlocal">contexte local</param>
        /// <param name="reference">entite reference</param>
        /// <param name="line_dictionnary">dictionnaire de ligne</param>
        /// <param name="GetRef_IdFromQuoteTable">si true on recherche l'id dans la tabel quote, si false , l'id des peices est renvoyé directement</param>
        /// <returns>true si la reference est detectee en fonction du numero de plan</returns>
        public bool GetReference(IContext contextlocal, ref IEntity reference, ref Dictionary<string, object> line_dictionnary, ref string NewReferenceName, bool GetRef_IdFromQuoteTable)
        {
            reference = null;
            //IEntityList references = null;
            Int32 new_reference_id = 0;
            IEntityList quote_part_list = null;
            IEntity quote_part = null;
            //IEntity material = null;
            bool result = false;

            try
            {
                //int index_extension = 7;
                if (Data_Model.ExistsInDictionnary("ID_PIECE_CFAO", ref line_dictionnary))
                { //IEntity reference sur la base d'un id de la quotepart

                    IEntityList reference_partlist;
                    IEntity reference_part;
                    Int32 id_piece_cfao;

                    if (line_dictionnary["ID_PIECE_CFAO"].GetType() == typeof(string))
                    {
                        id_piece_cfao = Convert.ToInt32(line_dictionnary["ID_PIECE_CFAO"]);
                    }
                    else
                    {
                        id_piece_cfao = (int)line_dictionnary["ID_PIECE_CFAO"];
                    }

                    //on recherche une reference cree par le devis ou alors la reference directement dans la table reference
                    if (GetRef_IdFromQuoteTable)
                    {
                        //on suppose que le champs est un indice de reference
                        reference_partlist = contextlocal.EntityManager.GetEntityList("_REFERENCE", "ID", ConditionOperator.Equal, id_piece_cfao);
                        reference_partlist.Fill(false);
                        if (reference_partlist.Count() > 0) {
                            new_reference_id = id_piece_cfao;
                        }
                        else {
                            //on regarde ensuite si le champs est negatif (--> a ce moment la c'est un quote part)
                            //depuis le sp3 on recherche dans quote part
                            quote_part_list = contextlocal.EntityManager.GetEntityList("_REFERENCE", "_QUOTE_PART", ConditionOperator.Equal, -id_piece_cfao);

                            quote_part = ImportTools.SimplifiedMethods.GetFirtOfList(quote_part_list);
                            if (quote_part != null) {
                                // on calcul l'id de reference sur la base du quote part
                                new_reference_id = quote_part.GetFieldValueAsInt("ID");
                            }
                            else {
                                // sinon erreur et on creer une nouvelle piece
                                Alma_Log.Error("Quote Part NON TROUVEE : ref " + id_piece_cfao.ToString(), MethodBase.GetCurrentMethod().Name + "Reference non trouvée : import impossible");
                                Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + ":" + id_piece_cfao + ": not found :");
                                result = false;
                            }
                            //

                        }

                    }
                    else {
                        //on recupere directment l'id de reference
                        new_reference_id = id_piece_cfao;
                    }

                    //on recupere la reference piece et on regarde si elle existe bien
                    reference_partlist = contextlocal.EntityManager.GetEntityList("_REFERENCE", "ID", ConditionOperator.Equal, new_reference_id);
                    reference_partlist.Fill(false);
                    reference_part = ImportTools.SimplifiedMethods.GetFirtOfList(reference_partlist);


                    if (reference_part != null)
                    {
                        NewReferenceName = reference_part.GetFieldValueAsString("_NAME");
                        reference = reference_part;
                        result = true;
                    }
                    else
                    {
                        Alma_Log.Error("REFERENCE NON TROUVEE : ref " + id_piece_cfao.ToString(), MethodBase.GetCurrentMethod().Name + "Reference non trouvée : import impossible");
                        Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + ":" + id_piece_cfao + ": not found :");
                        NewReferenceName = null;
                        result = false;
                    }



                }

                else
                {

                    //reference non indiqué on cree  une nouvelle piece 
                    Alma_Log.Error("AUCUNE REFERENCE TRANSMISE : ", MethodBase.GetCurrentMethod().Name + "Reference non trouvée : creation d'une nouvelle piece");
                    Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + ":Part id_cfao   : not found :");
                    NewReferenceName = line_dictionnary["_NAME"].ToString(); ;
                    result = false;

                }

                return result;


            }

            catch (Exception ie)
            {
                //on log
                Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + ": erreur :");
                MessageBox.Show(ie.Message);
                return result;
            }



        }
        */
        /// <summary>
        /// recupere une reference en fonction d'un 
        /// numero d'indice //(remplacé par un indice d'identification piece en position 27)
        /// ce numero d'indice est egale a l'id de la piece dans la table reference sauf si l'indice est negatif.
        /// Si l'indice est negatif alors l'indice vient d'une piece cotée.
        /// 
        /// </summary>
        /// <param name="contextlocal">contexte local</param>
        /// <param name="reference">entite reference</param>
        /// <param name="line_dictionnary">dictionnaire de ligne</param>
        /// <returns>true si la reference est detectee en fonction du numero de plan</returns>
        public bool GetReference(IContext contextlocal, ref IEntity reference, ref Dictionary<string, object> line_dictionnary, ref string NewReferenceName)
        {
            reference = null;
            //IEntityList references = null;
            Int32 new_reference_id = 0;
            IEntityList quote_part_list = null;
            IEntity quote_part = null;
            //IEntity material = null;
            bool result = false;


            try
            {
                //int index_extension = 7;
                if (Data_Model.ExistsInDictionnary("ID_PIECE_CFAO", ref line_dictionnary))
                { //IEntity reference sur la base d'un id de la quotepart

                    IEntityList reference_partlist;
                    IEntity reference_part;
                    Int32 id_piece_cfao = 0;

                    if (line_dictionnary["ID_PIECE_CFAO"].GetType() == typeof(string))
                    {
                        id_piece_cfao = Convert.ToInt32(line_dictionnary["ID_PIECE_CFAO"]);
                    }
                    else
                    {
                        id_piece_cfao = (int)line_dictionnary["ID_PIECE_CFAO"];
                    }

                    //on recherche une reference cree par le devis ou alors la reference directement dans la table reference                    
                    //on regarde ensuite si le champs est negatif (--> a ce moment la c'est un quote part)
                    if (id_piece_cfao < 0)
                    {
                        Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + ":Pièce" + id_piece_cfao + " venant d'almaquote identifiée. ");
                        //depuis le sp3 on recherche dans quote part
                        id_piece_cfao = id_piece_cfao * (-1);
                        quote_part_list = contextlocal.EntityManager.GetEntityList("_QUOTE_PART", "ID", ConditionOperator.Equal, id_piece_cfao);
                        quote_part = AF_ImportTools.SimplifiedMethods.GetFirtOfList(quote_part_list);
                        if (quote_part != null)
                        {
                            // on calcul l'id de reference sur la base du quote part
                            new_reference_id = quote_part.GetFieldValueAsInt("_ALMACAM_REFERENCE");
                            IEntityList Existreferences = contextlocal.EntityManager.GetEntityList("_REFERENCE", "ID", ConditionOperator.Equal, new_reference_id);
                            Existreferences.Fill(false);

                            //on test si la pieces existe vraiment dans les references
                            if (Existreferences.Count == 0)
                            {
                                Alma_Log.Error("REFERENCE NON TROUVEE dans LES PIECES AlmaCam : ref " + new_reference_id.ToString(), MethodBase.GetCurrentMethod().Name + "Reference non trouvée : import impossible");
                                Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + ":" + id_piece_cfao + ": not found :");
                                result = false;
                            }
                            
                        }
                        else {
                            // sinon erreur et on creer une nouvelle piece
                            Alma_Log.Error("Quote Part NON TROUVEE dans les pieces de devis : ref " + id_piece_cfao.ToString(), MethodBase.GetCurrentMethod().Name + "Quotepart non trouvée : import impossible");
                            Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + ":" + id_piece_cfao + ": not found :");
                            result = false;
                        }

                    }
                    else {
                        //on recupere directment l'id de reference
                        new_reference_id = id_piece_cfao;
                    }

                    //on recupere la reference piece et on regarde si elle existe bien
                    reference_partlist = contextlocal.EntityManager.GetEntityList("_REFERENCE", "ID", ConditionOperator.Equal, new_reference_id);
                    reference_partlist.Fill(false);
                    reference_part = AF_ImportTools.SimplifiedMethods.GetFirtOfList(reference_partlist);
                    


                    if (reference_part != null)
                    {
                        NewReferenceName = reference_part.GetFieldValueAsString("_NAME");
                        reference = reference_part;
                        result = true;
                    }
                    else
                    {
                        Alma_Log.Error("REFERENCE NON TROUVEE : ref " + id_piece_cfao.ToString(), MethodBase.GetCurrentMethod().Name + "Reference non trouvée : import impossible");
                        Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + ":" + id_piece_cfao + ": not found :");
                        NewReferenceName = null;
                        result = false;
                    }



                }

                else
                {

                    //reference non indiqué on cree  une nouvelle piece 
                    Alma_Log.Error("AUCUNE REFERENCE TRANSMISE : ", MethodBase.GetCurrentMethod().Name + "Reference non trouvée : crreation d'une nouvelle piece");
                    Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + ":Part id_cfao   : not found :");
                    NewReferenceName = line_dictionnary["_NAME"].ToString(); ;
                    result = false;
                    //result = true;


                }

                return result;


            }

            catch (Exception ie)
            {
                //on log
                Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + ": erreur :");
                MessageBox.Show(ie.Message);
                return result;
            }



        }
        
        /// met a jour les valeurs   dans les pieces a produires almacam
        /// </summary>
        /// <param name="contextlocal">contexte context</param>
        /// <param name="sheet">ientity sheet  </param>
        /// <param name="stock">inentity stock</param>
        /// <param name="line_dictionnary">dictionnary linedisctionary</param>
        /// <param name="type_tole">type tole  ou chute</param>
        /// 
        public void Update_Part_Item(IContext contextlocal, ref IEntity reference_to_produce, ref Dictionary<string, string> CentreFrais_Dictionnary, ref Dictionary<string, object> line_dictionnary)
        {
            try
            {
                foreach (var field in line_dictionnary)
                {
                    //on recupere la reference a usiner
                    //rien pour le moment
                    //on verifie que le champs existe bien avant de l'ecrire
                    if (contextlocal.Kernel.GetEntityType("_TO_PRODUCE_REFERENCE").FieldList.ContainsKey(field.Key))
                    {
                        //traitement specifique
                        switch (field.Key)
                        {

                            case "_MATERIAL":
                                //rien pour le moment mais on pourrait verifier si une nouvelle matiere est a declarer ou non
                                //recherche de l'epaisseur de la chaine 
                                //on importe jamais une matiere qui n'existe pas
                                
                                break;

                            case "CENTREFRAIS":


                                IEntityList centre_frais = contextlocal.EntityManager.GetEntityList("_CENTRE_FRAIS", "_CODE", ConditionOperator.Equal, field.Value);
                                centre_frais.Fill(false);
                                if (centre_frais.Count() > 0)
                                {
                                    //premier de la liste ou rien
                                    reference_to_produce.SetFieldValue(field.Key, centre_frais.FirstOrDefault());
                                    if (Data_Model.ExistsInDictionnary(centre_frais.FirstOrDefault().GetFieldValueAsString("_CODE"), ref CentreFrais_Dictionnary))
                                    { reference_to_produce.SetFieldValue("MACHINE_FROM_CF", CentreFrais_Dictionnary[centre_frais.FirstOrDefault().GetFieldValueAsString("_CODE")]); }
                                    else
                                    { centre_frais.FirstOrDefault().GetFieldValueAsString("_CODE"); }

                                }
                                else
                                {
                                    reference_to_produce.SetFieldValue("MACHINE_FROM_CF", string.Format(" !!{0} pas de correspondance machine sur ce centre de frais", field.Value.ToString()));
                                    reference_to_produce.SetFieldValue("NEED_PREP", true);
                                }
                                break;



                            case "_FIRM":

                                IEntityList firmlist = contextlocal.EntityManager.GetEntityList("_FIRM", "_NAME", ConditionOperator.Equal, field.Value);
                                firmlist.Fill(false);
                                if (firmlist.Count() > 0)
                                {
                                    //premier de la liste ou rien
                                    reference_to_produce.SetFieldValue(field.Key, firmlist.FirstOrDefault().Id);
                                   /* if (Data_Model.ExistsInDictionnary(firmlist.FirstOrDefault().GetFieldValueAsString("_CODE"), ref CentreFrais_Dictionnary))
                                    { reference_to_produce.SetFieldValue("_FIRM", CentreFrais_Dictionnary[FIRM.FirstOrDefault().GetFieldValueAsString("_CODE")]); }
                                    else
                                    { centre_frais.FirstOrDefault().GetFieldValueAsString("_CODE"); }*/

                                }

                                break;

                            case "IDLNROUT":
                                //on verifie si la reference n'exist pas deja

                                IEntityList idlnrout = contextlocal.EntityManager.GetEntityList("_TO_PRODUCE_REFERENCE", "IDLNROUT", ConditionOperator.Equal, field.Value);
                                idlnrout.Fill(false);
                                if (idlnrout.Count() == 0)
                                {
                                    reference_to_produce.SetFieldValue(field.Key, field.Value);
                                }
                                else
                                { //pas de mise à jour des quantités a produire                                         
                                    //write **_
                                    MessageBox.Show(string.Format("La gamme n° {0} a été trouvé en double, elle sera prefixé du caractère **_ pour control", field.Value));
                                    reference_to_produce.SetFieldValue(field.Key, "**_" + field.Value);
                                    //eventuellement on lance une exception
                                    //throw new InvalidDataException("doublon sur le numéro de gamme  'idlnrout' voir numero prefixé par **_"); 

                                }

                                break;

                            case "ECOQTY":

                                //formatage de La date;
                                //en cas d'erreur sur les types /// les ecoqty sont toujours en string mais dans certains base  on peut avoir l'erreur
                                if (reference_to_produce.GetFieldValue("ECOQTY").GetType()==typeof(Int64)) {
                                    reference_to_produce.SetFieldValue(field.Key, int.Parse (field.Value.ToString()));
                                }
                                else
                                {
                                    reference_to_produce.SetFieldValue(field.Key, field.Value);
                                }

                  

                                break;

                            case "STARTDATE":
                                //formatage de La date;
                                reference_to_produce.SetFieldValue("_DATE", field.Value);
                                reference_to_produce.SetFieldValue(field.Key, field.Value);

                                break;

                            case "AF_CDE":
                                //formatage de La date;
                                reference_to_produce.SetFieldValue("_CLIENT_ORDER_NUMBER", field.Value);
                                //reference_to_produce.SetFieldValue(field.Key, field.Value);

                                break;

                            default:
                                reference_to_produce.SetFieldValue(field.Key, field.Value);
                                break;
                        }
                    }


                }
                
            }
            catch (Exception ie)
            {
                Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + ": erreur :");
                MessageBox.Show(ie.Message);
            }
        }




        /// <summary>
        /// en standard
        /// import un of 
        /// </summary>
        /// <param name="contextlocal">contexte alma cam</param>
        /// <param name="pathToFile">chemin vers le fichier csv separateur ";"</param>
        /// <param name="sans_donnees_technique">true si import sans données techniques</param>
        /// <param name="DataModelString">string de description d'une ligne csv sous la forme 
        /// numeroIndex#NomChampAlmaCam#Type  exemple : 0#AFFAIRE#STRING</param>
        /// <summary>
        public void Import(IContext contextlocal, string pathToFile, string DataModelString, Boolean Sans_Donnees_Technique)
        {

            //recuperation des path
            CsvImportPath = pathToFile;


            try

            {
                //verification standards
                //creation du timetag d'import
                string timetag = string.Format("{0:d_M_yyyy_HH_mm_ss}", DateTime.Now);
                Alma_Log.Create_Log(Clipper_Param.getVerbose_Log());
                //bool import_sans_donnee_technique = false;
                Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + ": importe tag :" + timetag);
                //ouverture du fichier csv lancement du curseur
                // Set cursor as hourglass
                Cursor.Current = Cursors.WaitCursor;



                if (File.Exists(CsvImportPath) == false) {
                    Alma_Log.Error("Fichier Non Trouvé:" + CsvImportPath, MethodBase.GetCurrentMethod().Name);
                    throw new Exception("csv File Note Found:\r\n" + CsvImportPath); }
                //avec ou sans dt
                if (Sans_Donnees_Technique) { Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + ": import  sans dt !! " + CsvImportPath); } else { Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + ": import standard !! " + CsvImportPath); }


                using (StreamReader csvfile = new StreamReader(CsvImportPath, Encoding.Default))
                {
                    //recuperation des elements de la base almacam
                    //declaration des dictionaires
                    Dictionary<string, object> line_Dictionnary = new Dictionary<string, object>();
                    //on lit les centres de frais 
                    Dictionary<string, string> CentreFrais_Dictionnary = null;
                    Data_Model.setFieldDictionnary(DataModelString);
                    Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + ": reading data model :success !! ");
                    ///remplissage des machines et verification de la presence du centre de frais demandé par clipper
                    ///plus utile
                    //IEntityList machine_liste = contextlocal.EntityManager.GetEntityList("_CUT_MACHINE_TYPE");
                    //machine_liste.Fill(false);

                    //recuperation de la machine clipper
                    IEntity Clipper_Machine = null;
                    IEntity Clipper_Centre_Frais = null;
                    //recuperation de la machine clipper et construction de la liste machine
                    Get_Clipper_Machine(contextlocal, out Clipper_Machine, out Clipper_Centre_Frais, out CentreFrais_Dictionnary);

                    //verification que toutes les machines sont conformes pour une intégration clipper


                    int ligneNumber = 0;
                    //lecture à la ligne
                    string line;
                    line = null;

                    while (((line = csvfile.ReadLine()) != null))
                    {

                        //on ne traite pas les lignes vides
                        ligneNumber++;
                        if ((line.Trim()) == "")
                        { Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + " : " + ligneNumber + ": empty line detected !! ");
                            continue;
                        }

                        //lecture des donnees
                        line_Dictionnary = Data_Model.ReadCsvLine_With_Dictionnary(line);
                        Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + " : " + ligneNumber + ": line disctionnary interpreter succeeded !! ");

                        //control des données    //verification des donnée importées
                        if (CheckDataIntegerity(contextlocal, line_Dictionnary, CentreFrais_Dictionnary, Sans_Donnees_Technique) != true)
                        {
                            /*on log et on continue(on passe a la ligne suivante*/
                            Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + " : " + ligneNumber + ": data integrity fails, line ignored !!! ");
                            continue;
                        }





                        IEntity reference_to_produce = null;
                        IEntity reference = null;
                        string NewReferenceName = null;
                        //
                        //on recherche la refeence avec la bonne matiere /epaisseur si elles n'existe pas on la creer 
                        if (GetReference(contextlocal, ref reference, ref line_Dictionnary, ref NewReferenceName) == false | Sans_Donnees_Technique == true)
                        {
                            /*aucune reference n'existe dans cette matiere  alors on  la creer*/
                            /*sauf si NewReferenceName est null */
                            if (NewReferenceName != null) {
                                reference = CreateNewReference(contextlocal, line_Dictionnary, ref NewReferenceName, Clipper_Machine, Sans_Donnees_Technique);
                                Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + " : " + ligneNumber + " no reference found new part creation : success. ");
                                //on active le need prep
                                //need_prep = true;
                            }
                            else {

                                continue;
                            }

                        }

                        //////on met a jour les données sur les piece 2d  : CUSTOMER_REFERENCE_INFOS
                        if (reference != null)
                        {///champs spécifique piece 2d
                            //infos liées a l'import cfao
                            //CUSTOMER_REFERENCE_INFOS;
                            Clipper_Param.GetlistParam(contextlocal);
                            string Field_value = Clipper_Param.GetPath("CUSTOMER_REFERENCE_INFOS");
                            Field_value.Split('|');//"CUSTOMER"
                            reference.SetFieldValue(Field_value.Split('|')[0], line_Dictionnary[Field_value.Split('|')[1]]);
                            reference.Save();
                        }




                        //creation de la nouvelle piece a produire associée
                        if (Sans_Donnees_Technique == false) {
                            CreateNewPartToProduce(contextlocal, line_Dictionnary, CentreFrais_Dictionnary, ref reference_to_produce, ref reference, timetag, Sans_Donnees_Technique);
                        }
                    }



                }

                // Set cursor as default arrow
                Cursor.Current = Cursors.Default;
                File_Tools.Rename_Csv(CsvImportPath, timetag);
                Alma_Log.Final_Open_Log();
                //File_tools
            }

            catch (Exception e)
            {
                Alma_Log.Write_Log(e.Message);
                Alma_Log.Final_Open_Log();

            }
        }

    }

    #endregion



    //retour gp// retourne les informations de piece
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////IMPORT///////////////OF////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region retour_GP
    /// <summary> reajustement pour retour gp
    /// on recupere avant un tole de meme matiere meme epaisseur pour calculer un ratio de poids
    /// on estime un temps de coupe
    /// on recupere les infos de pîece ..rang...
    ///Print #1, "ENGAPI;"+CodePiece+";"+Affaire+";"+Designation+";"+MonDPR+".emf"+";"+Format$(Poids,"#0.0#################")+";"+EN_RANG+";"+EN_PERE_PIECE
    ///'Print #1, "GAPIECE;"+cfrais+";;"+Format((Temps*60/100),"##0.0###############")
    ///'vb22122014 unité de temps en heure decimale (actctut est en minutes decimales)
    /// Print #1, "GAPIECE;"+cfrais+";;"+Format((Temps/60),"##0.0###############")
    ///	Print #1, "NOMENPIECE;"+CodeMat+";"+CStr(Xtole)+";"+CStr(YTole)+";"+Format(SurfPiece/SurfTole,"#0.0#################")+";"+Split(Capable," ")(0)+";"+Split(Capable," ")(1)
    ///ENGAPI;P101;3150;250 X 250;c:\alma\data\laser\formes\p102.dpr.emf;5,67555315;1;P101
    ///GAPIECE;HLASE;;0,0177742074004079
    ///NOMENPIECE;TL*S235*JR+AR*5;3200;1500;0,0304193889166667;610.78;239.06
    /// </summary>
    /// attention remonter les infos sur les nested parts
    /// <summary>
    /// impossible pour le moment car les pieces/geométries ne sont pas modifiables apres coups
    /// </summary>
    public class Clipper_RemonteeDt : IDisposable
    {
        public void Dispose()
        {
            ///purge
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// /// retoune la premiere tole dans une matiere donnée 
        /// </summary>
        /// <param name="contexlocal">contexte</param>
        /// <param name="part">entite reference ou piece</param>
        /// <param name="stock">entite du stock : contient l'entitté du stock trouvée</param>
        /// <param name="sheet">entitte sheet: contient la tole trouvée</param>
        /// <returns>True ou false selon si une tole est trouvée</returns>
        public bool GetRadomSheet(IContext contexlocal, IEntity part, out IEntity stock, out IEntity sheet)

        {   //on initialise sheet et stock
            sheet = null;
            stock = null;
            bool result = false;
            IEntityList sheets, stocks = null;


            try {
                //recherche d'element de stock
                sheets = contexlocal.EntityManager.GetEntityList("_SHEET", "_MATERIAL", ConditionOperator.Equal, part.GetFieldValueAsEntity("_MATERIAL").Id32);
                sheets.Fill(false);

                if (sheets.Count() > 0)
                {
                    foreach (IEntity sh in sheets.ToList<IEntity>())
                    {
                        if (sh.Status.ToString() == "Normal")
                        {
                            stocks = contexlocal.EntityManager.GetEntityList("_STOCK", "_SHEET", ConditionOperator.Equal, sh.Id32);
                            stock = AF_ImportTools.SimplifiedMethods.GetFirtOfList(stocks);

                            if (stocks.Count() > 0)
                            {
                                stock = stocks.FirstOrDefault();
                                if ((stock != null) & (stock.GetFieldValueAsLong("_QUANTITY") > 0))
                                {

                                    sheet = sh;
                                    break;
                                }

                            }
                        }


                    }

                }
                //ImportTools.SimplifiedMethods.GetFirtOfList(sheets);

                result = true;
                return result;
            }

            catch {

                Alma_Log.Write_Log(part.GetFieldValueAsString("_NAME") + ": no matierial found " + part.GetFieldValueAsEntity("_MATERIAL").GetFieldValueAsString("NAME"));
                Alma_Log.Write_Log_Important(part.GetFieldValueAsString("_NAME") + ":" + part.GetFieldValueAsEntity("_MATERIAL").GetFieldValueAsString("NAME") + " Pas de stock pour cette piece");
                return result;
            }
        }
        /// <summary>
        /// retourn true si la donnée a exportée sont valides
        /// </summary>
        /// <param name="contextlocal">icontext context</param>
        /// <param name="referenceToProduce">entité referenceToProduce</param>
        /// <returns></returns>
        public bool CheckDataIntegrity(IContext contextlocal, IEntity referenceToProduce) {
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="contextlocal"></param>
        /// <param name="Clipper_Machine"></param>
        /// <param name="Clipper_Centre_Frais"></param>
        /// <param name="CentreFrais_Dictionnary"></param>
        /// <returns>retourn la liste des machines et le centre de frais clipper : attention le centre de frais est une clé unique</returns>
        public Boolean Get_Clipper_Machine(IContext contextlocal, out IEntity Clipper_Machine, out IEntity Clipper_Centre_Frais, out Dictionary<string, string> CentreFrais_Dictionnary)
        {



            CentreFrais_Dictionnary = new Dictionary<string, string>();
            IEntityList machine_liste = null;
            //recuperation de la machine clipper et initialisation des listes
            //CentreFrais_Dictionnary = null;
            Clipper_Machine = null;
            Clipper_Centre_Frais = null;
            //CentreFrais_Dictionnary.Clear();
            //verification que toutes les machineS sont conformes pour une intégration clipper
            ///remplissage des machines et verification de la presence du centre de frais demandé par clipper
            machine_liste = contextlocal.EntityManager.GetEntityList("_CUT_MACHINE_TYPE");
            machine_liste.Fill(false);


            foreach (IEntity machine in machine_liste)
            {
                IEntity cf = machine.GetFieldValueAsEntity("CENTREFRAIS_MACHINE");
                ///creation du dictionnaire des machines installées   
                if (cf.DefaultValue != "" && machine.DefaultValue != "" && Clipper_Param.get_Clipper_Machine_Cf() != null)
                {

                    CentreFrais_Dictionnary.Add(cf.DefaultValue, machine.DefaultValue);
                    if (cf.DefaultValue == Clipper_Param.get_Clipper_Machine_Cf())
                    {
                        if (Clipper_Param.get_Clipper_Machine_Cf() != "Undef clipper machine")
                        {
                            Clipper_Centre_Frais = cf;
                            Clipper_Machine = machine;
                        }
                        else
                        {
                            Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + ": Missing  clipper machine !!! ");
                            Alma_Log.Error("IL MANQUE LA MACHINE CLIPPER !!!", MethodBase.GetCurrentMethod().Name);
                            return false;//throw new Exception(machine.DefaultValue + " : Missing  cost center definition"); 
                        }

                    }

                }

                else { /*on log on arrete tout */
                    Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + ": Missing  cost center definition on a machine !!! ");
                    Alma_Log.Error("IL MANQUE LE CENTRE DE FRAIS SUR L UNE DES MACHINES INSTALLEE !!!", MethodBase.GetCurrentMethod().Name);
                    return false;//throw new Exception(machine.DefaultValue + " : Missing  cost center definition"); 
                }
            }
            return true;


        }
        /// <summary>
        /// retourne le fichier d'echange clipper.
        ///exemple de creation du fichier de retour
        ///ENGAPI;P101;3150;250 X 250;c:\alma\data\laser\formes\p102.dpr.emf;5,67555315;1;P101
        ///GAPIECE;HLASE;;0,0177742074004079
        ///NOMENPIECE;TL*S235*JR+AR*5;3200;1500;0,0304193889166667;610.78;239.06
        /// </summary>
        /// <param name="contextlocal">contexte courant</param>
        public void Export_To_File(IContext contextlocal)
        {
            //recupere les path
            Clipper_Param.GetlistParam(contextlocal);
            string CsvExportPath = Clipper_Param.GetPath("EXPORT_Dt") + "\\DonnesTech.txt";
            //chargement de la liste de piece a retourner
            IEntitySelector select_to_produce_list = new EntitySelector();
            //on retourne les pieces marquées "sansdt"

            IEntityList sans_dt_filter = contextlocal.EntityManager.GetEntityList("_TO_PRODUCE_REFERENCE", "SANS_DT", ConditionOperator.Equal, true);
            sans_dt_filter.Fill(false);

            IExtendedEntityList references_sansdt = contextlocal.EntityManager.GetExtendedEntityList("_TO_PRODUCE_REFERENCE", sans_dt_filter);
            references_sansdt.Fill(false);


            select_to_produce_list.Init(contextlocal, references_sansdt);
            select_to_produce_list.MultiSelect = true;
            //select_to_produce_list.Init()

            //IEntityList machineList = Command.WorkOnEntityList; 

            //ecriture du fichier

            using (StreamWriter csvfile = new StreamWriter(CsvExportPath, true))
            {


                if (select_to_produce_list.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    //on control si la matiere est la meme que la matiere precedement demandée

                    foreach (IEntity to_produce_ref in select_to_produce_list.SelectedEntity)
                    {
                        IEntity stock = null;
                        IEntity sheet = null;
                        //IEntity current_machine = null;
                        IEntity selected_reference = null;


                        IMachineManager machinemanager = new MachineManager();
                        selected_reference = to_produce_ref.GetFieldValueAsEntity("_REFERENCE");
                        //reference non vide et integrité
                        if (selected_reference != null && CheckDataIntegrity(contextlocal, to_produce_ref) == true)
                        {

                            //creation d'un part info
                            AF_ImportTools.PartInfo part_infos = new PartInfo();

                            part_infos.GetPartinfos(ref contextlocal, selected_reference);

                            //ImportTools.Machine_Info.GetDefaultMachine(selected_reference, out current_machine);
                            //ICutMachine cutmachine = machinemanager.GetCutMachine(contextlocal, pi.DefaultMachine);
                            //ImportTools.Machine_Info.GetFeedList(contextlocal, current_machine);

                            //IEntity selected_reference =null;
                            //pour facilite l'ecriture du fichier de sortie on stock toutes les infos dans des listes d'objets
                            List<object> engapi = new List<object>();
                            List<object> gapiece = new List<object>();
                            List<object> nomenpiece = new List<object>();
                            string separator = ";";

                            // selected_reference = to_produce_ref.GetFieldValueAsEntity("_REFERENCE");


                            //recuperation d'une tole/stock dont la matiere est egale a celle de la  piece
                            if (GetRadomSheet(contextlocal, to_produce_ref.GetFieldValueAsEntity("_REFERENCE"), out stock, out sheet) != true) {
                                Alma_Log.Write_Log_Important("aucun element de stock n' a ete trouve, seules les informations renseignées ou calculable vont etre renvoyee ");
                            };


                            //recuperation des infos de la piece
                            double surface = 0;
                            double parttime = part_infos.Quote_part_cyle_time;
                            //unité en m2
                            //surface = selected_reference.GetFieldValueAsDouble("_SURFACE") * 10E-6;
                            surface = part_infos.Surface * 10E-6;
                            string codepiece = SimplifiedMethods.ConvertNullStringToEmptystring(selected_reference.GetFieldValueAsString("_NAME"));
                            //unite en mm
                            //Double thickness = selected_reference.GetFieldValueAsEntity("_MATERIAL").GetFieldValueAsDouble("_THICKNESS");
                            double thickness = part_infos.Thickness;
                            //string matiere = selected_reference.GetFieldValueAsEntity("_MATERIAL").GetFieldValueAsString("_NAME");
                            string matiere = SimplifiedMethods.ConvertNullStringToEmptystring(part_infos.Material);
                            //double xdim = selected_reference.GetFieldValueAsDouble("_DIMENS1");
                            //double ydim = selected_reference.GetFieldValueAsDouble("_DIMENS2");
                            double xdim = part_infos.Width;
                            double ydim = part_infos.Height * 10E-3;
                            //double poids = selected_reference.GetFieldValueAsDouble("_WEIGHT") * 10E-3;
                            double poids = part_infos.Weight;

                            //données clipper de la piece
                            string affaire = SimplifiedMethods.ConvertNullStringToEmptystring(to_produce_ref.GetFieldValueAsString("AFFAIRE"));
                            string description = SimplifiedMethods.ConvertNullStringToEmptystring(to_produce_ref.GetFieldValueAsString("_DESCRIPTION"));
                            //string emffile = part_infos.EmfFile;
                            string emffile;
                            if (to_produce_ref.GetFieldValueAsEntity("_REFERENCE") != null) {
                                //emffile = SimplifiedMethods.ConvertNullStringToEmptystring(@to_produce_ref.GetFieldValueAsEntity("_REFERENCE").GetImageFieldValueAsLinkFile("_PREVIEW"));
                                emffile = SimplifiedMethods.GetPreview(to_produce_ref);
                            } else { emffile = ""; }

                            string en_rang = SimplifiedMethods.ConvertNullStringToEmptystring(to_produce_ref.GetFieldValueAsString("EN_RANG"));
                            string en_pere_piece = SimplifiedMethods.ConvertNullStringToEmptystring(to_produce_ref.GetFieldValueAsString("EN_PERE_PIECE"));
                            string centrefrais = "";
                            if (to_produce_ref.GetFieldValueAsEntity("CENTREFRAIS") != null) {
                                centrefrais = SimplifiedMethods.ConvertNullStringToEmptystring(to_produce_ref.GetFieldValueAsEntity("CENTREFRAIS").GetFieldValueAsString("_CODE"));
                            } else { centrefrais = ""; }
                            //string centrefrais = SimplifiedMethods.ConvertEmptyStringToNullstring(to_produce_ref.GetFieldValueAsEntity("CENTREFRAIS").ToString());
                            string codematiere = SimplifiedMethods.ConvertNullStringToEmptystring(stock.GetFieldValueAsString("_NAME"));
                            //recuperation des infos de la tole
                            double xDim_Tole = sheet.GetFieldValueAsDouble("_LENGTH");
                            double yDim_Tole = sheet.GetFieldValueAsDouble("_WIDTH");
                            //double surface_Tole = sheet.GetFieldValueAsDouble("_SURFACE");
                            //pour le moment on considere que la surface de la tole est
                            double surface_Tole = xDim_Tole * yDim_Tole * 10E-6;
                            string nomdelaTole = SimplifiedMethods.ConvertNullStringToEmptystring(stock.GetFieldValueAsString("_NAME"));
                            //section engapi
                            engapi.Add("ENGAPI");
                            engapi.Add(codepiece);//nacleunik
                            engapi.Add(affaire);//numero_gamme
                            engapi.Add(description);
                            engapi.Add(emffile);//emf
                            engapi.Add(string.Format("{0:0.00000}", poids));
                            engapi.Add(en_rang);
                            engapi.Add(en_pere_piece);
                            //engapi.Add(indice_c);
                            //section gapiece, données clipper de la gamme
                            gapiece.Add("GAPIECE");
                            gapiece.Add(centrefrais);
                            //pour le moment ces valeurs sont nulles
                            gapiece.Add(string.Format("{0:0.00000}", parttime));
                            gapiece.Add("");
                            ///
                            nomenpiece.Add("NOMEPIECE");
                            nomenpiece.Add(codematiere);
                            nomenpiece.Add(xDim_Tole);
                            nomenpiece.Add(yDim_Tole);

                            if (surface_Tole > 0)
                            {
                                nomenpiece.Add(string.Format("{0:0.00000}", surface / surface_Tole));
                            }

                            nomenpiece.Add(xdim);
                            nomenpiece.Add(ydim);

               

                            string myline = "";
                            foreach (object o in engapi)
                            {
                                myline += o.ToString() + separator;
                            }
                            csvfile.WriteLine(myline);

                            //ecriture de gapiece
                            myline = "";
                            foreach (object o in gapiece)
                            {
                                myline += o.ToString() + separator;
                            }
                            csvfile.WriteLine(myline);
                            myline = "";
                            //ecriture de nomepiece
                            foreach (object o in nomenpiece)
                            {
                                myline += o.ToString() + separator;
                            }
                            csvfile.WriteLine(myline);
                            //on set le setfiled value a false

                            to_produce_ref.SetFieldValue("SANS_DT", false);
                            to_produce_ref.Save();
                            part_infos = null;

                        }
                    }

                }
                csvfile.Close();

            }


        }




        /// <summary>
        /// rxport un dossier technique
        /// </summary>
        /// <param name="contextlocal"></param>
        /// <param name="EngapiOnly">si false alors, la ligne de gamme n'est pas exportée</param>
        public void Export_Piece_To_File(IContext contextlocal, bool EngapiOnly)
        {
            //bool export ligne de gamme
            //bool export_ 
            //var answer= MessageBox.Show.   

            DialogResult result1 = MessageBox.Show("Voulez vous exporter les informations de gammes?", "WARNING !!!", MessageBoxButtons.YesNo);

            if (result1 == DialogResult.Yes)
            { EngapiOnly = false;
            }


            //recupere les path

            Clipper_Param.GetlistParam(contextlocal);
            string CsvExportPath = Clipper_Param.GetPath("EXPORT_Dt") + "\\DonnesTech.txt";
            //chargement de la liste de piece a retourner
            IEntitySelector select_reference_list = new EntitySelector();
            IEntitySelector select_preparation_list = new EntitySelector();

            IEntity clipper_machine = null;
            IEntity centre_frais_clipper = null;

            Dictionary<string, string> centre_frais_dictionnary = null;
            centre_frais_dictionnary = new Dictionary<string, string>();
            //machine clipper
            Get_Clipper_Machine(contextlocal, out clipper_machine, out centre_frais_clipper, out centre_frais_dictionnary);
            //on retourne les pieces marquées "sansdt"

            IEntityList sans_dt_filter = contextlocal.EntityManager.GetEntityList("_REFERENCE", "REMONTER_DT", ConditionOperator.Equal, true);
            sans_dt_filter.Fill(false);


            IExtendedEntityList references_sansdt = contextlocal.EntityManager.GetExtendedEntityList("_REFERENCE", sans_dt_filter);
            references_sansdt.Fill(false);

            select_reference_list.Init(contextlocal, references_sansdt);
            select_reference_list.MultiSelect = true;




            using (StreamWriter csvfile = new StreamWriter(CsvExportPath, true))
            {


                if (select_reference_list.ShowDialog() == System.Windows.Forms.DialogResult.OK)

                {
                    //on control si la matiere est la meme que la matiere precedement demandée

                    foreach (IEntity reference in select_reference_list.SelectedEntity)
                    {
                        IEntity stock = null;
                        IEntity sheet = null;
                        //IEntity current_machine = null;

                        //IEntity to_produce_ref = null;
                        IMachineManager machinemanager = new MachineManager();

                        //reference non vide et integrité
                        if (reference != null && CheckDataIntegrity(contextlocal, reference) == true)
                        {

                            //creation d'un part info
                            AF_ImportTools.PartInfo part_infos = new PartInfo();
                            part_infos.GetPartinfos(ref contextlocal, reference);


                            //pour facilite l'ecriture du fichier de sortie on stock toutes les infos dans des listes d'objets
                            List<object> engapi = new List<object>();
                            List<object> gapiece = new List<object>();
                            List<object> nomenpiece = new List<object>();
                            string separator = ";";

                            // selected_reference = to_produce_ref.GetFieldValueAsEntity("_REFERENCE");


                            //recuperation d'une tole/stock dont la matiere est egale a celle de la  piece
                            if (GetRadomSheet(contextlocal, reference, out stock, out sheet) != true)
                            {
                                Alma_Log.Write_Log_Important("aucun element de stock n' a ete trouve, seules les informations renseignées ou calculables vont etre renvoyees ");
                            };


                            //recuperation des infos de la piece
                            double surface = 0;
                            double parttime = part_infos.Quote_part_cyle_time + 0.1;
                            //unité en m2
                            //surface = selected_reference.GetFieldValueAsDouble("_SURFACE") * 10E-6;
                            surface = part_infos.Surface * 10E-6;
                            string codepiece = SimplifiedMethods.ConvertNullStringToEmptystring(reference.GetFieldValueAsString("_NAME"));
                            //unite en mm

                            double thickness = part_infos.Thickness;
                            //string matiere = selected_reference.GetFieldValueAsEntity("_MATERIAL").GetFieldValueAsString("_NAME");
                            string matiere = SimplifiedMethods.ConvertNullStringToEmptystring(part_infos.Material);
                            double feed = AF_ImportTools.Machine_Info.GetFeed(contextlocal, part_infos.DefaultMachineEntity, part_infos.MaterialEntity);

                            //double xdim = selected_reference.GetFieldValueAsDouble("_DIMENS1");
                            //double ydim = selected_reference.GetFieldValueAsDouble("_DIMENS2");
                            double xdim = part_infos.Width;
                            double ydim = part_infos.Height * 10E-3;
                            //double poids = selected_reference.GetFieldValueAsDouble("_WEIGHT") * 10E-3;
                            double poids = part_infos.Weight;

                            //données clipper de la piece
                            string affaire = SimplifiedMethods.ConvertNullStringToEmptystring(reference.GetFieldValueAsString("AFFAIRE"));
                            string description = SimplifiedMethods.ConvertNullStringToEmptystring(reference.GetFieldValueAsString("_REFERENCE"));
                            //string emffile = part_infos.EmfFile;
                            string emffile;
                            /// a revoir avec les grometrie par defaut
                            //emffile = SimplifiedMethods.ConvertNullStringToEmptystring(@reference.GetImageFieldValueAsLinkFile("_PREVIEW"));
                            emffile = SimplifiedMethods.GetPreview(reference);
                            string en_rang = SimplifiedMethods.ConvertNullStringToEmptystring(reference.GetFieldValueAsString("EN_RANG"));
                            string en_pere_piece = SimplifiedMethods.ConvertNullStringToEmptystring(reference.GetFieldValueAsString("EN_PERE_PIECE"));
                            string idlnbom = SimplifiedMethods.ConvertNullStringToEmptystring(reference.GetFieldValueAsString("IDLNBOM"));
                            string referenceId = reference.Id32.ToString();

                            string centrefrais = "";
                            centrefrais = SimplifiedMethods.ConvertNullStringToEmptystring(reference.GetFieldValueAsEntity("_DEFAULT_CUT_MACHINE_TYPE").GetFieldValueAsEntity("CENTREFRAIS_MACHINE").GetFieldValueAsString("_CODE"));

                            //string centrefrais = SimplifiedMethods.ConvertEmptyStringToNullstring(to_produce_ref.GetFieldValueAsEntity("CENTREFRAIS").ToString());
                            string codematiere = SimplifiedMethods.ConvertNullStringToEmptystring(stock.GetFieldValueAsString("_NAME"));
                            //recuperation des infos de la tole
                            double xDim_Tole = sheet.GetFieldValueAsDouble("_LENGTH");
                            double yDim_Tole = sheet.GetFieldValueAsDouble("_WIDTH");
                            //double surface_Tole = sheet.GetFieldValueAsDouble("_SURFACE");
                            //pour le moment on considere que la surface de la tole est
                            double surface_Tole = xDim_Tole * yDim_Tole * 10E-6;
                            string nomdelaTole = SimplifiedMethods.ConvertNullStringToEmptystring(stock.GetFieldValueAsString("_NAME"));
                            //section engapi

                            engapi.Add("ENGAPI");
                            engapi.Add(codepiece);
                            engapi.Add(affaire);
                            engapi.Add(description);
                            engapi.Add(emffile);
                            engapi.Add(string.Format("{0:0.00000}", poids / 1000));
                            //engapi.Add(idlnbom);
                            engapi.Add(en_rang);
                            engapi.Add(en_pere_piece);
                            engapi.Add(referenceId);
                            //var answer= MessageBox.Show.   
                            //DialogResult result1 = MessageBox.Show("Voulez vous exporter les informations de gammes?", "WARNING !!!",  MessageBoxButtons.YesNo);
                            // if (result1 == DialogResult.Yes && EngapiOnly == true)
                            if (EngapiOnly == false)
                            {
                                gapiece.Add("GAPIECE");
                                gapiece.Add(centrefrais);
                                //pour le moment ces valeurs sont nulles
                                gapiece.Add(string.Format("{0:0.10000}", parttime));
                                gapiece.Add("");
                                ///
                            }
                            nomenpiece.Add("NOMENPIECE");
                            nomenpiece.Add(codematiere);
                            nomenpiece.Add(xDim_Tole);
                            nomenpiece.Add(yDim_Tole);


                            if (surface_Tole > 0)
                            {
                                nomenpiece.Add(string.Format("{0:0.00000}", surface / surface_Tole));
                            }

                            nomenpiece.Add(xdim);
                            nomenpiece.Add(ydim);

                       

                            string myline = "";
                            var last_o = engapi.LastOrDefault();
                            separator = ";";
                            foreach (object o in engapi)
                            { if (o.Equals(last_o))
                                { separator = ""; }
                                myline += o.ToString() + separator;
                            }
                            last_o = null;
                            csvfile.WriteLine(myline.Replace(",", "."));

                            //ecriture de gapiece uniquement si retour possible
                            //actuellement si la gamme existe clipper rajoute une nouvelle gamme
                            if (EngapiOnly == false) {
                                myline = "";
                                last_o = gapiece.LastOrDefault();
                                separator = ";";

                                foreach (object o in gapiece)
                                {
                                    if (o.Equals(last_o)) { separator = ""; }
                                    myline += o.ToString() + separator;
                                }
                                csvfile.WriteLine(myline.Replace(",", "."));
                            }
                            //ecriture de nomepiece
                            myline = "";
                            last_o = gapiece.LastOrDefault();
                            separator = ";";

                            foreach (object o in nomenpiece)
                            {
                                if (o.Equals(last_o)) { separator = ""; }
                                myline += o.ToString() + separator;
                            }

                            csvfile.WriteLine(myline.Replace(",", "."));
                            last_o = null;
                            //on set le setfiled value a false

                            reference.SetFieldValue("REMONTER_DT", false);
                            reference.Save();
                            part_infos = null;

                        }
                    }

                }
                csvfile.Close();

            }


        }



    }


    #endregion


    //IMPORT STOCK
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////IMPORT///////////////OF////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// a faire
    /// </summary>
    /// 
    ////boutnon d'import du stock
    public class Import_Stock_Processor : CommandProcessor
    {
        //public IContext contextlocal = null;
        public override bool Execute()
        {

            //recuperation du context
            //string DbName = Alma_RegitryInfos.GetLastDataBase();
            //IModelsRepository modelsRepository = new ModelsRepository();
            //contextlocal = modelsRepository.GetModelContext(DbName);


            using (Clipper_Stock Stock = new Clipper_Stock())
            {
                Stock.Import(Context);//), csvImportPath);
            }


            return base.Execute();
        }
    }



    #region Import_Stock
    public class Clipper_Stock : IDisposable
    {
        //string CsvImportPath = null;
        //string error_on_field; //pour l aide mais dico a surchager
        //IMPORT_DM

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        //a mettre en override dans import tools
        /// <summary>
        /// met a jour une entité  a partit d'un dictionnaire de ligne
        /// </summary>
        /// <param name="contextlocal">context</param>
        /// <param name="item">ientity item </param>
        /// <param name="line_dictionnary">dictionnaire de ligne</param>
        public void update_Item(IContext contextlocal, IEntity item, Dictionary<string, object> line_dictionnary)
        {
            try
            {
                foreach (var field in line_dictionnary)
                {
                    item.SetFieldValue(field.Key, field.Value);
                }
            }
            catch (Exception ie) { MessageBox.Show(ie.Message); }
        }
        /// <summary>
        /// met a jour les valeurs stock et sheet  dans le stock almacam
        /// </summary>
        /// <param name="contextlocal">contexte context</param>
        /// <param name="sheet">ientity sheet  </param>
        /// <param name="stock">inentity stock</param>
        /// <param name="line_dictionnary">dictionnary linedisctionary</param>
        /// <param name="type_tole">type tole  ou chute</param>
        public void Update_Stock_Item(IContext contextlocal, ref IEntity sheet, ref IEntity stock, ref Dictionary<string, object> line_dictionnary, TypeTole type_tole)
        {
            try
            {
                foreach (var field in line_dictionnary)
                {

                    //on verifie que le champs existe bien avant de l'ecrire
                    if (contextlocal.Kernel.GetEntityType("_SHEET").FieldList.ContainsKey(field.Key) || contextlocal.Kernel.GetEntityType("_STOCK").FieldList.ContainsKey(field.Key))
                    {

                        //traitement specifique

                        switch (field.Key)
                        {
                            /*
                            if (Data_Model.ReturnObject_If_ExistsInDictionnary(field.Key,ref line_dictionnary)!=null)  {
                            item.SetFieldValue(field.Key, field.Value);
                            }*/

                            case "_WIDTH":
                                if (type_tole == TypeTole.Tole)
                                {
                                    sheet.SetFieldValue("_WIDTH", field.Value);
                                }
                                break;
                            case "_LENGTH":
                                if (type_tole == TypeTole.Tole)
                                {
                                    sheet.SetFieldValue("_LENGTH", field.Value);
                                }
                                break;

                            case "_NAME":
                                if (type_tole == TypeTole.Tole)
                                {
                                    //sheet_to_update.SetFieldValue("_REFERENCE", sheet_to_update_reference);
                                    /*
                                    string sheet_to_update_reference = item.GetFieldValueAsString("_NAME");
                                    item.SetFieldValue("_NAME", sheet_to_update_reference);
                                    */

                                }
                                else if (type_tole == TypeTole.Chute)
                                {
                                    stock.SetFieldValue(field.Key, field.Value);
                                    //stock.SetFieldValue("_SHEET", sheet_to_update);
                                }
                                //
                                break;
                            case "_MATERIAL":

                                //rien pour le moment mais on pourrait verifier si une nouvelle matiere est a declarer ou non
                                //recherche de l'epaisseur de la chaine
                                //on recupere la matiere
                                /*
                                string nuance_name = null;
                                nuance_name = field.Value.ToString().Replace('§', '*');
                                if (type_tole == TypeTole.Tole){
                                sheet.SetFieldValue(field.Key, material);
                                }*/


                                //plus tard on verifira que l'epaisseur existe dans cette nuance sinon on la créer
                                //materials = contextlocal.EntityManager.GetEntityList("_MATERIAL",LogicOperator.And,"_THICKNESS",
                                //ConditionOperator.Equal, line_Dictionnary["THICKNESS"], "_QUALITY", ConditionOperator.Equal, nuance_name);
                                //stocks.Fill(false);     
                                //on importe jamais une matiere qui n'existe pas
                                //contextlocal.EntityManager.GetEntityList("_SHEET", "_REFERENCE", ConditionOperator.Equal, reference)
                                //ON CREER LA MATIERE SI ELLE N EXISTE PAS  

                                break;

                            case "NUMMATLOT":
                                //on recuepere  le numero de matiere lotie 
                                //on le copie dans le numero de coulées
                                

                                stock.SetFieldValue(field.Key, field.Value);
                                stock.SetFieldValue("_HEAT_NUMBER",field.Value );




                                break;

                            
                            case "_QUANTITY":
                                //on recuepere  les quantités de la chute courante de la chute
                                //on verifie si les il y a des quantité en prod
                                //on requalifie les quantité

                                stock.SetFieldValue(field.Key, field.Value);




                                break;

                            case "FILENAME":
                                //pointage de l'emf si besoin
                                //IWpmImage emf = WpmImage("");
                                //emf.FileName;
                                //stock.SetFieldValue(field.Key, field.Value);
                                //rien pour le moment mais on pourrait verifier si une nouvelle matiere est a declarer ou non
                                //recherche de l'epaisseur de la chaine 
                                //on importe jamais une matiere qui n'existe pas
                                if (type_tole == TypeTole.Tole)
                                {

                                }
                                else if (type_tole == TypeTole.Chute)
                                {
                                    stock.SetFieldValue(field.Key, field.Value);
                                    //stock.SetFieldValue("_SHEET", sheet_to_update);
                                }

                                break;


                            default:


                                stock.SetFieldValue(field.Key, field.Value);


                                break;
                        }
                    }
                }


                //on sauvegarde

                sheet.Save();
                stock.Save();
            }
            catch (Exception ie) {
                MessageBox.Show(ie.Message);
            }
        }
        /// <summary>
        /// verification des données du fichier texte
        /// </summary>
        /// <param name="line_dictionnary">dictionnaire de ligne interprété par le datamodel</param>
        /// <returns>false ou tuue si integre</returns>
        public Boolean CheckDataIntegerity(IContext contextlocal, Dictionary<string, object> line_dictionnary)
        {
            //
            try
            {
                ///////////////////////////////////////////////////////////////////////////
                ///matiere exits?
                //IEntityList materials;
                Boolean result = true;
                //string nuance_name = null;
                string currenfieldsname;

                currenfieldsname = "_MATERIAL";
                if (line_dictionnary.ContainsKey(currenfieldsname)) {

                    ///////////////////////////////////////////////////////////////////////////
                    //les matiere
                    //string nuance_name = line_dictionnary["_MATERIAL"].ToString().Replace('§', '*');
                    string nuance = null;
                    string material_name = null;
                    double thickness = 0;

                    nuance = line_dictionnary[currenfieldsname].ToString().Replace('§', '*');
                    thickness = Convert.ToDouble(line_dictionnary["THICKNESS"]);
                    material_name = AF_ImportTools.Material.getMaterial_Name(contextlocal, nuance, thickness);
                    if (material_name == string.Empty) {
                        /*on log matiere non existante*/
                        Alma_Log.Error(nuance + " " + thickness + ":" + line_dictionnary["_NAME"] + "mm matiere non existante --> line ignored", MethodBase.GetCurrentMethod().Name);
                        Alma_Log.Write_Log_Important(nuance + " " + thickness + ":" + line_dictionnary["_NAME"] + "mm matiere non existante, line ignored"); result = false;
                    }
                }
                else
                { result = false; }

                ///////////////////////////////////////////////////////////////////////////
                //les quantités negatives sont interdites
                currenfieldsname = "IDCLIP";
                if (line_dictionnary.ContainsKey(currenfieldsname))
                {
                    if (line_dictionnary[currenfieldsname].ToString() == "")
                    {
                        Alma_Log.Error(line_dictionnary[currenfieldsname] + ":IDCLIP non detecté sur la ligne a importée, line ignored", MethodBase.GetCurrentMethod().Name);
                        Alma_Log.Write_Log_Important(MethodBase.GetCurrentMethod().Name + ":" + line_dictionnary["_NAME"] + ":_ID non detecté sur la ligne a importée, line ignored"); result = false;
                    }
                }
                else { result = false; }
                ///////////////////////////////////////////////////////////////////////////
                //les quantités negatives sont interdites
                currenfieldsname = "_QUANTITY";
                if (line_dictionnary.ContainsKey(currenfieldsname))
                {
                    if ((int)line_dictionnary[currenfieldsname] < 0) {
                        Alma_Log.Error(line_dictionnary["_NAME"] + ":_QUANTITY non detecté sur la ligne a importée, line ignored", MethodBase.GetCurrentMethod().Name);
                        Alma_Log.Write_Log_Important(MethodBase.GetCurrentMethod().Name + ":" + line_dictionnary["_NAME"] + ":_QUANTITY non detecté sur la ligne a importée, line ignored"); result = false;
                    }
                }
                else { result = false; }
                ///////////////////////////////////////////////////////////////////////////
                //les longeur negative ou egales a 0  sont interdites
                currenfieldsname = "_WIDTH";
                if (line_dictionnary.ContainsKey(currenfieldsname))
                {
                    if ((double)line_dictionnary[currenfieldsname] <= 0) {
                        Alma_Log.Error(line_dictionnary["_NAME"] + ": NULL _WIDTH , line ignored", MethodBase.GetCurrentMethod().Name);
                        Alma_Log.Write_Log_Important(MethodBase.GetCurrentMethod().Name + ":" + line_dictionnary["_NAME"] + ": NULL _WIDTH , line ignored"); result = false;
                    }
                }
                else { result = false; }
                ///////////////////////////////////////////////////////////////////////////
                //les quantités negatives sont interdites
                currenfieldsname="_LENGTH";
                if (line_dictionnary.ContainsKey(currenfieldsname))
                {
                    if ((double)line_dictionnary[currenfieldsname] <= 0) {
                        Alma_Log.Error(line_dictionnary["_NAME"] + ": NULL _LENGTH , line ignored", MethodBase.GetCurrentMethod().Name);
                        Alma_Log.Write_Log_Important(MethodBase.GetCurrentMethod().Name + ":" + line_dictionnary["_NAME"] + ": NULL _LENGTH , line ignored"); result = false;
                    }
                }
                else { result = false; }

                ///////////////////////////////////////////////////////////////////////////
                //les quantités negatives sont interdites
                currenfieldsname = "THICKNESS";
                if (line_dictionnary.ContainsKey(currenfieldsname))
                {
                    if ((double)line_dictionnary[currenfieldsname] <= 0) {

                        Alma_Log.Error(line_dictionnary["_NAME"] + ": NULL THICKNESS, line ignored", MethodBase.GetCurrentMethod().Name);
                        Alma_Log.Write_Log_Important(MethodBase.GetCurrentMethod().Name + ":" + line_dictionnary["_NAME"] + ": NULL OR NEGATIVE THICKNESS this ligne , line ignored"); result = false;
                    }
                }
                else { result = false; }



                return result;
            }

            catch (Exception ie)
            {
                Alma_Log.Write_Log_Important(MethodBase.GetCurrentMethod().Name + ":" + line_dictionnary["_NAME"] + ":" + ie.Message);
                return false;
                //MessageBox.Show(ie.Message);

            }



        }
        /// <summary>
        /// renvoie l'entite matiere a partir d'un dictionnaire de ligne
        /// </summary>
        /// <param name="contextlocal">ientity context</param>
        /// <param name="line_dictionnary">dictionnary <string,object> line_dictionnary</param>
        /// <returns></returns>
        public IEntity GetMaterialEntity(IContext contextlocal, string material_name, ref Dictionary<string, object> line_dictionnary)
        { IEntity material = null;

            try {

                IEntityList materials = null;

                //verification simple par nom nuance*etat epaisseur en rgardnat une structure comme ceci
                //"SPC*BRUT 1.00" //attention pas de control de l'obsolecence pour le moment


                materials = contextlocal.EntityManager.GetEntityList("_MATERIAL", "_NAME", ConditionOperator.Like, material_name);
                material = AF_ImportTools.SimplifiedMethods.GetFirtOfList(materials);
                if ((material == null) || (material.Status.ToString() == "Normal")) { material = null; }

                /*materials.Fill(false);

                    if (materials.Count() > 0  )
                    { if (materials.FirstOrDefault().Status.ToString() == "Normal") { material = materials.FirstOrDefault();}
                          }
                    else { material=null; }*/


                return material;
            }
            catch (Exception ie)
            {
                Alma_Log.Error(" Lecture impossible de la matiere ", MethodBase.GetCurrentMethod().Name);
                Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + ": Lecture impossible de la matiere " + ie.Message);
                //MessageBox.Show(ie.Message);
                return material;
            }

        }
        /// <summary>
        /// renvoie l'entite matiere a partir du nom string 
        /// </summary>
        /// <param name="contextlocal">ientity context</param>
        /// <param name="line_dictionnary">dictionnary <string,object> line_dictionnary</param>
        /// <returns>entity de type matiere</returns>
        public IEntity GetMaterialEntity(IContext contextlocal, ref Dictionary<string, object> line_dictionnary)
        {
            IEntity material = null;

            try
            {

                //IEntityList materials = null;

                //verification simple par nom nuance*etat epaisseur en rgardnat une structure comme ceci
                //"SPC*BRUT 1.00" //attention pas de control de l'obsolecence pour le moment
                if (line_dictionnary.ContainsKey("_MATERIAL") && line_dictionnary.ContainsKey("THICKNESS"))
                {
                    material = Material.getMaterial_Entity(contextlocal, line_dictionnary["_MATERIAL"].ToString(), Convert.ToDouble(line_dictionnary["THICKNESS"]));
                }/*
                materials = contextlocal.EntityManager.GetEntityList("_MATERIAL", "_NAME", ConditionOperator.Equal, material_name);
                materials.Fill(false);

                if (materials.Count() > 0 && materials.FirstOrDefault().Status.ToString() == "Normal")
                { material = materials.FirstOrDefault(); }
                else { material = null; }*/


                return material;
            }
            catch (Exception ie)
            {
                Alma_Log.Write_Log_Important(MethodBase.GetCurrentMethod().Name + ": Lecture impossible de la matiere :" + ie.Message);
                //MessageBox.Show(ie.Message);
                return material;
            }

        }
        /// <summary>
        /// import des stock clipper a partir dun fiocheir impordm
        /// </summary>
        /// <param name="contextlocal"></param>
        /// <param name="pathToFile">path to dispomat.csv file</param>
        /// <param name="DataModelString"></param>
        //public void Import(IContext contextlocal, string pathToFile, string DataModelString)
        public void Import(IContext contextlocal)//, string pathToFile)
        {   ///definiton des path
           

            //recuperation des path
            //CsvImportPath = pathToFile;
            string methodename = MethodBase.GetCurrentMethod().Name;
            try
            {  //creation du timetag d'import
                string timetag = string.Format("{0:d_M_yyyy_HH_mm_ss}", DateTime.Now);
                //creation du log
                bool testlog = Alma_Log.Create_Log(Clipper_Param.getVerbose_Log());
                long ligneNumber = 0;
                Alma_Log.Write_Log(methodename + ": time tag:  " + timetag);
                //ouverture du fichier csv lancement du curseur
                //Set cursor as hourglass
                Cursor.Current = Cursors.WaitCursor;
                //declaration du dictionnaire d'id clip contenu dans le fichier pour le mode ommission
                List<string> sheetId_list_from_txt_file= new List<string>();
                List<string> sheetId_list_from_database = new List<string>();
                //on demarre
                ///definiton des path
               // if (!Clipper_Param.GetlistParam(contextlocal)) { throw new Exception (ClipperExit.Close()); };
                Clipper_Param.GetlistParam(contextlocal);// ? true : ClipperExit.Close; //
                string CsvImportPath = Clipper_Param.GetPath("IMPORT_DM");
                Alma_Log.Write_Log("[Import du stock ]:" + CsvImportPath);
                string DataModelString = Clipper_Param.GetModelDM();
                Alma_Log.Write_Log("lecture du DataModel du stock:Success !!!");

                using (StreamReader csvfile = new StreamReader(CsvImportPath, Encoding.Default, true))
                {
                    //construction du dictionnaire de champs
                    Dictionary<string, object> line_Dictionnary = new Dictionary<string, object>();
                    Data_Model.setFieldDictionnary(DataModelString);
                    Alma_Log.Write_Log(methodename + ": DataModel String success !!   ");

                    //lecture à la ligne
                    string line;
                    //on import pas les quantitée nulles
                    while (((line = csvfile.ReadLine()) != null))
                    {
                        ////
                        //
                        TypeTole type_tole;
                        //declaration des entités
                        //stock lists
                        IEntityList stocks = null;
                        IEntityList sheets_to_update = null;
                        //IEntityList materials = null;
                        //entity
                        IEntity stock = null;
                        IEntity sheet_to_update = null;
                        IEntity material = null;
                        //pour detection de nouveau format

                        //id courante du  format
                        Int32 CurrentSheetId = -1;
                        Int32 CurrentStockId = -1;
                        /////////////////////////////////////////////////////////////////
                        //integrite des données
                        //lignes vides on passe
                        //pour debuggage
                        ligneNumber++;
                        Alma_Log.Write_Log(methodename + ": reading line " + ligneNumber);
                        if (line.Trim() == "")
                        {
                            Alma_Log.Write_Log_Important(methodename + ": empty line detected  :  " + ligneNumber);
                            continue;
                        }
                        
                        line_Dictionnary = Data_Model.ReadCsvLine_With_Dictionnary(line);
                        Alma_Log.Write_Log(methodename + ": line " + ligneNumber + ":line_dictionnary success !!    ");
                        //on verifie les données d'entrées (matiere existe, longeur largeur !=0, quantité decimales)
                        if (CheckDataIntegerity(contextlocal, line_Dictionnary) == false)
                        {
                            //Alma_Log.Write_Log_Important(System.Reflection.MethodBase.GetCurrentMethod().Name);
                            Alma_Log.Write_Log_Important(methodename + ":-----> line " + ligneNumber + ":" + line_Dictionnary["_NAME"] + ":integrity tests fails, line ignored");
                            continue;
                        }
                        //constrution du nom par defaut du sheet (format)
                        //
                        string sheet_to_update_reference = string.Format("{0}*{1}*{2}*{3}",
                        //line_Dictionnary["_MATERIAL"].ToString(),
                        line_Dictionnary["_MATERIAL"].ToString().Replace('§', '*'),
                        line_Dictionnary["_LENGTH"].ToString(),
                        line_Dictionnary["_WIDTH"].ToString(),
                        line_Dictionnary["THICKNESS"].ToString());
                        ///1 pour tole neuve 2 pour chute selon la chaine filename : par defaut null
                        // type --> chute si emf present
                        type_tole = TypeTole.Tole;
                        if (line_Dictionnary.ContainsKey("FILENAME")) { type_tole = TypeTole.Chute; /*string pathtoemf = line_Dictionnary["FILENAME"];*/ }
                        //recuperation des infos de matiere

                        string nuance_name = null;
                        nuance_name = line_Dictionnary["_MATERIAL"].ToString().Replace('§', '*');
                        string material_name = string.Format("{0} {1:0.00} mm", nuance_name, line_Dictionnary["THICKNESS"]);
                        //normaleement il n'ya pas besoin de condition car l'integrite et deja verifier dans checkdataintegrity
                        //material = GetMaterialEntity(contextlocal, material_name, ref line_Dictionnary);
                        material = GetMaterialEntity(contextlocal, ref line_Dictionnary);
                        Alma_Log.Write_Log(methodename + ": material success !!    ");
                        
                        //implementation de la liste sheetId_list_from_txt_file pour l'ommission
                        sheetId_list_from_txt_file.Add(line_Dictionnary["IDCLIP"].ToString());

                        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        ///tole existante--> on ne fait que de la mise à jour
                        //on travail au maximum sur le stock
                        //l'existants --> l'id clip est unique, il permet donc de retrouver les elements de stock a mettre à jous
                        stocks = contextlocal.EntityManager.GetEntityList("_STOCK", "IDCLIP", ConditionOperator.Equal, line_Dictionnary["IDCLIP"]);
                        stocks.Fill(false);
                        if (stocks.Count > 0)
                        { if (stocks.FirstOrDefault().Status.ToString() == "Normal") {
                                //cas du doublon
                                if (stocks.Count > 1)
                                {
                                    //on log et on ne fait rien
                                    Alma_Log.Error("--> ligne " + ligneNumber + ":" + line_Dictionnary["_NAME"] + " doublons detecté sur entité" + line_Dictionnary["IDCLIP"].ToString(), methodename);
                                    Alma_Log.Write_Log_Important(methodename + ": ligne " + ligneNumber + ":" + line_Dictionnary["_NAME"] + " doublons detecté sur entité" + line_Dictionnary["IDCLIP"].ToString());
                                    continue;
                                }
                                else
                                {
                                   
                                    //on met a jour et continue
                                    stock = stocks.FirstOrDefault();
                                    sheet_to_update = stock.GetFieldValueAsEntity("_SHEET");
                                    Update_Stock_Item(contextlocal, ref sheet_to_update, ref stock, ref line_Dictionnary, type_tole);
                                    Alma_Log.Write_Log(methodename + ": ligne " + ligneNumber + ":" + line_Dictionnary["_NAME"] + " mise à jour de " + line_Dictionnary["IDCLIP"].ToString());
                                    sheet_to_update = null;
                                    stock = null;
                                    continue;
                                }
                            }
                        }



                        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        ///les toles neuves
                        if (type_tole == TypeTole.Tole)
                        {

                            //recherche de format ayant la  meme reference
                            //sheets_to_update = contextlocal.EntityManager.GetEntityList("_SHEET",LogicOperator.And, "_REFERENCE", ConditionOperator.Equal, sheet_to_update_reference);("_STOCK", LogicOperator.And, "IDCLIP", ConditionOperator.Equal, line_Dictionnary["IDCLIP"], "_SHEET", ConditionOperator.GreaterOrEqual, 0);
                            sheets_to_update = contextlocal.EntityManager.GetEntityList("_SHEET", "_REFERENCE", ConditionOperator.Equal, sheet_to_update_reference);
                            sheets_to_update.Fill(false);

                            if (sheets_to_update.Count > 0)
                            {
                                if (sheets_to_update.FirstOrDefault().Status.ToString() == "Normal") {
                                    //la reference existe on capture la premiere 
                                    sheet_to_update = sheets_to_update.FirstOrDefault();
                                    CurrentSheetId = sheet_to_update.Id32;
                                    Alma_Log.Write_Log(methodename + ":" + line_Dictionnary["_NAME"] + ": capture du sheet id success ");
                                }
                            }
                            else
                            {
                                //on creer un nouveau nom
                                // on la creer un nouveau format aved la nouvelle reference
                                Alma_Log.Write_Log(methodename + ": creation d'un nouveau sheet ");
                                sheet_to_update = contextlocal.EntityManager.CreateEntity("_SHEET");
                                sheet_to_update.SetFieldValue("_TYPE", (int)type_tole);
                                sheet_to_update.SetFieldValue("_REFERENCE", sheet_to_update_reference);
                                sheet_to_update.SetFieldValue("_NAME", sheet_to_update_reference);
                                sheet_to_update.SetFieldValue("_MATERIAL", material.Id32);
                                //sheets_to_update.complete=true;
                                sheet_to_update.Complete = true;



                                //sheet_to_update.SetFieldValue("_COMPLETE", true);
                                sheet_to_update.Save();

                                CurrentSheetId = sheet_to_update.GetFieldValueAsInt("ID");

                                Alma_Log.Write_Log(methodename + ":" + line_Dictionnary["_NAME"] + ": nouveau sheet créé id: " + CurrentSheetId.ToString());



                            }




                        }
                        ///chute on travail le plus possible sur le stock
                        ///on verifie  l'id exite deja, dans ce cas on ne travail que sur le stock
                        ///
                        else if (type_tole == TypeTole.Chute)
                        {
                            // si l id n'existe pas, on verifie que la geometrie a ete générée et on recupere le sheet associé
                            //on recupere le sheet
                            // nota dans le cas d'une chute, il est impossible d'avoir 2 geometries du meme nom donc
                            //on se base sur le nom de la geometrie pour retrouver la chute creer par alma
                            //on doit  pouvoir mettre a jour que les toles validée sur les prochaines version
                            sheets_to_update = contextlocal.EntityManager.GetEntityList("_SHEET", "FILENAME", ConditionOperator.Equal, line_Dictionnary["FILENAME"]);
                            sheets_to_update.Fill(false);
                            Alma_Log.Write_Log(methodename + " requete sur les emf success for " + line_Dictionnary["FILENAME"]);
                            if (sheets_to_update.Count == 1)
                            {
                                Alma_Log.Write_Log(methodename + " sheet trouvée ");
                                if (sheets_to_update.FirstOrDefault().Status.ToString() == "Normal") {
                                    //le sheet existe on capture la premiere 
                                    sheet_to_update = sheets_to_update.FirstOrDefault();/**/
                                    CurrentSheetId = sheet_to_update.Id32;
                                    //recupe de l'element de stock
                                    stocks = contextlocal.EntityManager.GetEntityList("_STOCK", "_SHEET", ConditionOperator.Equal, CurrentSheetId);
                                    stocks.Fill(false);
                                    Alma_Log.Write_Log(methodename + " requete du  sheet associé au stock " + CurrentSheetId.ToString());
                                    if (stocks.Count > 0) {
                                        if (sheets_to_update.FirstOrDefault().Status.ToString() == "Normal")
                                        {
                                            stock = stocks.FirstOrDefault();
                                            CurrentStockId = stock.Id32;
                                            Alma_Log.Write_Log(methodename + " found " + stock.Id32.ToString());
                                        }
                                    }
                                    else
                                    {//la tole nexiste pas encore dans le stock//on log on continue
                                        Alma_Log.Write_Log(methodename + ":" + line_Dictionnary["_NAME"] + " la chute  " + line_Dictionnary["FILENAME"] + "n existe plus");
                                        continue;
                                    }
                                }


                            }

                            else
                            {   //sheets_to_update.Count=0 ou  2,3... il y plus de 1 sheet ayant la meme filename -->ou bien il n'y a pas de sheet
                                //on log et on arrete tout car on ne creer jamais un format pour une chute dont la geometrie n'existe pas
                                Alma_Log.Error("--> ligne " + ligneNumber + ":" + " la chute  " + line_Dictionnary["FILENAME"] + " n existe pas, la ligne sera ignorée.", methodename);
                                Alma_Log.Write_Log_Important(methodename + ":" + line_Dictionnary["_NAME"] + " la chute  " + line_Dictionnary["FILENAME"] + " n existe pas, la ligne sera ignorée.");
                                continue;
                            }

                        }

                        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        //on recupere le stock et on fait les modif
                        //on creer le stock
                        //on creer un nouveau stock sur la reference
                        if (type_tole == TypeTole.Tole)
                        {
                            stock = contextlocal.EntityManager.CreateEntity("_STOCK");
                            //on creer le timetag
                            stock.SetFieldValue("STOCK_IMPORT_NUMBER", timetag.Replace("_", ""));
                            stock.SetFieldValue("_NAME", line_Dictionnary["_NAME"]);
                            stock.SetFieldValue("_SHEET", CurrentSheetId);
                            //on creer le timetag
                            stock.SetFieldValue("STOCK_IMPORT_NUMBER", timetag.Replace("_", ""));
                            stock.Save();
                            Alma_Log.Write_Log(methodename + " update stock sucess  ");
                        }
                        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        //on set                          
                        //on set les valeurs
                        Update_Stock_Item(contextlocal, ref sheet_to_update, ref stock, ref line_Dictionnary, type_tole);

                        //sheet_to_update.Validate();
                        sheet_to_update.Save();
                        Alma_Log.Error("--> line " + ligneNumber + " treated...", methodename);
                        Alma_Log.Write_Log(methodename + ": line " + ligneNumber + " update item success  ");



                    }
                }

                //purge
                using (StreamReader csvfile = new StreamReader(CsvImportPath, Encoding.Default, true))
                {
                    //construction de la liste 1


                    //construiction de la liste 2
                    //stocks = contextlocal.EntityManager.GetEntityList("_STOCK", "IDCLIP", ConditionOperator.Equal, line_Dictionnary["IDCLIP"]);
                    //stocks.Fill(false);
                    //fuisun /
                    //suppression

                }

                    // Set cursor as default arrow
                    Cursor.Current = Cursors.Default;
                //rename the File once imported
                //ImportTools.File_Tools.Rename_Csv(CsvImportPath);
                File_Tools.Rename_Csv(CsvImportPath, timetag);
                Alma_Log.Write_Log(methodename + " fichier   " + CsvImportPath + " renommé");
                Alma_Log.Final_Open_Log(ligneNumber);
            }
            catch (Exception e)
            {
                Alma_Log.Write_Log(e.Message);
                Alma_Log.Final_Open_Log();
                //MessageBox.Show(e.Message);
            }

        }
    }
    #endregion







    //do on action
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////IMPORT///////////////OF////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region doonaction_retour_gp
    /// <summary>
    /// retour gp à la cloture
    /// </summary>
    //do on action
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////IMPORT///////////////OF////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region doonaction_retour_gp
    /// <summary>
    /// retour gp la cloture
    /// </summary>
    ///
    
    public class Clipper_DoOnAction_After_Cutting_end : AfterEndCuttingEvent
    {
        //Clipper_DoOnAction_After_Cutting_end
        public override void OnAfterEndCuttingEvent(IContext context, AfterEndCuttingArgs args)
        {  
            {
                //this.execute(contextlocal, args.NestingEntity);
                execute(args.ToCutSheetEntity);
            }
        }



        /// <summary>
        /// creation auto du fichier texte à  la cloture
        /// </summary>
        /// <param name="args"></param>
        public void execute(IEntity entity)
        {
            //recuperation des path
            Clipper_Param.GetlistParam(entity.Context);
            string export_gpao_path = Clipper_Param.GetPath("Export_GPAO");

            //using (Clipper_Infos current_clipper_nestinfos = new Clipper_Infos())
            {
                Clipper_Infos current_clipper_nestinfos = new Clipper_Infos();
                current_clipper_nestinfos.GetNestInfosBySheet(entity);
                current_clipper_nestinfos.Export_NestInfosToFile(export_gpao_path);
                //validation du stock

                //
                current_clipper_nestinfos = null;
            }


        }
        /// <summary>
        /// retourne le fichier clipper a partie de la boite de dialogue de la ploateforme
        /// 
        /// </summary>
        /// <param name="SelectedEntities"> IEntitySelector Entityselector </param>
        /// 
        public void execute(IEnumerable<IEntity> SelectedEntities)
            {

            try {  
                    
                    if (SelectedEntities.Count() > 0)
                    {
                        string stage = SelectedEntities.First().EntityType.Key;
                            //creation du fichier de sortie
                        Clipper_Param.GetlistParam(SelectedEntities.First().Context);
                            foreach (IEntity entity in SelectedEntities)
                            {
                                string export_gpao_path = Clipper_Param.GetPath("Export_GPAO");

                                //using (Clipper_Infos current_clipper_nestinfos = new Clipper_Infos())
                                {
                                    Clipper_Infos current_clipper_nestinfos = new Clipper_Infos();
                                    current_clipper_nestinfos.GetNestInfosBySheet(entity);
                                    current_clipper_nestinfos.Export_NestInfosToFile(export_gpao_path);
                                    //validation du stock

                                    //
                                    current_clipper_nestinfos = null;
                                }


                            }

                }
                else { MessageBox.Show("Aucune selection n'a été faite dans la liste proposée \r\n veuillez relancer l'opération."); }
            } 
            catch { }


        }

           

        }


    ////
    //
    /// <summary>
    /// retour gp à L ENVOIE A L ATELIER
    /// </summary>
    /// 

    public class Clipper_DoOnAction_AfterSendToWorkshop : AfterSendToWorkshopEvent
    {

        //cette fonction est lancée autant de fois qu'il y a de selection
        //la multiselection n'est pas controlée
        public override void OnAfterSendToWorkshopEvent(IContext contextlocal, AfterSendToWorkshopArgs args)
        {
            try {

                //this.execute(contextlocal, args.NestingEntity);
                execute(args.NestingEntity);
            }


            catch (Exception ie ){
                MessageBoxEx.ShowError(ie.Message);
            }
        }



        /// <summary>
        /// creation auto du fichier texte à  la cloture
        /// </summary>
        /// <param name="args"></param>
        public void execute(IEntity entity)
        {
            //recuperation des path
            Clipper_Param.GetlistParam(entity.Context);
            string export_gpao_path = Clipper_Param.GetPath("Export_GPAO");

          
            {
                Clipper_Infos current_clipper_nestinfos = new Clipper_Infos();
                //current_clipper_nestinfos.GetNestInfosBySheet(entity);

                current_clipper_nestinfos.GetNestInfosByNesting(entity.Context, entity, "_TO_CUT_NESTING");
                current_clipper_nestinfos.Export_NestInfosToFile(export_gpao_path);
                //validation du stock

                
                current_clipper_nestinfos = null;
            }


        }





    }


    /// <summary>
    /// retour gp generique
    /// on suppose que les toles sont deja associées
    /// </summary>
    /// 
            /*
            public class Clipper_DoOnAction : IDisposable
            {
                public void Dispose()
                {
                    ///purge
                    GC.SuppressFinalize(this);
                }


                /// <summary>
                /// retourne le fichier de sortie clip a partir des arguments de l'evenement vefroe send to workshop
                /// </summary>
                /// <param name="contextlocal"></param>
                /// <param name="nesting">nom du placement</param>
                public void Export(IContext contextlocal, IEntity nesting)
                {

                    //string stage = "_TO_CUT_NESTING";
                    //string sheet_type= "_TO_CUT_SHEET";
                    //creation du fichier de sortie

                    //recupere les path
                    Clipper_Param.GetlistParam(contextlocal);

                    string export_gpao_path = Clipper_Param.GetPath("Export_GPAO") + "\\" + nesting.DefaultValue.ToString() + ".txt";

                    //ecriture du fichier d'echange
                    using (StreamWriter export_gpao_file = new StreamWriter(@export_gpao_path))
                    {


                    }




                }

                /// <summary>
                /// retourne le fichier clipper a partie de la boite de dialogue de la ploateforme
                /// </summary>
                /// <param name="contextlocal"></param>
                public void Export(IContext contextlocal)
                {

                    string stage = "_TO_CUT_NESTING";
                    //creation du fichier de sortie
                    //recupere les path
                    Clipper_Param.GetlistParam(contextlocal);
                    IEntitySelector nestingselector = null;

                    nestingselector = new EntitySelector();

                    //entity type pointe sur la list d'objet du model
                    nestingselector.Init(contextlocal, contextlocal.Kernel.GetEntityType("_TO_CUT_NESTING"));
                    nestingselector.MultiSelect = true;


                    if (nestingselector.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        foreach (IEntity nesting in nestingselector.SelectedEntity)
                        {


                            string export_gpao_path = Clipper_Param.GetPath("Export_GPAO") + "\\" + nesting.DefaultValue.ToString() + ".txt";

                            //ecriture du fichier d'echange
                            using (StreamWriter export_gpao_file = new StreamWriter(@export_gpao_path))
                            {
                                //Lecture
                                using (Clipper_NestInfos current_clipper_nestinfos = new Clipper_NestInfos())
                                {  //on set le context
                                    current_clipper_nestinfos.Export_NestInfosToFile(ref contextlocal, stage, nesting.DefaultValue, export_gpao_file);

                                }

                            }

                        }


                    }
                    else { return; }


                }


            }


            */



            /// <summary>
            /// retour gp à L ENVOIE A L ATELIER
            /// </summary>
            /// 


            /*
                public class Clipper_DoOnAction_AfterSendToWorkshop : AfterSendToWorkshopEvent
                {
                    public override void OnAfterSendToWorkshopEvent(IContext contextlocal, AfterSendToWorkshopArgs args)
                    {

                        //this.execute(contextlocal, args.NestingEntity);

                    }


                }
                */

            /// <summary>
            /// retour gp AVANT L ENVOIE A L ATELIER
            /// </summary>
            /// MyBeforeSendToWorkshopEvent : BeforeSendToWorkshopEvent
            /// 
            /// 
            /// public class Clipper_DoOnAction_BeforeSendToWorkshop : BeforeSendToWorkshopEvent
            /// 

            /*
            {

                public override void OnBeforeSendToWorkshopEvent(IContext context, BeforeSendToWorkshopArgs args)
                {



                }

            }
            */



            #endregion



            #region export clipper gpao : retour tole

            //creation des clipper infos issue des generic gp infos
        public class Clipper_Infos : Generic_GP_Infos
    {
        public override void Export_NestInfosToFile(string export_gpao_path)
        {
            base.Export_NestInfosToFile(export_gpao_path);
            /***/

            ///recuperation des placements selectionnés

                        foreach (Nest_Infos_2 currentnestinfos in this.nestinfoslist)
                        {
                            using (StreamWriter export_gpao_file = new StreamWriter(@export_gpao_path + "\\" + currentnestinfos.Tole_Nesting.To_Cut_Sheet_Name + ".txt"))
                            {


                                string Separator = ";";
                                //recuperaiton des champs specifiques
                                string NUMMATLOT="";
                                currentnestinfos.Tole_Nesting.Specific_Tole_Fields.Get<string>("NUMMATLOT", out NUMMATLOT);
                                if(NUMMATLOT == "Undef"){ 
                                NUMMATLOT = string.Empty;
                                }
                    //ecriture des entetes de nesting
                    string Header_Line = "HEADER" + Separator +
                                //currentnestinfos.Tole_Nesting.To_Cut_Sheet_Name + Separator +
                                //ecriture des entetes de nesting
                                // currentnestinfos.Tole_Nesting.Sheet_Reference + Separator +
                                currentnestinfos.Tole_Nesting.Stock_Name + Separator +
                                //longeur
                                currentnestinfos.Tole_Nesting.Sheet_Length + Separator +
                                //largeur
                                currentnestinfos.Tole_Nesting.Sheet_Width + Separator +
                                //epaisseur
                                currentnestinfos.Tole_Nesting.Thickness + Separator +
                                //nuance
                                currentnestinfos.Tole_Nesting.GradeName + Separator +
                                //multiplicité
                                currentnestinfos.Tole_Nesting.Mutliplicity + Separator +
                                 // temps de chargement
                                 String.Format(Clipper_Param.get_string_format_double(), (currentnestinfos.NestingSheet_loadingTimeInit/60))+ Separator +
                                currentnestinfos.Nesting_CentreFrais_Machine + Separator +
                                "" + Separator + //on ignore le pdf
                                currentnestinfos.Tole_Nesting.Sheet_EmfFile + Separator +
                               //numero lot
                               NUMMATLOT + Separator +
                                // String.Format(Clipper_Param.get_string_format_double(), (Alma_Time.minutes(this.Sheet_loadingTimeInit + this.Sheet_loadingTimeEnd)));
                               (currentnestinfos.NestingSheet_loadingTimeInit + currentnestinfos.NestingSheet_loadingTimeEnd) / 60+
                                  "";
                               export_gpao_file.WriteLine(Header_Line.Replace(",", "."));
                            

                    //ecriture des details pieces
                    //recue
                    foreach (Nested_PartInfo clipperpart in currentnestinfos.Nested_Part_Infos_List)
                    {
                        string idlnrout; 
                        clipperpart.Nested_PartInfo_specificFields.Get<string>("IDLNROUT", out idlnrout);
                        string idlnbom;
                        clipperpart.Nested_PartInfo_specificFields.Get<string>("IDLNBOM", out idlnbom);
                        string CuttingTime = (currentnestinfos.Calculus_Parts_Total_Time==0) == false? String.Format(Clipper_Param.get_string_format_double(), clipperpart.Part_Time * clipperpart.Nested_Quantity / currentnestinfos.Calculus_Parts_Total_Time) : "0";

                        string detail_Line =
                        "DETAIL" + Separator +
                         idlnrout + Separator +
                         idlnbom + Separator +
                         clipperpart.Nested_Quantity + Separator +
                         clipperpart.Height + Separator +
                         clipperpart.Width + Separator +
                         String.Format(Clipper_Param.get_string_format_double(), clipperpart.Part_Balanced_Weight * clipperpart.Nested_Quantity / (currentnestinfos.Tole_Nesting.Sheet_Weight * currentnestinfos.Tole_Nesting.Mutliplicity)) + Separator +// * Tole_Nesting.Mutliplicity)) + Separator +
                        // clipperpart.Width + Separator;//+
                        //String.Format(Clipper_Param.get_string_format_double(), clipperpart.Part_Balanced_Weight * clipperpart.Nested_Quantity / (currentnestinfos.Tole_Nesting.Sheet_Weight * currentnestinfos.Tole_Nesting.Mutliplicity)) + Separator +
                        String.Format(Clipper_Param.get_string_format_double(), clipperpart.Weight * 0.001) + Separator +
                        ///String.Format(Clipper_Param.get_string_format_double(), clipperpart.Part_Time * clipperpart.Nested_Quantity / currentnestinfos.Calculus_Parts_Total_Time);//current_clipper_nestinfos.Nesting_TotalTime);
                        CuttingTime;
                        export_gpao_file.WriteLine(detail_Line.Replace(",", "."));

                    }


                    //ecriture  des chutes
                    foreach (Tole currentoffcut in currentnestinfos.Offcut_infos_List)
                    {
                        string IsRectagular = (currentoffcut.Sheet_Length == currentoffcut.Sheet_Width) == true ? "1" : "0";
                        double longueur, largeur;
                        //si la tole est tournée, on inverse longueur et largeur
                        longueur = currentoffcut.Sheet_Length;
                        largeur = currentoffcut.Sheet_Width;

                        if (currentoffcut.Sheet_Is_rotated) {
                            longueur = currentoffcut.Sheet_Width;
                            largeur = currentoffcut.Sheet_Length;
                        }

                            string offcut_Line =
                        "CHUTE" + Separator +

                        longueur + Separator +
                        largeur + Separator +
                        currentoffcut.Mutliplicity + Separator +
                        //calcul du ratio
                        String.Format(Clipper_Param.get_string_format_double(), (currentoffcut.Sheet_Surface / currentnestinfos.Tole_Nesting.Sheet_Total_Surface)) + Separator +
                         // currentoffcut.rectangular + Separator +
                         IsRectagular + Separator +
                        //clipperoffcut.Rectangular = true ? "1" : "0" + Separator +
                        /*chemin vers dpr  */
                        //Separator +--> chemin vers dpr (n'a pas d'utilité dans clip)
                        "" + Separator +
                        currentoffcut.Sheet_EmfFile + Separator +
                        String.Format(Clipper_Param.get_string_format_double(), currentoffcut.Sheet_Weight * 0.001);
                        export_gpao_file.WriteLine(offcut_Line.Replace(",", "."));

                        //validaton du stock sur les chutes
                        //ecriture du SHEET_FILENAME
                        IEntityList Sheets_To_Update;
                        IEntity Sheet_To_Update;
                        Sheets_To_Update = currentoffcut.SheetEntity.Context.EntityManager.GetEntityList("_SHEET", "ID", ConditionOperator.Equal, currentoffcut.Sheet_Id);///NestingStockEntity.Id);
                        Sheets_To_Update.Fill(false);
                        //construction de la liste des chutes
                        Sheet_To_Update = SimplifiedMethods.GetFirtOfList(Sheets_To_Update);
                        Sheet_To_Update.SetFieldValue("FILENAME", currentoffcut.Sheet_EmfFile);
                        Sheet_To_Update.Save();

                      


                    }
                    
                      


                }






            }
           


        }

        #endregion

        //public string Numlot { get; set; }
        //public string NumMatlot { get; set; }


        /// <summary>
        /// inforlmation specifique a recuperer
        /// </summary>
        /// <param name="Tole"></param>
        /// 

        //infos spec des toles
        public override void SetSpecific_Tole_Infos(Tole Tole)
        {
            base.SetSpecific_Tole_Infos(Tole);
            string numatlot = Tole.StockEntity.GetFieldValueAsString("NUMMATLOT");
            string numlot = Tole.StockEntity.GetFieldValueAsString("NUMLOT");

            Tole.Specific_Tole_Fields.Add<string>("NUMMATLOT", Tole.StockEntity.GetFieldValueAsString("NUMMATLOT"));
            Tole.Specific_Tole_Fields.Add<string>("NUMLOT", Tole.StockEntity.GetFieldValueAsString("NUMLOT"));
            
        }
        

        //inofs specifiques des parts
        public override void SetSpecific_Part_Infos(List<Nested_PartInfo> Nested_Part_Infos_List)
        {
            base.SetSpecific_Part_Infos(Nested_Part_Infos_List);

            foreach(Nested_PartInfo part in Nested_Part_Infos_List)
            {
                part.Nested_PartInfo_specificFields.Add<string>("AFFAIRE", part.Part_To_Produce_IEntity.GetFieldValueAsString("AFFAIRE"));
                part.Nested_PartInfo_specificFields.Add<string>("FAMILY", part.Part_To_Produce_IEntity.GetFieldValueAsString("FAMILY"));
                part.Nested_PartInfo_specificFields.Add<string>("IDLNROUT", part.Part_To_Produce_IEntity.GetFieldValueAsString("IDLNROUT"));
                part.Nested_PartInfo_specificFields.Add<string>("IDLNBOM", part.Part_To_Produce_IEntity.GetFieldValueAsString("IDLNBOM"));
                //on recherche les pieces fantomes
                if (part.Part_To_Produce_IEntity.GetFieldValueAsString("IDLNROUT") == string.Empty) {
                    part.Part_IsGpao = false;
                }
                
                //part.Nested_PartInfo_specificFields.Add<string>("IDLNROUT", part.Part_To_Produce_IEntity.GetFieldValueAsString("IDLNROUT"));

                //clipperpart.Nested_PartInfo_specificFields.Get<string>("IDLBOM", out idlnbom);


            }




        }

        //infos specifique des chutes
        public override void SetSpecific_Offcut_Infos(List<Tole> Offcut_infos_List)
        {
            base.SetSpecific_Offcut_Infos(Offcut_infos_List);


            foreach (Tole offcut in Offcut_infos_List)
            {
                if (offcut.no_Stock==false) { 
                offcut.Specific_Tole_Fields.Add<string>("NUMLOT", offcut.StockEntity.GetFieldValueAsString("NUMLOT"));
                offcut.Specific_Tole_Fields.Add<string>("NUMMATLOT", offcut.StockEntity.GetFieldValueAsString("NUMMATLOT"));
                }
                else
                {  ///chmaps vide pour les valuers non existantes
                    offcut.Specific_Tole_Fields.Add<string>("NUMLOT","");
                    offcut.Specific_Tole_Fields.Add<string>("NUMMATLOT", "");


                }

            }




        }


    }

     


    #region SHEET_REQUIREMENT

    public class Clipper_Sheet_Requirement : IDisposable
    {



        public void Export(IContext contextlocal)
        {


            //verification standards
            //creation du timetag d'import
            string timetag = string.Format("{0:d_M_yyyy_HH_mm_ss}", DateTime.Now);
            Alma_Log.Create_Log(Clipper_Param.getVerbose_Log());
            //bool import_sans_donnee_technique = false;
            Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + ": importe tag :" + timetag);
            //ouverture du fichier csv lancement du curseur
            // Set cursor as hourglass
            Cursor.Current = Cursors.WaitCursor;


            //chargement de la liste de piece a retourner
            Clipper_Param.GetlistParam(contextlocal);
            string CsvExportPath = Clipper_Param.GetPath("SHEET_REQUIREMENT");

            try {



                string fileName = "Sheet_requirements.txt";
                string sourcePath = @"C:\temp\";
                Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + ": importe tag :" + timetag);

                // Use Path class to manipulate file and directory paths.
                string sourceFile = System.IO.Path.Combine(sourcePath, fileName);
                string destFile = System.IO.Path.Combine(CsvExportPath, fileName);

                // To copy a folder's contents to a new location:
                // Create a new target folder, if necessary.
                if (!System.IO.Directory.Exists(CsvExportPath))
                {
                    //System.IO.Directory.CreateDirectory(CsvExportPath);
                    Alma_Log.Write_Log(MethodBase.GetCurrentMethod().Name + "--> Dossier d' Import non detecté :" + CsvExportPath);
                }
                //si pas de fichier de reference alors on en creer un

                if (!System.IO.File.Exists(sourceFile))
                {
                    StreamWriter sr = new StreamWriter(sourceFile);
                    string myline = "NS;TX;0;0;EMF1;38;1;SHEET_TYPE;OFFCUT;;44;7;12;";
                    sr.WriteLine(myline.Replace(",", "."));
                    sr.Close();
                    sr.Dispose();
                }
               

                // To copy a file to another location and 
                // overwrite the destination file if it already exists.
                System.IO.File.Copy(sourceFile, destFile, true);
                Cursor.Current = Cursors.Default;
                Alma_Log.Final_Open_Log();

            }
            catch (Exception ie) {

                MessageBox.Show(ie.Message);
                Cursor.Current = Cursors.Default;
                Alma_Log.Write_Log(ie.Message);
                Alma_Log.Final_Open_Log();
                //File_tools
            }    

        }
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

}

    #endregion

}





///recuperation des pipes pour communication interapplication
///https://msdn.microsoft.com/en-us/library/bb546102.aspx
///eviter les socket pour des raisons de securité
#endregion