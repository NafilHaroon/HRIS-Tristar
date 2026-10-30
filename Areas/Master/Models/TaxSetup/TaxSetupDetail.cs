using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace HRIS.Areas.Master.Models
{
    public class TaxSetupDetail : IDisposable
    {
        #region private properties
        private static readonly SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties        
        public int DetailId { get; set; }
        public decimal IncomeFrom { get; set; }
        public decimal IncomeTo { get; set; }
        public decimal BaseTax { get; set; }
        public decimal TaxRate { get; set; }
        public bool Add { get; set; }
        public bool Edit { get; set; }
        public bool Delete { get; set; }
        #endregion

        #region constructor
        public TaxSetupDetail()
        {
            DetailId = -1;
            Add = true; Edit = false; Delete = false;
        }

        public TaxSetupDetail(int _detailId, decimal _incomeFrom, decimal _incomeTo, decimal _baseTax, decimal _taxRate)
        {
            DetailId = _detailId;
            IncomeFrom = _incomeFrom;
            IncomeTo = _incomeTo;
            BaseTax = _baseTax;
            TaxRate = _taxRate;
            Add = false; Edit = false; Delete = false;
        }
        #endregion

        #region internal methods
        internal static List<TaxSetupDetail> Get(short _period)
        {
            try
            {
                List<TaxSetupDetail> tsdList = new List<TaxSetupDetail>();
                using (DbCommand dbCommand = db.GetStoredProcCommand("GetTaxSetupDetailByYear"))
                {
                    db.AddInParameter(dbCommand, "TaxYear", SqlDbType.SmallInt, _period);
                    using (DataSet ds = db.ExecuteDataSet(dbCommand))
                    {
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                tsdList.Add(new TaxSetupDetail(
                                    Convert.ToInt16(dr["DetailId"]),
                                    Convert.ToDecimal(dr["IncomeFrom"]),
                                    Convert.ToDecimal(dr["IncomeTo"]),
                                    Convert.ToDecimal(dr["BaseTax"]),
                                    Convert.ToDecimal(dr["TaxRate"])));
                            }
                        }
                    }
                }
                return tsdList;
            }
            catch (Exception)
            { throw; }
        }

        internal static bool Save(short  _taxYear, List<TaxSetupDetail> _details, DbTransaction _transaction)
        {
            try
            {
                foreach (TaxSetupDetail _tsd in getTSDetailChanges(_details))
                {
                    using (DbCommand dbCommand = db.GetStoredProcCommand("SaveTaxSetupDetail"))
                    {
                        db.AddInParameter(dbCommand, "DetailId", SqlDbType.SmallInt, _tsd.DetailId);
                        db.AddInParameter(dbCommand, "TaxYear", SqlDbType.SmallInt, _taxYear);
                        db.AddInParameter(dbCommand, "IncomeFrom", SqlDbType.Decimal, _tsd.IncomeFrom);
                        db.AddInParameter(dbCommand, "IncomeTo", SqlDbType.Decimal, _tsd.IncomeTo);
                        db.AddInParameter(dbCommand, "BaseTax", SqlDbType.Decimal, _tsd.BaseTax);
                        db.AddInParameter(dbCommand, "TaxRate", SqlDbType.Decimal, _tsd.TaxRate);
                        db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                        db.AddInParameter(dbCommand, "Action", SqlDbType.Char, (
                            _tsd.Add ? "I" : (_tsd.Delete ? "D" : "U")));
                        db.ExecuteNonQuery(dbCommand, _transaction);
                    }
                }
                return true;
            }
            catch (Exception)
            { throw; }
        }
        #endregion

        #region private methods
        private static IEnumerable<TaxSetupDetail> getTSDetailChanges(List<TaxSetupDetail> _details)
        {
            return (_details.Where(x => x.Add || x.Edit || x.Delete));
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