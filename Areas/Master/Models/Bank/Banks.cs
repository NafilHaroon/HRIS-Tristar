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
    public class Banks : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        [DataMember(Order = 0, Name = "Bank Id")]
        public short BankId { get; set; }
        [DataMember(Order = 1, Name = "Bank Name")]
        public string BankName { get; set; }
        //[isplay(Name = "Company Name")]
        //public string CompanyName { get; set; }
        [DataMember(Order = 2)]
        public bool IsActive { get; set; }
        #endregion

        #region constructor
        public Banks()
        {
        }

        //public Banks(short _bankId, string _bankName, bool _isActive = true)//string _companyName, 
        //{
        //    BankId = _bankId;
        //    BankName = _bankName;
        //    //CompanyName = _companyName;
        //    IsActive = _isActive;
        //}
        #endregion

        #region internal methods
        internal static List<Banks> Get(bool _isActive = true)
        {
            List<Banks> banks = new List<Banks>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetBanks"))
            {
                db.AddInParameter(dbCommand, "CompanyId", SqlDbType.SmallInt, ServerSession.GetCurrentCompany());
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                db.AddInParameter(dbCommand, "ActiveOnly", SqlDbType.Bit, _isActive);
                using (DataTable dt = db.ExecuteDataSet(dbCommand).Tables[0])
                {
                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            banks.Add(new Banks
                            {
                                BankId = Convert.ToInt16(dr["BankId"]),
                                BankName = dr["BankName"].ToString(),
                                IsActive = Convert.ToBoolean(dr["IsActive"])
                            });
                        }
                    }
                }
            }
            return banks;
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