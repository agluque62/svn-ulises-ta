using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;
using Microsoft.Practices.CompositeUI;
using Microsoft.Practices.CompositeUI.EventBroker;
using Microsoft.Practices.ObjectBuilder;
using HMI.Model.Module.Messages;
using HMI.Model.Module.BusinessEntities;
using HMI.Model.Module.Services;
using HMI.OPE.Module.Constants;
using HMI.OPE.Module.BusinessEntities;
using HMI.OPE.Module.Properties;
using Utilities;
using NLog;

namespace HMI.OPE.Module.Services
{
	class OpeCmdManagerService : IEngineCmdManagerService
	{
		private static Logger _Logger = LogManager.GetCurrentClassLogger();

		private OpeCommunicationsService _Comm = null;

		[CreateNew]
		public OpeCommunicationsService Comm
		{
			get { return _Comm; }
			set { _Comm = value; }
		}

		public void Run()
		{
			_Comm.Run();
		}

		public void Stop()
		{
			_Comm.Stop();
		}

		#region IEngineCmdManagerService Members

		public string Name
		{
			get { return "Ope"; }
		}

		public void GetEngineInfo()
		{
			_Comm.Send(new byte[] { (byte)OpeEventType.Ctl, (byte)CtlEventType.OpeVersion, 0, (byte)_Comm.OpeId, 0, 0 });
			_Comm.Send(new byte[] { (byte)OpeEventType.Ctl, (byte)CtlEventType.IolVersion, 0, (byte)_Comm.IolId, 0, 0 });
			//			_Comm.Send(new byte[] { (byte)OpeEventType.Ctl, (byte)CtlEventType.OpeSwitchs, 0, 0 });
			_Logger.Debug("(Tx) Ctl.GetEngineInfo: [OpeId={0}] [IolId={1}]", _Comm.OpeId, _Comm.IolId);
		}

		public void SetSplitMode(SplitMode mode)
		{
			int split = (mode == SplitMode.Off ? 1 : (mode == SplitMode.RdLc ? 2 : 3));

			_Comm.Send(new byte[] { (byte)OpeEventType.Ctl, (byte)CtlEventType.SplitMode, (byte)split });
			_Logger.Debug("(Tx) Ctl: [SplitMode={0}]", mode);
		}

		public void SetBuzzerState(bool enabled)
		{
			_Comm.Send(new byte[] { (byte)OpeEventType.Tlf, (byte)(enabled ? TlfCmdType.BuzzerShortClick : TlfCmdType.BuzzerLongClick) });
			_Logger.Debug("(Tx) Tlf: [BuzzerEnabled={0}]", enabled);
		}

