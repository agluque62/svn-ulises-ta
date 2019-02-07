//----------------------------------------------------------------------------------------
// patterns & practices - Smart Client Software Factory - Guidance Package
//
// This file was generated by the "Add Business Module" recipe.
//
// For more information see: 
// ms-help://MS.VSCC.v80/MS.VSIPCC.v80/ms.practices.scsf.2007may/SCSF/html/02-08-060-Add_Business_Module_Next_Steps.htm
//
// Latest version of this Guidance Package: http://go.microsoft.com/fwlink/?LinkId=62182
//----------------------------------------------------------------------------------------

namespace HMI.Model.Module.Constants
{
	/// <summary>
	/// Constants for event topic names.
	/// </summary>
	public class EventTopicNames : HMI.Infrastructure.Interface.Constants.EventTopicNames
	{
		#region State Changes Events

		public const string TftEnabledChanged = "TftEnabledChanged";
		public const string EngineStateChanged = "EngineStateChanged";
		public const string ScreenSaverChanged = "ScreenSaverChanged";
		public const string TitleIdChanged = "TitleIdChanged";
		public const string JacksChanged = "JacksChanged";
		public const string SplitModeChanged = "SplitModeChanged";
		public const string BrightnessLevelChanged = "BrightnessLevelChanged";
		public const string BuzzerLevelChanged = "BuzzerLevelChanged";
		public const string BuzzerStateChanged = "BuzzerStateChanged";
		public const string ActiveScvChanged = "ActiveScvChanged";
        public const string ProxyPresent = "ProxyPresent";
        public const string RdHfSpeakerLevelChanged = "RdHfSpeakerLevelChanged";
        public const string RdSpeakerLevelChanged = "RdSpeakerLevelChanged";
        public const string RdHeadPhonesLevelChanged = "RdHeadPhonesLevelChanged";
		public const string PttOnChanged = "PttOnChanged";
		public const string RtxChanged = "RtxChanged";
		public const string RadioChanged = "RadioChanged";
		public const string RdPageChanged = "RdPageChanged";
		public const string LcChanged = "LcChanged";
		public const string TlfChanged = "TlfChanged";
		public const string LcSpeakerLevelChanged = "LcSpeakerLevelChanged";
		public const string TlfHeadPhonesLevelChanged = "TlfHeadPhonesLevelChanged";
        public const string TlfSpeakerLevelChanged = "TlfSpeakerLevelChanged";
        public const string TlfPriorityChanged = "TlfPriorityChanged";
		public const string TlfIntrudedByChanged = "TlfIntrudedByChanged";
		public const string TlfInterruptedByChanged = "TlfInterruptedByChanged";
		public const string TlfIntrudeToChanged = "TlfIntrudeToChanged";
		public const string TlfListenChanged = "TlfListenChanged";
		public const string TlfListenByChanged = "TlfListenByChanged";
		public const string TlfTransferChanged = "TlfTransferChanged";
		public const string TlfHangToneChanged = "TlfHangToneChanged";
		public const string TlfUnhangChanged = "TlfUnhangChanged";
		public const string TlfConfListChanged = "TlfConfListChanged";
		public const string AgendaChanged = "AgendaChanged";
		public const string NumberBookChanged = "NumberBookChanged";
		public const string PermissionsChanged = "PermissionsChanged";
        public const string BriefingChanged = "BriefingChanged";
        public const string SpeakerChanged = "SpeakerChanged";
        public const string PlayingChanged = "PlayingChanged";
        public const string HistoricalOfLocalCalls = "HistoricalOfLocalCalls";
        public const string SelCalResponse = "SelCalResponse";
        public const string HfGlobalStatus = "HfGlobalStatus";
        public const string SiteManagerChanged = "SiteManagerChanged";
        public const string SiteChanged = "SiteChanged";
        public const string ChangeTlfSpeaker = "ChangeTlfSpeaker";
        /** 20190205 */
        public const string TxInProgressError = "RtxInProgressError";
        #endregion

		#region Presentation Events

		public const string SplitShowModeSelectionUI = "SplitShowModeSelectionUI";
		public const string ShowInfoUI = "ShowInfoUI";
		public const string SwitchTlfViewUI = "SwitchTlfViewUI";
		public const string LoadTlfDaPageUI = "LoadTlfDaPageUI";
		public const string ShowNotifMsgUI = "ShowNotifMsgUI";
		public const string HideNotifMsgUI = "HideNotifMsgUI";
        public const string BriefingFunctionUI = "BriefingFunctionUI";
        public const string BriefingSessionUI = "BriefingSessionUI";
        public const string ReplayUI = "ReplayUI";
        public const string DeleteSessionGlp = "DeleteSessionGlp";

		#endregion

		#region Engine Events

