using System;
using System.Windows.Forms;
using Microsoft.Practices.ObjectBuilder;
using HMI.Model.Module.Constants;

namespace HMI.Presentation.Asecna.Layout
{
	public partial class ShellLayoutView : UserControl
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:ShellLayoutView"/> class.
		/// </summary>
		public ShellLayoutView()
		{
			InitializeComponent();

            Control[] ctrls = Controls.Find("_MainTLP", true);
            if (ctrls.Length > 0)
            {
                System.Windows.Forms.TableLayoutPanel _MainTLP = ctrls[0].Name == "_MainTLP" ? (System.Windows.Forms.TableLayoutPanel)ctrls[0] : null;
                // Ajustar el alto del área de Radio y Telefonía de acuerdo al número de filas de LC
                _MainTLP.RowStyles[2].Height = HMI.Presentation.Asecna.Properties.Settings.Default.LcRows == 2 ? _MainTLP.RowStyles[2].Height : _MainTLP.RowStyles[2].Height / 2;
                _MainTLP.RowStyles[1].Height = HMI.Presentation.Asecna.Properties.Settings.Default.LcRows == 2 ? _MainTLP.RowStyles[1].Height : (_MainTLP.RowStyles[1].Height + (_MainTLP.RowStyles[2].Height / 2));
            }

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
