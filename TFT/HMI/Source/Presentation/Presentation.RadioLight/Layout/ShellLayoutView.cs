using System;
using System.Windows.Forms;
using Microsoft.Practices.ObjectBuilder;
using HMI.Model.Module.Constants;
using HMI.Presentation.RadioLight.Properties;

namespace HMI.Presentation.RadioLight.Layout
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
