using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class AllocationDetail : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public int DetailId { get; set; } = -1;
        public short TemplateId { get; set; }
        public string  DepartmentCode { get; set; }
        [Range(0, 100)]
        public double Rate { get; set; } = 0;
        public bool Add { get; set; } = true;
        public bool Edit { get; set; } = false;
        public bool Delete { get; set; } = false;
        #endregion

        #region constructor
        public AllocationDetail()
        {
            //DetailId = -1;
            //Rate = 0;
            //Add = true; Edit = false; Delete = false;
        }
        //public AllocationDetail(int _detailId, string  _departmentCode, double _rate)
        //{
        //    DetailId = _detailId;
        //    DepartmentCode = _departmentCode;
        //    Rate = _rate;
        //    Add = false; Edit = false; Delete = false;
        //}
        #endregion

        #region internal methods
        internal static List<AllocationDetail> Get(short _templateid)
        {
            try
            {
                List<AllocationDetail> details = new List<AllocationDetail>();
                using (DbCommand dbCommand = db.GetStoredProcCommand("getAllocationTemplateDetailById"))
                {
                    db.AddInParameter(dbCommand, "TemplateId", SqlDbType.SmallInt, _templateid);
                    using (DataSet ds = db.ExecuteDataSet(dbCommand))
                    {
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                details.Add(new AllocationDetail
                                {
                                    DetailId = Convert.ToInt32(dr["DetailId"]),
                                    DepartmentCode = dr["DepartmentCode"].ToString(),
                                    Rate = Convert.ToDouble(dr["Rate"]),
                                    Add = false
                                });
                            }
                        }
                    }
                }
                return details;
            }
            catch (Exception)
            { throw; }
        }

        internal static bool Save(short _templateid, List<AllocationDetail> _details, DbTransaction _transaction)
        {
            try
            {
                foreach (AllocationDetail ATD in Helper.AgilityHelper.GetChanges(_details))
                {
                    using (DbCommand dbCommand = db.GetStoredProcCommand("SaveAllocationTemplateDetail"))
                    {
                        dbCommand.Transaction = _transaction;
                        db.AddInParameter(dbCommand, "DetailId", SqlDbType.Int, ATD.DetailId);
                        db.AddInParameter(dbCommand, "TemplateId", SqlDbType.SmallInt, _templateid);
                        db.AddInParameter(dbCommand, "DepartmentCode", SqlDbType.VarChar, ATD.DepartmentCode);
                        db.AddInParameter(dbCommand, "Rate", SqlDbType.Float, ATD.Rate);
                        db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                        db.AddInParameter(dbCommand, "Action", SqlDbType.Char, (
                           ATD.Add ? "I" : (ATD.Delete ? "D" : "U")));
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