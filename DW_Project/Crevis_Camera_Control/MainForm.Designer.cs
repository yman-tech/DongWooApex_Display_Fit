
namespace Crevis_Camera_Control
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            this.timerCameraConnection = new System.Windows.Forms.Timer(this.components);
            this.timerCamguideMonitoring = new System.Windows.Forms.Timer(this.components);
            this.timerConnectionUI = new System.Windows.Forms.Timer(this.components);
            this.timerLogDelete = new System.Windows.Forms.Timer(this.components);
            this.timerCamCheck = new System.Windows.Forms.Timer(this.components);
            this.panelCam = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.btnExit = new System.Windows.Forms.Button();
            this.btnRotate = new System.Windows.Forms.Button();
            this.btnSetting = new System.Windows.Forms.Button();
            this.btnAdmin = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panelCam.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // timerCameraConnection
            // 
            this.timerCameraConnection.Interval = 3000;
            this.timerCameraConnection.Tick += new System.EventHandler(this.timerCameraConnection_Tick);
            // 
            // timerCamguideMonitoring
            // 
            this.timerCamguideMonitoring.Interval = 5000;
            this.timerCamguideMonitoring.Tick += new System.EventHandler(this.timerCamguideMonitoring_Tick);
            // 
            // timerConnectionUI
            // 
            this.timerConnectionUI.Interval = 300;
            this.timerConnectionUI.Tick += new System.EventHandler(this.timerConnectionUI_Tick);
            // 
            // timerLogDelete
            // 
            this.timerLogDelete.Interval = 1800000;
            this.timerLogDelete.Tick += new System.EventHandler(this.timerLogDelete_Tick);
            // 
            // timerCamCheck
            // 
            this.timerCamCheck.Tick += new System.EventHandler(this.timerCamCheck_Tick);
            // 
            // panelCam
            // 
            this.panelCam.BackColor = System.Drawing.Color.Yellow;
            this.panelCam.Controls.Add(this.label1);
            this.panelCam.Location = new System.Drawing.Point(1500, 0);
            this.panelCam.Name = "panelCam";
            this.panelCam.Size = new System.Drawing.Size(100, 100);
            this.panelCam.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(100, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 26);
            this.label1.TabIndex = 5;
            // 
            // btnExit
            // 
            this.btnExit.BackgroundImage = global::Crevis_Camera_Control.Properties.Resources.Exit_00;
            this.btnExit.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnExit.Location = new System.Drawing.Point(1500, 716);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(100, 40);
            this.btnExit.TabIndex = 4;
            this.btnExit.TabStop = false;
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // btnRotate
            // 
            this.btnRotate.BackgroundImage = global::Crevis_Camera_Control.Properties.Resources.Origin_00;
            this.btnRotate.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnRotate.Location = new System.Drawing.Point(1500, 484);
            this.btnRotate.Name = "btnRotate";
            this.btnRotate.Size = new System.Drawing.Size(100, 40);
            this.btnRotate.TabIndex = 1;
            this.btnRotate.TabStop = false;
            this.btnRotate.UseVisualStyleBackColor = true;
            this.btnRotate.Click += new System.EventHandler(this.btnRotate_Click);
            // 
            // btnSetting
            // 
            this.btnSetting.BackgroundImage = global::Crevis_Camera_Control.Properties.Resources.Setting_00;
            this.btnSetting.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSetting.Location = new System.Drawing.Point(1500, 582);
            this.btnSetting.Name = "btnSetting";
            this.btnSetting.Size = new System.Drawing.Size(100, 40);
            this.btnSetting.TabIndex = 3;
            this.btnSetting.TabStop = false;
            this.btnSetting.UseVisualStyleBackColor = true;
            this.btnSetting.Click += new System.EventHandler(this.btnSetting_Click);
            // 
            // btnAdmin
            // 
            this.btnAdmin.BackgroundImage = global::Crevis_Camera_Control.Properties.Resources.Operator_00;
            this.btnAdmin.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnAdmin.Location = new System.Drawing.Point(1500, 865);
            this.btnAdmin.Name = "btnAdmin";
            this.btnAdmin.Size = new System.Drawing.Size(100, 40);
            this.btnAdmin.TabIndex = 2;
            this.btnAdmin.TabStop = false;
            this.btnAdmin.UseVisualStyleBackColor = true;
            this.btnAdmin.Click += new System.EventHandler(this.btnAdmin_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Black;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(1500, 900);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BackgroundImage = global::Crevis_Camera_Control.Properties.Resources.Background;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1600, 900);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.panelCam);
            this.Controls.Add(this.btnRotate);
            this.Controls.Add(this.btnSetting);
            this.Controls.Add(this.btnAdmin);
            this.Controls.Add(this.pictureBox1);
            this.Cursor = System.Windows.Forms.Cursors.Default;
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.panelCam.ResumeLayout(false);
            this.panelCam.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button btnAdmin;
        private System.Windows.Forms.Timer timerCameraConnection;
        private System.Windows.Forms.Button btnSetting;
        private System.Windows.Forms.Button btnRotate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panelCam;
        private System.Windows.Forms.Timer timerCamguideMonitoring;
        private System.Windows.Forms.Timer timerLogDelete;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Timer timerCamCheck;
        public System.Windows.Forms.Timer timerConnectionUI;
    }
}