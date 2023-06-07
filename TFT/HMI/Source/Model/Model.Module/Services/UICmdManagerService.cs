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
using System.IO;

namespace HMI.Model.Module.Services
{
	public class changefr : EventArgs
	{
		public string pantalla { get; set; }
		public int id { get; set; }
		public string frecuencias { get; set; }
		public List<string> listafrecuencias;
	}

	public class UICmdManagerService : IModelCmdManagerService
	{
		private StateManagerService _StateManager = null;
		private IEngineCmdManagerService _EngineCmdManager = null;
		private bool _RdButtonClick = false;

		[EventPublication(EventTopicNames.SplitShowModeSelectionUI, PublicationScope.Global)]
		public event EventHandler SplitShowModeSelectionUI;

        [EventPublication(EventTopicNames.ShowInfoUI, PublicationScope.Global)]
        public event EventHandler ShowInfoUI;

		[EventPublication(EventTopicNames.SwitchTlfViewUI, PublicationScope.Global)]
		public event EventHandler<EventArgs<string>> SwitchTlfViewUI;
		
		[EventPublication(EventTopicNames.SwitchRadViewUI, PublicationScope.Global)]
		public event EventHandler<EventArgs<changefr>> SwitchRadViewUI;

		[EventPublication(EventTopicNames.BriefingSessionUI, PublicationScope.Global)]
        public event EventHandler BriefingSessionUI;

        [EventPublication(EventTopicNames.LoadTlfDaPageUI, PublicationScope.Global)]
		public event EventHandler<PageMsg> LoadTlfDaPageUI;

        [EventPublication(EventTopicNames.ReplayUI, PublicationScope.Global)]
        public event EventHandler ReplayUI;

        [EventPublication(EventTopicNames.DeleteSessionGlp, PublicationScope.Global)]
        public event EventHandler DeleteSessionGlp;

