using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Download.Models
{
    public class EmployeeSalaryDetail : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string Designation { get; set; }
        public string BranchName { get; set; }
        public string DepartmentName { get; set; }
        public string Grade { get; set; }
        public DateTime JoiningDate { get; set; }
        public string Qualification { get; set; }
        public double GrossSalary { get; set; }
        public double CarAllowance { get; set; }
        public double FuelAllowance { get; set; }
        public double MobileAllowance { get; set; }
        public double CarMaintAllowance { get; set; }
        public double OtherAllowance { get; set; }
        #endregion

        #region constructor
        public EmployeeSalaryDetail()
        {
        }

        public EmployeeSalaryDetail(string _employeeCode, string _employeeName, string _designation,
            string _branchName, string _departmentName, string _grade, DateTime _joiningDate, string _paymentMode, 
            double _grossSalary, double _carAllowance, double _fuelAllowance, double _mobileAllowance,
            double _carMaintAllowance, double _otherAllowance)
        {
            EmployeeCode = _employeeCode;
            EmployeeName = _employeeName;
            Designation = _designation;
            BranchName = _branchName;
            DepartmentName = _departmentName;
            Grade = _grade;
            JoiningDate = _joiningDate;
            Qualification = _paymentMode;
            GrossSalary = _grossSalary;
            CarAllowance = _carAllowance;
            FuelAllowance = _fuelAllowance;
            MobileAllowance = _mobileAllowance;
            CarMaintAllowance = _carMaintAllowance;
            OtherAllowance = _otherAllowance;
        }
        #endregion

        #region internal methods
        internal static List<EmployeeSalaryDetail> Get()//string _employeeCode, string _branchCode, string _departmentCode, short _gradeId)
        {
            List<EmployeeSalaryDetail> lstESD = new List<EmployeeSalaryDetail>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("extEmployeeSalaryPackage"))
            {
                db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, ServerSession.GetCurrentCompany());
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            lstESD.Add(new EmployeeSalaryDetail(
                                dr["EmployeeCode"].ToString(),
                                dr["EmployeeName"].ToString(),
                                dr["Designation"].ToString(),
                                dr["BranchName"].ToString(),
                                dr["DepartmentName"].ToString(),
                                dr["GradeNo"].ToString(),
                                Convert.ToDateTime(dr["JoiningDate"]),
                                dr["QualificationName"].ToString(),
                                Convert.ToDouble(dr["GrossSalary"]),
                                Convert.ToDouble(dr["CarAllowance"]),
                                Convert.ToDouble(dr["FuelAllowance"]),
                                Convert.ToDouble(dr["MobileAllowance"]),
                                Convert.ToDouble(dr["CarMaintenanceAllowance"]),
                                Convert.ToDouble(dr["OtherAllowance"])));
                        }
                    }
                }
            }
            return lstESD;
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            //
        }
        #endregion
    }
}