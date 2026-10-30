using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class EmpDocumentStream
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public string DocName { get; set; }
        public string DocContentType { get; set; }
        public byte[] DocContent { get; set; }
        #endregion

        #region constructor
        public EmpDocumentStream()
        {
        }

        public EmpDocumentStream(string _docName, string _docContentType, byte[] _docContent)
        {
            DocName = _docName;
            DocContentType = _docContentType;
            DocContent = _docContent;
        }
        #endregion

        #region internal methods
        internal static EmpDocumentStream Get(int _documentId)
        {
            try
            {
                using (DbCommand dbCommand = db.GetStoredProcCommand("GetEmployeeDocumentStreamById"))
                {
                    db.AddInParameter(dbCommand, "DocumentId", SqlDbType.Int, _documentId);
                    db.AddInParameter(dbCommand, "CompanyId", SqlDbType.SmallInt, ServerSession.GetCurrentCompany());
                    db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                    using (DataSet ds = db.ExecuteDataSet(dbCommand))
                    {
                        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                        {
                            DataRow dr = ds.Tables[0].Rows[0];
                            return new EmpDocumentStream(
                                dr["DocumentName"].ToString(),
                                dr["ContentType"].ToString(),
                                (byte[])(dr["DocumentContent"]));
                        }
                    }
                }
                return null;
            }
            catch (Exception)
            { throw; }
        }
        #endregion
    }
}