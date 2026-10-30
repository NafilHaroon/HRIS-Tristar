using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class Bank : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short? BankId { get; set; }
        public short? CompanyId { get; set; }
        public string BankName { get; set; }
        public string Address { get; set; }
        public string AccountNo { get; set; }
        public string ContactPerson { get; set; }
        public string HeaderText { get; set; }
        public bool EmployeeBankDetail { get; set; }
        public bool IsActive { get; set; } = true;
        public Footer AuditTrail { get; set; } = new Footer();
        #endregion

        #region constructor
        public Bank()
        {
            //AuditTrail = new Footer();
            //IsActive = true;
        }
        #endregion

        #region internal methods
        internal static Bank Get(short _bankId)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetBankById"))
            {
                db.AddInParameter(dbCommand, "BankId", SqlDbType.SmallInt, _bankId);
                db.AddInParameter(dbCommand, "CompanyId", SqlDbType.SmallInt, ServerSession.GetCurrentCompany());
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds.Tables != null && ds.Tables[0].Rows.Count > 0)
                    {
                        using (Bank _bnk = new Bank())
                        {
                            DataRow dr = ds.Tables[0].Rows[0];
                            _bnk.BankId = _bankId;
                            _bnk.CompanyId = Convert.ToInt16(dr["CompanyId"]);
                            _bnk.BankName = dr["BankName"].ToString();
                            _bnk.Address = dr["Address"].ToString();
                            _bnk.AccountNo = dr["AccountNo"].ToString();
                            _bnk.ContactPerson = dr["ContactPerson"].ToString();
                            _bnk.HeaderText = dr["HeaderText"].ToString();
                            _bnk.EmployeeBankDetail = Convert.ToBoolean(dr["EmployeeBankDetail"]);
                            _bnk.IsActive = Convert.ToBoolean(dr["IsActive"]);
                            _bnk.AuditTrail = new Footer(dr);
                            return _bnk;
                        }
                    }
                    else
                        return null;
                }
            }
        }

        internal static bool Save(Bank _bnk)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("SaveBank"))
            {
                try
                {
                    db.AddInParameter(dbCommand, "BankId", SqlDbType.SmallInt, _bnk.BankId);
                    db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, ServerSession.GetCurrentCompany());
                    db.AddInParameter(dbCommand, "BankName", SqlDbType.VarChar, _bnk.BankName);
                    db.AddInParameter(dbCommand, "Address", SqlDbType.VarChar, _bnk.Address);
                    db.AddInParameter(dbCommand, "AccountNo", SqlDbType.VarChar, _bnk.AccountNo);
                    db.AddInParameter(dbCommand, "ContactPerson", SqlDbType.VarChar, _bnk.ContactPerson);
                    db.AddInParameter(dbCommand, "HeaderText", SqlDbType.VarChar, _bnk.HeaderText);
                    db.AddInParameter(dbCommand, "EmployeeBankDetail", SqlDbType.Bit, _bnk.EmployeeBankDetail);
                    db.AddInParameter(dbCommand, "IsActive", SqlDbType.Bit, _bnk.IsActive);
                    db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                    db.AddInParameter(dbCommand, "UpdatedOn", SqlDbType.DateTime, _bnk.AuditTrail.UpdatedOn);
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