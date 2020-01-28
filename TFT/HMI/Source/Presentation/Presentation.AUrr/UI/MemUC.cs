using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Microsoft.Practices.CompositeUI;
using Microsoft.Practices.CompositeUI.SmartParts;
using Microsoft.Practices.CompositeUI.EventBroker;
using HMI.Model.Module.BusinessEntities;
using HMI.Model.Module.Services;
using HMI.Model.Module.Messages;
using HMI.Presentation.AUrr.Constants;
using HMI.Presentation.AUrr.Properties;

using Utilities;

namespace HMI.Presentation.AUrr.UI
{
    [SmartPart]
    public partial class MemUC : UserControl
    {
        public event GenericEventHandler<Number> OkClick;
        public event GenericEventHandler CancelClick;

        private StateManagerService _StateManager;

        private bool _OkEnabled
        {
            get
            {
                return (_MemLB.SelectedIndex >= 0) &&
                    _StateManager.Tft.Enabled && _StateManager.Engine.Operative &&
                    (_StateManager.Tlf.Priority.State != FunctionState.Error) &&
                    (_StateManager.Tlf.Listen.State != FunctionState.Executing) &&
                    (_StateManager.Tlf.Listen.State != FunctionState.Error) &&
                    (_StateManager.Tlf.Transfer.State != FunctionState.Executing) &&
                    (_StateManager.Tlf.Transfer.State != FunctionState.Error) &&
                    (_StateManager.Tlf[Tlf.IaMappedPosition].State != TlfState.In) &&
                    (_StateManager.Tlf[Tlf.IaMappedPosition].State != TlfState.InPrio) &&
                    (_StateManager.Tlf[Tlf.IaMappedPosition].State != TlfState.RemoteIn);
            }
        }
        private bool _CancelEnabled
        {
            get { return _StateManager.Tft.Enabled; }
        }
        private bool _MemLBEnabled
        {
            get { return _StateManager.Tft.Enabled; }
        }

        private string _Aceptar // Miguel
        {
            get { return Resources.Aceptar; }
        }

        private string _Cancelar // Miguel
        {
            get { return Resources.Cancelar; }
        }


        public MemUC([ServiceDependency] StateManagerService stateManager)
        {
            InitializeComponent();

            _StateManager = stateManager;

            _MemLB.SelectedIndex = -1;
            _OkBT.Enabled = _OkEnabled;
            _CancelBT.Enabled = _CancelEnabled;
            _MemLB.Enabled = _MemLBEnabled;

            // Miguel
            this._OkBT.Text = _Aceptar;
            this._CancelBT.Text = _Cancelar;
        }

        public void Reset()
        {
            _MemLB.SelectedIndex = -1;
        }

        public void Reset(IEnumerable<Number> numbers)
        {
            _MemLB.Items.Clear();

            foreach (object num in numbers)
            {
                _MemLB.Items.Add(num);
            }

            _MemLB.SelectedIndex = -1;
        }

        [EventSubscription(EventTopicNames.TftEnabledChanged, ThreadOption.Publisher)]
        [EventSubscription(EventTopicNames.EngineStateChanged, ThreadOption.Publisher)]
        public void OnTftEngineChanged(object sender, EventArgs e)
        {
            _OkBT.Enabled = _OkEnabled;
            _CancelBT.Enabled = _CancelEnabled;
            _MemLB.Enabled = _MemLBEnabled;
        }

        [EventSubscription(EventTopicNames.TlfPriorityChanged, ThreadOption.Publisher)]
        [EventSubscription(EventTopicNames.TlfListenChanged, ThreadOption.Publisher)]
        [EventSubscription(EventTopicNames.TlfTransferChanged, ThreadOption.Publisher)]
        public void OnFacilityChanged(object sender, EventArgs e)
        {
            _OkBT.Enabled = _OkEnabled;
        }

        [EventSubscription(EventTopicNames.TlfChanged, ThreadOption.Publisher)]
        public void OnTlfChanged(object sender, RangeMsg e)
        {
            if ((e.From + e.Count) > Tlf.IaMappedPosition)
            {
                _OkBT.Enabled = _OkEnabled;
            }
        }

        private void _OkBT_Click(object sender, EventArgs e)
        {
            General.SafeLaunchEvent(OkClick, this, (Number)_MemLB.SelectedItem);
        }

        private void _CancelBT_Click(object sender, EventArgs e)
        {
            General.SafeLaunchEvent(CancelClick, this);
        }

        private void _MemLB_SelectedIndexChanged(object sender, EventArgs e)
        {
            _OkBT.Enabled = _OkEnabled;
        }

        private void _MemLB_DrawItem(object sender, DrawItemEventArgs e)
        {
            if ((e.State & DrawItemState.Selected) != DrawItemState.Selected)
            {
                e.DrawBackground();
            }
            else
            {
                e.Graphics.FillRectangle(Brushes.Yellow, e.Bounds);
                e.DrawFocusRectangle();
            }

            if (e.Index >= 0)
            {
                using (SolidBrush fbr = new SolidBrush(_MemLB.ForeColor))
                {
                    StringFormat format = new StringFormat();
                    format.LineAlignment = StringAlignment.Center;
                    format.Alignment = StringAlignment.Near;
                    e.Graphics.DrawString(_MemLB.Items[e.Index].ToString(), e.Font, fbr, e.Bounds, format);
                }
            }
        }
    }
}
