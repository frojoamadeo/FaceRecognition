using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using System.Net;
using System.Net.Http;
using System.Collections.Specialized;
using System.IO;
using System.Drawing.Imaging;
using System.Web;
using Newtonsoft.Json;
using System.Web.Script.Serialization;

namespace Camera
{
    public partial class Form1 : Form
    {
        private Capture capture;        //takes images from camera as image frames
        private bool captureInProgress; // checks if capture is executing
        private Mat ImageFrame;
        

        public Form1()
        {
            InitializeComponent();
        }

        private void ProcessFrame(object sender, EventArgs arg)
        {
            ImageFrame = capture.QueryFrame();  //line 1

            

            if (ImageFrame != null)
            {
                
                
                
            }
            CamImageBox.Image = ImageFrame;
            ImageFrame = null;
            //CamImageBox.Image = ImageFrame;        //line 2
            
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            #region if capture is not created, create it now
            if (capture == null)
            {
                try
                {
                    capture = new Capture();
                }
                catch (NullReferenceException excpt)
                {
                    MessageBox.Show(excpt.Message);
                }
            }
            #endregion

            if (capture != null)
            {
                if (captureInProgress)
                {  //if camera is getting frames then stop the capture and set button Text
                    // "Start" for resuming capture
                    btnStart.Text = "Start!"; //
                    Application.Idle -= ProcessFrame;
                }
                else
                {
                    //if camera is NOT getting frames then start the capture and set button
                    // Text to "Stop" for pausing capture
                    btnStart.Text = "Stop";
                    Application.Idle += ProcessFrame;
                }

                captureInProgress = !captureInProgress;
            }
        }

        private void ReleaseData()
        {
            if (capture != null)
                capture.Dispose();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Recognize_Click(object sender, EventArgs e)
        {
            var request = (HttpWebRequest)WebRequest.Create("http://localhost:60507/api/recognize/RecognizeImage");


            ((HttpWebRequest)request).UserAgent = ".NET Framework FaceRecognition";

            Mat frame = capture.QueryFrame();
            Bitmap b = frame.Bitmap;

            Image im = b;

            ImageConverter converter = new ImageConverter();
            byte[] imageInBytes = (byte[])converter.ConvertTo(im, typeof(byte[]));

            //Esto despues sacar
            request.Timeout = 10000000;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = imageInBytes.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(imageInBytes, 0, imageInBytes.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();
            
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            
            EmployeeStructure employee = new JavaScriptSerializer().Deserialize<EmployeeStructure>(responseString);

            if (employee.result == "Recognized")
                label1.Text = employee.name + " " + employee.lastName;
            else label1.Text = employee.result;
            frame = null;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var request = (HttpWebRequest)WebRequest.Create("http://localhost:60507/api/recognize/saveImageStream");


            ((HttpWebRequest)request).UserAgent = ".NET Framework Example Client";

            Mat frame = capture.QueryFrame();
            Bitmap b = frame.Bitmap;

            Image im = b;

            //Begin bytes codification
            ImageConverter converter = new ImageConverter();
            byte[] imageInBytes = (byte[])converter.ConvertTo(im, typeof(byte[]));
            
            long[] parameters = new long[5];

            string name = textBox1.Text;
            string middleName = textBox2.Text;
            string lastName = textBox3.Text;
            string email = textBox4.Text;
            
            parameters[0] = name.Length;
            parameters[1] = middleName.Length;
            parameters[2] = lastName.Length;
            parameters[3] = email.Length;
            parameters[4] = imageInBytes.Length;
          
            byte[] bytesParameters = new byte[parameters.Length * sizeof(long)];
            Buffer.BlockCopy(parameters, 0, bytesParameters, 0, bytesParameters.Length);
            byte[] bytesName = new byte[name.Length * sizeof(char)];
            System.Buffer.BlockCopy(name.ToCharArray(), 0, bytesName, 0, bytesName.Length);
            byte[] bytesMiddleName = new byte[middleName.Length * sizeof(char)];
            System.Buffer.BlockCopy(middleName.ToCharArray(), 0, bytesMiddleName, 0, bytesMiddleName.Length);
            byte[] bytesLastName = new byte[lastName.Length * sizeof(char)];
            System.Buffer.BlockCopy(lastName.ToCharArray(), 0, bytesLastName, 0, bytesLastName.Length);
            byte[] bytesEmail = new byte[email.Length * sizeof(char)];
            System.Buffer.BlockCopy(email.ToCharArray(), 0, bytesEmail, 0, bytesEmail.Length);

            List<byte> result = new List<byte>();
            result.AddRange(bytesParameters);
            result.AddRange(bytesName);
            result.AddRange(bytesMiddleName);
            result.AddRange(bytesLastName);
            result.AddRange(bytesEmail);
            result.AddRange(imageInBytes);
            
            byte[] byteToSend = result.ToArray();

            //End bytes codification

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteToSend.Length;
            
            //request.Headers.Add("MiddleName: Augusto");
            //request.Headers.Add("LastName: Rojo Amadeo");
            using (var stream = request.GetRequestStream())
            {
                stream.Write(byteToSend, 0, byteToSend.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();


            
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            frame = null;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (var client = new HttpClient())
            {
                using (var content = new MultipartFormDataContent())
                {
                    Mat frame = capture.QueryFrame();
                    Bitmap b = frame.Bitmap;

                    Image im = b;

                    //Begin bytes codification
                    ImageConverter converter = new ImageConverter();
                    byte[] imageInBytes = (byte[])converter.ConvertTo(im, typeof(byte[]));
                    Stream stream = new MemoryStream(imageInBytes);
                    client.BaseAddress = new Uri("http://localhost:60507/api/recognize/saveImage");

                    content.Add(new StreamContent(stream), "ImageCapture");
                  
                    Employee employee = new Employee { name = textBox1.Text, middleName = textBox2.Text, lastName = textBox3.Text, email = textBox4.Text };
                      
                    var json = JsonConvert.SerializeObject(employee);                                      

                    var jsonParams = new StringContent(json, Encoding.UTF8, "application/json");
                    content.Add(jsonParams, "Parametros");

                    
                    //content.Add(new (parameters, new JsonMediaTypeFormatter()), "parameters");

                    var result = client.PostAsync("/api/recognize/saveImage", content).Result;

                    var stringResult = result.Content.ReadAsStringAsync().Result;

                    label6.Text = stringResult;
                }
            }
        }

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

        private void button3_Click(object sender, EventArgs e)
        {
            var request = (HttpWebRequest)WebRequest.Create("http://localhost:60507/api/recognize/RecognizeMultipleImage");


            ((HttpWebRequest)request).UserAgent = ".NET Framework FaceRecognition";

            Mat frame = capture.QueryFrame();
            Bitmap b = frame.Bitmap;

            Image im = b;

            ImageConverter converter = new ImageConverter();
            byte[] imageInBytes = (byte[])converter.ConvertTo(im, typeof(byte[]));

            //Esto despues sacar
            request.Timeout = 10000000;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = imageInBytes.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(imageInBytes, 0, imageInBytes.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            List<EmployeeStructure> employees = new List<EmployeeStructure>();

            employees = new JavaScriptSerializer().Deserialize<List<EmployeeStructure>>(responseString);

            foreach (EmployeeStructure em in employees)
            {
                if (em.result == "Recognized")
                    label1.Text += em.name + " " + em.lastName;
                else label1.Text = em.result;
            }
            
            frame = null;
        }
    }
}
