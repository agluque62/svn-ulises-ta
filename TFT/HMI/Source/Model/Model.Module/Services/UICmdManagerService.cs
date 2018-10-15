using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Threading;
using Microsoft.Practices.CompositeUI;
using Microsoft.Practices.CompositeUI.EventBroker;
using HMI.Infrastructure.Interface;
using HMI.Model.Module.Messages;
using HMI.Model.Module.Properties;
using HMI.Model.Module.Constants;
using HMI.Model.Module.BusinessEntities;
using Utilities;

namespace HMI.Model.Module.Services
{
	public class UICmdManagerService : IModelCmdManagerService
	{
		private StateManagerService _StateManager = null;
		private IEngineCmdManagerService _EngineCmdManager = null;
		private bool _RdButtonClick = false;
        private List<int> idList = null;//Ids de los botones cambiados en escucha a otro puesto

		[EventPublication(EventTopicNames.SplitShowModeSelectionUI, PublicationScope.Global)]
		public event EventHandler SplitShowModeSelectionUI;

        [EventPublication(EventTopicNames.ShowInfoUI, PublicationScope.Global)]
        public event EventHandler ShowInfoUI;

        [EventPublication(EventTopicNames.SwitchTlfViewUI, PublicationScope.Global)]
        public event EventHandler<EventArgs<string>> SwitchTlfViewUI;

        [EventPublication(EventTopicNames.BriefingSessionUI, PublicationScope.Global)]
        public event EventHandler BriefingSessionUI;

        [EventPublication(EventTopicNames.LoadTlfDaPageUI, PublicationScope.Global)]
		public event EventHandler<PageMsg> LoadTlfDaPageUI;

        [EventPublication(EventTopicNames.ReplayUI, PublicationScope.Global)]
        public event EventHandler ReplayUI;

        [EventPublication(EventTopicNames.DeleteSessionGlp, PublicationScope.Global)]
        public event EventHandler DeleteSessionGlp;

        public UICmdManagerService([ServiceDependency] StateManagerService stateManager, [ServiceDependency] IEngineCmdManagerService engineCmdManager)
		{
			_StateManager = stateManager;
			_EngineCmdManager = engineCmdManager;
		}

		#region IModelCmdManagerService Members

