#define _HF_GLOBAL_STATUS_
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Timers;
using System.Diagnostics;
using Microsoft.Practices.CompositeUI;
using Microsoft.Practices.CompositeUI.EventBroker;
using HMI.Infrastructure.Interface;
using HMI.Model.Module.Constants;
using HMI.Model.Module.Messages;
using HMI.Model.Module.BusinessEntities;
using HMI.Model.Module.Properties;
using NLog;

namespace HMI.Model.Module.Services
{
	public class EngineEventsManagerService
	{
		private static Logger _Logger = LogManager.GetCurrentClassLogger();

		private StateManagerService _StateManager;
		private IEngineCmdManagerService _EngineCmdManager;
		private UiTimer _ScreenSaverTimer;
		private UiTimer _HangupCallsTimer;
        private bool ResetTimer = true;

		public EngineEventsManagerService([ServiceDependency] StateManagerService stateManager, [ServiceDependency] IEngineCmdManagerService engineCmdManager)
		{
			_StateManager = stateManager;
			_EngineCmdManager = engineCmdManager;

			_ScreenSaverTimer = new UiTimer(Settings.Default.NoJacksScreenSaverSg * 1000);
			_ScreenSaverTimer.AutoReset = false;
			_ScreenSaverTimer.Elapsed += OnScreenSaverTimerElapsed;

			_HangupCallsTimer = new UiTimer(Settings.Default.NoJacksEndComunicationsSg * 1000);
			_HangupCallsTimer.AutoReset = false;
			_HangupCallsTimer.Elapsed += OnHangupCallsTimerElapsed;
		}

		[EventSubscription(EventTopicNames.ConnectionStateEngine, ThreadOption.UserInterface)]
		public void OnConnectionStateEngine(object sender, EngineConnectionStateMsg msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.ConnectionStateEngine, msg);

			if (_StateManager.Engine.Connected != msg.Connected)
			{
                _Logger.Trace("EventTopicNames.ConnectionStateEngine connecting");
				_StateManager.Engine.Connected = msg.Connected;
				OnEngineStateChanged();
            }
            else
                _Logger.Trace("EventTopicNames.ConnectionStateEngine already connected");
		}

		[EventSubscription(EventTopicNames.IsolatedStateEngine, ThreadOption.UserInterface)]
		public void OnIsolatedStateEngine(object sender, EngineIsolatedStateMsg msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.IsolatedStateEngine, msg);

			//bool engineOperative = _StateManager.Engine.Operative;
			_StateManager.Engine.Isolated = msg.Isolated;

