using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace HMI.Presentation.Twr.Views
{
    public class HMIConfigHelper
    {
        
        public static bool CambioModoNocturno(string filename, string clave, string actualvalue, string newvalue)
        {
            if (File.Exists(filename))
            {
                var find = $"      <setting name=\"{clave}\" serializeAs=\"String\">\r\n        <value>{actualvalue}</value>\r\n      </setting>";
                var repl = $"      <setting name=\"{clave}\" serializeAs=\"String\">\r\n        <value>{newvalue}</value>\r\n      </setting>";


                var data = File.ReadAllText(filename);
                data = data.Trim();
                find = find.Trim();
                if (data.IndexOf(find) == -1)
                {
                    //return false;
                    var find1 = $"</HMI.Presentation.Twr.Properties.Settings>";
                    var repl1 = repl + "\r\n" + find1;
                    if (data.IndexOf(find1) == -1)
                        return false;
                    File.WriteAllText(filename + ".old", File.ReadAllText(filename)); // backup

                    File.WriteAllText(filename, data.Replace(find1, repl1)); //
                    return true;
                }

                File.WriteAllText(filename + ".old", File.ReadAllText(filename)); // backup

                File.WriteAllText(filename, data.Replace(find, repl)); //
                return true;
            }
            return false;
        }

        public static bool RestoreModoNocturno(string filename, string clave)
        {
            if (File.Exists(filename))
            {
                var data = File.ReadAllText(filename + ".old");
                if (data.IndexOf(clave) == -1) return false;

                File.WriteAllText(filename, File.ReadAllText(filename + ".old")); // recuperabackup

                return true;
            }
            return false;
        }
    }
}

