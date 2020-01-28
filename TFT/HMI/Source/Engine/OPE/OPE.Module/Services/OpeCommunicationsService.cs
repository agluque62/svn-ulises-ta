using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections;
using System.Net;
using Microsoft.Practices.ObjectBuilder;
using Microsoft.Practices.CompositeUI;
using Microsoft.Practices.CompositeUI.EventBroker;
using HMI.Infrastructure.Interface;
using HMI.Model.Module.Services;
using HMI.Model.Module.Messages;
using HMI.Model.Module.BusinessEntities;
using HMI.OPE.Module.Properties;
using HMI.OPE.Module.Constants;
using HMI.OPE.Module.BusinessEntities;
using NLog;
using Utilities;

namespace HMI.OPE.Module.Services
{
	class OpeCommunicationsService
	{
		#region Dll Interface

		enum Drv422EventType
		{
			M_EVENT_422_TXI = 0,	// Inicio de Transmision
			M_EVENT_422_TXE,		// Fin de Transmision
			M_EVENT_422_RXI,		// Inicio de Recepcion	
			M_EVENT_422_RXE,		// Fin de Recepcion
			M_EVENT_422_POLLING,	// Fin de Recepcion
			M_EVENT_422_CTS0,		// Cambio en linea CTS.
			M_EVENT_422_CTS1,		// Cambio en linea CTS.
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void Drv422EventHandler(Drv422EventType ev, int nby, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] datos, int port);

