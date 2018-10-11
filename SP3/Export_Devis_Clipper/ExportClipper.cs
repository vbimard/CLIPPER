using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.IO;
using System.Globalization;
using Wpm.Schema.Kernel;
using Wpm.Implement.Manager;
using Alma.NetKernel.TranslationManager;
using Actcut.QuoteModelManager;
using System.Windows.Forms;
using Actcut.CommonModel;
using static Alma.NetWrappers2D.Topo2d;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using log4net;

//namespace Actcut.QuoteModelManager

namespace AF_Export_Devis_Clipper
{


    #region debugage
    /// <summary>
    /// validation des devis.
    /// export des fichier dpr
    /// </summary>
    public static class ExportQuote
    {   //string globaux
        static public string dpr_directory;
        static public string dpr_exported;

        /// <summary>
        /// creer les dpr du devis associ�s et copie eventuellement les dpr dans un autre dossier de destination 
        /// 
        /// </summary>
        /// <param name="iquote">iquotye a transferer</param>
        /// <param name="CustomDestinationPath">Laisser vide si pas de necessit� ; chemin de copie vers un autre dossier (besoin oxytemps pour les fichier dpr par exemple) </param>
        /// <returns></returns>
        public static Dictionary<string, string> ExportDprFiles(IQuote iquote,string CustomDestinationPath)
        {
            Dictionary<string, string> filelist= new Dictionary<string, string>();

            try {

               
                IEntity quote;
                //recupe de l'entit� quote
                quote = iquote.QuoteEntity;
                long id_quote = quote.Id;


                //creation 
                string dpr_directory = quote.Context.ParameterSetManager.GetParameterValue("_EXPORT", "_ACTCUT_DPR_DIRECTORY").GetValueAsString();
                //export des dpr
                string dprExportDirect = dpr_directory + "\\" + "Quote_" + quote.Id.ToString();
                //emfFile vide
                AF_ImportTools.File_Tools.CreateDirectory(dprExportDirect);

                bool dpr_exported = Actcut.QuoteModelManager.ExportDpr.ExportQuoteDpr(quote.Context, quote);
                dpr_directory = quote.Context.ParameterSetManager.GetParameterValue("_EXPORT", "_ACTCUT_DPR_DIRECTORY").GetValueAsString();
                foreach (IEntity partEntity in iquote.QuotePartList)
                {
                        if (dpr_exported)
                            {
                                string partname = partEntity.GetFieldValueAsString("_REFERENCE");
                                string pathtofile = dpr_directory + "\\" +"Quote_"+ quote.Id+ "\\" + partname + ".dpr.emf";
                                if (File.Exists(pathtofile) && !filelist.ContainsKey(partname))
                                {

                                    if (string.IsNullOrEmpty (CustomDestinationPath)) { filelist.Add(partname, pathtofile); }
                                    else {
                                        if (Directory.Exists(CustomDestinationPath)& File.Exists(pathtofile)) { 
                                        File.Copy(pathtofile,  CustomDestinationPath + "\\" + Path.GetFileName(pathtofile));
                                        }
                                }

                                    
                                        
                                }
                               

                            }
                }


                return filelist;
            }


            catch (DirectoryNotFoundException dirEx)
            {
                // directory not found --> on quit
                System.Windows.Forms.MessageBox.Show(dirEx.Message);
                Environment.Exit(0);
                return null;
            }

            catch (Exception ie) { System.Windows.Forms.MessageBox.Show(ie.Message); return null; }
            ///attention quote sdtalone est obligatoire pour exporter les dpr 


        }



        /// <summary>
        /// recupere le dossier de creation des dpr
        /// </summary>
        /// <param name="quote"></param>
        /// <returns>chemin d'extraction des dpr</returns>
        public static string GetDprDirectory(IEntity quote)
        {

            try { return quote.Context.ParameterSetManager.GetParameterValue("_EXPORT", "_ACTCUT_DPR_DIRECTORY").GetValueAsString(); }
            catch (Exception ie) { MessageBox.Show(ie.Message); return null; }


        }



        /// <summary>
        /// export le devis texte de clipper et les dep
        /// </summary>
        /// <param name="contextelocal"></param>
        /// <param name="iquote"></param>
        /// <returns></returns>
        public static bool ExportQuoteRequest(IContext contextelocal, IQuote iquote)
        {
            
                //ExportDprFiles(IQuote iquote, string CustomDestinationPath)
            ///on passe par une liste de iquote
            ///
            try {
                List<IQuote> quotelist = new List<IQuote>();
                quotelist.Add(iquote);
                //creation du fichier trans
                CreateTransFile transfile = new CreateTransFile();
                bool rst = transfile.Export(iquote.Context, quotelist, "");
                //au besoin export des dpr
                return true;
            }
            catch {
                return false;
            }
        }

          
        /// <summary>
        ///  validation des devis
        /// </summary>
        /// <param name="iquote"></param>
        /// <returns>reue if of</returns>
        public static bool Validate_Quote (IQuote iquote)
        {
            try {

                bool rst = false;
                //quote 
                IEntityList closed_quotes = iquote.Context.EntityManager.GetEntityList("_QUOTE_CLOSED", "ID", ConditionOperator.Equal, iquote.QuoteEntity.Id);
                closed_quotes.Fill(false);

                IEntityList sent_quotes = iquote.Context.EntityManager.GetEntityList("_QUOTE_SENT", "ID", ConditionOperator.Equal, iquote.QuoteEntity.Id);
                sent_quotes.Fill(false);


                if ((closed_quotes.Count + sent_quotes.Count) > 0)
                {
                    rst= true;
                }else {


                    throw new  UnvalidatedQuoteStatus("Le devis n'est pas visible dans les devis envoy�s ou clos.");

                   
                }
               
                        return rst;
                //IEntityList closed_quotes = iquote.Context.EntityManager.GetEntityList("_QUOTE_CLOSED", "_CLOSE_REASON", ConditionOperator.Equal, 1);



            }
            
            catch (UnvalidatedQuoteStatus) { Environment.Exit(0); ; return false; }
            catch (Exception ie) { MessageBox.Show(ie.Message); return false; }
        }


    }
    #endregion

    #region export api
    ///internal class CreateTransFile : IQuoteGpExporter
    internal class CreateTransFile : IQuoteGpExporter
    { 
        private IDictionary<IEntity, KeyValuePair<string, string>> _ReferenceIdList = new Dictionary<IEntity, KeyValuePair<string, string>>();
        private IDictionary<string, string> _ReferenceList = new Dictionary<string, string>();
        private IDictionary<string, long> _ReferenceListCount = new Dictionary<string, long>();
        private IDictionary<long, long> _FixeCostPartExportedList = new Dictionary<long, long>();
        private IDictionary<string, string> _PathList = new Dictionary<string, string>();

        private bool _GlobalExported = false;
        //declaration du nouveau log //

        /// <summary>
        /// export des donn�es de devis
        /// </summary>
        /// <param name="Context"></param>
        /// <param name="QuoteList"></param>
        /// <param name="ExportDirectory"></param>
        /// <returns></returns>

        ///devis exportable ou non
        ///
       public static bool Validate_Quote(IQuote iquote)
        {
            try
            {

                bool valid_quote = false;
                //quote closed or sent
                IEntityList closed_quotes = iquote.Context.EntityManager.GetEntityList("_QUOTE_CLOSED", "ID", ConditionOperator.Equal, iquote.QuoteEntity.Id);
                closed_quotes.Fill(false);

                IEntityList sent_quotes = iquote.Context.EntityManager.GetEntityList("_QUOTE_SENT", "ID", ConditionOperator.Equal, iquote.QuoteEntity.Id);
                sent_quotes.Fill(false);


                if ((closed_quotes.Count + sent_quotes.Count) > 0)
                {
                    valid_quote = true;
                }
                else
                {
                    throw new UnvalidatedQuoteStatus("Le devis n'est pas visible dans les devis envoy�s ou clos.");
                    
                }
                
                return valid_quote;
                //IEntityList closed_quotes = iquote.Context.EntityManager.GetEntityList("_QUOTE_CLOSED", "_CLOSE_REASON", ConditionOperator.Equal, 1);



            }

            catch (UnvalidatedQuoteStatus) { Environment.Exit(0); ; return false; }
            catch (Exception ie) { MessageBox.Show(ie.Message); return false; }
        }

            /// <summary>
            /// validation du context
            /// verification des code articles
            /// verification des param�trage des prix
            /// verificaiton des cjhemins de sortie
            /// verifiation des centre de frais
            /// </summary>
            /// <param name="contextlocal"></param>
            /// <returns></returns>
        public  bool Validate_Context(IContext contextlocal) { 
          try
            {
                bool valid_context = true;
                
                /// code article sur matieres
                IEntityList materials = contextlocal.EntityManager.GetEntityList("_MATERIAL");
                materials.Fill(false);

                ///control des materiaux
                if (materials.Count >0)
                {
                    foreach (IEntity material in materials)
                    {

                        
                        if (string.IsNullOrEmpty(material.GetFieldValueAsString("_CLIPPER_CODE_ARTICLE"))){
                   
                            MessageBox.Show("Certaines matieres n'ont pas de code article, il y a un risque de fonctionnement degrad� de l'export.");
                            break;
                        }
                    } 

                }

                ///verificatiion du param�trage des prix
                ///prix par matier epaisseur
                IParameterValue parametrageprix;
                string parametersetkey = "_QUOTE";
                string parametre_name = "_MAT_COST_BY_MATERIAL";
                contextlocal.ParameterSetManager.TryGetParameterValue(parametersetkey, parametre_name, out parametrageprix);
                    
                if ( (bool) parametrageprix.Value == false)
                {
                    throw new UnvalidatedQuoteConfigurations("Mauvais param�trage des prix, le mode prix epaisseur doit etre coch�");
                    
                }


                ///verification des chemins
                ///
                ///
               //   On recupere les parametres d'export'

                string Export_GP_Directory = contextlocal.ParameterSetManager.GetParameterValue("_EXPORT", "_EXPORT_GP_DIRECTORY").GetValueAsString();
                string Export_DPR_Directory = contextlocal.ParameterSetManager.GetParameterValue("_EXPORT", "_ACTCUT_DPR_DIRECTORY").GetValueAsString();

                if (string.IsNullOrEmpty(Export_GP_Directory)) { throw new UnvalidatedQuoteConfigurations("Le chemin d'export des devis n'est pas defini, l'export va etre annul�.");  }
                else { _PathList.Add("Export_GP_Directory", Export_GP_Directory);  }

                if (!string.IsNullOrEmpty(Export_DPR_Directory)) { _PathList.Add("Export_DPR_Directory", Export_DPR_Directory);  }
                    
               




                //string Export_GP_Directory = contextlocal.ParameterSetManager.GetParameterValue("_EXPORT", "_EXPORT_GP_DIRECTORY").GetValueAsString();
                //string Export_DPR_Directory = contextelocal.ParameterSetManager.GetParameterValue("_EXPORT", "_ACTCUT_DPR_DIRECTORY").GetValueAsString();


                //control des machines
                /*
                IParameterValue quotemachineparameter;
                string parametersetkey = "_MACHINE_LASER";
                string parametre_name = "_CENTRE_FRAIS";
                context.ParameterSetManager.TryGetParameterValue(parametersetkey, parametre_name, out quotemachineparameter);
                */
       //control des machines
                //IEntityList quote_machines = context.EntityManager.GetEntityList("_MATERIAL");
                //materials.Fill(false);



                return valid_context;
                


            }

            catch (UnvalidatedQuoteStatus) { Environment.Exit(0);return false;   }
            catch (UnvalidatedQuoteConfigurations) { Environment.Exit(0);return false;   }
            catch (Exception ie) { MessageBox.Show(ie.Message); return false; }
        }




        /// <summary>
        /// preparation des directory en utilisant celles definies dans le contexte
        /// </summary>
        /// <param name="contextelocal"></param>
        /// <returns></returns>
        public  bool PrepareExportDirectory(IContext contextelocal)
        {

            try

            {

                //verification de l'integrit� des donn�es
                //On recupere les parametres d'export'
                foreach (string key in _PathList.Keys) {
                    CreateDirectory(_PathList[key]);
                }
                
               


                return true;

            } catch(Exception ie) { MessageBox.Show(ie.Message); return false; }
            



        }
        /// <summary>
        /// preparation des directory en utilisant celle definie par la chaine Export_Directory
        /// </summary>
        /// <param name="Export_Directory"></param>
        /// <returns></returns>
        public  bool  CreateDirectory(string Export_Directory)
        {
            try {
                    
                    //creation de la directory si elle n'exist pas
                    if (!Directory.Exists(Export_Directory))
                    {
                         Directory.CreateDirectory(Export_Directory);
                    }


                return true;

            }
            catch (Exception ie) { MessageBox.Show(ie.Message); return false; }
        }



