using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Practices.CompositeUI;
using Microsoft.Practices.CompositeUI.SmartParts;
using Microsoft.Practices.CompositeUI.EventBroker;
using HMI.Model.Module.UI;
using HMI.Model.Module.Services;
using HMI.Model.Module.BusinessEntities;
using HMI.Model.Module.Messages;
using HMI.Presentation.Asecna.Constants;
using HMI.Presentation.Asecna.Properties;
using NLog;

namespace HMI.Presentation.Asecna.Views
{
    [SmartPart]
    public partial class HfView : UserControl
    {
        private const int SELCAL_ERROR_INVALID_COMBINATION = 1;
        private const int SELCAL_ERROR_REPEATED_CODES = 2;

        private short Memory = 0;
        private bool PanelVisible = false;
        private bool Reset = true;

        private IModelCmdManagerService _CmdManager = null;
        private StateManagerService _StateManager = null;

        private static Logger _Logger = LogManager.GetCurrentClassLogger();

        private string _Enviar
        {
            get { return Resources.Enviar; }
        }

        public HfView([ServiceDependency] IModelCmdManagerService cmdManager, [ServiceDependency] StateManagerService stateManager)
        {
            InitializeComponent();

            _CmdManager = cmdManager;
            _StateManager = stateManager;

            _SelCalButton.Enabled = _SelCalEnabled;
            tableLayoutPanel2.Visible = _SelCalButton.Enabled;

            _BtEnviar.Text = _Enviar;
            _SelCalButton.Text = Resources.SelCall;
            _BtMem1.Text = Resources.SelCall1;
            _BtMem2.Text = Resources.SelCall2;
            _BtMem3.Text = Resources.SelCall3;
        }

        /// <summary>
        /// 
        /// </summary>
        private bool _SelCalEnabled
        {
            get
            {
                return _StateManager.Tft.Enabled && _StateManager.Engine.Operative &&
                    !_StateManager.Radio.PttOn &&
                    (_StateManager.Radio.EnableSelCal());
            }
        }

        /// <summary>
        /// Comprueba cuando está habilitada la capacidad de borrar texto del display del código SelCal
        /// </summary>
        private bool _EraseCodeEnabled
        {
            get
            {
                return _TBDisplayCode.Text.Length > 0;
            }
        }

        /// <summary>
        /// Comprueba cuando está habilitada la capacidad de introducir códigos SelCal
        /// </summary>
        private bool _CodePanelEnabled
        {
            get
            {
                return Reset || _TBDisplayCode.Text.Length < 4;
            }
        }

        /// <summary>
        /// Comprueba cuando está habilitada la capacidad de enviar el código SelCal introducido
        /// </summary>
        private bool _SendCodeEnabled
        {
            get
            {
                return _TBDisplayCode.Text.Length == 4;
            }
        }

        [EventSubscription(EventTopicNames.SelCalResponse, ThreadOption.Publisher)]
        public void OnSelCalResponse(object sender, StateMsg<string> msg)
        {
            if (msg.State != string.Empty)
            {
                _TBDisplayMessages.Text = (msg.State != "Error") ? Resources.SendingTonesSelCal : Resources.ErrorSelCal;

                switch (Memory++ % 3)
                {
                    case 0:
                        _BtMem1.Text = _TBDisplayCode.Text;
                        _BtMem1.Enabled = true;
                        break;
                    case 1:
                        _BtMem2.Text = _TBDisplayCode.Text;
                        _BtMem2.Enabled = true;
                        break;
                    case 2:
                        _BtMem3.Text = _TBDisplayCode.Text;
                        _BtMem3.Enabled = true;
                        break;
                }

                Reset = true;

                tableLayoutPanel2.Enabled = _CodePanelEnabled;

                // Prepara restauracion equipo SELCAL
                try
                {
                    if (msg.State != "Error")
                        _CmdManager.RdPrepareSelCal(false, "");
                }
                catch (Exception ex)
                {
                    _TBDisplayMessages.Text = Resources.ErrorSelCal;
                    _Logger.Error("ERROR envío comando SelCal", ex);
                    return;
                }

                _TBDisplayMessages.Text = (msg.State != "Error") ? msg.State : Resources.ErrorSelCal;
            }
        }


