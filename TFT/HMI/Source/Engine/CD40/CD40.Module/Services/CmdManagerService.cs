#define _HF_GLOBAL_STATUS_
#define _AUDIOGENERIC_
#define WT
//#define _MEZCLADOR_ASECNA_
#define _MEZCLADOR_TWR_
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Practices.ObjectBuilder;
using Microsoft.Practices.CompositeUI;
using Microsoft.Practices.CompositeUI.EventBroker;
using HMI.Infrastructure.Interface;
using HMI.Model.Module.Messages;
using HMI.Model.Module.BusinessEntities;
using HMI.Model.Module.Services;
using HMI.CD40.Module.Constants;
using HMI.CD40.Module.Properties;
using HMI.CD40.Module.BusinessEntities;
using HMI.CD40.Module.Snmp;
using U5ki.Infrastructure;
using Utilities;
using NLog;
using ProtoBuf;
using System.Threading;
/**
 * AGL 17072012. Trata de Rellenar la tabla de Dependencias desde un fichero local.
 * */
using System.Data;
using System.Data.OleDb;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
/*
 * Fin de la Modificacion */

namespace HMI.CD40.Module.Services
{
    class CmdManagerService : IEngineCmdManagerService
    {
        #region Events

        [EventPublication(EventTopicNames.ProxyStateChangedEngine, PublicationScope.Global)]
        public event EventHandler<StateMsg<bool>> ProxyStateChangedEngine;

        [EventPublication(EventTopicNames.ConnectionStateEngine, PublicationScope.Global)]
        public event EventHandler<EngineConnectionStateMsg> ConnectionStateEngine;

        [EventPublication(EventTopicNames.IsolatedStateEngine, PublicationScope.Global)]
        public event EventHandler<EngineIsolatedStateMsg> IsolatedStateEngine;

        [EventPublication(EventTopicNames.PositionIdEngine, PublicationScope.Global)]
        public event EventHandler<PositionIdMsg> PositionIdEngine;

        [EventPublication(EventTopicNames.SplitModeEngine, PublicationScope.Global)]
        public event EventHandler<StateMsg<SplitMode>> SplitModeEngine;

        [EventPublication(EventTopicNames.JacksStateEngine, PublicationScope.Global)]
        public event EventHandler<JacksStateMsg> JacksStateEngine;

        [EventPublication(EventTopicNames.SpeakerStateEngine, PublicationScope.Global)]
        public event EventHandler<JacksStateMsg> SpeakerStateEngine;

        [EventPublication(EventTopicNames.SpeakerExtStateEngine, PublicationScope.Global)]
        public event EventHandler<JacksStateMsg> SpeakerExtStateEngine;

        [EventPublication(EventTopicNames.BuzzerStateEngine, PublicationScope.Global)]
        public event EventHandler<StateMsg<bool>> BuzzerStateEngine;

        [EventPublication(EventTopicNames.BuzzerLevelEngine, PublicationScope.Global)]
        public event EventHandler<LevelMsg<Buzzer>> BuzzerLevelEngine;

        [EventPublication(EventTopicNames.TlfInfoEngine, PublicationScope.Global)]
        public event EventHandler<RangeMsg<TlfInfo>> TlfInfoEngine;

        [EventPublication(EventTopicNames.TlfPosStateEngine, PublicationScope.Global)]
        public event EventHandler<RangeMsg<TlfState>> TlfPosStateEngine;

        [EventPublication(EventTopicNames.LcSpeakerLevelEngine, PublicationScope.Global)]
        public event EventHandler<LevelMsg<LcSpeaker>> LcSpeakerLevelEngine;

        [EventPublication(EventTopicNames.TlfHeadPhonesLevelEngine, PublicationScope.Global)]
        public event EventHandler<LevelMsg<TlfHeadPhones>> TlfHeadPhonesLevelEngine;

        [EventPublication(EventTopicNames.TlfSpeakerLevelEngine, PublicationScope.Global)]
        public event EventHandler<LevelMsg<LcSpeaker>> TlfSpeakerLevelEngine;

        [EventPublication(EventTopicNames.RdInfoEngine, PublicationScope.Global)]
        public event EventHandler<RangeMsg<RdInfo>> RdInfoEngine;

        [EventPublication(EventTopicNames.RdPageEngine, PublicationScope.Global)]
        public event EventHandler<PageMsg> RdPageEngine;

        [EventPublication(EventTopicNames.RdPttEngine, PublicationScope.Global)]
        public event EventHandler<StateMsg<bool>> RdPttEngine;

        [EventPublication(EventTopicNames.HoldTlfCallEngine, PublicationScope.Global)]
        public event EventHandler<StateMsg<bool>> HoldTlfCallEngine;

        [EventPublication(EventTopicNames.CompletedIntrusionStateEngine, PublicationScope.Global)]
        public event EventHandler<StateMsg<string>> CompletedIntrusionStateEngine;

        [EventPublication(EventTopicNames.BeginingIntrudeToStateEngine, PublicationScope.Global)]
        public event EventHandler<StateMsg<string>> BeginingIntrudeToStateEngine;

        [EventPublication(EventTopicNames.IntrudeToStateEngine, PublicationScope.Global)]
        public event EventHandler<StateMsg<string>> IntrudeToStateEngine;

        [EventPublication(EventTopicNames.RemoveRtxGroup, PublicationScope.Global)]
        public event EventHandler RemoveRtxGroup;

        //[EventPublication(EventTopicNames.RdPosPttStateEngine, PublicationScope.Global)]
        //public event EventHandler<RangeMsg<PttState>> RdPosPttStateEngine;

        //[EventPublication(EventTopicNames.RdPosSquelchStateEngine, PublicationScope.Global)]
        //public event EventHandler<RangeMsg<SquelchState>> RdPosSquelchStateEngine;

        [EventPublication(EventTopicNames.BriefingStateEngine, PublicationScope.Global)]
        public event EventHandler<StateMsg<bool>> BriefingStateEngine;

        [EventPublication(EventTopicNames.PlayingStateEngine, PublicationScope.Global)]
        public event EventHandler<StateMsg<bool>> PlayingStateEngine;

        [EventPublication(EventTopicNames.RdPosStateEngine, PublicationScope.Global)]
        public event EventHandler<RangeMsg<RdState>> RdPosStateEngine;

        [EventPublication(EventTopicNames.RdRtxModificationEndEngine, PublicationScope.Global)]
        public event EventHandler RdRtxModificationEndEngine;

        //[EventPublication(EventTopicNames.RdRtxGroupsEngine, PublicationScope.Global)]
        //public event EventHandler<RangeMsg<RdRtxGroup>> RdRtxGroupsEngine;

        [EventPublication(EventTopicNames.RdSpeakerLevelEngine, PublicationScope.Global)]
        public event EventHandler<LevelMsg<RdSpeaker>> RdSpeakerLevelEngine;

        [EventPublication(EventTopicNames.RdHFLevelEngine, PublicationScope.Global)]
        public event EventHandler<LevelMsg<HfSpeaker>> RdHFLevelEngine;

        [EventPublication(EventTopicNames.RdHeadPhonesLevelEngine, PublicationScope.Global)]
        public event EventHandler<LevelMsg<RdHeadPhones>> RdHeadPhonesLevelEngine;

        [EventPublication(EventTopicNames.RdFrAsignedToOtherEngine, PublicationScope.Global)]
        public event EventHandler<RdFrAsignedToOtherMsg> RdFrAsignedToOtherEngine;

        [EventPublication(EventTopicNames.RdHfFrAssignedEngine, PublicationScope.Global)]
        public event EventHandler<RdHfFrAssigned> RdHfFrAssignedEngine;

        [EventPublication(EventTopicNames.LcInfoEngine, PublicationScope.Global)]
        public event EventHandler<RangeMsg<LcInfo>> LcInfoEngine;

        [EventPublication(EventTopicNames.LcPosStateEngine, PublicationScope.Global)]
        public event EventHandler<RangeMsg<LcState>> LcPosStateEngine;

        [EventPublication(EventTopicNames.TransferStateEngine, PublicationScope.Global)]
        public event EventHandler<StateMsg<FunctionState>> TransferStateEngine;

        //[EventPublication(EventTopicNames.IntrudedByEngine, PublicationScope.Global)]
        //public event EventHandler<StateMsg<string>> IntrudedByEngine;

        //[EventPublication(EventTopicNames.InterruptedByEngine, PublicationScope.Global)]
        //public event EventHandler<StateMsg<string>> InterruptedByEngine;

        [EventPublication(EventTopicNames.ListenStateEngine, PublicationScope.Global)]
        public event EventHandler<ListenPickUpMsg> ListenStateEngine;

        [EventPublication(EventTopicNames.HangToneStateEngine, PublicationScope.Global)]
        public event EventHandler<StateMsg<bool>> HangToneStateEngine;

        [EventPublication(EventTopicNames.TlfIaPosStateEngine, PublicationScope.Global)]
        public event EventHandler<RangeMsg<TlfIaDestination>> TlfIaPosStateEngine;

        [EventPublication(EventTopicNames.ShowNotifMsgEngine, PublicationScope.Global)]
        public event EventHandler<NotifMsg> ShowNotifMsgEngine;

        [EventPublication(EventTopicNames.HideNotifMsgEngine, PublicationScope.Global)]
        public event EventHandler<EventArgs<string>> HideNotifMsgEngine;

        [EventPublication(EventTopicNames.RemoteListenStateEngine, PublicationScope.Global)]
        public event EventHandler<ListenPickUpMsg> RemoteListenStateEngine;

        [EventPublication(EventTopicNames.ConfListEngine, PublicationScope.Global)]
        public event EventHandler<RangeMsg<string>> ConfListEngine;

        [EventPublication(EventTopicNames.PermissionsEngine, PublicationScope.Global)]
        public event EventHandler<StateMsg<Permissions>> PermissionsEngine;

        [EventPublication(EventTopicNames.AgendaChangedEngine, PublicationScope.Global)]
        public event EventHandler<RangeMsg<Number>> AgendaChangedEngine;

        //[EventPublication(EventTopicNames.NumberBookChangedEngine, PublicationScope.Global)]
        //public event EventHandler<RangeMsg<Area>> NumberBookChangedEngine;

        [EventPublication(EventTopicNames.HistoricalOfLocalCallsEngine, PublicationScope.Global)]
        public event EventHandler<RangeMsg<LlamadaHistorica>> HistoricalOfLocalCallsEngine;

        [EventPublication(EventTopicNames.SelCalResponseEngine, PublicationScope.Global)]
        public event EventHandler<StateMsg<string>> SelCalResponseEngine;

#if _HF_GLOBAL_STATUS_
        [EventPublication(EventTopicNames.HfGlobalStatusEngine, PublicationScope.Global)]
        public event EventHandler<StateMsg<string>> HfGlobalStatusEngine;
#endif

        [EventPublication(EventTopicNames.SiteManagerEngine, PublicationScope.Global)]
        public event EventHandler<StateMsg<bool>> SiteManagerEngine;

        [EventPublication(EventTopicNames.SiteChangedResultEngine, PublicationScope.Global)]
        public event EventHandler<StateMsg<string>> SiteChangedResultEngine;

        [EventPublication(EventTopicNames.PickUpStateEngine, PublicationScope.Global)]
        public event EventHandler<ListenPickUpMsg> PickUpStateEngine;

        [EventPublication(EventTopicNames.ForwardStateEngine, PublicationScope.Global)]
        public event EventHandler<ListenPickUpMsg> ForwardStateEngine;

        [EventPublication(EventTopicNames.RemoteForwardStateEngine, PublicationScope.Global)]
        public event EventHandler<ListenPickUpMsg> RemoteForwardStateEngine;

        [EventPublication(EventTopicNames.RedirectedCallEngine, PublicationScope.Global)]
        public event EventHandler<PositionIdMsg> RedirectedCallEngine;

        //lalam 211008
        //#2629 Presentar via utilizada en llamada saliente.
        [EventPublication(EventTopicNames.TlfResStateEngine, PublicationScope.Global)]
        public event EventHandler<RangeMsg<TlfInfo>> TlfResStateEngine;

        //RQF36
        [EventPublication(EventTopicNames.ChangedRTXSQU, PublicationScope.Global)]
        public event EventHandler<StateMsg<bool>> ChangedRTXSQU;
        #endregion