			//if (engineOperative != _StateManager.Engine.Operative)
			//{
			//    OnEngineStateChanged();
			//}
		}

		[EventSubscription(EventTopicNames.ActiveScvEngine, ThreadOption.UserInterface)]
		public void OnActiveScvEngine(object sender, ActiveScvMsg msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.ActiveScvEngine, msg);

			_StateManager.Scv.Active = msg.Scv;
		}

		[EventSubscription(EventTopicNames.ShowNotifMsgEngine, ThreadOption.UserInterface)]
		public void OnShowNotifMsgEngine(object sender, NotifMsg msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.ShowNotifMsgEngine, msg);

			_StateManager.ShowUIMessage(msg);
		}

		[EventSubscription(EventTopicNames.HideNotifMsgEngine, ThreadOption.UserInterface)]
		public void OnHideNotifMsgEngine(object sender, EventArgs<string> msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.HideNotifMsgEngine, msg.Data);

			_StateManager.HideUIMessage(msg.Data);
		}

		[EventSubscription(EventTopicNames.PositionIdEngine, ThreadOption.UserInterface)]
		public void OnPositionIdEngine(object sender, PositionIdMsg msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.PositionIdEngine, msg);

            _StateManager.Title.Id = msg.Id; // msg.Id.Length > 12 ? (msg.Id.Substring(0, 10) + "...") : msg.Id;
		}

		[EventSubscription(EventTopicNames.ResetEngine, ThreadOption.UserInterface)]
		public void OnResetEngine(object sender, EventArgs e)
		{
			_Logger.Trace("Procesando {0}", EventTopicNames.ResetEngine);

			//_StateManager.Radio.Reset();
			//_StateManager.Lc.Reset();
			_StateManager.Tlf.Reset();
		}

		[EventSubscription(EventTopicNames.SplitModeEngine, ThreadOption.UserInterface)]
		public void OnSplitModeEngine(object sender, StateMsg<SplitMode> msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.SplitModeEngine, msg);

			_StateManager.Split.Mode = msg.State;
		}

		[EventSubscription(EventTopicNames.JacksStateEngine, ThreadOption.UserInterface)]
		public void OnJacksStateEngine(object sender, JacksStateMsg msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.JacksStateEngine, msg);

			_StateManager.Jacks.Reset(msg.LeftJack, msg.RightJack);

			if (_StateManager.Jacks.SomeJack)
			{
                ResetTimer = false;

				_ScreenSaverTimer.Enabled = false;
				_HangupCallsTimer.Enabled = false;

				_StateManager.Tft.Enabled = true;
				_StateManager.ScreenSaver.On = false;

				_EngineCmdManager.SendTrapScreenSaver(false);

                _StateManager.Radio.ResetAssignatedState();
			}
			else
			{
				if (_StateManager.Tlf.Priority.State != PriorityState.Idle)
				{
					_StateManager.Tlf.Priority.Reset();
				}

				if (_StateManager.Tlf.Listen.State == ListenState.Ready)
				{
					_StateManager.Tlf.Listen.State = ListenState.Idle;
				}
				else if (_StateManager.Tlf.Listen.State == ListenState.Executing)
				{
					_EngineCmdManager.CancelListen();
				}
				else if (_StateManager.Tlf.Listen.State == ListenState.Error)
				{
					_EngineCmdManager.RecognizeListenState();
				}

				if (_StateManager.Tlf.Transfer.State == TransferState.Ready)
				{
					_StateManager.Tlf.Transfer.State = TransferState.Idle;
				}
				else if (_StateManager.Tlf.Transfer.State == TransferState.Executing)
				{
					_EngineCmdManager.CancelTransfer();
				}
				else if (_StateManager.Tlf.Transfer.State == TransferState.Error)
				{
					_EngineCmdManager.RecognizeTransferState();
				}

				_HangupCallsTimer.Enabled = true;
				if (!_StateManager.ScreenSaver.On)
				{
                    if (ResetTimer)
                    {
                        _StateManager.ScreenSaver.On = true;
                        _EngineCmdManager.SendTrapScreenSaver(true);
                        ResetTimer = false;
                    }
                    else
                        _ScreenSaverTimer.Enabled = true;
                }
			}
		}

        [EventSubscription(EventTopicNames.SpeakerStateEngine, ThreadOption.UserInterface)]
        public void OnSpeakerStateEngine(object sender, JacksStateMsg msg)
        {
            _Logger.Trace("Procesando {0}: left {1} right {2}", EventTopicNames.SpeakerStateEngine, msg.LeftJack, msg.RightJack);
            _StateManager.LcSpeaker.Presencia = msg.RightJack;
            _StateManager.RdSpeaker.Presencia = msg.LeftJack;
        }

        [EventSubscription(EventTopicNames.SpeakerExtStateEngine, ThreadOption.UserInterface)]
        public void OnSpeakerExtStateEngine(object sender, JacksStateMsg msg)
        {
            _Logger.Trace("Procesando {0}: left {1} right {2}", EventTopicNames.SpeakerExtStateEngine, msg.LeftJack, msg.RightJack);
            _StateManager.HfSpeaker.Presencia = msg.LeftJack;
        }

		[EventSubscription(EventTopicNames.BuzzerStateEngine, ThreadOption.UserInterface)]
		public void OnBuzzerStateEngine(object sender, StateMsg<bool> msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.BuzzerStateEngine, msg);

			_StateManager.Buzzer.Enabled = msg.State;
		}

		[EventSubscription(EventTopicNames.BuzzerLevelEngine, ThreadOption.UserInterface)]
		public void OnBuzzerLevelEngine(object sender, LevelMsg<Buzzer> msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.BuzzerLevelEngine, msg);

			_StateManager.Buzzer.Level = msg.Level;
		}

		[EventSubscription(EventTopicNames.RdInfoEngine, ThreadOption.UserInterface)]
		public void OnRdInfoEngine(object sender, RangeMsg<RdInfo> msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.RdInfoEngine, msg);

			_StateManager.Radio.Reset(msg);
        
            // Si la radio tiene alguna posición no desasignable, el volumen del altavoz no puede estar por debajo del 30%
            _EngineCmdManager.SetRdSpeakerLevel(_StateManager.Radio.RadioMonitoring ? Math.Max(_StateManager.RdSpeaker.Level, 3) : _StateManager.RdSpeaker.Level);
        }

		[EventSubscription(EventTopicNames.RdPositionsEngine, ThreadOption.UserInterface)]
		public void OnRdPositionsEngine(object sender, RangeMsg<RdDestination> msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.RdPositionsEngine, msg);

			_StateManager.Radio.Reset(msg);
		}

		[EventSubscription(EventTopicNames.RdPageEngine, ThreadOption.UserInterface)]
		public void OnRdPageEngine(object sender, PageMsg msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.RdPageEngine, msg);

			_StateManager.Radio.Page = msg.Page;
		}

		[EventSubscription(EventTopicNames.RdPttEngine, ThreadOption.UserInterface)]
		public void OnRdPttEngine(object sender, StateMsg<bool> msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.RdPttEngine, msg);

			_StateManager.Radio.PttOn = msg.State;
		}

        [EventSubscription(EventTopicNames.SelCalResponseEngine, ThreadOption.UserInterface)]
        public void OnSelCalResponse(object sender, StateMsg<string> msg)
        {
            _Logger.Trace("Procesando {0}: {1}", EventTopicNames.SelCalResponseEngine, msg);

            _StateManager.Radio.SetSelCalMessage(msg);
        }

        [EventSubscription(EventTopicNames.SiteChangedResultEngine, ThreadOption.UserInterface)]
        public void OnSiteChangedResultEngine(object sender, StateMsg<string> msg)
        {
            _Logger.Trace("Procesando {0}: {1}", EventTopicNames.SiteChangedResultEngine, msg);

            _StateManager.Radio.SetSiteChanged(msg);
        }
                

		[EventSubscription(EventTopicNames.RdPosPttStateEngine, ThreadOption.UserInterface)]
		public void OnRdPosPttStateEngine(object sender, RangeMsg<PttState> msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.RdPosPttStateEngine, msg);

			_StateManager.Radio.Reset(msg);
		}

		[EventSubscription(EventTopicNames.RdPosSquelchStateEngine, ThreadOption.UserInterface)]
		public void OnRdPosSquelchStateEngine(object sender, RangeMsg<SquelchState> msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.RdPosSquelchStateEngine, msg);

			_StateManager.Radio.Reset(msg);
		}

		[EventSubscription(EventTopicNames.RdPosAsignStateEngine, ThreadOption.UserInterface)]
		public void OnRdPosAsignStateEngine(object sender, RangeMsg<RdAsignState> msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.RdPosAsignStateEngine, msg);

			_StateManager.Radio.Reset(msg);
		}

		[EventSubscription(EventTopicNames.RdPosStateEngine, ThreadOption.UserInterface)]
		public void OnRdPosStateEngine(object sender, RangeMsg<RdState> msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.RdPosStateEngine, msg);

			_StateManager.Radio.Reset(msg);

            // Si la radio tiene alguna posición no desasignable, el volumen del altavoz no puede estar por debajo del 30%
            int level = _StateManager.Radio.RadioMonitoring ? Math.Max(_StateManager.RdSpeaker.Level, 3) : _StateManager.RdSpeaker.Level;
            if (level != _StateManager.RdSpeaker.Level)
                _EngineCmdManager.SetRdSpeakerLevel(level);
        }

        [EventSubscription(EventTopicNames.SiteManagerEngine, ThreadOption.UserInterface)]
        public void OnSiteManagerEngine(object sender, StateMsg<bool> msg)
        {
            _Logger.Trace("Procesando {0}: {1}", EventTopicNames.SiteManagerEngine, msg);

            _StateManager.Radio.SiteManager = msg.State;
            //_StateManager.SiteManager = msg.State;
        }
        
        [EventSubscription(EventTopicNames.RdRtxModificationEndEngine, ThreadOption.UserInterface)]
		public void OnRdRtxModificationEndEngine(object sender, EventArgs msg)
		{
			_Logger.Trace("Procesando {0}", EventTopicNames.RdRtxModificationEndEngine);

			_StateManager.HideUIMessage(Resources.RtxOperationRunning);
		}

		[EventSubscription(EventTopicNames.RdRtxGroupsEngine, ThreadOption.UserInterface)]
		public void OnRdRtxGroupsEngine(object sender, RangeMsg<RdRtxGroup> msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.RdRtxGroupsEngine, msg);

			_StateManager.Radio.Reset(msg);
		}

        [EventSubscription(EventTopicNames.RdSpeakerLevelEngine, ThreadOption.UserInterface)]
        public void OnRdSpeakerLevelEngine(object sender, LevelMsg<RdSpeaker> msg)
        {
            _Logger.Trace("Procesando {0}: {1}", EventTopicNames.RdSpeakerLevelEngine, msg);

            _StateManager.RdSpeaker.Level = msg.Level;
        }

        [EventSubscription(EventTopicNames.RdHFLevelEngine, ThreadOption.UserInterface)]
        public void OnRdHfLevelEngine(object sender, LevelMsg<HfSpeaker> msg)
        {
            _Logger.Trace("Procesando {0}: {1}", EventTopicNames.RdHFLevelEngine, msg);

            _StateManager.HfSpeaker.Level = msg.Level;
        }

        [EventSubscription(EventTopicNames.RdHeadPhonesLevelEngine, ThreadOption.UserInterface)]
		public void OnRdHeadPhonesLevelEngine(object sender, LevelMsg<RdHeadPhones> msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.RdHeadPhonesLevelEngine, msg);

			_StateManager.RdHeadPhones.Level = msg.Level;
		}

        [EventSubscription(EventTopicNames.RdFrAsignedToOtherEngine, ThreadOption.UserInterface)]
        public void RdFrAsignedToOtherEngine(object sender, RdFrAsignedToOtherMsg msg)
        {
            _Logger.Trace("Procesando {0}: {1}", EventTopicNames.RdFrAsignedToOtherEngine, msg);

            RdDst rd = _StateManager.Radio[msg.RdId];
            NotifMsg notif;

            if (!rd.Tx)
            {
                string text = string.Format(Resources.RdFrAsignedToOtherConfirmation, rd.Frecuency);
                notif = new NotifMsg("RdFrAsignedToOtherConfirmation", Resources.MessageInfoCaption, text, 0, MessageType.Warning, MessageButtons.OkCancel, msg);
            }
            else
            {
                string text = string.Format(Resources.RdFrAsignedToOtherNotification, msg.Owner, rd.Frecuency);
                notif = new NotifMsg("RdFrAsignedToOther", Resources.MessageInfoCaption, text, Settings.Default.MessageToutSg * 1000, MessageType.Information, MessageButtons.Ok, null);
            }

            _StateManager.ShowUIMessage(notif);
        }

        [EventSubscription(EventTopicNames.RdHfFrAssignedEngine, ThreadOption.UserInterface)]
        public void RdHfFrAssignedEngine(object sender, RdHfFrAssigned msg)
        {
            string text;

            _Logger.Trace("Procesando {0}: {1}", EventTopicNames.RdHfFrAssignedEngine, msg);

            RdDst rd = _StateManager.Radio[msg.Id];
            NotifMsg notif;

            switch (msg.HfEstado)
            {
                case 0xFF:
                    text = Resources.RdHfFrAssignedEngine;
                    break;
                case 0xFD:
                    text = string.Format(Resources.RdFrAsignedToOtherConfirmation, rd.Frecuency);
                    break;
                case 0xFE:
                    text = string.Format(Resources.RdHfResourceError, rd.Frecuency);
                    break;
                default:
                    text = Resources.RdHfEquipment;
                    break;
            }

             //= msg.HfEstado == 0xFF ? Resources.RdHfFrAssignedEngine : Resources.RdHfEquipment;
            notif = new NotifMsg("RdHfFrAssignedEngine", Resources.MessageInfoCaption, text, 0, MessageType.Information, MessageButtons.Ok, msg);

            _StateManager.ShowUIMessage(notif);
        }

