using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace HRIS.Areas.Master.Models
{
    public class EmpSalary : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public int DetailId { get; set; } = -1;
        public short? HeadId { get; set; }
        public double Amount { get; set; }
        [Browsable(false)]
        public bool SystemCalculated { get; set; } = false;
        public bool Add { get; set; } = true;
        public bool Edit { get; set; } = false;
        public bool Delete { get; set; } = false;
        #endregion

        #region constructor
        public EmpSalary()
        {
            //DetailId = -1;
            //SystemCalculated = false;
            //Add = true; Edit = false; Delete = false;
        }
        //public EmpSalary(int _detailId, short _headId, double _amount, bool _systemCalculated)
        //{
        //    DetailId = _detailId;
        //    HeadId = _headId;
        //    Amount = _amount;
        //    SystemCalculated = _systemCalculated;
        //    Add = false; Edit = false; Delete = false;
        //}
        #endregion

        #region internal methods
        internal static List<EmpSalary> Get(short _empId)
        {
            try
            {
                List<EmpSalary> salary = new List<EmpSalary>();
                using (DbCommand dbCommand = db.GetStoredProcCommand("GetEmployeeSalaryById"))
                {
                    db.AddInParameter(dbCommand, "EmployeeId", SqlDbType.SmallInt, _empId);
                    db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                    using (DataSet ds = db.ExecuteDataSet(dbCommand))
                    {
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                salary.Add(new EmpSalary
                                {
                                    DetailId = Convert.ToInt32(dr["DetailId"]),
                                    HeadId = Convert.ToInt16(dr["HeadId"]),
                                    Amount = Convert.ToDouble(dr["Amount"]),
                                    SystemCalculated = Convert.ToBoolean(dr["SystemCalculated"]),
                                    Add = false
                                });
                            }
                        }
                    }
                }
                return salary;
            }
            catch (Exception)
            { throw; }
        }

        internal static bool Save(short _empId, List<EmpSalary> _details, string _userId, DbTransaction _transaction)
        {
            try
            {
                foreach (EmpSalary _es in Helper.AgilityHelper.GetChanges(_details).Where(x => x.HeadId.HasValue || x.Delete))
                {
                    using (DbCommand dbCommand = db.GetStoredProcCommand("SaveEmployeeSalary"))
                    {
                        db.AddInParameter(dbCommand, "DetailId", SqlDbType.Int, _es.DetailId);
                        db.AddInParameter(dbCommand, "EmployeeId", SqlDbType.SmallInt, _empId);
                        db.AddInParameter(dbCommand, "HeadId", SqlDbType.TinyInt, _es.HeadId);
                        db.AddInParameter(dbCommand, "Amount", SqlDbType.Float, _es.Amount);

                        db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, _userId);
                        db.AddInParameter(dbCommand, "Action", SqlDbType.Char, (
                            _es.Add ? "I" : (_es.Delete ? "D" : "U")));
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