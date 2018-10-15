using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Practices.CompositeUI.EventBroker;
using HMI.Model.Module.Constants;
using HMI.Model.Module.Properties;
using Utilities;

namespace HMI.Model.Module.BusinessEntities
{
	public sealed class Buzzer
	{
		private int _Level = 0;
		private bool _Enabled = true;

		[EventPublication(EventTopicNames.BuzzerLevelChanged, PublicationScope.Global)]
		public event EventHandler BuzzerLevelChanged;

		[EventPublication(EventTopicNames.BuzzerStateChanged, PublicationScope.Global)]
		public event EventHandler BuzzerStateChanged;

		public int Level
		{
			get { return _Level; }
			set 
			{ 
				if (_Level != value)
				{
					_Level = value;

					Settings.Default.BuzzerLevel = value;
					Settings.Default.Save();

					General.SafeLaunchEvent(BuzzerLevelChanged, this);
				}
			}
		}

		public bool Enabled
		{
			get { return _Enabled; }
			set 
			{
				if (_Enabled != value)
				{
					_Enabled = value;

                    //Settings.Default.BuzzerEnabled = value;
                    //Settings.Default.Save();

					General.SafeLaunchEvent(BuzzerStateChanged, this);
				}
			}
		}

		public Buzzer()
		{
			_Enabled = Settings.Default.BuzzerEnabled;
			_Level = Settings.Default.BuzzerLevel;
		}
	}
}
