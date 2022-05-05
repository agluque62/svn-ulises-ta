
namespace HMI.Presentation.Twr.UI
{
    partial class newControlRecord
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
            this.splitUC1 = new HMI.Presentation.Twr.UI.SplitUC();
            this.SuspendLayout();
            // 
            // splitUC1
            // 
            this.splitUC1.Location = new System.Drawing.Point(24, 0);
            this.splitUC1.Margin = new System.Windows.Forms.Padding(0);
            this.splitUC1.Name = "splitUC1";
            this.splitUC1.Size = new System.Drawing.Size(276, 203);
            this.splitUC1.TabIndex = 0;
            // 
            // newControlRecord
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitUC1);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "newControlRecord";
            this.Size = new System.Drawing.Size(501, 231);
            this.ResumeLayout(false);

        }

        #endregion

        private SplitUC splitUC1;
    }
}
