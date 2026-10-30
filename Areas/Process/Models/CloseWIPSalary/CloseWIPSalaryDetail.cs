using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Process.Models
{
    public class CloseWIPSalaryDetail : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public int SalaryId { get; set; }
        public bool Close { get; set; }
        public string EmployeeName { get; set; }
        public string BranchName { get; set; }
        public string DepartmentName { get; set; }
        public string Designation { get; set; }
        public string UnpaidDays { get; set; }
        public string LeaveDays { get; set; }
        public string GrossSalary { get; set; }
        public string Allowance { get; set; }
        public string PersonalDeduction { get; set; }
        public string AbsentDeduction { get; set; }
        public string OtherDeduction { get; set; }
        public string NetSalary { get; set; }
        #endregion

        #region constructor
        public CloseWIPSalaryDetail()
        {
        }

        public CloseWIPSalaryDetail(int _salaryId, bool _close, string _employeeName, 
            string _branchName, string _departmentName, string _designation, string _unpaidDays, string _leaveDays, 
            string _grossSalary, string _allowance, string _personalDeduction, string _absentDeduction,
            string _otherDeduction, string _netSalary)
        {
            SalaryId = _salaryId;
            Close = _close;
            BranchName = _branchName;
            DepartmentName = _departmentName;
            EmployeeName = _employeeName;
            Designation = _designation;
            UnpaidDays = _unpaidDays;
            LeaveDays = _leaveDays;
            GrossSalary = _grossSalary;
            Allowance = _allowance;
            PersonalDeduction = _personalDeduction;
            AbsentDeduction = _absentDeduction;
            OtherDeduction = _otherDeduction;
            NetSalary = _netSalary;
        }
        #endregion

        #region internal methods
        internal static List<CloseWIPSalaryDetail> Get()
        {
            List<CloseWIPSalaryDetail> lstWSD = new List<CloseWIPSalaryDetail>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetWIPSalaryForVerification"))
            {
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, ServerSession.GetCurrentCompany());
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            lstWSD.Add(new CloseWIPSalaryDetail(
                                Convert.ToInt32(dr["SalaryId"]),
                                Convert.ToBoolean(dr["Verified"]),
                                dr["EmployeeName"].ToString(),
                                dr["BranchName"].ToString(),
                                dr["DepartmentName"].ToString(),
                                dr["Designation"].ToString(),
                                Convert.ToInt32(dr["UnPaidDays"]).ToString("#,#.00;(#,#.00)"),
                                Convert.ToInt32(dr["LeaveDays"]).ToString("#,#.00;(#,#.00)"),
                                Convert.ToDouble(dr["GrossSalary"]).ToString("#,#.00;(#,#.00)"),
                                Convert.ToDouble(dr["Allowances"]).ToString("#,#.00;(#,#.00)"),
                                Convert.ToDouble(dr["PersonalDeduction"]).ToString("#,#.00;(#,#.00)"),
                                Convert.ToDouble(dr["AbsentDeduction"]).ToString("#,#.00;(#,#.00)"),
                                Convert.ToDouble(dr["OtherDeduction"]).ToString("#,#.00;(#,#.00)"),
                                Convert.ToDouble(dr["NetSalary"]).ToString("#,#.00;(#,#.00)")));
                        }
                    }
                }
            }
            return lstWSD;
        }

        internal static bool CloseSalary(List<CloseWIPSalaryDetail> _details, DbTransaction _transaction)
        {
            try
            {
                string _userId = ServerSession.GetCurrentUser();
                foreach (CloseWIPSalaryDetail ms in _details)
                {
                    using (DbCommand dbCommand = db.GetStoredProcCommand("VerifyWIPSalary"))
                    {
                        db.AddInParameter(dbCommand, "SalaryId", SqlDbType.Int, ms.SalaryId);
                        db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, _userId);
                        db.ExecuteNonQuery(dbCommand, _transaction);
                    }
                }
                return true;
            }
            catch (Exception)
            { throw; }
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