/// 프로젝트명 : 동우아펙스 크래비스 카메라 제어 프로그램
/// 개발 기간 : 2021.10.07~10.29
/// 개발업체 : 시그널시스템
/// 개발자 : 안용무
/// 추가 라이브러리 : OpenCVSharp(버전 4.5.3.20210817)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using SSCameraLibrary;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Security;


namespace Crevis_Camera_Control
{
    // 데이터 바인딩을 위한 INotifyPropertyChanged 구현
    public partial class MainForm : Form
    {
        // 디스플레이 파라미터 클래스
        public class DisplayParameter
        {
            public enum DisplayRatio
            {
                Nine_Ratio=0,
                Ten_Ratio=1
            }
        }

        // 화면 표시를 위한 변수
        //private int diplay_width_ratio;
        //private int display_height_ratio;
        //private int cam_display_width_ratio;
        //private int cam_display_height_ratio;
        private DisplayParameter.DisplayRatio displayRatio;
        private bool cam_connection_display=false;
        //private Bitmap Deep_Bitmap; // 영상 출력을 위한 비트맵 데이터(Camguide 실행 시 발생 문제 해결을 위해 DeepCopy)

        // 시스템 변수
        private string defaultDirectory = "C:\\Crevis_Control";
        private string systemFilePath = "C:\\Crevis_Control\\SystemData\\ProgramSetting.ini";
        private string logPath;
        private int logSaveDuration;
        private bool logFrameCountEnable;
        private string cameraModel;
        private string displayRotation;
        private string systemPassword;
        private bool isAdminMode = false;
        private bool isSettingMode = false;

        // 시스템 Thread 변수
        private Thread RunCam;
        //private Thread RunGrab;
        //private Thread RunDisplay;
        private bool isThreadRunning;

        // 카메라 변수
        private Camera ColorCam;
        private Dictionary<string, string> CamInfo;
        private Camera.CameraParameter.GrabMode GrabMode = Camera.CameraParameter.GrabMode.Hardware;
        private Camera.CameraParameter.PixelFormat pixelFormat;
        private Camera.CameraParameter.ColorType ColorType;
        private Camera.CameraParameter.ByteType ByteType;
        private bool CamConnected=false;
        private bool isCamReady;
        private bool isCamGrabbed;
        //private bool CamGrabbed;
        private bool isCamCallback = true;
        private int FrameCount = 0; //카메라 획득 프레임 카운트 변수

        // 이미지 처리 변수
        private bool isFirstBuffer= true; // 할당될 버퍼메모리가 첫번째인 경우 true, 두번째인 경우 false
        private bool isRotate = false;
        private Bitmap displayBitmap;
        private Byte[] pByteImage;
        private BitmapData bitmapData;
        //private IntPtr imageBuffer;
        private int bufferSize;

        // 로그 변수
        #region Log Variables
        private DateTime updateTime;
        private FileStream m_LogFS;
        private BufferedStream m_LogBS;
        #endregion

        //double diplay_width_heigth_ratio=9/16;
        //double cam_display_width_height_ratio = 9 / 14;

        public MainForm()
        {
            InitializeComponent();

            Show_Initial_UI();

            Initialize_System();
            
        }
       
        /// 폼 Closed 프로세스
        /// 쓰레드 동작 해제와 카메라 시스템 종료
        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            // 쓰레드 동작 해제
            //if (isThreadRunning)
            //    isThreadRunning = false;

            // 쓰레드 동작 해제
            if (RunCam != null)
            {
                RunCam.Abort();
                RunCam = null;
            }

            //// 쓰레드 동작 해제
            //if (RunGrab != null)
            //{
            //    RunGrab.Abort();
            //    RunGrab = null;
            //}

            //// 쓰레드 동작 해제
            //if (RunDisplay != null)
            //{
            //    RunDisplay.Abort();
            //    RunDisplay = null;
            //}

            // 비트맵 이미지 할당 해제
            if (displayBitmap!=null)
                displayBitmap.Dispose();

            // Crevis 카메라 연결및 시스템 해제
            if (CamConnected)
                ColorCam.SystemClose();

            Add_LogData(0, 1, "System : System Finish");

            foreach (Process process in Process.GetProcesses())
            {

                if (process.ProcessName.StartsWith(Process.GetCurrentProcess().ProcessName))
                {
                    process.Kill();
                }
            }
        }

        /// 키버튼 입력에 대한 프로세스 - 사용하지 않음
        /// ESC 버튼 이벤트 등에 대한 구현 부분
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            //// esc키 버튼 이벤트 실행 시 Form 클로즈 실행
            //if (keyData == Keys.Escape)
            //{

                
            //}

