using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace HRIS.Areas.Master.Models
{
    public class _legacyEmpEmergencyContact : IDisposable, ICloneable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        [Display(Name = "Relation")]
        public short? RelationId { get; set; }
        [Display(Name = "NoK Name")]
        public string NoKName { get; set; }
        [Display(Name = "Contact #")]
        public string NoKContactNo { get; set; }
        public int DetailId { get; set; }
        public string Status { get; set; }


        #endregion

        #region constructor
        public _legacyEmpEmergencyContact()
        {
            DetailId = 1;
            Status = "Added";
        }
        public _legacyEmpEmergencyContact(int _detailId, short _RelationId, string _NoKName, string _NoKContactNo,
            string _itemState = "UnChanged")
        {

            DetailId = _detailId;
            RelationId = _RelationId;
            NoKName = _NoKName;
            NoKContactNo = _NoKContactNo;
            Status = _itemState;
        }
        #endregion

        #region internal methods
        internal static List<_legacyEmpEmergencyContact> Get(short _empId)
        {
            try
            {
                List<_legacyEmpEmergencyContact> lstEMD = new List<_legacyEmpEmergencyContact>();
                using (DbCommand dbCommand = db.GetStoredProcCommand("getEmployeeEmergencyContactDetailByCode"))
                {
                    db.AddInParameter(dbCommand, "EmployeeId", SqlDbType.SmallInt, _empId);

                    using (DataSet ds = db.ExecuteDataSet(dbCommand))
                    {
                        if (ds != null && ds.Tables.Count > 0)
                        {

                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                lstEMD.Add(new _legacyEmpEmergencyContact(
                                    Convert.ToInt32(dr["DetailId"]),
                                    Convert.ToInt16(dr["RelationId"]),
                                    dr["NoKName"].ToString(),
                                     dr["NoKContactNo"].ToString()));
                            }
                        }
                    }
                }
                return lstEMD;
            }
            catch (Exception)
            { throw; }
        }

        internal static bool Save(short _empId, List<_legacyEmpEmergencyContact> _details, List<_legacyEmpEmergencyContact> _originalDetails,
            string _userId, DbTransaction _transaction)
        {
            try
            {
                foreach (_legacyEmpEmergencyContact eld in getChanges(_details, _originalDetails))
                {
                    using (DbCommand dbCommand = db.GetStoredProcCommand("SaveEmployeeEmergencyContactDetail"))
                    {
                        dbCommand.Transaction = _transaction;
                        db.AddInParameter(dbCommand, "DetailId", SqlDbType.Int, eld.DetailId);
                        db.AddInParameter(dbCommand, "EmployeeId", SqlDbType.SmallInt, _empId);
                        db.AddInParameter(dbCommand, "RelationId", SqlDbType.TinyInt, eld.RelationId);
                        db.AddInParameter(dbCommand, "NoKName", SqlDbType.VarChar, eld.NoKName);
                        db.AddInParameter(dbCommand, "NoKContactNo", SqlDbType.VarChar, eld.NoKContactNo);

                        db.AddInParameter(dbCommand, "userid", SqlDbType.VarChar, _userId);
                        db.AddInParameter(dbCommand, "action", SqlDbType.Char, (
                            eld.Status == "Added" ? "I" : (
                            eld.Status == "Deleted" ? "D" : "U")));

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
        private static IEnumerable<_legacyEmpEmergencyContact> getChanges(List<_legacyEmpEmergencyContact> _details,
         List<_legacyEmpEmergencyContact> _originalDetails)
        {
            //changing all rows in original list to deleted so that mismatched rows are returned with status deleted
            _originalDetails.ForEach(o => o.Status = "Deleted");

            //extracting data through union query, first part look for items with status Added & Edited whereas second part returns
            //items that were part of initial list but not present in current list
            IEnumerable<_legacyEmpEmergencyContact> editedItems = (IEnumerable<_legacyEmpEmergencyContact>)
                    (from actualItem in _details
                     where (actualItem.Status == "Edited" ||
                     actualItem.Status == "Added")
                     select actualItem).Union(
                        from actualItem in _originalDetails
                        where !
                        (from newItem in _details select newItem.DetailId).Contains(actualItem.DetailId)
                        select actualItem);
            return editedItems.ToList<_legacyEmpEmergencyContact>();
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