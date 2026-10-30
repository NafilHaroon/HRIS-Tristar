using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Transaction.Models
{
    public class SalaryCreditLetter : IDisposable
    {
        #region private properties
        private static readonly SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short? LetterId { get; set; }
        public DateTime? LetterDate { get; set; }
        public short? BankId { get; set; }
        public short? PeriodId { get; set; }
        public string Remarks { get; set; }
        public bool Printed { get; set; }
        public List<SalaryCreditDetail> Details { get; set; }
        public Footer AuditTrail { get; set; }
        #endregion

        #region constructor
        public SalaryCreditLetter()
        {
            AuditTrail = new Footer();
            LetterDate = DateTime.Now;
            PeriodId = ServerSession.GetCurrentPeriod();
            Printed = false;
            Details = new List<SalaryCreditDetail>();
        }
        #endregion

        #region internal methods
        internal static SalaryCreditLetter Get(short _letterId)
        {
            try
            {
                using (DbCommand dbCommand = db.GetStoredProcCommand("GetSalaryCreditLetterById"))
                {
                    db.AddInParameter(dbCommand, "LetterId", SqlDbType.SmallInt, _letterId);
                    db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, ServerSession.GetCurrentCompany());
                    db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                    using (DataSet ds = db.ExecuteDataSet(dbCommand))
                    {
                        if (ds.Tables != null && ds.Tables[0].Rows.Count > 0)
                        {
                            using (SalaryCreditLetter _bl = new SalaryCreditLetter())
                            {
                                DataRow dr = ds.Tables[0].Rows[0];
                                _bl.LetterId = _letterId;
                                _bl.LetterDate = Convert.ToDateTime(dr["LetterDate"]);
                                _bl.BankId = Convert.ToInt16(dr["BankId"]);
                                _bl.PeriodId = Convert.ToInt16(dr["PeriodId"]);
                                _bl.Remarks = dr["Remarks"].ToString();
                                _bl.Printed = Convert.ToBoolean(dr["Printed"]);
                                _bl.AuditTrail.CreatedBy = dr["CreatedBy"].ToString();
                                _bl.AuditTrail.CreatedOn = Convert.ToDateTime(dr["CreatedOn"]);
                                _bl.AuditTrail.UpdatedBy = dr["UpdatedBy"].ToString();
                                if (dr["UpdatedOn"] != DBNull.Value)
                                    _bl.AuditTrail.UpdatedOn = Convert.ToDateTime(dr["UpdatedOn"]);
                                _bl.Details = SalaryCreditDetail.Get(_bl.LetterId.Value);
                                return _bl;
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

        internal static bool Save(SalaryCreditLetter _bl)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("SaveSalaryCreditLetter"))
            {
                db.AddInParameter(dbCommand, "LetterId", SqlDbType.SmallInt, _bl.LetterId);
                db.AddInParameter(dbCommand, "CompanyId", SqlDbType.SmallInt, ServerSession.GetCurrentCompany()); //_bl.CompanyId);
                db.AddInParameter(dbCommand, "LetterDate", SqlDbType.DateTime, _bl.LetterDate);
                db.AddInParameter(dbCommand, "BankId", SqlDbType.SmallInt, _bl.BankId);
                db.AddInParameter(dbCommand, "PeriodId", SqlDbType.SmallInt, _bl.PeriodId);
                db.AddInParameter(dbCommand, "Remarks", SqlDbType.VarChar, _bl.Remarks);
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                db.AddInParameter(dbCommand, "UpdatedOn", SqlDbType.DateTime, _bl.AuditTrail.UpdatedOn);
                db.AddOutParameter(dbCommand, "NewLetterId", SqlDbType.SmallInt, 32);
                using (DbConnection dbconnection = db.CreateConnection())
                {
                    dbconnection.Open();
                    DbTransaction transaction = dbconnection.BeginTransaction();
                    try
                    {
                        db.ExecuteNonQuery(dbCommand, transaction);
                        _bl.LetterId = Convert.ToInt16(dbCommand.Parameters["@NewLetterId"].Value);
                        SalaryCreditDetail.Save(_bl.LetterId.Value, _bl.Details, ServerSession.GetCurrentUser(), transaction); 
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
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            //
        }
        #endregion
    }
}