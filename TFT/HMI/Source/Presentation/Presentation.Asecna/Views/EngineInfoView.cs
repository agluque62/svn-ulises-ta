using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

using Microsoft.Practices.CompositeUI;
using Microsoft.Practices.CompositeUI.SmartParts;
using Microsoft.Practices.CompositeUI.EventBroker;
using Microsoft.Practices.ObjectBuilder;

using HMI.Infrastructure.Interface;
using HMI.Presentation.Asecna.Constants;
using HMI.Model.Module.Services;
using Utilities;
using NLog;

namespace HMI.Presentation.Asecna.Views
{
    [SmartPart]
    public partial class EngineInfoView : UserControl
    {
        private static Logger _Logger = LogManager.GetCurrentClassLogger();
        private IModelCmdManagerService _CmdManager = null;
        private StateManagerService _StateManager = null;


        public EngineInfoView([ServiceDependency] IModelCmdManagerService cmdManager, [ServiceDependency] StateManagerService stateManager)
        {
            InitializeComponent();
            _CmdManager = cmdManager;
            _StateManager = stateManager;
        }

        [EventSubscription(EventTopicNames.ActiveViewChanging, ThreadOption.Publisher)]
        public void OnActiveViewChanging(object sender, EventArgs<string> e)
        {
            if (e.Data == ViewNames.InfoEngine)
            {
                foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    switch (assembly.GetName().Name)
                    {
                        case "HMI":
                        case "Utilities":
                        case "Model.Module":
                        case "CD40.Module":
                        case "Infrastructure.Interface":
                        case "Infrastructure.Library":
                        case "Presentation.Rabat":
                        case "Cd40.Infrastructure":
                            ListViewItem.ListViewSubItem[] subItem = new ListViewItem.ListViewSubItem[2];
                            subItem[0] = new ListViewItem.ListViewSubItem();
                            subItem[0].Text = assembly.GetName().Name;
                            subItem[1] = new ListViewItem.ListViewSubItem();
                            subItem[1].Text = assembly.GetName().Version.ToString();

                            ListViewItem item = new ListViewItem(subItem, null);
                        
                            _LVVersionModules.Items.Add(item);
                        break;
                    }
                }
            }
        }

        private void _CloseBT_Click(object sender, EventArgs e)
        {
            try
            {
                _LVVersionModules.Items.Clear();
                _CmdManager.SwitchTlfView(null);
            }
            catch (Exception ex)
            {
                _Logger.Error("ERROR cerrando la vista info versiones", ex);
            }
        }
    }
}
