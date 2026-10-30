using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Download.Models
{
    public class EmployeePersonalDetail : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string BranchName { get; set; }
        public string DepartmentName { get; set; }
        public string Designation { get; set; }
        public string Grade { get; set; }
        public string HeadName { get; set; }
        public string CNIC { get; set; }
        public DateTime BirthDate { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string CellNo { get; set; }
        public string Email { get; set; }
        public string ESSNo { get; set; }
        public string FamilyNo { get; set; }
        public string SSMember { get; set; }
        public string EOBINo { get; set; }
        public string MaritalStatus { get; set; }
        public string PaymentMode { get; set; }
        public string BankName { get; set; }
        public string BankAccountNo { get; set; }
        public double MedicalCost { get; set; }
        public double LifeInsuranceCost { get; set; }
        public string SupervisorCode { get; set; }
        public string SupervisorName { get; set; }
        public string SupervisorESS { get; set; }
        public string WorkerTypeName { get; set; }
        #endregion

        #region constructor
        public EmployeePersonalDetail()
        {
        }

        public EmployeePersonalDetail(string _employeeCode, string _employeeName, string _designation,
            string _branchName, string _departmentName, string _grade, string _headName, string _cnic, DateTime _birthDate,
            string _gender, string _address, string _cellNo, string _eMail, string _supervisorCode, string _supervisorName,
            string _supervisorESS, string _workerTypeName, string _essNo, string _familyNo, string _ssMember, string _eobiNo, string _maritalStatus,
            string _paymentMode, string _bankName, string _bankAccountNo , double _medicalCost, double _lifeInsurance)
        {
            EmployeeCode = _employeeCode;
            EmployeeName = _employeeName;
            Designation = _designation;
            BranchName = _branchName;
            DepartmentName = _departmentName;
            Grade = _grade;
            HeadName = _headName;
            CNIC = _cnic;
            BirthDate = _birthDate;
            Gender = _gender;
            Address = _address;
            CellNo = _cellNo;
            Email = _eMail;
            SupervisorCode = _supervisorCode;
            SupervisorName = _supervisorName;
            SupervisorESS = _supervisorESS;
            WorkerTypeName = _workerTypeName;
            ESSNo = _essNo;
            FamilyNo = _familyNo;
            SSMember = _ssMember;
            EOBINo = _eobiNo;
            MaritalStatus = _maritalStatus;
            PaymentMode = _paymentMode;
            BankName = _bankName;
            BankAccountNo = _bankAccountNo;
            MedicalCost = _medicalCost;
            LifeInsuranceCost = _lifeInsurance;
        }
        #endregion

        #region internal methods
        internal static List<EmployeePersonalDetail> Get()
        {
            List<EmployeePersonalDetail> lstEPD = new List<EmployeePersonalDetail>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("extEmployeePersonalDetail"))
            {
                db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, ServerSession.GetCurrentCompany());
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            lstEPD.Add(new EmployeePersonalDetail(
                                dr["EmployeeCode"].ToString(),
                                dr["EmployeeName"].ToString(),
                                dr["Designation"].ToString(),
                                dr["BranchName"].ToString(),
                                dr["DepartmentName"].ToString(),
                                dr["GradeName"].ToString(),
                                dr["HeadName"].ToString(),
                                dr["CNICNo"].ToString(),
                                Convert.ToDateTime(dr["BirthDate"]),
                                dr["Gender"].ToString(),
                                dr["Address"].ToString(),
                                dr["CellNo"].ToString(),
                                dr["Email"].ToString(),
                                dr["SupervisorCode"].ToString(),
                                dr["SupervisorName"].ToString(),
                                dr["SupervisorESS"].ToString(),
                                dr["WorkerTypeName"].ToString(),
                                dr["ESSNo"].ToString(),
                                dr["FamilyNo"].ToString(),
                                dr["SSMember"].ToString(),
                                dr["EOBINo"].ToString(),
                                dr["MaritalStatusName"].ToString(),
                                dr["PaymentModeName"].ToString(),
                                dr["BankName"].ToString(),
                                dr["BankAccountNo"].ToString(),
                                Convert.ToDouble(dr["MedicalCost"]),
                                Convert.ToDouble(dr["LifeInsuranceCost"])));
                        }
                    }
                }
            }
            return lstEPD;
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