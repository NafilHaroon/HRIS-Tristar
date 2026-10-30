using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class EmpFamily : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public int DetailId { get; set; } = 1;
        public short? RelationId { get; set; }
        public string MemberName { get; set; }
        public DateTime? DoB { get; set; } = DateTime.Now.AddYears(-18);
        public bool EmergencyContact { get; set; }
        public bool MedicalCovered { get; set; }
        public string ContactNo { get; set; }
        public bool Add { get; set; } = true;
        public bool Edit { get; set; } = false;
        public bool Delete { get; set; } = false;
        #endregion

        #region constructor
        public EmpFamily()
        {
            //DetailId = 1;
            //DoB = DateTime.Now.AddYears(-18);
            //Add = true; Edit = false; Delete = false;
        }
        //public EmpFamily(int _detailId, short _relationId, string _memberName, DateTime _doB, 
        //    bool _emergencyContact, bool _medicalCovered, string _contactNo)
        //{
        //    DetailId = _detailId;
        //    RelationId = _relationId;
        //    MemberName = _memberName;
        //    DoB = _doB;
        //    EmergencyContact = _emergencyContact;
        //    MedicalCovered = _medicalCovered;
        //    ContactNo = _contactNo;
        //    Add = false; Edit = false; Delete = false;
        //}
        #endregion

        #region internal methods
        internal static List<EmpFamily> Get(short _employeeId)
        {
            try
            {
                List<EmpFamily> family = new List<EmpFamily>();
                using (DbCommand dbCommand = db.GetStoredProcCommand("getEmployeeFamilyById"))
                {
                    db.AddInParameter(dbCommand, "EmployeeId", SqlDbType.SmallInt, _employeeId);
                    using (DataSet ds = db.ExecuteDataSet(dbCommand))
                    {
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                family.Add(new EmpFamily
                                {
                                    DetailId = Convert.ToInt32(dr["DetailId"]),
                                    RelationId = Convert.ToInt16(dr["RelationId"]),
                                    MemberName = dr["MemberName"].ToString(),
                                    DoB = Convert.ToDateTime(dr["BirthDate"]),
                                    EmergencyContact = Convert.ToBoolean(dr["EmergencyContact"]),
                                    MedicalCovered = Convert.ToBoolean(dr["MedicalCovered"]),
                                    ContactNo = dr["ContactNo"].ToString(),
                                    Add = false
                                });
                            }
                        }
                    }                  
                }
                return family;
            }
            catch (Exception)
            { throw; }
        }

        internal static bool Save(short _empId, List<EmpFamily> _details, string _userId, DbTransaction _transaction)
        {
            try
            {
                foreach (EmpFamily _efm in Helper.AgilityHelper.GetChanges(_details))
                {
                    using (DbCommand dbCommand = db.GetStoredProcCommand("SaveEmployeeFamily"))
                    {
                        db.AddInParameter(dbCommand, "DetailId", SqlDbType.Int, _efm.DetailId);
                        db.AddInParameter(dbCommand, "EmployeeId", SqlDbType.SmallInt, _empId);
                        db.AddInParameter(dbCommand, "RelationId", SqlDbType.TinyInt, _efm.RelationId);
                        db.AddInParameter(dbCommand, "MemberName", SqlDbType.VarChar, _efm.MemberName);
                        db.AddInParameter(dbCommand, "BirthDate", SqlDbType.SmallDateTime, _efm.DoB);
                        db.AddInParameter(dbCommand, "EmergencyContact", SqlDbType.Bit, _efm.EmergencyContact);
                        db.AddInParameter(dbCommand, "MedicalCovered", SqlDbType.Bit, _efm.MedicalCovered);
                        db.AddInParameter(dbCommand, "ContactNo", SqlDbType.VarChar, _efm.ContactNo);
                        db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, _userId);
                        db.AddInParameter(dbCommand, "Action", SqlDbType.Char, (
                            _efm.Add ? "I" : (_efm.Delete ? "D" : "U")));
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