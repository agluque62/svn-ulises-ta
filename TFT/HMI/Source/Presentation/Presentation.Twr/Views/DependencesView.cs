//----------------------------------------------------------------------------------------
// patterns & practices - Smart Client Software Factory - Guidance Package
//
// This file was generated by the "Add View" recipe.
//
// This class is the concrete implementation of a View in the Model-View-Presenter 
// pattern. Communication between the Presenter and this class is acheived through 
// an interface to facilitate separation and testability.
// Note that the Presenter generated by the same recipe, will automatically be created
// by CAB through [CreateNew] and bidirectional references will be added.
//
// For more information see:
// ms-help://MS.VSCC.v80/MS.VSIPCC.v80/ms.practices.scsf.2007may/SCSF/html/02-09-010-ModelViewPresenter_MVP.htm
//
// Latest version of this Guidance Package: http://go.microsoft.com/fwlink/?LinkId=62182
//----------------------------------------------------------------------------------------

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using Microsoft.Practices.CompositeUI;
using Microsoft.Practices.CompositeUI.SmartParts;
using Microsoft.Practices.CompositeUI.EventBroker;
using Microsoft.Practices.ObjectBuilder;
using HMI.Infrastructure.Interface;
using HMI.Model.Module.Services;
using HMI.Model.Module.Messages;
using HMI.Model.Module.BusinessEntities;
using HMI.Model.Module.UI;
using HMI.Presentation.Twr.Constants;
using HMI.Presentation.Twr.Properties;
using Utilities;
using NLog;

namespace HMI.Presentation.Twr.Views
{
	[SmartPart]
	public partial class DependencesView : UserControl
	{
		[EventPublication(EventTopicNames.DependencesNumberCalled, PublicationScope.WorkItem)]
		public event EventHandler<EventArgs<string>> DependencesNumberCalled;

		private static Logger _Logger = LogManager.GetCurrentClassLogger();

		private IModelCmdManagerService _CmdManager = null;
		private StateManagerService _StateManager = null;
		private bool _IsCurrentView = false;

		private bool _PathEnabled
		{
			get { return _StateManager.Tft.Enabled; }
		}
		private bool _DependencesEnabled
		{
			get { return _StateManager.Tft.Enabled; }
		}
		private bool _TypeEnabled
		{
			get { return _StateManager.Tft.Enabled; }
		}
		private bool _AgvnEnabled
		{
			get { return _StateManager.Tft.Enabled; }
		}
		private bool _RtbEnabled
		{
			get { return _StateManager.Tft.Enabled; }
		}
		private bool _FunctionEnabled
		{
			get { return _StateManager.Tft.Enabled; }
		}
		private bool _CloseEnabled
		{
			get { return _StateManager.Tft.Enabled; }
		}
		private bool _CallEnabled
		{
			get 
			{
				return (_AgvnTB.Text.Length > 0) && 
					_StateManager.Tft.Enabled && _StateManager.Engine.Operative &&
					(_StateManager.Tlf.Priority.State != FunctionState.Error) &&
					(_StateManager.Tlf.Listen.State != FunctionState.Executing) &&
					(_StateManager.Tlf.Listen.State != FunctionState.Error) &&
					(_StateManager.Tlf.Transfer.State != FunctionState.Error) &&
					(_StateManager.Tlf.Transfer.State != FunctionState.Executing) &&
					(_StateManager.Tlf[Tlf.IaMappedPosition].State != TlfState.In) &&
					(_StateManager.Tlf[Tlf.IaMappedPosition].State != TlfState.InPrio) &&
					(_StateManager.Tlf[Tlf.IaMappedPosition].State != TlfState.RemoteIn);
			}
		}
        // Miguel
        private string _Call
        {
            get { return Resources.Llamar; }
        }

        private string _Close
        {
            get { return Resources.Cerrar; }
        }

        private string _Title
        {
            get { return Resources.DirectorioDependenciasUsuario; }
        }
        private string _Type
        {
            get { return Resources.Tipo; }
        }

        private string _Function
        {
            get { return Resources.Funcion; }
        }

        private string _AGVN
        {
            get { return Resources.AGVN; }
        }
        private string _RTB
        {
            get { return Resources.RTB; }
        }


		public DependencesView([ServiceDependency] IModelCmdManagerService cmdManager, [ServiceDependency] StateManagerService stateManager)
		{
			InitializeComponent();

			_CmdManager = cmdManager;
			_StateManager = stateManager;
            
            
            // Miguel
            _CallBT.Text = _Call;
            _CloseBT.Text = _Close;
            _TitleLB.Text = _Title;
            label1.Text = _Type;
            label2.Text = _Function;
            label3.Text = _AGVN;
            label4.Text = _RTB;
            if (global::HMI.Presentation.Twr.Properties.Settings.Default.BigFonts)
            {
                _PathTB.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                _TitleLB.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                _RtbTB.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                _AgvnTB.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                _FunctionTB.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                _TypeTB.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                _DependencesTV.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            }

        }

