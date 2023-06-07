
namespace HMI.Presentation.Twr.UI
{
    partial class ListaDeParticipantes
    {
        /// <summary> 
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
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
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.uiTimer1 = new HMI.Model.Module.BusinessEntities.UiTimer();
            this.IdConferencia = new System.Windows.Forms.TextBox();
            this.Todos = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.uiTimer1)).BeginInit();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(15, 26);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(116, 186);
            this.listBox1.TabIndex = 0;
            // 
            // uiTimer1
            // 
            this.uiTimer1.Enabled = true;
            this.uiTimer1.Interval = 1000D;
            this.uiTimer1.SynchronizingObject = this;
            this.uiTimer1.Elapsed += new System.Timers.ElapsedEventHandler(this.uiTimer1_Elapsed);
            // 
            // IdConferencia
            // 
            this.IdConferencia.BackColor = System.Drawing.SystemColors.ControlLight;
            this.IdConferencia.Dock = System.Windows.Forms.DockStyle.Top;
            this.IdConferencia.Location = new System.Drawing.Point(0, 0);
            this.IdConferencia.Name = "IdConferencia";
            this.IdConferencia.Size = new System.Drawing.Size(146, 20);
            this.IdConferencia.TabIndex = 1;
            // 
            // Todos
            // 
            this.Todos.Appearance = System.Windows.Forms.Appearance.Button;
            this.Todos.AutoSize = true;
            this.Todos.BackColor = System.Drawing.SystemColors.ControlLight;
            this.Todos.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.Todos.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.Todos.Location = new System.Drawing.Point(0, 218);
            this.Todos.Name = "Todos";
            this.Todos.Size = new System.Drawing.Size(146, 23);
            this.Todos.TabIndex = 3;
            this.Todos.Text = "Configuración";
            this.Todos.UseVisualStyleBackColor = false;
            this.Todos.CheckedChanged += new System.EventHandler(this.Todos_CheckedChanged);
            this.Todos.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Todos_MouseUp);
            // 
            // button1
            // 
            this.button1.Dock = System.Windows.Forms.DockStyle.Left;
            this.button1.Location = new System.Drawing.Point(0, 20);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(10, 198);
            this.button1.TabIndex = 4;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Dock = System.Windows.Forms.DockStyle.Right;
            this.button2.Location = new System.Drawing.Point(136, 20);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(10, 198);
            this.button2.TabIndex = 5;
            this.button2.Text = "button2";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // ListaDeParticipantes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.Todos);
            this.Controls.Add(this.IdConferencia);
            this.Controls.Add(this.listBox1);
            this.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.Name = "ListaDeParticipantes";
            this.Size = new System.Drawing.Size(146, 241);
            ((System.ComponentModel.ISupportInitialize)(this.uiTimer1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBox1;
        private Model.Module.BusinessEntities.UiTimer uiTimer1;
        private System.Windows.Forms.TextBox IdConferencia;
        private System.Windows.Forms.CheckBox Todos;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
    }
}
