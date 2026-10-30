using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class SalaryHeads : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short HeadId { get; set; }
        public string HeadName { get; set; }
        public string HeadTypeName { get; set; }
        public bool IsActive { get; set; }
        [Browsable(false)]
        public bool DeductForAbsent { get; set; }
        [Browsable(false)]
        public short Symbol { get; set; }
        [Browsable(false)]
        public short HeadTypeId { get; set; }
        [Browsable(false)]
        public bool SystemCalculated { get; set; }
        #endregion

        #region constructor
        public SalaryHeads()
        {
        }

        //public SalaryHeads(short _headId, string _headName, string _headTypeName, bool _isActive, 
        //    bool _deductForAbsent, short _symbol, short _headTypeId, bool _systemCalculated)
        //{
        //    HeadId = _headId;
        //    HeadName = _headName;
        //    HeadTypeName = _headTypeName;
        //    IsActive = _isActive;
        //    DeductForAbsent = _deductForAbsent;
        //    Symbol = _symbol;
        //    HeadTypeId = _headTypeId;
        //    SystemCalculated = _systemCalculated;
        //}
        #endregion

        #region internal methods
        internal static List<SalaryHeads> Get(bool _isActive, bool _incomeOnly = false)
        {
            List<SalaryHeads> heads = new List<SalaryHeads>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetSalaryHeads"))
            {
                db.AddInParameter(dbCommand, "ActiveOnly", SqlDbType.Bit, _isActive);
                db.AddInParameter(dbCommand, "IncomeOnly", SqlDbType.Bit, _incomeOnly);
                using (DataTable dt = db.ExecuteDataSet(dbCommand).Tables[0])
                {
                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            heads.Add(new SalaryHeads
                            {
                                HeadId = Convert.ToInt16(dr["HeadId"]),
                                HeadName = dr["HeadName"].ToString(),
                                HeadTypeName = dr["HeadTypeName"].ToString(),
                                IsActive = Convert.ToBoolean(dr["IsActive"]),
                                DeductForAbsent = Convert.ToBoolean(dr["DeductForAbsent"]),
                                Symbol = Convert.ToInt16(dr["Symbol"]),
                                HeadTypeId = Convert.ToInt16(dr["HeadTypeId"]),
                                SystemCalculated = Convert.ToBoolean(dr["SystemCalculated"])
                            });
                        }
                    }
                }
            }
            return heads;
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