using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace HRIS.Areas.Transaction.Models
{
    public class MonthlyLeaveDetail : IDisposable, IValidatableObject
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public int DetailId { get; set; }
        public short? LeaveTypeId { get; set; }     
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public bool HalfDay { get; set; }
        public string Reason { get; set; }
        public bool Add { get; set; }
        public bool Edit { get; set; }
        public bool Delete { get; set; }
        #endregion

        #region constructor
        public MonthlyLeaveDetail()
        {
            DetailId = -1;
            DateFrom = DateTime.Now;
            DateTo = DateTime.Now.AddDays(30);
            Add = true; Edit = false; Delete = false;
        }

        public MonthlyLeaveDetail(int _detailId, short _leaveTypeId,
            DateTime _dateFrom, DateTime _dateTo, bool _halfDay, string _reason)
        {
            DetailId = _detailId;
            LeaveTypeId = _leaveTypeId;
            DateFrom = _dateFrom;
            DateTo = _dateTo;
            HalfDay = _halfDay;
            Reason=_reason;
            Add = false; Edit = false; Delete = false;
        }
        #endregion

        #region internal methods
        internal static List<MonthlyLeaveDetail> Get(short _employeeId)
        {
            try
            {
                List<MonthlyLeaveDetail> lstMLD = new List<MonthlyLeaveDetail>();
                using (DbCommand dbCommand = db.GetStoredProcCommand("GetMonthlyLeavebyEmployee"))
                {
                    db.AddInParameter(dbCommand, "EmployeeId", SqlDbType.SmallInt, _employeeId);
                    db.AddInParameter(dbCommand, "CompanyId", SqlDbType.SmallInt, ServerSession.GetCurrentCompany());
                    db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                    using (DataSet ds = db.ExecuteDataSet(dbCommand))
                    {
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                lstMLD.Add(new MonthlyLeaveDetail(
                                    Convert.ToInt32(dr["DetailId"]),
                                    Convert.ToInt16(dr["LeaveTypeId"]),
                                    Convert.ToDateTime(dr["DateFrom"]),
                                    Convert.ToDateTime(dr["DateTo"]),
                                    Convert.ToBoolean(dr["HalfDay"]),                                   
                                    dr["Reason"].ToString()));
                            }
                        }
                    }
                }
                return lstMLD;
            }
            catch (Exception)
            { throw; }
        }

        internal static bool Save(short _employeeId, List<MonthlyLeaveDetail> _details, DbTransaction _transaction)
        {
            try
            {
                foreach (MonthlyLeaveDetail _mld in getCDDetailChanges(_details))
                {
                    using (DbCommand dbCommand = db.GetStoredProcCommand("SaveMonthlyLeave"))
                    {
                        db.AddInParameter(dbCommand, "DetailId", SqlDbType.SmallInt, _mld.DetailId);
                        db.AddInParameter(dbCommand, "EmployeeId", SqlDbType.SmallInt, _employeeId);
                        db.AddInParameter(dbCommand, "LeaveTypeId", SqlDbType.SmallInt, _mld.LeaveTypeId);
                        db.AddInParameter(dbCommand, "DateFrom", SqlDbType.DateTime, _mld.DateFrom);
                        db.AddInParameter(dbCommand, "DateTo", SqlDbType.DateTime, _mld.DateTo);
                        db.AddInParameter(dbCommand, "HalfDay", SqlDbType.Bit, _mld.HalfDay);
                        db.AddInParameter(dbCommand, "Reason", SqlDbType.VarChar, _mld.Reason);
                        db.AddInParameter(dbCommand, "Action", SqlDbType.Char, (
                           _mld.Add ? "I" : (_mld.Delete ? "D" : "U")));
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
        private static IEnumerable<MonthlyLeaveDetail> getCDDetailChanges(List<MonthlyLeaveDetail> _details)
        {
            return (_details.Where(x => (x.Add && !x.Delete) || (x.Edit && !x.Delete) || x.Delete));
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            //
        }
        #endregion

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> res = new List<ValidationResult>();
            if (DateTo.Value.Date < DateFrom.Value.Date)
            {
                res.Add(new ValidationResult("Date To must always be newer than or equal to Date From", new string[] { "DateTo" }));
            }
            return res;
        }
    }
}