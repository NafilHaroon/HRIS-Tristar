using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class Relations : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short RelationId { get; set; }
        public string RelationName { get; set; }
        #endregion

        #region constructor
        public Relations()
        {
        }
        #endregion

        #region internal methods
        internal static List<Relations> Get()
        {
            List<Relations> relations = new List<Relations>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetRelations"))
            {
                using (DataTable dt = db.ExecuteDataSet(dbCommand).Tables[0])
                {
                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            relations.Add(new Relations
                            {
                                RelationId = Convert.ToInt16(dr["RelationId"]),
                                RelationName = dr["RelationName"].ToString()
                            });
                        }
                    }
                }
            }
            return relations;
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