using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;

namespace HRIS.Areas.Download.Models
{
    public class EmployeePersonal : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties        
        public List<EmployeePersonalDetail> Details { get; set; }
        #endregion

        #region constructor
        public EmployeePersonal()
        {
            Details = new List<EmployeePersonalDetail>();
        }
        #endregion

        #region internal methods
        internal static EmployeePersonal Get()
        {
            try
            {
                using (EmployeePersonal _ep = new EmployeePersonal())
                {
                    _ep.Details = EmployeePersonalDetail.Get();
                    return _ep;
                }
            }
            catch (Exception)
            { throw; }
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