		[DllImport("TFTLIB32stx", EntryPoint = "?OpenDrv422@@YAHHPADP6AXW4tEventDrv422@@HPAXH@Z@Z", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		static extern int OpenDrv422(int port, string setup, Drv422EventHandler handler);
		[DllImport("TFTLIB32stx", EntryPoint = "?CloseDrv422@@YAHH@Z", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		static extern int CloseDrv422(int port);
		[DllImport("TFTLIB32stx", EntryPoint = "?TxTrama422@@YAHHHPAX@Z", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		static extern int TxTrama422(int port, int nby, byte[] data);
		[DllImport("TFTLIB32stx", EntryPoint = "?SetProtocoloStxEtx@@YAXH_N@Z", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		static extern void SetProtocoloStxEtx(int port, bool stx_etx);
		[DllImport("TFTLIB32stx", EntryPoint = "?SetSilencio@@YAXH@Z", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		static extern void SetSilencio(int port);
		[DllImport("TFTLIB32stx", EntryPoint = "?ResetSilencio@@YAXH@Z", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		static extern void ResetSilencio(int port);

		#endregion

		private static OpeCommunicationsService _This = null;
		private static Logger _Logger = LogManager.GetCurrentClassLogger();

		private bool _Running = false;
		private bool _Silence = false;
		private bool _Connected = false;
		private bool _PttOn = false;
		private object _Sync = new object();
		private byte _NumUser = 0;
		private int _Page = 0;
		private string _IaNumber = "";
		private string _LastIaNumber = "";
		private string _LastIaAlias = "";
		private string _OpeNumberAlias = "";
		private SplitMode _SplitMode = SplitMode.Off;
		private PriorityStType _Priority = PriorityStType.Idle;
		private Drv422EventHandler _OnRxData = new Drv422EventHandler(OnRxData);
		private Encoding _Encoding = Encoding.GetEncoding(1252);
		private BitArray _RdMask = new BitArray(Radio.NumDestinations, false);
		private BitArray _TlfMask = new BitArray(Tlf.NumDestinations, false);
		private BitArray _LcMask = new BitArray(Lc.NumDestinations, false);
		private Dictionary<int, Dictionary<int, byte[]>> _Cache = new Dictionary<int, Dictionary<int, byte[]>>();
		private ScvInfo _ScvInfo = new ScvInfo();
		private ServerCommunicationsService _CfgServer = null;
		private int _NumPositionsByPage = Settings.Default.NumPosByRdPage;
		private int _DisconnectionTime = Settings.Default.CommTout;
		private int _Port;
		private Timer _DisconnectionTimer;
		private Queue<Pair<Drv422EventType, byte[]>> _ReceivedData;
		private Queue<byte[]> _SendedData;
		private Semaphore _ReceiveQueue;
		private Semaphore _SendQueue;
		private Thread _ReceiveThread;
		private Thread _SendThread;
		private UdpSocket _Simulator;
		private IPEndPoint _SimulatorEP;
		private ManualResetEvent _EndEvent;
        private RadioAsgHist.stdPos _ActualState, _OldState = RadioAsgHist.stdPos.Desasignado;

		[EventPublication(EventTopicNames.ConnectionStateEngine, PublicationScope.Global)]
		public event EventHandler<EngineConnectionStateMsg> ConnectionStateEngine;

		[EventPublication(EventTopicNames.IsolatedStateEngine, PublicationScope.Global)]
		public event EventHandler<EngineIsolatedStateMsg> IsolatedStateEngine;

		[EventPublication(EventTopicNames.ActiveScvEngine, PublicationScope.Global)]
		public event EventHandler<ActiveScvMsg> ActiveScvEngine;

		[EventPublication(EventTopicNames.PositionIdEngine, PublicationScope.Global)]
		public event EventHandler<PositionIdMsg> PositionIdEngine;

		[EventPublication(EventTopicNames.ResetEngine, PublicationScope.Global)]
		public event EventHandler ResetEngine;

		[EventPublication(EventTopicNames.SplitModeEngine, PublicationScope.Global)]
		public event EventHandler<StateMsg<SplitMode>> SplitModeEngine;

		[EventPublication(EventTopicNames.JacksStateEngine, PublicationScope.Global)]
		public event EventHandler<JacksStateMsg> JacksStateEngine;

		[EventPublication(EventTopicNames.BuzzerStateEngine, PublicationScope.Global)]
		public event EventHandler<StateMsg<bool>> BuzzerStateEngine;

		[EventPublication(EventTopicNames.BuzzerLevelEngine, PublicationScope.Global)]
		public event EventHandler<LevelMsg<Buzzer>> BuzzerLevelEngine;

		[EventPublication(EventTopicNames.TlfPositionsEngine, PublicationScope.Global)]
		public event EventHandler<RangeMsg<TlfDestination>> TlfPositionsEngine;

		[EventPublication(EventTopicNames.TlfPosStateEngine, PublicationScope.Global)]
		public event EventHandler<RangeMsg<TlfState>> TlfPosStateEngine;

		[EventPublication(EventTopicNames.LcSpeakerLevelEngine, PublicationScope.Global)]
		public event EventHandler<LevelMsg<LcSpeaker>> LcSpeakerLevelEngine;

		[EventPublication(EventTopicNames.TlfHeadPhonesLevelEngine, PublicationScope.Global)]
		public event EventHandler<LevelMsg<TlfHeadPhones>> TlfHeadPhonesLevelEngine;

		[EventPublication(EventTopicNames.RdPositionsEngine, PublicationScope.Global)]
		public event EventHandler<RangeMsg<RdDestination>> RdPositionsEngine;

		[EventPublication(EventTopicNames.RdPageEngine, PublicationScope.Global)]
		public event EventHandler<PageMsg> RdPageEngine;

		[EventPublication(EventTopicNames.RdPttEngine, PublicationScope.Global)]
		public event EventHandler<StateMsg<bool>> RdPttEngine;

		[EventPublication(EventTopicNames.RdPosPttStateEngine, PublicationScope.Global)]
		public event EventHandler<RangeMsg<PttState>> RdPosPttStateEngine;

		[EventPublication(EventTopicNames.RdPosSquelchStateEngine, PublicationScope.Global)]
		public event EventHandler<RangeMsg<SquelchState>> RdPosSquelchStateEngine;

		[EventPublication(EventTopicNames.RdPosAsignStateEngine, PublicationScope.Global)]
		public event EventHandler<RangeMsg<RdAsignState>> RdPosAsignStateEngine;

		[EventPublication(EventTopicNames.RdRtxModificationEndEngine, PublicationScope.Global)]
		public event EventHandler RdRtxModificationEndEngine;

		[EventPublication(EventTopicNames.RdRtxGroupsEngine, PublicationScope.Global)]
		public event EventHandler<RangeMsg<RdRtxGroup>> RdRtxGroupsEngine;

		[EventPublication(EventTopicNames.RdSpeakerLevelEngine, PublicationScope.Global)]
		public event EventHandler<LevelMsg<RdSpeaker>> RdSpeakerLevelEngine;

		[EventPublication(EventTopicNames.RdHeadPhonesLevelEngine, PublicationScope.Global)]
		public event EventHandler<LevelMsg<RdHeadPhones>> RdHeadPhonesLevelEngine;

		[EventPublication(EventTopicNames.RdFrAsignedToOtherEngine, PublicationScope.Global)]
		public event EventHandler<RdFrAsignedToOtherMsg> RdFrAsignedToOtherEngine;

		[EventPublication(EventTopicNames.LcPositionsEngine, PublicationScope.Global)]
		public event EventHandler<RangeMsg<LcDestination>> LcPositionsEngine;

		[EventPublication(EventTopicNames.LcPosStateEngine, PublicationScope.Global)]
		public event EventHandler<RangeMsg<LcState>> LcPosStateEngine;

		[EventPublication(EventTopicNames.PriorityStateEngine, PublicationScope.Global)]
		public event EventHandler<StateMsg<FunctionState>> PriorityStateEngine;

		[EventPublication(EventTopicNames.TransferStateEngine, PublicationScope.Global)]
		public event EventHandler<StateMsg<FunctionState>> TransferStateEngine;

		[EventPublication(EventTopicNames.IntrudedByEngine, PublicationScope.Global)]
		public event EventHandler<StateMsg<string>> IntrudedByEngine;

		[EventPublication(EventTopicNames.InterruptedByEngine, PublicationScope.Global)]
		public event EventHandler<StateMsg<string>> InterruptedByEngine;

		[EventPublication(EventTopicNames.IntrudeToEngine, PublicationScope.Global)]
		public event EventHandler<StateMsg<string>> IntrudeToEngine;

		[EventPublication(EventTopicNames.ListenStateEngine, PublicationScope.Global)]
		public event EventHandler<ListenPickUpMsg> ListenStateEngine;

		[EventPublication(EventTopicNames.HangToneStateEngine, PublicationScope.Global)]
		public event EventHandler<StateMsg<bool>> HangToneStateEngine;

		[EventPublication(EventTopicNames.TlfIaPosStateEngine, PublicationScope.Global)]
		public event EventHandler<RangeMsg<TlfIaDestination>> TlfIaPosStateEngine;

		[EventPublication(EventTopicNames.ShowNotifMsgEngine, PublicationScope.Global)]
		public event EventHandler<NotifMsg> ShowNotifMsgEngine;

		//[EventPublication(EventTopicNames.HideNotifMsgEngine, PublicationScope.Global)]
		//public event EventHandler<EventArgs<string>> HideNotifMsgEngine;

		[EventPublication(EventTopicNames.RemoteListenStateEngine, PublicationScope.Global)]
		public event EventHandler<ListenPickUpMsg> RemoteListenStateEngine;

		[EventPublication(EventTopicNames.ConfListEngine, PublicationScope.Global)]
		public event EventHandler<RangeMsg<string>> ConfListEngine;

		[EventPublication(EventTopicNames.PermissionsEngine, PublicationScope.Global)]
		public event EventHandler<StateMsg<Permissions>> PermissionsEngine;

		[CreateNew]
		public ServerCommunicationsService CfgServer
		{
			get { return _CfgServer; }
			set { _CfgServer = value; }
		}

		public bool Connected
		{
			get { return _Connected; }
			private set
			{
				if (value != _Connected)
				{
					_Connected = value;
					if (!value)
					{
						_Logger.Warn("La OPE paso a estar desconectada");
					}
					else
					{
						_Logger.Info("La OPE paso a estar conectada");
					}

					try
					{
						General.SafeLaunchEvent(ConnectionStateEngine, this, new EngineConnectionStateMsg(_Connected));
					}
					catch (Exception ex)
					{
						string msg = string.Format("ERROR notificando que la OPE esta {0}", value ? "conectada" : "desconectada");
						_Logger.Error(msg, ex);
					}
				}

				if (_Connected)
				{
					_DisconnectionTimer.Change(_DisconnectionTime, Timeout.Infinite);
				}
			}
		}

		public int OpeId
		{
			get { return _ScvInfo.OpeId; }
		}

		public int IolId
		{
			get { return _ScvInfo.IolId; }
		}

		[InjectionConstructor]
		public OpeCommunicationsService()
		{
			_This = this;
		}

		public void Run()
		{
			lock (_Sync)
			{
				if (!_Running)
				{
					try
					{
						_CfgServer.Run();

						_Port = Settings.Default.CommPort;
						_DisconnectionTimer = new Timer(OnOpeDisconnected);

						_EndEvent = new ManualResetEvent(false);

						_ReceivedData = new Queue<Pair<Drv422EventType, byte[]>>(50);
						_ReceiveQueue = new Semaphore(0, int.MaxValue);
						_ReceiveThread = new Thread(ProcessReceivedDataThread);

						_ReceiveThread.Start();

						_SendedData = new Queue<byte[]>(50);
						_SendQueue = new Semaphore(0, int.MaxValue);
						_SendThread = new Thread(ProcessSendedDataThread);

						_SendThread.Start();

						if (_Port < 20)
						{
							_Silence = false;
							ResetSilencio(_Port - 1);
							SetProtocoloStxEtx(_Port - 1, Settings.Default.UseStxEtx);

							int res = OpenDrv422(_Port - 1, Settings.Default.CommCfg, _OnRxData);
							if (res == 0)
							{
								throw new Exception(string.Format(Resources.OpenDrv422Error, _Port, Settings.Default.CommCfg));
							}
						}
						else
						{
							_Simulator = new UdpSocket("127.0.0.1", 10001);
							_Simulator.MaxReceiveThreads = 1;
							_Simulator.NewDataEvent += OnReceivedData;

							_SimulatorEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 10000);

							_Simulator.BeginReceive();
						}

						_Running = true;
					}
					catch (Exception ex)
					{
						_Logger.Fatal("ERROR inicializando OPE", ex);
						Stop();
						throw;
					}
				}
			}
		}

		public void Stop()
		{
			_EndEvent.Set();

			if (_Running && (_Port < 20))
			{
				_Silence = true;
				SetSilencio(_Port - 1);
				Thread.Sleep(50);
			}

			lock (_Sync)
			{
				if ((_SendThread != null) && _SendThread.IsAlive)
				{
					_SendThread.Join();
				}

				if (_Running)
				{
					_Running = false;
					if (_Port < 20)
					{
						CloseDrv422(_Port - 1);
					}
					else
					{
						_Simulator.Dispose();
						_Simulator = null;
					}
				}

				if ((_ReceiveThread != null) && _ReceiveThread.IsAlive)
				{
					_ReceiveThread.Join();
				}

				if (_DisconnectionTimer != null)
				{
					_DisconnectionTimer.Dispose();
					_DisconnectionTimer = null;
				}

				_CfgServer.Stop();

				_EndEvent = null;
				_Connected = false;
				_ReceiveThread = null;
				_ReceiveQueue = null;
				_ReceivedData = null;
				_SendThread = null;
				_SendQueue = null;
				_SendedData = null;
			}
		}

		public void Send(byte[] data)
		{
			lock (_Sync)
			{
				if (_Running && _Connected)
				{
					_Logger.Trace("(Tx) Data:{0}{1}", Environment.NewLine, new BinToLogString(data));

					_SendedData.Enqueue(data);
					_SendQueue.Release();
				}
			}
		}

		public void SetIaNumber(string number)
		{
			_IaNumber = number;
		}

		public void SetPriority(bool prio)
		{
			if (prio && ((_Priority == PriorityStType.Idle) || (_Priority == PriorityStType.Error)))
			{
				Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.SetPriorityOn });
				_Logger.Debug("(Tx) Tlf.SetPriority: [On={0}]", prio);
			}
			else if (!prio && (_Priority == PriorityStType.On))
			{
				Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.SetPriorityOff });
				_Logger.Debug("(Tx) Tlf.SetPriority: [On={0}]", prio);
			}
		}

		public bool AllowTlf()
		{
			if (_PttOn && (_SplitMode == SplitMode.Off))
			{
				NotifMsg errMsg = new NotifMsg("CtlEventType.Error" + OpeErrorType.PTT_PULSADO, Resources.OpeErrorCaption, Resources.PttRunningError, 0, MessageType.Error, MessageButtons.Ok);
				General.SafeLaunchEvent(ShowNotifMsgEngine, this, errMsg);

				return false;
			}

			return true;
		}

		private void ProcessSendedDataThread()
		{
			try
			{
				WaitHandle[] handles = { _EndEvent, _SendQueue };

				while (WaitHandle.WaitAny(handles) == 1)
				{
					byte[] data = null;

					lock (_Sync)
					{
						data = _SendedData.Dequeue();
					}

					if ((data[0] == (byte)OpeEventType.Int) && (data[1] == (byte)IntCmdType.Wait))
					{
						Thread.Sleep(data[2] * 10);
					}
					else if (_Port < 20)
					{
						try
						{
							TxTrama422(_Port - 1, data.Length, data);
						}
						catch (Exception ex)
						{
							if (!_Logger.IsTraceEnabled)
							{
								_Logger.Error("(Tx) Data:{0}{1}", Environment.NewLine, new BinToLogString(data));
							}

							_Logger.Error("ERROR en envio a la OPE", ex);
						}
					}
					else
					{
						_Simulator.Send(_SimulatorEP, data);
					}
				}
			}
			catch (Exception ex)
			{
				_Logger.Fatal("ERROR en hilo de envio a la OPE", ex);
				throw;
			}
		}

		private void ProcessReceivedDataThread()
		{
			try
			{
				WaitHandle[] handles = { _EndEvent, _ReceiveQueue };

				while (WaitHandle.WaitAny(handles) == 1)
				{
					Pair<Drv422EventType, byte[]> ev;
					lock (_Sync)
					{
						ev = _ReceivedData.Dequeue();
					}

					switch (ev.First)
					{
						case Drv422EventType.M_EVENT_422_POLLING:
							Connected = true;
							break;
						case Drv422EventType.M_EVENT_422_CTS0:
							ProcessScvChange(0);
							break;
						case Drv422EventType.M_EVENT_422_CTS1:
							ProcessScvChange(1);
							break;
						case Drv422EventType.M_EVENT_422_RXE:
							Connected = true;
							ProcessReceivedData(ev.Second);
							break;
					}
				}
			}
			catch (Exception ex)
			{
				_Logger.Fatal("ERROR en hilo de recepcion de la OPE", ex);
				throw;
			}
		}

		private void OnOpeDisconnected(object state)
		{
			Connected = false;
		}

		// OPE Real
		private static void OnRxData(Drv422EventType ev, int nby, byte[] data, int port)
		{
			_This.NewReceivedData(ev, data);
		}

		// Simulador
		private void OnReceivedData(object sender, DataGram dg)
		{
			if (Drv422EventType.M_EVENT_422_POLLING == (Drv422EventType)dg.Data[0])
			{
				_Simulator.Send(_SimulatorEP, dg.Data);
			}

			byte[] data = new byte[dg.Data.Length - 1];
			Array.Copy(dg.Data, 1, data, 0, data.Length);

			NewReceivedData((Drv422EventType)dg.Data[0], data);
		}

		private void NewReceivedData(Drv422EventType ev, byte[] data)
		{
			if (!_Silence)
			{
				lock (_Sync)
				{
					if (_Running)
					{
						_ReceivedData.Enqueue(new Pair<Drv422EventType, byte[]>(ev, data));
						_ReceiveQueue.Release();
					}
				}
			}
		}

		private void ProcessScvChange(int scv)
		{
			try
			{
				General.SafeLaunchEvent(ActiveScvEngine, this, new ActiveScvMsg(scv));
			}
			catch (Exception ex)
			{
				string msg = string.Format("ERROR notificando cambio de SCV activo [SCV={0}]", scv);
				_Logger.Error(msg, ex);
			}
		}

		private void ProcessReceivedData(byte[] data)
		{
			try
			{
				if (data.Length >= 3)
				{
					Dictionary<int, byte[]> info = null;

					if (!_Cache.TryGetValue(data[0] | (data[1] << 8), out info))
					{
						info = new Dictionary<int, byte[]>(1);
						_Cache[data[0] | (data[1] << 8)] = info;
					}

					byte[] lastData = null;
					info.TryGetValue(data[2], out lastData);

					if ((lastData != null) && (lastData.Length == data.Length))
					{
						bool equal = true;

						for (int i = 3, to = lastData.Length; i < to; i++)
						{
							if (lastData[i] != data[i])
							{
								equal = false;
								break;
							}
						}

						if (equal)
						{
							return;
						}
					}
				}

				_Logger.Trace("(Rx) Data:{0}{1}", Environment.NewLine, new BinToLogString(data));

				switch ((OpeEventType)data[0])
				{
					case OpeEventType.Ctl:
						ProcessReceivedCtlData(data);
						break;
					case OpeEventType.Tlf:
						ProcessReceivedTlfData(data);
						break;
					case OpeEventType.Radio:
						ProcessReceivedRdData(data);
						break;
					case OpeEventType.Lc:
						ProcessReceivedLcData(data);
						break;
					case OpeEventType.Cfg:
						ProcessReceivedCfgData(data);
						break;
				}
			}
			catch (Exception ex)
			{
				if (!_Logger.IsTraceEnabled)
				{
					_Logger.Error("(Rx) Data:{0}{1}", Environment.NewLine, new BinToLogString(data));
				}

				_Logger.Error("ERROR procesando comando recibido de la OPE", ex);
			}
		}

		private void ProcessReceivedCtlData(byte[] data)
		{
			switch ((CtlEventType)data[1])
			{
				case CtlEventType.SysInfo:
					_ScvInfo.Reset(data[3], data[5]);

					General.SafeLaunchEvent(IsolatedStateEngine, this, new EngineIsolatedStateMsg(data[5] == 0xFF));
					General.SafeLaunchEvent(JacksStateEngine, this, new JacksStateMsg(data[6] != 0, data[7] != 0));
					break;
				case CtlEventType.SplitMode:
					_SplitMode = (data[2] == 1 ? SplitMode.Off : (data[2] == 2 ? SplitMode.RdLc : SplitMode.LcTf));
					General.SafeLaunchEvent(SplitModeEngine, this, new StateMsg<SplitMode>(_SplitMode));
					break;
				case CtlEventType.OpeVersion:
					_ScvInfo.OpeVersion = _Encoding.GetString(data, 4, 8).TrimEnd(' ', '\x0');
					_ScvInfo.OpeDate = _Encoding.GetString(data, 12, 17).TrimEnd(' ', '\x0');
					_ScvInfo.DspVersion = _Encoding.GetString(data, 44, 8).TrimEnd(' ', '\x0');
					_ScvInfo.DspDate = _Encoding.GetString(data, 52, 17).TrimEnd(' ', '\x0');

					if (_ScvInfo.IolVersion.Length > 0)
					{
						NotifMsg scvInfoMsg = new NotifMsg("ScvInfo", Resources.ScvInfo, _ScvInfo.ToString(), 0, MessageType.Information, MessageButtons.Ok);
						scvInfoMsg.Height = 150;
						scvInfoMsg.Width = 720;
						General.SafeLaunchEvent(ShowNotifMsgEngine, this, scvInfoMsg);
					}

					break;
				case CtlEventType.IolVersion:
					_ScvInfo.IolVersion = _Encoding.GetString(data, 4, 8).TrimEnd(' ', '\x0');
					_ScvInfo.IolDate = _Encoding.GetString(data, 12, 17).TrimEnd(' ', '\x0');

					if (_ScvInfo.OpeVersion.Length > 0)
					{
						NotifMsg scvInfoMsg = new NotifMsg("ScvInfo", Resources.ScvInfo, _ScvInfo.ToString(), 0, MessageType.Information, MessageButtons.Ok);
						scvInfoMsg.Height = 150;
						scvInfoMsg.Width = 720;
						General.SafeLaunchEvent(ShowNotifMsgEngine, this, scvInfoMsg);
					}

					break;
				case CtlEventType.M4lVersion:
					break;
				case CtlEventType.RepeatedUser:
					break;
				case CtlEventType.OpeSwitchs:
					break;
				case CtlEventType.Error:
					string err = null;

					switch ((OpeErrorType)data[2])
					{
						case OpeErrorType.ERROR_OPE:
							err = Resources.OperativityError;
							break;
						case OpeErrorType.EMPL_UNICO:
							err = Resources.PlacementError;
							break;
						case OpeErrorType.CANAL_EN_GRTX:
							err = Resources.RtxError;
							break;
						case OpeErrorType.PTT_PULSADO:
							err = Resources.PttRunningError;
							break;
						case OpeErrorType.NO_JACKS_COOR:
							err = Resources.CoorJacksError;
							break;
						case OpeErrorType.NO_JACKS:
							err = Resources.JacksError;
							break;
						case OpeErrorType.MAX_FRQ_GRTX:
							err = Resources.RtxGroupMaxError;
							break;
						case OpeErrorType.RTX_NO_ACEPTADA:
							err = Resources.RtxGroupError;
							break;
						case OpeErrorType.UNA_FRQ_GRTX:
							err = Resources.FrecuencyRtxError;
							break;
						case OpeErrorType.SERV_NO_CAPT:
							err = Resources.SrvError;
							break;
						case OpeErrorType.FAC_IMPOSIBLE:
							err = Resources.FacilityError;
							break;
						case OpeErrorType.CODIGO_ERRONEO:
							err = Resources.CodeError;
							break;
						case OpeErrorType.CODIGO_NO_DEPEN:
							err = Resources.DependCodeError;
							break;
						case OpeErrorType.ENLACE_CONF:
							err = Resources.LinkError;
							break;
						case OpeErrorType.TF_FUERA_SECTOR:
							err = Resources.TlfUnsectorError;
							break;
						case OpeErrorType.TF_NO_DISPONIBLE:
							err = Resources.TlfUnavailableError;
							break;
						case OpeErrorType.OPE_AISLADA:
							err = Resources.IsolatedError;
							break;
						case OpeErrorType.PTT_ERROR:
							//err = Resources.PttError;
							break;
						case OpeErrorType.TIME_OUT_PTT:
							err = Resources.PttTimeoutError;
							break;
						case OpeErrorType.SUB_TF_USADO:
							err = Resources.TlfUsedError;
							break;
						case OpeErrorType.SQUELCH_EN_MAS_FRQ:
							err = Resources.RtxGroupSquelchError;
							break;
						case OpeErrorType.MAX_PARTI_CONF:
							err = Resources.ConfError;
							break;
						case OpeErrorType.LLAMADA_EN_CURSO:
							err = Resources.CallError;
							break;
						default:
							err = string.Format(Resources.GenericError, data[2]);
							break;
					}

					if (err != null)
					{
						NotifMsg errMsg = new NotifMsg("CtlEventType.Error" + ((OpeErrorType)data[2]), Resources.OpeErrorCaption, err, 0, MessageType.Error, MessageButtons.Ok);
						General.SafeLaunchEvent(ShowNotifMsgEngine, this, errMsg);
					}
					break;

				case CtlEventType.Log:
					_Logger.Log(LogLevel.FromOrdinal(data[2]), new BinToLogString(data));
					break;
			}
		}

		private void ProcessReceivedTlfData(byte[] data)
		{
			switch ((TlfEventType)data[1])
			{
				case TlfEventType.TlfPosState:
					_Cache[data[0] | (data[1] << 8)][data[2]] = data;

					if ((data[2] > 0) && (data[2] <= Tlf.NumDestinations) && _TlfMask[data[2] - 1])
					{
						TlfState st = GetTlfState(data);
						if ((st == TlfState.Mem) || (st == TlfState.RemoteMem))
						{
							Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.DaLongClick, data[2] });
						}

						General.SafeLaunchEvent(TlfPosStateEngine, this, new RangeMsg<TlfState>(data[2] - 1, st));
					}
					break;

				case TlfEventType.BuzzerState:
               _Cache[data[0] | (data[1] << 8)].Clear();
					_Cache[data[0] | (data[1] << 8)][data[2]] = data;

					if ((BuzzerStateType)data[2] != BuzzerStateType.UnavailableTemp)
					{
						bool available = ((BuzzerStateType)data[2]) != BuzzerStateType.Unavailable;
						General.SafeLaunchEvent(BuzzerStateEngine, this, new StateMsg<bool>(available));
					}
					break;
				case TlfEventType.TlfAiState:
					_Cache[data[0] | (data[1] << 8)][data[2]] = data;

					if ((data[2] > 0) && (data[2] <= Tlf.NumIaDestinations))
					{
						TlfState tlfIaState = GetTlfState(data);

						if ((tlfIaState != TlfState.Unavailable) && (tlfIaState != TlfState.Idle) &&
							(tlfIaState != TlfState.PaPBusy) && (tlfIaState != TlfState.Mem) && (tlfIaState != TlfState.RemoteMem))
						{
							if (tlfIaState == TlfState.Congestion)
							{
								string rtbNumber = _CfgServer.GetRtbNumber(_IaNumber);
								if (rtbNumber != null)
								{
									Debug.Assert(rtbNumber.Length > 2);

									Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.IaShortClick, data[2] });
									Send(new byte[] { (byte)OpeEventType.Int, (byte)IntCmdType.Wait, (byte)50 });

									byte[] cmd = new byte[2 + rtbNumber.Length];
									cmd[0] = (byte)OpeEventType.Tlf;
									cmd[1] = (byte)TlfCmdType.NumberClick;
									cmd[2] = (byte)(rtbNumber.Length - 1);
									cmd[3] = (byte)((((byte)rtbNumber[0] - 0x30) * 10) + ((byte)rtbNumber[1] - 0x30));

									for (int i = 2; i < rtbNumber.Length; i++)
									{
										cmd[2 + i] = (byte)rtbNumber[i];
									}

									Send(cmd);
									break;
								}
							}

							_OpeNumberAlias = _Encoding.GetString(data, 4, 8).Trim(' ', '\x0');

							if ((tlfIaState == TlfState.Out) && (_OpeNumberAlias.Length == 0))
							{
								_LastIaNumber = _IaNumber;
								_LastIaAlias = _CfgServer.GetNumberAlias(_LastIaNumber);
							}
							else if ((tlfIaState == TlfState.In) || (tlfIaState == TlfState.InPrio) || (tlfIaState == TlfState.RemoteIn))
							{
								_LastIaNumber = GetTlfIaNumber(data);
								_LastIaAlias = _CfgServer.GetNumberAlias(_LastIaNumber);
							}

							if (((data.Length > 13) && (data[13] != 0) && (_OpeNumberAlias.Length > 0)) || (_LastIaAlias.Length == 0))
							{
								_LastIaAlias = _CfgServer.GetEquivalentName(_OpeNumberAlias);
							}
						}
						else
						{
							if ((tlfIaState == TlfState.Mem) || (tlfIaState == TlfState.RemoteMem))
							{
								Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.IaLongClick, data[2] });
							}

							if (_LastIaNumber.Length == 0)
							{
								_LastIaAlias = "";
							}
							_OpeNumberAlias = "";
						}

						TlfIaDestination tlfIaDst = new TlfIaDestination(_LastIaAlias, _LastIaNumber, tlfIaState);
						General.SafeLaunchEvent(TlfIaPosStateEngine, this, new RangeMsg<TlfIaDestination>(data[2] - 1 + Tlf.NumDestinations, tlfIaDst));
					}
					break;

				case TlfEventType.HangUpDownState:
					break;
				case TlfEventType.HeadphonesLevel:
					General.SafeLaunchEvent(TlfHeadPhonesLevelEngine, this, new LevelMsg<TlfHeadPhones>(data[2]));
					break;
				case TlfEventType.Permissions:
					Permissions permissions = Permissions.Hold | Permissions.Transfer;
					permissions |= (data[5] != 0 ? Permissions.Listen : 0);
					permissions |= (data[6] != 0 ? Permissions.Priority : 0);
					General.SafeLaunchEvent(PermissionsEngine, this, new StateMsg<Permissions>(permissions));
					break;
				case TlfEventType.TransferState:
					FunctionState transferSt = FunctionState.Idle;
					switch ((TransferStType)data[2])
					{
						case TransferStType.Idle:
							break;
						case TransferStType.Error:
							transferSt = FunctionState.Error;
							break;
						case TransferStType.Ready:
							transferSt = FunctionState.Executing;
							break;
						default:
							_Logger.Warn("Estado TransferStType desconocido: {0}", data[2]);
							break;
					}

					General.SafeLaunchEvent(TransferStateEngine, this, new StateMsg<FunctionState>(transferSt));
					break;

				case TlfEventType.ConfState:
					break;
				case TlfEventType.BuzzerLevel:
					General.SafeLaunchEvent(BuzzerLevelEngine, this, new LevelMsg<Buzzer>(data[2]));
					break;
				case TlfEventType.ListenOn:
					if ((TlfPosType)data[2] == TlfPosType.Ad)
					{
						if ((data[3] > 0) && (data[3] <= Tlf.NumDestinations) && _TlfMask[data[3] - 1])
						{
							string onListenDst = _CfgServer.GetEquivalentName(_Encoding.GetString(data, 4, 8).Trim());
							General.SafeLaunchEvent(ListenStateEngine, this, new ListenPickUpMsg(FunctionState.Executing, onListenDst));
						}
					}
					else if ((TlfPosType)data[2] == TlfPosType.Ai)
					{
						if ((data[3] > 0) && (data[3] <= Tlf.NumIaDestinations))
						{
							string onListenDst = _CfgServer.GetEquivalentName(_Encoding.GetString(data, 4, 8).Trim());
							General.SafeLaunchEvent(ListenStateEngine, this, new ListenPickUpMsg(FunctionState.Executing, onListenDst));
						}
					}
					break;
				case TlfEventType.RejectListen:
					General.SafeLaunchEvent(ListenStateEngine, this, new ListenPickUpMsg(FunctionState.Error));
					break;
				case TlfEventType.ListenOff:
					General.SafeLaunchEvent(ListenStateEngine, this, new ListenPickUpMsg(FunctionState.Idle));
					break;
				case TlfEventType.ListenReady:
					General.SafeLaunchEvent(ListenStateEngine, this, new ListenPickUpMsg(FunctionState.Ready));
					break;
				case TlfEventType.Priority:
					_Priority = (PriorityStType)data[2];

					switch (_Priority)
					{
						case PriorityStType.Idle:
						case PriorityStType.On:
							break;
						case PriorityStType.Error:
							Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.SetPriorityOff });
							General.SafeLaunchEvent(PriorityStateEngine, this, new StateMsg<FunctionState>(FunctionState.Error));
							break;
						default:
							_Logger.Warn("Estado PriorityStType desconocido: {0}", data[2]);
							break;
					}
					break;

				case TlfEventType.RecallState:
					break;
				case TlfEventType.NoticeAlertOn:
					break;
				case TlfEventType.RejectIntrussion:
					break;
				case TlfEventType.IntrussionOff:
					break;
				case TlfEventType.IntrussionOn:
					break;
				case TlfEventType.AlertOn:
					break;
				case TlfEventType.AlertOff:
					break;
				case TlfEventType.RejectAlert:
					break;
				case TlfEventType.NoticeAlertOff:
					break;
				case TlfEventType.CoorState:
					break;
				case TlfEventType.IntrudedBy:
					string intrudedBy = _CfgServer.GetEquivalentName(_Encoding.GetString(data, 2, 8).Trim());
					General.SafeLaunchEvent(IntrudedByEngine, this, new StateMsg<string>(intrudedBy));
					break;
				case TlfEventType.IntrudeTo:
					string intrudeTo = _CfgServer.GetEquivalentName(_Encoding.GetString(data, 2, 8).Trim());
					General.SafeLaunchEvent(IntrudeToEngine, this, new StateMsg<string>(intrudeTo));
					break;
				case TlfEventType.InterruptedBy:
					string interruptedBy = _CfgServer.GetEquivalentName(_Encoding.GetString(data, 2, 8).Trim());
					General.SafeLaunchEvent(InterruptedByEngine, this, new StateMsg<string>(interruptedBy));
					break;
				case TlfEventType.AlreadyInConf:
					break;
				case TlfEventType.ConfList:
					int numConfParticipants = (data.Length - 3) / 8;
					RangeMsg<string> confList = new RangeMsg<string>(0, numConfParticipants);

					for (int i = 0; i < confList.Count; i++)
					{
						string opeAlias = _Encoding.GetString(data, 2 + (i * 8), 8).Trim();
						confList.Info[i] = (opeAlias == _OpeNumberAlias) ? _LastIaAlias : _CfgServer.GetEquivalentName(opeAlias);
					}

					General.SafeLaunchEvent(ConfListEngine, this, confList);
					break;

				case TlfEventType.HangTone:
					General.SafeLaunchEvent(HangToneStateEngine, this, new StateMsg<bool>(data[2] != 0));
					break;
				case TlfEventType.TransferDirectState:
					if (((TransferDirectStType)data[2]) == TransferDirectStType.Accepted)
					{
						Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.CancelClick });
						_Logger.Debug("(Tx) Tlf.RecognizeTransferDirectState");
					}
					else
					{
						FunctionState directTransferSt = FunctionState.Idle;
						switch ((TransferDirectStType)data[2])
						{
							case TransferDirectStType.Idle:
								break;
							case TransferDirectStType.Error:
								directTransferSt = FunctionState.Error;
								break;
							case TransferDirectStType.Ready:
								directTransferSt = FunctionState.Executing;
								break;
							default:
								_Logger.Warn("Estado TransferDirectStType desconocido: {0}", data[2]);
								break;
						}

						General.SafeLaunchEvent(TransferStateEngine, this, new StateMsg<FunctionState>(directTransferSt));
					}
					break;

