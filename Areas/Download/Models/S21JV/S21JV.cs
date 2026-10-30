using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;

namespace HRIS.Areas.Download.Models
{
    public class S21JV : IDisposable
    {
        #region private properties
        //private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties    
        public short PeriodId { get; set; }
        public List<S21JVDetail> Details { get; set; }
        #endregion

        #region constructor
        public S21JV()
        {
            Details = new List<S21JVDetail>();
        }
        #endregion

        #region internal methods
        internal static S21JV Get(short _periodId)
        {
            try
            {
                using (S21JV _esy = new S21JV())
                {
                    _esy.Details = S21JVDetail.Get(_periodId);
                    return _esy;
                }
            }
            catch (Exception)
            { throw; }
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