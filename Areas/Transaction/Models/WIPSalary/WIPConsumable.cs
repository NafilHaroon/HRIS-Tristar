using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace HRIS.Areas.Transaction.Models
{
    public class WIPConsumable : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public int? DetailId { get; set; }
        public short? EntitlementTypeId { get; set; }
        public double Limit { get; set; }
        public string Remarks { get; set; }
        public bool Add { get; set; }
        public bool Edit { get; set; }
        public bool Delete { get; set; }
        #endregion

        #region constructor
        public WIPConsumable()
        {
            DetailId = -1;
            Add = false; Edit = false; Delete = false;
        }
        public WIPConsumable(int _detailId, short _entitlementTypeId, double _limit, string _remarks)
        {
            DetailId = _detailId;
            EntitlementTypeId = _entitlementTypeId;
            Limit = _limit;
            Remarks = _remarks;
            Add = false; Edit = false; Delete = false;
        }
        #endregion

        #region internal methods
        internal static List<WIPConsumable> Get(int _salaryId)
        {
            try
            {
                List<WIPConsumable> lstMA = new List<WIPConsumable>();
                using (DbCommand dbCommand = db.GetStoredProcCommand("GetWIPConsumableById"))
                {
                    db.AddInParameter(dbCommand, "SalaryId", SqlDbType.Int, _salaryId);
                    using (DataSet ds = db.ExecuteDataSet(dbCommand))
                    {
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                lstMA.Add(new WIPConsumable(
                                    Convert.ToInt32(dr["DetailId"]),
                                    Convert.ToInt16(dr["EntitlementTypeId"]),
                                    Convert.ToDouble(dr["Limit"]),
                                    dr["Remarks"].ToString()));
                            }
                        }
                    }
                }
                return lstMA;
            }
            catch (Exception)
            { throw; }
        }

        internal static bool Save(int _salaryId, List<WIPConsumable> _details, DbTransaction _transaction)
        {
            try
            {
                foreach (WIPConsumable _ma in getChanges(_details))
                {
                    using (DbCommand dbCommand = db.GetStoredProcCommand("SaveWIPConsumable"))
                    {
                        dbCommand.Transaction = _transaction;
                        db.AddInParameter(dbCommand, "DetailId", SqlDbType.SmallInt, _ma.DetailId);
                        db.AddInParameter(dbCommand, "SalaryId", SqlDbType.Int, _salaryId);
                        db.AddInParameter(dbCommand, "EntitlementTypeId", SqlDbType.TinyInt, _ma.EntitlementTypeId);
                        db.AddInParameter(dbCommand, "Remarks", SqlDbType.VarChar, _ma.Remarks);
                        db.AddInParameter(dbCommand, "Limit", SqlDbType.Float, _ma.Limit);
                        db.AddInParameter(dbCommand, "Action", SqlDbType.Char, (
                               _ma.Add ? "I" : (_ma.Delete ? "D" : "U")));
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
        private static IEnumerable<WIPConsumable> getChanges(List<WIPConsumable> _details)
        {
            return (_details.Where(x => (x.Add && !x.Delete) || (x.Edit && !x.Delete) || x.Delete));
        }
        #endregion

        #region dispose method
        public void Dispose()
        {
        }
        #endregion
    }
}