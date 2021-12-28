
namespace Crevis_Camera_Control
{
    partial class DeviceForm
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.cb_ExpAuto = new System.Windows.Forms.CheckBox();
            this.numUD_Exp = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cb_GainAuto = new System.Windows.Forms.CheckBox();
            this.numUD_Gain = new System.Windows.Forms.NumericUpDown();
            this.trackB_Gain = new System.Windows.Forms.TrackBar();
            this.trackB_Exp = new System.Windows.Forms.TrackBar();
            this.label3 = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.trackB_Target = new System.Windows.Forms.TrackBar();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.label4 = new System.Windows.Forms.Label();
            this.cB_Trigger = new System.Windows.Forms.CheckBox();
            this.btn_SoftTrigger = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numUD_Exp)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numUD_Gain)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackB_Gain)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackB_Exp)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackB_Target)).BeginInit();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.tabControl1.Location = new System.Drawing.Point(4, 13);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(468, 436);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.Transparent;
            this.tabPage1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(460, 410);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Color";
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.Color.Transparent;
            this.tabPage2.Controls.Add(this.trackB_Target);
            this.tabPage2.Controls.Add(this.numericUpDown1);
            this.tabPage2.Controls.Add(this.label3);
            this.tabPage2.Controls.Add(this.trackB_Exp);
            this.tabPage2.Controls.Add(this.trackB_Gain);
            this.tabPage2.Controls.Add(this.cb_GainAuto);
            this.tabPage2.Controls.Add(this.numUD_Gain);
            this.tabPage2.Controls.Add(this.label2);
            this.tabPage2.Controls.Add(this.cb_ExpAuto);
            this.tabPage2.Controls.Add(this.numUD_Exp);
            this.tabPage2.Controls.Add(this.label1);
            this.tabPage2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(460, 410);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Exposure";
            // 
            // cb_ExpAuto
            // 
            this.cb_ExpAuto.AutoSize = true;
            this.cb_ExpAuto.Location = new System.Drawing.Point(356, 120);
            this.cb_ExpAuto.Name = "cb_ExpAuto";
            this.cb_ExpAuto.Size = new System.Drawing.Size(56, 21);
            this.cb_ExpAuto.TabIndex = 2;
            this.cb_ExpAuto.Text = "Auto";
            this.cb_ExpAuto.UseVisualStyleBackColor = true;
            // 
            // numUD_Exp
            // 
            this.numUD_Exp.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numUD_Exp.Location = new System.Drawing.Point(249, 120);
            this.numUD_Exp.Maximum = new decimal(new int[] {
            40,
            0,
            0,
            0});
            this.numUD_Exp.Name = "numUD_Exp";
            this.numUD_Exp.Size = new System.Drawing.Size(87, 23);
            this.numUD_Exp.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label1.Location = new System.Drawing.Point(30, 120);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Exposure";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label2.Location = new System.Drawing.Point(30, 75);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 17);
            this.label2.TabIndex = 3;
            this.label2.Text = "Gain";
            // 
            // cb_GainAuto
            // 
            this.cb_GainAuto.AutoSize = true;
            this.cb_GainAuto.Location = new System.Drawing.Point(356, 75);
            this.cb_GainAuto.Name = "cb_GainAuto";
            this.cb_GainAuto.Size = new System.Drawing.Size(56, 21);
            this.cb_GainAuto.TabIndex = 5;
            this.cb_GainAuto.Text = "Auto";
            this.cb_GainAuto.UseVisualStyleBackColor = true;
            // 
            // numUD_Gain
            // 
            this.numUD_Gain.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numUD_Gain.Location = new System.Drawing.Point(249, 75);
            this.numUD_Gain.Maximum = new decimal(new int[] {
            399,
            0,
            0,
            131072});
            this.numUD_Gain.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numUD_Gain.Name = "numUD_Gain";
            this.numUD_Gain.Size = new System.Drawing.Size(87, 23);
            this.numUD_Gain.TabIndex = 4;
            this.numUD_Gain.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // trackB_Gain
            // 
            this.trackB_Gain.Location = new System.Drawing.Point(99, 75);
            this.trackB_Gain.Maximum = 399;
            this.trackB_Gain.Minimum = 100;
            this.trackB_Gain.Name = "trackB_Gain";
            this.trackB_Gain.Size = new System.Drawing.Size(125, 45);
            this.trackB_Gain.TabIndex = 7;
            this.trackB_Gain.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackB_Gain.Value = 100;
            // 
            // trackB_Exp
            // 
            this.trackB_Exp.Location = new System.Drawing.Point(99, 120);
            this.trackB_Exp.Maximum = 400;
            this.trackB_Exp.Name = "trackB_Exp";
            this.trackB_Exp.Size = new System.Drawing.Size(125, 45);
            this.trackB_Exp.TabIndex = 8;
            this.trackB_Exp.TickStyle = System.Windows.Forms.TickStyle.None;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label3.Location = new System.Drawing.Point(76, 165);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 17);
            this.label3.TabIndex = 9;
            this.label3.Text = "Auto Target";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(316, 165);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(87, 23);
            this.numericUpDown1.TabIndex = 10;
            // 
            // trackB_Target
            // 
            this.trackB_Target.Location = new System.Drawing.Point(174, 165);
            this.trackB_Target.Maximum = 255;
            this.trackB_Target.Name = "trackB_Target";
            this.trackB_Target.Size = new System.Drawing.Size(125, 45);
            this.trackB_Target.TabIndex = 11;
            this.trackB_Target.TickStyle = System.Windows.Forms.TickStyle.None;
            // 
            // tabPage3
            // 
            this.tabPage3.BackColor = System.Drawing.Color.Transparent;
            this.tabPage3.Controls.Add(this.btn_SoftTrigger);
            this.tabPage3.Controls.Add(this.cB_Trigger);
            this.tabPage3.Controls.Add(this.label4);
            this.tabPage3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(460, 410);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Trigger";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(30, 75);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(54, 17);
            this.label4.TabIndex = 0;
            this.label4.Text = "Trigger";
            // 
            // cB_Trigger
            // 
            this.cB_Trigger.AutoSize = true;
            this.cB_Trigger.Location = new System.Drawing.Point(113, 75);
            this.cB_Trigger.Name = "cB_Trigger";
            this.cB_Trigger.Size = new System.Drawing.Size(71, 21);
            this.cB_Trigger.TabIndex = 1;
            this.cB_Trigger.Text = "Enable";
            this.cB_Trigger.UseVisualStyleBackColor = true;
            // 
            // btn_SoftTrigger
            // 
            this.btn_SoftTrigger.Location = new System.Drawing.Point(52, 112);
            this.btn_SoftTrigger.Name = "btn_SoftTrigger";
            this.btn_SoftTrigger.Size = new System.Drawing.Size(132, 26);
            this.btn_SoftTrigger.TabIndex = 2;
            this.btn_SoftTrigger.Text = "Software Trigger";
            this.btn_SoftTrigger.UseVisualStyleBackColor = true;
            // 
            // DeviceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 461);
            this.Controls.Add(this.tabControl1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "DeviceForm";
            this.Text = "Device Properties";
            this.tabControl1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numUD_Exp)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numUD_Gain)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackB_Gain)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackB_Exp)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackB_Target)).EndInit();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cb_ExpAuto;
        private System.Windows.Forms.NumericUpDown numUD_Exp;
        private System.Windows.Forms.CheckBox cb_GainAuto;
        private System.Windows.Forms.NumericUpDown numUD_Gain;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TrackBar trackB_Target;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TrackBar trackB_Exp;
        private System.Windows.Forms.TrackBar trackB_Gain;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Button btn_SoftTrigger;
        private System.Windows.Forms.CheckBox cB_Trigger;
        private System.Windows.Forms.Label label4;
    }
}