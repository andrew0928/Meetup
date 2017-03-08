using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DemoWeb.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.ServerIP = this.Request.ServerVariables["LOCAL_ADDR"];
            ViewBag.Version = this.GetType().Assembly.GetName().Version.ToString();

            return View();
        }

        public ActionResult Logo()
        {
            string file = this.Server.MapPath("~/App_Data/Logo.png");
            if (System.IO.File.Exists(file))
            {
                return File(file, "image/png");
            }
            else
            {
                return File("~/Content/default-logo.png", "image/png");
            }
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}