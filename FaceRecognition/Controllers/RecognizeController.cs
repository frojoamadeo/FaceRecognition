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
        [HttpPost]
        public string Post(/*[FromBody] Image img*/)
        {
            //MemoryStream ms = new MemoryStream(img);
            //Image returnImage = Image.FromStream(ms);
            
            //img.Save("aaa", ImageFormat.Jpeg);
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
                        recognize.saveEmployee(image, "Juan", "Ignacio", "Fer", "rojo");

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