        public void Run()
        {
            try
            {
#if _NEWSTART_
                Top.Init();

                if (Top.Registry != null) Top.Registry.ChannelError += OnSpreadChannelError;

                if (Top.Hw != null)
                {
                    Top.Hw.JacksChangedHw += OnJacksChanged;
                    Top.Hw.SpeakerChangedHw += OnSpeakerChanged;
                    Top.Hw.SpeakerExtChangedHw += OnSpeakerExtChanged;
                }

                if (Top.Mixer != null) Top.Mixer.SplitModeChanged += OnSplitModeChanged;

                if (Top.Cfg != null)
                {
                    Top.Cfg.ConfigChanged += OnConfigChanged;
                    Top.Cfg.ProxyStateChangeCfg += OnProxyStateChange;
                }
                if (Top.Tlf != null)
                {
                    Top.Tlf.NewPositions += OnNewTlfPositions;
                    Top.Tlf.PositionsChanged += OnTlfPositionsChanged;
                    Top.Tlf.IaPositionsChanged += OnTlfIaPositionsChanged;
                    Top.Tlf.HangToneChanged += OnTlfHangToneChanged;
                    Top.Tlf.ConfListChanged += OnTlfConfListChanged;
                    Top.Tlf.Transfer.StateChanged += OnTransferChanged;
                    Top.Tlf.Transfer.SetSnmpString += OnSetSnmpString;
                    Top.Tlf.Listen.ListenChanged += OnListenChanged;
                    Top.Tlf.Listen.RemoteListenChanged += OnRemoteListenChanged;
                    Top.Tlf.Listen.SetSnmpString += OnSetSnmpString;
                    Top.Tlf.PickUp.PickUpChanged += OnPickUpChanged;
                    Top.Tlf.PickUp.SipMessageReceived += OnSipMessageReceived;
                    Top.Tlf.PickUp.PickUpError += OnFunctionError;
                    Top.Tlf.PickUp.SetSnmpString += OnSetSnmpString;
                    Top.Tlf.CompletedIntrusion += OnCompletedIntrusion;
                    Top.Tlf.IntrudeToStateEngine += OnIntrudeToStateEngine;
                    Top.Tlf.BegeningIntrudeTo += OnBeginingIntrudeTo;
                    Top.Tlf.IntrudedTo += OnIntrudedTo;
                    Top.Tlf.SetSnmpString += OnSetSnmpString;
                    Top.Tlf.SendSnmpTrapString += OnSendSnmpTrapString;
                    Top.Tlf.HistoricalOfLocalCallsEngine += OnLoadHistoricalOfLocalCalls;
                    Top.Tlf.RedirectedCall += OnRedirectedCall;
                    Top.Tlf.Forward.ForwardChanged += OnForwardChanged;
                    Top.Tlf.Forward.RemoteForwardChanged += OnRemoteForwardChanged;
                    Top.Tlf.Forward.ForwardError += OnFunctionError;
                    Top.Tlf.Forward.SetSnmpString += OnSetSnmpString;
                    //LALM 211007
                    Top.Tlf.ResourceChanged += OnTlfResourceChanged;
                }
                if (Top.Lc != null)
                {
                    Top.Lc.NewPositions += OnNewLcPositions;
                    Top.Lc.PositionsChanged += OnLcPositionsChanged;
                    Top.Lc.SetSnmpString += OnSetSnmpString;
                    Top.Lc.HoldTlfCallEvent += OnHoldTlfCall;
                }

                if (Top.Rd != null)
                {
                    Top.Rd.NewPositions += OnNewRdPositions;
                    Top.Rd.PositionsChanged += OnRdPositionsChanged;
                    Top.Rd.TxAssign += OnRdTxAssign;
                    Top.Rd.TxHfAssign += OnRdTxHfAssign;
                    Top.Rd.PttChanged += OnPttChanged;
                    Top.Rd.PTTMaxTime += OnPTTMaxTime;
                    Top.Rd.HoldTlfCall += OnHoldTlfCall;
                    Top.Rd.SetSnmpString += OnSetSnmpString;
                    Top.Rd.SetSnmpInt += OnSetSnmpInt;
                    Top.Rd.SelCalMessage += OnSelCalMessage;
                    Top.Rd.SiteChangedResultMessage += OnSiteChangedResultMessage;
                    Top.Rd.AudioViaNotAvailable += OnAudioViaNotAvailable;
#if _HF_GLOBAL_STATUS_
                    Top.Rd.HFGlobalStatus += OnHfGlobalStatus;
#endif
                }
                if (Top.Hw != null) Top.Hw.SetSnmpInt += OnSetSnmpInt;
                if (Top.Recorder != null) Top.Recorder.SetSnmpString += OnSetSnmpString;
                if (Top.Replay != null) Top.Replay.SetSnmpString += OnSetSnmpString;
                if (Top.Recorder != null) Top.Recorder.BriefingChanged += OnBriefingChanged;
                if (Top.Recorder != null) Top.Recorder.FileRecordedChanged += OnFileRecorderChanged;

                if (Top.Replay != null) Top.Replay.PlayingChanged += OnPlayingChanged;

                Top.Start();

                Top.PublisherThread.Enqueue(EventTopicNames.ConnectionStateEngine, delegate ()
                {
                    General.SafeLaunchEvent(ConnectionStateEngine, this, new EngineConnectionStateMsg(true));
                });

                if (!Top.Mixer.BuzzerEnabled)
                {
                    Top.PublisherThread.Enqueue(EventTopicNames.BuzzerStateEngine, delegate ()
                    {
                        General.SafeLaunchEvent(BuzzerStateEngine, this, new StateMsg<bool>(false));
                    });
                }
#else
				Top.Init();

				Top.Registry.ChannelError += OnSpreadChannelError;

				Top.Hw.JacksChanged += OnJacksChanged;

				Top.Mixer.SplitModeChanged += OnSplitModeChanged;

				Top.Cfg.ConfigChanged += OnConfigChanged;

				Top.Tlf.NewPositions += OnNewTlfPositions;
				Top.Tlf.PositionsChanged += OnTlfPositionsChanged;
				Top.Tlf.IaPositionsChanged += OnTlfIaPositionsChanged;
				Top.Tlf.HangToneChanged += OnTlfHangToneChanged;
				Top.Tlf.ConfListChanged += OnTlfConfListChanged;
				Top.Tlf.Transfer.StateChanged += OnTransferChanged;
				Top.Tlf.Transfer.SetSnmpString += OnSetSnmpString;
				Top.Tlf.Listen.ListenChanged += OnListenChanged;
				Top.Tlf.Listen.RemoteListenChanged += OnRemoteListenChanged;
				Top.Tlf.Listen.SetSnmpString += OnSetSnmpString;
				Top.Tlf.CompletedIntrusion += OnCompletedIntrusion;
				Top.Tlf.BegeningIntrudeTo += OnBeginingIntrudeTo;
				Top.Tlf.IntrudedTo += OnIntrudedTo;
				Top.Tlf.SetSnmpString += OnSetSnmpString;
				Top.Tlf.SendSnmpTrapString += OnSendSnmpTrapString;
                Top.Tlf.HistoricalOfLocalCallsEngine += OnLoadHistoricalOfLocalCalls;

				Top.Lc.NewPositions += OnNewLcPositions;
				Top.Lc.PositionsChanged += OnLcPositionsChanged;
				Top.Lc.SetSnmpString += OnSetSnmpString;

				Top.Rd.NewPositions += OnNewRdPositions;
				Top.Rd.PositionsChanged += OnRdPositionsChanged;
				Top.Rd.TxAssign += OnRdTxAssign;
                Top.Rd.TxHfAssign += OnRdTxHfAssign;
				Top.Rd.PttChanged += OnPttChanged;
				Top.Rd.PTTMaxTime += OnPTTMaxTime;
				Top.Rd.HoldTlfCall += OnHoldTlfCall;
				Top.Rd.SetSnmpString += OnSetSnmpString;
				Top.Rd.SetSnmpInt += OnSetSnmpInt;
                Top.Rd.SelCalMessage += OnSelCalMessage;
				Top.Hw.SetSnmpInt += OnSetSnmpInt;
                Top.Recorder.SetSnmpString += OnSetSnmpString;
                Top.Replay.SetSnmpString += OnSetSnmpString;
                Top.Recorder.BriefingChanged += OnBriefingChanged;
                Top.Replay.PlayingChanged += OnPlayingChanged;

				Top.Start();

				Top.PublisherThread.Enqueue(EventTopicNames.ConnectionStateEngine, delegate()
				{
					General.SafeLaunchEvent(ConnectionStateEngine, this, new EngineConnectionStateMsg(true));
				});

				if (!Top.Mixer.BuzzerEnabled)
				{
					Top.PublisherThread.Enqueue(EventTopicNames.BuzzerStateEngine, delegate()
					{
						General.SafeLaunchEvent(BuzzerStateEngine, this, new StateMsg<bool>(false));
					});
				}
#endif
            }
            catch (Exception ex)
            {
                _Logger.Fatal("ERROR inicializando ULISES-TA: \n{0}", ex.Message);
            }
            finally
            {
                /**
                 * AGL 17072012. Trata de Rellenar la tabla de Dependencias desde un fichero local.
                 * */
                SetDependences_from_xls();
                /**
                 * Fin de la Modificacion */
            }
        }

        public void Stop()
        {
            Top.End();
        }

        #region IEngineCmdManagerService Members

        public string Name
        {
            get { return "Cd40"; }
        }

        public void GetEngineInfo()
        {
            Top.PublisherThread.Enqueue(EventTopicNames.ShowNotifMsgEngine, delegate ()
            {
                DirectoryInfo di = new DirectoryInfo(".");
                FileInfo[] fi = di.GetFiles("*.exe", SearchOption.TopDirectoryOnly);
                FileInfo[] f2 = di.GetFiles("*.dll", SearchOption.TopDirectoryOnly);
                int lenfi = fi.Length;
                Array.Resize<FileInfo>(ref fi, lenfi + f2.Length);
                Array.Copy(f2, 0, fi, lenfi, f2.Length); 
                if (fi.Length == 0)
                    return;

                FileInfo lastInfo = fi[fi.Length - 1];
                FileInfo ftmp;
                for (int j =0;j<fi.Length-1;j++)
                for (int i =j;i< fi.Length-1;i++)
                {
                    if (fi[i].LastWriteTime < fi[i + 1].LastWriteTime)
                    {
                        ftmp = fi[i];
                        fi[i] = fi[i + 1];
                        fi[i + 1] = ftmp;
                    }
                }
                System.IO.FileInfo[] patron = { fi[0], fi[1], fi[2] };// = { f2020, f2020, f2020};
                string text = "";
                foreach (System.IO.FileInfo f in patron)
                {
                    string Name = f.Name;
                    int maxlen = 32;
                    if (f.Name.Length > maxlen-4)
                        Name = f.Name.Substring(0, maxlen-4) + "*" + f.Name.Substring(f.Name.Length-4);
                    text += Name + ":" + f.LastWriteTime.ToString() + "\r\n";
                }
                NotifMsg msg = new NotifMsg("Informacion", "Versiones", text, 0, MessageType.Information, MessageButtons.Ok);
                General.SafeLaunchEvent(ShowNotifMsgEngine, this, msg);
            });
        }

        public bool HayConferencia()
        {
            return Top.Tlf.HayConferencia;
        }

        public void SetSplitMode(SplitMode mode)
        {
            Top.WorkingThread.Enqueue("SetSplitMode", delegate ()
            {
                if (Top.Mixer.SplitMode == mode)
                {
                    Top.PublisherThread.Enqueue(EventTopicNames.SplitModeEngine, delegate ()
                    {
                        General.SafeLaunchEvent(SplitModeEngine, this, new StateMsg<SplitMode>(mode));
                    });
                }
                else if (Top.Lc.Activity || (Top.Rd.PttSource != PttSource.NoPtt) || Top.Tlf.Activity())
                {
                    int _BadOperationTone = SipAgent.CreateWavPlayer("Resources/Tones/Falsa_Maniobra.wav", true);
                    Top.Mixer.Link(_BadOperationTone, MixerDev.SpkRd, MixerDir.Send, Mixer.RD_PRIORITY, FuentesGlp.RxRadio);
                    Top.Mixer.SetVolumeTones(CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER);//#5829

                    Top.PublisherThread.Enqueue(EventTopicNames.ShowNotifMsgEngine, delegate ()
                    {
                        NotifMsg msg = new NotifMsg("SplitModeError", Resources.BadOperation, Resources.ActivityError, 0, MessageType.Error, MessageButtons.Ok);
                        General.SafeLaunchEvent(ShowNotifMsgEngine, this, msg);
                    });

                    Wait(500);
                    Top.Mixer.Unlink(_BadOperationTone);
                    SipAgent.DestroyWavPlayer(_BadOperationTone);
                }
                else if (!Top.Mixer.SetSplitMode(mode))
                {
                    int _BadOperationTone = SipAgent.CreateWavPlayer("Resources/Tones/Falsa_Maniobra.wav", true);
                    Top.Mixer.Link(_BadOperationTone, MixerDev.SpkRd, MixerDir.Send, Mixer.RD_PRIORITY, FuentesGlp.RxRadio);

                    Top.PublisherThread.Enqueue(EventTopicNames.ShowNotifMsgEngine, delegate ()
                    {
                        NotifMsg msg = new NotifMsg("SplitModeError", Resources.BadOperation, Resources.SplitModeError, 0, MessageType.Error, MessageButtons.Ok);
                        General.SafeLaunchEvent(ShowNotifMsgEngine, this, msg);
                    });

                    Wait(500);
                    Top.Mixer.Unlink(_BadOperationTone);
                    SipAgent.DestroyWavPlayer(_BadOperationTone);
                }
            });
        }

