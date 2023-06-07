
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
            ((System.ComponentModel.ISupportInitialize)(this.uiTimer1)).BeginInit();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.BackColor = System.Drawing.Color.LightGray;
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(4, 30);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(95, 134);
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
            this.IdConferencia.Location = new System.Drawing.Point(4, 4);
            this.IdConferencia.Name = "IdConferencia";
            this.IdConferencia.Size = new System.Drawing.Size(95, 20);
            this.IdConferencia.TabIndex = 1;
            // 
            // Todos
            // 
            this.Todos.Appearance = System.Windows.Forms.Appearance.Button;
            this.Todos.AutoSize = true;
            this.Todos.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.Todos.Location = new System.Drawing.Point(4, 170);
            this.Todos.Name = "Todos";
            this.Todos.Size = new System.Drawing.Size(82, 23);
            this.Todos.TabIndex = 3;
            this.Todos.Text = "Configuración";
            this.Todos.UseVisualStyleBackColor = true;
            this.Todos.CheckedChanged += new System.EventHandler(this.Todos_CheckedChanged);
            this.Todos.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Todos_MouseUp);
            // 
            // ListaDeParticipantes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.Controls.Add(this.Todos);
            this.Controls.Add(this.IdConferencia);
            this.Controls.Add(this.listBox1);
            this.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.Name = "ListaDeParticipantes";
            this.Size = new System.Drawing.Size(117, 202);
            ((System.ComponentModel.ISupportInitialize)(this.uiTimer1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBox1;
        private Model.Module.BusinessEntities.UiTimer uiTimer1;
        private System.Windows.Forms.TextBox IdConferencia;
        private System.Windows.Forms.CheckBox Todos;
    }
}
