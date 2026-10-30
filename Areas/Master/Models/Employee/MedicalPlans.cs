using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class MedicalPlans : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short MedicalPlanId { get; set; }
        public string MedicalPlanName { get; set; }

        #endregion

        #region constructor
        public MedicalPlans()
        {
        }
        #endregion

        #region internal methods
        internal static List<MedicalPlans> Get()
        {
            List<MedicalPlans> plans = new List<MedicalPlans>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("getMedicalPlans"))
            {
                using (DataTable dt = db.ExecuteDataSet(dbCommand).Tables[0])
                {
                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            plans.Add(new MedicalPlans
                            {
                                MedicalPlanId = Convert.ToInt16(dr["MedicalPlanId"]),
                                MedicalPlanName = dr["MedicalPlanName"].ToString()
                            });
                        }
                    }
                }
            }
            return plans;
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