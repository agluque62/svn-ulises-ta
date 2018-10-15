namespace HMI.Presentation.Asecna.Views
{
    partial class HfView
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this._SelCalButton = new HMI.Model.Module.UI.HMIButton();
            this._TBDisplayMessages = new System.Windows.Forms.TextBox();
            this._BtMem3 = new HMI.Model.Module.UI.HMIButton();
            this._BtMem2 = new HMI.Model.Module.UI.HMIButton();
            this._BtMem1 = new HMI.Model.Module.UI.HMIButton();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this._BtA = new HMI.Model.Module.UI.HMIButton();
            this._BtB = new HMI.Model.Module.UI.HMIButton();
            this._BtC = new HMI.Model.Module.UI.HMIButton();
            this._BtD = new HMI.Model.Module.UI.HMIButton();
            this._BtE = new HMI.Model.Module.UI.HMIButton();
            this._BtF = new HMI.Model.Module.UI.HMIButton();
            this._BtG = new HMI.Model.Module.UI.HMIButton();
            this._BtH = new HMI.Model.Module.UI.HMIButton();
            this._BtJ = new HMI.Model.Module.UI.HMIButton();
            this._BtK = new HMI.Model.Module.UI.HMIButton();
            this._BtL = new HMI.Model.Module.UI.HMIButton();
            this._BtM = new HMI.Model.Module.UI.HMIButton();
            this._BtP = new HMI.Model.Module.UI.HMIButton();
            this._BtQ = new HMI.Model.Module.UI.HMIButton();
            this._BtR = new HMI.Model.Module.UI.HMIButton();
            this._BtS = new HMI.Model.Module.UI.HMIButton();
            this._BtBorrar = new HMI.Model.Module.UI.HMIButton();
            this._BtEnviar = new HMI.Model.Module.UI.HMIButton();
            this._TBDisplayCode = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.Controls.Add(this._SelCalButton, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this._TBDisplayMessages, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this._BtMem3, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this._BtMem2, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this._BtMem1, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this._BtBorrar, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this._BtEnviar, 4, 1);
            this.tableLayoutPanel1.Controls.Add(this._TBDisplayCode, 3, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(447, 102);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // _SelCalButton
            // 
            this._SelCalButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this._SelCalButton.Enabled = false;
            this._SelCalButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._SelCalButton.Location = new System.Drawing.Point(1, 1);
            this._SelCalButton.Margin = new System.Windows.Forms.Padding(1);
            this._SelCalButton.Name = "_SelCalButton";
            this._SelCalButton.Size = new System.Drawing.Size(87, 32);
            this._SelCalButton.TabIndex = 0;
            this._SelCalButton.Text = "Sel. Call";
            this._SelCalButton.Click += new System.EventHandler(this._SelCalButton_Click);
            // 
            // _TBDisplayMessages
            // 
            this.tableLayoutPanel1.SetColumnSpan(this._TBDisplayMessages, 3);
            this._TBDisplayMessages.Dock = System.Windows.Forms.DockStyle.Fill;
            this._TBDisplayMessages.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._TBDisplayMessages.Location = new System.Drawing.Point(3, 37);
            this._TBDisplayMessages.Name = "_TBDisplayMessages";
            this._TBDisplayMessages.ReadOnly = true;
            this._TBDisplayMessages.Size = new System.Drawing.Size(261, 18);
            this._TBDisplayMessages.TabIndex = 1;
            this._TBDisplayMessages.Visible = false;
            // 
            // _BtMem3
            // 
            this._BtMem3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._BtMem3.Enabled = false;
            this._BtMem3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._BtMem3.Location = new System.Drawing.Point(268, 1);
            this._BtMem3.Margin = new System.Windows.Forms.Padding(1);
            this._BtMem3.Name = "_BtMem3";
            this._BtMem3.Size = new System.Drawing.Size(87, 32);
            this._BtMem3.TabIndex = 5;
            this._BtMem3.Text = "Sel.Call 3";
            this._BtMem3.Visible = false;
            this._BtMem3.Click += new System.EventHandler(this._BtMem_Click);
            // 
            // _BtMem2
            // 
            this._BtMem2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._BtMem2.Enabled = false;
            this._BtMem2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._BtMem2.Location = new System.Drawing.Point(179, 1);
            this._BtMem2.Margin = new System.Windows.Forms.Padding(1);
            this._BtMem2.Name = "_BtMem2";
            this._BtMem2.Size = new System.Drawing.Size(87, 32);
            this._BtMem2.TabIndex = 4;
            this._BtMem2.Text = "Sel.Call2";
            this._BtMem2.Visible = false;
            this._BtMem2.Click += new System.EventHandler(this._BtMem_Click);
            // 
            // _BtMem1
            // 
            this._BtMem1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._BtMem1.Enabled = false;
            this._BtMem1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._BtMem1.Location = new System.Drawing.Point(90, 1);
            this._BtMem1.Margin = new System.Windows.Forms.Padding(1);
            this._BtMem1.Name = "_BtMem1";
            this._BtMem1.Size = new System.Drawing.Size(87, 32);
            this._BtMem1.TabIndex = 3;
            this._BtMem1.Text = "Sel.Call 1";
            this._BtMem1.Visible = false;
            this._BtMem1.Click += new System.EventHandler(this._BtMem_Click);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 16;
            this.tableLayoutPanel1.SetColumnSpan(this.tableLayoutPanel2, 5);
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25F));
            this.tableLayoutPanel2.Controls.Add(this._BtA, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this._BtB, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this._BtC, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this._BtD, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this._BtE, 4, 0);
            this.tableLayoutPanel2.Controls.Add(this._BtF, 5, 0);
            this.tableLayoutPanel2.Controls.Add(this._BtG, 6, 0);
            this.tableLayoutPanel2.Controls.Add(this._BtH, 7, 0);
            this.tableLayoutPanel2.Controls.Add(this._BtJ, 8, 0);
            this.tableLayoutPanel2.Controls.Add(this._BtK, 9, 0);
            this.tableLayoutPanel2.Controls.Add(this._BtL, 10, 0);
            this.tableLayoutPanel2.Controls.Add(this._BtM, 11, 0);
            this.tableLayoutPanel2.Controls.Add(this._BtP, 12, 0);
            this.tableLayoutPanel2.Controls.Add(this._BtQ, 13, 0);
            this.tableLayoutPanel2.Controls.Add(this._BtR, 14, 0);
            this.tableLayoutPanel2.Controls.Add(this._BtS, 15, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 68);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(441, 34);
            this.tableLayoutPanel2.TabIndex = 6;
            this.tableLayoutPanel2.Visible = false;
            // 
            // _BtA
            // 
            this._BtA.Dock = System.Windows.Forms.DockStyle.Fill;
            this._BtA.Location = new System.Drawing.Point(1, 1);
            this._BtA.Margin = new System.Windows.Forms.Padding(1);
            this._BtA.Name = "_BtA";
            this._BtA.Size = new System.Drawing.Size(25, 32);
            this._BtA.TabIndex = 0;
            this._BtA.Text = "A";
            this._BtA.Click += new System.EventHandler(this._BtAnyCode_Click);
            // 
            // _BtB
            // 
            this._BtB.Dock = System.Windows.Forms.DockStyle.Fill;
            this._BtB.Location = new System.Drawing.Point(28, 1);
            this._BtB.Margin = new System.Windows.Forms.Padding(1);
            this._BtB.Name = "_BtB";
            this._BtB.Size = new System.Drawing.Size(25, 32);
            this._BtB.TabIndex = 1;
            this._BtB.Text = "B";
            this._BtB.Click += new System.EventHandler(this._BtAnyCode_Click);
            // 
            // _BtC
            // 
            this._BtC.Dock = System.Windows.Forms.DockStyle.Fill;
            this._BtC.Location = new System.Drawing.Point(55, 1);
            this._BtC.Margin = new System.Windows.Forms.Padding(1);
            this._BtC.Name = "_BtC";
            this._BtC.Size = new System.Drawing.Size(25, 32);
            this._BtC.TabIndex = 2;
            this._BtC.Text = "C";
            this._BtC.Click += new System.EventHandler(this._BtAnyCode_Click);
            // 
            // _BtD
            // 
            this._BtD.Dock = System.Windows.Forms.DockStyle.Fill;
            this._BtD.Location = new System.Drawing.Point(82, 1);
            this._BtD.Margin = new System.Windows.Forms.Padding(1);
            this._BtD.Name = "_BtD";
            this._BtD.Size = new System.Drawing.Size(25, 32);
            this._BtD.TabIndex = 3;
            this._BtD.Text = "D";
            this._BtD.Click += new System.EventHandler(this._BtAnyCode_Click);
            // 
            // _BtE
            // 
            this._BtE.Dock = System.Windows.Forms.DockStyle.Fill;
            this._BtE.Location = new System.Drawing.Point(109, 1);
            this._BtE.Margin = new System.Windows.Forms.Padding(1);
            this._BtE.Name = "_BtE";
            this._BtE.Size = new System.Drawing.Size(25, 32);
            this._BtE.TabIndex = 4;
            this._BtE.Text = "E";
            this._BtE.Click += new System.EventHandler(this._BtAnyCode_Click);
            // 
            // _BtF
            // 
            this._BtF.Dock = System.Windows.Forms.DockStyle.Fill;
            this._BtF.Location = new System.Drawing.Point(136, 1);
            this._BtF.Margin = new System.Windows.Forms.Padding(1);
            this._BtF.Name = "_BtF";
            this._BtF.Size = new System.Drawing.Size(25, 32);
            this._BtF.TabIndex = 5;
            this._BtF.Text = "F";
            this._BtF.Click += new System.EventHandler(this._BtAnyCode_Click);
            // 
            // _BtG
            // 
            this._BtG.Dock = System.Windows.Forms.DockStyle.Fill;
            this._BtG.Location = new System.Drawing.Point(163, 1);
            this._BtG.Margin = new System.Windows.Forms.Padding(1);
            this._BtG.Name = "_BtG";
            this._BtG.Size = new System.Drawing.Size(25, 32);
            this._BtG.TabIndex = 6;
            this._BtG.Text = "G";
            this._BtG.Click += new System.EventHandler(this._BtAnyCode_Click);
            // 
            // _BtH
            // 
            this._BtH.Dock = System.Windows.Forms.DockStyle.Fill;
            this._BtH.Location = new System.Drawing.Point(190, 1);
            this._BtH.Margin = new System.Windows.Forms.Padding(1);
            this._BtH.Name = "_BtH";
            this._BtH.Size = new System.Drawing.Size(25, 32);
            this._BtH.TabIndex = 7;
            this._BtH.Text = "H";
            this._BtH.Click += new System.EventHandler(this._BtAnyCode_Click);
            // 
            // _BtJ
            // 
            this._BtJ.Dock = System.Windows.Forms.DockStyle.Fill;
            this._BtJ.Location = new System.Drawing.Point(217, 1);
            this._BtJ.Margin = new System.Windows.Forms.Padding(1);
            this._BtJ.Name = "_BtJ";
            this._BtJ.Size = new System.Drawing.Size(25, 32);
            this._BtJ.TabIndex = 8;
            this._BtJ.Text = "J";
            this._BtJ.Click += new System.EventHandler(this._BtAnyCode_Click);
            // 
            // _BtK
            // 
            this._BtK.Dock = System.Windows.Forms.DockStyle.Fill;
            this._BtK.Location = new System.Drawing.Point(244, 1);
            this._BtK.Margin = new System.Windows.Forms.Padding(1);
            this._BtK.Name = "_BtK";
            this._BtK.Size = new System.Drawing.Size(25, 32);
            this._BtK.TabIndex = 9;
            this._BtK.Text = "K";
            this._BtK.Click += new System.EventHandler(this._BtAnyCode_Click);
            // 
            // _BtL
            // 
            this._BtL.Dock = System.Windows.Forms.DockStyle.Fill;
            this._BtL.Location = new System.Drawing.Point(271, 1);
            this._BtL.Margin = new System.Windows.Forms.Padding(1);
            this._BtL.Name = "_BtL";
            this._BtL.Size = new System.Drawing.Size(25, 32);
            this._BtL.TabIndex = 10;
            this._BtL.Text = "L";
            this._BtL.Click += new System.EventHandler(this._BtAnyCode_Click);
            // 
            // _BtM
            // 
            this._BtM.Dock = System.Windows.Forms.DockStyle.Fill;
            this._BtM.Location = new System.Drawing.Point(298, 1);
            this._BtM.Margin = new System.Windows.Forms.Padding(1);
            this._BtM.Name = "_BtM";
            this._BtM.Size = new System.Drawing.Size(25, 32);
            this._BtM.TabIndex = 11;
            this._BtM.Text = "M";
            this._BtM.Click += new System.EventHandler(this._BtAnyCode_Click);
            // 
            // _BtP
            // 
            this._BtP.Dock = System.Windows.Forms.DockStyle.Fill;
            this._BtP.Location = new System.Drawing.Point(325, 1);
            this._BtP.Margin = new System.Windows.Forms.Padding(1);
            this._BtP.Name = "_BtP";
            this._BtP.Size = new System.Drawing.Size(25, 32);
            this._BtP.TabIndex = 12;
            this._BtP.Text = "P";
            this._BtP.Click += new System.EventHandler(this._BtAnyCode_Click);
            // 
            // _BtQ
            // 
            this._BtQ.Dock = System.Windows.Forms.DockStyle.Fill;
            this._BtQ.Location = new System.Drawing.Point(352, 1);
            this._BtQ.Margin = new System.Windows.Forms.Padding(1);
            this._BtQ.Name = "_BtQ";
            this._BtQ.Size = new System.Drawing.Size(25, 32);
            this._BtQ.TabIndex = 13;
            this._BtQ.Text = "Q";
            this._BtQ.Click += new System.EventHandler(this._BtAnyCode_Click);
            // 
            // _BtR
            // 
            this._BtR.Dock = System.Windows.Forms.DockStyle.Fill;
            this._BtR.Location = new System.Drawing.Point(379, 1);
            this._BtR.Margin = new System.Windows.Forms.Padding(1);
            this._BtR.Name = "_BtR";
            this._BtR.Size = new System.Drawing.Size(25, 32);
            this._BtR.TabIndex = 14;
            this._BtR.Text = "R";
            this._BtR.Click += new System.EventHandler(this._BtAnyCode_Click);
            // 
            // _BtS
            // 
            this._BtS.Dock = System.Windows.Forms.DockStyle.Fill;
            this._BtS.Location = new System.Drawing.Point(406, 1);
            this._BtS.Margin = new System.Windows.Forms.Padding(1);
            this._BtS.Name = "_BtS";
            this._BtS.Size = new System.Drawing.Size(34, 32);
            this._BtS.TabIndex = 15;
            this._BtS.Text = "S";
            this._BtS.Click += new System.EventHandler(this._BtAnyCode_Click);
            // 
            // _BtBorrar
            // 
            this._BtBorrar.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._BtBorrar.Enabled = false;
            this._BtBorrar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._BtBorrar.Location = new System.Drawing.Point(358, 2);
            this._BtBorrar.Margin = new System.Windows.Forms.Padding(2);
            this._BtBorrar.Name = "_BtBorrar";
            this._BtBorrar.Size = new System.Drawing.Size(87, 30);
            this._BtBorrar.TabIndex = 7;
            this._BtBorrar.Text = "<<";
            this._BtBorrar.Visible = false;
            this._BtBorrar.Click += new System.EventHandler(this._BtBorrar_Click);
            // 
            // _BtEnviar
            // 
            this._BtEnviar.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._BtEnviar.Enabled = false;
            this._BtEnviar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._BtEnviar.Location = new System.Drawing.Point(358, 36);
            this._BtEnviar.Margin = new System.Windows.Forms.Padding(2);
            this._BtEnviar.Name = "_BtEnviar";
            this._BtEnviar.Size = new System.Drawing.Size(87, 30);
            this._BtEnviar.TabIndex = 8;
            this._BtEnviar.Text = "Enviar";
            this._BtEnviar.Visible = false;
            this._BtEnviar.Click += new System.EventHandler(this._BtEnviar_Click);
            // 
            // _TBDisplayCode
            // 
            this._TBDisplayCode.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._TBDisplayCode.Location = new System.Drawing.Point(270, 37);
            this._TBDisplayCode.Name = "_TBDisplayCode";
            this._TBDisplayCode.ReadOnly = true;
            this._TBDisplayCode.Size = new System.Drawing.Size(83, 20);
            this._TBDisplayCode.TabIndex = 9;
            this._TBDisplayCode.Visible = false;
            this._TBDisplayCode.TextChanged += new System.EventHandler(this._TBDisplayCode_TextChanged);
            // 
            // HfView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "HfView";
            this.Size = new System.Drawing.Size(447, 102);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Model.Module.UI.HMIButton _SelCalButton;
        private System.Windows.Forms.TextBox _TBDisplayMessages;
        private Model.Module.UI.HMIButton _BtMem1;
        private Model.Module.UI.HMIButton _BtMem2;
        private Model.Module.UI.HMIButton _BtMem3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private Model.Module.UI.HMIButton _BtA;
        private Model.Module.UI.HMIButton _BtB;
        private Model.Module.UI.HMIButton _BtC;
        private Model.Module.UI.HMIButton _BtD;
        private Model.Module.UI.HMIButton _BtE;
        private Model.Module.UI.HMIButton _BtF;
        private Model.Module.UI.HMIButton _BtG;
        private Model.Module.UI.HMIButton _BtH;
        private Model.Module.UI.HMIButton _BtJ;
        private Model.Module.UI.HMIButton _BtK;
        private Model.Module.UI.HMIButton _BtL;
        private Model.Module.UI.HMIButton _BtM;
        private Model.Module.UI.HMIButton _BtP;
        private Model.Module.UI.HMIButton _BtQ;
        private Model.Module.UI.HMIButton _BtR;
        private Model.Module.UI.HMIButton _BtS;
        private Model.Module.UI.HMIButton _BtBorrar;
        private Model.Module.UI.HMIButton _BtEnviar;
        private System.Windows.Forms.TextBox _TBDisplayCode;
    }
}
