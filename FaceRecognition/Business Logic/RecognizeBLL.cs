using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing.Imaging;
using Emgu.CV.Face;
using FaceRecognition.Models;
using FaceRecognition.UnitOfWork;

namespace FaceRecognition.Business_Logic
{
    public class RecognizeBLL
    {
        private const int LABEL = 0; //Ahora no tiene uso
        private const int DISTANCE = 1; //Ahora no tiene uso

        private Color rectangleColor = Color.Green;
        private int widthImage = 40; //tiene que ser multiplo de 4
        private int heighImage = 40; //tiene que ser multiplo de 4   
        private double scaleFactor = 1.2;
        private int minNeighbors = 3;

        private int numComponentsEigen = 80;
        private double thresholdEigen = 2000;

        private int numComponentsFisher = 0;
        private double thresholdFisher = 3500;

        private int radiusLBPH = 1;
        private int neighborsLBPH = 8;
        private int gridXLBPH = 8;
        private int gridYLBPH = 8;
        private double thresholdLBPH = 100;

        public enum FaceRecognizerMethode { EigenFaceRecognizerMethode, FisherFaceRecognizerMethode, LBPHFaceRecognizerMethode };

        private Size minSize = new Size(25, 25);
        private Size maxSize = new Size(400, 400);

        private Double[] distances;

        string pathXMLHaarcascade;

        private Image<Bgr, Byte>[] imagesDB; //Tengo que cargar las imagenes y los labels
        private int[] labels;

        GenericUnitOfWork unitOfWork;

        public RecognizeBLL()
        {

            unitOfWork = new GenericUnitOfWork();
            var _em = unitOfWork.GetRepoInstance<Employee>().GetAllRecords();

            //Tengo que cargar las imagenes y los labels aca
            this.pathXMLHaarcascade = @"haarcascade_frontalface_default.XML";
        }


        //el resturn lo cambie de Int32 a 64
        public Int64[] recognizeFaces(Image<Bgr, Byte> inputImage, string pathXMLHaarcascade, FaceRecognizerMethode faceRecognizerMethode)
        {

            if(pathXMLHaarcascade!="")
            {
                this.pathXMLHaarcascade = pathXMLHaarcascade;
            }

            Bitmap[] extractedFace;
            Rectangle[] rectangleFace = detection(inputImage, pathXMLHaarcascade);
            Image<Gray, byte> grayFrame = toGrayEqualizeFrame(inputImage);
            Int64[] outLabels = null; //Lo cambie de 32 a 64
            FaceRecognizer faceRecognition;
            
            switch (faceRecognizerMethode.ToString())
            {
                case "EigenFaceRecognizerMethode": faceRecognition = new EigenFaceRecognizer(numComponentsEigen, thresholdEigen); //try catch aca
                    break;
                case "FisherFaceRecognizerMethode": faceRecognition = new FisherFaceRecognizer(numComponentsFisher, thresholdFisher);
                    break;
                case "LBPHFaceRecognizerMethode": faceRecognition = new LBPHFaceRecognizer(radiusLBPH, neighborsLBPH, gridXLBPH, gridYLBPH, thresholdLBPH);
                    break;
                default: return null;
            };

            if (rectangleFace.Length > 0)
            {
                faceRecognition.Train(imagesDB, labels);
                extractedFace = new Bitmap[rectangleFace.Length];
                outLabels = new Int64[labels.Length];
                distances = new Double[rectangleFace.Length];

                Parallel.For(0, rectangleFace.Length, i =>
                {
                    extractedFace[i] = formatRectangleFaces(grayFrame.ToBitmap(), rectangleFace[i]);
                    Image<Gray, byte> faceEMGUCV = new Image<Gray, byte>(extractedFace[i]);

                    FaceRecognizer.PredictionResult ER = faceRecognition.Predict((IImage)faceEMGUCV);
                    outLabels[i] = ER.Label;
                    distances[i] = ER.Distance;
                });
            }

            return outLabels;
        }


