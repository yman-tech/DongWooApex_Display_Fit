// 초기 화면 출력을 위한 쓰레드 프로세스

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Crevis_Camera_Control
{
    class InitialProcess
    {
        private Thread init_thread;
        private InitialForm init_form;
        private EventWaitHandle init_thread_event;

        private delegate void FormCloseCallback();
        private delegate void UpdateProgressCallback(int value);

        // InitialProcess 생성자
        public InitialProcess()
        {
            init_thread = new Thread(Run_Init);
            init_thread_event = new EventWaitHandle(false, EventResetMode.ManualReset);
        }

        public void Process_Start()
        {
            init_thread.Start();
        }

        public void Process_Stop()
        {
            init_thread_event.WaitOne();
            init_form.Invoke(new FormCloseCallback(init_form.Close));

            init_thread.Join();
        }

        public void Update_Progress(int value)
        {
            init_thread_event.WaitOne();
            init_form.Invoke(new UpdateProgressCallback(init_form.Update_Progress_Value), value);
                
        }
        
        private void Run_Init()
        {
            init_form = new InitialForm();
            init_form.Load += new EventHandler(Form_Load);
            init_form.ShowDialog();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            init_thread_event.Set();
        }

    }
}
