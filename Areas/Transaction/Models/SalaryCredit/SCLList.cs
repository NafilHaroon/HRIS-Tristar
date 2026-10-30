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
    public class SCLList : IDisposable
    {
        #region private properties
        private static readonly SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        [DataMember(Order = 0, Name = "Letter #")]
        public short? LetterId { get; set; }
        [DataMember(Order = 1, Name = "Date")]
        public string LetterDate { get; set; }
        [DataMember(Order = 2, Name = "Bank")]
        public string BankName { get; set; }
        [DataMember(Order = 3, Name = "Period")]
        public string PeriodName { get; set; }
        #endregion

        #region constructor
        public SCLList()
        {
        }

        public SCLList(short _letterId, string _date, string _bankName, string _periodName)
        {
            LetterId = _letterId;
            LetterDate = _date;
            BankName = _bankName;
            PeriodName = _periodName;
        }
        #endregion

        #region internal methods
        internal static List<SCLList> Get()
        {
            List<SCLList> lstBL = new List<SCLList>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetSalaryCreditLetters"))
            {
                db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, ServerSession.GetCurrentCompany());
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            lstBL.Add(new SCLList(
                                Convert.ToInt16(dr["LetterId"]),
                                dr["LetterDate"].ToString(),
                                dr["BankName"].ToString(),
                                dr["PeriodName"].ToString()));
                        }
                    }
                }
            }
            return lstBL;
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