        /// <summary>
        /// creer les dpr du devis associ�s et copie eventuellement les dpr dans un autre dossier de destination 
        /// 
        /// </summary>
        /// <param name="iquote">iquotye a transferer</param>
        /// <param name="CustomDestinationPath">Laisser vide si pas de necessit� ; chemin de copie vers un autre dossier (besoin oxytemps pour les fichier dpr par exemple) </param>
        /// <returns></returns>
        public Dictionary<string, string> ExportDprFiles(IQuote iquote, string CustomDestinationPath)
        {
            Dictionary<string, string> filelist = new Dictionary<string, string>();

            try
            {


                IEntity quote;
                //recupe de l'entit� quote
                quote = iquote.QuoteEntity;
                long id_quote = quote.Id;


                //cretation 
                //string dpr_directory;
                 _PathList.TryGetValue("Export_DPR_Directory",out string dpr_directory) ;//quote.Context.ParameterSetManager.GetParameterValue("_EXPORT", "_ACTCUT_DPR_DIRECTORY").GetValueAsString();

                //export des dpr

                string dprExportDirect = dpr_directory + "\\" + "Quote_" + quote.Id.ToString();
                //emfFile vide
                CreateDirectory(dprExportDirect);

                bool dpr_exported = Actcut.QuoteModelManager.ExportDpr.ExportQuoteDpr(quote.Context, quote);
                dpr_directory = quote.Context.ParameterSetManager.GetParameterValue("_EXPORT", "_ACTCUT_DPR_DIRECTORY").GetValueAsString();
                foreach (IEntity partEntity in iquote.QuotePartList)
                {
                    if (dpr_exported)
                    {
                        string partname = partEntity.GetFieldValueAsString("_REFERENCE");
                        string pathtofile = dpr_directory + "\\" + "Quote_" + quote.Id + "\\" + partname + ".dpr.emf";
                        if (File.Exists(pathtofile) && !filelist.ContainsKey(partname))
                        {

                            if (string.IsNullOrEmpty(CustomDestinationPath)) { filelist.Add(partname, pathtofile); }
                            else
                            {
                                if (Directory.Exists(CustomDestinationPath) & File.Exists(pathtofile))
                                {
                                    File.Copy(pathtofile, CustomDestinationPath + "\\" + Path.GetFileName(pathtofile));
                                }
                            }



                        }


                    }
                }


                return filelist;
            }


            catch (DirectoryNotFoundException dirEx)
            {
                // directory not found --> on quit
                System.Windows.Forms.MessageBox.Show(dirEx.Message);
                Environment.Exit(0);
                return null;
            }

            catch (Exception ie) { System.Windows.Forms.MessageBox.Show(ie.Message); return null; }
            ///attention quote sdtalone est obligatoire pour exporter les dpr 


        }




        public bool Export(IContext Context, IEnumerable<IQuote> QuoteList, string CustomExportDirectory)
        {
            try {
                bool rst = false;
                string ExportDirectory="";
                //verification de l'integrit� des donn�es
                //preparing export
                //check_database_Integerity
                Validate_Context(Context);
                //creation des directory
                PrepareExportDirectory(Context);

                //on recuper le path
                if (string.IsNullOrEmpty(CustomExportDirectory))
                {
                     _PathList.TryGetValue("Export_GP_Directory", out ExportDirectory);
                }
                else{
                    ExportDirectory = CustomExportDirectory;
                }
                    
                string FullPath_FileName = "";
                string FileName = "";

                if (QuoteList.Count() == 1)
                {


                    FileName = "Trans_" + QuoteList.First().QuoteEntity.Id.ToString("####") + ".txt"; // QuoteList.First().QuoteInformation.IncNo.ToString("####") + ".txt";
                        IQuote quote = QuoteList.FirstOrDefault();
                        FullPath_FileName = Path.Combine(ExportDirectory, FileName);
                        rst = InternalExport(Context, QuoteList, FullPath_FileName);
                }


                return rst;


            }

            catch (DirectoryNotFoundException dirEx)
            {
                // directory not found --> on quit
                System.Windows.Forms.MessageBox.Show(dirEx.Message);
                Environment.Exit(0); return false;
            }

            catch (Exception ie) { System.Windows.Forms.MessageBox.Show(ie.Message); return false; }
        }

        #region IQuoteGpExporter Membres

        public  bool Export(IContext contextlocal, IEnumerable<IQuote> QuoteList, string ExportDirectory, string FileName)
        {
            try
            {


                bool rst = false;
                string FullPath_FileName = "";

                //verification de l'integrit� des donn�es
                //check_database_Integerity
                Validate_Context(contextlocal);
                //preparing export
                PrepareExportDirectory(contextlocal);

                if (QuoteList.Count() == 1)
                {
                    if (string.IsNullOrEmpty(FileName))
                    {
                        IQuote quote = QuoteList.FirstOrDefault();
                        FileName = "Trans_" + QuoteList.First().QuoteInformation.IncNo.ToString("####") + ".txt";
                                             
                       
                    }
                    else
                    {

                    }

                    FullPath_FileName = Path.Combine(ExportDirectory, FileName);
                    rst= InternalExport(contextlocal, QuoteList, FullPath_FileName);
                }
                else { rst = false; }

                return rst;

            }

            catch (DirectoryNotFoundException dirEx)
            {
                // directory not found --> on quit
                System.Windows.Forms.MessageBox.Show(dirEx.Message);
                Environment.Exit(0);
                return false;
            }


            catch (Exception ie) { System.Windows.Forms.MessageBox.Show(ie.Message); return false; }

        }


       

        #endregion
        /// <summary>
        /// export les dpr ainsi que le fichier trans
        /// </summary>
        /// <param name="context"></param>
        /// <param name="quoteList"></param>
        /// <param name="clipperFileName">nom du fichier trans</param>
        /// <returns></returns>
        internal bool InternalExport(IContext context, IEnumerable<IQuote> quoteList, string clipperFileName)
        {
            NumberFormatInfo formatProvider = new CultureInfo("en-US", false).NumberFormat;
            formatProvider.CurrencyDecimalSeparator = ".";
            formatProvider.CurrencyGroupSeparator = "";

            string file = "";
            File.Delete(clipperFileName);


            

            foreach (IQuote quote in quoteList)
            {

                Dictionary<string, string> filelist= new Dictionary<string, string>();
                // export systematique des dpr si le chemn d'export est defini//create dpr and directory
               
                 _PathList.TryGetValue("Export_DPR_Directory", out string dpr_directory);
                if (string.IsNullOrEmpty(dpr_directory))
                { 
                  filelist=ExportDprFiles(quote,"");
                }
               


                ///
                _ReferenceIdList = new Dictionary<IEntity, KeyValuePair<string, string>>();
                _ReferenceList = new Dictionary<string, string>();
                _ReferenceListCount = new Dictionary<string, long>();

                QuoteHeader(ref file, quote, formatProvider);
                QuoteOffre(ref file, quote, formatProvider);

                QuotePart(ref file, quote, "001", formatProvider);
                QuoteSet(ref file, quote, "001", formatProvider);

                file = file + "Fin d'enregistrement OK�" + Environment.NewLine;
            }
            file = file + "Fin du fichier OK";

            File.AppendAllText(clipperFileName, file, Encoding.Default);
            return true;
        }

        private void QuoteHeader(ref string file, IQuote quote, NumberFormatInfo formatProvider)
        {
            IEntity quoteEntity = quote.QuoteEntity;
            IEntity clientEntity = quoteEntity.GetFieldValueAsEntity("_FIRM");
            IEntity contactEntity = quoteEntity.GetFieldValueAsEntity("_CONTACT");
            string contactName = "";
            if (contactEntity != null)
                contactName = contactEntity.GetFieldValueAsString("_LAST_NAME") + " " + contactEntity.GetFieldValueAsString("_FIRST_NAME");

            IEntity quotterEntity = quoteEntity.GetFieldValueAsEntity("_QUOTER");
            string userCode = "";
            if (quotterEntity != null)
                userCode = quotterEntity.GetFieldValueAsString("USER_NAME");

            file = file + "du devis " + GetQuoteNumber(quoteEntity) + Environment.NewLine;

            long i = 0;
            string[] data = new string[50];
            data[i++] = "IDDEVIS";
            data[i++] = GetQuoteNumber(quoteEntity); //N� devis
            string indice; //max15 char
            indice=EmptyString(quoteEntity.GetFieldValueAsString("_REFERENCE"));
            if (indice.Length > 15)
            {
                indice = indice.Substring(0, 15);
            }

            data[i++] = indice; //Indice
            IField field;
            if (quoteEntity.EntityType.TryGetField("_CLIENT_ORDER_NUMBER", out field))
                data[i++] = EmptyString(quoteEntity.GetFieldValueAsString("_CLIENT_ORDER_NUMBER")).ToUpper(); //Rep�re commercial interne
            else
                data[i++] = ""; //Rep�re commercial interne
            data[i++] = EmptyString(clientEntity.GetFieldValueAsString("_EXTERNAL_ID")).ToUpper(); //Code client
            data[i++] = EmptyString(clientEntity.GetFieldValueAsString("_NAME")); //Nom client
            data[i++] = EmptyString(quoteEntity.GetFieldValueAsString("_DELIVERY_ADDRESS")); //Ligne adresse 1
            data[i++] = EmptyString(quoteEntity.GetFieldValueAsString("_DELIVERY_ADDRESS2")); //Ligne adresse 2
            data[i++] = ""; //Ligne adresse 3
            data[i++] = EmptyString(quoteEntity.GetFieldValueAsString("_DELIVERY_POSTCODE")); //Code postal
            data[i++] = EmptyString(quoteEntity.GetFieldValueAsString("_DELIVERY_CITY")); //Ville
            data[i++] = GetFieldDate(quoteEntity, "_CREATION_DATE"); //Date devis client
            data[i++] = GetFieldDate(quoteEntity, "_CREATION_DATE"); //Date enregistrement devis
            data[i++] = ""; //Code activit�
            data[i++] = "1"; //Etat
            data[i++] = ""; //N� revue de contrat
            data[i++] = ""; //Organisme de contr�le
            data[i++] = userCode; //Code employ� (d�faut) Sce tech. commercial ou m�thodes
            data[i++] = ""; //R�f�rence client de l'AO
            data[i++] = ""; //Responsable M�thode chez le client
            data[i++] = contactName; //Responsable achat chez le client
            data[i++] = ""; //Responsable qualit� chez le client
            data[i++] = userCode; //Employ� Commercial
            data[i++] = ""; //Employ� responsable qualit�
            data[i++] = ""; //Responsable achat
            data[i++] = ""; //Responsable validation visa
            data[i++] = GetFieldDate(quoteEntity, "_SENT_DATE"); //Date visa resp. (d�faut) Sce tech. commercial ou m�thodes
            data[i++] = GetFieldDate(quoteEntity, "_SENT_DATE"); //Date visa resp. Commercial
            data[i++] = ""; //Date visa resp. Qualit�
            data[i++] = ""; //Date visa resp. Achat
            data[i++] = ""; //Date responsablevalidation visa//
            data[i++] = ""; //Date r�ponse souhait�e//
            data[i++] = ""; //Temps mis pour faire le devis//
            data[i++] = ""; //Date de d�but//
            data[i++] = "9"; //Monnaie//
            data[i++] = EmptyString(quoteEntity.GetFieldValueAsString("_COMMENTS")+";");//Observations Ent�te devis
            data[i++] = ""; //Incoterms (champ observations)
            DateTime validityDate = quoteEntity.GetFieldValueAsDateTime("_SENT_DATE").AddDays(Convert.ToInt32(quoteEntity.GetFieldValueAsDouble("_ACCEPTANCE_PERIOD")));
            data[i++] = validityDate.ToString("yyyyMMdd");
            WriteData(data, i, ref file);
        }


       



