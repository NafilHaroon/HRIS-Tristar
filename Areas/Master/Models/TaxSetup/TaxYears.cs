using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.Serialization;

namespace HRIS.Areas.Master.Models
{
    [DataContract]
    public class TaxYears : IDisposable
    {
        #region private properties
        private static readonly SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short TaxYear { get; set; }
        #endregion

        #region constructor
        public TaxYears()
        {
        }

        public TaxYears(short _periodId)
        {
            TaxYear = _periodId;
        }
        #endregion

        #region internal methods
        internal static List<TaxYears> Get()
        {
            try
            {
                List<TaxYears> lstPeriods = new List<TaxYears>();
                using (DbCommand dbCommand = db.GetStoredProcCommand("GetTaxYears"))
                {
                    using (DataSet ds = db.ExecuteDataSet(dbCommand))
                    {
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                lstPeriods.Add(new TaxYears(
                                    Convert.ToInt16(dr["TaxYear"])));
                            }
                        }
                    }
                }
                return lstPeriods;
            }
            catch (Exception)
            { throw; }
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