		[EventSubscription(EventTopicNames.TftEnabledChanged, ThreadOption.Publisher)]
		[EventSubscription(EventTopicNames.EngineStateChanged, ThreadOption.Publisher)]
		public void OnTftEngineChanged(object sender, EventArgs e)
		{
			if (_IsCurrentView)
			{
				_PathTB.Enabled = _PathEnabled;
				_DependencesTV.Enabled = _DependencesEnabled;
				_TypeTB.Enabled = _TypeEnabled;
				_FunctionTB.Enabled = _FunctionEnabled;
				_AgvnTB.Enabled = _AgvnEnabled;
				_RtbTB.Enabled = _RtbEnabled;
				_CallBT.Enabled = _CallEnabled;
				_CloseBT.Enabled = _CloseEnabled;
			}
		}

		[EventSubscription(EventTopicNames.ActiveViewChanging, ThreadOption.Publisher)]
		public void OnActiveViewChanging(object sender, EventArgs<string> e)
		{
			if (e.Data == ViewNames.Depencences)
			{
				_IsCurrentView = true;
				_DependencesTV.SelectedNode = null;
				SelectNode(null);

				_PathTB.Enabled = _PathEnabled;
				_DependencesTV.Enabled = _DependencesEnabled;
				_TypeTB.Enabled = _TypeEnabled;
				_FunctionTB.Enabled = _FunctionEnabled;
				_AgvnTB.Enabled = _AgvnEnabled;
				_RtbTB.Enabled = _RtbEnabled;
				_CallBT.Enabled = _CallEnabled;
				_CloseBT.Enabled = _CloseEnabled;
			}
			else if (_IsCurrentView)
			{
				_IsCurrentView = false;
			}
		}

		[EventSubscription(EventTopicNames.TlfPriorityChanged, ThreadOption.Publisher)]
		[EventSubscription(EventTopicNames.TlfListenChanged, ThreadOption.Publisher)]
		[EventSubscription(EventTopicNames.TlfTransferChanged, ThreadOption.Publisher)]
		public void OnFacilityChanged(object sender, EventArgs e)
		{
			if (_IsCurrentView)
			{
				_CallBT.Enabled = _CallEnabled;
			}
		}

		[EventSubscription(EventTopicNames.TlfChanged, ThreadOption.Publisher)]
		public void OnTlfChanged(object sender, RangeMsg e)
		{
			if (_IsCurrentView && ((e.From + e.Count) > Tlf.IaMappedPosition))
			{
				_CallBT.Enabled = _CallEnabled;
			}
		}

		[EventSubscription(EventTopicNames.NumberBookChanged, ThreadOption.Publisher)]
		public void OnNumberBookChanged(object sender, EventArgs e)
		{
			_DependencesTV.Nodes.Clear();

			foreach (Area area in _StateManager.NumberBook.Areas)
			{
				TreeNode areaNode = new TreeNode(area.Name);
				_DependencesTV.Nodes.Add(areaNode);

				foreach (KeyValuePair<string, Fir> fir in area)
				{
					TreeNode firNode = new TreeNode(fir.Key);
					areaNode.Nodes.Add(firNode);

					foreach (KeyValuePair<string, Depencence> dependence in fir.Value)
					{
						TreeNode dependenceNode = new TreeNode(dependence.Key);
						firNode.Nodes.Add(dependenceNode);

						foreach (KeyValuePair<string, UserNumber> user in dependence.Value)
						{
							TreeNode userNode = new TreeNode(user.Key);
							dependenceNode.Nodes.Add(userNode);

							userNode.Tag = user.Value;
						}
					}
				}
			}
		}

		private void _DependencesTV_AfterSelect(object sender, TreeViewEventArgs e)
		{
			try
			{
				SelectNode(e.Node);
			}
			catch (Exception ex)
			{
				string node = e.Node != null ? e.Node.Name : "";
				_Logger.Error("ERROR intentando seleccionar la dependencia " + node, ex);
			}
		}

		private void _CallBT_Click(object sender, EventArgs e)
		{
			try
			{
				General.SafeLaunchEvent(DependencesNumberCalled, this, new EventArgs<string>(_AgvnTB.Text));
			}
			catch (Exception ex)
			{
				_Logger.Error("ERROR intentando llamar a la dependencia " + _AgvnTB.Text, ex);
			}
		}

		private void _CloseBT_Click(object sender, EventArgs e)
		{
			try
			{
				_CmdManager.SwitchTlfView(null);
			}
			catch (Exception ex)
			{
				_Logger.Error("ERROR cerrando la vista dependencia", ex);
			}
		}

		private void DependencesView_BackColorChanged(object sender, EventArgs e)
		{
			Invalidate(true);
		}

		private void SelectNode(TreeNode node)
		{
			if ((node != null) && (node.Tag != null))
			{
				UserNumber number = (UserNumber)node.Tag;

				_TypeTB.Text = number.Type;
				_FunctionTB.Text = number.Role;
				_AgvnTB.Text = number.R2Number;
				_RtbTB.Text = number.RtbNumber;

				_PathTB.Text = node.FullPath;
			}
			else
			{
				_TypeTB.Text = "";
				_FunctionTB.Text = "";
				_AgvnTB.Text = "";
				_RtbTB.Text = "";

				if (node != null)
				{
					_PathTB.Text = node.FullPath;
				}
			}

			_CallBT.Enabled = _CallEnabled;
		}
	}
}

