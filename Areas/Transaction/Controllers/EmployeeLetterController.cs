using HRIS.Areas.Common.Models;
using HRIS.Areas.Master.Models;
using HRIS.Areas.Transaction.Models;
using HRIS.Helper;
using System;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace HRIS.Areas.Transaction.Controllers
{
    [NoCache]
    [Authorize]
    public class EmployeeLetterController : Controller
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
            optionName = "Employee Letter";
            ViewBag.LetterId = _formId;
            return View("DataForm");
        }

        [HttpGet]
        public JsonResult Add()
        {
            try
            {
                ModelState.Clear();
                return Json( new { success = true, employeeLetter = new EmployeeLetter(), formMode = AgilityEnums.formMode_Add }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMsg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Save(EmployeeLetter _el)
        {
            var msg = new Object();
            try
            {
                if ((string.IsNullOrWhiteSpace(_el.LetterNo) && ServerSession.AddAllowed(optionName)) ||
                      (!string.IsNullOrWhiteSpace(_el.LetterNo) && ServerSession.EditAllowed(optionName)))
                {
                    #region validation
                    validate(_el);
                    #endregion
                    #region save to db
                    if (ModelState.IsValid)
                    {
                        if (EmployeeLetter.Save(_el))
                        {
                            msg = new { success = true, message = "Employee Letter saved successfully, please click submit to proceed this form onward for approval", formMode = AgilityEnums.formMode_ReadOnly, employeeLetter = _el };
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

        [HttpPost]
        public JsonResult Cancel(string  _letterNo)
        {
            var msg = new Object();
            try
            {               
                #region Canel to db
                if (ModelState.IsValid)
                {
                    if (EmployeeLetter.CancelLetter(_letterNo))
                    {
                        msg = new { success = true, formMode = AgilityEnums.formMode_Initialize };
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
            catch (Exception ex)
            {
                msg = new { success = false, message = ex.Message, formMode = AgilityEnums.formMode_Edit };
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult Get(string _letterNo, int? _letterId)
        {
            var msg = new Object();
            try
            {
                EmployeeLetter _el = EmployeeLetter.Get(_letterNo, _letterId);
                if (_el == null)
                {
                    throw new Exception("The Letter Ref # doesn't exist or you donot have permission to access it");
                }
                msg = new { success = true, employeeLetter = _el, formMode = AgilityEnums.formMode_ReadOnly };
            }
            catch (Exception ex)
            {
                msg = new { success = false, message = ex.Message, formMode = AgilityEnums.formMode_Recall };
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
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
        public JsonResult New(short _employeeId, short _templateId )
        {
            var msg = new Object();
            try
            {
                EmployeeLetter _el = EmployeeLetter.New(_employeeId, _templateId);
                if (_el == null)
                {
                    throw new Exception("The Employee OR Letter Template doesn't exist or you donot have permission to access it");
                }
                msg = new { success = true, employeeLetter = _el, formMode = AgilityEnums.formMode_Edit };
            }
            catch (Exception ex)
            {
                msg = new { success = false, message = ex.Message, formMode = AgilityEnums.formMode_Edit };
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
                    columnDefs = ColumnDefinition.getColDef(new EmployeeLetterList()),
                    rowData = EmployeeLetterList.Get()
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
            return Json(new {
                lstAllEmployee = EmployeeShortList.Get(false),
                lstLetterTemplate = LetterTemplates.Get(true),
                lstSalaryHead = SalaryHeads.Get(true, true)
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region private methods
        private void validate(EmployeeLetter _el)
        {
            if (_el.Completed)
                ModelState.AddModelError("Validation", "No further changes are allowed on this Letter!");
            else
            {
                if (_el.Owner != _el.AuditTrail.CreatedBy || !AgilityHelper.InList(_el.StateId, 1, 5))
                    ModelState.AddModelError("Validation", "=> No further changes could be applied by you at this stage!");

                if (!_el.Employee.EmployeeId.HasValue)
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Employee"));

                if (!_el.TemplateId.HasValue)
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Letter Template"));

                if (!_el.TemplateVersion.HasValue)
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Template Version"));

                if (string.IsNullOrEmpty(_el.LetterContent))
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Letter Content"));
                else
                {
                    if (_el.LetterContent.IndexOf("@@@Letter_No") < 0)
                        ModelState.AddModelError("Validation", "Letter Template must contain literal @@@Letter_No to embed system generated unique letter number while saving Letter");
                    if (_el.ImpactsPayroll && !_el.NoSalaryChange && _el.LetterContent.IndexOf("@@@Salary_Breakup") < 0)
                        ModelState.AddModelError("Validation", "Letter Template must contain literal @@@Salary_Breakup when `Impacts Payroll` flag is turned-on");
                    if (_el.NoSalaryChange && _el.LetterContent.IndexOf("@@@Salary_Breakup") >= 0)
                        ModelState.AddModelError("Validation", "@@@Salary_Breakup literal could only be used if 'No Change in Salary' flag is turned off");
                }
            }
        }
        #endregion

        #region form submission
        [HttpGet]
        public JsonResult FormSubmission(int _formId, short _stateId)
        {
            var msg = new Object();
            Recipient r = new Recipient();
            AgilityEnums.EmployeeLetterState nextStateId = AgilityEnums.EmployeeLetterState.New;
            try
            {
                RecipientModel rm = new RecipientModel();
                if (AgilityHelper.InList((AgilityEnums.EmployeeLetterState)_stateId,
                    AgilityEnums.EmployeeLetterState.Rejected))//, AgilityEnums.EmployeeLetterState.Approved,
                    //AgilityEnums.EmployeeLetterState.Returned))
                {
                    r.RecipientList = Recipient.GetFormCreator(AgilityEnums.WorkFlow.EmployeeLetter, _formId);
                    nextStateId = (AgilityEnums.EmployeeLetterState)_stateId;
                }
                else
                {
                    r.RecipientList = Recipient.GetLetterRecipient(_formId, 
                        (AgilityEnums.EmployeeLetterState)_stateId, out nextStateId);
                    //nextStateId = AgilityEnums.EmployeeLetterState.SubmittedForApproval;
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
                if (_stateId == (short)AgilityEnums.EmployeeLetterState.Rejected)
                {
                    _sub.Rejected = true;
                    _sub.Completed = true;
                }
                else if (_stateId == (short)AgilityEnums.EmployeeLetterState.Approved)
                {
                    _sub.Approved = true;
                    _sub.Completed = true;
                }

                _sub.Owner = _owner;
                _sub.Comments = _comments;
                _sub.FormId = _formId;
                _sub.StateId = _stateId;
                _sub.UserId = ServerSession.GetCurrentUser();

                if (EmployeeLetter.SubmitRequest(_sub))
                {
                    string _msg = "Employee Letter # " + _sub.FormId.ToString() + " was successfully submitted to " +
                        _sub.Owner + (_sub.Comments == "" ? " with no comments " : " with the comments " + _sub.Comments);
                    return Json(new { success = true, message = _msg }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    string _msg = "Employee Letter submission failed, Please retry or contact your system admin for further details";
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