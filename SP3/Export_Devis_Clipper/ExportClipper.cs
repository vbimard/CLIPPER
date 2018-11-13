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
        /// creer les dpr du devis associés et copie eventuellement les dpr dans un autre dossier de destination 
        /// 
        /// </summary>
        /// <param name="iquote">iquotye a transferer</param>
        /// <param name="CustomDestinationPath">Laisser vide si pas de necessité ; chemin de copie vers un autre dossier (besoin oxytemps pour les fichier dpr par exemple) </param>
        /// <returns></returns>
        public static Dictionary<string, string> ExportDprFiles(IQuote iquote,string CustomDestinationPath)
        {
            Dictionary<string, string> filelist= new Dictionary<string, string>();

            try {

               
                IEntity quote;
                //recupe de l'entité quote
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


                    throw new  UnvalidatedQuoteStatus("Le devis "+iquote.QuoteEntity.Id.ToString() + " n'est pas visible dans les devis envoyés ou clos ");

                   
                }
               
                        return rst;
             


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
        /// export des données de devis
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
                    throw new UnvalidatedQuoteStatus("Le devis "+iquote.QuoteEntity.Id.ToString()+" n'est pas visible dans les devis envoyés ou clos.");
                    
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
            /// verification des paramétrage des prix
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
                   
                            MessageBox.Show("Certaines matieres n'ont pas de code article, il y a un risque de fonctionnement degradé de l'export.");
                            break;
                        }
                    } 

                }

                ///verificatiion du paramétrage des prix
                ///prix par matier epaisseur
                IParameterValue parametrageprix;
                string parametersetkey = "_QUOTE";
                string parametre_name = "_MAT_COST_BY_MATERIAL";
                contextlocal.ParameterSetManager.TryGetParameterValue(parametersetkey, parametre_name, out parametrageprix);
                    
                if ( (bool) parametrageprix.Value == false)
                {
                    throw new UnvalidatedQuoteConfigurations("Mauvais paramétrage des prix, le mode prix epaisseur doit etre coché");
                    
                }


                ///verification des chemins
                ///
                ///
               //   On recupere les parametres d'export'

                string Export_GP_Directory = contextlocal.ParameterSetManager.GetParameterValue("_EXPORT", "_EXPORT_GP_DIRECTORY").GetValueAsString();
                string Export_DPR_Directory = contextlocal.ParameterSetManager.GetParameterValue("_EXPORT", "_ACTCUT_DPR_DIRECTORY").GetValueAsString();

                if (string.IsNullOrEmpty(Export_GP_Directory)) { throw new UnvalidatedQuoteConfigurations("Le chemin d'export des devis n'est pas defini, l'export va etre annulé.");  }
                else { _PathList.Add("Export_GP_Directory", Export_GP_Directory);  }

                if (!string.IsNullOrEmpty(Export_DPR_Directory)) { _PathList.Add("Export_DPR_Directory", Export_DPR_Directory);  }
                    
       

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

                //verification de l'integrité des données
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
        /// creer les dpr du devis associés et copie eventuellement les dpr dans un autre dossier de destination 
        /// export les dpr dans une directory custom au besoin
        /// </summary>
        /// <param name="iquote">iquotye a transferer</param>
        /// <param name="CustomDestinationPath">Laisser vide si pas de necessité ; chemin de copie vers un autre dossier (besoin oxytemps pour les fichier dpr par exemple) </param>
        /// <returns></returns>
        public Dictionary<string, string> ExportDprFiles(IQuote iquote, string CustomDestinationPath)
        {
            Dictionary<string, string> filelist = new Dictionary<string, string>();

            try
            {


                IEntity quote;
                //recupe de l'entité quote
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
                                ///on pourrait ne rine fair si le fichier exist deja
                                if (Directory.Exists(CustomDestinationPath) & File.Exists(pathtofile))
                                {
                                    File.Copy(pathtofile, CustomDestinationPath + "\\" + Path.GetFileName(pathtofile),false);
                                    //on met a jour la liste avecle custom path
                                    filelist.Add(partname, CustomDestinationPath + "\\" + Path.GetFileName(pathtofile));
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
        /// export pour debugage
        /// </summary>
        /// <param name="Context"></param>
        /// <param name="QuoteList"></param>
        /// <param name="CustomExportDirectory"></param>
        /// <returns></returns>
        public bool Export(IContext Context, IEnumerable<IQuote> QuoteList, string CustomExportDirectory)
        {
            try {
                bool rst = false;
                string ExportDirectory="";
                //verification de l'integrité des données
                //preparing export
                
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

                    if (Validate_Quote(quote)) {
                        FullPath_FileName = Path.Combine(ExportDirectory, FileName);
                        rst = InternalExport(Context, QuoteList, FullPath_FileName);

                    }
                    else
                    {
                        throw new UnvalidatedQuoteStatus("Probleme detecté sur le devis " + quote.QuoteEntity.Id);
                        //Environement.exit(0)
                        //return false;
                    }


                } 

                return rst;


            }
            catch (UnvalidatedQuoteStatus msg)
            {
                Environment.Exit(0); return false;
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

                //verification de l'integrité des données
                
                //check_database_Integerity
                Validate_Context(contextlocal);
                //preparing export
                PrepareExportDirectory(contextlocal);

                if (QuoteList.Count() == 1)
                {
                    if (string.IsNullOrEmpty(FileName))
                    {
                        IQuote quote = QuoteList.FirstOrDefault();
                        //verification du devis 
                        if (Validate_Quote(quote)) { 
                        FileName = "Trans_" + QuoteList.First().QuoteInformation.IncNo.ToString("####") + ".txt";
                        }
                        else
                        {
                            Environment.Exit(0);

                        }


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
            catch (UnvalidatedQuoteStatus)
             {
                return false;
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
                //export de l' entetes
                QuoteHeader(ref file, quote, formatProvider);
                //export des offres
                QuoteOffre(ref file, quote, formatProvider);
                //export des part
                QuotePart(ref file, quote, "001", formatProvider);
                //export des ensembles
                QuoteSet(ref file, quote, "001", formatProvider);

                file = file + "Fin d'enregistrement OK¤" + Environment.NewLine;
            }
            file = file + "Fin du fichier OK";

            File.AppendAllText(clipperFileName, file, Encoding.Default);
            return true;
        }
        /// <summary>
        /// export de l'entet des devis
        /// </summary>
        /// <param name="file">nom du fichier de sortie</param>
        /// <param name="quote">Objet Iquote</param>
        /// <param name="formatProvider">gestion des format deciamaux...</param>
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
            data[i++] = GetQuoteNumber(quoteEntity); //N° devis
            string indice; //max15 char
            indice=EmptyString(quoteEntity.GetFieldValueAsString("_REFERENCE"));
            if (indice.Length > 15)
            {
                indice = indice.Substring(0, 15);
            }

            data[i++] = indice; //Indice
            IField field;
            string internal_comment = ""; //commentaires internes
            string external_comment =""; //commentaires externe

            if (quoteEntity.EntityType.TryGetField("_CLIENT_ORDER_NUMBER", out field))
                data[i++] = EmptyString(quoteEntity.GetFieldValueAsString("_CLIENT_ORDER_NUMBER")).ToUpper(); //Repère commercial interne
            else
            //commentaires
            external_comment = "";  //
            internal_comment = FormatComment(EmptyString(quoteEntity.GetFieldValueAsString("_COMMENTS"))); //commentaires internes des devis

            data[i++] = ""; //Repère commercial interne
            data[i++] = EmptyString(clientEntity.GetFieldValueAsString("_EXTERNAL_ID")).ToUpper(); //Code client
            data[i++] = EmptyString(clientEntity.GetFieldValueAsString("_NAME")); //Nom client
            data[i++] = EmptyString(quoteEntity.GetFieldValueAsString("_DELIVERY_ADDRESS")); //Ligne adresse 1
            data[i++] = EmptyString(quoteEntity.GetFieldValueAsString("_DELIVERY_ADDRESS2")); //Ligne adresse 2
            data[i++] = ""; //Ligne adresse 3
            data[i++] = EmptyString(quoteEntity.GetFieldValueAsString("_DELIVERY_POSTCODE")); //Code postal
            data[i++] = EmptyString(quoteEntity.GetFieldValueAsString("_DELIVERY_CITY")); //Ville
            data[i++] = GetFieldDate(quoteEntity, "_CREATION_DATE"); //Date devis client
            data[i++] = GetFieldDate(quoteEntity, "_CREATION_DATE"); //Date enregistrement devis
            data[i++] = ""; //Code activité
            data[i++] = "1"; //Etat
            data[i++] = ""; //N° revue de contrat
            data[i++] = "1"; //Organisme de contrôle 16
            data[i++] = userCode; //Code employé (défaut) Sce tech. commercial ou méthodes 17
            data[i++] = ""; //Référence client de l'AO 18
            data[i++] = ""; //Responsable Méthode chez le client 19
            data[i++] = contactName; //Responsable achat chez le client 20
            data[i++] = ""; //Responsable qualité chez le client 21 
            data[i++] = userCode; //Employé Commercial 22
            data[i++] = ""; //Employé responsable qualité 23 
            data[i++] = ""; //Responsable achat 24
            data[i++] = ""; //Responsable validation visa 25
            data[i++] = GetFieldDate(quoteEntity, "_SENT_DATE"); //Date visa resp. (défaut) Sce tech. commercial ou méthodes 26
            data[i++] = GetFieldDate(quoteEntity, "_SENT_DATE"); //Date visa resp. Commercial 27
            data[i++] = ""; //Date visa resp. Qualité 28
            data[i++] = ""; //Date visa resp. Achat 29
            data[i++] = ""; //Date responsablevalidati on visa// 30
            data[i++] = ""; //Date réponse souhaitée// 31
            data[i++] = "0"; //Temps mis pour faire le devis// 32
            data[i++] = ""; //Date de début// 33
            data[i++] = "9"; //Monnaie// 34 
            data[i++] = external_comment; ; //Observations Entête devis            35
            data[i++] = ""; //Incoterms (champ observations) 36
            DateTime validityDate = quoteEntity.GetFieldValueAsDateTime("_SENT_DATE").AddDays(Convert.ToInt32(quoteEntity.GetFieldValueAsDouble("_ACCEPTANCE_PERIOD"))); //37
            data[i++] = validityDate.ToString("yyyyMMdd"); //38
            data[i++] = internal_comment;//Observations Entête devis //39
            WriteData(data, i, ref file);


            Write_Documents_Quote_Pdf(quoteEntity);
        }
        /// <summary>
        /// ligne apparaissant sur le devis (offre)
        /// </summary>
        /// <param name="file">nom du fichier de sortie</param>
        /// <param name="quote">Objet Iquote</param>
        /// <param name="formatProvider">gestion des format deciamaux...</param>
        private void QuoteOffre(ref string file, IQuote quote, NumberFormatInfo formatProvider)
        {
            IEntity quoteEntity = quote.QuoteEntity;
            IEntity clientEntity = quoteEntity.GetFieldValueAsEntity("_FIRM");
            IEntity paymentRuleEntity = quoteEntity.GetFieldValueAsEntity("_PAYMENT_RULE");
            string paymentRule = "";
            if (paymentRuleEntity != null)
                paymentRule = EmptyString(paymentRuleEntity.GetFieldValueAsString("_EXTERNAL_ID")).ToUpper();


            #region observation piece et ensemble

            string internal_comment = string.Empty;
            string external_comment = string.Empty;
            /*
            foreach (IEntity partEntity in quote.QuotePartList)
            {
                long partQty = 0;
                partQty = partEntity.GetFieldValueAsLong("_PART_QUANTITY");
                string internal_comment = string.Empty;
                string external_comment = string.Empty ;

                if (partQty > 0)
                {
                    internal_comment = FormatComment(EmptyString(quoteEntity.GetFieldValueAsString("_COMMENTS")));

                    if (!string.IsNullOrEmpty(internal_comment))
                    { 

                    long i = 0;
                    string[] data = new string[50];
                    string reference = null;
                    string modele = null;
                    GetReference(partEntity, "PART", true, out reference, out modele);


                 
                    data[i++] = "OBSDEVIS";
                    data[i++] = internal_comment; //Observation interne
                    data[i++] = external_comment; //Observation client
                    data[i++] = ""; // Conditions de règlement
                    data[i++] = GetQuoteNumber(quoteEntity);//N° devis
                    data[i++] = reference;//Code pièce
                    data[i++] = "";//Ordre d'impression
                    data[i++] = "";//Cycle de fab
                    data[i++] = "";//Code activitée de la pièce
                    data[i++] = "";//Modele de gamme

                    WriteData(data, i, ref file);
                    }

                }
            }
            */

            #endregion


            #region Offre pièces/observations
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
                    data[i++] = reference; //Code pièce
                    data[i++] = GetQuoteNumber(quoteEntity); //N° devis
                    data[i++] = partQty.ToString(); //Qté offre

                    double cost = partEntity.GetFieldValueAsDouble("_CORRECTED_FRANCO_UNIT_COST");
                    data[i++] = cost.ToString("#0.0000", formatProvider); //Prix de revient
                    data[i++] = cost.ToString("#0.0000", formatProvider); //Prix brut
                    data[i++] = cost.ToString("#0.0000", formatProvider); //Prix de vente
                    data[i++] = cost.ToString("#0.0000", formatProvider); //Prix dans la monnaie
                    data[i++] = "1"; //N° de ligne "Offre"

                    IField field;
                    if (quoteEntity.EntityType.TryGetField("_DELIVERY_DATE", out field))
                    {
                        data[i++] = GetFieldDate(quoteEntity, "_DELIVERY_DATE"); //Nb délai
                        data[i++] = "4"; //Type délai 1=jour 4=date
                    }
                    else
                    {
                        data[i++] = "0"; //Nb délai
                        data[i++] = "1"; //Type délai 1=jour 4=date
                    }
                    data[i++] = "1"; //Unité de prix
                    data[i++] = "0"; //Remise 1
                    data[i++] = "0"; //Remise 2
                    data[i++] = paymentRule; //Code de reglement
                    data[i++] = CreateTransFile.GetTransport(quoteEntity); // Port
                    data[i++] = modele; //Modèle
                    data[i++] = "1"; //Imprimable
                    WriteData(data, i, ref file);

                    ////observations

                    internal_comment = FormatComment(EmptyString(partEntity.GetFieldValueAsString("_COMMENTS")));

                    if (!string.IsNullOrEmpty(internal_comment))
                    {

                        long iobs = 0;
                        string[] dataobs = new string[50];
                        //string referenceobs = null;
                        //string modeleobs = null;
                        //GetReference(partEntity, "PART", true, out reference, out modele);

                        dataobs[iobs++] = "OBSDEVIS";
                        dataobs[iobs++] = internal_comment; //Observation interne
                        dataobs[iobs++] = external_comment; //Observation client
                        dataobs[iobs++] = ""; // Conditions de règlement
                        dataobs[iobs++] = GetQuoteNumber(quoteEntity);//N° devis
                        dataobs[iobs++] = reference;//Code pièce
                        dataobs[iobs++] = "";//Ordre d'impression
                        dataobs[iobs++] = "";//Cycle de fab
                        dataobs[iobs++] = "";//Code activitée de la pièce
                        dataobs[iobs++] = "";//Modele de gamme

                        WriteData(dataobs, iobs, ref file);


                    }
                }

                #endregion


            }
            #endregion

            #region Offre Ensembles
            /*
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

                        //commentaires
                        external_comment = "";  //
                        internal_comment = FormatComment(EmptyString(setEntity.GetFieldValueAsString("_COMMENTS"))); //commentaires internes des devis


                        data[i++] = "OBSDEVIS";
                        data[i++] = external_comment; //Observation interne
                        data[i++] = internal_comment;// EmptyString(setEntity.GetFieldValueAsString("_COMMENTS")); //Observation client
                        data[i++] = ""; // Conditions de règlement
                        data[i++] = GetQuoteNumber(quoteEntity);//N° devis
                        data[i++] = reference;//Code pièce
                        data[i++] = "";//Ordre d'impression
                        data[i++] = "";//Cycle de fab
                        data[i++] = "";//Code activitée de la pièce
                        data[i++] = "";//Modele de gamme
                        WriteData(data, i, ref file);
                    }



                }*/


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
                        data[i++] = reference; //Code pièce
                        data[i++] = GetQuoteNumber(quoteEntity); //N° devis
                        data[i++] = qty.ToString(formatProvider); //Qté offre

                        double cost = setEntity.GetFieldValueAsDouble("_CORRECTED_FRANCO_UNIT_COST") - totalPartCost;
                        data[i++] = cost.ToString("#0.0000", formatProvider); //Prix de revient
                        data[i++] = cost.ToString("#0.0000", formatProvider); //Prix brut
                        data[i++] = cost.ToString("#0.0000", formatProvider); //Prix de vente
                        data[i++] = cost.ToString("#0.0000", formatProvider); //Prix dans la monnaie
                        data[i++] = "1"; //N° de ligne "Offre"
                        IField field;
                        if (quoteEntity.EntityType.TryGetField("_DELIVERY_DATE", out field))
                        {
                            data[i++] = GetFieldDate(quoteEntity, "_DELIVERY_DATE"); //Nb délai
                            data[i++] = "4"; //Type délai 1=jour 4=date
                        }
                        else
                        {
                            data[i++] = "0"; //Nb délai
                            data[i++] = "1"; //Type délai 1=jour 4=date
                        }
                        data[i++] = "1"; //Unité de prix
                        data[i++] = "0"; //Remise 1
                        data[i++] = "0"; //Remise 2
                        data[i++] = paymentRule; //Code de reglement
                        data[i++] = CreateTransFile.GetTransport(quoteEntity); //Port
                        data[i++] = modele; //Modèle
                        data[i++] = "1"; //Imprimable
                        WriteData(data, i, ref file);


                    

                    if (qty > 0)
                    {
                        long iobs = 0;
                        string[] dataobs = new string[50];
               

                        //commentaires
                        external_comment = "";  //
                        internal_comment = FormatComment(EmptyString(setEntity.GetFieldValueAsString("_COMMENTS"))); //commentaires internes des devis


                        dataobs[iobs++] = "OBSDEVIS";
                        dataobs[iobs++] = external_comment; //Observation interne
                        dataobs[iobs++] = internal_comment;// EmptyString(setEntity.GetFieldValueAsString("_COMMENTS")); //Observation client
                        dataobs[iobs++] = ""; // Conditions de règlement
                        dataobs[iobs++] = GetQuoteNumber(quoteEntity);//N° devis
                        dataobs[iobs++] = reference;//Code pièce
                        dataobs[iobs++] = "";//Ordre d'impression
                        dataobs[iobs++] = "";//Cycle de fab
                        dataobs[iobs++] = "";//Code activitée de la pièce
                        dataobs[iobs++] = "";//Modele de gamme
                        WriteData(dataobs, iobs, ref file);


                    }

                    }
                }
            #endregion

                if (quote.QuoteEntity.GetFieldValueAsLong("_TRANSPORT_PAYMENT_MODE") == 1) // Transport facturé
                    Transport(ref file, quote, formatProvider, true, "001", "PORT", "0");

                GlobalItem(ref file, quote, formatProvider, true, "001", "GLOBAL", "0");
            
        }
        /// <summary>
        /// gestion du transport
        /// </summary>
        /// <param name="file"></param>
        /// <param name="quote"></param>
        /// <param name="formatProvider"></param>
        /// <param name="doOffre"></param>
        /// <param name="rang"></param>
        /// <param name="reference"></param>
        /// <param name="modele"></param>
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
                    data[i++] = reference; //Code pièce
                    data[i++] = GetQuoteNumber(quoteEntity); //N° devis
                    data[i++] = "1"; //Qté offre

                    data[i++] = cost.ToString("#0.0000", formatProvider); //Prix de revient
                    data[i++] = cost.ToString("#0.0000", formatProvider); //Prix brut
                    data[i++] = cost.ToString("#0.0000", formatProvider); //Prix de vente
                    data[i++] = cost.ToString("#0.0000", formatProvider); //Prix dans la monnaie
                    data[i++] = "1"; //N° de ligne "Offre"
                    IField field;
                    if (quoteEntity.EntityType.TryGetField("_DELIVERY_DATE", out field))
                    {
                        data[i++] = GetFieldDate(quoteEntity, "_DELIVERY_DATE"); //Nb délai
                        data[i++] = "4"; //Type délai 1=jour 4=date
                    }
                    else
                    {
                        data[i++] = "0"; //Nb délai
                        data[i++] = "1"; //Type délai 1=jour 4=date
                    }
                    data[i++] = "1"; //Unité de prix
                    data[i++] = "0"; //Remise 1
                    data[i++] = "0"; //Remise 2
                    data[i++] = paymentRule; //Code de reglement
                    data[i++] = CreateTransFile.GetTransport(quoteEntity); //Port
                    data[i++] = "0"; //Modèle
                    data[i++] = "1"; //Imprimable
                    WriteData(data, i, ref file);
                }
                #endregion

                #region Creation de la pièce global

                {
                    long i = 0;
                    string[] data = new string[50];

                    data[i++] = "ENDEVIS";
                    data[i++] = GetQuoteNumber(quoteEntity); //N° devis
                    data[i++] = EmptyString(clientEntity.GetFieldValueAsString("_EXTERNAL_ID")).ToUpper(); //Code client
                    data[i++] = reference; //Code pièce
                    data[i++] = ""; //Type (non utilisé)
                    data[i++] = FormatDesignation(""); //Désignation 1
                    data[i++] = FormatDesignation(""); //Désignation 2
                    data[i++] = FormatDesignation(""); //Désignation 3
                    data[i++] = rang; //Rang
                    data[i++] = reference; //Code pièce ou Sous pièce (sous rang)
                    data[i++] = FormatDesignation(""); //Désignation pièce ou Sous pièce (sous rang)
                    data[i++] = ""; //N° plan
                    data[i++] = rang; //Niveau rang
                    data[i++] = "3"; //Etat devis
                    data[i++] = ""; //Repère
                    data[i++] = "0"; //Origine fourniture
                    data[i++] = "1"; //Qté dus/ensemble : 1 pour le rang 001
                    data[i++] = "1"; //Qté totale de l'ensemble : 1 pour le rang 001
                    data[i++] = ""; //Indice plan
                    data[i++] = ""; //Indice gamme
                    data[i++] = ""; //Indice nomenclature
                    data[i++] = ""; //Indice pièce
                    data[i++] = ""; //Indice A
                    data[i++] = ""; //Indice B
                    data[i++] = ""; //Indice C
                    data[i++] = ""; //Indice D
                    data[i++] = ""; //Indice E
                    data[i++] = ""; //Indice F
                    data[i++] = "0"; //N° identifiant GED 1
                    data[i++] = "0"; //N° identifiant GED 2
                    data[i++] = "0"; //N° identifiant GED 3
                    data[i++] = "0"; //N° identifiant GED 4
                    data[i++] = "0"; //N° identifiant GED 5
                    data[i++] = "0"; //N° identifiant GED 6
                    data[i++] = "0"; //N° identifiant GED 7
                    data[i++] = "0"; //N° identifiant GED 8
                    data[i++] = "0"; //N° identifiant GED 9
                    data[i++] = "0"; //N° identifiant GED 10
                    data[i++] = ""; //Fichier joint
                    data[i++] = ""; //Date d'injection
                    data[i++] = "0"; //Modèle
                    data[i++] = ""; //Employé responsable                
                    WriteData(data, i, ref file);
                }

                #endregion

                #region Creation de l'achat

                {
                    long i = 0;
                    string[] data = new string[50];

                    data[i++] = "GADEVIS";
                    data[i++] = rang; //Rang
                    data[i++] = ""; //inutilisé
                    data[i++] = gadevisPhase.ToString(formatProvider); //Phase
                    data[i++] = FormatDesignation("ACHAT NOMENCLATURE"); //Désignation 1
                    data[i++] = FormatDesignation(""); //Désignation 2
                    data[i++] = FormatDesignation(""); //Désignation 3
                    data[i++] = FormatDesignation(""); //Désignation 4
                    data[i++] = FormatDesignation(""); //Désignation 5
                    data[i++] = FormatDesignation(""); //Désignation 6
                    data[i++] = "NOMEN"; //Centre de frais

                    data[i++] = "0"; //Tps Prep
                    data[i++] = "0"; //Tps Unit
                    data[i++] = "0"; //Coût Opération
                    data[i++] = "0"; //Taux horaire
                    data[i++] = GetFieldDate(quoteEntity, "_CREATION_DATE"); //Date
                    data[i++] = GetQuoteNumber(quoteEntity); //N° devis
                    data[i++] = ""; //Nom fichier joint
                    data[i++] = "0"; //N° identifiant GED 1
                    data[i++] = "0"; //N° identifiant GED 2
                    data[i++] = "0"; //N° identifiant GED 3
                    data[i++] = "0"; //N° identifiant GED 4
                    data[i++] = "0"; //N° identifiant GED 5
                    data[i++] = "0"; //N° identifiant GED 6
                    data[i++] = "0"; //Niveau du rang
                    data[i++] = "";//Observations
                    data[i++] = ""; //Lien avec la phase de nomenclature
                    data[i++] = ""; //Date dernière modif
                    data[i++] = ""; //Employé modif
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
                    data[i++] = reference; //Code pièce
                    data[i++] = rang; //Rang
                    data[i++] = nomendvPhase.ToString(formatProvider); //Phase
                    data[i++] = ""; //Repère
                    data[i++] = "TRANSPORT_ALMA"; //Code article
                    data[i++] = FormatDesignation("cout de transport devis nuemro "); //Désignation 1
                    data[i++] = FormatDesignation(""); //Désignation 2
                    data[i++] = FormatDesignation(""); //Désignation 3
                    data[i++] = ""; //Temps de réappro
                    data[i++] = "1"; //Qté
                    data[i++] = calCost.ToString("#0.000", formatProvider); //Px article ou Px/Kg
                    data[i++] = calCost.ToString("#0.000", formatProvider); //Prix total
                    data[i++] = ""; //Code Fournisseur
                    data[i++] = ""; //2sd fournisseur
                    data[i++] = "1"; //Type
                    data[i++] = "1"; //Prix constant
                    data[i++] = ""; //Poids tôle ou article
                    data[i++] = transportFamilly; //Famille
                    data[i++] = ""; //N° tarif de Clipper
                    data[i++] = "Cout de transport du devis alma " + GetQuoteNumber(quoteEntity); //Observation
                    data[i++] = ""; //Observation interne
                    data[i++] = ""; //Observation débit
                    data[i++] = ""; //Val Débit 1
                    data[i++] = ""; //Val Débit 2
                    data[i++] = ""; //Qté Débit
                    data[i++] = ""; //Nb pc/débit ou débit/pc
                    data[i++] = ""; //Lien avec la phase de gamme
                    data[i++] = ""; //Unite de quantité
                    data[i++] = ""; //Unité de prix
                    data[i++] = ""; //Coef Unite
                    data[i++] = ""; //Coef Prix
                    data[i++] = "0"; //Prix constant ??? semble plutot correcpondre au Modèle
                    data[i++] = "0"; //Modèle ??? semble plutot correcpondre au Prix constant
                    data[i++] = ""; //Qté constant
                    data[i++] = gadevisPhase.ToString(formatProvider); //Magasin ???? erreur

                    WriteData(data, i, ref file);
                }
                #endregion
            }
        }
        /// <summary>
        /// creation de la piece globale
        /// </summary>
        /// <param name="file"></param>
        /// <param name="quote"></param>
        /// <param name="formatProvider"></param>
        /// <param name="doOffre"></param>
        /// <param name="rang"></param>
        /// <param name="reference"></param>
        /// <param name="modele"></param>
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
                    data[i++] = reference; //Code pièce
                    data[i++] = GetQuoteNumber(quoteEntity); //N° devis
                    data[i++] = "1"; //Qté offre

                    double operationCost = globalOperationList.Sum(p => p.GetFieldValueAsDouble("_CORRECTED_COST"));
                    double supplyCost = globalSupplyList.Sum(p => p.GetFieldValueAsDouble("_CORRECTED_COST"));
                    double cost = operationCost + supplyCost;
                    data[i++] = cost.ToString("#0.0000", formatProvider); //Prix de revient
                    data[i++] = cost.ToString("#0.0000", formatProvider); //Prix brut
                    data[i++] = cost.ToString("#0.0000", formatProvider); //Prix de vente
                    data[i++] = cost.ToString("#0.0000", formatProvider); //Prix dans la monnaie

                    data[i++] = "1"; //N° de ligne "Offre"
                    IField field;
                    if (quoteEntity.EntityType.TryGetField("_DELIVERY_DATE", out field))
                    {
                        data[i++] = GetFieldDate(quoteEntity, "_DELIVERY_DATE"); //Nb délai
                        data[i++] = "4"; //Type délai 1=jour 4=date
                    }
                    else
                    {
                        data[i++] = "0"; //Nb délai
                        data[i++] = "1"; //Type délai 1=jour 4=date
                    }
                    data[i++] = "1"; //Unité de prix
                    data[i++] = "0"; //Remise 1
                    data[i++] = "0"; //Remise 2
                    data[i++] = paymentRule; //Code de reglement
                    data[i++] = CreateTransFile.GetTransport(quoteEntity); //Port
                    data[i++] = "0"; //Modèle
                    data[i++] = "1"; //Imprimable
                    WriteData(data, i, ref file);
                }

                #endregion

                #region Creation de la pièce global

                {
                    long i = 0;
                    string[] data = new string[50];

                    data[i++] = "ENDEVIS";
                    data[i++] = GetQuoteNumber(quoteEntity); //N° devis
                    data[i++] = EmptyString(clientEntity.GetFieldValueAsString("_EXTERNAL_ID")).ToUpper(); //Code client
                    data[i++] = reference; //Code pièce
                    data[i++] = ""; //Type (non utilisé)
                    data[i++] = FormatDesignation(""); //Désignation 1
                    data[i++] = FormatDesignation(""); //Désignation 2
                    data[i++] = FormatDesignation(""); //Désignation 3
                    data[i++] = rang; //Rang
                    data[i++] = reference; //Code pièce ou Sous pièce (sous rang)
                    data[i++] = FormatDesignation(""); //Désignation pièce ou Sous pièce (sous rang)
                    data[i++] = ""; //N° plan
                    data[i++] = rang; //Niveau rang
                    data[i++] = "3"; //Etat devis
                    data[i++] = ""; //Repère
                    data[i++] = "0"; //Origine fourniture
                    data[i++] = "1"; //Qté dus/ensemble : 1 pour le rang 001
                    data[i++] = "1"; //Qté totale de l'ensemble : 1 pour le rang 001
                    data[i++] = ""; //Indice plan
                    data[i++] = ""; //Indice gamme
                    data[i++] = ""; //Indice nomenclature
                    data[i++] = ""; //Indice pièce
                    data[i++] = ""; //Indice A
                    data[i++] = ""; //Indice B
                    data[i++] = ""; //Indice C
                    data[i++] = ""; //Indice D
                    data[i++] = ""; //Indice E
                    data[i++] = ""; //Indice F
                    data[i++] = "0"; //N° identifiant GED 1
                    data[i++] = "0"; //N° identifiant GED 2
                    data[i++] = "0"; //N° identifiant GED 3
                    data[i++] = "0"; //N° identifiant GED 4
                    data[i++] = "0"; //N° identifiant GED 5
                    data[i++] = "0"; //N° identifiant GED 6
                    data[i++] = "0"; //N° identifiant GED 7
                    data[i++] = "0"; //N° identifiant GED 8
                    data[i++] = "0"; //N° identifiant GED 9
                    data[i++] = "0"; //N° identifiant GED 10
                    data[i++] = ""; //Fichier joint
                    data[i++] = ""; //Date d'injection
                    data[i++] = "0"; //Modèle
                    data[i++] = ""; //Employé responsable                
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
        /// <summary>
        /// creation des quotes part
        /// </summary>
        /// <param name="file"></param>
        /// <param name="quote"></param>
        /// <param name="rang"></param>
        /// <param name="formatProvider"></param>
        private void QuotePart(ref string file, IQuote quote, string rang, NumberFormatInfo formatProvider)
        {
            IEntity quoteEntity = quote.QuoteEntity;
            IEntity clientEntity = quoteEntity.GetFieldValueAsEntity("_FIRM");
            //creation 
            //string dpr_directory = quote.Context.ParameterSetManager.GetParameterValue("_EXPORT", "_ACTCUT_DPR_DIRECTORY").GetValueAsString();

            //create dpr and directory
            
            //obsolete fait par le getDrpPart
           
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
                    data[i++] = GetQuoteNumber(quoteEntity); //N° devis
                    data[i++] = EmptyString(clientEntity.GetFieldValueAsString("_EXTERNAL_ID")).ToUpper(); //Code client
                    data[i++] = partReference; //Code pièce
                    data[i++] = ""; //Type (non utilisé)
                    data[i++] = FormatDesignation(partEntity.GetFieldValueAsString("_DESCRIPTION")); //Désignation 1
                    data[i++] = FormatDesignation(materialName); //Désignation 2
                    data[i++] = FormatDesignation(""); //Désignation 3
                    data[i++] = rang; //Rang
                    data[i++] = partReference; //Code pièce ou Sous pièce (sous rang)
                    data[i++] = FormatDesignation(partEntity.GetFieldValueAsString("_DESCRIPTION")); //Désignation pièce ou Sous pièce (sous rang)
                    data[i++] = partEntity.Id.ToString(); //N° plan
                    data[i++] = rang; //Niveau rang
                    data[i++] = "3"; //Etat devis
                    data[i++] = ""; //Repère
                    data[i++] = "0"; //Origine fourniture
                    data[i++] = "1"; //Qté dus/ensemble : 1 pour le rang 001
                    data[i++] = "1"; //Qté totale de l'ensemble : 1 pour le rang 001
                    data[i++] = ""; //Indice plan
                    data[i++] = ""; //Indice gamme
                    data[i++] = ""; //Indice nomenclature
                    data[i++] = ""; //Indice pièce
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
                    data[i++] = "0"; //N° identifiant GED 1
                    data[i++] = "0"; //N° identifiant GED 2
                    data[i++] = "0"; //N° identifiant GED 3
                    data[i++] = "0"; //N° identifiant GED 4
                    data[i++] = "0"; //N° identifiant GED 5
                    data[i++] = "0"; //N° identifiant GED 6
                    data[i++] = "0"; //N° identifiant GED 7
                    data[i++] = "0"; //N° identifiant GED 8
                    data[i++] = "0"; //N° identifiant GED 9
                    data[i++] = "0"; //N° identifiant GED 10

                    ///
                    /// a supprimer apres 2 mois d'instal/
                    /*
                      ///on laisse pour le moment
                      ///
                      string assistantType = partEntity.GetFieldValueAsString("_ASSISTANT_TYPE");
                      string partFileName = partEntity.GetFieldValueAsString("_FILENAME");


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

                         if (!string.IsNullOrEmpty(dpr_directory)) { 

                              string empty_emfFile;

                              //cas général
                              emfFile = partEntity.GetFieldValueAsString("_DPR_FILENAME") + ".emf";
                              //emfFile vide
                              empty_emfFile = dpr_directory + "\\" + "Quote_" + quote.QuoteEntity.Id + "\\"+ partEntity.GetFieldValueAsString("_REFERENCE")+".dpr.emf"; // + Path.GetFileName(partEntity.GetImageFieldValueAsLinkFile("_PREVIEW"));
                              //cas general
                               emfFile = GetEmfFile(partEntity, empty_emfFile);


                              if (assistantType.Contains("DprAssistant") || isGenericDpr)
                              {

                                  emfFile = partEntity.GetFieldValueAsString("_FILENAME")+ ".emf";

                              }

                              //traitement spé
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
                      }*/


                    data[i++] = GetDprPath(partEntity, quote);
                    data[i++] = ""; //Date d'injection
                    data[i++] = partModele; //Modèle
                    data[i++] = ""; //Employé responsable                
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
                        if (quote.QuoteEntity.GetFieldValueAsLong("_TRANSPORT_PAYMENT_MODE") != 1) // Transport non facturé
                            Transport(ref file, quote, formatProvider, false, "003", partReference, partModele);

                        // On met les item globaux masqués sur la première pièces dans le rang "002"
                        GlobalItem(ref file, quote, formatProvider, false, "002", partReference, partModele);
                        _GlobalExported = true;
                    }
                }
            }
        }

        /// <summary>
        /// creation des quotes tube
        /// </summary>
        /// <param name="file"></param>
        /// <param name="quote"></param>
        /// <param name="rang"></param>
        /// <param name="formatProvider"></param>
        private void QuoteTube(ref string file, IQuote quote, string rang, NumberFormatInfo formatProvider)
        {
            IEntity quoteEntity = quote.QuoteEntity;
            IEntity clientEntity = quoteEntity.GetFieldValueAsEntity("_FIRM");
            //creation 
            //string dpr_directory = quote.Context.ParameterSetManager.GetParameterValue("_EXPORT", "_ACTCUT_DPR_DIRECTORY").GetValueAsString();

            //create dpr and directory

            //obsolete fait par le getDrpPart

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
                    data[i++] = GetQuoteNumber(quoteEntity); //N° devis
                    data[i++] = EmptyString(clientEntity.GetFieldValueAsString("_EXTERNAL_ID")).ToUpper(); //Code client
                    data[i++] = partReference; //Code pièce
                    data[i++] = ""; //Type (non utilisé)
                    data[i++] = FormatDesignation(partEntity.GetFieldValueAsString("_DESCRIPTION")); //Désignation 1
                    data[i++] = FormatDesignation(materialName); //Désignation 2
                    data[i++] = FormatDesignation(""); //Désignation 3
                    data[i++] = rang; //Rang
                    data[i++] = partReference; //Code pièce ou Sous pièce (sous rang)
                    data[i++] = FormatDesignation(partEntity.GetFieldValueAsString("_DESCRIPTION")); //Désignation pièce ou Sous pièce (sous rang)
                    data[i++] = partEntity.Id.ToString(); //N° plan
                    data[i++] = rang; //Niveau rang
                    data[i++] = "3"; //Etat devis
                    data[i++] = ""; //Repère
                    data[i++] = "0"; //Origine fourniture
                    data[i++] = "1"; //Qté dus/ensemble : 1 pour le rang 001
                    data[i++] = "1"; //Qté totale de l'ensemble : 1 pour le rang 001
                    data[i++] = ""; //Indice plan
                    data[i++] = ""; //Indice gamme
                    data[i++] = ""; //Indice nomenclature
                    data[i++] = ""; //Indice pièce
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
                    data[i++] = "0"; //N° identifiant GED 1
                    data[i++] = "0"; //N° identifiant GED 2
                    data[i++] = "0"; //N° identifiant GED 3
                    data[i++] = "0"; //N° identifiant GED 4
                    data[i++] = "0"; //N° identifiant GED 5
                    data[i++] = "0"; //N° identifiant GED 6
                    data[i++] = "0"; //N° identifiant GED 7
                    data[i++] = "0"; //N° identifiant GED 8
                    data[i++] = "0"; //N° identifiant GED 9
                    data[i++] = "0"; //N° identifiant GED 10

                   


                    data[i++] = GetDprPath(partEntity, quote);
                    data[i++] = ""; //Date d'injection
                    data[i++] = partModele; //Modèle
                    data[i++] = ""; //Employé responsable                
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
                        if (quote.QuoteEntity.GetFieldValueAsLong("_TRANSPORT_PAYMENT_MODE") != 1) // Transport non facturé
                            Transport(ref file, quote, formatProvider, false, "003", partReference, partModele);

                        // On met les item globaux masqués sur la première pièces dans le rang "002"
                        GlobalItem(ref file, quote, formatProvider, false, "002", partReference, partModele);
                        _GlobalExported = true;
                    }
                }
            }
        }









        /// <summary>
        /// ajoute un point rouge sur l'emf généré par almacam
        /// </summary>
        /// <param name="empty_emfFile">fullpath vers l'emf</param>
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
        /// cree les ensemble
        /// </summary>
        /// <param name="file">fichier texte de sortie des devis</param>
        /// <param name="quote">devis en cours</param>
        /// <param name="rang">rang - niveau dans l'ensemble</param>
        /// <param name="formatProvider">nombre de chiffre apres la virgule</param>
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
                    data[i++] = GetQuoteNumber(quoteEntity); //N° devis
                    data[i++] = EmptyString(clientEntity.GetFieldValueAsString("_EXTERNAL_ID")).ToUpper(); //Code client
                    data[i++] = setReference; //Code pièce
                    data[i++] = ""; //Type (non utilisé)
                    data[i++] = FormatDesignation(setEntity.GetFieldValueAsString("_DESCRIPTION")); //Désignation 1
                    data[i++] = FormatDesignation(""); //Désignation 2
                    data[i++] = FormatDesignation(""); //Désignation 3
                    data[i++] = rang; //Rang
                    data[i++] = setReference; //Code pièce ou Sous pièce (sous rang)
                    data[i++] = FormatDesignation(setEntity.GetFieldValueAsString("_DESCRIPTION")); //Désignation pièce ou Sous pièce (sous rang)
                    data[i++] = setEntity.Id.ToString(); //N° plan
                    data[i++] = rang; //Niveau rang
                    data[i++] = "3"; //Etat devis
                    data[i++] = ""; //Repère
                    data[i++] = "0"; //Origine fourniture
                    data[i++] = "1"; //Qté dus/ensemble : 1 pour le rang 001
                    data[i++] = "1"; //Qté totale de l'ensemble : 1 pour le rang 001
                    data[i++] = ""; //Indice plan
                    data[i++] = ""; //Indice gamme
                    data[i++] = ""; //Indice nomenclature
                    data[i++] = ""; //Indice pièce
                    data[i++] = ""; //Indice A
                    data[i++] = ""; //Indice B
                    data[i++] = ""; //Indice C
                    data[i++] = ""; //Indice D
                    data[i++] = ""; //Indice E
                    data[i++] = ""; //Indice F
                    data[i++] = "0"; //N° identifiant GED 1
                    data[i++] = "0"; //N° identifiant GED 2
                    data[i++] = "0"; //N° identifiant GED 3
                    data[i++] = "0"; //N° identifiant GED 4
                    data[i++] = "0"; //N° identifiant GED 5
                    data[i++] = "0"; //N° identifiant GED 6
                    data[i++] = "0"; //N° identifiant GED 7
                    data[i++] = "0"; //N° identifiant GED 8
                    data[i++] = "0"; //N° identifiant GED 9
                    data[i++] = "0"; //N° identifiant GED 10
                    data[i++] = ""; //Fichier joint
                    data[i++] = ""; //Date d'injection
                    data[i++] = setModele; //Modèle
                    data[i++] = ""; //Employé responsable                
                    WriteData(data, i, ref file);

                    long gaDevisPhase = 0;
                    long nomendvPhase = 0;

                    // Fourniture de l'ensemble
                    IList<IEntity> setSupplyList = new List<IEntity>(quote.GetSetSupplyList(setEntity));
                    QuoteSupply(ref file, quote, 1, setSupplyList, rang, ref gaDevisPhase, ref nomendvPhase, formatProvider, true, setReference, setModele);

                    // Operation de l'ensemble
                    IList<IEntity> setOperationList = new List<IEntity>(quote.GetSetOperationList(setEntity));
                    QuoteOperation(ref file, quote, null, setOperationList, rang, formatProvider, 1, setQty, ref gaDevisPhase, ref nomendvPhase, setReference, setModele, 0.0);

                    // Pièces de l'ensemble
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
        /// <summary>
        /// edition des pieces d'ensemble
        /// </summary>
        /// <param name="file">fichier texte de sortie des devis</param>
        /// <param name="quote">devis en cours</param>
        /// <param name="setEntity"></param>
        /// <param name="partEntity"></param>
        /// <param name="partSetQty"></param>
        /// <param name="rang">rang - niveau dans l'ensemble</param>
        /// <param name="formatProvider">nombre de chiffre apres la virgule</param>
        /// 
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
                data[i++] = GetQuoteNumber(quoteEntity); //N° devis
                data[i++] = EmptyString(clientEntity.GetFieldValueAsString("_EXTERNAL_ID")).ToUpper(); //Code client
                data[i++] = setReference; //Code pièce
                data[i++] = ""; //Type (non utilisé)
                data[i++] = FormatDesignation(setEntity.GetFieldValueAsString("_DESCRIPTION")); //Désignation 1
                data[i++] = FormatDesignation(""); //Désignation 2
                data[i++] = FormatDesignation(""); //Désignation 3
                data[i++] = rang; //Rang
                data[i++] = partReference; //Code pièce ou Sous pièce (sous rang)
                data[i++] = FormatDesignation(partEntity.GetFieldValueAsString("_DESCRIPTION")); //Désignation pièce ou Sous pièce (sous rang)
                data[i++] = partEntity.Id.ToString(); //N° plan
                data[i++] = rang; //Niveau rang
                data[i++] = "3"; //Etat devis
                data[i++] = ""; //Repère
                data[i++] = "0"; //Origine fourniture
                data[i++] = partSetQty.ToString(formatProvider); //Qté dus/ensemble
                data[i++] = "1"; //Qté totale de l'ensemble
                data[i++] = ""; //Indice plan
                data[i++] = ""; //Indice gamme
                data[i++] = ""; //Indice nomenclature
                data[i++] = ""; //Indice pièce
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
                data[i++] = "0"; //N° identifiant GED 1
                data[i++] = "0"; //N° identifiant GED 2
                data[i++] = "0"; //N° identifiant GED 3
                data[i++] = "0"; //N° identifiant GED 4
                data[i++] = "0"; //N° identifiant GED 5
                data[i++] = "0"; //N° identifiant GED 6
                data[i++] = "0"; //N° identifiant GED 7
                data[i++] = "0"; //N° identifiant GED 8
                data[i++] = "0"; //N° identifiant GED  9
                data[i++] = "0"; //N° identifiant GED 10
                //data[i++] = ""; //Fichier joint
                data[i++] = GetDprPath(partEntity, quote);
                data[i++] = ""; //Date d'injection
                data[i++] = setModele; //Modèle
                data[i++] = ""; //Employé responsable                
                WriteData(data, i, ref file);

                long gaDevisPhase = 0;
                long nomendvPhase = 0;

                // Fournitures de la pièce dans l'ensemble
                IList<IEntity> setSupplyList = new List<IEntity>(quote.GetPartSupplyList(partEntity));
                QuoteSupply(ref file, quote, partSetQty, setSupplyList, rang, ref gaDevisPhase, ref nomendvPhase, formatProvider, true, setReference, setModele);

                // Operations de la pièce dans l'ensemble
                double totalMaterialPrice = partEntity.GetFieldValueAsDouble("_CORRECTED_MAT_COST");
                long totalPartQty = partEntity.GetFieldValueAsLong("_QUANTITY");
                double materialPrice = totalMaterialPrice / totalPartQty;
                IList<IEntity> partOperationList = new List<IEntity>(quote.GetPartOperationList(partEntity));
                QuoteOperation(ref file, quote, partEntity, partOperationList, rang, formatProvider, partSetQty, partSetQty, ref gaDevisPhase, ref nomendvPhase, setReference, setModele, materialPrice);
            }
        }
        #region gestion des emf/dpr
                    /// <summary>
                    /// creer les liens emf dans le fichier clipper
                    /// creer un dpr vide  en cas de nom envoie du fichier dpr
                    /// </summary>
                    /// <param name="partEntity">part exporté</param>
                    /// <param name="empty_emfFile">liens vide si besoin</param>
                    /// <returns></returns>
                    private string GetEmfFile(IEntity partEntity, string empty_emfFile)
                    {
                        try
                        {
                            string emfFile = "";

                            //cas général
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
                                //CreateEmptyDpr(emfFile.Replace(".emf", ".dpr"));
                                string matiere = partEntity.GetFieldValueAsEntity("_MATERIAL").GetFieldValueAsEntity("_QUALITY").GetFieldValueAsString("_NAME");
                                string thickness = partEntity.GetFieldValueAsEntity("_MATERIAL").GetFieldValueAsLong("_THICKNESS").ToString(); //partEntity.GetFieldValueAsLong("_THICKNESS").ToString();

                    CreateEmptyDprWithThickness(emfFile.Replace(".emf", ".dpr"), matiere, thickness);
                                CreateEmptyEmf(emfFile.Replace(".emf", ".dpr.emf"));

                            }




                            return emfFile;



                        }
                        catch (Exception ie) { MessageBox.Show(ie.Message); return ""; };
                    }

                    [Obsolete("Use CreateEmptyDprWithThickness")]
                    /// cette fonction creer un emf vide sans control de la matiere et de l'epaisseur
                    private void CreateEmptyDpr(string filePath)
                    {
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@filePath))
                        {
                            file.WriteLine("/ DPR 4.0 R 1");
                            file.WriteLine("/ HEADER");
                            file.WriteLine("$UNIT = 1");
                            file.WriteLine("$THICK = 0");///on pourrait ajouter l'epaisseur 
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
                            file.WriteLine("$MATERIAL =");///on pourrait ajouter l'epaisseur 

                            file.Close();
                        }
                    }

                    /// <summary>
                    /// creer un dpr vide avec la bonne matiere paramétree et la bonne epaisseur
                    /// </summary>
                    /// <param name="filePath"></param>
                    /// <param name="Matiere"></param>
                    /// <param name="epaisseur"></param>
                    private void CreateEmptyDprWithThickness(string filePath, string Matiere, string epaisseur)
                    {
                        try
                        {
                            //creation du dossier
                            CreateDirectory(Path.GetDirectoryName(filePath));
                            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@filePath))
                            {


                                //remplacement chaine vide

                                if (String.IsNullOrEmpty(Matiere)) { Matiere = ""; }
                                if (String.IsNullOrEmpty(epaisseur)) { epaisseur = "0"; }

                                file.WriteLine("/ DPR 4.0 R 1");
                                file.WriteLine("/ HEADER");
                                file.WriteLine("$UNIT = 1");
                                file.WriteLine("$THICK = " + epaisseur);///on pourrait ajouter l'epaisseur 
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
                                file.WriteLine("$MATERIAL = " + Matiere);

                                file.Close();
                            }

                        }

                        catch (Exception ie) { MessageBox.Show(ie.Message, "creation d'un dpr vide impossible"); }

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

        #endregion
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
                    data[i++] = reference; //Code pièce
                    data[i++] = rang; //Rang
                    data[i++] = nomendvPhase.ToString(formatProvider); //Phase
                    data[i++] = ""; //Repère
                    data[i++] = "MATIERE"; //Code article
                    data[i++] = "MATIERE"; //Désignation 1
                    data[i++] = ""; //Désignation 2
                    data[i++] = ""; //Désignation 3
                    data[i++] = ""; //Temps de réappro
                    data[i++] = mainParentQuantity.ToString(); //Qté
                    data[i++] = materialPrice.ToString("#0.0000", formatProvider); //Px article ou Px/Kg
                    data[i++] = materialPrice.ToString("#0.0000", formatProvider); //Prix total
                    data[i++] = ""; //Code Fournisseur
                    data[i++] = ""; //2sd fournisseur
                    data[i++] = "1"; //Type
                    data[i++] = ""; //Prix constant
                    data[i++] = ""; //Poids tôle ou article
                    data[i++] = "DEVIS"; //Famille
                    data[i++] = ""; //N° tarif de Clipper
                    data[i++] = ""; //Observation
                    data[i++] = ""; //Observation interne
                    data[i++] = ""; //Observation débit
                    data[i++] = ""; //Val Débit 1
                    data[i++] = ""; //Val Débit 2
                    data[i++] = ""; //Qté Débit
                    data[i++] = ""; //Nb pc/débit ou débit/pc
                    data[i++] = ""; //Lien avec la phase de gamme
                    data[i++] = ""; //Unite de quantité
                    data[i++] = ""; //Unité de prix
                    data[i++] = ""; //Coef Unite
                    data[i++] = ""; //Coef Prix
                    data[i++] = modele; //Prix constant ??? semble plutot correcpondre au Modèle
                    data[i++] = "0"; //Modèle ??? semble plutot correcpondre au Prix constant
                    data[i++] = ""; //Qté constant
                    data[i++] = cutGadevisPhase.ToString(formatProvider); //Magasin ???? erreur

                    WriteData(data, i, ref file);
                }
                else
                {
                    double surface = partEntity.GetFieldValueAsDouble("_SURFACE");
                    data[i++] = "NOMENDVALMA";
                    data[i++] = GetQuoteNumber(quoteEntity); //Code devis
                    data[i++] = reference; //Code pièce
                    data[i++] = modele; //Modèle
                    data[i++] = rang; //Rang
                    data[i++] = nomendvPhase.ToString(formatProvider); //Phase
                    data[i++] = codeArticleMaterial; //Code article
                    data[i++] = surface.ToString("#0.0000", formatProvider); //Surface pour faire une pièce
                    data[i++] = materialPrice.ToString("#0.0000", formatProvider); //Prix total pour faire une pièce

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
                data[i++] = ""; //inutilisé
                data[i++] = groupedCutOperation.GadevisPhase.ToString(formatProvider); //Phase

                data[i++] = "COUPE"; //Désignation 1
                data[i++] = FormatDesignation(""); //Désignation 2
                data[i++] = FormatDesignation(""); //Désignation 3
                data[i++] = FormatDesignation(""); //Désignation 4
                data[i++] = FormatDesignation(""); //Désignation 5
                data[i++] = FormatDesignation(""); //Désignation 6
                data[i++] = groupedCutOperation.CentreFrais; //Centre de frais 

                double tpsPrep = groupedCutOperation.CorrectedUnitPrepTime;
                double tpsUnit = mainParentQuantity * groupedCutOperation.UnitTime;
                ///
                data[i++] = tpsPrep.ToString("#0.0000", formatProvider); //Tps Prep
                data[i++] = tpsUnit.ToString("#0.0000", formatProvider); //Tps Unit (heure)
                ///
                double unitCost = groupedCutOperation.UnitCost;
                data[i++] = (unitCost * mainParentQuantity).ToString("#0.0000", formatProvider); //Coût Opération
                ///
                double hourlyCost = 0;
                if (((tpsPrep / parentQty) + tpsUnit != 0))
                    hourlyCost = unitCost / ((tpsPrep / parentQty) + tpsUnit);
                data[i++] = hourlyCost.ToString("#0.0000", formatProvider); //Taux horaire (/heure)
                ///
                data[i++] = GetFieldDate(quoteEntity, "_CREATION_DATE"); //Date
                data[i++] = GetQuoteNumber(quoteEntity); //N° devis
                data[i++] = ""; //Nom fichier joint
                data[i++] = "0"; //N° identifiant GED 1
                data[i++] = "0"; //N° identifiant GED 2
                data[i++] = "0"; //N° identifiant GED 3
                data[i++] = "0"; //N° identifiant GED 4
                data[i++] = "0"; //N° identifiant GED 5
                data[i++] = "0"; //N° identifiant GED 6
                data[i++] = "0"; //Niveau du rang
                data[i++] = groupedCutOperation.Comments; //Observations
                data[i++] = ""; //Lien avec la phase de nomenclature
                data[i++] = ""; //Date dernière modif
                data[i++] = ""; //Employé modif
                data[i++] = ""; //Niveau de blocage
                data[i++] = ""; //Taux homme TP
                data[i++] = ""; //Taux homme TU
                data[i++] = ""; //Nb pers TP
                data[i++] = ""; //Nb Pers TU
                WriteData(data, i, ref file);


                ///ecriture de l'attachement piece
                Write_Documents(partEntity, quote.QuoteEntity.Id, ref file);


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
                data[i++] = ""; //inutilisé
                data[i++] = gadevisPhase.ToString(formatProvider); //Phase

                if ((quote as Quote).GetOperationType(operationEntity) == OperationType.Stt)
                    data[i++] = FormatDesignation("SOUS-TRAITANCE"); //Désignation 1
                else
                    data[i++] = FormatDesignation(operationEntity.GetFieldValueAsString("_NAME")); //Désignation 1

                data[i++] = FormatDesignation(""); //Désignation 2
                data[i++] = FormatDesignation(""); //Désignation 3
                data[i++] = FormatDesignation(""); //Désignation 4
                data[i++] = FormatDesignation(""); //Désignation 5
                data[i++] = FormatDesignation(""); //Désignation 6
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
                data[i++] = unitCost.ToString("#0.0000", formatProvider); //Coût Opération

                double hourlyCost = 0;
                if (((tpsPrep / parentQty) + tpsUnit != 0))
                    hourlyCost = unitCost / ((tpsPrep / parentQty) + tpsUnit);
                data[i++] = hourlyCost.ToString("#0.0000", formatProvider); //Taux horaire (/heure)

                string internal_comments;


                data[i++] = GetFieldDate(quoteEntity, "_CREATION_DATE"); //Date
                data[i++] = GetQuoteNumber(quoteEntity); //N° devis
                data[i++] = ""; //Nom fichier joint
                data[i++] = "0"; //N° identifiant GED 1
                data[i++] = "0"; //N° identifiant GED 2
                data[i++] = "0"; //N° identifiant GED 3
                data[i++] = "0"; //N° identifiant GED 4
                data[i++] = "0"; //N° identifiant GED 5
                data[i++] = "0"; //N° identifiant GED 6
                data[i++] = "0"; //Niveau du rang
                data[i++] = "" ; //Observations
                data[i++] = ""; //Lien avec la phase de nomenclature
                data[i++] = ""; //Date dernière modif
                data[i++] = ""; //Employé modif
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
                    data[i++] = reference; //Code pièce
                    data[i++] = rang; //Rang
                    data[i++] = nomendvPhase.ToString(formatProvider); //Phase
                    data[i++] = ""; //Repère
                    data[i++] = EmptyString(operationEntity.GetFieldValueAsString("_NAME")); //Code article
                    data[i++] = FormatDesignation(operationEntity.GetFieldValueAsString("_NAME")); //Désignation 1
                    data[i++] = FormatDesignation(""); //Désignation 2
                    data[i++] = FormatDesignation(""); //Désignation 3
                    data[i++] = ""; //Temps de réappro

                    data[i++] = mainParentQuantity.ToString(); //Qté
                    data[i++] = operationEntity.GetFieldValueAsDouble("_IN_COST").ToString("#0.0000", formatProvider); //Px article ou Px/Kg
                    data[i++] = operationEntity.GetFieldValueAsDouble("_IN_COST").ToString("#0.0000", formatProvider); //Prix total

                    data[i++] = ""; //Code Fournisseur
                    data[i++] = ""; //2sd fournisseur
                    data[i++] = "3"; //Type : 3 pour sous-traitance
                    data[i++] = ""; //Prix constant
                    data[i++] = ""; //Poids tôle ou article
                    data[i++] = CreateTransFile.GetSttFamily(subOperationEntity); //Famille
                    data[i++] = ""; //N° tarif de Clipper
                    data[i++] = ""; //Observation
                    data[i++] = ""; //Observation interne
                    data[i++] = ""; //Observation débit
                    data[i++] = ""; //Val Débit 1
                    data[i++] = ""; //Val Débit 2
                    data[i++] = ""; //Qté Débit
                    data[i++] = ""; //Nb pc/débit ou débit/pc
                    data[i++] = ""; //Lien avec la phase de gamme
                    data[i++] = ""; //Unite de quantité
                    data[i++] = ""; //Unité de prix
                    data[i++] = ""; //Coef Unite
                    data[i++] = ""; //Coef Prix
                    data[i++] = modele; //Prix constant ??? semble plutot correcpondre au Modèle
                    data[i++] = "0"; //Modèle ??? semble plutot correcpondre au Prix constant
                    data[i++] = ""; //Qté constant
                    data[i++] = gadevisPhase.ToString(formatProvider); //Magasin ???? erreur

                    WriteData(data, i, ref file);
                }

                #endregion
            }

            #endregion
        }
        /// <summary>
        /// traitement des fournitures
        /// </summary>
        /// <param name="file"></param>
        /// <param name="quote"></param>
        /// <param name="parentQty"></param>
        /// <param name="supplyList"></param>
        /// <param name="rang"></param>
        /// <param name="gadevisPhase"></param>
        /// <param name="nomendvPhase"></param>
        /// <param name="formatProvider"></param>
        /// <param name="includeHeader"></param>
        /// <param name="reference"></param>
        /// <param name="modele"></param>
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
                data[i++] = ""; //inutilisé
                data[i++] = gadevisPhase.ToString(formatProvider); //Phase
                data[i++] = FormatDesignation("ACHAT NOMENCLATURE"); //Désignation 1
                data[i++] = FormatDesignation(""); //Désignation 2
                data[i++] = FormatDesignation(""); //Désignation 3
                data[i++] = FormatDesignation(""); //Désignation 4
                data[i++] = FormatDesignation(""); //Désignation 5
                data[i++] = FormatDesignation(""); //Désignation 6
                data[i++] = "NOMEN"; //Centre de frais

                data[i++] = "0"; //Tps Prep
                data[i++] = "0"; //Tps Unit
                data[i++] = "0"; //Coût Opération
                data[i++] = "0"; //Taux horaire
                data[i++] = GetFieldDate(quoteEntity, "_CREATION_DATE"); //Date
                data[i++] = GetQuoteNumber(quoteEntity); //N° devis
                data[i++] = ""; //Nom fichier joint
                data[i++] = "0"; //N° identifiant GED 1
                data[i++] = "0"; //N° identifiant GED 2
                data[i++] = "0"; //N° identifiant GED 3
                data[i++] = "0"; //N° identifiant GED 4
                data[i++] = "0"; //N° identifiant GED 5
                data[i++] = "0"; //N° identifiant GED 6
                data[i++] = "0"; //Niveau du rang
                data[i++] = ""; //Observations
                data[i++] = ""; //Lien avec la phase de nomenclature
                data[i++] = ""; //Date dernière modif
                data[i++] = ""; //Employé modif
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
                
                //long supplyQty = Convert.ToInt64(doubleSupplyQty);
                //ecriture des quantité decimales
                double supplyQty = Convert.ToDouble(doubleSupplyQty);
                if (supplyQty > 0)
                {
                    nomendvPhase = nomendvPhase + 10;
                    i = 0;
                    data = new string[50];

                    data[i++] = "NOMENDV";
                    data[i++] = GetQuoteNumber(quoteEntity); //Code devis
                    data[i++] = reference; //Code pièce
                    data[i++] = rang; //Rang
                    data[i++] = nomendvPhase.ToString(formatProvider); //Phase
                    data[i++] = ""; //Repère
                    data[i++] = EmptyString(supplyTypeEntity.GetFieldValueAsString("_REFERENCE")); ; //Code article
                    data[i++] = FormatDesignation(supplyTypeEntity.GetFieldValueAsString("_DESIGNATION")); //Désignation 1
                    data[i++] = FormatDesignation(""); //Désignation 2
                    data[i++] = FormatDesignation(""); //Désignation 3
                    data[i++] = ""; //Temps de réappro
                    data[i++] = (supplyQty * parentQty).ToString(); //Qté
                    data[i++] = (supplyEntity.GetFieldValueAsDouble("_IN_COST") / supplyQty).ToString("#0.0000", formatProvider); //Px article ou Px/Kg
                    data[i++] = supplyEntity.GetFieldValueAsDouble("_IN_COST").ToString("#0.0000", formatProvider); //Prix total
                    data[i++] = ""; //Code Fournisseur
                    data[i++] = ""; //2sd fournisseur
                    data[i++] = "1"; //Type
                    data[i++] = ""; //Prix constant
                    data[i++] = ""; //Poids tôle ou article
                    data[i++] = "DEVIS"; //Famille
                    data[i++] = ""; //N° tarif de Clipper
                    data[i++] = supplyEntity.GetFieldValueAsString("_COMMENTS") ; //Observation
                    data[i++] = ""; //Observation interne
                    data[i++] = ""; //Observation débit
                    data[i++] = ""; //Val Débit 1
                    data[i++] = ""; //Val Débit 2
                    data[i++] = ""; //Qté Débit
                    data[i++] = ""; //Nb pc/débit ou débit/pc
                    data[i++] = ""; //Lien avec la phase de gamme
                    data[i++] = ""; //Unite de quantité
                    data[i++] = ""; //Unité de prix
                    data[i++] = ""; //Coef Unite
                    data[i++] = ""; //Coef Prix
                    data[i++] = modele; //Prix constant ??? semble plutot correcpondre au Modèle
                    data[i++] = "0"; //Modèle ??? semble plutot correcpondre au Prix constant
                    data[i++] = ""; //Qté constant
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
                stringData = stringData + "¤" + data[i];
            }
            stringData = stringData + "¤" + Environment.NewLine;
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
        ///remplacement les sauts de ligne par des espaces
        /// pas de gestion des rtf car specifique clipper sur les 
        /// pad de gestion des caracteres spéciaux (valeur_sortie=Regex.Replace(valeur_entrée, "[^a-zA-Z0-9_]", "");)
        private static string FormatComment(string Comment)
        {   //remplace les sauts de ligne par les espaces
            //
            Comment=Comment.Replace("\r\n", " ");

            return Comment;
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
        private string GetDprPath(IEntity partEntity, IQuote quote)
        {
            try {

                //create dpr and directory
                _PathList.TryGetValue("Export_DPR_Directory", out string dpr_directory);
                _PathList.TryGetValue("Custom_Export_DPR_Directory", out string Custom_dpr_directory);

                if (!string.IsNullOrEmpty(dpr_directory))
                {
                    // Dictionary<string, string> filelist = ExportDprFiles(quote, dpr_directory);//AF_Export_Devis_Clipper.Export_Quote_Files.Export(quote);
                    Dictionary<string, string> filelist = ExportDprFiles(quote, Custom_dpr_directory);//AF_Export_Devis_Clipper.Export_Quote_Files.Export(quote);

                }


                ///on laisse pour le moment
                ///
                string assistantType = partEntity.GetFieldValueAsString("_ASSISTANT_TYPE");
                string partFileName = partEntity.GetFieldValueAsString("_FILENAME");


                //ATTENTION LES GENERATION DES DPR DEPEND DE LA LICENCE/ IL FAUT UNE QUOTE CUT 
                //SINON IL S4AGIT DE PIECES ALMACAM
                //recuperation des emf par defaut
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

                //recuperation et construction du chemin des parts

                {

                    if (!string.IsNullOrEmpty(dpr_directory))
                    {

                        string empty_emfFile;

                        //cas général
                        emfFile = partEntity.GetFieldValueAsString("_DPR_FILENAME") + ".emf";
                        //emfFile vide
                        empty_emfFile = dpr_directory + "\\" + "Quote_" + quote.QuoteEntity.Id + "\\" + partEntity.GetFieldValueAsString("_REFERENCE") + ".dpr.emf"; // + Path.GetFileName(partEntity.GetImageFieldValueAsLinkFile("_PREVIEW"));
                                                                                                                                                                     //cas general
                        emfFile = GetEmfFile(partEntity, empty_emfFile);

                        /// piece dpr existantes passées par l'assistant dpr
                        if (assistantType.Contains("DprAssistant") || isGenericDpr)
                        {

                            emfFile = partEntity.GetFieldValueAsString("_FILENAME") + ".emf";

                        }

                        //traitement spé
                        //cas des pieces simples//
                        else if (assistantType.Contains("PluggedSimpleAssistantEx"))
                        {
                            //creation du point rouge dans l'emf : signature des apercus de pieces quotes
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
                        emfFile = partEntity.GetImageFieldValueAsLinkFile("_PREVIEW");
                    }

                    
                }

                return emfFile;
                ///return string.Empty;



            } catch (Exception ie) { return string.Empty; }


        }
        #endregion
        # region Document
        /// <summary>
        /// ecriteure de l'attacjhement DOCUMENT
        /// sous la forme DOCUMENT¤3¤<RépertoireSociété>\Documents\403 704 275 0.jpg¤403 704 275 0¤0
        /// </summary>
        private bool Write_Documents(IEntity Part, long quote_id,ref string file)
        {
            try {


                IAttachmentValueList part_attachement_list = Part.GetFieldValue("_ATTACHMENTS") as IAttachmentValueList;

                if (part_attachement_list.Count>0)
                {

                string ExportDirectory = "";
                _PathList.TryGetValue("Export_GP_Directory", out ExportDirectory);

                  foreach (IAttachmentValue document in part_attachement_list)
                    {

                        string filename = Path.GetFileName(document.FileName);
                        string outputdirectory = ExportDirectory + "\\attachements\\" + quote_id.ToString() ;
                        CreateDirectory(outputdirectory);
                        string fullfileName = outputdirectory + "\\" + Path.GetFileName(document.FileName);
                        part_attachement_list.SaveAs(document, @fullfileName);
                    
                    //stockTarget.FieldAddAttachment("_CCPU", tempfileName);
                    //File.Delete(tempfileName);
                    //repertoire export
                    //
                   
                    
                    file += "DOCUMENT¤3¤"+ @fullfileName + "¤"+Path.GetFileNameWithoutExtension(filename)+ "¤0¤\r\n";


                    }
                }






                return true;
            } catch (Exception ie) {


                return false;

            }

        }
        /// <summary>
        /// ecriture du devis dans un dossier externalisé pour consultations
        /// </summary>
        private bool Write_Documents_Quote_Pdf(IEntity quoteEntity)
        {
            try
            {


                IAttachmentValueList quote_attachement_list = quoteEntity.GetFieldValue("_PROPOSAL") as IAttachmentValueList;
            
                if (quote_attachement_list.Count > 0)
                {

                    string ExportDirectory = "";
                    _PathList.TryGetValue("Export_GP_Directory", out ExportDirectory);

                    foreach (IAttachmentValue document in quote_attachement_list)
                    {

                        string filename = Path.GetFileName(document.FileName).Replace(@"\\Wpm.Implement.Manager.Entity","");
                              string outputdirectory = ExportDirectory + "\\attachements";
                        CreateDirectory(outputdirectory);
                        string fullfileName = outputdirectory + "\\" + filename;
                        quote_attachement_list.SaveAs(document, @fullfileName);

                  

                    }
                }






                return true;
            }
            catch (Exception ie)
            {


                return false;

            }

        }
        #endregion 


    }
   
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
            MessageBox.Show(base.Message, string.Format("Probleme sur le status ou l'existance du devis selectionné" ,"", DateTime.Now.ToLongTimeString()), MessageBoxButtons.OK, MessageBoxIcon.Error);
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

    #endregion


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
   
  
}
