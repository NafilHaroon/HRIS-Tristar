using HRIS.Areas.Common.Models;
using HRIS.Helper;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class Employee : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short? EmployeeId { get; set; }
        public string EmployeeCode { get; set; }
        public short? SalutationId { get; set; }
        public string EmployeeName { get; set; }
        public short? WorkerTypeId { get; set; }
        public string HeadName { get; set; }
        public short? CompanyId { get; set; }
        public short? SupervisorId { get; set; }
        public string BranchCode { get; set; }
        public string DepartmentCode { get; set; }
        public short? GradeId { get; set; }
        public string Designation { get; set; }
        public short? HiringTypeId { get; set; } = 1;
        public short? ReplacementEmployeeId { get; set; }
        public string HiringBasis { get; set; }
        public short? JobNatureId { get; set; }
        public string Email { get; set; }
        public DateTime? JoiningDate { get; set; } = DateTime.Now;
        public DateTime? ConfirmationDueDate { get; set; }= DateTime.Now.AddMonths(3);
        public bool IsConfirmed { get; set; }
        public DateTime? ConfirmationDate { get; set; }
        public DateTime? SeparationDate { get; set; }
        public short? StatusId { get; set; } = 1;
        public string ReasonForSeperation { get; set; }
        public string ValidationError { get; set; }
        public short? MedicalPlanId { get; set; }
        public double MedicalCost { get; set; }
        public double LifeInsuranceCost { get; set; }
        public bool DisabledQuota { get; set; }
        public short? HFMSubCategoryId { get; set; }
        public EmpPersonalInfo EmpPersonalInfo { get; set; } = new EmpPersonalInfo();
        public List<EmpSalary> Salary { get; set; } = new List<EmpSalary>();
        public List<EmpCostAllocation> CostAllocations { get; set; } = new List<EmpCostAllocation>();
        public List<EmpFamily> Family { get; set; } = new List<EmpFamily>();
        public List<EmpEntitlement> Entitlements { get; set; } = new List<EmpEntitlement>();
        public Footer AuditTrail { get; set; } = new Footer();
        #endregion

        #region constructor
        public Employee()
        {

        }
        #endregion

        #region internal methods
        internal static Employee Get(string _empCode)
        {
            try
            {
                using (DbCommand dbCommandDetail = db.GetStoredProcCommand("GetEmployeeByCode"))
                {
                    db.AddInParameter(dbCommandDetail, "EmployeeCode", SqlDbType.VarChar, _empCode);
                    db.AddInParameter(dbCommandDetail, "CompanyId", SqlDbType.SmallInt, ServerSession.GetCurrentCompany());
                    db.AddInParameter(dbCommandDetail, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                    using (DataSet ds = db.ExecuteDataSet(dbCommandDetail))
                    {
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            using (Employee _emp = new Employee())
                            {
                                DataRow dr = ds.Tables[0].Rows[0];
                                _emp.EmployeeCode = _empCode;
                                _emp.EmployeeId = Convert.ToInt16(dr["EmployeeId"]);
                                _emp.WorkerTypeId = Convert.ToInt16(dr["WorkerTypeId"]);
                                _emp.SalutationId = Convert.ToInt16(dr["SalutationId"]);
                                _emp.EmployeeName = dr["EmployeeName"].ToString();
                                _emp.HeadName = dr["HeadName"].ToString();
                                _emp.CompanyId = Convert.ToInt16(dr["CompanyId"]);
                                _emp.SupervisorId = Convert.ToInt16(dr["SupervisorId"]);
                                _emp.BranchCode = dr["BranchCode"].ToString();
                                _emp.DepartmentCode = dr["DepartmentCode"].ToString();
                                _emp.GradeId = Convert.ToInt16(dr["GradeId"]);
                                _emp.Designation = dr["Designation"].ToString();
                                if (dr["HiringTypeId"] != DBNull.Value)
                                    _emp.HiringTypeId = Convert.ToInt16(dr["HiringTypeId"]);

                                if (_emp.HiringTypeId.HasValue && AgilityHelper.InList(_emp.HiringTypeId.Value, 2, 3) && dr["ReplacementEmployeeId"] != DBNull.Value)
                                    _emp.ReplacementEmployeeId = Convert.ToInt16(dr["ReplacementEmployeeId"]);
                                _emp.HiringBasis = dr["HiringBasis"].ToString();

                                _emp.JobNatureId = Convert.ToInt16(dr["JobNatureId"]);
                                _emp.Email = dr["Email"].ToString();
                                _emp.JoiningDate = Convert.ToDateTime(dr["JoiningDate"]);

                                if (dr["ConfirmationDueDate"] != DBNull.Value)
                                    _emp.ConfirmationDueDate = Convert.ToDateTime(dr["ConfirmationDueDate"]);

                                if (dr["ConfirmationDate"] != DBNull.Value)
                                {
                                    _emp.IsConfirmed = true;
                                    _emp.ConfirmationDate = Convert.ToDateTime(dr["ConfirmationDate"]);
                                }
                                else
                                    _emp.IsConfirmed = false;

                                _emp.StatusId = Convert.ToInt16(dr["StatusId"]);
                                if (_emp.StatusId != 1)
                                {
                                    if (dr["SeparationDate"] != DBNull.Value)
                                        _emp.SeparationDate = Convert.ToDateTime(dr["SeparationDate"]);
                                    _emp.ReasonForSeperation = dr["ReasonForSeparation"].ToString();
                                }

                                if (dr["MedicalPlanId"] != DBNull.Value)
                                {
                                    _emp.MedicalPlanId = Convert.ToInt16(dr["MedicalPlanId"]);
                                    _emp.MedicalCost = Convert.ToDouble(dr["MedicalCost"]);
                                }
                                _emp.LifeInsuranceCost = Convert.ToDouble(dr["LifeInsuranceCost"]);
                                _emp.DisabledQuota = Convert.ToBoolean(dr["DisabledQuota"]);
                                if (dr["HFMSubCategoryId"] != DBNull.Value)
                                    _emp.HFMSubCategoryId = Convert.ToInt16(dr["HFMSubCategoryId"]);
                                _emp.AuditTrail = new Footer(dr);

                                _emp.EmpPersonalInfo = EmpPersonalInfo.Get(_emp.EmployeeId.Value);
                                _emp.Salary = EmpSalary.Get(_emp.EmployeeId.Value);
                                _emp.CostAllocations = EmpCostAllocation.Get(_emp.EmployeeId.Value);
                                _emp.Family = EmpFamily.Get(_emp.EmployeeId.Value); 
                                _emp.Entitlements = EmpEntitlement.Get(_emp.EmployeeId.Value); 
                                return _emp;
                            }
                        }
                        else
                            return null;
                    }
                }
            }
            catch (Exception)
            { throw; }
        }

        internal static bool Save(Employee _emp)
        {
            string _userId = ServerSession.GetCurrentUser();
            _emp.EmployeeCode = _emp.EmployeeCode?.ToUpper();
            using (DbCommand dbCommand = db.GetStoredProcCommand("SaveEmployee"))
            {
                db.AddInParameter(dbCommand, "EmployeeId", SqlDbType.SmallInt, _emp.EmployeeId);
                db.AddInParameter(dbCommand, "EmployeeCode", SqlDbType.VarChar, _emp.EmployeeCode);
                db.AddInParameter(dbCommand, "SalutationId", SqlDbType.VarChar, _emp.SalutationId);
                db.AddInParameter(dbCommand, "EmployeeName", SqlDbType.VarChar, _emp.EmployeeName);
                db.AddInParameter(dbCommand, "HeadName", SqlDbType.VarChar, _emp.HeadName);
                db.AddInParameter(dbCommand, "WorkerTypeId", SqlDbType.SmallInt, _emp.WorkerTypeId);
                db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, _emp.CompanyId);
                db.AddInParameter(dbCommand, "SupervisorId", SqlDbType.SmallInt, _emp.SupervisorId);
                db.AddInParameter(dbCommand, "BranchCode", SqlDbType.VarChar, _emp.BranchCode);
                db.AddInParameter(dbCommand, "DepartmentCode", SqlDbType.VarChar, _emp.DepartmentCode);
                db.AddInParameter(dbCommand, "GradeId", SqlDbType.VarChar, _emp.GradeId);
                db.AddInParameter(dbCommand, "Designation", SqlDbType.VarChar, _emp.Designation);
                db.AddInParameter(dbCommand, "HiringTypeId", SqlDbType.SmallInt, _emp.HiringTypeId);
                db.AddInParameter(dbCommand, "ReplacementEmployeeId", SqlDbType.SmallInt, _emp.ReplacementEmployeeId);
                db.AddInParameter(dbCommand, "HiringBasis", SqlDbType.VarChar, _emp.HiringBasis);
                db.AddInParameter(dbCommand, "JobNatureId", SqlDbType.SmallInt, _emp.JobNatureId);
                db.AddInParameter(dbCommand, "Email", SqlDbType.VarChar, _emp.Email);
                db.AddInParameter(dbCommand, "JoiningDate", SqlDbType.DateTime, _emp.JoiningDate);
                db.AddInParameter(dbCommand, "ConfirmationDueDate", SqlDbType.DateTime, _emp.ConfirmationDueDate);
                db.AddInParameter(dbCommand, "ConfirmationDate", SqlDbType.DateTime, _emp.ConfirmationDate);
                db.AddInParameter(dbCommand, "SeparationDate", SqlDbType.DateTime, _emp.SeparationDate);
                db.AddInParameter(dbCommand, "ReasonForSeparation", SqlDbType.VarChar, _emp.ReasonForSeperation);
                db.AddInParameter(dbCommand, "StatusId", SqlDbType.SmallInt, _emp.StatusId);
                db.AddInParameter(dbCommand, "MedicalPlanId", SqlDbType.SmallInt, _emp.MedicalPlanId);
                db.AddInParameter(dbCommand, "MedicalCost", SqlDbType.Float, _emp.MedicalCost);
                db.AddInParameter(dbCommand, "LifeInsuranceCost", SqlDbType.Float, _emp.LifeInsuranceCost);
                db.AddInParameter(dbCommand, "DisabledQuota", SqlDbType.Bit, _emp.DisabledQuota);
                db.AddInParameter(dbCommand, "HFMSubCategoryId", SqlDbType.TinyInt, _emp.HFMSubCategoryId);
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, _userId);
                //db.AddOutParameter(dbCommand, "newEmployeeCode", SqlDbType.VarChar, 32);
                db.AddOutParameter(dbCommand, "newEmployeeId", SqlDbType.SmallInt, 32);
                db.AddInParameter(dbCommand, "UpdatedOn", SqlDbType.DateTime, _emp.AuditTrail.UpdatedOn);

                using (DbConnection dbconnection = db.CreateConnection())
                {
                    dbconnection.Open();
                    DbTransaction transaction = dbconnection.BeginTransaction();
                    try
                    {
                        db.ExecuteNonQuery(dbCommand, transaction);
                        //_emp.EmployeeCode = dbCommand.Parameters["@newEmployeeCode"].Value.ToString();
                        _emp.EmployeeId = Convert.ToInt16(dbCommand.Parameters["@newEmployeeId"].Value);

                        EmpPersonalInfo.Save(_emp.EmployeeId.Value, _emp.EmpPersonalInfo, _userId, transaction);
                        EmpSalary.Save(_emp.EmployeeId.Value, _emp.Salary, _userId, transaction);
                        EmpCostAllocation.Save(_emp.EmployeeId.Value, _emp.CostAllocations, _userId, transaction);
                        EmpFamily.Save(_emp.EmployeeId.Value, _emp.Family, _userId, transaction);
                        EmpEntitlement.Save(_emp.EmployeeId.Value, _emp.Entitlements, _userId, transaction);
                        transaction.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        //to method to be refactored later
        /// <summary>
        /// not final
        /// </summary>
        /// <param name="_emp"></param>
        /// <param name="_userId"></param>
        /// <returns></returns>
        internal static bool ChangeSupervisor(Employee _emp)
        {
            try
            {
                using (DbCommand dbCommand = db.GetStoredProcCommand("ChangeSupervisor"))
                {
                    db.AddInParameter(dbCommand, "BranchCode", SqlDbType.VarChar, _emp.BranchCode);
                    db.AddInParameter(dbCommand, "DepartmentCode", SqlDbType.VarChar, _emp.DepartmentCode);
                    db.AddInParameter(dbCommand, "CurrentSuperVisorId", SqlDbType.SmallInt, _emp.SupervisorId);
                    //db.AddInParameter(dbCommand, "NewSupervisorId", SqlDbType.SmallInt, _emp.SupervisorId);
                    db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                    db.ExecuteNonQuery(dbCommand);
                    return true;
                }
            }
            catch (Exception)
            { throw; }
        }

        internal static bool Delete(string _empId)
        {
            try
            {
                using (DbCommand dbCommand = db.GetStoredProcCommand("DeleteEmployee"))
                {
                    db.AddInParameter(dbCommand, "EmployeeId", SqlDbType.VarChar, _empId);
                    db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                    db.ExecuteNonQuery(dbCommand);
                    return true;
                }
            }
            catch (Exception)
            { throw; }
        }
        #endregion

        #region dispose method
        public void Dispose()
        {
        }
        #endregion
    }
}