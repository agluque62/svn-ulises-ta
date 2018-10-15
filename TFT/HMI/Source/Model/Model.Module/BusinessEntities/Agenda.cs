using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Practices.CompositeUI.EventBroker;
using HMI.Model.Module.Constants;
using HMI.Model.Module.Properties;
using HMI.Model.Module.Messages;
using Utilities;

namespace HMI.Model.Module.BusinessEntities
{
	public sealed class Number
	{
		public string Digits = "";
		public string Alias = "";

		public Number()
		{
		}

		public Number(string number, string alias)
		{
			Digits = number;
			Alias = alias;
		}

		public override string ToString()
		{
			return Alias + " (" + Digits + ")";
		}
	}

	public sealed class Agenda
	{
		private Number[] _Numbers = new Number[0];

		[EventPublication(EventTopicNames.AgendaChanged, PublicationScope.Global)]
		public event EventHandler AgendaChanged;

		public IEnumerable<Number> Numbers
		{
			get { return _Numbers; }
		}

		public void Reset(RangeMsg<Number> msg)
		{
			_Numbers = msg.Info;
			General.SafeLaunchEvent(AgendaChanged, this);
		}
	}
}
