using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Crevis_Camera_Control
{
    

    public partial class InitialForm : Form
    {
        private int screen_width;
        private int screen_height;
        private int dot;

        public InitialForm()
        {
            InitializeComponent();

            screen_width = Screen.PrimaryScreen.Bounds.Width;
            screen_height = Screen.PrimaryScreen.Bounds.Height;

            // 진행바, 진행상태 표시 위치 설정
            int barstart_X = /*screen_width / 2 - this.progressBar1.Width / 2*/200;
            int barstart_Y = /*screen_height - 400*/200;
            this.progressBar1.Location = new Point(barstart_X, barstart_Y);

            int label_X = /*screen_width / 2 - 100*/250;
            int label_Y = /*screen_height - 300*/250;
            this.label1.Location = new Point(label_X, label_Y);

            dot = 0;
        }

        public void Update_Progress_Value(int value)
        {
            label1.Text = string.Format("Progress : {0}%", value);
            progressBar1.Value = value;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            string title = "Program Loading....";
            Text = title.Substring(0, title.Length - 4 + (dot++ % 5));
            
        }
    }
}
