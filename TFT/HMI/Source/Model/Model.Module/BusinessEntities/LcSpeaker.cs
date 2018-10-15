using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Practices.CompositeUI.EventBroker;
using HMI.Model.Module.Constants;
using HMI.Model.Module.Properties;
using Utilities;

namespace HMI.Model.Module.BusinessEntities
{
	public sealed class LcSpeaker
	{
        // La LC y la Telfonía van por este altavoz. MAnejan volúmenes diferentes.
		private int _LevelLC = 0;
        private int _LevelTlf = 0;
        private bool _Presencia = false;

		[EventPublication(EventTopicNames.LcSpeakerLevelChanged, PublicationScope.Global)]
		public event EventHandler LcSpeakerLevelChanged;
        [EventPublication(EventTopicNames.SpeakerChanged, PublicationScope.Global)]
        public event EventHandler SpeakerChanged;
        [EventPublication(EventTopicNames.TlfSpeakerLevelChanged, PublicationScope.Global)]
        public event EventHandler TlfSpeakerLevelChanged;

		public int LevelLC
		{
			get { return _LevelLC; }
			set 
			{ 
				if (_LevelLC != value)
				{
					_LevelLC = value;

					Settings.Default.LcSpeakerLevel = value;
					Settings.Default.Save();

					General.SafeLaunchEvent(LcSpeakerLevelChanged, this);
				}
			}
		}

        public int LevelTlf
        {
            get { return _LevelTlf; }
            set
            {
                if (_LevelTlf != value)
                {
                    _LevelTlf = value;

                    Settings.Default.TlfSpeakerLevel = value;
                    Settings.Default.Save();

                    General.SafeLaunchEvent(TlfSpeakerLevelChanged, this);
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

		public LcSpeaker()
		{
			_LevelLC = Settings.Default.LcSpeakerLevel;
            _LevelTlf = Settings.Default.TlfSpeakerLevel;
        }
	}
}
