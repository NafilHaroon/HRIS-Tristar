using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class LetterTemplate: IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short? TemplateId { get; set; }
        public string TemplateName { get; set; }
        public string LetterPrefix { get; set; }
        public short? DocumentTypeId { get; set; }
        public string TemplateContent { get; set; }
        public bool IsActive { get; set; } = true;
        public bool ImpactsPayroll { get; set; } = false;
        public bool IncludeSeparatedEmployees { get; set; } = false;
        public Footer AuditTrail { get; set; } = new Footer();
        #endregion

        #region constructor
        public LetterTemplate()
        {
            //IsActive = true;
            //ImpactsPayroll = false;
            //IncludeSeparatedEmployees = false;
            //AuditTrail = new Footer();
        }
        #endregion

        #region internal methods
        internal static LetterTemplate Get(short _templateId)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetLetterTemplateById"))
            {
                db.AddInParameter(dbCommand, "TemplateId", SqlDbType.SmallInt, _templateId);
                db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, ServerSession.GetCurrentCompany());
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds.Tables != null && ds.Tables[0].Rows.Count > 0)
                    {
                        using (LetterTemplate _lt = new LetterTemplate())
                        {
                            DataRow dr = ds.Tables[0].Rows[0];
                            _lt.TemplateId = _templateId;
                            _lt.TemplateName = dr["TemplateName"].ToString();
                            _lt.LetterPrefix = dr["LetterPrefix"].ToString();
                            _lt.DocumentTypeId = Convert.ToInt16(dr["DocumentTypeId"]);
                            _lt.TemplateContent = dr["TemplateContent"].ToString();
                            _lt.IsActive = Convert.ToBoolean(dr["IsActive"]);
                            _lt.ImpactsPayroll = Convert.ToBoolean(dr["ImpactsPayroll"]);
                            _lt.IncludeSeparatedEmployees = Convert.ToBoolean(dr["IncludeSeparatedEmployees"]);
                            _lt.AuditTrail = new Footer(dr);
                            return _lt;
                        }
                    }
                    else
                        return null;
                }
            }
        }

        internal static bool Save(LetterTemplate _lt)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("SaveLetterTemplate"))
            {
                try
                {
                    db.AddInParameter(dbCommand, "TemplateId", SqlDbType.TinyInt, _lt.TemplateId);
                    db.AddInParameter(dbCommand, "TemplateName", SqlDbType.VarChar, _lt.TemplateName);
                    db.AddInParameter(dbCommand, "LetterPrefix", SqlDbType.VarChar, _lt.LetterPrefix);
                    db.AddInParameter(dbCommand, "DocumentTypeId", SqlDbType.TinyInt, _lt.DocumentTypeId);
                    db.AddInParameter(dbCommand, "TemplateContent", SqlDbType.VarChar, _lt.TemplateContent);
                    db.AddInParameter(dbCommand, "ImpactsPayroll", SqlDbType.Bit, _lt.ImpactsPayroll);
                    db.AddInParameter(dbCommand, "IncludeSeparatedEmployees", SqlDbType.Bit, _lt.IncludeSeparatedEmployees);
                    db.AddInParameter(dbCommand, "IsActive", SqlDbType.Bit, _lt.IsActive);
                    db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, ServerSession.GetCurrentCompany());
                    db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                    db.AddInParameter(dbCommand, "UpdatedOn", SqlDbType.DateTime, _lt.AuditTrail.UpdatedOn);
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