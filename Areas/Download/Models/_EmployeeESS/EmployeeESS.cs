using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;

namespace HRIS.Areas.Download.Models
{
    public class EmployeeESS : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties        
        public List<EmployeeESSDetail> Details { get; set; }
        #endregion

        #region constructor
        public EmployeeESS()
        {
            Details = new List<EmployeeESSDetail>();
        }
        #endregion

        #region internal methods
        internal static EmployeeESS Get()
        {
            try
            {
                using (EmployeeESS _esy = new EmployeeESS())
                {
                    _esy.Details = EmployeeESSDetail.Get();
                    return _esy;
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