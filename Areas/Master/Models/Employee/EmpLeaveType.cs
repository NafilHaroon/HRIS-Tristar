using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace HRIS.Areas.Master.Models
{
    public class EmpLeaveType : IDisposable, ICloneable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public int DetailId { get; set; }
        public short? LeaveTypeId { get; set; }
        public short? AllowedDays { get; set; }
        public DateTime? DueDate { get; set; }
        public bool Add { get; set; }
        public bool Edit { get; set; }
        public bool Delete { get; set; }
        //public string Status { get; set; }
        #endregion

        #region constructor
        public EmpLeaveType()
        {
            DetailId = 1;
            Add = true; Edit = false; Delete = false;
            //Status = "Added";
        }

        public EmpLeaveType(int _detailId, short _leaveId, short _allowedDays, DateTime _dueDate)//, string _itemState = "UnChanged")
        {
            DetailId = _detailId;
            LeaveTypeId = _leaveId;
            AllowedDays = _allowedDays;
            DueDate = _dueDate;
            Add = false; Edit = false; Delete = false;
            //Status = _itemState;
        }
        #endregion

        #region internal methods
        internal static List<EmpLeaveType> Get(short _empId)
        {
            try
            {
                List<EmpLeaveType> lstLeave = new List<EmpLeaveType>();
                using (DbCommand dbCommand = db.GetStoredProcCommand("getEmployeeLeaveTypesById"))
                {
                    db.AddInParameter(dbCommand, "EmployeeId", SqlDbType.SmallInt, _empId);
                    using (DataSet ds = db.ExecuteDataSet(dbCommand))
                    {
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                lstLeave.Add(new EmpLeaveType(
                                    Convert.ToInt32(dr["DetailId"]),
                                    Convert.ToInt16(dr["LeaveTypeId"]),
                                    Convert.ToInt16(dr["AllowedDays"]),
                                    Convert.ToDateTime(dr["DueDate"])));
                            }
                        }
                    }
                }
                return lstLeave;
            }
            catch (Exception)
            { throw; }
        }

        internal static bool Save(short _empId, List<EmpLeaveType> _details, string _userId, DbTransaction _transaction)//, List<EmpLeave> _originalDetails
        {
            try
            {
                foreach (EmpLeaveType _el in getChanges(_details))//, _originalDetails))
                {
                    using (DbCommand dbCommand = db.GetStoredProcCommand("SaveEmployeeLeaveType"))
                    {
                        //dbCommand.Transaction = _transaction;
                        db.AddInParameter(dbCommand, "DetailId", SqlDbType.Int, _el.DetailId);
                        db.AddInParameter(dbCommand, "EmployeeId", SqlDbType.SmallInt, _empId);
                        db.AddInParameter(dbCommand, "LeaveTypeId", SqlDbType.TinyInt, _el.LeaveTypeId);
                        db.AddInParameter(dbCommand, "AllowedDays", SqlDbType.TinyInt, _el.AllowedDays);
                        db.AddInParameter(dbCommand, "DueDate", SqlDbType.SmallDateTime, _el.DueDate);
                        db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, _userId);
                        db.AddInParameter(dbCommand, "Action", SqlDbType.Char, (
                            _el.Add ? "I" : (_el.Delete ? "D" : "U")));
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
        private static IEnumerable<EmpLeaveType> getChanges(List<EmpLeaveType> _details)//,List<EmpLeave> _originalDetails)
        {
            return (_details.Where(x => x.Add || x.Edit || x.Delete));
            //changing all rows in original list to deleted so that mismatched rows are returned with status deleted
            //_originalDetails.ForEach(o => o.Status = "Deleted");

            ////extracting data through union query, first part look for items with status Added & Edited whereas second part returns
            ////items that were part of initial list but not present in current list
            //IEnumerable<EmpLeave> editedItems = (IEnumerable<EmpLeave>)
            //        (from actualItem in _details
            //         where (actualItem.Status == "Edited" ||
            //         actualItem.Status == "Added")
            //         select actualItem).Union(
            //            from actualItem in _originalDetails
            //            where !
            //            (from newItem in _details select newItem.DetailId).Contains(actualItem.DetailId)
            //            select actualItem);
            //return editedItems.ToList<EmpLeave>();
        }
        #endregion

        #region dispose method
        public void Dispose()
        {
        }
        #endregion

        #region ICloneable Members
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        #endregion
    }
}