namespace HMI.Presentation.Asecna.Views
{
    partial class EngineInfoView
    {
        /// <summary> 
        /// Variable del diseñador requerida.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Limpiar los recursos que se estén utilizando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben eliminar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de componentes

        /// <summary> 
        /// Método necesario para admitir el Diseñador. No se puede modificar 
        /// el contenido del método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this._TitleLB = new System.Windows.Forms.Label();
            this._LVVersionModules = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this._CloseBT = new HMI.Model.Module.UI.HMIButton();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // _TitleLB
            // 
            this._TitleLB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._TitleLB.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this._TitleLB.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tableLayoutPanel1.SetColumnSpan(this._TitleLB, 3);
            this._TitleLB.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._TitleLB.Location = new System.Drawing.Point(5, 3);
            this._TitleLB.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this._TitleLB.Name = "_TitleLB";
            this._TitleLB.Size = new System.Drawing.Size(346, 24);
            this._TitleLB.TabIndex = 1;
            this._TitleLB.Text = "Versiones librerías";
            this._TitleLB.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _LVVersionModules
            // 
            this._LVVersionModules.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.tableLayoutPanel1.SetColumnSpan(this._LVVersionModules, 3);
            this._LVVersionModules.Dock = System.Windows.Forms.DockStyle.Fill;
            this._LVVersionModules.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._LVVersionModules.FullRowSelect = true;
            this._LVVersionModules.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this._LVVersionModules.HideSelection = false;
            this._LVVersionModules.Location = new System.Drawing.Point(3, 38);
            this._LVVersionModules.MultiSelect = false;
            this._LVVersionModules.Name = "_LVVersionModules";
            this._LVVersionModules.Size = new System.Drawing.Size(350, 335);
            this._LVVersionModules.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this._LVVersionModules.TabIndex = 11;
            this._LVVersionModules.UseCompatibleStateImageBehavior = false;
            this._LVVersionModules.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Módulo";
            this.columnHeader1.Width = 178;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Versión";
            this.columnHeader2.Width = 125;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.Controls.Add(this._CloseBT, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this._TitleLB, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this._LVVersionModules, 0, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 9.49367F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 90.50633F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 49F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(356, 426);
            this.tableLayoutPanel1.TabIndex = 12;
            // 
            // _CloseBT
            // 
            this._CloseBT.Dock = System.Windows.Forms.DockStyle.Fill;
            this._CloseBT.Location = new System.Drawing.Point(121, 379);
            this._CloseBT.Name = "_CloseBT";
            this._CloseBT.Size = new System.Drawing.Size(112, 44);
            this._CloseBT.TabIndex = 10;
            this._CloseBT.Text = "Cerrar";
            this._CloseBT.Click += new System.EventHandler(this._CloseBT_Click);
            // 
            // EngineInfoView
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "EngineInfoView";
            this.Size = new System.Drawing.Size(362, 432);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Model.Module.UI.HMIButton _CloseBT;
        private System.Windows.Forms.ListView _LVVersionModules;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label _TitleLB;
    }
}
