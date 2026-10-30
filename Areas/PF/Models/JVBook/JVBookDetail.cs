using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace HRIS.Areas.PF.Models
{
    public class JVBookDetail : IDisposable
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
        //public bool Add { get; set; }
        //public bool Edit { get; set; }
        public bool Delete { get; set; }
        #endregion

        #region constructor
        public JVBookDetail()
        {
            DetailId = -1;
            Credit = 0;
            Debit = 0;
            Delete = false;
        }
        public JVBookDetail(int _detailId, short _accountId, string _description,
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
        internal static List<JVBookDetail> Get(short _voucherId)
        {
            try
            {
                List<JVBookDetail> lstJVD = new List<JVBookDetail>();
                using (DbCommand dbCommand = db.GetStoredProcCommand("GetPFJVDetailById"))
                {
                    db.AddInParameter(dbCommand, "VoucherId", SqlDbType.VarChar, _voucherId);
                    using (DataSet ds = db.ExecuteDataSet(dbCommand))
                    {
                        if (ds != null && ds.Tables.Count > 0)
                        {
                           
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                lstJVD.Add(new JVBookDetail(
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
                return lstJVD;
            }
            catch (Exception)
            { throw; }
        }


        internal static bool Save(short _voucherid, List<JVBookDetail> _details, DbTransaction _transaction)
        {
            try
            {
                foreach (JVBookDetail _jvd in _details.Where(x => !x.Delete))
                {
                    using (DbCommand dbCommand = db.GetStoredProcCommand("SavePFJVDetail"))
                    {
                        dbCommand.Transaction = _transaction;
                        db.AddInParameter(dbCommand, "VoucherId", SqlDbType.SmallInt, _voucherid);
                        db.AddInParameter(dbCommand, "AccountId", SqlDbType.SmallInt, _jvd.AccountId);
                        db.AddInParameter(dbCommand, "Description", SqlDbType.VarChar, _jvd.Description);
                        db.AddInParameter(dbCommand, "Credit", SqlDbType.Float, _jvd.Credit);
                        db.AddInParameter(dbCommand, "Debit", SqlDbType.Float, _jvd.Debit);
                        db.AddInParameter(dbCommand, "LoanId", SqlDbType.SmallInt, _jvd.LoanId);
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