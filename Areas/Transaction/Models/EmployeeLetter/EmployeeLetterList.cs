using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.Serialization;

namespace HRIS.Areas.Transaction.Models
{
    public class EmployeeLetterList : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        [DataMember(Order =0, Name = "Letter #")]
        public string LetterNo { get; set; }
        [DataMember(Order = 1, Name = "Letter Date")]
        public string LetterDate { get; set; }
        [DataMember(Order = 2, Name = "Template Name")]
        public string TemplateName { get; set; }
        [DataMember(Order = 3, Name = "Employee Name")]
        public string EmployeeName { get; set; }
        public string StateName { get; set; }

        #endregion

        #region constructor
        public EmployeeLetterList()
        {
        }

        public EmployeeLetterList(string _letterNo, string _letterDate, string _employeeName, string _templateName, string _stateName)
        {
            LetterNo = _letterNo;
            LetterDate = _letterDate;
            EmployeeName = _employeeName;
            TemplateName = _templateName;
            StateName = _stateName;
        }
        #endregion

        #region internal methods
        internal static List<EmployeeLetterList> Get()
        {
            List<EmployeeLetterList> lstEL = new List<EmployeeLetterList>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetEmployeeLetters"))
            {
                db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, ServerSession.GetCurrentCompany());
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                using (DataTable dt = db.ExecuteDataSet(dbCommand).Tables[0])
                {
                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            lstEL.Add(new EmployeeLetterList(
                                dr["LetterNo"].ToString(),
                                dr["LetterDate"].ToString(),
                                dr["EmployeeName"].ToString(),
                                dr["TemplateName"].ToString(),
                                dr["StateName"].ToString()));
                        }
                    }
                }
            }
            return lstEL;
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