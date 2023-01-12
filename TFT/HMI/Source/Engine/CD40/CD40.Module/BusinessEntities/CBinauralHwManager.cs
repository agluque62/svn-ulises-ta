using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Net;
using System.Timers;
using System.Threading;

using Microsoft.Win32.SafeHandles;

using HMI.CD40.Module.Properties;
using HMI.Model.Module.Messages;
using HMI.CD40.Module.Snmp;

using U5ki.Infrastructure;
using Utilities;
using NLog;
using System.Linq;
//using Lextm.SharpSnmpLib;
//using Lextm.SharpSnmpLib.Messaging;

namespace HMI.CD40.Module.BusinessEntities
{
    /// <summary>
    /// Implementacion de la Clase para dispositivos Genericos
    /// </summary>
    class HidBinauralSndDev : ISndDevIO
    {
        #region INTERFACE ISndDevIO
        /// <summary>
        /// Log this class
        /// </summary>
        private static Logger _Logger = LogManager.GetCurrentClassLogger();
        
        public event GenericEventHandler<bool> JackConnected;
        public event GenericEventHandler<bool> PttPulsed;

        public CORESIP_SndDevType Type { get; set; }
        public CORESIP_SndDevType SubType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private bool _Jack = false;
        public bool Jack {
            get { return _Jack; }
            set
            {
                if (value != _Jack)
                {
                    _Jack = value;
                    General.SafeLaunchEvent(JackConnected, this, _Jack);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private bool _Ptt = false;
        public bool Ptt {
            get { return _Ptt; }
            set
            {
                if (value != _Ptt)
                {
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
                /** Usage=4, UsagePage=1 */
                if (MyHid.Capabilities.Usage != 4 || MyHid.Capabilities.UsagePage != 1)
                {
                    return false;
                }

                return Error==false && Type != CORESIP_SndDevType.CORESIP_SND_UNKNOWN;            
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
        public void LedAltavoz(bool OnOff)
        {
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
        /// <param name="gpio"></param>
        /// <param name="estado"></param>
        /// <returns></returns>
        protected int SetGpio(int gpio, byte estado) 
        {
            return 0; 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected byte GetGpio()         
        {
            /** Marco el Registro a Leer */
            byte[] cmd = new byte[MyHid.Capabilities.InputReportByteLength];
            if (GenericHid.Hid.HidD_GetInputReport(hidHandle, ref cmd[0], MyHid.Capabilities.InputReportByteLength) == true)
            {
                _Logger.Trace("LEIDO HID-REPORT: {0}", BitConverter.ToString(cmd));
                return (byte)cmd[_PttByte];
            }
            return 0;
        }

        /** 20180409. Para obtener la version del FIRMWARE*/
        public string SwVersion { get { return "GENERIC-VER-TODO"; } }
        public string SwDate { get { return "GENERIC-SN-TODO"; } }
        #endregion

        #region CONSTRUCTOR
        private static int _g_instance = 0;
        private int _instance = _g_instance++;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dev"></param>
        public HidBinauralSndDev(GenericHid.HidDeviceManagement.DeviceDescription dev)
        {
            try
            {
                Type = CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP;      
                _dev = dev;

                if (!DevList.ByteBit(_dev.id, ref _PttByte, ref _PttBit))
                    throw new Exception(String.Format("No encuentro la referencia del dispositivo {0}",_dev.id));

                Open();
                /** Arrancar el Lazo de Escucha */
                StartRead();
                Error = false;
            }
            catch (Exception x)
            {
                /** TODO Gestion de la Excepcion */
                _Logger.Fatal("HidBinauralSndDev", x);
                Error = true;
            }
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
                StartRead();
                Error = false;
            }
            catch (Exception x)
            {
                _Logger.Fatal("RESET", x);
            }
        }

        #endregion

        #region PRIVADOS

        GenericHid.HidDeviceManagement.DeviceDescription _dev = null;
        FileStream _Stream;
        byte[] _Buffer;

        bool Reading { get; set; }
        private SafeFileHandle hidHandle;
        private SafeFileHandle readHandle;
        private GenericHid.Hid MyHid = new GenericHid.Hid();

        /// <summary>
        /// 
        /// </summary>
        int _PttByte = 0x01;
        int _PttBit = 0x01;  

        /// <summary>
        /// 
        /// </summary>
        private void Open()
        {
            Reading = false;
            hidHandle = GenericHid.FileIO.CreateFile(_dev.path, 0,
                GenericHid.FileIO.FILE_SHARE_READ | GenericHid.FileIO.FILE_SHARE_WRITE, null, GenericHid.FileIO.OPEN_EXISTING, 0, 0);

            MyHid.DeviceAttributes.Size = Marshal.SizeOf(MyHid.DeviceAttributes);
            GenericHid.Hid.HidD_GetAttributes(hidHandle, ref MyHid.DeviceAttributes);
            MyHid.GetDeviceCapabilities(hidHandle);

            /** Leer el Hardware */
            byte _initial_report = GetGpio();

            /** El tipo será siempre EJECUTIVO / ALUMNO */
            Type = HidType(_initial_report);

            /** Siempre a TRUE */
            Jack = HidJack(_initial_report);

            /** Leer PTT en el HW */
            Ptt = HidPtt(_initial_report);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iResult"></param>
        private void ReadCompleted(IAsyncResult iResult)
        {
            try
            {
                if (_Stream.EndRead(iResult) == MyHid.Capabilities.InputReportByteLength)
                {
                    /** Leer PTT en el HW */
                    Ptt = HidPtt(_Buffer[_PttByte]);        //HidPtt((byte)(~_Buffer[1]));
                    _Stream.BeginRead(_Buffer, 0, MyHid.Capabilities.InputReportByteLength, new AsyncCallback(ReadCompleted), null);
                }
                else
                    _Logger.Error("HID Callback...");
            }
            catch (System.OperationCanceledException)
            {
            }
            catch (System.IO.IOException)
            {
                // Se ha desconectado usb.
                Error = true;
            }
            catch (Exception x)
            {
                _Logger.Fatal("ReadCompleted", x);
                Error = true;
            }
        }

        /// <summary>
        /// Mascara Configurable.
        /// </summary>
        /// <param name="_data"></param>
        /// <returns></returns>
        private bool HidPtt(byte _data)
        {
            int bptt = _data & (byte )_PttBit;

            _Logger.Debug("HidBinauralSndDev.HidPtt ({0})-->({1}))", _data, bptt);
            return bptt != 0 ? true : false;
        }

        /// <summary>
        /// Siempre Será ON...
        /// </summary>
        /// <param name="_data"></param>
        /// <returns></returns>
        private bool HidJack(byte _data)
        {
            return true;
        }

        /// <summary>
        /// Siempre Será EJECUTIVO / ALUMNO
        /// </summary>
        /// <param name="_data"></param>
        /// <returns></returns>
        private CORESIP_SndDevType HidType(byte _data)
        {
            return CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_data"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        private byte [] GetReport()
        {
            byte [] _data = new byte[MyHid.Capabilities.InputReportByteLength];
            if (GenericHid.Hid.HidD_GetInputReport(hidHandle, ref _data[0], MyHid.Capabilities.InputReportByteLength) == true)
                return _data;
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_report"></param>
        private void SetReport(byte[] _report)
        {
            byte[] _data = new byte[MyHid.Capabilities.OutputReportByteLength];
            _report.CopyTo(_data,0);
            GenericHid.Hid.HidD_SetOutputReport(hidHandle, ref _data[0], MyHid.Capabilities.OutputReportByteLength); 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool StartRead()
        {
            if (!Reading)
            {
                readHandle = GenericHid.FileIO.CreateFile(_dev.path,
                    GenericHid.FileIO.GENERIC_READ, GenericHid.FileIO.FILE_SHARE_READ | GenericHid.FileIO.FILE_SHARE_WRITE, null,
                    GenericHid.FileIO.OPEN_EXISTING, GenericHid.FileIO.FILE_FLAG_OVERLAPPED, 0);

                if (!readHandle.IsInvalid)
                {
                    _Stream = new FileStream(readHandle, FileAccess.Read, MyHid.Capabilities.InputReportByteLength, true);
                    _Buffer = new byte[MyHid.Capabilities.InputReportByteLength];
                    _Stream.BeginRead(_Buffer, 0, MyHid.Capabilities.InputReportByteLength, new AsyncCallback(ReadCompleted), null);
                    Reading = true;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool StopRead()
        {
            if (Reading)
            {
                _Stream.Close();
                readHandle.Close();

                Reading = false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        private void Dispose()
        {
            if (Reading)
            {
                StopRead();
            }
            hidHandle.Close();
        }

        #endregion

        #region PUBLICOS

        #endregion
    }

    /// <summary>
    /// Implementacion de la Clase para Genericos
    /// </summary>
    class HidBinauralHwManager : HwManager
    {
        class DeviceSupervision
        {
            static Dictionary<CORESIP_SndDevType, bool> Estado = new Dictionary<CORESIP_SndDevType, bool>()
            {
                {CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP, false},
                {CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER, false},
                {CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER, false}
            };

            /// <summary>
            /// 
            /// </summary>
            /// <param name="tipo"></param>
            /// <param name="current"></param>
            /// <returns></returns>
            static public bool Supervisa(CORESIP_SndDevType tipo, bool current)
            {
                foreach (KeyValuePair<CORESIP_SndDevType, bool> par in Estado)
                {
                    if (tipo == par.Key && current != par.Value)
                    {
                        Estado[tipo] = current;
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected List<GenericHid.HidDeviceManagement.DeviceIdentification> _did = new List<GenericHid.HidDeviceManagement.DeviceIdentification>()
        {
            new GenericHid.HidDeviceManagement.DeviceIdentification(){ id=DevList.Senheiser.IdGlobal, vid=DevList.Senheiser.IdVendor, pid=DevList.Senheiser.IdProduct},
            new GenericHid.HidDeviceManagement.DeviceIdentification(){ id=DevList.Plantronic.IdGlobal, vid=DevList.Plantronic.IdVendor, pid=DevList.Plantronic.IdProduct},
        };
        protected List<GenericHid.HidDeviceManagement.DeviceDescription> _dev_desc = null;
        protected List<HidBinauralSndDev> _devs = new List<HidBinauralSndDev>();
        public struct DevData
        {
            public CORESIP_SndDevType TipoDev;
            public string AsioName;
        }
        /// <summary>
        /// 
        /// </summary>
        public int Count
        {
            get
            {
                if (PTTDevice == true)
                    return GenericHid.HidDeviceManagement.DeviceList(_did).Count;

                LoadChannels();
                return _input_channels.Count + _output_channels.Count;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public HidBinauralHwManager(bool bHIDDevice) : base()
        {
            PTTDevice = bHIDDevice;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Init()
        {
            if (PTTDevice == true)
            {
                _dev_desc = GenericHid.HidDeviceManagement.DeviceList(_did);

                _devs.Clear();

                foreach (GenericHid.HidDeviceManagement.DeviceDescription _dd in _dev_desc)
                {
                    HidBinauralSndDev _dev = new HidBinauralSndDev(_dd);
                    if (_dev.IsValid)
                    {
                        _SOSndDvcs.Add(_dev.Type);
                        _devs.Add(_dev);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Start()
        {
            if (PTTDevice == true)
            {
                _Logger.Debug("HidBinauralHwManager.Start. Numero de dispositivos = {0}", _dev_desc.Count);

                foreach (HidBinauralSndDev _dev in _devs)
                {
                    StartDevice(_dev);
                }
            }

            PostStart(PTTDevice);

            /** 20180626. #3609. Se anidan eventos y no se tratan bien en la cola.*/
            HwSupervisor.Enabled = true;
            /** Para Inicializar el estado de JACKS */
            // LALM 210420.1 presencia de altavoz LC
            // Peticiones #4810
            CheckPresencia();

        }

        protected void CheckPresencia()
        {
            // Fuerza la presencia del altavoz radio
            SetPresenceRdSpeaker(true);
            // Fuerza la presencia del altavoz TF/LC
            _Logger.Info("CheckPresencia LcSpeakerSimul={0}", Settings.Default.LcSpeakerSimul);
            //if (Settings.Default.LcSpeakerSimul)
            SetPresenceLcSpeaker(true);
        }

        /// <summary>
        /// Se llama desde HwManager de forma temporizada
        /// </summary>
        /// <returns></returns>
        public override bool CheckDevs()
        {

            // TODO. Replantear la funcion....
            if (PTTDevice == true)
                return true;
            return true;
        }
        public override string SwVersion(int devIndex)
        {
            return "Generic Device, Version not available. ";
        }
        public override string SwDate(int devIndex)
        {
            return " not available ";
        }
        /// <summary>
        /// 
        /// </summary>
        private void CheckHidDevices()
        {
            bool _instructor = false;
            bool _alumno = false;
            foreach (CORESIP_SndDevType tipo in _SOSndDvcs)
            {
                _instructor = tipo == CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP ? true : _instructor;
                _alumno = tipo == CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP ? true : _alumno;
            }

            if (_instructor == false)
                _Logger.Fatal("No se ha encontrado el dispositivo HID del AYUDANTE");
            if (_alumno == false)
                _Logger.Fatal("No se ha encontrado el dispositivo HID del EJECUTIVO");
        }

        #region AUDIO

        /// <summary>
        /// 
        /// </summary>
        static public void LoadChannels()
        {
            int _nchannel = 0;
            _input_channels.Clear();
            _output_channels.Clear();

            AsioChannels.Init();//Aqui obtenemos todos los dispositivos asio, si tenemos 2 dispositivos binaurales y 2 dispositivos en una IAO
            //seran 8 dispositivos
            foreach (String name in AsioChannels.InChannels)
            {
                CORESIP_SndDevType tipo = GetTipoIn(name);
                _input_channels[_nchannel++] = new DevData { TipoDev = tipo, AsioName = name };
                _Logger.Info("Encontrado Canal de Audio. Entrada {0}: {1} = {2}", _nchannel, tipo, name);
            }

            _nchannel = 0;
            foreach (String name in AsioChannels.OutChannels)
            {
                /** Identificar el tipo por el nombre */
                CORESIP_SndDevType tipo = GetTipoOut(name); // _tipos[_index_tipos++];
                _output_channels[_nchannel++] = new DevData { TipoDev = tipo, AsioName = name };
                _Logger.Info("Encontrado Canal de Audio. Salida {0}: {1} = {2} {3}", _nchannel, tipo, name, AsioChannels.SampleRate);
            }
            //RQF20
            //canales de windows

            SetTipoOutWindows(CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP, Settings.Default.CasInstructorId);
            SetTipoOutWindows(CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP, Settings.Default.CasAlumnoId);
            SetTipoOutWindows(CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER, Settings.Default.LcSpkWindowsId);
            SetTipoOutWindows(CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER, Settings.Default.RdSpkWindowsId);
            //SetTipoOutWindows(CORESIP_SndDevType.CORESIP_SND_HF_SPEAKER, Settings.Default.RdSpkWindowsId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_enabled"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        static public int AddDevice(bool _enabled, CORESIP_SndDevType tipo)
        {
            int _device = -1;
            if (_enabled)
            {
                int _channel_in = tipo == CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP || tipo == CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP ? GetInputChannelFor(tipo) : -1;
                int _channel_out = GetOutputChannelFor(tipo);

                _device = SipAgent.AddSndDevice(tipo, _channel_in, _channel_out);
                _Logger.Info("Agregado Dispositivo de sonido ({0}) = {1:X}: IN:{2},OUT:{3}", tipo, _device, _channel_in, _channel_out);
            }
            return _device;
        }

        /// <summary>
        /// 
        /// </summary>
        static Dictionary<int, DevData> _input_channels = new Dictionary<int, DevData>();
        static Dictionary<int, DevData> _output_channels = new Dictionary<int, DevData>();
        static bool PTTDevice { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        static private CORESIP_SndDevType GetTipoIn(string channel)
        {
            if (DevList.IsChannelInput(channel))
            {
                bool first = (AsioChannels.InChannels.Count == 0);
                if (first) return CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP;
                return CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP;
            }

            return CORESIP_SndDevType.CORESIP_SND_UNKNOWN;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        static private CORESIP_SndDevType GetTipoOut(string channel)
        {
            if (DevList.IsChannelOutputCas(channel))
            {
                return CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP;
            }

            if (DevList.IsChannelOutputAlt1(channel))
                return CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER;
            if (DevList.IsChannelOutputAlt2(channel))
                return CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER;

            return CORESIP_SndDevType.CORESIP_SND_UNKNOWN;
        }

        public struct sIAO
        {
            public string name;
            public CORESIP_SndDevType UlisesDev;
            public sIAO(string n, CORESIP_SndDevType c)
            {
                name = n;
                UlisesDev = c;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        /// RQF20 aqui se añadiran todos los tipos de dispositivos.
        static private void SetTipoOutWindows(CORESIP_SndDevType UlisesDev, string namewindows)
        {
            bool EsIAO = false;
            _Logger.Trace("SetTipoOutWindows (si es IAO envio los 8 dipositivos");
            sIAO[] IAO = {
                new sIAO("CWP USB Device # 01 b", CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP),
                new sIAO("CWP USB Device # 02 b", CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP),
                new sIAO("CWP USB Device # 03 b", CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER),
                new sIAO("CWP USB Device # 04 b", CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER),
                // tambien los de grabacion
                new sIAO("CWP USB Device # 01 a", CORESIP_SndDevType.CORESIP_SND_ALUMN_RECORDER),
                new sIAO("CWP USB Device # 02 a", CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_RECORDER),
                new sIAO("CWP USB Device # 03 a", CORESIP_SndDevType.CORESIP_SND_RADIO_RECORDER),
                new sIAO("CWP USB Device # 04 a", CORESIP_SndDevType.CORESIP_SND_LC_RECORDER)
            };
            // de momento hf speaker no hago nada.
            //if (Settings.Default.HfSpeaker)
            //{
            //    int _HfSpeakerDev = SipAgent.AddSndDevice(CORESIP_SndDevType.CORESIP_SND_HF_SPEAKER, -1, Settings.Default.HfSpeakerChannel);
            //    if (_HfSpeakerDev >= 0)
            //    {
            //        sIAO elem_IAO = new sIAO("CWP USB Device # 03 2", CORESIP_SndDevType.CORESIP_SND_HF_SPEAKER);
            //        IAO[IAO.Length] = elem_IAO;
            //    }
            //}
            foreach (sIAO s in IAO) if (namewindows!=null && namewindows!="" && namewindows.Contains(s.name))
            {
                EsIAO = true;
                SipAgent.Asignacion(s.UlisesDev, s.name);
            }
            if (!EsIAO)
                SipAgent.Asignacion(UlisesDev, namewindows);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tipo"></param>
        /// <returns></returns>
        static private int GetInputChannelFor(CORESIP_SndDevType tipo)
        {
            foreach (KeyValuePair<int, DevData> par in _input_channels)
            {
                if (par.Value.TipoDev == tipo)
                    return par.Key;
            }

            _Logger.Fatal("No encontrado AUDIO InputChannel para {0}", tipo);
            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tipo"></param>
        /// <returns></returns>
        static private int GetOutputChannelFor(CORESIP_SndDevType tipo)
        {
            foreach (KeyValuePair<int, DevData> par in _output_channels)
            {
                if (par.Value.TipoDev == tipo)
                    return par.Key;
            }

            _Logger.Fatal("No encontrado AUDIO OutputChannel para {0}", tipo);
            return -1;
        }

        #endregion
    }

}
