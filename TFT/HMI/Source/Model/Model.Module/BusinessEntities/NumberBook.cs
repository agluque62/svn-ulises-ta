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
    [Serializable]
    public sealed class UserNumber
	{
		public string Name;
		public string R2Number;
		public string RtbNumber;
		public string Type;
		public string Role;

		public UserNumber(string name, string r2, string rtb, string type, string role)
		{
			Name = name;
			R2Number = r2;
			RtbNumber = rtb;
			Type = type;
			Role = role;
		}
	}
   
    [Serializable]
	public sealed class Depencence : SortedList<string, UserNumber>
	{
		public string Name;

		public Depencence(string name)
		{
			Name = name;
		}
	}
    
    [Serializable]
	public sealed class Fir : SortedDictionary<string, Depencence>
	{
		public string Name;

		public Fir(string name)
		{
			Name = name;
		}
	}
   
    [Serializable]
	public sealed class Area : SortedDictionary<string, Fir>
	{
		public string Name;

		public Area(string name)
		{
			Name = name;
		}
	}

	public sealed class NumberBook
	{
		private Area[] _Areas = new Area[0];

		[EventPublication(EventTopicNames.NumberBookChanged, PublicationScope.Global)]
		public event EventHandler NumberBookChanged;

		public IEnumerable<Area> Areas
		{
			get { return _Areas; }
		}

		public void Reset(RangeMsg<Area> msg)
		{
			_Areas = msg.Info;
			General.SafeLaunchEvent(NumberBookChanged, this);
		}
	}
}
