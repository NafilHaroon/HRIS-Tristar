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
    public class TaxCredit: IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public int? TaxCreditId { get; set; }
        public EmployeeShortList Employee { get; set; } = new EmployeeShortList();
        public short? TaxCreditTypeId { get; set; }
        public short? TaxYear { get; set; }
        public double Amount { get; set; }
        public string Remarks { get; set; }
        public bool IsActive { get; set; }
        public string Owner { get; set; } = ServerSession.GetCurrentUser();
        public bool Approved { get; set; }
        public bool Rejected { get; set; }
        public bool Completed { get; set; }
        public List<Attachment> Attachments { get; set; } = new List<Attachment>();
        public Footer AuditTrail { get; set; } = new Footer { CreatedBy = ServerSession.GetCurrentUser() };

        #region section to upload new attachment to the payment request
        public bool AddNew { get; set; }
        public short? AttachmentTypeId { get; set; }
        public string EmployeeName { get; set; }
        public string TaxCreditName { get; set; }
        #endregion
        #endregion

        #region constructor
        public TaxCredit()
        {
            //Employee = new EmployeeShortList();
            //AuditTrail = new Footer();
            //AuditTrail.CreatedBy = ServerSession.GetCurrentUser();
            //Owner = AuditTrail.CreatedBy;
        }
        #endregion

        #region internal methods
        internal static TaxCredit Get(int _creditId)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetTaxCreditById"))
            {
                db.AddInParameter(dbCommand, "TaxCreditId", SqlDbType.Int, _creditId);
                db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, ServerSession.GetCurrentCompany());
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds.Tables != null && ds.Tables[0].Rows.Count > 0)
                    {
                        using (TaxCredit _tc = new TaxCredit())
                        {
                            DataRow dr = ds.Tables[0].Rows[0];
                            _tc.TaxCreditId = _creditId;
                            _tc.Employee = new EmployeeShortList { EmployeeId = Convert.ToInt16(dr["EmployeeId"]), EmployeeName = dr["EmployeeName"].ToString() };
                            _tc.TaxCreditTypeId = Convert.ToInt16(dr["TaxCreditTypeId"]);
                            _tc.TaxYear = Convert.ToInt16(dr["TaxYear"]);
                            _tc.Amount = Convert.ToDouble(dr["Amount"]);
                            _tc.Remarks = dr["Remarks"].ToString();
                            _tc.IsActive = Convert.ToBoolean(dr["IsActive"]);
                            _tc.Approved = Convert.ToBoolean(dr["Approved"]);
                            _tc.Rejected = Convert.ToBoolean(dr["Rejected"]);
                            _tc.Completed = Convert.ToBoolean(dr["Completed"]);
                            _tc.EmployeeName = dr["EmployeeName"].ToString();
                            _tc.TaxCreditName = dr["TaxCreditTypeName"].ToString();
                            _tc.Owner = dr["Owner"].ToString();
                            _tc.AuditTrail = new Footer(dr);
                            _tc.Attachments = Attachment.GetWOStream(_tc.TaxCreditId.Value, AgilityEnums.WorkFlow.TaxCredit);
                            return _tc;
                        }
                    }
                    else
                        return null;
                }
            }
        }

        internal static bool Save(TaxCredit _tc)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("SaveTaxCredit"))
            {
                db.AddInParameter(dbCommand, "TaxCreditId", SqlDbType.SmallInt, _tc.TaxCreditId);
                db.AddInParameter(dbCommand, "EmployeeId", SqlDbType.SmallInt, _tc.Employee.EmployeeId);
                db.AddInParameter(dbCommand, "TaxCreditTypeId", SqlDbType.SmallInt, _tc.TaxCreditTypeId);
                db.AddInParameter(dbCommand, "TaxYear", SqlDbType.SmallInt, _tc.TaxYear);
                db.AddInParameter(dbCommand, "Amount", SqlDbType.Float, _tc.Amount);
                db.AddInParameter(dbCommand, "Remarks", SqlDbType.VarChar, _tc.Remarks);
                db.AddInParameter(dbCommand, "IsActive", SqlDbType.Bit, _tc.IsActive);
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                db.AddInParameter(dbCommand, "UpdatedOn", SqlDbType.DateTime, _tc.AuditTrail.UpdatedOn);
                db.AddOutParameter(dbCommand, "newTaxCreditId", SqlDbType.SmallInt, 32);
                using (DbConnection dbconnection = db.CreateConnection())
                {
                    dbconnection.Open();
                    DbTransaction transaction = dbconnection.BeginTransaction();
                    try
                    {
                        db.ExecuteNonQuery(dbCommand, transaction);
                        _tc.TaxCreditId = Convert.ToInt16(dbCommand.Parameters["@newTaxCreditId"].Value);
                        Attachment.Remove(_tc.TaxCreditId.Value, _tc.Attachments, transaction);
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
                using (DbCommand dbCommand= db.GetStoredProcCommand("SubmitTaxCredit"))
                {
                    db.AddInParameter(dbCommand, "TaxCreditId", SqlDbType.Int, _sub.FormId);
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