using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class EmpCostAllocation : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public int DetailId { get; set; } = 1;
        public short? TemplateId { get; set; }
        public string BranchCode { get; set; }
        public double Rate { get; set; }
        public bool Add { get; set; } = true;
        public bool Edit { get; set; } = false;
        public bool Delete { get; set; } = false;
        #endregion

        #region constructor
        public EmpCostAllocation()
        {
            //DetailId = 1;
            //Add = true; Edit = false; Delete = false;
        }
        //public EmpCostAllocation(int _detailId, short _templateId, string _branchCode, double _rate)//,string _itemState = "UnChanged")
        //{
        //    DetailId = _detailId;
        //    TemplateId = _templateId;
        //    BranchCode = _branchCode;
        //    Rate = _rate;
        //    Add = false; Edit = false; Delete = false;
        //}
        #endregion

        #region internal methods
        internal static List<EmpCostAllocation> Get(short _employeeId)
        {
            try
            {
                List<EmpCostAllocation> lstECA = new List<EmpCostAllocation>();
                using (DbCommand dbCommand = db.GetStoredProcCommand("getEmployeeCostAllocationById"))
                {
                    db.AddInParameter(dbCommand, "EmployeeId", SqlDbType.SmallInt, _employeeId);
                    using (DataSet ds = db.ExecuteDataSet(dbCommand))
                    {
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                lstECA.Add(new EmpCostAllocation
                                {
                                    DetailId = Convert.ToInt32(dr["DetailId"]),
                                    TemplateId = Convert.ToInt16(dr["TemplateId"]),
                                    BranchCode = dr["BranchCode"].ToString(),
                                    Rate = Convert.ToDouble(dr["Rate"]),
                                    Add = false
                                });
                            }
                        }
                    }
                }
                return lstECA;
            }
            catch (Exception)
            { throw; }
        }

        internal static bool Save(short _empId, List<EmpCostAllocation> _details, string _userId, DbTransaction _transaction)
        {
            try
            {
                foreach (EmpCostAllocation _eca in Helper.AgilityHelper.GetChanges(_details))
                {
                    using (DbCommand dbCommand = db.GetStoredProcCommand("SaveEmployeeCostAllocation"))
                    {
                        db.AddInParameter(dbCommand, "DetailId", SqlDbType.Int, _eca.DetailId);
                        db.AddInParameter(dbCommand, "EmployeeId", SqlDbType.SmallInt, _empId);
                        db.AddInParameter(dbCommand, "BranchCode", SqlDbType.VarChar, _eca.BranchCode);
                        db.AddInParameter(dbCommand, "TemplateId", SqlDbType.TinyInt, _eca.TemplateId);
                        db.AddInParameter(dbCommand, "Rate", SqlDbType.Float, _eca.Rate);
                        db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, _userId);
                        db.AddInParameter(dbCommand, "Action", SqlDbType.Char, (
                            _eca.Add ? "I" : (_eca.Delete ? "D" : "U")));
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