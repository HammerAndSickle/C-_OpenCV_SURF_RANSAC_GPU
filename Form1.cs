using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using System.Threading;


using OpenCvSharp;
using OpenCvSharp.Util;
using OpenCvSharp.Detail;
using OpenCvSharp.XFeatures2D;
using OpenCvSharp.Extensions;
using OpenCvSharp.UserInterface;


namespace Cam
{
    public partial class Form1 : Form
    {
        VideoCapture vc;
        Mat imgSrc;
        CudaSurfDetection.CudaSurfWrap detector;

        public bool isComputing;                           //지금 추출 중인가?
        //OpenCvSharp.CPlusPlus.Point[] rectanPts;    //추출된 영역. 4개의 Point로 구성되어 있다.
        Byte[] filename;

        public int capture_type;
        static int DO_WITH_DIRECT_WEBCAM = 0;       //PC에 직접연결된 웹캠
        static int DO_WITH_IP_CAMERA = 1;           //IP 웹캠(안드로이드도 된다!)

        int[] rectPts;

        public Form1()
        {
            InitializeComponent();
            isComputing = false;
            //capture_type = DO_WITH_IP_CAMERA;           //웹캠 or IP웹캠
            capture_type = DO_WITH_DIRECT_WEBCAM;

            //target 이미지 이름을 ASCII 바이트로 변경하여 C++로 보낸다.
            string fnamestr = "origim.png";
            filename = Encoding.ASCII.GetBytes(fnamestr);

            rectPts = new int[8];
            detector = new CudaSurfDetection.CudaSurfWrap(filename, 400);
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
    
            //이미지를 IplImage 타입으로 추출하여 가져온 후, resize한 이미지를 사용
            //IplImage temimg = new IplImage(680, 480, BitDepth.U8, 3);
            vc.Set(CaptureProperty.FrameWidth, 640);
            vc.Set(CaptureProperty.FrameHeight, 480);

            Console.WriteLine("vc 가졌다");

            //이미지를 추출.
            imgSrc = vc.RetrieveMat();

            //temimg = capture.QueryFrame();
            

            if (imgSrc.Data == null) return;

            //이미지 resize (imgSrc는 원본)
            Cv2.Resize(imgSrc, imgSrc, new OpenCvSharp.Size(640, 480));

            //이미지 흑백화 (grayed가 흑백 이미지)
            Mat grayed = new Mat(480, 640, MatType.CV_8UC1);
            Cv2.CvtColor(imgSrc, grayed, ColorConversionCodes.BGR2GRAY);

            
            //grayed의 내용을 Byte array로 변환
            Byte[] matarrs = new Byte[grayed.Rows * grayed.Cols];
            Marshal.Copy(grayed.Data, matarrs, 0, grayed.Rows * grayed.Cols);

            detector.doMatching(matarrs, 480, 640);
            drawRec(imgSrc);
            //이건 byte -> intptr이다.
            //IntPtr chararr = Marshal.AllocHGlobal(grayed.Rows * grayed.Cols);
            //Marshal.Copy(matarrs, 0, chararr, grayed.Rows * grayed.Cols);

            

            //Mat another = new Mat(480, 640, MatType.CV_8UC1, chararr);

            //영역 계산 중이라면 스킵. 계산 중이 아니라면 영역 계산 스레드를 실행
            /*
            if (!isComputing)
            {
                isComputing = true;

                ComputingSURF = new Thread(() => TestSIFT.setImg(tsift, this, imgSrc));
                ComputingSURF.Start();
                
            }
            */

            //detector.doMatching(imgSrc);

            //가지고 있는 영역 값을 이용해 매 영상마다 영역은 항상 표시하도록 한다.
            //drawRec(rectanPts);
            pictureBoxIpl1.ImageIpl = imgSrc;

        }

