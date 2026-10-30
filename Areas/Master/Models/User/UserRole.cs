using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class UserRole :IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short? UserRoleId { get; set; }
        public short RoleId { get; set; }
        public string RoleName { get; set; }
        public bool Selected { get; set; }
        #endregion

        #region constructor
        public UserRole()
        {

        }
//short _serialNo, 
        internal UserRole(object _userRoleId, short _roleId, string _roleName, bool _selected)
        {
            //SerialNo = _serialNo;
            if (_userRoleId != DBNull.Value)
                UserRoleId = Convert.ToInt16(_userRoleId);
            RoleId = _roleId;
            RoleName = _roleName;
            Selected = _selected;
        }
        #endregion

        #region internal methods
        internal static List<UserRole> Get(string _userId)
        {
            List<UserRole> lstRole = new List<UserRole>();
            try
            {
                using (DbCommand dbCommandDetail = db.GetStoredProcCommand("GetUserRolesbyId"))
                {
                    db.AddInParameter(dbCommandDetail, "UserId", SqlDbType.VarChar, _userId);
                    using (DataSet ds = db.ExecuteDataSet(dbCommandDetail))
                    {
                        if (ds.Tables.Count > 0)
                        {
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                lstRole.Add(new UserRole(
                                    dr["UserRoleId"],
                                    Convert.ToInt16(dr["RoleId"]),
                                    dr["RoleName"].ToString(),
                                    Convert.ToBoolean(dr["Selected"])));
                            }
                        }
                    }
                }
                return lstRole;
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
