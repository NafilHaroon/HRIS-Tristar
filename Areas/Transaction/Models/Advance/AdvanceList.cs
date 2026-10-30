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
    public class AdvanceList : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        [DataMember(Order = 0, Name = "Ref #")]
        public short AdvanceId { get; set; }
        [DataMember(Order = 1, Name = "Advance Date")]
        public string AdvanceDate { get; set; }
        [DataMember(Order = 2, Name = "Employee Name")]
        public string EmployeeName { get; set; }
        [DataMember(Order =3, Name = "Amount")]
        public decimal Amount { get; set; }
        #endregion

        #region constructor
        public AdvanceList()
        {
        }

        public AdvanceList(short _advanceId, string _advanceDate, string _employeeName, decimal _amount)
        {
            AdvanceId = _advanceId;
            AdvanceDate = _advanceDate;
            EmployeeName = _employeeName;
            Amount = _amount;
        }
        #endregion

        #region internal methods
        internal static List<AdvanceList> Get()
        {
            List<AdvanceList> lstAdv = new List<AdvanceList>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetAdvanceSalaries_Pending"))
            {
                db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, ServerSession.GetCurrentCompany());
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            lstAdv.Add(new AdvanceList(
                                Convert.ToInt16(dr["AdvanceId"]),
                                dr["AdvanceDate"].ToString(),
                                dr["EmployeeName"].ToString(),
                                Convert.ToDecimal(dr["Amount"])));
                        }
                    }
                }
            }
            return lstAdv;
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