using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using XiboClient.Properties;
namespace XiboClient
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private Label labelPleaseWait;

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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelPleaseWait = new System.Windows.Forms.Label();
            this.labelPleaseWait.AutoSize = true;
            this.labelPleaseWait.Font = new System.Drawing.Font("Microsoft Sans Serif", 27.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPleaseWait.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.labelPleaseWait.Location = new System.Drawing.Point(0, 0);
            this.labelPleaseWait.Size = new System.Drawing.Size(296, 42);
            this.labelPleaseWait.TabIndex = 0;
            this.labelPleaseWait.Text = "PLEASE WAIT...";
            //this.Controls.Add(this.labelPleaseWait);
            //labelPleaseWait.Visible = false;

            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.SuspendLayout();
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ClientSize = new System.Drawing.Size(1024, 768);
            this.DoubleBuffered = global::XiboClient.ApplicationSettings.Default.DoubleBuffering;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            this.Name = "MainForm";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
        }

        #endregion





    }
}

