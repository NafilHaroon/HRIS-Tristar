using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Data;
using System.Data.Common;
namespace HRIS.Areas.Master.Models
{
    public class EmpPersonalInfo : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short EmployeeId { get; set; }
        public short? GenderId { get; set; }
        public short? MaritalStatusId { get; set; }
        public short? QualificationId { get; set; }
        public string HusbandName { get; set; }
        public short? PaymentModeId { get; set; }
        public short? CompanyBankId { get; set; }
        public DateTime? BirthDate { get; set; } = DateTime.Now.AddYears(-18);
        public string CNICNo { get; set; }
        public string FamilyNo { get; set; }
        public string NTN { get; set; }
        public string Religion { get; set; }
        public string CurrentAddress { get; set; }
        public bool DifferentPermanentAdd { get; set; }
        public string PermanentAddress { get; set; }
        public string PhoneNo { get; set; }
        public string CellNo { get; set; }
        public string BankAccountNo { get; set; }
        public string BankDetail { get; set; }
        public string Nationality { get; set; }
        public bool IsSSMember { get; set; } = true;
        public string SESSINo { get; set; }
        public string EOBINo { get; set; }
        public string ESSNo { get; set; }
       
        public string PersonalEmail { get; set; }
        public DateTime? WorkingSince { get; set; }
        #endregion

        #region constructor
        public EmpPersonalInfo()
        {

        }
        #endregion

