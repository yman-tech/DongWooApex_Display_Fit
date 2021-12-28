using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Crevis_Camera_Control
{
    class ImageProcessing
    {
        public static Bitmap RotateImage(Bitmap SourceImage, float Angle=180)
        {
            Bitmap ConvertImage = new Bitmap(SourceImage.Width, SourceImage.Height);

            Graphics g = Graphics.FromImage(ConvertImage);
            // 이미지 중심을 (0,0)이동
            g.TranslateTransform(SourceImage.Width / 2, SourceImage.Height / 2);
            // 회전
            g.RotateTransform(Angle);
            // 이미지 중심을 원래 좌표로 이동
            g.TranslateTransform(-SourceImage.Width / 2, -SourceImage.Height / 2);
            // 원본 이미지로 그리기
            g.DrawImage(SourceImage, new Point(0, 0));

            return ConvertImage;
        }
    }
}