		public void DisableTft()
		{
			if (_StateManager.Tft.Enabled)
			{
				_StateManager.Radio.SetRtx(0,0);
				if (_StateManager.Tlf.Priority.State == PriorityState.Ready)
				{
					_StateManager.Tlf.Priority.Reset();
				}
				if (_StateManager.Tlf.Listen.State == ListenState.Ready)
				{
					_StateManager.Tlf.Listen.State = ListenState.Idle;
				}
				if (_StateManager.Tlf.Transfer.State == TransferState.Ready)
				{
					_StateManager.Tlf.Transfer.State = TransferState.Idle;
				}

				_StateManager.Tft.Enabled = false;
			}
            else
            {
                if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Control) == System.Windows.Forms.Keys.Control)
                {
                    _StateManager.Tft.Enabled = true;
                }
            }
        }

		public void MessageResponse(NotifMsg msg, NotifMsgResponse response)
		{
			_StateManager.HideUIMessage(msg.Id);

			if (msg.Id.StartsWith("ListenBy"))
			{
				ListenMsg info = (ListenMsg)msg.Info;

				_EngineCmdManager.SetRemoteListen(response == NotifMsgResponse.Ok || response == NotifMsgResponse.Timeout, info.Id);
                if (response == NotifMsgResponse.Ok || response == NotifMsgResponse.Timeout)
				{
					_StateManager.Tlf.ListenBy.Reset(info);
				}
			}
			else if (msg.Id == "RdFrAsignedToOtherConfirmation")
			{
				if (response == NotifMsgResponse.Ok)
				{
					RdFrAsignedToOtherMsg info = (RdFrAsignedToOtherMsg)msg.Info;
					_EngineCmdManager.ConfirmRdTx(info.RdId);
				}
			}
            else if (msg.Id == Resources.BriefingFunction)
            {
                if (response == NotifMsgResponse.Ok)
                {
                    _EngineCmdManager.BriefingFunction();
                }
            }
            else if (msg.Id == Resources.DeleteSessionGLP)
            {
                if (response == NotifMsgResponse.Ok)
                {
                    General.SafeLaunchEvent(DeleteSessionGlp, this);
                }
            }
		}

		public void ShowSplitModeSelection()
		{
			General.SafeLaunchEvent(SplitShowModeSelectionUI, this);
		}

		public void SetSplitMode(SplitMode mode)
		{
			Debug.Assert(mode != _StateManager.Split.Mode);
			_EngineCmdManager.SetSplitMode(mode);
		}

		public void ShowInfo()
		{
			General.SafeLaunchEvent(ShowInfoUI, this);
		}

        public void BriefingFunction()
        {
            if (_StateManager.Tft.Briefing)
                _EngineCmdManager.BriefingFunction();   // Parar Briefing
            else
            {                                           // Solicitar confirmación para iniciar Briefing
                if (!_StateManager.Tft.Playing)
                {
                    // Solicitar confirmación para iniciar Briefing
                    NotifMsg msg = new NotifMsg(Resources.BriefingFunction, Resources.MessageInfoCaption, Resources.BriefingFunction, 0, MessageType.Information, MessageButtons.OkCancel);
                    _StateManager.ShowUIMessage(msg);
                }
                else
                {
                    General.SafeLaunchEvent(BriefingSessionUI, this);
                }
            }
        }

		public void SetBrightnessLevel(int level)
		{
			_StateManager.Brightness.Level = level;
		}

		public void SetBuzzerState(bool enabled)
		{
			if (_StateManager.Buzzer.Enabled != enabled)
			{
				_EngineCmdManager.SetBuzzerState(enabled);
			}
		}

		public void SetBuzzerLevel(int level)
		{
			if (!_StateManager.Buzzer.Enabled)
			{
				_EngineCmdManager.SetBuzzerState(true);
			}
			else
			{
				_EngineCmdManager.SetBuzzerLevel(level);
			}
		}

		public void SwitchTlfView(string view)
		{
            //if (!_StateManager.Tft.Briefing)
                General.SafeLaunchEvent(SwitchTlfViewUI, this, new EventArgs<string>(view));
            //else
            //    General.SafeLaunchEvent(BriefingSessionUI, this);
		}

        public void RdSetSpeakerLevel(int level)
        {
            // Si existen posiciones no desasignables en el puesto, el nivel de 
            // volumen del altavoz radio no puede bajar del 30% del máximo.
            if (_StateManager.Radio.RadioMonitoring && level < 3)
                return;

            _EngineCmdManager.SetRdSpeakerLevel(level);
        }

        public void RdSetHfSpeakerLevel(int level)
        {
            _EngineCmdManager.SetRdHfSpeakerLevel(level);
        }

        public void RdSetHeadPhonesLevel(int level)
		{
			_EngineCmdManager.SetRdHeadPhonesLevel(level);
		}

		public void RdSetPtt(bool on)
		{
			_EngineCmdManager.SetRdPtt(on);
		}

        public void RdSiteManagerClick()
        {
            _StateManager.ManagingSite = !_StateManager.ManagingSite;
            _EngineCmdManager.SetManagingSite(_StateManager.ManagingSite);
            if (!_StateManager.ManagingSite)
            {
                for (int i = 0; i < Radio.NumDestinations; i++)
                {
                    if (_StateManager.Radio.ChangingSite(i))
                    {
                        _EngineCmdManager.ChangeSite(i, _StateManager.Radio.GetTmpAlias(i));
                        Thread.Sleep(50);
                    }
                }
            }
        }

		public void RdRtxClick(int numPositionsByPage)
		{
			if (!_StateManager.Radio.PttOn)
			{
				int tempRtxGroup = _StateManager.Radio.Rtx;

				if (tempRtxGroup == 0)
				{
					_RdButtonClick = false;
				}

				if (!_RdButtonClick)
				{
					_StateManager.Radio.SetRtx((tempRtxGroup + 1) % (Settings.Default.MaxRtxGroups + 1), numPositionsByPage);
				}
				else
				{
					Dictionary<int, RtxState> newRtxGroup = _StateManager.Radio.SetRtx(0, numPositionsByPage);

					if (newRtxGroup.Count > 0)
					{
						int numElementsInRxtGroup = 0;
						int numElementsToAdd = 0;
						int notChangedPos = 0;

						foreach (KeyValuePair<int, RtxState> p in newRtxGroup)
						{
							switch (p.Value)
							{
								case RtxState.Add:
									numElementsInRxtGroup++;
									numElementsToAdd++;
									break;
								case RtxState.NoChanged:
									numElementsInRxtGroup++;
									notChangedPos = p.Key;
									break;
							}
						}

						if (numElementsInRxtGroup == 1)
						{
							if (numElementsToAdd > 0)
							{
								NotifMsg msg = new NotifMsg(Resources.RtxNeedMoreFrecuenties, Resources.MessageErrorCaption, Resources.RtxNeedMoreFrecuenties, Settings.Default.MessageToutSg * 1000, MessageType.Error, MessageButtons.Ok);
								_StateManager.ShowUIMessage(msg);
							}
							else
							{
								newRtxGroup[notChangedPos] = RtxState.Delete;
								_EngineCmdManager.SetRtxGroup(tempRtxGroup, newRtxGroup);

                                //No es necesaria esta ventana de notificación porque el procesamiento es muy rápido y no se llega a leer.
                                //NotifMsg msg = new NotifMsg(Resources.RtxOperationRunning, Resources.MessageInfoCaption, Resources.RtxOperationRunning, 10000, MessageType.Processing, MessageButtons.None);
                                //_StateManager.ShowUIMessage(msg);
							}
						}
						else
						{
							_EngineCmdManager.SetRtxGroup(tempRtxGroup, newRtxGroup);

                            //No es necesaria esta ventana de notificación porque el procesamiento es muy rápido y no se llega a leer.
                            //NotifMsg msg = new NotifMsg(Resources.RtxOperationRunning, Resources.MessageInfoCaption, Resources.RtxOperationRunning, 10000, MessageType.Processing, MessageButtons.None);
                            //_StateManager.ShowUIMessage(msg);
						}
					}
				}
			}
		}

		public void RdLoadNextPage(int oldPage, int numPosByPage)
		{
			int numPages = (Radio.NumDestinations + numPosByPage - 1) / numPosByPage;
			int newPage = (oldPage + 1) % numPages;

			while (newPage != oldPage) 
			{
				for (int i = newPage * numPosByPage, to = Math.Min(Radio.NumDestinations, (newPage + 1) * numPosByPage); i < to; i++)
				{
					if (_StateManager.Radio[i].IsConfigurated)
					{
						_EngineCmdManager.SetRdPage(oldPage, newPage, numPosByPage);
						return;
					}
				}

				newPage = (newPage + 1) % numPages;
			}
		}

		public void RdLoadPrevPage(int oldPage, int numPosByPage)
		{
			int numPages = (Radio.NumDestinations + numPosByPage - 1) / numPosByPage;
			int newPage = (oldPage + numPages - 1) % numPages;

			while (newPage != oldPage)
			{
				for (int i = newPage * numPosByPage, to = Math.Min(Radio.NumDestinations, (newPage + 1) * numPosByPage); i < to; i++)
				{
					if (_StateManager.Radio[i].IsConfigurated)
					{
						_EngineCmdManager.SetRdPage(oldPage, newPage, numPosByPage);
						return;
					}
				}

				newPage = (newPage + numPages - 1) % numPages;
			}
		}

        public void ChangeSite(int id)
        {
            string alias = _EngineCmdManager.ChangingPositionSite(id);
            _StateManager.Radio.ChangingPositionSite(id,alias);
        }

		public void RdSwitchRtxState(int id)
		{
			_RdButtonClick = true;

			NotifMsg msg = _StateManager.Radio.SwitchTempGroupIfRtxOn(id, _StateManager.Jacks.SomeJack);
			if (msg != null)
			{
				_StateManager.ShowUIMessage(msg);
			}
		}

        public void RdSwitchTxState(int id)
        {
            RdDst dst = _StateManager.Radio[id];
            Debug.Assert((_StateManager.Radio.Rtx == 0) || !dst.Tx || ((dst.RtxGroup != 0) && (dst.RtxGroup != _StateManager.Radio.Rtx)));

            _EngineCmdManager.SetRdTx(id, !dst.Tx);
        }

        public void RdForceTxOff(int id)
        {
            RdDst dst = _StateManager.Radio[id];
            Debug.Assert((_StateManager.Radio.Rtx == 0) || !dst.Tx || ((dst.RtxGroup != 0) && (dst.RtxGroup != _StateManager.Radio.Rtx)));

            _EngineCmdManager.ForceTxOff(id);
        }


        public void RdConfirmTxState(int id)
        {
            RdDst dst = _StateManager.Radio[id];
            Debug.Assert((_StateManager.Radio.Rtx == 0) || !dst.Tx || ((dst.RtxGroup != 0) && (dst.RtxGroup != _StateManager.Radio.Rtx)));

            _EngineCmdManager.ConfirmRdTx(id);
        }

        public void RdConfirmRxAudio(int id, RdRxAudioVia via)
        {
            RdDst dst = _StateManager.Radio[id];
            Debug.Assert((_StateManager.Radio.Rtx == 0) || !dst.Tx || ((dst.RtxGroup != 0) && (dst.RtxGroup != _StateManager.Radio.Rtx)));

            _EngineCmdManager.SetRdAudio(id, via);
        }

        /* VMG 04/09/2018 */
        ///<summary>
        /// Cambiar el estado a altavoz con escucha a otro puesto.
        ///</summary>
        public void RdSwitchRxToSpeaker()
        {
            int id = 0;
            idList = new List<int>();

            foreach (RdDst dst in _StateManager.Radio.Destinations)
            {
                if (dst.AudioVia == RdRxAudioVia.HeadPhones)
                {
                    RdForceRxState(id);
                    idList.Add(id);
                }
                id++;
            }
        }

        /* VMG 04/09/2018 */
        ///<summary>
        /// Cambiar el estado a micro cuando se termina la escucha.
        ///</summary>
        public void RdSwitchRxToHeadphone()
        {
            foreach (int id in idList)
                RdForceRxState(id);
            idList.Clear();
        }

        public void RdSwitchRxState(int id, bool longClick)
        {
            RdDst dst = _StateManager.Radio[id];
            Debug.Assert((_StateManager.Radio.Rtx == 0) || !dst.Tx || ((dst.RtxGroup != 0) && (dst.RtxGroup != _StateManager.Radio.Rtx)));

            if (!dst.Rx || dst.Monitoring)
            {
                _EngineCmdManager.SetRdRx(id, true);
            }
            else if ((dst.Ptt != PttState.NoPtt && dst.Ptt != PttState.ExternPtt) || 
                longClick)
            {
                _EngineCmdManager.SetRdRx(id, false);
            }
            else
            {
                _EngineCmdManager.NextRdAudio(id);
            }
        }

        public void RdForceRxState(int id)
        {
            RdDst dst = _StateManager.Radio[id];
            Debug.Assert((_StateManager.Radio.Rtx == 0) || !dst.Tx || ((dst.RtxGroup != 0) && (dst.RtxGroup != _StateManager.Radio.Rtx)));

            if (!dst.Rx)
            {
                _EngineCmdManager.SetRdRx(id, true, true);
            }
            else if ((!dst.Tx && (dst.AudioVia == RdRxAudioVia.HeadPhones)))
            {
                _EngineCmdManager.SetRdRx(id, false, true);
            }
            else
            {
                RdRxAudioVia audio;

                if (dst.TipoFrecuencia == TipoFrecuencia_t.HF)
                {
                    if (dst.AudioVia == RdRxAudioVia.HeadPhones)
                        audio = RdRxAudioVia.Speaker;
                    else if (dst.AudioVia == RdRxAudioVia.Speaker)
                        audio = RdRxAudioVia.HfSpeaker;
                    else
                        audio = RdRxAudioVia.HeadPhones;
                }
                else
                {
                    audio = dst.AudioVia == RdRxAudioVia.HeadPhones ? RdRxAudioVia.Speaker : RdRxAudioVia.HeadPhones;
                }
                _EngineCmdManager.SetRdAudio(id, audio, true);
            }
        }

        public void LcSet(int id, bool on)
		{
			LcDst dst = _StateManager.Lc[id];

			if (on && (dst.Rx == LcRxState.Mem))
			{
				_StateManager.Lc.ResetMem(id);
			}
			else
			{
				_EngineCmdManager.SetLc(id, on);
			}
		}

		public void LcSetSpeakerLevel(int level)
		{
			_EngineCmdManager.SetLcSpeakerLevel(level);
		}

		public void TlfSetHeadPhonesLevel(int level)
		{
			_EngineCmdManager.SetTlfHeadPhonesLevel(level);
		}

        public void TlfSetSpeakerLevel(int level)
        {
            _EngineCmdManager.SetTlfSpeakerLevel(level);
        }

        public void TlfLoadDaPage(int page)
		{
			General.SafeLaunchEvent(LoadTlfDaPageUI, this, new PageMsg(page));
		}

		public void TlfClick(int id)
		{
			switch (_StateManager.Tlf[id].State)
			{
				case TlfState.Idle:
				case TlfState.PaPBusy:
					if (id < Tlf.NumDestinations)
					{
						if (_StateManager.Tlf.Listen.State == ListenState.Ready)
						{
							_EngineCmdManager.ListenTo(id);
						}
						else if (_StateManager.Tlf.Transfer.State == TransferState.Ready)
						{
							_EngineCmdManager.TransferTo(id, true);
						}
						else
						{
							_EngineCmdManager.BeginTlfCall(id, _StateManager.Tlf.Priority.NewCall(id));
						}
					}
					else
					{
						TlfClick(_StateManager.Tlf[id].Number, false, null, id);
					}
					break;
				case TlfState.In:
				case TlfState.InPrio:
				case TlfState.RemoteIn:
					_EngineCmdManager.AnswerTlfCall(id);
					break;
				case TlfState.Out:
				case TlfState.Set:
				case TlfState.RemoteHold:
				case TlfState.Busy:
				case TlfState.Congestion:
				case TlfState.OutOfService:
				case TlfState.NotAllowed:
					_EngineCmdManager.EndTlfCall(id);
					if (_StateManager.Tlf.Priority.AssociatePosition == id)
					{
						_StateManager.Tlf.Priority.Reset();
					}
					break;
				case TlfState.Conf:
					_EngineCmdManager.EndTlfConfCall(id);
					break;
				case TlfState.Mem:
				case TlfState.RemoteMem:
					_StateManager.Tlf.ResetMem(id);
					break;
				case TlfState.Hold:
					if (_StateManager.Tlf.Transfer.State == TransferState.Ready)
					{
						_EngineCmdManager.TransferTo(id, false);
					}
                    else if (_EngineCmdManager.HayConferencia())
                    {
                        _EngineCmdManager.SetHold(false);
                    }
                    else
					{
						_EngineCmdManager.SetHold(id, false);
					}
					break;
			}
		}

		public void TlfClick(string number, string literal)
		{
			TlfClick(number, true, literal);
		}

		public void PriorityClick()
		{
			switch (_StateManager.Tlf.Priority.State)
			{
				case PriorityState.Idle:
					if (_StateManager.Tlf[TlfState.Congestion] + _StateManager.Tlf[TlfState.Busy] > 0)
					{
						int id = _StateManager.Tlf.GetFirstInState(TlfState.Congestion, TlfState.Busy);

						_EngineCmdManager.RetryTlfCall(id);
						_StateManager.Tlf.Priority.Reset(id);
					}
					else
					{
						_StateManager.Tlf.Priority.Reset(-1);
					}
					break;
				case PriorityState.Ready:
				case PriorityState.Executing:
				case PriorityState.Error:
					_StateManager.Tlf.Priority.Reset();
					break;
			}
		}

		public void ListenClick()
		{
			switch (_StateManager.Tlf.Listen.State)
			{
				case ListenState.Idle:
					_StateManager.Tlf.Listen.State = ListenState.Ready;
					break;
				case ListenState.Ready:
					_StateManager.Tlf.Listen.State = ListenState.Idle;
					break;
				case ListenState.Executing:
					_EngineCmdManager.CancelListen();
					break;
				case ListenState.Error:
					_EngineCmdManager.RecognizeListenState();
					break;
			}
		}

		public void HoldClick()
		{
            if (_EngineCmdManager.HayConferencia())
            {
                _EngineCmdManager.SetHold(true);
            }
            else
            {
                int id = _StateManager.Tlf.GetFirstInState(TlfState.Set, TlfState.Conf, TlfState.RemoteHold);

                if (id >= 0)
                {
                    _EngineCmdManager.SetHold(id, true);
                }
            }
		}

		public void TransferClick()
		{
			switch (_StateManager.Tlf.Transfer.State)
			{
				case TransferState.Idle:
					_StateManager.Tlf.Transfer.State = TransferState.Ready;
					break;
				case TransferState.Ready:
					_StateManager.Tlf.Transfer.State = TransferState.Idle;
					break;
				case TransferState.Executing:
					_EngineCmdManager.CancelTransfer();
					break;
				case TransferState.Error:
					_EngineCmdManager.RecognizeTransferState();
					break;
			}
		}

        public void ConferenceClick()
        {
            if (!_EngineCmdManager.HayConferencia())
            {
                if (_StateManager.Tlf[TlfState.Hold] + _StateManager.Tlf[TlfState.Set] <= Settings.Default.NumMaxInConference - 1)
                    _EngineCmdManager.MakeConference(true);
                else
                    _EngineCmdManager.MakeConference(false);
            }
            else
                _EngineCmdManager.EndTlfConf();
        }

		public void CancelTlfClick()
		{
			if (_StateManager.Tlf.Priority.State == PriorityState.Error)
			{
				_StateManager.Tlf.Priority.Reset();
			}
			else if (_StateManager.Tlf.Listen.State == ListenState.Error)
			{
				_EngineCmdManager.RecognizeListenState();
			}
			else if (_StateManager.Tlf.Transfer.State == TransferState.Error)
			{
				_EngineCmdManager.RecognizeTransferState();
			}
			else if (_StateManager.Tlf.Priority.State == PriorityState.Ready)
			{
				_StateManager.Tlf.Priority.Reset();
			}
			else if (_StateManager.Tlf.Listen.State == ListenState.Ready)
			{
				_StateManager.Tlf.Listen.State = ListenState.Idle;
			}
			else if (_StateManager.Tlf.Transfer.State == TransferState.Ready)
			{
				_StateManager.Tlf.Transfer.State = TransferState.Idle;
			}
			else if (_StateManager.Tlf.Transfer.State == TransferState.Executing)
			{
				_EngineCmdManager.CancelTransfer();
			}
			else if ((_StateManager.Tlf[TlfState.RemoteIn] > 0) && Settings.Default.SupportInTlfCancel)
			{
				int id = _StateManager.Tlf.GetFirstInState(TlfState.RemoteIn);

				_EngineCmdManager.EndTlfCall(id);
				if (_StateManager.Tlf.Priority.AssociatePosition == id)
				{
					_StateManager.Tlf.Priority.Reset();
				}
			}
			else if ((_StateManager.Tlf[TlfState.In] > 0) && Settings.Default.SupportInTlfCancel)
			{
				int id = _StateManager.Tlf.GetFirstInState(TlfState.In);

				_EngineCmdManager.EndTlfCall(id);
				if (_StateManager.Tlf.Priority.AssociatePosition == id)
				{
					_StateManager.Tlf.Priority.Reset();
				}
			}
			else if ((_StateManager.Tlf[TlfState.InPrio] > 0) && Settings.Default.SupportInTlfCancel)
			{
				int id = _StateManager.Tlf.GetFirstInState(TlfState.InPrio);

				_EngineCmdManager.EndTlfCall(id);
				if (_StateManager.Tlf.Priority.AssociatePosition == id)
				{
					_StateManager.Tlf.Priority.Reset();
				}
			}
			else if (_StateManager.Tlf[TlfState.Conf] > 0)
			{
				_EngineCmdManager.EndTlfAll();
			}
			else if (_StateManager.Tlf[TlfState.Out] + _StateManager.Tlf[TlfState.NotAllowed] > 0)
			{
				int id = _StateManager.Tlf.GetFirstInState(TlfState.Out, TlfState.NotAllowed);

				_EngineCmdManager.EndTlfCall(id);
				if (_StateManager.Tlf.Priority.AssociatePosition == id)
				{
					_StateManager.Tlf.Priority.Reset();
				}
			}
			else if (_StateManager.Tlf[TlfState.Set] + _StateManager.Tlf[TlfState.RemoteHold] +
                _StateManager.Tlf[TlfState.Busy] + _StateManager.Tlf[TlfState.Congestion] + _StateManager.Tlf[TlfState.OutOfService] > 0)
			{
                int id = _StateManager.Tlf.GetFirstInState(TlfState.Set, TlfState.RemoteHold, TlfState.Busy, TlfState.Congestion, TlfState.OutOfService);
				_EngineCmdManager.EndTlfCall(id);
			}
			else if (_StateManager.Tlf[TlfState.Mem] + _StateManager.Tlf[TlfState.RemoteMem] > 0)
			{
				int id = _StateManager.Tlf.GetFirstInState(TlfState.Mem, TlfState.RemoteMem, TlfState.NotAllowed);
				//if (id == Tlf.IaMappedPosition)
					_StateManager.Tlf.ResetMem(id);
				//else
				//	_StateManager.Tlf.Reset(new RangeMsg<TlfState>(id, TlfState.Idle));
			}
			else if (_StateManager.Tlf.HangTone.On)
			{
				_EngineCmdManager.SetHangToneOff();
			}
			else if (_StateManager.Tlf[TlfState.Hold] > 0)
			{
				// No hacemos nada
			}
            else if (_StateManager.Tlf.ListenBy.IsListen)
            {
                // No hacemos nada
            }
			else
			{
				_EngineCmdManager.Cancel();
			}
		}

		public void NewDigit(int id, char key)
		{
			_EngineCmdManager.SendDigit(key);
			_StateManager.Tlf[id].Digits += key;
		}

        public void ReplayClick()
        {
            General.SafeLaunchEvent(ReplayUI, this);
        }

        public void FunctionReplay(FunctionReplay function, ViaReplay via, string fileName, long fileLength)
        {
            if (function != BusinessEntities.FunctionReplay.Erase)
                _EngineCmdManager.FunctionReplay(function, via, fileName, fileLength);
            else
            {
                NotifMsg msg = new NotifMsg(Resources.DeleteSessionGLP, Resources.MessageInfoCaption, Resources.DeleteSessionGLP, 0, MessageType.Information, MessageButtons.OkCancel);
                _StateManager.ShowUIMessage(msg);
            }
        }


        public void RdPrepareSelCal(bool prepareOn, string code)
        {
            _EngineCmdManager.SelCalPrepare(prepareOn, code);
        }

        public void SpeakerTlfClick()
        {
                bool oldState = _StateManager.Tlf.AltavozTlfEstado;
                // si no hay altavoz, no se permite el cambio a altavoz
                if ((_StateManager.LcSpeaker.Presencia) || (oldState == true))
                {
                    if (_EngineCmdManager.SetAudioViaTlf(!_StateManager.Tlf.AltavozTlfEstado))
                        _StateManager.Tlf.AltavozTlfEstado = !_StateManager.Tlf.AltavozTlfEstado;
                }
        }

		#endregion

        private void TlfClick(string number, bool ia, string givenLiteral = null, int id = Int32.MaxValue)
        {
            string literal = null;
            if (number.Length > 0)
            {
                number = Tlf.NumberToEngine(number);

                if (_StateManager.Tlf.Listen.State == ListenState.Ready)
                {
                    _EngineCmdManager.ListenTo(number);
                }
                else if (_StateManager.Tlf.Transfer.State == TransferState.Ready)
                {
                    _EngineCmdManager.TransferTo(number);
                }
                else
                {
                    bool wait = false;

                    for (int i = 0; i < Tlf.NumDestinations + Tlf.NumIaDestinations; i++)
                    {
                        TlfDst dst = _StateManager.Tlf[i];
                        literal = dst.Dst;

                        switch (dst.State)
                        {
                            case TlfState.Hold:
                                //if (i == Tlf.IaMappedPosition)
                                //{
                                //    _EngineCmdManager.EndTlfCall(i, TlfState.Hold);
                                //    wait = true;
                                //}
                                break;
                            case TlfState.Conf:
                                _EngineCmdManager.EndTlfConfCall(i);
                                wait = true;
                                break;
                            case TlfState.Out:
                            case TlfState.Set:
                            case TlfState.RemoteHold:
                            case TlfState.Congestion:
                            case TlfState.OutOfService:
                            case TlfState.Busy:
                                _EngineCmdManager.EndTlfCall(i);
                                wait = true;
                                break;
                        }
                    }

                    if (wait && (_EngineCmdManager.Name == "Ope"))
                    {
                        _EngineCmdManager.Wait(500);
                    }
                    if (id != Int32.MaxValue)
                        _EngineCmdManager.BeginTlfCall(number, _StateManager.Tlf.Priority.NewCall(Tlf.IaMappedPosition), id, literal);
                    else
                        _EngineCmdManager.BeginTlfCall(number, _StateManager.Tlf.Priority.NewCall(Tlf.IaMappedPosition), givenLiteral);
                    _StateManager.Tlf.Unhang.NewCall(ia);
                }
            }
        }

        public void SendCmdHistoricalEvent(string user, string frec)
        {
            _EngineCmdManager.SendCmdHistoricalEvent(user, frec);
        }
    }
}
