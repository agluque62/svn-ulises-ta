using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Practices.CompositeUI.EventBroker;
using HMI.Model.Module.Constants;
using HMI.Model.Module.Properties;
using Utilities;

namespace HMI.Model.Module.BusinessEntities
{
    public class HfSpeaker
    {
		private int _Level = 0;
        private bool _Presencia = false;

		[EventPublication(EventTopicNames.RdHfSpeakerLevelChanged, PublicationScope.Global)]
		public event EventHandler RdHfSpeakerLevelChanged;
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

					Settings.Default.RdHfSpeakerLevel = value;
					Settings.Default.Save();

					General.SafeLaunchEvent(RdHfSpeakerLevelChanged, this);
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

        public HfSpeaker()
		{
			_Level = Settings.Default.RdHfSpeakerLevel;
		}
    }
}