        private void QuoteOffre(ref string file, IQuote quote, NumberFormatInfo formatProvider)
        {
            IEntity quoteEntity = quote.QuoteEntity;
            IEntity clientEntity = quoteEntity.GetFieldValueAsEntity("_FIRM");
            IEntity paymentRuleEntity = quoteEntity.GetFieldValueAsEntity("_PAYMENT_RULE");
            string paymentRule = "";
            if (paymentRuleEntity != null)
                paymentRule = EmptyString(paymentRuleEntity.GetFieldValueAsString("_EXTERNAL_ID")).ToUpper();


            #region observation piece


            foreach (IEntity partEntity in quote.QuotePartList)
            {
                long partQty = 0;
                partQty = partEntity.GetFieldValueAsLong("_PART_QUANTITY");

                if (partQty > 0)
                {
                    long i = 0;
                    string[] data = new string[50];
                    string reference = null;
                    string modele = null;
                    GetReference(partEntity, "PART", true, out reference, out modele);

                    data[i++] = "OBSDEVIS";
                    data[i++] = ""; //Observation interne
                    data[i++] = EmptyString(partEntity.GetFieldValueAsString("_COMMENTS")); //Observation client
                    data[i++] = ""; // Conditions de r�glement
                    data[i++] = GetQuoteNumber(quoteEntity);//N� devis
                    data[i++] = reference;//Code pi�ce
                    data[i++] = "";//Ordre d'impression
                    data[i++] = "";//Cycle de fab
                    data[i++] = "";//Code activit�e de la pi�ce
                    data[i++] = "";//Modele de gamme

                    WriteData(data, i, ref file);
                }
            }











            #endregion


            #region Offre pi�ces

            foreach (IEntity partEntity in quote.QuotePartList)
            {
                long partQty = 0;
                partQty = partEntity.GetFieldValueAsLong("_PART_QUANTITY");

                if (partQty > 0)
                {
                    long i = 0;
                    string[] data = new string[50];
                    string reference = null;
                    string modele = null;
                    GetReference(partEntity, "PART", true, out reference, out modele);

                    data[i++] = "OFFRE";
                    data[i++] = reference; //Code pi�ce
                    data[i++] = GetQuoteNumber(quoteEntity); //N� devis
                    data[i++] = partQty.ToString(); //Qt� offre

                    double cost = partEntity.GetFieldValueAsDouble("_CORRECTED_FRANCO_UNIT_COST");
                    data[i++] = cost.ToString("#0.0000", formatProvider); //Prix de revient
                    data[i++] = cost.ToString("#0.0000", formatProvider); //Prix brut
                    data[i++] = cost.ToString("#0.0000", formatProvider); //Prix de vente
                    data[i++] = cost.ToString("#0.0000", formatProvider); //Prix dans la monnaie
                    data[i++] = "1"; //N� de ligne "Offre"

                    IField field;
                    if (quoteEntity.EntityType.TryGetField("_DELIVERY_DATE", out field))
                    {
                        data[i++] = GetFieldDate(quoteEntity, "_DELIVERY_DATE"); //Nb d�lai
                        data[i++] = "4"; //Type d�lai 1=jour 4=date
                    }
                    else
                    {
                        data[i++] = "0"; //Nb d�lai
                        data[i++] = "1"; //Type d�lai 1=jour 4=date
                    }
                    data[i++] = "1"; //Unit� de prix
                    data[i++] = "0"; //Remise 1
                    data[i++] = "0"; //Remise 2
                    data[i++] = paymentRule; //Code de reglement
                    data[i++] = CreateTransFile.GetTransport(quoteEntity); // Port
                    data[i++] = modele; //Mod�le
                    data[i++] = "1"; //Imprimable
                    WriteData(data, i, ref file);
                }
            }

            #endregion




            #region Offre Ensembles
            foreach (IEntity setEntity in quote.QuoteSetList)
            {
                //// OBSERVATION DE DEVIS PAR ENSEMBLE
                // long partQty = 0;
                long qty = setEntity.GetFieldValueAsLong("_QUANTITY");

                if (qty > 0)
                {
                    long i = 0;
                    string[] data = new string[50];
                    string reference = null;
                    string modele = null;
                    GetReference(setEntity, "SET", true, out reference, out modele);

                    data[i++] = "OBSDEVIS";
                    data[i++] = ""; //Observation interne
                    data[i++] = EmptyString(setEntity.GetFieldValueAsString("_COMMENTS")); //Observation client
                    data[i++] = ""; // Conditions de r�glement
                    data[i++] = GetQuoteNumber(quoteEntity);//N� devis
                    data[i++] = reference;//Code pi�ce
                    data[i++] = "";//Ordre d'impression
                    data[i++] = "";//Cycle de fab
                    data[i++] = "";//Code activit�e de la pi�ce
                    data[i++] = "";//Modele de gamme
                    WriteData(data, i, ref file);
                }

                
               
            }
            foreach (IEntity setEntity in quote.QuoteSetList)
            {
                
               // long partQty = 0;
                long qty = setEntity.GetFieldValueAsLong("_QUANTITY");

               
                if (qty > 0)
                {
                    long i = 0;
                    string[] data = new string[50];
                    string reference = null;
                    string modele = null;
                    GetReference(setEntity, "SET", true, out reference, out modele);

                    double totalPartCost = 0;
                    data[i++] = "OFFRE";
                    data[i++] = reference; //Code pi�ce
                    data[i++] = GetQuoteNumber(quoteEntity); //N� devis
                    data[i++] = qty.ToString(formatProvider); //Qt� offre

                    double cost = setEntity.GetFieldValueAsDouble("_CORRECTED_FRANCO_UNIT_COST") - totalPartCost;
                    data[i++] = cost.ToString("#0.0000", formatProvider); //Prix de revient
                    data[i++] = cost.ToString("#0.0000", formatProvider); //Prix brut
                    data[i++] = cost.ToString("#0.0000", formatProvider); //Prix de vente
                    data[i++] = cost.ToString("#0.0000", formatProvider); //Prix dans la monnaie
                    data[i++] = "1"; //N� de ligne "Offre"
                    IField field;
                    if (quoteEntity.EntityType.TryGetField("_DELIVERY_DATE", out field))
                    {
                        data[i++] = GetFieldDate(quoteEntity, "_DELIVERY_DATE"); //Nb d�lai
                        data[i++] = "4"; //Type d�lai 1=jour 4=date
                    }
                    else
                    {
                        data[i++] = "0"; //Nb d�lai
                        data[i++] = "1"; //Type d�lai 1=jour 4=date
                    }
                    data[i++] = "1"; //Unit� de prix
                    data[i++] = "0"; //Remise 1
                    data[i++] = "0"; //Remise 2
                    data[i++] = paymentRule; //Code de reglement
                    data[i++] = CreateTransFile.GetTransport(quoteEntity); //Port
                    data[i++] = modele; //Mod�le
                    data[i++] = "1"; //Imprimable
                    WriteData(data, i, ref file);
                }
            }

            #endregion

            if (quote.QuoteEntity.GetFieldValueAsLong("_TRANSPORT_PAYMENT_MODE") == 1) // Transport factur�
                Transport(ref file, quote, formatProvider, true, "001", "PORT", "0");

            GlobalItem(ref file, quote, formatProvider, true, "001", "GLOBAL", "0");
        }

