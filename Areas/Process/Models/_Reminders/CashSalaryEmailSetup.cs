using HRIS.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace HRIS.Areas.Process.Models
{
    public class EmailSetup: IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public List<EmailSetupDetail> Details { get; set; }
        #endregion

        #region constructor
        public EmailSetup()
        {
            Details = new List<EmailSetupDetail>();
        }


        #endregion


        #region internal methods

        internal static EmailSetup GetCSE()
        {
            try
            {
                using (EmailSetup _cd = new EmailSetup())
                {
                    _cd.Details = EmailSetupDetail.GetCashSalary();
                    return _cd;
                }
            }
            catch (Exception)
            { throw; }
        }

        internal static bool SaveCSE(EmailSetup _cd)
        {
            using (DbConnection dbconnection = db.CreateConnection())
            {
                dbconnection.Open();
                DbTransaction transaction = dbconnection.BeginTransaction();
                try
                {
                    EmailSetupDetail.SaveCashSalary(_cd.Details, transaction);
                    transaction.Commit();
                    return true;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }


        internal static EmailSetup GetPRE()
        {
            try
            {
                using (EmailSetup _cd = new EmailSetup())
                {
                    _cd.Details = EmailSetupDetail.GetProbationReminder();
                    return _cd;
                }
            }
            catch (Exception)
            { throw; }
        }

        internal static bool SavePRE(EmailSetup _cd)
        {
            using (DbConnection dbconnection = db.CreateConnection())
            {
                dbconnection.Open();
                DbTransaction transaction = dbconnection.BeginTransaction();
                try
                {
                    EmailSetupDetail.SaveProbationReminder(_cd.Details, transaction);
                    transaction.Commit();
                    return true;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
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