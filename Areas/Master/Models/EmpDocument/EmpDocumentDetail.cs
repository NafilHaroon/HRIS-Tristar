using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace HRIS.Areas.Master.Models
{
    public class EmpDocumentDetail
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public int? DocumentId { get; set; }
        public short? DocTypeId { get; set; }
        public string DocName { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public short? ReminderDays { get; set; }
        public bool Edit { get; set; }
        public bool Delete { get; set; }
        #endregion

        #region constructor
        public EmpDocumentDetail()
        {
            DocumentId = 0;
            //Add = false;
            Delete = false;
            Edit = false;
        }

        public EmpDocumentDetail(int _documentId, short _docTypeId, string _docName,
            //string _docContentType, //byte[] _docContent, 
            object _expiryDate, object _reminderDays)
        {
            //Add = false; 
            Edit = false; Delete = false;
            DocumentId = _documentId;
            DocTypeId = _docTypeId;
            DocName = _docName;
            //DocContentType = _docContentType;
            //DocContent = _docContent;
            if (_expiryDate != DBNull.Value)
                ExpiryDate = Convert.ToDateTime(_expiryDate);
            if (_reminderDays != DBNull.Value)
                ReminderDays = Convert.ToInt16(_reminderDays);
        }
        #endregion

        #region internal methods
        internal static List<EmpDocumentDetail> Get(string _employeeCode)
        {
            try
            {
                List<EmpDocumentDetail> lstED = new List<EmpDocumentDetail>();
                using (DbCommand dbCommand = db.GetStoredProcCommand("GetEmployeeDocumentsByCode"))
                {
                    db.AddInParameter(dbCommand, "EmployeeCode", SqlDbType.VarChar, _employeeCode);
                    db.AddInParameter(dbCommand, "CompanyId", SqlDbType.SmallInt, ServerSession.GetCurrentCompany());
                    db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                    using (DataSet ds = db.ExecuteDataSet(dbCommand))
                    {
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                lstED.Add(new EmpDocumentDetail(
                                    Convert.ToInt32(dr["DocumentId"]),
                                    Convert.ToInt16(dr["DocumentTypeId"]),
                                    dr["DocumentName"].ToString(),
                                    //dr["ContentType"].ToString(),
                                    //(byte[])(dr["DocumentContent"]),
                                    dr["ExpiryDate"],
                                    dr["ReminderDays"]));
                            }
                        }
                    }
                }
                return lstED;
            }
            catch (Exception)
            { throw; }
        }

        //save changes to existing record excluding document content
        internal static bool Save(string _employeeCode, List<EmpDocumentDetail> _details, string _userId, DbTransaction _transaction)
        {
            try
            {
                foreach (EmpDocumentDetail _edd in getChanges(_details))
                {
                    using (DbCommand dbCommand = db.GetStoredProcCommand("SaveEmployeeDocuments"))
                    {
                        db.AddInParameter(dbCommand, "DocumentId", SqlDbType.VarChar, _edd.DocumentId);
                        db.AddInParameter(dbCommand, "EmployeeCode", SqlDbType.VarChar, _employeeCode);
                        db.AddInParameter(dbCommand, "DocumentTypeId", SqlDbType.SmallInt, _edd.DocTypeId);
                        db.AddInParameter(dbCommand, "ExpiryDate", SqlDbType.DateTime, _edd.ExpiryDate);
                        db.AddInParameter(dbCommand, "ReminderDays", SqlDbType.SmallInt, _edd.ReminderDays);
                        db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, _userId);
                        db.AddInParameter(dbCommand, "Action", SqlDbType.Char, (_edd.Delete ? "D" : "U"));
                        db.ExecuteNonQuery(dbCommand, _transaction);
                    }
                }
                return true;
            }
            catch (Exception)
            { throw; }
        }

        //upload new document - single document at a time
        internal static bool Save(string _employeeCode, short _docTypeId, string _docName, string _contentType, object _expiryDate, 
            object _reminderDays, byte[] _content)
        {
            try
            {
                using (DbCommand dbCommand = db.GetStoredProcCommand("SaveEmployeeDocument"))
                {
                    db.AddInParameter(dbCommand, "EmployeeCode", SqlDbType.VarChar, _employeeCode);
                    db.AddInParameter(dbCommand, "DocumentTypeId", SqlDbType.SmallInt, _docTypeId);
                    db.AddInParameter(dbCommand, "DocumentName", SqlDbType.VarChar, _docName);
                    db.AddInParameter(dbCommand, "ContentType", SqlDbType.VarChar, _contentType);
                    db.AddInParameter(dbCommand, "ExpiryDate", SqlDbType.DateTime, _expiryDate);
                    db.AddInParameter(dbCommand, "ReminderDays", SqlDbType.SmallInt, _reminderDays);
                    db.AddInParameter(dbCommand, "DocumentContent", SqlDbType.VarBinary, _content);
                    db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                    db.ExecuteNonQuery(dbCommand);
                }
                return true;
            }
            catch (Exception)
            { throw; }
        }
        #endregion

        #region private methods
        private static IEnumerable<EmpDocumentDetail> getChanges(List<EmpDocumentDetail> _details)
        {
            return (_details.Where(x=> x.Edit || x.Delete));//x => x.Add || 
        }
        #endregion
    }
}