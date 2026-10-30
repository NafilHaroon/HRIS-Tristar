using HRIS.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class Calendar : IDisposable
    {
        #region private properties
        private static readonly SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short? PeriodId { get; set; }
        public List<CalendarDetail> Details { get; set; }
        #endregion

        #region constructor
        public Calendar()
        {
            Details = new List<CalendarDetail>();
        }
          
        #endregion

        #region internal methods
        internal static Calendar Get(short _periodid)
        {
            try
            {
                using (Calendar _mc = new Calendar())
                {
                    _mc.PeriodId = _periodid;                   
                    _mc.Details = CalendarDetail.Get(_periodid);
                    return _mc;
                }
            }
            catch (Exception)
            { throw; }
        }

        internal static bool Save(Calendar _mc)
        {
            using (DbConnection dbconnection = db.CreateConnection())
            {
                dbconnection.Open();
                DbTransaction transaction = dbconnection.BeginTransaction();
                try
                {
                    CalendarDetail.Save( _mc.Details, transaction);
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