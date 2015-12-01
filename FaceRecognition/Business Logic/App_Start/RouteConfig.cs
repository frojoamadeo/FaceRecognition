using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Routing.Constraints;
using System.Web.Mvc;
using System.Web.Routing;

namespace FaceRecognition
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
                //constraints: new { key = new GuidRouteConstraint() } //Agregado
            );

            //routes.MapRoute(
            //    name: "Default2",
            //    url: "{controller}/{saveImage}/{id}",
            //    defaults: new { action = "saveImage", id = UrlParameter.Optional }
            //    //constraints: new { key = new GuidRouteConstraint() } //Agregado
            //);
        }
    }
}
