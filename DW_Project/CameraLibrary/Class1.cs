using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading;
using Crevis.VirtualFG40Library;
using System.Drawing;
using System.Drawing.Imaging;

namespace SSCameraLibrary
{
    public class ImageAcqusitionClass
    {
        //카메라 파라미터 클래스
        public class CameraParameter
        {
            // 그랩 모드
            public enum GrabMode
            {
                Normal = 0,
                Software = 1,
                Hardware = 2
            }

            // 픽셀 포맷
            public enum PixelFormat
            {
                Mono8 = 0,
                Mono10 = 1,
                Mono12 = 2,
                Mono10Packed = 3,
                Mono12Packed = 4,
                BayerRG8 = 5,
                BayerRG10 = 6,
                BayerRG12 = 7,
                BayerRG10Packed = 8,
                BayerRG12Packed = 9,
                YUV422Packed = 10
            }

            // 영상의 컬러타입
            public enum ColorType
            {
                Mono=0,
                Color=1
            }

            // 영상의 바이트타입
            public enum ByteType
            {
                One=0,
                Two=1,
                Etc = 2
            }
        }

        //Crevise 영상획득 라이브러리 클래스 생성
        public VirtualFG40Library _virtualFG40 = new VirtualFG40Library();

        #region 변수
        //필드 (데이터)
        public UInt32 _camNum;
        public Int32 _hDevice;
        public Int32 _width;
        public Int32 _height;
        public IntPtr[] _pImages; // 카메라 이미지 멀티 버퍼 메모리 변수
        public IntPtr _pImage;
        public IntPtr[] _cvtImages; // 카메라 이미지 멀티 버퍼 메모리 변수
        public IntPtr _cvtImage;
        private Queue<IntPtr> _qImageBuffer;
        public Int32 _bufferSize;
        private IntPtr _userdata = new IntPtr(); // 콜백 User Define Data
        private GCHandle _gch; // 콜백에서 버퍼 메모리 접근을 위한 변수
        public bool _isOpen = false;
        public bool _isCloseProcessed = false; // 카메라 끊어진 경우 Camera Close 프로세스 확인 변수 
        public bool _isGrabbed = false;
        public string ErrorMessage;
        public Bitmap BitmapImage;
        public string _DeviceID;
        public string _DeviceModelName;
        //public string _PixelFormat;
        public string _TriggerMode;
        public string _TriggerSource;
        public double _ExposureTime;
        public Int32 _GainRaw;
        // 카메라 영상 관련 변수
        public CameraParameter.PixelFormat _PixelFormat;
        public CameraParameter.ColorType _ColorType;
        public CameraParameter.ByteType _ByteType;
        public Int32 _FrameCount;
        public Int32 _PacketLossCount;
        //public Int32 _EventID; // 테스트
        #endregion

        public ImageAcqusitionClass()
        {
            //영상관련 변수
            _camNum = 0;
            _hDevice = 0;
            _width = 0;
            _height = 0;
            //_pImage = new IntPtr();
            //_cvtImage = new IntPtr();
            _bufferSize = 0;
            _FrameCount = 0;
            _PacketLossCount = 0;

        }

