using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using   System.Management;

namespace HMI.Model.Module.BusinessEntities
{
    class ControlBrillo
    {
        /// <summary>
        /// 
        /// </summary>
        public ControlBrillo()
        {
            mclass = new ManagementClass("WmiMonitorBrightnessMethods");
            mclass.Scope = new ManagementScope(@"\\.\root\wmi");
            instances = mclass.GetInstances();
            if (instances == null || instances.Count==0)
            {
                throw new ApplicationException("Control de Brillo 2 incompatible con la maquina");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="percent"></param>
        public void SetBrilloPerCent(ushort percent)
        {
            foreach (ManagementObject instance in instances)
            {
                ulong timeout = 1; // en segundos
                ushort brightness = percent; // en porcentaje
                object[] args = new object[] { timeout, brightness };
                instance.InvokeMethod("WmiSetBrightness", args);
            }
        }

        private ManagementClass mclass = null;
        private ManagementObjectCollection instances = null;
    }
}
