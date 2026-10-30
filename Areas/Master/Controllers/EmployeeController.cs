using HRIS.Areas.Common.Models;
using HRIS.Areas.Master.Models;
using HRIS.Helper;
using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace HRIS.Areas.Master.Controllers
{
    [NoCache]
    [Authorize]
    public class EmployeeController : Controller
    {
        private static string optionName = "";
        #region public methods
        public ActionResult DataForm(string screenName)
        {
            optionName = screenName;
            return View();
        }

        [HttpGet]
        public JsonResult Add()
        {
            try
            {
                return Json(new { success = true, employee = new Employee(), formMode = AgilityEnums.formMode_Add }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMsg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Save(Employee _emp)
        {
            var msg = new Object();
            try
            {
                if ((!_emp.EmployeeId.HasValue && ServerSession.AddAllowed(optionName)) ||
                      (_emp.EmployeeId.HasValue && ServerSession.EditAllowed(optionName)))
                {
                    #region validation
                    validate(_emp);
                    #endregion

                    #region save to db
                    if (ModelState.IsValid)
                    {
                        if (!_emp.CompanyId.HasValue)
                            _emp.CompanyId = ServerSession.GetCurrentCompany(); 
                        if (Employee.Save(_emp))
                        {
                            msg = new { success = true, message = AgilityEnums.Save_Success, formMode = AgilityEnums.formMode_Initialize };
                        }
                    }
                    else
                    {
                        msg = new
                        {
                            success = false,
                            message = ModelState.Keys.SelectMany(k => ModelState[k].Errors).Select(m => m.ErrorMessage).ToList(),
                            formMode = AgilityEnums.formMode_Edit
                        };
                    }
                    #endregion
                }
                else
                {
                    msg = new { success = false, message = "Sorry, you don't have required access to Save this Entry", formMode = AgilityEnums.formMode_Edit };
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
                Employee _emp = Employee.Get(_keyId);
                if (_emp == null)
                {
                    throw new Exception("The Employee Code doesn't exist or you donot have permission to access it");
                }
                msg = new
                {
                    success = true,
                    data = _emp,
                    lstReplacement = EmployeeShortList.GetEmployeesNotReplaced(_emp.ReplacementEmployeeId),
                    formMode = AgilityEnums.formMode_ReadOnly
                };
            }
            catch (Exception ex)
            {
                msg = new { success = false, message = ex.Message, formMode = AgilityEnums.formMode_Recall };
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Delete(string _keyId)
        {
            try
            {
                if (ServerSession.DeleteAllowed(optionName))
                {
                    if (Employee.Delete(_keyId))
                        return Json(new { success = true, message = "Employee Id " + _keyId + " deleted successfully", formMode = AgilityEnums.formMode_Initialize }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { success = false, message = "Sorry, you don't have access to Delete Function", formMode = AgilityEnums.formMode_Edit }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, formMode = AgilityEnums.formMode_Edit }, JsonRequestBehavior.AllowGet);
            }
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
                    lstBranch = Branches.Get(true),
                    lstDepartment = Departments.Get(),
                    lstSupervisor = EmployeeShortList.GetAllEmployeesForCompany(),
                    lstReplacement = EmployeeShortList.GetEmployeesNotReplaced(-1),
                    lstEmployeeStatus = EmpStatuses.Get(),
                    lstJobNature = JobNatures.Get(),
                    lstMaritalStatus = MaritalStatuses.Get(),
                    lstGrade = Grades.Get(true),
                    lstQualification = Qualifications.Get(),
                    lstPaymentMode = PaymentModes.Get(),
                    lstBank = Banks.Get(true),
                    lstEntitlementType = EntitlementTypes.Get(),
                    lstRelation = Relations.Get(),
                    lstMedicalPlan = MedicalPlans.Get(),
                    lstSalaryHead = SalaryHeads.Get(true),
                    lstAllocTemplate = Allocations.Get(true),
                    lstWorkerType = WorkerTypes.Get(),
                    lstSalutation = Salutations.Get(),
                    lstSubCategory = HFMSubCategory.Get()
                }, JsonRequestBehavior.AllowGet);
                //lstAttachmentType = AttachmentTypeList.Get(true),
                //lstLeaveType = LeaveTypeList.Get(true),
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMsg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region private methods
        private void validate(Employee e)
        {
            if (!e.SalutationId.HasValue)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Salutation"));

            if (string.IsNullOrWhiteSpace(e.EmployeeName))
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Employee Name"));

            if (string.IsNullOrWhiteSpace(e.HeadName))
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Father Name"));

            if (e.EmpPersonalInfo.GenderId == 2 && e.EmpPersonalInfo.MaritalStatusId == 1 && string.IsNullOrWhiteSpace(e.EmpPersonalInfo.HusbandName))
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Husband Name"));

            if (string.IsNullOrWhiteSpace(e.BranchCode))
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Branch"));

            if (string.IsNullOrEmpty(e.DepartmentCode))
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Department"));

            if (!e.GradeId.HasValue)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Grade"));

            if (string.IsNullOrEmpty(e.Designation))
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Designation"));

            if (!e.SupervisorId.HasValue)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Supervisor"));

            if (!e.WorkerTypeId.HasValue)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Worker Type"));

            if (!e.StatusId.HasValue)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Status"));

            if (!e.JobNatureId.HasValue)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Job Nature"));

            if (!e.EmpPersonalInfo.PaymentModeId.HasValue)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Payment Mode"));

            if (!e.HFMSubCategoryId.HasValue)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("HFM Sub Category"));

            if (e.StatusId!=1)
            {
                if (!e.SeparationDate.HasValue)
                {
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Seperation Date"));
                }              
            }

            if (e.EmpPersonalInfo.PaymentModeId == 1)
            {
                if (!e.EmpPersonalInfo.CompanyBankId.HasValue)
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Company Bank"));

                if (string.IsNullOrWhiteSpace(e.EmpPersonalInfo.BankAccountNo))
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("BankAccountNo"));
            }

            if (e.HiringTypeId.HasValue)
            {
                if (e.HiringTypeId == 2 && !e.ReplacementEmployeeId.HasValue)
                    ModelState.AddModelError("Validation", "Please select valid employee name from drop down against which this replacement is hired");

                if (e.HiringTypeId == 3 && !e.ReplacementEmployeeId.HasValue)
                    ModelState.AddModelError("Validation", "Please select valid employee name from drop down against which this replacement is hired");

                if (AgilityHelper.InList(e.HiringTypeId.Value, 4, 5, 6) && string.IsNullOrWhiteSpace(e.HiringBasis))
                {
                    switch(e.HiringTypeId.Value)
                    {
                        case 4:
                            ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("ERF #"));
                            break;
                        case 5:
                            ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Project Name"));
                            break;
                        case 6:
                            ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Hiring Details"));
                            break;
                    }
                }
            }

            if (e.JoiningDate.Value > e.ConfirmationDueDate.Value)
            {
                ModelState.AddModelError("Validation", "The Confirmation Due Date can't be older than Joining Date");
            }

            if (e.IsConfirmed && e.JoiningDate.Value > e.ConfirmationDate.Value)
            {
                ModelState.AddModelError("Validation", "The confirmation data can't be older than joining date");
            }

            if (!e.EmpPersonalInfo.MaritalStatusId.HasValue)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Marital Status"));

            if (!e.EmpPersonalInfo.GenderId.HasValue)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Gender"));

            if (string.IsNullOrWhiteSpace(e.EmpPersonalInfo.FamilyNo))
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Family #"));

            if ( string.IsNullOrEmpty(e.EmpPersonalInfo.CurrentAddress))
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Current Address"));

            if (e.EmpPersonalInfo.DifferentPermanentAdd && string.IsNullOrEmpty(e.EmpPersonalInfo.PermanentAddress))
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Permanent Address"));

            if (!string.IsNullOrWhiteSpace(e.Email))
            {
                string emailFormat = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
                if (!Regex.IsMatch(e.Email, emailFormat))
                    ModelState.AddModelError("Validation", AgilityHelper.EmailFormat("Email Address"));
            }

            if (!string.IsNullOrWhiteSpace(e.EmpPersonalInfo.CellNo))
            {
                //ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Mobile #"));
                //else 
                if (!Regex.IsMatch(e.EmpPersonalInfo.CellNo, @"^\(?([0-9]{4})\)?[-]?([0-9]{7})$"))
                    ModelState.AddModelError("Validation", AgilityHelper.CallFormat("Mobile #"));
            }

            if (!string.IsNullOrWhiteSpace(e.EmpPersonalInfo.PhoneNo))
            {
                if (!Regex.IsMatch(e.EmpPersonalInfo.PhoneNo, @"^\(?([0-9]{3,4})\)?[-]?([0-9]{6,7})$"))
                    ModelState.AddModelError("Validation", AgilityHelper.CallFormat("Phone #"));
            }

            if (!e.EmpPersonalInfo.WorkingSince.HasValue)
            {
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Working Since Date"));
            }          

            if (string.IsNullOrWhiteSpace(e.EmpPersonalInfo.CNICNo))
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("CNIC #"));
            else if (!string.IsNullOrWhiteSpace(e.EmpPersonalInfo.CNICNo) && !Regex.IsMatch(e.EmpPersonalInfo.CNICNo, @"^\(?([0-9]{5})\)?[-]?([0-9]{7})?[-]?([0-9]{1})$"))
                ModelState.AddModelError("Validation", "Please enter CNIC # in valid format like xxxxx-xxxxxxx-x");

            if (!e.MedicalPlanId.HasValue)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Medical Plan"));

            if (e.MedicalCost < 0)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixZero("Medical Cost"));

            if (e.LifeInsuranceCost < 0)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixZero("Life Insurance"));

            if (e.Salary.Count == 0)
                ModelState.AddModelError("Validation", "At least one Salary Head must exist in Salary Setup before saving the Employee profile");
            else
            {
                if (e.Salary.Where(x => !x.Delete && !x.HeadId.HasValue).Any())
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Salary head"));

                if (e.Salary.Where(x => !x.Delete && x.Amount <= 0).Any())
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixgtZero("Amount"));

                if (e.Salary.Where(y=> !y.Delete).GroupBy(x => x.HeadId).Any(grp => grp.Count() > 1))
                    ModelState.AddModelError("Validation", "Salary heads must be unique");
            }

            if (e.CostAllocations.Count == 0)
                ModelState.AddModelError("Validation", "Cost allocation must be defined and total of allocation must be 100");
            else
            {
                if (e.CostAllocations.Where(x => !x.Delete && string.IsNullOrEmpty(x.BranchCode)).Any())
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Allocation Branch"));

                if (e.CostAllocations.Where(x => !x.Delete && !x.TemplateId.HasValue).Any())
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Allocation Template"));

                if (e.CostAllocations.Where(x => !x.Delete &&  x.Rate <= 0).Any())
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixgtZero("Allocation Rate"));

                if (e.CostAllocations.Where(y=> !y.Delete).GroupBy(x => new { x.BranchCode, x.TemplateId }).Any(grp => grp.Count() > 1))
                    ModelState.AddModelError("Validation", "Allocation Branch & Template must be unique");

                if (e.CostAllocations.Sum(x => x.Rate) != 100)
                    ModelState.AddModelError("Validation", "The cost allocation Total must be 100");
            }

            if (e.Family.Count == 0)
                ModelState.AddModelError("Validation", "Atleast one Family member must be defined");
            else
            {
                if (e.Family.Where(x => !x.Delete && string.IsNullOrEmpty(x.MemberName)).Any())
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Family Member Name"));

                if (e.Family.Where(x => !x.Delete && !x.RelationId.HasValue).Any())
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Family Relation"));

                if (e.Family.Where(x => !x.Delete && !x.DoB.HasValue).Any())
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Date of Birth"));

                if (e.Family.Where(x => !x.Delete && x.EmergencyContact && string.IsNullOrWhiteSpace(x.ContactNo)).Any())
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Emergency Contact #"));

                if (!e.Family.Where(x => !x.Delete && x.EmergencyContact).Any())
                    ModelState.AddModelError("Validation", "There must be at least one Family member marked as Emergency Contact!");

                if (e.Family.Where(x => !string.IsNullOrWhiteSpace(x.ContactNo)).Any())
                {
                    if (e.Family.Where(x => !string.IsNullOrWhiteSpace(x.ContactNo) && !Regex.IsMatch(x.ContactNo, @"^\(?([0-9]{3,4})\)?[-]?([0-9]{6,7})$")).Any())
                        ModelState.AddModelError("Validation", AgilityHelper.CallFormat("Emergency Contact #"));
                }               
            }

            if (e.Entitlements.Count > 0)
            {
                if (e.Entitlements.Where(x => !x.Delete && !x.EntitlementTypeId.HasValue).Any())
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Entitlement Type"));

                if (e.Entitlements.Where(x => !x.Delete && string.IsNullOrWhiteSpace(x.RegistrationNo)).Any())
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Entitlement Reg #"));

                if (e.Entitlements.Where(x => !x.Delete && string.IsNullOrWhiteSpace(x.Model)).Any())
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Entitlement Model"));

                if (e.Entitlements.Where(x => !x.Delete && x.BaseValue < 0).Any())
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixgtZero("Base Value"));

                if (e.Entitlements.Where(x => !x.Delete && x.Limit < 0).Any())
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixgtZero("Entitlement Limit"));

                if (e.Entitlements.Where(x => !x.Delete && !x.IssueDate.HasValue).Any())
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixgtZero("Entitlement Issue Date"));

                if (e.Entitlements.Where(y=> !y.Delete).GroupBy(x => new { x.RegistrationNo}).Any(grp => grp.Count() > 1))
                    ModelState.AddModelError("Validation",  "Entitlement Reg # must be unique");
            }
        }
        #endregion
    }
}