		public const string ConnectionStateEngine = "ConnectionStateEngine";
		public const string IsolatedStateEngine = "IsolatedStateEngine";
		public const string ActiveScvEngine = "ActiveScvEngine";
		public const string PositionIdEngine = "PositionIdEngine";
		public const string ResetEngine = "ResetEngine";
		public const string SplitModeEngine = "SplitModeEngine";
        public const string ProxyStateChangedEngine = "ProxyStateChangedEngine";
        public const string JacksStateEngine = "JacksStateEngine";
        public const string SpeakerStateEngine = "SpeakerStateEngine";
        public const string SpeakerExtStateEngine = "SpeakerExtStateEngine";
        public const string BuzzerStateEngine = "BuzzerStateEngine";
		public const string BuzzerLevelEngine = "BuzzerLevelEngine";
		public const string TlfInfoEngine = "TlfInfoEngine";
		public const string TlfPositionsEngine = "TlfPositionsEngine";
		public const string TlfPosStateEngine = "TlfPosStateEngine";
		public const string TlfHeadPhonesLevelEngine = "TlfHeadPhonesLevelEngine";
        public const string TlfSpeakerLevelEngine = "TlfSpeakerLevelEngine";
        public const string RdInfoEngine = "RdInfoEngine";
		public const string RdPositionsEngine = "RdPositionsEngine";
		public const string RdPageEngine = "RdPageEngine";
		public const string RdPttEngine = "RdPttEngine";
		public const string RdPosPttStateEngine = "RdPosPttStateEngine";
		public const string RdPosSquelchStateEngine = "RdPosSquelchStateEngine";
		public const string RdPosAsignStateEngine = "RdPosAsignStateEngine";
		public const string RdPosStateEngine = "RdPosStateEngine";
		public const string RdRtxModificationEndEngine = "RdRtxModificationEndEngine";
		public const string RdRtxGroupsEngine = "RdRtxGroupsEngine";
		public const string RdSpeakerLevelEngine = "RdSpeakerLevelEngine";
        public const string RdHeadPhonesLevelEngine = "RdHeadPhonesLevelEngine";
        public const string RdHFLevelEngine = "RdHFLevelEngine";
        public const string RdFrAsignedToOtherEngine = "RdFrAsignedToOtherEngine";
		public const string LcInfoEngine = "LcInfoEngine";
		public const string LcPositionsEngine = "LcPositionsEngine";
		public const string LcPosStateEngine = "LcPosStateEngine";
		public const string LcSpeakerLevelEngine = "LcSpeakerLevelEngine";
		public const string PriorityStateEngine = "PriorityStateEngine";
		public const string IntrudedByEngine = "IntrudedByEngine";
		public const string InterruptedByEngine = "InterruptedByEngine";
		public const string IntrudeToEngine = "IntrudeToEngine";
		public const string ListenStateEngine = "ListenStateEngine";
		public const string RemoteListenStateEngine = "RemoteListenStateEngine";
		public const string TransferStateEngine = "TransferStateEngine";
		public const string HangToneStateEngine = "HangToneStateEngine";
		public const string TlfIaPosStateEngine = "TlfIaPosStateEngine";
		public const string ConfListEngine = "ConfListEngine";
		public const string ShowNotifMsgEngine = "ShowNotifMsgEngine";
		public const string HideNotifMsgEngine = "HideNotifMsgEngine";
		public const string PermissionsEngine = "PermissionsEngine";
        public const string BriefingStateEngine = "BriefingStateEngine";
        public const string PlayingStateEngine = "PlayingStateEngine";
        public const string RdHfFrAssignedEngine = "RdHfFrAssignedEngine";
        public const string SelCalResponseEngine = "SelCalResponseEngine";
        public const string HfGlobalStatusEngine = "HfGlobalStatusEngine";
        public const string SiteManagerEngine = "SiteManagerEngine";
        public const string SiteChangedResultEngine = "SiteChangedResultEngine";

		public const string AgendaChangedEngine = "AgendaChangedEngine";
		public const string NumberBookChangedEngine = "NumberBookChangedEngine";
        public const string ShowInfoEngine = "ShowInfoEngine";

		public const string HoldTlfCallEngine = "HoldTlfCallEngine";
		public const string RemoveRtxGroup = "RemoveRtxGroup";
		public const string PTTMaxTime = "PTTMaxTime";
		public const string CompletedIntrusionStateEngine = "CompletedIntrusionStateEngine";
		public const string BeginingIntrudeToStateEngine = "BeginingIntrudeToStateEngine";
		public const string IntrudeToStateEngine = "IntrudeToStateEngine";

		public const string SetSnmpInt = "SetSnmpInt";
		public const string SetSnmpString = "SetSnmpString";

        public const string HistoricalOfLocalCallsEngine = "HistoricalOfLocalCallsEngine";
        public const string DoubleRadioSpeaker = "DoubleRadioSpeaker";

        #endregion
	}
}