        public void SetBuzzerState(bool enabled)
        {
            Top.WorkingThread.Enqueue("SetBuzzerState", delegate ()
            {
                if (Top.Mixer.SetBuzzerState(enabled))
                {
                    Top.PublisherThread.Enqueue(EventTopicNames.BuzzerStateEngine, delegate ()
                    {
                        General.SafeLaunchEvent(BuzzerStateEngine, this, new StateMsg<bool>(enabled));
                    });
                }
            });
        }

        public void SetBuzzerLevel(int level)
        {
            Top.WorkingThread.Enqueue("SetBuzzerLevel", delegate ()
            {
                if (Top.Mixer.SetBuzzerLevel(level))
                {
                    Top.PublisherThread.Enqueue(EventTopicNames.BuzzerLevelEngine, delegate ()
                    {
                        General.SafeLaunchEvent(BuzzerLevelEngine, this, new LevelMsg<Buzzer>(level));
                    });
                }
            });
        }

        public void SetRdHeadPhonesLevel(int level)
        {
            Top.WorkingThread.Enqueue("SetRdHeadPhonesLevel", delegate ()
            {
                if (Top.Mixer.SetRdHeadPhonesLevel(level))
                {
                    Top.PublisherThread.Enqueue(EventTopicNames.RdHeadPhonesLevelEngine, delegate ()
                    {
                        General.SafeLaunchEvent(RdHeadPhonesLevelEngine, this, new LevelMsg<RdHeadPhones>(level));
                    });
                }
            });
        }

        public void SetRdSpeakerLevel(int level)
        {
            Top.WorkingThread.Enqueue("SetRdSpeakerLevel", delegate ()
            {
                if (Top.Mixer.SetRdSpeakerLevel(level))
                {
                    Top.PublisherThread.Enqueue(EventTopicNames.RdSpeakerLevelEngine, delegate ()
                    {
                        General.SafeLaunchEvent(RdSpeakerLevelEngine, this, new LevelMsg<RdSpeaker>(level));
                    });
                }
            });
        }

        public void SetRdHfSpeakerLevel(int level)
        {
            Top.WorkingThread.Enqueue("SetRdHfSpeakerLevel", delegate ()
            {
                if (Top.Mixer.SetHfSpeakerLevel(level))
                {
                    Top.PublisherThread.Enqueue(EventTopicNames.RdHFLevelEngine, delegate ()
                    {
                        General.SafeLaunchEvent(RdHFLevelEngine, this, new LevelMsg<HfSpeaker>(level));
                    });
                }
            });
        }

        public void SetRdPage(int oldPage, int newPage, int numPosByPage)
        {
            Top.WorkingThread.Enqueue("SetRdPage", delegate ()
            {
                Top.Rd.SetRdPage(oldPage, newPage, numPosByPage);

                Top.PublisherThread.Enqueue(EventTopicNames.RdPageEngine, delegate ()
                {
                    General.SafeLaunchEvent(RdPageEngine, this, new PageMsg(newPage));
                });
            });
        }

        public void SetRdPtt(bool on)
        {
            Top.WorkingThread.Enqueue("SetRdPtt", delegate ()
            {
                //Se permite Ptt SW desde HMI
                if (!on || AllowRd(true))
                {
                    if (Top.Recorder.Briefing)  // Si se hace PTT mientras est� abierta una sesi�n briefing, esta se corta
                        Top.Recorder.SessionGlp(FuentesGlp.Briefing, false);

                    Top.Rd.SetPtt(on, PttSource.Hmi);
                }
            });
        }

        public void SetRdRx(int id, bool on, bool forced)
        {
            Top.WorkingThread.Enqueue("SetRdRx", delegate ()
            {
                //if (!on || AllowRd())
                if (forced || (AllowRd() && (Twr() || AllowBriefing())))
                {
                    Top.Rd.SetRx(id, on, forced);
                }
            });
        }

        public void SetRdTx(int id, bool on)
        {
            Top.WorkingThread.Enqueue("SetRdTx", delegate ()
            {
                //if (!on || AllowRd())
                if (AllowRd(id) && (Twr() || AllowBriefing()))
                {
                    Top.Rd.SetTx(id, on);
                }
            });
        }

        public void ForceTxOff(int id)
        {
            Top.WorkingThread.Enqueue("ForceTxOff", delegate ()
            {
                Top.Rd.ForceTxOff(id);
            });
        }

        public void ConfirmRdTx(int id)
        {
            Top.WorkingThread.Enqueue("ConfirmRdTx", delegate ()
            {
                if (AllowRd() && (Twr() || AllowBriefing()))
                {
                    Top.Rd.ConfirmTx(id);
                }
            });
        }

        public void ResetRdPosition(int id)
        {
            Top.WorkingThread.Enqueue("ResetRdPositon", delegate ()
            {
                Top.Rd.SetQuiet(id);
            });
        }

        public void SetRdAudio(int id, RdRxAudioVia audioVia, bool forced)
        {
            Top.WorkingThread.Enqueue("SetRdAudio", delegate ()
            {
                if (forced || (AllowRd() && (Twr() || AllowBriefing())))
                {
                    Top.Rd.SetAudioVia(id, audioVia);
                }
            });
        }
        public bool Twr()
        {
#if _MEZCLADOR_ASECNA_
            return false;
#endif
#if _MEZCLADOR_TWR_
            return true;
#endif
        }
            public void NextRdAudio(int id)
        {
            Top.WorkingThread.Enqueue("SetRdAudio", delegate ()
            {
                if (AllowRd() && (Twr() || AllowBriefing()) )
                {
                    Top.Rd.NextAudioVia(id);
                }
            });
        }
        public void SetManagingSite(bool managing)
        {
            Top.WorkingThread.Enqueue("ManagingSite", delegate ()
            {
                Top.Rd.SiteManaging = managing;
            });
        }

        public void ChangeSite(int pos, string alias)
        {
            Top.WorkingThread.Enqueue("ChangeSite", delegate ()
            {
                Top.Rd.ChangSite(pos, alias);
            });
        }

        public string ChangingPositionSite(int id)
        {
            string alias = string.Empty;
            if (Top.Rd.SiteManaging)
            {
                alias = Top.Rd.ChangeSite(id);
            }

            return alias;
        }

        /** */
        public void SetRtxGroup(int rtxGroup, Dictionary<int, RtxState> newRtxGroup, bool force = false)
        {
            Top.WorkingThread.Enqueue("SetRtxGroup", delegate ()
            {
                bool disableGroup = true;

                foreach (RtxState st in newRtxGroup.Values)
                {
                    if (st != RtxState.Delete)
                    {
                        disableGroup = false;
                        break;
                    }
                }
                /** */
                if (disableGroup || force || (AllowRd() && AllowRtx(newRtxGroup)))
                {
                    Top.Rd.SetRtxGroup(rtxGroup, newRtxGroup);
                }

                Top.PublisherThread.Enqueue(EventTopicNames.ConnectionStateEngine, delegate ()
                {
                    General.SafeLaunchEvent(RdRtxModificationEndEngine, this);
                });
            });
        }

        public void SetLc(int id, bool on)
        {
            Top.WorkingThread.Enqueue("SetLc", delegate ()
            {
                //Cuando on == false i.e. se suelta la tecla, 
                //se continua siempre para poder colgar la llamada
                if (AllowLc() || !on)
                {
                    // Si la telefon�a va por altavoz y hay llamada de LC se quita 
                    // el estado "en espera de cuelgue"
                    if (Top.Mixer.RxTlfAudioVia == TlfRxAudioVia.Speaker)
                        Top.Tlf.SetHangToneOff();
                    Top.Lc.SetLc(id, on);
                }
            });
        }

        public void SetLcSpeakerLevel(int level)
        {
            Top.WorkingThread.Enqueue("SetLcSpeakerLevel", delegate ()
            {
                if (Top.Mixer.SetLcSpeakerLevel(level))
                {
                    Top.PublisherThread.Enqueue(EventTopicNames.LcSpeakerLevelEngine, delegate ()
                    {
                        General.SafeLaunchEvent(LcSpeakerLevelEngine, this, new LevelMsg<LcSpeaker>(level));
                    });
                }
            });
        }

        public void SetTlfHeadPhonesLevel(int level)
        {
            Top.WorkingThread.Enqueue("SetTlfHeadPhonesLevel", delegate ()
            {
                if (Top.Mixer.SetTlfHeadPhonesLevel(level))
                {
                    Top.PublisherThread.Enqueue(EventTopicNames.TlfHeadPhonesLevelEngine, delegate ()
                    {
                        General.SafeLaunchEvent(TlfHeadPhonesLevelEngine, this, new LevelMsg<TlfHeadPhones>(level));
                    });
                }
            });
        }

        public void SetTlfSpeakerLevel(int level)
        {
            Top.WorkingThread.Enqueue("SetTlfSpeakerLevel", delegate ()
            {
                if (Top.Mixer.SetTlfSpeakerLevel(level))
                {
                    Top.PublisherThread.Enqueue(EventTopicNames.TlfSpeakerLevelEngine, delegate ()
                    {
                        General.SafeLaunchEvent(TlfSpeakerLevelEngine, this, new LevelMsg<LcSpeaker>(level));
                    });
                }
            });
        }

        public bool SetAudioViaTlf(bool speaker)
        {
            bool done = true;
            //No se permite cambiar a altavoz por telefon�a si  
            //est� en uso por LC
            if ((Top.Lc.AnyActiveLcRx) && speaker)
            {
                int _BadOperationTone = SipAgent.CreateWavPlayer("Resources/Tones/Falsa_Maniobra.wav", true);
                Top.Mixer.Link(_BadOperationTone, MixerDev.SpkRd, MixerDir.Send, Mixer.RD_PRIORITY, FuentesGlp.RxRadio);
                Top.Mixer.SetVolumeTones(CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER);//#5829
                Top.PublisherThread.Enqueue(EventTopicNames.ShowNotifMsgEngine, delegate ()
                {
                    NotifMsg msg = new NotifMsg(Resources.LCSpeakerBusy, Resources.BadOperation, Resources.LCSpeakerBusy, 3000, MessageType.Error, MessageButtons.Ok);
                    General.SafeLaunchEvent(ShowNotifMsgEngine, this, msg);
                });

                Wait(500);
                Top.Mixer.Unlink(_BadOperationTone);
                SipAgent.DestroyWavPlayer(_BadOperationTone);
                done = false;
            }
            else
                Top.WorkingThread.Enqueue("SetAudioViaTlf", delegate ()
                {
                    Top.Mixer.SetTlfAudioVia(speaker);
                });
            return done;
        }

        public void ModoSoloAltavoces()
        {
            Top.Mixer.ModoSoloAltavoces = true;
        }

        public void SetDoubleRadioSpeaker()
        {
            Top.Rd.DoubleRadioSpeaker = true;
        }

        public void BeginTlfCall(int id, bool prio)
        {
            Top.WorkingThread.Enqueue("BeginTlfCall", delegate ()
            {
                if (AllowTlf())
                {
                    if (Top.Recorder.Briefing)  // Si se hace una llamada mientras est� abierta una sesi�n briefing, esta se corta
                        Top.Recorder.SessionGlp(FuentesGlp.Briefing, false);
                    if (Top.Replay.Replaying)
                        Top.Replay.DoFunction(Model.Module.BusinessEntities.FunctionReplay.Stop, ViaReplay.None, null, 0);

                    Top.Tlf.Call(id, prio);
                }
            });
        }
        /// <summary>
        /// Llamada desde teclado o AI
        /// La prioridad s�lo se gestiona para prefijos 0 y 3 (destinos ATS)
        /// </summary>
        /// <param name="number"></param>
        /// <param name="prio"></param>
        /// <param name="literal"></param>
        public void BeginTlfCall(string number, bool prio, string literal)
        {
            // LALM 210127
            // Errores #3952
            // Evito la llamada a mi mismo
            if (EsMiNumeroPropio(number))
                return;
            Top.WorkingThread.Enqueue("BeginTlfCall", delegate ()
            {
                if (AllowTlf())
                {
                    if (Top.Recorder.Briefing)  // Si se hace una llamada mientras est� abierta una sesi�n briefing, esta se corta
                        Top.Recorder.SessionGlp(FuentesGlp.Briefing, false);
                    if (Top.Replay.Replaying)
                        Top.Replay.DoFunction(Model.Module.BusinessEntities.FunctionReplay.Stop, ViaReplay.None, null, 0);

                    uint prefix;
                    string dst;

                    if (TryParseNumber(number, out prefix, out dst, ref literal))
                    {
                        if ((prefix != Cd40Cfg.ATS_DST) && (prefix != Cd40Cfg.INT_DST))
                            prio = false;
                        Top.Tlf.Call(prefix, dst, number, prio, literal);
                    }
                }
            });
        }

