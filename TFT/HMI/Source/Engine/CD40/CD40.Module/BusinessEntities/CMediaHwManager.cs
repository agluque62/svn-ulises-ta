using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Net;
using System.Timers;

using Microsoft.Win32.SafeHandles;

using HMI.CD40.Module.Properties;
using HMI.Model.Module.Messages;
using HMI.CD40.Module.Snmp;

using U5ki.Infrastructure;
using Utilities;

using NLog;
//using Lextm.SharpSnmpLib;
//using Lextm.SharpSnmpLib.Messaging;

namespace HMI.CD40.Module.BusinessEntities
{
    /// <summary>
    /// 
    /// </summary>
    enum CMediaDevMode { Output = 0, Input = 1, Bidirectional = 2 };

    /// <summary>
    /// Implementacion de la Clase para dispositivos CMEDIA
    /// </summary>
    class HidCMediaSndDev : ISndDevIO
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
        public bool Jack
        {
            get { return _Jack; }
            set
            {
                if (value != _Jack)
                {
                    _Logger.Debug("HidCMediaSndDev. JACK <{0}> => {1}", Type, value);
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
                    _Logger.Debug("HidCMediaSndDev. PTT  <{0}> => {1}", Type, value);
                    _Ptt = value;
#if DEBUG_TIME
                    if (Top.Registry.Channel.timeMeasure== null)
                        Top.Registry.Channel.timeMeasure = new TimeMeasurement("Ptt");
#endif
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
            _Logger.Trace("SenGrabacion");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        public void SetMultiplexor(bool onoff)
        {
            if (/*Type == CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP || */Type == CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP)
            {
                SetGpio(4, (byte)(onoff ? 0 : 1));
                SetGpio(5, (byte)(onoff ? 1 : 0));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public bool GetPresencia(int pos)
        {
            // Solo son significativos los 2 primeros bits.
            byte leido = (byte)(GetGpio() & (byte)0x03);
            _Logger.Trace("HID-REPORT {1}: {0:X}", leido, Type);
            bool pos1 = (leido & (byte)0x01) == 1 ? true : false;
            bool pos2 = (leido & (byte)0x02) == 2 ? true : false;
            pos &= 0x01;
            return pos == 1 ? pos2 : pos1;
        }

        /**  */
        public int SetGpio(int gpio, byte estado)
        {
            /** Leo Registro C0 */
            byte[] cmd = new byte[] { 0x01, 0x00, 0xFE, 0x08, 0x00, 0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            GenericHid.Hid.HidD_SetOutputReport(hidHandle, ref cmd[0], MyHid.Capabilities.OutputReportByteLength);

            cmd[0] = 1;
            if (GenericHid.Hid.HidD_GetInputReport(hidHandle, ref cmd[0], MyHid.Capabilities.InputReportByteLength) == true)
            {
                byte[] _set = new byte[] { 0x01, 0x00, 0xC0, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                int bit = 0x0001 << gpio;
                int val = estado == 0 ? cmd[6] & ~bit : cmd[6] | bit;
                _set[4] = (byte)val;

                GenericHid.Hid.HidD_SetOutputReport(hidHandle, ref _set[0], MyHid.Capabilities.OutputReportByteLength);
                _Logger.Trace("ESCRITO HID-REPORT: {0}", BitConverter.ToString(_set));
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// GPI0 -- GPI7.
        /// </summary>
        /// <returns></returns>
        protected byte GetGpio()
        {
            /** Marco el Registro a Leer */
            byte[] cmd = new byte[] { 0x01, 0x00, 0xFE, 0x08, 0x00, 0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            GenericHid.Hid.HidD_SetOutputReport(hidHandle, ref cmd[0], MyHid.Capabilities.OutputReportByteLength);

            cmd[0] = 1;
            if (GenericHid.Hid.HidD_GetInputReport(hidHandle, ref cmd[0], MyHid.Capabilities.InputReportByteLength) == true)
            {
                _Logger.Trace("HID-REPORT {1}: {0}", BitConverter.ToString(cmd), Type);
                /** 
                 * 8.0 --> R.0      GPI-00
                 * 8.1 --> R.1      GPI-01
                 * 9.2 --> R.2      TIPO-0
                 * 8.3 --> R.3      TIPO-1
                 * 8.2 --> R.4      TIPO-2
                 */
                int lsb0 = (cmd[8] & 0x03);
                int lsb1 = (cmd[8] & 0x04) << 2;
                int lsb2 = (cmd[8] & 0x08);
                int msb2 = (cmd[9] & 0x04);

                int ret = lsb0 | lsb1 | lsb2 | msb2;
                return (byte)ret;
            }
            return 0;
        }

        /** 20180409. Para obtener la version del FIRMWARE*/
        public string SwVersion { get; set; }
        public string SwDate { get; set; }
        #endregion

        #region CONSTRUCTOR
        private static int _g_instance = 0;
        private int _instance = _g_instance++;

        /// <summary>
        /// 
        /// </summary>
        public HidCMediaSndDev()
        {
            // Para pruebas...
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dev"></param>
        public HidCMediaSndDev(GenericHid.HidDeviceManagement.DeviceDescription dev)
        {
            try
            {
                Type = CORESIP_SndDevType.CORESIP_SND_UNKNOWN;      // Por si falla algo...
                _dev = dev;

                Open();

                /** Arrancar el Lazo de Escucha sólo para jacks */
                if ((Type == CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP) ||( Type == CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP))
                        StartRead();
                Error = false;
            }
            catch (Exception x)
            {
                /** TODO Gestion de la Excepcion */
                LogManager.GetCurrentClassLogger().Warn("HidCMediaSndDev", x);
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
                _Logger.Warn("RESET", x);
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
        private void Open()
        {
            Reading = false;
            hidHandle = GenericHid.FileIO.CreateFile(_dev.path, 0,
                GenericHid.FileIO.FILE_SHARE_READ | GenericHid.FileIO.FILE_SHARE_WRITE, null, GenericHid.FileIO.OPEN_EXISTING, 0, 0);

            MyHid.DeviceAttributes.Size = Marshal.SizeOf(MyHid.DeviceAttributes);
            GenericHid.Hid.HidD_GetAttributes(hidHandle, ref MyHid.DeviceAttributes);
            MyHid.GetDeviceCapabilities(hidHandle);

            /** 20180409. Para obtener la version del FIRMWARE*/
            SwVersion = String.Format("{0:X}",MyHid.DeviceAttributes.VersionNumber);
            SwDate = GenericHid.Hid.GetSerialNumberString(hidHandle);

            /** Leer el Hardware */
            byte _initial_report = GetGpio();

            /** Leer el Tipo en el HW */
            Type = HidType(_initial_report);

            /** Leer JACK en e HW */
            // Jack = HidJack(_initial_report);

            /** Leer PTT en el HW */
            //Es muy pronto para leer, no llegan los eventos a destino */
            //Ptt = HidPtt(_initial_report);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iResult"></param>
        private void ReadCompleted(IAsyncResult iResult)
        {
            try
            {
                if (_Stream.EndRead(iResult) == 16)
                {
                    /** Leer JACK en e HW. En el Evento viene con lógica cambiada */
                    // Jack = HidJack((byte)(~_Buffer[1]));

                    /** Leer PTT en el HW En el Evento viene con lógica cambiada */
                    Ptt = HidPtt((byte)(~_Buffer[1]));

                    _Stream.BeginRead(_Buffer, 0, 16, new AsyncCallback(ReadCompleted), null);

                }
                else
                    _Logger.Error("HID Callback...");
            }
            catch (System.OperationCanceledException)
            {
            }
            catch (Exception x)
            {
                _Logger.Warn("ReadCompleted", x);
                // _Stream.BeginRead(_Buffer, 0, 16, new AsyncCallback(ReadCompleted), null);
                Error = true;
            }
        }

        /// <summary>
        /// Ajecutivo / Alumno. BIT0
        /// Ayudante / Instructor. BIT1
        /// </summary>
        /// <param name="_data"></param>
        /// <returns></returns>
        private bool HidPtt(byte _data)
        {
            int bptt = 0;
            if (Settings.Default.CMediaBkpVersion == "B41A")
            {
                // EN BKP-IAU-B41A
                bptt = Type == CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP ? _data & (byte)0x01 : _data & (byte)0x02;
            }
            else
            {
                // En BKP-IAU-B43A
                bptt = _data & (byte)0x01;
            }
            _Logger.Trace("HidCMediaSndDev.HidPtt ({0})-->({1}))", _data, bptt);
            return bptt != 0 ? true : false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_data"></param>
        /// <returns></returns>
        private bool HidJack_nousar(byte _data)
        {
            int bjack = _data & (byte)0x01;

            _Logger.Trace("HidCMediaSndDev.HidJack ({0})-->({1}))", _data, bjack);
            return bjack != 0 ? true : false;
        }

        /// <summary>
        /// BIT 0/1/2. 
        /// </summary>
        /// <param name="_data"></param>
        /// <returns></returns>
        private CORESIP_SndDevType HidType(byte _data)
        {
            int tipo = (_data >> 2) & 0x03; // (_data & 0x0C) >> 2;
            CORESIP_SndDevType _tipo = tipo == 0 ? CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP :
                tipo == 1 ? CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP :
                tipo == 2 ? CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER :
                tipo == 3 ? CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER : CORESIP_SndDevType.CORESIP_SND_UNKNOWN;

            _Logger.Debug("HidCMediaSndDev.HidType ({0})-->({1})-->({2})", _data, tipo, _tipo);
            return _tipo;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_data"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        private byte[] GetReport()
        {
            byte[] _data = new byte[MyHid.Capabilities.InputReportByteLength];
            _data[0] = 1;

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
            _report.CopyTo(_data, 0);
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
    /// Implementacion de la Clase para CMEDIA
    /// </summary>
    class HidCMediaHwManager : HwManager
    {
        protected enum LastStateGpio
        {
            OFF = 0,
            ON = 1, 
            INIT = 2
        }

        /// <summary>
        /// 
        /// </summary>
        protected List<GenericHid.HidDeviceManagement.DeviceIdentification> _did = new List<GenericHid.HidDeviceManagement.DeviceIdentification>()
        {
            new GenericHid.HidDeviceManagement.DeviceIdentification(){ id="C-Media-Demo", vid="VID_0D8C", pid="PID_017C"},
            new GenericHid.HidDeviceManagement.DeviceIdentification(){ id="C-Media-basic-1", vid="VID_0D8C", pid="PID_01A4"},
            new GenericHid.HidDeviceManagement.DeviceIdentification(){ id="C-Media-basic-2", vid="VID_0D8C", pid="PID_0174"},
            new GenericHid.HidDeviceManagement.DeviceIdentification(){ id="C-Media-nucleo-hs", vid="VID_0D8C", pid="PID_01C1"},
            new GenericHid.HidDeviceManagement.DeviceIdentification(){ id="C-Media-nucleo-hs", vid="VID_0D8C", pid="PID_0178"}
        };
        protected List<GenericHid.HidDeviceManagement.DeviceDescription> _dev_desc = null;

        HidCMediaSndDev _dev1_eje = null, _dev2_ayu = null;
        HidCMediaSndDev _dev3_alr = null, _dev4_alt = null;
        Dictionary<CORESIP_SndDevType, LastStateGpio> ledState = new Dictionary<CORESIP_SndDevType, LastStateGpio>();

        /// <summary>
        /// 
        /// </summary>
        public HidCMediaHwManager()
            : base()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Init()
        {
            base.Init();

            _dev1_eje = _dev2_ayu = null;
            _dev3_alr = _dev4_alt = null;

            ///** 20180626. #3609. En los RESET da una excepcion y aborta el arranque subsiquiente, con lo cual deja
            ///de funcionar hasta que se resetea a mano. */
            //ledState.Add(CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER, LastStateGpio.INIT);
            //ledState.Add(CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER, LastStateGpio.INIT);
            //ledState.Add(CORESIP_SndDevType.CORESIP_SND_HF_SPEAKER, LastStateGpio.INIT);
            ledState[CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER] = LastStateGpio.INIT;
            ledState[CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER] = LastStateGpio.INIT;
            ledState[CORESIP_SndDevType.CORESIP_SND_HF_SPEAKER] = LastStateGpio.INIT;
            /*******************************************************************************/

            _SOSndDvcs.Clear();

            _dev_desc = GenericHid.HidDeviceManagement.DeviceList(_did);
            foreach (GenericHid.HidDeviceManagement.DeviceDescription _dd in _dev_desc)
            {
                HidCMediaSndDev dev = new HidCMediaSndDev(_dd);
                if (dev.IsValid)
                {
                    _SOSndDvcs.Add(dev.Type);
                    switch (dev.Type)
                    {
                        case CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP:
                        _dev1_eje = dev;
                        break;
                        case CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP:
                        _dev2_ayu = dev;
                        break;
                        case CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER:
                        _dev3_alr = dev;
                        break;
                        case CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER:
                        _dev4_alt = dev;
                        break;
                        default:
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Start()
        {
            base.Start();

            _Logger.Debug("HidCMediaHwManager.Start. Numero de dispositivos = {0}", _dev_desc.Count);

            HwSupervisor.Enabled = true;

            ///** 20180626. #3609. */
            if (_dev1_eje != null) StartDevice(_dev1_eje);
            if (_dev2_ayu != null) StartDevice(_dev2_ayu);
            if (_dev3_alr != null) StartDevice(_dev3_alr);
            if (_dev4_alt != null) StartDevice(_dev4_alt);
            PostStart(true);

            /** Para Inicializar el estado de JACKS */
            CheckPresencia();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool CheckDevs()
        {
            CheckPresencia();

            return _SndDevs.Values.Count == 4;
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

        /// <summary>
        /// 
        /// </summary>
        public int Count
        {
            get
            {
                return GenericHid.HidDeviceManagement.DeviceList(_did).Count;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="ON"></param>
        public override void EnciendeLed(CORESIP_SndDevType Type, byte ON)
        {
            switch (Type)
            {
                case CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER:
                    if ((_dev4_alt != null) && (ledState[Type] != (LastStateGpio)ON))
                    {
                        _dev4_alt.SetGpio(4, ON);
                        ledState[Type] = (LastStateGpio)ON;
                    }
                    break;
                case CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER:
                    if ((_dev3_alr != null) && (ledState[Type] != (LastStateGpio)ON))
                    {
                        _dev3_alr.SetGpio(4, ON);
                        ledState[Type] = (LastStateGpio)ON;
                    }
                    break;
                case CORESIP_SndDevType.CORESIP_SND_HF_SPEAKER:
                    if ((_dev3_alr != null)&& (ledState[Type] != (LastStateGpio)ON))
                    {
                        _dev3_alr.SetGpio(5, ON);
                        ledState[Type] = (LastStateGpio)ON;
                    }
                    break;
                default:
                    _Logger.Error("HidCMediaHwManager.EnciendeLed. Tipo Altavoz No Soportado: {0}", Type);
                    break;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public override void ApagarSndSpeaker()
        {
            EnciendeLed(CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER, 0);
            EnciendeLed(CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER, 0);
            EnciendeLed(CORESIP_SndDevType.CORESIP_SND_HF_SPEAKER, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        protected void CheckPresencia()
        {
            if (_dev1_eje != null)
            {
                /** Escribo un CERO en el control del MTX 
                    El multiplexor se ha eliminado */
                // _dev1_eje.SetMultiplexor(true);

                if (_dev3_alr != null)
                {
                    /** Leo las Presencias de JACKS */
                    if (Settings.Default.CMediaBkpVersion == "B41A")
                    {
                        // EN BKP-IAU-B41A
                        _dev1_eje.Jack = _dev3_alr.GetPresencia(0);          // Estoy leyendo GPI00
                        if (_dev2_ayu != null)
                            _dev2_ayu.Jack = _dev3_alr.GetPresencia(1);      // Estoy leyendo GPI01
                    }
                    else
                    {
                        // EN BKP-IAU-B43A
                        _dev1_eje.Jack = _dev1_eje.GetPresencia(1);            // Estoy leyendo GPI01
                        if (_dev2_ayu != null)
                            _dev2_ayu.Jack = _dev2_ayu.GetPresencia(1);        // Estoy leyendo GPI01
                    }
                }

                if (_dev4_alt != null)
                {
                    /** Leo Presencias de Altavoz. */
                    if (Settings.Default.CMediaBkpVersion == "B41A")
                    {
                        // EN BKP-IAU-B41A
                        if (_dev3_alr != null)
                        {
                            _dev3_alr.Jack = _dev4_alt.GetPresencia(0);     // Presencia de Altavoz Radio. Estoy Leyendo GPI00
                            _dev3_alr.Ptt = _dev2_ayu.GetPresencia(0);      // Presencia de Altavoz Radio HF.
                        }
                        _dev4_alt.Jack = _dev4_alt.GetPresencia(1);         // Presencia de Altavoz LC.
                        /** Presencia Cable Grabacion */
                        _dev4_alt.Ptt = _dev1_eje.GetPresencia(1);          // Presencia Cable Grabacion. 
                    }
                    else
                    {
                        // EN BKP-IAU-B43A
                        if (_dev3_alr != null)
                        {
                            _dev3_alr.Jack = _dev3_alr.GetPresencia(0);     // Presencia de Altavoz Radio. Estoy Leyendo GPI00
                            _dev3_alr.Ptt = _dev3_alr.GetPresencia(1);      // Presencia de Altavoz Radio HF.
                        }
                        _dev4_alt.Jack = _dev4_alt.GetPresencia(0);         // Presencia de Altavoz LC.
                    /** Presencia Cable Grabacion */
                    _dev4_alt.Ptt = _dev4_alt.GetPresencia(1);          // Presencia Cable Grabacion. 
                    }
                }
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
            int _nchannel = 0;

            _input_channels.Clear();
            _output_channels.Clear();

            AsioChannels.Init();
            foreach (String name in AsioChannels.InChannels)
            {
                /** Identificar el tipo por el nombre */
                CORESIP_SndDevType tipo = GetTipoIn(name);
                _input_channels[tipo] = _nchannel++;
                _Logger.Debug("Encontrado Canal de Audio. Entrada {0}: {1} = {2}", _nchannel, tipo, name);
            }

            _nchannel = 0;
            foreach (String name in AsioChannels.OutChannels)
            {
                /** Identificar el tipo por el nombre */
                CORESIP_SndDevType tipo = GetTipoOut(name); // _tipos[_index_tipos++];
                _output_channels[tipo] = _nchannel++;
                _Logger.Debug("Encontrado Canal de Audio. Salida {0}: {1} = {2}", _nchannel, tipo, name);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        static private CORESIP_SndDevType GetTipoIn(string channel)
        {
            if (channel.Contains("# 01 1"))
                return CORESIP_SndDevType.CORESIP_SND_ALUMN_RECORDER;
            if (channel.Contains("# 01 2"))
                return CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP;

            if (channel.Contains("# 02 1"))
                return CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_RECORDER;
            if (channel.Contains("# 02 2"))
                return CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP;

            if (channel.Contains("# 03 1"))
                return CORESIP_SndDevType.CORESIP_SND_RADIO_RECORDER;
            if (channel.Contains("# 03 2"))
                return CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER;

            if (channel.Contains("# 04 1"))
                return CORESIP_SndDevType.CORESIP_SND_LC_RECORDER;
            if (channel.Contains("# 04 2"))
                return CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER;

            return CORESIP_SndDevType.CORESIP_SND_UNKNOWN;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        static private CORESIP_SndDevType GetTipoOut(string channel)
        {
            if (channel.Contains("# 01 1"))
                return CORESIP_SndDevType.CORESIP_SND_ALUMN_RECORDER;
            if (channel.Contains("# 01 2"))
                return CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP;

            if (channel.Contains("# 02 1"))
                return CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_RECORDER;
            if (channel.Contains("# 02 2"))
                return CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP;

            if (channel.Contains("# 03 1"))
                return CORESIP_SndDevType.CORESIP_SND_RADIO_RECORDER;
            if (channel.Contains("# 03 2"))
                return CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER;

            if (channel.Contains("# 04 1"))
                return CORESIP_SndDevType.CORESIP_SND_LC_RECORDER;
            if (channel.Contains("# 04 2"))
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
            if (_input_channels.ContainsKey(tipo))
                return _input_channels[tipo];
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
            if (_output_channels.ContainsKey(tipo))
                return _output_channels[tipo];

            _Logger.Fatal("No encontrado AUDIO OutputChannel para {0}", tipo);
            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_enabled"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        static public int AddDevice(bool _enabled, CORESIP_SndDevType tipo, CMediaDevMode mode)
        {
            int _device = -1;
            if (_enabled)
            {
                int _channel_in = mode != CMediaDevMode.Output ? GetInputChannelFor(tipo) : -1;
                int _channel_out = mode != CMediaDevMode.Input ? GetOutputChannelFor(tipo) : -1;

                _device = SipAgent.AddSndDevice(tipo, _channel_in, _channel_out);
                _Logger.Debug("Agregado Dispositivo de sonido ({0}) = {1:X}: IN:{2},OUT:{3}", tipo, _device, _channel_in, _channel_out);
            }
            return _device;
        }

        #endregion
    }
}
