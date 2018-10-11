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

namespace AF_Clipper_Export_Devis_Dll.cs
{
    internal class ExportClipper : QuoteGpExporter
    {
        private IDictionary<IEntity, KeyValuePair<string, string>> _ReferenceIdList = new Dictionary<IEntity, KeyValuePair<string, string>>();
        private IDictionary<string, string> _ReferenceList = new Dictionary<string, string>();
        private IDictionary<string, long> _ReferenceListCount = new Dictionary<string, long>();
        private IDictionary<long, long> _FixeCostPartExportedList = new Dictionary<long, long>();

        private bool _GlobalExported = false;

        #region IQuoteGpExporter Membres

        public override bool Export(IContext context, IEnumerable<IQuote> quoteList, string exportDir)
        {
            string clipperFileName = "";
            string filename = "";
            if (quoteList.Count() == 1)
            {
                filename = "Trans_" + quoteList.First().QuoteInformation.IncNo.ToString("00000") + ".txt";
            }
            else
            {
                string name = "quote";
                filename = "Trans_" + name + ".txt";
            }
            clipperFileName = Path.Combine(exportDir, filename);

            return InternalExport(context, quoteList, clipperFileName);
        }

        #endregion

        internal bool InternalExport(IContext context, IEnumerable<IQuote> quoteList, string clipperFileName)
        {
            NumberFormatInfo formatProvider = new CultureInfo("en-US", false).NumberFormat;
            formatProvider.CurrencyDecimalSeparator = ".";
            formatProvider.CurrencyGroupSeparator = "";

            string file = "";
            File.Delete(clipperFileName);

            foreach (IQuote quote in quoteList)
            {
                _ReferenceIdList = new Dictionary<IEntity, KeyValuePair<string, string>>();
                _ReferenceList = new Dictionary<string, string>();
                _ReferenceListCount = new Dictionary<string, long>();

                QuoteHeader(ref file, quote, formatProvider);
                QuoteOffre(ref file, quote, formatProvider);

                QuotePart(ref file, quote, "001", formatProvider);
                QuoteSet(ref file, quote, "001", formatProvider);

                file = file + "Fin d'enregistrement OK¤" + Environment.NewLine;
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
            data[i++] = GetQuoteNumber(quoteEntity); //N° devis
            data[i++] = EmptyString(quoteEntity.GetFieldValueAsString("_REFERENCE")); //Indice
            IField field;
            if (quoteEntity.EntityType.TryGetField("_CLIENT_ORDER_NUMBER", out field))
                data[i++] = EmptyString(quoteEntity.GetFieldValueAsString("_CLIENT_ORDER_NUMBER")).ToUpper(); //Repère commercial interne
            else
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
            data[i++] = ""; //Organisme de contrôle
            data[i++] = userCode; //Code employé (défaut) Sce tech. commercial ou méthodes
            data[i++] = ""; //Référence client de l'AO
            data[i++] = ""; //Responsable Méthode chez le client
            data[i++] = contactName; //Responsable achat chez le client
            data[i++] = ""; //Responsable qualité chez le client
            data[i++] = userCode; //Employé Commercial
            data[i++] = ""; //Employé responsable qualité
            data[i++] = ""; //Responsable achat
            data[i++] = ""; //Responsable validation visa
            data[i++] = GetFieldDate(quoteEntity, "_SENT_DATE"); //Date visa resp. (défaut) Sce tech. commercial ou méthodes
            data[i++] = GetFieldDate(quoteEntity, "_SENT_DATE"); //Date visa resp. Commercial
            data[i++] = ""; //Date visa resp. Qualité
            data[i++] = ""; //Date visa resp. Achat
            data[i++] = ""; //Date responsablevalidation visa
            data[i++] = ""; //Date réponse souhaitée
            data[i++] = ""; //Temps mis pour faire le devis
            data[i++] = ""; //Date de début
            data[i++] = "9"; //Monnaie
            data[i++] = ""; //Observations Entête devis
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

            #region Offre pièces

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
                    data[i++] = ExportClipper.GetTransport(quoteEntity); // Port
                    data[i++] = modele; //Modèle
                    data[i++] = "1"; //Imprimable
                    WriteData(data, i, ref file);
                }
            }

            #endregion

            #region Offre Ensembles

            foreach (IEntity setEntity in quote.QuoteSetList)
            {
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
                    data[i++] = ExportClipper.GetTransport(quoteEntity); //Port
                    data[i++] = modele; //Modèle
                    data[i++] = "1"; //Imprimable
                    WriteData(data, i, ref file);
                }
            }

            #endregion

            if (quote.QuoteEntity.GetFieldValueAsLong("_TRANSPORT_PAYMENT_MODE") == 1) // Transport facturé
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
                    data[i++] = ExportClipper.GetTransport(quoteEntity); //Port
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