        private void Transport(ref string file, IQuote quote, NumberFormatInfo formatProvider, bool doOffre, string rang, string reference, string modele)
        {
            IEntity quoteEntity = quote.QuoteEntity;
            IEntity clientEntity = quoteEntity.GetFieldValueAsEntity("_FIRM");
            IEntity paymentRuleEntity = quoteEntity.GetFieldValueAsEntity("_PAYMENT_RULE");
            string paymentRule = "";
            if (paymentRuleEntity != null)
                paymentRule = EmptyString(paymentRuleEntity.GetFieldValueAsString("_EXTERNAL_ID")).ToUpper();

            double calCost = quote.QuoteEntity.GetFieldValueAsDouble("_TRANSPORT_CAL_COST");
            double cost = quote.QuoteEntity.GetFieldValueAsDouble("_TRANSPORT_CORRECTED_COST");
            if (cost > 0)
            {
                long gadevisPhase = 10;
                long nomendvPhase = 10;

                #region Creation de l'offre

                if (doOffre)
                {
                    long i = 0;
                    string[] data = new string[50];
                    data[i++] = "OFFRE";
                    data[i++] = reference; //Code pi�ce
                    data[i++] = GetQuoteNumber(quoteEntity); //N� devis
                    data[i++] = "1"; //Qt� offre

                    data[i++] = cost.ToString("#0.0000", formatProvider); //Prix de revient
                    data[i++] = cost.ToString("#0.0000", formatProvider); //Prix brut
                    data[i++] = cost.ToString("#0.0000", formatProvider); //Prix de vente
                    data[i++] = cost.ToString("#0.0000", formatProvider); //Prix dans la monnaie
                    data[i++] = "1"; //N� de ligne "Offre"
                    IField field;
                    if (quoteEntity.EntityType.TryGetField("_DELIVERY_DATE", out field))
                    {
                        data[i++] = GetFieldDate(quoteEntity, "_DELIVERY_DATE"); //Nb d�lai
                        data[i++] = "4"; //Type d�lai 1=jour 4=date
                    }
                    else
                    {
                        data[i++] = "0"; //Nb d�lai
                        data[i++] = "1"; //Type d�lai 1=jour 4=date
                    }
                    data[i++] = "1"; //Unit� de prix
                    data[i++] = "0"; //Remise 1
                    data[i++] = "0"; //Remise 2
                    data[i++] = paymentRule; //Code de reglement
                    data[i++] = CreateTransFile.GetTransport(quoteEntity); //Port
                    data[i++] = "0"; //Mod�le
                    data[i++] = "1"; //Imprimable
                    WriteData(data, i, ref file);
                }
                #endregion

                #region Creation de la pi�ce global

                {
                    long i = 0;
                    string[] data = new string[50];

                    data[i++] = "ENDEVIS";
                    data[i++] = GetQuoteNumber(quoteEntity); //N� devis
                    data[i++] = EmptyString(clientEntity.GetFieldValueAsString("_EXTERNAL_ID")).ToUpper(); //Code client
                    data[i++] = reference; //Code pi�ce
                    data[i++] = ""; //Type (non utilis�)
                    data[i++] = FormatDesignation(""); //D�signation 1
                    data[i++] = FormatDesignation(""); //D�signation 2
                    data[i++] = FormatDesignation(""); //D�signation 3
                    data[i++] = rang; //Rang
                    data[i++] = reference; //Code pi�ce ou Sous pi�ce (sous rang)
                    data[i++] = FormatDesignation(""); //D�signation pi�ce ou Sous pi�ce (sous rang)
                    data[i++] = ""; //N� plan
                    data[i++] = rang; //Niveau rang
                    data[i++] = "3"; //Etat devis
                    data[i++] = ""; //Rep�re
                    data[i++] = "0"; //Origine fourniture
                    data[i++] = "1"; //Qt� dus/ensemble : 1 pour le rang 001
                    data[i++] = "1"; //Qt� totale de l'ensemble : 1 pour le rang 001
                    data[i++] = ""; //Indice plan
                    data[i++] = ""; //Indice gamme
                    data[i++] = ""; //Indice nomenclature
                    data[i++] = ""; //Indice pi�ce
                    data[i++] = ""; //Indice A
                    data[i++] = ""; //Indice B
                    data[i++] = ""; //Indice C
                    data[i++] = ""; //Indice D
                    data[i++] = ""; //Indice E
                    data[i++] = ""; //Indice F
                    data[i++] = "0"; //N� identifiant GED 1
                    data[i++] = "0"; //N� identifiant GED 2
                    data[i++] = "0"; //N� identifiant GED 3
                    data[i++] = "0"; //N� identifiant GED 4
                    data[i++] = "0"; //N� identifiant GED 5
                    data[i++] = "0"; //N� identifiant GED 6
                    data[i++] = "0"; //N� identifiant GED 7
                    data[i++] = "0"; //N� identifiant GED 8
                    data[i++] = "0"; //N� identifiant GED 9
                    data[i++] = "0"; //N� identifiant GED 10
                    data[i++] = ""; //Fichier joint
                    data[i++] = ""; //Date d'injection
                    data[i++] = "0"; //Mod�le
                    data[i++] = ""; //Employ� responsable                
                    WriteData(data, i, ref file);
                }

                #endregion

                #region Creation de l'achat

                {
                    long i = 0;
                    string[] data = new string[50];

                    data[i++] = "GADEVIS";
                    data[i++] = rang; //Rang
                    data[i++] = ""; //inutilis�
                    data[i++] = gadevisPhase.ToString(formatProvider); //Phase
                    data[i++] = FormatDesignation("ACHAT NOMENCLATURE"); //D�signation 1
                    data[i++] = FormatDesignation(""); //D�signation 2
                    data[i++] = FormatDesignation(""); //D�signation 3
                    data[i++] = FormatDesignation(""); //D�signation 4
                    data[i++] = FormatDesignation(""); //D�signation 5
                    data[i++] = FormatDesignation(""); //D�signation 6
                    data[i++] = "NOMEN"; //Centre de frais

                    data[i++] = "0"; //Tps Prep
                    data[i++] = "0"; //Tps Unit
                    data[i++] = "0"; //Co�t Op�ration
                    data[i++] = "0"; //Taux horaire
                    data[i++] = GetFieldDate(quoteEntity, "_CREATION_DATE"); //Date
                    data[i++] = GetQuoteNumber(quoteEntity); //N� devis
                    data[i++] = ""; //Nom fichier joint
                    data[i++] = "0"; //N� identifiant GED 1
                    data[i++] = "0"; //N� identifiant GED 2
                    data[i++] = "0"; //N� identifiant GED 3
                    data[i++] = "0"; //N� identifiant GED 4
                    data[i++] = "0"; //N� identifiant GED 5
                    data[i++] = "0"; //N� identifiant GED 6
                    data[i++] = "0"; //Niveau du rang
                    data[i++] = "";//Observations
                    data[i++] = ""; //Lien avec la phase de nomenclature
                    data[i++] = ""; //Date derni�re modif
                    data[i++] = ""; //Employ� modif
                    data[i++] = ""; //Niveau de blocage
                    data[i++] = ""; //Taux homme TP
                    data[i++] = ""; //Taux homme TU
                    data[i++] = ""; //Nb pers TP
                    data[i++] = ""; //Nb Pers TU
                    WriteData(data, i, ref file);
                }

                {
                    long i = 0;
                    string[] data = new string[50];

                    i = 0;
                    data = new string[50];
                    //a modifier
                    string transportFamilly = quote.Context.ParameterSetManager.GetParameterValue("_EXPORT", "_CLIPPER_TRANSPORT_FAMILLY").GetValueAsString();

                    data[i++] = "NOMENDV";
                    data[i++] = GetQuoteNumber(quoteEntity); //Code devis
                    data[i++] = reference; //Code pi�ce
                    data[i++] = rang; //Rang
                    data[i++] = nomendvPhase.ToString(formatProvider); //Phase
                    data[i++] = ""; //Rep�re
                    data[i++] = "TRANSPORT_ALMA"; //Code article
                    data[i++] = FormatDesignation("cout de transport devis nuemro "); //D�signation 1
                    data[i++] = FormatDesignation(""); //D�signation 2
                    data[i++] = FormatDesignation(""); //D�signation 3
                    data[i++] = ""; //Temps de r�appro
                    data[i++] = "1"; //Qt�
                    data[i++] = calCost.ToString("#0.000", formatProvider); //Px article ou Px/Kg
                    data[i++] = calCost.ToString("#0.000", formatProvider); //Prix total
                    data[i++] = ""; //Code Fournisseur
                    data[i++] = ""; //2sd fournisseur
                    data[i++] = "1"; //Type
                    data[i++] = "1"; //Prix constant
                    data[i++] = ""; //Poids t�le ou article
                    data[i++] = transportFamilly; //Famille
                    data[i++] = ""; //N� tarif de Clipper
                    data[i++] = "Cout de transport du devis alma " + GetQuoteNumber(quoteEntity); //Observation
                    data[i++] = ""; //Observation interne
                    data[i++] = ""; //Observation d�bit
                    data[i++] = ""; //Val D�bit 1
                    data[i++] = ""; //Val D�bit 2
                    data[i++] = ""; //Qt� D�bit
                    data[i++] = ""; //Nb pc/d�bit ou d�bit/pc
                    data[i++] = ""; //Lien avec la phase de gamme
                    data[i++] = ""; //Unite de quantit�
                    data[i++] = ""; //Unit� de prix
                    data[i++] = ""; //Coef Unite
                    data[i++] = ""; //Coef Prix
                    data[i++] = "0"; //Prix constant ??? semble plutot correcpondre au Mod�le
                    data[i++] = "0"; //Mod�le ??? semble plutot correcpondre au Prix constant
                    data[i++] = ""; //Qt� constant
                    data[i++] = gadevisPhase.ToString(formatProvider); //Magasin ???? erreur

                    WriteData(data, i, ref file);
                }
                #endregion
            }
        }
        private void GlobalItem(ref string file, IQuote quote, NumberFormatInfo formatProvider, bool doOffre, string rang, string reference, string modele)
        {
            IEntity quoteEntity = quote.QuoteEntity;
            IEntity clientEntity = quoteEntity.GetFieldValueAsEntity("_FIRM");
            IEntity paymentRuleEntity = quoteEntity.GetFieldValueAsEntity("_PAYMENT_RULE");
            string paymentRule = "";
            if (paymentRuleEntity != null)
                paymentRule = EmptyString(paymentRuleEntity.GetFieldValueAsString("_EXTERNAL_ID")).ToUpper();

            IList<IEntity> globalSupplyList = new List<IEntity>(quote.FreeSupplyList.Where(p => p.GetFieldValueAsBoolean("_FRANCO") != doOffre));
            IList<IEntity> globalOperationList = new List<IEntity>(quote.FreeOperationList.Where(p => p.GetFieldValueAsBoolean("_FRANCO") != doOffre));

            if (globalOperationList.Count > 0 || globalSupplyList.Count > 0)
            {
                #region Creation de l'offre

                if (doOffre)
                {
                    long i = 0;
                    string[] data = new string[50];
                    data[i++] = "OFFRE";
                    data[i++] = reference; //Code pi�ce
                    data[i++] = GetQuoteNumber(quoteEntity); //N� devis
                    data[i++] = "1"; //Qt� offre

                    double operationCost = globalOperationList.Sum(p => p.GetFieldValueAsDouble("_CORRECTED_COST"));
                    double supplyCost = globalSupplyList.Sum(p => p.GetFieldValueAsDouble("_CORRECTED_COST"));
                    double cost = operationCost + supplyCost;
                    data[i++] = cost.ToString("#0.0000", formatProvider); //Prix de revient
                    data[i++] = cost.ToString("#0.0000", formatProvider); //Prix brut
                    data[i++] = cost.ToString("#0.0000", formatProvider); //Prix de vente
                    data[i++] = cost.ToString("#0.0000", formatProvider); //Prix dans la monnaie

                    data[i++] = "1"; //N� de ligne "Offre"
                    IField field;
                    if (quoteEntity.EntityType.TryGetField("_DELIVERY_DATE", out field))
                    {
                        data[i++] = GetFieldDate(quoteEntity, "_DELIVERY_DATE"); //Nb d�lai
                        data[i++] = "4"; //Type d�lai 1=jour 4=date
                    }
                    else
                    {
                        data[i++] = "0"; //Nb d�lai
                        data[i++] = "1"; //Type d�lai 1=jour 4=date
                    }
                    data[i++] = "1"; //Unit� de prix
                    data[i++] = "0"; //Remise 1
                    data[i++] = "0"; //Remise 2
                    data[i++] = paymentRule; //Code de reglement
                    data[i++] = CreateTransFile.GetTransport(quoteEntity); //Port
                    data[i++] = "0"; //Mod�le
                    data[i++] = "1"; //Imprimable
                    WriteData(data, i, ref file);
                }

                #endregion

                #region Creation de la pi�ce global

                {
                    long i = 0;
                    string[] data = new string[50];

                    data[i++] = "ENDEVIS";
                    data[i++] = GetQuoteNumber(quoteEntity); //N� devis
                    data[i++] = EmptyString(clientEntity.GetFieldValueAsString("_EXTERNAL_ID")).ToUpper(); //Code client
                    data[i++] = reference; //Code pi�ce
                    data[i++] = ""; //Type (non utilis�)
                    data[i++] = FormatDesignation(""); //D�signation 1
                    data[i++] = FormatDesignation(""); //D�signation 2
                    data[i++] = FormatDesignation(""); //D�signation 3
                    data[i++] = rang; //Rang
                    data[i++] = reference; //Code pi�ce ou Sous pi�ce (sous rang)
                    data[i++] = FormatDesignation(""); //D�signation pi�ce ou Sous pi�ce (sous rang)
                    data[i++] = ""; //N� plan
                    data[i++] = rang; //Niveau rang
                    data[i++] = "3"; //Etat devis
                    data[i++] = ""; //Rep�re
                    data[i++] = "0"; //Origine fourniture
                    data[i++] = "1"; //Qt� dus/ensemble : 1 pour le rang 001
                    data[i++] = "1"; //Qt� totale de l'ensemble : 1 pour le rang 001
                    data[i++] = ""; //Indice plan
                    data[i++] = ""; //Indice gamme
                    data[i++] = ""; //Indice nomenclature
                    data[i++] = ""; //Indice pi�ce
                    data[i++] = ""; //Indice A
                    data[i++] = ""; //Indice B
                    data[i++] = ""; //Indice C
                    data[i++] = ""; //Indice D
                    data[i++] = ""; //Indice E
                    data[i++] = ""; //Indice F
                    data[i++] = "0"; //N� identifiant GED 1
                    data[i++] = "0"; //N� identifiant GED 2
                    data[i++] = "0"; //N� identifiant GED 3
                    data[i++] = "0"; //N� identifiant GED 4
                    data[i++] = "0"; //N� identifiant GED 5
                    data[i++] = "0"; //N� identifiant GED 6
                    data[i++] = "0"; //N� identifiant GED 7
                    data[i++] = "0"; //N� identifiant GED 8
                    data[i++] = "0"; //N� identifiant GED 9
                    data[i++] = "0"; //N� identifiant GED 10
                    data[i++] = ""; //Fichier joint
                    data[i++] = ""; //Date d'injection
                    data[i++] = "0"; //Mod�le
                    data[i++] = ""; //Employ� responsable                
                    WriteData(data, i, ref file);
                }

                #endregion

                long cutGaDevisPhase = 0;
                long gadevisPhase = 0;
                long nomendvPhase = 0;

                QuoteSupply(ref file, quote, 1, globalSupplyList, rang, ref gadevisPhase, ref nomendvPhase, formatProvider, true, reference, modele);

                AQuoteOperation(ref file, quote, null, globalOperationList, ref cutGaDevisPhase, rang, formatProvider, 1, 1, ref gadevisPhase, ref nomendvPhase, reference, modele);
            }
        }

