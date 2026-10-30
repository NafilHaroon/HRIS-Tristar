using HRIS.Areas.Common.Models;
using HRIS.Areas.Master.Models;
using HRIS.Helper;
using HRIS.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Transaction.Models
{
    public class TaxDebit: IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public int? TaxDebitId { get; set; }
        public EmployeeShortList Employee { get; set; } = new EmployeeShortList();
        public short? TaxDebitTypeId { get; set; }
        public short? TaxYear { get; set; }
        public double Amount { get; set; }
        public string Remarks { get; set; }
        public bool IsActive { get; set; }
        public string Owner { get; set; } = ServerSession.GetCurrentUser();
        public bool Approved { get; set; }
        public bool Rejected { get; set; }
        public bool Completed { get; set; }
        public List<Attachment> Attachments { get; set; } = new List<Attachment>();
        public Footer AuditTrail { get; set; } = new Footer { CreatedBy= ServerSession.GetCurrentUser() };

        #region section to upload new attachment to the payment request
        public bool AddNew { get; set; }
        public short? AttachmentTypeId { get; set; }
        public string EmployeeName { get; set; }
        public string TaxDebitName { get; set; }
        #endregion
        #endregion

        #region constructor
        public TaxDebit()
        {
            //Employee = new EmployeeShortList();
            //AuditTrail = new Footer();
            //AuditTrail.CreatedBy = ServerSession.GetCurrentUser();
            //Owner = AuditTrail.CreatedBy;
        }
        #endregion

        #region internal methods
        internal static TaxDebit Get(int _debitId)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetTaxDebitById"))
            {
                db.AddInParameter(dbCommand, "TaxDebitId", SqlDbType.Int, _debitId);
                db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, ServerSession.GetCurrentCompany());
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds.Tables != null && ds.Tables[0].Rows.Count > 0)
                    {
                        using (TaxDebit _td = new TaxDebit())
                        {
                            DataRow dr = ds.Tables[0].Rows[0];
                            _td.TaxDebitId = _debitId;
                            _td.Employee = new EmployeeShortList { EmployeeId = Convert.ToInt16(dr["EmployeeId"]), EmployeeName = dr["EmployeeName"].ToString() };
                            _td.TaxDebitTypeId = Convert.ToInt16(dr["TaxDebitTypeId"]);
                            _td.TaxYear = Convert.ToInt16(dr["TaxYear"]);
                            _td.Amount = Convert.ToDouble(dr["Amount"]);
                            _td.Remarks = dr["Remarks"].ToString();
                            _td.IsActive = Convert.ToBoolean(dr["IsActive"]);
                            _td.Approved = Convert.ToBoolean(dr["Approved"]);
                            _td.Rejected = Convert.ToBoolean(dr["Rejected"]);
                            _td.Completed = Convert.ToBoolean(dr["Completed"]);
                            _td.EmployeeName = dr["EmployeeName"].ToString();
                            _td.TaxDebitName = dr["TaxDebitTypeName"].ToString();
                            _td.Owner = dr["Owner"].ToString();
                            _td.AuditTrail = new Footer(dr);
                            _td.Attachments = Attachment.GetWOStream(_td.TaxDebitId.Value, AgilityEnums.WorkFlow.TaxDebit);
                            return _td;
                        }
                    }
                    else
                        return null;
                }
            }
        }

        internal static bool Save(TaxDebit _td)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("SaveTaxDebit"))
            {
                db.AddInParameter(dbCommand, "TaxDebitId", SqlDbType.SmallInt, _td.TaxDebitId);
                db.AddInParameter(dbCommand, "EmployeeId", SqlDbType.SmallInt, _td.Employee.EmployeeId);
                db.AddInParameter(dbCommand, "TaxDebitTypeId", SqlDbType.SmallInt, _td.TaxDebitTypeId);
                db.AddInParameter(dbCommand, "TaxYear", SqlDbType.SmallInt, _td.TaxYear);
                db.AddInParameter(dbCommand, "Amount", SqlDbType.Float, _td.Amount);
                db.AddInParameter(dbCommand, "Remarks", SqlDbType.VarChar, _td.Remarks);
                db.AddInParameter(dbCommand, "IsActive", SqlDbType.Bit, _td.IsActive);
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                db.AddInParameter(dbCommand, "UpdatedOn", SqlDbType.DateTime, _td.AuditTrail.UpdatedOn);
                db.AddOutParameter(dbCommand, "newTaxDebitId", SqlDbType.SmallInt, 32);
                using (DbConnection dbconnection = db.CreateConnection())
                {
                    dbconnection.Open();
                    DbTransaction transaction = dbconnection.BeginTransaction();
                    try
                    {
                        db.ExecuteNonQuery(dbCommand, transaction);
                        _td.TaxDebitId = Convert.ToInt16(dbCommand.Parameters["@newTaxDebitId"].Value);
                        Attachment.Remove(_td.TaxDebitId.Value, _td.Attachments, transaction);
                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw ex;
                    }
                }
            }
        }

        internal static bool SubmitRequest(Submission _sub)
        {
            try
            {
                using (DbCommand dbCommand = db.GetStoredProcCommand("SubmitTaxDebit"))
                {
                    db.AddInParameter(dbCommand, "TaxDebitId", SqlDbType.Int, _sub.FormId);
                    db.AddInParameter(dbCommand, "SubmissionComments", SqlDbType.VarChar, _sub.Comments);
                    db.AddInParameter(dbCommand, "StateId", SqlDbType.Int, _sub.StateId);
                    db.AddInParameter(dbCommand, "UpdatedBy", SqlDbType.VarChar, _sub.UserId);
                    db.AddInParameter(dbCommand, "Owner", SqlDbType.VarChar, _sub.Owner);
                    db.AddInParameter(dbCommand, "Completed", SqlDbType.Bit, _sub.Completed);
                    db.AddInParameter(dbCommand, "Rejected", SqlDbType.Bit, _sub.Rejected);
                    db.AddInParameter(dbCommand, "Approved", SqlDbType.Bit, _sub.Approved);
                    db.ExecuteNonQuery(dbCommand);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return true;
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