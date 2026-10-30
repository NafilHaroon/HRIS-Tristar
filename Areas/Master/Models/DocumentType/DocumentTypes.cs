using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Runtime.Serialization;

namespace HRIS.Areas.Master.Models
{
    public class DocumentTypes : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        [DataMember(Name = "Type Id", Order = 0)]
        public short DocumentTypeId { get; set; }
        [DataMember(Name = "Document Type Desc", Order = 1)]
        public string DocumentTypeName { get; set; }
        [DataMember(Name = "Mandatory", Order = 2)]
        public bool Mandatory { get; set; }
        [DataMember(Name = "Expires?", Order = 3)]
        public bool Expires { get; set; }
        [DataMember(Name = "Active?", Order = 4)]
        public bool IsActive { get; set; }
        #endregion

        #region constructor
        public DocumentTypes() { }
        #endregion

        #region internal methods
        internal static List<DocumentTypes> Get(bool _isActive = true)
        {
            List<DocumentTypes> documenttypes = new List<DocumentTypes>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetDocumentTypes"))
            {
                db.AddInParameter(dbCommand, "ActiveOnly", SqlDbType.Bit, _isActive);
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            documenttypes.Add(new DocumentTypes
                            {
                                DocumentTypeId = Convert.ToInt16(dr["DocumentTypeId"]),
                                DocumentTypeName = dr["DocumentTypeName"].ToString(),
                                Mandatory = Convert.ToBoolean(dr["Mandatory"]),
                                Expires = Convert.ToBoolean(dr["Expires"]),
                                IsActive = Convert.ToBoolean(dr["IsActive"])
                            });
                        }
                    }
                }
            }
            return documenttypes;
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
