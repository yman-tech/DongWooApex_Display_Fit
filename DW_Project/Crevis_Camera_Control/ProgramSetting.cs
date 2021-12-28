using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Crevis_Camera_Control
{
    class ProgramSetting
    {
        // .ini 파일 핸들링을 위한 IniFile 클래스
        class IniFile
        {
            public string path;

            [DllImport("Kernel32")]
            private static extern long WritePrivateProfileString(string section,
                string key, string val, string filePath);

            [DllImport("kernel32")]
            private static extern int GetPrivateProfileString(string section,
                string key, string def, StringBuilder retVal, int size, string filePath);

            public IniFile(string IniFile)
            {
                path = IniFile;
            }

            public void WriteValue(string Section, string Key, string Value)
            {
                WritePrivateProfileString(Section, Key, Value, path);
            }

            public string ReadValue(string Section, string Key, string Default)
            {
                StringBuilder buffer = new StringBuilder(255);
                GetPrivateProfileString(Section, Key, Default, buffer, 255, this.path);
                return buffer.ToString();
            }

            public void WriteValue(string Section, string Key, int Value)
            {
                WritePrivateProfileString(Section, Key, Value.ToString(), path);
            }

            public int ReadValue(string Section, string Key, int Default)
            {
                StringBuilder buffer = new StringBuilder(255);
                GetPrivateProfileString(Section, Key, Default.ToString(), buffer, 255, this.path);
                return int.Parse(buffer.ToString());
            }

        }

        public class Recipe_Data
        {
            public static string LogPath = "C:\\Crevis_Control\\Log";
            public static int LogSaveDuration = 90;
            public static string LogFrameCountEnable = "T";
            public static string DisplayRotated = "F";
            //public static int DisplayWidthRatio = 16;
            //public static int DisplayHeightRatio = 9;
            //public static int CameraWidthRatio = 14;
            //public static int CameraHeightRatio = 9;
            public static string CameraModel = "A";

        }
        public class Recipe_Password
        {
            public static string Password = "1";
        }

        public static void Para_Load()
        {
            IniFile ini=new IniFile(@"C:\Crevis_Control\SystemData" + "\\ProgramSetting.ini");
            Recipe_Data.LogPath = ini.ReadValue("Data", "LogPath", Recipe_Data.LogPath);
            Recipe_Data.LogSaveDuration = ini.ReadValue("Data", "LogSaveDuration", Recipe_Data.LogSaveDuration);
            Recipe_Data.LogFrameCountEnable = ini.ReadValue("Data", "LogFrameCount", Recipe_Data.LogFrameCountEnable);
            Recipe_Data.DisplayRotated = ini.ReadValue("Data", "DisplayRotateStatus", Recipe_Data.DisplayRotated);

            //Recipe_Data.DisplayWidthRatio = ini.ReadValue("Data", "DisplayWidthRatio", Recipe_Data.DisplayWidthRatio);
            //Recipe_Data.DisplayHeightRatio = ini.ReadValue("Data", "DisplayHeightRatio", Recipe_Data.DisplayHeightRatio);
            //Recipe_Data.CameraWidthRatio = ini.ReadValue("Data", "CameraWidthRatio", Recipe_Data.CameraWidthRatio);
            //Recipe_Data.CameraHeightRatio = ini.ReadValue("Data", "CameraHeightRatio", Recipe_Data.CameraHeightRatio);
            Recipe_Data.CameraModel = ini.ReadValue("Data", "CameraModel", Recipe_Data.CameraModel);

            Recipe_Password.Password = ini.ReadValue("Password", "Password", Recipe_Password.Password);

        }

        public static void Para_Load(string FilePath)
        {
            IniFile ini = new IniFile(FilePath);
            Recipe_Data.LogPath = ini.ReadValue("Data", "LogPath", Recipe_Data.LogPath);
            Recipe_Data.LogSaveDuration = ini.ReadValue("Data", "logSaveDuration", Recipe_Data.LogSaveDuration);
            Recipe_Data.LogFrameCountEnable = ini.ReadValue("Data", "LogFrameCount", Recipe_Data.LogFrameCountEnable);
            Recipe_Data.DisplayRotated = ini.ReadValue("Data", "DisplayRotateStatus", Recipe_Data.DisplayRotated);
            //Recipe_Data.DisplayWidthRatio = ini.ReadValue("Data", "DisplayWidthRatio", Recipe_Data.DisplayWidthRatio);
            //Recipe_Data.DisplayHeightRatio = ini.ReadValue("Data", "DisplayHeightRatio", Recipe_Data.DisplayHeightRatio);
            //Recipe_Data.CameraWidthRatio = ini.ReadValue("Data", "CameraWidthRatio", Recipe_Data.CameraWidthRatio);
            //Recipe_Data.CameraHeightRatio = ini.ReadValue("Data", "CameraHeightRatio", Recipe_Data.CameraHeightRatio);
            Recipe_Data.CameraModel = ini.ReadValue("Data", "CameraModel", Recipe_Data.CameraModel);

            Recipe_Password.Password = ini.ReadValue("Password", "Password", Recipe_Password.Password);

        }
        public static void Para_Save()
        {
            IniFile ini = new IniFile(@"C:\Crevis_Control\SystemData" + "\\ProgramSetting.ini");
            ini.WriteValue("Data", "LogPath", Recipe_Data.LogPath);
            ini.WriteValue("Data", "LogSaveDuration", Recipe_Data.LogSaveDuration);
            ini.WriteValue("Data", "LogFrameCount", Recipe_Data.LogFrameCountEnable);
            ini.WriteValue("Data", "DisplayRotateStatus", Recipe_Data.DisplayRotated);
            //ini.WriteValue("Data", "DisplayWidthRatio", Recipe_Data.DisplayWidthRatio);
            //ini.WriteValue("Data", "DisplayHeightRatio", Recipe_Data.DisplayHeightRatio);
            //ini.WriteValue("Data", "CameraWidthRatio", Recipe_Data.CameraWidthRatio);
            //ini.WriteValue("Data", "CameraHeightRatio", Recipe_Data.CameraHeightRatio);
            ini.WriteValue("Data", "CameraModel", Recipe_Data.CameraModel);

            ini.WriteValue("Password", "Password", Recipe_Password.Password);

        }
    }
}
