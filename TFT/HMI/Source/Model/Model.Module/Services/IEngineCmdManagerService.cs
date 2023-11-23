using System;
using System.Collections.Generic;
using System.Text;
using HMI.Model.Module.BusinessEntities;

namespace HMI.Model.Module.Services
{
	public interface IEngineCmdManagerService
	{
		string Name { get; }

		void GetEngineInfo();
        void BriefingFunction();
		void SetSplitMode(SplitMode mode);
		void SetBuzzerState(bool enabled);
		void SetBuzzerLevel(int level);
		void SetRdHeadPhonesLevel(int level);
        void SetRdHfSpeakerLevel(int level);
        void SetRdSpeakerLevel(int level);
        void SetRdPtt(bool on);
		void SetRdPage(int oldPage, int newPage, int numPosByPage);
		void SetRdRx(int id, bool on, bool forced = false);
		void SetRdTx(int id, bool on);
		void SetNewFrecuency(int id, string fr);//LALM 221102 cambiofrecuencia
        void ForceTxOff(int id);
		void ConfirmRdTx(int id);
		void SetRdAudio(int id, RdRxAudioVia audio, bool forced = false);
        void NextRdAudio(int id);
        /** */
        void SetRtxGroup(int rtxGroup, Dictionary<int, RtxState> newRtxGroup, bool force=false);
		void SetLc(int id, bool on);
		void BeginTlfCall(int id, bool prio);
        void BeginTlfCall(string number, bool prio, string literal);
        void BeginTlfCall(string number, bool prio, int id, string literal);
		void RetryTlfCall(int id);
		void AnswerTlfCall(int id);
		void EndTlfCall(int id);
		void EndTlfCall(int id, TlfState st);
		void EndTlfConfCall(int id);
		void RecognizeTlfState(int id);
		void EndTlfConf();
        void EndTlfAll();
		void SetTlfHeadPhonesLevel(int level);
        void SetTlfSpeakerLevel(int level);
        void SetLcSpeakerLevel(int level);
		void ListenTo(int id);
		void ListenTo(string number);
		void CancelListen();
		void RecognizeListenState();
		void SetHold(int id, bool on, bool porPttLc = false);
		void TransferTo(int id, bool direct);
		void TransferTo(string number);
		void CancelTransfer();
		void RecognizeTransferState();
		void SetHangToneOff();
		void SendDigit(char ch);
		void SetRemoteListen(bool allow, int id);
		void Cancel();
		void Wait(int ms);
		void SendTrapScreenSaver(bool status);
		void ResetRdPosition(int id);
        bool SetAudioViaTlf(bool speaker);
        void ModoSoloAltavoces();
        void SetDoubleRadioSpeaker();
        void PreparePickUp(int id);
        void PreparePickUp(string number);
        void PickUp(int id);
        void CancelPickUp();
        void MakeConference(bool viable);
        void SetHold(bool on);
        bool HayConferencia();
        void FunctionReplay(FunctionReplay function, ViaReplay via, string fileName, long fileLength);
        void SelCalPrepare(bool prepareOnOff, string code);
        void PrepareForward(int id);
        void PrepareForward(string number);
        void CancelForward();

        void SendCmdHistoricalEvent(string user, string frec);
        void SetManagingSite(bool managing);
        string ChangingPositionSite(int id);
        void ChangeSite(int i, string alias);

        /** 20180716. Para activar tonos de Falsa Maniobra Radio */
        void GenerateRadioBadOperationTone(int durationInMsec);
		//LALM 210224 Errores #4756 visualizacion de mensaje de error por frecuencia prioritaria
		void SetErrorFP();
		void ResetErrorFP();
		//LALM 2102024
		void SetCambioRadio(bool up);
		//LALM 211201
		void Descuelga();
		void SetTonesLevel(int level);
		List <string> RefrescaListaParticipantesConf(string sala);//lalm 230510
		List <string> GetListaParticipantesEstado(string sala);//lalm 230510
		string GetSala(int poshmi);//lalm 230517
		void ShowAdButtons();
        bool HayConferenciaPreprogramada();

		void SetToneporllamadaEngine(string llamada, string tono, string tonoprio);

    }
}
