using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class SalaryHeadTypes : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short HeadTypeId { get; set; }
        public string HeadTypeName { get; set; }
        #endregion

        #region constructor
        public SalaryHeadTypes()
        {
        }

        //public SalaryHeadTypes(short _HeadTypeId, string _HeadTypeName)
        //{
        //    HeadTypeId = _HeadTypeId;
        //    HeadTypeName = _HeadTypeName;

        //}
        #endregion

        #region internal methods
        internal static List<SalaryHeadTypes> Get()
        {
            List<SalaryHeadTypes> types = new List<SalaryHeadTypes>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetSalaryHeadTypes"))
            {
                using (DataTable dt = db.ExecuteDataSet(dbCommand).Tables[0])
                {
                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            types.Add(new SalaryHeadTypes
                            {
                                HeadTypeId = Convert.ToInt16(dr["HeadTypeId"]),
                                HeadTypeName = dr["HeadTypeName"].ToString()
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