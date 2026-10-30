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
    public class EmployeeLetterSalary : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public int DetailId { get; set; }
        public short? HeadId { get; set; }
        public string HeadName { get; set; }
        public double Amount { get; set; }
        public bool Add { get; set; }
        public bool Edit { get; set; }
        public bool Delete { get; set; }
        #endregion

        #region constructor
        public EmployeeLetterSalary()
        {
            DetailId = -1;
            Add = true; Edit = false; Delete = false;
        }

        public EmployeeLetterSalary(int _detailId, short _headId, string _headName, double _amount, bool _add = false)
        {
            DetailId = _detailId;
            HeadId = _headId;
            HeadName = _headName;
            Amount = _amount;
            Add = _add; Edit = false; Delete = false;
        }
        #endregion

        #region internal methods
        internal static List<EmployeeLetterSalary> Get(short _empId)
        {
            try
            {
                List<EmployeeLetterSalary> lstSalary = new List<EmployeeLetterSalary>();
                using (DbCommand dbCommand = db.GetStoredProcCommand("GetEmployeeSalaryById"))
                {
                    db.AddInParameter(dbCommand, "EmployeeId", SqlDbType.SmallInt, _empId);
                    db.AddInParameter(dbCommand, "IncomeOnly", SqlDbType.Bit, true);
                    db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                    using (DataSet ds = db.ExecuteDataSet(dbCommand))
                    {
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                lstSalary.Add(new EmployeeLetterSalary(
                                    Convert.ToInt32(dr["DetailId"]),
                                    Convert.ToInt16(dr["HeadId"]),
                                    dr["HeadName"].ToString(),
                                    Convert.ToDouble(dr["Amount"]),
                                    true));
                            }
                        }
                    }
                }
                return lstSalary;
            }
            catch (Exception)
            { throw; }
        }

        internal static List<EmployeeLetterSalary> GetForLetter(int _letterId)
        {
            try
            {
                List<EmployeeLetterSalary> lstSalary = new List<EmployeeLetterSalary>();
                using (DbCommand dbCommand = db.GetStoredProcCommand("GetEmployeeLetterSalaryById"))
                {
                    db.AddInParameter(dbCommand, "LetterId", SqlDbType.SmallInt, _letterId);
                    db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                    using (DataSet ds = db.ExecuteDataSet(dbCommand))
                    {
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                lstSalary.Add(new EmployeeLetterSalary(
                                    Convert.ToInt32(dr["DetailId"]),
                                    Convert.ToInt16(dr["HeadId"]),
                                    dr["HeadName"].ToString(),
                                    Convert.ToDouble(dr["Amount"])));
                            }
                        }
                    }
                }
                return lstSalary;
            }
            catch (Exception)
            { throw; }
        }

        internal static bool Save(int _letterId, List<EmployeeLetterSalary> _details, DbTransaction _transaction)
        {
            try
            {
                foreach (EmployeeLetterSalary _es in getChanges(_details))
                {
                    using (DbCommand dbCommand = db.GetStoredProcCommand("SaveEmployeeLetterSalary"))
                    {
                        db.AddInParameter(dbCommand, "DetailId", SqlDbType.Int, _es.DetailId);
                        db.AddInParameter(dbCommand, "LetterId", SqlDbType.SmallInt, _letterId);
                        db.AddInParameter(dbCommand, "HeadId", SqlDbType.TinyInt, _es.HeadId);
                        db.AddInParameter(dbCommand, "Amount", SqlDbType.Float, _es.Amount);

                        db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
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

        #region private methods
        private static IEnumerable<EmployeeLetterSalary> getChanges(List<EmployeeLetterSalary> _details)//,List<Salary> _originalDetails)
        {
            return (_details.Where(x => x.Add || x.Edit || x.Delete));
        }
        #endregion

        #region dispose method
        public void Dispose()
        {
        }
        #endregion
    }
}