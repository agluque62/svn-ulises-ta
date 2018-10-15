//----------------------------------------------------------------------------------------
// patterns & practices - Smart Client Software Factory - Guidance Package
//
// This file was generated by the "Add Business Module" recipe.
//
// This class contains placeholder methods for the common module initialization 
// tasks, such as adding services, or user-interface element
//
// For more information see: 
// ms-help://MS.VSCC.v80/MS.VSIPCC.v80/ms.practices.scsf.2007may/SCSF/html/02-08-060-Add_Business_Module_Next_Steps.htm
//
// Latest version of this Guidance Package: http://go.microsoft.com/fwlink/?LinkId=62182
//----------------------------------------------------------------------------------------

using System;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Practices.CompositeUI;
using Microsoft.Practices.CompositeUI.Commands;
using Microsoft.Practices.CompositeUI.EventBroker;
using HMI.Infrastructure.Interface;
using HMI.Infrastructure.Library.UI;
using HMI.Model.Module.Services;
using HMI.Model.Module.BusinessEntities;
using HMI.Model.Module.Messages;
using HMI.Model.Module.UI;
using HMI.Presentation.Twr.Constants;
using HMI.Presentation.Twr.Views;
using HMI.Presentation.Twr.Properties;
using HMI.Presentation.Twr.UI;
using Utilities;

namespace HMI.Presentation.Twr
{
	public class ModuleController : WorkItemController
	{
		private IModelCmdManagerService _CmdManager = null;
		private StateManagerService _StateManager = null;
		private string _ActualTlfView = ViewNames.TlfDa;
		private string _PreviusTlfView = null;
		private List<MessageBoxView> _Messages = new List<MessageBoxView>();

		public string ActualTlfView
		{
			get { return _ActualTlfView; }
		}

		public ModuleController([ServiceDependency] IModelCmdManagerService cmdManager, [ServiceDependency] StateManagerService stateManager)
		{
			_CmdManager = cmdManager;
			_StateManager = stateManager;
		}

		public override void Run()
		{
			AddServices();
			ExtendMenu();
			ExtendToolStrip();
			AddViews();

			OnActiveScvChanged(this, EventArgs.Empty);
		}

		private void AddServices()
		{
			//TODO: add services provided by the Module. See: Add or AddNew method in 
			//		WorkItem.Services collection or see ms-help://MS.VSCC.v80/MS.VSIPCC.v80/ms.practices.2005Nov.cab/CAB/html/03-020-Adding%20Services.htm
		}

		private void ExtendMenu()
		{
			//TODO: add menu items here, normally by calling the "Add" method on
			//		on the WorkItem.UIExtensionSites collection. For an example 
			//		See: ms-help://MS.VSCC.v80/MS.VSIPCC.v80/ms.practices.scsf.2007may/SCSF/html/02-04-340-Showing_UIElements.htm
		}

		private void ExtendToolStrip()
		{
			//TODO: add new items to the ToolStrip in the Shell. See the UIExtensionSites collection in the WorkItem. 
			//		See: ms-help://MS.VSCC.v80/MS.VSIPCC.v80/ms.practices.scsf.2007may/SCSF/html/02-04-340-Showing_UIElements.htm
		}

		private void AddViews()
		{
			//TODO: create the Module views, add them to the WorkItem and show them in 
			//		a Workspace. See: ms-help://MS.VSCC.v80/MS.VSIPCC.v80/ms.practices.scsf.2007may/SCSF/html/03-01-040-How_to_Add_a_View_with_a_Presenter.htm

			// To create and add a view you can customize the following sentence
			// SampleView view = ShowViewInWorkspace<SampleView>(WorkspaceNames.SampleWorkspace);

			WorkItem.SmartParts.AddNew<ScreenSaverView>(ViewNames.ScreenSaver);
			WorkItem.SmartParts.AddNew<TlfIaView>(ViewNames.TlfIa);
			WorkItem.SmartParts.AddNew<DependencesView>(ViewNames.Depencences);

			ShowViewInWorkspace<HeaderView>(WorkspaceNames.HeaderWorkspace);
			ShowViewInWorkspace<RadioView>(ViewNames.Radio, WorkspaceNames.RdWorkspace);
			ShowViewInWorkspace<LcView>(ViewNames.Lc, WorkspaceNames.LcWorkspace);
			ShowViewInWorkspace<TlfView>(ViewNames.Tlf, WorkspaceNames.TlfWorkspace);
			ShowViewInWorkspace<TlfDaView>(ViewNames.TlfDa, WorkspaceNames.TlfNumbersWorkspace);
			ShowViewInWorkspace<TlfFunctionsView>(ViewNames.TlfFunctions, WorkspaceNames.TlfFunctionsWorkspace);
		}