        private void QuotePart(ref string file, IQuote quote, string rang, NumberFormatInfo formatProvider)
        {
            IEntity quoteEntity = quote.QuoteEntity;
            IEntity clientEntity = quoteEntity.GetFieldValueAsEntity("_FIRM");
            //creation 
            //string dpr_directory = quote.Context.ParameterSetManager.GetParameterValue("_EXPORT", "_ACTCUT_DPR_DIRECTORY").GetValueAsString();

            //create dpr and directory
            _PathList.TryGetValue("Export_DPR_Directory", out string dpr_directory);
            if (!string.IsNullOrEmpty(dpr_directory)) { 
            Dictionary<string, string> filelist = ExportDprFiles(quote, dpr_directory);//AF_Export_Devis_Clipper.Export_Quote_Files.Export(quote);
            }


            foreach (IEntity partEntity in quote.QuotePartList)
            {
                IEntity materialEntity = partEntity.GetFieldValueAsEntity("_MATERIAL");
                string materialName = "";
                if (materialEntity != null)
                    materialName = materialEntity.GetFieldValueAsString("_NAME");

                long partQty = 0;
                partQty = partEntity.GetFieldValueAsLong("_PART_QUANTITY");

                long totalPartQty = partEntity.GetFieldValueAsLong("_QUANTITY");
                if (partQty > 0)
                {
                    long i = 0;
                    string[] data = new string[50];
                    string partReference = null;
                    string partModele = null;
                    GetReference(partEntity, "PART", false, out partReference, out partModele);

                    data[i++] = "ENDEVIS";
                    data[i++] = GetQuoteNumber(quoteEntity); //N� devis
                    data[i++] = EmptyString(clientEntity.GetFieldValueAsString("_EXTERNAL_ID")).ToUpper(); //Code client
                    data[i++] = partReference; //Code pi�ce
                    data[i++] = ""; //Type (non utilis�)
                    data[i++] = FormatDesignation(partEntity.GetFieldValueAsString("_DESCRIPTION")); //D�signation 1
                    data[i++] = FormatDesignation(materialName); //D�signation 2
                    data[i++] = FormatDesignation(""); //D�signation 3
                    data[i++] = rang; //Rang
                    data[i++] = partReference; //Code pi�ce ou Sous pi�ce (sous rang)
                    data[i++] = FormatDesignation(partEntity.GetFieldValueAsString("_DESCRIPTION")); //D�signation pi�ce ou Sous pi�ce (sous rang)
                    data[i++] = partEntity.Id.ToString(); //N� plan
                    data[i++] = rang; //Niveau rang
                    data[i++] = "3"; //Etat devis
                    data[i++] = ""; //Rep�re
                    data[i++] = "0"; //Origine fourniture
                    data[i++] = "1"; //Qt� dus/ensemble : 1 pour le rang 001
                    data[i++] = "1"; //Qt� totale de l'ensemble : 1 pour le rang 001
                    data[i++] = ""; //Indice plan
                    data[i++] = ""; //Indice gamme
                    data[i++] = ""; //Indice nomenclature
                    data[i++] = ""; //Indice pi�ce
                    double weight = partEntity.GetFieldValueAsDouble("_WEIGHT");
                    double weightEx = partEntity.GetFieldValueAsDouble("_WEIGHT_EX");
                    weight = weight / 1000;
                    weightEx = weightEx / 1000;
                    data[i++] = weight.ToString("#0.0000", formatProvider); //Indice A
                    data[i++] = weightEx.ToString("#0.0000", formatProvider); //Indice B
                    data[i++] = "-" + partEntity.Id.ToString(); //Indice C  valeur - si piece quote et + si piece cam
                    data[i++] = ""; //Indice D
                    data[i++] = ""; //Indice E
                    data[i++] = ""; //Indice F
                    data[i++] = "0"; //N� identifiant GED 1
                    data[i++] = "0"; //N� identifiant GED 2
                    data[i++] = "0"; //N� identifiant GED 3
                    data[i++] = "0"; //N� identifiant GED 4
                    data[i++] = "0"; //N� identifiant GED 5
                    data[i++] = "0"; //N� identifiant GED 6
                    data[i++] = "0"; //N� identifiant GED 7
                    data[i++] = "0"; //N� identifiant GED 8
                    data[i++] = "0"; //N� identifiant GED 9
                    data[i++] = "0"; //N� identifiant GED 10

                  
                    ///on laisse pour le moment
                    ///
                    string assistantType = partEntity.GetFieldValueAsString("_ASSISTANT_TYPE");
                    string partFileName = partEntity.GetFieldValueAsString("_FILENAME");

                    //string partName="" ;
                    //filelist.TryGetValue(partEntity.GetFieldValueAsString("_REFERENCE"), out partName );
                    ///
                    //ATTENTION LES GENERATION DES DPR DEPEND DE LA LICENCE/ IL FAUT UNE QUOTE CUT 
                    //SINON IL S4AGIT DE PIECES ALMACAM
                    //
                    string emfFile = "";

                    bool isGenericDpr = false;
                    if (assistantType.Contains("GenericEditAssistant"))
                    {
                        if (string.IsNullOrEmpty(partFileName) == false)
                        {
                            if (partFileName.EndsWith(".dpr", StringComparison.InvariantCultureIgnoreCase))
                            {
                                isGenericDpr = true;
                            }
                        }
                    }

                    

                    
                    
                    {
                        //emfFile;
                       
                       // if (_PathList["ACTCUT_DPR_DIRECTORY"] != "") {
                       if (!string.IsNullOrEmpty(dpr_directory)) { 
                            
                            string empty_emfFile;

                            //cas g�n�ral
                            emfFile = partEntity.GetFieldValueAsString("_DPR_FILENAME") + ".emf";
                            //emfFile vide
                            empty_emfFile = dpr_directory + "\\" + "Quote_" + quote.QuoteEntity.Id + "\\"+ partEntity.GetFieldValueAsString("_REFERENCE")+".emf"; // + Path.GetFileName(partEntity.GetImageFieldValueAsLinkFile("_PREVIEW"));
                            //cas general
                             emfFile = GetEmfFile(partEntity, empty_emfFile);

                           
                            if (assistantType.Contains("DprAssistant") || isGenericDpr)
                            {
                                                         
                                emfFile = partEntity.GetFieldValueAsString("_FILENAME")+ ".emf";
                               
                            }
                            
                            //traitement sp�
                            //cas des pieces simples//
                            else if (assistantType.Contains("PluggedSimpleAssistantEx")) {
                                //creation du point rouge dans l'emf : signature des apercus de peices quotes
                                Sign_quote_Emf(emfFile);                               
                            }


                            //PluggedSketchAssistant
                            else if (assistantType.Contains("PluggedSketchAssistant"))
                            {
                                //creation du point rouge dans l'emf : signature des apercus de peices quotes
                                Sign_quote_Emf(emfFile);
                              
                            }
                            //PluggedGeometryAssistant
                            else if (assistantType.Contains("PluggedGeometryAssistant"))
                            {
                                //creation du point rouge dans l'emf : signature des apercus de peices quotes
                                Sign_quote_Emf(emfFile);
                              
                            }
                            //PluggedDplAssistant
                            else if (assistantType.Contains("PluggedDplAssistan"))
                            {   //creation du point rouge dans l'emf : signature des apercus de peices quotes
                                Sign_quote_Emf(emfFile);
                               
                            }

                            //PluggedDxfAssistant
                            else if (assistantType.Contains("PluggedDxfAssistant"))
                            {   //creation du point rouge dans l'emf : signature des apercus de peices quotes
                                Sign_quote_Emf(emfFile);
                                

                            }

                            
                            else
                            {  //creation du point rouge dans l'emf : signature des apercus de peices quotes
                                Sign_quote_Emf(emfFile);
                               
                            }
                            








                        }
                        else
                        {
                            emfFile=partEntity.GetImageFieldValueAsLinkFile("_PREVIEW");
                        }

                        if (emfFile != null)
                            data[i++] = emfFile; //Fichier joint
                        else
                            data[i++] = ""; //Fichier joint
                    }
                    data[i++] = ""; //Date d'injection
                    data[i++] = partModele; //Mod�le
                    data[i++] = ""; //Employ� responsable                
                    WriteData(data, i, ref file);

                    long gadevisPhase = 0;
                    long nomendvPhase = 0;

                    // Fourniture
                    IList<IEntity> partSupplyList = new List<IEntity>(quote.GetPartSupplyList(partEntity));
                    QuoteSupply(ref file, quote, 1, partSupplyList, rang, ref gadevisPhase, ref nomendvPhase, formatProvider, true, partReference, partModele);

                    // Operation
                    double totalMaterialPrice = partEntity.GetFieldValueAsDouble("_MAT_IN_COST");
                    double materialPrice = totalMaterialPrice / totalPartQty;
                    IList<IEntity> partOperationList = new List<IEntity>(quote.GetPartOperationList(partEntity));
                    QuoteOperation(ref file, quote, partEntity, partOperationList, rang, formatProvider, 1, partQty, ref gadevisPhase, ref nomendvPhase, partReference, partModele, materialPrice);

                    if (_GlobalExported == false)
                    {
                        if (quote.QuoteEntity.GetFieldValueAsLong("_TRANSPORT_PAYMENT_MODE") != 1) // Transport non factur�
                            Transport(ref file, quote, formatProvider, false, "003", partReference, partModele);

                        // On met les item globaux masqu�s sur la premi�re pi�ces dans le rang "002"
                        GlobalItem(ref file, quote, formatProvider, false, "002", partReference, partModele);
                        _GlobalExported = true;
                    }
                }
            }
        }



        //trace une ligne rouge ou un point rouge sur  les dpr issue de quote

        private void Sign_quote_Emf(string empty_emfFile)
        {

            string pathtoemf = @empty_emfFile;
            string initialemf = empty_emfFile.Replace(".dpr.emf", ".tmp");
            string mAttributes = " ";


            if (File.Exists(pathtoemf))
            {
                File.Move(pathtoemf, initialemf);

                Metafile m = new Metafile(initialemf);
                MetafileHeader header = m.GetMetafileHeader();
                mAttributes += "Size :" + header.MetafileSize.ToString();
                int H = header.Bounds.Height;
                int W = header.Bounds.Width;

                Bitmap b = new Bitmap(W, H);
                using (Graphics g = Graphics.FromImage(b))
                {
                    g.Clear(Color.White);
                    Point p = new Point(0, 0);
                    RectangleF bounds = new RectangleF(0, 0, W, H);
                    g.DrawImage(m, bounds);

                    
                    Pen RedPen = new Pen(Color.Red, 10);
                    g.DrawEllipse(RedPen, 20, 20, 10, 10);
                    
                    //  Pen RedPen = new Pen(Color.Red, 10);
                    // Create array of points that define lines to draw.
                    /*Point[] points =
                             {
                            new Point(10,  10),
                            new Point(H-10, W-10),

                             };

                    g.DrawLines(RedPen, points);
                    */
                }
                b.Save(@pathtoemf, ImageFormat.Emf);
                b.Dispose();
                m.Dispose();

                File.Delete(@initialemf);


            }
            
            

        }
        /// <summary>
            /// creer les liens emf dans le fichier clipper
            /// </summary>
            /// <param name="partEntity">part exportt�</param>
            /// <param name="empty_emfFile">liens vide si besoin</param>
            /// <returns></returns>
            private string GetEmfFile(IEntity partEntity, string empty_emfFile)
        {
            try
            {
                string emfFile = "";
               
                //cas g�n�ral
                emfFile = partEntity.GetFieldValueAsString("_DPR_FILENAME") + ".emf";
                
                
                if (string.IsNullOrEmpty(partEntity.GetFieldValueAsString("_DPR_FILENAME")) == false)
                {
                    emfFile = partEntity.GetFieldValueAsString("_DPR_FILENAME") + ".emf";
                }
                else if (string.IsNullOrEmpty(partEntity.GetFieldValueAsString("_FILENAME")) == false)
                {
                    emfFile = partEntity.GetFieldValueAsString("_FILENAME") + ".emf";
                }
                else
                {
                    emfFile = empty_emfFile;
                    CreateEmptyDpr(emfFile.Replace(".emf", ".dpr"));
                    CreateEmptyEmf(emfFile.Replace(".emf", ".dpr.emf"));

                }




                return emfFile;



            }
            catch (Exception ie) { MessageBox.Show(ie.Message); return ""; };
        }

        private void CreateEmptyDpr(string filePath)
        {
            using (System.IO.StreamWriter file =new System.IO.StreamWriter(@filePath))
            {
                file.WriteLine("/ DPR 4.0 R 1");
                file.WriteLine("/ HEADER");
                file.WriteLine("$UNIT = 1");
                file.WriteLine("$THICK = 0");
                file.WriteLine("$ANGLE = 0");
                file.WriteLine("$SYMX = 0");
                file.WriteLine("$SYMY = 0");
                file.WriteLine("$SURFACE = 0");
                file.WriteLine("$SURFEXT = 0");
                file.WriteLine("$PERIMET = 0");
                file.WriteLine("$ATTACHSTD = 0");
                file.WriteLine("$COVERSTD = 0");
                file.WriteLine("$WORKONSTD = 0");
                file.WriteLine("$GRAVITY = 0 0");
                file.WriteLine("$DIMENS = 0 0");
                file.WriteLine("$LIMIT = 0 0 0 0 0 0 0 0");
                file.WriteLine("$MATERIAL =");

                file.Close();
            }
        }

