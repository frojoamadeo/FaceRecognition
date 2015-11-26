using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FaceRecognition.Business_Logic;

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

        public string Post([FromBody]Image img)
        {
            //recognize.recognizeFaces(img,"", "",RecognizeBLL.FaceRecognizerMethode.EigenFaceRecognizerMethode);
            
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