            return base.ProcessCmdKey(ref msg, keyData);
        }
       
        #region 시스템 초기화
        // 초기화 창 실행을 위한 프로세스 
        private void Show_Initial_UI()
        {
            InitialProcess init = new InitialProcess();

            init.Process_Start();

            for (int i = 0; i <= 100; i++)
            {
                init.Update_Progress(i);

                Thread.Sleep(40);// Thread.Sleep(40);
            }
            Thread.Sleep(1000);

            init.Process_Stop();

        }

        // 시스템 초기화 프로세스
        private void Initialize_System()
        {
            // 초기 디렉토리 생성
            Create_DefaultDirectory();

            // ini 파일 상의 프로그램 시스템 데이터 로드 
            Initialize_Data();

            FileInfo file = new FileInfo(systemFilePath);
            if (!file.Exists)
                ProgramSetting.Para_Save();

            Update_Display(1);

            try
            {
                if (!Initialize_Log())
                    throw new Exception("System : Log Initialize Failed");
                if (!Initialize_Camera())
                    throw new Exception("System : Camera Initialize Failed");
                else
                {
                    Add_LogData(1, 1, "System : System Start");
                }
            }
            catch (Exception ex)
            {
                Add_LogData(1, 1, ex.Message);
                MessageBox.Show(ex.Message);

                // 프로그램 문제 발생 시 종료 프로세스 추가
                Environment.Exit(0);
            }

        }

        // 초기 디렉터리를 생성
        private void Create_DefaultDirectory()
        {
            string path;

            path = defaultDirectory;
            if (Directory.Exists(path) == false)   //폴더 확인후 없으면 생성
                Directory.CreateDirectory(path);

            path += "\\SystemData";
            if (Directory.Exists(path) == false)   //폴더 확인후 없으면 생성
                Directory.CreateDirectory(path);
        }

        // 시스템 데이터 초기화 프로세스
        private void Initialize_Data()
        {
            string readStringLogFrameCountData;
            ProgramSetting.Para_Load(systemFilePath);

            logPath = ProgramSetting.Recipe_Data.LogPath;
            logSaveDuration = ProgramSetting.Recipe_Data.LogSaveDuration;
            readStringLogFrameCountData = ProgramSetting.Recipe_Data.LogFrameCountEnable;
            cameraModel = ProgramSetting.Recipe_Data.CameraModel;
            displayRotation = ProgramSetting.Recipe_Data.DisplayRotated;

            systemPassword = ProgramSetting.Recipe_Password.Password;

            // 레시피 데이터의 로그프레임카운트 확인하여 프레임 카운트 로그 활성화
            if (readStringLogFrameCountData == "F")
                logFrameCountEnable = false;
            else
                logFrameCountEnable = true;
            // 레시피 데이터의 영상 출력 회전 상태를 확인하여 영상 출력
            if (displayRotation == "T")
                isRotate = true;
            else
                isRotate = false;

        }

        // 카메라 초기화 프로세스
        private bool Initialize_Camera()
        {
            bool result;
            string tempString;


            try
            {
                // 카메라 객체 생성 : VirtualFG초기화~영상 그랩 대기 프로세스 까지 진행
                if (isCamCallback)
                    ColorCam = new Camera(0, GrabMode, 2, isCamCallback);
                else
                    ColorCam = new Camera(0, GrabMode);
                //ColorCam = new Camera(0, setWidth, setHeight, GrabMode);

                // 카메라 객체 생성 과정에서 문제 발생시 에러 로그 기록 후 카메라 초기화 프로세스 종료
                result = ColorCam.InitializeResult;
                if (!result)
                {
                    tempString = ColorCam.ErrorMessage;
                    Add_LogData(0, 1, tempString);
                    throw new Exception(tempString);
                }

                // 카메라 설정 값을 읽어옴
                Get_CameraSettingValue();

                // 카메라 픽셀포맷을 확인
                if (!Check_PixelFormat())
                    throw new Exception();

                // 카메라 해상도를 PictureBox에 맞게 설정
                if (!Set_Resolution())
                    throw new Exception();

                // 카메라 설정 후 Acquistion 시작
                // 콜백 그랩에 따라서 Acquisition 프로세스 진행
                if (!Start_Acquistion(isCamCallback))
                    throw new Exception();

                //// 카메라 데이터를 읽어올 Byte[] 를 생성
                //pByteImage = new byte[ColorCam._bufferSize];
                bufferSize = ColorCam._bufferSize;

                CamConnected = true;
                Update_Display(4);

                timerConnectionUI.Start();
                timerLogDelete.Start();
                if (isCamCallback)
                    RunCam = new Thread(new ThreadStart(GrabCallbackImage));
                else
                    RunCam = new Thread(new ThreadStart(GrabNormalImage));
                RunCam.IsBackground = true; // 쓰래드를 배경 스레드로 설정하는 경우 
                RunCam.Start();

                #region 멀티 쓰레드 테스트
                //RunGrab = new Thread(new ThreadStart(GrabNormal));
                //RunGrab.IsBackground = true;
                //RunGrab.Start();
                //RunDisplay = new Thread(new ThreadStart(DisplayNormal));
                //RunDisplay.IsBackground = true;
                //RunDisplay.Start();
                #endregion

                isCamReady = true;
                isThreadRunning = true;

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        #endregion

        
        #region Log Functions

        // 로그 초기화 프로세스
        private bool Initialize_Log()
        {
            string path;
            string tempString;

            try
            {
                updateTime = DateTime.Now;

                // 로그 파일을 년월일 폴더 생성 저장
                tempString = updateTime.ToString("yyyyMMdd");
                path = logPath;
                path += "\\";
                path += tempString;

                if (Directory.Exists(path) == false)   //폴더 확인후 없으면 생성
                    Directory.CreateDirectory(path);

                //updateTime = DateTime.Now;

                //tempString = updateTime.ToString("yyyyMMdd");

                path += "\\" + tempString + "_" + DateTime.Now.ToString("HH") + ".LOG";

                m_LogFS = null;
                m_LogFS = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);

                if (m_LogFS == null)
                    new Exception("로그 초기화 실패");
                else
                {
                    m_LogBS = null;
                    m_LogBS = new BufferedStream(m_LogFS);
                    m_LogBS.Seek(0, SeekOrigin.End);
                }

                return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        // 로그파일에 데이터 추가 
        private void Add_LogData(int nBeforeBlankLine, int nTab, string LogString)
        {

            Reinitialize_Log();

            string tempString;
            string tempContents;
            DateTime currentTime = DateTime.Now;

            tempString = currentTime.ToString("yyyyMMdd") + "_" + currentTime.ToString("HH") + ":" +
                currentTime.ToString("mm") + ":" + currentTime.ToString("ss.fff");
            //tempString = currentTime.ToString("yyyy") + "." + currentTime.ToString("MM") + "." + currentTime.ToString("dd") + "_" + currentTime.ToString("HH") + ":" +
            //    currentTime.ToString("mm") + ":" + currentTime.ToString("ss.fff");

            tempContents = "";
            for (int i = 0; i < nBeforeBlankLine; i++) // 줄 건너 뛰기
                tempContents += "\r\n";


            tempContents += "[";
            tempContents += tempString; // 년월일시분초
            tempContents += "]";
            tempContents += " ";

            for (int j = 0; j < nTab; j++) // Tab기능
                tempContents += "     ";

            tempContents += LogString;
            tempContents += "\r\n";

            try
            {
                if (m_LogBS != null)
                {
                    m_LogBS.Write(Encoding.Default.GetBytes(tempContents), 0, Encoding.Default.GetByteCount(tempContents));
                    m_LogBS.Flush();
                }
                else
                    throw new Exception("Add_LogData : Log 쓰기 error.");
            }
            catch (Exception ex)
            {
                Add_LogData(0, 1, ex.Message);
            }
        }

        // 년, 월, 일이 다른 경우 로그 초기화
        private void Reinitialize_Log()
        {
            DateTime currentTime = DateTime.Now;

            if (currentTime.Year != updateTime.Year ||
                currentTime.Month != updateTime.Month ||
                currentTime.Day != updateTime.Day ||
                currentTime.Hour != updateTime.Hour)
            {
                Close_Log();
                Initialize_Log();
            }
        }

        private void Close_Log()
        {
            if (m_LogBS != null)
                m_LogBS.Flush();
        }

        private void Delete_Log(int saveDuration)
        {
            //// 현재 일자를 획득
            //DateTime currentTime = DateTime.Now;

            //// Log 폴더 내 년월일 디렉토리 생성 없이 년월일.LOG 저장하는 경우 - Option 1(2021.10.25 기준 Option 2 변경)
            //// 파일 삭제 일자를 획득, 현재 일자에서 저장 기간을 차감
            //DateTime deleteDate = currentTime.AddDays(-saveDuration);

            //DirectoryInfo directoryInfo = new DirectoryInfo(LogPath);

            //foreach (FileInfo file in directoryInfo.GetFiles())
            //{
            //    if (DateTime.Compare(file.CreationTime, deleteDate) > 0)
            //    {
            //        File.Delete(file.FullName);

            //    }
            //}

            // Log 폴더 내 년월일 디렉토리 생성, 년월일_시.LOG 저장하는 경우 - Option 2

            // 현재 일자를 획득
            DateTime currentTime = DateTime.Now;
            // 파일 삭제 일자를 획득, 현재 일자에서 저장 기간을 차감
            DateTime deleteDate = currentTime.AddDays(-saveDuration);
            string strDeleteDate = deleteDate.ToString("yyyyMMdd");

            // 로그 경로 내 디렉토리 목록을 가져옴
            DirectoryInfo logDirectory = new DirectoryInfo(logPath);
            DirectoryInfo[] subDirectories = logDirectory.GetDirectories();

            // 파일삭제일자와 디렉토리 목록의 생성 일자 확인하여 파일삭제 일자 이전이면 디렉토리내 모든 파일및 폴더 제거 
            foreach (DirectoryInfo subDirectory in subDirectories)
            {
                if (strDeleteDate.CompareTo(subDirectory.CreationTime.ToString("yyyyMMdd")) > 0)
                {
                    //폴더 속성이 바뀌어 있을 수 있으므로 미리 속성을 Normal로 설정
                    subDirectory.Attributes = FileAttributes.Normal;

                    //폴더 삭제
                    subDirectory.Delete(true);
                }
            }
        }

        #endregion

        #region Camera Function
        // 카메라 Feature 값을 획득
        private void Get_CameraSettingValue()
        {
            CamInfo = new Dictionary<string, string>();
            CamInfo.Add("DeviceID", ColorCam._DeviceID);
            CamInfo.Add("Width", string.Format("{0}", ColorCam._width));
            CamInfo.Add("Height", string.Format("{0}", ColorCam._height));
            CamInfo.Add("TriggerMode", ColorCam._TriggerMode);
            CamInfo.Add("TriggerSource", ColorCam._TriggerSource);
            pixelFormat = ColorCam._PixelFormat;
            ColorType = ColorCam._ColorType;
            ByteType = ColorCam._ByteType;
        }

        // 카메라 설정 픽셀 포맷이 Mono8, RG8 또는 YUV422Packed가 아닌 경우 픽셀 포맷을 YUV422Packd 재 설정
        private bool Check_PixelFormat()
        {
            string tempString;
            if (pixelFormat != Camera.CameraParameter.PixelFormat.Mono8 && pixelFormat != Camera.CameraParameter.PixelFormat.BayerRG8 && pixelFormat != Camera.CameraParameter.PixelFormat.YUV422Packed)
            {
                tempString = "Camera_Error : " + "Pixel Format Setting Failed and Reset Pixel Format To YUV422Packed";
                Add_LogData(0, 1, tempString);
                if (!ColorCam.SetPixelFormat(Camera.CameraParameter.PixelFormat.YUV422Packed))
                {
                    tempString = ColorCam.ErrorMessage;
                    Add_LogData(0, 1, tempString);
                    return false;
                }
                // 변경된 픽셀 포맷 값 적용을 위해 Feature 값을 획득
                Get_CameraSettingValue();


                Add_LogData(0, 1, "Pixel Format Set Succeded");
            }
            return true;
        }

        // 카메라 해상도를 PictureBox 비율에 맞게 설정
        private bool Set_Resolution()
        {
            string tempString;
            // PictureBox 비율에 따라 카메라 해상도 설정
            int setDWWidth;
            int setDWHeight;

            // 카메라 A타입 - MG-D200B 
            if(cameraModel=="A"||cameraModel=="a")
            {
                setDWWidth = 1644;
                setDWHeight = 1236;
                if (displayRatio == DisplayParameter.DisplayRatio.Nine_Ratio)
                    setDWHeight = 986;
                else if (displayRatio == DisplayParameter.DisplayRatio.Ten_Ratio)
                    setDWHeight = 1096;
            }
            else
            {
                setDWWidth = 2064;
                setDWHeight = 1544;
                if (displayRatio == DisplayParameter.DisplayRatio.Nine_Ratio)
                    setDWHeight = 1238;
                else if (displayRatio == DisplayParameter.DisplayRatio.Ten_Ratio)
                    setDWHeight = 1376;
            }
           

            if (CamInfo["Height"] != setDWHeight.ToString())
            {
                tempString = string.Format("Camera_Setting : " + "Setting Resolution Error and Reset {0} X {1}", setDWWidth, setDWHeight);
                Add_LogData(0, 1, tempString);

                if (!ColorCam.SetResolution(setDWWidth, setDWHeight))
                {
                    tempString = ColorCam.ErrorMessage;
                    Add_LogData(0, 1, tempString);
                    return false;
                }

                // 카메라 재 연결시 버퍼 메모리 할당하지 않는 것으로 변경 - 2021.11.02
                if (!ColorCam.AllocateImageBuffer(2))
                {
                    tempString = ColorCam.ErrorMessage;
                    Add_LogData(0, 1, tempString);
                    return false;
                }

                //// 버퍼 메모리 할당
                //if (ByteType == Camera.CameraParameter.ByteType.One)
                //{
                //    bufferSize = setDWWidth * setDWHeight * 3;
                //}
                //else
                //{
                //    bufferSize = setDWWidth * setDWHeight * 2 * 3;
                //}
                //imageBuffer = Marshal.AllocHGlobal(bufferSize);

                // 변경된 Height 설정값 적용을 위해 Feature 값을 획득
                Get_CameraSettingValue();

                Add_LogData(0, 1, "Camera Resolution Set Succeded");
            }


            return true;
        }

        // 그램 Acquistion 시작 프로세스 - 그랩을 콜백으로 진행 or 아닌 경우 판단 진행
        private bool Start_Acquistion(bool CamCallback)
        {

            // 카메라 설정 후 Acquistion 시작
            // 콜백 그랩에 따라서 Acquisition 프로세스 진행
            if (isCamCallback)
            {
                if (!ColorCam.StartAcquisition(GrabMode, isCamCallback))
                    return false;
            }
            else
            {
                if (!ColorCam.StartAcquisition(GrabMode))
                    return false;
            }   

            return true;

        }

        /// 카메라 일반 영상 그랩 프로세스 
        /// 설정한 BufferMemoryNumber 에 따라 해당되는 메모리에 이미지 데이터를 저장 
        private bool  Grab_Process(UInt32 BufferMemoryNumber)
        {
            // 픽셀 포맷 모노로 설정된 경우 프로세스
            if (ColorType == Camera.CameraParameter.ColorType.Mono)
            {
                //영상 MonoGrab
                if (!ColorCam.MonoGrab(BufferMemoryNumber))
                {
                    Add_LogData(0, 1, "Grab_Error : " + ColorCam.ErrorMessage);
                    return false;
                }

                //if (ColorCam._isGrabbed)
                //{
                //    //모노 Bitmap 이미지 생성
                //    displayBitmap=ColorCam.CreateBitmap(ByteType, BufferMemoryNumber);
                //}

                return true;

                ////모노 Bitmap 이미지 생성
                //return ColorCam.CreateBitmap(ByteType);
            }
            // 컬러로 설정된 경우 프로세스
            else
            {
               
                 //컬러 영상 포맷 ColorGrab
                if (!ColorCam.ColorGrab(BufferMemoryNumber, GrabMode))
                {
                    Add_LogData(0, 1, "Grab_Error : " + ColorCam.ErrorMessage);
                    return false;
                }

                //if (ColorCam._isGrabbed)
                //{
                //    //컬러 Bitmap 이미지 생성
                //    displayBitmap=ColorCam.CreateColorBitmap();
                //}

                return true;

                ////컬러 Bitmap 이미지 생성
                //return ColorCam.CreateColorBitmap();
            }
        }
      
        // 카메라 콜백 영상 그랩 프로세스
        private Bitmap Callback_Grab_Process(UInt32 BufferMemoryNumber)
        {
            // 픽셀 포맷 모노로 설정된 경우 프로세스
            if (ColorType == Camera.CameraParameter.ColorType.Mono)
            {
             
                //모노 Bitmap 이미지 생성
                return ColorCam.CreateBitmap(ByteType, BufferMemoryNumber);

            }
            // 컬러로 설정된 경우 프로세스
            else
            {
             
                //컬러 Bitmap 이미지 생성
                return ColorCam.CreateColorBitmap(0);
            }
        }

        #endregion


        #region Image Display
        // 화면 표시 프로세스
        private void Update_Display(int index)
        {
            switch (index)
            {
                #region 초기 화면 배치
                case 1:
                    //  프로그램 화면을 디스플레이 해상도에 맞게 배치 - 16:9 / 16:10 비율 
                    int screen_height = Screen.PrimaryScreen.Bounds.Height;
                    int screen_width = Screen.PrimaryScreen.Bounds.Width;
                    this.Size = new Size(screen_width, screen_height);
                    this.Location = new Point(0, 0);

                    float fDisplayRatio = (float)screen_width / (float)screen_height;
                    if (fDisplayRatio == (float)1.6)
                        displayRatio = DisplayParameter.DisplayRatio.Ten_Ratio;
                    else
                        displayRatio = DisplayParameter.DisplayRatio.Nine_Ratio;

                    // 영상 출력 화면을 배치 - 15:10 비율 
                    //int cam_screen_width = Convert.ToInt32(screen_height * cam_display_width_ratio / cam_display_height_ratio);
                    int cam_screen_width = Convert.ToInt32(screen_width * 15 / 16);
                    this.pictureBox1.Size = new Size(cam_screen_width, screen_height);
                    this.pictureBox1.Location = new Point(0, 0);

                    int button_height = 50;

                    // Operator/Admin 버튼 배치
                    this.btnAdmin.Size = new Size(screen_width - cam_screen_width, button_height);
                    this.btnAdmin.Location = new Point(cam_screen_width, screen_height - button_height);
                    this.btnAdmin.BackgroundImage = Properties.Resources.Operator_00;

                    // 카메라 연결 상태 화면(패널과 레이블)을 배치 
                    this.panelCam.Size = new Size(screen_width - cam_screen_width, button_height);
                    this.panelCam.Location = new Point(cam_screen_width, 30);

                    if (fDisplayRatio == (float)1.6)
                        this.label1.Font = new Font("Microsoft Sans Serif style", 12F, FontStyle.Bold);
                    else
                        this.label1.Font = new Font("Microsoft Sans Serif style", 14F, FontStyle.Bold);

                    this.label1.Size = this.panelCam.Size;
                    this.label1.Location = new Point(0, 10);

                    // 종료 버튼 배치
                    this.btnExit.Size = this.btnAdmin.Size;
                    this.btnExit.Location = new Point(cam_screen_width, screen_height - 3 * button_height);
                    this.btnExit.Visible = false;

                    // 셋팅 버튼 배치
                    this.btnSetting.Size = this.btnAdmin.Size;
                    this.btnSetting.Location = new Point(cam_screen_width, screen_height - 5 * button_height);
                    this.btnSetting.Visible = false;

                    // 회전 버튼 배치
                    this.btnRotate.BackgroundImage = Properties.Resources.Origin_00;
                    //this.btnRotate.Size = new Size(btnSetting.Size.Width * 1 / 2, btnSetting.Size.Width * 1 / 2);
                    this.btnRotate.Size = this.btnAdmin.Size;
                    this.btnRotate.Location = new Point(cam_screen_width, screen_height - 7 * button_height);
                    this.btnRotate.Visible = true;

                    break;
                #endregion

                case 2:
                    // Admin 모드 UI - 셋팅, 회전 버튼 표시
                    this.btnAdmin.BackgroundImage = Properties.Resources.Admin_00;
                    this.btnSetting.Visible = true;
                    this.btnExit.Visible = true;
                    //this.btnRotate.Visible = true;
                    break;

                case 3:
                    // Operator 모드 UI - 셋팅, 회전 버튼 표시하지 않음
                    this.btnAdmin.BackgroundImage = Properties.Resources.Operator_00;
                    this.btnSetting.Visible = false;
                    this.btnExit.Visible = false;
                    break;

                case 4:
                    // 카메라 연결 상태 표시
                    this.label1.ForeColor = Color.Green;
                    this.label1.Text = "Connected";
                    break;

                case 5:
                    // 카메라 연결되지 않은 상태 표시
                    this.label1.ForeColor = Color.Red;
                    this.label1.Text = "Disconnect";
                    break;

                case 6:
                    // UI 동작을 위해 카메라 연결 초기화
                    this.label1.Text = "";
                    break;

                case 7:
                    // 회전 동작하지 않는 경우 UI
                    this.btnRotate.BackgroundImage = Properties.Resources.Origin_00;
                    break;

                case 8:
                    // 회전 동작시 UI
                    this.btnRotate.BackgroundImage = Properties.Resources.Rotate_00;
                    break;

            }
        }

        /// 비트맵 이미지를 PictureBox에 디스플레이
        /// 현재 사용하고 있지 않음
        private void Display_BitmapImage()
        {
            // Camguide40 프로그램 실행 시 발생하는 에러를 해결하기 위해 DeepCopy 진행
            long us_time = 0;
            long ms_time = 0;
            int a = 0;
            
            Bitmap Deep_Bitmap = new Bitmap(displayBitmap);
            
            // 영상 깨지는 현상 테스트 - 2021.10.20
            if (!isRotate)
                pictureBox1.Image = Deep_Bitmap;
            else
                pictureBox1.Image = ImageProcessing.RotateImage(Deep_Bitmap);

            if (displayBitmap != null)
                displayBitmap.Dispose();
        }

        /// 비트맵 이미지를 PictureBox에 디스플레이
        /// 현재 사용하고 있지 않음
        private void Display_BitmapImage(Bitmap bitmap)
        {
            //Bitmap bitmapOld = pictureBox1.Image as Bitmap;
            // 영상 깨지는 현상 테스트 - 2021.10.20
            if (!isRotate)
                pictureBox1.Image = bitmap;
            else
                pictureBox1.Image = ImageProcessing.RotateImage(bitmap);


            //if (bitmapOld != null)
            //    bitmapOld.Dispose();
        }

        // 버퍼메모리를 Bitmap으로 변환 PictureBox에 디스플레이 - 2021.12.14 테스트
        private void Display_Image(UInt32 BufferMemoryNumber)
        {
            Int32 bitsPerPixel = 0;
            Int32 stride = 0;
            int width;
            int height;
            bool isColor=false;

            try
            {
                width = ColorCam._width;
                height = ColorCam._height;

                //Queue<IntPtr> q= new Queue<IntPtr>();
                //unsafe
                //{
                //    if (ColorType == Camera.CameraParameter.ColorType.Color)
                //        isColor = true;
                //    else
                //        isColor = false;

                //    if (isColor)
                //        displayBitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                //    else
                //        displayBitmap = new Bitmap(ColorCam._width, ColorCam._height, PixelFormat.Format8bppIndexed);

                //    BitmapData bmpData = displayBitmap.LockBits(new Rectangle(0, 0, displayBitmap.Width, displayBitmap.Height),
                //       ImageLockMode.ReadOnly, displayBitmap.PixelFormat);
                //    IntPtr ptr = bmpData.Scan0;

                //    byte[] source;

                //    if (isColor)
                //    {
                //        source = sizeof(IntPtr) == 4 ? BitConverter.GetBytes((int)ColorCam._cvtImages[BufferMemoryNumber]) :
                //            BitConverter.GetBytes((long)ColorCam._cvtImages[BufferMemoryNumber]);
                //        System.Runtime.InteropServices.Marshal.Copy(source, 0, ptr, width * height * 3);
                //    }
                //    else
                //    {
                //        source = sizeof(IntPtr) == 4 ? BitConverter.GetBytes((int)ColorCam._pImages[BufferMemoryNumber]) :
                //           BitConverter.GetBytes((long)ColorCam._pImages[BufferMemoryNumber]);
                //        System.Runtime.InteropServices.Marshal.Copy(source, 0, ptr, width * height);
                //    }
                //    displayBitmap.UnlockBits(bmpData);
                //}

                unsafe
                {
                    //color 영상 표시

                    if (ColorType == Camera.CameraParameter.ColorType.Color)
                    {
                        bitsPerPixel = 24;
                        stride = (Int32)((ColorCam._width * bitsPerPixel + 7) / 8);
                        displayBitmap = new Bitmap(ColorCam._width, ColorCam._height, stride, System.Drawing.Imaging.PixelFormat.Format24bppRgb,
                            ColorCam._cvtImages[BufferMemoryNumber]);
                    }
                    //Mono 영상 표시
                    else
                    {
                        bitsPerPixel = 8;
                        stride = (Int32)((ColorCam._width * bitsPerPixel + 7) / 8);
                        displayBitmap = new Bitmap(ColorCam._width, ColorCam._height, stride, System.Drawing.Imaging.PixelFormat.Format8bppIndexed,
                            ColorCam._pImages[BufferMemoryNumber]);

                        ColorPalette GrayscalePalette = displayBitmap.Palette;

                        for (int i = 0; i < 255; i++)
                        {
                            GrayscalePalette.Entries[i] = Color.FromArgb(i, i, i);
                        }

                        displayBitmap.Palette = GrayscalePalette;

                    }
                }

                Bitmap oldBitmap = pictureBox1.Image as Bitmap;

                if (displayBitmap != null)
                {
                    if (!isRotate)
                        pictureBox1.Image = displayBitmap;
                    else
                    {
                        displayBitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        pictureBox1.Image = displayBitmap;
                    }
                    //pictureBox1.Image = ImageProcessing.RotateImage(bitmap);
                    if (oldBitmap != null)
                        oldBitmap.Dispose();
                }
                else
                {
                    if (logFrameCountEnable)
                        Add_LogData(0, 1, "Display_Error : No Bitmap Image");

                }
            }
            catch (InvalidOperationException err)
            {
                Add_LogData(0, 1, "Display_Error : " + err.Message);
                //pictureBox1.Image = displayBitmap;
                ShowImage(displayBitmap);
            }
            catch (Exception ex)
            {
                Add_LogData(0, 1, "Display_error : " + ex.Message);
            }

        }

        // 콜백 이미지 데이터를 Bitmap으로 변환 PictureBox에 디스플레이 - 2021.12.20
        private void Display_Callback_Image()
        {
            Int32 bitsPerPixel = 0;
            Int32 stride = 0;
            bool isColor = false;
            int width;
            int height;
            ColorPalette GrayscalePalette;

            width = ColorCam._width;
            height = ColorCam._height;

            try
            {

                #region Image Option 2 - Bitmap Lock->Marshal Copy->Unlock
                if (ColorType == Camera.CameraParameter.ColorType.Color)
                    isColor = true;
                else
                    isColor = false;

                if (isColor)
                    displayBitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                else
                    displayBitmap = new Bitmap(width, height, PixelFormat.Format8bppIndexed);

                bitmapData = displayBitmap.LockBits(new Rectangle(0, 0, displayBitmap.Width, displayBitmap.Height),
                       ImageLockMode.ReadWrite, displayBitmap.PixelFormat);


                byte[] pBuffer;
                int bSize=bitmapData.Stride*displayBitmap.Height;

                unsafe
                {
                    IntPtr ptr = bitmapData.Scan0;

                    if (isColor)
                    {
                        pBuffer = new byte[3* bufferSize];
                        //Marshal.Copy(ptr, 0, ptr, 3 * bufferSize);
                        Marshal.Copy(ColorCam._cvtImage, pBuffer, 0, 3* bufferSize);
                        Marshal.Copy(pBuffer, 0, ptr, bSize);
                    }
                    else
                    {
                        pBuffer = new byte[bufferSize];
                        Marshal.Copy(ColorCam._pImage, pBuffer, 0, bufferSize);
                        Marshal.Copy(pBuffer, 0, ptr, bSize);
                    }
                    displayBitmap.UnlockBits(bitmapData);
                }

                if (!isColor)
                {
                    GrayscalePalette = displayBitmap.Palette;

                    for (int i = 0; i < 255; i++)
                    {
                        GrayscalePalette.Entries[i] = Color.FromArgb(i, i, i);
                    }

                    displayBitmap.Palette = GrayscalePalette;
                }
                #endregion

                #region Image Option 1 - 버퍼메모리에서 비트맵 바로 생성

                //unsafe
                //{
                //    Marshal.Copy(ColorCam._pImage, pByteImage, 0, ColorCam._bufferSize);
                //    if (pByteImage[0] == 0)
                //        throw new Exception("No Buffer Memory Update");

                //    //color 영상 표시
                //    if (ColorType == Camera.CameraParameter.ColorType.Color)
                //    {
                //        bitsPerPixel = 24;
                //        stride = (Int32)((ColorCam._width * bitsPerPixel + 7) / 8);

                //        displayBitmap = new Bitmap(width, height, stride, System.Drawing.Imaging.PixelFormat.Format24bppRgb,
                //            ColorCam._cvtImage);

                //        #region 버퍼 메모리 복사 테스트
                //        //if (ByteType == Camera.CameraParameter.ByteType.One)
                //        //{
                //        //    bufferSize = width * height * 3;
                //        //}
                //        //else
                //        //{
                //        //    bufferSize = width * height * 2 * 3;
                //        //}
                //        //IntPtr imageBuffer = Marshal.AllocHGlobal(bufferSize);

                //        //Buffer.MemoryCopy(ColorCam._cvtImage.ToPointer(), imageBuffer.ToPointer(), bufferSize, bufferSize);
                //        //Marshal.FreeHGlobal(testBuffer);

                //        //displayBitmap = new Bitmap(width, height, stride, System.Drawing.Imaging.PixelFormat.Format24bppRgb,
                //        //    imageBuffer);
                //        ////System.Runtime.InteropServices.Marshal.Copy(ColorCam._cvtImage, 0,  width*height);  
                //        #endregion

                //        //displayBitmap = new Bitmap(width, height, stride, System.Drawing.Imaging.PixelFormat.Format24bppRgb,
                //        //    ColorCam._cvtImage);
                //    }
                //    //Mono 영상 표시
                //    else
                //    {
                //        bitsPerPixel = 8;
                //        stride = (Int32)((ColorCam._width * bitsPerPixel + 7) / 8);
                //        displayBitmap = new Bitmap(ColorCam._width, ColorCam._height, stride, System.Drawing.Imaging.PixelFormat.Format8bppIndexed,
                //            ColorCam._pImage);

                //        GrayscalePalette = displayBitmap.Palette;

                //        for (int i = 0; i < 255; i++)
                //        {
                //            GrayscalePalette.Entries[i] = Color.FromArgb(i, i, i);
                //        }

                //        displayBitmap.Palette = GrayscalePalette;

                //    }
                //}
                #endregion

                //Bitmap oldBitmap = pictureBox1.Image as Bitmap;

                if (displayBitmap != null)
                {
                    if (!isRotate)
                    {
                        //// 영상 이미지를 출력할 Picturebox에 대해 delegate를 이용 Cross Thread 방지
                        //ShowImage();
                        pictureBox1.Image = displayBitmap;
                    }
                    else
                    {
                        displayBitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        //// 영상 이미지를 출력할 Picturebox에 대해 delegate를 이용 Cross Thread 방지
                        //ShowImage();
                        pictureBox1.Image = displayBitmap;
                    }
                    //pictureBox1.Image = ImageProcessing.RotateImage(bitmap);
                    //if (oldBitmap != null)
                    //    oldBitmap.Dispose();
                }
                else
                {
                    if (logFrameCountEnable)
                        throw new Exception("No Bitmap Image");
                        //Add_LogData(0, 1, "Display_Error : No Bitmap Image");

                }

                Marshal.WriteIntPtr(ColorCam._pImage, ((IntPtr)(0)));
                Marshal.WriteIntPtr(ColorCam._cvtImage, ((IntPtr)(0)));

            }
            catch (InvalidOperationException err)
            {
                Add_LogData(0, 1, "Display_Error : "+err.Message);
                // 영상 출력 
                Thread.Sleep(20);
                //pictureBox1.Image = displayBitmap; 
                //ShowImage(displayBitmap);
            }
            catch(Exception ex)
            {
                Add_LogData(0, 1, "Display_error : " + ex.Message);
            }
            

        }

        #region Bitmap Delegate 추가
        public delegate void SetImageCallback(Bitmap bitmap);

        public void ShowImage(Bitmap image)
        {
            if (this.pictureBox1.InvokeRequired)
            {
                SetImageCallback imageDelegate = new SetImageCallback(ShowImage);
                this.Invoke(imageDelegate, new object[] { image });
            }
            else
                this.pictureBox1.Image = image;
        }

        #endregion

        // 이미지 저장 프로세스 - 사용하지 않음
        private void Save_Image()
        {
            string folder = @"C:\Crevis_Control\Image";
            string filename = folder+"\\"+DateTime.Now.ToString("yyyyMMdd_hhmmss_fff") + ".jpg";
            displayBitmap.Save(filename, System.Drawing.Imaging.ImageFormat.Jpeg);
        }
        #endregion

        #region UI 이벤트
        // Admin/Operator 버튼 이벤트
        private void btnAdmin_Click(object sender, EventArgs e)
        {
            if (!isAdminMode)
            {
                PassForm passForm = new PassForm();
                passForm.Location = new Point(500, 200);
                passForm.SetPassword = systemPassword;

                passForm.PasswordFormEvent += PassFormEventMethod;
                passForm.ShowDialog();

            }
            else
            {
                Add_LogData(0, 1, "Setting : Operator Mode Start");
                isAdminMode = false;
                Update_Display(3);

            }
        }

        // PassForm 의 결과를 확인 Admin 모드로 전환
        // 결과가 true이면 UI 전환, 그렇지 않은 경우 유지
        private void PassFormEventMethod(bool result)
        {
            isAdminMode = result;
            if (isAdminMode)
            {
                Add_LogData(0, 1, "Setting : Admin Mode Start");
                Update_Display(2);
            }
        }

        //  Exit 버튼 이벤트
        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        // Setting 버튼 이벤트
        private void btnSetting_Click(object sender, EventArgs e)
        {
            string tempString = "";
            #region Camguide 연동 프로세스
            if (!isSettingMode)
            {
                try
                {

                    // 영상 획득 과정에서 셋팅 모드 전환시 발생할 에러 방지를 위해 쓰레드와 타이머(카메라 재연결) 동작에 대한 Flag를 설정 
                    isSettingMode = true;
                    isCamReady = false;
                    // Setting 동작 시 프레임카운트 리셋
                    FrameCount = 0; 
                    // 콜백 프로세스 진행시
                    //ColorCam._FrameCount = 0;

                    // 카메라 연결 타이머가 활성화된 상태에서는 연결을 완료할 수 있도록 3초 딜레이 진행
                    if (timerCameraConnection.Enabled)
                    {
                        Delay(3000);
                        timerCameraConnection.Stop();
                 
                    }

                    // 딜레이 1000->200 조정 - 2021.11.24
                    Delay(200);

                    // 프로세스가 남아있는 경우 제거 후 프로그램 재 실행
                    Process[] processes = null;
                    processes = Process.GetProcessesByName("CamGuide40");
                    foreach (Process process in processes)
                    {
                        process.Kill();
                    }
                    processes = Process.GetProcessesByName("CamGuide");
                    foreach (Process process in processes)
                    {
                        process.Kill();
                    }

                    // 카메라 연결 상태 확인 연결된 경우 카메라 Close 프로세스 진행
                    if (CamConnected)
                    {
                        // 영상 획득 중이면 영상 획득 후 카메라 종료하도록 2000ms 대기
                        if (ColorCam._isGrabbed)
                            Delay(2000);

                        /// 로그 기록 없이 실행 - 2021.11.04
                        /// 카메라만 클로즈 - 옵션 1 
                        ColorCam.CamClose();

                        ///// 시스템 클로즈 - 옵션 2
                        //if (!ColorCam.SystemClose())
                        //{
                        //    tempString = ColorCam.ErrorMessage;
                        //    Add_LogData(0, 1, tempString);
                        //}

                        tempString = "Setting : Camera Close Succeded";
                        Add_LogData(0, 1, tempString);

                        CamConnected = false;
                        //Update_Display(5);
                    }

                    //Delay(1500);
                    //Thread.Sleep(1000);



                    // Camguide 프로그램 실행
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = @"C:\Program Files\CREVIS\Cameras\MCam40\bin\x64\CamGuide40.exe";
                    startInfo.Arguments = null;
                    Process.Start(startInfo);

                    tempString = "Setting : Setting Mode Start";
                    Add_LogData(0, 1, tempString);

                    // Camguide 실행 과정에서 취소를 누르는 경우 대비 타이머 우선적으로 동작 진행 - 2021.11.01 수정
                    // 사용자계정컨트롤 설정을 통해서 Camguide40 프로그램 실행 전 예/아니오 설정 가능 타이머 시작 위치 수정 - 2021.11.09
                    timerCamguideMonitoring.Start();
                }
                catch (Exception ex)
                {
                    tempString = "Setting : Setting Mode Start Failed";
                    Add_LogData(0, 1, tempString);
                    MessageBox.Show(ex.Message);
                }
            }
            else
                MessageBox.Show("Already Running Setting Mode");
            #endregion
        }

        // 회전 버튼 이벤트
        private void btnRotate_Click(object sender, EventArgs e)
        {
            // 영상 획득하지 않은 상태에서 회전 버튼 눌린 경우 발생하는 버그 수정 - 2021.11.08
            if (pictureBox1.Image != null)
            {
                Bitmap curentDisplay = new Bitmap(pictureBox1.Image);
                Bitmap rotateDisplay = ImageProcessing.RotateImage(curentDisplay);
                pictureBox1.Image = rotateDisplay;

                if (!isRotate)
                {
                    displayRotation = "T";
                    ProgramSetting.Recipe_Data.DisplayRotated = displayRotation;
                    ProgramSetting.Para_Save();

                    Update_Display(8);
                    isRotate = true;
                }
                else
                {
                    displayRotation = "F";
                    ProgramSetting.Recipe_Data.DisplayRotated = displayRotation;
                    ProgramSetting.Para_Save();

                    Update_Display(7);
                    isRotate = false;
                }
            }
            else
                MessageBox.Show("No Grabbed Image");
           
        }
        #endregion

        #region Thread
        private int GrabCheckNum = 0; // 카메라 연결을 확인을 위한 변수
        private int AcqNum = 0; // 가비지 콜렉터를 위해 취득 영상 확인 변수
        private object lockobject = new object();
        // 콜백영상 그랩~화면 출력 쓰레드 프로세스
        private void GrabCallbackImage()
        {
            long grab_time = 0;
            long disp_time = 0;
            int buffered_count = 0;

            while (isThreadRunning)
            {
                // 카메라 영상 그랩 Flag가 true 인 경우 영상 획득 프로세스 진행
                if (isCamReady)
                {
                    try
                    {
                        #region 영상 획득 옵션1 - 영상 획득 전 카메라 연결 무조건 확인
                        //// 카메라 영상 획득 전 카메라 연결 상태 무조건 확인 진행 - 옵션 1
                        //ColorCam.GetDeviceOpenStatus();
                        //CamConnected = ColorCam._isOpen;

                        ////CamConnected = true;
                        //// 카메라가 연결된 경우 영상 획득 프로세스
                        //// Crevis 카메라에 대해서 콜백 함수 동작도 할 수 있도록 구현 - 2021.10.23
                        //if (CamConnected)
                        //{

                        //    // Callback 프로세스의 경우 영상 획득 됐는지 확인 후 비트맵 생성
                        //    if (isCamCallback)
                        //    {
                        //        if (ColorCam._isGrabbed)
                        //        {
                        //            // 콜백의 경우 
                        //            Callback_Grab_Process();
                        //            Display_BitmapImage();
                        //            ColorCam._isGrabbed = false;
                        //        }
                        //    }
                        //    else
                        //    {
                        //        Grab_Process();
                        //    }
                        //}

                        //// 카메라 연결되지 않은 경우 카메라 영상 그랩 Flag false 설정 영상 획득 프로세스 중지
                        //else
                        //{
                        //    Add_LogData(0, 1, "Camera_Error : Camera Connection Failed");
                        //    if (!ColorCam.CamClose())
                        //        new Exception(ColorCam.ErrorMessage);
                        //    CamThreadFlag = false;
                        //    timerCameraConnection.Start();
                        //    //new Exception("Camera_Error : Camera Connection Failed");
                        //}
                        #endregion

                        System.Diagnostics.Stopwatch swh = new System.Diagnostics.Stopwatch();

                        #region 영상획득 옵션2 - 카메라 일정 이상 영상 그랩하지 못한 경우 장치 연결 확인
                        /// 영상 취득 콜백 프로세스
                        /// Callback 프로세스의 경우 영상 획득 됐는지 확인 후 비트맵 생성
                        if (ColorCam._isGrabbed)
                        {
                            GrabCheckNum = 0;
                            ColorCam._isGrabbed = false;

                            buffered_count = ColorCam.GetBufferedCount();

                            if (buffered_count == 0)
                                throw new Exception("Grab_Error : No Image Data in Queue Buffer");
                            // 그랩 프로세스
                            swh.Reset(); swh.Start();

                            //// 큐를 이용한 멀티 버퍼 프로세스
                            //ColorCam.GetImageData();

                            // 저장된 이미지 버퍼 큐을 수를 확인 나중에 들어온 이미지 획득
                            ColorCam.GetImageData(buffered_count);

                            swh.Stop();
                            grab_time = swh.Elapsed.Ticks / 10;

                            swh.Reset(); swh.Start();
                            // 영상 출력 lock
                            lock (lockobject)
                            {
                                Display_Callback_Image();
                            }
                            swh.Stop();
                            disp_time = swh.Elapsed.Ticks / 10;

                            // 레시피 데이터에 저장된 프레임카운트 로그 활성화 시 로그 기록
                            if (logFrameCountEnable)
                                Add_LogData(0, 1, String.Format("GrabProcess : {0} us, DiplayProcess : {1} us", grab_time, disp_time));

                            FrameCount++;

                            //// 콜백의 경우 
                            //displayBitmap = Callback_Grab_Process(1);

                            ////if(displayBitmap!=null)
                            ////Display_BitmapImage(displayBitmap);

                            //Display_BitmapImage();

                            // 이미지 해제를 위해 GC를 해제
                            AcqNum++;

                            if (AcqNum > 4)
                            {
                                GC.Collect();
                                AcqNum = 0;
                            }

                            Add_LogData(0, 1, String.Format("Frame Count : {0}, Saved Buffer Count : {1}", FrameCount, buffered_count));
                        }
                        /// 카메라 그랩 상태(100번 이상 false인 경우) 체크 후 카메라 연결 상태 모니터링  
                        else
                        {
                            GrabCheckNum++;

                            if (GrabCheckNum >= 100)
                            {
                                GrabCheckNum = 0;

                                // 카메라 오픈 상태 확인
                                ColorCam.GetDeviceOpenStatus();
                                CamConnected = ColorCam._isOpen;

                                /// 카메라가 오픈되지 않은 경우
                                if (!CamConnected)
                                {
                                    Add_LogData(0, 1, "Camera_Connection : Connection Failed");
                                    if (!ColorCam.CamClose())
                                        Add_LogData(0, 1, ColorCam.ErrorMessage);
                                    isCamReady = false;
                                    timerCameraConnection.Start();
                                }
                                else
                                    continue;
                            }
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        Add_LogData(0, 1, ex.Message);

                    }
                    Thread.Sleep(10);
                    //Thread.Sleep(2);

                }
                else
                    Delay(1000);

            }
        }

        // 노멀영상 그랩~ 화면 출력 쓰레드 프로세스
        private void GrabNormalImage()
        {
            bool isImageReady=false;
            bool isImageGrabbed=false;
            long grab_time = 0;
            long disp_time = 0;

            while (isThreadRunning)
            {
                // 카메라 영상 그랩 Flag가 true 인 경우 영상 획득 프로세스 진행
                if (isCamReady)
                {
                    try
                    {
                        // 카메라가 새로운 영상 획득 되었는지 확인 후 새로운 영상 획득된 경우에만 비동기 영상 획득
                        System.Diagnostics.Stopwatch swh = new System.Diagnostics.Stopwatch();
                        isImageReady = ColorCam.GetImageAvailable();
                        if (isImageReady)
                        {
                            // 첫번째 버퍼메모리에 이미지 획득과 영상 출력
                            if (isFirstBuffer)
                            {
                                // 그랩 프로세스
                                swh.Reset(); swh.Start();

                                isImageGrabbed = Grab_Process(0); 

                                swh.Stop();
                                grab_time = swh.Elapsed.Ticks / 10;

                                if(isImageGrabbed)
                                {
                                    // 디스플레이 프로세스
                                    swh.Reset(); swh.Start();
                                    Display_Image(0);
                                    swh.Stop();
                                    disp_time = swh.Elapsed.Ticks / 10;

                                    isImageGrabbed = false;
                                }

                                isFirstBuffer = false;
                            }
                            // 두번째 버퍼메모리에 이미지 획득과 영상 출력
                            else
                            {
                                // 그랩 프로세스
                                swh.Reset(); swh.Start();

                                isImageGrabbed = Grab_Process(1);

                                swh.Stop();
                                grab_time = swh.Elapsed.Ticks / 10;

                                if (isImageGrabbed)
                                {
                                    swh.Reset(); swh.Start();
                                    Display_Image(1);
                                    swh.Stop();
                                    disp_time = swh.Elapsed.Ticks / 10;

                                    isImageGrabbed = false;

                                }

                                isFirstBuffer = true;
                            }
                            // 레시피 데이터에 저장된 프레임카운트 로그 활성화 시 로그 기록
                            if (logFrameCountEnable)
                                Add_LogData(0, 1, String.Format("GrabProcess : {0} us, DiplayProcess : {1} us", grab_time, disp_time));

                            isImageReady = false;

                            FrameCount++;

                            // 이미지 해제를 위해 GC를 해제
                            AcqNum++;
                            if (AcqNum > 4)
                            {
                                GC.Collect();
                                AcqNum = 0;
                            }
                            // 레시피 데이터에 저장된 프레임카운트 로그 활성화 시 로그 기록
                            if(logFrameCountEnable)
                                Add_LogData(0, 1, String.Format("Frame Count : {0}", FrameCount));
                        }
                        /// 새로운 영상 상태(100번 이상 false인 경우) 체크 후 카메라 연결 상태 모니터링  
                        else
                        {
                            GrabCheckNum++;

                            if (GrabCheckNum >= 100)
                            {
                                GrabCheckNum = 0;

                                // 카메라 오픈 상태 확인
                                ColorCam.GetDeviceOpenStatus();
                                CamConnected = ColorCam._isOpen;

                                /// 카메라가 오픈되지 않은 경우 재연결 타이머 동작
                                if (!CamConnected)
                                {
                                    Add_LogData(0, 1, "Camera_Connection : Connection Failed");
                                    if (!ColorCam.CamClose())
                                        Add_LogData(0, 1, ColorCam.ErrorMessage);
                                    isCamReady = false;
                                    timerCameraConnection.Start();
                                }
                                else
                                    continue;
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        Add_LogData(0, 1, ex.Message);
                    }
                    Thread.Sleep(5);

                }
                else
                    Delay(1000);

                //Delay(10);
            }
        }

        // 멀티 쓰레드 테스트 동작 테스트를 위한 영상 획득 프로세스 - 2021.12.16
        private void GrabNormal()
        {
            bool isImageReady = false;
            long grab_time = 0;
            int bufferPos;
            System.Diagnostics.Stopwatch swh = new System.Diagnostics.Stopwatch();

            while (isThreadRunning)
            {
                if (isCamReady)
                {
                    if(!isCamGrabbed)
                    {
                        isImageReady = ColorCam.GetImageAvailable();
                        if (isImageReady)
                        {
                            // 그랩 프로세스
                            swh.Reset(); swh.Start();

                            // 첫번째 버퍼메모리에 이미지 획득과 영상 출력
                            if (isFirstBuffer)
                            {
                                bufferPos = 1;
                                isCamGrabbed = Grab_Process(0);

                                isFirstBuffer = false;
                            }
                            else
                            {
                                bufferPos = 2;
                                isCamGrabbed = Grab_Process(1);

                                isFirstBuffer = true;
                            }

                            swh.Stop();
                            grab_time = swh.Elapsed.Ticks / 10;

                            FrameCount++;

                            // 레시피 데이터에 저장된 프레임카운트 로그 활성화 시 로그 기록
                            if (logFrameCountEnable)
                                Add_LogData(0, 1, String.Format("Grab : {0} Frames - {1} Buffer {2} us", FrameCount, bufferPos, grab_time));

                            // 이미지 해제를 위해 GC를 해제
                            AcqNum++;
                            if (AcqNum > 8)
                            {
                                GC.Collect();
                                AcqNum = 0;
                            }

                            isImageReady = false;
                        }
                        /// 새로운 영상 상태(100번 이상 false인 경우) 체크 후 카메라 연결 상태 모니터링  
                        else
                        {
                            GrabCheckNum++;

                            if (GrabCheckNum >= 100)
                            {
                                GrabCheckNum = 0;

                                // 카메라 오픈 상태 확인
                                ColorCam.GetDeviceOpenStatus();
                                CamConnected = ColorCam._isOpen;

                                /// 카메라가 오픈되지 않은 경우 재연결 타이머 동작
                                if (!CamConnected)
                                {
                                    Add_LogData(0, 1, "Camera_Connection : Connection Failed");
                                    if (!ColorCam.CamClose())
                                        Add_LogData(0, 1, ColorCam.ErrorMessage);
                                    isCamReady = false;
                                    timerCameraConnection.Start();
                                }
                                else
                                    continue;
                            }
                        }
                    }

                    Thread.Sleep(10);
                }
                else
                    Delay(1000);
            }
        }

        private void DisplayNormal()
        {
            long disp_time = 0;
            int bufferPos;
            System.Diagnostics.Stopwatch swh = new System.Diagnostics.Stopwatch();

            while (isThreadRunning)
            {
                if (isCamGrabbed)
                {
                    swh.Reset(); swh.Start();
                    if (!isFirstBuffer)
                    {
                        bufferPos = 1;
                        Display_Image(0);
                    }
                    else
                    {
                        bufferPos = 2;
                        Display_Image(1);
                    }

                    swh.Stop();
                    disp_time = swh.Elapsed.Ticks / 10;

                    isCamGrabbed = false;

                    // 레시피 데이터에 저장된 프레임카운트 로그 활성화 시 로그 기록
                    if (logFrameCountEnable)
                        Add_LogData(0, 1, String.Format("Display : {0} Frames - {1} Buffer {2} us", FrameCount, bufferPos, disp_time));
                }
                else
                    continue;
                Thread.Sleep(20);
            }

        }
        #endregion

        #region Timer 
        // 카메라 연결 상태를 모니터링하는 타이머
        // 연결이 끊기면 재 연결
        private void timerCameraConnection_Tick(object sender, EventArgs e)
        {
            string tempString = "";
            bool camAvailable = false;
            bool systemInitialize = false;
            // 카메라 연결 상태 Flag가 false이거나 셋팅모드 Flag가 false 인 경우 카메라 재 연결 진행
            // 그렇지 않은 경우 프로세스 진행하지 않음
            if (!CamConnected && !isSettingMode)
            {
                
                // 카메라 재연결은 셋팅 모드인 경우 진행
                // VirtualFG 시스템에 대한 초기화는 요구되지 않음 - 2021.10.19 테스트
                // Camguide 프로그램 실행 후 종료한 경우는 Open 프로세스만 진행 - 2021.10.20 테스트 확인
                tempString = "Camera_Connection : Retry Connection";
                Add_LogData(0, 1, tempString);
                
                // VirtualFG40  라이브러리 초기화 없이 진행
                systemInitialize = true;

                if(systemInitialize)
                {
                   
                    // 연결 가능한 카메라 확인
                    if (!ColorCam.UpdateDeviceList())
                    {
                        camAvailable = false;
                    }
                    else
                        camAvailable = true;

                    // 재연결 진행
                    if (camAvailable)
                    {
                        tempString = "Camera_Connection : Find Available Camera";
                        Add_LogData(0, 1, tempString);

                        #region 재연결 프로세스
                        // 실제 재연결 프로세스 
                        if (!ColorCam.ReOpenProcess(GrabMode, isCamCallback))
                        {
                            // 재 연결 실패한 경우  
                            CamConnected = false;
                            tempString = ColorCam.ErrorMessage;
                            Add_LogData(0, 1, tempString);
                            return;
                        }
                        else
                        {
                            // 재 연결 성공한 경우
                            // 연결된 카메라의 설정값 다시 획득과 픽셀 포맷과 해상도 설정
                            // 카메라 연결 상태와 영상 그랩 Thread Flag를  true로 살려줌
                            Get_CameraSettingValue();
                            try
                            {
                                // 카메라 픽셀포맷을 확인
                                if (!Check_PixelFormat())
                                {
                                    tempString = ColorCam.ErrorMessage;
                                    Add_LogData(0, 1, tempString);
                                    throw new Exception();
                                }

                                // 카메라 해상도를 PictureBox에 맞게 설정
                                if (!Set_Resolution())
                                {
                                    tempString = ColorCam.ErrorMessage;
                                    Add_LogData(0, 1, tempString);
                                    throw new Exception();
                                }

                                // 카메라 설정 후 Acquistion 시작
                                if (!Start_Acquistion(isCamCallback))
                                {
                                    tempString = ColorCam.ErrorMessage;
                                    Add_LogData(0, 1, tempString);
                                    throw new Exception();
                                }

                            }
                            catch (Exception ex)
                            {

                                return;
                            }

                            // 카메라가 끊어진 경우 재 연결 시 Close 프로세스 진행을 위행 Flag false
                            // 이 부분 테스트 필요
                            //ColorCam._isCloseProcessed = false;
                            CamConnected = true;
                            isCamReady = true;
                            tempString = "Camera_Connection : " + "ReConnection Succeded";
                            Add_LogData(0, 1, tempString);
                            Delay(400);
                            timerCameraConnection.Stop();
                        }
                        #endregion

                    }
                    // 연결 가능한 카메라를 찾지 못한 경우
                    else
                    {
                        tempString = "Camera_Connection : Can not Find Available Camera";
                        Add_LogData(0, 1, tempString);
                    }
                }
                   
            }
            //else
            //timerCameraConnection.Stop();
        }

        // 카메라 연결 상태 UI 업데이트 표시 타이머
        private void timerConnectionUI_Tick(object sender, EventArgs e)
        {
            //if(!cam_connection_display)
            //{
            //    // 카메라 연결 상태 글자 표시 하지 않기
            //    Update_Display(6);
            //    cam_connection_display = true;
            //}
            //else
            //{
            //    // 카메라 연결 상태에 따라서 글자 표시
            //    if (CamConnected)
            //        Update_Display(4);
            //    else
            //        Update_Display(5);
            //    cam_connection_display = false;
            //}
            if (!CamConnected)
            {
                if (!cam_connection_display)
                {
                    // 카메라 연결 상태 글자 표시 하지 않기
                    Update_Display(6);
                    cam_connection_display = true;
                }
                else
                {
                    Update_Display(5);
                    cam_connection_display = false;
                }
            }
            else
            {

                Update_Display(4);

                /// 카메라 연결 상태를 점멸 동작하는 경우
                //if (!cam_connection_display)
                //{
                //    // 카메라 연결 상태 글자 표시 하지 않기
                //    Update_Display(6);
                //    cam_connection_display = true;
                //}
                //else
                //{
                //    Update_Display(4);
                //    cam_connection_display = false;
                //}
            }
        }

        // 캠가이드 프로그램 실행 상태 모니터링하는 타이머
        private void timerCamguideMonitoring_Tick(object sender, EventArgs e)
        {
            string tempString;
            Process[] processes = null;

            // CamGuide40 프로그램 실행 확인 
            processes = Process.GetProcessesByName("CamGuide40");
            if (processes.Length == 0)
            {

                Add_LogData(0, 1, "System : Setting Mode Finish");

                // Camguide 프로그램 종료 후 충분한 카메라 종료 대기 시간을 위해 딜레이 진행
                // 카메라 Acqstop~Close 진행 택타임 약2000ms 확인, 지연 시간은 3500ms 변경 2020.11.03
                Delay(4500);

                // 셋팅 모드 전환
                // Camguid 프로그램 종료 시 Acquisition Stop 과 카메라 Close 프로세스 이미 진행 
                isSettingMode = false;

                // 캠가이드 모니터링 타이머 Stop
                timerCamguideMonitoring.Stop();

                // 카메라 연결 타이머 Start
                timerCameraConnection.Start();
               
            }
        }

        // 로그 기록을 모니터링후 삭제 타이머
        private void timerLogDelete_Tick(object sender, EventArgs e)
        {
            DateTime currentTime = DateTime.Now;

            if (currentTime.Hour == 18)
            {
                Delete_Log(logSaveDuration);
            }
        }

        // 테스트용 소프트웨어 트리거 타이머 - 2021.11.03
        private void timerCamCheck_Tick(object sender, EventArgs e)
        {
            //ColorCam.GetDeviceOpenStatus();
            //CamConnected = ColorCam._isOpen;
            ColorCam.CreateSoftwareTrigger();
        }
        #endregion

        // UI 구동에 영향을 주지 않는 Delay 프로세스
        private static DateTime Delay(int ms)
        {
            DateTime ThisMoment = DateTime.Now;

            TimeSpan duration = new TimeSpan(0, 0, 0, 0, ms);

            DateTime AfterWards = ThisMoment.Add(duration);

            while (AfterWards >= ThisMoment)
            {
                System.Windows.Forms.Application.DoEvents();
                ThisMoment = DateTime.Now;
            }
            return DateTime.Now;
        }
       
    }
}
