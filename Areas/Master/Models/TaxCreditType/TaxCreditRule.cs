using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.Serialization;

namespace HRIS.Areas.Master.Models
{
    public class TaxCreditRule : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short CreditRuleId { get; set; }
        [DataMember(Name = "Tax Credit Type Desc", Order = 1)]
        public string CreditRuleName { get; set; }
        #endregion

        #region constructor
        public TaxCreditRule() { }
        public TaxCreditRule(short _ruleId, string _ruleName)
        {
            CreditRuleId = _ruleId;
            CreditRuleName = _ruleName;
        }
        #endregion

        #region internal methods
        internal static List<TaxCreditRule> Get(bool _isActive = true)
        {
            List<TaxCreditRule> lstCR = new List<TaxCreditRule>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetTaxCreditRules"))
            {
                db.AddInParameter(dbCommand, "ActiveOnly", SqlDbType.Bit, _isActive);
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            lstCR.Add(new TaxCreditRule(
                                Convert.ToInt16(dr["CreditRuleId"]),
                                dr["CreditRuleName"].ToString()));
                        }
                    }
                }
            }
            return lstCR;
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
