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
using System.Web.Script.Serialization;

namespace FaceRecognition.Business_Logic
{
    public class RecognizeBLL
    {
        private const int LABEL = 0; //Ahora no tiene uso
        private const int DISTANCE = 1; //Ahora no tiene uso

        private Color rectangleColor = Color.Green;
        private int widthImage = 200; //tiene que ser multiplo de 4. Antes tenia 40 en ambos pero se venia medio mal la foto, con 200 la foto es mas nitida logicamente, creo que es mejor
        private int heighImage = 200; //tiene que ser multiplo de 4   
        private double scaleFactor = 1.2;
        private int minNeighbors = 3;

        private int numComponentsEigen = 50; //Estos valores son los que me dio la funcion para estimar
        private double thresholdEigen = 6200; //2000 es default. Mientras menos es mas preciso, aunque podria no llegar a reconocer, si es mas podria reconocer cualquier cosa

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
        private Image<Gray, Byte>[] imagesDB;
        private int[] labels;

        private string pathXMLHaarcascade;
        private string pathImg;
        private string pathFaceRecognition;
        
        GenericUnitOfWork unitOfWork;

        private string imagesFolder = "Images";
        private string HaarCascadeFolder = "HaarCascade";
        private string HaarCascadeFile = "haarcascade_frontalface_default.XML";

        public enum Result { Recognized, Unknown, NoDetected, MultipleFacesDetected, Saved, Error };

        public RecognizeBLL()
        {
            
            unitOfWork = new GenericUnitOfWork();
            unitOfWork.SaveChanges();
          
            pathFaceRecognition = HttpRuntime.AppDomainAppPath;
            pathImg = pathFaceRecognition + @"\" + imagesFolder;
            pathXMLHaarcascade = pathFaceRecognition + @"\" + HaarCascadeFolder + @"\" + HaarCascadeFile;            
        
        }

