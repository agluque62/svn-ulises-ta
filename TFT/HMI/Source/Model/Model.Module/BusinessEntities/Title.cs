using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Microsoft.Practices.CompositeUI.EventBroker;
using HMI.Model.Module.Constants;
using HMI.Model.Module.Properties;
using Utilities;
using NLog;

namespace HMI.Model.Module.BusinessEntities
{
	public sealed class Title
	{
		private static Logger _Logger = LogManager.GetCurrentClassLogger();

		private string _Id = "";
		private Image _Logo = null;

		[EventPublication(EventTopicNames.TitleIdChanged, PublicationScope.Global)]
		public event EventHandler TitleIdChanged;

		public Image Logo
		{
			get { return _Logo; }
		}

		public string Id
		{
			get { return _Id; }
			set 
			{
				if (_Id != value)
				{
					_Id = value;
					General.SafeLaunchEvent(TitleIdChanged, this);
				}
			}
		}

		public Title()
		{
			string logo = Settings.Default.Logo;

			try
			{
				_Logo = new Bitmap(logo);
			}
			catch (Exception ex)
			{
				_Logger.Warn("ERROR cargando logo (" + logo + ")", ex);
			}
		}

        public int WidthLogo
        {
            get { return Settings.Default.WidthLogo; }
        }
    
        public int HeightLogo
        {
            get { return Settings.Default.HeightLogo; }
        }
    }
}
