using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NLog;
using NAudio;
using NAudio.CoreAudioApi;
using IniParser;
using IniParser.Model;
using System.Diagnostics;

namespace HMI.CD40.Module.BusinessEntities
{
    class AudioDeviceInfo
    {
        public string id { get; set; }
        public float vmax { get; set; }
        public float vmin { get; set; }
        public int channel { get; set; }
        public bool speaker { get; set; }
    }

    class MMAudioDeviceManager
    {       
        /// <summary>
        /// 
        /// </summary>
        public virtual void Init()
        {
            DevCol = MMDE.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active);
        }
 
        public void Dispose()
        {
            foreach (var dev in DevCol)
                dev.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<AudioDeviceInfo> InfoDevices(bool isDeviceOut)
        {
            List<AudioDeviceInfo> devs = new List<AudioDeviceInfo>();
            foreach (MMDevice dev in DevCol)
            {
                if ((isDeviceOut == true && dev.DataFlow == DataFlow.Render) ||
                    (isDeviceOut == false && dev.DataFlow == DataFlow.Capture))
                {
                    for (int channel = 0; channel < dev.AudioEndpointVolume.Channels.Count; channel++)
                    {
                        devs.Add(new AudioDeviceInfo()
                            {
                                id = dev.DeviceFriendlyName,
                                vmax = dev.AudioEndpointVolume.VolumeRange.MaxDecibels,
                                vmin = dev.AudioEndpointVolume.VolumeRange.MinDecibels,
                                channel = channel,
                                speaker = dev.DataFlow == DataFlow.Capture ? false : true
                            });
                    }
                }
            }
            return devs;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="device_id"></param>
        /// <returns></returns>
        public float VolumeGet(string device_id, bool isDeviceOut=true)
        {
            MMDevice dev = DeviceGet(device_id, isDeviceOut ? DataFlow.Render : DataFlow.Capture);
            if (dev != null)
            {
                return dev.AudioEndpointVolume.MasterVolumeLevel;
            }
            logger.Error("MMAudioDeviceManager.VolumeGet({0},{1}) Error. No existe el dispositivo", device_id, isDeviceOut);
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="device_id"></param>
        /// <param name="channel"></param>
        /// <param name="volume"></param>
        /// <returns></returns>
        public float VolumeGet(string device_id, int channel, bool isDeviceOut=true)
        {
            MMDevice dev = DeviceGet(device_id, isDeviceOut ? DataFlow.Render : DataFlow.Capture);
            if (dev != null)
            {
                if (dev.AudioEndpointVolume.Channels.Count > channel)
                {
                    return dev.AudioEndpointVolume.Channels[channel].VolumeLevel;
                }
            }
            logger.Error("MMAudioDeviceManager.VolumeGet({0},{1},{2}) Error. No existe el dispositivo o el canal", device_id, channel, isDeviceOut);
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="device_id"></param>
        /// <param name="volume"></param>
        /// <returns></returns>
        public bool VolumeSet(string device_id, float volume, bool isDeviceOut=true)
        {
            MMDevice dev = DeviceGet(device_id, isDeviceOut ? DataFlow.Render : DataFlow.Capture);
            if (dev != null)
            {
                dev.AudioEndpointVolume.MasterVolumeLevel = volume;
                System.Threading.Thread.Sleep(100);
                dev.AudioEndpointVolume.MasterVolumeLevel = volume;
                return true;
            }
            logger.Error("MMAudioDeviceManager.VolumeSet({0},{1},{2}) Error. No existe el dispositivo", device_id, volume, isDeviceOut);
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="device_id"></param>
        /// <param name="channel"></param>
        /// <param name="volume"></param>
        /// <returns></returns>
        public bool VolumeSet(string device_id, int channel, float volume, bool isDeviceOut=true)
        {
            try
            {
                MMDevice dev = DeviceGet(device_id, isDeviceOut ? DataFlow.Render : DataFlow.Capture);
                if (dev != null)
                {
                    using (var enpv = dev.AudioEndpointVolume)
                    {
                        if (enpv.Channels.Count > channel)
                        {
                            enpv.Channels[channel].VolumeLevel = volume;
                            System.Threading.Thread.Sleep(100);
                            enpv.Channels[channel].VolumeLevel = volume;
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("MMAudioDeviceManager.VolumeSet Exception {0} dev {1} channel {2}", ex.Message, device_id, channel);
                return false;
            }
            logger.Error("MMAudioDeviceManager.VolumeSet({0},{1},{2},{3}) Error. No existe el dispositivo o el canal", device_id, channel, volume, isDeviceOut);
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="device_id"></param>
        /// <returns></returns>
        protected MMDevice DeviceGet(string device_id, DataFlow dataflow= DataFlow.Render)
        {
            if (DevCol == null)
                return null;
            foreach (MMDevice dev in DevCol)
            {
                if (dev.DeviceFriendlyName.Contains(device_id)==true && dev.DataFlow == dataflow)
                    return dev;
            }
            logger.Error("MMAudioDeviceManager.DeviceGet({0},{1}) Error. No encuentro el dispositivo", device_id, dataflow);
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        private MMDeviceEnumerator MMDE = new MMDeviceEnumerator();
        private MMDeviceCollection DevCol = null;
        protected Logger logger = LogManager.GetCurrentClassLogger();
    }

    /// <summary>
    /// 
    /// </summary>
    class CMedia_MMAudioDeviceManager : MMAudioDeviceManager
    {
        long MemoryOnInit = 0;
        /// <summary>
        /// 
        /// </summary>
        class CMediaDevsTableItem
        {
            public string idDev { get; set; }
            public int channel { get; set; }
            public bool isDevOut { get; set; }
            public float def_val { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Init()
        {
            ReadINI();
            SetDevs();
            MemoryOnInit = Process.GetCurrentProcess().PrivateMemorySize64;
        }

        /// <summary>
        /// Se llama cada segundo...
        /// </summary>
        public void Tick()
        {
            if (val_tsup > 0)
            {
                if (--current_sup <= 0)
                {
                    SetDevs();
                    current_sup = val_tsup;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void SetDevs()
        {
            base.Init();
            foreach (CMediaDevsTableItem dev in devTable.Values)
            {
                VolumeSet(dev.idDev, dev.channel, dev.def_val, dev.isDevOut);
            }
            base.Dispose();
        }
        
        /// <summary>
        /// 
        /// </summary>
        int current_sup = 0;
        int val_tsup = 0;
        Dictionary<string, CMediaDevsTableItem> devTable = new Dictionary<string, CMediaDevsTableItem>();
        void ReadINI()
        {
            try
            {
                var ini_parser = new FileIniDataParser();
                IniData ini_data = ini_parser.ReadFile("winaudio.ini");

                val_tsup = int.Parse(ini_data["GENERAL"]["TSUP"]);
                current_sup = val_tsup;

                devTable.Add("GrabacionEjecutivo", new CMediaDevsTableItem() { idDev = "CWP USB Device # 01", channel = 0, isDevOut = true, def_val = float.Parse(ini_data["SALIDAS"]["GrabacionEjecutivo"]) });
                devTable.Add("CascoEjecutivo", new CMediaDevsTableItem() { idDev = "CWP USB Device # 01", channel = 1, isDevOut = true, def_val = float.Parse(ini_data["SALIDAS"]["CascoEjecutivo"]) });

                devTable.Add("GrabacionAyudante", new CMediaDevsTableItem() { idDev = "CWP USB Device # 02", channel = 0, isDevOut = true, def_val = float.Parse(ini_data["SALIDAS"]["GrabacionAyudante"]) });
                devTable.Add("CascoAyudante", new CMediaDevsTableItem() { idDev = "CWP USB Device # 02", channel = 1, isDevOut = true, def_val = float.Parse(ini_data["SALIDAS"]["CascoAyudante"]) });
                
                devTable.Add("AltavozRadioHF", new CMediaDevsTableItem() { idDev = "CWP USB Device # 03", channel = 0, isDevOut = true, def_val = float.Parse(ini_data["SALIDAS"]["AltavozRadioHF"]) });
                devTable.Add("AltavozRadioVHF", new CMediaDevsTableItem() { idDev = "CWP USB Device # 03", channel = 1, isDevOut = true, def_val = float.Parse(ini_data["SALIDAS"]["AltavozRadioVHF"]) });
                
                devTable.Add("GrabacionIntegrada", new CMediaDevsTableItem() { idDev = "CWP USB Device # 04", channel = 0, isDevOut = true, def_val = float.Parse(ini_data["SALIDAS"]["GrabacionIntegrada"]) });
                devTable.Add("AltavozLC", new CMediaDevsTableItem() { idDev = "CWP USB Device # 04", channel = 1, isDevOut = true, def_val = float.Parse(ini_data["SALIDAS"]["AltavozLC"]) });

                devTable.Add("RetornoEjecutivo", new CMediaDevsTableItem() { idDev = "CWP USB Device # 01", channel = 0, isDevOut = false, def_val = float.Parse(ini_data["ENTRADAS"]["RetornoEjecutivo"]) });
                devTable.Add("MicrofonoEjecutivo", new CMediaDevsTableItem() { idDev = "CWP USB Device # 01", channel = 1, isDevOut = false, def_val = float.Parse(ini_data["ENTRADAS"]["MicrofonoEjecutivo"]) });

                devTable.Add("RetornoAyudante", new CMediaDevsTableItem() { idDev = "CWP USB Device # 02", channel = 0, isDevOut = false, def_val = float.Parse(ini_data["ENTRADAS"]["RetornoAyudante"]) });
                devTable.Add("MicrofonoAyudante", new CMediaDevsTableItem() { idDev = "CWP USB Device # 02", channel = 1, isDevOut = false, def_val = float.Parse(ini_data["ENTRADAS"]["MicrofonoAyudante"]) });

                devTable.Add("RetornoAltavozRadioHF", new CMediaDevsTableItem() { idDev = "CWP USB Device # 03", channel = 0, isDevOut = false, def_val = float.Parse(ini_data["ENTRADAS"]["RetornoAltavozRadioHF"]) });
                devTable.Add("RetornoAltavozRadioVHF", new CMediaDevsTableItem() { idDev = "CWP USB Device # 03", channel = 1, isDevOut = false, def_val = float.Parse(ini_data["ENTRADAS"]["RetornoAltavozRadioVHF"]) });

                devTable.Add("Libre", new CMediaDevsTableItem() { idDev = "CWP USB Device # 04", channel = 0, isDevOut = false, def_val = 0 });
                devTable.Add("RetornoAltavozLC", new CMediaDevsTableItem() { idDev = "CWP USB Device # 04", channel = 1, isDevOut = false, def_val = float.Parse(ini_data["ENTRADAS"]["RetornoAltavozLC"]) });
            }
            catch (Exception x)
            {
                logger.Error("Error-Excepcion en CMedia_MMAudioDeviceManager.readINI", x);
            }
        }
    }
}
