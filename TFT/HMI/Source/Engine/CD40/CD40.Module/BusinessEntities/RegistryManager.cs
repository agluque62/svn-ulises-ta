#define _HF_GLOBAL_STATUS_
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text;
using HMI.CD40.Module.Properties;
using U5ki.Infrastructure;
using Utilities;
using ProtoBuf;
using NLog;

namespace HMI.CD40.Module.BusinessEntities
{
#if DEBUG
    public class TopRegistry : Registry
#else
	class TopRegistry : Registry
#endif     	
	{
		public event GenericEventHandler<Cd40Cfg> NewConfig;

		//public static string Id
		//{
		//   get { return _Registry.Id; }
		//}

		//public static string UserName
		//{
		//   set { _UserName = value; }
		//}

		public TopRegistry()
			: base("Cd40Top")
		{
		}

		public void Init()
		{
			ChannelError += OnChannelError;
			ResourceChanged += OnRsChanged;
			UserMsgReceived += OnMsgReceived;

			SubscribeToTopic<Cd40Cfg>(Identifiers.CfgTopic);
			SubscribeToTopic<TopRs>(Identifiers.TopTopic);
			SubscribeToTopic<GwTlfRs>(Identifiers.GwTopic);
			SubscribeToTopic<GwLcRs>(Identifiers.GwTopic);
			SubscribeToTopic<RdSrvRxRs>(Identifiers.RdTopic);
			SubscribeToTopic<RdSrvTxRs>(Identifiers.RdTopic);
			SubscribeToTopic<RdSrvFrRs>(Identifiers.RdTopic);
		}

		public void Start()
		{
			Join(Identifiers.CfgTopic, Identifiers.TopTopic, Identifiers.GwTopic, Identifiers.RdTopic);

			SetValue<TopRs>(Identifiers.TopTopic, Top.HostId, new TopRs());
			Publish();
		}

		public void End()
		{
			Dispose();
		}

		public Rs<T> GetRs<T>(string rsName) where T : class, new()
		{
			Resource rs;
			string type = Identifiers.TypeId(typeof(T));
			string rsUid = rsName + "_" + type;

			_Logger.Trace($"GetRs <{type}>: {rsName}");

			if (!GetResource(rsUid, out rs))
			{
				rs = new Rs<T>(rsName);
				_Resources[rsUid] = rs;

				_Logger.Trace($"GetRs <{type}>: {rsName}. New Resource...");
			}
			// LALM 210824 este else hay que comentarlo
			// Incidencia #4682
			//else 
			{
				// lalm 20201211
				// Incidencia #4682
				// si el recurso ya esta creado, hay que volver ha obtenerlo para su posterior comprobacion.
				rs = _Resources[rsUid];
				if ((rs is Rs<GwTlfRs>) || (rs is Rs<GwLcRs>))
				{
					DireccionamientoIP hostInfo = Top.Cfg.GetGwRsHostInfo(rsName);
					if ((hostInfo != null) && (hostInfo.TipoHost != Tipo_Elemento_HW.TEH_TOP) &&
                        (hostInfo.TipoHost != Tipo_Elemento_HW.TEH_TIFX) &&
                        !((hostInfo.TipoHost == Tipo_Elemento_HW.TEH_EXTERNO_TELEFONIA) && (hostInfo.Interno))
                                                        //Si es recurso externo de telefonia y es de la centralita interna (ipbx) entonces
                                                        //no se llama a Reset para que no inicialice Content y el boton
                                                        //aparezca con aspa al inicio si no está presente
                        )
					{
						rs.Reset(null, new T());   //Se inicializa sin ASPA
					}
				}
			}

			return (Rs<T>)rs;  //Se inicializa con ASPA
		}

		public void SetRx(string fr, bool rx)
		{
			_Logger.Trace($"SetRx <{fr}>: {rx}");

			FrRxChangeAsk change = new FrRxChangeAsk();
			change.HostId = Top.HostId;
			change.Frecuency = fr;
			change.Rx = rx;

			Send(Identifiers.RdMasterTopic, Identifiers.FR_RX_CHANGE_ASK_MSG, change);
		}

		public void SetTx(string fr, bool tx, uint pttType, bool checkAlreadyAssigned)
		{
			_Logger.Trace($"SetTx <{fr}>: {tx}, pttTupe {pttType}, AlreadyAssigned {checkAlreadyAssigned}");

			FrTxChangeAsk change = new FrTxChangeAsk();
			change.HostId = Top.HostId;
			change.Frecuency = fr;
			change.Tx = tx;
			change.PttType = pttType;
			change.CheckAlreadyAssigned = checkAlreadyAssigned;

			Send(Identifiers.RdMasterTopic, Identifiers.FR_TX_CHANGE_ASK_MSG, change);
		}

		public void SetPtt(PttSource src)
		{
			_Logger.Trace($"SetPtt <{src}>");

			PttChangeAsk change = new PttChangeAsk();
			change.HostId = Top.HostId;
			change.Src = src;

			Send(Identifiers.RdMasterTopic, Identifiers.PTT_CHANGE_ASK_MSG, change);
		}

		public void SetTxAssigned(string fr)
		{
			_Logger.Trace($"SetTxAssigned <{fr}>");

			FrTxAssigned notif = new FrTxAssigned();
		   notif.Frecuency = fr;
			notif.UserId = Top.Cfg.PositionId;

			Send(Identifiers.TopTopic, Identifiers.FR_TX_ASSIGNED_MSG, notif);
		}

		public void ChangeRtxGroup(int rtxGroup, IEnumerable<string> frIds, IEnumerable<RtxGroupChangeAsk.ChangeType> changes)
		{
			_Logger.Trace($"ChangeRtxGroup <{rtxGroup}>");

			RtxGroupChangeAsk rtx = new RtxGroupChangeAsk();
			rtx.HostId = Top.HostId;
			rtx.GroupId = (uint)rtxGroup;
			rtx.FrIds.AddRange(frIds);
			rtx.Changes.AddRange(changes);

			Send(Identifiers.RdMasterTopic, Identifiers.RTX_GROUP_CHANGE_ASK_MSG, rtx);
		}

        /// <summary>
        /// Preparación envío codigos SelCal
        /// </summary>
        /// 
        public void PrepareSelCal(bool onOff, string code)
        {
			_Logger.Trace($"PrepareSelCal <{onOff}>: {code}");

			SelcalPrepareMsg msg = new SelcalPrepareMsg();

            msg.HostId = Top.HostId;
            msg.OnOff = onOff;
            msg.Code = code;

            Send(Identifiers.RdMasterTopic, Identifiers.SELCAL_PREPARE, msg);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tones"></param>
        public void SendTonesSelCal(string tones)
        {
			_Logger.Trace($"SendTonesSelCall <{tones}>");

			SelcalPrepareMsg msg = new SelcalPrepareMsg();
            msg.HostId = Top.HostId;
            msg.Code = tones;

            Send(Identifiers.RdMasterTopic, Identifiers.SELCAL_SEND_TONES, msg);
        }

        public void ChangeSite(string frId, string frAlias)
        {
			_Logger.Trace($"ChangeSite <{frId}> {frAlias}");

			ChangeSiteMsg msg = new ChangeSiteMsg();

            msg.HostId = Top.HostId;
            msg.Frequency = frId;
            msg.Alias = frAlias;

            Send(Identifiers.RdMasterTopic, Identifiers.SITE_CHANGING_MSG, msg);
        }

		//LALM 221102 cambiofrecuencia
		public void SetNewFrecuency(string fr, string literal, uint pttType, bool checkAlreadyAssigned)
		{
			_Logger.Trace($"SetNewFrecuency <{fr}>: {fr}, Literal {literal}, AlreadyAssigned {checkAlreadyAssigned}");
			// Aqui habría que crear otra estructura, con protobuf.
			FrTxChangeAsk change = new FrTxChangeAsk();
			change.HostId = Top.HostId;
			change.Frecuency = fr;
			//change.Tx = tx;
			//change.PttType = pttType;
			//change.CheckAlreadyAssigned = checkAlreadyAssigned;

			Send(Identifiers.RdMasterTopic, Identifiers.FR_RXTX_CHANGE_ASK_MSG, change);
		}

		#region Private Members

		private static Logger _Logger = LogManager.GetLogger("TopRegistry");
		//private static Logger _Logger = LogManager.GetCurrentClassLogger();
		private Dictionary<string, Resource> _Resources = new Dictionary<string, Resource>();

		private T Deserialize<T>(byte[] data) where T : class
		{
			T rs = null;

			if (data != null)
			{
				MemoryStream ms = new MemoryStream(data);
				rs = Serializer.Deserialize<T>(ms);
			}

			return rs;
		}

		private T Deserialize<T>(byte[] data, int length) where T : class
		{
			T rs = null;

			if (data != null)
			{
				MemoryStream ms = new MemoryStream(data, 0, length);
				rs = Serializer.Deserialize<T>(ms);
			}

			return rs;
		}

		private void OnChannelError(object sender, string error)
		{
			_Logger.Error(error);
		}

		private void OnRsChanged(object sender, RsChangeInfo change)
		{
			try
			{
				if (change.Type == Identifiers.TypeId(typeof(Cd40Cfg)))
				{
					CfgChanged(change);
				}
				else if (change.Type == Identifiers.TypeId(typeof(TopRs)))
				{
					RsChanged<TopRs>(change);
				}
				else if (change.Type == Identifiers.TypeId(typeof(GwTlfRs)))
				{
					RsChanged<GwTlfRs>(change);
				}
				else if (change.Type == Identifiers.TypeId(typeof(GwLcRs)))
				{
					RsChanged<GwLcRs>(change);
				}
				else if (change.Type == Identifiers.TypeId(typeof(RdSrvRxRs)))
				{
					RsChanged<RdSrvRxRs>(change);
				}
				else if (change.Type == Identifiers.TypeId(typeof(RdSrvFrRs)))
				{
					RsChanged<RdSrvFrRs>(change);
				}
                else if (change.Type == Identifiers.TypeId(typeof(RdSrvTxRs)))
                {
                    TxChanged(change);
                }
				else
				{
					_Logger.Error($"OnRsChanged Error, Unkown Type <{change.Type}>");
				}
			}
			catch (Exception ex)
			{
				_Logger.Error("ERROR deserializando recurso de tipo " + change.Type, ex);
			}
		}

		private void OnMsgReceived(object sender, SpreadDataMsg msg)
		{
			try
			{
				_Logger.Trace($"OnMsgReceived <{msg.Type}>");

				if ((msg.Type == Identifiers.FR_TX_CHANGE_RESPONSE_MSG) ||
					(msg.Type == Identifiers.FR_RX_CHANGE_RESPONSE_MSG))
				{
					FrChangeResponse answer = Deserialize<FrChangeResponse>(msg.Data, msg.Length);
					string type = Identifiers.TypeId(typeof(RdSrvFrRs));
					string rsUid = answer.Frecuency.ToUpper() + "_" + type;

					_Logger.Trace($"OnMsg FR TX or RX Response <{answer.Frecuency}>: {rsUid}");
					Top.WorkingThread.Enqueue("FrChangeAnswer", delegate()
					{
						Resource resource;
						//if (_Resources.TryGetValue(rsUid, out resource))
						if (GetResource(rsUid, out resource))
						{
							resource.NotifNewMsg(msg.Type, answer.Set);
						}
						else
						{
							_Logger.Error($"OnMsg FR TX or RX Response Error. Resource not Found <{rsUid}>");
						}
					});
				}
				else if (msg.Type == Identifiers.FR_TX_ASSIGNED_MSG)
				{
					FrTxAssigned notif = Deserialize<FrTxAssigned>(msg.Data, msg.Length);
					string type = Identifiers.TypeId(typeof(RdSrvFrRs));
					string rsUid = notif.Frecuency.ToUpper() + "_" + type;

					_Logger.Trace($"OnMsg FR TX Assigned <{notif.Frecuency}>: {rsUid}");

					Top.WorkingThread.Enqueue("FrTxAssignedNotif", delegate()
					{
						Resource resource;
						//if (_Resources.TryGetValue(rsUid, out resource))
						if (GetResource(rsUid, out resource))
						{
							resource.NotifNewMsg(msg.Type, notif.UserId);
						}
						else
						{
							_Logger.Error($"OnMsg FR TX Assigned Error. Resource not Found <{rsUid}>");
						}
					});
				}
                else if (msg.Type == Identifiers.FR_HF_TX_CHANGE_RESPONSE_MSG)
                {
                    FrChangeResponse answer = Deserialize<FrChangeResponse>(msg.Data, msg.Length);
                    string type = Identifiers.TypeId(typeof(RdSrvFrRs));
                    string rsUid = answer.Frecuency.ToUpper() + "_" + type;

					_Logger.Trace($"OnMsg HF TX Change Response <{answer.Frecuency}>: {rsUid}");

                    Top.WorkingThread.Enqueue("FrHfTxChangeAnswer", delegate()
					{
                        Resource resource;
						//if (_Resources.TryGetValue(rsUid, out resource))
						if (GetResource(rsUid, out resource))
						{
							// resource.NotifNewMsg(msg.Type, answer.Set);
							resource.NotifNewMsg(msg.Type, answer.Estado);
                        }
						else
						{
							_Logger.Error($"OnMsg HF TX Change Response Error. Resource not Found <{rsUid}>");
						}
					});
                }
                else if (msg.Type == Identifiers.SELCAL_PREPARE_RSP)
                {
                    SelcalPrepareRsp resp = Deserialize<SelcalPrepareRsp>(msg.Data, msg.Length);
                    string type = Identifiers.TypeId(typeof(RdSrvFrRs));
                    string rsUid = resp.Frecuency.ToUpper() + "_" + type;

					_Logger.Trace($"OnMsg SelCal Prepare Response <{resp.Frecuency}>: {rsUid}");

                    Top.WorkingThread.Enqueue("SelCalPrepareAnswer", delegate()
					{
                        Resource resource;
						//if (_Resources.TryGetValue(rsUid, out resource))
						if (GetResource(rsUid, out resource))
						{
							resource.NotifSelCal(msg.Type, resp.Code);
                        }
						else
						{
							_Logger.Error($"OnMsg SelCal Prepare Response Error. Resource not Found <{rsUid}>");
						}
					});
                }
                else if (msg.Type == Identifiers.SITE_CHANGING_RSP)
                {
                    ChangeSiteRsp resp = Deserialize<ChangeSiteRsp>(msg.Data, msg.Length);
                    string type = Identifiers.TypeId(typeof(RdSrvFrRs));
                    string rsUid = resp.Frecuency.ToUpper() + "_" + type;

					_Logger.Trace($"OnMsg Site Change Response <{resp.Frecuency}>: {rsUid}");

                    Top.WorkingThread.Enqueue("ChangingSiteResponse", delegate()
					{
                        Resource resource;
						//if (_Resources.TryGetValue(rsUid, out resource))
						if (GetResource(rsUid, out resource))
						{
							resource.NotifSiteChanged(msg.Type, resp);
                        }
						else
						{
							_Logger.Error($"OnMsg Site Change Response Error. Resource not Found <{rsUid}>");
						}
					});
                }
#if _HF_GLOBAL_STATUS_
                else if (msg.Type == Identifiers.HF_STATUS)
                {
                    HFStatus status = Deserialize<HFStatus>(msg.Data, msg.Length);

                    string type = "_" + Identifiers.TypeId(typeof(RdSrvFrRs));

                    Top.WorkingThread.Enqueue("HFStatus", delegate()
                    {
                        foreach (KeyValuePair<string, Resource> par in _Resources)
                        {
                            if (par.Key.Contains(type))
                            {
                                par.Value.NotifNewMsg(msg.Type, status.code);
                            }
                        }
                    }
                    );
                }
				else
				{
					_Logger.Error($"OnMsgReceived Error. Unknown type <{msg.Type}>");
				}
#endif
			}
			catch (Exception ex)
			{
				_Logger.Error("ERROR deserializando mensaje de tipo " + msg.Type, ex);
			}
		}

        private void TxChanged(RsChangeInfo change)
        {
            RdSrvTxRs txRs = Deserialize<RdSrvTxRs>(change.Content);

            if (txRs != null)
            {
                Top.WorkingThread.Enqueue("NewConfig", delegate()
                {
                    //General.SafeLaunchEvent(TxStateChanged, this, txRs);
                });
            }
        }

		private void CfgChanged(RsChangeInfo change)
		{
            Cd40Cfg cfg = Deserialize<Cd40Cfg>(Tools.Decompress(change.Content));

            if (cfg != null)
			{
				_Logger.Trace($"CfgChanged <{cfg.Version}>");

				Top.WorkingThread.Enqueue("NewConfig", delegate()
				{
					foreach (Resource rs in _Resources.Values)
					{
						rs.ResetSubscribers();
						_Logger.Trace($"CfgChanged. Resource {rs.Id} => ResetSubscribers");
					}

					General.SafeLaunchEvent(NewConfig, this, cfg);

					Dictionary<string, Resource> resources = new Dictionary<string, Resource>(_Resources);
					_Resources.Clear();

					foreach (KeyValuePair<string, Resource> p in resources)
					{
						if (!p.Value.IsUnreferenced)
						{
							_Resources.Add(p.Key, p.Value);
							_Logger.Trace($"CfgChanged. Resource {p.Key} => Added..");
						}
						else
						{
							_Logger.Error($"CfgChanged. Adding Resource Error. {p.Value.Id} is Unreferenced");
						}
					}
					LogResourcesConfig();
				});
			}
		}

		//RQF-49
		public class dependencia
		{
			private string id;
			private string ip;
			private GwTlfRs.State std;

			public string Id { get => id; set => id = value; }
			public string Ip { get => ip; set => ip = value; }
			public GwTlfRs.State Std { get => std; set => std = value; }
			public void setstd(GwTlfRs.State std)
            {
				Std = std;
            }
            public dependencia(string id, string ip)
            {
				Id = id;
				Ip = ip;
				Std = GwTlfRs.State.NotAvailable;
			}
		}
		
		public class Cdependencias
        {
			public List<dependencia> dependencias;

			public dependencia find(string id, string ip)
			{
				foreach (dependencia dep in dependencias)
				{
					if (dep.Id == id && dep.Ip == ip)
						return dep;
				}
				return null;
			}

			public bool presente(string ip)
            {
				foreach (dependencia dep in dependencias)
				{
					if (dep.Ip == ip)
						if (dep.Std != GwTlfRs.State.NotAvailable)
							return true;
				}
				return false;
			}

			public Cdependencias()
			{
				dependencias = new List<dependencia>();
			}
			public void inserta(dependencia dep)
            {
				dependencias.Add(dep);
            }
		}
		public Cdependencias dependencias = new Cdependencias();

		private void RsChanged<T>(RsChangeInfo change) where T : class
		{
			T rs = Deserialize<T>(change.Content);
            string id = change.Id;

			string type = Identifiers.TypeId(typeof(T));
			string rsUid = id + "_" + type;

			_Logger.Trace($"RsChanged <{type} {rsUid}>");

			if (rs is GwTlfRs) 
            {
                object rsTlf = rs;
				//RQF-49
				//Trato eventos recursos externos.
				if (((GwTlfRs)rsTlf).Type > (uint)RsChangeInfo.RsTypeInfo.InternalAltProxy)
                {
					if ( ((((GwTlfRs)rsTlf).Type == (uint)RsChangeInfo.RsTypeInfo.ExternalProxy) || 
						 (((GwTlfRs)rsTlf).Type == (uint)RsChangeInfo.RsTypeInfo.ExternalAltProxy) ))
                    {
						String[] userData = ((GwTlfRs)rsTlf).GwIp.ToString().Split(':');
						
						string ip = userData[0];
						dependencia dep = dependencias.find(id,ip);
						GwTlfRs.State std = ((GwTlfRs)rsTlf).St;
						RsChangeInfo.RsTypeInfo tipo = (RsChangeInfo.RsTypeInfo)((GwTlfRs)rsTlf).Type;
						if (dep == null)
						{
							dep = new dependencia(id,ip);
							dependencias.inserta(dep);
						}
						if (dep.Std!=std)
							dep.setstd(std);
						_Logger.Trace($"RsChanged. Resource Changed <{id} {ip} {tipo} {std}>");
						#if TEST1
						// *********************
						// Test1 cuando este 192.168.1.112 pongo que esta 192.168.2.206(1)
						// Test2 cuando este 192.168.1.112 pongo que NO esta 192.168.2.206(2) y viceversa
						//***********************
						// si se pone una ip invalida no pasará por aqui.
						if (ip=="192.168.1.112--")
                        {
							ip = "192.168.2.206";
							dep = dependencias.find(id, ip);
							std = ((GwTlfRs)rsTlf).St;
							// (2) si se quiere estado contrario
							/*if (std == GwTlfRs.State.Idle)
								std = GwTlfRs.State.NotAvailable;
							else
								std = GwTlfRs.State.Idle;
							*/
							if (dep == null)
							{
								dep = new dependencia(id, ip);
								dependencias.inserta(dep);
							}
							if (dep.Std != std)
								dep.setstd(std);

						}
						#endif
					}
					return;
				}

				else if (((GwTlfRs)rsTlf).Type >= (uint)RsChangeInfo.RsTypeInfo.ExternalSub)
                {
                    //TODO Le quito la informacion de puerto que no me sirve, de momento, hay que cambiar CORESIP y mas
                    String[] userData = ((GwTlfRs)rsTlf).GwIp.ToString().Split(':');
                    ((GwTlfRs)rsTlf).GwIp = userData[0];
					
					_Logger.Trace($"Recibiendo Resource :{((GwTlfRs)rsTlf).Type},ip= {((GwTlfRs)rsTlf).GwIp.ToString()}, type { ((GwTlfRs)rsTlf).Type},estado{((GwTlfRs)rsTlf).St}");

					if (((GwTlfRs)rsTlf).St == GwTlfRs.State.NotAvailable)
					{
						rs = null;
					}

				}
            }

            Top.WorkingThread.Enqueue("RsChanged", delegate()
            {
                Resource resource;
				//if (!_Resources.TryGetValue(rsUid, out resource))
				if (!GetResource(rsUid, out resource))
				{
					resource = new Rs<T>(change.Id);
                    _Resources[rsUid] = resource;

					_Logger.Trace($"RsChanged. Resource Added <{type} {rsUid}>");
				}

				resource.Reset(change.ContainerId, rs);
				_Logger.Trace($"RsChanged. Resource Reset <{type} {rsUid}>");
            });
		}

		private bool GetResource(string key, out Resource rs)
		{
			var key_found = _Resources.Keys.Where(k => k.ToUpper().Equals(key.ToUpper())).FirstOrDefault();
			rs = key_found == null ? null : _Resources[key_found];
			return key_found == null ? false : true;
		}

		private void LogResourcesConfig()
		{
			_Logger.Trace("Resources List");
			foreach(var rs in _Resources)
			{
				_Logger.Trace($"{rs.Key} => {rs.Value.Id}");
			}
		}

#endregion
	}
}
