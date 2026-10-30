using HRIS.Areas.Common.Models;
using HRIS.Areas.Master.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Transaction.Models
{
    public class Advance: IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public int? AdvanceId { get; set; }
        public DateTime? AdvanceDate { get; set; }
        //public short? EmployeeId { get; set; }
        public EmployeeShortList Employee { get; set; }
        public string Description { get; set; }
        public double Amount { get; set; }
        public string StatusName { get; set; }
        public Footer AuditTrail { get; set; }
        #endregion

        #region constructor
        public Advance()
        {
            Employee = new EmployeeShortList();
            StatusName = "Open";
            AuditTrail = new Footer();
            AdvanceDate = DateTime.Now;
        }
        #endregion

        #region internal methods
        internal static Advance Get(int _advanceId)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetAdvanceSalaryById"))
            {
                db.AddInParameter(dbCommand, "AdvanceId", SqlDbType.Int, _advanceId);
                db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, ServerSession.GetCurrentCompany());
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds.Tables != null && ds.Tables[0].Rows.Count > 0)
                    {
                        using (Advance _adv = new Advance())
                        {
                            DataRow dr = ds.Tables[0].Rows[0];
                            _adv.AdvanceId = _advanceId;
                            _adv.AdvanceDate = Convert.ToDateTime(dr["AdvanceDate"]);
                            //_adv.EmployeeId = Convert.ToInt16(dr["EmployeeId"]);
                            _adv.Employee = new EmployeeShortList { EmployeeId = Convert.ToInt16(dr["EmployeeId"]), EmployeeName = dr["EmployeeName"].ToString() };
                            _adv.Description = dr["Description"].ToString();
                            _adv.Amount = Convert.ToDouble(dr["Amount"]);
                            _adv.StatusName = dr["StatusName"].ToString();
                            _adv.AuditTrail.CreatedBy = dr["CreatedBy"].ToString();
                            _adv.AuditTrail.CreatedOn = Convert.ToDateTime(dr["CreatedOn"]);
                            _adv.AuditTrail.UpdatedBy = dr["UpdatedBy"].ToString();
                            if (dr["UpdatedOn"] != DBNull.Value)
                                _adv.AuditTrail.UpdatedOn = Convert.ToDateTime(dr["UpdatedOn"]);
                            return _adv;
                        }
                    }
                    else
                        return null;
                }
            }
        }

        internal static bool Save(Advance _adv)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("SaveAdvanceSalary"))
            {
                try
                {
                    db.AddInParameter(dbCommand, "AdvanceId", SqlDbType.Int, _adv.AdvanceId);
                    db.AddInParameter(dbCommand, "AdvanceDate", SqlDbType.DateTime, _adv.AdvanceDate);
                    db.AddInParameter(dbCommand, "EmployeeId", SqlDbType.SmallInt, _adv.Employee.EmployeeId);//_adv.EmployeeId);
                    db.AddInParameter(dbCommand, "Description", SqlDbType.VarChar, _adv.Description);
                    db.AddInParameter(dbCommand, "Amount", SqlDbType.Float, _adv.Amount);
                    db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                    db.AddInParameter(dbCommand, "UpdatedOn", SqlDbType.DateTime, _adv.AuditTrail.UpdatedOn);
                    db.ExecuteNonQuery(dbCommand);
                    return true;
                }
                catch (Exception)
                { throw; }
            }
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            // no implementation
        }
        #endregion
    }
}