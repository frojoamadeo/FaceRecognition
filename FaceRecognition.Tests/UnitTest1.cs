using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web;
using FaceRecognition.Controllers;
using System.Drawing;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Drawing.Imaging;
namespace FaceRecognition.Tests
{
    [TestClass]
    public class UnitTest1
    {
        //private HttpContextBase context;
        string url = "http://localhost:60507/api/recognize/2/";
        RecognizeController recognizeController = new RecognizeController();

        [TestMethod]
        public void TestMethod1()
        {
        }

        [TestMethod]
        public void TestPostImage()
        {
            Bitmap img1 = new Bitmap(@"C:\Users\felipe.rojo.amadeo\Documents\Visual Studio 2013\Projects\FaceRecognition\FaceRecognition\emgucv.jpg");
            
            
            
            //controller.Request = new HttpRequestMessage();
            //controller.Configuration = new HttpConfiguration();
            var response = recognizeController.Post(img1);
            // Act
            
            // Assert
            
            //Assert.IsTrue(response.TryGetContentValue<Product>(out product));
            Assert.AreEqual("ok", response);

            
            
            //String filePath = HostingEnvironment.MapPath("~/Images/HT.jpg");
        }
    }
}
