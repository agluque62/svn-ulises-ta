using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Utilities
{
    public class StartSync
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ProcessName"></param>
        /// <param name="Seconds"></param>
        /// 
        /// <returns></returns>
        public static bool ProcessRunningSync(String ProcessName, int Seconds = 0)
        {
            DateTime dtEntrada = DateTime.Now;
            do
            {
                try
                {
                    Process pname = Process.GetProcesses().Where(p => p.ProcessName.ToLower().Contains(ProcessName.ToLower())).FirstOrDefault();
                    
                    if (pname != null)
                        return true;

                }
                catch (Exception )
                {
                    return false;
                }

                System.Threading.Thread.Yield();
                
            } while ((DateTime.Now-dtEntrada).Seconds < Seconds);

            return false;
        }
    }
}
