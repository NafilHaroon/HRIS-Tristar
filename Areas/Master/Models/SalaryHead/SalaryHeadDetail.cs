using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class SalaryHeadDetail : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public int DetailId { get; set; } = -1;
        public short HeadId { get; set; }
        public short? WorkerTypeId { get; set; }
        public string DebitAccount { get; set; }
        public string CreditAccount { get; set; }
        public bool Add { get; set; } = true;
        public bool Edit { get; set; } = false;
        public bool Delete { get; set; } = false;
        #endregion

        #region constructor
        public SalaryHeadDetail()
        {
            //DetailId = -1;
            //Add = true; Edit = false; Delete = false;
        }
        //public SalaryHeadDetail(int _detailId, short _workerTypeId, string _debitAccount, string _creditAccount)
        //{
        //    DetailId = _detailId;
        //    WorkerTypeId = _workerTypeId;
        //    DebitAccount = _debitAccount;
        //    CreditAccount = _creditAccount;
        //    Add = false; Edit = false; Delete = false;
        //}
        #endregion

        #region internal methods
        internal static List<SalaryHeadDetail> Get(short _headId)
        {
            try
            {
                List<SalaryHeadDetail> details = new List<SalaryHeadDetail>();
                using (DbCommand dbCommand = db.GetStoredProcCommand("GetSalaryHeadMappingById"))
                {
                    db.AddInParameter(dbCommand, "HeadId", SqlDbType.SmallInt, _headId);
                    db.AddInParameter(dbCommand, "CompanyId", SqlDbType.SmallInt, ServerSession.GetCurrentCompany());
                    using (DataSet ds = db.ExecuteDataSet(dbCommand))
                    {
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                details.Add(new SalaryHeadDetail
                                {
                                    DetailId = Convert.ToInt32(dr["DetailId"]),
                                    WorkerTypeId = Convert.ToInt16(dr["WorkerTypeId"]),
                                    DebitAccount = dr["DebitAccount"].ToString(),
                                    CreditAccount = dr["CreditAccount"].ToString(),
                                    Add = false
                                });
                            }
                        }
                    }                  
                }
                return details;
            }
            catch (Exception)
            { throw; }
        }

        internal static bool Save(short _headId, List<SalaryHeadDetail> _details, DbTransaction _transaction)
        {
            try
            {
                foreach (SalaryHeadDetail _shd in Helper.AgilityHelper.GetChanges(_details))
                {
                    using (DbCommand dbCommand = db.GetStoredProcCommand("SaveSalaryHeadMapping"))
                    {
                        dbCommand.Transaction = _transaction;
                        db.AddInParameter(dbCommand, "DetailId", SqlDbType.Int, _shd.DetailId);
                        db.AddInParameter(dbCommand, "HeadId", SqlDbType.TinyInt, _headId);
                        db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, ServerSession.GetCurrentCompany());
                        db.AddInParameter(dbCommand, "WorkerTypeId", SqlDbType.SmallInt, _shd.WorkerTypeId);
                        db.AddInParameter(dbCommand, "DebitAccount", SqlDbType.VarChar, _shd.DebitAccount);
                        db.AddInParameter(dbCommand, "CreditAccount", SqlDbType.VarChar, _shd.CreditAccount);
                        db.AddInParameter(dbCommand, "Userid", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                        db.AddInParameter(dbCommand, "Action", SqlDbType.Char, (
                           _shd.Add ? "I" : (_shd.Delete ? "D" : "U")));
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