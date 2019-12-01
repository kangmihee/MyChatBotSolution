using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ChatBotApplication.Controllers
{
    public class KeyboardController : Controller
    {

        public ActionResult Index()
        {
            Keyboard keyboard = new Keyboard();

            keyboard.type = "buttons";
            keyboard.buttons = new string[] { "인사", "대화" };

            return Json(keyboard, JsonRequestBehavior.AllowGet);
        }
    }

    public class Keyboard
    {
        public string type { get; set; }
        public string[] buttons { get; set; }
    }
}



