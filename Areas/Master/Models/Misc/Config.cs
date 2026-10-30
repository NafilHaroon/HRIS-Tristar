using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class Config : IDisposable
    {
        #region private properties
        private static readonly SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short? LoanDeductionId { get; set; }
        public short? AdvanceHeadId { get; set; }
        public short? AbsentHeadId { get; set; }
        public short? PFHeadId { get; set; }
        public short? BasicSalaryHeadId { get; set; }
        //public short PeriodId { get; set; }
        //public string PeriodName { get; set; }
        public string EmailText { get; set; }
        //public string CurrentMonth { get; set; }
        //public int CurrentYear { get; set; }
        #endregion

        #region Constructor
        public Config()
        {
 
        }
        #endregion

        #region internal methods
        internal static Config Get()
        {
            try
            {
                using (DbCommand dbCommand = db.GetStoredProcCommand("GetConfiguration"))
                {
                    db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, ServerSession.GetCurrentCompany());
                    using (DataSet ds = db.ExecuteDataSet(dbCommand))
                    {
                        if (ds != null && ds.Tables[0].Rows.Count > 0)
                        {
                            using (Config _cfg = new Config())
                            {
                                DataRow dr = ds.Tables[0].Rows[0];
                                //_cfg.PeriodId = Convert.ToInt16(dr["PeriodId"]);
                                //_cfg.PeriodName = dr["PeriodName"].ToString();
                                _cfg.AbsentHeadId = Convert.ToInt16(dr["AbsentId"]);
                                _cfg.BasicSalaryHeadId = Convert.ToInt16(dr["BasicSalaryId"]);
                                _cfg.PFHeadId = Convert.ToInt16(dr["PFId"]);
                                _cfg.AdvanceHeadId = Convert.ToInt16(dr["AdvanceId"]);
                                _cfg.LoanDeductionId = Convert.ToInt16(dr["LoanHeadId"]);
                                _cfg.EmailText = dr["EmailText"].ToString();
                                return _cfg;
                            }
                        }
                        else
                            return null;
                    }
                }
            }
            catch (Exception)
            { throw; }
        }

        internal static bool Save(Config _cfg)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("SaveConfiguration"))
            {
                try
                {
                    db.AddInParameter(dbCommand, "LoanHeadId", SqlDbType.TinyInt, _cfg.LoanDeductionId);
                    db.AddInParameter(dbCommand, "AdvanceHeadId", SqlDbType.TinyInt, _cfg.AdvanceHeadId);
                    db.AddInParameter(dbCommand, "AbsentHeadId", SqlDbType.TinyInt, _cfg.AbsentHeadId);
                    db.AddInParameter(dbCommand, "EmailText", SqlDbType.VarChar, _cfg.EmailText);  
                    db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                    db.ExecuteNonQuery(dbCommand);
                    return true;
                }
                catch (Exception)
                { throw; }
            }
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            // no implementation
        }
        #endregion
    }
}