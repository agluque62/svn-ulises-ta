using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Practices.CompositeUI.EventBroker;
using HMI.Model.Module.Constants;
using HMI.Model.Module.Properties;
using Utilities;

namespace HMI.Model.Module.BusinessEntities
{
	public sealed class RdSpeaker
	{
		private int _Level = 0;
        private bool _Presencia = false;

		[EventPublication(EventTopicNames.RdSpeakerLevelChanged, PublicationScope.Global)]
		public event EventHandler RdSpeakerLevelChanged;
        [EventPublication(EventTopicNames.SpeakerChanged, PublicationScope.Global)]
        public event EventHandler SpeakerChanged;

		public int Level
		{
			get { return _Level; }
			set 
			{ 
				if (_Level != value)
				{
					_Level = value;

					Settings.Default.RdSpeakerLevel = value;
					Settings.Default.Save();

					General.SafeLaunchEvent(RdSpeakerLevelChanged, this);
				}
			}
		}
        public bool Presencia
        {
            get { return _Presencia; }
            set
            {
                if (_Presencia != value)
                {
                    _Presencia = value;

                    General.SafeLaunchEvent(SpeakerChanged, this);
                }
            }
        }

		public RdSpeaker()
		{
			_Level = Settings.Default.RdSpeakerLevel;
		}
	}
}