        #region Crevis 카메라 프로세스
        //Crevis Camera System Initialize 기능
        public bool Initialize()
        {

            Int32 status = VirtualFG40Library.MCAM_ERR_SUCCESS;
            try
            {
                //System Initialize
                status = _virtualFG40.InitSystem();
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("VirtualFG40 Initialize failed : {0}", status));
                }

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }
        }

        //Camera Open 기능
        public bool CamOpen(UInt32 cameraIndex)
        {
            Int32 status = VirtualFG40Library.MCAM_ERR_SUCCESS;

            try
            {
                status = _virtualFG40.OpenDevice(cameraIndex, ref _hDevice, true);
                Delay(1000);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    // 카메라 재 오픈 가능하도록 FreeSystem 제거 - 21.10.19
                    //_virtualFG40.FreeSystem();
                    throw new Exception(String.Format("Open device failed : {0}", status));
                }

                _isOpen = true;

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message; return false;
            }
        }

        /// Camera Close 기능
        /// 메모리 해제 없이 카메라 종료하는 기능 업데이트 - 2021.11.02
        /// 시스템 종료 하지 않고 메모리 해제 없이 카메라 재연결하는 경우는 _pImage와 _cvtImage 메모리 해제진행하지 않음 
        /// 디폴트는 메모리해제하도록 설정
        public bool CamClose()
        {
            Int32 status = VirtualFG40Library.MCAM_ERR_SUCCESS;

            try
            {
                long us_time = 0;
                long ms_time = 0;
                int a = 0;
                
                status = _virtualFG40.AcqStop(_hDevice);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Acquisition Stop failed : {0}", status));
                }

                
                //카메라 종료 전 GC 핸들러 해제
                //if (_gch != null)
                
                // 할당한 버퍼메모리 만큼 메모리 해제 
                for(int i=0;i<_pImages.Length;i++)
                {
                    _pImages[i] = IntPtr.Zero;
                }
                for (int i = 0; i < _cvtImages.Length; i++)
                    _cvtImages[i] = IntPtr.Zero;
                _pImage = IntPtr.Zero;
                _cvtImage = IntPtr.Zero;

                // Acquisition Stop 처리 시간을 
                Delay(2000);

                status = _virtualFG40.CloseDevice(_hDevice);

                Delay(1000);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Close Device failed : {0}", status));
                }

                _isOpen = false;
                return true;
            }
            catch(Exception ex)
            {
                // 메모리 해제 과정에서 발생하는 시스템 예외 발생시에도 카메라 종료하도록 딜레이 충분하게 진행하도록 구현 - 2021.11.01
                status = _virtualFG40.CloseDevice(_hDevice);
                Delay(5000);

                ErrorMessage = ex.Message; return false;
            }
        }

        //Camera Close 기능 - close 후 지연 기능 추가
        public bool CamClose(int delaytime)
        {
            Int32 status = VirtualFG40Library.MCAM_ERR_SUCCESS;

            try
            {

                status = _virtualFG40.AcqStop(_hDevice);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Acquisition Stop failed : {0}", status));
                }

                //if (_pImage != IntPtr.Zero)
                //{
                //    Marshal.FreeHGlobal(_pImage);
                //    _pImage = IntPtr.Zero;
                //}

                for (int i = 0; i < _pImages.Length; i++)
                {
                    if(_pImages[i] != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(_pImages[i]);
                        _pImages[i] = IntPtr.Zero;
                    }
                }

                for (int i = 0; i < _cvtImages.Length; i++)
                {
                    if (_cvtImages[i] != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(_cvtImages[i]);
                        _cvtImages[i] = IntPtr.Zero;
                    }
                }

                //if (_cvtImage != IntPtr.Zero)
                //{
                //    Marshal.FreeHGlobal(_cvtImage);
                //    _cvtImage = IntPtr.Zero;
                //}

                //Thread.Sleep(1000);


                status = _virtualFG40.CloseDevice(_hDevice);
                Delay(delaytime);

                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Close Device failed : {0}", status));
                }

                //Delay(delaytime);

                _isOpen = false;
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message; return false;
            }
        }
        //폼 종료 시 호출
        public bool FreeSystem()
        {
            Int32 status = VirtualFG40Library.MCAM_ERR_SUCCESS;

            try
            {
               
                status=_virtualFG40.FreeSystem();
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("VirtualFG40 FreeSystem failed: {0}", status));
                }

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message; return false;
            }
           
        }

        //Acquisition 시작
        public bool AcqusitionStart()
        {
            Int32 status = VirtualFG40Library.MCAM_ERR_SUCCESS;
            try
            {
                // Acqusition Start
                status = _virtualFG40.AcqStart(_hDevice);
                Delay(30);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Acqusition Start failed : {0}", status));
                }
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }
        }

        //Acquisition 중지
        public void AcqusitionStop()
        {
            Int32 status = VirtualFG40Library.MCAM_ERR_SUCCESS;
            try
            {
                // Acqusition Start
                status = _virtualFG40.AcqStop(_hDevice);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Acqusition Stop failed : {0}", status));
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        // 트리거용 GrabAsyn 시작
        public bool GrabAsyncStart()
        {
            Int32 status = VirtualFG40Library.MCAM_ERR_SUCCESS;
            try
            {
                //uint maxdelay = 20;
                status = _virtualFG40.GrabStartAsync(_hDevice, 0xFFFFFFFF);
                //status = _virtualFG40.GrabStartAsync(_hDevice, maxdelay);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Grab Start Aynchronous Mode failed : {0}", status));
                }
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message; return false;
            }
        }

        // Normal(Free-run) 모드
        // MonoGrab Sync - Continuous 모드
        /// 멀티 이미지 버퍼 메모리 중 해당 인덱스 버퍼메모리에 저장 
        public bool GrabMonoImageSync(UInt32 BufferIndex)
        {
            Int32 status = VirtualFG40Library.MCAM_ERR_SUCCESS;
            try
            {
                // MonoGrab Function
                status = _virtualFG40.GrabImage(_hDevice, _pImages[BufferIndex], (UInt32)_bufferSize);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Grab Mono Image failed : {0}", status));
                }
                _isGrabbed = true;
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }
            
        }

        /// ColorGrab Sync - Continuous 모드
        /// 멀티 이미지 버퍼 메모리 중 해당 인덱스 버퍼메모리에 저장 
        public bool GrabColorImageSync(UInt32 BufferIndex)
        {
            Int32 status = VirtualFG40Library.MCAM_ERR_SUCCESS;
            try
            {
                // Grab Function
                status = _virtualFG40.GrabImage(_hDevice, _pImages[BufferIndex], (UInt32)_bufferSize);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Grab Color Image failed : {0}", status));
                }
                // 획득된 버퍼메모리 데이터에 대해서 컬러로 변환
                // 8비트에 대해서만 변환
                // 10, 12비트에 대해서는 추후 업데이트 예정 - 2021.10.21
                if (_PixelFormat == CameraParameter.PixelFormat.BayerRG8  /*||_PixelFormat == CameraParameter.PixelFormat.BayerRG10 || _PixelFormat == CameraParameter.PixelFormat.BayerRG12*/)
                {
                    status = _virtualFG40.CvtColor(_pImages[BufferIndex], _cvtImages[BufferIndex], _width, _height, VirtualFG40Library.CV_BayerRG2RGB);
                    if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                    {
                        throw new Exception(String.Format("Convert Bayer to RGB Color Image failed : {0}", status));
                    }
                }

                else if (_PixelFormat ==CameraParameter.PixelFormat.YUV422Packed)
                {
                    status = _virtualFG40.CvtColor(_pImages[BufferIndex], _cvtImages[BufferIndex], _width, _height, VirtualFG40Library.CV_YUV2BGR_UYVY);
                    if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                    {
                        throw new Exception(String.Format("Convert YUV to RGB Color Image failed : {0}", status));
                    }
                }
                _isGrabbed = true;

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }
        }

        // 소프트웨어 트리거 1회 발생(노광시작)
        public void CreateSoftwareTrigger()
        {
            _virtualFG40.SetCmdReg(_hDevice, VirtualFG40Library.MCAM_TRIGGER_SOFTWARE);
        }

        /// MonoGrab Async 1회 실행
        /// 멀티 이미지 버퍼 메모리 중 해당 인덱스 버퍼메모리에 저장 
        public bool GrabMonoImageAsync(UInt32 BufferIndex)
        {
            Int32 status = VirtualFG40Library.MCAM_ERR_SUCCESS;
            try
            {
                #region Single buffer Image Grab
                // MonoGrab Function
                Marshal.WriteIntPtr(_pImages[BufferIndex], ((IntPtr)(0)));
                status = _virtualFG40.GrabImageAsync(_hDevice, _pImages[BufferIndex], (UInt32)_bufferSize, 0xFFFFFFFF);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Grab Mono Image failed : {0}", status));
                }
                #endregion

                #region Single buffer Image Grab
                //// MonoGrab Function
                //status = _virtualFG40.GrabImageAsync(_hDevice, _pImage, (UInt32)_bufferSize, 0xFFFFFFFF);
                //if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                //{
                //    throw new Exception(String.Format("Grab Mono Image failed : {0}", status));
                //}
                #endregion
                _isGrabbed = true;

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }
        }

        /// Color Image ColorGrab Asyn 1회 실행
        /// 멀티 이미지 버퍼 메모리 중 해당 인덱스 버퍼메모리에 저장 
        public bool GrabColorImageAsysn(UInt32 BufferIndex)
        {
            Int32 status = VirtualFG40Library.MCAM_ERR_SUCCESS;

            try
            {
                #region Double Buffer Image Grab
                Marshal.WriteIntPtr(_pImages[BufferIndex], ((IntPtr)(0)));
                Marshal.WriteIntPtr(_cvtImages[BufferIndex], ((IntPtr)(0)));
                //if (BufferIndex == 0)
                //{
                //    status = _virtualFG40.GrabImageAsync(_hDevice, _pImages[BufferIndex], (UInt32)_bufferSize, 0xFFFFFFFF);
                //    if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                //    {
                //        throw new Exception(String.Format("Grab Color Image failed : {0}", status));
                //    }
                //}

                status = _virtualFG40.GrabImageAsync(_hDevice, _pImages[BufferIndex], (UInt32)_bufferSize, 0xFFFFFFFF);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Grab Color Image failed : {0}", status));
                }

                if (_PixelFormat == CameraParameter.PixelFormat.BayerRG8
                        /*|| _PixelFormat == CameraParameter.PixelFormat.BayerRG10 || _PixelFormat == CameraParameter.PixelFormat.BayerRG12*/)
                {
                    status = _virtualFG40.CvtColor(_pImages[BufferIndex], _cvtImages[BufferIndex], _width, _height, VirtualFG40Library.CV_BayerRG2RGB);
                    if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                    {
                        throw new Exception(String.Format("Convert Bayer to RGB Color Image failed : {0}", status));
                    }
                }
                // YUV 포맷에 대해서 컬러 변환
                else if (_PixelFormat == CameraParameter.PixelFormat.YUV422Packed)
                {
                    status = _virtualFG40.CvtColor(_pImages[BufferIndex], _cvtImages[BufferIndex], _width, _height, VirtualFG40Library.CV_YUV2BGR_UYVY);
                    if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                    {
                        throw new Exception(String.Format("Convert YUV to RGB Color Image failed : {0}", status));
                    }

                }
                #endregion


                #region Single buffer Image Grab
                //// Grab Function
                //status = _virtualFG40.GrabImageAsync(_hDevice, _pImage, (UInt32)_bufferSize, 0xFFFFFFFF);
                //if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                //{
                //    throw new Exception(String.Format("Grab Color Image failed : {0}", status));
                //}

                ////Thread.Sleep(300);
                ////GetImageAvailable();

                //// 획득된 버퍼메모리 데이터에 대해서 컬러로 변환
                //// 8비트에 대해서만 변환
                //// 10, 12비트에 대해서는 추후 업데이트 예정 - 2021.10.21
                //if (_PixelFormat == CameraParameter.PixelFormat.BayerRG8 /*|| _PixelFormat == CameraParameter.PixelFormat.BayerRG10 || _PixelFormat == CameraParameter.PixelFormat.BayerRG12*/)
                //{
                //    status=_virtualFG40.CvtColor(_pImage, _cvtImage, _width, _height, VirtualFG40Library.CV_BayerRG2RGB);
                //    if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                //    {
                //        throw new Exception(String.Format("Convert Bayer to RGB Color Image failed : {0}", status));
                //    }
                //}
                //// YUV 포맷에 대해서 컬러 변환
                //else if(_PixelFormat == CameraParameter.PixelFormat.YUV422Packed)
                //{
                //    status = _virtualFG40.CvtColor(_pImage, _cvtImage, _width, _height, VirtualFG40Library.CV_YUV2BGR_UYVY);
                //    if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                //    {
                //        throw new Exception(String.Format("Convert YUV to RGB Color Image failed : {0}", status));
                //    }

                //}
                #endregion

                _isGrabbed = true;

                return true;
            }
            catch(Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }

        }

        // 영상 획득 되었는지 확인 함수 테스트 -2021.10.22
        public bool GetImageAvailable()
        {
            uint grab = 0;
            // Grab Function
            _virtualFG40.GetImageAvailable(_hDevice, ref grab);
            if (grab == 0)
            {
                _isGrabbed = false;
            }
            else
                _isGrabbed = true;
            return _isGrabbed;
        }

        //연결되어 있는 카메라 목록 Update
        public bool UpdateDeviceList()
        {
            Int32 status = VirtualFG40Library.MCAM_ERR_SUCCESS;

            try
            {
                // Update Device List
                status = _virtualFG40.UpdateDevice();
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    // 카메라 재 오픈 가능하도록 FreeSystem 제거 - 21.11.03
                    //_virtualFG40.FreeSystem();
                    throw new Exception(String.Format("Update Device list failed : {0}", status));
                }

                status = _virtualFG40.GetAvailableCameraNum(ref _camNum);
                if (_camNum <= 0)
                {
                    // 카메라 재 오픈 가능하도록 FreeSystem 제거 - 21.11.03
                    //_virtualFG40.FreeSystem();
                    throw new Exception(string.Format("Connection Camera Failed : {0}", _camNum));
                }

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }

        }

        // 카메라의 사이즈를 획득&버퍼메모리 할당
        // 카메라 픽셀포맷 기준 픽셀 당 바이트 수와 컬러타입을 확인 메모리 할당
        public bool AllocateImageBuffer(UInt32 MaxNumberBuffer)
        {
            Int32 status = VirtualFG40Library.MCAM_ERR_SUCCESS;

            _pImages = new IntPtr[MaxNumberBuffer];
            _cvtImages = new IntPtr[MaxNumberBuffer];
            _pImage = new IntPtr();
            _cvtImage = new IntPtr();
            _qImageBuffer = new Queue<IntPtr>();

            try
            {
                status = _virtualFG40.GetIntReg(_hDevice, VirtualFG40Library.MCAM_WIDTH, ref _width);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Read Register failed : {0}", status));
                }

                // Get Height
                status = _virtualFG40.GetIntReg(_hDevice, VirtualFG40Library.MCAM_HEIGHT, ref _height);

                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Read Register failed : {0}", status));
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message; return false;
            }

            // 모노 카메라의 경우 _pImage 메모리 할당
            if(_ColorType==CameraParameter.ColorType.Mono)
            {
                // 픽셀 당 바이트 확인 메모리 할당
                // 픽셀 당 1바이트인 경우
                if (_ByteType == CameraParameter.ByteType.One)
                {
                    //버퍼 사이즈 저장
                    _bufferSize = _width * _height;

                    //Image 버퍼 메모리 할당
                    _pImage = Marshal.AllocHGlobal(_bufferSize);

                    // Image 더블 버퍼로 메모리 할당
                    for (int i = 0; i < _pImages.Length; i++)
                    {
                        _pImages[i] = Marshal.AllocHGlobal(_bufferSize);
                    }
                }
                // 픽셀 당 2바이트인 경우
                else if (_ByteType == CameraParameter.ByteType.Two)
                {
                    //버퍼 사이즈 저장
                    _bufferSize = _width * _height * 2;

                    //Image 버퍼 메모리 할당
                    _pImage = Marshal.AllocHGlobal(_bufferSize);

                    // Image 더블 버퍼로 메모리 할당
                    for (int i = 0; i < _pImages.Length; i++)
                    {
                        _pImages[i] = Marshal.AllocHGlobal(_bufferSize);
                    }
                }
            }
                       
            // 컬러 포맷인 경우 컬러 영상을 위한 버퍼 메모리 할당 - 컬러 포맷 판단 구현 예정
            else
            {
                if (_ByteType == CameraParameter.ByteType.One)
                {
                    //버퍼 사이즈 저장
                    _bufferSize = _width * _height;

                    //Image 버퍼 메모리 할당
                    _pImage = Marshal.AllocHGlobal(_bufferSize);
                    _cvtImage = Marshal.AllocHGlobal(_bufferSize * 3);

                    // Image 더블 버퍼로 메모리 할당
                    for (int i = 0; i < _pImages.Length; i++)
                    {
                        _pImages[i] = Marshal.AllocHGlobal(_bufferSize);
                    }

                    for (int i = 0; i < _cvtImages.Length; i++)
                        _cvtImages[i] = Marshal.AllocHGlobal(_bufferSize * 3);
                }
                // 픽셀 당 2바이트인 경우
                else if (_ByteType == CameraParameter.ByteType.Two)
                {
                    //버퍼 사이즈 저장
                    _bufferSize = _width * _height * 2;

                    //Image 버퍼 메모리 할당
                    _pImage = Marshal.AllocHGlobal(_bufferSize);
                    _cvtImage = Marshal.AllocHGlobal(_bufferSize * 3);


                    // Image 더블 버퍼로 메모리 할당
                    for (int i = 0; i < _pImages.Length; i++)
                    {
                        _pImages[i] = Marshal.AllocHGlobal(_bufferSize);
                    }

                    for (int i = 0; i < _cvtImages.Length; i++)
                        _cvtImages[i] = Marshal.AllocHGlobal(_bufferSize * 3);
                }
            }

            return true;

        }
        
        // 그랩 콜백 할당 프로세스
        public void GrabCallback()
        {
            VirtualFG40Library.CallbackFunc grabCallback = new VirtualFG40Library.CallbackFunc(OnGrabCallback);
            VirtualFG40Library.ST_SetCallbackFunction(_hDevice, VirtualFG40Library.EVENT_NEW_IMAGE, grabCallback, _userdata);
            _gch = GCHandle.Alloc(grabCallback);
        }

        // 콜백 영상 프로세스
        public Int32 OnGrabCallback(Int32 EventID, IntPtr pImage, IntPtr userData)
        {
            //Int32 status = VirtualFG40Library.MCAM_ERR_SUCCESS;
            ////_EventID = EventID;

            //_FrameCount++;

            //if (EventID==VirtualFG40Library.MCAM_ERR_TIMEOUT)
            //{
            //    _PacketLossCount++;
            //    return -1;
            //}

            //else if (EventID != VirtualFG40Library.EVENT_NEW_IMAGE)
            //{
            //    return -1;
            //}
            ////return -1;

            //// 모노카메라 영상 획득 프로세스
            //if (_ColorType == CameraParameter.ColorType.Mono)
            //{
            //    //_pImage = pImage;

            //    _pImages[0] = pImage;
            //}
            //// 컬러카메라 영상 획득 프로세스
            //else
            //{
            //    try
            //    {
            //        if (_PixelFormat == CameraParameter.PixelFormat.BayerRG8 /*|| _PixelFormat == CameraParameter.PixelFormat.BayerRG10 || _PixelFormat == CameraParameter.PixelFormat.BayerRG12*/)
            //        {
            //            status = _virtualFG40.CvtColor(pImage, _cvtImages[0], _width, _height, VirtualFG40Library.CV_BayerRG2RGB);
            //            if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
            //            {
            //                throw new Exception(String.Format("RGB Format Convert failed : {0}", status));
            //            }
            //        }
            //        // YUV 포맷에 대해서 컬러 변환
            //        else if (_PixelFormat == CameraParameter.PixelFormat.YUV422Packed)
            //        {

            //            status = _virtualFG40.CvtColor(pImage, _cvtImages[0], _width, _height, VirtualFG40Library.CV_YUV2BGR_UYVY);
            //            if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
            //            {
            //                throw new Exception(String.Format("YUV Format Convert failed : {0}", status));
            //            }
            //        }

            //    }
            //    catch (Exception ex)
            //    {
            //        _isGrabbed = false;
            //        ErrorMessage = ex.Message; return -1;
            //    }
            //}

            ////_FrameCount++;
            //_isGrabbed = true;

            //return 0;
            if (EventID != VirtualFG40Library.EVENT_NEW_IMAGE)
            {
                _isGrabbed = false;
                return -1;
            }
            else
            {
                _qImageBuffer.Enqueue(pImage);
                _isGrabbed = true;

                return 0;
            }
            
        }
        #endregion

        #region Crevis 카메라 기능 설정값 획득
        // 카메라의 가로 사이즈를 획득
        public bool GetDeviceWidth()
        {
            Int32 status = VirtualFG40Library.MCAM_ERR_SUCCESS;
            try
            {
                status = _virtualFG40.GetIntReg(_hDevice, VirtualFG40Library.MCAM_WIDTH, ref _width);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Read Register failed : {0}", status));
                }
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message; return false;
            }

        }

        // 카메라의 세로 사이즈를 획득
        public bool GetDeviceHeight()
        {
            Int32 status = VirtualFG40Library.MCAM_ERR_SUCCESS;
            try
            {
                status = _virtualFG40.GetIntReg(_hDevice, VirtualFG40Library.MCAM_HEIGHT, ref _height);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Read Register failed : {0}", status));
                }
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message; return false;
            }

        }

        // 카메라의 연결 상태를 가져옴
        public bool GetDeviceOpenStatus()
        {
            Int32 status = VirtualFG40Library.MCAM_ERR_SUCCESS;
            bool connected=false;
            try
            {
                status= _virtualFG40.IsOpenDevice(_hDevice, ref connected);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Read Camera Status Register failed : {0}", status));
                }

                _isOpen = connected;
                //_isCloseProcessed = false;
                return true;
            }
            catch(Exception ex)
            {
                ErrorMessage = ex.Message; return false;
            }
        }

        // 카메라 디바이스 모델네임을 가져옴
        public bool GetModelName()
        {
            UInt32 size = 256;
            Byte[] pInfo;
            Int32 status = VirtualFG40Library.MCAM_ERR_SUCCESS;
            // EnumNum 확인 필요
            UInt32 EnumNum = 0;

            try
            {
                pInfo = new Byte[256];
                //status = _virtualFG40.GetEnumDeviceID(0, pInfo, ref size);
                status = _virtualFG40.GetStrReg(_hDevice, VirtualFG40Library.MCAM_DEVICE_ID, pInfo, ref size);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Read DeviceModelName failed : {0}", status));
                }
                _DeviceModelName = System.Text.Encoding.Default.GetString(pInfo).Trim('\0');

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }
        }

        // 카메라 디바이스 ID(시리얼넘버)를 가져옴
        public bool GetDeviceID()
        {
            UInt32 size = 256;
            Byte[] pInfo;
            Int32 status = VirtualFG40Library.MCAM_ERR_SUCCESS;
          
            try
            {
                pInfo = new Byte[256];
                //status = _virtualFG40.GetEnumDeviceID(EnumNum, pInfo, ref size);
                status = _virtualFG40.GetStrReg(_hDevice, VirtualFG40Library.MCAM_DEVICE_ID, pInfo, ref size);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Read DeviceID failed : {0}", status));
                }
                _DeviceID = System.Text.Encoding.Default.GetString(pInfo).Trim('\0');

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }
        }

        // 카메라 픽셀포맷 가져옴
        public bool GetPixelFormat()
        {
            UInt32 size = 256;
            Byte[] pInfo;
            Int32 status = VirtualFG40Library.MCAM_ERR_SUCCESS;
            string format;
            try
            {
                pInfo = new Byte[256];

                status = _virtualFG40.GetEnumReg(_hDevice, VirtualFG40Library.MCAM_PIXEL_FORMAT, pInfo, ref size);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Read Pixel Format failed : {0}", status));
                }

                format = System.Text.Encoding.Default.GetString(pInfo).Trim('\0');

                Get_CamImage_Parameter(format);

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }
        }

        // String 픽셀 포맷 값으로 부터 카메라 영상 파라미터 값 획득 
        private void Get_CamImage_Parameter(string strFormat)
        {
            switch (strFormat)
            {
                case "Mono8":
                    _PixelFormat = CameraParameter.PixelFormat.Mono8;
                    _ColorType = CameraParameter.ColorType.Mono;
                    _ByteType = CameraParameter.ByteType.One;
                    break;
                case "Mono10":
                    _PixelFormat = CameraParameter.PixelFormat.Mono10;
                    _ColorType = CameraParameter.ColorType.Mono;
                    _ByteType = CameraParameter.ByteType.Two;
                    break;
                case "Mono12":
                    _PixelFormat = CameraParameter.PixelFormat.Mono12;
                    _ColorType = CameraParameter.ColorType.Mono;
                    _ByteType = CameraParameter.ByteType.Two;
                    break;
                case "Mono10Packed":
                    _PixelFormat = CameraParameter.PixelFormat.Mono10Packed;
                    _ColorType = CameraParameter.ColorType.Mono;
                    _ByteType = CameraParameter.ByteType.Etc;
                    break;
                case "Mono12Packed":
                    _PixelFormat = CameraParameter.PixelFormat.Mono12Packed;
                    _ColorType = CameraParameter.ColorType.Mono;
                    _ByteType = CameraParameter.ByteType.Etc;
                    break;
                case "BayerRG8":
                    _PixelFormat = CameraParameter.PixelFormat.BayerRG8;
                    _ColorType = CameraParameter.ColorType.Color;
                    _ByteType = CameraParameter.ByteType.One;
                    break;
                case "BayerRG10":
                    _PixelFormat = CameraParameter.PixelFormat.BayerRG10;
                    _ColorType = CameraParameter.ColorType.Color;
                    _ByteType = CameraParameter.ByteType.Two;
                    break;
                case "BayerRG12":
                    _PixelFormat = CameraParameter.PixelFormat.BayerRG12;
                    _ColorType = CameraParameter.ColorType.Color;
                    _ByteType = CameraParameter.ByteType.Two;
                    break;
                case "BayerRG10Packed":
                    _PixelFormat = CameraParameter.PixelFormat.BayerRG10Packed;
                    _ColorType = CameraParameter.ColorType.Color;
                    _ByteType = CameraParameter.ByteType.Etc;
                    break;
                case "BayerRG12Packed":
                    _PixelFormat = CameraParameter.PixelFormat.BayerRG12Packed;
                    _ColorType = CameraParameter.ColorType.Color;
                    _ByteType = CameraParameter.ByteType.Etc;
                    break;
                case "YUV422Packed":
                    _PixelFormat = CameraParameter.PixelFormat.YUV422Packed;
                    _ColorType = CameraParameter.ColorType.Color;
                    _ByteType = CameraParameter.ByteType.Two;
                    break;
            }
        }

        // 카메라 트리거모드를 가져옴
        public bool GetTriggerMode()
        {
            UInt32 size = 256;
            Byte[] pInfo;
            Int32 status = VirtualFG40Library.MCAM_ERR_SUCCESS;
            try
            {
                pInfo = new Byte[256];

                status = _virtualFG40.GetEnumReg(_hDevice, VirtualFG40Library.MCAM_TRIGGER_MODE, pInfo, ref size);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Read Trigger Mode failed : {0}", status));
                }

                string mode = System.Text.Encoding.Default.GetString(pInfo).Trim('\0');

                _TriggerMode = mode;
                //if (mode == "")
                //    _TriggerMode = true;
                //else
                //    _TriggerMode = false;
                
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }
        }

        //카메라 트리거소스를 가져옴
        public bool GetTriggerSource()
        {
            UInt32 size = 256;
            Byte[] pInfo;
            Int32 status = VirtualFG40Library.MCAM_ERR_SUCCESS;
            try
            {
                pInfo = new Byte[256];

                status = _virtualFG40.GetEnumReg(_hDevice, VirtualFG40Library.MCAM_TRIGGER_SOURCE, pInfo, ref size);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Read Trigger Source failed : {0}", status));
                }

                _TriggerSource = System.Text.Encoding.Default.GetString(pInfo).Trim('\0');

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }
        }

        // 카메라의 노출시간값을 가져옴
        public bool GetExposureTime()
        {
            Int32 status = VirtualFG40Library.MCAM_ERR_SUCCESS;
            try
            {
                status = _virtualFG40.GetFloatReg(_hDevice, VirtualFG40Library.MCAM_EXPOSURE_TIME, ref _ExposureTime);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Read Register failed : {0}", status));
                }

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message; return false;
            }
        }

        //카메라의 Gain값을 가져옴
        public float GetGainAbsolute()
        {
            Int32 status = VirtualFG40Library.MCAM_ERR_SUCCESS;
            try
            {
                status = _virtualFG40.GetIntReg(_hDevice, VirtualFG40Library.MCAM_GAIN_RAW, ref _GainRaw);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Read Register failed : {0}", status));
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            return (float)_GainRaw / 65536;
        }
        #endregion

        #region Crevis 카메라 기능 설정
        // 카메라의 가로, 세로 해상도 설정
        public bool SetResolution(int setWidth, int setHeight)
        {
            Int32 status = VirtualFG40Library.MCAM_ERR_SUCCESS;
            try
            {
                status = _virtualFG40.SetIntReg(_hDevice, VirtualFG40Library.MCAM_WIDTH, setWidth);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Device Width Set failed : {0}", status));
                }
                status = _virtualFG40.SetIntReg(_hDevice, VirtualFG40Library.MCAM_HEIGHT, setHeight);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Device Height Set failed : {0}", status));
                }

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message; return false;
            }

        }

        // 카메라의 OffsetX, OffsetY 설정
        public bool SetOffset(int offsetX, int offsetY)
        {
            Int32 status = VirtualFG40Library.MCAM_ERR_SUCCESS;
            try
            {
                status = _virtualFG40.SetIntReg(_hDevice, VirtualFG40Library.MCAM_OFFSET_X, offsetX);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Device OffsetX Set failed : {0}", status));
                }
                status = _virtualFG40.SetIntReg(_hDevice, VirtualFG40Library.MCAM_OFFSET_Y, offsetY);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Device OffsetY Set failed : {0}", status));
                }

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message; return false;
            }
        }
        //ExposureTIme 설정
        public bool SetExposureTime(float inputExposureTime)
        {
            Int32 status = VirtualFG40Library.MCAM_ERR_SUCCESS;
            try
            {
                status = _virtualFG40.SetFloatReg(_hDevice, VirtualFG40Library.MCAM_EXPOSURE_TIME, inputExposureTime);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Exposure Time Set failed : {0}", status));
                }
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message; return false;

            }
        }

        // Gain 절대값 설정
        public bool SetGainAbsolute(float inputGainValue)
        {
            //1부터 7.9까지가 설정가능한 범위 이므로 7.9보다 큰 값이 들어오면 7.9, 1보다 작은 값이 들어오면 1로 자동설정.
            if (inputGainValue >= 7.9)
                inputGainValue = 7.9f;

            if (inputGainValue < 1)
                inputGainValue = 1;

            Int32 status = VirtualFG40Library.MCAM_ERR_SUCCESS;

            // Gain변경은 Raw값을 입력을 해줘야 하므로 65536 곱해준 값을 곱하여 Raw값 계산
            // Raw값은 int형만 가능하여 계산 후 반올림 진행
            inputGainValue = inputGainValue * 65536;
            int GainRaw = (int)inputGainValue;

            try
            {
                status = _virtualFG40.SetIntReg(_hDevice, VirtualFG40Library.MCAM_GAIN_RAW, GainRaw);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Gain Absolute Set failed : {0}", status));

                }

                return true;
            }
            catch(Exception ex)
            {
                ErrorMessage = ex.Message; return false;

            }
        }

        // Device Filter Driver 설정
        public bool SetDeviceFilterDriver(bool enabled)
        {
            Int32 status = VirtualFG40Library.MCAM_ERR_SUCCESS;

            try
            {
                if(enabled)
                    status = _virtualFG40.SetEnumReg(_hDevice, VirtualFG40Library.MCAM_DEVICE_FILTER_DRIVER_MODE, 
                          VirtualFG40Library.DEVICE_FILTER_DRIVER_MODE_ON);
                else
                    status = _virtualFG40.SetEnumReg(_hDevice, VirtualFG40Library.MCAM_DEVICE_FILTER_DRIVER_MODE,
                              VirtualFG40Library.DEVICE_FILTER_DRIVER_MODE_OFF);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Device Filter Driver Set failed : {0}", status));
                }

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message; return false;
            }
        }

        // MaxPacketResendCount 설정
        public bool SetMaxPacketResendCount(int count)
        {
            Int32 status = VirtualFG40Library.MCAM_ERR_SUCCESS;

            try
            {
                status = _virtualFG40.SetIntReg(_hDevice, VirtualFG40Library.MCAM_DEVICE_MAX_PACKET_RESEND_COUNT, count);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Max Packet Resend Set failed : {0}", status));
                }

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message; return false;
            }
        }

        // PacketDelay 설정
        public bool SetPacketDelay(int delay)
        {
            Int32 status = VirtualFG40Library.MCAM_ERR_SUCCESS;

            try
            {
                status = _virtualFG40.SetIntReg(_hDevice, VirtualFG40Library.GEV_SCPD, delay);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Packet Delay Set failed : {0}", status));
                }

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message; return false;
            }
        }

        // Missing Pakcet 표시 설정
        public bool SetMissingPacketDisplay()
        {
            Int32 status = VirtualFG40Library.MCAM_ERR_SUCCESS;

            try
            {
                status = _virtualFG40.SetEnumReg(_hDevice, VirtualFG40Library.MCAM_DEVICE_MISSING_PACKET_RECEIVE,
                              VirtualFG40Library.DEVICE_MISSING_PACKET_RECEIVE_ON);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Missing Packet Display Set failed : {0}", status));
                }

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message; return false;
            }
        }

            // Pixel Format 설정
        public bool SetPixelFormat(CameraParameter.PixelFormat pixelFormat)
        {
            Int32 status = VirtualFG40Library.MCAM_ERR_SUCCESS;

            string strPixelFormat=Set_CamImage_StringPixelFormat(pixelFormat);
            try
            {
                status = _virtualFG40.SetEnumReg(_hDevice, VirtualFG40Library.MCAM_PIXEL_FORMAT, strPixelFormat);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Pixel Format Set failed : {0}", status));
                }

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message; return false;

            }
        }

        // 카메라 영상 파라미터 값에 대해서 String 픽셀 포맷 값으로 변환 
        private string Set_CamImage_StringPixelFormat(CameraParameter.PixelFormat pixelFormat)
        {
            string strPixelFormat = "";
            switch (pixelFormat)
            {
                case CameraParameter.PixelFormat.Mono8:
                    strPixelFormat = "Mono8";
                    break;
                case CameraParameter.PixelFormat.Mono10:
                    strPixelFormat = "Mono10";
                    break;
                case CameraParameter.PixelFormat.Mono12:
                    strPixelFormat = "Mono12";
                    break;
                case CameraParameter.PixelFormat.Mono10Packed:
                    strPixelFormat = "Mono10Packed";
                    break;
                case CameraParameter.PixelFormat.Mono12Packed:
                    strPixelFormat = "Mono12Packed";
                    break;
                case CameraParameter.PixelFormat.BayerRG8:
                    strPixelFormat = "BayerRG8";
                    break;
                case CameraParameter.PixelFormat.BayerRG10:
                    strPixelFormat = "BayerRG10";
                    break;
                case CameraParameter.PixelFormat.BayerRG12:
                    strPixelFormat = "BayerRG12";
                    break;
                case CameraParameter.PixelFormat.BayerRG10Packed:
                    strPixelFormat = "BayerRG10Packed";
                    break;
                case CameraParameter.PixelFormat.BayerRG12Packed:
                    strPixelFormat = "BayerRG12Packed";
                    break;
                case CameraParameter.PixelFormat.YUV422Packed:
                    strPixelFormat = "YUV422Packed";
                    break;
            }
            return strPixelFormat;
        }

        //카메라 초기 설정 시 Software TriggerMode 사용할 수 있도록 세팅
        public bool SetFeature_SoftwareTriggerMode()
        {
            Int32 status = VirtualFG40Library.MCAM_ERR_SUCCESS;
            try
            {
                // Set Trigger Mode
                status = _virtualFG40.SetEnumReg(_hDevice, VirtualFG40Library.MCAM_TRIGGER_MODE, VirtualFG40Library.TRIGGER_MODE_ON);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Write Register failed : {0}", status));
                }

                //TriggerSource => Software
                status = _virtualFG40.SetEnumReg(_hDevice, VirtualFG40Library.MCAM_TRIGGER_SOURCE, VirtualFG40Library.TRIGGER_SOURCE_SOFTWARE);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Write Register failed : {0}", status));
                }

                //TriggerDelay => 1us
                status = _virtualFG40.SetFloatReg(_hDevice, VirtualFG40Library.MCAM_TRIGGER_DELAY, 1.0f);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Write Register failed : {0}", status));
                }

                //// Set ExposureMode
                //status = _virtualFG40.SetEnumReg(_hDevice, VirtualFG40Library.MCAM_EXPOSURE_MODE, VirtualFG40Library.EXPOSURE_MODE_TIMED);
                //if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                //{
                //    throw new Exception(String.Format("Write Register failed : {0}", status));
                //}

                //// Set ExposureTime
                //status = _virtualFG40.SetFloatReg(_hDevice, VirtualFG40Library.MCAM_EXPOSURE_TIME, 1000);
                //if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                //{
                //    throw new Exception(String.Format("Write Register failed : {0}", status));
                //}

                // 컬러 카메라의 경우 픽셀포맷 설정 X
                //// Set PixelFormat
                //status = _virtualFG40.SetEnumReg(_hDevice, VirtualFG40Library.MCAM_PIXEL_FORMAT, VirtualFG40Library.PIXEL_FORMAT_MONO8);
                //if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                //{
                //    throw new Exception(String.Format("Write Register failed : {0}", status));
                //}

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }
        }

        //카메라 초기 설정 시 Hardware TriggerMode 사용할 수 있도록 세팅
        public bool SetFeature_HardwareTriggerMode()
        {
            Int32 status = VirtualFG40Library.MCAM_ERR_SUCCESS;
            try
            {
                // Set Trigger Mode
                status = _virtualFG40.SetEnumReg(_hDevice, VirtualFG40Library.MCAM_TRIGGER_MODE, VirtualFG40Library.TRIGGER_MODE_ON);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Set Trigger Mode failed : {0}", status));
                }

                //TriggerSource => Software
                status = _virtualFG40.SetEnumReg(_hDevice, VirtualFG40Library.MCAM_TRIGGER_SOURCE, VirtualFG40Library.TRIGGER_SOURCE_LINE1);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Set Trigger Source failed : {0}", status));
                }

                //TriggerDelay => 1us
                status = _virtualFG40.SetFloatReg(_hDevice, VirtualFG40Library.MCAM_TRIGGER_DELAY, 1.0f);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Set Trigger Delay failed : {0}", status));
                }

                // GrabTimeout => 50ms
                status = _virtualFG40.SetGrabTimeout(_hDevice, 50);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Set Grab Timeout failed : {0}", status));
                }


                //// Set ExposureMode
                //status = _virtualFG40.SetEnumReg(_hDevice, VirtualFG40Library.MCAM_EXPOSURE_MODE, VirtualFG40Library.EXPOSURE_MODE_TIMED);
                //if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                //{
                //    throw new Exception(String.Format("Write Register failed : {0}", status));
                //}

                //// Set ExposureTime
                //status = _virtualFG40.SetFloatReg(_hDevice, VirtualFG40Library.MCAM_EXPOSURE_TIME, 1000);
                //if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                //{
                //    throw new Exception(String.Format("Write Register failed : {0}", status));
                //}

                // 컬러 카메라의 경우 픽셀포맷 설정 X
                //// Set PixelFormat
                //status = _virtualFG40.SetEnumReg(_hDevice, VirtualFG40Library.MCAM_PIXEL_FORMAT, VirtualFG40Library.PIXEL_FORMAT_MONO8);
                //if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                //{
                //    throw new Exception(String.Format("Write Register failed : {0}", status));
                //}

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }
        }

        //카메라 초기 설정 시 NormalMode로 사용할 수 있도록 세팅
        public bool SetFeature_NormalMode()
        {

            Int32 status = VirtualFG40Library.MCAM_ERR_SUCCESS;
            try
            {

                // Set Trigger Mode
                status = _virtualFG40.SetEnumReg(_hDevice, VirtualFG40Library.MCAM_TRIGGER_MODE, VirtualFG40Library.TRIGGER_MODE_OFF);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Write Register failed : {0}", status));
                }

                // Set ExposureMode
                status = _virtualFG40.SetEnumReg(_hDevice, VirtualFG40Library.MCAM_EXPOSURE_MODE, VirtualFG40Library.EXPOSURE_MODE_TIMED);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    throw new Exception(String.Format("Write Register failed : {0}", status));
                }

                return true;
                //// Set ExposureTime
                //status = _virtualFG40.SetFloatReg(_hDevice, VirtualFG40Library.MCAM_EXPOSURE_TIME, 1000);
                //if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                //{
                //    throw new Exception(String.Format("Write Register failed : {0}", status));
                //}

                //// Set PixelFormat
                //status = _virtualFG40.SetEnumReg(_hDevice, VirtualFG40Library.MCAM_PIXEL_FORMAT, VirtualFG40Library.PIXEL_FORMAT_MONO8);
                //if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                //{
                //    throw new Exception(String.Format("Write Register failed : {0}", status));
                //}
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message; return false;
            }
        }
        #endregion 

        #region Crevis 카메라 영상 
        // 비트맵 흑백영상으로 변경 - 8비트 그레이스케일만 구현
        // 추후 업데이트 예정 - 2021.10.21
        public void SetGrayscalePalette(Bitmap bitmap)
        {
            ColorPalette GrayscalePalette = bitmap.Palette;

            for (int i = 0; i < 255; i++)
            {
                GrayscalePalette.Entries[i] = Color.FromArgb(i, i, i);
            }

            bitmap.Palette = GrayscalePalette;
        }

        /// 비트맵을 생성 - Mono
        /// 사용자가 버퍼메모리
        public Bitmap CreateBitmap(CameraParameter.ByteType byteType, UInt32 BufferMemoryNumber)
        {
            Int32 bitsPerPixel = 0;
            Int32 stride = 0;
            Bitmap bitmap;

            //// 픽셀당 1바이트인 경우 8 설정
            //if (byteType == CameraParameter.ByteType.One)
            //{
            //    bitsPerPixel = 8;
            //    stride = (Int32)((_width * bitsPerPixel + 7) / 8);
            //    //BitmapImage = new Bitmap(_width, _height, stride, PixelFormat.Format8bppIndexed, _pImage);
            //    bitmap = new Bitmap(_width, _height, stride, PixelFormat.Format8bppIndexed, _pImages[BufferMemoryNumber]);
                
            //}
            //// 픽셀당 2바이트인 경우 추후 업데이트 예정 - 2021.10.21
            //else if (byteType == CameraParameter.ByteType.Two)
            //{
            //    //bitsPerPixel = 2 * 8;
            //    //stride = (Int32)((_width * bitsPerPixel + 7) / 8);
            //    //BitmapImage = new Bitmap(_width, _height, stride, PixelFormat.Format16bppGrayScale, _pImage);

            //}
            //// Packed 와 같은 10또는 12인 경우 추후 업데이트 예정 - 2021.10.21
            //else
            //{

            //}

            bitsPerPixel = 8;
            stride = (Int32)((_width * bitsPerPixel + 7) / 8);
            //BitmapImage = new Bitmap(_width, _height, stride, PixelFormat.Format8bppIndexed, _pImage);
            bitmap = new Bitmap(_width, _height, stride, PixelFormat.Format8bppIndexed, _pImages[BufferMemoryNumber]);

            //그레이스케일 변경
            SetGrayscalePalette(bitmap);

            return bitmap;
        }

        //비트맵을 생성 - Color
        public Bitmap CreateColorBitmap(UInt32 BufferMemoryNumber)
        {
            Int32 bitsPerPixel = 0;
            Int32 stride = 0;
            Bitmap bitmap;

            //color
            bitsPerPixel = 24;
            stride = (Int32)((_width * bitsPerPixel + 7) / 8);
            bitmap = new Bitmap(_width, _height, stride, PixelFormat.Format24bppRgb, _cvtImages[BufferMemoryNumber]);

            return bitmap;
        }

        public Int32 GetBufferedCount()
        {
            return _qImageBuffer.Count;
        }

        // 큐 버퍼에서 이미지 데이터 획득
        public IntPtr GetImageData()
        {
      
            _pImage = _qImageBuffer.Dequeue();

            if(_ColorType == CameraParameter.ColorType.Mono)
                return _pImage;
            else
            {
                if (_PixelFormat == CameraParameter.PixelFormat.BayerRG8)
                {
                    _virtualFG40.CvtColor(_pImage, _cvtImage, _width, _height, VirtualFG40Library.CV_BayerRG2RGB);
                }
                else if (_PixelFormat == CameraParameter.PixelFormat.YUV422Packed)
                {
                    _virtualFG40.CvtColor(_pImage, _cvtImage, _width, _height, VirtualFG40Library.CV_YUV2BGR_UYVY);
                }

                return _cvtImage;
            }
        }

        // 큐 버퍼에서 해당 index의 이미지 데이터 획득
        public IntPtr GetImageData(int index)
        {
            for(int i=0;i<index;i++)
               _pImage = _qImageBuffer.Dequeue();

            if (_ColorType == CameraParameter.ColorType.Mono)
                return _pImage;
            else
            {
                if (_PixelFormat == CameraParameter.PixelFormat.BayerRG8)
                {
                    _virtualFG40.CvtColor(_pImage, _cvtImage, _width, _height, VirtualFG40Library.CV_BayerRG2RGB);
                }
                else if (_PixelFormat == CameraParameter.PixelFormat.YUV422Packed)
                {
                    _virtualFG40.CvtColor(_pImage, _cvtImage, _width, _height, VirtualFG40Library.CV_YUV2BGR_UYVY);
                }

                return _cvtImage;
            }
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
                //System.Windows.Forms.Application.DoEvents();
                ThisMoment = DateTime.Now;
            }
            return DateTime.Now;
        }
    }

    public class Camera : ImageAcqusitionClass
    {
        
        public bool IsBusy;
        public bool IsThreadworking;
        public bool OnceGrab;
        public bool TriggerFlag;
        public bool InitializeResult;

        #region 생성자와 소멸자
        //생성자 CamIndex와 그랩모드를 입력하여 객체 생성 -  기본 모드는 소프트웨어 트리거
        public Camera(UInt32 cameraIndex, CameraParameter.GrabMode grabMode= CameraParameter.GrabMode.Software, UInt32 maxBufferNumber = 2, bool IsCallbackMode=false)
        {
            //Thread 구동 Flag
            IsThreadworking = true;

            //1회 그랩 Flag
            OnceGrab = false;

            //그랩완료 확인 Flag
            IsBusy = true;

            //MonoGrab Trigger Flag
            TriggerFlag = false;

            try
            {
                //카메라 이니셜라이즈
                if(!Initialize())
                    throw new Exception(ErrorMessage);

                //디바이스 업데이트
                if(!UpdateDeviceList())
                    throw new Exception(ErrorMessage);

                if(!OpenProcess(cameraIndex, maxBufferNumber, grabMode))
                    throw new Exception(ErrorMessage);

                if(IsCallbackMode)
                {
                    GrabCallback();
                }
                //// 라이브러리 내에서 영상 쓰레드 기능 포함 동작시 Acuisition Start 진행
                //if (!StartAcquisition(grabMode))
                //    throw new Exception(ErrorMessage);

                ////영상 그랩 용 쓰레드 생성 및 구동 시작
                //Thread RunCam = new Thread(Run);

                //RunCam.Start();

                InitializeResult = true;
            }
            catch(Exception ex)
            {
                InitializeResult = false;
            }
        }
      
        #endregion
        public void Run()
        {
            bool result;
            while (IsThreadworking)
            {
                
                IsBusy = true;
                if (TriggerFlag)
                {
                    if (_ColorType==CameraParameter.ColorType.Mono)
                    {
                        //모노 영상 포맷 MonoGrab (SoftwareTrigger + GrabAsyn)
                        MonoGrab(0);

                        //Bitmap 생성
                        CreateBitmap(_ByteType, 0);
                    }
                    else
                    {
                        //컬러 영상 포맷 MonoGrab (SoftwareTrigger + GrabAsyn)
                        ColorGrab(0);

                        //Bitmap 이미지 생성
                        CreateColorBitmap(0);
                    }                        

                    //OnceGrab 시 한번만 동작하게 함.
                    if (OnceGrab == true)
                    {
                        TriggerFlag = false;
                        OnceGrab = false;
                    }
                    IsBusy = false;


                }
                Thread.Sleep(10);
            }
        }

        // SystemClose 재정의(오버라이딩)
        // SystemClose 수정 - 2021.10.22
        // 카메라 연결 확인 연결 상태이면 카메라 종료후 시스템 해제
        public bool SystemClose()
        {

            if (_isOpen == true)
            {
                IsThreadworking = false;

                if (!CamClose())
                    return false;
            }
            if (!FreeSystem())
                return false;
            return true;
           
        }
        
        // 카메라 연결 프로세스
        public bool OpenProcess(UInt32 cameraIndex, UInt32 maxNumberBuffer, CameraParameter.GrabMode Mode)
        {
            if (!CamOpen(cameraIndex))
                return false;

            //디바이스 정보 획득
            if (!GetDeviceInfo())
                return false;
            #region 패킷로스 감소를 위한 카메라 설정
            // 패킷로스 감소를 위해 Device Filter Driver On 설정
            if (!SetDeviceFilterDriver(true))
                return false;

            // 패킷로스 감소를 위해 Max Packet Resend Count를 max(255) 설정
            if (!SetMaxPacketResendCount(255))
                return false;

            // 패킷로스 감소를 위해 Packet Delay 설정
            int delayTime=1000;
            if(!SetPacketDelay(delayTime))
                return false;

            // 패킷로스 난 경우에도 영상 표시하도록 Device Missing Packet Display설정
            if (!SetMissingPacketDisplay())
                return false;
            #endregion


            #region 카메라 셋팅 프로세스 - 노말모드, 소프트웨어, 하드웨어 트리거 모드
            //노말모드 셋팅
            if (Mode == CameraParameter.GrabMode.Normal)
            {
                if (_TriggerMode!="Off")
                {
                    if (!SetFeature_NormalMode())
                        return false;
                }
            }
            //소프트웨어모드 셋팅
            else if (Mode == CameraParameter.GrabMode.Software)
            {
                if (_TriggerMode != "On" || _TriggerSource!="Software")
                {
                    if (!SetFeature_SoftwareTriggerMode())
                        return false;
                }
            }
            //하드웨어트리거모드 셋팅
            else if (Mode == CameraParameter.GrabMode.Hardware)
            {
                if (_TriggerMode != "On" || _TriggerSource != "Line1")
                {
                    if (!SetFeature_HardwareTriggerMode())
                        return false;
                }
            }
            #endregion

            //영상을 획득할 버퍼메모리 할당
            if (!AllocateImageBuffer(maxNumberBuffer))
                return false;

            return true;

        }
       
        // 카메라 중간에 끊어진 경우 카메라 재연결 프로세스
        public bool ReOpenProcess(CameraParameter.GrabMode Mode=CameraParameter.GrabMode.Software, bool IsCallbackMode = false)
        {

            if (!OpenProcess(0, 2, Mode))
                return false;

            if (IsCallbackMode)
            {
                GrabCallback();
            }

            return true;

            //if (!CamOpen(0))
            //    return false;

            ////디바이스 정보 획득
            //if (!GetDeviceInfo())
            //    return false;

            //// 디바이스 해상도 설정

            //#region 카메라 셋팅 프로세스 - 노말모드, 소프트웨어, 하드웨어 트리거 모드
            ////노말모드 셋팅
            //if (Mode == CameraParameter.GrabMode.Normal)
            //{
            //    if (_TriggerMode != "Off")
            //    {
            //        if (!SetFeature_NormalMode())
            //            return false;
            //    }
            //}
            ////소프트웨어모드 셋팅
            //else if (Mode == CameraParameter.GrabMode.Software)
            //{
            //    if (_TriggerMode != "On" || _TriggerSource != "Software")
            //    {
            //        if (!SetFeature_SoftwareTriggerMode())
            //            return false;
            //    }
            //}
            ////하드웨어트리거모드 셋팅
            //else if (Mode == CameraParameter.GrabMode.Hardware)
            //{
            //    if (_TriggerMode != "On" || _TriggerSource != "Line1")
            //    {
            //        if (!SetFeature_HardwareTriggerMode())
            //            return false;
            //    }
            //}
            //#endregion

            //if (IsCallbackMode)
            //{
            //    GrabCallback();
            //}

            //return true;
        }

        // 카메라 그랩 모드에 따라서 그랩 Acquisition 시작 프로세스
        public bool StartAcquisition(CameraParameter.GrabMode Mode = CameraParameter.GrabMode.Software, bool IsCallbackMode = false)
        {
            ////Acquistion Start
            if (!AcqusitionStart())
                return false;

            // 콜백모드가 아니면서 비동기 모드(Software, Hardware)인 경우에만 진행
            if(!IsCallbackMode)
            {
                ////트리거(소프트웨어, 하드웨어) 모드인 경우에만 Grab Asyn Start
                if (Mode != CameraParameter.GrabMode.Normal)
                {
                    if (!GrabAsyncStart())
                        return false;
                }
            }
            
            return true;
        }

        // Mono 영상 획득
        public bool MonoGrab(UInt32 BufferMemoryNumber, CameraParameter.GrabMode Mode = CameraParameter.GrabMode.Software)
        {
            if (Mode == CameraParameter.GrabMode.Normal)
                return GrabMonoImageSync(BufferMemoryNumber);

            else if (Mode == CameraParameter.GrabMode.Software)
            {
                CreateSoftwareTrigger();

                return GrabMonoImageAsync(BufferMemoryNumber);
            }
            else
                return GrabMonoImageAsync(BufferMemoryNumber);
        }

        // 컬러 영상 획득
        public bool ColorGrab(UInt32 BufferMemoryNumber, CameraParameter.GrabMode Mode = CameraParameter.GrabMode.Software)
        {
            if (Mode == CameraParameter.GrabMode.Normal)
                return GrabColorImageSync(BufferMemoryNumber);

            else if (Mode == CameraParameter.GrabMode.Software)
            {
                CreateSoftwareTrigger();

                return GrabColorImageAsysn(BufferMemoryNumber);
            }
            else
                return GrabColorImageAsysn(BufferMemoryNumber);

        }

        // 카메라 정보 획득
        public bool GetDeviceInfo()
        {
            
            // 디바이스 ID 획득
            if (!GetDeviceID())
                return false;
            
            // 디바이스 현재 설정 PixelFormat 값을 획득
            if (!GetPixelFormat())
                return false;

            // 디바이스 현재 설정 트리거 모드와 트리거 소스 값을 획득
            if (!GetTriggerMode())
                return false;
            if (!GetTriggerSource())
                return false;

            return true;
        }

        // UI 구동에 영향을 주지 않는 Delay 프로세스
        private static DateTime Delay(int ms)
        {
            DateTime ThisMoment = DateTime.Now;

            TimeSpan duration = new TimeSpan(0, 0, 0, 0, ms);

            DateTime AfterWards = ThisMoment.Add(duration);

            while (AfterWards >= ThisMoment)

            {
                //System.Windows.Forms.Application.DoEvents();
                ThisMoment = DateTime.Now;
            }
            return DateTime.Now;
        }
    }
}