        [EventSubscription(EventTopicNames.RadioChanged, ThreadOption.Publisher)]
        public void OnRadioChanged(object sender, RangeMsg e)
        {
            _SelCalButton.Enabled = _SelCalEnabled;

            if (PanelVisible && !_SelCalEnabled)
                ShowHfPanel();
        }

        private void _SelCalButton_Click(object sender, EventArgs e)
        {
            ShowHfPanel();
        }

        private bool CheckCode(string code, ref int errorCode)
        {
            int []inpt = {0,0,0,0};

            /* check the input to be in [A..H, J..M, P..S] and translate to [0..15] */
            for (int i = 0; i <= 3; i++)
            {
                int j = code[i] - 65;
                if ((j < 0) || (j > 18) || (j == 8) || (j == 13) || (j == 14))
                {
                    return false;
                }
                if (j > 12) j = j - 2;  /* N and O left out */
                if (j > 7) j = j - 1;  /* I left out */
                inpt[i] = j;
            }
            /* ignore invalid selcal combinations like BA.. */
            for (int i = 1; i <= 3; i = i + 2)
            {
                if (inpt[i] <= inpt[i - 1])
                {
                    errorCode = SELCAL_ERROR_INVALID_COMBINATION;
                    return false;
                }
            }
            /* ignore invalid selcal combinations like ABAB */
            if ((inpt[0] == inpt[2]) && (inpt[1] == inpt[3]))
            {
                errorCode = SELCAL_ERROR_REPEATED_CODES;
                return false;
            }

            return true;
        }

        private void ShowHfPanel()
        {
            PanelVisible = !PanelVisible;

            tableLayoutPanel2.Visible = PanelVisible;
            _BtMem1.Visible = _BtMem2.Visible = _BtMem3.Visible = PanelVisible;
            _TBDisplayCode.Visible = _TBDisplayMessages.Visible = PanelVisible;
            _BtEnviar.Visible = _BtBorrar.Visible = PanelVisible;
        }

        private void _BtMem_Click(object sender, EventArgs e)
        {
            _TBDisplayCode.Text = ((HMIButton)sender).Text;
        }

        private void _BtAnyCode_Click(object sender, EventArgs e)
        {
            HMIButton btCode = (HMIButton)sender;

            if (Reset)
            {
                Reset = false;
                _TBDisplayCode.Text = string.Empty;
            }

            _TBDisplayCode.Text += btCode.Text;
        }

        private void _BtBorrar_Click(object sender, EventArgs e)
        {
            _TBDisplayCode.Text = _TBDisplayCode.Text.Substring(0, _TBDisplayCode.Text.Length - 1);

            _TBDisplayMessages.Text = string.Empty;
            Reset = false;
        }

        private void _TBDisplayCode_TextChanged(object sender, EventArgs e)
        {
            _BtBorrar.Enabled = _EraseCodeEnabled;
            tableLayoutPanel2.Enabled = _CodePanelEnabled;
            _BtEnviar.Enabled = _SendCodeEnabled;

            _TBDisplayMessages.Text = string.Empty;
        }

        private void _BtEnviar_Click(object sender, EventArgs e)
        {
            int errorCode = 0;

            _TBDisplayMessages.Text = Resources.SendingSelCal;

            if (!CheckCode(_TBDisplayCode.Text, ref errorCode))
            {
                switch (errorCode)
                {
                    case SELCAL_ERROR_INVALID_COMBINATION:
                        _TBDisplayMessages.Text = Resources.SelCalMessageInvalidCombination;
                        break;
                    case SELCAL_ERROR_REPEATED_CODES:
                        _TBDisplayMessages.Text = Resources.SelCalMessageRepeatedCodes;
                        break;
                }

                Reset = true;

                tableLayoutPanel2.Enabled = _CodePanelEnabled;
            }
            else
            {
                // Prepara envío SELCAL
                try
                {
                    _CmdManager.RdPrepareSelCal(true, _TBDisplayCode.Text);
                }
                catch (Exception ex)
                {
                    _Logger.Error("ERROR envío comando SelCal", ex);
                }
            }
        }
    }
}
