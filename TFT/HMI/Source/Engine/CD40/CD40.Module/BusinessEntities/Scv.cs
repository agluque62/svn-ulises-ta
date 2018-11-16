using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using U5ki.Infrastructure;
using Utilities;

namespace HMI.CD40.Module.BusinessEntities
{
    /// <summary>
    /// Stores static and dynamic data related with configured SCVs (mine and others)
    /// Provide methods to get information
    /// Created by CfgManager and accessed through it
    /// </summary>
    public class Scv
    {
        public enum RolScv { SCV_PRINCIPAL = 0,  SCV_ALTERNATIVO1 = 1, SCV_ALTERNATIVO2 = 2};
        public enum TipoRangoScv { OPERADOR = 0, PRIVILEGIADO = 1};
        public event GenericEventHandler<bool> ProxyStateChange;

        private class RangeTlf
        {
            public RangosSCV rango;
            public TipoRangoScv tipo;

            public RangeTlf(RangosSCV rango, TipoRangoScv tipo)
            {
                this.rango = rango;
                this.tipo = tipo;
            }
        }

        public string Id
        {
            get { return id; }
            set { id = value; }
        }
        public bool Propio
        {
            get { return propio; }
            set { propio = value; }
        }
        public bool EsCentralIp
        {
            get { return esCentralIp; }
            set { esCentralIp = value; }
        }
 
        public string GetProxyIpAddress(out string id)
        {
            id = null;
            if (!EsCentralIp) return null;
            id = Id;
            return ipProxy[(int)RolScv.SCV_PRINCIPAL].Address.ToString();
        }

        public string GetProxyIp(out string id)
        {
            id = null;
            if (!EsCentralIp) return null;
            id = Id;
            return ipProxy[(int)RolScv.SCV_PRINCIPAL].ToString();
        }

        public void SetIpServPres(RolScv index, string ipAddr)
        {
            srvPresencia[(int)index] = SipUtilities.SipEndPoint.Parse(ipAddr);
        }

        public void SetIpData(DireccionamientoIP objCfg)
        {
            esCentralIp = objCfg.EsCentralIP;

            ipProxy[(int)RolScv.SCV_PRINCIPAL] = SipUtilities.SipEndPoint.Parse(objCfg.IpRed1);
            //En el arranque este es el activo
            ipProxy[(int)RolScv.SCV_ALTERNATIVO1] = SipUtilities.SipEndPoint.Parse(objCfg.IpRed2);
            ipProxy[(int)RolScv.SCV_ALTERNATIVO2] = SipUtilities.SipEndPoint.Parse(objCfg.IpRed3);
            srvPresencia[(int)RolScv.SCV_PRINCIPAL] = SipUtilities.SipEndPoint.Parse(objCfg.SrvPresenciaIpRed1);
            srvPresencia[(int)RolScv.SCV_ALTERNATIVO1] = SipUtilities.SipEndPoint.Parse(objCfg.SrvPresenciaIpRed2);
            srvPresencia[(int)RolScv.SCV_ALTERNATIVO2] = SipUtilities.SipEndPoint.Parse(objCfg.SrvPresenciaIpRed3);
        }

        //Constructor sin rango
        //Caso posible pero no es funcional
        public Scv(DireccionamientoIP objCfg)
        {
            Id = objCfg.IdHost;
            Propio = objCfg.Interno;
            SetIpData(objCfg);
            if (Propio)
            {
                ManageProxyState();
            }

        }

        //Constructor normal, completo
        //Caso habitual
        public Scv(NumeracionATS scvAts)
        {
            id = scvAts.Central;
            Propio = scvAts.CentralPropia;
            if (Propio)
            {
                ManageProxyState();
            }
            rangos = new List<RangeTlf>();
            foreach (RangosSCV rango in scvAts.RangosOperador)
                rangos.Add(new RangeTlf(rango, TipoRangoScv.OPERADOR));
             foreach (RangosSCV rango in scvAts.RangosPrivilegiados)
                rangos.Add(new RangeTlf(rango, TipoRangoScv.PRIVILEGIADO));

        }

        public bool IsInRangeScv(ulong number)
        {
            if (rangos == null)
                return false;
            RangeTlf found = rangos.FirstOrDefault(r=> (r.rango.Inicial <= number) && (r.rango.Final >= number));
            return (found != null);
        }

        public bool IsPrivilegiado(ulong number)
        {
            if (rangos == null)
                return false;
            RangeTlf found = rangos.FirstOrDefault(r => (r.rango.Inicial <= number) && (r.rango.Final >= number) && (r.tipo == TipoRangoScv.PRIVILEGIADO));            
            return (found != null);
        }
        #region Private Members
        private const int SCV_MAX_IP_ADDR = 3;
        /// <summary>
        /// Datos procedentes de configuracion 
        /// </summary>
        private string id;
        private bool propio;

        private List<RangeTlf> rangos;
        private bool esCentralIp;
        /// <summary>
        /// Lista de IPs de proxies y servidores de presencia.
        /// El primero es el principal y los otros son alternativos.
        /// </summary>
        private IPEndPoint[] ipProxy = new IPEndPoint[SCV_MAX_IP_ADDR];
        private IPEndPoint[] srvPresencia = new IPEndPoint[SCV_MAX_IP_ADDR];
        private enum ProxyStateValue { UNKNOWN = 0,  PRESENT = 1, NOT_PRESENT = 2};
        private ProxyStateValue proxyState = ProxyStateValue.UNKNOWN;
        
        private void ManageProxyState()
        {
            Resource recursoProxy = Top.Registry.GetRs<GwTlfRs>(Id);
            recursoProxy.Changed += OnStateProxyChanged;
            //Valor inicial del recurso, forzar el refresco
            proxyState = ProxyStateValue.UNKNOWN;
            OnStateProxyChanged(recursoProxy);
        }

        //Se utiliza para señalizar el modo de funcionamiento sin proxy
        private void OnStateProxyChanged(object resource)
        {
            Top.PublisherThread.Enqueue("ProxyStateChanged", delegate()
            {
                ProxyStateValue oldProxyState = proxyState;
                proxyState = ((Resource)resource).IsValid ? ProxyStateValue.PRESENT : ProxyStateValue.NOT_PRESENT;
                //filtrar eventos redundantes
                if ((oldProxyState != proxyState))
                {
                    General.SafeLaunchEvent(ProxyStateChange, this, ((Resource)resource).IsValid);
                }
            });
        }
        #endregion
    }
}
