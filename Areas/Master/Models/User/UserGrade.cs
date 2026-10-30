using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class UserGrade :IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        //public short SerialNo { get; set; }
        public short? UserGradeId { get; set; }
        public short GradeId { get; set; }
        public string GradeName { get; set; }
        public bool Selected { get; set; }
        #endregion

        #region constructor
        public UserGrade()
        {

        }
//short _serialNo, 
        internal UserGrade(object _userGradeId, short _GradeId, string _GradeName, bool _selected)
        {
            //SerialNo = _serialNo;
            if (_userGradeId != DBNull.Value)
                UserGradeId = Convert.ToInt16(_userGradeId);
            GradeId = _GradeId;
            GradeName = _GradeName;
            Selected = _selected;
        }
        #endregion

        #region internal methods
        internal static List<UserGrade> Get(string _userId)
        {
            List<UserGrade> lstGrade = new List<UserGrade>();
            try
            {
                using (DbCommand dbCommandDetail = db.GetStoredProcCommand("getUserGradesbyId"))
                {
                    db.AddInParameter(dbCommandDetail, "UserId", SqlDbType.VarChar, _userId);
                    using (DataSet ds = db.ExecuteDataSet(dbCommandDetail))
                    {
                        if (ds.Tables.Count > 0)
                        {
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                lstGrade.Add(new UserGrade(
                                    dr["UserGradeId"],
                                    Convert.ToInt16(dr["GradeId"]),
                                    dr["GradeName"].ToString(),
                                    Convert.ToBoolean(dr["Selected"])));
                            }
                        }
                    }
                }
                return lstGrade;
            }
            catch (Exception)
            { throw; }
        }
        #endregion

        #region dispose method
        public void Dispose()
        {
        }
        #endregion
    }
}
