using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.Serialization;

namespace HRIS.Areas.Master.Models
{
    public class LoanType : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        [DataMember(Order = 0, Name = "Loan Type Id")]
        public short LoanTypeId { get; set; }
        [DataMember(Order = 1, Name = "Loan Type Name")]
        public string LoanTypeName { get; set; }
        #endregion

        #region constructor
        public LoanType()
        {
        }

        public LoanType(short _loanTypeId, string _loanTypeName)
        {
            LoanTypeId = _loanTypeId;
            LoanTypeName = _loanTypeName;
        }
        #endregion

        #region internal methods
        internal static List<LoanType> Get()
        {
            List<LoanType> lstLT = new List<LoanType>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetLoanTypes"))
            {
                using (DataTable dt = db.ExecuteDataSet(dbCommand).Tables[0])
                {
                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            lstLT.Add(new LoanType(
                                Convert.ToInt16(dr["LoanTypeId"]),
                                dr["LoanTypeName"].ToString()));
                        }
                    }
                }
            }
            return lstLT;
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