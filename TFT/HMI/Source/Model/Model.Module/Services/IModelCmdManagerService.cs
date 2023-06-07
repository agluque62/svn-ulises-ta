using System;
using System.Collections.Generic;
using System.Text;
using HMI.Model.Module.BusinessEntities;
using HMI.Model.Module.Messages;

namespace HMI.Model.Module.Services
{
	public interface IModelCmdManagerService
	{
		void DisableTft();
		void MessageResponse(NotifMsg msg, NotifMsgResponse response);
		void ShowSplitModeSelection();
		void SetSplitMode(SplitMode mode);
		void ShowInfo();
		void SetBrightnessLevel(int level);
		void SetBuzzerState(bool enabled);
		void SetBuzzerLevel(int level);
		void RdSetSpeakerLevel(int level);
		void RdSetHeadPhonesLevel(int level);
        void RdSetHfSpeakerLevel(int level);
		void SetTonesLevel(int level);
        void RdRtxClick(int numPositionsByPage);
		void RdSetPtt(bool on);
		void RdLoadNextPage(int oldPage, int numPosByPage);
		void RdLoadPrevPage(int oldPage, int numPosByPage);
		void RdSwitchRtxState(int id);
        void RdSwitchTxState(int id);
        void RdForceRxState(int id);
        void RdForceTxOff(int id);
        void RdConfirmTxState(int id);
        void RdConfirmRxAudio(int id, RdRxAudioVia via);
        void RdSwitchRxState(int id, bool longClick);
		void LcSet(int id, bool on);
		void LcSetSpeakerLevel(int level);
		void TlfSetHeadPhonesLevel(int level);
        void TlfSetSpeakerLevel(int level);
        void TlfLoadDaPage(int page);
		void TlfClick(int id);
        void TlfClick(string number, string literal=null);
		void PriorityClick();
		void ListenClick();
		void HoldClick();
		void TransferClick();
		void SwitchTlfView(string view);
		void SwitchRadView(string pantalla,int id, string fr);
		bool CancelTlfClick(bool test=false);//#2816 LALM 220615
		void SpeakerTlfClick();
        void NewDigit(int id, char key);
        void BriefingFunction();
        void ConferenceClick();
        void ReplayClick();
        void FunctionReplay(FunctionReplay function, ViaReplay via, string fileName, long fileLength);
        void RdPrepareSelCal(bool prepareOn, string code);
        void SendCmdHistoricalEvent(string user, string frec);
        void RdSiteManagerClick();
        void ChangeSite(int id);
        void PickUpClick();
        void ForwardClick();
		void SetErrorFP();
		void ResetErrorFP();
		//LALM 210324
		void SetCambioRadio(bool up);
		void PlayRadioClick();
		void StopAudioReproduccion();
		void SetNewFrecuency(int id, string fr);//LALM 221102 cambiofrecuencia
		List<string> RefrescaListaParticipantesConf(string sala);//lalm 230510 model 
		List<string> GetListaParticipantesEstado(string sala);//lalm 230510 model 
		string GetSala(int poshmi);//lalm 230517
		void DisableFunctionsPagConferencia(bool disable=true);
		void ShowAdButtons(int page);
	}
}
