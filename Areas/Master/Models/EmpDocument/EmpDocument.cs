using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class EmpDocument : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public List<EmpDocumentDetail> Details { get; set; }
        #region section to add new record
        public bool AddNew { get; set; }
        public short? DocTypeId { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public short? ReminderDays { get; set; }
        #endregion
        #endregion

        #region constructor
        public EmpDocument()
        {
            Details = new List<EmpDocumentDetail>();
            //NewDocument = new EmpDocumentDetail();
            AddNew = false;
        }
        #endregion

        #region internal methods
        internal static EmpDocument Get(string _employeeCode)
        {
            try
            {
                using (EmpDocument _ed = new EmpDocument())
                {
                    EmployeeShortList _esl = EmployeeShortList.GetEmployeeByCode(_employeeCode);
                    if (_esl != null)
                    {
                        _ed.EmployeeCode = _employeeCode;
                        _ed.EmployeeName = _esl.EmployeeName;
                        _ed.Details = EmpDocumentDetail.Get(_employeeCode);
                        return _ed;
                    }
                    throw new Exception("Employee code doesn't exist or you don't have permission to access this employee!");
                }
            }
            catch (Exception)
            { throw; }
        }

        internal static bool Save(EmpDocument _ed)
        {
            using (DbConnection dbconnection = db.CreateConnection())
            {
                dbconnection.Open();
                DbTransaction _transaction = dbconnection.BeginTransaction();
                try
                {
                    EmpDocumentDetail.Save(_ed.EmployeeCode, _ed.Details, ServerSession.GetCurrentUser(), _transaction);
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

        #region dispose method
        public void Dispose()
        {
        }
        #endregion
    }
}