		//LALM 210224 Errores #4755 confirmación de cambio de página radio
		[EventPublication(EventTopicNames.CambioPaginaRadioUp, PublicationScope.Global)]
		public event EventHandler CambioPaginaRadioUp;
		[EventPublication(EventTopicNames.CambioPaginaRadioDown, PublicationScope.Global)]
		public event EventHandler CambioPaginaRadioDown;
		[EventPublication(EventTopicNames.PlayRadio, PublicationScope.Global)]
		public event EventHandler PlayRadio;

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
				if (_StateManager.Tlf.Priority.State == FunctionState.Ready)
				{
					_StateManager.Tlf.Priority.Reset();
				}
				if (_StateManager.Tlf.Listen.State == FunctionState.Ready)
				{
					_StateManager.Tlf.Listen.State = FunctionState.Idle;
				}
				if (_StateManager.Tlf.Transfer.State == FunctionState.Ready)
				{
					_StateManager.Tlf.Transfer.State = FunctionState.Idle;
				}
                if (_StateManager.Tlf.PickUp.State == FunctionState.Ready)
                {
                    _StateManager.Tlf.PickUp.State = FunctionState.Idle;
                }
                if (_StateManager.Tlf.Forward.State == FunctionState.Ready)
                {
                    _StateManager.Tlf.Forward.State = FunctionState.Idle;
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
				ListenPickUpMsg info = (ListenPickUpMsg)msg.Info;

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
			else if (msg.Id == "RdFrAsignedRtxToOtherConfirmation")
			{
				if (response == NotifMsgResponse.Ok)
				{
					RdFrAsignedToOtherMsg info = (RdFrAsignedToOtherMsg)msg.Info;
					_EngineCmdManager.ConfirmRdTx(info.RdId);//LALM220328 
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
			//LALM 210224 Errores #4755 confirmación de cambio de página radio
			else if (msg.Id == "Cambio de Página de Radio")
			{
				if (response == NotifMsgResponse.Ok)
				{
					if ((bool)msg.Info==true)
						General.SafeLaunchEvent(CambioPaginaRadioUp, this);
					else
						General.SafeLaunchEvent(CambioPaginaRadioDown, this);
					_StateManager.Radio.pagina_confirmada = true;
				}
				else
				{
					_StateManager.Radio.pagina_confirmada = false;
					// se queda igual
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

		public void SwitchRadView(string pantalla, int id, string fr)
		{
			if (pantalla=="CambioFrecuencia" && _StateManager.Radio[id].AudioVia != RdRxAudioVia.NoAudio)
				return;
			changefr view = new changefr();
			view.id = id;
			view.frecuencias = fr;
			view.pantalla = pantalla;
			view.listafrecuencias = _StateManager.Radio[id].Frecuencia_Sel;
			General.SafeLaunchEvent(SwitchRadViewUI, this, new EventArgs<changefr>(view));
		}

		public void RdSetSpeakerLevel(int level)
        {
            // Si existen posiciones no desasignables en el puesto, el nivel de 
            // volumen del altavoz radio no puede bajar del 30% del máximo.
            if (_StateManager.Radio.RadioMonitoring && level < 3)
                return;
			//RQF-14
			int numPositionsByPage = _StateManager.Radio.PageSize;
			if (_StateManager.Radio.FrecuenciaNoDesasignable(numPositionsByPage) && level < 3)
				return;

			_EngineCmdManager.SetRdSpeakerLevel(level);
        }

        public void RdSetHfSpeakerLevel(int level)
        {
            _EngineCmdManager.SetRdHfSpeakerLevel(level);
        }

        public void RdSetHeadPhonesLevel(int level)
		{
			//RQF-14
			int numPositionsByPage = _StateManager.Radio.PageSize;
			if (level < 3 && _StateManager.Radio.FrecuenciaNoDesasignable(numPositionsByPage) )
				return;

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

        public void RdSwitchRxState(int id, bool longClick)
        {
            RdDst dst = _StateManager.Radio[id];
            Debug.Assert((_StateManager.Radio.Rtx == 0) || !dst.Tx || ((dst.RtxGroup != 0) && (dst.RtxGroup != _StateManager.Radio.Rtx)));

            if (!dst.Rx || dst.Monitoring)
            {
                _EngineCmdManager.SetRdRx(id, true);
            }
			//LALM Errores #4685 
			// Si el canal esta en un grupo RTX propio no se debe desasignar en TX.
			else if ((longClick) && (dst.RtxGroup > 0))
			{
				_EngineCmdManager.NextRdAudio(id);
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
            try
            {
				General.SafeLaunchEvent(LoadTlfDaPageUI, this, new PageMsg(page));

			}
			catch (Exception)
            {

                throw;
            }
		}

		public void TlfClick(int id)
		{
			switch (_StateManager.Tlf[id].State)
			{
				case TlfState.Idle:
				case TlfState.PaPBusy:
				case TlfState.offhook://#2855
					if (id < Tlf.NumDestinations)
					{
						if (_StateManager.Tlf.Listen.State == FunctionState.Ready)
						{
							_EngineCmdManager.ListenTo(id);
						}
						else if (_StateManager.Tlf.Transfer.State == FunctionState.Ready)
						{
							_EngineCmdManager.TransferTo(id, true);
						}
                        else if (_StateManager.Tlf.PickUp.State == FunctionState.Ready)
                        {
                            _EngineCmdManager.PreparePickUp(id);
                        }
                        else if (_StateManager.Tlf.PickUp.State == FunctionState.Executing) 
                        {
                            _EngineCmdManager.CancelPickUp();
                            _EngineCmdManager.PreparePickUp(id);
                        }
                        else if (_StateManager.Tlf.Forward.State == FunctionState.Ready)
                        {
                            _EngineCmdManager.PrepareForward(id);
                        }
						else
						{
							//#2855 si la tecla AI esta descolgada, la cuelgo.
							if (_StateManager.Tlf.Unhang.State == UnhangState.Descolgado)
                            {
								_EngineCmdManager.Descuelga();
								_EngineCmdManager.EndTlfCall(Tlf.IaMappedPosition);
							}
							if (_StateManager.Tlf.PickUp.State == FunctionState.Error)
                                _EngineCmdManager.CancelPickUp();
							_EngineCmdManager.BeginTlfCall(id, _StateManager.Tlf.Priority.NewCall(id));
						}
					}
					else
					{
						//#2855
						TlfClick(_StateManager.Tlf[id].Number, false, null, id);
						//Para comunicar al boton que cambia de color.
						if (_StateManager.Tlf.Unhang.State == UnhangState.Descolgado)
							_StateManager.Tlf.Unhang.Cuelga();
					}
					break;
				case TlfState.In:
				case TlfState.InPrio:
				case TlfState.RemoteIn:
                    if (_StateManager.Tlf.PickUp.State == FunctionState.Ready)
                    {
                        _StateManager.Tlf.PickUp.Reset();
                    }
					_EngineCmdManager.AnswerTlfCall(id);
					break;
				case TlfState.Out:
				case TlfState.Set:
				case TlfState.RemoteHold:
				case TlfState.Busy:
				case TlfState.Congestion:
				case TlfState.OutOfService:
				case TlfState.NotAllowed:
                case TlfState.ConfPreprogramada:
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
					if (_StateManager.Tlf.Transfer.State == FunctionState.Ready)
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
                case TlfState.InProcess:
                    //Do nothing
                    break;
			}
		}

		public void TlfClick(string number, string literal)
		{
			TlfClick(number, true, literal);
		}

		public void PriorityClick()
		{
			if (_StateManager.Tlf.pageconf)//230523
				return;
			switch (_StateManager.Tlf.Priority.State)
			{
				case FunctionState.Idle:
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
				case FunctionState.Ready:
				case FunctionState.Executing:
				case FunctionState.Error:
					_StateManager.Tlf.Priority.Reset();
					break;
			}
		}

		public void ListenClick()
		{
			if (_StateManager.Tlf.pageconf)//230523
				return;
			switch (_StateManager.Tlf.Listen.State)
			{
				case FunctionState.Idle:
					_StateManager.Tlf.Listen.State = FunctionState.Ready;
					break;
				case FunctionState.Ready:
					_StateManager.Tlf.Listen.State = FunctionState.Idle;
					break;
				case FunctionState.Executing:
					_EngineCmdManager.CancelListen();
					break;
				case FunctionState.Error:
					_EngineCmdManager.RecognizeListenState();
					break;
			}
		}

		public void HoldClick()
		{

    //        if (_StateManager.Tlf.pageconf)//230523
				//return;
            if (_EngineCmdManager.HayConferenciaPreprogramada())
            {
                return;
            }
            if (_EngineCmdManager.HayConferencia())
            {
                _EngineCmdManager.SetHold(true);
            }
            else
            {
				//GetListaSalasEstado();
				//RefrescaListaParticipantesConf("");

				int id = _StateManager.Tlf.GetFirstInState(TlfState.Set, TlfState.Conf, TlfState.RemoteHold);
				////firstbuttonConf=_StateManager.Tlf.GetNumDestinations();
				//int nd = _StateManager.Tlf.NumDestinations;
				//230525 las conferencias preprogramadas no se pueden retener.
                {
                    _EngineCmdManager.SetHold(id, true);
                }
            }
		}

		public void TransferClick()
		{
			if (_StateManager.Tlf.pageconf)//230523
				return;
			if (_EngineCmdManager.HayConferenciaPreprogramada())
			{
				return;
			}
			switch (_StateManager.Tlf.Transfer.State)
			{
				case FunctionState.Idle:
					_StateManager.Tlf.Transfer.State = FunctionState.Ready;
					break;
				case FunctionState.Ready:
					_StateManager.Tlf.Transfer.State = FunctionState.Idle;
					break;
				case FunctionState.Executing:
					_EngineCmdManager.CancelTransfer();
					break;
				case FunctionState.Error:
					_EngineCmdManager.RecognizeTransferState();
					break;
			}
		}

        public void ConferenceClick()
        {
			if (_StateManager.Tlf.pageconf)//230523
				return;
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

		public bool CancelTlfClick(bool test=false)
		{
			if (_StateManager.Tlf.Priority.State == FunctionState.Error)
			{
				if (test) return true;
				_StateManager.Tlf.Priority.Reset();
			}
			else  if (_StateManager.Tlf.Listen.State == FunctionState.Error)
			{
				if (test) return true;
				_EngineCmdManager.RecognizeListenState();
			}
			else if (_StateManager.Tlf.Transfer.State == FunctionState.Error)
			{
				if (test) return true;
				_EngineCmdManager.RecognizeTransferState();
			}
			else if (_StateManager.Tlf.Priority.State == FunctionState.Ready)
			{
				if (test) return true;
				_StateManager.Tlf.Priority.Reset();
			}
			else if (_StateManager.Tlf.Listen.State == FunctionState.Ready)
			{
				if (test) return true;
				_StateManager.Tlf.Listen.State = FunctionState.Idle;
			}
			else if (_StateManager.Tlf.Transfer.State == FunctionState.Ready)
			{
				if (test) return true;
				_StateManager.Tlf.Transfer.State = FunctionState.Idle;
			}
            else if (_StateManager.Tlf.Transfer.State == FunctionState.Executing)
            {
				if (test) return true;
				_EngineCmdManager.CancelTransfer();
            }
			//#2855
			else if (_StateManager.Tlf.Unhang.State == UnhangState.Descolgado)
			{
				if (test) return true;
				//_StateManager.Tlf.Unhang.Reset();
				TlfClick(Tlf.IaMappedPosition);
				_StateManager.Tlf.Unhang.Cuelga();
			}
			else if ((_StateManager.Tlf[TlfState.RemoteIn] > 0) && Settings.Default.SupportInTlfCancel)
			{
				if (test) return true;
				int id = _StateManager.Tlf.GetFirstInState(TlfState.RemoteIn);

				_EngineCmdManager.EndTlfCall(id);
				if (_StateManager.Tlf.Priority.AssociatePosition == id)
				{
					_StateManager.Tlf.Priority.Reset();
				}
			}
			else if ((_StateManager.Tlf[TlfState.In] > 0) && Settings.Default.SupportInTlfCancel)
			{
				if (test) return true;
				int id = _StateManager.Tlf.GetFirstInState(TlfState.In);

				_EngineCmdManager.EndTlfCall(id);
				if (_StateManager.Tlf.Priority.AssociatePosition == id)
				{
					_StateManager.Tlf.Priority.Reset();
				}
			}
			else if ((_StateManager.Tlf[TlfState.InPrio] > 0) && Settings.Default.SupportInTlfCancel)
			{
				if (test) return true;
				int id = _StateManager.Tlf.GetFirstInState(TlfState.InPrio);

				_EngineCmdManager.EndTlfCall(id);
				if (_StateManager.Tlf.Priority.AssociatePosition == id)
				{
					_StateManager.Tlf.Priority.Reset();
				}
			}
            else if (_StateManager.Tlf[TlfState.Conf] > 0)
            {
                if (test) return true;
                _EngineCmdManager.EndTlfAll();
            }
            else if (_StateManager.Tlf[TlfState.ConfPreprogramada] > 0)
            {
                if (test) return true;
                _EngineCmdManager.EndTlfAll();
            }
            else if (_StateManager.Tlf[TlfState.Out] + _StateManager.Tlf[TlfState.NotAllowed] > 0)
			{
				if (test) return true;
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
				if (test) return true;
				int id = _StateManager.Tlf.GetFirstInState(TlfState.Set, TlfState.RemoteHold, TlfState.Busy, TlfState.Congestion, TlfState.OutOfService);
				_EngineCmdManager.EndTlfCall(id);
			}
			else if (_StateManager.Tlf[TlfState.Mem] + _StateManager.Tlf[TlfState.RemoteMem] > 0)
			{
				if (test) return true;
				int id = _StateManager.Tlf.GetFirstInState(TlfState.Mem, TlfState.RemoteMem, TlfState.NotAllowed);
				//if (id == Tlf.IaMappedPosition)
					_StateManager.Tlf.ResetMem(id);
				//else
				//	_StateManager.Tlf.Reset(new RangeMsg<TlfState>(id, TlfState.Idle));
			}
			else if (_StateManager.Tlf.HangTone.On)
			{
				if (test) return true;
				_EngineCmdManager.SetHangToneOff();
			}
			//RQF-18#5009 Anular escucha con cancel
			else if (_StateManager.Tlf.Listen.State > 0)
			{
				if (test) return true;
				_EngineCmdManager.CancelListen();
			}
			else if (_StateManager.Tlf.Forward.State > 0)
			{
				if (test) return true;
				_EngineCmdManager.CancelForward();
			}
			else if (_StateManager.Tlf[TlfState.Hold] > 0)
			{
				return false;
				// No hacemos nada
			}
			else
			{
				_EngineCmdManager.Cancel();
				return false;
			}
			return false;
		}

		public void NewDigit(int id, char key)
		{
			_EngineCmdManager.SendDigit(key);
			_StateManager.Tlf[id].Digits += key;
		}

        public void ReplayClick()
        {
			if (_StateManager.Tlf.pageconf)//230523
				return;
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

        public void PickUpClick()
        {
			if (_StateManager.Tlf.pageconf)//230523
				return;
			switch (_StateManager.Tlf.PickUp.State)
            {
                case FunctionState.Idle:
                    _StateManager.Tlf.PickUp.State = FunctionState.Ready;
                    break;
                case FunctionState.Ready:
                    _StateManager.Tlf.PickUp.Reset();
                    break;
                case FunctionState.Executing:
                case FunctionState.Error:
                    _EngineCmdManager.CancelPickUp();
                    break;
            }
        }

        public void ForwardClick()
        {
			if (_StateManager.Tlf.pageconf)//230523
				return;
			switch (_StateManager.Tlf.Forward.State)
            {
                case FunctionState.Idle:
                    _StateManager.Tlf.Forward.State = FunctionState.Ready;
                    break;
                case FunctionState.Ready:
                    _StateManager.Tlf.Forward.State = FunctionState.Idle;
                    break;
                case FunctionState.Executing:
                case FunctionState.Error:
                    _EngineCmdManager.CancelForward();
                    break;
            }
        }

		//LALM 210224 Errores #4756 visualizacion de mensaje de error por frecuencia prioritaria
		public void SetErrorFP()
		{
			_EngineCmdManager.SetErrorFP();
		}
		
		public void ResetErrorFP()
		{
			_EngineCmdManager.ResetErrorFP();
		}

		//LALM 210224
		public void SetCambioRadio(bool up)
		{
			//Debug.Assert(mode != _StateManager.Light.Mode);
			_EngineCmdManager.SetCambioRadio( up);
		}
		#endregion

		private void TlfClick(string number, bool ia, string givenLiteral = null, int id = Int32.MaxValue)
        {
            string literal = null;
			//LALM 211201
			//#2855
			if (number.Length == 0 || number=="9999")
			{
				_EngineCmdManager.Descuelga();
				if (_StateManager.Tlf[Tlf.IaMappedPosition].State==TlfState.offhook)
					_EngineCmdManager.EndTlfCall(Tlf.IaMappedPosition);
				// habria que colgar todo lo que esta en uso.
				if (_StateManager.Tlf.Listen.State == FunctionState.Ready)
				{
					// espero a que marquen
				}
				else if (_StateManager.Tlf.Transfer.State == FunctionState.Ready)
				{
					// espero a que marquen
					//_EngineCmdManager.TransferTo(number);
				}
				else if (_StateManager.Tlf.PickUp.State == FunctionState.Ready)
				{
					// espero a que marquen
					//_EngineCmdManager.PreparePickUp(number);
				}
				else if (_StateManager.Tlf.PickUp.State == FunctionState.Executing)
				{
					// espero a que marquen
					//_EngineCmdManager.CancelPickUp();
					//_EngineCmdManager.PreparePickUp(number);
				}
				else if (_StateManager.Tlf.Forward.State == FunctionState.Ready)
				{
					// espero a que marquen
					//_EngineCmdManager.PrepareForward(number);
				}
				else if (_StateManager.Tlf.PickUp.State == FunctionState.Idle)
				{
					// espero a que marquen
					//_EngineCmdManager.CancelPickUp();
					//_EngineCmdManager.PreparePickUp(number);
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
							case TlfState.InProcess:
								//Do nothing
								break;
						}
					}

					if (wait && (_EngineCmdManager.Name == "Ope"))
					{
						_EngineCmdManager.Wait(500);
					}
					if (_StateManager.Tlf.PickUp.State == FunctionState.Error)
						_EngineCmdManager.CancelPickUp();
					if (id != Int32.MaxValue)
						_EngineCmdManager.BeginTlfCall(number, _StateManager.Tlf.Priority.NewCall(Tlf.IaMappedPosition), id, literal);
					else
						_EngineCmdManager.BeginTlfCall(number, _StateManager.Tlf.Priority.NewCall(Tlf.IaMappedPosition), givenLiteral);
					_StateManager.Tlf.Unhang.NewCall(ia);
				}
			}
			//*2855
			if (number.Length > 0 && number!="9999")
            {
                number = Tlf.NumberToEngine(number);

                if (_StateManager.Tlf.Listen.State == FunctionState.Ready)
                {
                    _EngineCmdManager.ListenTo(number);
                }
                else if (_StateManager.Tlf.Transfer.State == FunctionState.Ready)
                {
                    _EngineCmdManager.TransferTo(number);
                }
                else if (_StateManager.Tlf.PickUp.State == FunctionState.Ready)
                {
                    _EngineCmdManager.PreparePickUp(number);
                }
                else if (_StateManager.Tlf.PickUp.State == FunctionState.Executing)
                {
                    _EngineCmdManager.CancelPickUp();
                    _EngineCmdManager.PreparePickUp(number);
                }
                else if (_StateManager.Tlf.Forward.State == FunctionState.Ready)
                {
                    _EngineCmdManager.PrepareForward(number);
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
                            case TlfState.InProcess:
                            //Do nothing
                                break;
                        }
                    }

                    if (wait && (_EngineCmdManager.Name == "Ope"))
                    {
                        _EngineCmdManager.Wait(500);
                    }
                    if (_StateManager.Tlf.PickUp.State == FunctionState.Error)
                        _EngineCmdManager.CancelPickUp();
					if (id != Int32.MaxValue)
						_EngineCmdManager.BeginTlfCall(number, _StateManager.Tlf.Priority.NewCall(Tlf.IaMappedPosition), id, literal);
					else
					{
						//#2855 Si la llamada es de acceso directo se cuelga todo lo de AI.
						_EngineCmdManager.Descuelga();
						if (_StateManager.Tlf[Tlf.IaMappedPosition].State == TlfState.offhook)
							_EngineCmdManager.EndTlfCall(Tlf.IaMappedPosition);

						_EngineCmdManager.BeginTlfCall(number, _StateManager.Tlf.Priority.NewCall(Tlf.IaMappedPosition), givenLiteral);
					}
                    _StateManager.Tlf.Unhang.NewCall(ia);
                }
			}
		}

		public void SendCmdHistoricalEvent(string user, string frec)
        {
            _EngineCmdManager.SendCmdHistoricalEvent(user, frec);
        }
				
		public class item
		{
			private string seg;
			private string tipo;
			private string text;
			private long lenght;
            private DateTime datetime;
            public string Text { get => text; set => text = value; }
            public long Lenght { get => lenght; set => lenght = value; }
            public string Tipo { get => tipo; set => tipo = value; }
            public string Seg { get => seg; set => seg = value; }
            public DateTime Datetime { get => datetime; set => datetime = value; }
        }

        private item FilterLastFile()
		{
			List<item> ListViewItem = new List<item>();
			try
			{
                DirectoryInfo di = new DirectoryInfo(Settings.Default.DirectorioGLPRxRadio);

				FileInfo[] fi = di.GetFiles("*.*", SearchOption.AllDirectories);
				if (fi.Length == 0)
					return null;

				FileInfo lastInfo = fi[fi.Length - 1];
				foreach (System.IO.FileInfo f in fi)
				{
					if (!f.Name.Contains("@"))
                    {
						item subItem = new item();

						switch (f.Name.Split('_')[0])
						{
							case "RxRadio":
								subItem.Tipo = "_RxRadio";
								subItem.Seg = String.Format("{0:0.00} s.", (float)f.Length / 16000.0);
								subItem.Lenght = f.Length;
								subItem.Text = f.DirectoryName + "/" + f.Name;
								subItem.Datetime = File.GetCreationTime(subItem.Text);
								if (subItem.Lenght>1600)// reproduzco ficheros mayores de 100ms
									ListViewItem.Add(subItem);
								break;
						}

					}
				}
			}
			catch (DirectoryNotFoundException)
			{
			}
			DateTime d = new DateTime(0);
			item ret = null;
			foreach (item i in ListViewItem)
				if (i.Datetime > d)
                {
					d = i.Datetime;
					ret = i;
                }
			return ret;
		}

		public bool CanReplayRadio()
        {
			item item = FilterLastFile();
			if (item != null)
				return true;
			return false;

		}

		public void StopAudioReproduccion()
        {
			FunctionReplay function = BusinessEntities.FunctionReplay.Stop;
			ViaReplay via = BusinessEntities.ViaReplay.SpeakerLc;
			FunctionReplay(function, via, "", 0);
		}

		public void PlayRadioClick()
		{
			//TODO  algo como esto.
			if (Directory.Exists(Settings.Default.DirectorioGLPRxRadio))
			{
				item item = FilterLastFile();
				if (item!=null)
                {
					FunctionReplay function = BusinessEntities.FunctionReplay.PlayNoLoop;
					ViaReplay via = BusinessEntities.ViaReplay.SpeakerLc;
					string fileName = item.Text;
					long fileLength = item.Lenght;
					FunctionReplay(function, via, fileName, fileLength);
					_StateManager.Radio.SetTiempoReplay((fileLength / 16000)>0?(int)(fileLength / 16000):1);
				}
				else
                {
					//  Envio tiempo a cero para inhabilitar botón
					_StateManager.Radio.SetTiempoReplay(0);
				}
			}
			//General.SafeLaunchEvent(PlayRadio, this);
			//this._BtnPlay.Click += new System.EventHandler(this._BtnPlay_Click);

			;//	Directory.Delete(Settings.Default.DirectorioGLP, true);

		}

		//#5829 Funcion para unificiar los tonos de señalización
		public void SetTonesLevel(int level)
		{
			// si hay conversacion de algun tipo en ese dispositivo no aplica.
			_EngineCmdManager.SetTonesLevel(level);
		}

		//LALM 221028 CambioFrecuencia
		public void SetNewFrecuency(int id,string frecuency)//lalm 230301
		{
			RdDst dst = _StateManager.Radio[id];
			Debug.Assert((_StateManager.Radio.Rtx == 0) || !dst.Tx || ((dst.RtxGroup != 0) && (dst.RtxGroup != _StateManager.Radio.Rtx)));

			_EngineCmdManager.SetNewFrecuency(id, frecuency);//lalm 230301
		}

		//LALM 230510
		public List<string> RefrescaListaParticipantesConf(string sala)
		{
			return _EngineCmdManager.RefrescaListaParticipantesConf(sala);
		}

		public List<string> GetListaParticipantesEstado(string sala)
		{
			return _EngineCmdManager.GetListaParticipantesEstado(sala);
		}
		public string GetSala(int poshmi)
		{
			return _EngineCmdManager.GetSala(poshmi);
		}
		public void DisableFunctionsPagConferencia(bool disable = true)
        {
            if (disable)
            {
                if (_StateManager.Tlf.Priority.State == FunctionState.Ready)
                    _StateManager.Tlf.Priority.Reset();
				if (_StateManager.Tlf.Listen.State == FunctionState.Ready)
				{
					_StateManager.Tlf.Listen.State = FunctionState.Idle;
					_StateManager.Tlf.Listen.Reset();
				}
				if (_StateManager.Tlf.Transfer.State == FunctionState.Ready)
                    _StateManager.Tlf.Transfer.State = FunctionState.Idle;
                if (_StateManager.Tlf.PickUp.State == FunctionState.Ready)
                    _StateManager.Tlf.PickUp.State = FunctionState.Idle;
                if (_StateManager.Tlf.Forward.State == FunctionState.Ready)
                    _StateManager.Tlf.Forward.State = FunctionState.Idle;
            }
			else
            {
				if (_StateManager.Tlf.Priority.State == FunctionState.Idle)
					_StateManager.Tlf.Priority.Reset();
				if (_StateManager.Tlf.Listen.State == FunctionState.Idle)
				{
					_StateManager.Tlf.Listen.State = FunctionState.Ready;
					_StateManager.Tlf.Listen.State = FunctionState.Idle;
					_StateManager.Tlf.Listen.State = FunctionState.Ready;
					_StateManager.Tlf.Listen.Reset();
				}
				if (_StateManager.Tlf.Transfer.State == FunctionState.Idle)
				{
					_StateManager.Tlf.Transfer.State = FunctionState.Ready;
					_StateManager.Tlf.Transfer.State = FunctionState.Idle;
				}
				if (_StateManager.Tlf.PickUp.State == FunctionState.Idle)
					_StateManager.Tlf.PickUp.State = FunctionState.Ready;
				if (_StateManager.Tlf.Forward.State == FunctionState.Idle)
					_StateManager.Tlf.Forward.State = FunctionState.Ready;

			}
		}

		/// <summary>
		/// Muestra los botones de la pagina de conferencia que pueden haber sido ocultados 
		/// al presentar la lista de participantes.
		/// </summary>
		public void ShowAdButtons(int page)
		{
			
			TlfLoadDaPage(page);

			//_EngineCmdManager.ShowAdButtons();
		}
	}
}
