using System;
using System.Net;

using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Pipeline;

namespace HMI.CD40.Module.Snmp
{
	sealed class SnmpIntObject : ScalarObject
	{
		public int Value
		{
			get { return _data.ToInt32(); }
			set
			{
				if (Value != value)
				{
					_data = new Integer32(value);

					if ((_trapsEps != null) && (_trapsEps.Length > 0))
					{
						SnmpAgent.Trap(Variable.Id, _data, _trapsEps);
					}
				}
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

				_data = (Integer32)value;
			}
		}

		public SnmpIntObject(string oid, int value, params IPEndPoint[] trapsEps)
			: base(oid)
		{
			_data = new Integer32(value);
			_trapsEps = trapsEps;
		}

		public static SnmpIntObject Get(string oid)
		{
			return SnmpAgent.Store.GetObject(new ObjectIdentifier(oid)) as SnmpIntObject;
		}

		#region Private Members

		private Integer32 _data;
		private IPEndPoint[] _trapsEps;

		#endregion
	}
}
