using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AF_Import_ODBC_Clipper_AlmaCam;
using AF_ImportTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Wpm.Implement.Manager;


namespace ConsoleApp2
{

   

    class Program
    {
        static void Main(string[] args)
        {
            bool import_matiere = true;
            bool tube_rond = true;
            bool rond =true;
            bool tube_rectangle = true;
            bool tube_carre = true;
            bool tube_flat = true;
            bool tube_speciaux = true;

            Clipper_ImportTubes_Processor tubeimporter = new Clipper_ImportTubes_Processor(import_matiere,  tube_rond,  rond,  tube_rectangle,  tube_carre, tube_flat,  tube_speciaux);
            tubeimporter.Execute();


        }
    }
}
