namespace Crevis_Camera_Control
{
    partial class TestForm
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pStartBar = new System.Windows.Forms.ProgressBar();
            this.MainPanel = new System.Windows.Forms.Panel();
            this.InitialPanel = new System.Windows.Forms.Panel();
            this.labelProgress = new System.Windows.Forms.Label();
            this.timerprogress = new System.Windows.Forms.Timer(this.components);
            this.InitialPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // pStartBar
            // 
            this.pStartBar.Location = new System.Drawing.Point(354, 328);
            this.pStartBar.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.pStartBar.Maximum = 101;
            this.pStartBar.Name = "pStartBar";
            this.pStartBar.Size = new System.Drawing.Size(800, 60);
            this.pStartBar.Step = 1;
            this.pStartBar.TabIndex = 3;
            // 
            // MainPanel
            // 
            this.MainPanel.Location = new System.Drawing.Point(0, 0);
            this.MainPanel.Name = "MainPanel";
            this.MainPanel.Size = new System.Drawing.Size(1600, 900);
            this.MainPanel.TabIndex = 5;
            this.MainPanel.Visible = false;
            // 
            // InitialPanel
            // 
            this.InitialPanel.BackColor = System.Drawing.Color.SlateGray;
            this.InitialPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.InitialPanel.Controls.Add(this.pStartBar);
            this.InitialPanel.Controls.Add(this.labelProgress);
            this.InitialPanel.Location = new System.Drawing.Point(0, 0);
            this.InitialPanel.Name = "InitialPanel";
            this.InitialPanel.Size = new System.Drawing.Size(1600, 900);
            this.InitialPanel.TabIndex = 2;
            // 
            // labelProgress
            // 
            this.labelProgress.AutoSize = true;
            this.labelProgress.Font = new System.Drawing.Font("Microsoft Sans Serif", 28F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelProgress.ForeColor = System.Drawing.Color.Red;
            this.labelProgress.Location = new System.Drawing.Point(710, 414);
            this.labelProgress.Name = "labelProgress";
            this.labelProgress.Size = new System.Drawing.Size(0, 44);
            this.labelProgress.TabIndex = 0;
            // 
            // timerprogress
            // 
            this.timerprogress.Interval = 50;
            this.timerprogress.Tick += new System.EventHandler(this.timerProgress_Tick);
            // 
            // InitialForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1600, 900);
            this.Controls.Add(this.MainPanel);
            this.Controls.Add(this.InitialPanel);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "InitialForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Form1";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Shown += new System.EventHandler(this.InitialForm_Shown);
            this.InitialPanel.ResumeLayout(false);
            this.InitialPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel InitialPanel;
        private System.Windows.Forms.ProgressBar pStartBar;
        private System.Windows.Forms.Panel MainPanel;
        private System.Windows.Forms.Timer timerprogress;
        private System.Windows.Forms.Label labelProgress;
    }
}

