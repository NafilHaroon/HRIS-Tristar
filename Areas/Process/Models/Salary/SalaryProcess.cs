using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Process.Models
{
    public class SalaryProcess : IDisposable
    {
        #region private properties
        private static readonly SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        //#region public properties       
        //public string CurrentMonth { get; set; }
        //public int CurrentYear { get; set; }
        //public bool GenerateOnlyNew { get; set; }
        //#endregion

        #region constructor
        public SalaryProcess()
        {
        }
        #endregion

        #region internal methods
        //internal static SalaryProcess GetCurrentPeriod()
        //{
        //    try
        //    {
        //        using (DbCommand dbCommand = db.GetStoredProcCommand("GetCurrentPeriod"))
        //        {
        //            db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, ServerSession.GetCurrentCompany());
        //            using (DataSet ds = db.ExecuteDataSet(dbCommand))
        //            {
        //                if (ds != null && ds.Tables[0].Rows.Count > 0)
        //                {
        //                    using (SalaryProcess _sp = new SalaryProcess())
        //                    {
        //                        DataRow dr = ds.Tables[0].Rows[0];
        //                        _sp.CurrentMonth = dr["CurrentMonth"].ToString();
        //                        _sp.CurrentYear = Convert.ToInt32(dr["CurrentYear"]);
        //                        return _sp;
        //                    }
        //                }
        //                else
        //                    return null;
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    { throw; }
        //}

        internal static bool Generate(bool _overwrite)
        {
            try
            {
                using (DbCommand dbCommand = db.GetStoredProcCommand("GenerateSalary"))
                {
                    db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, ServerSession.GetCurrentCompany());
                    db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                    db.AddInParameter(dbCommand, "Overwrite", SqlDbType.Bit, _overwrite);
                    db.ExecuteNonQuery(dbCommand);
                    return true;
                }
            }
            catch (Exception ex)
            { throw ex; }
        }

        internal static bool Post()
        {
            try
            {
                using (DbCommand dbCommand = db.GetStoredProcCommand("PostSalary"))
                {
                    db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, ServerSession.GetCurrentCompany());
                    db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                    db.ExecuteNonQuery(dbCommand);
                    return true;
                }
            }
            catch (Exception)
            { throw; }
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
        }
        #endregion
    }
}