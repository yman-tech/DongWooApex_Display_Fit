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
    public partial class PassForm : Form
    {
        private string InputString = "";
        public string SetPassword;

        // MainForm에 데이터 전달을 위한 델리게이트 선언
        public delegate void PasswordResultDataSendHandler(bool result);

        public event PasswordResultDataSendHandler PasswordFormEvent;

        public PassForm()
        {
            InitializeComponent();
        }

        #region 패스워드 입력 버튼 이벤트
        private void button10_Click(object sender, EventArgs e)
        {
            InputString += "0";
            maskedTextBox1.Text = InputString;
     
        }

        private void button7_Click(object sender, EventArgs e)
        {
            InputString += "1";
            maskedTextBox1.Text = InputString;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            InputString += "2";
            maskedTextBox1.Text = InputString;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            InputString += "3";
            maskedTextBox1.Text = InputString;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            InputString += "4";
            maskedTextBox1.Text = InputString;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            InputString += "5";
            maskedTextBox1.Text = InputString;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            InputString += "6";
            maskedTextBox1.Text = InputString;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            InputString += "7";
            maskedTextBox1.Text = InputString;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            InputString += "8";
            maskedTextBox1.Text = InputString;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            InputString += "9";
            maskedTextBox1.Text = InputString;
        }

        private void button13_Click(object sender, EventArgs e)
        {
            var textLength = InputString.Length;
            if (textLength > 0)
            {
                InputString = InputString.Substring(0, textLength - 1);
                maskedTextBox1.Text = InputString;
            }
        }

        //private void button12_Click(object sender, EventArgs e)
        //{
        //    var textLength = InputString.Length;
        //    if(textLength>0)
        //    {
        //        InputString = InputString.Substring(0, textLength - 1);
        //        maskedTextBox1.Text = InputString;
        //    }
        //}

        private void button11_Click(object sender, EventArgs e)
        {
            InputString = "";
            maskedTextBox1.Text = InputString;
        }
        #endregion


        private void btnYes_Click(object sender, EventArgs e)
        {
            string pass = SetPassword;
            if (pass == InputString)
            {
                this.PasswordFormEvent(true);
                this.Close();
            }
            else
            {
                InputString = "";
                maskedTextBox1.Text = InputString;
                MessageBox.Show("Do not match Password");

            }
        }

        private void btnNo_Click(object sender, EventArgs e)
        {
            this.PasswordFormEvent(false);
            this.Close();

        }

        
    }
}
