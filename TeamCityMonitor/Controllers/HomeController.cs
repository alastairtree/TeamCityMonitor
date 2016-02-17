using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BuildMonitor.Repository;
using BuildMonitor.Attributes;

namespace BuildMonitor.Controllers
{   [DisableCaching]
    public class HomeController : Controller
    {
        [DisableCaching]
        public ActionResult Index()
        {
            return View(); ;
        }

        [DisableCaching]
        public JsonResult GetProjects(int page = 1, int items = 25)
        {
            try
            {
                using (BuildMonitorRepository repo = new BuildMonitorRepository(Properties.Settings.Default.TeamCityURL))
                {
                    var nextPage=string.Empty;
                    bool morePages;
                    var results = repo.GetPageOfProjectSummary(out morePages, page, items);

                    if (morePages)
                        nextPage = String.Format("/Home/GetProjects?page={0}&items={1}", page + 1, items);

                    return Json(new { success = true, results = results, nextPage= nextPage }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


    }
}
