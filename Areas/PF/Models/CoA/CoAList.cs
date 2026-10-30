using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.Serialization;

namespace HRIS.Areas.PF.Models
{
    public class CoAList : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        [DataMember(Order = 0, Name = "Account Id")]
        public short AccountId { get; set; }

        [DataMember(Order = 1, Name = "Account Code")]
        public string AccountCode { get; set; }
       
        [DataMember(Order = 2, Name = "Account Name")]
        public string AccountName { get; set; }
        [DataMember(Order = 3, Name = "Account Type")]
        public string AccountType { get; set; }
        //[DataMember(Order = 4)]
        //[isplay(Name = "Company Code")]
        //public string CompanyCode { get; set; }
        //[DataMember(Order = 5)]
        //[isplay(Name = "Company Code")]
        //[Browsable(false)]
        //public short CompanyId { get; set; }
        //[Browsable(false)]
        //[DataMember(Order = 6)]
        //[isplay(Name = "Account Type")]
        //public short? AccountTypeId { get; set; }
        //[DataMember(Order = 7)]
        //[Browsable(false)]
        //[isplay(Name = "Bank Account")]
        //public bool BankAccount { get; set; }
        #endregion

        #region constructor
        public CoAList()
        {
        }
       
        public CoAList(short _accountId, string _accountName)
        {
            AccountId = _accountId;
            AccountName = _accountName;
        }

        public CoAList(short _accountId, string _accountCode, string _accountName, string _accountType)//, string _CompanyCode)
        {
            AccountId = _accountId;
            AccountCode = _accountCode;
            AccountName = _accountName;
            AccountType = _accountType;
            //CompanyCode = _CompanyCode;
        }
        #endregion

        #region internal methods
        ///returns list of all accounts that are not tagged to any employee
        internal static List<CoAList> Get()
        {
            List<CoAList> lstCoA = new List<CoAList>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetCoAs"))
            {
                db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, ServerSession.GetCurrentCompany());
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                {
                    using (DataSet ds = db.ExecuteDataSet(dbCommand))
                    {
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                lstCoA.Add(new CoAList(
                                    Convert.ToInt16(dr["AccountId"]),
                                    dr["AccountCode"].ToString(),
                                    dr["AccountName"].ToString(),
                                    dr["AccountTypeName"].ToString()));
                            }
                        }
                    }
                }
            }
            return lstCoA;
        }

        /// <summary>
        /// Returns list of accounts for specific Account Type
        /// </summary>
        /// <param name="_accountTypeId">Account Type for which account list is required</param>
        /// <returns></returns>
        internal static List<CoAList> Get(short _accountTypeId)
        {
            return get("GetCOAsByType", _accountTypeId);
        }

        /// <summary>
        /// Return list of accounts that are marked as Bank Account
        /// </summary>
        /// <returns></returns>
        internal static List<CoAList> GetBankAccounts()
        {
            return get("GetCoABanks");
        }

        /// <summary>
        /// Returns complete list of account for login company
        /// </summary>
        /// <returns></returns>
        internal static List<CoAList> GetCoAsForDropdown()
        {
            return get("GetCoAsForDropdown");
        }
        #endregion

        #region private methods
        private static List<CoAList> get(string _spName, short? _accountTypeId=null)
        {
            List<CoAList> lstCoA = new List<CoAList>();
            using (DbCommand dbCommand = db.GetStoredProcCommand(_spName))
            {
                if (_accountTypeId.HasValue)
                    db.AddInParameter(dbCommand, "AccountTypeId", SqlDbType.TinyInt, _accountTypeId);
                db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, ServerSession.GetCurrentCompany());
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                using (DataTable dt = db.ExecuteDataSet(dbCommand).Tables[0])
                {
                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            lstCoA.Add(new CoAList(
                            Convert.ToInt16(dr["AccountId"]),
                            dr["AccountName"].ToString()));
                        }
                    }
                }
            }
            return lstCoA;
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