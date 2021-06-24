using System;
using System.Net;

using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Pipeline;

namespace HMI.CD40.Module.Snmp
{
	sealed class SnmpStringObject : ScalarObject
	{
		public string Value
		{
			get { return _data.ToString(); }
			set
			{
				if (Value != value)
				{
					_data = new OctetString(value);
				}

				if ((_trapsEps != null) && (_trapsEps.Length > 0))
				{
					// SnmpAgent.Trap(Variable.Id, _data, _trapsEps);
					SnmpAgent.TrapFromTo(Properties.Settings.Default.SipIp, Variable.Id, _data, _trapsEps);
				}
			}
		}
		public void SendTrap(string value)
		{
			OctetString data = new OctetString(value);

			if ((_trapsEps != null) && (_trapsEps.Length > 0))
            {
				SnmpAgent.TrapFromTo(Properties.Settings.Default.SipIp, Variable.Id, data, _trapsEps);
				//SnmpAgent.Trap(Variable.Id, data, _trapsEps);
			}
		}

		public override ISnmpData Data
		{
			get { return _data; }
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}

				_data = (OctetString)value;
			}
		}

		public SnmpStringObject(string oid, string value, params IPEndPoint[] trapsEps)
			: base(oid)
		{
			_data = new OctetString(value);
			_trapsEps = trapsEps;
		}

		public static SnmpStringObject Get(string oid)
		{
			return SnmpAgent.Store.GetObject(new ObjectIdentifier(oid)) as SnmpStringObject;
		}

		#region Private Members

		private Lextm.SharpSnmpLib.OctetString _data;
		private IPEndPoint[] _trapsEps;

		#endregion
	}
}
