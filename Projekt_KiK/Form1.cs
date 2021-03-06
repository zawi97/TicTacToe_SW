﻿using System;
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
        bool gra = true;                //gra w trakcie
        static int size = 5;
        int[,] plansza = new int[size, size]; //0 - puste; 1 - kółko; 2 - krzyżyk
        int tura = 0;                   //kolejne tury. Parzyste - kółko; Nieparzyste - X
        bool startRecording = false;    //do kamery
        Random rng = new Random();      //inicjaliozacja RNG


        Image<Bgr, byte> imagePB1, imagePB2, imagePB3;        //inicjalizacja pictureboxów
        VideoCapture cap;

        public Form1()
        {
            InitializeComponent();
            imagePB1 = new Image<Bgr, byte>(new Size(300, 300));        //wyiary 300x300
            imagePB2 = new Image<Bgr, byte>(new Size(300, 300));
            imagePB3 = new Image<Bgr, byte>(new Size(300, 300));

                //obsługa kamery
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
            Array.Clear(plansza, 0, size^2);
            rysuj_plansze();
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
                if (tura % 2 == 0)
                {
                    textBox1.Text = "O";
                    try
                    {
                        //wykrywanie kształtów
                        var temp = imagePB1.SmoothGaussian(5).Convert<Gray, byte>().
                            ThresholdBinaryInv(new Gray(110), new Gray(255));

                        VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
                        Mat m = new Mat();

                        CvInvoke.FindContours(temp, contours, m, Emgu.CV.CvEnum.RetrType.External,
                            Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
                        
                        imagePB3 = imagePB1.SmoothGaussian(5);
                        Image<Gray, Byte> grayImage = temp;
                        CvInvoke.DrawContours(grayImage, contours, -1, new MCvScalar(255,255,0), 1);
                        pictureBox3.Image = grayImage.Bitmap;
                   
                        for (int i = 0; i < contours.Size; i++)
                        {
                            double perimiter = CvInvoke.ArcLength(contours[i], true);
                            VectorOfPoint approx = new VectorOfPoint();
                            CvInvoke.ApproxPolyDP(contours[i], approx, 0.04 * perimiter, true);

                            CvInvoke.DrawContours(imagePB1, contours, i, new MCvScalar(0, 0, 255), 2);

                            //środki geometryczne figur
                            var moments = CvInvoke.Moments(contours[i]);
                            int x = (int)(moments.M10 / moments.M00);
                            int y = (int)(moments.M01 / moments.M00);
                            Point srodek = new Point((int)(moments.M10 / moments.M00), (int)(moments.M01 / moments.M00));

                            CvInvoke.Circle(imagePB1, srodek, 4, new MCvScalar(50, 127, 127), 2);
                                                        
                            int px = (x - 0)/(300/size);
                            int py = (y - 0)/(300/size);

                            if (approx.Size > 6)     //czy to kółko?
                            {
                                CvInvoke.DrawContours(imagePB3, contours, -1, new MCvScalar(0, 0, 255), 2);
                                CvInvoke.Circle(imagePB3, srodek, 4, new MCvScalar(50, 127, 127), 2);
                                if (plansza[px,py] == 0)
                                {
                                    plansza[px, py] = 1;
                                }
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
                        int[] randomowe = new int[2];
                        for (int i = 0; i < 2; i++)
                        {
                            randomowe[i] = rng.Next(size);
                        }  
                        int rand_x = randomowe[0];
                        int rand_y = randomowe[1];
                        if (plansza[rand_x, rand_y] == 0)
                        {
                            plansza[rand_x, rand_y] = 2;
                            losowanie = false;
                        }
                    }
                }
                rysowanie();
                czy_wygrana();
            }
        }
    
        void winDetected(int figure)
        {
            if (figure == 2)
            {
                ruch_button.Text = "Wygrały Krzyżyki"; // boxmessage = "wygrały krzyżyki"
                gra = false;
            }
            if (figure == 1)
            {
                ruch_button.Text = "Wygrały Kółka"; // boxmessage = "wygrały kółka"
                gra = false;
            }
        }
        void czy_wygrana()
        {
            for (int i = 0; i < size; i++) { //wykrywanie wygranej w poziomie             
                if (plansza[i,0] == 2 && plansza[i,1] == 2 && plansza[i,2] == 2 && plansza[i, 3] == 2 && plansza[i, 4] == 2) winDetected(2); // horizontal pattern of crosses
                if (plansza[i,0] == 1 && plansza[i,1] == 1 && plansza[i,2] == 1 && plansza[i, 3] == 1 && plansza[i, 4] == 1) winDetected(1); // horizontal pattern of circles  
            }

            for (int j = 0; j < size; j++) { //wykrywanie wygranej w pionie
                if (plansza[0,j] == 2 && plansza[1,j] == 2 && plansza[2,j] == 2 && plansza[3, j] == 3 && plansza[4, j] == 4) winDetected(2); // vertical pattern of crosses
                if (plansza[0,j] == 1 && plansza[1,j] == 1 && plansza[2,j] == 1 && plansza[3, j] == 1 && plansza[4, j] == 1) winDetected(1); // vertical pattern of circles                  
            }
            //Wykrywanie wygranej po przekątnych
            if (plansza[0, 0] == 1 && plansza[1, 1] == 1 && plansza[2, 2] == 1 && plansza[3, 3] == 1 && plansza[4, 4] == 1) winDetected(1);
            if (plansza[0, 0] == 2 && plansza[1, 1] == 2 && plansza[2, 2] == 2 && plansza[3, 3] == 2 && plansza[4, 4] == 2) winDetected(2);

            if (plansza[4, 0] == 1 && plansza[3, 1] == 1 && plansza[2, 2] == 1 && plansza[1, 3] == 1 && plansza[0, 4] == 1) winDetected(1);
            if (plansza[4, 0] == 2 && plansza[3, 1] == 2 && plansza[2, 2] == 2 && plansza[1, 3] == 2 && plansza[0, 4] == 2) winDetected(2);
        }

        private void rysuj_plansze()
        {
            imagePB2.SetZero();

            for (int ii = 1; ii < size; ii++)
            {
                CvInvoke.Line(imagePB2, new Point(60 * ii, 0), new Point(60 * ii, 300), new MCvScalar(0, 255, 255), 1);
            }
            for (int jj = 1; jj < size; jj++)
            {
                CvInvoke.Line(imagePB2, new Point(0, 60 * jj), new Point(300, 60 * jj), new MCvScalar(0, 255, 255), 1);
            }
        }
        private void rysowanie()
        {
            for (int dx = 0; dx < size; dx++)
            {
                for (int dy = 0; dy < size; dy++)
                {
                    Point P = new Point();
                    P.X = dx * (300/size) + (300/(size*2)); P.Y = dy * (300 / size) + (300 / (size * 2));     //punkty środka rysowanego okręgu
                    switch (plansza[dx,dy])
                    {
                        case 1:
                            CvInvoke.Circle(imagePB2, P, 25, new MCvScalar(255, 0, 0), 2);
                            break;
                        case 2:
                            CvInvoke.Line(imagePB2, new Point(P.X - 20, P.Y + 20), new Point(P.X + 20, P.Y - 20), new MCvScalar(0, 255, 0), 2);
                            CvInvoke.Line(imagePB2, new Point(P.X + 20, P.Y + 20), new Point(P.X - 20, P.Y - 20), new MCvScalar(0, 255, 0), 2);
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
