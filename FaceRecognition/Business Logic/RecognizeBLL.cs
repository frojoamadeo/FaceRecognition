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
using FaceRecognition.DAL;
using System.Diagnostics;

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

        //I think this not should be global
        private Double[] distances;
        private Image<Bgr, Byte>[] imagesDB;
        private int[] labels;

        private string pathXMLHaarcascade;
        private string pathImg;
        private string pathFaceRecognition;
        
        GenericUnitOfWork unitOfWork;

        private string imagesFolder = "Images";
        private string HaarCascadeFolder = "HaarCascade";
        private string HaarCascadeFile = "haarcascade_frontalface_default.XML";

        public RecognizeBLL()
        {
            
            unitOfWork = new GenericUnitOfWork();
            unitOfWork.SaveChanges();
          
            pathFaceRecognition = HttpRuntime.AppDomainAppPath;
            pathImg = pathFaceRecognition + @"\" + imagesFolder;
            pathXMLHaarcascade = pathFaceRecognition + @"\" + HaarCascadeFolder + @"\" + HaarCascadeFile;            
        
        }

        public string saveEmployee(Image newImage, string name, string middleName, string lastName, string email)
        {
            unitOfWork = new GenericUnitOfWork();
            GenericRepository<Employee> employeeRepo = unitOfWork.GetRepoInstance<Employee>();

            Employee employee = null;
            try
            {
                
                employee = (employeeRepo.GetAllRecords().Where<Employee>(e => e.email == email)).First<Employee>();
                return "Ususario Existente";
            }
            catch
            {
                Debug.WriteLine("Nuevo usuario");
            }

            //Add Employee if not exist. The email is unique
            if (employee == null)
            {
                employee = new Employee { name = name, middleName = middleName, lastName = lastName, email = email };
                employeeRepo.Add(employee);
                unitOfWork.SaveChanges();
            }
            
            //I save the image with a guid as a name
            GenericRepository<DistanceResult> distanceResultRepo = unitOfWork.GetRepoInstance<DistanceResult>();
            Guid guid = Guid.NewGuid();           

            var inputImage = new Image<Bgr, Byte>(new Bitmap(newImage));
            Rectangle[] rectangleFace = detection(inputImage, pathXMLHaarcascade);
            
            //The function detection(..) can extract N faces
            if (rectangleFace.Length == 1)
            {
                Image<Gray, byte> grayFrame = toGrayEqualizeFrame(inputImage);
                Bitmap extractedFace;
                extractedFace = formatRectangleFaces(grayFrame.ToBitmap(), rectangleFace[0]);
               
                extractedFace.Save(pathImg + @"\" + guid.ToString(), ImageFormat.Jpeg);

                Image<Gray, byte> faceEMGUCV = new Image<Gray, byte>(extractedFace);

                FaceRecognizer faceRecognition = new EigenFaceRecognizer(numComponentsEigen, thresholdEigen);

                double distance = 2;

                //Save register
                DistanceResult dist = new DistanceResult();
                dist.algorithm = RecognizeBLL.FaceRecognizerMethode.EigenFaceRecognizerMethode.ToString();
                dist.employeeId = employee.employeeId;
                dist.photoName = guid.ToString(); 
                dist.distance = distance;
                distanceResultRepo.Add(dist);

                unitOfWork.SaveChanges();

                int lengthArrays = distanceResultRepo.GetAllRecords().Count();
                imagesDB = new Image<Bgr, Byte>[lengthArrays];
                labels = new int[lengthArrays];
                int i = 0;
                foreach (DistanceResult di in distanceResultRepo.GetAllRecords())
                {
                    //This is to recalculate the faceRecognition and save it, but I think is not necesari declare imageDB and labels as global                    
                    imagesDB[i] = new Image<Bgr, Byte>(pathImg + @"\" + di.photoName);
                    labels[i] = di.distanceResultId;
                    i++;
                }

                faceRecognition.Train(imagesDB,labels);
                faceRecognition.Save(pathImg + @"\" + "TrainingSet");
            }
            return "";
        }

        public string recognizeFaces(Image newImage, string pathXMLHaarcascade, FaceRecognizerMethode faceRecognizerMethode)
        {
            var inputImage = new Image<Bgr, Byte>(new Bitmap(newImage));
            Rectangle[] rectangleFace = detection(inputImage, this.pathXMLHaarcascade);

            if (rectangleFace.Length == 1)
            {
                Image<Gray, byte> grayFrame = toGrayEqualizeFrame(inputImage);
                Bitmap extractedFace;
                extractedFace = formatRectangleFaces(grayFrame.ToBitmap(), rectangleFace[0]);

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
            
                faceRecognition.Load(pathImg + @"\" + "TrainingSet");

                        //Aca tengo que calcular la distancia
                Image<Gray, byte> faceEMGUCV = new Image<Gray, byte>(extractedFace);
                
                FaceRecognizer.PredictionResult ER = faceRecognition.Predict((IImage)faceEMGUCV);
                
                if(ER.Label!=-1)
                {
                    int distanceResult = ER.Label;
                    GenericRepository<DistanceResult> distanceResultRepo = unitOfWork.GetRepoInstance<DistanceResult>();
                    DistanceResult di = distanceResultRepo.GetFirstOrDefault(distanceResult);

                    GenericRepository<Employee> emplyeeRepo = unitOfWork.GetRepoInstance<Employee>();
                    Employee em = emplyeeRepo.GetFirstOrDefault(di.employeeId);
                    return em.name + " " + em.middleName + " " + em.lastName;
                }
                return "Not Found";
            }
            return "Face no Detected or Multiple Faces Detected";
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

        //Esto tal vez haya que borrarlo
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
            CvInvoke.EqualizeHist(grayFrame, grayFrame);
            
            return grayFrame;
        }

        //Code to work with multiple images
        //Parallel.For(0, rectangleFace.Length, i =>
        //{
        //     extractedFace[i] = formatRectangleFaces(grayFrame.ToBitmap(), rectangleFace[i]);
        //     Image<Gray, byte> faceEMGUCV = new Image<Gray, byte>(extractedFace[i]);

        //     FaceRecognizer.PredictionResult ER = faceRecognition.Predict((IImage)faceEMGUCV);
        //     outLabels[i] = ER.Label;
        //     distances[i] = ER.Distance;
        // });

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