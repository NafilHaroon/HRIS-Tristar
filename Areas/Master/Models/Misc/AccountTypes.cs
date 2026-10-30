using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class AccountTypes : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short AccountTypeId { get; set; }
        public string AccountTypeName { get; set; }
        #endregion

        #region constructor
        public AccountTypes()
        {
        }

        //public AccountTypes(short _accountTypeId, string _accountTypeName)
        //{
        //    AccountTypeId = _accountTypeId;
        //    AccountTypeName = _accountTypeName;
        //}
        #endregion

        #region internal methods
        /// <summary>
        /// returns list of Account Types
        /// </summary>
        /// <returns></returns>
        internal static List<AccountTypes> Get()
        {
            List<AccountTypes> types = new List<AccountTypes>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetAccountTypes"))
            {
                using (DataTable dt = db.ExecuteDataSet(dbCommand).Tables[0])
                {
                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            types.Add(new AccountTypes
                            {
                                AccountTypeId = Convert.ToInt16(dr["AccountTypeId"]),
                                AccountTypeName = dr["AccountTypeName"].ToString()
                            });
                        }
                    }
                }
            }
            return types;
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