		//TODO: Add CommandHandlers and/or Event Subscriptions
		//		See: ms-help://MS.VSCC.v80/MS.VSIPCC.v80/ms.practices.scsf.2007may/SCSF/html/02-04-350-Registering_Commands.htm
		//		See: ms-help://MS.VSCC.v80/MS.VSIPCC.v80/ms.practices.scsf.2007may/SCSF/html/02-04-320-Publishing_and_Subscribing_to_Events.htm

		[EventSubscription(EventTopicNames.ActiveScvChanged, ThreadOption.Publisher)]
		public void OnActiveScvChanged(object sender, EventArgs e)
		{
			Color color = _StateManager.Scv.Active == -1 ? VisualStyle.UnknownScv :
				_StateManager.Scv.Active == 0 ? VisualStyle.ScvA : VisualStyle.ScvB;

			WorkItem.SmartParts.Get<RadioView>(ViewNames.Radio).BackColor = color;
			WorkItem.SmartParts.Get<LcView>(ViewNames.Lc).BackColor = color;
			WorkItem.SmartParts.Get<TlfView>(ViewNames.Tlf).BackColor = color;
			WorkItem.SmartParts.Get<DependencesView>(ViewNames.Depencences).BackColor = color;
		}

		[EventSubscription(EventTopicNames.ShowInfoUI, ThreadOption.Publisher)]
		public void OnShowInfoUI(object sender, EventArgs e)
		{
			NavigateTo(ViewNames.Depencences);
		}

		[EventSubscription(EventTopicNames.SwitchTlfViewUI, ThreadOption.Publisher)]
		public void OnSwitchTlfViewUI(object sender, EventArgs<string> e)
		{
			if (e.Data != null)
			{
				NavigateTo(e.Data);
			}
			else if (_ActualTlfView == ViewNames.TlfDa)
			{
				NavigateTo(ViewNames.TlfIa);
			}
			else if (_ActualTlfView == ViewNames.TlfIa)
			{
				NavigateTo(ViewNames.TlfDa);
			}
			else if (_ActualTlfView == ViewNames.Depencences)
			{
				NavigateToPrevius();
			}
		}

		[EventSubscription(EventTopicNames.ScreenSaverChanged, ThreadOption.Publisher)]
		public void OnScreenSaverChanged(object sender, EventArgs e)
		{
			if (_StateManager.ScreenSaver.On)
			{
				WindowSmartPartInfo info = new WindowSmartPartInfo();
				info.Keys[WindowWorkspaceSetting.FormBorderStyle] = FormBorderStyle.None;
				ShowViewInWorkspace<ScreenSaverView>(ViewNames.ScreenSaver, WorkspaceNames.ScreenSaverWindow, info);
			}
			else
			{
				HideViewInWorkspace<ScreenSaverView>(ViewNames.ScreenSaver, WorkspaceNames.ScreenSaverWindow);

				foreach (MessageBoxView view in _Messages)
				{
					WindowSmartPartInfo info = new WindowSmartPartInfo();

					info.ControlBox = false;
					info.MaximizeBox = false;
					info.MinimizeBox = false;
					info.Title = view.Message.Caption;
					info.Keys[WindowWorkspaceSetting.FormBorderStyle] = FormBorderStyle.Sizable;
					view.Width = view.Message.Width;
					view.Height = view.Message.Height;
					info.Location = new Point((Screen.PrimaryScreen.Bounds.Width - view.Width) / 2, (Screen.PrimaryScreen.Bounds.Height - view.Height) / 2);

					WorkItem.Workspaces[WorkspaceNames.ModalWindows].Show(view, info);
				}

				_Messages.Clear();
			}
		}

		[EventSubscription(EventTopicNames.TlfChanged, ThreadOption.Publisher)]
		public void OnTlfChanged(object sender, RangeMsg e)
		{
			if (_ActualTlfView == ViewNames.Depencences)
			{
				for (int i = e.From, to = e.From + e.Count; i < to; i++)
				{
					TlfDst dst = _StateManager.Tlf[i];

					if ((dst.State == TlfState.In) || (dst.State == TlfState.InPrio) || (dst.State == TlfState.RemoteIn))
					{
						NavigateToPrevius();
						break;
					}
				}
			}
		}

