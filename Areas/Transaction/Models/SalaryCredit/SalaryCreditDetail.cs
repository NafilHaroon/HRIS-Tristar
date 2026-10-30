using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace HRIS.Areas.Transaction.Models
{
    public class SalaryCreditDetail : IDisposable
    {
        #region private properties
        private static readonly SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public int? DetailId { get; set; }
        public short? EmployeeId { get; set; }
        //public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string BranchName { get; set; }
        public string DepartmentName { get; set; }
        public string GradeNo { get; set; }
        public string AccountNo { get; set; }
        public double NetAmount { get; set; }
        //public bool StopSalary { get; set; }
        public bool Selected { get; set; }
        #endregion

        #region constructor
        public SalaryCreditDetail()
        {
            DetailId = -1;
        }

        public SalaryCreditDetail(int _detailId, bool _selected, short _employeeId, //string _employeeCode, 
            string _employeeName, string _branchName, string _departmentName, string _gradeNo, 
            string _accountNo, double _netAmount)//, , bool _stopSalary
        {
            Selected = _selected;
            DetailId = _detailId;
            EmployeeId = _employeeId;
            //EmployeeCode = _employeeCode;
            EmployeeName = _employeeName;
            BranchName = _branchName;
            DepartmentName = _departmentName;
            GradeNo = _gradeNo;
            AccountNo = _accountNo;
            NetAmount = _netAmount;
            //StopSalary = _stopSalary;
        }
        #endregion

        #region internal methods
        internal static List<SalaryCreditDetail> Get(short _letterId)
        {
            try
            {
                List<SalaryCreditDetail> lstBD = new List<SalaryCreditDetail>();
                using (DbCommand dbCommand = db.GetStoredProcCommand("GetSalaryCreditLetterDetails"))
                {                   
                    db.AddInParameter(dbCommand, "LetterId", SqlDbType.SmallInt, _letterId);
                    //db.AddInParameter(dbCommand, "PeriodId", SqlDbType.TinyInt, ServerSession.GetCurrentPeriod());
                    db.AddInParameter(dbCommand, "CompanyId", SqlDbType.SmallInt, ServerSession.GetCurrentCompany());
                    db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                    using (DataSet ds = db.ExecuteDataSet(dbCommand))
                    {
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                lstBD.Add(new SalaryCreditDetail(
                                    Convert.ToInt32(dr["DetailId"]),
                                    Convert.ToBoolean(dr["Selected"]),
                                    //true,
                                    Convert.ToInt16(dr["EmployeeId"]),
                                    //dr["EmployeeCode"].ToString(),
                                    dr["EmployeeName"].ToString(),
                                    dr["BranchName"].ToString(),
                                    dr["DepartmentName"].ToString(),
                                    dr["GradeNo"].ToString(),
                                    dr["BankAccountNo"].ToString(),
                                    Convert.ToDouble(dr["NetAmount"])));
                            }
                        }
                    }
                }
                return lstBD;
            }
            catch (Exception)
            { throw; }
        }

        internal static List<SalaryCreditDetail> GetPendingSalaries(short _bankId, short _periodId)
        {
            try
            {
                List<SalaryCreditDetail> lstBD = new List<SalaryCreditDetail>();
                using (DbCommand dbCommand = db.GetStoredProcCommand("GetSalaryDetailsForLetter_Pending"))
                {
                    db.AddInParameter(dbCommand, "BankId", SqlDbType.SmallInt, _bankId);
                    db.AddInParameter(dbCommand, "PeriodId", SqlDbType.TinyInt, _periodId);
                    db.AddInParameter(dbCommand, "CompanyId", SqlDbType.SmallInt, ServerSession.GetCurrentCompany());
                    db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                    using (DataSet ds = db.ExecuteDataSet(dbCommand))
                    {
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                lstBD.Add(new SalaryCreditDetail(
                                    Convert.ToInt32(dr["DetailId"]),
                                    false,
                                    Convert.ToInt16(dr["EmployeeId"]),
                                    dr["EmployeeName"].ToString(),
                                    dr["BranchName"].ToString(),
                                    dr["DepartmentName"].ToString(),
                                    dr["GradeNo"].ToString(),
                                    dr["BankAccountNo"].ToString(),
                                    Convert.ToDouble(dr["NetAmount"])));
                            }
                        }
                    }
                }
                return lstBD;
            }
            catch (Exception)
            { throw; }
        }

        internal static bool Save(short _letterId, List<SalaryCreditDetail> _details, string _userId, DbTransaction _transaction)
        {
            try
            {
                foreach (SalaryCreditDetail _cdd in getSCDChanges(_details))
                {
                    using (DbCommand dbCommand = db.GetStoredProcCommand("SaveSalaryCreditDetail"))
                    {
                        db.AddInParameter(dbCommand, "DetailId", SqlDbType.SmallInt, _cdd.DetailId);
                        db.AddInParameter(dbCommand, "Selected", SqlDbType.Bit, _cdd.Selected);
                        db.AddInParameter(dbCommand, "LetterId", SqlDbType.SmallInt, _letterId);
                        db.AddInParameter(dbCommand, "EmployeeId", SqlDbType.SmallInt, _cdd.EmployeeId);
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

        #region private methods
        private static IEnumerable<SalaryCreditDetail> getSCDChanges(List<SalaryCreditDetail> _bld)
        {
            return _bld.Where(a=> (a.Selected && a.DetailId == 0) || (!a.Selected && a.DetailId > 0));
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