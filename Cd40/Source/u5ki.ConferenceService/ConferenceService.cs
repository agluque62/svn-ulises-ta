using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;

using U5ki.PresenceService.Properties;
using U5ki.Infrastructure;
using WebSocket4Net;
using Utilities;
using System.IO;
using ProtoBuf;
using U5ki.PresenceService.Interfaces;
using U5ki.PresenceService;
using System.Threading;
using static U5ki.Infrastructure.Code.Globals.Test;
using System.Security.Cryptography;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace u5ki.ConferenceService
{
    public class ConferenceService : BaseCode, IService
    {
        private class bkkNotifyStatusParamInfo
        {
            public string user;                     // user extension
            public string status;                      // status code
            public string other_number;             // the extension of the other user in the call
            public string room_id;                  // room ID of the call, required by some request methods below as rid
            public string talker_id;                // talker ID of the user, required by some request methods below as tid
            public string user_display_name;        // the display name of the user
            public string other_user_display_name;  // the display name of the other user in the call
            public string time;                     // the time stamp of the call
            public string logid;                    // the ID number, which is the same as the number in the request method
            public string rescode;                  // response code, used only in the disconnected call response with status -1
            public string disconnected_by;          // 1 or 0, used only in the disconnected call response with status -1. “1” means the session is disconnected by the UA and “0” means the session is disconnected by PBX.
            public string q850code;                 // q850 code, used only in the disconnected call response with status -1
        };

        //Status codes:
        private enum NotifyStatusCode
        {
            CALLING = 0,
            INCOMING = 1,
            CALL_SUCCESS = 2,
            CALL_SUCCESS_BIS = 205,
            ENDTALKING = 12,
            ANSWER_SUCCESS = 14,
            PARK_CANCEL = 21,
            PARK_START = 30,
            STARTRINGING = 65,
            HOLD = 35,
            UNHOLD = 36,
            DISCONNECT = -1
        };

        private class bkkMessage
        {
            public string jsonrpc { get; set; }
            public string method { get; set; }
            public bkkNotifyStatusParamInfo @params { get; set; }
        };

        private class bkkCommand
        {           
            public class params_commad
            {
                public string command;
                public string param;
            }

            public string jsonrpc;
            public string method;
            public string id;
            public params_commad @params;
        };

        private uint bkkCommandId = 1;

        public string Name { get; set; }

        public ServiceStatus Status { get; set; }

        public bool Master { get; set; }

        private static Registry ConfRegistry = null;

        private Mutex mut;

        private Cd40Cfg Cfg = null;

        private string BrekekeAddress = null;
        private uint BrekekeWebPort = 8080;
        private WebSocket pbxws = null;
        private String pbxUrl = "";

        private System.Timers.Timer Timer = new System.Timers.Timer();

        class RoomConference
        {
            public RoomConference()
            {
                roomName = "";
                room_id = "";
                participants = new List<string>();
            }

            public RoomConference(string roomname)
            {
                roomName = roomname;
                room_id = "";
                participants = new List<string>();
            }

            public string roomName;
            public string room_id;                  //Identificador que maneja Brekeke.
            public List<string> participants;       //Son los participantes que envia la PBX en los mensajes de estado
        }
        private Dictionary<string, RoomConference> roomConferences;  //La clave es el identificador de la sala de conferencia

        public ConferenceService()
        {
            Name = "ConferenceService";
            Status = ServiceStatus.Stopped;
            Master = false;
            roomConferences = new Dictionary<string, RoomConference>();
            //roomConferences.Add("1000", new RoomConference("1000"));
            //roomConferences.Add("2000", new RoomConference("2000"));
        }

        public void Start()
        {
            Status = ServiceStatus.Running;
            LogInfo<ConferenceService>("Iniciando Servicio ...");

            try
            {
                mut = new Mutex();

                Timer.Interval = 3000;
                Timer.Elapsed += (sender, eventArgs) => { OnTimedEvent(); };
                Timer.AutoReset = false;
                Timer.Start();

                InitRegistry();
            }
            catch (Exception ex)
            {
                ExceptionManage<ConferenceService>("Start", ex, "Excepcion no esperada arrancando servicio de conferencias. ERROR: " + ex.Message);
                Stop();
            }
        }

        public void Stop()
        {
            LogInfo<ConferenceService>("Finalizando Servicio ...");
            InitAndPublishAllConferenceStatus(ConferenceStatus.ConfStatus.Ok);
            Status = ServiceStatus.Stopped;
            Thread.Sleep(1000);            

            try
            {                               
                Timer.Elapsed -= (sender, eventArgs) => { OnTimedEvent(); };
                Timer.Stop();                

                if (pbxws != null)
                {
                    pbxws.Close();
                }
                EndRegistry();
            }
            catch(Exception ex)
            {
                ExceptionManage<ConferenceService>("Stop", ex, "Excepcion no esperada parando servicio de conferencias. ERROR: " + ex.Message);
            }           

            mut.Close();
        }

        public bool Commander(ServiceCommands cmd, string par, ref string err, List<string> resp = null)
        {
            throw new NotImplementedException();
        }
        public bool DataGet(ServiceCommands cmd, ref List<object> rsp)
        {
            throw new NotImplementedException();
        }
        public object AllDataGet()
        {
            throw new NotImplementedException();
        }

        private void InitRegistry()
        {
            ConfRegistry = new Registry(Identifiers.ConferenceMasterTopic);

            ConfRegistry.ChannelError += OnChannelError;
            ConfRegistry.MasterStatusChanged += OnMasterStatusChanged;
            ConfRegistry.ResourceChanged += OnRsChanged;

            ConfRegistry.SubscribeToMasterTopic(Identifiers.ConferenceMasterTopic);
            ConfRegistry.SubscribeToTopic<SrvMaster>(Identifiers.ConferenceMasterTopic);
            ConfRegistry.SubscribeToTopic<Cd40Cfg>(Identifiers.CfgTopic);
            //ConfRegistry.SubscribeToTopic<TopRs>(Identifiers.TopTopic);

            ConfRegistry.Join(Identifiers.ConferenceTopic, Identifiers.CfgTopic, Identifiers.ConferenceMasterTopic);
        }
        private void EndRegistry()
        {
            try
            {
                if (ConfRegistry != null) ConfRegistry.Dispose();
            }
            catch(Exception ex)
            {
            }
            ConfRegistry = null;
        }

        private void OnChannelError(object sender, string error)
        {
            LogError<ConferenceService>("OnChannelError: " + error, U5kiIncidencias.U5kiIncidencia.IGRL_U5KI_NBX_ERROR);
        }

        public void OnMasterStatusChanged(object sender, bool master)
        {
            if (Status != ServiceStatus.Running) return;
            
            bool retmut = mut.WaitOne(10000);
            if (retmut == false)
            {
                LogError<ConferenceService>("OnMasterStatusChanged: mutex timeout. Se finaliza el servicio");
                Stop();
                return;
            }
            
            try
            {
                if (master && !Master)
                {
                    LogInfo<ConferenceService>("MASTER", U5kiIncidencias.U5kiIncidencia.IGRL_U5KI_NBX_INFO, "ConferenceService", "MASTER");
                }
                else if (!master && Master)
                {
                    LogInfo<ConferenceService>("SLAVE", U5kiIncidencias.U5kiIncidencia.IGRL_U5KI_NBX_INFO, "ConferenceService", "SLAVE");
                    if (pbxws != null) pbxws.Close();
                }
                Master = master;
                mut.ReleaseMutex();
            }
            catch (Exception ex)
            {
                ExceptionManage<ConferenceService>("OnMasterStatusChanged", ex, "Excepcion no esperada: " + ex.Message);
                mut.ReleaseMutex();
                Stop();
            }
        }

        private void OnRsChanged(object sender, RsChangeInfo e)
        {
            if (Status != ServiceStatus.Running) return;

            bool retmut = mut.WaitOne(10000);
            if (retmut == false)
            {
                LogError<ConferenceService>("OnRsChanged: mutex timeout. Se finaliza el servicio");
                Stop();
                return;
            }

            bool stop_service = false;

            if (e.Type == Identifiers.TypeId(typeof(Cd40Cfg)))
            {
                if (e.Content != null)
                {
                    try
                    {
                        MemoryStream ms = new MemoryStream(Tools.Decompress(e.Content));
                        Cfg = Serializer.Deserialize<Cd40Cfg>(ms);

                        LogInfo<ConferenceService>(String.Format("Recibida nueva configuracion ({0})", Cfg.Version), U5kiIncidencias.U5kiIncidencia.IGNORE);

                        try
                        {
                            ProcessNewConfig(Cfg);
                        }
                        catch (Exception ex)
                        {
                            ExceptionManage<ConferenceService>("ProcessNewConfig", ex, "ProcessNewConfig Exception: " + ex.Message);
                            stop_service = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        ExceptionManage<ConferenceService>("OnRsChanged", ex, "OnRsChanged Exception: " + ex.Message);
                    }
                }
            }
            mut.ReleaseMutex();

            if (stop_service == true)
            {
                Stop();
            }
        }

        private void ProcessNewConfig(Cd40Cfg cfg)
        {           
            List<DireccionamientoIP> planDireccionamientoIP = cfg.ConfiguracionGeneral.PlanDireccionamientoIP;
            foreach(DireccionamientoIP direccionamientoIP in planDireccionamientoIP)
            {
                if (direccionamientoIP.TipoHost == Tipo_Elemento_HW.TEH_EXTERNO_TELEFONIA && 
                    direccionamientoIP.EsCentralIP &&
                    direccionamientoIP.Interno)
                {                    
                    if (BrekekeAddress != direccionamientoIP.IpRed1)
                    {
                        BrekekeAddress = direccionamientoIP.IpRed1;
                        if (pbxws != null) pbxws.Close();
                        break;
                    }
                }
            }

            //Agregamos a roomConferences las salas de conferencia nuevas
            foreach (U5ki.Infrastructure.Conferencia conferencia in cfg.ConferenciasPreprogramadas)
            {
                RoomConference roomconf;
                if (roomConferences.TryGetValue(conferencia.IdSalaBkk, out roomconf))
                {
                    //La sala de conferencia ya esta incluida                    
                }
                else
                {
                    //La sala de conferencia no esta incluida
                    roomConferences.Add(conferencia.IdSalaBkk, new RoomConference(conferencia.IdSalaBkk));
                }
            }

            //Quitamos de roomConferences las salas que ya no esten configuradas
            List<string> roomConferences_to_remove = new List<string>();
            foreach (string roomname in roomConferences.Keys)
            {
                bool found = false;
                foreach (U5ki.Infrastructure.Conferencia conferencia in cfg.ConferenciasPreprogramadas)
                {
                    if (conferencia.IdSalaBkk == roomname)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    roomConferences_to_remove.Add(roomname);
                }
            }

            foreach (string roomname_to_remove in roomConferences_to_remove)
            {
                roomConferences.Remove(roomname_to_remove);
            }

        }

        public void WebSocketInit()
        {
            pbxws = null;
            if (BrekekeAddress == null) return;
            if (BrekekeWebPort == null) return;

            pbxUrl = String.Format("ws://{0}:{1}/pbx/ws?login_user={2}&login_password={3}&user=*&status=True",
                BrekekeAddress, BrekekeWebPort,
                U5ki.PresenceService.Properties.Settings.Default.BkkUser, U5ki.PresenceService.Properties.Settings.Default.BkkPwd);
            pbxws = new WebSocket(pbxUrl);
            pbxws.Opened += new EventHandler(OnWsOpened);
            pbxws.Error += new EventHandler<SuperSocket.ClientEngine.ErrorEventArgs>(OnWsError);
            pbxws.Closed += new EventHandler(OnWsClosed);
            pbxws.MessageReceived += new EventHandler<MessageReceivedEventArgs>(OnWsData);

            LogInfo<ConferenceService>(String.Format("Inicia conexion WebSocket {0}", pbxUrl), U5kiIncidencias.U5kiIncidencia.IGNORE);
        }

        protected void OnWsOpened(object sender, EventArgs e)
        {
            if (Status != ServiceStatus.Running) return;

            bool retmut = mut.WaitOne(10000);
            if (retmut == false)
            {
                LogError<ConferenceService>("OnWsOpened: mutex timeout. Se finaliza el servicio");
                Stop();
                return;
            }

            LogInfo<ConferenceService>(String.Format("WebSocket Open url {0}", pbxUrl), U5kiIncidencias.U5kiIncidencia.IGNORE);
            InitAndPublishAllConferenceStatus(ConferenceStatus.ConfStatus.Ok);

            mut.ReleaseMutex();
        }

        protected void OnWsClosed(object sender, EventArgs e)
        {
            if (Status != ServiceStatus.Running) return;

            bool retmut = mut.WaitOne(10000);
            if (retmut == false)
            {
                LogError<ConferenceService>("OnWsClosed: mutex timeout. Se finaliza el servicio");
                Stop();
                return;
            }

            LogInfo<ConferenceService>(String.Format("WebSocket closed url {0} state {1}", pbxUrl, (pbxws != null) ? pbxws.State.ToString() : "null"), U5kiIncidencias.U5kiIncidencia.IGNORE);
            InitAndPublishAllConferenceStatus(ConferenceStatus.ConfStatus.Error);

            mut.ReleaseMutex();
        }

        protected void OnWsError(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            if (Status != ServiceStatus.Running) return;

            bool retmut = mut.WaitOne(10000);
            if (retmut == false)
            {
                LogError<ConferenceService>("OnWsClosed: mutex timeout. Se finaliza el servicio");
                Stop();
                return;
            }

            LogInfo<ConferenceService>(String.Format("WebSocket error websocket state {0} mess {1} url {2}", 
                    (pbxws != null) ? pbxws.State.ToString():"null", e.Exception.Message, pbxUrl), U5kiIncidencias.U5kiIncidencia.IGNORE);

            mut.ReleaseMutex();
        }

        protected void OnWsData(object sender, MessageReceivedEventArgs e)
        {
            if (Status != ServiceStatus.Running) return;

            bool retmut = mut.WaitOne(10000);
            if (retmut == false)
            {
                LogError<ConferenceService>("OnWsData: mutex timeout. Se finaliza el servicio");
                Stop();
                return;
            }

            try
            {
                LogTrace<ConferenceService>(String.Format("WebSocket Brekeke: Mensaje Recibido: {0}", e.Message));

                if (e.Message.StartsWith("{"))
                {
                    bkkMessage bkkmsg = JsonConvert.DeserializeObject<bkkMessage>(e.Message);
                    ProcessData(bkkmsg);
                }
                else if (e.Message.StartsWith("["))
                {
                    JsonConvert.DeserializeObject<bkkMessage[]>(e.Message).ToList().ForEach(bkkmsg =>
                    {
                        ProcessData(bkkmsg);
                    });
                }
            }
            catch (Exception x)
            {
                LogException<ConferenceService>("OnWsData Exception", x, false);
            }

            mut.ReleaseMutex();
        }

        private void OnTimedEvent()
        {
            if (Status != ServiceStatus.Running) return;

            bool retmut = mut.WaitOne(10000);
            if (retmut == false)
            {
                LogError<ConferenceService>("OnWsClosed: mutex timeout. Se finaliza el servicio");
                Stop();
                return;
            }

            bool stop_service = false;

            try
            {
                if (Status == ServiceStatus.Running && Master)
                {
                    if (pbxws == null ||
                        (pbxws != null && pbxws.State != WebSocketState.Open && pbxws.State != WebSocketState.Connecting))
                    {
                        WebSocketInit();
                        if (pbxws != null) pbxws.Open();
                    }                    
                }
            }
            catch (Exception x)
            {
                LogException<ConferenceService>("OnTimedEvent Exception", x, false);
                stop_service = true;
            }

            mut.ReleaseMutex();

            if (stop_service == true)
            {
                Stop();
            }
            else
            {
                Timer.Start();
            }
        }

        private void ProcessData(bkkMessage bkkmsg)
        {
            switch (bkkmsg.method)
            {
                case "notify_status":
                    ProcessNotifyStatus(bkkmsg);
                    break;
                case "notify_serverstatus":
                    break;
            }
        }

        private void ProcessNotifyStatus(bkkMessage bkkmsg)
        {
            RoomConference roomConf = null;

            if (bkkmsg.@params.user == null || bkkmsg.@params.room_id == null) return;
            if (bkkmsg.@params.user.Length == 0 || bkkmsg.@params.room_id.Length == 0) return;

            bool room_id_changed = false;
            bool conference_changed = false;

            LogTrace<ConferenceService>(String.Format("ProcessNotifyStatus: other_number {0} user {1}", bkkmsg.@params.other_number, bkkmsg.@params.user));

            //Busca el identificador de la sala de conferencia recibida en roomConferences
            try
            {
                if (roomConferences.TryGetValue(bkkmsg.@params.other_number, out roomConf))
                {
                    if (bkkmsg.@params.room_id != roomConf.room_id)
                    {                        
                        //El identificador interno de la PBX de la sala de conferencia ha cambiado
                        //Por alguna causa la conferencia establecida ha cambiado
                        room_id_changed = true;
                    }
                }
            }
            catch (Exception x)
            {                
            }

            //Si no lo encuentra, puede ser que el mensaje no lo incluye, entonces lo buscamos por room_id
            //que es el identificador interno que utiliza la PBX
            //Cuando un participante desaparece de la sala de conferencia, la nofificacion websocket que envia
            //la PBX no incluye other_number, por eso usamos en este caso room_id
            try
            {
                if (roomConf == null)
                {
                    foreach (RoomConference roomConference in roomConferences.Values)
                    {
                        if (roomConference.room_id == bkkmsg.@params.room_id)
                        {
                            roomConf = roomConference;
                            break;
                        }
                    }
                }
            }
            catch(Exception x)
            {
            }
                        
            if (roomConf != null)
            {
                //La sala de conferencia recibida en la notificacion esta incluida en roomConferences

                int status;
                if (int.TryParse(bkkmsg.@params.status, out status))
                {
                    if (status == (int)NotifyStatusCode.CALL_SUCCESS ||
                        status == (int)NotifyStatusCode.CALL_SUCCESS_BIS ||
                        status == (int)NotifyStatusCode.ANSWER_SUCCESS)
                    {
                        //Un usuario se ha agregado a la sala de conferencia

                        if (room_id_changed)
                        {
                            //Como el room_id ha cambiado, que es el identificador interno que usa la PBX,
                            //entonces descartamos los participantes que ya tenia roomConferences

                            roomConf.room_id = bkkmsg.@params.room_id;
                            roomConf.participants.Clear();
                            conference_changed = true;
                        }

                        //Se agrega el participante
                        string participant = bkkmsg.@params.user;
                        if (roomConf.participants.IndexOf(participant) < 0)
                        {
                            roomConf.participants.Add(participant);
                            LogTrace<ConferenceService>(String.Format("ProcessNotifyStatus: Agrega a {0} room_id {1} user {2}", bkkmsg.@params.other_number, roomConf.room_id, bkkmsg.@params.user));
                            conference_changed = true;
                        }
                    }
                    else if (status == (int)NotifyStatusCode.DISCONNECT)
                    {
                        //El participante se ha ido de la sala de conferencia
                        if (room_id_changed)
                        {
                            //Como room_id, que es el identificador interno que usa la PBX.
                            //Descartamos todos los participantes que tenia roomConferences
                            roomConf.room_id = bkkmsg.@params.room_id;
                            roomConf.participants.Clear();
                            conference_changed = true;
                        }

                        //Eliminamos el participante
                        string participant = bkkmsg.@params.user;
                        if (roomConf.participants.IndexOf(participant) >= 0)
                        {
                            roomConf.participants.Remove(participant);
                            LogTrace<ConferenceService>(String.Format("ProcessNotifyStatus: Quita de {0} room_id {1} user {2}", bkkmsg.@params.other_number, roomConf.room_id, bkkmsg.@params.user));
                            conference_changed = true;
                        }
                    }
                }
            }

            //Se envia el estado de la conferencia a los puestos, si procede
            if (roomConf != null && ConfRegistry != null && conference_changed)
            {
                ConferenceStatus message = new ConferenceStatus();
                message.RoomName = roomConf.roomName;
                message.Status = ConferenceStatus.ConfStatus.Ok;
                foreach (string part in roomConf.participants)
                {
                    message.ActiveParticipants.Add(part);
                }

                MemoryStream ms = new MemoryStream();
                Serializer.Serialize(ms, message);
                byte[] data = ms.ToArray();

                ConfRegistry.Channel.Send(Identifiers.CONFERENCE_STATUS, data, Identifiers.TopTopic);
            }
        }

        private void InitAndPublishAllConferenceStatus(ConferenceStatus.ConfStatus st)
        {
            //Esta funcion esta pensada para usarse unicamente cuando se abre y cierra el WebSocket

            try
            {
                foreach (RoomConference roomConf in roomConferences.Values)
                {
                    //Init
                    roomConf.room_id = "";
                    roomConf.participants.Clear();

                    //Publish
                    ConferenceStatus message = new ConferenceStatus();
                    message.RoomName = roomConf.roomName;
                    message.Status = st;

                    MemoryStream ms = new MemoryStream();
                    Serializer.Serialize(ms, message);
                    byte[] data = ms.ToArray();

                    ConfRegistry.Channel.Send(Identifiers.CONFERENCE_STATUS, data, Identifiers.TopTopic);
                }
            }
            catch(Exception ex)
            {
                LogException<ConferenceService>("InitAndPublishAllConferenceStatus Exception", ex, false);
            }
        }

        private void SendGetRoomsCommand()
        {
            if (pbxws == null) return;
            if (pbxws.State != WebSocketState.Open) return;

            bkkCommand comand_mess = new bkkCommand();

            comand_mess.jsonrpc = "2.0";
            comand_mess.method = "command";
            comand_mess.id = bkkCommandId.ToString();
            bkkCommandId++;
            comand_mess.@params = new bkkCommand.params_commad();
            comand_mess.@params.command = "getrooms";
            comand_mess.@params.param = "- conf *";
            string msg = JsonConvert.SerializeObject(comand_mess);
            pbxws.Send(msg);
        }
    }
}
