using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.Serialization;

namespace HRIS.Areas.Master.Models
{
    public class AttachmentTypes : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        [DataMember(Name = "Attachment Type Id", Order = 0)]
        public short TypeId { get; set; }
        [DataMember(Name = "Attachment Type Desc", Order = 1)]
        public string TypeName { get; set; }
        [DataMember(Name = "Active?", Order = 4)]
        public bool IsActive { get; set; }
        #endregion

        #region constructor
        public AttachmentTypes() { }
        #endregion

        #region internal methods
        internal static List<AttachmentTypes> Get(bool _isActive = true)
        {
            List<AttachmentTypes> lstAT = new List<AttachmentTypes>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetAttachmentTypes"))
            {
                db.AddInParameter(dbCommand, "ActiveOnly", SqlDbType.Bit, _isActive);
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            lstAT.Add(new AttachmentTypes
                            {
                                TypeId = Convert.ToInt16(dr["AttachmentTypeId"]),
                                TypeName = dr["AttachmentTypeName"].ToString(),
                                IsActive = Convert.ToBoolean(dr["IsActive"])
                            });
                        }
                    }
                }
            }
            return lstAT;
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