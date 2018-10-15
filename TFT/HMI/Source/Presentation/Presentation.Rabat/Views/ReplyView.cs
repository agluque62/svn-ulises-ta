using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;

using Microsoft.Practices.CompositeUI;
using Microsoft.Practices.CompositeUI.SmartParts;
using Microsoft.Practices.CompositeUI.EventBroker;
using Microsoft.Practices.ObjectBuilder;

using HMI.Infrastructure.Interface;
using HMI.Model.Module.Services;
using HMI.Model.Module.Messages;
using HMI.Model.Module.BusinessEntities;
using HMI.Model.Module.UI;
using HMI.Presentation.Rabat.Constants;
using HMI.Presentation.Rabat.Properties;

using Utilities;
using NLog;


namespace HMI.Presentation.Rabat.Views
{
    [SmartPart]
    public partial class ReplyView : UserControl
    {
        private static Logger _Logger = LogManager.GetCurrentClassLogger();

        private IModelCmdManagerService _CmdManager = null;
        private StateManagerService _StateManager = null;
        private bool _IsCurrentView = false;
        private long _FileLength;
        bool _SlowBlinkOn = false;
        bool _FastBlinkOn = false;
        Color _BackColor = VisualStyle.ButtonColor;
        double _ProgressBarValue = 0.0;
        double _delta;

        private string _RxRadio   
        {
            get { return Resources.RxSession; }
        }
        private string _TxRadio 
        {
            get { return Resources.TxSession; }
        }
        private string _Tlf
        {
            get { return Resources.TelfSession; }
        }
        private string _RxLc 
        {
            get { return Resources.RxLcSession; }
        }
        private string _TxLc
        {
            get { return Resources.TxLcSession; }
        }
        private string _Brf
        {
            get { return Resources.BrfSession; }
        }

        public ReplyView([ServiceDependency] IModelCmdManagerService cmdManager, [ServiceDependency] StateManagerService stateManager)
        {
            InitializeComponent();

            _CmdManager = cmdManager;
            _StateManager = stateManager;

            _BtnClose.Text = Resources.Cerrar; 
            columnHeader1.Text = Resources.DateTime;
            columnHeader2.Text = Resources.Session;
            columnHeader3.Text = Resources.Duration;
        }

        [EventSubscription(EventTopicNames.ActiveViewChanging, ThreadOption.Publisher)]
        public void OnActiveViewChanging(object sender, EventArgs<string> e)
        {
            if (e.Data == ViewNames.Replay)
            {
                _IsCurrentView = true;
                SetBtnCloseColor();
                LoadSessions();
                _CmdManager.FunctionReplay(FunctionReplay.DisableSupervisor, ViaReplay.None, null, 0);
            }
            else if (_IsCurrentView)
            {
                _IsCurrentView = false;
                _CmdManager.FunctionReplay(FunctionReplay.EnableSupervisor, ViaReplay.None, null, 0);
            }
        }

        [EventSubscription(EventTopicNames.DeleteSessionGlp, ThreadOption.Publisher)]
        public void OnDeleteSessionGlp(object sender, EventArgs e)
        {
            if (_LVSessions.SelectedIndices.Count == 1)
            {
                string fileName = (string)_LVSessions.Items[_LVSessions.SelectedIndices[0]].Tag;

                try
                {
                    File.Delete(fileName);
                    LoadSessions();
                }
                catch (DirectoryNotFoundException)
                {
                }
            }
        }

        [EventSubscription(EventTopicNames.SpeakerChanged, ThreadOption.Publisher)]
        public void OnSpeakerChanged(object sender, JacksStateMsg e)
        {
            bool altavozRd = e.LeftJack;
            bool altavozLc = e.RightJack;

            _RBAltavozLC.Enabled = altavozLc;
            _RBAltavozRadio.Enabled = altavozRd;
        }

        [EventSubscription(EventTopicNames.JacksChanged, ThreadOption.Publisher)]
        public void OnJacksChanged(object sender, EventArgs e)
        {
            Jacks jacks = _StateManager.Jacks;

            _RBJacksAlumno.Enabled = jacks.LeftJack;
            _RBJacksInstructor.Enabled = jacks.RightJack;

            if (_RBJacksInstructor.Checked && _RBJacksAlumno.Enabled && !_RBJacksInstructor.Enabled)
            {
                _RBJacksAlumno.Checked = true;
                _RBJacksInstructor.Checked = false;
            }
            if (_RBJacksAlumno.Checked && _RBJacksInstructor.Enabled && !_RBJacksAlumno.Enabled)
            {
                _RBJacksAlumno.Checked = false;
                _RBJacksInstructor.Checked = true;
            }
        }

