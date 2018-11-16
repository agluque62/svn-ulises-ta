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
    public partial class UpDownButton : UserControl
    {
        private EventHandler _LongClickHander;

        public event EventHandler LevelDown;
        public event EventHandler LevelUp;
        public event EventHandler LongClick
        {
            add
            {
                _LongClickHander += value;
                _DownBT.LongClick += _BT_LongClick;
                _UpBT.LongClick += _BT_LongClick;
            }
            remove
            {
                _LongClickHander -= value;
                if (_LongClickHander == null)
                {
                    _DownBT.LongClick -= _BT_LongClick;
                    _UpBT.LongClick -= _BT_LongClick;
                }
            }
        }

        [CategoryAttribute("Appearance"),
        Description("Image of down button"),
        RefreshProperties(RefreshProperties.Repaint),
        DefaultValue(null)
        ]
        public Image DownImage
        {
            get { return _DownBT.ImageNormal; }
            set { _DownBT.ImageNormal = value; }
        }

        [CategoryAttribute("Appearance"),
        Description("Image of up button"),
        RefreshProperties(RefreshProperties.Repaint),
        DefaultValue(null)
        ]
        public Image UpImage
        {
            get { return _UpBT.ImageNormal; }
            set { _UpBT.ImageNormal = value; }
        }

        [Browsable(false),
        DefaultValue(0)
        ]
        public int Level
        {
            get { return _LevelBar.Value; }
            set
            {
                _LevelBar.Value = value;
                _DownBT.Enabled = _LevelBar.Value > _LevelBar.Minimum;
                _UpBT.Enabled = _LevelBar.Value < _LevelBar.Maximum;
            }
        }

        [Browsable(false),
        DefaultValue(false)
        ]
        public bool DrawX
        {
            get { return _DownBT.DrawX; }
            set { _DownBT.DrawX = _UpBT.DrawX = value; }
        }

        public UpDownButton()
        {
            InitializeComponent();
        }

        private void _DownBT_Click(object sender, EventArgs e)
        {
            General.SafeLaunchEvent(LevelDown, this);
        }

        private void _UpBT_Click(object sender, EventArgs e)
        {
            General.SafeLaunchEvent(LevelUp, this);
        }

        private void _BT_LongClick(object sender, EventArgs e)
        {
            General.SafeLaunchEvent(_LongClickHander, this);
        }
    }
}
