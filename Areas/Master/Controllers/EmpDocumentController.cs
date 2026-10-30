using HRIS.Areas.Common.Models;
using HRIS.Areas.Master.Models;
using HRIS.Helper;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HRIS.Areas.Master.Controllers
{
    [NoCache]
    [Authorize]
    public class EmpDocumentController : Controller
    {
        private static string optionName = "";
        #region public methods
        public ActionResult DataForm(string screenName)
        {
            optionName = screenName;
            return View();
        }

        [HttpPost]
        public JsonResult Save(EmpDocument _ed)
        {
            var msg = new Object();
            try
            {
                if (ServerSession.EditAllowed(optionName))
                {
                    #region Validation 
                    Validate(_ed);
                    #endregion

                    #region save to db
                    if (ModelState.IsValid)
                    {
                        if (EmpDocument.Save(_ed))
                        {
                            msg = new { success = true, message = AgilityEnums.Save_Success, formMode = AgilityEnums.formMode_Initialize };
                        }
                    }
                    else
                    {
                        msg = new
                        {
                            success = false,
                            message = ModelState.Keys.SelectMany(k => ModelState[k].Errors).Select(m => m.ErrorMessage).ToList(),//ModelState.Keys.SelectMany(k => ModelState[k].Errors).Select(m => m.ErrorMessage).ToArray(),
                            formMode = AgilityEnums.formMode_Edit
                        };
                    }
                    #endregion
                }
                else
                {
                    msg = new { success = false, message = AgilityEnums.Save_Access_Denied, formMode = AgilityEnums.formMode_Edit };
                }
            }
            catch (Exception ex)
            {
                msg = new { success = false, message = ex.Message, formMode = AgilityEnums.formMode_Edit };
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult Get(string _keyId)
        {
            var msg = new Object();
            try
            {
                EmpDocument _ed = EmpDocument.Get(_keyId);
                if (_ed == null)
                {
                    throw new Exception("The Employee Code doesn't exist or you donot have permission to access it");
                }
                msg = new { success = true, data = _ed, formMode = AgilityEnums.formMode_ReadOnly };
            }
            catch (Exception ex)
            {
                msg = new { success = false, message = ex.Message, formMode = AgilityEnums.formMode_Recall };
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Upload(string _employeeCode, short _docTypeId, DateTime? _expiryDate, short? _reminderDays, HttpPostedFileBase[] files)
        {
            var msg = new Object();
            try
            {
                #region save to db
                if (ModelState.IsValid)
                {
                    byte[] _fileBytes = new byte[files[0].ContentLength];
                    var _file = files[0].InputStream.Read(_fileBytes, 0, files[0].ContentLength);

                    if (EmpDocumentDetail.Save(_employeeCode, _docTypeId, files[0].FileName, files[0].ContentType, _expiryDate, _reminderDays, _fileBytes))
                    {
                        EmpDocument _ed = EmpDocument.Get(_employeeCode);
                        msg = new { success = true, data = _ed, formMode = AgilityEnums.formMode_Edit };
                    }
                }
                if (!ModelState.IsValid)
                {
                    msg = new
                    {
                        success = false,
                        message = ModelState.Keys.SelectMany(k => ModelState[k].Errors).Select(m => m.ErrorMessage).ToArray(),
                        formMode = AgilityEnums.formMode_Edit
                    };
                }
                #endregion
            }
            catch (Exception ex)
            {
                msg = new { success = false, message = ex.Message, formMode = AgilityEnums.formMode_Edit };
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Search()
        {
            var _data = new object();
            try
            {
                _data = new { success = true, columnDefs = ColumnDefinition.getColDef(new Employees()),
                    rowData = Employees.Get() };
                return Json(_data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _data = new { success = false, message = ex.Message };
                return Json(_data, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public FileStreamResult View(int _documentId)
        {
            EmpDocumentStream _eds = EmpDocumentStream.Get(_documentId);
            if (_eds != null)
            {
                MemoryStream ms = new MemoryStream();
                ms.Write(_eds.DocContent, 0, _eds.DocContent.Length);
                ms.Position = 0;
                Response.AddHeader("Content-Disposition", "attachment; filename=" + _eds.DocName);
                return new FileStreamResult(ms, _eds.DocContentType);
            }
            throw new Exception("File not found");
        }
        #endregion

        #region Json Method
        [HttpGet]
        public JsonResult GetLookups()
        {
            try
            {
                return Json(new
                {
                    success = true,
                    lstDocumentType = DocumentTypes.Get()
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMsg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region private methods
        private void Validate(EmpDocument _ed)
        {
            if (_ed.Details.Count > 0)
            {
                if (_ed.Details.Where(x => !x.DocTypeId.HasValue).Any())
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Document Type"));

                if (_ed.Details.Where(x => x.ExpiryDate.HasValue && x.Edit).Where(z => z.ExpiryDate.Value.Date < DateTime.Now.Date).Count() > 0)
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixgtCurrent("Expiry Date"));

                if (_ed.Details.Where(x => x.ReminderDays.HasValue && x.ReminderDays.Value < 0).Count() > 0)
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixZero("Reminder Days"));
            }
        }
        #endregion
    }
}