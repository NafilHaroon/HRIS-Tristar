using HRIS.Areas.Common.Models;
using HRIS.Areas.Master.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Transaction.Models
{
    public class EmployeeLetter : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short? LetterId { get; set; }
        public string LetterNo { get; set; }
        //public short EmployeeId { get; set; }
        public EmployeeShortList Employee { get; set; }
        public short? TemplateId { get; set; }
        public short? TemplateVersion { get; set; }
        public string TemplateName { get; set; }
        public string LetterContent { get; set; }
        public short StateId { get; set; }
        public string StateName { get; set; }
        //public bool Cancel { get; set; }
        public bool IncludeSepEmp { get; set; }
        public bool ImpactsPayroll { get; set; }
        public bool NoSalaryChange { get; set; }
        public bool Loaded { get; set; }
        public List<EmployeeLetterSalary> Salary { get; set; }
        public string Owner { get; set; }
        public bool Approved { get; set; }
        public bool Rejected { get; set; }
        public bool Completed { get; set; }
        public Footer AuditTrail { get; set; }
        #endregion

        #region constructor
        public EmployeeLetter()
        {
            Employee = new EmployeeShortList();
            Loaded = false;
            ImpactsPayroll = false;
            NoSalaryChange = true;
            StateId = 1;
            StateName = Helper.AgilityEnums.EmployeeLetterState.New.ToString();
            //Cancel = false;
            Owner = ServerSession.GetCurrentUser();
            IncludeSepEmp = false;
            Salary = new List<EmployeeLetterSalary>();
            AuditTrail = new Footer();
            AuditTrail.CreatedBy = Owner;
        }
        #endregion

        #region internal methods
        internal static EmployeeLetter Get(string _letterNo, int? _letterId)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetEmployeeLetter"))
            {
                db.AddInParameter(dbCommand, "LetterNo", SqlDbType.VarChar, _letterNo);
                db.AddInParameter(dbCommand, "LetterId", SqlDbType.Int, _letterId);
                db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, ServerSession.GetCurrentCompany());
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds.Tables != null && ds.Tables[0].Rows.Count > 0)
                    {
                        using (EmployeeLetter _el = new EmployeeLetter())
                        {
                            DataRow dr = ds.Tables[0].Rows[0];
                            _el.LetterNo = dr["LetterNo"].ToString();
                            _el.LetterId = Convert.ToInt16(dr["LetterId"]);
                            _el.Employee = new EmployeeShortList { EmployeeId = Convert.ToInt16(dr["EmployeeId"]), EmployeeName = dr["EmployeeName"].ToString() };
                            _el.TemplateId = Convert.ToInt16(dr["TemplateId"]);
                            _el.TemplateVersion = Convert.ToInt16(dr["TemplateVersion"]);
                            _el.TemplateName = dr["TemplateName"].ToString();
                            _el.LetterContent = dr["LetterContent"].ToString();
                            _el.IncludeSepEmp = Convert.ToBoolean(dr["IncludeSeparatedEmployees"]);
                            _el.ImpactsPayroll = Convert.ToBoolean(dr["ImpactsPayroll"]);
                            _el.NoSalaryChange = Convert.ToBoolean(dr["NoSalaryChange"]);
                            if (_el.ImpactsPayroll && !_el.NoSalaryChange)
                                _el.Salary = EmployeeLetterSalary.GetForLetter(_el.LetterId.Value);
                            //_el.Cancel = Convert.ToBoolean(dr["Cancel"]);
                            _el.StateId = Convert.ToInt16(dr["StateId"]);
                            _el.StateName = dr["StateName"].ToString();
                            _el.Loaded = true;
                            _el.Approved = Convert.ToBoolean(dr["Approved"]);
                            _el.Rejected = Convert.ToBoolean(dr["Rejected"]);
                            _el.Completed = Convert.ToBoolean(dr["Completed"]);
                            _el.Owner = dr["Owner"].ToString();
                            _el.AuditTrail.CreatedBy = dr["CreatedBy"].ToString();
                            _el.AuditTrail.CreatedOn = Convert.ToDateTime(dr["CreatedOn"]);
                            _el.AuditTrail.UpdatedBy = dr["UpdatedBy"].ToString();
                            if (dr["UpdatedOn"] != DBNull.Value)
                                _el.AuditTrail.UpdatedOn = Convert.ToDateTime(dr["UpdatedOn"]);
                            return _el;
                        }
                    }
                    else
                        return null;
                }
            }
        }

        internal static EmployeeLetter New(short _employeeId, short _templateId)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetLetterForEmployeeById"))
            {
                db.AddInParameter(dbCommand, "EmployeeId", SqlDbType.SmallInt, _employeeId);
                db.AddInParameter(dbCommand, "TemplateId", SqlDbType.SmallInt, _templateId);
                db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, ServerSession.GetCurrentCompany());
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds.Tables != null && ds.Tables[0].Rows.Count > 0)
                    {
                        using (EmployeeLetter _el = new EmployeeLetter())
                        {
                            DataRow dr = ds.Tables[0].Rows[0];
                            _el.Employee.EmployeeId = _employeeId;
                            _el.TemplateId = _templateId;
                            _el.Loaded = true;
                            _el.Employee.EmployeeName = dr["EmployeeName"].ToString();
                            _el.TemplateVersion = Convert.ToInt16(dr["TemplateVersion"]);
                            _el.LetterContent = dr["LetterContent"].ToString();
                            _el.ImpactsPayroll = Convert.ToBoolean(dr["ImpactsPayroll"]);
                            _el.NoSalaryChange = !_el.ImpactsPayroll;
                            if (_el.ImpactsPayroll)
                                _el.Salary = EmployeeLetterSalary.Get(_employeeId);
                            _el.AuditTrail.CreatedBy = ServerSession.GetCurrentUser();
                            return _el;
                        }
                    }
                    else
                        return null;
                }
            }
        }

        internal static bool Save(EmployeeLetter _el)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("SaveEmployeeLetter"))
            {
                db.AddInParameter(dbCommand, "LetterId", SqlDbType.SmallInt, _el.LetterId);
                db.AddInParameter(dbCommand, "LetterNo", SqlDbType.VarChar, _el.LetterNo);
                db.AddInParameter(dbCommand, "EmployeeId", SqlDbType.SmallInt, _el.Employee.EmployeeId);
                db.AddInParameter(dbCommand, "TemplateId", SqlDbType.TinyInt, _el.TemplateId);
                db.AddInParameter(dbCommand, "TemplateVersion", SqlDbType.SmallInt, _el.TemplateVersion);
                db.AddInParameter(dbCommand, "LetterContent", SqlDbType.VarChar, _el.LetterContent);
                db.AddInParameter(dbCommand, "IncSeparatedEmp", SqlDbType.Bit, _el.IncludeSepEmp);
                db.AddInParameter(dbCommand, "ImpactsPayroll", SqlDbType.Bit, _el.ImpactsPayroll);
                db.AddInParameter(dbCommand, "NoSalaryChange", SqlDbType.Bit, _el.NoSalaryChange);
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                db.AddInParameter(dbCommand, "UpdatedOn", SqlDbType.DateTime, _el.AuditTrail.UpdatedOn);
                db.AddOutParameter(dbCommand, "newLetterNo", SqlDbType.VarChar, 15);
                db.AddOutParameter(dbCommand, "newLetterId", SqlDbType.SmallInt, 16);

                using (DbConnection dbconnection = db.CreateConnection())
                {
                    dbconnection.Open();
                    DbTransaction transaction = dbconnection.BeginTransaction();
                    try
                    {
                        db.ExecuteNonQuery(dbCommand, transaction);
                        _el.LetterNo = dbCommand.Parameters["@newLetterNo"].Value.ToString();
                        _el.LetterId = Convert.ToInt16(dbCommand.Parameters["@newLetterId"].Value);
                        EmployeeLetterSalary.Save(_el.LetterId.Value, _el.Salary, transaction);
                        transaction.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        internal static bool CancelLetter(string  _letterNo)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("CancelEmployeeLetter"))
            {
                try
                {
                    db.AddInParameter(dbCommand, "LetterNo", SqlDbType.VarChar, _letterNo);
                    db.ExecuteNonQuery(dbCommand);
                    return true;
                }
                catch (Exception)
                { throw; }
            }
        }

        internal static bool SubmitRequest(Submission _sub)
        {
            try
            {
                using (DbCommand dbCommand = db.GetStoredProcCommand("SubmitEmployeeLetter"))
                {
                    db.AddInParameter(dbCommand, "LetterId", SqlDbType.Int, _sub.FormId);
                    db.AddInParameter(dbCommand, "SubmissionComments", SqlDbType.VarChar, _sub.Comments);
                    db.AddInParameter(dbCommand, "StateId", SqlDbType.Int, _sub.StateId);
                    db.AddInParameter(dbCommand, "UpdatedBy", SqlDbType.VarChar, _sub.UserId);
                    db.AddInParameter(dbCommand, "Owner", SqlDbType.VarChar, _sub.Owner);
                    db.AddInParameter(dbCommand, "Completed", SqlDbType.Bit, _sub.Completed);
                    db.AddInParameter(dbCommand, "Rejected", SqlDbType.Bit, _sub.Rejected);
                    db.AddInParameter(dbCommand, "Approved", SqlDbType.Bit, _sub.Approved);
                    db.ExecuteNonQuery(dbCommand);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return true;
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            // no implementation
        }
        #endregion
    }
}