        //Esta llamada debe intentarse por n�mero y si falla el TryParseNumber, por el id
        //Se utiliza para las llamadas por la 19+1
        //Si viene un literal, lo uso, si no, el literal es el n�mero
        public void BeginTlfCall(string number, bool prio, int id, string literal)
        {
            // LALM 210127
            // Errores #3952
            // Evito la llamada a mi mismo
            if (EsMiNumeroPropio(number))
                return;
            Top.WorkingThread.Enqueue("BeginTlfCall", delegate ()
            {
                if (AllowTlf())
                {
                    if (Top.Recorder.Briefing)  // Si se hace una llamada mientras est� abierta una sesi�n briefing, esta se corta
                        Top.Recorder.SessionGlp(FuentesGlp.Briefing, false);
                    if (Top.Replay.Replaying)
                        Top.Replay.DoFunction(Model.Module.BusinessEntities.FunctionReplay.Stop, ViaReplay.None, null, 0);

                    uint prefix;
                    string dst;

                    if (TryParseNumber(number, out prefix, out dst, ref literal, false))
                    {
                        Top.Tlf.Call(prefix, dst, number, prio, literal);
                    }
                    else
                        Top.Tlf.Call(id, prio);
                }
            });
        }
        
        //211201
        //#2855
        public void Descuelga()
        {
            Top.WorkingThread.Enqueue("Descuelga", delegate ()
            {
                if (AllowTlf())
                    Top.Tlf.Descuelga();

            });
        }
        //*2855

        public void RetryTlfCall(int id)
        {
            Top.WorkingThread.Enqueue("RetryTlfCall", delegate ()
            {
                if (AllowTlf())
                {
                    if (Top.Recorder.Briefing)  // Si se hace una llamada mientras est� abierta una sesi�n briefing, esta se corta
                        Top.Recorder.SessionGlp(FuentesGlp.Briefing, false);
                    if (Top.Replay.Replaying)
                        Top.Replay.DoFunction(Model.Module.BusinessEntities.FunctionReplay.Stop, ViaReplay.None, null, 0);

                    Top.Tlf.RetryCall(id);
                }
            });
        }

        public void AnswerTlfCall(int id)
        {
            Top.WorkingThread.Enqueue("AnswerTlfCall", delegate ()
            {
                if (AllowTlf())
                {
                    if (!Top.Recorder.Briefing && !Top.Replay.Replaying)  // Si no est� abierta una sesi�n briefing, se permite responder a la llamada
                        Top.Tlf.Accept(id, null);
                    else
                    {
                        NotifMsg msg = new NotifMsg(Resources.ActivityError, Resources.BadOperation, Resources.ActivityError, 0, MessageType.Error, MessageButtons.Ok);
                        General.SafeLaunchEvent(ShowNotifMsgEngine, this, msg);
                    }
                }
            });
        }

        public void EndTlfCall(int id, TlfState st)
        {
            EndTlfCall(id);
        }

        public void EndTlfCall(int id)
        {
            Top.WorkingThread.Enqueue("EndTlfCall", delegate ()
            {
                Top.Tlf.HangUp(id);
            });
        }

        public void EndTlfConfCall(int id)
        {
            Top.WorkingThread.Enqueue("EndTlfConfCall", delegate ()
            {
                Top.Tlf.HangUp(id);
            });
        }

        public void RecognizeTlfState(int id)
        {
            Top.WorkingThread.Enqueue("RecognizeTlfState", delegate ()
            {
                Top.Tlf.RecognizeTlfState(id);
            });
        }

        public void EndTlfConf()
        {
            Top.WorkingThread.Enqueue("EndTlfConf", delegate ()
            {
                Top.Tlf.HangUpConf();
            });
        }

        public void EndTlfAll()
        {
            Top.WorkingThread.Enqueue("EndTlfAll", delegate ()
            {
                Top.Tlf.EndTlfAll();
            });
        }

        public void MakeConference(bool viable)
        {
            if (viable)
            {
                Top.WorkingThread.Enqueue("MakeConference", delegate ()
                {
                    Top.Tlf.MakeConference();
                });
            }
            else
            {
                Top.PublisherThread.Enqueue(EventTopicNames.ShowNotifMsgEngine, delegate ()
                {
                    NotifMsg msg = new NotifMsg(Resources.MaxConferenceNumber, Resources.BadOperation, Resources.MaxConferenceNumber, 0, MessageType.Error, MessageButtons.Ok);
                    General.SafeLaunchEvent(ShowNotifMsgEngine, this, msg);
                });
            }
        }

        public void SetHold(bool on)
        {
            Top.WorkingThread.Enqueue("SetHold", delegate ()
            {
                if (AllowTlf() && !Top.Recorder.Briefing && !Top.Replay.Replaying)
                    Top.Tlf.HoldConference(on);
                else
                {
                    NotifMsg msg = new NotifMsg(Resources.ActivityError, Resources.BadOperation, Resources.ActivityError, 0, MessageType.Error, MessageButtons.Ok);
                    General.SafeLaunchEvent(ShowNotifMsgEngine, this, msg);
                }
            });
        }

        //LALM 210127
        // Errores #3952
        // Funcion que comprueba si el numero pertence al puesto
        private bool EsMiNumeroPropio(String NumPropio)
        {
            foreach (StrNumeroAbonado num in Top.Cfg.HostAddresses)
            {
                String miNumero = string.Format("{0:D2}{1}", num.Prefijo, num.NumeroAbonado);
                if (NumPropio == miNumero)
                    return true;
            }
            return false;
        }


        public void ListenTo(int id)
        {
            Top.WorkingThread.Enqueue("ListenTo", delegate ()
            {
                if (AllowTlf())
                {
                    if (!Top.Recorder.Briefing && !Top.Replay.Replaying)
                    {
                        Top.Tlf.Listen.To(id);
                        if (Top.Tlf.Listen.State == FunctionState.Error && _ListenOperationTone == 0)
                        {
                            ListenToError();
                        }
                    }
                    else
                    {
                        NotifMsg msg = new NotifMsg(Resources.ActivityError, Resources.BadOperation, Resources.ActivityError, 0, MessageType.Error, MessageButtons.Ok);
                        General.SafeLaunchEvent(ShowNotifMsgEngine, this, msg);
                    }
                }
            });
        }

        /// <summary>
        /// Acciones que se realizan cuando se detecta un error en la ejecuci�n de Listen.To
        /// </summary>
        private void ListenToError()
        {
            _ListenOperationTone = SipAgent.CreateWavPlayer("Resources/Tones/Falsa_Maniobra.wav", true);
            Top.Mixer.LinkTlf(_ListenOperationTone, MixerDir.Send, Mixer.RD_PRIORITY);
            Wait(500);
            Top.Mixer.Unlink(_ListenOperationTone);
            SipAgent.DestroyWavPlayer(_ListenOperationTone);
            _ListenOperationTone = 0;
        }

        public void ListenTo(string number)
        {
            // LALM 210127
            // Errores #3952
            // Evito la llamada a mi mismo
            if (EsMiNumeroPropio(number))
                return;
            Top.WorkingThread.Enqueue("ListenTo", delegate ()
            {
                if (AllowTlf())
                {
                    if (!Top.Recorder.Briefing && !Top.Replay.Replaying)
                    {
                        uint prefix;
                        string dst, lit = null;

                        if (TryParseNumber(number, out prefix, out dst, ref lit))
                        {
                            Top.Tlf.Listen.To(prefix, dst, number);
                            if (Top.Tlf.Listen.State == FunctionState.Error && _ListenOperationTone == 0)
                            {
                                ListenToError();
                            }
                        }
                    }
                    else
                    {
                        NotifMsg msg = new NotifMsg(Resources.ActivityError, Resources.BadOperation, Resources.ActivityError, 0, MessageType.Error, MessageButtons.Ok);
                        General.SafeLaunchEvent(ShowNotifMsgEngine, this, msg);
                    }
                }
            });
        }

		//LALM 221102 cambiofrecuencia
        public void SetNewFrecuency(int id, string frecuency)
        {
            Top.WorkingThread.Enqueue("SetNewFrecuency", delegate ()
            {
                if (AllowRd(id) && (Twr() || AllowBriefing()))
                {
                    Top.Rd.SetNewFrecuency(id, frecuency);//SetNewFrecuency
                }
            });
        }

        public void CancelListen()
        {
            Top.WorkingThread.Enqueue("CancelListen", delegate ()
            {
                if (Top.Tlf.Listen.State == FunctionState.Executing)
                {
                    Top.Tlf.Listen.Cancel(-1);
                }
            });
        }

        public void RecognizeListenState()
        {
            Top.WorkingThread.Enqueue("RecognizeListenState", delegate ()
            {
                if (Top.Tlf.Listen.State == FunctionState.Error)
                {
                    Top.Tlf.Listen.Cancel(-1);
                }
            });
        }

        public void SetRemoteListen(bool allow, int id)
        {
            Top.WorkingThread.Enqueue("SetRemoteListen", delegate ()
            {
                if (allow)
                {
                    Top.Tlf.Listen.Accept(id);
                }
                else
                {
                    Top.Tlf.Listen.Cancel(id);
                }
            });
        }

        public void PreparePickUp(int id)
        {
            Top.WorkingThread.Enqueue("PreparePickUp", delegate ()
            {
                if (AllowTlf())
                    if (!Top.Recorder.Briefing && !Top.Replay.Replaying)  // Si no est� abierta una sesi�n briefing, se permite iniciar el pickUp
                    {
                        Top.Tlf.PickUp.Prepare(id);
                    }
                    else
                    {
                        NotifMsg msg = new NotifMsg(Resources.ActivityError, Resources.BadOperation, Resources.ActivityError, 0, MessageType.Error, MessageButtons.Ok);
                        General.SafeLaunchEvent(ShowNotifMsgEngine, this, msg);
                    }
            });
        }

        public void PreparePickUp(string number)
        {
            // LALM 210127
            // Errores #3952
            // Evito la llamada a mi mismo
            if (EsMiNumeroPropio(number))
                return;
            Top.WorkingThread.Enqueue("PreparePickUp", delegate ()
            {
                if (AllowTlf())
                    if (!Top.Recorder.Briefing && !Top.Replay.Replaying)  // Si no est� abierta una sesi�n briefing, se permite iniciar el pickUp
                    {
                        uint prefix;
                        string dst, lit = null;

                        if (TryParseNumber(number, out prefix, out dst, ref lit))
                        {
                            Top.Tlf.PickUp.Prepare(prefix, dst, number, lit);
                        }
                    }
                    else
                    {
                        NotifMsg msg = new NotifMsg(Resources.ActivityError, Resources.BadOperation, Resources.ActivityError, 0, MessageType.Error, MessageButtons.Ok);
                        General.SafeLaunchEvent(ShowNotifMsgEngine, this, msg);
                    }
            });
        }
        public void PickUp(int id)
        {
            Top.WorkingThread.Enqueue("PickUp", delegate ()
           {
               if (AllowTlf())
                   if (!Top.Recorder.Briefing && !Top.Replay.Replaying)  // Si no est� abierta una sesi�n briefing, se permite iniciar el pickUp
                   {
                       Top.Tlf.PickUp.Capture(id);
                   }
                   else
                   {
                       NotifMsg msg = new NotifMsg(Resources.ActivityError, Resources.BadOperation, Resources.ActivityError, 0, MessageType.Error, MessageButtons.Ok);
                       General.SafeLaunchEvent(ShowNotifMsgEngine, this, msg);
                   }
           });
        }

        public void CancelPickUp()
        {
            Top.WorkingThread.Enqueue("PickUp", delegate ()
            {
                Top.Tlf.PickUp.Cancel();
            });
        }
        public void PrepareForward(int id)
        {
            Top.WorkingThread.Enqueue("Forward", delegate ()
            {
                if (AllowTlf())
                    if (!Top.Recorder.Briefing && !Top.Replay.Replaying)  // Si no est� abierta una sesi�n briefing, se permite iniciar el pickUp
                    {
                        Top.Tlf.Forward.Prepare(id);
                    }
                    else
                    {
                        NotifMsg msg = new NotifMsg(Resources.ActivityError, Resources.BadOperation, Resources.ActivityError, 0, MessageType.Error, MessageButtons.Ok);
                        General.SafeLaunchEvent(ShowNotifMsgEngine, this, msg);
                    }
            });
        }

