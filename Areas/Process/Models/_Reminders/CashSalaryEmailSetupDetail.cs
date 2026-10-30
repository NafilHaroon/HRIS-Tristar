using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace HRIS.Areas.Process.Models
{
    public class EmailSetupDetail : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public string BranchCode { get; set; }
        public short? CompanyId{ get; set; }
        public string ContactName { get; set; }
        public string EmailTo { get; set; }
        public string EmailCC { get; set; }
        public string EmailBCC { get; set; }
        public short DetailId { get; set; }
        public bool Add { get; set; }
        public bool Edit { get; set; }
        public bool Delete { get; set; }
        #endregion

        #region constructor
        public EmailSetupDetail()
        {
            DetailId = -1;
            Add = true; Edit = false; Delete = false;
        }

        public EmailSetupDetail(string _BranchCode, short  _CompanyId, string _ContactName, string _EmailTo,
            string _EmailCC, string _EmailBCC, short _DetailId)
        {
            BranchCode = _BranchCode;
            CompanyId = _CompanyId;
            ContactName = _ContactName;
            EmailTo = _EmailTo;
            EmailCC = _EmailCC;
            EmailBCC = _EmailBCC;
            DetailId = _DetailId;
            Add = false; Edit = false; Delete = false;

        }
        #endregion

        #region prviate methods
        internal static List<EmailSetupDetail> GetCashSalary()
        {
            List<EmailSetupDetail> lstESD = new List<EmailSetupDetail>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("getCashSalaryEmail"))
            {
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            lstESD.Add(new EmailSetupDetail(
                                dr["BranchCode"].ToString(),
                                Convert.ToInt16(dr["CompanyId"]),
                                dr["ContactName"].ToString(),
                                dr["EmailTo"].ToString(),
                                dr["EmailCC"].ToString(),
                                dr["EmailBCC"].ToString(),
                                Convert.ToInt16(dr["DetailId"])));
                        }
                    }
                }
            }
            return lstESD;
        }

        internal static List<EmailSetupDetail> GetProbationReminder()
        {
            List<EmailSetupDetail> lstESD = new List<EmailSetupDetail>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("getProbationReminderEmail"))
            {
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            lstESD.Add(new EmailSetupDetail(
                                dr["BranchCode"].ToString(),
                                Convert.ToInt16(dr["CompanyId"]),
                                dr["ContactName"].ToString(),
                                dr["EmailTo"].ToString(),
                                dr["EmailCC"].ToString(),
                                dr["EmailBCC"].ToString(),
                                Convert.ToInt16(dr["DetailId"])));
                        }
                    }
                }
            }
            return lstESD;
        }

        internal static bool SaveCashSalary(List<EmailSetupDetail> _details, DbTransaction _transaction)
        {
            try
            {
                foreach (EmailSetupDetail _esd in getCDDetailChanges(_details))
                {
                    using (DbCommand dbCommand = db.GetStoredProcCommand("SaveCashSalaryEmail"))
                    {
                        db.AddInParameter(dbCommand, "DetailId", SqlDbType.Int, _esd.DetailId);
                        db.AddInParameter(dbCommand, "BranchCode", SqlDbType.VarChar, _esd.BranchCode);
                        db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, _esd.CompanyId);
                        db.AddInParameter(dbCommand, "ContactName", SqlDbType.VarChar, _esd.ContactName);
                        db.AddInParameter(dbCommand, "EmailTo", SqlDbType.VarChar, _esd.EmailTo);
                        db.AddInParameter(dbCommand, "EmailCC", SqlDbType.VarChar, _esd.EmailCC);
                        db.AddInParameter(dbCommand, "EmailBCC", SqlDbType.VarChar, _esd.EmailBCC);
                        db.AddInParameter(dbCommand, "Action", SqlDbType.Char, (
                          _esd.Add ? "I" : (_esd.Delete ? "D" : "U")));
                        db.ExecuteNonQuery(dbCommand, _transaction);
                    }
                }
                return true;
            }
            catch (Exception)
            { throw; }
        }

        internal static bool SaveProbationReminder(List<EmailSetupDetail> _details, DbTransaction _transaction)
        {
            try
            {
                foreach (EmailSetupDetail _esd in getCDDetailChanges(_details))
                {
                    using (DbCommand dbCommand = db.GetStoredProcCommand("SaveProbationReminderEmail"))
                    {
                        db.AddInParameter(dbCommand, "DetailId", SqlDbType.Int, _esd.DetailId);
                        db.AddInParameter(dbCommand, "BranchCode", SqlDbType.VarChar, _esd.BranchCode);
                        db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, _esd.CompanyId);
                        db.AddInParameter(dbCommand, "ContactName", SqlDbType.VarChar, _esd.ContactName);
                        db.AddInParameter(dbCommand, "EmailTo", SqlDbType.VarChar, _esd.EmailTo);
                        db.AddInParameter(dbCommand, "EmailCC", SqlDbType.VarChar, _esd.EmailCC);
                        db.AddInParameter(dbCommand, "EmailBCC", SqlDbType.VarChar, _esd.EmailBCC);
                        db.AddInParameter(dbCommand, "Action", SqlDbType.Char, (
                         _esd.Add ? "I" : (_esd.Delete ? "D" : "U")));
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
        private static IEnumerable<EmailSetupDetail> getCDDetailChanges(List<EmailSetupDetail> _details)
        {
            return (_details.Where(x => (x.Add && !x.Delete) || (x.Edit && !x.Delete) || x.Delete));
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