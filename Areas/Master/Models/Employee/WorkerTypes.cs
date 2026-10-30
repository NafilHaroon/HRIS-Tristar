using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class WorkerTypes : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short WorkerTypeId { get; set; }
        public string WorkerTypeName { get; set; }
        #endregion

        #region constructor
        public WorkerTypes()
        {
        }
        #endregion

        #region internal methods
        internal static List<WorkerTypes> Get()
        {
            List<WorkerTypes> types = new List<WorkerTypes>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("getWorkerTypes"))
            {
                using (DataTable dt = db.ExecuteDataSet(dbCommand).Tables[0])
                {
                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            types.Add(new WorkerTypes
                            {
                                WorkerTypeId = Convert.ToInt16(dr["WorkerTypeId"]),
                                WorkerTypeName = dr["WorkerTypeName"].ToString()
                            });
                        }
                    }
                }
            }
            return types;
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