        public void PrepareForward(string number)
        {
            // LALM 210127
            // Errores #3952
            // Evito la llamada a mi mismo
            if (EsMiNumeroPropio(number))
                return;
            Top.WorkingThread.Enqueue("Forward", delegate ()
            {
                if (AllowTlf())
                    if (!Top.Recorder.Briefing && !Top.Replay.Replaying)  // Si no est� abierta una sesi�n briefing, se permite iniciar el pickUp
                    {
                        uint prefix;
                        string dst, lit = null;

                        if (TryParseNumber(number, out prefix, out dst, ref lit))
                        {
                            Top.Tlf.Forward.Prepare(prefix, dst, number, lit);
                        }
                    }
                    else
                    {
                        NotifMsg msg = new NotifMsg(Resources.ActivityError, Resources.BadOperation, Resources.ActivityError, 0, MessageType.Error, MessageButtons.Ok);
                        General.SafeLaunchEvent(ShowNotifMsgEngine, this, msg);
                    }
            });
        }
        public void CancelForward()
        {
            Top.WorkingThread.Enqueue("Forward", delegate ()
            {
                Top.Tlf.Forward.Cancel(false);
            });
        }

        //LALM 210224 Nuevo mensaje de canal ocupado por fgrecuencia prioritaria.
        //Errores #4756 visualizacion de mensaje de error por frecuencia prioritaria
        public void SetErrorFP()
        {
            Top.PublisherThread.Enqueue(EventTopicNames.ShowNotifMsgEngine, delegate ()
            {
                String Activity;
                Activity = "Inhabilitaci�n del canal de radio por frecuencia prioritaria.";
                NotifMsg msg = new NotifMsg("ErrorFP", Resources.BadOperation, Activity, 30000, MessageType.Error, MessageButtons.Ok);
                General.SafeLaunchEvent(ShowNotifMsgEngine, this, msg);
            });
        }

        //LALM 210616 Nuevo mensaje de canal ocupado por fgrecuencia prioritaria.
        //Errores #4756 Quitar mensaje de error por frecuencia prioritaria
        public void ResetErrorFP()
        {
            Top.PublisherThread.Enqueue(EventTopicNames.HideNotifMsgEngine, delegate ()
            {
                String Activity;
                Activity = "Inhabilitaci�n del canal de radio por frecuencia prioritaria.";
                Activity = "ErrorFP";
                General.SafeLaunchEvent(HideNotifMsgEngine, this, new EventArgs<string>(Activity));
            });
        }
        // 210224
        public void SetCambioRadio(bool up)
        {
            Top.PublisherThread.Enqueue(EventTopicNames.ShowNotifMsgEngine, delegate ()
            {
                String Activity;
                {
                    Activity = "�Desea cambiar de p�gina radio?";
                    NotifMsg msg = new NotifMsg("Cambio de P�gina de Radio", "Aviso", Activity, 3000, MessageType.Error, MessageButtons.OkCancel);
                    msg.Info = (object)up;
                    General.SafeLaunchEvent(ShowNotifMsgEngine, this, msg);
                }
            });
        }

        /// <summary>
        /// Funcion pra gestionar el aparcado por colisi�n con PTT o LC
        /// Para aparcar la llamada se le pasa el id y se guarda si progresa
        /// para desaparcar se usa el guardado y se borra si progresa
        /// Se protege del caso de llamada aparcada por PTT y simultaneamente LC: 
        /// en este caso se desaparca al terminar ambos, cuando realmente progresa.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="on"></param>
        /// <param name="porPttLc"></param> Indica que la funcion se llama por activacion o desactivacion de Ptt o LC
		public void SetHold(int id, bool on, bool porPttLc = false)
		{
			Top.WorkingThread.Enqueue("SetHold", delegate()
			{
                if (AllowTlf(on, porPttLc))
				{
                    if (!Top.Recorder.Briefing && !Top.Replay.Replaying)
                    {
                        if (on)
                            _HoldedPosId = id;
                        else
                        {
                            if ((id == -1) && (_HoldedPosId != -1))
                                id = _HoldedPosId;
                            _HoldedPosId = -1;
                        }
                        if (id != -1)
                            Top.Tlf.Hold(id, on);
                    }
                    else
                    {
                        NotifMsg msg = new NotifMsg(Resources.ActivityError, Resources.BadOperation, Resources.ActivityError, 0, MessageType.Error, MessageButtons.Ok);
                        General.SafeLaunchEvent(ShowNotifMsgEngine, this, msg);
                    }
				}
			});
		}

		public void TransferTo(int id, bool direct)
		{
			Top.WorkingThread.Enqueue("TransferTo", delegate()
			{
                if (AllowTlf())
				{
                    if (!Top.Recorder.Briefing && !Top.Replay.Replaying)
                        Top.Tlf.Transfer.To(id);
                    else
                    {
                        NotifMsg msg = new NotifMsg(Resources.ActivityError, Resources.BadOperation, Resources.ActivityError, 0, MessageType.Error, MessageButtons.Ok);
                        General.SafeLaunchEvent(ShowNotifMsgEngine, this, msg);
                    }
				}
			});
		}

		public void TransferTo(string number)
		{
            // LALM 210127
            // Errores #3952
            // Evito la llamada a mi mismo
            if (EsMiNumeroPropio(number))
                return;
            Top.WorkingThread.Enqueue("TransferTo", delegate()
			{
                if (AllowTlf())
				{
                    if (!Top.Recorder.Briefing && !Top.Replay.Replaying)
                    {
                        uint prefix;
                        string dst;
                        string lit=null;

                        if (TryParseNumber(number, out prefix, out dst, ref lit))
                        {
                            Top.Tlf.Transfer.To(prefix, dst, number);
                        }
                    }
                    else
                    {
                        NotifMsg msg = new NotifMsg(Resources.ActivityError, Resources.BadOperation, Resources.ActivityError, 0, MessageType.Error, MessageButtons.Ok);
                        General.SafeLaunchEvent(ShowNotifMsgEngine, this, msg);
                    }
				}
			});
		}

		public void CancelTransfer()
		{
			Top.WorkingThread.Enqueue("CancelTransfer", delegate()
			{
				if (Top.Tlf.Transfer.State == FunctionState.Executing)
				{
					Top.Tlf.Transfer.Cancel();
				}
			});
		}

		public void RecognizeTransferState()
		{
			Top.WorkingThread.Enqueue("RecognizeTransferState", delegate()
			{
				if (Top.Tlf.Transfer.State == FunctionState.Error)
				{
					Top.Tlf.Transfer.Cancel();
				}
			});
		}

		public void SetHangToneOff()
		{
			Top.WorkingThread.Enqueue("SetHangToneOff", delegate()
			{
				Top.Tlf.SetHangToneOff();
			});
		}

		public void SendDigit(char ch)
		{
		}

		public void Cancel()
		{
        }

        public void Wait(int ms)
		{
			Top.WorkingThread.Enqueue("Wait", delegate()
			{
				Thread.Sleep(ms);
			});
		}

		public void SendTrapScreenSaver(bool status)
		{
            Top.WorkingThread.Enqueue("SendTrapScreenSaver", delegate()
            {
                Top.SendTrapScreenSaver(status);
                Top.ScreenSaverEnabled = status;
            });
		}

        public void BriefingFunction()
        {
            Top.WorkingThread.Enqueue("BriefingFunction", delegate()
            {
                Top.Recorder.SessionGlp(FuentesGlp.Briefing, !Top.Recorder.Briefing);
            });
        }

        public void FunctionReplay(FunctionReplay function, ViaReplay via, string fileName, long fileLength)
        {
            Top.WorkingThread.Enqueue("FunctionReplay", delegate()
            {
                if (function != Model.Module.BusinessEntities.FunctionReplay.EnableSupervisor &&
                    function != Model.Module.BusinessEntities.FunctionReplay.DisableSupervisor)
                    Top.Replay.DoFunction(function, via, fileName, fileLength);
                else
                    Top.Recorder.EnableSupervisor(function == Model.Module.BusinessEntities.FunctionReplay.EnableSupervisor);
            });
        }

        public void SelCalPrepare(bool prepareOnOff, string code)
        {
            Top.WorkingThread.Enqueue("SelCalPrepare", delegate()
            {
                Top.Rd.PrepareSelCal(prepareOnOff, code);
            });
        }

        // Prueba de envio a servidor mantto. CD30
        public void SendCmdHistoricalEvent(string user, string frec)
        {
            //RadioAsgHist.SendCmdHistorico("192.168.10.3", 9505, user, frec, Top.Rd._ActualState, Top.Rd._OldState);
            //Top.Rd._OldState = Top.Rd._ActualState;
        }


        /** 20180716. Para activar tonos de Falsa Maniobra Radio */
        public void GenerateRadioBadOperationTone(int durationInMsec)
        {
            Top.WorkingThread.Enqueue("GenerateRadioBadOperationTone", delegate()
            {
                Top.Rd.GenerateBadOperationTone(durationInMsec);
            });
        }

        #endregion

		#region Private Members

		private static Logger _Logger = LogManager.GetCurrentClassLogger();

		private int _ListenOperationTone = 0;
        private int _HoldedPosId = -1;

        private void OnSpreadChannelError(object sender, string msg)
		{
			Process.Start("Launcher.exe", "HMI.exe");
		}

        //Evento recibido por cambio en el estado del SCV propio
        private void OnProxyStateChange(object sender, bool state)
        {
            Top.PublisherThread.Enqueue(EventTopicNames.ProxyStateChangedEngine, delegate()
            {
                General.SafeLaunchEvent(ProxyStateChangedEngine, this, new StateMsg<bool>(state));
            });
        }

        private void OnJacksChanged(object sender, JacksStateMsg msg)
		{
			Top.PublisherThread.Enqueue(EventTopicNames.JacksStateEngine, delegate()
			{
				General.SafeLaunchEvent(JacksStateEngine, this, msg);
			});
		}

        //Evento recibido por cambio en la presencia de altavoz radio y LC
        private void OnSpeakerChanged(object sender, JacksStateMsg msg)
        {
            Top.PublisherThread.Enqueue(EventTopicNames.SpeakerStateEngine, delegate()
            {
                General.SafeLaunchEvent(SpeakerStateEngine, this, msg);
            });
        }

        //Evento recibido por cambio en la presencia de altavoz radio HF y cable grabaci�n
        private void OnSpeakerExtChanged(object sender, JacksStateMsg msg)
        {
            Top.PublisherThread.Enqueue(EventTopicNames.SpeakerExtStateEngine, delegate()
            {
                General.SafeLaunchEvent(SpeakerExtStateEngine, this, msg);
            });
        }

		private void OnSplitModeChanged(object sender, SplitMode oldMode)
		{
			SplitMode mode = Top.Mixer.SplitMode;

			Top.PublisherThread.Enqueue(EventTopicNames.SplitModeEngine, delegate()
			{
				General.SafeLaunchEvent(SplitModeEngine, this, new StateMsg<SplitMode>(mode));
			});
		}

