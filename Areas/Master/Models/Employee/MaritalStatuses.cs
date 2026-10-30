using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class MaritalStatuses : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short MaritalStatusId { get; set; }
        public string MaritalStatusName { get; set; }
  
        #endregion

        #region constructor
        public MaritalStatuses()
        {
        }
        #endregion

        #region internal methods
        internal static List<MaritalStatuses> Get()
        {
            List<MaritalStatuses> statuses = new List<MaritalStatuses>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("getMaritalStatuses"))
            {
                using (DataTable dt = db.ExecuteDataSet(dbCommand).Tables[0])
                {
                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            statuses.Add(new MaritalStatuses
                            {
                                MaritalStatusId = Convert.ToInt16(dr["MaritalStatusId"]),
                                MaritalStatusName = dr["MaritalStatusName"].ToString()
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