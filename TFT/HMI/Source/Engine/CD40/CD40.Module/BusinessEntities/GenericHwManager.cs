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
    /// 
    /// </summary>
    class DevDescr
    {
        public string IdVendor { get; set; }
        public string IdProduct { get; set; }
        public string IdGlobal { get; set; }
        public string IdInputMic { get; set; }
        public string IdOutputCas { get; set; }
        public string IdOutputAlt1 { get; set; }
        public string IdOutputAlt2 { get; set; }
        public int PttByte { get; set; }
        public int PttBit { get; set; }
    }

    class DevList
    {
        /// <summary>
        /// Casco Plantronic.
        /// </summary>
        public static DevDescr Plantronic = new DevDescr()
        {
            IdGlobal = "Plantronic",
            IdVendor = "VID_047F",
            IdProduct = "PID_FAA1",
            IdInputMic = "PTT Headset Adapter 1",
            IdOutputCas = "PTT Headset Adapter 1",
            IdOutputAlt1 = "NoUtilizado",
            IdOutputAlt2 = "NoUtilizado",
            PttByte = 1,
            PttBit = 1
        };
        /// <summary>
        /// Casco Senheiser
        /// </summary>
        public static DevDescr Senheiser = new DevDescr()
        {
            IdGlobal = "Senheiser",
            IdVendor = "VID_0D8C",
            IdProduct = "PID_0175",
            IdInputMic = "USA PTT Headset Adapter 1",
            IdOutputCas = "USA PTT Headset Adapter 1",
            IdOutputAlt1 = "NoUtilizado",
            IdOutputAlt2 = "NoUtilizado",
            PttByte = 5,
            PttBit = 32
        };
        /// <summary>
        /// Cascos Genericos sin PTT
        /// </summary>
        public static DevDescr Microcascos = new DevDescr()
        {
            IdGlobal = "Microcascos",
            IdVendor = "",
            IdProduct = "",
            IdInputMic =Settings.Default.MicAsioId,
            IdOutputCas = Settings.Default.CasAsioId,
            IdOutputAlt1 = "NoUtilizado",
            IdOutputAlt2 = "NoUtilizado",
            PttByte = 0,
            PttBit = 0
        };
        /// <summary>
        /// Altavoces.
        /// </summary>
        public static DevDescr Altavoces = new DevDescr()
        {
            IdGlobal = "Altavoces",
            IdVendor = "",
            IdProduct = "",
            IdInputMic = "NoUtilizado",
            IdOutputCas = "NoUtilizado",
            IdOutputAlt1 = Settings.Default.RdSpkAsioId,   // "HD Audio output 1",
            IdOutputAlt2 = Settings.Default.LcSpkAsioId,   // "HD Audio output 2",
            PttByte = 0,
            PttBit = 0
        };

        /// <summary>
        /// 
        /// </summary>
        static List<DevDescr> _devs = new List<DevDescr>() { Plantronic, Senheiser, Altavoces, Microcascos };
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="Byte"></param>
        /// <param name="Bit"></param>
        /// <returns></returns>
        public static bool ByteBit(string id, ref int Byte, ref int Bit)
        {
            foreach (DevDescr dev in _devs)
            {
                if (dev.IdGlobal == id)
                {
                    Byte = dev.PttByte;
                    Bit = dev.PttBit;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool IsChannelInput(string name)
        {
            foreach (DevDescr dev in _devs)
            {
                if (name.Contains(dev.IdInputMic))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool IsChannelOutputCas(string name)
        {
            foreach (DevDescr dev in _devs)
            {
                if (name.Contains(dev.IdOutputCas))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool IsChannelOutputAlt1(string name)
        {
            foreach (DevDescr dev in _devs)
            {
                if (name.Contains(dev.IdOutputAlt1))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool IsChannelOutputAlt2(string name)
        {
            foreach (DevDescr dev in _devs)
            {
                if (name.Contains(dev.IdOutputAlt2))
                    return true;
            }
            return false;
        }
    }


    /// <summary>
    /// Implementacion de la Clase para dispositivos Genericos
    /// </summary>
    class HidGenericSndDev : ISndDevIO
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
        public HidGenericSndDev(GenericHid.HidDeviceManagement.DeviceDescription dev)
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
                _Logger.Fatal("HidGenericSndDev", x);
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

            _Logger.Debug("HidGenericSndDev.HidPtt ({0})-->({1}))", _data, bptt);
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
    class HidGenericHwManager : HwManager
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
        protected List<HidGenericSndDev> _devs = new List<HidGenericSndDev>();
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
        public HidGenericHwManager(bool bHIDDevice) : base ()
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
                    HidGenericSndDev _dev = new HidGenericSndDev(_dd);
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
                _Logger.Debug("HidGenericHwManager.Start. Numero de dispositivos = {0}", _dev_desc.Count);

                foreach (HidGenericSndDev _dev in _devs)
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

            AsioChannels.Init();
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
                return CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP;
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