        // Return a metafile with the indicated size.
        private void CreateEmptyEmf(string filename)
        {
            using (var bitmap = new Bitmap(100, 100))
            {
                Bitmap bmp = new Bitmap(78, 78);
                using (Graphics gr = Graphics.FromImage(bmp))
                {
                    gr.Clear(Color.FromKnownColor(KnownColor.Window));
                    
                }
                bmp.Save(@filename);
            }
        }
        private void QuoteSet(ref string file, IQuote quote, string rang, NumberFormatInfo formatProvider)
        {
            IEntity quoteEntity = quote.QuoteEntity;
            IEntity clientEntity = quoteEntity.GetFieldValueAsEntity("_FIRM");

            foreach (IEntity setEntity in quote.QuoteSetList)
            {
                long setQty = setEntity.GetFieldValueAsLong("_QUANTITY");
                if (setQty > 0)
                {
                    long i = 0;
                    string[] data = new string[50];

                    string setReference = null;
                    string setModele = null;
                    GetReference(setEntity, "SET", false, out setReference, out setModele);

                    data[i++] = "ENDEVIS";
                    data[i++] = GetQuoteNumber(quoteEntity); //N� devis
                    data[i++] = EmptyString(clientEntity.GetFieldValueAsString("_EXTERNAL_ID")).ToUpper(); //Code client
                    data[i++] = setReference; //Code pi�ce
                    data[i++] = ""; //Type (non utilis�)
                    data[i++] = FormatDesignation(setEntity.GetFieldValueAsString("_DESCRIPTION")); //D�signation 1
                    data[i++] = FormatDesignation(""); //D�signation 2
                    data[i++] = FormatDesignation(""); //D�signation 3
                    data[i++] = rang; //Rang
                    data[i++] = setReference; //Code pi�ce ou Sous pi�ce (sous rang)
                    data[i++] = FormatDesignation(setEntity.GetFieldValueAsString("_DESCRIPTION")); //D�signation pi�ce ou Sous pi�ce (sous rang)
                    data[i++] = setEntity.Id.ToString(); //N� plan
                    data[i++] = rang; //Niveau rang
                    data[i++] = "3"; //Etat devis
                    data[i++] = ""; //Rep�re
                    data[i++] = "0"; //Origine fourniture
                    data[i++] = "1"; //Qt� dus/ensemble : 1 pour le rang 001
                    data[i++] = "1"; //Qt� totale de l'ensemble : 1 pour le rang 001
                    data[i++] = ""; //Indice plan
                    data[i++] = ""; //Indice gamme
                    data[i++] = ""; //Indice nomenclature
                    data[i++] = ""; //Indice pi�ce
                    data[i++] = ""; //Indice A
                    data[i++] = ""; //Indice B
                    data[i++] = ""; //Indice C
                    data[i++] = ""; //Indice D
                    data[i++] = ""; //Indice E
                    data[i++] = ""; //Indice F
                    data[i++] = "0"; //N� identifiant GED 1
                    data[i++] = "0"; //N� identifiant GED 2
                    data[i++] = "0"; //N� identifiant GED 3
                    data[i++] = "0"; //N� identifiant GED 4
                    data[i++] = "0"; //N� identifiant GED 5
                    data[i++] = "0"; //N� identifiant GED 6
                    data[i++] = "0"; //N� identifiant GED 7
                    data[i++] = "0"; //N� identifiant GED 8
                    data[i++] = "0"; //N� identifiant GED 9
                    data[i++] = "0"; //N� identifiant GED 10
                    data[i++] = ""; //Fichier joint
                    data[i++] = ""; //Date d'injection
                    data[i++] = setModele; //Mod�le
                    data[i++] = ""; //Employ� responsable                
                    WriteData(data, i, ref file);

                    long gaDevisPhase = 0;
                    long nomendvPhase = 0;

                    // Fourniture de l'ensemble
                    IList<IEntity> setSupplyList = new List<IEntity>(quote.GetSetSupplyList(setEntity));
                    QuoteSupply(ref file, quote, 1, setSupplyList, rang, ref gaDevisPhase, ref nomendvPhase, formatProvider, true, setReference, setModele);

                    // Operation de l'ensemble
                    IList<IEntity> setOperationList = new List<IEntity>(quote.GetSetOperationList(setEntity));
                    QuoteOperation(ref file, quote, null, setOperationList, rang, formatProvider, 1, setQty, ref gaDevisPhase, ref nomendvPhase, setReference, setModele, 0.0);

                    // Pi�ces de l'ensemble
                    IEntityList partSetList = setEntity.Context.EntityManager.GetEntityList("_QUOTE_SET_PART", setEntity.EntityType.Key, ConditionOperator.Equal, setEntity.Id);
                    partSetList.Fill(false);
                    long subRang = 1;
                    foreach (IEntity partSet in partSetList)
                    {
                        long partId = partSet.GetFieldValueAsLong("_QUOTE_PART");
                        long partSetQty = partSet.GetFieldValueAsLong("_QUANTITY");

                        IEntity partEntity = quote.QuotePartList.Where(p => p.Id == partId).FirstOrDefault();
                        if (partEntity != null && partSetQty > 0)
                        {
                            QuoteSetPart(ref file, quote, setEntity, partEntity, partSetQty, rang + "/" + subRang.ToString("000"), formatProvider);
                            subRang++;
                        }
                    }
                }
            }
        }
        private void QuoteSetPart(ref string file, IQuote quote, IEntity setEntity, IEntity partEntity, long partSetQty, string rang, NumberFormatInfo formatProvider)
        {
            IEntity quoteEntity = quote.QuoteEntity;
            IEntity clientEntity = quoteEntity.GetFieldValueAsEntity("_FIRM");
            if (partSetQty > 0)
            {
                long i = 0;
                string[] data = new string[50];

                string partReference = null;
                string partModele = null;
                GetReference(partEntity, "PART", false, out partReference, out partModele);

                string setReference = null;
                string setModele = null;
                GetReference(setEntity, "SET", false, out setReference, out setModele);

                data[i++] = "ENDEVIS";
                data[i++] = GetQuoteNumber(quoteEntity); //N� devis
                data[i++] = EmptyString(clientEntity.GetFieldValueAsString("_EXTERNAL_ID")).ToUpper(); //Code client
                data[i++] = setReference; //Code pi�ce
                data[i++] = ""; //Type (non utilis�)
                data[i++] = FormatDesignation(setEntity.GetFieldValueAsString("_DESCRIPTION")); //D�signation 1
                data[i++] = FormatDesignation(""); //D�signation 2
                data[i++] = FormatDesignation(""); //D�signation 3
                data[i++] = rang; //Rang
                data[i++] = partReference; //Code pi�ce ou Sous pi�ce (sous rang)
                data[i++] = FormatDesignation(partEntity.GetFieldValueAsString("_DESCRIPTION")); //D�signation pi�ce ou Sous pi�ce (sous rang)
                data[i++] = partEntity.Id.ToString(); //N� plan
                data[i++] = rang; //Niveau rang
                data[i++] = "3"; //Etat devis
                data[i++] = ""; //Rep�re
                data[i++] = "0"; //Origine fourniture
                data[i++] = partSetQty.ToString(formatProvider); //Qt� dus/ensemble
                data[i++] = "1"; //Qt� totale de l'ensemble
                data[i++] = ""; //Indice plan
                data[i++] = ""; //Indice gamme
                data[i++] = ""; //Indice nomenclature
                data[i++] = ""; //Indice pi�ce
                double weight = partEntity.GetFieldValueAsDouble("_WEIGHT");
                double weightEx = partEntity.GetFieldValueAsDouble("_WEIGHT_EX");
                weight = weight / 1000;
                weightEx = weightEx / 1000;
                data[i++] = weight.ToString("#0.0000", formatProvider); //Indice A
                data[i++] = weightEx.ToString("#0.0000", formatProvider); //Indice B
                data[i++] = "-" + partEntity.Id.ToString(); //Indice C
                data[i++] = ""; //Indice D
                data[i++] = ""; //Indice E
                data[i++] = ""; //Indice F
                data[i++] = "0"; //N� identifiant GED 1
                data[i++] = "0"; //N� identifiant GED 2
                data[i++] = "0"; //N� identifiant GED 3
                data[i++] = "0"; //N� identifiant GED 4
                data[i++] = "0"; //N� identifiant GED 5
                data[i++] = "0"; //N� identifiant GED 6
                data[i++] = "0"; //N� identifiant GED 7
                data[i++] = "0"; //N� identifiant GED 8
                data[i++] = "0"; //N� identifiant GED  9
                data[i++] = "0"; //N� identifiant GED 10
                data[i++] = ""; //Fichier joint
                data[i++] = ""; //Date d'injection
                data[i++] = setModele; //Mod�le
                data[i++] = ""; //Employ� responsable                
                WriteData(data, i, ref file);

                long gaDevisPhase = 0;
                long nomendvPhase = 0;

                // Fournitures de la pi�ce dans l'ensemble
                IList<IEntity> setSupplyList = new List<IEntity>(quote.GetPartSupplyList(partEntity));
                QuoteSupply(ref file, quote, partSetQty, setSupplyList, rang, ref gaDevisPhase, ref nomendvPhase, formatProvider, true, setReference, setModele);

                // Operations de la pi�ce dans l'ensemble
                double totalMaterialPrice = partEntity.GetFieldValueAsDouble("_CORRECTED_MAT_COST");
                long totalPartQty = partEntity.GetFieldValueAsLong("_QUANTITY");
                double materialPrice = totalMaterialPrice / totalPartQty;
                IList<IEntity> partOperationList = new List<IEntity>(quote.GetPartOperationList(partEntity));
                QuoteOperation(ref file, quote, partEntity, partOperationList, rang, formatProvider, partSetQty, partSetQty, ref gaDevisPhase, ref nomendvPhase, setReference, setModele, materialPrice);
            }
        }

