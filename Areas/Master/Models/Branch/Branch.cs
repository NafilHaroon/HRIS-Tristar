using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class Branch : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        public double SSRate { get; set; }
        public double SSMaxAmount { get; set; }
        public bool IsActive { get; set; } = true;
        public Footer AuditTrail { get; set; } = new Footer();
        #endregion

        #region constructor
        public Branch()
        {
            //AuditTrail = new Footer();
            //IsActive = true;
        }
        #endregion

        #region internal methods
        internal static Branch Get(string _branchCode)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetBranchByCode"))
            {
                db.AddInParameter(dbCommand, "BranchCode", SqlDbType.VarChar, _branchCode);
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds.Tables != null && ds.Tables[0].Rows.Count > 0)
                    {
                        using (Branch _b = new Branch())
                        {
                            DataRow dr = ds.Tables[0].Rows[0];
                            _b.BranchCode = dr["BranchCode"].ToString();
                            _b.BranchName = dr["BranchName"].ToString();
                            _b.SSRate = Convert.ToDouble(dr["SSRate"]);
                            _b.SSMaxAmount = Convert.ToDouble(dr["SSMaxAmount"]);
                            _b.IsActive = Convert.ToBoolean(dr["IsActive"]);
                            _b.AuditTrail = new Footer(dr);
                            return _b;
                        }
                    }
                    else
                        return null;
                }
            }
        }

        internal static bool Save(Branch _b)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("SaveBranch"))
            {
                try
                {
                    db.AddInParameter(dbCommand, "BranchCode", SqlDbType.VarChar, _b.BranchCode);
                    db.AddInParameter(dbCommand, "SSRate", SqlDbType.Float, _b.SSRate);
                    db.AddInParameter(dbCommand, "SSMaxAmount", SqlDbType.Float, _b.SSMaxAmount);
                    db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                    db.AddInParameter(dbCommand, "UpdatedOn", SqlDbType.DateTime, _b.AuditTrail.UpdatedOn);
                    db.ExecuteNonQuery(dbCommand);
                    return true;
                }
                catch (Exception)
                { throw; }
            }
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            // no implementation
        }
        #endregion
    }
}