        [EventSubscription(EventTopicNames.PlayingChanged, ThreadOption.Publisher)]
        public void OnPlayingChanged(object sender, EventArgs e)
        {
            //_BtnClose.Enabled = !_StateManager.Tft.Playing;
            _BtnErase.Enabled = !_StateManager.Tft.Playing;
            _BtnPlay.Enabled = !_StateManager.Tft.Playing;
            _BtnStop.Enabled = _StateManager.Tft.Playing;
            _timerPlaying.Enabled = _StateManager.Tft.Playing;
            _PnlViasAudio.Enabled = !_StateManager.Tft.Playing;
            if (!_StateManager.Tft.Playing)
                _PBFichero.Value = 0;
        }

        [EventSubscription(EventTopicNames.PttOnChanged, ThreadOption.Publisher)]
        public void OnPttOnChanged(object sender, EventArgs e)
        {
            CloseWindow();
        }

        [EventSubscription(EventTopicNames.LcChanged, ThreadOption.Publisher)]
        public void OnLcChanged(object sender, RangeMsg e)
        {
            for (int i = e.From, to = e.From + e.Count; i < to; i++)
            {
                LcDst dst = _StateManager.Lc[i];
                if (dst.Tx == LcTxState.Tx)
                {
                    CloseWindow();

                    break;
                }
            }
        }

        [EventSubscription(EventTopicNames.TlfChanged, ThreadOption.Publisher)]
        public void OnTlfChanged(object sender, RangeMsg e)
        {
            SetBtnCloseColor();
        }

        private void SetBtnCloseColor()
        {
            if (_StateManager.Tlf.HangTone.On)
                _BackColor = VisualStyle.Colors.Yellow;
            else
            {
                TlfState st = _StateManager.Tlf.GetTlfState(0, Tlf.NumDestinations);

                switch (st)
                {
                    case TlfState.Idle:
                    case TlfState.PaPBusy:
                        _BackColor = VisualStyle.ButtonColor;
                        break;
                    case TlfState.In:
                        _BackColor = VisualStyle.Colors.Orange;
                        _SlowBlinkTimer.Enabled = true;
                        break;
                    case TlfState.Out:
                        _BackColor = VisualStyle.Colors.Blue;
                        _SlowBlinkTimer.Enabled = false;
                        break;
                    case TlfState.Set:
                    case TlfState.Conf:
                        _BackColor = VisualStyle.Colors.Green;
                        _SlowBlinkTimer.Enabled = false;
                        break;
                    case TlfState.Busy:
                        _BackColor = VisualStyle.Colors.Red;
                        _SlowBlinkTimer.Enabled = false;
                        break;
                    case TlfState.Mem:
                        _BackColor = VisualStyle.Colors.Orange;
                        _SlowBlinkTimer.Enabled = false;
                        break;
                    case TlfState.RemoteMem:
                        _BackColor = VisualStyle.Colors.DarkGray;
                        _SlowBlinkTimer.Enabled = false;
                        break;
                    case TlfState.Hold:
                    case TlfState.RemoteHold:
                        _BackColor = VisualStyle.Colors.Green;
                        _SlowBlinkTimer.Enabled = true;
                        break;
                    case TlfState.RemoteIn:
                        _BackColor = VisualStyle.Colors.DarkGray;
                        _SlowBlinkTimer.Enabled = true;
                        break;
                    case TlfState.Congestion:
                        _BackColor = VisualStyle.Colors.Red;
                        _SlowBlinkTimer.Enabled = true;
                        break;
                    case TlfState.InPrio:
                        _BackColor = VisualStyle.Colors.Orange;
                        _FastBlinkTimer.Enabled = true;
                        break;
                    case TlfState.NotAllowed:
                        _BackColor = VisualStyle.Colors.Yellow;
                        _FastBlinkTimer.Enabled = true;
                        break;
                }
            }

            _BtnClose.ButtonColor = _BackColor;

        }

        private void CloseWindow()
        {
            if (_timerPlaying.Enabled)  // Reproduciendo
            {
                _timerPlaying.Enabled = false;

                _CmdManager.FunctionReplay(FunctionReplay.Stop, ViaReplay.None, null, 0);

                _CmdManager.SwitchTlfView(null);
            }
        }

        private void LoadSessions()
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo("Recording");

                _LVSessions.Items.Clear();
                
                FileInfo[] fi = di.GetFiles("*.*", SearchOption.AllDirectories);
                if (fi.Length == 0)
                    return;

                FileInfo lastInfo = fi[fi.Length - 1];

