using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class PaymentModes : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short PaymentModeId { get; set; }
        public string PaymentModeName { get; set; }
        #endregion

        #region constructor
        public PaymentModes()
        {
        }

        //public PaymentMode(short _paymentModeId, string _paymentModeName)
        //{
        //    PaymentModeId = _paymentModeId;
        //    PaymentModeName = _paymentModeName;

        //}
        #endregion

        #region internal methods
        /// <summary>
        /// retunrs list of payment modes
        /// </summary>
        /// <returns></returns>
        internal static List<PaymentModes> Get()
        {
            List<PaymentModes> modes = new List<PaymentModes>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetPaymentModes"))
            {
                using (DataTable dt = db.ExecuteDataSet(dbCommand).Tables[0])
                {
                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            modes.Add(new PaymentModes
                            {
                                PaymentModeId = Convert.ToInt16(dr["PaymentModeId"]),
                                PaymentModeName = dr["PaymentModeName"].ToString()
                            });
                        }
                    }
                }
            }
            return modes;
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