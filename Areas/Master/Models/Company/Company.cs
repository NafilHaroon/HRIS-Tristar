using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class Company : IDisposable
    {
        #region private properties
        private static readonly SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short? CompanyId { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string NTN { get; set; }
        public string PFTitle { get; set; }
        public short? PFReceivableId { get; set; }
        public double  EOBIContribution { get; set; }
        public string EOBIRegNo { get; set; }
        public string HFMCode { get; set; }
        public short? PFBankAccountId { get; set; }
        public bool FinancialExtract { get; set; }
        public string CompanyLogo { get; set; }
        //public string image { get; set; }
        public bool IsActive { get; set; }
        public Footer AuditTrail { get; set; }
        #endregion

        #region constructor
        public Company()
        {
            IsActive = true;
            AuditTrail = new Footer();
        }
        #endregion

        #region internal methods
        internal static Company Get(short _companyId)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetCompanyById"))
            {
                db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, _companyId);
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds.Tables != null && ds.Tables[0].Rows.Count > 0)
                    {
                        using (Company _co = new Company())
                        {
                            DataRow dr = ds.Tables[0].Rows[0];
                            _co.CompanyId = _companyId;
                            _co.CompanyCode = dr["CompanyCode"].ToString();
                            _co.CompanyName = dr["CompanyName"].ToString();
                            //_co.CompanyNameOnPayslip = dr["CompanyNameOnPayslip"].ToString();
                            _co.CompanyAddress = dr["CompanyAddress"].ToString();
                            _co.NTN = dr["NTN"].ToString();
                            _co.PFTitle = dr["PFTitle"].ToString();
                            if (dr["PFReceivableId"] != DBNull.Value)
                                _co.PFReceivableId = Convert.ToInt16(dr["PFReceivableId"]);
                            if (dr["PFBankAccountId"] != DBNull.Value)
                                _co.PFBankAccountId = Convert.ToInt16(dr["PFBankAccountId"]);
                            _co.EOBIRegNo = dr["EOBIRegNo"].ToString();
                            _co.EOBIContribution = Convert.ToDouble(dr["EOBIContribution"]);
                            _co.HFMCode = dr["HFMCode"].ToString();
                            _co.FinancialExtract = Convert.ToBoolean(dr["FinancialExtract"]);
                            //_co.CompanyLogoOnPaySlip =  Convert.ToBoolean(dr["CompanyLogoOnPaySlip"]);
                            if (dr["CompanyLogo"] != DBNull.Value)
                                _co.CompanyLogo = dr["CompanyLogo"].ToString();
                            
                            _co.IsActive = Convert.ToBoolean(dr["IsActive"]);
                            _co.AuditTrail.CreatedBy = dr["CreatedBy"].ToString();
                            _co.AuditTrail.CreatedOn = Convert.ToDateTime(dr["CreatedOn"]);
                            _co.AuditTrail.UpdatedBy = dr["UpdatedBy"].ToString();
                            if (dr["UpdatedOn"] != DBNull.Value)
                                _co.AuditTrail.UpdatedOn = Convert.ToDateTime(dr["UpdatedOn"]);
                            return _co;
                        }
                    }
                    else
                        return null;
                }
            }
        }

        internal static bool Save(Company _co)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("SaveCompany"))
            {
                try
                {
                    db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, _co.CompanyId);
                    db.AddInParameter(dbCommand, "CompanyCode", SqlDbType.VarChar, _co.CompanyCode);
                    db.AddInParameter(dbCommand, "CompanyName", SqlDbType.VarChar, _co.CompanyName);
                    db.AddInParameter(dbCommand, "CompanyAddress", SqlDbType.VarChar, _co.CompanyAddress);
                    db.AddInParameter(dbCommand, "NTN", SqlDbType.VarChar, _co.NTN);
                    db.AddInParameter(dbCommand, "PFTitle", SqlDbType.VarChar, _co.PFTitle);
                    db.AddInParameter(dbCommand, "EOBIContribution", SqlDbType.Float, _co.EOBIContribution);
                    db.AddInParameter(dbCommand, "EOBIRegNo", SqlDbType.VarChar, _co.EOBIRegNo);
                    db.AddInParameter(dbCommand, "PFReceivableId", SqlDbType.SmallInt, _co.PFReceivableId);
                    db.AddInParameter(dbCommand, "PFBankAccountId", SqlDbType.SmallInt, _co.PFBankAccountId);
                    db.AddInParameter(dbCommand, "HFMCode", SqlDbType.VarChar, _co.HFMCode);
                    db.AddInParameter(dbCommand, "FinancialExtract", SqlDbType.Bit, _co.FinancialExtract);
                    db.AddInParameter(dbCommand, "CompanyLogo", SqlDbType.VarChar, _co.CompanyLogo);
                    db.AddInParameter(dbCommand, "IsActive", SqlDbType.Bit, _co.IsActive);
                    db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                    db.AddInParameter(dbCommand, "UpdatedOn", SqlDbType.DateTime, _co.AuditTrail.UpdatedOn);
                    db.ExecuteNonQuery(dbCommand);
                    return true;
                }
                catch (Exception)
                { throw; }
            }
        }

        internal static CompanyLogo GetUserDefaultCompany()
        {
            try
            {
                using (DbCommand dbCommand = db.GetStoredProcCommand("GetUserDefaultCompany"))
                {
                    db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                    using (DataSet ds = db.ExecuteDataSet(dbCommand))
                    {
                        if (ds.Tables != null && ds.Tables[0].Rows.Count > 0)
                        {
                            return new CompanyLogo(Convert.ToInt16(ds.Tables[0].Rows[0]["CompanyId"]),
                                ds.Tables[0].Rows[0]["CompanyLogo"].ToString());
                        }
                        else
                            return null;
                    }
                }
            }
            catch (Exception) { throw; }
        }

        internal static short? GetUserDefaultCompanyWithoutLogo()
        {
            try
            {
                using (DbCommand dbCommand = db.GetStoredProcCommand("GetUserDefaultCompanyWithoutLogo"))
                {
                    db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                    using (DataSet ds = db.ExecuteDataSet(dbCommand))
                    {
                        if (ds.Tables != null && ds.Tables[0].Rows.Count > 0)
                        {
                            return Convert.ToInt16(ds.Tables[0].Rows[0]["CompanyId"]);
                        }
                        else
                            return null;
                    }
                }
            }
            catch (Exception) { throw; }
        }

        internal static bool SetUserDefaultCompany(short _companyId)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("SetUserDefaultCompany"))
            {
                try
                {
                    db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, _companyId);
                    db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
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

    public class CompanyLogo
    {
        public short CompanyId { get; set; }
        public string Logo { get; set; }

        public CompanyLogo(short _companyId, string _logo)
        {
            CompanyId = _companyId;
            Logo = _logo;
        }
    }
}
