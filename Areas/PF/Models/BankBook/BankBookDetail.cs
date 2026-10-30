using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace HRIS.Areas.PF.Models
{
    public class BankBookDetail : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public int DetailId { get; set; }
        public short? AccountId { get; set; }
        public string Description { get; set; }
        public double Debit { get; set; }
        public double Credit { get; set; }
        public short? LoanId { get; set; }
        public bool Delete { get; set; }
        #endregion

        #region constructor
        public BankBookDetail()
        {
            DetailId = -1;
            Debit = 0;
            Credit = 0;
            Delete = false;
        }

        public BankBookDetail(int _detailId, short _accountId, string _description,
            bool _debit, bool _credit, double _amount, object _loanId)
        {
            DetailId = _detailId;
            AccountId = _accountId;
            Description = _description;
            Debit = (_debit ? _amount : 0);
            Credit = (_credit ? _amount : 0);
            if (_loanId != DBNull.Value)
                LoanId = Convert.ToInt16(_loanId);
        }
        #endregion

        #region internal methods
        internal static List<BankBookDetail> Get(short _voucherId)
        {
            try
            {
                List<BankBookDetail> lstBBD = new List<BankBookDetail>();
                using (DbCommand dbCommand = db.GetStoredProcCommand("GetPFBBDetailById"))
                {
                    db.AddInParameter(dbCommand, "VoucherId", SqlDbType.SmallInt, _voucherId);

                    using (DataSet ds = db.ExecuteDataSet(dbCommand))
                    {
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                lstBBD.Add(new BankBookDetail(
                                    Convert.ToInt32(dr["DetailId"]),
                                    Convert.ToInt16(dr["AccountId"]),
                                    dr["Description"].ToString(),
                                    Convert.ToBoolean(dr["Debit"]),
                                    Convert.ToBoolean(dr["Credit"]),
                                    Convert.ToDouble(dr["Amount"]),
                                    dr["LoanId"]));
                            }
                        }
                    }
                }
                return lstBBD;
            }
            catch (Exception)
            { throw; }
        }

        internal static bool Save(short _voucherId, List<BankBookDetail> _details, DbTransaction _transaction)
        {
            try
            {
                foreach (BankBookDetail _bbd in _details.Where(x => !x.Delete))
                {
                    using (DbCommand dbCommand = db.GetStoredProcCommand("SavePFBBDetail"))
                    {
                        dbCommand.Transaction = _transaction;
                        db.AddInParameter(dbCommand, "VoucherId", SqlDbType.SmallInt, _voucherId);
                        db.AddInParameter(dbCommand, "AccountId", SqlDbType.VarChar, _bbd.AccountId);
                        db.AddInParameter(dbCommand, "Description", SqlDbType.VarChar, _bbd.Description);
                        db.AddInParameter(dbCommand, "Debit", SqlDbType.Float, _bbd.Debit);
                        db.AddInParameter(dbCommand, "Credit", SqlDbType.Float, _bbd.Credit);
                        db.AddInParameter(dbCommand, "LoanId", SqlDbType.SmallInt, _bbd.LoanId);
                        db.ExecuteNonQuery(dbCommand, _transaction);
                    }
                }
                return true;
            }
            catch (Exception)
            { throw; }
        }
        #endregion

        #region dispose method
        public void Dispose()
        {
        }
        #endregion
    }
}