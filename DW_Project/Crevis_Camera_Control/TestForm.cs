/// 프로젝트명 : 동우아펙스 크래비스 카메라 제어 프로그램
/// 개발 기간 : 2021.10.07~
/// 개발업체 : 시그널시스템
/// 개발자 : 안용무
/// 추가 라이브러리 : OpenCVSharp(버전 4.5.3.20210817)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using SSCameraLibrary;

namespace Crevis_Camera_Control
{
    public partial class TestForm : Form
    {
        #region 변수
        //
        public int screen_width;
        public int screen_height;

        private bool m_bInitial = true;

        private Camera colorCam;
        private string device_id;

        private CameraParameter.PixelFormat pixelFormat;

        // 카메라 파라미터(PixelFormat)
        public class CameraParameter
        {
            public enum PixelFormat
            {
                Mono8=0,
                Mono10=1,
                Mono12=2,
                Mono10Packed=3,
                Mono12Packed=4,
                BayerRG8=5,
                BayerRG10=6,
                BayerRG12=7,
                BayerRG10Packed=8,
                BayerRG12Packed=9,
                YUV422Packed=10
            }
        }
        
        #endregion

        public TestForm()
        {
            InitializeComponent();
            Initialize_Display();

        }

        // 폼이 생성 후 실행 이벤트
        private void InitialForm_Shown(object sender, EventArgs e)
        {
            // 초기 화면 실행 후 설정 값만큼 딜레이 후 메인 화면 전환
            //// 타이머 동작 진행바 동작 테스트- 21.10.08
            timerprogress.Start();

            pixelFormat=Check_Cam_PixelFormat();
        }

        #region 화면 출력
        // 디스플레이 초기화 프로세스 - 진행바와 초기 패널 배치
        public void Initialize_Display()
        {
            screen_width = Screen.PrimaryScreen.Bounds.Width;
            screen_height = Screen.PrimaryScreen.Bounds.Height;

            // 진행바, 진행상태 표시 위치 설정
            int barstart_X = screen_width / 2 - this.pStartBar.Width / 2;
            int barstart_Y = screen_height - 400;
            this.pStartBar.Location = new Point(barstart_X, barstart_Y);
            int label_X = screen_width / 2 - 100;
            int label_Y = screen_height - 300;
            this.labelProgress.Location = new Point(label_X, label_Y);

            // 초기패널 설정
            this.InitialPanel.Size = new Size(screen_width, screen_height);
            this.InitialPanel.Location = new Point(0, 0);
            this.InitialPanel.BackgroundImage = Properties.Resources.Initial;

        }

        // 메인 화면 표시
        public void Show_Main_Display()
        {
            // 초기 실행 도구 숨기기
            this.InitialPanel.Visible = false;
            this.pStartBar.Visible = false;
            this.labelProgress.Visible = false;

            // 새로운 도구 보이기
            this.MainPanel.Location = new Point(0, 0);
            this.MainPanel.Size = new Size(screen_width, screen_height);
            this.MainPanel.Visible = true;
        }
        #endregion

        // 카메라 영상 출력 전 카메라 설정 픽셀 포맷 확인
        private CameraParameter.PixelFormat Check_Cam_PixelFormat()
        {
            CameraParameter.PixelFormat format= CameraParameter.PixelFormat.Mono8;
            //Camera
            colorCam = new Camera(0);
            device_id = colorCam._DeviceID;
            //pixel_format = colorCam._PixelFormat;

            //switch (pixel_format)
            //{
            //    case "Mono8":
            //        format = CameraParameter.PixelFormat.Mono8;
            //        break;
            //    case "Mono10":
            //        format = CameraParameter.PixelFormat.Mono10;
            //        break;
            //    case "Mono12":
            //        format = CameraParameter.PixelFormat.Mono12;
            //        break;
            //    case "Mono10Packed":
            //        format = CameraParameter.PixelFormat.Mono10Packed;
            //        break;
            //    case "Mono12Packed":
            //        format = CameraParameter.PixelFormat.Mono12Packed;
            //        break;
            //    case "BayerRG8":
            //        format = CameraParameter.PixelFormat.BayerRG8;
            //        break;
            //    case "BayerRG10":
            //        format = CameraParameter.PixelFormat.BayerRG10;
            //        break;
            //    case "BayerRG12":
            //        format = CameraParameter.PixelFormat.BayerRG12;
            //        break;
            //    case "BayerRG10Packed":
            //        format = CameraParameter.PixelFormat.BayerRG10Packed;
            //        break;
            //    case "BayerRG12Packed":
            //        format = CameraParameter.PixelFormat.BayerRG12Packed;
            //        break;
            //    case "YUV422Packed":
            //        format = CameraParameter.PixelFormat.YUV422Packed;
            //        break;

            //}

            return format;
        }

        

        // 시스템 딜레이 기능
        public void System_Sleep(int delay_time)
        {
            // 초기 화면에서 메인 화면으로 전환을 위해 2초 딜레이
            Thread.Sleep(delay_time);

        }

        // 타이머를 이용 ProgressBar 동작 
        private void timerProgress_Tick(object sender, EventArgs e)
        {
            //if(m_bInitial)
            //{
            //    pStartBar.PerformStep();
            //    if (pStartBar.Value <100)
            //        labelProgress.Text = string.Format("Loading... {0}%", pStartBar.Value);
            //    else if(pStartBar.Value==100)
            //    {   labelProgress.Text = string.Format("Done..."); }
            //    else
            //    { m_bInitial = false; }
            //}

            //else
            //{ timerprogress.Stop(); System_Sleep(500); Show_Main_Display(); }
        }
    }
}
