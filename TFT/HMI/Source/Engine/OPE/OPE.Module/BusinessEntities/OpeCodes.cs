using System;
using System.Collections.Generic;
using System.Text;

namespace HMI.OPE.Module.BusinessEntities
{
	enum OpeEventType : byte
	{
		Ctl = 0x1,
		Tlf = 0x2,
		Radio = 0x3,
		Lc = 0x4,
		Cfg = 0x5,
		Int = 0x6,
	}

	enum IntCmdType : byte
	{
		Wait = 0x1
	}

	enum CtlEventType : byte
	{
		SysInfo = 0x1,
		SplitMode = 0xA0,
		OpeVersion = 0xA3,
		IolVersion = 0xA4,
		M4lVersion = 0xA5,
		RepeatedUser = 0xAA,
		OpeSwitchs = 0xAD,
		Error = 0xAF,
		Log
	}

	enum TlfEventType : byte
	{
		TlfPosState = 0x80,
		BuzzerState = 0x81,
		TlfAiState = 0x82,
		HangUpDownState = 0x84,
		HeadphonesLevel = 0x86,
		Permissions = 0x89,
		TransferState = 0x8A,
		ConfState = 0x8C,
		BuzzerLevel = 0x8D,
		ListenOn = 0x90,
		RejectListen = 0x91,
		ListenOff = 0x92,
		ListenReady = 0x93,
		Priority = 0x95,
		RecallState = 0x96,
		NoticeAlertOn = 0x97,
		RejectIntrussion = 0x99,
		IntrussionOff = 0x9A,
		IntrussionOn = 0x9B,
		AlertOn = 0x9C,
		AlertOff = 0x9D,
		RejectAlert = 0x9E,
		NoticeAlertOff = 0x9F,
		CoorState = 0xA0,
		IntrudedBy = 0xA1,
		AlreadyInConf = 0xA2,
		ConfList = 0xA3,
		HangTone = 0xA4,
		InterruptedBy = 0xA5,
		IntrudeTo = 0xA7,
		TransferDirectState = 0xB0,
		IdleState = 0xB5,
		RemoteListen = 0xB6
	}

	enum TlfCmdType : byte
	{
		DaShortClick = 0x10,
		DaLongClick = 0x11,
		UnhangClick = 0x12,
		IaShortClick = 0x15,
		IaLongClick = 0x16,
		Digit = 0x19,
		Hold = 0x20,
		AskHeadPhonesLevel = 0x23,
		SetHeadPhonesLevel = 0x24,
		BuzzerShortClick = 0x28,
		BuzzerLongClick = 0x29,
		TransferClick = 0x2A,
		TransferTo = 0x2B,
		AskBuzzerLevel = 0x32,
		SetBuzzerLevel = 0x33,
		SetListenOn = 0x34,
		SetListenOff = 0x35,
		SetIntrusionOff = 0x37,
		CancelClick = 0x38,
		SetPriorityOn = 0x3A,
		AskPermissions = 0x43,
		SetPriorityOff = 0x45,
		SetHangToneOff = 0x46,
		DirectTransferClick = 0x50,
		RemoteListenAnswer = 0x53,
		NumberClick = 0x60
	}

	enum RdEventType : byte
	{
		RdPosTxRx = 0x80,
		RdPageTx = 0x81,
		RdPageRx = 0x82,
		SpeakerLevel = 0x83,
		HeadphonesLevel = 0x84,
		RdFrAsignedToOther = 0x85,
		RdSiteChanged = 0x86,
		RtxGroupInfoOld = 0x87,
		RtxGroupInfoAbs = 0x88,
		RtxEnd = 0x89,
		RdPageRtxGroup = 0x8A,
		CoorState = 0x8B,
		RdPageAsign = 0x8C,
		InCaState = 0x8D,
		ActiveRdPage = 0x8E,
		VisibleRdPage = 0x8F,
		TxChannels = 0x90
	}

