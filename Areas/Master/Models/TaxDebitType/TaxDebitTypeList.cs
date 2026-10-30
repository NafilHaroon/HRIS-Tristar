using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.Serialization;

namespace HRIS.Areas.Master.Models
{
    public class TaxDebitTypeList : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        [DataMember(Name = "Type Id", Order = 0)]
        public short TaxDebitTypeId { get; set; }
        [DataMember(Name = "Tax Debit Type Desc", Order = 1)]
        public string TaxDebitTypeName { get; set; }
        [DataMember(Name = "Active?", Order = 4)]
        public bool IsActive { get; set; }
        #endregion

        #region constructor
        public TaxDebitTypeList() { }
        public TaxDebitTypeList(short _typeId, string _typeName, bool _isActive = true)
        {
            TaxDebitTypeId = _typeId;
            TaxDebitTypeName = _typeName;
            IsActive = _isActive;
        }
        #endregion

        #region internal methods
        internal static List<TaxDebitTypeList> Get(bool _isActive = true)
        {
            List<TaxDebitTypeList> lstAT = new List<TaxDebitTypeList>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetTaxDebitTypes"))
            {
                db.AddInParameter(dbCommand, "ActiveOnly", SqlDbType.Bit, _isActive);
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            lstAT.Add(new TaxDebitTypeList(
                                Convert.ToInt16(dr["TaxDebitTypeId"]),
                                dr["TaxDebitTypeName"].ToString(),
                                Convert.ToBoolean(dr["IsActive"])));
                        }
                    }
                }
            }
            return lstAT;
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
