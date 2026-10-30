using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class UserBranch : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short? UserBranchId { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        public bool Selected { get; set; }
        #endregion

        #region constructors
        public UserBranch()
        {
            Selected = false;
        }

        internal UserBranch(object _userBranchId, string _branchCode, string _branchName, bool _selected)
        {
            if (_userBranchId != DBNull.Value)
                UserBranchId = Convert.ToInt16(_userBranchId);
            BranchCode = _branchCode;
            BranchName = _branchName;
            Selected = _selected;
        }
        #endregion

        #region internal methods
        internal static List<UserBranch> Get(string _userId)
        {
            List<UserBranch> lstBranch = new List<UserBranch>();
            try
            {
                using (DbCommand dbCommandDetail = db.GetStoredProcCommand("GetUserBranchesById"))
                {
                    db.AddInParameter(dbCommandDetail, "UserId", SqlDbType.VarChar, _userId);
                    using (DataSet ds = db.ExecuteDataSet(dbCommandDetail))
                    {
                        if (ds.Tables.Count > 0)
                        {
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                lstBranch.Add(new UserBranch(
                                    dr["UserBranchId"],
                                    dr["BranchCode"].ToString(),
                                    dr["BranchName"].ToString(),
                                    Convert.ToBoolean(dr["Selected"])));
                            }
                        }
                    }
                }
                return lstBranch;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region dispose method
        public void Dispose()
        {
        }
        #endregion
    }
}
