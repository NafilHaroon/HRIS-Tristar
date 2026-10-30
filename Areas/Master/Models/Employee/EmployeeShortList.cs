using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.Serialization;

namespace HRIS.Areas.Master.Models
{
    public class EmployeeShortList : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        [DataMember(Name = "Employee Id", Order = 1)]
        public short? EmployeeId { get; set; }
        [DataMember(Name = "Employee Name", Order = 2)]
        public string EmployeeName { get; set; }
        public short StatusId { get; set; }
        #endregion

        #region constructor
        public EmployeeShortList()
        {
        }
        #endregion

        #region internal methods
        internal static List<EmployeeShortList> Get(bool _activeOnly = true)
        {
            return get("GetEmployeesShortInfo", _activeOnly);
        }

        internal static List<EmployeeShortList> GetSupervisors()
        {
            return get("GetSupervisors");
        }

        internal static List<EmployeeShortList> GetForWIPSalary()
        {
            return get("GetWIPSalaryEmployees");
        }

        internal static List<EmployeeShortList> GetAllEmployeesForCompany()
        {
            return get("GetEmployeesForCompany");
        }
                
        internal static List<EmployeeShortList> GetEmployeesNotReplaced(short? _replacementEmployeeId)
        {
            return getEmployeesNR(_replacementEmployeeId);
        }

        internal static EmployeeShortList GetEmployeeByCode(string _employeeCode)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetEmployeeShortInfoByCode"))
            {
                db.AddInParameter(dbCommand, "CompanyId", SqlDbType.SmallInt, ServerSession.GetCurrentCompany());
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                db.AddInParameter(dbCommand, "EmployeeCode", SqlDbType.VarChar, _employeeCode);
                using (DataTable dt = db.ExecuteDataSet(dbCommand).Tables[0])
                {
                    if (dt != null)
                    {
                        return new EmployeeShortList
                        {
                            EmployeeId = Convert.ToInt16(dt.Rows[0]["EmployeeId"]),
                            EmployeeName = dt.Rows[0]["EmployeeName"].ToString()
                        };
                    }
                }
            }
            return null;
        }
        #endregion

        #region private methods
        private static List<EmployeeShortList> get(string _spName, bool? _activeOnly = null)
        {
            List<EmployeeShortList> employees = new List<EmployeeShortList>();
            using (DbCommand dbCommand = db.GetStoredProcCommand(_spName))
            {
                db.AddInParameter(dbCommand, "CompanyId", SqlDbType.SmallInt, ServerSession.GetCurrentCompany());
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                if (_activeOnly.HasValue)
                    db.AddInParameter(dbCommand, "ActiveOnly", SqlDbType.Bit, _activeOnly);

                using (DataTable dt = db.ExecuteDataSet(dbCommand).Tables[0])
                {
                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            if (_activeOnly.HasValue)
                            {
                                employees.Add(new EmployeeShortList
                                {
                                    EmployeeId = Convert.ToInt16(dr["EmployeeId"]),
                                    EmployeeName = dr["EmployeeName"].ToString(),
                                    StatusId = Convert.ToInt16(dr["StatusId"])
                                });
                            }
                            else
                            {
                                employees.Add(new EmployeeShortList
                                {
                                    EmployeeId = Convert.ToInt16(dr["EmployeeId"]),
                                    EmployeeName = dr["EmployeeName"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            return employees;
        }
        
        private static List<EmployeeShortList> getEmployeesNR(short? _employeeId)
        {
            List<EmployeeShortList> employees = new List<EmployeeShortList>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetEmployeesNotReplaced"))
            {
                db.AddInParameter(dbCommand, "CompanyId", SqlDbType.SmallInt, ServerSession.GetCurrentCompany());
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                db.AddInParameter(dbCommand, "ReplacementEmployeeId", SqlDbType.SmallInt, _employeeId);
                using (DataTable dt = db.ExecuteDataSet(dbCommand).Tables[0])
                {
                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            employees.Add(new EmployeeShortList
                            {
                                EmployeeId = Convert.ToInt16(dr["EmployeeId"]),
                                EmployeeName = dr["EmployeeName"].ToString()
                            });
                        }
                    }
                }
            }
            return employees;
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