        public string saveEmployee(Image newImage, string name, string middleName, string lastName, string email, FaceRecognizerMethode faceRecognizerMethode)
        {
            unitOfWork = new GenericUnitOfWork();
            GenericRepository<Employee> employeeRepo = unitOfWork.GetRepoInstance<Employee>();

            Employee employee = null;
            try
            {
                
                employee = (employeeRepo.GetAllRecords().Where<Employee>(e => e.email == email)).First<Employee>();               
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
            if (rectangleFace.Length <= 0)
            {
                return Result.NoDetected.ToString();
            }
            else if (rectangleFace.Length > 1)
            {
                return Result.MultipleFacesDetected.ToString();
            }
            else
            {
                Image<Gray, byte> grayFrame = toGrayEqualizeFrame(inputImage);
                
                Image<Gray, Byte> faceEMGUCV = formatRectangleFaces(grayFrame.ToBitmap(), rectangleFace[0]);
             
                faceEMGUCV._EqualizeHist();

                faceEMGUCV.Save(pathImg + @"\" + guid.ToString()+".Jpeg");

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


                double distance = 2;

                //Save register
                DistanceResult dist = new DistanceResult();
                dist.algorithm = RecognizeBLL.FaceRecognizerMethode.FisherFaceRecognizerMethode.ToString();
                dist.employeeId = employee.employeeId;
                dist.photoName = guid.ToString(); 
                dist.distance = distance;
                distanceResultRepo.Add(dist);

                unitOfWork.SaveChanges();

                int lengthArrays = distanceResultRepo.GetAllRecords().Count();
                imagesDB = new Image<Gray, Byte>[lengthArrays];
                labels = new int[lengthArrays];
                int i = 0;
                foreach (DistanceResult di in distanceResultRepo.GetAllRecords())
                {
                    //This is to recalculate the faceRecognition and save it, but I think is not necesari declare imageDB and labels as global                    
                    imagesDB[i] = new Image<Gray, Byte>(pathImg + @"\" + di.photoName + ".Jpeg");
                    labels[i] = di.employeeId;
                    i++;
                }

                if (employeeRepo.GetAllRecords().Count() > 1)
                {
                    faceRecognition.Train(imagesDB, labels);
                    faceRecognition.Save(pathImg + @"\" + "TrainingSet");
                }
                return Result.Saved.ToString();
            }
            //return Result.Error.ToString();
        }

        public EmployeeStructure[] recognizeMultipleFaces(Image newImage, FaceRecognizerMethode faceRecognizerMethode)
        {
            var inputImage = new Image<Bgr, Byte>(new Bitmap(newImage));
            Rectangle[] rectangleFace = detection(inputImage, this.pathXMLHaarcascade);
            EmployeeStructure[] employeeStructure;            

            if (rectangleFace.Length <= 0)
            {
                employeeStructure = new EmployeeStructure[0];
                employeeStructure[0].result = Result.NoDetected.ToString();
                return employeeStructure;
            }
            else
            {

                Image<Gray, byte> grayFrame = toGrayEqualizeFrame(inputImage);              
                employeeStructure = new EmployeeStructure[rectangleFace.Length];

                FaceRecognizer faceRecognition;
                

                switch (faceRecognizerMethode.ToString())
                {
                    case "EigenFaceRecognizerMethode": faceRecognition = new EigenFaceRecognizer(numComponentsEigen, thresholdEigen); //try catch aca
                        break;
                    case "FisherFaceRecognizerMethode": faceRecognition = new FisherFaceRecognizer(numComponentsFisher, thresholdFisher);
                        break;
                    case "LBPHFaceRecognizerMethode": faceRecognition = new LBPHFaceRecognizer(radiusLBPH, neighborsLBPH, gridXLBPH, gridYLBPH, thresholdLBPH);
                        break;
                    default: faceRecognition = new EigenFaceRecognizer(numComponentsEigen, thresholdEigen);
                        break;
                };

                faceRecognition.Load(pathImg + @"\" + "TrainingSet");

                Parallel.For(0, rectangleFace.Length, i =>
                {
                    Image<Gray, byte> faceEMGUCV = formatRectangleFaces(grayFrame.ToBitmap(), rectangleFace[i]);
                                
                    FaceRecognizer.PredictionResult ER = faceRecognition.Predict(faceEMGUCV);

                    if (ER.Label != -1 /*&& ER.Distance > thresholdEigen*/)
                    {
                        int label = ER.Label;

                        GenericRepository<Employee> emplyeeRepo = unitOfWork.GetRepoInstance<Employee>();
                        Employee em = emplyeeRepo.GetFirstOrDefault(label);

                        employeeStructure[i] = new EmployeeStructure(Result.Recognized.ToString(), em.name, em.middleName, em.lastName, em.email, rectangleFace[0].X, rectangleFace[0].Y, rectangleFace[0].Width, rectangleFace[0].Height);
                    }
                    employeeStructure[i].result = Result.Unknown.ToString();

                });

                return employeeStructure;
            }
        }

        public EmployeeStructure recognizeFaces(Image newImage, FaceRecognizerMethode faceRecognizerMethode)
        {
            var inputImage = new Image<Bgr, Byte>(new Bitmap(newImage));
            Rectangle[] rectangleFace = detection(inputImage, this.pathXMLHaarcascade);
            EmployeeStructure employeeStructure = new EmployeeStructure();

            if (rectangleFace.Length <= 0)
            { 
                employeeStructure.result = Result.NoDetected.ToString();
                return employeeStructure;
            }
            else if (rectangleFace.Length > 1)
            {
                employeeStructure.result = Result.MultipleFacesDetected.ToString();
                return employeeStructure;
            }
            else
            {
                Image<Gray, byte> grayFrame = toGrayEqualizeFrame(inputImage);
                Image<Gray, Byte> faceEMGUCV = formatRectangleFaces(grayFrame.ToBitmap(), rectangleFace[0]);

                //estimateParametersEigen(faceEMGUCV);

                FaceRecognizer faceRecognition;

                switch (faceRecognizerMethode.ToString())
                {
                    case "EigenFaceRecognizerMethode": faceRecognition = new EigenFaceRecognizer(numComponentsEigen, thresholdEigen); //try catch aca
                        break;
                    case "FisherFaceRecognizerMethode": faceRecognition = new FisherFaceRecognizer(numComponentsFisher, thresholdFisher);
                        break;
                    case "LBPHFaceRecognizerMethode": faceRecognition = new LBPHFaceRecognizer(radiusLBPH, neighborsLBPH, gridXLBPH, gridYLBPH, thresholdLBPH);
                        break;
                    default: faceRecognition = new EigenFaceRecognizer(numComponentsEigen, thresholdEigen);
                        break;
                };

                faceRecognition.Load(pathImg + @"\" + "TrainingSet");


                FaceRecognizer.PredictionResult ER = faceRecognition.Predict(faceEMGUCV);


                if (ER.Label != -1 /*&& ER.Distance > thresholdEigen*/)
                {
                    int label = ER.Label;

                    GenericRepository<Employee> emplyeeRepo = unitOfWork.GetRepoInstance<Employee>();
                    Employee em = emplyeeRepo.GetFirstOrDefault(label);

                    employeeStructure = new EmployeeStructure(Result.Recognized.ToString(), em.name, em.middleName, em.lastName, em.email, rectangleFace[0].X, rectangleFace[0].Y, rectangleFace[0].Width, rectangleFace[0].Height);

                    return employeeStructure;
                }
                employeeStructure.result = Result.Unknown.ToString();
                return employeeStructure;
            }
        }

        private Image<Gray, Byte> formatRectangleFaces(Bitmap grayFrame, Rectangle rectangleFace)
        {
            Point point = new Point(0, 0);
            Rectangle rec = new Rectangle(point, rectangleFace.Size);

            //This diferences (-45 and -5)are to take only the face and delete the edges that not are part of the face
            Bitmap extractedFace = new Bitmap(rectangleFace.Width - 45, rectangleFace.Height - 5);
            Graphics faceCanvas = Graphics.FromImage(extractedFace);
            //This sum (+20) are to take only the face and delete the edges that not are part of the face
            faceCanvas.DrawImage(grayFrame, rec, rectangleFace.X + 20, rectangleFace.Y, rectangleFace.Width, rectangleFace.Height, GraphicsUnit.Pixel);
            extractedFace = new Bitmap(extractedFace, new Size(widthImage, HeighImage));            

            //Aca normalizo la imagen
            Image<Gray, Byte> faceEMGUCV = new Image<Gray, Byte>(extractedFace);
            faceEMGUCV._EqualizeHist();

            return faceEMGUCV;
        }

        public Rectangle[] detection(Image<Bgr, Byte> inputImage, string pathXMLHaarcascade)
        {
            Rectangle[] rectangleFace = null;
            Image<Gray, byte> grayFrame;

            if (inputImage != null)
            {
                //ver si esto esta bien
                grayFrame = inputImage.Convert<Gray, Byte>();
                grayFrame._EqualizeHist();
                
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
            Image<Gray, byte>[] extractedFace;
            Rectangle[] rectangleFace = detection(inputImage, pathXMLHaarcascade);
            
            Image<Gray, byte> grayFrame = toGrayEqualizeFrame(inputImage);

            if (rectangleFace.Length > 0)
            {
                extractedFace = new Image<Gray, byte>[rectangleFace.Length];
                Parallel.For(0, rectangleFace.Length, i =>
                {
                    extractedFace[i] = formatRectangleFaces(grayFrame.ToBitmap(), rectangleFace[i]);
                    extractedFace[i].Save("");
                    //Image<Gray, byte> faceEMGUCV = new Image<Gray, byte>(extractedFace[i]);                                   
                });
            }
        }

        //0: Default, 1:to Accuracy 2: Middium, 3: Imprecise, 4:Ambiguous
        //public void estimateParametersEigen(IImage[] imagesInput, IImage[] imagesDB, int[] labels)
        public void estimateParametersEigen(Image<Gray, Byte> imagesInput, int accuracy)
        {
            int tmpNumComponentsEigen;
            double tmpThresholdEigen;
            int EficientNumComponentsEigen = 80;
            double EficientThresholdEigen = 2000;
            int countRecognitionFaces = 0;
            int countRecognitionFacesMax = 0;
            FaceRecognizer faceRecognition;

            for (tmpThresholdEigen = 1000; tmpThresholdEigen < 10000; tmpThresholdEigen+=100)
            {
                for (tmpNumComponentsEigen = 50; tmpNumComponentsEigen < 100; tmpNumComponentsEigen+=10)
                {
                    faceRecognition = new EigenFaceRecognizer(tmpNumComponentsEigen, tmpThresholdEigen);
                    GenericRepository<DistanceResult> distanceResultRepo = unitOfWork.GetRepoInstance<DistanceResult>();

                    int lengthArrays = distanceResultRepo.GetAllRecords().Count();
                    imagesDB = new Image<Gray, Byte>[lengthArrays];
                    labels = new int[lengthArrays];

                    int i = 0;
                    foreach (DistanceResult di in distanceResultRepo.GetAllRecords())
                    {
                        //This is to recalculate the faceRecognition and save it, but I think is not necesari declare imageDB and labels as global                    
                        imagesDB[i] = new Image<Gray, Byte>(pathImg + @"\" + di.photoName + ".Jpeg");
                        labels[i] = di.employeeId;
                        i++;
                    }


                    faceRecognition.Train(imagesDB, labels);


                    //faceRecognition.Load(pathImg + @"\" + "TrainingSet");
                    FaceRecognizer.PredictionResult ER = faceRecognition.Predict(imagesInput);

                    
                    if (ER.Label != -1)
                    {
                        if (accuracy == 1)
                        {
                            numComponentsEigen = EficientNumComponentsEigen;
                            thresholdEigen = EficientThresholdEigen;
                            return;
                        }
                        else if (accuracy == 2)
                        {
                            numComponentsEigen = EficientNumComponentsEigen;
                            thresholdEigen = EficientThresholdEigen + 300;
                            return;
                        }
                        else if (accuracy == 3)
                        {
                            numComponentsEigen = EficientNumComponentsEigen;
                            thresholdEigen = EficientThresholdEigen + 600;
                            return;
                        }
                        else if (accuracy == 4)
                        {
                            numComponentsEigen = EficientNumComponentsEigen;
                            thresholdEigen = EficientThresholdEigen + 900;
                            return;
                        }
                        else if (accuracy > 4)
                        {
                            thresholdEigen = Double.PositiveInfinity;
                        }
                        else return;
                    }
                    faceRecognition.Dispose();
                    //if (countRecognitionFaces > countRecognitionFacesMax)
                    //{
                    //    EficientNumComponentsEigen = tmpNumComponentsEigen;
                    //    EficientThresholdEigen = tmpThresholdEigen;
                    //}
                    //countRecognitionFaces = 0;
                }
            }

            
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

        public struct EmployeeStructure
        {
            public string name, middleName, lastName, email, result;
            int coorX, coorY, width, height;

            public EmployeeStructure(string result, string name, string middleName, string lastName, string email, int coorX, int coorY, int width, int height)
            {
                this.name = name;
                this.middleName = middleName;
                this.lastName = lastName;
                this.email = email;
                this.coorX = coorX;
                this.coorY = coorY;
                this.width = width;
                this.height = height;
                this.result = result;
            }
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