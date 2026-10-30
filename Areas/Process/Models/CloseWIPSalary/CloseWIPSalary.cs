using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace HRIS.Areas.Process.Models
{
    public class CloseWIPSalary : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties        
        public List<CloseWIPSalaryDetail> Details { get; set; }
        #endregion

        #region constructor
        public CloseWIPSalary()
        {
            Details = new List<CloseWIPSalaryDetail>();
        }
        #endregion

        #region internal methods
        internal static CloseWIPSalary Get()
        {
            try
            {
                using (CloseWIPSalary _cws = new CloseWIPSalary())
                {
                    _cws.Details = CloseWIPSalaryDetail.Get();
                    return _cws;
                }
            }
            catch (Exception)
            { throw; }
        }
        internal static bool Close(CloseWIPSalary _cwp)
        {
            using (DbConnection dbconnection = db.CreateConnection())
            {
                dbconnection.Open();
                DbTransaction _transaction = dbconnection.BeginTransaction();
                try
                {
                    CloseWIPSalaryDetail.CloseSalary(_cwp.Details.Where(x => x.Close).ToList(), _transaction);
                    _transaction.Commit();
                    return true;
                }
                catch (Exception)
                {
                    _transaction.Rollback();
                    throw;
                }
            }
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