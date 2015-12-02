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
using System.Net.Http.Headers;
using System.Web;
using System.Web.Script.Serialization;

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
        public HttpResponseMessage saveImageStream()
        {
            string resp = "";
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

                //Begin bytes codification
                Byte[] buffer = requestContent.ReadAsByteArrayAsync().Result;

                int srcOffSet = 0;
                long[] parameters = new long[5];
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
                char[] charEmail = new char[parameters[3]];
                System.Buffer.BlockCopy(buffer, srcOffSet, charEmail, 0, charEmail.Length * sizeof(char));
                string email = new string(charEmail);

                srcOffSet += (int)parameters[3] * sizeof(char);
                byte[] byteImage = new byte[parameters[4]];
                System.Buffer.BlockCopy(buffer, srcOffSet, byteImage, 0, byteImage.Length);
                //End byte codification

                using (MemoryStream memoryStream = new MemoryStream(byteImage))
                {
                    using (Image image = Image.FromStream(memoryStream))
                    {
                        resp = recognize.saveEmployee(image, name, middleName, lastName, email);                  
                    }

                }

                Request.CreateResponse();
            }

            var result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new StringContent(resp);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");

            return result;
        }

        public async Task<HttpResponseMessage> saveImage()
        {

            string resp = "";

            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            string root = HttpContext.Current.Server.MapPath("~/App_Data");
            var provider = new MultipartFormDataStreamProvider(root);

            // Read the form data.
            await Request.Content.ReadAsMultipartAsync(provider);
            var data = provider.FileData;
            var content = provider.Contents;
            string jsonParams = content[1].ReadAsStringAsync().Result;



            Models.Employee employee = new JavaScriptSerializer().Deserialize<Models.Employee>(jsonParams);

            Byte[] byteImage = content[0].ReadAsByteArrayAsync().Result;
            using (MemoryStream memoryStream = new MemoryStream(byteImage))
            {
                using (Image image = Image.FromStream(memoryStream))
                {
                    resp = recognize.saveEmployee(image, employee.name, employee.middleName, employee.lastName, employee.email);
                }

            }

            var result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new StringContent(resp);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");



            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpPost]
        public HttpResponseMessage RecognizeImage(/*[FromBody] Image img*/)
        {
            string returnedString = "";
            
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
                
                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (Image image = Image.FromStream(memoryStream))
                    //Bitmap b = Bitmap.
                    {
                        returnedString = recognize.recognizeFaces(image, "", RecognizeBLL.FaceRecognizerMethode.EigenFaceRecognizerMethode);                
                    }

                }
            }        
            var result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new StringContent(returnedString);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");

            return result;
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