        //가지고 있는 영역 값을 설정한다. 스레드에서 이 함수를 호출하여, 계산이 끝날 때마다 업데이트 시킨다.
       /*
        public void setDetectionRec(int u0, int v0, int u1, int v1, int u2, int v2, int u3, int v3)
        {
            rectanPts[0] = new OpenCvSharp.CPlusPlus.Point(u0, v0);
            rectanPts[1] = new OpenCvSharp.CPlusPlus.Point(u1, v1);
            rectanPts[2] = new OpenCvSharp.CPlusPlus.Point(u2, v2);
            rectanPts[3] = new OpenCvSharp.CPlusPlus.Point(u3, v3);
        }
        */

        
        //가지고 있는 영역 값대로 사각형을 그린다.
        public void drawRec(Mat frameorig)
        {
            rectPts = detector.getRectPts();

            Cv2.Line(frameorig, new OpenCvSharp.Point(rectPts[0], rectPts[1]), new OpenCvSharp.Point(rectPts[2], rectPts[3]), new Scalar(0, 255, 0), 4);
            Cv2.Line(frameorig, new OpenCvSharp.Point(rectPts[2], rectPts[3]), new OpenCvSharp.Point(rectPts[4], rectPts[5]), new Scalar(0, 255, 0), 4);
            Cv2.Line(frameorig, new OpenCvSharp.Point(rectPts[4], rectPts[5]), new OpenCvSharp.Point(rectPts[6], rectPts[7]), new Scalar(0, 255, 0), 4);
            Cv2.Line(frameorig, new OpenCvSharp.Point(rectPts[6], rectPts[7]), new OpenCvSharp.Point(rectPts[0], rectPts[1]), new Scalar(0, 255, 0), 4);

            //Cv.Line(imgSrc, new CvPoint(pts[0].X, pts[0].Y), new CvPoint(pts[1].X, pts[1].Y), new CvScalar(0, 255, 0), 4);
            //Cv.Line(imgSrc, new CvPoint(pts[1].X, pts[1].Y), new CvPoint(pts[2].X, pts[2].Y), new CvScalar(0, 255, 0), 4);
            //Cv.Line(imgSrc, new CvPoint(pts[2].X, pts[2].Y), new CvPoint(pts[3].X, pts[3].Y), new CvScalar(0, 255, 0), 4);
            //Cv.Line(imgSrc, new CvPoint(pts[3].X, pts[3].Y), new CvPoint(pts[0].X, pts[0].Y), new CvScalar(0, 255, 0), 4);   
        }
        
        
        private void pictureBoxIpl1_Click(object sender, EventArgs e)
        {
            if (initCamera())
            {
                StartTimer();
            }
            else
            {
                MessageBox.Show("문제발생");
            }
        }

        private bool initCamera()
        {
            Console.WriteLine("aaaaaaaaaaaaaaa");

            try
            {
                if (capture_type == DO_WITH_DIRECT_WEBCAM)
                {
                    Console.WriteLine("=1=");

                    vc = new OpenCvSharp.VideoCapture(0);		//웹캠으로부터

                    Console.WriteLine("=2=");
                    
                    vc.Open(CaptureDevice.DShow, 0);
                    //capture = CvCapture.FromCamera(CaptureDevice.DShow, 0);
                    //capture.SetCaptureProperty(CaptureProperty.FrameWidth, 680);
                    //capture.SetCaptureProperty(CaptureProperty.FrameHeight, 480);

                    Console.WriteLine("=3=");

                    return true;
                }

                if (capture_type == DO_WITH_IP_CAMERA)
                {
                    //capture = CvCapture.FromFile("http://192.168.0.5:8080/shot.jpg");
                    //capture.SetCaptureProperty(CaptureProperty.FrameWidth, 680);
                    //capture.SetCaptureProperty(CaptureProperty.FrameHeight, 480);

                    return true;
                }
                    
                return false;
            }
            catch
            {
                return false;
            }
        }
        private void StartTimer()
        {
            timer1.Interval = 5;
            timer1.Enabled = true;
        }
        
    }
}
