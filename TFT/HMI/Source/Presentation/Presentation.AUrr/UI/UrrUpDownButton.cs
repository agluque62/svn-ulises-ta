using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Utilities;

namespace HMI.Presentation.AUrr.UI
{
    public partial class UrrUpDownButton : UserControl
    {
        private EventHandler _LongClickHander;

        public event EventHandler LevelDown;
        public event EventHandler LevelUp;

        /*public event EventHandler LongClick
        {
            add
            {
                //_LongClickHander += value;
                //_DownBT.LongClick += _BT_LongClick;
                //_UpBT.LongClick += _BT_LongClick;
            }
            remove
            {
                _LongClickHander -= value;
                if (_LongClickHander == null)
                {
                    //_DownBT.LongClick -= _BT_LongClick;
                    //_UpBT.LongClick -= _BT_LongClick;
                }
            }
        }
        */
        [CategoryAttribute("Appearance"),
        Description("Image of down button"),
        RefreshProperties(RefreshProperties.Repaint),
        DefaultValue(null)
        ]
        public Image DownImage
        {
            get { return _UrrMidBT.ImageNormal; }
            set { _UrrMidBT.ImageNormal = value; }
        }

        /*[CategoryAttribute("Appearance"),
        Description("Image of up button"),
        RefreshProperties(RefreshProperties.Repaint),
        DefaultValue(null)
        ]
        /*public Image UpImage
        {
            get { return _UpBT.ImageNormal; }
            set { _UpBT.ImageNormal = value; }
        }
        */
        [Browsable(false),
        DefaultValue(0)
        ]
        public int Level
        {
            get { return _UrrLevelBar.Value; }
            set
            {
                if (value == 0)
                    value = 1;
                _UrrLevelBar.Value = value;
                _UrrDownBT.Enabled = _UrrLevelBar.Value > 1;
                _UrrUpBT.Enabled = _UrrLevelBar.Value < 7;
            }
        }

        public void ChangeColor(Color c)
        {
            _UrrMidBT.ButtonColorDisabled = c;

            //No hace disabled --> _UrrUpBT.ButtonColorDisabled = c;
            /*_UrrDownBT.StartColor = c;
            _UrrDownBT.EndColor = c;
            _UrrUpBT.StartColor = c;
            _UrrUpBT.EndColor = c;*/
            //-->Esto no chuta
        }

        /*[Browsable(false),
        DefaultValue(false)
        ]
        /*public bool DrawX
        {
            get { return _DownBT.DrawX; }
            set { _DownBT.DrawX = _UpBT.DrawX = value; }
        }
        */
        public UrrUpDownButton()
        {
            InitializeComponent();
        }

        private void _DownBT_Click(object sender, EventArgs e)
        {
            General.SafeLaunchEvent(LevelDown, this);
        }

        /*private void _UpBT_Click(object sender, EventArgs e)
        {
            General.SafeLaunchEvent(LevelUp, this);
        }*/

        private void _BT_LongClick(object sender, EventArgs e)
        {
            General.SafeLaunchEvent(_LongClickHander, this);
        }

        private void decreaseButton1_Click(object sender, EventArgs e)
        {
            //int level = _RdSpeakerUDB.Level - 1;
            if (this._UrrLevelBar.cursorLevel > 0)
            {
                this._UrrLevelBar.cursorLevel -= 1;
                _UrrLevelBar.Refresh();
            }
            /*try
            {
                _CmdManager.RdSetSpeakerLevel(level);
            }
            catch (Exception ex)
            {
                _Logger.Error("ERROR bajando el nivel del altavoz RD a " + level, ex);
            }*/
        }

        //Habilitar y Deshabilitar botones cuando se llega a los valores frontera 1<>7
        private void _UpButton_Click(object sender, EventArgs e)
        {
            General.SafeLaunchEvent(LevelUp, this);

            if (this._UrrLevelBar.actualValue < 7)
            {
                this._UrrLevelBar.actualValue += 1;
                _UrrLevelBar.Refresh();
            }

            if (this._UrrLevelBar.actualValue == 7)
                this._UrrUpBT.Enabled = false;
            this._UrrDownBT.Enabled = true;
        }
        private void _DownButton_Click(object sender, EventArgs e)
        {
            General.SafeLaunchEvent(LevelDown, this);

            if (this._UrrLevelBar.actualValue > 1)
            {
                this._UrrLevelBar.actualValue -= 1;
                _UrrLevelBar.Refresh();
            }

            if (this._UrrLevelBar.actualValue == 1)
                this._UrrDownBT.Enabled = false;
            this._UrrUpBT.Enabled = true;
        }
    }
}
