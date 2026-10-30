using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Download.Models
{
    public class EmployeeESSDetail : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string Designation { get; set; }
        public string BranchName { get; set; }
        public string DepartmentName { get; set; }
        public string MaritalStatus { get; set; }
        public string NTN { get; set; }
        public string ESSNo { get; set; }
        public string Gender { get; set; }
        public string EOBINo { get; set; }
        public string SSMember { get; set; }
        #endregion

        #region constructor
        public EmployeeESSDetail()
        {
        }

        public EmployeeESSDetail(string _employeeCode, string _employeeName, string _designation,
            string _branchName, string _departmentName, string _maritalStatus, string _ntn, 
            string _essNo, string _gender, string _eobiNo, string _ssMember)
        {
            EmployeeCode = _employeeCode;
            EmployeeName = _employeeName;
            Designation = _designation;
            BranchName = _branchName;
            DepartmentName = _departmentName;
            MaritalStatus = _maritalStatus;
            NTN = _ntn;
            ESSNo = _essNo;
            Gender = _gender;
            EOBINo = _eobiNo;
            SSMember = _ssMember;
        }
        #endregion

        #region internal methods
        internal static List<EmployeeESSDetail> Get()
        {
            List<EmployeeESSDetail> lstESS = new List<EmployeeESSDetail>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("extEmployeeDetailWithESS"))
            {
                db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, ServerSession.GetCurrentCompany());
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            lstESS.Add(new EmployeeESSDetail(
                                dr["EmployeeCode"].ToString(),
                                dr["EmployeeName"].ToString(),
                                dr["Designation"].ToString(),
                                dr["BranchName"].ToString(),
                                dr["DepartmentName"].ToString(),
                                dr["MaritalStatusName"].ToString(),
                                dr["NTN"].ToString(),
                                dr["ESSNo"].ToString(),
                                dr["Gender"].ToString(),
                                dr["EOBINo"].ToString(),
                                dr["SSMember"].ToString()));
                        }
                    }
                }
            }
            return lstESS;
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