		private void OnConfigChanged(object sender)
		{
			string id = Top.Cfg.PositionId;
			Permissions permissions = Top.Cfg.Permissions;

			Top.PublisherThread.Enqueue(EventTopicNames.IsolatedStateEngine, delegate()
			{
				General.SafeLaunchEvent(IsolatedStateEngine, this, new EngineIsolatedStateMsg(false));
			});
			Top.PublisherThread.Enqueue(EventTopicNames.PositionIdEngine, delegate()
			{
				General.SafeLaunchEvent(PositionIdEngine, this, new PositionIdMsg(id));
			});
			Top.PublisherThread.Enqueue(EventTopicNames.PermissionsEngine, delegate()
			{
				General.SafeLaunchEvent(PermissionsEngine, this, new StateMsg<Permissions>(permissions));
			});
            Top.PublisherThread.Enqueue(EventTopicNames.SiteManagerEngine, delegate()
            {
                General.SafeLaunchEvent(SiteManagerEngine, this, new StateMsg<bool>(Top.Cfg.SitesConfiguration()));
            });

            Top.PublisherThread.Enqueue(EventTopicNames.HistoricalOfLocalCallsEngine, delegate()
            {
                // Cargar el fichero de historico de llamadas locales
                List<LlamadaHistorica> listaLlamadas = HistoricalManager.GetHistoricalCalls(id);
                General.SafeLaunchEvent(HistoricalOfLocalCallsEngine, this, new RangeMsg<LlamadaHistorica>(listaLlamadas.ToArray()));
            });
            //RQF36
            Top.PublisherThread.Enqueue(EventTopicNames.ChangedRTXSQU, delegate ()
            {
                General.SafeLaunchEvent(ChangedRTXSQU, this, new StateMsg<bool>(Top.Cfg.PermisoRTXSQ()));
            });


            List<Number> ag = new List<Number>();
			foreach (CfgEnlaceInterno agLink in Top.Cfg.AgLinks)
			{
				Debug.Assert(agLink.ListaRecursos.Count == 1);
				CfgRecursoEnlaceInterno link = agLink.ListaRecursos[0];

				string num = (link.Prefijo == Cd40Cfg.ATS_DST) ? link.NumeroAbonado.ToString() :
					string.Format("{0:D2}{1}", link.Prefijo, link.NumeroAbonado);

				Number number = new Number(num, agLink.Literal);
				ag.Add(number);
			}

			Top.PublisherThread.Enqueue(EventTopicNames.AgendaChangedEngine, delegate()
			{
				General.SafeLaunchEvent(AgendaChangedEngine, this, new RangeMsg<Number>(ag.ToArray()));
			});

            // Eliminar grupos de RTX si los hubiera
            // 20200909. Eliminar esta funci�n.
            //if (Top.Cfg.ResetUsuario)
            //{
            //	SetRdPtt(false);

            //	Top.PublisherThread.Enqueue(EventTopicNames.RemoveRtxGroup, delegate()
            //	{
            //		General.SafeLaunchEvent(RemoveRtxGroup, this);
            //	});
            //}

        }

        private void OnTransferChanged(object sender)
		{
			FunctionState st = Top.Tlf.Transfer.State;

			Top.PublisherThread.Enqueue(EventTopicNames.TransferStateEngine, delegate()
			{
				General.SafeLaunchEvent(TransferStateEngine, this, new StateMsg<FunctionState>(st));
			});
		}

		private void OnListenChanged(object sender, ListenPickUpMsg msg)
		{
			Top.PublisherThread.Enqueue(EventTopicNames.ListenStateEngine, delegate()
			{
				General.SafeLaunchEvent(ListenStateEngine, this, msg);
			});
		}

		private void OnRemoteListenChanged(object sender, ListenPickUpMsg msg)
		{
			Top.PublisherThread.Enqueue(EventTopicNames.RemoteListenStateEngine, delegate()
			{
				General.SafeLaunchEvent(RemoteListenStateEngine, this, msg);
			});
		}

        private void OnPickUpChanged(object sender, ListenPickUpMsg msg)
        {
            Top.PublisherThread.Enqueue(EventTopicNames.PickUpStateEngine, delegate()
            {
                General.SafeLaunchEvent(PickUpStateEngine, this, msg);
            });
        }

        private void OnSipMessageReceived(object sender, string textMsg)
        {
            Top.PublisherThread.Enqueue(EventTopicNames.ShowNotifMsgEngine, delegate()
            {
                NotifMsg msg = new NotifMsg("SipMessage", Resources.Info, textMsg, 20000, MessageType.Information, MessageButtons.Ok);
                General.SafeLaunchEvent(ShowNotifMsgEngine, this, msg);
            });
        }
        private void OnFunctionError(object sender, string textMsg)
        {
            int _BadOperationTone = SipAgent.CreateWavPlayer("Resources/Tones/Falsa_Maniobra.wav", true);
            Top.Mixer.Link(_BadOperationTone, MixerDev.SpkRd, MixerDir.Send, Mixer.RD_PRIORITY, FuentesGlp.RxRadio);
            Top.Mixer.SetVolumeTones(CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER);//#5829
            Top.PublisherThread.Enqueue(EventTopicNames.ShowNotifMsgEngine, delegate()
            {
                NotifMsg msg = new NotifMsg("PickUpError", Resources.Info, textMsg, 20000, MessageType.Error, MessageButtons.Ok);
                General.SafeLaunchEvent(ShowNotifMsgEngine, this, msg);
            });
            Wait(500);
            Top.Mixer.Unlink(_BadOperationTone);
            SipAgent.DestroyWavPlayer(_BadOperationTone);
        }

        private void OnRedirectedCall(object sender, PositionIdMsg position)
        {
            Top.PublisherThread.Enqueue(EventTopicNames.ForwardStateEngine, delegate ()
            {
                General.SafeLaunchEvent(RedirectedCallEngine, this, position);
            });
        }
        private void OnForwardChanged(object sender, ListenPickUpMsg msg)
        {
            Top.PublisherThread.Enqueue(EventTopicNames.ForwardStateEngine, delegate()
            {
                General.SafeLaunchEvent(ForwardStateEngine, this, msg);
            });
        }
        private void OnRemoteForwardChanged(object sender, ListenPickUpMsg remoteName)
        {
            Top.PublisherThread.Enqueue(EventTopicNames.RemoteForwardStateEngine, delegate ()
            {
                General.SafeLaunchEvent(RemoteForwardStateEngine, this, remoteName);
            });
        }

        private void OnNewTlfPositions(object sender, RangeMsg<TlfInfo> tlfPositions)
		{
			Top.PublisherThread.Enqueue(EventTopicNames.TlfInfoEngine, delegate()
            {
				General.SafeLaunchEvent(TlfInfoEngine, this, tlfPositions);
            });
		}
        //lalam 211007
        //#2629 Presentar via utilizada en llamada saliente.
        private void OnTlfResourceChanged(object sender, RangeMsg<TlfInfo> TlfInfoR)
        {
            Top.PublisherThread.Enqueue(EventTopicNames.TlfResStateEngine, delegate ()
            {
                General.SafeLaunchEvent(TlfResStateEngine, this, TlfInfoR);
            });
        }

        private void OnTlfPositionsChanged(object sender, RangeMsg<TlfState> tlfStates)
		{
			Top.PublisherThread.Enqueue(EventTopicNames.TlfPosStateEngine, delegate()
			{
				General.SafeLaunchEvent(TlfPosStateEngine, this, tlfStates);
			});
		}

		public void OnCompletedIntrusion(object sender, StateMsg<string> msg)
		{
			_Logger.Debug("Procesando {0}: {1}", EventTopicNames.CompletedIntrusionStateEngine, msg);

			Top.PublisherThread.Enqueue(EventTopicNames.CompletedIntrusionStateEngine, delegate()
			{
				General.SafeLaunchEvent(CompletedIntrusionStateEngine, this, msg);
			});
		}

        public void OnIntrudeToStateEngine(object sender, StateMsg<string> msg)
        {
            _Logger.Debug("Procesando {0}: {1}", EventTopicNames.CompletedIntrusionStateEngine, msg);

            Top.PublisherThread.Enqueue(EventTopicNames.CompletedIntrusionStateEngine, delegate()
            {
                General.SafeLaunchEvent(IntrudeToStateEngine, this, msg);
            });
        }

		public void OnBeginingIntrudeTo(object sender, StateMsg<string> msg)
		{
			_Logger.Debug("Procesando {0}: {1}", EventTopicNames.BeginingIntrudeToStateEngine, msg);

			Top.PublisherThread.Enqueue(EventTopicNames.BeginingIntrudeToStateEngine, delegate()
			{
				General.SafeLaunchEvent(BeginingIntrudeToStateEngine, this, msg);
			});
		}

		public void OnIntrudedTo(object sender, StateMsg<string> msg)
		{
			_Logger.Debug("Procesando {0}: {1}", EventTopicNames.IntrudeToStateEngine, msg);

			Top.PublisherThread.Enqueue(EventTopicNames.IntrudeToStateEngine, delegate()
			{
				General.SafeLaunchEvent(IntrudeToStateEngine, this, msg);
			});
		}

		private void OnTlfIaPositionsChanged(object sender, RangeMsg<TlfIaDestination> tlfIaStates)
		{
			Top.PublisherThread.Enqueue(EventTopicNames.TlfIaPosStateEngine, delegate()
			{
				General.SafeLaunchEvent(TlfIaPosStateEngine, this, tlfIaStates);
			});
		}

		private void OnTlfHangToneChanged(object sender, bool st)
		{
			Top.PublisherThread.Enqueue(EventTopicNames.HangToneStateEngine, delegate()
			{
				General.SafeLaunchEvent(HangToneStateEngine, this, new StateMsg<bool>(st));
			});
		}

		private void OnTlfConfListChanged(object sender, RangeMsg<string> msg)
		{
			Top.PublisherThread.Enqueue(EventTopicNames.ConfListEngine, delegate()
			{
				General.SafeLaunchEvent(ConfListEngine, this, msg);
			});
		}

		private void OnNewLcPositions(object sender, RangeMsg<LcInfo> lcPositions)
		{
			Top.PublisherThread.Enqueue(EventTopicNames.LcInfoEngine, delegate()
			{
				General.SafeLaunchEvent(LcInfoEngine, this, lcPositions);
			});
		}

		private void OnLcPositionsChanged(object sender, RangeMsg<LcState> lcStates)
		{
			Top.PublisherThread.Enqueue(EventTopicNames.LcPosStateEngine, delegate()
			{
				General.SafeLaunchEvent(LcPosStateEngine, this, lcStates);
			});
		}

		private void OnNewRdPositions(object sender, RangeMsg<RdInfo> rdPositions)
		{
			Top.PublisherThread.Enqueue(EventTopicNames.RdInfoEngine, delegate()
			{
				General.SafeLaunchEvent(RdInfoEngine, this, rdPositions);
			});
		}

		private void OnRdPositionsChanged(object sender, RangeMsg<RdState> rdStates)
		{
			Top.PublisherThread.Enqueue(EventTopicNames.RdPosStateEngine, delegate()
			{
				General.SafeLaunchEvent(RdPosStateEngine, this, rdStates);
			});
		}

		private void OnRdTxAssign(object sender, RdFrAsignedToOtherMsg msg)
		{
			Top.PublisherThread.Enqueue(EventTopicNames.RdFrAsignedToOtherEngine, delegate()
			{
				General.SafeLaunchEvent(RdFrAsignedToOtherEngine, this, msg);
			});
		}

        private void OnRdTxHfAssign(object sender, RdHfFrAssigned msg)
        {
            Top.PublisherThread.Enqueue(EventTopicNames.RdHfFrAssignedEngine, delegate()
            {
                General.SafeLaunchEvent(RdHfFrAssignedEngine, this, msg);
            });
        }

		private void OnPttChanged(object sender, StateMsg<bool> st)
		{
			Top.PublisherThread.Enqueue(EventTopicNames.RdPttEngine, delegate()
			{
				General.SafeLaunchEvent(RdPttEngine, this, st);
			});
		}

        private void OnSelCalMessage(object sender, StateMsg<string> msg)
        {
            Top.PublisherThread.Enqueue(EventTopicNames.SelCalResponseEngine, delegate()
            {
                General.SafeLaunchEvent(SelCalResponseEngine, this, msg);
            });
        }

        private void OnSiteChangedResultMessage(object sender, ChangeSiteRsp msg)
        {
            Top.PublisherThread.Enqueue(EventTopicNames.SiteChangedResultEngine, delegate()
            {
                string mensaje = msg.Alias + "," + msg.Frecuency + "," + msg.resultado;
                StateMsg<string> mes = new StateMsg<string>(mensaje); ;
                General.SafeLaunchEvent(SiteChangedResultEngine, this, mes);
            });
        }

        private void OnAudioViaNotAvailable(object sender, RdRxAudioVia audioVia)
        {
            Top.PublisherThread.Enqueue(EventTopicNames.ShowNotifMsgEngine, delegate()         
            {
                String audioViaText;
                switch (audioVia)
                {
                    case RdRxAudioVia.Speaker:
                    case RdRxAudioVia.HfSpeaker:
                        audioViaText = Resources.Speaker;
                        break;
                    case RdRxAudioVia.HeadPhones:
                        audioViaText = Resources.Headphones;
                        break;
                    default:
                        audioViaText = Resources.Unknown;
                        break;
                }

                NotifMsg msg = new NotifMsg(Resources.AudioViaNotAvailable, Resources.DeviceError , Resources.AudioViaNotAvailable + audioViaText, 3000, MessageType.Error, MessageButtons.Ok);
                General.SafeLaunchEvent(ShowNotifMsgEngine, this, msg);
            });
        }
		 

#if _HF_GLOBAL_STATUS_
        private void OnHfGlobalStatus(object sender, HFStatusCodes status)
        {
            Top.PublisherThread.Enqueue(EventTopicNames.HfGlobalStatusEngine, delegate()
            {
                StateMsg<string> msg = new StateMsg<string>(status.ToString());
                General.SafeLaunchEvent(HfGlobalStatusEngine, this, msg);
            });
        }
#endif

