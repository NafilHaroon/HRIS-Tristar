using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class EmpStatuses : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short StatusId { get; set; }
        public string StatusName { get; set; }
        #endregion

        #region constructor
        public EmpStatuses()
        {
        }
        #endregion

        #region internal methods
        internal static List<EmpStatuses> Get()
        {
            List<EmpStatuses> statuses = new List<EmpStatuses>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("getEmployeeStatuses"))
            {
                using (DataTable dt = db.ExecuteDataSet(dbCommand).Tables[0])
                {
                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            statuses.Add(new EmpStatuses
                            {
                                StatusId = Convert.ToInt16(dr["StatusId"]),
                                StatusName = dr["StatusName"].ToString()
                            });
                        }
                    }
                }
            }
            return statuses;
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