		[EventSubscription(EventTopicNames.ShowNotifMsgUI, ThreadOption.Publisher)]
		public void OnShowNotifMsgUI(object sender, NotifMsg e)
		{
			MessageBoxView view = WorkItem.SmartParts.Contains(e.Id) ?
				WorkItem.SmartParts.Get<MessageBoxView>(e.Id) :
				WorkItem.SmartParts.AddNew<MessageBoxView>(e.Id);
			view.Message = e;

			if (!_StateManager.ScreenSaver.On)
			{
				WindowSmartPartInfo info = new WindowSmartPartInfo();

				info.ControlBox = false;
				info.MaximizeBox = false;
				info.MinimizeBox = false;
				info.Title = view.Message.Caption;
				info.Keys[WindowWorkspaceSetting.FormBorderStyle] = FormBorderStyle.Sizable;
				view.Width = view.Message.Width;
				view.Height = view.Message.Height;
				info.Location = new Point((Screen.PrimaryScreen.Bounds.Width - view.Width) / 2, (Screen.PrimaryScreen.Bounds.Height - view.Height) / 2);

				WorkItem.Workspaces[WorkspaceNames.ModalWindows].Show(view, info);
			}
			else if (!_Messages.Contains(view))
			{
				_Messages.Add(view);
			}
		}

		[EventSubscription(EventTopicNames.HideNotifMsgUI, ThreadOption.Publisher)]
		public void OnHideNotifMsgUI(object sender, EventArgs<string> e)
		{
			foreach (MessageBoxView view in WorkItem.Workspaces[WorkspaceNames.ModalWindows].SmartParts)
			{
				if (string.IsNullOrEmpty(e.Data) || view.Message.Id.StartsWith(e.Data))
				{
					WorkItem.Workspaces[WorkspaceNames.ModalWindows].Close(view);
					WorkItem.SmartParts.Remove(view);
					view.Dispose();
				}
			}

			List<MessageBoxView> viewsToRemove = _Messages.FindAll(delegate(MessageBoxView v) { return v.Message.Id.StartsWith(e.Data); });
			foreach (MessageBoxView view in viewsToRemove)
			{
				_Messages.Remove(view);
				WorkItem.SmartParts.Remove(view);
				view.Dispose();
			}
		}

		public void NavigateTo(string viewId)
		{
			if (viewId != _ActualTlfView)
			{
				_PreviusTlfView = _ActualTlfView;
				_ActualTlfView = viewId;

				switch (_ActualTlfView)
				{
					case ViewNames.TlfDa:
						ShowViewInWorkspace<TlfView>(ViewNames.Tlf, WorkspaceNames.TlfWorkspace);
						ShowViewInWorkspace<TlfFunctionsView>(ViewNames.TlfFunctions, WorkspaceNames.TlfFunctionsWorkspace);
						ShowViewInWorkspace<TlfDaView>(ViewNames.TlfDa, WorkspaceNames.TlfNumbersWorkspace);
						break;
					case ViewNames.TlfIa:
						ShowViewInWorkspace<TlfView>(ViewNames.Tlf, WorkspaceNames.TlfWorkspace);
						ShowViewInWorkspace<TlfFunctionsView>(ViewNames.TlfFunctions, WorkspaceNames.TlfFunctionsWorkspace);
						ShowViewInWorkspace<TlfIaView>(ViewNames.TlfIa, WorkspaceNames.TlfNumbersWorkspace);
						break;
					case ViewNames.Depencences:
						ShowViewInWorkspace<DependencesView>(ViewNames.Depencences, WorkspaceNames.TlfWorkspace);
						break;
				}
			}
		}

		public void NavigateToPrevius()
		{
			if (_PreviusTlfView != null)
			{
				_ActualTlfView = _PreviusTlfView;
				_PreviusTlfView = null;

				switch (_ActualTlfView)
				{
					case ViewNames.TlfDa:
						ShowViewInWorkspace<TlfView>(ViewNames.Tlf, WorkspaceNames.TlfWorkspace);
						ShowViewInWorkspace<TlfDaView>(ViewNames.TlfDa, WorkspaceNames.TlfNumbersWorkspace);
						ShowViewInWorkspace<TlfFunctionsView>(ViewNames.TlfFunctions, WorkspaceNames.TlfFunctionsWorkspace);
						break;
					case ViewNames.TlfIa:
						ShowViewInWorkspace<TlfView>(ViewNames.Tlf, WorkspaceNames.TlfWorkspace);
						ShowViewInWorkspace<TlfIaView>(ViewNames.TlfIa, WorkspaceNames.TlfNumbersWorkspace);
						ShowViewInWorkspace<TlfFunctionsView>(ViewNames.TlfFunctions, WorkspaceNames.TlfFunctionsWorkspace);
						break;
					case ViewNames.Depencences:
						ShowViewInWorkspace<DependencesView>(ViewNames.Depencences, WorkspaceNames.TlfWorkspace);
						break;
				}
			}
		}
	}
}
