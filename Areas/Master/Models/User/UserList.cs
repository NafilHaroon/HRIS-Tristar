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
    public class UserList : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        [DataMember(Order = 0, Name = "User Id")]
        public string UserId { get; set; }
        [DataMember(Order = 1, Name = "User Name")]
        public string UserName { get; set; }
        [DataMember(Order = 2, Name = "Branch Name")]
        public string BranchName { get; set; }
        [DataMember(Order = 4, Name = "Department Name")]
        public string DepartmentName { get; set; }
        #endregion

        #region constructor
        public UserList()
        {
        }

        public UserList(string _userId, string _userName, string _branchName, string _departmentName)
        {
            UserId = _userId;
            UserName = _userName;
            BranchName = _branchName;
            DepartmentName = _departmentName;
        }
        #endregion

        #region internal methods
        internal static List<UserList> Get(bool _activeOnly)
        {
            try
            {
                List<UserList> lstUser = new List<UserList>();
                using (DbCommand dbCommand = db.GetStoredProcCommand("GetUsers"))
                {
                    db.AddInParameter(dbCommand, "ActiveOnly", SqlDbType.Bit, _activeOnly);
                    using (DataSet ds = db.ExecuteDataSet(dbCommand))
                    {
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                lstUser.Add(new UserList(
                                    dr["UserId"].ToString(),
                                    dr["UserName"].ToString(),
                                    dr["BranchName"].ToString(),
                                    dr["DepartmentName"].ToString()));
                            }
                        }
                    }
                }
                return lstUser;
            }
            catch (Exception)
            { throw; }
        }
        #endregion

        #region IDisposable Implementation
        public void Dispose()
        {
        }
        #endregion
    }
}