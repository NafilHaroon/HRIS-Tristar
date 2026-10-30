using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;

namespace HRIS.Areas.Download.Models
{
    public class EmployeeDeduction : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties        
        public List<EmployeeDeductionDetail> Details { get; set; }
        #endregion

        #region constructor
        public EmployeeDeduction()
        {
            Details = new List<EmployeeDeductionDetail>();
        }
        #endregion

        #region internal methods
        internal static EmployeeDeduction Get()
        {
            try
            {
                using (EmployeeDeduction _ed = new EmployeeDeduction())
                {
                    _ed.Details = EmployeeDeductionDetail.Get();
                    return _ed;
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