        #region internal methods
        internal static EmpPersonalInfo Get(short _empId)
        {
            try
            {
                using (DbCommand dbCommandDetail = db.GetStoredProcCommand("GetEmployeePersonalInfoById"))
                {
                    db.AddInParameter(dbCommandDetail, "EmployeeId", SqlDbType.SmallInt, _empId);
                    using (DataSet ds = db.ExecuteDataSet(dbCommandDetail))
                    {
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            EmpPersonalInfo _epi = new EmpPersonalInfo();
                            DataRow dr = ds.Tables[0].Rows[0];
                            _epi.EmployeeId = _empId;
                            _epi.BirthDate = Convert.ToDateTime(dr["BirthDate"]);
                            _epi.CNICNo = dr["CNICNo"].ToString();

                            _epi.NTN = dr["NTN"].ToString();
                            _epi.Religion = dr["Religion"].ToString();
                            _epi.CellNo = dr["CellNo"].ToString();
                            _epi.PhoneNo = dr["PhoneNo"].ToString();
                            _epi.GenderId = Convert.ToInt16(dr["GenderId"]);

                            _epi.MaritalStatusId = Convert.ToInt16(dr["MaritalStatusId"]);
                            _epi.HusbandName = dr["HusbandName"].ToString();
                            _epi.CurrentAddress = dr["CurrentAddress"].ToString();
                            _epi.DifferentPermanentAdd = Convert.ToBoolean(dr["DifferentPermanentAdd"]);
                            _epi.PermanentAddress = dr["PermanentAddress"].ToString();
                            _epi.QualificationId = Convert.ToInt16(dr["QualificationId"]);
                            _epi.WorkingSince = Convert.ToDateTime(dr["WorkingSince"]);
                            if (dr["PaymentModeId"] != DBNull.Value)
                                _epi.PaymentModeId = Convert.ToInt16(dr["PaymentModeId"]);

                            if (dr["CompanyBankId"] != DBNull.Value)
                                _epi.CompanyBankId = Convert.ToInt16(dr["CompanyBankId"]);
                            _epi.BankAccountNo = dr["BankAccountNo"].ToString();
                            _epi.BankDetail = dr["BankDetail"].ToString();
                            _epi.FamilyNo = dr["FamilyNo"].ToString();

                            _epi.Nationality = dr["Nationality"].ToString();
                            _epi.IsSSMember = Convert.ToBoolean(dr["SSMember"]);
                            _epi.SESSINo = dr["SESSINo"].ToString();
                            _epi.EOBINo = dr["EOBINo"].ToString();
                            _epi.ESSNo = dr["ESSNo"].ToString();                        
                            _epi.PersonalEmail = dr["PersonalEmail"].ToString();
                            return _epi;
                        }
                        else
                            return new EmpPersonalInfo();
                    }
                }
            }
            catch (Exception)
            { throw; }
        }

        internal static bool Save(short _empId, EmpPersonalInfo _emp, string _userId, DbTransaction _transaction)
        {
            try
            {
                using (DbCommand dbCommand = db.GetStoredProcCommand("SaveEmployeePersonalInfo"))
                {
                    dbCommand.Transaction = _transaction;
                    db.AddInParameter(dbCommand, "EmployeeId", SqlDbType.SmallInt, _empId);
                    db.AddInParameter(dbCommand, "GenderId", SqlDbType.SmallInt, _emp.GenderId);
                    db.AddInParameter(dbCommand, "MaritalStatusId", SqlDbType.SmallInt, _emp.MaritalStatusId);
                    db.AddInParameter(dbCommand, "HusbandName", SqlDbType.VarChar, _emp.HusbandName);
                    db.AddInParameter(dbCommand, "QualificationId", SqlDbType.SmallInt, _emp.QualificationId);
                    db.AddInParameter(dbCommand, "PaymentModeId", SqlDbType.SmallInt, _emp.PaymentModeId);
                    db.AddInParameter(dbCommand, "CompanyBankId", SqlDbType.SmallInt, _emp.CompanyBankId);
                    db.AddInParameter(dbCommand, "BirthDate", SqlDbType.DateTime, _emp.BirthDate);
                    db.AddInParameter(dbCommand, "CNICNo", SqlDbType.VarChar, _emp.CNICNo);
                    db.AddInParameter(dbCommand, "FamilyNo", SqlDbType.VarChar, _emp.FamilyNo);
                    db.AddInParameter(dbCommand, "NTN", SqlDbType.VarChar, _emp.NTN);
                    db.AddInParameter(dbCommand, "Religion", SqlDbType.VarChar, _emp.Religion);
                    db.AddInParameter(dbCommand, "CurrentAddress", SqlDbType.VarChar, _emp.CurrentAddress);
                    db.AddInParameter(dbCommand, "DifferentPermanentAdd", SqlDbType.Bit, _emp.DifferentPermanentAdd);
                    db.AddInParameter(dbCommand, "PermanentAddress", SqlDbType.VarChar, _emp.PermanentAddress);
                    db.AddInParameter(dbCommand, "PhoneNo", SqlDbType.VarChar, _emp.PhoneNo);
                    db.AddInParameter(dbCommand, "CellNo", SqlDbType.VarChar, _emp.CellNo);
                    db.AddInParameter(dbCommand, "BankAccountNo", SqlDbType.VarChar, _emp.BankAccountNo);
                    db.AddInParameter(dbCommand, "BankDetail", SqlDbType.VarChar, _emp.BankDetail);
                    db.AddInParameter(dbCommand, "Nationality", SqlDbType.VarChar, _emp.Nationality);
                    db.AddInParameter(dbCommand, "SSMember", SqlDbType.Bit, _emp.IsSSMember);
                    db.AddInParameter(dbCommand, "SESSINo", SqlDbType.VarChar, _emp.SESSINo);
                    db.AddInParameter(dbCommand, "EOBINo", SqlDbType.VarChar, _emp.EOBINo);
                    db.AddInParameter(dbCommand, "ESSNo", SqlDbType.VarChar, _emp.ESSNo);
                    db.AddInParameter(dbCommand, "PersonalEmail", SqlDbType.VarChar, _emp.PersonalEmail);
                    db.AddInParameter(dbCommand, "WorkingSince", SqlDbType.SmallDateTime, _emp.WorkingSince);
                    db.ExecuteNonQuery(dbCommand, _transaction);
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