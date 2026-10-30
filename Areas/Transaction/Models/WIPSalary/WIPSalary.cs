using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Transaction.Models
{
    public class WIPSalary : IDisposable
    {
        #region private properties
        private static readonly SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short? EmployeeId { get; set; }
        public int? SalaryId { get; set; }
        public string BranchName { get; set; }
        public string DepartmentName { get; set; }
        public string Designation { get; set; }
        public string PeriodName { get; set; }
        public string JoiningDate { get; set; }
        public short SalaryDays { get; set; }
        public short UnpaidDays { get; set; }
        public short LeaveDays { get; set; }
        public short PaidDays { get; set; }
        public bool Verified { get; set; }
        public List<WIPSalaryDetail> Salary { get; set; }
        public List<WIPConsumable> Consumable { get; set; }
        public short AbsentId { get; set; }
        public Footer AuditTrail { get; set; }
        #endregion

        #region constructor
        public WIPSalary()
        {
            AuditTrail = new Footer();
            Salary = new List<WIPSalaryDetail>();
            Consumable = new List<WIPConsumable>();
            AbsentId = ServerSession.GetAbsentId();
        }
        #endregion

        #region internal methods
        internal static WIPSalary Get(short _employeeId)
        {
            try
            {
                using (DbCommand dbCommandDetail = db.GetStoredProcCommand("GetWIPSalaryById"))
                {
                    db.AddInParameter(dbCommandDetail, "EmployeeId", SqlDbType.SmallInt, _employeeId);
                    db.AddInParameter(dbCommandDetail, "CompanyId", SqlDbType.SmallInt, ServerSession.GetCurrentCompany());
                    db.AddInParameter(dbCommandDetail, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());

                    using (DataSet ds = db.ExecuteDataSet(dbCommandDetail))
                    {
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            WIPSalary _ms = new WIPSalary();
                            DataRow dr = ds.Tables[0].Rows[0];
                            _ms.EmployeeId = _employeeId;
                            _ms.SalaryId = Convert.ToInt32(dr["SalaryId"]);
                            _ms.BranchName = dr["BranchName"].ToString();
                            _ms.DepartmentName = dr["DepartmentName"].ToString();
                            _ms.Designation = dr["Designation"].ToString();
                            _ms.PeriodName = dr["PeriodName"].ToString();
                            _ms.JoiningDate = dr["JoiningDate"].ToString();
                            _ms.SalaryDays = Convert.ToInt16(dr["SalaryDays"]);
                            _ms.UnpaidDays = Convert.ToInt16(dr["UnpaidDays"]);
                            _ms.LeaveDays = Convert.ToInt16(dr["LeaveDays"]);
                            //_cn.StopSalary = Convert.ToBoolean(dr["StopSalary"]);
                            _ms.PaidDays = Convert.ToInt16(_ms.SalaryDays - _ms.UnpaidDays);
                            //_cn.SalaryMonth = Convert.ToInt16(dr["SalaryMonth"]);
                            //_cn.SalaryYear = Convert.ToInt16(dr["SalaryYear"]);
                            _ms.Verified = Convert.ToBoolean(dr["Verified"]);
                            //if (_cn.Verified)
                            //{
                            //    _cn.VerifiedBy = dr["VerifiedBy"].ToString();
                            //    _cn.VerifiedOn = Convert.ToDateTime(dr["VerifiedOn"]);
                            //}
                            _ms.AuditTrail.CreatedBy = dr["CreatedBy"].ToString();
                            _ms.AuditTrail.CreatedOn = Convert.ToDateTime(dr["CreatedOn"]);
                            _ms.AuditTrail.UpdatedBy = dr["UpdatedBy"].ToString();
                            if (dr["UpdatedOn"] != DBNull.Value)
                                _ms.AuditTrail.UpdatedOn = Convert.ToDateTime(dr["UpdatedOn"]);
                            _ms.Salary = WIPSalaryDetail.Get(_ms.SalaryId.Value);
                            _ms.Consumable = WIPConsumable.Get(_ms.SalaryId.Value);
                            return _ms;
                        }
                        else
                            return null;
                    }
                }
            }
            catch (Exception)
            { throw; }
        }

        internal static bool Save(WIPSalary _ms)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("SaveWIPSalary"))
            {
                db.AddInParameter(dbCommand, "EmployeeId", SqlDbType.SmallInt, _ms.EmployeeId);
                db.AddInParameter(dbCommand, "UnpaidDays", SqlDbType.SmallInt, _ms.UnpaidDays);
                db.AddInParameter(dbCommand, "LeaveDays", SqlDbType.SmallInt, _ms.LeaveDays);
                //db.AddInParameter(dbCommand, "StopSalary", SqlDbType.Bit, _ms.StopSalary);
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                using (DbConnection dbconnection = db.CreateConnection())
                {
                    dbconnection.Open();
                    DbTransaction transaction = dbconnection.BeginTransaction();
                    try
                    {
                        db.ExecuteNonQuery(dbCommand, transaction);
                        WIPSalaryDetail.Save(_ms.SalaryId.Value, _ms.Salary, transaction);
                        WIPConsumable.Save(_ms.SalaryId.Value, _ms.Consumable, transaction);                   
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

        internal static bool Delete(short _employeeId)
        {
            try
            {
                using (DbCommand dbCommand = db.GetStoredProcCommand("DeleteWIPSalary"))
                {
                    db.AddInParameter(dbCommand, "EmployeeId", SqlDbType.SmallInt, _employeeId);
                    db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, ServerSession.GetCurrentCompany());
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