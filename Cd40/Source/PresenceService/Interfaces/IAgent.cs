﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace U5ki.PresenceService.Interfaces
{
    public enum AgentType { Gw = 1, ForInternalSub = 5, ForExternalSub = 3, ForProxies = 4, ForExternalResources=6 }
    public enum AgentEvents { Active, Inactive, Refresh, Ping, SubscribeUser, UnsubscribeUser, LogException, ResourceOptions }
    public enum AgentStates { NotConnected, Connected };
    public class AgentEventArgs : EventArgs
    {
        public IAgent agent { get; set; }
        public AgentEvents ev;
        public string p1;
        public Exception x;
        /***/
        public Action<object> retorno;
    }
    public interface IAgent : IDisposable
    {
        IAgentEngine engine { get; set; }
        IPEndPoint ProxyEndpoint { get; set; }
        IPEndPoint PresenceEndpoint { get; set; }
        AgentType Type { get; }
        String Name { get; }
        AgentStates State { get; set; }
        bool MainService { get; set; }
        string DependencyName { get; set; }
        //string ProxyOptionsUri { get; }
        int Order { get; set; }
        string callIdOptions { get; set; }
        List<PresenceServerResource> RsTable { get; }

        event EventHandler<AgentEventArgs> EventOccurred;
        void Init(EventHandler<AgentEventArgs> OnEventOccurred, Object cfg);
        void Start();
        byte[] Frame { get; }
        void PingResponse(string ipfrom, string callid, AgentStates res, int code=200);
        void PresenceEventOcurred(string sipuriUser, bool available);
    }
}
