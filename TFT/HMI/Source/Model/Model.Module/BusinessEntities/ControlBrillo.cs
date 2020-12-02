using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Management;
using NLog;

namespace HMI.Model.Module.BusinessEntities
{
    class ControlBrillo
    {
        Action<string> Log { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ControlBrillo(Action<string> logmsg)
        {
            Log = logmsg;
            mclass = new ManagementClass("WmiMonitorBrightnessMethods");
            mclass.Scope = new ManagementScope(@"\\.\root\wmi");
            instances = mclass.GetInstances();
            if (instances == null || instances.Count==0)
            {
                throw new ApplicationException("Control de Brillo 2 incompatible con la maquina");
            }
            Log($"Instances: {instances}, {instances.Count}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="percent"></param>
        public void SetBrilloPerCent_old(ushort percent)
        {
            foreach (ManagementObject instance in instances)
            {
                ulong timeout = 1; // en segundos
                ushort brightness = percent; // en porcentaje
                object[] args = new object[] { timeout, brightness };
                instance.InvokeMethod("WmiSetBrightness", args);
            }
        }

        public void SetBrilloPerCent(ushort percent)
        {
            Log($"SetBrilloPerCent: percent=>{percent}");
            var mclass = new ManagementClass("WmiMonitorBrightnessMethods") { Scope = new ManagementScope(@"\\.\root\wmi") };
            foreach (ManagementObject instance in mclass.GetInstances())
            {
                ulong timeout = 1; // en segundos
                ushort brightness = percent; // en porcentaje
                object[] args = new object[] { timeout, brightness };
                Log($"Invoking instance <{instance}> => (timeout {timeout}, brillo(%) {brightness}");
                instance.InvokeMethod("WmiSetBrightness", args);
            }
        }

        private ManagementClass mclass = null;
        private ManagementObjectCollection instances = null;
    }
}
