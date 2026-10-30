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
    public class Grades : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        [DataMember(Order = 0, Name = "Grade Id")]
        public short GradeId { get; set; }
        [DataMember(Order = 1, Name = "Grade Name")]
        public string GradeName { get; set; }
        [DataMember(Order = 2)]
        public bool IsActive { get; set; }
        #endregion

        #region constructor
        public Grades()
        {
        }
        #endregion

        #region internal methods
        internal static List<Grades> Get(bool _isActive)
        {
            List<Grades> grades = new List<Grades>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetGrades"))
            {
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                db.AddInParameter(dbCommand, "ActiveOnly", SqlDbType.Bit, _isActive);
                using (DataTable dt = db.ExecuteDataSet(dbCommand).Tables[0])
                {
                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            grades.Add(new Grades
                            {
                                GradeId = Convert.ToInt16(dr["GradeId"]),
                                GradeName = dr["GradeName"].ToString(),
                                IsActive = Convert.ToBoolean(dr["IsActive"])
                            });
                        }
                    }
                }
            }
            return grades;
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