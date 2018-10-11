using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Odbc;
using System.IO;
using Wpm.Implement.Processor;
using AF_ImportTools;
using Wpm.Implement.Manager;
using Wpm.Schema.Kernel;
using System.Diagnostics;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AF_Import_ODBC_Clipper_AlmaCam

{
   //= new TextWriterTraceListener(System.IO.Path.GetTempPath() + "\\" + Properties.Resources.ImportTubeLog);
    #region  commandes
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// creation des commandes d'imports
    /// using (Clipper_Import_Matiere Update_Material = new Clipper_Import_Matiere())
     ///    {
     ///                try
     ///                {
     ///           Update_Material.Almacam_Update_Material(contextlocal);
        ///        Update_Material.Close();
        ///         Cursor.Current = Cursors.Default;
        ///
        ///             }
    ///              catch { Cursor.Current = Cursors.Default; }
    //      }
    /// </summary>
    //section des commandes





    public class Clipper_ImportMatiere_Processor : CommandProcessor
    {
        public override bool Execute()
        {
            
            try {
                //creation des logs
                TextWriterTraceListener logFile;

                using (Clipper_Import_Matiere Update_Material = new Clipper_Import_Matiere())
                {
                    try {
                    Cursor.Current = Cursors.WaitCursor;
                    string DbName = Alma_RegitryInfos.GetLastDataBase();
                    IModelsRepository modelsRepository = new ModelsRepository();
                    IContext contextlocal = modelsRepository.GetModelContext(DbName);

                    Update_Material.Almacam_Update_Material(contextlocal);
                    Update_Material.Close();
                        Cursor.Current= Cursors.Default;
                    }
                    catch { Cursor.Current = Cursors.Default; }
                }



                return base.Execute();
            }
            catch
            {
                return base.Execute();
            }
           
        }
    }


/// <summary>
    
    //    bool import_matiere = true;
    // bool tube_rond = true;
    //bool rond = true;
    //bool tube_rectangle = true;
    //bool tube_carre = true;
    //bool tube_flat = true;
    //bool tube_speciaux = true;
    //  Clipper_ImportTubes_Processor tubeimporter = new Clipper_ImportTubes_Processor(import_matiere, tube_rond, rond, tube_rectangle, tube_carre, tube_flat, tube_speciaux);
    //  tubeimporter.Execute();

/// </summary>

    public class Clipper_ImportTubes_Processor : CommandProcessor
    {
        Boolean Import_Matiere = true;
        Boolean Tube_Rond = true;
        Boolean Tube_Speciaux = true;
        Boolean Rond = true;
        Boolean Tube_Rectangle = true;
        Boolean Tube_Carre = true;
        Boolean Tube_Flat = true;

        /// <summary>
        /// constructeur de l'import des tube, tu stock et des matieres.........
        /// import_matiere,  tube_rond,  rond,  tube_rectangle,  tube_carre, tube_flat,  Tube_Speciaux
        /// </summary>
        /// <param name="import_matiere">true ou false pour declencher l'import</param>
        /// <param name="tube_rond">true ou false pour declencher l'import</param>
        /// <param name="rond">true ou false pour declencher l'import</param>
        /// <param name="tube_rectangle">true ou false pour declencher l'import</param>
        /// <param name="tube_carre">true ou false pour declencher l'import</param>
        /// <param name="tube_flat">true ou false pour declencher l'import</param>
        /// <param name="Tube_Speciaux">true ou false pour declencher l'import</param>
        /// 
        public Clipper_ImportTubes_Processor(bool import_matiere, bool tube_rond, bool rond, bool tube_rectangle,  bool tube_carre, bool tube_flat, bool tube_speciaux)
        {

                                    Import_Matiere = import_matiere;
                                    Tube_Rond = tube_rond;
                                    Rond = rond;
                                    Tube_Rectangle = tube_rectangle;
                                    Tube_Carre = tube_carre;
                                    Tube_Flat = tube_flat;
                                    Tube_Speciaux = tube_speciaux;
                                    

        }




        public override bool Execute()
        {
            //creation des logs
            TextWriterTraceListener logFile;
            try
            {   
                //detection du contexte
                Cursor.Current = Cursors.WaitCursor;
                string DbName = Alma_RegitryInfos.GetLastDataBase();
                IModelsRepository modelsRepository = new ModelsRepository();
                IContext contextlocal = modelsRepository.GetModelContext(DbName);
                //creation des logs
                //creation du listener
                ////
                if (Import_Matiere) { 
                using (Clipper_Import_Matiere Update_Material = new Clipper_Import_Matiere())
                {
                    try
                    {
                        Update_Material.Almacam_Update_Material(contextlocal);
                        Update_Material.Close();
                        Cursor.Current = Cursors.Default;
                    }
                    catch { Cursor.Current = Cursors.Default; }
                }

                }


                if (Tube_Speciaux)
                {
                    using (Clipper_Import_Tube_Speciaux tubesronds = new Clipper_Import_Tube_Speciaux(contextlocal))
                    {
                        try
                        {

                            tubesronds.ReadTubes();
                            tubesronds.WriteTubes();
                            tubesronds.Close();

                        }
                        catch (Exception ie) { }

                    }
                }



                if (Tube_Rond) { 
                        using (Clipper_Import_Tube_Rond tubesronds = new Clipper_Import_Tube_Rond(contextlocal))
                        {
                            try
                            {

                                tubesronds.ReadTubes();
                                tubesronds.WriteTubes();
                                tubesronds.Close();

                            }
                            catch (Exception ie) { }
                    
                        }
                }

                if (Rond) { 
                        using (Clipper_Import_Rond tubesronds = new Clipper_Import_Rond(contextlocal))
                        {
                            try
                            {


                                tubesronds.ReadTubes();
                                tubesronds.WriteTubes();
                                tubesronds.Close();

                            }
                            catch (Exception ie) { }

                        }
                }

                if (Tube_Rectangle) { 

                        using (Clipper_Import_Tube_Rectangle tubesrectangle = new Clipper_Import_Tube_Rectangle(contextlocal))
                        {
                            try
                            {
                                tubesrectangle.ReadTubes();
                                tubesrectangle.WriteTubes();
                                tubesrectangle.Close();

                            }
                            catch (Exception ie) { }


                        }
                }
                if (Tube_Carre) {
                        using (Clipper_Import_Tube_Carre tubescar = new Clipper_Import_Tube_Carre(contextlocal))
                        {
                            try
                            {
                                tubescar.ReadTubes();
                                tubescar.WriteTubes();
                                tubescar.Close();

                            }
                            catch (Exception ie) { }


                        }
                }

                if (Tube_Flat) { 
                    using (Clipper_Import_Tube_Flat  tubesflats = new Clipper_Import_Tube_Flat (contextlocal))
                        {
                            try
                            {
                                tubesflats.ReadTubes();
                                tubesflats.WriteTubes();
                                tubesflats.Close();

                            }
                            catch (Exception ie) { }


                            }
                }

                return base.Execute();
            }
            catch
            {
                return base.Execute();
            }

        }
    }




    #endregion
    #region json
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// gestion des fichier json (partira dans import tools plus tard
    /// </summary>
    //section JSON
    public class JsonTools
        {
            //recupee le fichier json dans le dossier de la dll
            private string PATH = Directory.GetCurrentDirectory()+"\\"+ Properties.Resources.jsonimportclipper;//"import-clipper.json";

            public string getJsonStringParametres(string ParameterName)
            {
                string returnvalue = "";

                using (StreamReader file = File.OpenText(this.PATH))
                {
                   
                   
                    using (JsonTextReader reader = new JsonTextReader(file))
                    {
                        JObject o = (JObject)JToken.ReadFrom(reader);


                        returnvalue = (string)o.SelectToken(ParameterName);
                    }
                    

                }

                return returnvalue;


            }


        }
    #endregion

    #region Dll

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// routine d'import satndards, matiere, tubes.. toles, vis......
    /// </summary>
    //section des commandes
    public enum TypeTube
    {
        Aucun = 0,
        Rond = 4,
        Round = 5,
        Rectangle = 10,
        Flat = 3,
        Speciaux = 6
    }

    /// <summary>
    ///   class Clipper_Material.. class de stockage des matieres
    /// </summary>
    public class Clipper_Material
        {
        //string prefix; // prefixe article matiere
        //definiton de l'objetc material
            public string Coarti = "UNDEF"; //code etat (nuance epaisseur)
            public double Prixart=0;// prixart
            public double Densite=1;//
            public string Nuance= "UNDEF";
            public string Etat = "";//code nuance
            public string Quality; //quality
            public double Thickness = 0;  //DIM1
            private string Materialname = "UNDEF";
            public string Comments = "";
        //champs clipper//
        //string clippernuancedesc; //nuance
        //string clipperetatdesc; //etat
        //string clippernuancecode;
        //string clipperetatcode;
        //string Cofou;
        //string Cofa;
        //double Dernierprix;
        //public double Price=0;
        //DateTime Dateso;
        //aucun interet
        public string getNuance() { return Nuance + "*" + Etat; }
            public double getThickess() { return Thickness; }

        //construit le nom matiere en concatenant nuance*etat
        public void setMaterialName()
            {

                if (Etat != string.Empty)
                {
                            Quality = Nuance + "*" + Etat;
                            Materialname = Nuance + "*" + Etat + " " + Thickness + " mm";
                    
                }
                else
                {
                        Quality = Nuance ;
                        Materialname = Nuance + " " + Thickness + " mm";
                }

                // return Materialname ;
            }

        public string getMaterialName() {return  Materialname; }

        }
    /// <summary>
    ///   class Clipper_IMport matiere.. derive de clipper article
    /// </summary>
    public class Clipper_Import_Matiere : IDisposable
        {
            private string DSN;
            private string SQL;
            private string SQL_NUANCE_ETAT;
            private JsonTools JSTOOLS;
            private OdbcDataReader TABLE_ARTICLEM_TOLE;


            private OdbcConnection DbConnection;
            private OdbcCommand DbCommand;
            IList<Clipper_Material> MATERIAL_LIST = new List<Clipper_Material>();
        //recuperation des parametres

        //creation du listener
        TextWriterTraceListener logFile = new TextWriterTraceListener(System.IO.Path.GetTempPath() +"\\"+ Properties.Resources.ImportMatiereLog);

        

          public IList<Clipper_Material> GetMaterial_List() { return MATERIAL_LIST; }


            public Clipper_Import_Matiere(string dsn, string sqlTole)
            {
                //material matiere;                       
                DSN = dsn;
                SQL = sqlTole;
          
                Init();
            }

            public void Dispose()
            {
                //Dispose(true);
                GC.SuppressFinalize(this);
            }

            public Clipper_Import_Matiere()
            {

           
            logFile.Write("debut import matiere ");
            Init();
            }

            private void Init()
            {    ///premier paramertre dsn = clipper dsn= data source name
                try
                {
                   logFile.WriteLine("initialisatin de la methode");

                //lecture du fichier json
                    logFile.WriteLine("lecture du fichier Json");

                    //Material matiere = new material();
                    JSTOOLS = new JsonTools();
                    //recup parametre dsn
                    DSN = JSTOOLS.getJsonStringParametres("dsn");
                    logFile.WriteLine("DSN detecté "+ DSN);

                    DbConnection = null;
                    DbCommand = null;
                    this.DbConnection = new OdbcConnection("DSN=" + DSN);
                    this.DbConnection.Open();
                    this.DbCommand = DbConnection.CreateCommand();
                    logFile.WriteLine("etat de la connexion " + DbConnection.State.ToString());


                GetMaterial();


                    //Close();


                }
                catch (Exception ie)
                {
                //
                logFile.WriteLine("ERREUR initialisation connexion: " +ie.Message.ToString());
                //Console.Write(ie.Message.ToString());

                }
            }
            private void GetMaterial()
            {
                //IList <Material> materialist = new List<Material>();
                //string prefix; // prefixe article matiere
                //string grade;//code nuance
                //string name; //code etat (nuance epaisseur)
                //double price; // prix
                //double densité;
                //double Thickness;
                int ii = 0;
            logFile.WriteLine("paramétrage de la requete du fichier json " );
            logFile.WriteLine("monodim = " + JSTOOLS.getJsonStringParametres("multidim"));
            //recuperation des nuance etat--> 304L*DKP
            if (JSTOOLS.getJsonStringParametres("multidim") != "True")
            //requete monodim 
            { this.SQL_NUANCE_ETAT = JSTOOLS.getJsonStringParametres("sql.nuance_etat_monodim"); }
                //requete multidim
            else
            {  this.SQL_NUANCE_ETAT = JSTOOLS.getJsonStringParametres("sql.nuance_etat");}
                                                                                      //coupel nuance etat
                                                                                      //Material matiere = new material();
                                                                                      //getJsonParametres();
            this.DbCommand.CommandText = this.SQL_NUANCE_ETAT;
            logFile.WriteLine("requete utilisée = " + this.SQL_NUANCE_ETAT);
            // requete type matiere
            /*" SELECT ARTICLEM.COARTI, ARTICLEM.ETATMAT,ARTICLEM.NUANCE, ARTICLEM.PRIXART, FAMILLE.MULTIDIM, "+
            "Tech_EtatMatiere.Etat, Tech_NuanceMatiere.Nuance,Tech_NuanceMatiere.Densite "+
             "FROM ARTICLEM LEFT JOIN FAMILLE ON FAMILLE.COFA = ARTICLEM.COFA "+
            "LEFT JOIN Tech_EtatMatiere ON Tech_EtatMatiere.Libelle = ARTICLEM.ETATMAT "+ 
            "LEFT JOIN Tech_NuanceMatiere  ON Tech_NuanceMatiere.Libelle=ARTICLEM.NUANCE "+
            "WHERE  ARTICLEM.CHUTE<>'O' AND FAMILLE.DIMENSIONS=3; ";*/


            //recuperation des enregistrements
            TABLE_ARTICLEM_TOLE = DbCommand.ExecuteReader();

                while (TABLE_ARTICLEM_TOLE.Read())
                {
                    ii++;
                    Clipper_Material material = new Clipper_Material();
                    material.Coarti = TABLE_ARTICLEM_TOLE["COARTI"].ToString().Trim();
                    material.Nuance = TABLE_ARTICLEM_TOLE["CODENUANCE"].ToString().Trim();
                    material.Etat = TABLE_ARTICLEM_TOLE["CODEETAT"].ToString().Trim();
                    material.Prixart = Convert.ToDouble(TABLE_ARTICLEM_TOLE["PRIXART"]);
                    material.Thickness = Convert.ToDouble(TABLE_ARTICLEM_TOLE["EPAISSEUR"]);
                    material.Densite = Convert.ToDouble(TABLE_ARTICLEM_TOLE["DENSITE"]);
                //issue du tube ou non
                    if (TABLE_ARTICLEM_TOLE["TYPE"].ToString().Trim() == "3") {material.Comments = "" ; } else { material.Comments = "Matiere_Tube" ; }
                    
                    material.setMaterialName();
                    MATERIAL_LIST.Add(material);


                };
            logFile.WriteLine("reconstitution de la table des matieres terminés : " + this.MATERIAL_LIST.Count().ToString()+ " trouvées ");
            //MATERIAL_LIST.OrderBy(i => i.Quality);


        }
            public void Close()
            {

                // reader.Close();

                DSN = "";
                SQL = "";
                TABLE_ARTICLEM_TOLE.Close();
                DbConnection.Dispose();
                DbCommand.Dispose();
                MATERIAL_LIST = null;
                DbCommand.Dispose();
                DbConnection.Close();

                logFile.Close();

        }

            //ecrit les nouvelles matieres dans almacam
            public void Almacam_Update_Material(IContext contextlocal) {

            //  logFile.WriteLine("mise a jour des matieres dans la base " + contextlocal.Model.DatabaseName);
            //ecrirture de la liste des matiere//
                IEntityList qualityentitylist = contextlocal.EntityManager.GetEntityList("_QUALITY");
                IList<string> qualities_To_Create = new List<string>();
                qualityentitylist.Fill(false);
                IList<IEntity> qualitylist = new List<IEntity>();
                qualitylist = qualityentitylist.ToList();
                bool newdatabase = qualitylist.Count == 0;
            ////liste des qualité de materiaux
                IDictionary<long,string> updatedstringqualitylist = new Dictionary<long,string>();

            //////
            ///////detection des qualités a creer//// 
            logFile.WriteLine("creation dela liste des qualité de clipper ");
            foreach (Clipper_Material m in MATERIAL_LIST)
            {
               

                if (qualities_To_Create.Contains(m.Quality)==false)
                {
                           qualities_To_Create.Add(m.Quality);
                          
                }
               



            }
            logFile.WriteLine(qualities_To_Create.Count().ToString()+" qualités trouvées ");
            ///////////
            ///creation des qualité dans la base si necessaire
            
            
            foreach (string quality in qualities_To_Create)
            {

                // Find material           
                IEntity currentQuality = qualitylist.Where(q => q.DefaultValue.Equals(quality)).FirstOrDefault();

                if (currentQuality == null)
                { //creation de la nuance et sauvegarde// ou retour de la qualité courente
                    currentQuality = contextlocal.EntityManager.CreateEntity("_QUALITY");
                    currentQuality.SetFieldValue("_NAME", quality);
                    logFile.WriteLine("creation de la nouvelle qualité  "+ quality);
                    currentQuality.Save();

                }




            }

            logFile.WriteLine( " qualités crées ");

            qualityentitylist.Fill(false);
            qualitylist = qualityentitylist.ToList();

            ///////////
            ///creation des matieres assocées au qualités
          

            foreach (Clipper_Material m in MATERIAL_LIST) {

                // Find material           
                IEntity currentQuality = qualitylist.Where(q => q.DefaultValue.Equals(m.Quality)).FirstOrDefault();
                
                
                //update
                currentQuality.SetFieldValue("_NAME", m.Quality);
                currentQuality.SetFieldValue("_DENSITY", (m.Densite)/1000);
                //calcul du prix moyen toutes epaisseurs confondues
                currentQuality.SetFieldValue("_BUY_COST",( MATERIAL_LIST.Average(p => p.Prixart)));
                currentQuality.SetFieldValue("_OFFCUT_COST", (MATERIAL_LIST.Average(p => p.Prixart)/1000));
                currentQuality.SetFieldValue("_COMMENTS", m.Comments); 

                currentQuality.Save();

                //on rempli la liste des qualités
                if (currentQuality != null) { 
                        if (!updatedstringqualitylist.ContainsKey(currentQuality.Id) )
                        {
                            updatedstringqualitylist.Add(currentQuality.Id,m.Quality);
                        }
                }

            }


            //création des matieres   ---> pas d'unicité (voir dans la requete)
            logFile.WriteLine("creation des matieres assocées aux qualités  ");
            foreach (var currentstringquality in updatedstringqualitylist)
            {
                {   //recuperation de la lisre de matieres associée avec la qualité

                 
                    IEntityList almacam_materialentitylist = contextlocal.EntityManager.GetEntityList("_MATERIAL", "_QUALITY", ConditionOperator.Equal, currentstringquality.Key);
                    almacam_materialentitylist.Fill(false);

                    IList<IEntity> almacam_material_list = new List<IEntity>();
                    almacam_material_list = almacam_materialentitylist.ToList();

                    foreach (Clipper_Material m in this.MATERIAL_LIST)
                    {
                        if (m.Quality == currentstringquality.Value)
                        {
                            IEntity currentmaterial = almacam_material_list.Where(q => q.GetFieldValueAsString("_NAME").Equals(m.getMaterialName())).FirstOrDefault();
                            
                            if (currentmaterial == null)
                            { //creation de la matiere et sauvegarde//

                                currentmaterial = contextlocal.EntityManager.CreateEntity("_MATERIAL");
                                logFile.WriteLine("creation de la matieres   " + m.getMaterialName());

                            }
                            //update and save
                                logFile.WriteLine("mise à jour de la matiere :   " + m.getMaterialName());
                                currentmaterial.SetFieldValue("_NAME", m.getMaterialName());
                                currentmaterial.SetFieldValue("_QUALITY", currentstringquality.Key);
                                currentmaterial.SetFieldValue("_THICKNESS", m.Thickness);
                                currentmaterial.SetFieldValue("_BUY_COST", (m.Prixart)/1000);
                                currentmaterial.SetFieldValue("_CLIPPER_CODE_ARTICLE", m.Coarti);
                                currentmaterial.SetFieldValue("_COMMENTS","Issue de la famille " + m.Comments +", Densité=" +m.Densite);
                                currentmaterial.Save();

                        }
                    }


                }
            



            }
            //*//

            logFile.Flush();
        }





    }
    /// <summary>
    /// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>
    /// 
    /// 
    /// <summary>
    ///   class de base clipper article
    /// </summary>
    public class Clipper_Article
    {
        //string prefix; // prefixe article matiere
        //creation du listener

            public IContext contextlocal;

            public string Coarti = "UNDEF"; //code etat (nuance epaisseur)
            public double Prixart;// prixart
            public double Densite;//
            public IEntity Material;
            public string Nuance;
            public string Etat = "";
            //public double Thickness = 0;  //DIM3
            public string Name = "UNDEF";
            public string Cofa = "undef";
            private string DSN;
            // private string SQL;
            // private string SQL_TUBE;
            public OdbcDataReader TABLE_ARTICLEM;//TABLE_ARTICLEM;
                                                      //public OdbcDataReader TABLE_ARTICLEM_ROND;
            public OdbcConnection DbConnection;
            public OdbcCommand DbCommand;

            public void Dispose()
            {
            TABLE_ARTICLEM = null;
            //TABLE_ARTICLEM_ROND = null;
            DSN = null;
            }
        public JsonTools JSTOOLS;
       
        IList<Clipper_Material> TUBE_LIST = new List<Clipper_Material>();

        public Clipper_Article()
        {
            
           // Odbc_Connexion();//
        }

        public void Odbc_Connexion()
        {    ///premier paramertre dsn = clipper dsn= data source name
            try
            {

                //Material matiere = new material();
                JSTOOLS = new JsonTools();
                //recup parametre dsn
                DSN = JSTOOLS.getJsonStringParametres("dsn");

                //recupe requete
                DbConnection = null;
                DbCommand = null;
                this.DbConnection = new OdbcConnection("DSN=" + DSN);
                this.DbConnection.Open();
                this.DbCommand = DbConnection.CreateCommand();

                //logFile.WriteLine("connexion ok    : " + DSN);


            }
            catch
            {

            }
        }


        public string getSqlNumericValue(string fieldname)
        {
            try
            {
                string value;
                if (TABLE_ARTICLEM[fieldname].ToString().Trim() == string.Empty) { return "0"; }
                else { return TABLE_ARTICLEM[fieldname].ToString().Trim(); }

            }
            catch { return "0"; }

        }



        public void Close()
        {

            


            DSN = "";
            //SQL = "";
            TABLE_ARTICLEM.Close();

            DbConnection.Dispose();
            DbCommand.Dispose();
            //IList<Material> MATERIAL_LIST = new List<Material>();

            DbCommand.Dispose();
            DbConnection.Close();
           
            //logFile.WriteLine("connexion    : Fin" );
            //logFile.Close();
            //logFile.Flush();
            //logFile.Dispose();


        }


        public IEntity GetGrade(IContext contextlocal, string nuance, string etat)
        {
            try
            {
                //en cas de lenteru on pourrais stocker toutes les gardes poour eviter de faire troo de requetes
                //nous verrons
                IEntityList grades;
                IEntity grade;
                string qualityname;
                if (etat == "") { qualityname = nuance; } else { qualityname = nuance + "*" + etat; }

                grades = contextlocal.EntityManager.GetEntityList("_QUALITY", "_NAME", ConditionOperator.Equal, qualityname);
                grades.Fill(false);

                if (grades.Count() > 0)
                {
                    grade = grades.FirstOrDefault();
                

                }
                else
                {
                    grade = null;
                  
                }


                return grade;


            }
            catch (Exception ie) { return null; }
        }
      

    }

    /// <summary>
    ///   class Clipper_Article_Tube.. derive de clipper article
    /// </summary>
    public class Clipper_Article_Tube :Clipper_Article
    {
        //string prefix; // prefixe article matiere
        //creation du listener
        //tools
        public string Section_Key; //"_SECTION_CIRCLE" or....
        public List<string> SectionExclusion = new List<string>();
        public List<string> TubeExclusion = new List<string>();
       
        
      

       /* public JsonTools JSTOOLS;*/

        IList<Clipper_Material> TUBE_LIST = new List<Clipper_Material>();

        public Clipper_Article_Tube()
        {

            Odbc_Connexion();
        }



        /// <summary>
        /// retourn une liste d'exclusion des sections ou des tubes
        /// </summary>
        /// <param name="entitype"></param>
        /// <param name="entity_uniquestring_field"></param>
        /// <returns></returns>
        public List<string> getSectionExclusionList(string entitype)
        {
            try
            {

                IEntityList exclusionlist;
                exclusionlist = contextlocal.EntityManager.GetEntityList(entitype);
                exclusionlist.Fill(false);
                List<string> exclusion = new List<string>();

                // Find material           
                //Entity currentQuality = qualitylist.Where(q => q.DefaultValue.Equals(quality)).FirstOrDefault();
                foreach (IEntity ex in exclusionlist)
                {
                    string uid = ex.GetImplementEntity("_SECTION").GetFieldValueAsString("_NAME").Trim();
                    ex.GetImplementEntity(Section_Key);
                    if (exclusion.Contains(uid) == false)
                    { exclusion.Add(uid); }

                }


                return exclusion;
            }

            catch
            {

                return null;
            }


        }

        /// <summary>
        /// retourn une liste d'exclusion des sections ou des tubes
        /// </summary>
        /// <param name="entitype"></param>
        /// <param name="entity_uniquestring_field"></param>
        /// <returns></returns>
        public List<string> getExclusionList(string entitype, string uid_field)
        {
            try
            {

                IEntityList exclusionlist;
                exclusionlist = contextlocal.EntityManager.GetEntityList(entitype);
                //exclusionlist[0].GetFieldValueAsEntity("_SECTION").ImplementedEntityType.Key
                exclusionlist.Fill(false);
                List<string> exclusion = new List<string>();

                // Find material           
                //Entity currentQuality = qualitylist.Where(q => q.DefaultValue.Equals(quality)).FirstOrDefault();
                foreach (IEntity ex in exclusionlist)
                {
                    string uid = ex.GetFieldValueAsString(uid_field).Trim();

                    ex.GetImplementEntity(Section_Key);
                    if (exclusion.Contains(uid) == false)
                    { exclusion.Add(uid); }

                }


                return exclusion;
            }

            catch
            {

                return null;
            }


        }
       

        public long getExistingSection(string section_name,string section_key)
        {

            try
            {
                //en cas de lenteur on pourrais stocker toutes les gardes poour eviter de faire troo de requetes
                //nous verrons

                IExtendedEntityList sections;
                IExtendedEntity section=null;
                long section_id =0;

                if (section_name != "")
                {
                    //sectionEntity.GetImplementEntity("_SECTION").SetFieldValue("_NAME", name);

                    sections = contextlocal.EntityManager.GetExtendedEntityList(contextlocal.Kernel.GetEntityType(section_key));
                    sections.Fill(false);

                    foreach (IExtendedEntity s in sections.AsExtendedEntityList)
                    {
                        object o = s.GetFieldValue(section_key + "\\IMPLEMENT__SECTION\\_NAME");
                        if (o.ToString() == section_name) {
                            section_id = s.Id32;
                            section = s;
                            break;

                        }
                    }





                    return section_id;
                 
                }
                else
                { return 0; }
             


            }
            catch (Exception ie) { return 0; }


        }
        public long getExistingImplementedSection(string section_name, string section_key)
        {

            try
            {
                //en cas de lenteur on pourrais stocker toutes les gardes poour eviter de faire troo de requetes
                //nous verrons

                IExtendedEntityList sections;
                IExtendedEntity section = null;
                long section_id = 0;

                if (section_name != "")
                {
                    //sectionEntity.GetImplementEntity("_SECTION").SetFieldValue("_NAME", name);

                    sections = contextlocal.EntityManager.GetExtendedEntityList(contextlocal.Kernel.GetEntityType(section_key));
                    sections.Fill(false);

                    foreach (IExtendedEntity s in sections.AsExtendedEntityList)
                    
                    {
                        object o = s.GetFieldValue(section_key + "\\IMPLEMENT__SECTION\\_NAME");
                        object id= s.GetFieldValue(section_key + "\\IMPLEMENT__SECTION");
                        if (o.ToString() == section_name)
                        {
                            section_id =(long) id;

                            //section = s;
                            break;

                        }
                        
                    }





                    return section_id;

                }
                else
                { return 0; }



            }
            catch (Exception ie) { return 0; }


        }
        /// <summary>
        /// lecture des tube de la base clipper
        /// </summary>
        public virtual void ReadTubes() { }
        /// <summary>
        /// ecriture des tubes dans almacam
        /// </summary>
        public virtual void WriteTubes() { }

        public virtual void DimInterpretor() { }
    }
    /// <summary>
    /// class TubeRond.. derive de clipper article
    /// class tube rond
    /// on ajoute longeur et diametre
    /// </summary>
    public class TubeRond : Clipper_Article
    {
        

        //private List<TubeRond>listeTubeRond;
        private TypeTube type=TypeTube.Rond;
        public double Diametre;
        public double Longueur;
        public double epaisseur;


    }


    /// <summary>
    /// class TubeRec.. derive de clipper article
    /// </summary>
    public class TubeRec : Clipper_Article
    {

        //private List<TubeRond>listeTubeRond;
        private TypeTube type = TypeTube.Rectangle;
        public double Largeur;
        public double Hauteur;
        public double Longueur;
        public double epaisseur;

    }

    public class TubeSpe : Clipper_Article
    {

        //private List<TubeRond>listeTubeRond;
        private TypeTube type = TypeTube.Speciaux;
        public double Longueur;
        public string Section;
        public string Section_Spe_Key;
        
       
    }



    /// <summary>
    /// recuperation des tubes ronds
    /// </summary>

    /// <summary>
    /// recuperation des tubes ronds dans une liste de d'objet tubes ronds
    /// la liste TubeExclusion recupere les tubes deja importés et empeche la creation de doublons
    /// les methodes virtuelles read : lise le contenu de lcipper avec le lien odbc 
    /// les methodes virtuelles write : ecrive les tubes selon la typologie
    /// </summary>
    public class Clipper_Import_Tube_Rond : Clipper_Article_Tube, IDisposable
    {
        //private List<TubeRond>listeTubeRond;
        private TypeTube type = TypeTube.Rond;
        public double Diametre;
        public double Longueur;
        private List<TubeRond> List_Tube_Ronds;
     
        TextWriterTraceListener logFileTubeRond = new TextWriterTraceListener(System.IO.Path.GetTempPath() + "\\" + "TUBE_ROND_" + Properties.Resources.ImportTubeLog);

        /// <summary>
        /// constructeur obligatoire pour ajuster les elements a controler
        /// </summary>
        /// <param name="context"></param>
        public Clipper_Import_Tube_Rond(IContext context){

            this.contextlocal = context;
            Section_Key= "_SECTION_CIRCLE";
            SectionExclusion = getSectionExclusionList(Section_Key);
            TubeExclusion = getExclusionList("_BARTUBE", "_REFERENCE");

        }
       

        

        /// <summary>
        /// recuperation des ronds dans une liste de d'objet rond
        /// </summary>
        /// <returns>List<TubeRond></returns>
        public override void ReadTubes()
        {
            //creation de la liste des tube ronds
            //List<TubeRond> listeTubeRond;//= new List<TubeRond>();

            try
            {
                //creation de la liste des tube ronds
                //listeTubeRond = new List<TubeRond>();
                this.List_Tube_Ronds = new List<TubeRond>();
                //recuperation des tube rectangluaires
                string sql_tube_rec = this.JSTOOLS.getJsonStringParametres("sql.TubeRond");
                logFileTubeRond.Write("requete utilisée pour l'import des tubes \r\n "+ sql_tube_rec);
                int ii = 0;
                this.DbCommand.CommandText = sql_tube_rec;

                TABLE_ARTICLEM = DbCommand.ExecuteReader();

                while (TABLE_ARTICLEM.Read())
                {
                    ii++;
                    TubeRond tuberond = new TubeRond();
                    tuberond.Coarti = TABLE_ARTICLEM["COARTI"].ToString().Trim();
                    tuberond.Nuance = TABLE_ARTICLEM["CODENUANCE"].ToString().Trim(); //CODEETAT, Tech_NuanceMatiere.Nuance AS CODENUANCE
                    tuberond.Etat = TABLE_ARTICLEM["CODEETAT"].ToString().Trim();
                    tuberond.Prixart = Convert.ToDouble(getSqlNumericValue("PRIXART"));
                    //tuberond.Thickness = Convert.ToDouble(TABLE_ARTICLEM_TUBE["EPAISSEUR"]);
                    tuberond.Densite = Convert.ToDouble(getSqlNumericValue("DENSITE"));
                    tuberond.Longueur = Convert.ToDouble(getSqlNumericValue("LNG"));
                    tuberond.Diametre = Convert.ToDouble(getSqlNumericValue("DIAM"));
                    tuberond.epaisseur= Convert.ToDouble(getSqlNumericValue("EPAISSEUR"));
                    tuberond.Cofa = TABLE_ARTICLEM["COFA"].ToString().Trim(); ;
                    tuberond.Name = TABLE_ARTICLEM["COARTI"].ToString().Trim() + "*" + getSqlNumericValue("LNG");
                    this.List_Tube_Ronds.Add(tuberond);
                    logFileTubeRond.Write("Tube : " + tuberond.Coarti + " capturé");

                };

                TABLE_ARTICLEM.Close();
                //return listeTubeRond;
            }

            catch (Exception ie) { List_Tube_Ronds = null;

                logFileTubeRond.Close();

                /*return listeTubeRond;*/
            }

        }

        
        //public void create(IContext contextlocal,string section_name, double diam, double ep, double lng, double cost)
        public override  void WriteTubes()
        {


            if (List_Tube_Ronds.Any()) { 

            foreach (TubeRond tuberond in List_Tube_Ronds)
            {
                   
            //creation de la section (type...)//
            IEntity sectionEntity;
            string key = Guid.NewGuid().ToString();
            //creation de la section
            sectionEntity = contextlocal.EntityManager.CreateEntity(Section_Key);

             IEntity sectionQuality = contextlocal.EntityManager.CreateEntity("_SECTION_QUALITY");
             sectionEntity.GetImplementEntity("_SECTION").SetFieldValue("_KEY", key);
             string name = String.Format("Tube_Rond*{0}*{1}", tuberond.Diametre, tuberond.epaisseur);
             sectionEntity.GetImplementEntity("_SECTION").SetFieldValue("_NAME", name);// +"x"+ep.ToString());;

             
            sectionEntity.GetImplementEntity("_SECTION").SetFieldValue("_STANDARD", true);
            sectionEntity.SetFieldValue("_P_D", tuberond.Diametre);
            sectionEntity.SetFieldValue("_P_T", tuberond.epaisseur);
             //descpription //
             string description=String.Format("Tube Rond Diamete={0} mm Eaisseur={1} mm", tuberond.Diametre, tuberond.epaisseur);

            sectionEntity.GetImplementEntity("_SECTION").SetFieldValue("_DESCRIPTION", description);

            //sectionEntity.SetFieldValue("_P_T", this.ep);           
            sectionEntity.Complete = true;
             
            logFileTubeRond.Write("Tube : " + tuberond.Coarti + " sauvegarde");
                    //creation de la qualité
            tuberond.Material = tuberond.GetGrade(contextlocal, tuberond.Nuance, tuberond.Etat);
              
                    //recuperation de la section
                    if (SectionExclusion.Contains(name) == false)

                    {   //si n existe pas on recupere la nouvelle section //
                        sectionEntity.Save();
                        sectionQuality.SetFieldValue("_SECTION", sectionEntity.Id32); }
                    else  
                    //si elle existe deja on recupere la section //
                    {   sectionQuality.SetFieldValue("_SECTION", getExistingSection(name, Section_Key));}
            

           if (TubeExclusion.Contains(tuberond.Coarti) == false) {
                    sectionQuality.SetFieldValue("_QUALITY", tuberond.Material.Id32);
                    sectionQuality.SetFieldValue("_BUY_COST", tuberond.Prixart);
                    sectionQuality.Save();
                    logFileTubeRond.Write("section "+description+" sauvegardée");
                    //creation DE LA BARRE
                    IEntity barreEntity;
                    string keybar = Guid.NewGuid().ToString();
                    barreEntity = contextlocal.EntityManager.CreateEntity("_BARTUBE");
                    barreEntity.SetFieldValue("_REFERENCE", tuberond.Coarti);
                    barreEntity.SetFieldValue("_QUALITY", tuberond.Material.Id32);
                    barreEntity.SetFieldValue("_LENGTH", tuberond.Longueur);
                    //ATTENTION recuperation des infos de l'implemented section
                    barreEntity.SetFieldValue("_SECTION", sectionEntity.GetImplementEntity("_SECTION").Id); //sectionEntity.GetImplementEntity("_SECTION").Id
                    barreEntity.Save();
            }
             logFileTubeRond.Write("section " + description +"lng " + tuberond.Longueur + " sauvegardée");
            }
            }

        }















    }


    /// <summary>
    /// recuperation des ronds dans une liste de d'objet rond
    /// la liste TubeExclusion recupere les tubes deja importés et empeche la creation de doublons
    /// les methodes virtuelles read : lise le contenu de lcipper avec le lien odbc 
    /// les methodes virtuelles write : ecrive les tubes selon la typologie
    /// </summary>
    public class Clipper_Import_Rond : Clipper_Article_Tube, IDisposable
    {
        //private List<TubeRond>listeTubeRond;
        private TypeTube type = TypeTube.Rond;
        public double Diametre;
        public double Longueur;
        private List<TubeRond> List_Ronds;

        TextWriterTraceListener logFileRond = new TextWriterTraceListener(System.IO.Path.GetTempPath() + "\\" + "_ROND_" + Properties.Resources.ImportTubeLog);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Clipper_Import_Rond(IContext context) {
            this.contextlocal = context;
            Section_Key = "_SECTION_PLAIN_ROUND";
            SectionExclusion = getSectionExclusionList(Section_Key);
            TubeExclusion = getExclusionList("_BARTUBE", "_REFERENCE");

        }


        /// <summary>
        /// recuperation des ronds dans une liste de d'objet rond
        /// </summary>
        /// <returns>List<TubeRond></returns>
        public override void ReadTubes()
        {
            //creation de la liste des tube ronds
            //List<TubeRond> listeTubeRond;//= new List<TubeRond>();

            try
            {
                //creation de la liste des tube ronds
                //listeTubeRond = new List<TubeRond>();
                this.List_Ronds = new List<TubeRond>();
                //recuperation des tube rectangluaires
                string sql_tube_rec = this.JSTOOLS.getJsonStringParametres("sql.Rond");
                logFileRond.Write("requete utilisée pour l'import des tubes \r\n " + sql_tube_rec);
                int ii = 0;
                this.DbCommand.CommandText = sql_tube_rec;

                TABLE_ARTICLEM = DbCommand.ExecuteReader();

                while (TABLE_ARTICLEM.Read())
                {
                    ii++;
                    TubeRond tuberond = new TubeRond();
                    tuberond.Coarti = TABLE_ARTICLEM["COARTI"].ToString().Trim();
                    tuberond.Nuance = TABLE_ARTICLEM["CODENUANCE"].ToString().Trim(); //CODEETAT, Tech_NuanceMatiere.Nuance AS CODENUANCE
                    tuberond.Etat = TABLE_ARTICLEM["CODEETAT"].ToString().Trim();
                    tuberond.Prixart = Convert.ToDouble(getSqlNumericValue("PRIXART"));
                    //tuberond.Thickness = Convert.ToDouble(TABLE_ARTICLEM_TUBE["EPAISSEUR"]);
                    tuberond.Densite = Convert.ToDouble(getSqlNumericValue("DENSITE"));
                    tuberond.Longueur = Convert.ToDouble(getSqlNumericValue("LNG"));
                    tuberond.Diametre = Convert.ToDouble(getSqlNumericValue("DIAM"));
                    //tuberond.epaisseur= Convert.ToDouble(getSqlNumericValue("EPAISSEUR"));
                    tuberond.Cofa = TABLE_ARTICLEM["COFA"].ToString().Trim(); ;
                    tuberond.Name = TABLE_ARTICLEM["COARTI"].ToString().Trim() + "*" + getSqlNumericValue("LNG");
                    this.List_Ronds.Add(tuberond);
                    logFileRond.Write("Tube : " + tuberond.Coarti + " capturé");

                };

                TABLE_ARTICLEM.Close();
                //logFileRond.Flush();
                //logFileRond.Close();
                //return listeTubeRond;
            }

            catch (Exception ie)
            {
                List_Ronds = null;

                logFileRond.Close();

                /*return listeTubeRond;*/
            }

        }


        //public void create(IContext contextlocal,string section_name, double diam, double ep, double lng, double cost)
        public override void WriteTubes()
        {
            if (List_Ronds.Any())
            {
                foreach (TubeRond tuberond in List_Ronds)
                {
                    //tuberond.create(contextlocal);
                
                    //creation de la section (type...)//
                    IEntity sectionEntity;
                    string key = Guid.NewGuid().ToString();
                    //creation de la section
                    //sectionEntity = contextlocal.EntityManager.CreateEntity("_SECTION_CIRCLE");
                    sectionEntity = contextlocal.EntityManager.CreateEntity(Section_Key);
                    IEntity sectionQuality = contextlocal.EntityManager.CreateEntity("_SECTION_QUALITY");
                    sectionEntity.GetImplementEntity("_SECTION").SetFieldValue("_KEY", key);
                    string name = String.Format("Rond*{0}", tuberond.Diametre);
                    sectionEntity.GetImplementEntity("_SECTION").SetFieldValue("_NAME", name);// +"x"+ep.ToString());

                    sectionEntity.GetImplementEntity("_SECTION").SetFieldValue("_STANDARD", true);
                    sectionEntity.SetFieldValue("_P_D", tuberond.Diametre);
                 
                    //descpription //
          
                    sectionEntity.GetImplementEntity("_SECTION").SetFieldValue("_DESCRIPTION", String.Format("Rond Diametre={0} mm", tuberond.Diametre, tuberond.epaisseur));
              
                    sectionEntity.Complete = true;
                    sectionEntity.Save();
                    logFileRond.Write("Tube : " + tuberond.Coarti + " sauvegarde");
                    //creation de la qualité
                    tuberond.Material = tuberond.GetGrade(contextlocal, tuberond.Nuance, tuberond.Etat);

                    //recuperation de la section
                    if (SectionExclusion.Contains(name) == false)

                    {   //si n existe pas on recupere la nouvelle section //
                        sectionEntity.Save();
                        sectionQuality.SetFieldValue("_SECTION", sectionEntity.Id32);
                    }
                    else
                    //si elle existe deja on recupere la section //
                    { sectionQuality.SetFieldValue("_SECTION", getExistingSection(name, Section_Key)); }

                    if (TubeExclusion.Contains(tuberond.Coarti) == false)
                    {
                        sectionQuality.SetFieldValue("_SECTION", sectionEntity.Id32);
                        sectionQuality.SetFieldValue("_QUALITY", tuberond.Material.Id32);
                        sectionQuality.SetFieldValue("_BUY_COST", tuberond.Prixart);

                        sectionQuality.Save();
                        logFileRond.Write("section " + name + " sauvegardée");
                        //creation DE LA BARRE
                        IEntity barreEntity;
                        string keybar = Guid.NewGuid().ToString();
                        barreEntity = contextlocal.EntityManager.CreateEntity("_BARTUBE");
                        barreEntity.SetFieldValue("_REFERENCE", tuberond.Coarti);
                        barreEntity.SetFieldValue("_QUALITY", tuberond.Material.Id32);
                        barreEntity.SetFieldValue("_LENGTH", tuberond.Longueur);
                        //ATTENTION recuperation des infos de l'implemented section
                        barreEntity.SetFieldValue("_SECTION", sectionEntity.GetImplementEntity("_SECTION").Id); //sectionEntity.GetImplementEntity("_SECTION").Id
                        barreEntity.Save();
                        logFileRond.Write("section " + name + "lng " + tuberond.Longueur + " sauvegardée");
                    }
                }
            }

        }




    }


    /// <summary>
    /// recuperation des rectangles dans une liste de d'objet rectangles
    /// la liste TubeExclusion recupere les tubes deja importés et empeche la creation de doublons
    /// les methodes virtuelles read : lise le contenu de lcipper avec le lien odbc 
    /// les methodes virtuelles write : ecrive les tubes selon la typologie
    /// </summary>

    public class Clipper_Import_Tube_Rectangle : Clipper_Article_Tube, IDisposable
    {
        //private List<TubeRond>listeTubeRond;
        private TypeTube type = TypeTube.Rectangle;
        public double Diametre;
        public double Longueur;
        private List<TubeRec> List_Recs;

        TextWriterTraceListener logFileRec = new TextWriterTraceListener(System.IO.Path.GetTempPath() + "\\" + "_REC_" + Properties.Resources.ImportTubeLog);


        public Clipper_Import_Tube_Rectangle(IContext context){
             this.contextlocal = context;
            Section_Key= "_SECTION_SHARP_RECTANGLE";
            SectionExclusion = getSectionExclusionList(Section_Key);
            TubeExclusion = getExclusionList("_BARTUBE", "_REFERENCE");
       }

        /// <summary>
        /// recuperation des ronds dans une liste de d'objet rond
        /// </summary>
        /// <returns>List<TubeRond></returns>
        public override void ReadTubes()
        {
            //creation de la liste des tube ronds
            //List<TubeRond> listeTubeRond;//= new List<TubeRond>();

            try
            {
                //creation de la liste des tube ronds
                //listeTubeRond = new List<TubeRond>();
                this.List_Recs = new List<TubeRec>();
                //recuperation des tube rectangluaires
                string sql_tube_rec = this.JSTOOLS.getJsonStringParametres("sql.TubeRectangle");
                logFileRec.Write("requete utilisée pour l'import des tubes \r\n " + sql_tube_rec);
                int ii = 0;
                this.DbCommand.CommandText = sql_tube_rec;

                TABLE_ARTICLEM = DbCommand.ExecuteReader();

                while (TABLE_ARTICLEM.Read())
                {
                    ii++;
                    TubeRec tuberec= new TubeRec();
                    tuberec.Coarti = TABLE_ARTICLEM["COARTI"].ToString().Trim();
                    tuberec.Nuance = TABLE_ARTICLEM["CODENUANCE"].ToString().Trim(); //CODEETAT, Tech_NuanceMatiere.Nuance AS CODENUANCE
                    tuberec.Etat = TABLE_ARTICLEM["CODEETAT"].ToString().Trim();
                    tuberec.Prixart = Convert.ToDouble(getSqlNumericValue("PRIXART"));
                    //tuberond.Thickness = Convert.ToDouble(TABLE_ARTICLEM["EPAISSEUR"]);
                    tuberec.Densite = Convert.ToDouble(getSqlNumericValue("DENSITE"));
                    tuberec.Hauteur = Convert.ToDouble(getSqlNumericValue("HAUTEUR"));
                    tuberec.Largeur = Convert.ToDouble(getSqlNumericValue("LARGEUR"));
                    tuberec.epaisseur= Convert.ToDouble(getSqlNumericValue("EPAISSEUR"));
                    tuberec.Longueur = Convert.ToDouble(getSqlNumericValue("LNG"));
                    tuberec.Cofa = TABLE_ARTICLEM["COFA"].ToString().Trim(); ;
                    tuberec.Name = TABLE_ARTICLEM["COARTI"].ToString().Trim() + "*" + getSqlNumericValue("LNG");
                    this.List_Recs.Add(tuberec);
                    logFileRec.Write("Tube : " + tuberec.Coarti + " capturé");

                };

                TABLE_ARTICLEM.Close();
                //logFileRond.Flush();
                //logFileRond.Close();
                //return listeTubeRond;
            }

            catch (Exception ie)
            {
                List_Recs = null;

                logFileRec.Close();

                /*return listeTubeRond;*/
            }

        }


        //public void create(IContext contextlocal,string section_name, double diam, double ep, double lng, double cost)
        public override  void WriteTubes()
        {
            if (List_Recs.Any())
            {
                foreach (TubeRec tuberec in List_Recs)
                {
                   
                    //creation de la section (type...)//
                    IEntity sectionEntity;
                    string key = Guid.NewGuid().ToString();
                    //creation de la section
                    //sectionEntity = contextlocal.EntityManager.CreateEntity("_SECTION_CIRCLE");
                    sectionEntity = contextlocal.EntityManager.CreateEntity(Section_Key);
                    IEntity sectionQuality = contextlocal.EntityManager.CreateEntity("_SECTION_QUALITY");
                    sectionEntity.GetImplementEntity("_SECTION").SetFieldValue("_KEY", key);
                    string name = String.Format("Rec*{0}*{1}*{2}", tuberec.Largeur,tuberec.Hauteur,tuberec.epaisseur);
                    sectionEntity.GetImplementEntity("_SECTION").SetFieldValue("_NAME", name);// +"x"+ep.ToString());
                    sectionEntity.GetImplementEntity("_SECTION").SetFieldValue("_STANDARD", true);
                    sectionEntity.SetFieldValue("_P_B", tuberec.Largeur);
                    sectionEntity.SetFieldValue("_P_H", tuberec.Hauteur);
                    sectionEntity.SetFieldValue("_P_T", tuberec.epaisseur);
                    //descpription //
                    sectionEntity.GetImplementEntity("_SECTION").SetFieldValue("_DESCRIPTION", String.Format("Rec hauteur={1} mm Larg={0} mm  Ep={2} mm", tuberec.Hauteur, tuberec.Largeur, tuberec.epaisseur));
                    sectionEntity.Complete = true;
                    sectionEntity.Save();
                    logFileRec.Write("Tube : " + tuberec.Coarti + " sauvegarde");
                    //creation de la qualité
                    tuberec.Material = tuberec.GetGrade(contextlocal, tuberec.Nuance, tuberec.Etat);
                    //recuperation de la section
                    if (SectionExclusion.Contains(name) == false)

                    {   //si n existe pas on recupere la nouvelle section //
                        sectionEntity.Save();
                        sectionQuality.SetFieldValue("_SECTION", sectionEntity.Id32);
                    }
                    else
                    //si elle existe deja on recupere la section //
                    { sectionQuality.SetFieldValue("_SECTION", getExistingSection(name, Section_Key)); }
                    if (TubeExclusion.Contains(tuberec.Coarti) == false)
                    {
                                sectionQuality.SetFieldValue("_SECTION", sectionEntity.Id32);
                                sectionQuality.SetFieldValue("_QUALITY", tuberec.Material.Id32);
                                sectionQuality.SetFieldValue("_BUY_COST", tuberec.Prixart);
                                sectionQuality.Save();
                                logFileRec.Write("section " + name + " sauvegardée");
                                //creation DE LA BARRE
                                IEntity barreEntity;
                                string keybar = Guid.NewGuid().ToString();
                                barreEntity = contextlocal.EntityManager.CreateEntity("_BARTUBE");
                                barreEntity.SetFieldValue("_REFERENCE", tuberec.Coarti);
                                barreEntity.SetFieldValue("_QUALITY", tuberec.Material.Id32);
                                barreEntity.SetFieldValue("_LENGTH", tuberec.Longueur);
                                barreEntity.SetFieldValue("_COMMENTS", string.Format("REC*{0}*{1}*{2}*{3}", tuberec.Hauteur, tuberec.Largeur, tuberec.epaisseur, tuberec.Longueur));
                                //ATTENTION recuperation des infos de l'implemented section
                                barreEntity.SetFieldValue("_SECTION", sectionEntity.GetImplementEntity("_SECTION").Id); //sectionEntity.GetImplementEntity("_SECTION").Id
                                barreEntity.Save();
                                logFileRec.Write("section " + name + "lng " + tuberec.Hauteur + " sauvegardée");
                    }
                }
            }

        }







    }


    /// <summary>
    /// recuperation des carres dans une liste de d'objet carres
    /// la liste TubeExclusion recupere les tubes deja importés et empeche la creation de doublons
    /// les methodes virtuelles read : lise le contenu de lcipper avec le lien odbc 
    /// les methodes virtuelles write : ecrive les tubes selon la typologie
    /// </summary>



    public class Clipper_Import_Tube_Carre : Clipper_Article_Tube, IDisposable
    {
        //private List<TubeRond>listeTubeRond;
        private TypeTube type = TypeTube.Rectangle;
        public double Diametre;
        public double Longueur;
        private List<TubeRec> List_Recs;

        TextWriterTraceListener logFileCar = new TextWriterTraceListener(System.IO.Path.GetTempPath() + "\\" + "_REC_" + Properties.Resources.ImportTubeLog);


        
        public Clipper_Import_Tube_Carre(IContext context) {
            this.contextlocal = context;
            Section_Key = "_SECTION_SHARP_RECTANGLE";
            SectionExclusion = getSectionExclusionList(Section_Key);
            TubeExclusion = getExclusionList("_BARTUBE", "_REFERENCE");
        }

        /// <summary>
        /// recuperation des ronds dans une liste de d'objet rond
        /// </summary>
        /// <returns>List<TubeRond></returns>
        public override void ReadTubes()
        {
            //creation de la liste des tube ronds
            //List<TubeRond> listeTubeRond;//= new List<TubeRond>();

            try
            {
                //creation de la liste des tube ronds
                //listeTubeRond = new List<TubeRond>();
                this.List_Recs = new List<TubeRec>();
                //recuperation des tube rectangluaires
                string sql_tube_rec = this.JSTOOLS.getJsonStringParametres("sql.TubeCarre");
                logFileCar.Write("requete utilisée pour l'import des tubes \r\n " + sql_tube_rec);
                int ii = 0;
                this.DbCommand.CommandText = sql_tube_rec;

                TABLE_ARTICLEM = DbCommand.ExecuteReader();

                while (TABLE_ARTICLEM.Read())
                {
                    ii++;
                    TubeRec tuberec = new TubeRec();
                    tuberec.Coarti = TABLE_ARTICLEM["COARTI"].ToString().Trim();
                    tuberec.Nuance = TABLE_ARTICLEM["CODENUANCE"].ToString().Trim(); //CODEETAT, Tech_NuanceMatiere.Nuance AS CODENUANCE
                    tuberec.Etat = TABLE_ARTICLEM["CODEETAT"].ToString().Trim();
                    tuberec.Prixart = Convert.ToDouble(getSqlNumericValue("PRIXART"));
                    //tuberond.Thickness = Convert.ToDouble(TABLE_ARTICLEM["EPAISSEUR"]);
                    tuberec.Densite = Convert.ToDouble(getSqlNumericValue("DENSITE"));
                    tuberec.Hauteur = Convert.ToDouble(getSqlNumericValue("LARGEUR"));
                    tuberec.Largeur = Convert.ToDouble(getSqlNumericValue("LARGEUR"));
                    tuberec.epaisseur = Convert.ToDouble(getSqlNumericValue("EPAISSEUR"));
                    tuberec.Longueur = Convert.ToDouble(getSqlNumericValue("LNG"));
                    tuberec.Cofa = TABLE_ARTICLEM["COFA"].ToString().Trim(); ;
                    tuberec.Name = TABLE_ARTICLEM["COARTI"].ToString().Trim() + "*" + getSqlNumericValue("LNG");
                    this.List_Recs.Add(tuberec);
                    logFileCar.Write("Tube : " + tuberec.Coarti + " capturé");

                };

                TABLE_ARTICLEM.Close();
                //logFileRond.Flush();
                //logFileRond.Close();
                //return listeTubeRond;
            }

            catch (Exception ie)
            {
                List_Recs = null;

                logFileCar.Close();

                /*return listeTubeRond;*/
            }

        }


        //public void create(IContext contextlocal,string section_name, double diam, double ep, double lng, double cost)
        public override void WriteTubes()
        {
            if (List_Recs.Any())
            {
                foreach (TubeRec tuberec in List_Recs)
                {

                    //creation de la section (type...)//
                    IEntity sectionEntity;
                    string key = Guid.NewGuid().ToString();
                    //creation de la section
                    //sectionEntity = contextlocal.EntityManager.CreateEntity("_SECTION_CIRCLE");
                    sectionEntity = contextlocal.EntityManager.CreateEntity(Section_Key);
                    IEntity sectionQuality = contextlocal.EntityManager.CreateEntity("_SECTION_QUALITY");
                    sectionEntity.GetImplementEntity("_SECTION").SetFieldValue("_KEY", key);
                    string name = String.Format("Car*{0}*{2}", tuberec.Largeur, tuberec.Hauteur, tuberec.epaisseur);
                    sectionEntity.GetImplementEntity("_SECTION").SetFieldValue("_NAME", name);// +"x"+ep.ToString());
                    sectionEntity.GetImplementEntity("_SECTION").SetFieldValue("_STANDARD", true);
                    sectionEntity.SetFieldValue("_P_B", tuberec.Largeur);
                    sectionEntity.SetFieldValue("_P_H", tuberec.Hauteur);
                    sectionEntity.SetFieldValue("_P_T", tuberec.epaisseur);
                    //descpription //
                    sectionEntity.GetImplementEntity("_SECTION").SetFieldValue("_DESCRIPTION", String.Format("Car hauteur={1} mm Larg={0} mm  Ep={2} mm", tuberec.Hauteur, tuberec.Largeur, tuberec.epaisseur));
                    sectionEntity.Complete = true;
                    sectionEntity.Save();
                    logFileCar.Write("Tube : " + tuberec.Coarti + " sauvegarde");
                    //creation de la qualité
                    tuberec.Material = tuberec.GetGrade(contextlocal, tuberec.Nuance, tuberec.Etat);

                    //recuperation de la section
                    if (SectionExclusion.Contains(name) == false)

                    {   //si n existe pas on recupere la nouvelle section //
                        sectionEntity.Save();
                        sectionQuality.SetFieldValue("_SECTION", sectionEntity.Id32);
                    }
                    else
                    //si elle existe deja on recupere la section //
                    { sectionQuality.SetFieldValue("_SECTION", getExistingSection(name, Section_Key)); }
                    if (TubeExclusion.Contains(tuberec.Coarti) == false)
                    {
                        sectionQuality.SetFieldValue("_SECTION", sectionEntity.Id32);
                        sectionQuality.SetFieldValue("_QUALITY", tuberec.Material.Id32);
                        sectionQuality.SetFieldValue("_BUY_COST", tuberec.Prixart);

                        sectionQuality.Save();
                        logFileCar.Write("section " + name + " sauvegardée");
                        //creation DE LA BARRE
                        IEntity barreEntity;
                        string keybar = Guid.NewGuid().ToString();
                        barreEntity = contextlocal.EntityManager.CreateEntity("_BARTUBE");
                        barreEntity.SetFieldValue("_REFERENCE", tuberec.Coarti);
                        barreEntity.SetFieldValue("_QUALITY", tuberec.Material.Id32);
                        barreEntity.SetFieldValue("_LENGTH", tuberec.Longueur);
                        barreEntity.SetFieldValue("_COMMENTS", string.Format("CAR*{0}*{2}*{3}", tuberec.Hauteur, tuberec.Largeur, tuberec.epaisseur, tuberec.Longueur));
                        //ATTENTION recuperation des infos de l'implemented section
                        barreEntity.SetFieldValue("_SECTION", sectionEntity.GetImplementEntity("_SECTION").Id); //sectionEntity.GetImplementEntity("_SECTION").Id
                        barreEntity.Save();
                        logFileCar.Write("section " + name + "lng " + tuberec.Hauteur + " sauvegardée");
                    }
                }
            }

        }















    }


    /// <summary>
    /// recuperation des flats dans une liste de d'objet flat
    /// la liste TubeExclusion recupere les tubes deja importés et empeche la creation de doublons
    /// les methodes virtuelles read : lise le contenu de lcipper avec le lien odbc 
    /// les methodes virtuelles write : ecrive les tubes selon la typologie
    /// </summary>


    public class Clipper_Import_Tube_Flat : Clipper_Article_Tube, IDisposable
    {
        //private List<TubeRond>listeTubeRond;
        private TypeTube type = TypeTube.Flat;
        public double Diametre;
        public double Longueur;
        private List<TubeRec> List_Recs;
        
        public Clipper_Import_Tube_Flat(IContext context) {
            this.contextlocal = context;
            Section_Key = "_SECTION_FLAT";
            SectionExclusion = getSectionExclusionList(Section_Key);
            TubeExclusion = getExclusionList("_BARTUBE", "_REFERENCE");
        }

        TextWriterTraceListener logFileFlat = new TextWriterTraceListener(System.IO.Path.GetTempPath() + "\\" + "_FLAT_" + Properties.Resources.ImportTubeLog);
        /// <summary>
        /// recuperation des ronds dans une liste de d'objet rond
        /// la liste TubeExclusion recupere les tubes deja importés et empeche la creation de doublons
        /// </summary>
        /// <returns>List<TubeRond></returns>
        public override void ReadTubes()
        {
            //creation de la liste des tube ronds
            //List<TubeRond> listeTubeRond;//= new List<TubeRond>();

            try
            {
                //creation de la liste des tube ronds
                //listeTubeRond = new List<TubeRond>();
                this.List_Recs = new List<TubeRec>();
                //recuperation des tube rectangluaires
                string sql_tube_rec = this.JSTOOLS.getJsonStringParametres("sql.Flat");
                logFileFlat.Write("requete utilisée pour l'import des tubes \r\n " + sql_tube_rec);
                int ii = 0;
                this.DbCommand.CommandText = sql_tube_rec;

                TABLE_ARTICLEM = DbCommand.ExecuteReader();

                while (TABLE_ARTICLEM.Read())
                {
                    ii++;
                    TubeRec tuberec = new TubeRec();
                    tuberec.Coarti = TABLE_ARTICLEM["COARTI"].ToString().Trim();
                    tuberec.Nuance = TABLE_ARTICLEM["CODENUANCE"].ToString().Trim(); //CODEETAT, Tech_NuanceMatiere.Nuance AS CODENUANCE
                    tuberec.Etat = TABLE_ARTICLEM["CODEETAT"].ToString().Trim();
                    tuberec.Prixart = Convert.ToDouble(getSqlNumericValue("PRIXART"));
                    //tuberond.Thickness = Convert.ToDouble(TABLE_ARTICLEM["EPAISSEUR"]);
                    tuberec.Densite = Convert.ToDouble(getSqlNumericValue("DENSITE"));
                    tuberec.Hauteur = Convert.ToDouble(getSqlNumericValue("HAUTEUR"));
                    tuberec.Largeur = Convert.ToDouble(getSqlNumericValue("LARGEUR"));
                    tuberec.epaisseur = 0;
                    tuberec.Longueur = Convert.ToDouble(getSqlNumericValue("LNG"));
                    tuberec.Cofa = TABLE_ARTICLEM["COFA"].ToString().Trim(); ;
                    tuberec.Name = TABLE_ARTICLEM["COARTI"].ToString().Trim() + "*" + getSqlNumericValue("LNG");
                    this.List_Recs.Add(tuberec);
                    logFileFlat.Write("Tube : " + tuberec.Coarti + " capturé");

                };

                TABLE_ARTICLEM.Close();
                //logFileRond.Flush();
                //logFileRond.Close();
                //return listeTubeRond;
            }

            catch (Exception ie)
            {
                List_Recs = null;

                logFileFlat.Close();

                /*return listeTubeRond;*/
            }

        }

        public override  void WriteTubes()
        {
            if (List_Recs.Any())
            {
                foreach (TubeRec tuberec in List_Recs)
                {

                    //creation de la section (type...)//
                    IEntity sectionEntity;
                    string key = Guid.NewGuid().ToString();
                    //creation de la section
                    //sectionEntity = contextlocal.EntityManager.CreateEntity("_SECTION_CIRCLE");
                    sectionEntity = contextlocal.EntityManager.CreateEntity(Section_Key);
                    IEntity sectionQuality = contextlocal.EntityManager.CreateEntity("_SECTION_QUALITY");
                    sectionEntity.GetImplementEntity("_SECTION").SetFieldValue("_KEY", key);
                    string name = String.Format("Flat*{0}*{2}", tuberec.Largeur, tuberec.Largeur, tuberec.Hauteur);
                    sectionEntity.GetImplementEntity("_SECTION").SetFieldValue("_NAME", name);// +"x"+ep.ToString());
                    sectionEntity.GetImplementEntity("_SECTION").SetFieldValue("_STANDARD", true);
                    sectionEntity.SetFieldValue("_P_B", tuberec.Largeur);
                    sectionEntity.SetFieldValue("_P_H", tuberec.Hauteur);
                    //sectionEntity.SetFieldValue("_P_T", tuberec.epaisseur);
                    //descpription //
                    sectionEntity.GetImplementEntity("_SECTION").SetFieldValue("_DESCRIPTION", String.Format("Flat  Largeur={1} mm hauteur={0} mm ", tuberec.Hauteur, tuberec.Largeur, tuberec.epaisseur));
                    sectionEntity.Complete = true;
                    sectionEntity.Save();
                    logFileFlat.Write("Tube : " + tuberec.Coarti + " sauvegarde");
                    //creation de la qualité
                    tuberec.Material = tuberec.GetGrade(contextlocal, tuberec.Nuance, tuberec.Etat);

                    //recuperation de la section
                    if (SectionExclusion.Contains(name) == false)
                    {   //si n existe pas on recupere la nouvelle section //
                        sectionEntity.Save();
                        sectionQuality.SetFieldValue("_SECTION", sectionEntity.Id32);
                    }
                    else
                    //si elle existe deja on recupere la section //
                    { sectionQuality.SetFieldValue("_SECTION", getExistingSection(name, Section_Key)); }

                    if (TubeExclusion.Contains(tuberec.Coarti) == false)
                    {
                        sectionQuality.SetFieldValue("_SECTION", sectionEntity.Id32);
                        sectionQuality.SetFieldValue("_QUALITY", tuberec.Material.Id32);
                        sectionQuality.SetFieldValue("_BUY_COST", tuberec.Prixart);
                        sectionQuality.Save();
                        logFileFlat.Write("section " + name + " sauvegardée");
                        //creation DE LA BARRE
                        IEntity barreEntity;
                        string keybar = Guid.NewGuid().ToString();
                        barreEntity = contextlocal.EntityManager.CreateEntity("_BARTUBE");
                        barreEntity.SetFieldValue("_REFERENCE", tuberec.Coarti);
                        barreEntity.SetFieldValue("_QUALITY", tuberec.Material.Id32);
                        barreEntity.SetFieldValue("_LENGTH", tuberec.Longueur);
                        barreEntity.SetFieldValue("_COMMENTS", string.Format("FLAT*{0}*{1}*{3}", tuberec.Hauteur, tuberec.Largeur, tuberec.epaisseur, tuberec.Longueur));
                        //ATTENTION recuperation des infos de l'implemented section
                        barreEntity.SetFieldValue("_SECTION", sectionEntity.GetImplementEntity("_SECTION").Id); //sectionEntity.GetImplementEntity("_SECTION").Id
                        barreEntity.Save();
                        logFileFlat.Write("section " + name + "lng " + tuberec.Hauteur + " sauvegardée");
                    }
                }
            }

        }















    }

    /// <summary>
    /// importe les tubes spéciaux : la racine du nom du tube doit contenir le nom de la section du tube spécial
    /// par exemple IPN80*S235*BRUT*3000 : la strucuture doit etre au minimum comme ceci: [SECTION ALMACAM].....[LONGUEUR] 
    /// ->[IPN80].....[3000] la section choisie est IPN80, la longeur est 3000
    /// </summary>

    public class Clipper_Import_Tube_Speciaux : Clipper_Article_Tube, IDisposable
    {
        //private List<TubeRond>listeTubeRond;
        private TypeTube type = TypeTube.Speciaux;
        private List<TubeSpe> List_Spe;

        
        public Clipper_Import_Tube_Speciaux(IContext context)
        {
            this.contextlocal = context;
            string section_key;
            TubeExclusion = new List<string>();
            // Section_Key = typeTube; // "_SECTION_FLAT";
            //construction de la liste d'exclusion sur les section ci dessous
            getExclusionList( ref TubeExclusion, "_BARTUBE", "_REFERENCE");
          
            /*
              getExclusionList(ref TubeExclusion, "_BARTUBE", "_REFERENCE", "SECTION_IPN"); 
              getExclusionList(ref TubeExclusion, "_BARTUBE", "_REFERENCE", "SECTION_IPE"); 
              getExclusionList(ref TubeExclusion, "_BARTUBE", "_REFERENCE", "SECTION_UPN"); 
              getExclusionList(ref TubeExclusion, "_BARTUBE", "_REFERENCE", "SECTION_UPE"); 
              getExclusionList(ref TubeExclusion, "_BARTUBE", "_REFERENCE", "SECTION_L"); 
              getExclusionList(ref TubeExclusion, "_BARTUBE", "_REFERENCE", "SECTION_LROUND"); */


        }

        /// <summary>
        /// utilise pour les tubes spes uniquement ou le section_key  (SECTION_UPE,SECTION_UPN..) peut varier
        /// </summary>
        /// <param name="entitype"></param>
        /// <param name="uid_field">chzamps unique de recherche</param>
        /// <param name="exclusionlist">list cumule d'exclusion.. </param>
        /// 
        /// <returns></returns>
        public void getExclusionList(ref List<string> exclusion, string entitype, string uid_field)
        {
            try
            {

                IEntityList exclusionlist;
                exclusionlist = contextlocal.EntityManager.GetEntityList(entitype);
                //exclusionlist[0].GetFieldValueAsEntity("_SECTION").ImplementedEntityType.Key
                exclusionlist.Fill(false);
                //List<string> exclusion = new List<string>();
                //exclusionlist.ToList();
                // Find material           
                //Entity currentQuality = qualitylist.Where(q => q.DefaultValue.Equals(quality)).FirstOrDefault();

                if (exclusionlist.Count > 0)
                {
                    foreach (IEntity ex in exclusionlist)
                    {
                        if (ex.GetFieldValueAsString(uid_field) != null)
                        {
                            string uid = ex.GetFieldValueAsString(uid_field).Trim();
                            exclusion.Add(uid);

                        }




                    }

                }
                //return exclusion;
            }

            catch (Exception ie)
            {

                MessageBox.Show(ie.Message);
                // return null;
            }


        }
        /// <summary>
        /// utilise pour les tubes spes uniquement ou le section_key  (SECTION_UPE,SECTION_UPN..) peut varier
        /// </summary>
        /// <param name="entitype"></param>
        /// <param name="uid_field">chzamps unique de recherche</param>
        /// <param name="section_key">Entité section SECTION_UPE,SECTION_UPN.. </param>
        /// <param name="exclusionlist">list cumule d'exclusion.. </param>
        /// 
        /// <returns></returns>
        public void getRestrictedExclusionList( ref List<string> exclusion, string entitype, string uid_field, string section_key)
        {
            try
            {

                IEntityList exclusionlist;
                exclusionlist = contextlocal.EntityManager.GetEntityList(entitype);
                //exclusionlist[0].GetFieldValueAsEntity("_SECTION").ImplementedEntityType.Key
                exclusionlist.Fill(false);
                //List<string> exclusion = new List<string>();
                //exclusionlist.ToList();
                // Find material           
                //Entity currentQuality = qualitylist.Where(q => q.DefaultValue.Equals(quality)).FirstOrDefault();

                if (exclusionlist.Count>0){ 
                        foreach (IEntity ex in exclusionlist)
                        {
                            if   (ex.GetFieldValueAsString(uid_field) != null & ex.GetFieldValueAsEntity("_SECTION").ImplementedEntityType.Key== section_key) { 
                            string uid = ex.GetFieldValueAsString(uid_field).Trim();
                            exclusion.Add(uid);

                        }




                    }

                }
                //return exclusion;
            }

            catch (Exception ie)
            {
                
                MessageBox.Show(ie.Message);
                // return null;
            }


        }
      //traduit du code article les inforamtions en decomposant le code article
        private TubeSpe getTubeInfos(string coda) {

            try {  

                TubeSpe sp = new TubeSpe();
                string[] infos = null;

                if (coda!=null || coda != string.Empty)
               //recuperation de la section spe du tube
              
                infos= coda.Split('*');
                long dim = infos.Count()-1;
                sp.Section= infos[0].Trim();
                sp.Longueur = Convert.ToDouble(infos[dim]);
                //section
                if (sp.Section.Contains("IPN")) { sp.Section_Spe_Key = "_SECTION_IPN";    }
                else if (sp.Section.Contains("IPE"))  { sp.Section_Spe_Key = "_SECTION_IPE";    }
                else if (sp.Section.Contains("UPN")) { sp.Section_Spe_Key = "_SECTION_UPN";    }
                else if (sp.Section.Contains("UPE"))  { sp.Section_Spe_Key = "_SECTION_UPE";    }
                else if (sp.Section == "L")  { sp.Section_Spe_Key = "_SECTION_L";      }
                else if (sp.Section.Contains("LROUND"))   { sp.Section_Spe_Key = "_SECTION_LROUND";  }
                else { sp.Section_Spe_Key = string.Empty; }
                sp.Coarti = coda;
                return sp;
                
            }

            catch (Exception ie)
            {
                return null;
                MessageBox.Show(ie.Message);
            }

        }

        TextWriterTraceListener logFilSpe= new TextWriterTraceListener(System.IO.Path.GetTempPath() + "\\" + "_SPE_" + Properties.Resources.ImportTubeLog);
        /// <summary>
        /// recuperation des ronds dans une liste de d'objet rond
        /// </summary>
        /// <returns>List<TubeRond></returns>
        public override void ReadTubes()
        {
            //creation de la liste des tube ronds
            //List<TubeRond> listeTubeRond;//= new List<TubeRond>();

            try
            {
                //creation de la liste des tube ronds
                //listeTubeRond = new List<TubeRond>();
                this.List_Spe = new List<TubeSpe>();
                //recuperation des tube rectangluaires
                string sql_tube_spe = this.JSTOOLS.getJsonStringParametres("sql.Profilspeciaux");
                logFilSpe.Write("requete utilisée pour l'import des tubes \r\n " + sql_tube_spe);
                int ii = 0;
                this.DbCommand.CommandText = sql_tube_spe;

                TABLE_ARTICLEM = DbCommand.ExecuteReader();

                while (TABLE_ARTICLEM.Read())
                {
                    ii++;
                    TubeSpe tubespe = new TubeSpe();                    
                    //tubespe.Coarti = TABLE_ARTICLEM["COARTI"].ToString().Trim();
                    tubespe = getTubeInfos(TABLE_ARTICLEM["COARTI"].ToString().Trim());
                    tubespe.Nuance = TABLE_ARTICLEM["CODENUANCE"].ToString().Trim(); //CODEETAT, Tech_NuanceMatiere.Nuance AS CODENUANCE
                    tubespe.Etat = TABLE_ARTICLEM["CODEETAT"].ToString().Trim();
                    tubespe.Prixart = Convert.ToDouble(getSqlNumericValue("PRIXART"));
                    //tuberond.Thickness = Convert.ToDouble(TABLE_ARTICLEM["EPAISSEUR"]);
                    tubespe.Densite = Convert.ToDouble(getSqlNumericValue("DENSITE"));

                    tubespe.Longueur = Convert.ToDouble(getSqlNumericValue("LNG"));
                    tubespe.Cofa = TABLE_ARTICLEM["COFA"].ToString().Trim(); ;
                    tubespe.Name = TABLE_ARTICLEM["COARTI"].ToString().Trim() + "*" + getSqlNumericValue("LNG");
                    this.List_Spe.Add(tubespe);
                    logFilSpe.Write("Tube : " + tubespe.Coarti + " capturé");

                };

                TABLE_ARTICLEM.Close();
                //logFileRond.Flush();
                //logFileRond.Close();
                //return listeTubeRond;
            }

            catch (Exception ie)
            {
                List_Spe = null;

                logFilSpe.Close();

                /*return listeTubeRond;*/
            }

        }



        public override void WriteTubes()
        {
            if (List_Spe.Any())
            {
                //liste des type actuelles



                foreach (TubeSpe tubespe in List_Spe)
                {

                    //creation de la section (type...)//
                    /*
                    IEntityList sectionEntityList;
                    IEntity sectionEntity;
                    sectionEntityList = contextlocal.EntityManager.GetEntityList(tubespe.Coarti);
                    sectionEntityList.Fill(false);
                    sectionEntity = sectionEntityList.FirstOrDefault();*/
                   
                    //+ @"\IMPLEMENTED_SECTION", "_NAME", ConditionOperator.Equal, tubespe.Section
                    //logFilSpe.Write("Tube : " + tubespe.Coarti + " trouvée");
                    //onne creer que ce qui n'existe pas


                    if (TubeExclusion.Contains(tubespe.Coarti) == false)
                    {
                        //si elle existe deja on recupere la section //
                        IEntityList sectionQualityList, sectionEntityList;
                        IEntity sectionQuality, sectionEntity;
                        long sectionEntityid;

                        //creation de la qualité
                        tubespe.Material = tubespe.GetGrade(contextlocal, tubespe.Nuance, tubespe.Etat);
                        //sectionQualityList = contextlocal.EntityManager.GetEntityList(tubespe.Material.GetFieldValueAsString("_NAME"));
                        //sectionQualityList.Fill(false);
                        //sectionQuality = sectionQualityList.FirstOrDefault();
                        //recuperation de la section
                        sectionEntityid= getExistingImplementedSection(tubespe.Section, tubespe.Section_Spe_Key);
                        /*
                        sectionQuality.SetFieldValue("_SECTION", getExistingSection(tubespe.Section, tubespe.Section_Spe_Key));
                        sectionQuality.SetFieldValue("_SECTION", sectionEntity.Id32);
                        sectionQuality.SetFieldValue("_QUALITY", tubespe.Material.Id32);
                        sectionQuality.SetFieldValue("_BUY_COST", tubespe.Prixart);
                        sectionQuality.Save();
                        logFilSpe.Write("section " + tubespe.Section + " sauvegardée");*/


                        //creation DE LA BARRE
                        IEntity barreEntity;
                        string keybar = Guid.NewGuid().ToString();
                        barreEntity = contextlocal.EntityManager.CreateEntity("_BARTUBE");
                        barreEntity.SetFieldValue("_REFERENCE", tubespe.Coarti);
                        barreEntity.SetFieldValue("_QUALITY", tubespe.Material.Id32);
                        barreEntity.SetFieldValue("_LENGTH", tubespe.Longueur);
                        barreEntity.SetFieldValue("_COMMENTS", string.Format("section: {0} matiere: {1} longeur: {2}", tubespe.Section, tubespe.Material.GetFieldValueAsString("_NAME"), tubespe.Longueur));
                        //ATTENTION recuperation des infos de l'implemented section
                        barreEntity.SetFieldValue("_SECTION", sectionEntityid); //sectionEntity.GetImplementEntity("_SECTION").Id

                        barreEntity.Save();
                        logFilSpe.Write("section " + tubespe.Section + "lng " + tubespe.Longueur + " sauvegardée");
                    }

                   
                    }
                }
            }

        















    }


}

#endregion