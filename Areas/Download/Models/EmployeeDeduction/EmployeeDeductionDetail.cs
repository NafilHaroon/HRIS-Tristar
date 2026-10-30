using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Download.Models
{
    public class EmployeeDeductionDetail : IDisposable
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
        public double Lunch { get; set; }
        public double PF { get; set; }
        public double EOBI { get; set; }
        public double ITax { get; set; }
        public double Transportation { get; set; }
        public double OtherDeductions { get; set; }
        #endregion

        #region constructor
        public EmployeeDeductionDetail()
        {
        }

        public EmployeeDeductionDetail(string _employeeCode, string _employeeName, string _designation,
            string _branchName, string _departmentName, 
            double _lunch, double _pf, double _eobi, double _iTax, double _transportation, double _otherDeductions)
        {
            EmployeeCode = _employeeCode;
            EmployeeName = _employeeName;
            Designation = _designation;
            BranchName = _branchName;
            DepartmentName = _departmentName;
            Lunch = _lunch;
            PF = _pf;
            EOBI = _eobi;
            ITax = _iTax;
            Transportation = _transportation;
            OtherDeductions = _otherDeductions;
        }
        #endregion

        #region internal methods
        internal static List<EmployeeDeductionDetail> Get()
        {
            List<EmployeeDeductionDetail> lstEDD = new List<EmployeeDeductionDetail>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("extEmployeeStandardDeduction"))
            {
                db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, ServerSession.GetCurrentCompany());
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            lstEDD.Add(new EmployeeDeductionDetail(
                                dr["EmployeeCode"].ToString(),
                                dr["EmployeeName"].ToString(),
                                dr["Designation"].ToString(),
                                dr["BranchName"].ToString(),
                                dr["DepartmentName"].ToString(),
                                Convert.ToDouble(dr["LunchDeduction"]),
                                Convert.ToDouble(dr["PFDeduction"]),
                                Convert.ToDouble(dr["EOBIDeduction"]),
                                Convert.ToDouble(dr["ITaxDeduction"]),
                                Convert.ToDouble(dr["TransportDeduction"]),
                                Convert.ToDouble(dr["OtherDeduction"])));
                        }
                    }
                }
            }
            return lstEDD;
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