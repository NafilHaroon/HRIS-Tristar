using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace HRIS.Areas.Master.Models
{
    public class CalendarDetail : IDisposable
    {
        #region private properties
        private static readonly SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties        
        public DateTime CalendarDate { get; set; }
        public string WeekDay { get; set; }
        public bool Holiday { get; set; }
        public string Reason { get; set; }
        public bool Edit { get; set; }
        #endregion

        #region constructor
        public CalendarDetail()
        {
            
        }

        public CalendarDetail(DateTime _calendarDate, bool _holiday, string _reason)
        {           
            CalendarDate = _calendarDate;
            WeekDay = _calendarDate.ToString("ddd");
            Holiday = _holiday;
            Reason = _reason;
            Edit = false;
        }
        #endregion

        #region internal methods
        internal static List<CalendarDetail> Get(short _periodId)
        {
            try
            {
                List<CalendarDetail> adjList = new List<CalendarDetail>();
                using (DbCommand dbCommand = db.GetStoredProcCommand("GetCalendarByPeriodId"))
                {
                    db.AddInParameter(dbCommand, "PeriodId", SqlDbType.SmallInt, _periodId);
                    using (DataSet ds = db.ExecuteDataSet(dbCommand))
                    {
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                adjList.Add(new CalendarDetail(
                                    Convert.ToDateTime(dr["CalendarDate"]),
                                    Convert.ToBoolean(dr["Holiday"]),
                                     dr["Reason"].ToString()));
                            }
                        }
                    }
                }
                return adjList;
            }
            catch (Exception)
            { throw; }
        }

        internal static bool Save(List<CalendarDetail> _details, DbTransaction _transaction)
        {
            try
            {
                foreach (CalendarDetail _cdd in getCDDetailChanges(_details))
                {
                    using (DbCommand dbCommand = db.GetStoredProcCommand("SaveCalendar"))
                    {
                        db.AddInParameter(dbCommand, "CalendarDate", SqlDbType.DateTime, _cdd.CalendarDate);
                        db.AddInParameter(dbCommand, "Reason", SqlDbType.VarChar, _cdd.Reason);
                        db.AddInParameter(dbCommand, "Holiday", SqlDbType.Bit, _cdd.Holiday);
                        db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
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
        private static IEnumerable<CalendarDetail> getCDDetailChanges(List<CalendarDetail> _details)
        {
            return (_details.Where(x => x.Edit));
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