		private void OnHoldTlfCall(object sender, StateMsg<bool> st)
		{
			Top.WorkingThread.Enqueue(EventTopicNames.HoldTlfCallEngine, delegate()
			{
				General.SafeLaunchEvent(HoldTlfCallEngine, this, st);
			});
		}

		private void OnPTTMaxTime(object sender)
		{
			Top.PublisherThread.Enqueue(EventTopicNames.PTTMaxTime, delegate()
			{
				NotifMsg msg = new NotifMsg(Resources.PttCheckedTimeOut, Resources.BadOperation, Resources.PttCheckedTimeOut, 0, MessageType.Error, MessageButtons.Ok);
				General.SafeLaunchEvent(ShowNotifMsgEngine, this, msg);
			});
		}

		private void OnSetSnmpString(object sender, SnmpStringMsg<string, string> st)
		{
			if (Settings.Default.SNMPEnabled != 0)
				SnmpStringObject.Get(st.Oid).Value = st.Value;
		}

		private void OnSetSnmpInt(object sender, SnmpIntMsg<string, int> st)
		{
			if (Settings.Default.SNMPEnabled != 0)
				SnmpIntObject.Get(st.Oid).Value = st.Value;
		}

		private void OnSendSnmpTrapString(object sender, SnmpStringMsg<string, string> st)
		{
			if (Settings.Default.SNMPEnabled != 0)
				SnmpStringObject.Get(st.Oid).SendTrap(st.Value);
		}

        private void OnBriefingChanged(object sender, StateMsg<bool> briefingState)
        {
            Top.PublisherThread.Enqueue(EventTopicNames.BriefingStateEngine, delegate ()
            {
                General.SafeLaunchEvent(BriefingStateEngine, this, briefingState);
            });
        }

        private void OnFileRecorderChanged(object sender, StateMsg<bool> fileState)
        {
            // No se puede llamar a esta funcion desde aqui.
            //221121 para que al finalizar la grabacion aparezca antes el boton habilitado.
            Top.PublisherThread.Enqueue(EventTopicNames.PlayingStateEngine, delegate ()
            {
                StateMsg<bool> valor = new StateMsg<bool>(true);
                General.SafeLaunchEvent(PlayingStateEngine, this, valor);
            });
        }

        private void OnPlayingChanged(object sender, StateMsg<bool> playing)
        {
            Top.PublisherThread.Enqueue(EventTopicNames.PlayingStateEngine, delegate()
            {
                General.SafeLaunchEvent(PlayingStateEngine, this, playing);
            });
        }

