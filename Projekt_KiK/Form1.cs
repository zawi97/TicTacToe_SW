using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;


namespace Projekt_KiK
{
    
    public partial class Form1 : Form
    {
        bool gra = true;
        int[,] plansza = new int[3, 3]; //0 - puste; 1 - kółko; 2 - krzyżyk
        int tura = 0;
        bool startRecording = false;
        Random rnd = new Random();


        Image<Bgr, byte> imagePB1, imagePB2;
        VideoCapture cap;

        public Form1()
        {
            InitializeComponent();
            imagePB1 = new Image<Bgr, byte>(new Size(300, 300));
            imagePB2 = new Image<Bgr, byte>(new Size(300, 300));

            try
            {
                cap = new VideoCapture(0);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            cap.ImageGrabbed += ProcessFrame;
        }
        void ProcessFrame(object sender, EventArgs e)
        {
            Mat m = new Mat();
            cap.Read(m);
            if (m != null)
            {
                imagePB1 = m.ToImage<Bgr, byte>();
                pictureBox1.Image = imagePB1.Bitmap;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {


        }

        private void button1_Click(object sender, EventArgs e)
        {
            gra = true;
            startRecording = !startRecording;
            if (startRecording)
            {
                cap.Start();
            }
            else
            {
                cap.Stop();
                imagePB1.SetZero();
                pictureBox1.Image = imagePB1.Bitmap;
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            
            tura++;
            if (gra)
            {
                if (tura % 2 == 1)
                {
                    textBox1.Text = "O";
                    try
                    {
                        var temp = imagePB1.SmoothGaussian(5).Convert<Gray, byte>().
                            ThresholdBinaryInv(new Gray(100), new Gray(255));

                        VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
                        Mat m = new Mat();

                        CvInvoke.FindContours(temp, contours, m, Emgu.CV.CvEnum.RetrType.External,
                            Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

                        for (int i = 0; i < contours.Size; i++)
                        {
                            double perimiter = CvInvoke.ArcLength(contours[i], true);
                            VectorOfPoint approx = new VectorOfPoint();
                            CvInvoke.ApproxPolyDP(contours[i], approx, 0.04 * perimiter, true);

                            CvInvoke.DrawContours(imagePB1, contours, i, new MCvScalar(0, 0, 255), 2);

                            var moments = CvInvoke.Moments(contours[i]);
                            int x = (int)(moments.M10 / moments.M00);
                            int y = (int)(moments.M01 / moments.M00);

                            //int plansza_x = Math.Round()
                            //int plansza_y

                            if (contours.Size > 10)
                            {
                                plansza[(x - 50) / 100, (y - 50) / 100] = 2;
                            }

                        }

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }


                }
                else
                {
                    bool losowanie = true;
                    textBox1.Text = "X";

                    while (losowanie)
                    {
                        int rand_x = rnd.Next(3);
                        int rand_y = rnd.Next(3);
                        if (plansza[rand_x, rand_x] == 0)
                        {
                            plansza[rand_x, rand_x] = 2;
                            losowanie = false;
                        }
                    }

                    czy_wygrana();
                    rysowanie();
                }
            }
        }

        private void czy_wygrana() 
        { 
            
        }

        private void rysowanie()
        {
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    Point P = new Point();
                    P.X = x * 100 + 50; P.Y = y * 100 + 50;
                    switch (plansza[x,y])
                    {
                        case 1:
                            CvInvoke.Circle(imagePB2, P, 50, new MCvScalar(255, 0, 0), 5);
                            break;
                        case 2:


                            break;
                        default:
                            break;
                    }
                }
            }

            pictureBox2.Image = imagePB2.Bitmap;


        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }


    }
}
