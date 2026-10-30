using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Runtime.Serialization;

namespace HRIS.Areas.Transaction.Models
{
    public class WIPSalaryList : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        [DataMember(Order = 0, Name = "Employee Id")]
        [Browsable(false)]
        public short EmployeeId { get; set; }
        [DataMember(Order = 1, Name = "Employee Code")]
        public string EmployeeCode { get; set; }
        [DataMember(Order = 2, Name = "Employee Name")]
        public string EmployeeName { get; set; }
        [DataMember(Order = 3, Name = "Branch")]
        public string BranchName { get; set; }
        [DataMember(Order = 4, Name = "Department")]
        public string DepartmentName { get; set; }
        [DataMember(Order = 5, Name = "Grade")]
        public string GradeDesc { get; set; }
        [DataMember(Order = 6, Name = "Designation")]
        public string Designation { get; set; }
        #endregion

        #region constructor
        public WIPSalaryList()
        {
        }

        public WIPSalaryList(short _employeeId, string _employeeCode, string _employeeName,
            string _branchName, string _departmentName, string _gradeDesc, string _designation)
        {
            EmployeeId = _employeeId;
            EmployeeCode = _employeeCode;
            EmployeeName = _employeeName;
            BranchName = _branchName;
            DepartmentName = _departmentName;
            GradeDesc = _gradeDesc;
            Designation = _designation;
        }
        #endregion

        #region internal methods
        internal static List<WIPSalaryList> Get()
        {
            List<WIPSalaryList> lstMS = new List<WIPSalaryList>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetWIPSalaries"))
            {
                db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, ServerSession.GetCurrentCompany());
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            lstMS.Add(new WIPSalaryList(
                                Convert.ToInt16(dr["EmployeeId"]),
                                dr["EmployeeCode"].ToString(),
                                dr["EmployeeName"].ToString(),
                                dr["BranchName"].ToString(),
                                dr["DepartmentName"].ToString(),
                                dr["GradeNo"].ToString(),
                                dr["Designation"].ToString()));
                        }
                    }
                }
            }
            return lstMS;
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