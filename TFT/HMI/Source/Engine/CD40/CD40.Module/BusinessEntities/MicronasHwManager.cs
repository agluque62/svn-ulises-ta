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
    /// Implementacion de la Clase para dispositivos MICRONAS...
    /// </summary>
    class MicronasSndDev : ISndDevIO
	{
		#region Uac2 Dll Interface
        /// <summary>
        /// 
        /// </summary>
		const int UAC2_GPI_REG = 0xB0A2;
		const int UAC2_GPO_REG = 0xB0A0;
		const int UAC2_PLAYBACK_VOLUME = 0x10;
		const int UAC2_PLAYBACK_BALANCE = 0x11;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uacHandle"></param>
        /// <param name="addr"></param>
        /// <param name="dataLen"></param>
        /// <param name="data"></param>
        /// <returns></returns>
		[DllImport("uac2", EntryPoint = "_UacSetMem@16", CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int UacSetMem(IntPtr uacHandle, int addr, int dataLen, [In] byte[] data);
		[DllImport("uac2", EntryPoint = "_UacGetMem@16", CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int UacGetMem(IntPtr uacHandle, int addr, int dataLen, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] data);
		[DllImport("uac2", EntryPoint = "_UacGetXDFP@12", CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int UacGetXDFP(IntPtr uacHandle, int addr, out int data);
		[DllImport("uac2", EntryPoint = "_UacSetXDFP@12", CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int UacSetXDFP(IntPtr uacHandle, int addr, int data);

		#endregion

        /// <summary>
        /// 
        /// </summary>
		public event GenericEventHandler<bool> JackConnected;
        public event GenericEventHandler<bool> PttPulsed;

        /** 20180409. Para obtener la version del FIRMWARE*/
        public string SwVersion { get { return "MICRONAS-VER-TODO"; } }
        public string SwDate { get { return "MICRONAS-SN-TODO"; } }

        /// <summary>
        /// 
        /// </summary>
		public CORESIP_SndDevType Type
		{
			get { return _Type; }
            set { }
		}

        /// <summary>
        /// 
        /// </summary>
		public CORESIP_SndDevType SubType
		{
			get { return _SubType; }
			set { _SubType = value; }
		}

        /// <summary>
        /// 
        /// </summary>
		public bool Jack
		{
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
		public bool Ptt
		{
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
			get { return ((_Type != CORESIP_SndDevType.CORESIP_SND_UNKNOWN) && !_DevHandle.IsClosed); }
            set { }
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="usbHandle"></param>
        /// <param name="hidHandle"></param>
        /// <param name="devPath"></param>
		public MicronasSndDev(IntPtr usbHandle, IntPtr hidHandle, string devPath)
		{
			lock (_Sync)
			{
				IntPtr devFileHandle = Native.Kernel32.CreateFile(devPath, Native.Kernel32.GENERIC_READ | Native.Kernel32.GENERIC_WRITE,
					Native.Kernel32.FILE_SHARE_READ | Native.Kernel32.FILE_SHARE_WRITE, IntPtr.Zero, Native.Kernel32.OPEN_EXISTING,
					Native.Kernel32.FILE_FLAG_OVERLAPPED, IntPtr.Zero);

				_UsbHandle = usbHandle;
				_HidHandle = hidHandle;
				_DevHandle = new SafeFileHandle(devFileHandle, true);

				if (!_DevHandle.IsInvalid)
				{
					_Stream = new FileStream(_DevHandle, FileAccess.Read, 3, true);
					_Buffer = new byte[3];

					_Stream.BeginRead(_Buffer, 0, 3, new AsyncCallback(ReadCompleted), null);
				}

				byte[] data = new byte[1];
				if (UacGetMem(_UsbHandle, UAC2_GPI_REG, 1, data) == 1)
				{
					_Type = (CORESIP_SndDevType)((data[0] >> 3) & 0x7);

					Jack = ((data[0] & JACK_FLAG) != 0);
					Ptt = _Jack && ((data[0] & PTT_FLAG) != 0);

					//PublishState();
				}
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /*
		public void PublishState()
		{
			lock (_Sync)
			{
				General.SafeLaunchEvent(JackConnected, this, _Jack);
				General.SafeLaunchEvent(PttPulsed, this, _Ptt);
			}
		}
        */
        /// <summary>
        /// 
        /// </summary>
		public void Close()
		{
			_DevHandle.Close();
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="OnOff"></param>
        public void LedAltavoz(bool OnOff)
        {
            // public const int OUT_GRABACION = 7;
            // public const int OUT_ALTAVOZ = 6;
            SetGpio(6, (byte)(OnOff ? 1 : 0));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="OnOff"></param>
        public void SenGrabacion(bool OnOff)
        {
            // public const int OUT_GRABACION = 7;
            // public const int OUT_ALTAVOZ = 6;
            SetGpio(7, (byte)(OnOff ? 1 : 0));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
		protected bool SetOutVolume(int level)
		{
			Debug.Assert((level >= 0) && (level <= 7));

			int volume = 0x79 - ((7 - level) * 3);

			if (_Type == CORESIP_SndDevType.CORESIP_SND_MAIN_SPEAKERS)
			{
				int actualVolume, actualBalance;

				if ((UacGetXDFP(_UsbHandle, UAC2_PLAYBACK_VOLUME, out actualVolume) == 4) &&
					(UacGetXDFP(_UsbHandle, UAC2_PLAYBACK_BALANCE, out actualBalance) == 4))
				{
					actualVolume >>= 10;
					actualBalance >>= 10;

					sbyte balance = (sbyte)actualBalance;
					int leftVolume = (balance > 0) ? actualVolume - balance : actualVolume;
					int rightVolume = (balance < 0) ? actualVolume + balance : actualVolume;

					int channel = (_SubType == CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER ?
						Settings.Default.RdSpeakerOutChannel : Settings.Default.LcSpeakerOutChannel) % 2;

					int desiredLeftVolume = channel == 0 ? volume : leftVolume;
					int desiredRightVolume = channel == 1 ? volume : rightVolume;

					int newVolume = Math.Max(desiredLeftVolume, desiredRightVolume);
					int newBalance = (newVolume > desiredLeftVolume ? newVolume - desiredLeftVolume : desiredRightVolume - newVolume);

					newVolume = Math.Max(0, newVolume);
					newVolume = Math.Min(0x79, newVolume);
					newBalance = Math.Max(-21, newBalance);
					newBalance = Math.Min(21, newBalance);

					if ((UacSetXDFP(_UsbHandle, UAC2_PLAYBACK_VOLUME, newVolume << 10) == 4) &&
						(UacSetXDFP(_UsbHandle, UAC2_PLAYBACK_BALANCE, ((byte)newBalance) << 10) == 4))
					{
						return true;
					}
				}
			}
			else
			{
				if (UacSetXDFP(_UsbHandle, UAC2_PLAYBACK_VOLUME, volume << 10) == 4)
				{
					return true;
				}
			}

			return false;
		}

        /// <summary>
        /// Mover Líneas en el USB
        /// </summary>
        /// <param name="gpio"></param>
        /// <param name="estado"></param>
        /// <returns></returns>
		protected int SetGpio(int gpio, byte estado)
		{
			byte mask = gpio == 6 ? (byte)0x40 : (byte)0x80;
			byte[] datos = new byte[1];

			// Recuperar estado actual
			datos[0] = GetGpio();

			datos[0] = (estado == 0 ? (byte)(datos[0] & ((byte)(~mask))) : (byte)(datos[0] | mask));
			return UacSetMem(_UsbHandle, UAC2_GPO_REG, 1, datos);
		}

        /// <summary>
        /// Leer Líneas en el USB.
        /// </summary>
        /// <returns></returns>
		protected byte GetGpio()
		{
			byte[] datos = new byte[1];
			if (UacGetMem(_UsbHandle, UAC2_GPI_REG, 1, datos) == 1)
			{
				_gpio6 = (datos[0] & 0x40) == 0x40;
				_gpio7 = (datos[0] & 0x80) == 0x80;

				return datos[0];
			}

			return 0;
		}

		#region Private Members
        /// <summary>
        /// 
        /// </summary>
		private const int JACK_FLAG = 0x01;
		private const int AUC_FLAG = 0x02;
		private const int PTT_FLAG = 0x04;
        /// <summary>
        /// 
        /// </summary>
		private static Logger _Logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// 
        /// </summary>
		private IntPtr _UsbHandle;
		private IntPtr _HidHandle;
		private SafeFileHandle _DevHandle;
		private CORESIP_SndDevType _Type = CORESIP_SndDevType.CORESIP_SND_UNKNOWN;
		private CORESIP_SndDevType _SubType = CORESIP_SndDevType.CORESIP_SND_UNKNOWN;
		private FileStream _Stream;
		private byte[] _Buffer;
		private object _Sync = new object();
		private bool _Jack = false;
		private bool _Ptt = false;
		private bool _gpio6, _gpio7;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="iResult"></param>
		private void ReadCompleted(IAsyncResult iResult)
		{
			byte[] arrBuff = (byte[])iResult.AsyncState;	// retrieve the read buffer
			try
			{
				_Stream.EndRead(iResult);
				_Logger.Debug("Recibido cambio de estado en dispositivo {0}: 0x{1:X}", _Type, _Buffer[1]);

				lock (_Sync)
				{
					Jack = ((_Buffer[1] & JACK_FLAG) == 0);
					Ptt = _Jack && ((_Buffer[1] & PTT_FLAG) == 0);

					//PublishState();
				}

				_Stream.BeginRead(_Buffer, 0, 3, new AsyncCallback(ReadCompleted), null);
			}
			catch (IOException )
			{
				if (!_DevHandle.IsClosed)
				{
					Jack = false;
					Ptt = false;

					_DevHandle.Close();
				}
			}
			catch (Exception)
			{
				if (!_DevHandle.IsClosed)
				{
					Jack = false;
					Ptt = false;

					_DevHandle.Close();
				}
			}
		}

		#endregion
	}


    /// <summary>
    /// implementacion de la Clase para MICRONAS
    /// </summary>
    class MicronasHwManager : HwManager
    {
		#region Uac2 Dll Interface
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uFlags"></param>
        /// <returns></returns>
		[DllImport("uac2", EntryPoint = "_UacBuildDeviceList@4", CharSet = CharSet.Ansi, ExactSpelling = true)]
		static protected extern int UacBuildDeviceList(uint uFlags);
		[DllImport("uac2", EntryPoint = "_UacGetFirstDevice@0", CharSet = CharSet.Ansi, ExactSpelling = true)]
        static protected extern IntPtr UacGetFirstDevice();
		[DllImport("uac2", EntryPoint = "_UacGetNextDevice@4", CharSet = CharSet.Ansi, ExactSpelling = true)]
        static protected extern IntPtr UacGetNextDevice(IntPtr uacHandle);
		[DllImport("uac2", EntryPoint = "_UacGetHidDevice@4", CharSet = CharSet.Ansi, ExactSpelling = true)]
        static protected extern IntPtr UacGetHidDevice(IntPtr uacHandle);
		[DllImport("uac2", EntryPoint = "_UacGetInstanceID_A@12", CharSet = CharSet.Ansi, ExactSpelling = true)]
        static protected extern int UacGetInstanceID(IntPtr uacHandle, StringBuilder id, int size);
		[DllImport("uac2", EntryPoint = "_UacGetHardwareID_A@12", CharSet = CharSet.Ansi, ExactSpelling = true)]
        static protected extern int UacGetHardwareID(IntPtr uacHandle, StringBuilder id, int size);
		[DllImport("uac2", EntryPoint = "_UacGetDirectShowDeviceName_A@12", CharSet = CharSet.Ansi, ExactSpelling = true)]
        static protected extern int UacGetDirectShowDeviceName(IntPtr uacHandle, StringBuilder id, int size);
		
		#endregion

		#region Hid Dll Interface
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hidGuid"></param>
		[DllImport("hid.dll", SetLastError = true)]
		static protected extern void HidD_GetHidGuid(ref Guid hidGuid);

		#endregion

        /// <summary>
        /// 
        /// </summary>
        protected List<GenericHid.HidDeviceManagement.DeviceIdentification> _did = new List<GenericHid.HidDeviceManagement.DeviceIdentification>()
        {
            new GenericHid.HidDeviceManagement.DeviceIdentification(){ id="Micronas", vid="VID_074D", pid="PID_3576"}
        };

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
        public MicronasHwManager()
            : base()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Init()
        {
            /* Lista de dispositivos ordenada según los va suministrando el SO */

            Guid hidGuid = new Guid();
            HidD_GetHidGuid(ref hidGuid);

            UacBuildDeviceList(0);

            for (IntPtr usbHandle = UacGetFirstDevice(); usbHandle != IntPtr.Zero; usbHandle = UacGetNextDevice(usbHandle))
            {
                IntPtr hidHandle = UacGetHidDevice(usbHandle);
                if (hidHandle != IntPtr.Zero)
                {
                    /*********************************************/
                    //StringBuilder name = new StringBuilder(512);
                    //UacGetHardwareID(hidHandle, name, 512);

                    //UacGetDirectShowDeviceName(hidHandle, name, 512);
                    /*********************************************/

                    StringBuilder id = new StringBuilder(512);

                    if (UacGetInstanceID(hidHandle, id, 512) != 0)
                    {
                        id.Replace('\\', '#');

                        string devPath = string.Format("\\\\?\\{0}#{{{1}}}", id.ToString().ToLower(), hidGuid);
                        MicronasSndDev sndDev = new MicronasSndDev(usbHandle, hidHandle, devPath);

                        if (sndDev.IsValid)
                        {
                            _SOSndDvcs.Add(sndDev.Type);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Start()
        {
            Guid hidGuid = new Guid();
            HidD_GetHidGuid(ref hidGuid);

            UacBuildDeviceList(0);

            for (IntPtr usbHandle = UacGetFirstDevice(); usbHandle != IntPtr.Zero; usbHandle = UacGetNextDevice(usbHandle))
            {
                IntPtr hidHandle = UacGetHidDevice(usbHandle);
                if (hidHandle != IntPtr.Zero)
                {
                    StringBuilder id = new StringBuilder(512);

                    if (UacGetInstanceID(hidHandle, id, 512) != 0)
                    {
                        id.Replace('\\', '#');

                        string devPath = string.Format("\\\\?\\{0}#{{{1}}}", id.ToString().ToLower(), hidGuid);
                        MicronasSndDev sndDev = new MicronasSndDev(usbHandle, hidHandle, devPath);

                        if (sndDev.IsValid)
                        {
                            StartDevice(sndDev);
                        }

                    }
                }
            }

            PostStart(true);
            /** 20180626. #3609. Se anidan eventos y no se tratan bien en la cola. */
            HwSupervisor.Enabled = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool CheckDevs()
        {
            return _SndDevs.Values.Count == 4;
        }

    }
}