        private Bitmap formatRectangleFaces(Bitmap grayFrame, Rectangle rectangleFace)
        {
            Point point = new Point(0, 0);
            Rectangle rec = new Rectangle(point, rectangleFace.Size);
            Bitmap extractedFace = new Bitmap(rectangleFace.Width, rectangleFace.Height);
            Graphics faceCanvas = Graphics.FromImage(extractedFace);
            faceCanvas.DrawImage(grayFrame, rec, rectangleFace.X, rectangleFace.Y, rectangleFace.Width, rectangleFace.Height, GraphicsUnit.Pixel);
            extractedFace = new Bitmap(extractedFace, new Size(widthImage, HeighImage));

            return extractedFace;
        }

        public Rectangle[] detection(Image<Bgr, Byte> inputImage, string pathXMLHaarcascade)
        {
            Rectangle[] rectangleFace = null;
            Image<Gray, byte> grayFrame;

            if (inputImage != null)
            {
                grayFrame = toGrayEqualizeFrame(inputImage);
                CascadeClassifier haarCascadeXML = new CascadeClassifier(pathXMLHaarcascade);
                rectangleFace = haarCascadeXML.DetectMultiScale(grayFrame, ScaleFactor, minNeighbors, minSize, maxSize);
            }


            return rectangleFace;
        }

        public Image<Bgr, byte> squaredFaces(Image<Bgr, Byte> inputImage, string pathXMLHaarcascade, Color rectangleColor)
        {
            Point point;
            Rectangle rec;
            this.rectangleColor = rectangleColor;

            Rectangle[] rectangleFace = detection(inputImage, pathXMLHaarcascade);

            if (rectangleFace.Length > 0)
            {
                Parallel.ForEach(rectangleFace, actualFace =>
                {
                    point = new Point(actualFace.X, actualFace.Y);
                    rec = new Rectangle(point, actualFace.Size);
                    inputImage.Draw(rec, new Bgr(rectangleColor), 2);
                    //Console.WriteLine("Processing {0} on thread {1}", actualFace.Location,
                    //Thread.CurrentThread.ManagedThreadId);
                });
            }

            return inputImage;
        }

        public void saveDetectedFaces(Image<Bgr, Byte> inputImage, string path, string pathXMLHaarcascade)
        {
            Bitmap[] extractedFace;
            Rectangle[] rectangleFace = detection(inputImage, pathXMLHaarcascade);

            Image<Gray, byte> grayFrame = toGrayEqualizeFrame(inputImage);

            if (rectangleFace.Length > 0)
            {
                extractedFace = new Bitmap[rectangleFace.Length];
                Parallel.For(0, rectangleFace.Length, i =>
                {
                    extractedFace[i] = formatRectangleFaces(grayFrame.ToBitmap(), rectangleFace[i]);
                    extractedFace[i].Save(path, ImageFormat.Jpeg);
                    //Image<Gray, byte> faceEMGUCV = new Image<Gray, byte>(extractedFace[i]);                                   
                });
            }
        }

        //public void estimateParametersEigen(IImage[] imagesInput, IImage[] imagesDB, int[] labels)
        public void estimateParametersEigen(Image<Bgr, Byte>[] imagesInput, Image<Bgr, Byte>[] imagesDB, int[] labels)
        {
            int tmpNumComponentsEigen;
            double tmpThresholdEigen;
            int EficientNumComponentsEigen = 80;
            double EficientThresholdEigen = 2000;
            int countRecognitionFaces = 0;
            int countRecognitionFacesMax = 0;
            FaceRecognizer faceRecognition;

            for (tmpThresholdEigen = 0; tmpThresholdEigen < 3000; tmpThresholdEigen++)
            {
                for (tmpNumComponentsEigen = 0; tmpNumComponentsEigen < 100; tmpNumComponentsEigen++)
                {
                    foreach (IImage input in imagesInput)
                    {
                        faceRecognition = new EigenFaceRecognizer(tmpNumComponentsEigen, tmpThresholdEigen);
                        faceRecognition.Train(imagesDB, labels);
                        FaceRecognizer.PredictionResult ER = faceRecognition.Predict(input);
                        if (ER.Label != -1)
                        {
                            countRecognitionFaces++;
                        }
                    }
                    if (countRecognitionFaces > countRecognitionFacesMax)
                    {
                        EficientNumComponentsEigen = tmpNumComponentsEigen;
                        EficientThresholdEigen = tmpThresholdEigen;
                    }
                    countRecognitionFaces = 0;
                }
            }

            numComponentsEigen = EficientNumComponentsEigen;
            thresholdEigen = EficientThresholdEigen;
        }

