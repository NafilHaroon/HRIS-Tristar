using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Process.Models
{
    public class BulkSupervisor : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public string BranchCode { get; set; }
        public string DepartmentCode { get; set; }
        public short? SupervisorId { get; set; }
        public short? NewSupervisorId { get; set; }
        #endregion

        #region constructor
        public BulkSupervisor()
        {
        }
        #endregion

        #region internal methods
        internal static bool Update(BulkSupervisor _bs)
        {
            try
            {
                using (DbCommand dbCommand = db.GetStoredProcCommand("UpdateSupervisor"))
                {
                    db.AddInParameter(dbCommand, "BranchCode", SqlDbType.VarChar, _bs.BranchCode);
                    db.AddInParameter(dbCommand, "DepartmentCode", SqlDbType.VarChar, _bs.DepartmentCode);
                    db.AddInParameter(dbCommand, "SupervisorId", SqlDbType.SmallInt, _bs.SupervisorId);
                    db.AddInParameter(dbCommand, "NewSupervisorId", SqlDbType.SmallInt, _bs.NewSupervisorId);
                    db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());

                    db.ExecuteNonQuery(dbCommand);

                    return true;
                }
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