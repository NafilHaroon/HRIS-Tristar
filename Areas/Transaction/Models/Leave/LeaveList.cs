using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.Serialization;

namespace HRIS.Areas.Transaction.Models
{
    public class LeaveList : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        [DataMember(Order = 0, Name = "Detail Id")]
        public int DetailId { get; set; }
        [DataMember(Order = 1, Name = "Employee Name")]
        public string EmployeeName { get; set; }
        [DataMember(Order = 2, Name = "Leave Type")]
        public string LeaveTypeName { get; set; }
        [DataMember(Order = 3, Name = "Date From")]
        public string DateFrom { get; set; }
        [DataMember(Order = 4, Name = "Date To")]
        public string DateTo { get; set; }
        #endregion

        #region constructor
        public LeaveList()
        {
        }

        public LeaveList(int _detailId, string _employeeName, string _loanTypeName, string _dateFrom, string _dateTo)
        {
            DetailId = _detailId;
            EmployeeName = _employeeName;
            LeaveTypeName = _loanTypeName;
            DateFrom = _dateFrom;
            DateTo = _dateTo;
        }
        #endregion

        #region internal methods
        internal static List<LeaveList> Get()
        {
            List<LeaveList> lstLoan = new List<LeaveList>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetLeaves"))
            {
                db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, ServerSession.GetCurrentCompany());
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            lstLoan.Add(new LeaveList(
                                Convert.ToInt32(dr["DetailId"]),
                                dr["EmployeeName"].ToString(),
                                dr["LeaveTypeName"].ToString(),
                                dr["DateFrom"].ToString(),
                                dr["DateTo"].ToString()));
                        }
                    }
                }
            }
            return lstLoan;
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