using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.CompositeUI.EventBroker;
using HMI.Model.Module.Constants;
using HMI.Model.Module.Messages;
using Utilities;

namespace HMI.Model.Module.BusinessEntities
{
    [Serializable]
    public class LlamadaHistorica
    {
        public string Tipo;
        public string Ultima;
        public string Fecha_Hora;
        public string Acceso;
        public string Colateral;
    }

    public class HistoricOfLocalCalls
    {
        private LlamadaHistorica[] _Calls;

        [EventPublication(EventTopicNames.HistoricalOfLocalCalls, PublicationScope.Global)]
        public event EventHandler HistoricalOfLocalCalls;

        public LlamadaHistorica[] Call
        {
            get { return _Calls; }
        }

        public void Reset(RangeMsg<LlamadaHistorica> msg)
        {
            _Calls = msg.Info;
            General.SafeLaunchEvent(HistoricalOfLocalCalls, this);
        }
    }
}
