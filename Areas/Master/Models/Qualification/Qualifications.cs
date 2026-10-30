using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class Qualifications : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short QualificationId { get; set; }
        public string QualificationName { get; set; }
        #endregion

        #region constructor
        public Qualifications()
        {
        }

        //public QualificationList(short _qualificationId, string _qualificationName)
        //{
        //    QualificationId = _qualificationId;
        //    QualificationName = _qualificationName;

        //}
        #endregion

        #region internal methods
        internal static List<Qualifications> Get()
        {
            List<Qualifications> qualifications = new List<Qualifications>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetQualifications"))
            {

                using (DataTable dt = db.ExecuteDataSet(dbCommand).Tables[0])
                {
                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            qualifications.Add(new Qualifications
                            {
                                QualificationId = Convert.ToInt16(dr["QualificationId"]),
                                QualificationName = dr["QualificationName"].ToString()
                            });
                        }
                    }
                }
            }
            return qualifications;
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