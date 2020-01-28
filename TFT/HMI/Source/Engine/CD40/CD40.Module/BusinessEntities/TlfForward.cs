using HMI.CD40.Module.Properties;
using HMI.Model.Module.BusinessEntities;
using HMI.Model.Module.Messages;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using U5ki.Infrastructure;
using Utilities;

namespace HMI.CD40.Module.BusinessEntities
{
#if DEBUG
    public class ForwardManager
#else
    class ForwardManager
#endif
    {
        public class DiversionSet
        {
            private static readonly Logger _Logger = LogManager.GetCurrentClassLogger();
            ///Head's uri private member
            private string _Head;
            ///display Name of head for info purposes
            private string _HeadName;

            /// <summary>
            /// List of forwards
            /// </summary>
            public List<string> ListDiv = new List<string>();

            public List<string> ListDivUser
            {
                get { 
                    List<string> listUser = new List<string>();
                    foreach (string div in ListDiv)
                    {
                        listUser.Add(new SipUtilities.SipUriParser(div).User);
                    }
                    return listUser;
                }
            }
            ///Head's uri
            public string Head
            {
                get { return _Head; }
                set { _Head = value; }
            }

            ///head's user
            public string HeadUser
            { get { return new SipUtilities.SipUriParser(_Head).User; } }
            ///Tail's Uri
            public string Tail
            { get { return ListDiv.ElementAt(0); } }

            /// <summary>
            /// Tail's user
            /// </summary>
            public string TailUser
            { get { return new SipUtilities.SipUriParser(Tail).User; } }
            ///Head's name
            public string HeadName
            {
                get { return _HeadName; }
                set { _HeadName = value; }
            }
            ///Destinos participantes excluido propio, en los desvíos para detectar cambios de estado
            public List<TlfPosition> TlfParticipantsObj { get; set; } = new List<TlfPosition>();

            public DiversionSet()
            { }
            public DiversionSet(DiversionSet ds)
            {
                if (ds != null)
                {
                    this.Head = ds.Head;
                    this.ListDiv = new List<string>(ds.ListDiv);
                }
            }
            public void BuildXml(XmlWriter xmlWriter)
            {
                try
                {
                    xmlWriter.WriteStartElement("uri");
                    xmlWriter.WriteAttributeString("id", Head);
                    if (ListDiv.Count > 0)
                    {
                        xmlWriter.WriteStartElement("list");
                        xmlWriter.WriteAttributeString("type", "div");
                        foreach (string div in ListDiv)
                        {
                            xmlWriter.WriteStartElement("id");
                            xmlWriter.WriteString(div);
                            xmlWriter.WriteEndElement();
                        }                        
                        xmlWriter.WriteEndElement();
                    }
                    xmlWriter.WriteEndElement();

                }
                catch (Exception exc)
                {
                    _Logger.Error("BuildXml " + exc.Message);
                }
            }
            public void BuildReduced(XmlWriter xmlWriter)
            {
                try
                {
                    xmlWriter.WriteStartElement("uri");
                    xmlWriter.WriteAttributeString("id", Head);
                    xmlWriter.WriteEndElement();
                }
                catch (Exception exc)
                {
                    _Logger.Error("BuildReduced "+exc.Message);
                }
            }
            public bool Equals(DiversionSet other)
            {
                if (HeadUser.Equals(other.HeadUser) == false)
                    return false;
                int i = 0;
                foreach (string div in ListDiv)
                {
                    string otherUser = new SipUtilities.SipUriParser(other.ListDiv[i]).User;
                    if (new SipUtilities.SipUriParser(div).User.Equals(otherUser) == false)
                        return false;
                    else
                        i++;
                }
                return true;
            }

        }
        public class TlfForward
        {
            //Evento que avisa del borrado autónomo de un desvío, para actualizar el estado 
            //de la función y la información asociada
            public event GenericEventHandler DiversionSetAutoRemoved;

            public List<DiversionSet> _PendingDiversionSet = new List<DiversionSet>();
            public List<DiversionSet> _LocalDiversionSet = new List<DiversionSet>();
            //Account number of programmed Forward
            public string _AccNumber;
            private bool _UseProxy = false;
            private readonly bool _UnitTest = false;
            public TlfForward(string signature)
            {
                _AccNumber = signature;
                 Top.Cfg.ProxyStateChangeCfg += OnProxyStateChange;
            }
#if DEBUG
            /// <summary>
            /// Constructor only for testing
            /// </summary>
            /// <param name="sign"></param>
            /// <param name="tlfManager">injection data for testing</param>
            public TlfForward(string sign, TlfManager tlfManager)
            {
                _AccNumber = sign;
                _UnitTest = true;
                Top.Tlf = tlfManager;
            }
#endif
#region HeadNode
            private string MyUri()
            {
                if (_UseProxy)
                    return string.Format("sip:{0}@{1}", _AccNumber, Top.Cfg.GetProxyIp(out string idEquipo));
                else
                    return string.Format("sip:{0}@{1}", _AccNumber, Top.SipIp);
            }
            private void OnProxyStateChange(object sender, bool state)
            {
                _UseProxy = state;
            }
            /// <summary>
            /// Envio de un request forward programado desde el GUI
            /// </summary>
            /// <param name="targetUri"></param>
            /// 
            public void RequestForward(string targetUri)
            {

                DiversionSet[] newDs = new DiversionSet[1] { new DiversionSet() };
                _PendingDiversionSet.Add(newDs[0]);
                 newDs[0].Head = MyUri();
                //El tail lo saco de las uris del id
                newDs[0].ListDiv.Insert(0, targetUri.Replace(">", "").Replace("<", ""));
                SendRequest(newDs, targetUri);
            }
            /// <summary>
            /// Envio de un request forward por caída de un participante de un forward existente.
            /// Reprogramación del forward de forma retardada para que todos los participantes reciban 
            /// la caída.
            /// </summary>
            /// <param name="ds"></param>
            public void RequestForward(DiversionSet ds)
            {
                Thread.Sleep(100);
                _PendingDiversionSet.Add(ds);
                SendRequest(new DiversionSet[1] { ds }, ds.Tail);
 
            }
            /// <summary>
            /// Función que cancela un desvío programado, sólo desde el punto de vista de HeadPoint. 
            /// </summary>
            public void Cancel()
            {
                foreach (DiversionSet ds in _LocalDiversionSet.FindAll(x=>x.HeadUser.Equals(_AccNumber)))
                {
                    SendRelease(ds, ds.Tail);
                    //Limpio las suscripciones a cambios de estados de TlfPositions
                    RemoveParticipants(ds);
                }
                //que hago con los pending?
                _PendingDiversionSet.Clear();
            }

            /// <summary>
            /// Send an option with request
            /// </summary>
            public void SendRequest(DiversionSet[] forward, string destination)
            {
                string callId;
                string body = BuildXml(forward);
                if (!_UnitTest)
                    SipAgent.SendOptionsCFWD(_AccNumber, destination, CORESIP_CFWR_OPT_TYPE.CORESIP_CFWR_OPT_REQUEST, body, out callId, _UseProxy);
            }
            /// <summary>
            /// Send an option with release
            /// </summary>
            private void SendRelease(DiversionSet forward, string destination)
            {
                string callId;
                DiversionSet[] sendDS = new DiversionSet[1] { forward };
                string body=BuildXml(sendDS, true);
                if (!_UnitTest)
                  SipAgent.SendOptionsCFWD(_AccNumber, destination, CORESIP_CFWR_OPT_TYPE.CORESIP_CFWR_OPT_RELEASE, body, out callId, _UseProxy);
            }
            /// <summary>
            /// Send an option with release
            /// </summary>
            private void SendUpdate(DiversionSet[] forward, string destination)
            {
                string callId;
                string body = BuildXml(forward);
                if (!_UnitTest)
                  SipAgent.SendOptionsCFWD(_AccNumber, destination, CORESIP_CFWR_OPT_TYPE.CORESIP_CFWR_OPT_UPDATE, body, out callId, _UseProxy);
            }
            /// <summary>
            ///Receive an answer to request
            ///1-if another local diversion set exists, update existing diversion set inserting above, and send to its head.
            ///2-if answer is equal to pending store as local diversion set
            ///3-Modify request
            /// <param name="hopTail">returns tail if there is a hop in forward chain/param>
            /// <returns> true if negotiation finish here, false otherwise</returns>
            /// </summary>
            public bool AnswerRequest(string xmlBody, out string hopTail, int statusCode = SipAgent.SIP_OK)
            {
                bool terminated = false;
                hopTail = "";
                List<DiversionSet> removeLocal = new List<DiversionSet>();
                DiversionSet[] remoteDiversionSet;
                remoteDiversionSet = ParseXml(xmlBody);
                if ((remoteDiversionSet.Length == 0) || (statusCode != SipAgent.SIP_OK))
                {
                    _PendingDiversionSet.Clear();
                    terminated = true;
                }
                foreach (DiversionSet remote in remoteDiversionSet)
                {
                    if (statusCode != SipAgent.SIP_OK)
                    {
                        _PendingDiversionSet.RemoveAll(ds => ds.Equals(remote));
                        terminated = true;
                        break;
                    }

                    DiversionSet pending = _PendingDiversionSet.Find(fD => fD.HeadUser.Equals(remote.HeadUser));
                    //1- Update existing diversion set
                    foreach (DiversionSet local in _LocalDiversionSet)
                    {
                        foreach (string localDiv in local.ListDiv)
                        {

                            if (remote.HeadUser.Equals(new SipUtilities.SipUriParser(localDiv).User))
                            {
                                DiversionSet[] sendDS = new DiversionSet[1] ;
                                sendDS[0] = new DiversionSet(local);
                                removeLocal.Add(local);
                                sendDS[0].ListDiv.InsertRange(local.ListDiv.IndexOf(localDiv), remote.ListDiv);
                                SendUpdate(sendDS, sendDS[0].Head);
                                _Logger.Trace("AnswerRequest: update existing div set");
                            }
                        }
                    }
                    foreach (DiversionSet ds in removeLocal)
                    {
                        _LocalDiversionSet.Remove(ds);
                        RemoveParticipants(ds);
                    }
                    if (pending != null)
                    {
                        if (pending.Equals(remote))
                        {
                            //2-store new local diversion set
                            AddParticipants(remote);
                            _LocalDiversionSet.Add(remote);
                            _PendingDiversionSet.Remove(pending);
                            _Logger.Trace("AnswerRequest: store local div set");
                            terminated = true;
                            if (remote.ListDiv.Count > 1)
                                hopTail = remote.TailUser;
                            else hopTail = "";
                        }
                        else /*if (!pending.Head.Equals(remoteDiversionSet.Head))*/
                        {
                            //3-Modify request
                            _PendingDiversionSet.Remove(pending);
                            _PendingDiversionSet.Add(remote);
                            SendRequest(remoteDiversionSet, remote.Tail);
                            _Logger.Trace("AnswerRequest: request different div set");
                        }
                    }
                    else
                        _Logger.Error("AnswerRequest error, no pending request");
                }
                return terminated;
            }

            /// <summary>
            ///Receive an answer to release
            ///1-if there is a diversion set with same head, delete it
            ///2-else if it has a different one, send the update to its head
            /// </summary>
            public void AnswerRelease(string xmlBody, int statusCode = SipAgent.SIP_OK)
            {
                if (statusCode != SipAgent.SIP_OK)
                {
                    //Hay que borrar el desvío aun sin negociación
                    _LocalDiversionSet.Clear();
                    return;
                }

                DiversionSet[] answerRelease = ParseXml(xmlBody);
                foreach (DiversionSet remote in answerRelease)
                {
                    //1-if there is a diversion set with same head, delete it
                    DiversionSet toRemove =_LocalDiversionSet.Find(fD => fD.HeadUser.Equals(remote.HeadUser));
                    if (_LocalDiversionSet.RemoveAll(fD => fD.HeadUser.Equals(remote.HeadUser)) == 0)
                    {
                        if (remote.ListDiv.Count() > 0)
                        {
                            //2-else if it has a different one, send the update to its head
                            DiversionSet[] toSend = new DiversionSet[1] { remote };
                            SendUpdate(toSend, remote.Head);
                        }
                    }
                    else
                        RemoveParticipants(toRemove);
                }
            }
#endregion
#region EndPoint
            /// <summary>
            ///Receive a forward request
            ///1-if no local diversion set exists for head received, store as local
            ///2-If request diversion set head matches a tail of a local diversion set, send loop detected
            ///3-If any local head matches remote tail, change the received tail
            /// </summary>
            /// <param name="xmlBody"> xml received</param>
            /// <param name="intId">/param>
            /// <returns>bool, true if there is a new diversion set</returns>
            public string ReceiveRequest(string xmlBody, string fromName, uint intId= 0)
            {
                string diversion = "";
                bool answerSent = false;
                DiversionSet[] request = ParseXml(xmlBody);
                foreach (DiversionSet remoteReq in request)
                {
                    remoteReq.HeadName = fromName;
                    answerSent = false;
                    DiversionSet found = _LocalDiversionSet.Find(ds => ds.HeadUser.Equals(remoteReq.HeadUser));
                    DiversionSet[] toSend = new DiversionSet[1] { remoteReq };

                    if (_LocalDiversionSet.Count > 0)
                    {
                        //2 - If request diversion set head matches a tail of a local diversion set, send loop detected
                        foreach (DiversionSet local in _LocalDiversionSet)
                            if (local.TailUser.Equals(remoteReq.HeadUser))
                            {
                                SendAnswer(request, intId, SipAgent.SIP_LOOP_DETECTED);
                                return diversion;
                            }
                        //3-If any local head matches remote tail, change the received tail
                        foreach (DiversionSet local in _LocalDiversionSet)
                            if (local.HeadUser.Equals(remoteReq.TailUser))
                            {
                                remoteReq.ListDiv.InsertRange(0, local.ListDiv);
                                toSend = new DiversionSet[1] { remoteReq };
                                SendAnswer(toSend, intId, SipAgent.SIP_OK);
                                return diversion;
                            }
                    }
                    if (found == null)
                    {
                        //1 -if no local diversion set exists, store as local
                        AddParticipants(remoteReq);
                        _LocalDiversionSet.Add(remoteReq);
                        SendAnswer(toSend, intId, SipAgent.SIP_OK);
                        answerSent = true;
                    }
                    else if (found.Equals(remoteReq))
                    {
                        //In case of reception of a DS that matches another already configured 
                        //the answer is OK instead of rejected
                        //Resolves when Head looses the diversion set and retries to end user
                        //if end user accepts, it can be removed after, otherwise it exists forever
                        SendAnswer(toSend, intId, SipAgent.SIP_OK);
                        answerSent = true;
                    }
                }
                if (!answerSent)
                    SendAnswer(request, intId, SipAgent.SIP_NOT_ACCEPTABLE);
                List<string> listOfNames = new List<string>();
                foreach (DiversionSet ds in _LocalDiversionSet)
                {
                    SipUtilities.SipUriParser sipUri = new SipUtilities.SipUriParser(ds.Head);
                    string name = String.IsNullOrEmpty(ds.HeadName) ? sipUri.User : ds.HeadName;
                    //Para evitar que se repitan nombres (es el caso de puestos con una agrupación de sectores)
                    if (listOfNames.Contains(name) == false)
                        listOfNames.Add(name);
                }
                //Se construye la cadena con comas
                foreach (string name in listOfNames)
                {
                    if (diversion.Length > 0)
                        diversion += ", ";
                    diversion += name;
                }

                return diversion;
                
            }
            /// <summary>
            ///Receive a release request
            ///1-If head of diversion set received matches local diversion set, then remove it.
            ///2-If there is a local diversion set that includes, the head received, then modify existing diversion set
            /// </summary>
            /// <param name="xmlBody"> xml received</param>
            /// <param name="intId">/param>
            /// <returns>bool, true if a diversion set is removed from local</returns>
            public string ReceiveRelease(string xmlBody, uint internalId = 0)
            {
                DiversionSet[] release = ParseXml(xmlBody);

                foreach (DiversionSet remoteRelease in release)
                {                   
                    List<DiversionSet> toSend = new List<DiversionSet>();
                    DiversionSet found = _LocalDiversionSet.Find(fD => fD.HeadUser.Equals(remoteRelease.HeadUser));
                    if (found != null)
                    {
                        //1 - If head of diversion set received matches local diversion set, then remove it.
                        _LocalDiversionSet.Remove(found);
                        toSend.Add(remoteRelease);
                        RemoveParticipants(found);
                    }

                    if (_LocalDiversionSet.Count > 0)
                    {
                        //2 - If there is a local diversion set that includes, the head received, then modify existing diversion set
                        //Temporal copy to iterate
                        List<DiversionSet> tempCopy = new List<DiversionSet>(_LocalDiversionSet);
                        IEnumerable<DiversionSet> foundList = tempCopy.Where<DiversionSet>(ds => ds.ListDivUser.Contains(remoteRelease.HeadUser));
                        int index = -1;
                        foreach (DiversionSet newDs in foundList)
                        {
                            index = newDs.ListDivUser.IndexOf(remoteRelease.HeadUser);
                            if (index > 0)
                            {
                                _LocalDiversionSet.Remove(newDs);
                                newDs.ListDiv.RemoveRange(0, index);
                                toSend.Add(newDs);
                                RemoveParticipants(newDs);
                            }                           
                        }
                    }
                    if (toSend.Count > 0)
                        SendAnswer(toSend.ToArray(), internalId, SipAgent.SIP_OK);
                    else
                        //If there is nothing found, anyway I answer ok
                        //to allow remote to clean the forward
                        SendAnswer(release, internalId, SipAgent.SIP_OK);
                }
                return HeadsForwarded();
            }

            public string HeadsForwarded()
            {
                string removeDiversion = "";
                foreach (DiversionSet ds in _LocalDiversionSet)
                {
                    if (removeDiversion.Length > 0)
                        removeDiversion += ", ";
                    SipUtilities.SipUriParser sipUri = new SipUtilities.SipUriParser(ds.Head);
                    removeDiversion += String.IsNullOrEmpty(ds.HeadName) ? sipUri.User : ds.HeadName;
                }
                return removeDiversion;
            }
            /// <summary>
            ///Receive an update
            /// </summary>
            public void ReceiveUpdate(string xmlBody, uint internalId = 0)
            {
                DiversionSet[] update = ParseXml(xmlBody);
                foreach (DiversionSet remoteUpdate in update)
                {                   
                    List<DiversionSet> updateFound = _LocalDiversionSet.FindAll(local => local.HeadUser.Equals(remoteUpdate.HeadUser));
                    foreach (DiversionSet ds in updateFound)
                    {
                        _LocalDiversionSet.Remove(ds);
                        _PendingDiversionSet.Add(remoteUpdate);

                        DiversionSet[] toSend = new DiversionSet[1] { remoteUpdate };
                        SendRequest(toSend, remoteUpdate.Tail);
                        RemoveParticipants(ds);
                    }
                }
                if (!_UnitTest)
                    SipAgent.SendResponseCFWD(SipAgent.SIP_OK, xmlBody, internalId);
            }

            public string GetDiversionTail()
            {
                DiversionSet found = _LocalDiversionSet.Find(ds => ds.HeadUser.Equals(_AccNumber));
                return found == null ? "" : found.Tail;
            }

            /// <summary>
            /// Send answer to a request, update or release
            /// </summary>
            /// <param name="forward"></param>
            /// <param name="internalId">unique reference to answered message</param>
            /// <param name="code">error code</param>
            private void SendAnswer(DiversionSet[] forward, uint internalId, int code)
            {
                string body = BuildXml(forward);
                if (!_UnitTest)
                    SipAgent.SendResponseCFWD(code, body, internalId);
            }
#endregion

            private string BuildXml(DiversionSet[] sets, bool reduced= false)
            {
                string ret = null;
                try
                {
                    MemoryStream str = new MemoryStream();
                    XmlTextWriter xmlWriter = new XmlTextWriter(str, Encoding.UTF8) { Formatting = Formatting.Indented };
                    //XmlWriter xmlWriter = XmlWriter.Create(fileName, settings);

                    xmlWriter.WriteStartDocument();
                    xmlWriter.WriteStartElement("ed137");
                    xmlWriter.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                    xmlWriter.WriteStartElement("cfwd_basic");
                    foreach (DiversionSet ds in sets)
                        if (reduced)
                            ds.BuildReduced(xmlWriter);
                        else
                            ds.BuildXml(xmlWriter);
                    xmlWriter.WriteEndDocument();
                    xmlWriter.Flush();

                    if (_UnitTest)
                    {
                        string fileName = ".\\Forward_" + _AccNumber.Split('@')[0] + ".xml";
                        FileStream file = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                        str.Seek(0, SeekOrigin.Begin);
                        str.CopyTo(file);
                        file.Flush();
                        file.Close();
                    }
                    StreamReader reader = new StreamReader(str, Encoding.UTF8, true);
                    str.Seek(0, SeekOrigin.Begin);
                    ret = reader.ReadToEnd();
                    xmlWriter.Close();
                    reader.Dispose();
                }
                catch (Exception exc)
                {
                    _Logger.Error("BuildXml " + exc.Message);
                }
                //return (File.ReadAllText(fileName, Encoding.UTF8));
                return ret;
            }
            public DiversionSet[] ParseXml(string body)
            {
                List<DiversionSet> sets = new List<DiversionSet>();
                int index = 0;
                XmlReader xmlReader;
                if (!String.IsNullOrEmpty(body))
                {
                    try
                    {
                        if (_UnitTest)
                        {
                            //In unit tests the xml body is in a file named with origin of message                       
                            string fileName = ".\\Forward_" + body.Split('@')[0] + ".xml";
                            xmlReader = XmlReader.Create(fileName);
                        }
                        else
                            xmlReader = XmlReader.Create(new StringReader(body));

                        while (xmlReader.Read())
                        {
                            if ((xmlReader.NodeType == XmlNodeType.Element) && (xmlReader.Name == "uri"))
                            {
                                if (xmlReader.HasAttributes)
                                {
                                    DiversionSet ds = new DiversionSet() { Head = xmlReader.GetAttribute("id") };
                                    sets.Add(ds);
                                    index = sets.FindIndex(x => x.Equals(ds));
                                }
                            }
                            else if ((xmlReader.NodeType == XmlNodeType.Element) && (xmlReader.Name == "id"))
                            {
                                xmlReader.Read();
                                if (xmlReader.NodeType == XmlNodeType.Text)
                                {
                                    sets.ElementAt(index).ListDiv.Add(xmlReader.Value);
                                }
                            }
                        }
                        xmlReader.Close();
                    }
                    catch (Exception exc)
                    {
                        _Logger.Error("ParseXml " + exc.Message);
                    }
                }
                return sets.ToArray();
            }

            /// <summary>
            /// Añade participantes en los desvíos, para suscribirse a sus cambios de estado
            /// </summary>
            /// <param name="newDs"></param>
            private void AddParticipants(DiversionSet newDs)
            {
                //bool found = false;
                TlfPosition tlf;
                newDs.TlfParticipantsObj.Clear();
                if (!newDs.HeadUser.Equals(_AccNumber))
                {
                    tlf = Top.Tlf.GetTlfPosition(newDs.Head, newDs.HeadName, false, true);
                    if (tlf != null) 
                    {
                        if (_LocalDiversionSet.Exists(ds => ds.TlfParticipantsObj.Contains(tlf)) == false)
                        {
                            tlf.TlfPosStateChanged += OnTlfParticipantStateChange;
                        }
                        newDs.TlfParticipantsObj.Add(tlf);
                    }
                }
                foreach (string div in newDs.ListDiv)
                {
                    //found = false;
                    tlf = null;
                    string divUser = newDs.ListDivUser.ElementAt(newDs.ListDiv.IndexOf(div));
                    if (!divUser.Equals(_AccNumber))
                    {
                        //if ((Top.Tlf.Forward._Peer != null) &&
                        //    (new SipUtilities.SipUriParser(Top.Tlf.Forward._Peer.Uri).User.Equals(divUser)))
                        //    tlf = Top.Tlf.Forward._Peer;
                        if (tlf == null)
                    tlf = Top.Tlf.GetTlfPosition(div, null, false, true);

                    if (tlf != null)
                    {
                        if (_LocalDiversionSet.Exists(ds => ds.TlfParticipantsObj.Contains(tlf)) == false)
                        {
                            tlf.TlfPosStateChanged += OnTlfParticipantStateChange;                            
                        }
                        newDs.TlfParticipantsObj.Add(tlf);
                    }                    
                }
            }
            }

            private void RemoveParticipants(DiversionSet remDs)
            {
                foreach (TlfPosition tlf in remDs.TlfParticipantsObj)
                {
                     if (_LocalDiversionSet.Exists(ds => ds.TlfParticipantsObj.Contains(tlf)) == false)
                    tlf.TlfPosStateChanged -= OnTlfParticipantStateChange;
                    //Hay que hacer dispose de los TlfPosition creados desde aqui
                    if (tlf.Pos == Tlf.NumDestinations + Tlf.NumIaDestinations)
                        tlf.Dispose();
                }
                remDs.TlfParticipantsObj.Clear();
            }

            private void OnTlfParticipantStateChange(object sender)
            {
                TlfPosition tlf = (TlfPosition)sender;
                TlfState st = tlf.State;
                _Logger.Trace(String.Format("OnTlfParticipantStateChange {0}", st));
                if (st == TlfState.Unavailable)
                {
                   AutoRemoveDiversionSet(tlf);
                }
            }
            /// <summary>
            /// Borra o modifica los desvíos programados cuando uno de los participantes se cae.
            /// 1.Si es un head, se borra el desvío.
            /// 2.Si es un tail y no hay otros div en el diversion, se borra el desvio
            /// 3.Si es un tail y hay otros div, se negocia un nuevo desvío con otro tail y el mismo head.
            /// 4.Si es un elemento div que no es tail, no se hace nada
            /// </summary>
            /// <param name="tlf"></param>
            /// <param name="remDest"></param>
            public void AutoRemoveDiversionSet(TlfPosition tlf)
            {
                List<DiversionSet> affectedDs = new List<DiversionSet>(); ;
                affectedDs = _LocalDiversionSet.FindAll(x => x.TlfParticipantsObj.Contains(tlf));
                foreach (string remDest in tlf.NumerosAbonado)
                {
                    AutoRemove(remDest, affectedDs);
                }
            }

            public void AutoRemoveAllDiversionSet()
            {
                foreach(DiversionSet ds in _LocalDiversionSet)
                {
                    RemoveParticipants(ds);
                }
                _LocalDiversionSet.Clear();
                _PendingDiversionSet.Clear();
                General.SafeLaunchEvent(DiversionSetAutoRemoved, this);
            }

        /// <summary>
        /// Borra o modifica los desvíos programados cuando uno de los participantes se cae.
        /// 1.Si es un head, se borra el desvío.
        /// 2.Si es un tail y no hay otros div en el diversion, se borra el desvio
        /// 3.Si es un tail y hay otros div, se negocia un nuevo desvío con otro tail y el mismo head.
        /// 4.Si es un elemento div que no es tail, no se hace nada
        /// </summary>
        /// <param name="tlf"></param>
        /// <param name="remDest"></param>
        public void AutoRemoveDiversionSet(string remDest)
        {
            AutoRemove(remDest, new List<DiversionSet> (_LocalDiversionSet));
        }

        private void AutoRemove(string remDest, List<DiversionSet> affectedDs)
        {
                foreach (DiversionSet ds in affectedDs)
                {
                    if (ds.HeadUser.Equals(remDest))
                    {
                        //1.Si es un head, se borra el desvío.
                        _LocalDiversionSet.Remove(ds);
                        RemoveParticipants(ds);
                        General.SafeLaunchEvent(DiversionSetAutoRemoved, this);
                    }
                    else if (ds.ListDivUser.Contains(remDest))
                    {
                        if (ds.TailUser.Equals(remDest))
                        {
                            // 2.Si es un tail y no hay otros div en el diversion, se borra el desvio
                            _LocalDiversionSet.Remove(ds);
                            RemoveParticipants(ds);
                            if (ds.ListDiv.Count > 1)
                            {
                                // 3. Si es un tail y hay otros div, se negocia un nuevo desvío con otro tail y el mismo head.
                                ds.ListDiv.RemoveAt(0);
                                RequestForward(ds);
                            }
                            else
                                General.SafeLaunchEvent(DiversionSetAutoRemoved, this);
                        }
                        else
                        {
                            // 4.Si es un elemento div que no es tail, no se hace nada
                        }
                    }
                }
            }
        }

        public event GenericEventHandler<ListenPickUpMsg> ForwardChanged;
        public event GenericEventHandler<ListenPickUpMsg> RemoteForwardChanged;
        public event GenericEventHandler<string> ForwardError;
        public event GenericEventHandler<SnmpStringMsg<string, string>> SetSnmpString;

        private readonly static Logger _Logger = LogManager.GetCurrentClassLogger();
        private readonly List<string> _MyAccounts = new List<string>();
        //There is one TlfFoward for each different SIP account 
        private readonly Dictionary<string, TlfForward> _ForwardAcc = new Dictionary<string, TlfForward>();
        ///State of Forward procedure:
        ///Idle: No forward in progress
        ///Ready: forward initiated, waiting target to be selected
        ///Executing: Target selected, forward programmed
        ///Error: Forward process failed
        private FunctionState _State = FunctionState.Idle;
        TlfPosition _Peer;
        /// <summary>
        /// name of tail in diversionSet when is different from Peer 
        /// </summary>
        string _TailName = null;
        public FunctionState State
        {
            get { return _State; }
            private set
            {
                //if (value != _State)
                {
                    _State = value;

                    if (_State == FunctionState.Executing)
                    {
                         General.SafeLaunchEvent(ForwardChanged, this, new ListenPickUpMsg(_State, _Peer.Literal, _TailName));
                    }
                    else
                    {
                        General.SafeLaunchEvent(ForwardChanged, this, new ListenPickUpMsg(_State));
                    }
                    
                }
            }
        }

        public ForwardManager()
        {
            //Añadir suscripción a eventos. 
            //recepción de XML en el options cfwd basic
            Top.Sip.CallForwardAsk += OnCallForwardReceived;
            Top.Sip.CallForwardResp += OnCallForwardResponse;
            Top.Cfg.ConfigChanged += OnConfigChanged;
        }

        public void Prepare(int id)
        {
            PrepareCommon(Top.Tlf[id]);
        }

        /// <summary>
        /// Programación de un desvio desde el teclado de marcacion.
        /// Si la tecla a utilizar es una 19+1, nos hacemos una copia para poder liberarla 
        /// y que pueda ser utilizada
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="dst"></param>
        /// <param name="number"></param>
        /// <param name="lit"></param>
        public void Prepare(uint prefix, string dst, string number, string lit)
        {
            //Se busca si existe un AD que corresponda con esa marcacion
            TlfPosition target = Top.Tlf.SearchTlfPosition(prefix, dst, number, lit, false);
            if (target == null)
            {
                TlfIaPosition AIDTarget;
                AIDTarget = new TlfIaPosition(Tlf.NumDestinations + Tlf.NumIaDestinations);
                if (AIDTarget.CanHandleOutputCall(prefix, dst, number, lit, true)== true)
                    target = (TlfPosition)AIDTarget;
            }
            PrepareCommon(target);
        }
        /// <summary>
        /// Función que cancela todos los desvíos programados como head. 
        /// Se llama por el operador o por sectorizacion
        /// </summary>
        public void Cancel(bool all)
        {
            foreach (TlfForward forward in _ForwardAcc.Values)
            {
                if (all)
                    forward.AutoRemoveAllDiversionSet();
                else
                forward.Cancel();
            }

            State = FunctionState.Idle;
            if (_Peer != null)
            {
                Top.WorkingThread.Enqueue("SetSnmp", delegate ()
                {
                    string snmpString = Top.Cfg.PositionId + "_" + "CANCEL FORWARD" + "_"+ _Peer.Literal;
                    General.SafeLaunchEvent(SetSnmpString, this, new SnmpStringMsg<string, string>(Settings.Default.TlfFacilityOid, snmpString));
                });

            if (_Peer.Pos == Tlf.NumDestinations + Tlf.NumIaDestinations)
                _Peer.Dispose();
            _Peer = null;
        }
        }

        public string GetDiversionTail(string account)
        {
            string ret = "";
            TlfForward forward;
            if (_ForwardAcc.TryGetValue(account, out forward))
                ret = forward.GetDiversionTail();
            return ret;
        }
        /// <summary>
        /// Función llamada para aceptar o no una llamada saliente desde el TlfManager
        /// Se rechaza si se tiene un desvio desde ese numero
        /// </summary>
        /// <param name="numAbonados"></param>
        /// <returns></returns>
        public bool IsForwardedHead(IList<string> numAbonados)
        {
            foreach (string numAbonado in numAbonados)
                foreach (TlfForward forward in _ForwardAcc.Values)
                    if (forward._LocalDiversionSet.Find(x => x.HeadUser.Equals(numAbonado)) != null)
                    {
                        General.SafeLaunchEvent(ForwardError, this, Resources.ForwardError);
                        return true;
                    }
            return false;
        }

        private void PrepareCommon(TlfPosition target)
        {
            _Peer = target;

            if (_Peer != null)
            {
                foreach (string myAccount in _MyAccounts)
                {
                    TlfForward newForward;
                    _ForwardAcc.TryGetValue(myAccount, out newForward);
                    //foreach (string targetUri in target.Channels[0].GetUris)
                    string targetUri = _Peer.Uri;
                    newForward.RequestForward(targetUri);
                }
                Top.WorkingThread.Enqueue("SetSnmp", delegate ()
                {
                    string snmpString = Top.Cfg.PositionId + "_" + "FORWARD" + "_"+_Peer.Literal;
                    General.SafeLaunchEvent(SetSnmpString, this, new SnmpStringMsg<string, string>(Settings.Default.TlfFacilityOid, snmpString));
                });

            }
            else
            {
                _Logger.Error(String.Format("TlfForward:Prepare AID error target is null"));
                State = FunctionState.Error;
            }
        }

        /// <summary>
        /// Receive an options with a command for call forward
        /// </summary>
        /// <param name="sender"></param>
        private void OnCallForwardReceived(object sender, string accId, string from, CORESIP_CFWR_OPT_TYPE type, string xmlBody, uint intId)
        {
            TlfForward forward;
            if (_ForwardAcc.TryGetValue(accId, out forward))
            {
                switch (type)
                {
                    case CORESIP_CFWR_OPT_TYPE.CORESIP_CFWR_OPT_REQUEST:
                        SipUtilities.SipUriParser fromUri = new SipUtilities.SipUriParser(from);
                        string fromName = String.IsNullOrEmpty(fromUri.DisplayName) ? fromUri.User : fromUri.DisplayName;
                        string peers =forward.ReceiveRequest(xmlBody, fromName, intId);
                        if (peers.Length > 0)
                        {
                            General.SafeLaunchEvent(RemoteForwardChanged, this, new ListenPickUpMsg(_State, peers));
                        }
                        break;
                    case CORESIP_CFWR_OPT_TYPE.CORESIP_CFWR_OPT_RELEASE:
                        peers = forward.ReceiveRelease(xmlBody, intId);
                        if (peers.Length > 0)
                        {
                            //No cambiar el estado
                            General.SafeLaunchEvent(RemoteForwardChanged, this, new ListenPickUpMsg(_State, peers));
                        }
                        else
                            General.SafeLaunchEvent(RemoteForwardChanged, this, new ListenPickUpMsg(_State));
                        break;
                    case CORESIP_CFWR_OPT_TYPE.CORESIP_CFWR_OPT_UPDATE:
                        forward.ReceiveUpdate(xmlBody, intId);
                        break;
                    default:
                        _Logger.Error("OnCallForwardReceived: Unknown type of message in options");
                        break;
                }
            }
            else
                _Logger.Error("OnCallForwardReceived: no call forward for accId: "+accId);
        }

        /// <summary>
        /// Receive an options with a command response for Call forward
        /// </summary>
        /// <param name="sender"></param>
        private void OnCallForwardResponse(object sender, string accId, int code, CORESIP_CFWR_OPT_TYPE type, string xmlBody)
        {
            TlfForward forward;
            if (_ForwardAcc.TryGetValue(accId, out forward))
            {
                switch (type)
                {
                    case CORESIP_CFWR_OPT_TYPE.CORESIP_CFWR_OPT_REQUEST:
                        if (forward.AnswerRequest(xmlBody, out string tail, code))
                        {
                            _TailName = tail;
                            if (code != SipAgent.SIP_OK)
                                State = FunctionState.Error;
                            else
                                State = FunctionState.Executing;
                        }
                        break;
                    case CORESIP_CFWR_OPT_TYPE.CORESIP_CFWR_OPT_RELEASE:
                        forward.AnswerRelease(xmlBody, code);
                        _State = FunctionState.Idle;
                        break;
                    case CORESIP_CFWR_OPT_TYPE.CORESIP_CFWR_OPT_UPDATE:
                        //forward.ReceiveUpdate(xmlBody);
                        break;
                    default:
                        _Logger.Error("OnCallForwardResponse: Unknown type of message in options");
                        break;
                }
            }
            else
                _Logger.Error("OnCallForwardResponse: no call forward for accId: " + accId);

        }
        /// <summary>
        /// Receive change of configuration
        /// All programmed forwards must be cancelled
        /// </summary>
        /// <param name="sender"></param>
        private void OnConfigChanged(object sender)
        {
            //Cancel through negotiation first
            Cancel(false);
            //Cancel resto of forwards if any
            Cancel(true);

            Dictionary<string, TlfForward> oldForwardsAcc = new Dictionary<string, TlfForward>(_ForwardAcc);
            _MyAccounts.Clear();
            _ForwardAcc.Clear();
            foreach (StrNumeroAbonado num in Top.Cfg.HostAddresses)
            {
                //Solo para numeracion ATS del sector (se excluyen cuentas RTB por ejemplo)
                if (num.Prefijo == Cd40Cfg.ATS_DST)
                {
                _MyAccounts.Add(num.NumeroAbonado);
                TlfForward newForward;
                if (oldForwardsAcc.TryGetValue(num.NumeroAbonado, out newForward) == false)
                {
                    newForward = new TlfForward(num.NumeroAbonado);
                    newForward.DiversionSetAutoRemoved += OnAutoDiversionSetRemoved;
                }
                _ForwardAcc.Add(num.NumeroAbonado, newForward);
                oldForwardsAcc.Remove(num.NumeroAbonado);
            }
            }

            //Si han cambiado mis numeros de usuario, borro todos mis desvios 
            //if (oldForwardsAcc.Count > 0)
            //{
            //    //Borra los desvios de la cuenta existente
            //    //Cancel();
            //    //Borra los desvios de la cuenta que ya no está
            //    foreach (TlfForward forward in oldForwardsAcc.Values)
            //    {                    
            //        forward.AutoRemoveDiversionSet(forward._AccNumber);
            //        forward.DiversionSetAutoRemoved -= OnAutoDiversionSetRemoved;
            //    }
            //}
            //TODO si el otro cambia de host o de IP, habría que borrarlo
            //TODO Falta comprobar si han cambiado todos los participantes
            //foreach (TlfForward forward in _ForwardAcc.Values)
            //{
            //    forward.OnConfigChanged();
            //}

        }
        /// <summary>
        /// Evento enviado por el TlfForward para avisar de que se ha detectado 
        /// una caida y se ha borrado un desvío sin negociación
        /// </summary>
        /// <param name="sender"></param>
        private void OnAutoDiversionSetRemoved(object sender)
        {
            TlfForward forward = (TlfForward)sender;
            if (_ForwardAcc.Values.Where(x => x._LocalDiversionSet.Count != 0).Count() == 0)
                State = FunctionState.Idle;
                string peers = forward.HeadsForwarded();
				if (peers.Length > 0)
				{
					General.SafeLaunchEvent(RemoteForwardChanged, this, new ListenPickUpMsg(_State, peers));
				}
            else
                General.SafeLaunchEvent(RemoteForwardChanged, this, new ListenPickUpMsg(_State));

			}                

    }

}