	enum RdCmdType : byte
	{
		RxClick = 0x10,
		TxClick = 0x11,
		RxLongClick = 0x12,
		PttOn = 0x14,
		PttOff = 0x15,
		SetSpeakerLevel = 0x17,
		SetHeadPhonesLevel = 0x18,
		SetRtxGroupBegin = 0x1B,
		AddToRtxGroup = 0x1C,
		DeleteFromRtxGroup = 0x1D,
		SetRtxGroupEnd = 0x1E,
		AskSpeakerLevel = 0x1F,
		AskHeadphonesLevel = 0x20,
		SetActivePage = 0x25,
		AskShowPage = 0x27,
        ConfirmTx = 0x28
	}

	enum LcEventType : byte
	{
		LcPosState = 0x83,
		SpeakerLevel = 0x85
	}

	enum LcCmdType : byte
	{
		MouseDown = 0x17,
		MouseUp = 0x18,
		AskSpeakerLevel = 0x21,
		SetSpeakerLevel = 0x22
	}

	enum CfgEventType : byte
	{
		UserStates = 0xC0,
		Sites = 0xC1,
		Ini = 0xC2,
		RdPos = 0xC3,
		LcPos = 0xC4,
		TlfPos = 0xC5,
		RdMask = 0xC6,
		TlfMask = 0xC7,
		CfgLoadTime = 0xC8,
		RdAll = 0xC9,
		LcAll = 0xCA,
		TlfAll = 0xCB,
		Sect = 0xCC,
		ActiveScv = 0xCD
	}

	enum RdTxType : byte
	{
		Unavailable,
		PttOff,
		Ptt,
		Blocked,
		ExternPtt,
		UnallowedPtt,
		ErrorPtt,
		Rtx,
		ExternRtx,
		UnavailableNtz,
		UnavailableNtzExt
	}

	enum RdRxType : byte
	{
		Unavailable,
		SquelchOff,
		SquelchOn,
		SquelchMod
	}

	enum RdAsignType : byte
	{
		Idle = 0xE1,
		Rx = 0xE2,
		Tx = 0xE3
	}

	enum RdRxAudioType : byte
	{
		Unknown,
		HeadPhones = 0xA1,
		Speaker = 0xA2
	}

	enum LcRxType : byte
	{
		Idle,
		Rx,
		Mem,
		Unavailable
	}

	enum LcTxType : byte
	{
		Idle,
		Tx,
		Busy,
		Unavailable,
		Congestion = 7
	}

	enum BuzzerStateType : byte
	{
		Off,
		On,
		UnavailableTemp,
		Unavailable
	}

	enum TlfStType : byte
	{
		Idle,
		In,
		Out,
		Set,
		Busy,
		Parked,
		Blocked,
		Pending,
		RemoteParked,
		Mem,
		Conf = 0x0F,
		Hold = 0x14,
		Prio = 0x16,
		ParkedRemoteParked = 0x30,
		ConfHold = 0x33,
		Unavailable = 0x34,
		PaPBusy = 0x35,
		Congestion = 0x40,
		RemoteIn = 0x41,
		RemoteMem = 0x42
	}

	enum PriorityStType : byte
	{
		Idle,
		On = 0xFF,
		Error = 0x5
	}

	enum TransferStType : byte
	{
		Ready,
		Error,
		Idle
	}

	enum TransferDirectStType : byte
	{
		Idle,
		Ready,
		Accepted,
		Error
	}

	enum TlfPosType : byte
	{
		Ad = 1,
		Ai = 3
	}

	enum OpeErrorType : byte
	{
		ERROR_OPE,			/*0*/
		EMPL_UNICO,
		CANAL_EN_GRTX,
		PTT_PULSADO,
		NO_JACKS_COOR,
		NO_JACKS,			/*5*/
		MAX_FRQ_GRTX,
		RTX_NO_ACEPTADA,
		UNA_FRQ_GRTX,
		SERV_NO_CAPT,
		FAC_IMPOSIBLE,		/*10*/
		CODIGO_ERRONEO,
		CODIGO_NO_DEPEN,
		ENLACE_CONF,
		TF_FUERA_SECTOR,
		TF_NO_DISPONIBLE,	/*15*/
		OPE_AISLADA,
		PTT_ERROR,
		TIME_OUT_PTT,
		SUB_TF_USADO,
		SQUELCH_EN_MAS_FRQ,	/*20*/
		MAX_PARTI_CONF,
		LLAMADA_EN_CURSO
	}
}
