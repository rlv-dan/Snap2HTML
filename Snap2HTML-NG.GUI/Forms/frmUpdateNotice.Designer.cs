namespace Snap2HTMLNG.Forms
{
    partial class frmUpdateNotice
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
            this.lblNewRelease = new System.Windows.Forms.Label();
            this.lblCurrentVersionNotice = new System.Windows.Forms.Label();
            this.txtReleaseInformation = new System.Windows.Forms.TextBox();
            this.btnDownload = new System.Windows.Forms.Button();
            this.lblReleaseDate = new System.Windows.Forms.Label();
            this.lblReleaseDateValue = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblNewRelease
            // 
            this.lblNewRelease.AutoSize = true;
            this.lblNewRelease.Font = new System.Drawing.Font("Calibri", 22.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNewRelease.Location = new System.Drawing.Point(20, 20);
            this.lblNewRelease.Name = "lblNewRelease";
            this.lblNewRelease.Size = new System.Drawing.Size(366, 45);
            this.lblNewRelease.TabIndex = 0;
            this.lblNewRelease.Text = "New Release Available";
            // 
            // lblCurrentVersionNotice
            // 
            this.lblCurrentVersionNotice.AutoSize = true;
            this.lblCurrentVersionNotice.Font = new System.Drawing.Font("Calibri", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCurrentVersionNotice.Location = new System.Drawing.Point(25, 65);
            this.lblCurrentVersionNotice.Name = "lblCurrentVersionNotice";
            this.lblCurrentVersionNotice.Size = new System.Drawing.Size(344, 22);
            this.lblCurrentVersionNotice.TabIndex = 2;
            this.lblCurrentVersionNotice.Text = "You are on version {0}, the latest version is {0}";
            // 
            // txtReleaseInformation
            // 
            this.txtReleaseInformation.Location = new System.Drawing.Point(29, 126);
            this.txtReleaseInformation.Multiline = true;
            this.txtReleaseInformation.Name = "txtReleaseInformation";
            this.txtReleaseInformation.ReadOnly = true;
            this.txtReleaseInformation.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtReleaseInformation.Size = new System.Drawing.Size(521, 205);
            this.txtReleaseInformation.TabIndex = 3;
            this.txtReleaseInformation.TabStop = false;
            // 
            // btnDownload
            // 
            this.btnDownload.Location = new System.Drawing.Point(428, 337);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(122, 23);
            this.btnDownload.TabIndex = 4;
            this.btnDownload.Text = "Open Release";
            this.btnDownload.UseVisualStyleBackColor = true;
            this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
            // 
            // lblReleaseDate
            // 
            this.lblReleaseDate.AutoSize = true;
            this.lblReleaseDate.Font = new System.Drawing.Font("Calibri", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblReleaseDate.Location = new System.Drawing.Point(25, 87);
            this.lblReleaseDate.Name = "lblReleaseDate";
            this.lblReleaseDate.Size = new System.Drawing.Size(111, 22);
            this.lblReleaseDate.TabIndex = 5;
            this.lblReleaseDate.Text = "Release Date:";
            // 
            // lblReleaseDateValue
            // 
            this.lblReleaseDateValue.AutoSize = true;
            this.lblReleaseDateValue.Font = new System.Drawing.Font("Calibri", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblReleaseDateValue.Location = new System.Drawing.Point(142, 87);
            this.lblReleaseDateValue.Name = "lblReleaseDateValue";
            this.lblReleaseDateValue.Size = new System.Drawing.Size(121, 22);
            this.lblReleaseDateValue.TabIndex = 6;
            this.lblReleaseDateValue.Text = "{YYYY-MM-DD}";
            // 
            // frmUpdateNotice
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(579, 372);
            this.Controls.Add(this.lblReleaseDateValue);
            this.Controls.Add(this.lblReleaseDate);
            this.Controls.Add(this.btnDownload);
            this.Controls.Add(this.txtReleaseInformation);
            this.Controls.Add(this.lblCurrentVersionNotice);
            this.Controls.Add(this.lblNewRelease);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "frmUpdateNotice";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Update Notice";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblNewRelease;
        private System.Windows.Forms.Label lblCurrentVersionNotice;
        private System.Windows.Forms.TextBox txtReleaseInformation;
        private System.Windows.Forms.Button btnDownload;
        private System.Windows.Forms.Label lblReleaseDate;
        private System.Windows.Forms.Label lblReleaseDateValue;
    }
}