using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace HRIS.Areas.Transaction.Models
{
    public class WIPSalaryDetail : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public int? DetailId { get; set; }
        public short? HeadId { get; set; }
        public double Amount { get; set; }
        public bool SystemCalculated { get; set; }
        public string Remarks { get; set; }
        public int? LoanId { get; set; }
        //public bool AbsentDeduction { get; set; }
        public bool Add { get; set; }
        public bool Edit { get; set; }
        public bool Delete { get; set; }
        #endregion

        #region constructor
        public WIPSalaryDetail()
        {
            DetailId = -1;
            Add = false; Edit = false; Delete = false;
        }
        public WIPSalaryDetail(int _detailId, short _headId, double _amount, object _loanId, //short _loanId,  bool _absentDeduction,
            bool _sysCalculated, string _remarks)
        {
            DetailId = _detailId;
            HeadId = _headId;
            Amount = _amount;
            if (_loanId != DBNull.Value)
                LoanId = Convert.ToInt32(_loanId);
            //LoanId = _loanId;
            //AbsentDeduction = _absentDeduction;
            Remarks = _remarks;
            SystemCalculated = _sysCalculated;
            Add = false; Edit = false; Delete = false;
        }
        #endregion

        #region internal methods
        internal static List<WIPSalaryDetail> Get(int _salaryId)
        {
            try
            {
                List<WIPSalaryDetail> lstSD = new List<WIPSalaryDetail>();
                using (DbCommand dbCommand = db.GetStoredProcCommand("GetWIPSalaryDetailById"))
                {
                    db.AddInParameter(dbCommand, "SalaryId", SqlDbType.Int, _salaryId);
                    using (DataSet ds = db.ExecuteDataSet(dbCommand))
                    {
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                lstSD.Add(new WIPSalaryDetail(
                                    Convert.ToInt32(dr["DetailId"]),
                                    Convert.ToInt16(dr["HeadId"]),
                                    Convert.ToDouble(dr["Amount"]),
                                    dr["LoanId"],
                          //          Convert.ToInt16(dr["LoanId"]),
                            //        Convert.ToBoolean(dr["AbsentDeduction"]),
                                    Convert.ToBoolean(dr["SystemCalculated"]),
                                    dr["Remarks"].ToString()));
                            }
                        }
                    }
                }
                return lstSD;
            }
            catch (Exception)
            { throw; }
        }

        internal static bool Save(int _salaryId, List<WIPSalaryDetail> _details, DbTransaction _transaction)
        {
            try
            {
                foreach (WIPSalaryDetail _sd in getChanges(_details))
                {
                    using (DbCommand dbCommand = db.GetStoredProcCommand("SaveWIPSalaryDetail"))
                    {
                        dbCommand.Transaction = _transaction;
                        db.AddInParameter(dbCommand, "DetailId", SqlDbType.Int, _sd.DetailId);
                        db.AddInParameter(dbCommand, "SalaryId", SqlDbType.Int, _salaryId);
                        db.AddInParameter(dbCommand, "HeadId", SqlDbType.TinyInt, _sd.HeadId);
                        db.AddInParameter(dbCommand, "Amount", SqlDbType.Float, _sd.Amount);
                        db.AddInParameter(dbCommand, "SystemCalculated", SqlDbType.Bit, _sd.SystemCalculated);
                        db.AddInParameter(dbCommand, "Remarks", SqlDbType.VarChar, _sd.Remarks);
                        //db.AddInParameter(dbCommand, "LoanId", SqlDbType.TinyInt, _sd.LoanId);
                        db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                        db.AddInParameter(dbCommand, "Action", SqlDbType.Char, (
                               _sd.Add ? "I" : (_sd.Delete ? "D" : "U")));
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
        private static IEnumerable<WIPSalaryDetail> getChanges(List<WIPSalaryDetail> _details)
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