                foreach (System.IO.FileInfo f in fi)
                {
                    if (!f.Name.Contains("@"))
                    {
                        ListViewItem.ListViewSubItem[] subItem = new ListViewItem.ListViewSubItem[3];
                        subItem[0] = new ListViewItem.ListViewSubItem();
                        subItem[0].Text = f.LastWriteTime.ToString();
                        subItem[1] = new ListViewItem.ListViewSubItem();

                        switch (f.Name.Split('_')[0])
                        {
                            case "RxRadio":
                                subItem[1].Text = _RxRadio;
                                break;
                            case "TxRadio":
                                subItem[1].Text = _TxRadio;
                                break;
                            case "Telefonia":
                                subItem[1].Text = _Tlf;
                                break;
                            case "RxLc":
                                subItem[1].Text = _RxLc;
                                break;
                            case "TxLc":
                                subItem[1].Text = _TxLc;
                                break;
                            case "Briefing":
                                subItem[1].Text = _Brf;
                                break;
                        }
                        
                        subItem[2] = new ListViewItem.ListViewSubItem();
                        subItem[2].Text = String.Format("{0:0.00} s.", (float)f.Length /16000.0);
                        subItem[2].Tag = f.Length;

                        ListViewItem item = new ListViewItem(subItem, null);
                        item.Tag = f.DirectoryName + "/" + f.Name;

                        _LVSessions.Items.Add(item);
                    }
                }
            }
            catch (DirectoryNotFoundException )
            {
            }
        }

        private void _BtnClose_Click(object sender, EventArgs e)
        {
            try
            {
                if (_StateManager.Tft.Playing)
                {
                    _timerPlaying.Enabled = false;

                    _CmdManager.FunctionReplay(FunctionReplay.Stop, ViaReplay.None, null, 0);
                }

                _CmdManager.SwitchTlfView(null);
            }
            catch (Exception ex)
            {
                _Logger.Error("ERROR cerrando la vista Replay", ex);
            }
        }

        private void _BtnPlay_Click(object sender, EventArgs e)
        {
            if (_LVSessions.SelectedIndices.Count == 1)
            {
                string fileName = (string)_LVSessions.Items[_LVSessions.SelectedIndices[0]].Tag;
                _FileLength = (long)_LVSessions.Items[_LVSessions.SelectedIndices[0]].SubItems[2].Tag;
                _delta = (double)_PBFichero.Maximum / (10.0 * (double)_FileLength / 16000.0);
                _ProgressBarValue = 0;

                try
                {
                    if (_RBJacksAlumno.Checked)
                        _CmdManager.FunctionReplay(FunctionReplay.Play, ViaReplay.HeadphonesAlumn, fileName, _FileLength);
                    else if (_RBJacksInstructor.Checked)
                        _CmdManager.FunctionReplay(FunctionReplay.Play, ViaReplay.HeadphonesInstructor, fileName, _FileLength);
                    else if (_RBAltavozRadio.Checked)
                        _CmdManager.FunctionReplay(FunctionReplay.Play, ViaReplay.SpeakerRadio, fileName, _FileLength);
                    else
                        _CmdManager.FunctionReplay(FunctionReplay.Play, ViaReplay.SpeakerLc, fileName, _FileLength);

                    _timerPlaying.Enabled = true;
                }
                catch (Exception ex)
                {
                    _Logger.Error("ERROR reproduciendo sesion GLP", ex);
                }
            }
        }

        private void _BtnErase_Click(object sender, EventArgs e)
        {
            try
            {
                _CmdManager.FunctionReplay(FunctionReplay.Erase, ViaReplay.None, null, 0);
            }

            catch (DirectoryNotFoundException )
            {
            }
        }

        private void _BtnStop_Click(object sender, EventArgs e)
        {
            try
            {
                _CmdManager.FunctionReplay(FunctionReplay.Stop, ViaReplay.None, null, 0);
                _timerPlaying.Enabled = false;
                _PBFichero.Value = 0;
            }
            catch (Exception ex)
            {
                _Logger.Error("ERROR parando sesión GLP", ex);
            }
        }

        private void OnTimerPlaying(object sender, EventArgs e)
        {
            _PBFichero.Value = (_PBFichero.Value + (int)_delta > _PBFichero.Maximum) ? _PBFichero.Maximum : (int)Math.Round(_ProgressBarValue);
            _ProgressBarValue += _delta;
        }

        private void _SlowBlinkTimer_Tick(object sender, EventArgs e)
        {
            if (_SlowBlinkTimer.Enabled)
            {
                _SlowBlinkOn = !_SlowBlinkOn;
                _BtnClose.ButtonColor = _SlowBlinkOn ? _BackColor : VisualStyle.ButtonColor;
            }
        }

        private void _FastBlinkTimer_Tick(object sender, EventArgs e)
        {
            if (_FastBlinkTimer.Enabled)
            {
                _FastBlinkOn = !_FastBlinkOn;
                _BtnClose.ButtonColor = _FastBlinkOn ? _BackColor : VisualStyle.ButtonColor;
            }
        }
    }
}