        //public void estimateParametersFisher(IImage[] imagesInput, IImage[] imagesDB, int[] labels)
        public void estimateParametersFisher(Image<Bgr, Byte>[] imagesInput, Image<Bgr, Byte>[] imagesDB, int[] labels)
        {
            int tmpNumComponentsFisher;
            double tmpThresholdFisher;
            int EficientNumComponentsFisher = 80;
            double EficientThresholdFisher = 2000;
            int countRecognitionFaces = 0;
            int countRecognitionFacesMax = 0;
            FaceRecognizer faceRecognition;

            for (tmpThresholdFisher = 0; tmpThresholdFisher < 3000; tmpThresholdFisher++)
            {
                for (tmpNumComponentsFisher = 0; tmpNumComponentsFisher < 100; tmpNumComponentsFisher++)
                {
                    foreach (Image<Bgr, Byte> input in imagesInput)
                    {
                        faceRecognition = new EigenFaceRecognizer(tmpNumComponentsFisher, tmpThresholdFisher);
                        faceRecognition.Train(imagesDB, labels);
                        FaceRecognizer.PredictionResult ER = faceRecognition.Predict(input);
                        if (ER.Label != -1)
                        {
                            countRecognitionFaces++;
                        }
                    }
                    if (countRecognitionFaces > countRecognitionFacesMax)
                    {
                        EficientNumComponentsFisher = tmpNumComponentsFisher;
                        EficientThresholdFisher = tmpThresholdFisher;
                    }
                    countRecognitionFaces = 0;
                }
            }

            numComponentsFisher = EficientNumComponentsFisher;
            thresholdFisher = EficientThresholdFisher;
        }

        private Image<Gray, byte> toGrayEqualizeFrame(Image<Bgr, Byte> inputImage)
        {
            Image<Gray, byte> grayFrame = inputImage.Convert<Gray, byte>();
            //CvInvoke.cvEqualizeHist(grayFrame, grayFrame); ver
            
            return grayFrame;
        }

        public int WidthImage
        {
            get { return widthImage; }
            set { widthImage = value; }
        }

        public int HeighImage
        {
            get { return heighImage; }
            set { heighImage = value; }
        }

        public double ScaleFactor
        {
            get { return scaleFactor; }
            set { scaleFactor = value; }
        }

        public int MinNeighbors
        {
            get { return minNeighbors; }
            set { minNeighbors = value; }
        }

        public int NumComponentsEigen
        {
            get { return numComponentsEigen; }
            set { numComponentsEigen = value; }
        }

        public double ThresholdEigen
        {
            get { return thresholdEigen; }
            set { thresholdEigen = value; }
        }

        public int NumComponentsFisher
        {
            get { return numComponentsFisher; }
            set { numComponentsFisher = value; }
        }

        public double ThresholdFisher
        {
            get { return thresholdFisher; }
            set { thresholdFisher = value; }
        }
        public Color RectangleColor
        {
            get { return rectangleColor; }
            set { rectangleColor = value; }
        }

        public int RadiusLBPH
        {
            get { return radiusLBPH; }
            set { radiusLBPH = value; }
        }

        public int NeighborsLBPH
        {
            get { return neighborsLBPH; }
            set { neighborsLBPH = value; }
        }

        public int GridXLBPH
        {
            get { return gridXLBPH; }
            set { gridXLBPH = value; }
        }

        public int GridYLBPH
        {
            get { return gridYLBPH; }
            set { gridYLBPH = value; }
        }

        public double ThresholdLBPH
        {
            get { return thresholdLBPH; }
            set { thresholdLBPH = value; }
        }


    }
}