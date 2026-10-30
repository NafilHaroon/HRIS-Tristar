using HRIS.Areas.Common.Models;
using HRIS.Areas.Master.Models;
using HRIS.Areas.Transaction.Models;
using HRIS.Helper;
using HRIS.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static HRIS.Helper.AgilityEnums;

namespace HRIS.Areas.Transaction.Controllers
{
    [NoCache]
    [Authorize]
    public class TaxCreditController : Controller
    {
        private static string optionName = "";
        #region internal methods
        public ActionResult DataForm(string screenName)
        {
            optionName = screenName;
            return View();
        }

        public ActionResult Recall(string _formId)
        {
            optionName = "Tax Credit";
            ViewBag.TaxCreditId = _formId;
            return View("DataForm");
        }

        [HttpGet]
        public JsonResult Add()
        {
            try
            {
                ModelState.Clear();
                return Json( new { success = true, taxCredit = new TaxCredit(), formMode = AgilityEnums.formMode_Add }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMsg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Save(TaxCredit _tc)
        {
            var msg = new Object();
            try
            {
                if ((!_tc.TaxCreditId.HasValue && ServerSession.AddAllowed(optionName)) ||
                      (_tc.TaxCreditId.HasValue && ServerSession.EditAllowed(optionName)))
                {
                    #region validation
                    validate(_tc);
                    #endregion
                    #region save to db
                    if (ModelState.IsValid)
                    {
                        if (TaxCredit.Save(_tc))
                        {
                            msg = new { success = true, message = "Tax Credit entry saved successfully, please click submit to proceed this form onward for approval", formMode = AgilityEnums.formMode_ReadOnly, taxCredit = _tc };
                        }
                    }
                    else
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
        public JsonResult Get(short _keyId)
        {
            var msg = new Object(); 
            try
            {
                TaxCredit _tc = TaxCredit.Get(_keyId);
                if (_tc == null)
                {
                    throw new Exception("The Tax Credit Ref # doesn't exist or you donot have permission to access it");
                }
                msg = new { success = true, taxCredit = _tc, formMode = AgilityEnums.formMode_ReadOnly };
            }
            catch (Exception ex)
            {
                msg = new { success = false, message = ex.Message, formMode = AgilityEnums.formMode_Recall };
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult Search()
        {
            try
            {
                return Json(new
                {
                    success = true,
                    columnDefs = ColumnDefinition.getColDef(new TaxCredits()),
                    rowData = TaxCredits.Get()
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult History(int _formId, short _workFlowId)
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

        [HttpGet]
        public JsonResult GetLookups()
        {
            return Json(new
            {
                lstEmployee = EmployeeShortList.Get(),
                lstTaxCreditType = TaxCreditTypeList.Get(),
                lstAttachmentType = AttachmentTypes.Get()
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAttachmentTypes()
        {
            return Json(new
            {
                lstAttachmentType = AttachmentTypes.Get()
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Upload(int _creditId, short _attTypeId, HttpPostedFileBase[] files)
        {
            var msg = new Object();
            try
            {
                #region save to db
                if (ModelState.IsValid)
                {
                    byte[] _fileBytes = new byte[files[0].ContentLength];
                    var _file = files[0].InputStream.Read(_fileBytes, 0, files[0].ContentLength);

                    if (Attachment.Upload(_creditId, WorkFlow.TaxCredit, _attTypeId, files[0].FileName, files[0].ContentType, _fileBytes))
                    {
                        List<Attachment> lstAtt = Attachment.GetWOStream(_creditId, WorkFlow.TaxCredit);
                        msg = new { success = true, attachments = lstAtt };
                    }
                }
                if (!ModelState.IsValid)
                {
                    msg = new
                    {
                        success = false,
                        message = ModelState.Keys.SelectMany(k => ModelState[k].Errors).Select(m => m.ErrorMessage).ToArray(),
                    };
                }
                #endregion
            }
            catch (Exception ex)
            {
                msg = new { success = false, message = ex.Message };
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public FileStreamResult View(int _attachmentId)
        {
            Attachment _att = Attachment.GetStream(_attachmentId);
            if (_att != null)
            {
                MemoryStream ms = new MemoryStream();
                ms.Write(_att.AttachmentContent, 0, _att.AttachmentContent.Length);
                ms.Position = 0;
                Response.AddHeader("Content-Disposition", "attachment; filename=" + _att.AttachmentName);
                return new FileStreamResult(ms, _att.AttachmentContentType);
            }
            throw new Exception("File not found");
        }
        #endregion

        #region private methods
        private void validate(TaxCredit _tc)
        {
            ModelState.Clear();

            if (_tc.Completed)
                ModelState.AddModelError("Validation", "No further changes are allowed on this entry!");
            else
            {
                if (_tc.Owner != _tc.AuditTrail.CreatedBy)
                    ModelState.AddModelError("Validation", "No further changes could be applied by you!");

                if (!_tc.Employee.EmployeeId.HasValue)
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Employee"));

                if (!_tc.TaxCreditTypeId.HasValue)
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Tax Credit Type"));

                if (!_tc.TaxYear.HasValue)
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Tax Year"));
                else if (_tc.TaxYear < DateTime.Now.Year || _tc.TaxYear > DateTime.Now.Year + 1)
                    ModelState.AddModelError("Validation", "=> Tax year must be current");

                if (_tc.Amount <= 0)
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixgtZero("Amount"));
            }
        }
        #endregion

        #region form submission
        [HttpGet]
        public JsonResult FormSubmission(int _formId, short _stateId)
        {
            var msg = new Object();
            Recipient r = new Recipient();
            AgilityEnums.TaxCreditState nextStateId = AgilityEnums.TaxCreditState.New;
            try
            {
                RecipientModel rm = new RecipientModel();
                if (AgilityHelper.InList((AgilityEnums.TaxCreditState)_stateId,
                    AgilityEnums.TaxCreditState.Rejected, AgilityEnums.TaxCreditState.Approved,
                    AgilityEnums.TaxCreditState.Returned))
                {
                    r.RecipientList = Recipient.GetFormCreator(AgilityEnums.WorkFlow.TaxCredit, _formId);
                    nextStateId = (AgilityEnums.TaxCreditState)_stateId;
                }
                else
                {
                    r.RecipientList = Recipient.GetDirectManager();
                    nextStateId = AgilityEnums.TaxCreditState.SubmittedForApproval;
                }

                if (r.RecipientList.Count == 0)
                {
                    msg = "No submission user is configured for selected form state or No approving authority has access." +
                        "Submission can not proceed while submission users are missing." +
                        "Please contact your system admin for further details!";
                    return Json(new { success = false, message = msg }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    SelectList slRecipient = new SelectList(r.RecipientList, "RecipientId", "RecipientName", r.RecipientList[0].RecipientId);
                    rm.RecipientList = slRecipient;
                    rm.FormId = _formId;
                    rm.DocumentStatus = nextStateId.ToString();
                    rm.StateId = (short)nextStateId;
                    msg = new { success = true, RecipientModel = rm };
                }
            }
            catch (Exception ex)
            {
                msg = new { success = false, message = ex.Message };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult FormSubmitted(int _formId, short _stateId, string _comments, string _owner)
        {
            try
            {
                Submission _sub = new Submission();
                if (_stateId == (short)AgilityEnums.TaxCreditState.Rejected)
                {
                    _sub.Rejected = true;
                    _sub.Completed = true;
                }
                else if (_stateId == (short)AgilityEnums.TaxCreditState.Approved)
                {
                    _sub.Approved = true;
                    _sub.Completed = true;
                }

                _sub.Owner = _owner;
                _sub.Comments = _comments;
                _sub.FormId = _formId;
                _sub.StateId = _stateId;
                _sub.UserId = ServerSession.GetCurrentUser();

                if (TaxCredit.SubmitRequest(_sub))
                {
                    string _msg = "Tax Credit Request # " + _sub.FormId.ToString() + " was successfully submitted to " +
                        _sub.Owner +
                        (_sub.Comments == "" ? " with no comments " : " with the comments " + _sub.Comments);
                    return Json(new { success = true, message = _msg }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    string _msg = "Tax Credit Request submission failed, Please retry or contact your system admin for further details";
                    return Json(new { success = false, message = _msg }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}