#if _HF_GLOBAL_STATUS_
        [EventSubscription(EventTopicNames.HfGlobalStatusEngine, ThreadOption.UserInterface)]
        public void OnHfGlobalStatusEngine(object sender, StateMsg<string> msg)
        {
            _Logger.Trace("Procesando {0}: {1}", EventTopicNames.HfGlobalStatusEngine, msg);

            _StateManager.Radio.SetHfGlobalStatus(msg);
        }
#endif

        [EventSubscription(EventTopicNames.LcInfoEngine, ThreadOption.UserInterface)]
		public void OnLcInfoEngine(object sender, RangeMsg<LcInfo> msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.LcInfoEngine, msg);

			_StateManager.Lc.Reset(msg);
		}

		[EventSubscription(EventTopicNames.LcPositionsEngine, ThreadOption.UserInterface)]
		public void OnLcPositionsEngine(object sender, RangeMsg<LcDestination> msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.LcPositionsEngine, msg);

			_StateManager.Lc.Reset(msg);
		}

		[EventSubscription(EventTopicNames.LcPosStateEngine, ThreadOption.UserInterface)]
		public void OnLcPosStateEngine(object sender, RangeMsg<LcState> msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.LcPosStateEngine, msg);

			_StateManager.Lc.Reset(msg);
		}

		[EventSubscription(EventTopicNames.TlfInfoEngine, ThreadOption.UserInterface)]
		public void OnTlfInfoEngine(object sender, RangeMsg<TlfInfo> msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.TlfInfoEngine, msg);

			_StateManager.Tlf.Reset(msg);
		}

		[EventSubscription(EventTopicNames.TlfPositionsEngine, ThreadOption.UserInterface)]
		public void OnTlfPositionsEngine(object sender, RangeMsg<TlfDestination> msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.TlfPositionsEngine, msg);

			_StateManager.Tlf.Reset(msg);
		}

		[EventSubscription(EventTopicNames.TlfPosStateEngine, ThreadOption.UserInterface)]
		public void OnTlfPosStateEngine(object sender, RangeMsg<TlfState> msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.TlfPosStateEngine, msg);

			_StateManager.Tlf.Reset(msg);
		}

		[EventSubscription(EventTopicNames.LcSpeakerLevelEngine, ThreadOption.UserInterface)]
		public void OnLcSpeakerLevelEngine(object sender, LevelMsg<LcSpeaker> msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.LcSpeakerLevelEngine, msg);

			_StateManager.LcSpeaker.LevelLC = msg.Level;
		}

		[EventSubscription(EventTopicNames.TlfHeadPhonesLevelEngine, ThreadOption.UserInterface)]
		public void OnTlfHeadPhonesLevelEngine(object sender, LevelMsg<TlfHeadPhones> msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.TlfHeadPhonesLevelEngine, msg);

			_StateManager.TlfHeadPhones.Level = msg.Level;
		}

        [EventSubscription(EventTopicNames.TlfSpeakerLevelEngine, ThreadOption.UserInterface)]
        public void OnTlfSpeakerLevelEngine(object sender, LevelMsg<LcSpeaker> msg)
        {
            _Logger.Trace("Procesando {0}: {1}", EventTopicNames.TlfSpeakerLevelEngine, msg);
            //El volumen de telefonía por altavoz de LC es diferente del de LC
            _StateManager.LcSpeaker.LevelTlf = msg.Level;
        }
        
        [EventSubscription(EventTopicNames.PriorityStateEngine, ThreadOption.UserInterface)]
		public void OnPriorityStateEngine(object sender, StateMsg<PriorityState> msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.PriorityStateEngine, msg);

			_StateManager.Tlf.Priority.Reset(msg.State);
		}

		[EventSubscription(EventTopicNames.IntrudedByEngine, ThreadOption.UserInterface)]
		public void OnIntrudedByEngine(object sender, StateMsg<string> msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.IntrudedByEngine, msg);

			_StateManager.Tlf.IntrudedBy.Reset(msg);
		}

		[EventSubscription(EventTopicNames.InterruptedByEngine, ThreadOption.UserInterface)]
		public void OnInterruptedByEngine(object sender, StateMsg<string> msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.InterruptedByEngine, msg);

			_StateManager.Tlf.InterruptedBy.Reset(msg);
		}

		[EventSubscription(EventTopicNames.ConfListEngine, ThreadOption.UserInterface)]
		public void OnConfListEngine(object sender, RangeMsg<string> msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.ConfListEngine, msg);

            if (msg != null)
            {
                for (int i = 0, to = msg.Count; i < to; i++)
                {
                    msg.Info[i] = Tlf.NumberToPresentation(msg.Info[i]);
                }

                _StateManager.Tlf.ConfList.Reset(msg);
            }
            else
                _StateManager.Tlf.ConfList.Reset();
		}

		[EventSubscription(EventTopicNames.ListenStateEngine, ThreadOption.UserInterface)]
		public void OnListenStateEngine(object sender, ListenMsg msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.ListenStateEngine, msg);

			_StateManager.Tlf.Listen.Reset(msg);
		}

		[EventSubscription(EventTopicNames.RemoteListenStateEngine, ThreadOption.UserInterface)]
		public void OnRemoteListenStateEngine(object sender, ListenMsg msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.RemoteListenStateEngine, msg);

			if (msg.State == ListenState.Executing)
			{
				NotifMsg notif = new NotifMsg("ListenBy" + msg.Id, Resources.MessageInfoCaption, Resources.ListenByConfirmation, 29000, MessageType.Warning, MessageButtons.OkCancel, msg);
				_StateManager.ShowUIMessage(notif);
			}
			else
			{
				_StateManager.HideUIMessage("ListenBy" + msg.Id);
				_StateManager.Tlf.ListenBy.Reset(msg);
			}
		}

		[EventSubscription(EventTopicNames.TransferStateEngine, ThreadOption.UserInterface)]
		public void OnTransferStateEngine(object sender, StateMsg<TransferState> msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.TransferStateEngine, msg);

			_StateManager.Tlf.Transfer.Reset(msg);
		}

		[EventSubscription(EventTopicNames.HangToneStateEngine, ThreadOption.UserInterface)]
		public void OnHangToneStateEngine(object sender, StateMsg<bool> msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.HangToneStateEngine, msg);

			_StateManager.Tlf.HangTone.Reset(msg);
			if (!msg.State)
			{
				// Cuando se quita el tono de colgado quitamos también (si lo hay)
				// el mensaje de "Intervenido por:"
				_StateManager.Tlf.InterruptedBy.Reset();
			}
		}

		[EventSubscription(EventTopicNames.TlfIaPosStateEngine, ThreadOption.UserInterface)]
		public void OnTlfIaPosStateEngine(object sender, RangeMsg<TlfIaDestination> msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.TlfIaPosStateEngine, msg);

			foreach (TlfIaDestination tlf in msg.Info)
			{
				string number = Tlf.NumberToPresentation(tlf.Number);

				if (tlf.Number == tlf.Alias)
				{
					tlf.Alias = number;
				}
				tlf.Number = number;
			}

			_StateManager.Tlf.Reset(msg);
		}

		[EventSubscription(EventTopicNames.HoldTlfCallEngine, ThreadOption.UserInterface)]
		public void OnHoldTlfCallEngine(object sender, StateMsg<bool> msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.HoldTlfCallEngine, msg);

            // JCAM: 03/01/2017
            // Por aquí pasa cuando, habiendo actividad en telefonía se hace PTT 
            // o LC y la telefonía viene por altavoz
            // Debe aparcar las conversaciones establecidas y las aparcadas dejarlas aparcadas
            // Si la primera que encuentra está en estado de Hold o RemoteHold no aparcaría 
            // la que está en conversación. Es por eso que se elimina de la búsqueda 
            // las posiciones en estado de Hold o RemoteHold
            if (msg.State)
            {
                int id = _StateManager.Tlf.GetFirstInState(TlfState.Set, TlfState.Conf, TlfState.Out);  //, TlfState.RemoteHold, TlfState.Hold);
                if (id >= 0)
                {   
                    //Aparco las conversaciones establecidas y las salientes (en diferido)
                    _EngineCmdManager.SetHold(id, msg.State, true);
                }
            }
            else
            {
                    _EngineCmdManager.SetHold(-1, msg.State, true);
            }
		}

		[EventSubscription(EventTopicNames.RemoveRtxGroup, ThreadOption.UserInterface)]
		public void OnRemoveRtxGroup(object sender, EventArgs msg)
		{
			_Logger.Trace("Procesando {0}", EventTopicNames.RemoveRtxGroup);
			
			//int tempRtxGroup = _StateManager.Radio.Rtx;
			//_StateManager.Radio.SetRtx((tempRtxGroup + 1) % (Settings.Default.MaxRtxGroups + 1));

			Dictionary<int, RtxState> newRtxGroup = _StateManager.Radio.ResetRtx();

			_EngineCmdManager.SetRtxGroup((_StateManager.Radio.Rtx + 1) % (Settings.Default.MaxRtxGroups + 1), newRtxGroup);

			NotifMsg message = new NotifMsg(Resources.RtxOperationRunning, Resources.MessageInfoCaption, Resources.RtxOperationRunning, 10000, MessageType.Processing, MessageButtons.None);
			_StateManager.ShowUIMessage(message);


		}

		[EventSubscription(EventTopicNames.AgendaChangedEngine, ThreadOption.UserInterface)]
		public void OnAgendaChangedEngine(object sender, RangeMsg<Number> msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.AgendaChangedEngine, msg);

			_StateManager.Agenda.Reset(msg);
		}

        [EventSubscription(EventTopicNames.NumberBookChangedEngine, ThreadOption.UserInterface)]
        public void OnNumberBookChangedEngine(object sender, RangeMsg<Area> msg)
        {
            _Logger.Trace("Procesando {0}", EventTopicNames.NumberBookChangedEngine);

            _StateManager.NumberBook.Reset(msg);
        }

        [EventSubscription(EventTopicNames.HistoricalOfLocalCallsEngine, ThreadOption.Publisher)]
        public void OnHistoricalOfLocalCallsEngine(object sender, RangeMsg<LlamadaHistorica> msg)
        {
            _Logger.Trace("Procesando {0}", EventTopicNames.HistoricalOfLocalCallsEngine);

            _StateManager.HistoricalOfCalls.Reset(msg);
        }

		[EventSubscription(EventTopicNames.PermissionsEngine, ThreadOption.UserInterface)]
		public void OnPermissionsEngine(object sender, StateMsg<Permissions> msg)
		{
            _Logger.Trace("Procesando {0}: {1}", EventTopicNames.PermissionsEngine, msg.ToString());

			_StateManager.Permissions = msg.State;
		}

		[EventSubscription(EventTopicNames.CompletedIntrusionStateEngine, ThreadOption.UserInterface)]
		public void OnCompletedIntrusionStateEngine(object sender, StateMsg<string> msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.CompletedIntrusionStateEngine, msg);

			if (msg != null)
				_StateManager.Tlf.IntrudedBy.Reset(msg);
			else
				_StateManager.Tlf.IntrudedBy.Reset();
		}

		[EventSubscription(EventTopicNames.BeginingIntrudeToStateEngine, ThreadOption.UserInterface)]
		public void OnBeginIntrudeToStateEngine(object sender, StateMsg<string> msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.BeginingIntrudeToStateEngine, msg);

			if (msg != null)
				_StateManager.Tlf.IntrudeTo.Reset(msg);
			else
				_StateManager.Tlf.IntrudeTo.Reset();
		}

		[EventSubscription(EventTopicNames.IntrudeToStateEngine, ThreadOption.UserInterface)]
		public void IntrudeToStateEngine(object sender, StateMsg<string> msg)
		{
			_Logger.Trace("Procesando {0}: {1}", EventTopicNames.IntrudeToStateEngine, msg);

            if (msg != null)
			    _StateManager.Tlf.InterruptedBy.Reset(msg);
            else
                _StateManager.Tlf.InterruptedBy.Reset();
		}

        [EventSubscription(EventTopicNames.BriefingStateEngine, ThreadOption.UserInterface)]
        public void OnBriefingStateEngine(object sender, StateMsg<bool> msg)
        {
            _Logger.Trace("Procesando {0}: {1}", EventTopicNames.BriefingStateEngine, msg);

            _StateManager.Tft.Briefing = msg.State;
        }

        [EventSubscription(EventTopicNames.PlayingStateEngine, ThreadOption.UserInterface)]
        public void OnPlayingStateEngine(object sender, StateMsg<bool> msg)
        {
            _Logger.Trace("Procesando {0}: {1}", EventTopicNames.PlayingStateEngine, msg);

            _StateManager.Tft.Playing = msg.State;
        }


        private void OnEngineStateChanged()
		{
			if (_StateManager.Engine.Connected)
			{
                _Logger.Trace("OnEngineStateChanged connected");
                
                if (_EngineCmdManager.Name != "Ope")
				{
					_EngineCmdManager.SetBuzzerState(_StateManager.Buzzer.Enabled);
					if (_StateManager.Buzzer.Enabled)
					{
						_EngineCmdManager.SetBuzzerLevel(_StateManager.Buzzer.Level);
					}

					_EngineCmdManager.SetRdHeadPhonesLevel(_StateManager.RdHeadPhones.Level);
					_EngineCmdManager.SetRdSpeakerLevel(_StateManager.RdSpeaker.Level);
					_EngineCmdManager.SetTlfHeadPhonesLevel(_StateManager.TlfHeadPhones.Level);
                    _EngineCmdManager.SetTlfSpeakerLevel(_StateManager.LcSpeaker.LevelTlf);
                    _EngineCmdManager.SetLcSpeakerLevel(_StateManager.LcSpeaker.LevelLC);
					_EngineCmdManager.SetSplitMode(_StateManager.Split.Mode);
                    _EngineCmdManager.SetRdHfSpeakerLevel(_StateManager.HfSpeaker.Level);
                    _EngineCmdManager.SetAudioViaTlf(_StateManager.Tlf.AltavozTlfEstado);
                    if (_StateManager.Tlf.SoloAltavoces)
                        _EngineCmdManager.ModoSoloAltavoces();
                    if (_StateManager.Radio.DoubleRadioSpeaker)
                        _EngineCmdManager.SetDoubleRadioSpeaker();
				}
			}
			else
			{
                _Logger.Trace("OnEngineStateChanged not connected");
                if (!_StateManager.ScreenSaver.On)
				{
					_ScreenSaverTimer.Enabled = true;
				}

				if (_StateManager.Tlf.Listen.State == ListenState.Ready)
				{
					_StateManager.Tlf.Listen.State = ListenState.Idle;
				}
				if (_StateManager.Tlf.Transfer.State == TransferState.Ready)
				{
					_StateManager.Tlf.Transfer.State = TransferState.Idle;
				}
				if (_StateManager.Tlf.Priority.State == PriorityState.Ready)
				{
					_StateManager.Tlf.Priority.Reset();
				}

				//_StateManager.Scv.Active = -1;
				//_StateManager.Jacks.Reset(false, false);
				//_StateManager.Radio.Reset();
				//_StateManager.Lc.Reset();
				//_StateManager.Tlf.Reset();
			}
		}

		private void OnScreenSaverTimerElapsed(object sender, ElapsedEventArgs e)
		{
            if (!_StateManager.Jacks.SomeJack || !_StateManager.Engine.Connected)
			{
				_StateManager.ScreenSaver.On = true;
				_EngineCmdManager.SendTrapScreenSaver(true);
			}
		}

		private void OnHangupCallsTimerElapsed(object sender, ElapsedEventArgs e)
		{
			if (!_StateManager.Jacks.SomeJack && _StateManager.Engine.Connected)
			{
				for (int i = 0; i < Tlf.NumDestinations + Tlf.NumIaDestinations; i++)
				{
					TlfDst dst = _StateManager.Tlf[i];

					switch (dst.State)
					{
						case TlfState.Hold:
							_EngineCmdManager.EndTlfCall(i, TlfState.Hold);
							break;
						case TlfState.Conf:
							_EngineCmdManager.EndTlfConfCall(i);
							break;
						case TlfState.OutOfService:
						case TlfState.Congestion:
						case TlfState.Busy:
						case TlfState.Out:
						case TlfState.Set:
						case TlfState.RemoteHold:
							_EngineCmdManager.EndTlfCall(i);
							break;
					}
				}

				for (int i = 0; i < Radio.NumDestinations; i++)
				{
					RdDst dst = _StateManager.Radio[i];

					if (dst.Rx)
					{
						_EngineCmdManager.ResetRdPosition(i);
						//_EngineCmdManager.SetRdRx(i, false);
					}
				}
			}
		}
	}
}
