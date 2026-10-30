using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class EmpEntitlement : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public int DetailId { get; set; } = -1;
        public short? EntitlementTypeId { get; set; }
        public string RegistrationNo { get; set; }
        public string Model { get; set; }
        public double BaseValue { get; set; }
        public double Limit { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? ReminderDate { get; set; }
        public DateTime? DisposalDate { get; set; }
        //public bool Consumable { get; set; }
        public bool Active { get; set; }
        public bool Add { get; set; } = true;
        public bool Edit { get; set; } = false;
        public bool Delete { get; set; } = false;
        //public string Status { get; set; }
        #endregion

        #region constructor
        public EmpEntitlement()
        {
            //DetailId = -1;
            //Add = true; Edit = false; Delete = false;
            //Status = "Added";
        }
        public EmpEntitlement(int _detailId, short _entitlementTypeId, string _registrationNo, string _model,
            double _baseValue, double _limit, object _issueDate, object _expiryDate,
            object _reminderDate, object _disposalDate, bool _active)//,string _itemState = "UnChanged")bool _consumable, 
        {
            DetailId = _detailId;
            EntitlementTypeId = _entitlementTypeId;
            RegistrationNo = _registrationNo;
            Model = _model;
            BaseValue = _baseValue;
            Limit = _limit;
            if (_issueDate != DBNull.Value)
                IssueDate = Convert.ToDateTime(_issueDate);
            if (_expiryDate != DBNull.Value)
                ExpiryDate = Convert.ToDateTime(_expiryDate);
            if (_reminderDate != DBNull.Value)
                ReminderDate = Convert.ToDateTime(_reminderDate);
            if (_disposalDate != DBNull.Value)
                DisposalDate = Convert.ToDateTime(_disposalDate);
            //Consumable = _consumable;
            Active = _active;
            Add = false; Edit = false; Delete = false;
            //Status = _itemState;
        }
        #endregion

        #region internal methods
        internal static List<EmpEntitlement> Get(short _empId)
        {
            try
            {
                List<EmpEntitlement> lstEA = new List<EmpEntitlement>();
                using (DbCommand dbCommand = db.GetStoredProcCommand("GetEmployeeEntitlementById"))
                {
                    db.AddInParameter(dbCommand, "EmployeeId", SqlDbType.SmallInt, _empId);
                    using (DataSet ds = db.ExecuteDataSet(dbCommand))
                    {
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                lstEA.Add(new EmpEntitlement(
                                    Convert.ToInt32(dr["DetailId"]),
                                    Convert.ToInt16(dr["EntitlementTypeId"]),
                                    dr["RegistrationNo"].ToString(),
                                    dr["Model"].ToString(),
                                    Convert.ToDouble(dr["BaseValue"]),
                                    Convert.ToDouble(dr["Limit"]),
                                    dr["IssueDate"],
                                    dr["ExpiryDate"],
                                    dr["ReminderDate"],
                                    dr["DisposalDate"],
                                    //Convert.ToBoolean(dr["Consumable"]),
                                    Convert.ToBoolean(dr["Active"])));
                            }
                        }
                    }
                }
                return lstEA;
            }
            catch (Exception)
            { throw; }
        }

        internal static bool Save(short _empId, List<EmpEntitlement> _details, string _userId, DbTransaction _transaction)
        {
            try
            {
                foreach (EmpEntitlement _ea in Helper.AgilityHelper.GetChanges(_details))
                {
                    using (DbCommand dbCommand = db.GetStoredProcCommand("SaveEmployeeEntitlement"))
                    {
                        db.AddInParameter(dbCommand, "DetailId", SqlDbType.Int, _ea.DetailId);
                        db.AddInParameter(dbCommand, "EmployeeId", SqlDbType.SmallInt, _empId);
                        db.AddInParameter(dbCommand, "EntitlementTypeId", SqlDbType.TinyInt, _ea.EntitlementTypeId);
                        db.AddInParameter(dbCommand, "RegistrationNo", SqlDbType.VarChar, _ea.RegistrationNo);
                        db.AddInParameter(dbCommand, "IssueDate", SqlDbType.SmallDateTime, _ea.IssueDate);
                        db.AddInParameter(dbCommand, "Model", SqlDbType.VarChar, _ea.Model);
                        db.AddInParameter(dbCommand, "BaseValue", SqlDbType.Float, _ea.BaseValue);
                        db.AddInParameter(dbCommand, "Limit", SqlDbType.Float, _ea.Limit);
                        db.AddInParameter(dbCommand, "ExpiryDate", SqlDbType.SmallDateTime, _ea.ExpiryDate);
                        db.AddInParameter(dbCommand, "ReminderDate", SqlDbType.SmallDateTime, _ea.ReminderDate);
                        db.AddInParameter(dbCommand, "DisposalDate", SqlDbType.SmallDateTime, _ea.DisposalDate);
                        db.AddInParameter(dbCommand, "Active", SqlDbType.Bit, _ea.Active);
                        //db.AddInParameter(dbCommand, "Consumable", SqlDbType.Bit, _ea.Consumable);
                        db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, _userId);
                        db.AddInParameter(dbCommand, "Action", SqlDbType.Char, (
                            _ea.Delete ? "D" : (_ea.Add ? "I" : "U")));
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