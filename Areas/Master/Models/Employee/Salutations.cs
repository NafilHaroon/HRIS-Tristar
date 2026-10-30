using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class Salutations
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short SalutationId { get; set; }
        public string SalutationName { get; set; }

        #endregion

        #region constructor
        public Salutations()
        {
        }
        #endregion

        #region internal methods
        internal static List<Salutations> Get()
        {
            List<Salutations> salutations = new List<Salutations>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("getSalutations"))
            {
                using (DataTable dt = db.ExecuteDataSet(dbCommand).Tables[0])
                {
                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            salutations.Add(new Salutations
                            {
                                SalutationId = Convert.ToInt16(dr["SalutationId"]),
                                SalutationName = dr["SalutationName"].ToString()
                            });
                        }
                    }
                }
            }
            return salutations;
        }
        #endregion
    }
}
