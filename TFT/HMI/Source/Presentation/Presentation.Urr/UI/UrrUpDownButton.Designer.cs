namespace HMI.Presentation.Urr.UI
{
    partial class UrrUpDownButton
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this._UrrUpBT = new HMI.Presentation.Urr.UI.IncreaseButton();
            this._UrrMidBT = new HMI.Model.Module.UI.HMIButton();
            this._UrrDownBT = new HMI.Presentation.Urr.UI.DecreaseButton();
            this._UrrLevelBar = new HMI.Presentation.Urr.UI.UrrWidthBar();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this._UrrLevelBar, 0, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 73.52941F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 26.47059F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(113, 67);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this._UrrUpBT);
            this.panel1.Controls.Add(this._UrrMidBT);
            this.panel1.Controls.Add(this._UrrDownBT);
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(107, 43);
            this.panel1.TabIndex = 2;
            // 
            // _UrrUpBT
            // 
            this._UrrUpBT.BackColor = System.Drawing.Color.Gainsboro;
            this._UrrUpBT.BorderColor = System.Drawing.Color.Black;
            this._UrrUpBT.BorderWidth = 1;
            this._UrrUpBT.ButtonShape = HMI.Presentation.Urr.UI.IncreaseButton.ButtonsShapes.RightTriangle;
            this._UrrUpBT.ButtonText = "+";
            this._UrrUpBT.EndColor = System.Drawing.Color.LightGray;
            this._UrrUpBT.FlatAppearance.BorderSize = 0;
            this._UrrUpBT.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this._UrrUpBT.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this._UrrUpBT.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._UrrUpBT.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._UrrUpBT.ForeColor = System.Drawing.Color.Transparent;
            this._UrrUpBT.GradientAngle = 90;
            this._UrrUpBT.Location = new System.Drawing.Point(76, 0);
            this._UrrUpBT.MouseClickColor1 = System.Drawing.Color.White;
            this._UrrUpBT.MouseClickColor2 = System.Drawing.Color.Gainsboro;
            this._UrrUpBT.Name = "_UrrUpBT";
            this._UrrUpBT.ShowButtontext = true;
            this._UrrUpBT.Size = new System.Drawing.Size(28, 42);
            this._UrrUpBT.StartColor = System.Drawing.Color.WhiteSmoke;
            this._UrrUpBT.TabIndex = 2;
            this._UrrUpBT.Text = "increaseButton1";
            this._UrrUpBT.TextLocation_X = 2;
            this._UrrUpBT.TextLocation_Y = 9;
            this._UrrUpBT.Transparent1 = 250;
            this._UrrUpBT.Transparent2 = 250;
            this._UrrUpBT.UseVisualStyleBackColor = false;
            this._UrrUpBT.Click += new System.EventHandler(this._UpButton_Click);
            // 
            // _UrrMidBT
            // 
            this._UrrMidBT.ButtonColorDisabled = System.Drawing.Color.Gainsboro;
            this._UrrMidBT.Enabled = false;
            this._UrrMidBT.InnerBorderColorDisabled = System.Drawing.Color.LightGray;
            this._UrrMidBT.Location = new System.Drawing.Point(37, 2);
            this._UrrMidBT.Name = "_UrrMidBT";
            this._UrrMidBT.Size = new System.Drawing.Size(33, 38);
            this._UrrMidBT.TabIndex = 1;
            // 
            // _UrrDownBT
            // 
            this._UrrDownBT.BackColor = System.Drawing.Color.Gainsboro;
            this._UrrDownBT.BorderColor = System.Drawing.Color.Black;
            this._UrrDownBT.BorderWidth = 1;
            this._UrrDownBT.ButtonShape = HMI.Presentation.Urr.UI.DecreaseButton.ButtonsShapes.LeftTriangle;
            this._UrrDownBT.ButtonText = "-";
            this._UrrDownBT.EndColor = System.Drawing.Color.LightGray;
            this._UrrDownBT.FlatAppearance.BorderSize = 0;
            this._UrrDownBT.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this._UrrDownBT.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this._UrrDownBT.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._UrrDownBT.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._UrrDownBT.ForeColor = System.Drawing.Color.Transparent;
            this._UrrDownBT.GradientAngle = 90;
            this._UrrDownBT.Location = new System.Drawing.Point(3, 0);
            this._UrrDownBT.MouseClickColor1 = System.Drawing.Color.White;
            this._UrrDownBT.MouseClickColor2 = System.Drawing.Color.Gainsboro;
            this._UrrDownBT.Name = "_UrrDownBT";
            this._UrrDownBT.ShowButtontext = true;
            this._UrrDownBT.Size = new System.Drawing.Size(28, 42);
            this._UrrDownBT.StartColor = System.Drawing.Color.WhiteSmoke;
            this._UrrDownBT.TabIndex = 0;
            this._UrrDownBT.Text = "decreaseButton1";
            this._UrrDownBT.TextLocation_X = 9;
            this._UrrDownBT.TextLocation_Y = 7;
            this._UrrDownBT.Transparent1 = 250;
            this._UrrDownBT.Transparent2 = 250;
            this._UrrDownBT.UseVisualStyleBackColor = false;
            this._UrrDownBT.Click += new System.EventHandler(this._DownButton_Click);
            // 
            // _UrrLevelBar
            // 
            this._UrrLevelBar.BackColor = System.Drawing.Color.Transparent;
            this._UrrLevelBar.BorderColor = System.Drawing.Color.Black;
            this._UrrLevelBar.BorderWidth = 2;
            this._UrrLevelBar.ButtonShape = HMI.Presentation.Urr.UI.UrrWidthBar.ButtonsShapes.Rect;
            this._UrrLevelBar.ButtonText = "";
            this._UrrLevelBar.Enabled = false;
            this._UrrLevelBar.EndColor = System.Drawing.Color.Yellow;
            this._UrrLevelBar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._UrrLevelBar.GradientAngle = 0;
            this._UrrLevelBar.Location = new System.Drawing.Point(3, 52);
            this._UrrLevelBar.Maximum = 100;
            this._UrrLevelBar.Minimum = 0;
            this._UrrLevelBar.MouseClickColor1 = System.Drawing.Color.White;
            this._UrrLevelBar.MouseClickColor2 = System.Drawing.Color.Gainsboro;
            this._UrrLevelBar.MouseHoverColor1 = System.Drawing.Color.Gainsboro;
            this._UrrLevelBar.MouseHoverColor2 = System.Drawing.Color.DarkGray;
            this._UrrLevelBar.Name = "_UrrLevelBar";
            this._UrrLevelBar.ShowButtontext = true;
            this._UrrLevelBar.Size = new System.Drawing.Size(107, 5);
            this._UrrLevelBar.StartColor = System.Drawing.Color.Green;
            this._UrrLevelBar.TabIndex = 3;
            this._UrrLevelBar.TextLocation_X = 34;
            this._UrrLevelBar.TextLocation_Y = 9;
            this._UrrLevelBar.Transparent1 = 250;
            this._UrrLevelBar.Transparent2 = 250;
            this._UrrLevelBar.UseVisualStyleBackColor = false;
            this._UrrLevelBar.Value = 0;
            // 
            // UrrUpDownButton
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "UrrUpDownButton";
            this.Size = new System.Drawing.Size(114, 70);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private IncreaseButton _UrrUpBT;
        private DecreaseButton _UrrDownBT;
        private Model.Module.UI.HMIButton _UrrMidBT;
        private Presentation.Urr.UI.UrrWidthBar _UrrLevelBar;





    }
}
