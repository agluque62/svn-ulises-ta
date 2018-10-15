using System;
using System.Windows.Forms;
using Microsoft.Practices.ObjectBuilder;
using HMI.Model.Module.Constants;

namespace HMI.Presentation.Urr.Layout
{
    public partial class ShellLayoutView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:ShellLayoutView"/> class.
        /// </summary>
        public ShellLayoutView()
        {
            InitializeComponent();

            _MainToolsWS.Name = WorkspaceNames.HeaderWorkspace;
            _RdWS.Name = WorkspaceNames.RdWorkspace;
            _TlfWS.Name = WorkspaceNames.TlfWorkspace;
            _LcWS.Name = WorkspaceNames.LcWorkspace;
        }

        /// <summary>
        /// 
        /// </summary>
        public Microsoft.Practices.CompositeUI.WinForms.DeckWorkspace MainToolsWS
        {
            get
            {
                return _MainToolsWS;
            }
        }

    }
}