        public void OnLoadHistoricalOfLocalCalls(object sender, RangeMsg<LlamadaHistorica> llamadas)
        {
            Top.PublisherThread.Enqueue(EventTopicNames.HistoricalOfLocalCallsEngine, delegate()
            {
                General.SafeLaunchEvent(HistoricalOfLocalCallsEngine, this, llamadas);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [EventSubscription(EventTopicNames.BriefingSessionUI, ThreadOption.Publisher)]
        public void OnBriefingSessionUI(object sender, EventArgs e)
        {
            AllowBriefing();
        }

        /// <param name="porPttLc"></param> Indica que se ha llamado desde SetHold por evento en PTT o LC
        private bool AllowTlf(bool holdOn = false, bool porPttLc = false)
		{
            if (!Top.Hw.InstructorJack && !Top.Hw.AlumnJack)
			{
				int _BadOperationTone = SipAgent.CreateWavPlayer("Resources/Tones/Falsa_Maniobra.wav", true);
				Top.Mixer.Link(_BadOperationTone, MixerDev.SpkRd, MixerDir.Send, Mixer.RD_PRIORITY, FuentesGlp.RxRadio);
                Top.Mixer.SetVolumeTones(CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER);//#5829

                Top.PublisherThread.Enqueue(EventTopicNames.ShowNotifMsgEngine, delegate()
				{
					NotifMsg msg = new NotifMsg(Resources.NoJacksError, Resources.BadOperation, Resources.NoJacksError, 0, MessageType.Error, MessageButtons.Ok);
					General.SafeLaunchEvent(ShowNotifMsgEngine, this, msg);
				});

				Wait(500);
				Top.Mixer.Unlink(_BadOperationTone);
				SipAgent.DestroyWavPlayer(_BadOperationTone);

				return false;
			}

            if (Top.Rd.PttSource != PttSource.NoPtt && Top.Mixer.SplitMode == SplitMode.Off)
			{
                //PTT activado
                //if (Top.Rd.PttSource == PttSource.Alumn && Top.Mixer.SplitMode == SplitMode.Off)
                if (porPttLc == false)
                {
                    //Esta funcion no se ha llamado por un evento de PTT.
                    //En este caso, si el PTT esta activado no se permiten llamadas telefonicas
                    int _BadOperationTone = SipAgent.CreateWavPlayer("Resources/Tones/Falsa_Maniobra.wav", true);
                    Top.Mixer.Link(_BadOperationTone, MixerDev.SpkRd, MixerDir.Send, Mixer.RD_PRIORITY, FuentesGlp.RxRadio);
                    Top.Mixer.SetVolumeTones(CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER);//#5829

                    Top.PublisherThread.Enqueue(EventTopicNames.ShowNotifMsgEngine, delegate()
                    {
                        NotifMsg msg = new NotifMsg(Resources.PttPulsedError, Resources.BadOperation, Resources.PttPulsedError, 0, MessageType.Error, MessageButtons.Ok);
                        General.SafeLaunchEvent(ShowNotifMsgEngine, this, msg);
                    });

                    Wait(500);
                    Top.Mixer.Unlink(_BadOperationTone);
                    SipAgent.DestroyWavPlayer(_BadOperationTone);

                    return false;
                }
                
                 
                //Evita que se desaparque una llamada por fin de LC cuando hay PTT
                if ((holdOn == false) && (Top.Mixer.RxTlfAudioVia == TlfRxAudioVia.Speaker))
                    return false;
			}

            //No se permite llamar por telefon�a si est� seleccionado por altavoz y est� en uso por LC
            // Se permite si lo que quiero es aparcar la llamada.
            if ((holdOn == false) && (Top.Lc.AnyActiveLcRx) && (Top.Mixer.RxTlfAudioVia == TlfRxAudioVia.Speaker))
            {
                int _BadOperationTone = SipAgent.CreateWavPlayer("Resources/Tones/Falsa_Maniobra.wav", true);
                Top.Mixer.Link(_BadOperationTone, MixerDev.SpkRd, MixerDir.Send, Mixer.RD_PRIORITY, FuentesGlp.RxRadio);
                Top.Mixer.SetVolumeTones(CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER);//#5829
                Top.PublisherThread.Enqueue(EventTopicNames.ShowNotifMsgEngine, delegate()
                {
                    NotifMsg msg = new NotifMsg(Resources.LCSpeakerBusy, Resources.BadOperation, Resources.LCSpeakerBusy, 3000, MessageType.Error, MessageButtons.Ok);
                    General.SafeLaunchEvent(ShowNotifMsgEngine, this, msg);
                });

                Wait(500);
                Top.Mixer.Unlink(_BadOperationTone);
                SipAgent.DestroyWavPlayer(_BadOperationTone);

                return false;
            }

            // no se permite llamar si est� seleccionado el altavoz, no est� presente y estamos en modo solo altavoz
            if (Top.Mixer.ModoSoloAltavoces && 
                !Top.Hw.LCSpeaker && 
                Top.Mixer.RxTlfAudioVia == TlfRxAudioVia.Speaker)
            {
                int _BadOperationTone = SipAgent.CreateWavPlayer("Resources/Tones/Falsa_Maniobra.wav", true);
                Top.Mixer.Link(_BadOperationTone, MixerDev.SpkRd, MixerDir.Send, Mixer.RD_PRIORITY, FuentesGlp.RxRadio);
                Top.Mixer.SetVolumeTones(CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER);//#5829
                Top.PublisherThread.Enqueue(EventTopicNames.ShowNotifMsgEngine, delegate()
                {
                    NotifMsg msg = new NotifMsg(Resources.AudioViaNotAvailable, Resources.DeviceError, Resources.AudioViaNotAvailable + Resources.Speaker, 3000, MessageType.Error, MessageButtons.Ok);
                    General.SafeLaunchEvent(ShowNotifMsgEngine, this, msg);
                });
                Wait(500);
                Top.Mixer.Unlink(_BadOperationTone);
                SipAgent.DestroyWavPlayer(_BadOperationTone);

                return false;
            }

            return true;
		}

		private bool AllowLc()
		{
			if (!Top.Hw.InstructorJack && !Top.Hw.AlumnJack)
			{
				int _BadOperationTone = SipAgent.CreateWavPlayer("Resources/Tones/Falsa_Maniobra.wav", true);
				Top.Mixer.Link(_BadOperationTone, MixerDev.SpkRd, MixerDir.Send, Mixer.RD_PRIORITY, FuentesGlp.RxRadio);
                Top.Mixer.SetVolumeTones(CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER);//#5829

                Top.PublisherThread.Enqueue(EventTopicNames.ShowNotifMsgEngine, delegate()
				{
					NotifMsg msg = new NotifMsg(Resources.NoJacksError, Resources.BadOperation, Resources.NoJacksError, 3000, MessageType.Error, MessageButtons.Ok);
					General.SafeLaunchEvent(ShowNotifMsgEngine, this, msg);
				});

				Wait(500);
				Top.Mixer.Unlink(_BadOperationTone);
				SipAgent.DestroyWavPlayer(_BadOperationTone);

				return false;
			}

            if (Top.Recorder.Briefing)  // Si se hace PTT mientras est� abierta una sesi�n briefing, esta se corta
                Top.Recorder.SessionGlp(FuentesGlp.Briefing, false);
 
            // no se permite llamar si el altavoz no est� presente
            if (!Top.Hw.LCSpeaker)
            {
                int _BadOperationTone = SipAgent.CreateWavPlayer("Resources/Tones/Falsa_Maniobra.wav", true);
                Top.Mixer.Link(_BadOperationTone, MixerDev.SpkRd, MixerDir.Send, Mixer.RD_PRIORITY, FuentesGlp.RxRadio);
                Top.Mixer.SetVolumeTones(CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER);//#5829
                Top.PublisherThread.Enqueue(EventTopicNames.ShowNotifMsgEngine, delegate()
                {
                    NotifMsg msg = new NotifMsg(Resources.AudioViaNotAvailable, Resources.DeviceError, Resources.AudioViaNotAvailable + Resources.Speaker, 3000, MessageType.Error, MessageButtons.Ok);
                    General.SafeLaunchEvent(ShowNotifMsgEngine, this, msg);
                });

                Wait(500);
                Top.Mixer.Unlink(_BadOperationTone);
                SipAgent.DestroyWavPlayer(_BadOperationTone);
                return false;
            }
            
            return true;
		}

        private bool AllowBriefing()
        {
            if (Top.Recorder.Briefing || Top.Replay.Replaying )
            {

                int _BadOperationTone = SipAgent.CreateWavPlayer("Resources/Tones/Falsa_Maniobra.wav", true);
                Top.Mixer.Link(_BadOperationTone, MixerDev.SpkRd, MixerDir.Send, Mixer.RD_PRIORITY, FuentesGlp.RxRadio);
                Top.Mixer.SetVolumeTones(CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER);//#5829

                Top.PublisherThread.Enqueue(EventTopicNames.ShowNotifMsgEngine, delegate()
                {
                    NotifMsg msg = new NotifMsg(Resources.ActivityError, Resources.BadOperation, Resources.ActivityError, 0, MessageType.Error, MessageButtons.Ok);
                    General.SafeLaunchEvent(ShowNotifMsgEngine, this, msg);
                });

                Wait(500);
                Top.Mixer.Unlink(_BadOperationTone);
                SipAgent.DestroyWavPlayer(_BadOperationTone);
                return false;
            }

            return true;
        }

        private bool AllowRd(bool allowPttHmi = false)
		{
			if (!Top.Hw.InstructorJack && !Top.Hw.AlumnJack)
			{
				int _BadOperationTone = SipAgent.CreateWavPlayer("Resources/Tones/Falsa_Maniobra.wav", true);
				Top.Mixer.Link(_BadOperationTone, MixerDev.SpkRd, MixerDir.Send, Mixer.RD_PRIORITY, FuentesGlp.RxRadio);
                Top.Mixer.SetVolumeTones(CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER);//#5829

                Top.PublisherThread.Enqueue(EventTopicNames.ShowNotifMsgEngine, delegate()
				{
					NotifMsg msg = new NotifMsg(Resources.NoJacksError, Resources.BadOperation, Resources.NoJacksError, 0, MessageType.Error, MessageButtons.Ok);
					General.SafeLaunchEvent(ShowNotifMsgEngine, this, msg);
				});

				Wait(500);
				Top.Mixer.Unlink(_BadOperationTone);
				SipAgent.DestroyWavPlayer(_BadOperationTone);

				return false;
			}

            // Se permite un Ptt SW desde HMI, simult�neo con un ptt HW desde cascos en el mismo puesto
            // para deshacer un posible bloqueo por fallo en el HW
            if ((Top.Rd.PttSource != PttSource.NoPtt) && (allowPttHmi == false))
			{
				int _BadOperationTone = SipAgent.CreateWavPlayer("Resources/Tones/Falsa_Maniobra.wav", true);
				Top.Mixer.Link(_BadOperationTone, MixerDev.SpkRd, MixerDir.Send, Mixer.RD_PRIORITY, FuentesGlp.RxRadio);
                Top.Mixer.SetVolumeTones(CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER);//#5829

                Top.PublisherThread.Enqueue(EventTopicNames.ShowNotifMsgEngine, delegate()
				{
					NotifMsg msg = new NotifMsg(Resources.PttPulsedError, Resources.BadOperation, Resources.PttPulsedError, 0, MessageType.Error, MessageButtons.Ok);
					General.SafeLaunchEvent(ShowNotifMsgEngine, this, msg);
				});

				Wait(500);
				Top.Mixer.Unlink(_BadOperationTone);
				SipAgent.DestroyWavPlayer(_BadOperationTone);

				return false;
			}

			return true;
		}

		private bool AllowRd(int id)
		{
			if (!Top.Hw.InstructorJack && !Top.Hw.AlumnJack)
			{
				int _BadOperationTone = SipAgent.CreateWavPlayer("Resources/Tones/Falsa_Maniobra.wav", true);
				Top.Mixer.Link(_BadOperationTone, MixerDev.SpkRd, MixerDir.Send, Mixer.RD_PRIORITY, FuentesGlp.RxRadio);
                Top.Mixer.SetVolumeTones(CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER);//#5829

                Top.PublisherThread.Enqueue(EventTopicNames.ShowNotifMsgEngine, delegate()
				{
					NotifMsg msg = new NotifMsg(Resources.NoJacksError, Resources.BadOperation, Resources.NoJacksError, 0, MessageType.Error, MessageButtons.Ok);
					General.SafeLaunchEvent(ShowNotifMsgEngine, this, msg);
				});

				Wait(500);
				Top.Mixer.Unlink(_BadOperationTone);
				SipAgent.DestroyWavPlayer(_BadOperationTone);

				return false;
			}

			if (Top.Rd.PttSource != PttSource.NoPtt)
			{
				int _BadOperationTone = SipAgent.CreateWavPlayer("Resources/Tones/Falsa_Maniobra.wav", true);
				Top.Mixer.Link(_BadOperationTone, MixerDev.SpkRd, MixerDir.Send, Mixer.RD_PRIORITY, FuentesGlp.RxRadio);
                Top.Mixer.SetVolumeTones(CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER);//#5829

                Top.PublisherThread.Enqueue(EventTopicNames.ShowNotifMsgEngine, delegate()
				{
					NotifMsg msg = new NotifMsg(Resources.PttPulsedError, Resources.BadOperation, Resources.PttPulsedError, 0, MessageType.Error, MessageButtons.Ok);
					General.SafeLaunchEvent(ShowNotifMsgEngine, this, msg);
				});

				Wait(500);
				Top.Mixer.Unlink(_BadOperationTone);
				SipAgent.DestroyWavPlayer(_BadOperationTone);

				return false;
			}

			if (Top.Rd.GetRtxGroupPosition(id) > 0)
			{
				int _BadOperationTone = SipAgent.CreateWavPlayer("Resources/Tones/Falsa_Maniobra.wav", true);
				Top.Mixer.Link(_BadOperationTone, MixerDev.SpkRd, MixerDir.Send, Mixer.RD_PRIORITY, FuentesGlp.RxRadio);
                Top.Mixer.SetVolumeTones(CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER);//#5829

                Top.PublisherThread.Enqueue(EventTopicNames.ShowNotifMsgEngine, delegate()
				{
					NotifMsg msg = new NotifMsg(Resources.PttPulsedError, Resources.BadOperation, Resources.FrecuencyRtxError, 0, MessageType.Error, MessageButtons.Ok);
					General.SafeLaunchEvent(ShowNotifMsgEngine, this, msg);
				});

				Wait(500);
				Top.Mixer.Unlink(_BadOperationTone);
				SipAgent.DestroyWavPlayer(_BadOperationTone);

				return false;
			}

            return true;
		}

		private bool AllowRtx(Dictionary<int, RtxState> rtxGroup)
		{
            if (Top.Rd.HowManySquelchsInRtxGroup(rtxGroup) > 1 && !Top.Cfg.PermisoRTXSQ())
			{
				Top.PublisherThread.Enqueue(EventTopicNames.ShowNotifMsgEngine, delegate()
				{
					NotifMsg msg = new NotifMsg(Resources.PttPulsedError, Resources.BadOperation, Resources.RtxGroupSquelchError, 0, MessageType.Error, MessageButtons.Ok);
					General.SafeLaunchEvent(ShowNotifMsgEngine, this, msg);
				});

				return false;
			}

			return true;
		}

        /// <summary>
        /// Valida el numero a marcar, y saca una ventana emergente de error si el argumento message es true
        /// </summary>
        /// <param name="number"></param>
        /// <param name="prefix"></param>
        /// <param name="dst"></param>
        /// <param name="lit"></param>
        /// <param name="message"></param>
        /// <returns></returns>
		private bool TryParseNumber(string number, out uint prefix, out string dst, ref string lit, bool message = true)
		{
			prefix = uint.MaxValue;
			dst = null;

			if (number.Length > 2)
			{
				if (uint.TryParse(number.Substring(0, 2), out prefix))
				{
                    dst = number.Substring(2, number.Length - 2);
                    // Utilizamos el literal que llega antes que un numero
                    if (String.IsNullOrEmpty(lit))
                        lit = dst;

                    // LALM 210630
                    // Otra alternativa al Errores #4862
                    // Si se quisiera que las llamadas con prefijo "02" se se�alicen en las teclas de aceso directo 
                    // con prefijo punto a punto de telefonia IP "01" habria que introducir este path
                    // if (prefix == Cd40Cfg.IP_DST) prefix = Cd40Cfg.PP_DST;

                    switch (prefix)
                    {
                        //LALM 210707
                        // Errores #4862
                        // impedimos que las llamadas con prefijo 02 progresen.
                        case Cd40Cfg.IP_DST:
                            return false;
                        case Cd40Cfg.INT_DST:
                        case Cd40Cfg.PP_DST:
                        case Cd40Cfg.UNKNOWN_DST:
                            return true;
                        default:
							if (Top.Cfg.ExistNet(prefix, dst))
							{
								string[] user = Top.Cfg.GetUserFromAddress(prefix, dst);
								if (user != null)
								{
									prefix = Cd40Cfg.INT_DST;
									dst = user[1];
                                    lit = user[0];
								}

								return true;
							}
							break;
					}
				}
			}
            if (message)
            {
                // 20201120 lalm cambio el mensaje si es la red ATS (AGVN)
                if (prefix != Cd40Cfg.ATS_DST)
                {
                    Top.PublisherThread.Enqueue(EventTopicNames.ShowNotifMsgEngine, delegate ()
                    {
                        NotifMsg msg = new NotifMsg(Resources.BadNumberError, Resources.BadOperation, Resources.BadNumberError, 0, MessageType.Error, MessageButtons.Ok);
                        General.SafeLaunchEvent(ShowNotifMsgEngine, this, msg);
                    });
                }
                else
                {
                    ///20201120 LALM Errores #4534 sustituir el mensaje de error �N�mero Err�neo� por �N�mero No Accesible�
                    Top.PublisherThread.Enqueue(EventTopicNames.ShowNotifMsgEngine, delegate ()
                    {
                        NotifMsg msg = new NotifMsg(Resources.BadNumberATS, Resources.BadOperation, Resources.BadNumberATS, 0, MessageType.Error, MessageButtons.Ok);
                        General.SafeLaunchEvent(ShowNotifMsgEngine, this, msg);
                    });
                }
            }
			return false;
		}



        /**
               * AGL 17072012. Trata de Rellenar la tabla de Dependencias desde un fichero local.
               * */
        [EventPublication(EventTopicNames.NumberBookChangedEngine, PublicationScope.Global)]
        public event EventHandler<RangeMsg<Area>> NumberBookChangedEngine;

        [Serializable]
        class BookAddress 
        {
            private Dictionary<string, Area> _book;
            public BookAddress()

            {
                _book = new Dictionary<string, Area>();
            }

            public Dictionary<string, Area> Book
            {
                get { return _book; }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void SetDependences_from_xls()
        {
            string _UltimoPlanATS = "_PlanNumeracionATS.bin";



            BookAddress book = new BookAddress();
            try
            {
                if (LoadDependencesFromRemote(book.Book) == true)
                {
                    var file = File.Create(_UltimoPlanATS);             

                    BinaryFormatter bf = new BinaryFormatter();

                    try
                    {
                        bf.Serialize(file, book);
                    }
                    catch (Exception x)
                    {
                        _Logger.Error("Failed to serialize. Reason: {0}", x.Message);
                        throw;
                    }
                    finally
                    {
                        file.Close();
                    }


                }
                else
                {
                    var file = File.OpenRead(_UltimoPlanATS);
                    BinaryFormatter bf = new BinaryFormatter();
                    book = (BookAddress)bf.Deserialize(file);
                }
                General.SafeLaunchEvent(NumberBookChangedEngine, this, new RangeMsg<Area>(0, book.Book.Values.Count, book.Book.Values));
            }
            catch (Exception x)

            {
                _Logger.Error("ERROR. Al cargar el Fichero de Dependencia en Local. {0}", x.Message);

            }
        }
         
        /// <summary>
        /// 
        /// </summary>
        /// <param name="numberBook"></param>
        /// <returns></returns>
        private bool LoadDependencesFromRemote(Dictionary<string, Area> numberBook)
        {
            bool retorno = true;


            string ExcelPath = Settings.Default.Path_Dependencias;//"c:\\PlanNumeracionEUROCONTROL.xls";
            string ExcelPage = "[Plan Numeracion$]";
            string connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + ExcelPath + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=1\";";
            OleDbConnection _connection = new OleDbConnection(connectionString);
            OleDbDataAdapter _adapter = new OleDbDataAdapter();
            OleDbCommand _cmd = new OleDbCommand();
            OleDbDataAdapter oleAdapter = new OleDbDataAdapter();
            DataTable dt = new DataTable("RES");

            Area area;
            Fir fir;
            Depencence dependence;

            try

            {
                _connection.Open();
                _cmd.CommandText = "SELECT * FROM " + ExcelPage + ";";
                _cmd.Connection = _connection;

                oleAdapter.SelectCommand = _cmd;
                oleAdapter.FillSchema(dt, SchemaType.Source);
                oleAdapter.Fill(dt);



                foreach (DataRow fila in dt.Rows)

                {
                    string areaName = fila["Nombre_Pa�s"].ToString();
                    string firName = fila["FIR"].ToString();
                    string dependenceName = fila["Nombre_dependencia"].ToString();

                    if (!numberBook.TryGetValue(areaName, out area))

                    {
                        area = new Area(areaName);
                        numberBook[areaName] = area;
                    }
                    if (!area.TryGetValue(firName, out fir))
                    {
                        fir = new Fir(firName);
                        area[firName] = fir;
                    }
                    if (!fir.TryGetValue(dependenceName, out dependence))
                    {
                        dependence = new Depencence(dependenceName);
                        fir[dependenceName] = dependence;
                    }

                    UserNumber user = new UserNumber(fila["Nombre_usuario"].ToString(),
                                                     fila["N�mero_AGVN"].ToString(),
                                                     fila["N�mero_Externo"].ToString(),
                                                     fila["Tipo_usuario"].ToString(),
                                                     fila["Funci�n_usuario"].ToString());
                    dependence[user.Name + "." + user.Role] = user;
                }


            }
            catch (Exception x)
            {
                _Logger.Error("ERROR. Al cargar el Fichero de Dependencia en Remoto: {0}", x.Message);
                retorno = false;

            }
            finally
            {
                if (_connection.State != System.Data.ConnectionState.Closed)
                {
                    _connection.Close();
                }
            }

            return retorno;
        }

        //#5829 para unificar los tone de se�alizacion
        public void SetTonesLevel(int level)
        {
            //TODO
        }

        /**
         * Fin de Modificacion */
		#endregion
	}
}
