using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Microsoft.Practices.ObjectBuilder;
using Microsoft.Practices.CompositeUI.EventBroker;
using HMI.Infrastructure.Interface;
using HMI.Model.Module.BusinessEntities;
using HMI.Model.Module.Properties;
using HMI.Model.Module.Constants;
using HMI.Model.Module.Messages;
using Utilities;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

namespace HMI.Model.Module.Services
{
	public class StateManagerService
	{
		private Tft _Tft;
		private Engine _Engine;
		private ScreenSaver _ScreenSaver;
		private Scv _Scv;
		private Jacks _Jacks;
		private Title _Title;
		private Split _Split;
		private Brightness _Brightness;
		private Buzzer _Buzzer;
		private RdSpeaker _RdSpeaker;
        private HfSpeaker _HfSpeaker;
		private RdHeadPhones _RdHeadPhones;
		private Radio _Radio;
		private Lc _Lc;
		private LcSpeaker _LcSpeaker;
		private TlfHeadPhones _TlfHeadPhones;
        private Tlf _Tlf;
		private Agenda _Agenda;
		private NumberBook _NumberBook;
		private Permissions _Permissions;
        private HistoricOfLocalCalls _HistoricalOfCalls;
        private bool _ManagingSite;
		private PlayState _PlayBt;
		private Dictionary<string, string[]> _tonosPorLlamada = new Dictionary<string, string[]>();
        
        [EventPublication(EventTopicNames.ShowNotifMsgUI, PublicationScope.Global)]
		public event EventHandler<NotifMsg> ShowNotifMsgUI;

		[EventPublication(EventTopicNames.HideNotifMsgUI, PublicationScope.Global)]
		public event EventHandler<EventArgs<string>> HideNotifMsgUI;

        [EventPublication(EventTopicNames.PermissionsChanged, PublicationScope.Global)]
        public event EventHandler PermissionsChanged;

        [EventPublication(EventTopicNames.SiteManagerEngine, PublicationScope.Global)]
        public event EventHandler SiteManagerChanged;

		[CreateNew]
		public Tft Tft
		{
			get { return _Tft; }
			set { _Tft = value; }
		}

		[CreateNew]
		public Engine Engine
		{
			get { return _Engine; }
			set { _Engine = value; }
		}

		[CreateNew]
		public ScreenSaver ScreenSaver
		{
			get { return _ScreenSaver; }
			set { _ScreenSaver = value; }
		}

		[CreateNew]
		public Scv Scv
		{
			get { return _Scv; }
			set { _Scv = value; }
		}

		[CreateNew]
		public Jacks Jacks
		{
			get { return _Jacks; }
			set { _Jacks = value; }
		}

		[CreateNew]
		public Title Title
		{
			get { return _Title; }
			set { _Title = value; }
		}

		[CreateNew]
		public Split Split
		{
			get { return _Split; }
			set { _Split = value; }
		}

		[CreateNew]
		public Brightness Brightness
		{
			get { return _Brightness; }
			set { _Brightness = value; }
		}

		[CreateNew]
		public Buzzer Buzzer
		{
			get { return _Buzzer; }
			set { _Buzzer = value; }
		}

        [CreateNew]
        public RdSpeaker RdSpeaker
        {
            get { return _RdSpeaker; }
            set { _RdSpeaker = value; }
        }

        [CreateNew]
        public HfSpeaker HfSpeaker
        {
            get { return _HfSpeaker; }
            set { _HfSpeaker = value; }
        }

        [CreateNew]
		public RdHeadPhones RdHeadPhones
		{
			get { return _RdHeadPhones; }
			set { _RdHeadPhones = value; }
		}

		[CreateNew]
		public Radio Radio
		{
			get { return _Radio; }
			set { _Radio = value; }
		}

		[CreateNew]
		public Lc Lc
		{
			get { return _Lc; }
			set { _Lc = value; }
		}

		[CreateNew]
		public LcSpeaker LcSpeaker
		{
			get { return _LcSpeaker; }
			set { _LcSpeaker = value; }
		}

		[CreateNew]
		public TlfHeadPhones TlfHeadPhones
		{
			get { return _TlfHeadPhones; }
			set { _TlfHeadPhones = value; }
		}
       
        [CreateNew]
		public Tlf Tlf
		{
			get { return _Tlf; }
			set { _Tlf = value; }
		}

		[CreateNew]
		public Agenda Agenda
		{
			get { return _Agenda; }
			set { _Agenda = value; }
		}

		[CreateNew]
		public NumberBook NumberBook
		{
			get { return _NumberBook; }
			set { _NumberBook = value; }
		}

        [CreateNew]
        public HistoricOfLocalCalls HistoricalOfCalls
        {
            get { return _HistoricalOfCalls; }
            set { _HistoricalOfCalls = value; }
        }

        [CreateNew]
        public bool ManagingSite
        {
            get { return _ManagingSite; }
            set { _ManagingSite = value; }
        }

		[CreateNew]
		public PlayState PlayBt
		{
			get { return _PlayBt; }
			set { _PlayBt = value; }
		}

		public Permissions Permissions
		{
			get { return _Permissions; }
			set
			{
				if (_Permissions != value)
				{
					_Permissions = value;
					General.SafeLaunchEvent(PermissionsChanged, this);
				}
			}
		}

        public Dictionary<string, string[]> tonosPorLlamada
        {
            get 
			{ 
				return _tonosPorLlamada; 
			}
            set { 
				_tonosPorLlamada = value; 
			}
        }

		public void SetTonosPorLlamada(string clave, string valor,string valorprio)
        {
			_tonosPorLlamada[clave] = new string[] { valor, valorprio };
        }
        public string[] GetTonosPorLlamada(string clave)
        {
            return _tonosPorLlamada[clave];
        }
        public StateManagerService()
		{
		}

		public void ShowUIMessage(NotifMsg msg)
		{
			General.SafeLaunchEvent(ShowNotifMsgUI, this, msg);
		}

		public void HideUIMessage(string id)
		{
			General.SafeLaunchEvent(HideNotifMsgUI, this, new EventArgs<string>(id));
		}
		public string[] GetTono(string tipo_llamada)
		{
			return tonosPorLlamada[tipo_llamada];
		}
	}
}
