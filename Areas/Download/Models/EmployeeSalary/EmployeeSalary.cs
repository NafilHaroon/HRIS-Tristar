using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;

namespace HRIS.Areas.Download.Models
{
    public class EmployeeSalary : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties        
        public List<EmployeeSalaryDetail> Details { get; set; }
        #endregion

        #region constructor
        public EmployeeSalary()
        {
            Details = new List<EmployeeSalaryDetail>();
        }
        #endregion

        #region internal methods
        internal static EmployeeSalary Get()//string _employeeCode, string _branchCode, string _departmentCode, short _gradeId)
        {
            try
            {
                using (EmployeeSalary _esy = new EmployeeSalary())
                {
                    _esy.Details = EmployeeSalaryDetail.Get();//_employeeCode, _branchCode, _departmentCode, _gradeId);
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