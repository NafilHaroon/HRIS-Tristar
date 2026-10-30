using HRIS.Areas.Common.Models;
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
    public class Period : IDisposable
    {
        #region private properties
        private static readonly SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short PeriodId { get; set; }
        public string PeriodName { get; set; }
        #endregion

        #region constructor
        public Period()
        {
        }

        public Period(short _periodId, string _periodName)
        {
            PeriodId = _periodId;
            PeriodName = _periodName;

        }
        #endregion

        #region internal methods
        internal static List<Period> Get()
        {
            return get("GetPeriods");
        }

        internal static List<Period> GetClosedPeriods()
        {
            return get("GetPeriods_Closed", ServerSession.GetCurrentCompany());
        }

        internal static Period GetCurrentPeriod()
        {
            try
            {
                using (DbCommand dbCommand = db.GetStoredProcCommand("GetCurrentPeriod"))
                {
                    db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, ServerSession.GetCurrentCompany());
                    using (DataSet ds = db.ExecuteDataSet(dbCommand))
                    {
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                return new Period(Convert.ToInt16(ds.Tables[0].Rows[0]["PeriodId"]),
                                    ds.Tables[0].Rows[0]["PeriodName"].ToString());
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception)
            { throw; }
        }
        #endregion

        #region private methods
        private static List<Period> get(string _spName, short? _companyId = null)
        {
            try
            {
                List<Period> lstPeriods = new List<Period>();
                using (DbCommand dbCommand = db.GetStoredProcCommand(_spName))
                {
                    if (_companyId.HasValue)
                        db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, _companyId);
                    using (DataSet ds = db.ExecuteDataSet(dbCommand))
                    {
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                lstPeriods.Add(new Period(
                                    Convert.ToInt16(dr["PeriodId"]),
                                    dr["PeriodName"].ToString()));
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