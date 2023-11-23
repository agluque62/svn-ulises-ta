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
        private Image _LogoMn = null;
        private Image _Jackoff = null;
        private Image _Jackon = null;
        private Image _JackoffMn = null;
        private Image _JackonMn = null;
        private Image _BrilloDownImagemas = null;
        private Image _BrilloDownImagemenos = null;
        private Image _buzzermas = null;
        private Image _buzzermenos = null;
        // LALM 210203 Nuevas imagenes
        private Image _pttencendido = null;
        private Image _squelchencendido = null;
        // LALM: 210205
        private Image _PttBlocked = null;
        private Image _RdPageDown = null;
        private Image _RdPageDownDisabled = null;
        private Image _RdPageUp = null;
        private Image _RdPageUpDisabled = null;
        private Image _RxHeadPhones = null;
        private Image _RxSpeaker = null;
        private Image _SpeakerDown = null;
        private Image _SpeakerUp = null;
        private Image _SplitLcTfMn = null;
        private Image _SplitOffMn = null;
        private Image _SplitRdLcMn = null;
        private Image _Squelch = null;
        private Image _TlfPage = null;
        private Image _Unhang = null;
        private Image _UnhangDisabled = null;
        private Image _Wait = null;
        private Image _Warning = null;
        private Image _HfSpeaker = null;
        private Image _HFSpeakerDown = null;
        private Image _HFSpeakerUp = null;
        private Image _AnularPeq = null;
        private Image _SpeakerDownTlf = null;
        private Image _SpeakerUpTlf = null;
        private Image _SpeakerTlf = null;
        private Image _HeadPhonesTlf = null;
        private Image _ENNA = null;
        private Image _group = null;

        [EventPublication(EventTopicNames.TitleIdChanged, PublicationScope.Global)]
		public event EventHandler TitleIdChanged;

		public Image Logo
		{
			get { return _Logo; }
		}
        public Image LogoMn
        {
            get { return _LogoMn; }
        }
        public Image JackOffMn
        {
            get { return _JackoffMn; }
        }
        public Image JackOnMn
        {
            get { return _JackonMn; }
        }
        public int WidthJaks
        {
            get { return Settings.Default.WidthJacks; }
        }

        public int HeightJacks
        {
            get { return Settings.Default.HeightJacks; }
        }

        public Image BrilloDownImagemas
        {
            get { return _BrilloDownImagemas; }

        }

        public Image BrilloDownImagemenos
        {
            get { return _BrilloDownImagemenos; }
        }

        public Image Buzzermas
        {
            get { return _buzzermas; }

        }

        public Image Buzzermenos
        {
            get { return _buzzermenos; }
        }

        public Image PttEncendido
        {
            get { return _pttencendido; }
        }

        public Image SquelchEncendido
        {
            get { return _squelchencendido; }
        }
        // LALM 210205
        public Image PttBlocked
        {
            get { return _PttBlocked; }
        }

        public Image RdPageDown
        {
            get { return _RdPageDown; }
        }

        public Image RdPageDownDisabled
        {
            get { return _RdPageDownDisabled; }
        }

        public Image RdPageUp
        {
            get { return _RdPageUp; }
        }

        public Image RdPageUpDisabled
        {
            get { return _RdPageUpDisabled; }
        }

        public Image RxHeadPhones
        {
            get { return _RxHeadPhones; }
        }

        public Image RxSpeaker
        {
            get { return _RxSpeaker; }
        }

        public Image SpeakerDown
        {
            get { return _SpeakerDown; }
        }

        public Image SpeakerUp
        {
            get { return _SpeakerUp; }
        }

        public Image SplitLcTfMn
        {
            get { return _SplitLcTfMn; }
        }

        public Image SplitOffMn
        {
            get { return _SplitOffMn; }
        }

        public Image SplitRdLcMn
        {
            get { return _SplitRdLcMn; }
        }

        public Image Squelch
        {
            get { return _Squelch; }
        }

        public Image TlfPage
        {
            get { return _TlfPage; }
        }

        public Image Unhang
        {
            get { return _Unhang; }
        }

        public Image UnhangDisabled
        {
            get { return _UnhangDisabled; }
        }

        public Image Wait
        {
            get { return _Wait; }
        }

        public Image Warning
        {
            get { return _Warning; }
        }

        public Image HfSpeaker
        {
            get { return _HfSpeaker; }
        }

        public Image HFSpeakerDown
        {
            get { return _HFSpeakerDown; }
        }

        public Image HFSpeakerUp
        {
            get { return _HFSpeakerUp; }
        }

        public Image AnularPeq
        {
            get { return _AnularPeq; }
        }

        public Image SpeakerDownTlf
        {
            get { return _SpeakerDownTlf; }
        }

        public Image SpeakerUpTlf
        {
            get { return _SpeakerUpTlf; }
        }

        public Image SpeakerTlf
        {
            get { return _SpeakerTlf; }
        }

        public Image HeadPhonesTlf
        {
            get { return _HeadPhonesTlf; }
        }

        public Image ENNA
        {
            get { return _ENNA; }
        }

        public Image group
        {
            get { return _group; }
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

            //230807 todo esto es para el modo noctuno
            string logomMn = Settings.Default.LogoMn;
            try
            {
                _LogoMn = new Bitmap(logomMn);
            }
            catch (Exception ex)
            {
                _Logger.Warn("ERROR cargando logo (" + logomMn + ")", ex);
            }

            // Genero aqui jacks
            // LALM 210215 pongo jackoffmn
            string jackoffMn = Settings.Default.JackoffMn;
            try
            {
                _JackoffMn = new Bitmap(jackoffMn);
            }
            catch (Exception ex)
            {
                _Logger.Warn("ERROR cargando logo (" + jackoffMn + ")", ex);
            }

            // LALM 210215 pongo jackonmn
            string jackonMn = Settings.Default.JackonMn;
            try
            {
                _JackonMn = new Bitmap(jackonMn);
            }
            catch (Exception ex)
            {
                _Logger.Warn("ERROR cargando logo (" + jackonMn + ")", ex);
            }

            // brillo
            string brilloupmas = Settings.Default.Brillomas;
            try { _BrilloDownImagemas = new Bitmap(brilloupmas); }
            catch (Exception ex) { _Logger.Warn("ERROR cargando logo (" + brilloupmas + ")", ex); }

            string brilloupmenos = Settings.Default.Brillomenos;
            try { _BrilloDownImagemenos = new Bitmap(brilloupmenos); }
            catch (Exception ex) { _Logger.Warn("ERROR cargando logo (" + brilloupmenos + ")", ex); }


            // LALM 210203 cargo bitmap Buzzer 
            string Buzzermas = Settings.Default.BuzzerUp;
            try { _buzzermas = new Bitmap(Buzzermas); }
            catch (Exception ex) { _Logger.Warn("ERROR cargando logo (" + Buzzermas + ")", ex); }

            string Buzzermenos = Settings.Default.Buzzerdown;
            try { _buzzermenos = new Bitmap(Buzzermenos); }
            catch (Exception ex) { _Logger.Warn("ERROR cargando logo (" + Buzzermenos + ")", ex); }

            string Pttencendido = Settings.Default.PTTEncendido;
            try { _pttencendido = new Bitmap(Pttencendido); }
            catch (Exception ex) { _Logger.Warn("ERROR cargando logo (" + Pttencendido + ")", ex); }

            string Squechencendido = Settings.Default.SQUEncendido;
            try { _squelchencendido = new Bitmap(Squechencendido); }
            catch (Exception ex) { _Logger.Warn("ERROR cargando logo (" + Squechencendido + ")", ex); }

            //LALM: 210205
            string PttBlockencendido = Settings.Default.PttBlockedEncendido;
            try { _PttBlocked = new Bitmap(PttBlockencendido); }
            catch (Exception ex) { _Logger.Warn("ERROR cargando logo (" + PttBlocked + ")", ex); }

            string RdPageDownencendido = Settings.Default.RdPageDownEncendido;
            try { _RdPageDown = new Bitmap(RdPageDownencendido); }
            catch (Exception ex) { _Logger.Warn("ERROR cargando logo (" + RdPageDown + ")", ex); }

            // LALM 210205 de momento lo cargo sobre el nombre del bitmap
            // sin buscarlos en registro.
            string RdPageDownDisabledencendido = "Resources\\RdPageDownDisabled.gif";// Settings.Default.RdPageDownDisabledEncendido;
            try { _RdPageDownDisabled = new Bitmap(RdPageDownDisabledencendido); }
            catch (Exception ex) { _Logger.Warn("ERROR cargando logo (" + RdPageDownDisabledencendido + ")", ex); }

            //lo cargo por su nombre sin buscar en registro
            string RdPageUpencendido = "Resources\\RdPageUp.gif"; //Settings.Default.RdPageUpencendido;
            try { _RdPageUp = new Bitmap(RdPageUpencendido); }
            catch (Exception ex) { _Logger.Warn("ERROR cargando logo (" + RdPageUpencendido + ")", ex); }

            //lo cargo por su nombre sin buscar en registro
            string RdPageUpDisabledencendido = "Resources\\RdPageUpDisabled.gif"; //Settings.Default.RdPageUpDisabledencendido;
            try { _RdPageUpDisabled = new Bitmap(RdPageUpDisabledencendido); }
            catch (Exception ex) { _Logger.Warn("ERROR cargando logo (" + RdPageUpDisabledencendido + ")", ex); }

            //lo cargo por su nombre sin buscar en registro
            string RxHeadPhonesencendido = "Resources\\RxHeadPhones.gif"; //Settings.Default.RxHeadPhonesencendido;
            try { _RxHeadPhones = new Bitmap(RxHeadPhonesencendido); }
            catch (Exception ex) { _Logger.Warn("ERROR cargando logo (" + RxHeadPhonesencendido + ")", ex); }

            //lo cargo por su nombre sin buscar en registro
            string RxSpeakerencendido = "Resources\\RxSpeaker.gif"; //Settings.Default.RxSpeakerencendido;
            try { _RxSpeaker = new Bitmap(RxSpeakerencendido); }
            catch (Exception ex) { _Logger.Warn("ERROR cargando logo (" + RxSpeakerencendido + ")", ex); }

            //lo cargo por su nombre sin buscar en registro
            string SpeakerDownencendido = "Resources\\SpeakerDown.gif"; //Settings.Default.SpeakerDownencendido;
            try { _SpeakerDown = new Bitmap(SpeakerDownencendido); }
            catch (Exception ex) { _Logger.Warn("ERROR cargando logo (" + SpeakerDownencendido + ")", ex); }

            //lo cargo por su nombre sin buscar en registro
            string SpeakerUpencendido = "Resources\\SpeakerUp.gif"; //Settings.Default.SpeakerUpencendido;
            try { _SpeakerUp = new Bitmap(SpeakerUpencendido); }
            catch (Exception ex) { _Logger.Warn("ERROR cargando logo (" + SpeakerUpencendido + ")", ex); }

            //lo cargo por su nombre sin buscar en registro
            string SplitLcTfencendido = "Resources\\SplitLcTfNocturno.gif"; //Settings.Default.SplitLcTfencendido;
            try { _SplitLcTfMn = new Bitmap(SplitLcTfencendido); }
            catch (Exception ex) { _Logger.Warn("ERROR cargando logo (" + SplitLcTfencendido + ")", ex); }

            //lo cargo por su nombre sin buscar en registro

            string SplitOffencendido = "Resources\\SplitOffNocturno.gif"; //Settings.Default.SplitOffencendido;
            try { _SplitOffMn = new Bitmap(SplitOffencendido); }
            catch (Exception ex) { _Logger.Warn("ERROR cargando logo (" + SplitOffencendido + ")", ex); }

            //lo cargo por su nombre sin buscar en registro
            string SplitRdLcencendido = "Resources\\SplitRdLcNocturno.gif"; //Settings.Default.SplitRdLcencendido;
            try { _SplitRdLcMn = new Bitmap(SplitRdLcencendido); }
            catch (Exception ex) { _Logger.Warn("ERROR cargando logo (" + SplitRdLcencendido + ")", ex); }

            //lo cargo por su nombre sin buscar en registro
            string Squelchencendido = "Resources\\Squelch.gif"; //Settings.Default.Squelchencendido;
            try { _Squelch = new Bitmap(Squelchencendido); }
            catch (Exception ex) { _Logger.Warn("ERROR cargando logo (" + Squelchencendido + ")", ex); }

            //lo cargo por su nombre sin buscar en registro
            string TlfPageencendido = "Resources\\TlfPage.gif"; //Settings.Default.TlfPageencendido;
            try { _TlfPage = new Bitmap(TlfPageencendido); }
            catch (Exception ex) { _Logger.Warn("ERROR cargando logo (" + TlfPageencendido + ")", ex); }

            //lo cargo por su nombre sin buscar en registro
            string Unhangencendido = "Resources\\Unhang.gif"; //Settings.Default.Unhangencendido;
            try { _Unhang = new Bitmap(Unhangencendido); }
            catch (Exception ex) { _Logger.Warn("ERROR cargando logo (" + Unhangencendido + ")", ex); }

            //lo cargo por su nombre sin buscar en registro
            string UnhangDisabledencendido = "Resources\\UnhangDisabled.gif"; //Settings.Default.UnhangDisabledencendido;
            try { _UnhangDisabled = new Bitmap(UnhangDisabledencendido); }
            catch (Exception ex) { _Logger.Warn("ERROR cargando logo (" + UnhangDisabledencendido + ")", ex); }

            //lo cargo por su nombre sin buscar en registro
            string Waitencendido = "Resources\\Wait.png"; //Settings.Default.Waitencendido;
            try { _Wait = new Bitmap(Waitencendido); }
            catch (Exception ex) { _Logger.Warn("ERROR cargando logo (" + Waitencendido + ")", ex); }

            //lo cargo por su nombre sin buscar en registro
            string Warningencendido = "Resources\\Warning.png"; //Settings.Default.Warningencendido;
            try { _Warning = new Bitmap(Warningencendido); }
            catch (Exception ex) { _Logger.Warn("ERROR cargando logo (" + Warningencendido + ")", ex); }

            //lo cargo por su nombre sin buscar en registro
            string HfSpeakerencendido = "Resources\\HfSpeaker.gif"; //Settings.Default.HfSpeakerencendido;
            try { _HfSpeaker = new Bitmap(HfSpeakerencendido); }
            catch (Exception ex) { _Logger.Warn("ERROR cargando logo (" + HfSpeakerencendido + ")", ex); }

            //lo cargo por su nombre sin buscar en registro
            string HFSpeakerDownencendido = "Resources\\HFSpeakerDown.gif"; //Settings.Default.HFSpeakerDownencendido;
            try { _HFSpeakerDown = new Bitmap(HFSpeakerDownencendido); }
            catch (Exception ex) { _Logger.Warn("ERROR cargando logo (" + HFSpeakerDownencendido + ")", ex); }

            //lo cargo por su nombre sin buscar en registro
            string HFSpeakerUpencendido = "Resources\\HFSpeakerUp.gif"; //Settings.Default.HFSpeakerUpencendido;
            try { _HFSpeakerUp = new Bitmap(HFSpeakerUpencendido); }
            catch (Exception ex) { _Logger.Warn("ERROR cargando logo (" + HFSpeakerUpencendido + ")", ex); }

            //lo cargo por su nombre sin buscar en registro
            string AnularPeqencendido = "Resources\\AnularPeq.gif"; //Settings.Default.AnularPeqencendido;
            try { _AnularPeq = new Bitmap(AnularPeqencendido); }
            catch (Exception ex) { _Logger.Warn("ERROR cargando logo (" + AnularPeqencendido + ")", ex); }

            //lo cargo por su nombre sin buscar en registro
            string SpeakerDownTlfencendido = "Resources\\SpeakerDownTlf.gif"; //Settings.Default.SpeakerDownTlfencendido;
            try { _SpeakerDownTlf = new Bitmap(SpeakerDownTlfencendido); }
            catch (Exception ex) { _Logger.Warn("ERROR cargando logo (" + SpeakerDownTlfencendido + ")", ex); }

            //lo cargo por su nombre sin buscar en registro
            string SpeakerUpTlfencendido = "Resources\\SpeakerUpTlf.gif"; //Settings.Default.SpeakerUpTlfencendido;
            try { _SpeakerUpTlf = new Bitmap(SpeakerUpTlfencendido); }
            catch (Exception ex) { _Logger.Warn("ERROR cargando logo (" + SpeakerUpTlfencendido + ")", ex); }

            //lo cargo por su nombre sin buscar en registro
            string SpeakerTlfencendido = "Resources\\SpeakerTlf.gif"; //Settings.Default.SpeakerTlfencendido;
            try { _SpeakerTlf = new Bitmap(SpeakerTlfencendido); }
            catch (Exception ex) { _Logger.Warn("ERROR cargando logo (" + SpeakerTlfencendido + ")", ex); }

            //lo cargo por su nombre sin buscar en registro
            string HeadPhonesTlfencendido = "Resources\\HeadPhonesTlf.gif"; //Settings.Default.HeadPhonesTlfencendido;
            try { _HeadPhonesTlf = new Bitmap(HeadPhonesTlfencendido); }
            catch (Exception ex) { _Logger.Warn("ERROR cargando logo (" + HeadPhonesTlfencendido + ")", ex); }

            //lo cargo por su nombre sin buscar en registro
            string ENNAencendido = "Resources\\ENNA.jpg"; //Settings.Default.ENNAencendido;
            try { _ENNA = new Bitmap(ENNAencendido); }
            catch (Exception ex) { _Logger.Warn("ERROR cargando logo (" + ENNAencendido + ")", ex); }


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