				case TlfEventType.IdleState:
					RangeMsg<TlfState> tlfIdleSt = new RangeMsg<TlfState>(0, Tlf.NumDestinations, TlfState.UnChanged);

					for (int i = 2, to = data.Length - 1; i < to; i++)
					{
						if ((data[i] > 0) && (data[i] <= Tlf.NumDestinations) && _TlfMask[data[i] - 1])
						{
							tlfIdleSt.Info[data[i] - 1] = TlfState.Idle;
						}
					}

					Dictionary<int, byte[]> tlfPosCache;
					if (_Cache.TryGetValue(data[0] | (((int)TlfEventType.TlfPosState) << 8), out tlfPosCache))
					{
						tlfPosCache.Clear();
					}

					General.SafeLaunchEvent(TlfPosStateEngine, this, tlfIdleSt);
					break;
				case TlfEventType.RemoteListen:
					string listenBy = _CfgServer.GetEquivalentName(_Encoding.GetString(data, 4, 8).Trim());
					ListenPickUpMsg remoteListen = new ListenPickUpMsg(data[2] > 0 ? FunctionState.Executing : FunctionState.Idle, listenBy, data[3]);
					General.SafeLaunchEvent(RemoteListenStateEngine, this, remoteListen);
					break;
			}
		}

		private void ProcessReceivedRdData(byte[] data)
		{
			switch ((RdEventType)data[1])
			{
				case RdEventType.RdPosTxRx:
					_Cache[data[0] | (data[1] << 8)][data[2]] = data;
					if ((data[2] > 0) && (data[2] - 1 <= Radio.NumDestinations) && _RdMask[data[2] - 1])
					{
						RdAsignState posAsignState = GetRdAsignState(data, 3);
                        _ActualState = posAsignState.Rx ? RadioAsgHist.stdPos.Monitor : (posAsignState.Tx ? RadioAsgHist.stdPos.Trafico : RadioAsgHist.stdPos.Desasignado);
						General.SafeLaunchEvent(RdPosAsignStateEngine, this, new RangeMsg<RdAsignState>(data[2] - 1, 1, posAsignState));
					}
					break;
				case RdEventType.RdPageTx:
					_Cache[data[0] | (data[1] << 8)].Clear();
					_Cache[data[0] | (data[1] << 8)][data[2]] = data;

					RangeMsg<PttState> rdPageTx = new RangeMsg<PttState>(_Page * _NumPositionsByPage, _NumPositionsByPage);
					_PttOn = false;

					for (int i = 0; i < _NumPositionsByPage; i++)
					{
						PttState ptt = PttState.NoPtt;

						if (_RdMask[(_Page * _NumPositionsByPage) + i])
						{
							switch ((RdTxType)data[2 + i])
							{
								case RdTxType.PttOff:
									break;
								case RdTxType.Unavailable:
									ptt = PttState.Unavailable;
									break;
								case RdTxType.Ptt:
									ptt = PttState.PttOnlyPort;
									_PttOn = true;
									break;
								case RdTxType.Blocked:
									ptt = PttState.Blocked;
									_PttOn = true;
									break;
								case RdTxType.ExternPtt:
									ptt = PttState.ExternPtt;
									break;
								case RdTxType.ErrorPtt:
									ptt = PttState.Error;
									_PttOn = true;
									break;
								case RdTxType.UnallowedPtt:
								case RdTxType.UnavailableNtz:
								case RdTxType.UnavailableNtzExt:
								case RdTxType.Rtx:
								case RdTxType.ExternRtx:
									_Logger.Warn("Estado RdTxType no manejado: {0}", (RdTxType)data[2 + i]);
									break;
								default:
									_Logger.Warn("Estado RdTxType desconocido: {0}", data[2 + i]);
									break;
							}
						}
						rdPageTx.Info[i] = ptt;
					}

					General.SafeLaunchEvent(RdPttEngine, this, new StateMsg<bool>(_PttOn));
					General.SafeLaunchEvent(RdPosPttStateEngine, this, rdPageTx);
					break;

				case RdEventType.RdPageRx:
					_Cache[data[0] | (data[1] << 8)].Clear();
					_Cache[data[0] | (data[1] << 8)][data[2]] = data;

					RangeMsg<SquelchState> rdPageRx = new RangeMsg<SquelchState>(_Page * _NumPositionsByPage, _NumPositionsByPage);

					for (int i = 0; i < _NumPositionsByPage; i++)
					{
						SquelchState squelch = SquelchState.NoSquelch;

						if (_RdMask[(_Page * _NumPositionsByPage) + i])
						{
							switch ((RdRxType)data[2 + i])
							{
								case RdRxType.SquelchOff:
									break;
								case RdRxType.SquelchOn:
									squelch = SquelchState.SquelchOnlyPort;
									break;
								case RdRxType.SquelchMod:
									squelch = SquelchState.SquelchPortAndMod;
									break;
								case RdRxType.Unavailable:
									squelch = SquelchState.Unavailable;
									break;
								default:
									_Logger.Warn("Estado RdRxType desconocido: {0}", data[2 + i]);
									break;
							}
						}
						rdPageRx.Info[i] = squelch;
					}

					General.SafeLaunchEvent(RdPosSquelchStateEngine, this, rdPageRx);
					break;

				case RdEventType.SpeakerLevel:
					General.SafeLaunchEvent(RdSpeakerLevelEngine, this, new LevelMsg<RdSpeaker>(data[2]));
					break;
				case RdEventType.HeadphonesLevel:
					General.SafeLaunchEvent(RdHeadPhonesLevelEngine, this, new LevelMsg<RdHeadPhones>(data[2]));
					break;
				case RdEventType.RdFrAsignedToOther:
					if ((data[2] > 0) && (data[2] - 1 <= Radio.NumDestinations) && _RdMask[data[2] - 1])
					{
						string asignedTo = _CfgServer.GetEquivalentName(_Encoding.GetString(data, 4, 8).Trim());
						General.SafeLaunchEvent(RdFrAsignedToOtherEngine, this, new RdFrAsignedToOtherMsg(data[2] - 1, asignedTo));
					}
					break;
				case RdEventType.RdSiteChanged:
					break;
				case RdEventType.RtxGroupInfoOld:
					break;
				case RdEventType.RtxGroupInfoAbs:
					break;
				case RdEventType.RtxEnd:
					General.SafeLaunchEvent(RdRtxModificationEndEngine, this);
					break;
				case RdEventType.RdPageRtxGroup:
					_Cache[data[0] | (data[1] << 8)].Clear();
					_Cache[data[0] | (data[1] << 8)][data[2]] = data;

					RangeMsg<RdRtxGroup> rdPageRtxGroup = new RangeMsg<RdRtxGroup>(_Page * _NumPositionsByPage, _NumPositionsByPage);

					for (int i = 0; i < _NumPositionsByPage; i++)
					{
						int rtxGroup = 0;

						if (_RdMask[(_Page * _NumPositionsByPage) + i])
						{
							rtxGroup = data[(i * 2) + 3];
							if (rtxGroup != 0)
							{
								rtxGroup = (data[(i * 2) + 2] == _NumUser ? rtxGroup : -1);
							}
						}

						rdPageRtxGroup.Info[i] = new RdRtxGroup(rtxGroup);
					}

					General.SafeLaunchEvent(RdRtxGroupsEngine, this, rdPageRtxGroup);
					break;
				case RdEventType.CoorState:
					break;
				case RdEventType.RdPageAsign:
					_Cache[data[0] | (data[1] << 8)].Clear();
					_Cache[data[0] | (data[1] << 8)][data[2]] = data;

					RangeMsg<RdAsignState> rdPageAsign = new RangeMsg<RdAsignState>(_Page * _NumPositionsByPage, _NumPositionsByPage);

					for (int i = 0; i < _NumPositionsByPage; i++)
					{
						rdPageAsign.Info[i] = (_RdMask[(_Page * _NumPositionsByPage) + i]) ?
							GetRdAsignState(data, 2 + (i * 2)) : new RdAsignState();
					}

					General.SafeLaunchEvent(RdPosAsignStateEngine, this, rdPageAsign);
					break;

				case RdEventType.InCaState:
					break;
				case RdEventType.ActiveRdPage:
					if ((data[2] > 0) && (data[2] <= ((Radio.NumDestinations + _NumPositionsByPage - 1) / _NumPositionsByPage)))
					{
						int oldPage = _Page;
						_Page = data[2] - 1;

						Dictionary<int, byte[]> elementCache;
						if (_Cache.TryGetValue(data[0] | ((int)RdEventType.RdPosTxRx << 8), out elementCache))
						{
							elementCache.Clear();
						}
						if (_Cache.TryGetValue(data[0] | ((int)RdEventType.RdPageTx << 8), out elementCache))
						{
							elementCache.Clear();
						}
						if (_Cache.TryGetValue(data[0] | ((int)RdEventType.RdPageRx << 8), out elementCache))
						{
							elementCache.Clear();
						}
						if (_Cache.TryGetValue(data[0] | ((int)RdEventType.RdPageAsign << 8), out elementCache))
						{
							elementCache.Clear();
						}
						if (_Cache.TryGetValue(data[0] | ((int)RdEventType.RdPageRtxGroup << 8), out elementCache))
						{
							elementCache.Clear();
						}

						General.SafeLaunchEvent(RdPageEngine, this, new PageMsg(_Page));

						RangeMsg<PttState> rdPageNoPtt = new RangeMsg<PttState>(oldPage * _NumPositionsByPage, _NumPositionsByPage, PttState.NoPtt);
						RangeMsg<SquelchState> rdPageNoSquelch = new RangeMsg<SquelchState>(oldPage * _NumPositionsByPage, _NumPositionsByPage, SquelchState.NoSquelch);
						RangeMsg<RdAsignState> rdPageNoTxRx = new RangeMsg<RdAsignState>(oldPage * _NumPositionsByPage, _NumPositionsByPage, new RdAsignState(false, false, RdRxAudioVia.NoAudio));
						RangeMsg<RdRtxGroup> rdPageNoRtxGroup = new RangeMsg<RdRtxGroup>(oldPage * _NumPositionsByPage, _NumPositionsByPage, new RdRtxGroup(0));

						General.SafeLaunchEvent(RdPosPttStateEngine, this, rdPageNoPtt);
						General.SafeLaunchEvent(RdPosSquelchStateEngine, this, rdPageNoSquelch);
						General.SafeLaunchEvent(RdPosAsignStateEngine, this, rdPageNoTxRx);
						General.SafeLaunchEvent(RdRtxGroupsEngine, this, rdPageNoRtxGroup);
					}
					break;
				case RdEventType.VisibleRdPage:
					break;
				case RdEventType.TxChannels:
					break;
			}
		}

		private void ProcessReceivedLcData(byte[] data)
		{
			switch ((LcEventType)data[1])
			{
				case LcEventType.LcPosState:
					_Cache[data[0] | (data[1] << 8)][data[2]] = data;

					if ((data[2] > 0) && (data[2] <= Lc.NumDestinations) && _LcMask[data[2] - 1])
					{
						LcState lc = new LcState();

						switch ((LcRxType)data[3])
						{
							case LcRxType.Idle:
								break;
							case LcRxType.Mem:
								Send(new byte[] { (byte)OpeEventType.Lc, (byte)LcCmdType.MouseDown, (byte)data[2] });
								Send(new byte[] { (byte)OpeEventType.Lc, (byte)LcCmdType.MouseUp, (byte)data[2] });
								lc.Rx = LcRxState.Mem;
								break;
							case LcRxType.Rx:
								lc.Rx = LcRxState.Rx;
								break;
							case LcRxType.Unavailable:
								lc.Rx = LcRxState.Unavailable;
								break;
							default:
								_Logger.Warn("Estado LcRxType desconocido: {0}", data[3]);
								break;
						}

						switch ((LcTxType)data[4])
						{
							case LcTxType.Idle:
								break;
							case LcTxType.Tx:
								lc.Tx = LcTxState.Tx;
								break;
							case LcTxType.Busy:
								lc.Tx = LcTxState.Busy;
								break;
							case LcTxType.Unavailable:
								lc.Tx = LcTxState.Unavailable;
								break;
							case LcTxType.Congestion:
								lc.Tx = LcTxState.Congestion;
								break;
							default:
								_Logger.Warn("Estado LcTxType desconocido: {0}", data[4]);
								break;
						}

						General.SafeLaunchEvent(LcPosStateEngine, this, new RangeMsg<LcState>(data[2] - 1, lc));
					}
					break;

				case LcEventType.SpeakerLevel:
					General.SafeLaunchEvent(LcSpeakerLevelEngine, this, new LevelMsg<LcSpeaker>(data[2]));
					break;
			}
		}

		private void ProcessReceivedCfgData(byte[] data)
		{
			switch ((CfgEventType)data[1])
			{
				case CfgEventType.UserStates:
					break;
				case CfgEventType.Sites:
					break;
				case CfgEventType.Ini:
					_NumUser = data[2];
					_CfgServer.AskServerCfg(_NumUser);

					_Cache.Clear();

					bool opeReset = (data[29] != 0xFF);
					string positionId = _CfgServer.GetEquivalentName(_Encoding.GetString(data, 3, 8).Trim(' ', '\x0'));
					_SplitMode = (data[28] == 1 ? SplitMode.Off : (data[28] == 2 ? SplitMode.RdLc : SplitMode.LcTf));

					if (opeReset)
					{
						General.SafeLaunchEvent(ResetEngine, this);
					}

					General.SafeLaunchEvent(PositionIdEngine, this, new PositionIdMsg(positionId));
					General.SafeLaunchEvent(SplitModeEngine, this, new StateMsg<SplitMode>(_SplitMode));

					Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.AskPermissions });
					_Logger.Debug("(Tx) Tlf.AskPermissions");

					break;

				case CfgEventType.RdPos:
					break;
				case CfgEventType.LcPos:
					break;
				case CfgEventType.TlfPos:
					break;
				case CfgEventType.RdMask:
					byte[] rdMask = new byte[(Radio.NumDestinations + 7) / 8];
					Array.Copy(data, 2, rdMask, 0, rdMask.Length);
					_RdMask = new BitArray(rdMask);

					break;

				case CfgEventType.TlfMask:
					byte[] tlfMask = new byte[(Tlf.NumDestinations + 7) / 8];
					Array.Copy(data, 2, tlfMask, 0, tlfMask.Length);
					_TlfMask = new BitArray(tlfMask);

					byte[] lcMask = new byte[(Lc.NumDestinations + 7) / 8];
					Array.Copy(data, 2 + 7, lcMask, 0, lcMask.Length);
					_LcMask = new BitArray(lcMask);

					break;

				case CfgEventType.CfgLoadTime:
					break;
				case CfgEventType.RdAll:
					if (data[4] < Radio.NumDestinations)
					{
						int numRd = Math.Min(data[3], Radio.NumDestinations - data[4]);
						RangeMsg<RdDestination> rdConf = new RangeMsg<RdDestination>(data[4], numRd);

						for (int rd = 0, rdPos = 4; rd < numRd; rd++, rdPos += 14)
						{
							int id = data[rdPos];
							string dst = "";
							string alias = "";

							if ((id < Radio.NumDestinations) && _RdMask[id])
							{
								dst = _Encoding.GetString(data, rdPos + 1, 8).Trim();
								alias = _CfgServer.GetNameAlias(dst, "");
							}

							rdConf.Info[rd] = new RdDestination(dst, alias);
						}

						General.SafeLaunchEvent(RdPositionsEngine, this, rdConf);
					}
					break;

				case CfgEventType.LcAll:
					if (data[4] < Lc.NumDestinations)
					{
						int numLc = Math.Min(data[3], Lc.NumDestinations - data[4]);
						RangeMsg<LcDestination> lcConf = new RangeMsg<LcDestination>(data[4], numLc);

						for (int lc = 0, lcPos = 4; lc < numLc; lc++, lcPos += 10)
						{
							int id = data[lcPos];
							string dst = "";
							int group = 0;

							if ((id < Lc.NumDestinations) && _LcMask[id])
							{
								dst = _CfgServer.GetEquivalentName(_Encoding.GetString(data, lcPos + 1, 8).Trim());
								group = (data[lcPos + 9] >= 0x80) ? (int)data[lcPos + 9] : 0;
							}

							lcConf.Info[lc] = new LcDestination(dst, group);
						}

						General.SafeLaunchEvent(LcPositionsEngine, this, lcConf);
					}
					break;

				case CfgEventType.TlfAll:
					if (data[4] < Tlf.NumDestinations)
					{
						int numTlf = Math.Min(data[3], Tlf.NumDestinations - data[4]);
						RangeMsg<TlfDestination> tlfConf = new RangeMsg<TlfDestination>(data[4], numTlf);

						for (int tlf = 0, tlfPos = 4; tlf < numTlf; tlf++, tlfPos += 10)
						{
							int id = data[tlfPos];
							string dst = "";

							if ((id < Tlf.NumDestinations) && _TlfMask[id])
							{
								dst = _CfgServer.GetEquivalentName(_Encoding.GetString(data, tlfPos + 1, 8).Trim());
							}
								
							tlfConf.Info[tlf] = new TlfDestination(dst);
						}

						General.SafeLaunchEvent(TlfPositionsEngine, this, tlfConf);
					}
					break;

				case CfgEventType.Sect:
					break;
				case CfgEventType.ActiveScv:
               _Cache[data[0] | (data[1] << 8)].Clear();
					_Cache[data[0] | (data[1] << 8)][data[2]] = data;

					if (data[2] == 0x53)		// == 'S'
					{
						General.SafeLaunchEvent(ActiveScvEngine, this, new ActiveScvMsg(data[3] - 0x41));
					}
					break;
			}
		}

		private TlfState GetTlfState(byte[] data)
		{
			TlfState st = TlfState.Idle;

			switch ((TlfStType)data[3])
			{
				case TlfStType.Idle:
					break;
				case TlfStType.Mem:
					st = TlfState.Mem;
					break;
				case TlfStType.RemoteMem:
					st = TlfState.RemoteMem;
					break;
				case TlfStType.In:
					st = TlfState.In;
					break;
				case TlfStType.RemoteIn:
					st = TlfState.RemoteIn;
					break;
				case TlfStType.Out:
					st = TlfState.Out;
					break;
				case TlfStType.Set:
					st = TlfState.Set;
					break;
				case TlfStType.Conf:
					st = TlfState.Conf;
					break;
				case TlfStType.Busy:
				case TlfStType.Blocked:
					st = TlfState.Busy;
					break;
				case TlfStType.PaPBusy:
					st = TlfState.PaPBusy;
					break;
				case TlfStType.Parked:
				case TlfStType.Hold:
				case TlfStType.ConfHold:
				case TlfStType.ParkedRemoteParked:
					st = TlfState.Hold;
					break;
				case TlfStType.RemoteParked:
					st = TlfState.RemoteHold;
					break;
				case TlfStType.Prio:
					st = TlfState.InPrio;
					break;
				case TlfStType.Congestion:
					st = TlfState.Congestion;
					break;
				case TlfStType.Unavailable:
					st = TlfState.Unavailable;
					break;
				case TlfStType.Pending:
					_Logger.Warn("Estado TlfStType no manejado: {0}", (TlfStType)data[3]);
					break;
				default:
					_Logger.Warn("Estado TlfStType desconocido: {0}", data[3]);
					break;
			}

			return st;
		}

		private RdAsignState GetRdAsignState(byte[] data, int offset)
		{
			RdAsignState asign = new RdAsignState();

			switch ((RdAsignType)data[offset])
			{
				case RdAsignType.Idle:
					break;
				case RdAsignType.Tx:
					asign.Tx = true;
					goto case RdAsignType.Rx;
				case RdAsignType.Rx:
					asign.Rx = true;
					switch ((RdRxAudioType)data[offset + 1])
					{
						case RdRxAudioType.HeadPhones:
							asign.AudioVia = RdRxAudioVia.HeadPhones;
							break;
						case RdRxAudioType.Speaker:
							asign.AudioVia = RdRxAudioVia.Speaker;
							break;
						default:
							_Logger.Warn("Estado RdRxAudioType no válido: {0}", data[offset + 1]);
							break;
					}
					break;
				default:
					_Logger.Warn("Estado RdAsignType desconocido: {0}", data[offset]);
					break;
			}

			return asign;
		}

		private string GetTlfIaNumber(byte[] data)
		{
			const byte PRF_LOCAL = 1;
			//const byte PRF_LCE	= 2;
			const byte PRF_ATSN = 3;
			//const byte PRF_RTB = 4;
			const byte PRF_PAP = 5;
			//const byte PRF_RED6 = 6;
			//const byte PRF_LCI	= 7;
			//const byte PRF_RED8 = 8;
			//const byte PRF_RED9 = 9;
			const byte NIBBLE_PAUSA = 14;

			string number = "";

			switch (data[12])
			{
				case 0:
				case PRF_PAP:
					break;
				case PRF_LOCAL:
					number = string.Format("01{0:D2}", data[14]);
					break;
				case PRF_ATSN:
					number = string.Format("03{0:D}", ((data[14] << 24) + (data[15] << 16) + (data[16] << 8) + data[17]));
					if (number.Length > 8)
					{
						number.Substring(0, 8);
					}
					break;
				default:
				//case PRF_RTB:
				//case PRF_RED6:
				//case PRF_RED8:
				//case PRF_RED9:
				//...
					uint len = Math.Min((uint)data[14], 18);
					if (len > 0)
					{
						byte[] digits = new byte[18];

						for (int i = 0; i * 2 < len; i++)
						{
							int nibbleAlto = data[15 + i] & 0x0F;
							int nibbleBajo = data[15 + i] >> 4;

							digits[i * 2] = (byte)((nibbleBajo == NIBBLE_PAUSA) ? 0x2C : (0x30 + nibbleBajo));
							if ((i * 2 + 1) < len)
							{
								digits[i * 2 + 1] = (byte)((nibbleAlto == NIBBLE_PAUSA) ? 0x2C : (0x30 + nibbleAlto));
							}
						}
						number = string.Format("{0:D2}", data[12]) + _Encoding.GetString(digits, 0, (int)len);
					}
					break;
			}

			return number;
		}

        internal void SendCmdHistoricalEvent(string user, string frec)
        {
            RadioAsgHist.SendCmdHistorico(Settings.Default.ManttoServerIp, Convert.ToInt32(Settings.Default.ManttoServerPort), user, frec, _ActualState, _OldState);
            _OldState = _ActualState;
        }
    }
}
