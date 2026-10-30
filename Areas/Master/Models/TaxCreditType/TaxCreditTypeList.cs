using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.Serialization;

namespace HRIS.Areas.Master.Models
{
    public class TaxCreditTypeList : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        [DataMember(Name = "Type Id", Order = 0)]
        public short TaxCreditTypeId { get; set; }
        [DataMember(Name = "Tax Credit Type Desc", Order = 1)]
        public string TaxCreditTypeName { get; set; }
        [DataMember(Name = "Active?", Order = 4)]
        public bool IsActive { get; set; }
        #endregion

        #region constructor
        public TaxCreditTypeList() { }
        public TaxCreditTypeList(short _typeId, string _typeName, bool _isActive = true)
        {
            TaxCreditTypeId = _typeId;
            TaxCreditTypeName = _typeName;
            IsActive = _isActive;
        }
        #endregion

        #region internal methods
        internal static List<TaxCreditTypeList> Get(bool _isActive = true)
        {
            List<TaxCreditTypeList> lstAT = new List<TaxCreditTypeList>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetTaxCreditTypes"))
            {
                db.AddInParameter(dbCommand, "ActiveOnly", SqlDbType.Bit, _isActive);
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            lstAT.Add(new TaxCreditTypeList(
                                Convert.ToInt16(dr["TaxCreditTypeId"]),
                                dr["TaxCreditTypeName"].ToString(),
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
