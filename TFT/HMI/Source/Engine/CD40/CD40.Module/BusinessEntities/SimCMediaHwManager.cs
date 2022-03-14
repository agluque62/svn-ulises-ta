using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Net;
using System.Timers;
using System.Threading.Tasks;

using Microsoft.Win32.SafeHandles;

using HMI.CD40.Module.Properties;
using HMI.Model.Module.Messages;
using HMI.CD40.Module.Snmp;

using U5ki.Infrastructure;
using Utilities;

using NLog;

namespace HMI.CD40.Module.BusinessEntities
{
    /// <summary>
    /// Implementacion de la Clase para dispositivos CMEDIA
    /// </summary>
    class SimCMediaSndDev : ISndDevIO
    {
        #region INTERFACE ISndDevIO

        public event GenericEventHandler<bool> JackConnected;
        public event GenericEventHandler<bool> PttPulsed;

        public CORESIP_SndDevType Type { get; set; }
        public CORESIP_SndDevType SubType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private bool _Jack = false;
        public bool Jack
        {
            get { return _Jack; }
            set
            {
                if (value != _Jack)
                {
                    LogManager.GetCurrentClassLogger().Debug("HidCMediaSndDev. JACK <{0}> => {1}", Type, value);
                    _Jack = value;
                    General.SafeLaunchEvent(JackConnected, this, _Jack);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private bool _Ptt = false;
        public bool Ptt
        {
            get { return _Ptt; }
            set
            {
                if (value != _Ptt)
                {
                    LogManager.GetCurrentClassLogger().Debug("HidCMediaSndDev. PTT  <{0}> => {1}", Type, value);
                    _Ptt = value;
                    General.SafeLaunchEvent(PttPulsed, this, _Ptt);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsValid
        {
            get
            {
                return Error == false && Type != CORESIP_SndDevType.CORESIP_SND_UNKNOWN;
            }
            set
            {
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Error { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="OnOff"></param>
        public void SenGrabacion(bool OnOff)
        {            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public bool GetPresencia(int pos)
        {
            if (pos < file_in_content.Length)
            {
                return file_in_content[pos] == '1' ? true : false;
            }
            return false;
        }

        public int GetValue(int pos)
        {
            if (pos < file_in_content.Length)
            {
                return file_in_content[pos];
            }
            return -1;
        }

        /**  */
        public int SetGpio(int gpio, byte estado)
        {
            int pos = gpio == 4 ? 0 : 1;
            var array = file_out_content.ToCharArray();
            array[pos] = estado == 0 ? '0' : '1';
            file_out_content = new string(array);
            File.WriteAllText(FileOutPath, file_out_content);
            return 0;
        }

        /// <summary>
        /// GPI0 -- GPI7.
        /// </summary>
        /// <returns></returns>
        protected byte GetGpio()
        {
            return 0;
        }

        /** 20180409. Para obtener la version del FIRMWARE*/
        public string SwVersion { get { return "0301"; } }
        public string SwDate { get { return "20180409"; } }
        #endregion

        #region CONSTRUCTOR
        private static int _g_instance = 0;
        private int _instance = _g_instance++;

        /// <summary>
        /// 
        /// </summary>
        public SimCMediaSndDev(string path, int n)
        {
            FileInPath = Path.Combine(path, string.Format("cmedia_#{0}.sim", n));
            FileOutPath = Path.Combine(path, string.Format("cmedia_#{0}_out.sim", n));
            _ncmedia = n;
            Open();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Reset()
        {
            try
            {
                Dispose();
                Open();
            }
            catch (Exception x)
            {
                LogManager.GetCurrentClassLogger().Warn("RESET", x);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void CheckDevice()
        {
            file_in_content = File.Exists(FileInPath) ? File.ReadAllText(FileInPath) : "1011";
            Error = !File.Exists(FileInPath);
            File.WriteAllText(FileOutPath, file_out_content);
        }

        #endregion

        #region PRIVADOS

        /// <summary>
        /// 
        /// </summary>
        private void Open()
        {
            /** 20180524. Si no existe el fichero lo creo con todo habilitado */
            if (File.Exists(FileInPath) == false) File.WriteAllText(FileInPath, "1011");
            CheckDevice();
        }


        /// <summary>
        /// 
        /// </summary>
        private void Dispose()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        //private string read_file()
        //{
        //    System.IO.StreamReader file = new StreamReader(FileInPath);
        //    string leido = file.ReadLine();
        //    file.Close();
        //    return leido;
        //}
        
        /// <summary>
        /// 
        /// </summary>
        Logger _Log = LogManager.GetCurrentClassLogger();
        string FileInPath = string.Empty;
        string file_in_content = "1011";
        string FileOutPath = string.Empty;
        string file_out_content = "0000";
        int _ncmedia = 0;

        #endregion

        #region PUBLICOS

        #endregion
    }

    /// <summary>
    /// Implementacion de la Clase para CMEDIA
    /// </summary>
    class SimCMediaHwManager : HwManager
    {
        string pathForFiles = "c:\\tmp";
        SimCMediaSndDev _dev1_eje = null, _dev2_ayu = null;
        SimCMediaSndDev _dev3_alr = null, _dev4_alt = null;

        /// <summary>
        /// 
        /// </summary>
        public SimCMediaHwManager()
            : base()
        {
            pathForFiles = Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "cmedia_sim");
            System.IO.Directory.CreateDirectory(pathForFiles);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Init()
        {
            if (GlobalError) return;

            base.Init();

            _dev1_eje = new SimCMediaSndDev(pathForFiles, 1) { Type = CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP };
            _dev2_ayu = new SimCMediaSndDev(pathForFiles, 2) { Type = CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP };
            _dev3_alr = new SimCMediaSndDev(pathForFiles, 3) { Type = CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER };
            _dev4_alt = new SimCMediaSndDev(pathForFiles, 4) { Type = CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER };

            _SOSndDvcs.Clear();
            _SOSndDvcs.Add(CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP);
            _SOSndDvcs.Add(CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP);
            _SOSndDvcs.Add(CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER);
            _SOSndDvcs.Add(CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Start()
        {
            if (!GlobalError)
            {

                base.Start();

                _Logger.Debug("SimCMediaHwManager.Start. Numero de dispositivos = {0}", 4);

                StartDevice(_dev1_eje);
                StartDevice(_dev2_ayu);
                StartDevice(_dev3_alr);
                StartDevice(_dev4_alt);
            }
            /** Para Inicializar el estado de JACKS */
            CheckPresencia();

            HwSupervisor.Enabled = true;
            _Logger.Debug("SimCMediaHwManager.Starting Supervision TIMER");

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool CheckDevs()
        {
            if (!GlobalError)
            {
                if (_dev1_eje != null ) _dev1_eje.CheckDevice();
                if (_dev2_ayu != null ) _dev2_ayu.CheckDevice();
                if (_dev3_alr != null ) _dev3_alr.CheckDevice();
                if (_dev4_alt != null ) _dev4_alt.CheckDevice();

                CheckPresencia();
                return _SndDevs.Values.Count == 4;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public int Count
        {
            get
            {
                return 4;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="ON"></param>
        public override void OnOffLed(CORESIP_SndDevType Type, byte OnOff)
        {
            switch (Type)
            {
                case CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER:
                    if (_dev4_alt != null)
                        _dev4_alt.SetGpio(4, OnOff);
                    break;
                case CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER:
                    if (_dev3_alr != null)
                        _dev3_alr.SetGpio(4, OnOff);
                    break;
                case CORESIP_SndDevType.CORESIP_SND_HF_SPEAKER:
                    if (_dev3_alr != null)
                        _dev3_alr.SetGpio(5, OnOff);
                    break;
                default:
                    _Logger.Error("SimCMediaHwManager.OnOffLed. Tipo Altavoz No Soportado: {0}", Type);
                    break;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public override void ApagarSndSpeaker()
        {
            OnOffLed(CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER, 0);
            OnOffLed(CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER, 0);
            OnOffLed(CORESIP_SndDevType.CORESIP_SND_HF_SPEAKER, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        bool generandoRafaga = false;
        protected void CheckPresencia()
        {
            if (!GlobalError)
            {
                if (Settings.Default.CMediaBkpVersion == "B41A")
                {
                    if (_dev1_eje != null && _dev2_ayu != null && _dev3_alr != null && _dev4_alt != null)
                    {
                        _dev1_eje.Jack = _dev3_alr.GetPresencia(0);         // JACK de Ejecutivo
                        _dev1_eje.Ptt = _dev1_eje.GetPresencia(0);          // PTT de Ejecutivo

                        _dev2_ayu.Jack = _dev3_alr.GetPresencia(1);         // JACK de Ayudante.
                        _dev2_ayu.Ptt = _dev2_ayu.GetPresencia(1);          // PTT de Ayudante.

                        _dev3_alr.Jack = _dev3_alr.GetPresencia(0);         // Presencia Altavoz Radio VHF
                        _dev3_alr.Ptt = _dev2_ayu.GetPresencia(0);          // Presencia Altavoz Radio HF.

                        _dev4_alt.Jack = _dev4_alt.GetPresencia(0);         // Presencia de Altavoz LC.
                        _dev4_alt.Ptt = _dev4_alt.GetPresencia(1);          // Presencia Cable Grabacion.
                    }
                }
                else
                {
                    if (_dev1_eje != null && _dev2_ayu != null && _dev3_alr != null && _dev4_alt != null)
                    {
                        if (_dev1_eje.GetValue(1) == '2')
                        {
                            if (generandoRafaga == false)
                            {
                                generandoRafaga = true;

                                Task.Factory.StartNew(() =>
                                {
                                    Random r = new Random(DateTime.Now.Millisecond);
                                    /** Simula una rafaga de PTT's... */
                                    while (generandoRafaga)
                                    {
                                        _dev1_eje.Ptt = true;
                                        Task.Delay(r.Next(75, 500)).Wait();
                                        _dev1_eje.Ptt = false;
                                        Task.Delay(r.Next(75, 500)).Wait();
                                    }
                                    _dev1_eje.Ptt = true;
                                });
                            }
                        }
                        else
                        {
                            if (generandoRafaga == true)
                                generandoRafaga = false;
                            else
                            {

                                _dev1_eje.Jack = _dev1_eje.GetPresencia(0);         // JACK de Ejecutivo
                                _dev1_eje.Ptt = _dev1_eje.GetPresencia(1);          // PTT de Ejecutivo

                                _dev2_ayu.Jack = _dev2_ayu.GetPresencia(0);         // JACK de Ayudante.
                                _dev2_ayu.Ptt = _dev2_ayu.GetPresencia(1);          // PTT de Ayudante.

                                _dev3_alr.Jack = _dev3_alr.GetPresencia(0);         // Presencia Altavoz Radio VHF
                                _dev3_alr.Ptt = _dev3_alr.GetPresencia(1);          // Presencia Altavoz Radio HF.

                                _dev4_alt.Jack = _dev4_alt.GetPresencia(0);         // Presencia de Altavoz LC.
                                _dev4_alt.Ptt = _dev4_alt.GetPresencia(1);          // Presencia Cable Grabacion.
                            }
                        }
                    }
                }
            }
        }

        protected bool GlobalError
        {
            get
            {
                return File.Exists(Path.Combine(pathForFiles, "globalerror"));
            }
        }

        #region Audio-Static

        /// <summary>
        /// 
        /// </summary>
        static Dictionary<CORESIP_SndDevType, int> _input_channels = new Dictionary<CORESIP_SndDevType, int>();
        static Dictionary<CORESIP_SndDevType, int> _output_channels = new Dictionary<CORESIP_SndDevType, int>();

        /// <summary>
        /// 
        /// </summary>
        static public void LoadChannels()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_enabled"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        static public int AddDevice(bool _enabled, CORESIP_SndDevType tipo, CMediaDevMode mode)
        {
            return -1;
        }

        #endregion
    }
}
