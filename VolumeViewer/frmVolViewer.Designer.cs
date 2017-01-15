namespace VolumeViewer
{
    partial class frmVolViewer
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lvwVolViewer = new System.Windows.Forms.ListView();
            this.cmdRefresh = new System.Windows.Forms.Button();
            this.cmdExit = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lvwVolViewer
            // 
            this.lvwVolViewer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvwVolViewer.Location = new System.Drawing.Point(13, 13);
            this.lvwVolViewer.Name = "lvwVolViewer";
            this.lvwVolViewer.Size = new System.Drawing.Size(549, 304);
            this.lvwVolViewer.TabIndex = 0;
            this.lvwVolViewer.UseCompatibleStateImageBehavior = false;
            this.lvwVolViewer.View = System.Windows.Forms.View.Details;
            // 
            // cmdRefresh
            // 
            this.cmdRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmdRefresh.Location = new System.Drawing.Point(13, 324);
            this.cmdRefresh.Name = "cmdRefresh";
            this.cmdRefresh.Size = new System.Drawing.Size(87, 38);
            this.cmdRefresh.TabIndex = 1;
            this.cmdRefresh.Text = "&Refresh";
            this.cmdRefresh.UseVisualStyleBackColor = true;
            this.cmdRefresh.Click += new System.EventHandler(this.cmdRefresh_Click);
            // 
            // cmdExit
            // 
            this.cmdExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdExit.Location = new System.Drawing.Point(475, 323);
            this.cmdExit.Name = "cmdExit";
            this.cmdExit.Size = new System.Drawing.Size(87, 38);
            this.cmdExit.TabIndex = 2;
            this.cmdExit.Text = "&Exit";
            this.cmdExit.UseVisualStyleBackColor = true;
            // 
            // frmVolViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(574, 374);
            this.Controls.Add(this.cmdExit);
            this.Controls.Add(this.cmdRefresh);
            this.Controls.Add(this.lvwVolViewer);
            this.Name = "frmVolViewer";
            this.Text = "Volume Viewer";
            this.Load += new System.EventHandler(this.frmVolViewer_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lvwVolViewer;
        private System.Windows.Forms.Button cmdRefresh;
        private System.Windows.Forms.Button cmdExit;
    }
}