                {
                    long i = 0;
                    string[] data = new string[50];

                    i = 0;
                    data = new string[50];

                    string transportFamilly = quote.Context.ParameterSetManager.GetParameterValue("_EXPORT", "_CLIPPER_TRANSPORT_FAMILLY").GetValueAsString();

                    data[i++] = "NOMENDV";
                    data[i++] = GetQuoteNumber(quoteEntity); //Code devis
                    data[i++] = reference; //Code pièce
                    data[i++] = rang; //Rang
                    data[i++] = nomendvPhase.ToString(formatProvider); //Phase
                    data[i++] = ""; //Repère
                    data[i++] = ""; //Code article
                    data[i++] = FormatDesignation(""); //Désignation 1
                    data[i++] = FormatDesignation(""); //Désignation 2
                    data[i++] = FormatDesignation(""); //Désignation 3
                    data[i++] = ""; //Temps de réappro
                    data[i++] = "1"; //Qté
                    data[i++] = calCost.ToString("#0.0000", formatProvider); //Px article ou Px/Kg
                    data[i++] = calCost.ToString("#0.0000", formatProvider); //Prix total
                    data[i++] = ""; //Code Fournisseur
                    data[i++] = ""; //2sd fournisseur
                    data[i++] = "1"; //Type
                    data[i++] = ""; //Prix constant
                    data[i++] = ""; //Poids tôle ou article
                    data[i++] = transportFamilly; //Famille
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
                    data[i++] = "0"; //Prix constant ??? semble plutot correcpondre au Modèle
                    data[i++] = "0"; //Modèle ??? semble plutot correcpondre au Prix constant
                    data[i++] = ""; //Qté constant
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
                    data[i++] = ExportClipper.GetTransport(quoteEntity); //Port
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

        private void QuotePart(ref string file, IQuote quote, string rang, NumberFormatInfo formatProvider)
        {
            IEntity quoteEntity = quote.QuoteEntity;
            IEntity clientEntity = quoteEntity.GetFieldValueAsEntity("_FIRM");

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
                    data[i++] = "0"; //N° identifiant GED 9
                    data[i++] = "0"; //N° identifiant GED 10

                    string assistantType = partEntity.GetFieldValueAsString("_ASSISTANT_TYPE");
                    string partFileName = partEntity.GetFieldValueAsString("_FILENAME");

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
                        
                    if (assistantType.Contains("DprAssistant") || isGenericDpr)
                    {
                        string emfFile = partFileName + ".emf";
                        data[i++] = emfFile; //Fichier joint
                    }
                    else
                    {
                        string emfFile = partEntity.GetImageFieldValueAsLinkFile("_PREVIEW");
                        if (emfFile != null)
                            data[i++] = emfFile; //Fichier joint
                        else
                            data[i++] = ""; //Fichier joint
                    }
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
                data[i++] = ""; //Fichier joint
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

        private class GroupedCutOperation
        {
            public string CentreFrais;
            public long GadevisPhase;
            public double UnitPrepTime;
            public double CorrectedUnitPrepTime;
            public double UnitTime;
            public double OpeTime;
            public double UnitCost;
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
                string centreFrais = ExportClipper.GetClipperCentreFrais(subOperationEntity);

                long totalOperationQty = operationEntity.GetFieldValueAsLong("_PARENT_QUANTITY");
                if (totalOperationQty == 0) totalOperationQty = 1;

                GroupedCutOperation groupedCutOperation;
                if (groupedCutOperationList.TryGetValue(centreFrais, out groupedCutOperation) == false)
                {
                    groupedCutOperation = new GroupedCutOperation();
                    groupedCutOperation.CentreFrais = centreFrais;
                    gadevisPhase = gadevisPhase + 10;
                    groupedCutOperation.GadevisPhase = gadevisPhase;
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
                data[i++] = tpsPrep.ToString("#0.0000", formatProvider); //Tps Prep
                data[i++] = tpsUnit.ToString("#0.0000", formatProvider); //Tps Unit (heure)

                double unitCost = groupedCutOperation.UnitCost;
                data[i++] = (unitCost * mainParentQuantity).ToString("#0.0000", formatProvider); //Coût Opération

                double hourlyCost = 0;
                if (((tpsPrep / parentQty) + tpsUnit != 0))
                    hourlyCost = unitCost / ((tpsPrep / parentQty) + tpsUnit);
                data[i++] = hourlyCost.ToString("#0.0000", formatProvider); //Taux horaire (/heure)

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
                data[i++] = ExportClipper.GetClipperCentreFrais(subOperationEntity); //Centre de frais 

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
                    data[i++] = ExportClipper.GetSttFamily(subOperationEntity); //Famille
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
                long supplyQty = Convert.ToInt64(doubleSupplyQty);
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
}
