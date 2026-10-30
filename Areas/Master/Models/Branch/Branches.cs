using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class Branches : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        #endregion

        #region constructor
        public Branches()
        {
        }

        //public Branches(string _branchCode, string _branchName)
        //{
        //    BranchCode = _branchCode;
        //    BranchName = _branchName;

        //}
        #endregion

        #region internal methods
        internal static List<Branches> Get(bool _activeOnly)
        {
            List<Branches> branches = new List<Branches>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetBranches"))
            {
                db.AddInParameter(dbCommand, "ActiveOnly", SqlDbType.Bit, _activeOnly);
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                using (DataTable dt = db.ExecuteDataSet(dbCommand).Tables[0])
                {
                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            branches.Add(new Branches
                            {
                                BranchCode = dr["BranchCode"].ToString(),
                                BranchName = dr["BranchName"].ToString()
                            });
                        }
                    }
                }
            }
            return branches;
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