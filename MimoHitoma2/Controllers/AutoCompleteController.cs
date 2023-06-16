using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MimoHitoma2.Models;

namespace MimoHitoma2.Controllers
{
    public class AutoCompleteController : Controller
    {
        [HttpPost]
        public JsonResult Tags(string prefix)
        {
            AutoCompleteModel auto = new AutoCompleteModel();
            List<string> matchTags = auto.AutoComplete_GetImageTags(prefix);
            return Json(matchTags, JsonRequestBehavior.AllowGet);
        }
    }
}