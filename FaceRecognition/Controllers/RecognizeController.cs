using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FaceRecognition.Business_Logic;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace FaceRecognition.Controllers
{
    public class RecognizeController : ApiController
    {
        // GET api/<controller>

        RecognizeBLL recognize = new RecognizeBLL();


        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        //public void Post([FromBody]string value)
        //{

        //}


        //[ActionName("saveImage")]
        [HttpPost]
        public string saveImage()
        {
            var result = new HttpResponseMessage(HttpStatusCode.OK);
            if (Request.Content.IsMimeMultipartContent())
            {
                StreamContent content = (StreamContent)Request.Content;
                Task<Stream> task = content.ReadAsStreamAsync();
                Stream readOnlyStream = task.Result;
                Byte[] buffer = new Byte[readOnlyStream.Length];
                readOnlyStream.Read(buffer, 0, buffer.Length);
                MemoryStream memoryStream = new MemoryStream(buffer);
                Image image = Image.FromStream(memoryStream);

            }
            else
            {

                HttpContent requestContent = Request.Content;
                Byte[] buffer = requestContent.ReadAsByteArrayAsync().Result;

                int srcOffSet = 0;
                long[] parameters = new long[4];
                System.Buffer.BlockCopy(buffer, srcOffSet, parameters, 0, parameters.Length * sizeof(long));
                srcOffSet += parameters.Length * sizeof(long);

                char[] charName = new char[parameters[0]];
                System.Buffer.BlockCopy(buffer, srcOffSet, charName, 0, charName.Length * sizeof(char));
                string name = new string(charName);
                
                srcOffSet += (int)parameters[0] * sizeof(char);
                char[] charMiddleName = new char[parameters[1]];
                System.Buffer.BlockCopy(buffer, srcOffSet, charMiddleName, 0, charMiddleName.Length * sizeof(char));
                string middleName = new string(charMiddleName);

                srcOffSet += (int)parameters[1] * sizeof(char);
                char[] charLastName = new char[parameters[2]];
                System.Buffer.BlockCopy(buffer, srcOffSet, charLastName, 0, charLastName.Length * sizeof(char));
                string lastName = new string(charLastName);

                srcOffSet += (int)parameters[2] * sizeof(char);
                byte[] byteImage = new byte[parameters[3]];
                System.Buffer.BlockCopy(buffer, srcOffSet, byteImage, 0, byteImage.Length);
                


                //MemoryStream memoryStream = new MemoryStream(buffer);
                //Image image = Image.FromStream(memoryStream);
                //memoryStream.Dispose();
                //image.Save("aaaydh");

                using (MemoryStream memoryStream = new MemoryStream(byteImage))
                {
                    using (Image image = Image.FromStream(memoryStream))
                    //Bitmap b = Bitmap.
                    {
                        recognize.saveEmployee(image, name, middleName, lastName, "rojo@gmail");

                        //image.Save(memoryStream,ImageFormat.Jpeg);
                    }

                }

                Request.CreateResponse();
            }
            return "ok";
        }

        [HttpPost]
        public string RecognizeImage(/*[FromBody] Image img*/)
        {
            //MemoryStream ms = new MemoryStream(img);
            //Image returnImage = Image.FromStream(ms);
            
            //img.Save("aaa", ImageFormat.Jpeg);

            string returnedString;
            var result = new HttpResponseMessage(HttpStatusCode.OK);
            if (Request.Content.IsMimeMultipartContent())
            {
                StreamContent content = (StreamContent)Request.Content;
                Task<Stream> task = content.ReadAsStreamAsync();
                Stream readOnlyStream = task.Result;
                Byte[] buffer = new Byte[readOnlyStream.Length];
                readOnlyStream.Read(buffer, 0, buffer.Length);
                MemoryStream memoryStream = new MemoryStream(buffer);
                Image image = Image.FromStream(memoryStream);

            }
            else
            {

                HttpContent requestContent = Request.Content;
                Byte[] buffer = requestContent.ReadAsByteArrayAsync().Result;

                //MemoryStream memoryStream = new MemoryStream(buffer);
                //Image image = Image.FromStream(memoryStream);
                //memoryStream.Dispose();
                //image.Save("aaaydh");
                
                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (Image image = Image.FromStream(memoryStream))
                    //Bitmap b = Bitmap.
                    {
                        returnedString = recognize.recognizeFaces(image, "", RecognizeBLL.FaceRecognizerMethode.EigenFaceRecognizerMethode);

                        //image.Save(memoryStream,ImageFormat.Jpeg);
                    }

                }

                Request.CreateResponse();
                //StreamContent content = (StreamContent)Request.Content;
                //content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("multipart/related; boundary=cbsms-main-boundary");



                //Task<Stream> task = content.ReadAsStreamAsync();
                //Stream readOnlyStream = task.Result;
                //Byte[] buffer = new Byte[readOnlyStream.Length];
                //readOnlyStream.Read(buffer, 0, buffer.Length);
                //MemoryStream memoryStream = new MemoryStream(buffer);
                //Image image = Image.FromStream(memoryStream);

            }

            //var result = new HttpResponseMessage(HttpStatusCode.OK);
           // if (Request.Content.IsMimeMultipartContent())
            //{
                //Request.Content.ReadAsMultipartAsync<MultipartMemoryStreamProvider>(new MultipartMemoryStreamProvider()).ContinueWith((task) =>
                //{
                //    MultipartMemoryStreamProvider provider = task.Result;
                //    foreach (HttpContent content in provider.Contents)
                //    {
                //        Stream stream = content.ReadAsStreamAsync().Result;
                //        Image image = Image.FromStream(stream);
                //        var testName = content.Headers.ContentDisposition.Name;
                //        String filePath = HostingEnvironment.MapPath("~/Images/");
                //        String[] headerValues = (String[])Request.Headers.GetValues("UniqueId");
                //        String fileName = headerValues[0] + ".jpg";
                //        String fullPath = Path.Combine(filePath, fileName);
                //        image.Save(fullPath);
                //    }
                //});
               
           // }

            ////recognize.recognizeFaces(img,"", "",RecognizeBLL.FaceRecognizerMethode.EigenFaceRecognizerMethode);
            //recognize.saveEmployee(returnImage, "Juan", "Ignacio", "Fer", "juanignaaaa");

            return "ok";
        }
        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}