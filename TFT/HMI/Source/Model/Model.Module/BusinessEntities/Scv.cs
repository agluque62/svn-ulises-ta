using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Practices.CompositeUI.EventBroker;
using HMI.Model.Module.Constants;
using Utilities;

namespace HMI.Model.Module.BusinessEntities
{
	public sealed class Scv
	{
		private int _Active = -1;

		[EventPublication(EventTopicNames.ActiveScvChanged, PublicationScope.Global)]
		public event EventHandler ActiveScvChanged;

		public int Active
		{
			get { return _Active; }
			set 
			{ 
				if (_Active != value)
				{
					_Active = value;
					General.SafeLaunchEvent(ActiveScvChanged, this);
				}
			}
		}
	}
}
