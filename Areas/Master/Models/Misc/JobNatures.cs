using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class JobNatures : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short JobNatureId { get; set; }
        public string JobNatureName { get; set; }
        #endregion

        #region constructor
        public JobNatures()
        {
        }

        //public JobNature(short _JobNatureId, string _JobNatureName)
        //{
        //    JobNatureId = _JobNatureId;
        //    JobNatureName = _JobNatureName;

        //}
        #endregion

        #region internal methods
        /// <summary>
        /// returns list of Job Natures
        /// </summary>
        /// <returns></returns>
        internal static List<JobNatures> Get()
        {
            List<JobNatures> natures = new List<JobNatures>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("getJobNatures"))
            {
                using (DataTable dt = db.ExecuteDataSet(dbCommand).Tables[0])
                {
                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            natures.Add(new JobNatures
                            {
                                JobNatureId = Convert.ToInt16(dr["JobNatureId"]),
                                JobNatureName = dr["JobNatureName"].ToString()
                            });
                        }
                    }
                }
            }
            return natures;
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