using HRIS.Areas.Common.Models;
using HRIS.Helper;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace HRIS.Areas.Common.Controllers
{
    [NoCache]
    [Authorize]
    public class MyFormsController : Controller
    {
        #region public methods
        public ActionResult DataForm()
        {
            return View();
        }

        public JsonResult FormList(AgilityEnums.Forms _forms = AgilityEnums.Forms.Current,
            AgilityEnums.WorkFlow _workflowId = AgilityEnums.WorkFlow.All)
        {
            MyForm mf = new MyForm
            {
                ViewType = _forms
            };
            try
            {
                switch (_forms)
                {
                    case AgilityEnums.Forms.Sent:
                        mf.Detail = MyForm.SentForms(_workflowId);
                        break;
                    case AgilityEnums.Forms.Closed:
                        mf.Detail = MyForm.CompletedForms(_workflowId);
                        break;
                    default:
                        mf.Detail = MyForm.ActiveForms(_workflowId);
                        break;
                }
                return Json(new { success = true, myForm = mf }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, myForm = mf, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult History(int _formId, AgilityEnums.WorkFlow _workFlowId)
        {
            try
            {
                return Json(new
                {
                    success = true,
                    columnDefs = ColumnDefinition.getColDef(new FormHistory()),
                    rowData = FormHistory.Get(_formId, (AgilityEnums.WorkFlow)_workFlowId)
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}