        private class GroupedCutOperation
        {
            public string CentreFrais;
            public long GadevisPhase;
            public double UnitPrepTime;
            public double CorrectedUnitPrepTime;
            public double UnitTime;
            public double OpeTime;
            public double UnitCost;
            public string Comments;
        }
        private void QuoteOperation(ref string file, IQuote quote, IEntity partEntity, IEnumerable<IEntity> operationList, string rang, NumberFormatInfo formatProvider, long mainParentQuantity, long parentQty, ref long gadevisPhase, ref long nomendvPhase, string reference, string modele, double materialPrice)
        {
            long cutGadevisPhase = 0;
            IEntity quoteEntity = quote.QuoteEntity;
            IEntity clientEntity = quoteEntity.GetFieldValueAsEntity("_FIRM");

            // Operation
            AQuoteOperation(ref file, quote, partEntity, operationList, ref cutGadevisPhase, rang, formatProvider, mainParentQuantity, parentQty, ref gadevisPhase, ref nomendvPhase, reference, modele);

            #region Gestion de la matiere

            if (materialPrice > 0)
            {
                IEntity materialEntity = partEntity.GetFieldValueAsEntity("_MATERIAL");
                string codeArticleMaterial = materialEntity.GetFieldValueAsString("_CLIPPER_CODE_ARTICLE");

                long i = 0;
                string[] data = new string[50];

                nomendvPhase = nomendvPhase + 10;
                i = 0;
                data = new string[50];

                if (string.IsNullOrEmpty(codeArticleMaterial))
                {
                    data[i++] = "NOMENDV";
                    data[i++] = GetQuoteNumber(quoteEntity); //Code devis
                    data[i++] = reference; //Code pi�ce
                    data[i++] = rang; //Rang
                    data[i++] = nomendvPhase.ToString(formatProvider); //Phase
                    data[i++] = ""; //Rep�re
                    data[i++] = "MATIERE"; //Code article
                    data[i++] = "MATIERE"; //D�signation 1
                    data[i++] = ""; //D�signation 2
                    data[i++] = ""; //D�signation 3
                    data[i++] = ""; //Temps de r�appro
                    data[i++] = mainParentQuantity.ToString(); //Qt�
                    data[i++] = materialPrice.ToString("#0.0000", formatProvider); //Px article ou Px/Kg
                    data[i++] = materialPrice.ToString("#0.0000", formatProvider); //Prix total
                    data[i++] = ""; //Code Fournisseur
                    data[i++] = ""; //2sd fournisseur
                    data[i++] = "1"; //Type
                    data[i++] = ""; //Prix constant
                    data[i++] = ""; //Poids t�le ou article
                    data[i++] = "DEVIS"; //Famille
                    data[i++] = ""; //N� tarif de Clipper
                    data[i++] = ""; //Observation
                    data[i++] = ""; //Observation interne
                    data[i++] = ""; //Observation d�bit
                    data[i++] = ""; //Val D�bit 1
                    data[i++] = ""; //Val D�bit 2
                    data[i++] = ""; //Qt� D�bit
                    data[i++] = ""; //Nb pc/d�bit ou d�bit/pc
                    data[i++] = ""; //Lien avec la phase de gamme
                    data[i++] = ""; //Unite de quantit�
                    data[i++] = ""; //Unit� de prix
                    data[i++] = ""; //Coef Unite
                    data[i++] = ""; //Coef Prix
                    data[i++] = modele; //Prix constant ??? semble plutot correcpondre au Mod�le
                    data[i++] = "0"; //Mod�le ??? semble plutot correcpondre au Prix constant
                    data[i++] = ""; //Qt� constant
                    data[i++] = cutGadevisPhase.ToString(formatProvider); //Magasin ???? erreur

                    WriteData(data, i, ref file);
                }
                else
                {
                    double surface = partEntity.GetFieldValueAsDouble("_SURFACE");
                    data[i++] = "NOMENDVALMA";
                    data[i++] = GetQuoteNumber(quoteEntity); //Code devis
                    data[i++] = reference; //Code pi�ce
                    data[i++] = modele; //Mod�le
                    data[i++] = rang; //Rang
                    data[i++] = nomendvPhase.ToString(formatProvider); //Phase
                    data[i++] = codeArticleMaterial; //Code article
                    data[i++] = surface.ToString("#0.0000", formatProvider); //Surface pour faire une pi�ce
                    data[i++] = materialPrice.ToString("#0.0000", formatProvider); //Prix total pour faire une pi�ce

                    WriteData(data, i, ref file);
                }
            }

            #endregion
        }
        private void AQuoteOperation(ref string file, IQuote quote, IEntity partEntity, IEnumerable<IEntity> operationList, ref long cutGadevisPhase, string rang, NumberFormatInfo formatProvider, long mainParentQuantity, long parentQty, ref long gadevisPhase, ref long nomendvPhase, string reference, string modele)
        {
            IEntity quoteEntity = quote.QuoteEntity;
            IEntity clientEntity = quoteEntity.GetFieldValueAsEntity("_FIRM");

            bool fixeCostPartExported = false;
            if (partEntity != null)
            {
                if (_FixeCostPartExportedList.ContainsKey(partEntity.Id))
                    fixeCostPartExported = true;
                else
                    _FixeCostPartExportedList.Add(partEntity.Id, partEntity.Id);
            }

            #region Operation de coupe

            IList<IEntity> cutOperationList = new List<IEntity>(operationList.Where(p => (quote as Quote).GetOperationType(p) == OperationType.Cut));
            IDictionary<string, GroupedCutOperation> groupedCutOperationList = new Dictionary<string, GroupedCutOperation>();

            foreach (IEntity operationEntity in cutOperationList)
            {
                IEntity subOperationEntity = operationEntity.ImplementedEntity;
                string centreFrais = CreateTransFile.GetClipperCentreFrais(subOperationEntity);

                long totalOperationQty = operationEntity.GetFieldValueAsLong("_PARENT_QUANTITY");
                if (totalOperationQty == 0) totalOperationQty = 1;

                string comments = operationEntity.GetFieldValueAsString("_COMMENTS");

                GroupedCutOperation groupedCutOperation;
                if (groupedCutOperationList.TryGetValue(centreFrais, out groupedCutOperation) == false)
                {
                    groupedCutOperation = new GroupedCutOperation();
                    groupedCutOperation.CentreFrais = centreFrais;
                    gadevisPhase = gadevisPhase + 10;
                    groupedCutOperation.GadevisPhase = gadevisPhase;
                    groupedCutOperation.Comments = comments;
                    groupedCutOperationList.Add(centreFrais, groupedCutOperation);
                }

                if (operationEntity.GetFieldValueAsBoolean("_FIXE_COST"))
                {
                    groupedCutOperation.UnitPrepTime += operationEntity.GetFieldValueAsDouble("_CORRECTED_PREPARATION_TIME") / 3600;
                    groupedCutOperation.UnitPrepTime += operationEntity.GetFieldValueAsDouble("_CORRECTED_CYCLE_TIME") / 3600;
                    groupedCutOperation.UnitCost += operationEntity.GetFieldValueAsDouble("_IN_COST");
                }
                else
                {
                    groupedCutOperation.UnitPrepTime += operationEntity.GetFieldValueAsDouble("_CORRECTED_PREPARATION_TIME") / 3600;
                    groupedCutOperation.UnitTime += operationEntity.GetFieldValueAsDouble("_CORRECTED_CYCLE_TIME") / 3600;
                    groupedCutOperation.UnitCost += operationEntity.GetFieldValueAsDouble("_IN_COST");
                }

                groupedCutOperation.CorrectedUnitPrepTime = groupedCutOperation.UnitPrepTime;
                if (fixeCostPartExported) groupedCutOperation.CorrectedUnitPrepTime = 0;

                if (totalOperationQty != 0)
                    groupedCutOperation.OpeTime = groupedCutOperation.UnitTime + groupedCutOperation.UnitPrepTime / totalOperationQty;

                if (cutGadevisPhase == 0)
                    cutGadevisPhase = gadevisPhase;
            }

            foreach (GroupedCutOperation groupedCutOperation in groupedCutOperationList.Values)
            {
                long i = 0;
                string[] data = new string[50];

                data[i++] = "GADEVIS";
                data[i++] = rang; //Rang
                data[i++] = ""; //inutilis�
                data[i++] = groupedCutOperation.GadevisPhase.ToString(formatProvider); //Phase

                data[i++] = "COUPE"; //D�signation 1
                data[i++] = FormatDesignation(""); //D�signation 2
                data[i++] = FormatDesignation(""); //D�signation 3
                data[i++] = FormatDesignation(""); //D�signation 4
                data[i++] = FormatDesignation(""); //D�signation 5
                data[i++] = FormatDesignation(""); //D�signation 6
                data[i++] = groupedCutOperation.CentreFrais; //Centre de frais 

                double tpsPrep = groupedCutOperation.CorrectedUnitPrepTime;
                double tpsUnit = mainParentQuantity * groupedCutOperation.UnitTime;
                ///
                data[i++] = tpsPrep.ToString("#0.0000", formatProvider); //Tps Prep
                data[i++] = tpsUnit.ToString("#0.0000", formatProvider); //Tps Unit (heure)
                ///
                double unitCost = groupedCutOperation.UnitCost;
                data[i++] = (unitCost * mainParentQuantity).ToString("#0.0000", formatProvider); //Co�t Op�ration
                ///
                double hourlyCost = 0;
                if (((tpsPrep / parentQty) + tpsUnit != 0))
                    hourlyCost = unitCost / ((tpsPrep / parentQty) + tpsUnit);
                data[i++] = hourlyCost.ToString("#0.0000", formatProvider); //Taux horaire (/heure)
                ///
                data[i++] = GetFieldDate(quoteEntity, "_CREATION_DATE"); //Date
                data[i++] = GetQuoteNumber(quoteEntity); //N� devis
                data[i++] = ""; //Nom fichier joint
                data[i++] = "0"; //N� identifiant GED 1
                data[i++] = "0"; //N� identifiant GED 2
                data[i++] = "0"; //N� identifiant GED 3
                data[i++] = "0"; //N� identifiant GED 4
                data[i++] = "0"; //N� identifiant GED 5
                data[i++] = "0"; //N� identifiant GED 6
                data[i++] = "0"; //Niveau du rang
                data[i++] = groupedCutOperation.Comments; //Observations
                data[i++] = ""; //Lien avec la phase de nomenclature
                data[i++] = ""; //Date derni�re modif
                data[i++] = ""; //Employ� modif
                data[i++] = ""; //Niveau de blocage
                data[i++] = ""; //Taux homme TP
                data[i++] = ""; //Taux homme TU
                data[i++] = ""; //Nb pers TP
                data[i++] = ""; //Nb Pers TU
                WriteData(data, i, ref file);
            }

            #endregion

            #region Operation autre que coupe

            foreach (IEntity operationEntity in operationList)
            {
                if ((quote as Quote).GetOperationType(operationEntity) == OperationType.Cut) continue;

                long i = 0;
                string[] data = new string[50];

                gadevisPhase = gadevisPhase + 10;
                IEntity subOperationEntity = operationEntity.ImplementedEntity;

                data[i++] = "GADEVIS";
                data[i++] = rang; //Rang
                data[i++] = ""; //inutilis�
                data[i++] = gadevisPhase.ToString(formatProvider); //Phase

                if ((quote as Quote).GetOperationType(operationEntity) == OperationType.Stt)
                    data[i++] = FormatDesignation("SOUS-TRAITANCE"); //D�signation 1
                else
                    data[i++] = FormatDesignation(operationEntity.GetFieldValueAsString("_NAME")); //D�signation 1

                data[i++] = FormatDesignation(""); //D�signation 2
                data[i++] = FormatDesignation(""); //D�signation 3
                data[i++] = FormatDesignation(""); //D�signation 4
                data[i++] = FormatDesignation(""); //D�signation 5
                data[i++] = FormatDesignation(""); //D�signation 6
                data[i++] = CreateTransFile.GetClipperCentreFrais(subOperationEntity); //Centre de frais 

                double unitPrepTime = operationEntity.GetFieldValueAsDouble("_CORRECTED_PREPARATION_TIME") / 3600;
                double unitTime = operationEntity.GetFieldValueAsDouble("_CORRECTED_CYCLE_TIME") / 3600;

                double correctedUnitPrepTime = unitPrepTime;
                if (fixeCostPartExported) correctedUnitPrepTime = 0;

                double tpsPrep = 0;
                double tpsUnit = 0;

                if (operationEntity.GetFieldValueAsBoolean("_FIXE_COST"))
                {
                    tpsPrep = (mainParentQuantity * unitTime + correctedUnitPrepTime);
                    tpsUnit = 0;
                }
                else
                {
                    tpsPrep = correctedUnitPrepTime;
                    tpsUnit = (mainParentQuantity * unitTime);
                }
                data[i++] = tpsPrep.ToString("#0.0000", formatProvider); //Tps Prep (heure)
                data[i++] = tpsUnit.ToString("#0.0000", formatProvider); //Tps Unit (heure)

                double unitCost = 0;
                if ((quote as Quote).GetOperationType(operationEntity) == OperationType.Stt)
                    unitCost = 0;
                else
                    unitCost = (operationEntity.GetFieldValueAsDouble("_IN_COST") * mainParentQuantity);
                data[i++] = unitCost.ToString("#0.0000", formatProvider); //Co�t Op�ration

                double hourlyCost = 0;
                if (((tpsPrep / parentQty) + tpsUnit != 0))
                    hourlyCost = unitCost / ((tpsPrep / parentQty) + tpsUnit);
                data[i++] = hourlyCost.ToString("#0.0000", formatProvider); //Taux horaire (/heure)

                data[i++] = GetFieldDate(quoteEntity, "_CREATION_DATE"); //Date
                data[i++] = GetQuoteNumber(quoteEntity); //N� devis
                data[i++] = ""; //Nom fichier joint
                data[i++] = "0"; //N� identifiant GED 1
                data[i++] = "0"; //N� identifiant GED 2
                data[i++] = "0"; //N� identifiant GED 3
                data[i++] = "0"; //N� identifiant GED 4
                data[i++] = "0"; //N� identifiant GED 5
                data[i++] = "0"; //N� identifiant GED 6
                data[i++] = "0"; //Niveau du rang
                data[i++] = operationEntity.GetFieldValueAsString("_COMMENTS"); ; //Observations
                data[i++] = ""; //Lien avec la phase de nomenclature
                data[i++] = ""; //Date derni�re modif
                data[i++] = ""; //Employ� modif
                data[i++] = ""; //Niveau de blocage
                data[i++] = ""; //Taux homme TP
                data[i++] = ""; //Taux homme TU

                if (subOperationEntity.EntityType.Key == "_FOLD_QUOTE_OPE")
                {
                    long nbWorker = subOperationEntity.GetFieldValueAsLong("_NB_WORKER");
                    data[i++] = nbWorker.ToString(); //Nb pers TP
                    data[i++] = nbWorker.ToString(); //Nb Pers TU
                }
                else
                {
                    data[i++] = ""; //Nb pers TP
                    data[i++] = ""; //Nb Pers TU
                }
                WriteData(data, i, ref file);

                #region Ajout NOMENDV (nomemclature) pour operation de Sous-traintance

                if ((quote as Quote).GetOperationType(operationEntity) == OperationType.Stt)
                {
                    nomendvPhase = nomendvPhase + 10;
                    i = 0;
                    data = new string[50];

                    data[i++] = "NOMENDV";
                    data[i++] = GetQuoteNumber(quoteEntity); //Code devis
                    data[i++] = reference; //Code pi�ce
                    data[i++] = rang; //Rang
                    data[i++] = nomendvPhase.ToString(formatProvider); //Phase
                    data[i++] = ""; //Rep�re
                    data[i++] = EmptyString(operationEntity.GetFieldValueAsString("_NAME")); //Code article
                    data[i++] = FormatDesignation(operationEntity.GetFieldValueAsString("_NAME")); //D�signation 1
                    data[i++] = FormatDesignation(""); //D�signation 2
                    data[i++] = FormatDesignation(""); //D�signation 3
                    data[i++] = ""; //Temps de r�appro

                    data[i++] = mainParentQuantity.ToString(); //Qt�
                    data[i++] = operationEntity.GetFieldValueAsDouble("_IN_COST").ToString("#0.0000", formatProvider); //Px article ou Px/Kg
                    data[i++] = operationEntity.GetFieldValueAsDouble("_IN_COST").ToString("#0.0000", formatProvider); //Prix total

                    data[i++] = ""; //Code Fournisseur
                    data[i++] = ""; //2sd fournisseur
                    data[i++] = "3"; //Type : 3 pour sous-traitance
                    data[i++] = ""; //Prix constant
                    data[i++] = ""; //Poids t�le ou article
                    data[i++] = CreateTransFile.GetSttFamily(subOperationEntity); //Famille
                    data[i++] = ""; //N� tarif de Clipper
                    data[i++] = ""; //Observation
                    data[i++] = ""; //Observation interne
                    data[i++] = ""; //Observation d�bit
                    data[i++] = ""; //Val D�bit 1
                    data[i++] = ""; //Val D�bit 2
                    data[i++] = ""; //Qt� D�bit
                    data[i++] = ""; //Nb pc/d�bit ou d�bit/pc
                    data[i++] = ""; //Lien avec la phase de gamme
                    data[i++] = ""; //Unite de quantit�
                    data[i++] = ""; //Unit� de prix
                    data[i++] = ""; //Coef Unite
                    data[i++] = ""; //Coef Prix
                    data[i++] = modele; //Prix constant ??? semble plutot correcpondre au Mod�le
                    data[i++] = "0"; //Mod�le ??? semble plutot correcpondre au Prix constant
                    data[i++] = ""; //Qt� constant
                    data[i++] = gadevisPhase.ToString(formatProvider); //Magasin ???? erreur

                    WriteData(data, i, ref file);
                }

                #endregion
            }

            #endregion
        }
        private void QuoteSupply(ref string file, IQuote quote, long parentQty, IEnumerable<IEntity> supplyList, string rang, ref long gadevisPhase, ref long nomendvPhase, NumberFormatInfo formatProvider, bool includeHeader, string reference, string modele)
        {
            IEntity quoteEntity = quote.QuoteEntity;

            long i = 0;
            string[] data = new string[50];

            if (includeHeader)
            {
                gadevisPhase = gadevisPhase + 10;
                data[i++] = "GADEVIS";
                data[i++] = rang; //Rang
                data[i++] = ""; //inutilis�
                data[i++] = gadevisPhase.ToString(formatProvider); //Phase
                data[i++] = FormatDesignation("ACHAT NOMENCLATURE"); //D�signation 1
                data[i++] = FormatDesignation(""); //D�signation 2
                data[i++] = FormatDesignation(""); //D�signation 3
                data[i++] = FormatDesignation(""); //D�signation 4
                data[i++] = FormatDesignation(""); //D�signation 5
                data[i++] = FormatDesignation(""); //D�signation 6
                data[i++] = "NOMEN"; //Centre de frais

                data[i++] = "0"; //Tps Prep
                data[i++] = "0"; //Tps Unit
                data[i++] = "0"; //Co�t Op�ration
                data[i++] = "0"; //Taux horaire
                data[i++] = GetFieldDate(quoteEntity, "_CREATION_DATE"); //Date
                data[i++] = GetQuoteNumber(quoteEntity); //N� devis
                data[i++] = ""; //Nom fichier joint
                data[i++] = "0"; //N� identifiant GED 1
                data[i++] = "0"; //N� identifiant GED 2
                data[i++] = "0"; //N� identifiant GED 3
                data[i++] = "0"; //N� identifiant GED 4
                data[i++] = "0"; //N� identifiant GED 5
                data[i++] = "0"; //N� identifiant GED 6
                data[i++] = "0"; //Niveau du rang
                data[i++] = ""; //Observations
                data[i++] = ""; //Lien avec la phase de nomenclature
                data[i++] = ""; //Date derni�re modif
                data[i++] = ""; //Employ� modif
                data[i++] = ""; //Niveau de blocage
                data[i++] = ""; //Taux homme TP
                data[i++] = ""; //Taux homme TU
                data[i++] = ""; //Nb pers TP
                data[i++] = ""; //Nb Pers TU
                WriteData(data, i, ref file);
            }

            foreach (IEntity supplyEntity in supplyList)
            {
                IEntity supplyTypeEntity = supplyEntity.GetFieldValueAsEntity("_SUPPLY");
                double doubleSupplyQty = supplyEntity.GetFieldValueAsDouble("_DOUBLE_QUANTITY");
                long supplyQty = Convert.ToInt64(doubleSupplyQty);
                if (supplyQty > 0)
                {
                    nomendvPhase = nomendvPhase + 10;
                    i = 0;
                    data = new string[50];

                    data[i++] = "NOMENDV";
                    data[i++] = GetQuoteNumber(quoteEntity); //Code devis
                    data[i++] = reference; //Code pi�ce
                    data[i++] = rang; //Rang
                    data[i++] = nomendvPhase.ToString(formatProvider); //Phase
                    data[i++] = ""; //Rep�re
                    data[i++] = EmptyString(supplyTypeEntity.GetFieldValueAsString("_REFERENCE")); ; //Code article
                    data[i++] = FormatDesignation(supplyTypeEntity.GetFieldValueAsString("_DESIGNATION")); //D�signation 1
                    data[i++] = FormatDesignation(""); //D�signation 2
                    data[i++] = FormatDesignation(""); //D�signation 3
                    data[i++] = ""; //Temps de r�appro
                    data[i++] = (supplyQty * parentQty).ToString(); //Qt�
                    data[i++] = (supplyEntity.GetFieldValueAsDouble("_IN_COST") / supplyQty).ToString("#0.0000", formatProvider); //Px article ou Px/Kg
                    data[i++] = supplyEntity.GetFieldValueAsDouble("_IN_COST").ToString("#0.0000", formatProvider); //Prix total
                    data[i++] = ""; //Code Fournisseur
                    data[i++] = ""; //2sd fournisseur
                    data[i++] = "1"; //Type
                    data[i++] = ""; //Prix constant
                    data[i++] = ""; //Poids t�le ou article
                    data[i++] = "DEVIS"; //Famille
                    data[i++] = ""; //N� tarif de Clipper
                    data[i++] = supplyEntity.GetFieldValueAsString("_COMMENTS") ; //Observation
                    data[i++] = ""; //Observation interne
                    data[i++] = ""; //Observation d�bit
                    data[i++] = ""; //Val D�bit 1
                    data[i++] = ""; //Val D�bit 2
                    data[i++] = ""; //Qt� D�bit
                    data[i++] = ""; //Nb pc/d�bit ou d�bit/pc
                    data[i++] = ""; //Lien avec la phase de gamme
                    data[i++] = ""; //Unite de quantit�
                    data[i++] = ""; //Unit� de prix
                    data[i++] = ""; //Coef Unite
                    data[i++] = ""; //Coef Prix
                    data[i++] = modele; //Prix constant ??? semble plutot correcpondre au Mod�le
                    data[i++] = "0"; //Mod�le ??? semble plutot correcpondre au Prix constant
                    data[i++] = ""; //Qt� constant
                    data[i++] = gadevisPhase.ToString(formatProvider); //Magasin ???? erreur

                    WriteData(data, i, ref file);
                }
            }
        }

