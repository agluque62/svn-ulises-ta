using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Practices.CompositeUI.EventBroker;
using HMI.Model.Module.Constants;
using HMI.Model.Module.Properties;
using Utilities;
using NLog;

namespace HMI.Model.Module.BusinessEntities
{
	public sealed class Brightness
	{
		#region Dll Interface

		const int Adj_Backlight = 15;
		const int ADJUSTER_CMD = 0;
		const int BRIGHT_ID = 4;
		const int BACKLIGHT_ID = 5;
		const int CONTRAST_ID = 6;

      [StructLayout(LayoutKind.Sequential)]
		struct BrightnessValues
		{
			public uint Min;
			public uint Max;
			public uint Actual;

			public BrightnessValues(uint min, uint max, uint actual)
			{
				Min = min;
				Max = max;
				Actual = actual;
			}
		}

		[DllImport("crt2lcd1", EntryPoint = "OpenSerialCommunication")]
		static extern int OpenSerialCommunicationVer0(ushort serialPort, uint serialBaud);
		[DllImport("crt2lcd1", EntryPoint = "CloseSerialCommunication")]
		static extern void CloseSerialCommunicationVer0();
		[DllImport("crt2lcd1", EntryPoint = "Crt2LcdReadAdjusterMin")]
		static extern int ReadAdjusterMinVer0(int type);
		[DllImport("crt2lcd1", EntryPoint = "Crt2LcdReadAdjusterMax")]
		static extern int ReadAdjusterMaxVer0(int type);
		[DllImport("crt2lcd1", EntryPoint = "Crt2LcdSetAdjuster")]
		static extern int SetAdjusterVer0(int type, int val);

		[DllImport("crt2lcd", EntryPoint = "OpenSerialCommunication")]
		static extern int OpenSerialCommunicationVer1(ushort serialPort, uint serialBaud);
		[DllImport("crt2lcd", EntryPoint = "CloseSerialCommunication")]
		static extern void CloseSerialCommunicationVer1();
		[DllImport("crt2lcd", EntryPoint = "Crt2Lcd_ReadAdjuster")]
		static extern int ReadAdjusterVer1(int cmd, byte id, ref BrightnessValues values);
		[DllImport("crt2lcd", EntryPoint = "Crt2Lcd_SetAdjuster")]
		static extern int SetAdjusterVer1(int cmd, byte id, int val);


		#endregion

		private static Logger _Logger = LogManager.GetCurrentClassLogger();

		private bool _Open = false;
		private bool _Invert = false;
		private int _Level = 0;
		private double _Min = 0;
		private double _Step = 0;
		private double _MinBrightness = 0;
		private double _MinContrast = 0;
		private double _FactorBrightness = 0;
		private double _FactorContrast = 0;

        private ControlBrillo _controlBrillo = null;

		[EventPublication(EventTopicNames.BrightnessLevelChanged, PublicationScope.Global)]
		public event EventHandler BrightnessLevelChanged;

        public bool Open
        {
            get { return _Open; }
        }

		public int Level
		{
			get { return _Level; }
			set 
			{ 
				if ((_Level != value) && _Open)
				{
					int result = 0;
                    switch (Settings.Default.BrightnessVersion)
                    {
                        case 0:
                            result = SetLevelVer0(value);
                            break;
                        case 1:
                            result = SetLevelVer1(value);
                            break;
                        case 2:
                            try
                            {
                                _controlBrillo.SetBrilloPerCent((ushort)(value * 14));
                                result = value;
                            }
                            catch (System.Management.ManagementException x1)
                            {
                                if (x1.ErrorCode == System.Management.ManagementStatus.NotSupported)
                                {
                                    _Open = false;
                                    General.SafeLaunchEvent(BrightnessLevelChanged, this);
                                    return;
                                }
                            }
                            catch (Exception )
                            {
                                //if (ex.Message == "Incompatible ")
                                //{
                                //    _Open = false;
                                //    General.SafeLaunchEvent(BrightnessLevelChanged, this);
                                //    return;
                                //}
                            }
                            break;

                        case 1000:      // Simulador de Control de Brillo
                            result = value;
                            break;

                        default:
                            break;
                    }

					if (result >= 0)
					{
						_Level = result;

						Settings.Default.BrightnessLevel = value;
						Settings.Default.Save();

						General.SafeLaunchEvent(BrightnessLevelChanged, this);
					}
				}
			}
		}

		public Brightness()
		{
			try
			{
				int max = Settings.Default.BrightnessMax;
				int min = Settings.Default.BrightnessMin;
				int level = Settings.Default.BrightnessLevel;

                if (level > 0)
                {
				if (max < min)
				{
					int tmp = max;
					max = min;
					min = tmp;

					_Invert = true;
				}

				_Min = min;
				_Step = ((double)max - _Min) / 7;

				if (Settings.Default.BrightnessVersion == 0)
				{
					if (OpenSerialCommunicationVer0(Settings.Default.BrightnessComPort, 19200) > 0)
					{
						_Open = true;

						_MinBrightness = ReadAdjusterMinVer0(Adj_Backlight);
						_FactorBrightness = (ReadAdjusterMaxVer0(Adj_Backlight) - _MinBrightness) / 100;
					}
				}
				else if (Settings.Default.BrightnessVersion == 1)
				{
					if (OpenSerialCommunicationVer1(Settings.Default.BrightnessComPort, 57600) > 0)
					{
						_Open = true;

						BrightnessValues vs = new BrightnessValues(0, 0xFF, 0x90);
						ReadAdjusterVer1(ADJUSTER_CMD, BRIGHT_ID, ref vs);
						_MinBrightness = vs.Min;
						_FactorBrightness = (double)(vs.Max - vs.Min) / 100;

						ReadAdjusterVer1(ADJUSTER_CMD, CONTRAST_ID, ref vs);
						_MinContrast = vs.Min;
						_FactorContrast = (double)(vs.Max - vs.Min) / 100;
					}
                }
                else if (Settings.Default.BrightnessVersion == 2)
                {
                    _controlBrillo = new ControlBrillo();
                    _Open = true;
                }
                else if (Settings.Default.BrightnessVersion == 1000)
                {
                    _Open = true;
                }
                    else
                    {
                        throw new ApplicationException("Ocultar Control Brillo");
                    }
                }
				Level = level;
			}
			catch (Exception ex)
			{
				_Logger.Error(Resources.BrightnessError + ": " + ex.Message);
                _Open = false;
                    General.SafeLaunchEvent(BrightnessLevelChanged, this);
			}
		}

		private int SetLevelVer0(int level)
		{
			Debug.Assert((level >= 0) && (level <= 7));

			int value = _Invert ? (7 - level) : level;
			int val = (int)(((_Min + (value * _Step)) * _FactorBrightness) + _MinBrightness);

			if (SetAdjusterVer0(Adj_Backlight, val) != -1)
			{
				return level;
			}

			return -1;
		}

		private int SetLevelVer1(int level)
		{
			Debug.Assert((level >= 0) && (level <= 7));

			int value = _Invert ? (7 - level) : level;
			int val = (int)(((_Min + (value * _Step)) * _FactorBrightness) + _MinBrightness);

			if (SetAdjusterVer1(ADJUSTER_CMD, BRIGHT_ID, val) != -1)
			{
				val = (int)(((_Min + (value * _Step)) * _FactorContrast) + _MinContrast);
				if (SetAdjusterVer1(ADJUSTER_CMD, CONTRAST_ID, val) != -1)
				{
					return level;
				}
			}

			return -1;
		}
	}
}
