#define _HF_GLOBAL_STATUS_
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using HMI.CD40.Module.Properties;
using U5ki.Infrastructure;
using Utilities;
using ProtoBuf;
using NLog;

namespace HMI.CD40.Module.BusinessEntities
{
	class TopRegistry : Registry
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
			string rsUid = rsName.ToUpper() + "_" + type;

			if (!_Resources.TryGetValue(rsUid, out rs))
			{
				rs = new Rs<T>(rsName);
				_Resources[rsUid] = rs;
                
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
						rs.Reset(null, new T());
					}
				}
			}

			return (Rs<T>)rs;
		}

		public void SetRx(string fr, bool rx)
		{
			FrRxChangeAsk change = new FrRxChangeAsk();
			change.HostId = Top.HostId;
			change.Frecuency = fr;
			change.Rx = rx;

			Send(Identifiers.RdMasterTopic, Identifiers.FR_RX_CHANGE_ASK_MSG, change);
		}

		public void SetTx(string fr, bool tx, uint pttType, bool checkAlreadyAssigned)
		{
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
			PttChangeAsk change = new PttChangeAsk();
			change.HostId = Top.HostId;
			change.Src = src;

			Send(Identifiers.RdMasterTopic, Identifiers.PTT_CHANGE_ASK_MSG, change);
		}

		public void SetTxAssigned(string fr)
		{
		   FrTxAssigned notif = new FrTxAssigned();
		   notif.Frecuency = fr;
			notif.UserId = Top.Cfg.PositionId;

			Send(Identifiers.TopTopic, Identifiers.FR_TX_ASSIGNED_MSG, notif);
		}

		public void ChangeRtxGroup(int rtxGroup, IEnumerable<string> frIds, IEnumerable<RtxGroupChangeAsk.ChangeType> changes)
		{
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
            SelcalPrepareMsg msg = new SelcalPrepareMsg();
            msg.HostId = Top.HostId;
            msg.Code = tones;

            Send(Identifiers.RdMasterTopic, Identifiers.SELCAL_SEND_TONES, msg);
        }

        public void ChangeSite(string frId, string frAlias)
        {
            ChangeSiteMsg msg = new ChangeSiteMsg();

            msg.HostId = Top.HostId;
            msg.Frequency = frId;
            msg.Alias = frAlias;

            Send(Identifiers.RdMasterTopic, Identifiers.SITE_CHANGING_MSG, msg);
        }

		#region Private Members

		private static Logger _Logger = LogManager.GetCurrentClassLogger();
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
				if ((msg.Type == Identifiers.FR_TX_CHANGE_RESPONSE_MSG) ||
					(msg.Type == Identifiers.FR_RX_CHANGE_RESPONSE_MSG))
				{
					FrChangeResponse answer = Deserialize<FrChangeResponse>(msg.Data, msg.Length);
					string type = Identifiers.TypeId(typeof(RdSrvFrRs));
					string rsUid = answer.Frecuency.ToUpper() + "_" + type;

					Top.WorkingThread.Enqueue("FrChangeAnswer", delegate()
					{
						Resource resource;
						if (_Resources.TryGetValue(rsUid, out resource))
						{
							resource.NotifNewMsg(msg.Type, answer.Set);
						}
					});
				}
				else if (msg.Type == Identifiers.FR_TX_ASSIGNED_MSG)
				{
					FrTxAssigned notif = Deserialize<FrTxAssigned>(msg.Data, msg.Length);
					string type = Identifiers.TypeId(typeof(RdSrvFrRs));
					string rsUid = notif.Frecuency.ToUpper() + "_" + type;

					Top.WorkingThread.Enqueue("FrTxAssignedNotif", delegate()
					{
						Resource resource;
						if (_Resources.TryGetValue(rsUid, out resource))
						{
							resource.NotifNewMsg(msg.Type, notif.UserId);
						}
					});
				}
                else if (msg.Type == Identifiers.FR_HF_TX_CHANGE_RESPONSE_MSG)
                {
                    FrChangeResponse answer = Deserialize<FrChangeResponse>(msg.Data, msg.Length);
                    string type = Identifiers.TypeId(typeof(RdSrvFrRs));
                    string rsUid = answer.Frecuency.ToUpper() + "_" + type;

                    Top.WorkingThread.Enqueue("FrHfTxChangeAnswer", delegate()
                    {
                        Resource resource;
                        if (_Resources.TryGetValue(rsUid, out resource))
                        {
                            // resource.NotifNewMsg(msg.Type, answer.Set);
                            resource.NotifNewMsg(msg.Type, answer.Estado);
                        }
                    });
                }
                else if (msg.Type == Identifiers.SELCAL_PREPARE_RSP)
                {
                    SelcalPrepareRsp resp = Deserialize<SelcalPrepareRsp>(msg.Data, msg.Length);
                    string type = Identifiers.TypeId(typeof(RdSrvFrRs));
                    string rsUid = resp.Frecuency.ToUpper() + "_" + type;

                    Top.WorkingThread.Enqueue("SelCalPrepareAnswer", delegate()
                    {
                        Resource resource;
                        if (_Resources.TryGetValue(rsUid, out resource))
                        {
                            resource.NotifSelCal(msg.Type, resp.Code);
                        }
                    });
                }
                else if (msg.Type == Identifiers.SITE_CHANGING_RSP)
                {
                    ChangeSiteRsp resp = Deserialize<ChangeSiteRsp>(msg.Data, msg.Length);
                    string type = Identifiers.TypeId(typeof(RdSrvFrRs));
                    string rsUid = resp.Frecuency.ToUpper() + "_" + type;

                    Top.WorkingThread.Enqueue("ChangingSiteResponse", delegate()
                    {
                        Resource resource;
                        if (_Resources.TryGetValue(rsUid, out resource))
                        {
                            resource.NotifSiteChanged(msg.Type, resp);
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
			Cd40Cfg cfg = Deserialize<Cd40Cfg>(change.Content);

            if (cfg != null)
			{
				Top.WorkingThread.Enqueue("NewConfig", delegate()
				{
					foreach (Resource rs in _Resources.Values)
					{
						rs.ResetSubscribers();
					}

					General.SafeLaunchEvent(NewConfig, this, cfg);

					Dictionary<string, Resource> resources = new Dictionary<string, Resource>(_Resources);
					_Resources.Clear();

					foreach (KeyValuePair<string, Resource> p in resources)
					{
						if (!p.Value.IsUnreferenced)
						{
							_Resources.Add(p.Key, p.Value);
						}
					}
				});
			}
		}

		private void RsChanged<T>(RsChangeInfo change) where T : class
		{
			T rs = Deserialize<T>(change.Content);
            string id = change.Id;
            if (rs is GwTlfRs) 
            {
                object rsTlf = rs;
                // No trato los eventos de proxies externos
                if (((GwTlfRs)rsTlf).Type > (uint)RsChangeInfo.RsTypeInfo.InternalAltProxy)
                    return;
 
                if (((GwTlfRs)rsTlf).Type >= (uint)RsChangeInfo.RsTypeInfo.ExternalSub)
                {
                    if (((GwTlfRs)rsTlf).St == GwTlfRs.State.NotAvailable)
                        rs = null;
               }
            }
            string type = Identifiers.TypeId(typeof(T));

            string rsUid = id.ToUpper() + "_" + type;
            Top.WorkingThread.Enqueue("RsChanged", delegate()
            {
                Resource resource;
                if (!_Resources.TryGetValue(rsUid, out resource))
                {
                    resource = new Rs<T>(change.Id);
                    _Resources[rsUid] = resource;
                }

                resource.Reset(change.ContainerId, rs);
            });
        }

		#endregion
	}
}