        #region Export Tools

        private static void WriteData(string[] data, long nbItem, ref string file)
        {
            string stringData = data[0];
            for (long i = 1; i < nbItem; i++)
            {
                stringData = stringData + "�" + data[i];
            }
            stringData = stringData + "�" + Environment.NewLine;
            file = file + stringData;
        }
        private static string EmptyString(string s)
        {
            if (s == null)
                return "";
            else
                return s.Trim();
        }

        internal static string GetClipperCentreFrais(IEntity subQuoteOperation)
        {
            IParameterSet parameterSet = null;
            IParameterSetLink parameterSetLink = null;

            string parameterSetKey = null;
            IField machineField = null;
            string centreFrais = "";

            if (subQuoteOperation.EntityType.Key == "_SIMPLE_QUOTE_OPE")
            {
                IEntity opertationType = subQuoteOperation.GetFieldValueAsEntity("_SIMPLE_OPE_TYPE");
                if (opertationType != null)
                {
                    IEntity centreFraisEntity = opertationType.GetFieldValueAsEntity("_CENTRE_FRAIS");
                    if (centreFraisEntity != null)
                        centreFrais = centreFraisEntity.GetFieldValueAsString("_CODE");
                }
            }
            else
            {
                if (subQuoteOperation.EntityType.TryGetField("_MACHINE", out machineField))
                    parameterSetKey = subQuoteOperation.GetFieldValueAsString("_MACHINE");

                if (parameterSetKey == null)
                {
                    if (subQuoteOperation.EntityType.ParameterSetLinkListAsParameterSet.Count() == 1)
                        parameterSet = subQuoteOperation.EntityType.ParameterSetLinkListAsParameterSet.First();
                }

                if (parameterSetKey != null)
                {
                    if (subQuoteOperation.EntityType.ParameterSetLinkList.TryGetValue(parameterSetKey, out parameterSetLink))
                        parameterSet = parameterSetLink.ParameterSet;
                }

                if (parameterSet != null)
                {
                    IParameter parameter = null;
                    if (parameterSet.ParameterList.TryGetValue("_CENTRE_FRAIS", out parameter))
                        centreFrais = subQuoteOperation.Context.ParameterSetManager.GetParameterValue(parameter).GetValueAsString();
                }
            }
            return centreFrais;
        }
        private static string GetSttFamily(IEntity subQuoteOperation)
        {
            string family = "";
            if (subQuoteOperation.EntityType.Key == "_SUB_QUOTE_OPE")
            {
                IEntity opertationType = subQuoteOperation.GetFieldValueAsEntity("_SUBCONTRACTING_OPE_TYPE");
                if (opertationType != null)
                {
                    family = opertationType.GetFieldValueAsString("_FAMILY");
                }
            }

            return family;
        }
        private static string GetTransport(IEntity quoteEntity)
        {
            long transportPaymentMode = quoteEntity.GetFieldValueAsLong("_TRANSPORT_PAYMENT_MODE");
            if (transportPaymentMode == 0) // Franco
                return "2";
            else if (transportPaymentMode == 1) // Facture
                return "4";
            else if (transportPaymentMode == 2) // Depart
                return "5";
            else
                return "1";
        }
        private void GetReference(IEntity entity, string prefix, bool doModel, out string reference, out string modele)
        {
            string initalRefernce = EmptyString(entity.GetFieldValueAsString("_REFERENCE")).ToUpper().Trim();
            reference = null;

            KeyValuePair<string, string> t;
            if (_ReferenceIdList.TryGetValue(entity, out t))
            {
                reference = t.Key;
                modele = t.Value;
            }
            else
            {
                if (_ReferenceList.TryGetValue(initalRefernce, out reference))
                {
                    long longModele;
                    longModele = _ReferenceListCount[initalRefernce];
                    if (doModel)
                    {
                        longModele++;
                        _ReferenceListCount.Remove(initalRefernce);
                        _ReferenceListCount.Add(initalRefernce, longModele);
                    }
                    modele = longModele.ToString();
                }
                else
                {
                    reference = initalRefernce;
                    if (reference == "")
                        reference = prefix + entity.GetFieldValueAsLong("_NUMBER").ToString();

                    reference = reference.Substring(0, Math.Min(reference.Length, 30));
                    string baseReference = reference;

                    long i = 1;
                    while (_ReferenceList.Values.Contains(reference))
                    {
                        string index = " - " + i.ToString();
                        reference = baseReference.Substring(0, Math.Min(baseReference.Length, 30 - index.Length)) + index;
                        i++;
                    }

                    _ReferenceList.Add(initalRefernce, reference);
                    _ReferenceListCount.Add(initalRefernce, 0);
                    modele = "0";
                }
                _ReferenceIdList.Add(entity, new KeyValuePair<string, string>(reference, modele));
            }
        }
        private static string FormatDesignation(string designation)
        {
            return designation;
        }
        private static string GetFieldDate(IEntity quoteEntity, string fieldKey)
        {
            try
            {
                return quoteEntity.GetFieldValueAsDateTime(fieldKey).ToString("yyyyMMdd");
            }
            catch
            {
                return "";
            }

        }
        private string GetQuoteNumber(IEntity quoteEntity)
        {
            long offset = quoteEntity.Context.ParameterSetManager.GetParameterValue("_EXPORT", "_CLIPPER_QUOTE_NUMBER_OFFSET").GetValueAsLong();

            return (quoteEntity.GetFieldValueAsLong("_INC_NO") + offset).ToString();
        }

        #endregion


        


    }
    #endregion
    ///exception
    ///
    #region exception

    /// <summary>
    /// cas des devis non clos
    /// </summary>
    internal class UnvalidatedQuoteStatus : Exception
    {
        public UnvalidatedQuoteStatus(string message ) : base(message)
        {
            MessageBox.Show(base.Message, string.Format("Probleme sur le devis :" ,"", DateTime.Now.ToLongTimeString()), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// cas des devis non clos
    /// </summary>
    internal class UnvalidatedQuoteConfigurations : Exception
    {
        public UnvalidatedQuoteConfigurations(string message) : base(message)
        {
            MessageBox.Show(base.Message, string.Format("Probleme de configuration d' AlmaCam :", "", DateTime.Now.ToLongTimeString()), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }




    class AF_Export_Devis_Clipper_Log
    {
        static protected ILog log = LogManager.GetLogger("almaCam");
        static void logInit()
        {
            FileInfo fi = new FileInfo("log4net.xml");
            log4net.Config.XmlConfigurator.Configure(fi);
            log4net.GlobalContext.Properties["host"] = Environment.MachineName;

        }
        
    }
   
    #endregion
}