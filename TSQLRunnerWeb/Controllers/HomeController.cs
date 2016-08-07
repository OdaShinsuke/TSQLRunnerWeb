using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TSQLRunnerWeb.Models;

namespace TSQLRunnerWeb.Controllers {
  public class HomeController : Controller {
    [HttpGet]
    public ActionResult Index() {
      return View(new IndexModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Query(IndexModel model) {
      if (ModelState.IsValid) {
        var errors = await model.RunQuery();
        foreach (var error in errors) {
          if (error.MemberNames == null || !error.MemberNames.Any()) {
            ModelState.AddModelError("", error.ErrorMessage);
          }
          else {
            foreach (var name in error.MemberNames) {
              ModelState.AddModelError(name, error.ErrorMessage);
            }
          }
        }
      }
      if (ModelState.IsValid) {
        model.GenerateExecPlan(Server.MapPath("/qp_content.xslt"));
      }
      return PartialView("_Main", model);
    }
  }
}