		public void SetBuzzerLevel(int level)
		{
			_Comm.Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.SetBuzzerLevel, (byte)level });
			_Logger.Debug("(Tx) Tlf: [BuzzerLevel={0}]", level);
		}

		public void SetRdHeadPhonesLevel(int level)
		{
			_Comm.Send(new byte[] { (byte)OpeEventType.Radio, (byte)RdCmdType.SetHeadPhonesLevel, (byte)level });
			_Logger.Debug("(Tx) Radio: [RdHeadPhonesLevel={0}]", level);
		}

		public void SetRdSpeakerLevel(int level)
		{
			_Comm.Send(new byte[] { (byte)OpeEventType.Radio, (byte)RdCmdType.SetSpeakerLevel, (byte)level });
			_Logger.Debug("(Tx) Radio: [RdSpeakerLevel={0}]", level);
		}

		public void SetRdPage(int oldPage, int newPage, int numPosByPage)
		{
			_Comm.Send(new byte[] { (byte)OpeEventType.Radio, (byte)RdCmdType.SetActivePage, (byte)(newPage + 1) });
			_Logger.Debug("(Tx) Radio: [RdPage={0}]", newPage);
		}

		public void SetRdPtt(bool on)
		{
			_Comm.Send(new byte[] { (byte)OpeEventType.Radio, (byte)(on ? RdCmdType.PttOn : RdCmdType.PttOff) });
			_Logger.Debug("(Tx) Radio: [PttOn={0}]", on);
		}

		public void SetRdRx(int id, bool on)
		{
			_Comm.Send(new byte[] { (byte)OpeEventType.Radio, (byte)(on ? RdCmdType.RxClick : RdCmdType.RxLongClick), (byte)(id + 1) });
			_Logger.Debug("(Tx) Radio: [Id={0}] [Rx={1}]", id, on);
		}

		public void SetRdTx(int id, bool on)
		{
			_Comm.Send(new byte[] { (byte)OpeEventType.Radio, (byte)RdCmdType.TxClick, (byte)(id + 1) });
			_Logger.Debug("(Tx) Radio: [Id={0}] [Tx={1}]", id, on);
		}

		public void ConfirmRdTx(int id)
		{
			_Comm.Send(new byte[] { (byte)OpeEventType.Radio, (byte)RdCmdType.ConfirmTx, (byte)(id + 1) });
			_Logger.Debug("(Tx) Radio: [Confirm Tx Id={0}]", id);
		}

		public void SetRdAudio(int id, RdRxAudioVia audio)
		{
			_Comm.Send(new byte[] { (byte)OpeEventType.Radio, (byte)RdCmdType.RxClick, (byte)(id + 1) });
			_Logger.Debug("(Tx) Radio: [Id={0}] [AudioVia={1}]", id, audio);
		}

		public void SetRtxGroup(int rtxGroup, Dictionary<int, RtxState> newRtxGroup)
		{
			_Comm.Send(new byte[] { (byte)OpeEventType.Radio, (byte)RdCmdType.SetRtxGroupBegin, (byte)rtxGroup });
			_Logger.Debug("(Tx) Radio: [RtxGroup={0}]", rtxGroup);

			foreach (KeyValuePair<int, RtxState> p in newRtxGroup)
			{
				if (p.Value == RtxState.Add)
				{
					_Comm.Send(new byte[] { (byte)OpeEventType.Radio, (byte)RdCmdType.AddToRtxGroup, (byte)rtxGroup, (byte)(p.Key + 1) });
					_Logger.Debug("(Tx) RtxGroup Add: [Id={0}]", p.Key);
				}
				else if (p.Value == RtxState.Delete)
				{
					_Comm.Send(new byte[] { (byte)OpeEventType.Radio, (byte)RdCmdType.DeleteFromRtxGroup, (byte)rtxGroup, (byte)(p.Key + 1) });
					_Logger.Debug("(Tx) RtxGroup Remove: [Id={0}]", p.Key);
				}
			}

			_Comm.Send(new byte[] { (byte)OpeEventType.Radio, (byte)RdCmdType.SetRtxGroupEnd });
			_Logger.Debug("(Tx) EndRtxGroup");
		}

		public void SetLc(int id, bool on)
		{
			_Comm.Send(new byte[] { (byte)OpeEventType.Lc, (byte)(on ? LcCmdType.MouseDown : LcCmdType.MouseUp), (byte)(id + 1) });
			_Logger.Debug("(Tx) Lc: [Id={0}] [Clicked={1}]", id, on);
		}

		public void SetTlfHeadPhonesLevel(int level)
		{
			_Comm.Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.SetHeadPhonesLevel, (byte)level });
			_Logger.Debug("(Tx) Tlf: [TlfHeadPhonesLevel={0}]", level);
		}

		public void SetLcSpeakerLevel(int level)
		{
			_Comm.Send(new byte[] { (byte)OpeEventType.Lc, (byte)LcCmdType.SetSpeakerLevel, (byte)level });
			_Logger.Debug("(Tx) Lc: [LcSpeakerLevel={0}]", level);
		}

		public void BeginTlfCall(int id, bool prio)
		{
			if (_Comm.AllowTlf())
			{
				Debug.Assert(id < Tlf.NumDestinations);

				_Comm.SetPriority(prio);
				_Comm.Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.DaShortClick, (byte)(id + 1) });

				_Logger.Debug("(Tx) Tlf.BeginDaCall: [Id={0}] [Prio={1}]", id, prio);
			}
		}

		public void BeginTlfCall(string number, bool prio)
		{
			if (_Comm.AllowTlf())
			{
				Debug.Assert(number.Length > 2);

				_Comm.SetPriority(prio);
				_Comm.SetIaNumber(number);

				byte[] data = new byte[2 + number.Length];
				data[0] = (byte)OpeEventType.Tlf;
				data[1] = (byte)TlfCmdType.NumberClick;
				data[2] = (byte)(number.Length - 1);
				data[3] = (byte)((((byte)number[0] - 0x30) * 10) + ((byte)number[1] - 0x30));

				for (int i = 2; i < number.Length; i++)
				{
					data[2 + i] = (byte)number[i];
				}

				_Comm.Send(data);
				_Logger.Debug("(Tx) Tlf.BeginTlfCall: [Number={0}] [Prio={1}]", number, prio);
			}
		}

		public void RetryTlfCall(int id)
		{
			if (_Comm.AllowTlf())
			{
				_Comm.SetPriority(true);
				_Logger.Debug("(Tx) Tlf.RetryTlfCall: [Id={0}]", id);
			}
		}

		public void AnswerTlfCall(int id)
		{
			if (_Comm.AllowTlf())
			{
				if (id < Tlf.NumDestinations)
				{
					_Comm.Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.DaShortClick, (byte)(id + 1) });
					_Logger.Debug("(Tx) Tlf.AnswerDaCall: [Id={0}]", id);
				}
				else
				{
					_Comm.Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.IaShortClick, (byte)(id - Tlf.NumDestinations + 1) });
					_Logger.Debug("(Tx) Tlf.AnswerIaCall: [Id={0}]", id - Tlf.NumDestinations);
				}
			}
		}

		public void EndTlfCall(int id, TlfState st)
		{
			if (_Comm.AllowTlf())
			{
				if (st == TlfState.Hold)
				{
					SetHold(id, false);
				}
				EndTlfCall(id);
			}
		}

		public void EndTlfCall(int id)
		{
			if (_Comm.AllowTlf())
			{
				if (id < Tlf.NumDestinations)
				{
					_Comm.Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.DaShortClick, (byte)(id + 1) });
					_Logger.Debug("(Tx) Tlf.EndDaCall: [Id={0}]", id);
				}
				else
				{
					_Comm.Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.IaShortClick, (byte)(id - Tlf.NumDestinations + 1) });
					_Logger.Debug("(Tx) Tlf.EndIaCall: [Id={0}]", id - Tlf.NumDestinations);
				}
			}
		}

		public void EndTlfConfCall(int id)
		{
			if (_Comm.AllowTlf())
			{
				if (id < Tlf.NumDestinations)
				{
					_Comm.Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.DaLongClick, (byte)(id + 1) });
					_Logger.Debug("(Tx) Tlf.EndDaConfCall: [Id={0}]", id);
				}
				else
				{
					_Comm.Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.IaLongClick, (byte)(id - Tlf.NumDestinations + 1) });
					_Logger.Debug("(Tx) Tlf.EndIaConfCall: [Id={0}]", id - Tlf.NumDestinations);
				}
			}
		}

		public void RecognizeTlfState(int id)
		{
			if (_Comm.AllowTlf())
			{
				if (id < Tlf.NumDestinations)
				{
					_Comm.Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.DaShortClick, (byte)(id + 1) });
					_Logger.Debug("(Tx) Tlf.RecognizeDaState: [Id={0}]", id);
				}
				else
				{
					_Comm.Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.IaShortClick, (byte)(id - Tlf.NumDestinations + 1) });
					_Logger.Debug("(Tx) Tlf.RecognizeIaState: [Id={0}]", id - Tlf.NumDestinations);
				}
			}
		}

		public void EndTlfConf()
		{
			if (_Comm.AllowTlf())
			{
				_Comm.Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.CancelClick });
				_Logger.Debug("(Tx) Tlf.EndTlfConf");
			}
		}
        public void EndTlfAll()
        {
            EndTlfConf();
        }

		//public void SetPriority(bool on)
		//{
		//   _Comm.Send(new byte[] { (byte)OpeEventType.Tlf, (byte)(on ? TlfCmdType.SetPriorityOn : TlfCmdType.SetPriorityOff) });
		//   _Logger.Debug("(Tx) Tlf: [PriorityOn={0}]", on);
		//}

		//public void RecognizePriorityState()
		//{
		//   _Comm.Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.SetPriorityOff });
		//   _Logger.Debug("(Tx) Tlf.RecognizePriorityState");
		//}

		public void ListenTo(int id)
		{
			if (_Comm.AllowTlf())
			{
				Debug.Assert(id < Tlf.NumDestinations);

				_Comm.Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.SetListenOn });
				_Comm.Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.DaShortClick, (byte)(id + 1) });
				_Logger.Debug("(Tx) Tlf: [ListenToDa={0}]", id);
			}
		}

		public void ListenTo(string number)
		{
			if (_Comm.AllowTlf())
			{
				Debug.Assert(number.Length > 2);

				_Comm.Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.SetListenOn });

				byte[] data = new byte[2 + number.Length];
				data[0] = (byte)OpeEventType.Tlf;
				data[1] = (byte)TlfCmdType.NumberClick;
				data[2] = (byte)(number.Length - 1);
				data[3] = (byte)((((byte)number[0] - 0x30) * 10) + ((byte)number[1] - 0x30));

				for (int i = 2; i < number.Length; i++)
				{
					data[2 + i] = (byte)number[i];
				}

				_Comm.Send(data);
				_Logger.Debug("(Tx) Tlf: [ListenTo={0}]", number);
			}
		}

		public void CancelListen()
		{
			if (_Comm.AllowTlf())
			{
				_Comm.Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.SetListenOff });
				_Logger.Debug("(Tx) Tlf.CancelListen");
			}
		}

		public void RecognizeListenState()
		{
			if (_Comm.AllowTlf())
			{
				_Comm.Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.SetListenOff });
				_Logger.Debug("(Tx) Tlf.RecognizeListenState");
			}
		}

		public void SetHold(int id, bool on)
		{
			if (_Comm.AllowTlf())
			{
				if (id < Tlf.NumDestinations)
				{
					if (on)
					{
						_Comm.Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.Hold });
					}
					else
					{
						_Comm.Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.DaShortClick, (byte)(id + 1) });
					}
					_Logger.Debug("(Tx) Tlf: [DaId={0}] [HoldOn={1}]", id, on);
				}
				else
				{
					if (on)
					{
						_Comm.Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.Hold });
					}
					else
					{
						_Comm.Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.IaShortClick, (byte)(id - Tlf.NumDestinations + 1) });
					}
					_Logger.Debug("(Tx) Tlf: [IaId={0}] [HoldOn={1}]", id - Tlf.NumDestinations, on);
				}
			}
		}

		public void TransferTo(int id, bool direct)
		{
			if (_Comm.AllowTlf())
			{
				if (direct)
				{
					_Comm.Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.DirectTransferClick });

					if (id < Tlf.NumDestinations)
					{
						_Comm.Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.DaShortClick, (byte)(id + 1) });
						_Logger.Debug("(Tx) Tlf: [DirectTransferToDa={0}]", id);
					}
					else
					{
						_Comm.Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.IaShortClick, (byte)(id - Tlf.NumDestinations + 1) });
						_Logger.Debug("(Tx) Tlf: [DirectTransferToIa={0}]", id - Tlf.NumDestinations);
					}
				}
				else
				{
					_Comm.Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.TransferClick });

					if (id < Tlf.NumDestinations)
					{
						_Comm.Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.TransferTo, (byte)TlfPosType.Ad, (byte)(id + 1) });
						_Logger.Debug("(Tx) Tlf: [TransferToDa={0}]", id);
					}
					else
					{
						_Comm.Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.TransferTo, (byte)TlfPosType.Ai, (byte)(id - Tlf.NumDestinations + 1) });
						_Logger.Debug("(Tx) Tlf: [TransferToIa={0}]", id - Tlf.NumDestinations);
					}
				}
			}
		}

		public void TransferTo(string number)
		{
			if (_Comm.AllowTlf())
			{
				Debug.Assert(number.Length > 2);

				_Comm.Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.DirectTransferClick });

				byte[] data = new byte[2 + number.Length];
				data[0] = (byte)OpeEventType.Tlf;
				data[1] = (byte)TlfCmdType.NumberClick;
				data[2] = (byte)(number.Length - 1);
				data[3] = (byte)((((byte)number[0] - 0x30) * 10) + ((byte)number[1] - 0x30));

				for (int i = 2; i < number.Length; i++)
				{
					data[2 + i] = (byte)number[i];
				}

				_Comm.Send(data);
				_Logger.Debug("(Tx) Tlf: [TransferTo={0}]", number);
			}
		}

		public void CancelTransfer()
		{
			if (_Comm.AllowTlf())
			{
				_Comm.Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.CancelClick });
				_Logger.Debug("(Tx) Tlf.CancelTransfer");
			}
		}

		public void RecognizeTransferState()
		{
			if (_Comm.AllowTlf())
			{
				_Comm.Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.CancelClick });
				_Logger.Debug("(Tx) Tlf.RecognizeTransferState");
			}
		}

		public void SetHangToneOff()
		{
			_Comm.Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.SetHangToneOff });
			_Logger.Debug("(Tx) Tlf.HangToneOff");
		}

		public void SendDigit(char ch)
		{
			_Comm.Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.Digit, (byte)ch });
			_Logger.Debug("(Tx) Tlf: [Digit={0}]", ch);
		}

		public void SetRemoteListen(bool allow, int id)
		{
			_Comm.Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.RemoteListenAnswer, (byte)(allow ? 1 : 0), (byte)(id) });
			_Logger.Debug("(Tx) Tlf: [SetRemoteListen={0}] [Id={1}]", allow, id);
		}

		public void Cancel()
		{
			_Comm.Send(new byte[] { (byte)OpeEventType.Tlf, (byte)TlfCmdType.CancelClick });
			_Logger.Debug("(Tx) Tlf.Cancel");
		}

		public void Wait(int ms)
		{
			_Comm.Send(new byte[] { (byte)OpeEventType.Int, (byte)IntCmdType.Wait, (byte)(ms / 10) });
			_Logger.Debug("(Tx) Tlf.Wait [Ms={0}]", ms);
		}

		public void SendTrapScreenSaver(bool on)
		{
		}

		public void ResetRdPosition(int id)
		{ 
		}

        public void BriefingFunction()
        {
        }

        public void MakeConference(bool viable)
        {
        }

        public bool HayConferencia()
        {
            return false;
        }

        public void SetHold(bool on)
        {
        }

        public void FunctionReplay(FunctionReplay funcion, ViaReplay via, string fileName, long fileLength)
        {
        }

        public void SetRdHfSpeakerLevel(int level)
        {
        }

        public void ForceTxOff(int id)
        {
        }

        public void SelCalPrepare(bool prepareOnOff, string code)
        {
        }

        public void SendCmdHistoricalEvent(string user, string frec)
        {
            _Comm.SendCmdHistoricalEvent(user, frec);
        }

        public void ChangingPositionSite(int id)
        {
        }
        #endregion
	}
}
