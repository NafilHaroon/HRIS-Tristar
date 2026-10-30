using HRIS.Areas.Common.Models;
using HRIS.Helper;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace HRIS.Areas.Master.Models
{
    public class UserProfile : IDisposable
    {
        #region private properties
        private static readonly SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        //public string Designation { get; set; }
        public string EmailId { get; set; }
        public string DepartmentCode { get; set; }
        public string DepartmentName { get; set; }
        public short UserRoleId { get; set; }
        public bool UserActive { get; set; }
        public string UserLevel { get; set; }

        public List<UserGrade> Grades { get; set; }
        public List<UserOption> Options { get; set; }
        public List<UserBranch> Branches { get; set; }
        public List<UserCompany> Companies { get; set; }
        public List<UserRole> Roles { get; set; }
        public Footer AuditTrail { get; set; }
        #endregion

        #region constructor
        public UserProfile()
        {
            Grades = new List<UserGrade>();
            Options = new List<UserOption>();
            Branches = new List<UserBranch>();
            Companies = new List<UserCompany>();
            UserActive = true;
            AuditTrail = new Footer();
        }
        #endregion

        #region internal methods
        public AgilityEnums.UserValidation AuthenticateUser()
        {
            AgilityEnums.UserValidation _result = AgilityEnums.UserValidation.InvalidUserId;
            try
            {
                using (DbCommand dbCommand = db.GetStoredProcCommand("uspAuthenticateUser"))
                {
                    db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, this.UserId);
                    using (DataTable dt = db.ExecuteDataSet(dbCommand).Tables[0])
                    {
                        if (dt.Rows.Count > 0)
                        {
                            DataRow dr = dt.Rows[0];
                            {
                                if (!Convert.ToBoolean(dr["UserActive"]))
                                    _result = AgilityEnums.UserValidation.UserIdDisabled;
                                else if (this.Password != AgilityHelper.MaskPassword(dr["Password"].ToString(), false))
                                    _result = AgilityEnums.UserValidation.InvalidPassword;
                                else
                                {
                                    if (Convert.ToBoolean(dr["ForcePwdChange"]))
                                        _result = AgilityEnums.UserValidation.ForcePasswordChange;
                                    else
                                        _result = AgilityEnums.UserValidation.Successful;

                                    this.UserName = dr["UserName"].ToString();
                                    this.EmailId = dr["EmailAddress"].ToString();
                                    this.BranchCode = dr["BranchCode"].ToString();
                                    this.DepartmentCode = dr["DepartmentCode"].ToString();
                                    this.UserLevel = dr["UserLevel"].ToString();

                                    if (_result == AgilityEnums.UserValidation.Successful)
                                    {
                                        using (DbCommand dbAuxInfo = db.GetStoredProcCommand("GetUserAuxInfo"))
                                        {
                                            db.AddInParameter(dbAuxInfo, "UserId", SqlDbType.VarChar, this.UserId);
                                            using (DataTable dtAux = db.ExecuteDataSet(dbAuxInfo).Tables[0])
                                            {
                                                if (dtAux.Rows.Count > 0)
                                                {
                                                    DataRow drAux = dtAux.Rows[0];
                                                    this.UserRoleId = Convert.ToInt16(drAux["RoleId"]);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                            throw new Exception("The User Id doesn`t exist or has been deactivated!");
                    }
                }
                return _result;
            }
            catch (Exception)
            { throw; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pUserId">User Id for which password change is requested</param>
        /// <param name="cPassword">Current password of the user</param>
        /// <param name="pPassword">New password of the user</param>
        /// <param name="pForcePwdChange">Force password change flag</param>
        /// <returns></returns>
        internal static bool ChangePassword(string _userId, string _pcPassword, string _pnPassword)
        {
            try
            {
                using (DbCommand dbCommand = db.GetStoredProcCommand("uspChangePassword"))
                {
                    db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, _userId);
                    db.AddInParameter(dbCommand, "OldPassword", SqlDbType.VarChar, _pcPassword);
                    db.AddInParameter(dbCommand, "NewPassword", SqlDbType.VarChar, _pnPassword);
                    db.ExecuteNonQuery(dbCommand);
                    return true;
                }
            }
            catch (Exception)
            { throw; }
        }

        internal static bool SendPassword(string _userId)
        {
            try
            {
                using (DbCommand dbCommand = db.GetStoredProcCommand("uspSendPassword"))
                {
                    db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, _userId);
                    db.ExecuteNonQuery(dbCommand);
                    return true;
                }
            }
            catch (Exception)
            { throw; }
        }

        internal static DataTable GetAccess()
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetAccessForUser"))
            {
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                return db.ExecuteDataSet(dbCommand).Tables[0];
            }
        }

        internal static UserProfile Get(string _userId)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetUserById"))
            {
                db.AddInParameter(dbCommand, "userId", SqlDbType.VarChar, _userId);
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        using (UserProfile u = new UserProfile())
                        {
                            DataRow dr = ds.Tables[0].Rows[0];
                            u.UserId = dr["UserId"].ToString();
                            u.UserName = dr["UserName"].ToString();
                            u.BranchName = dr["BranchName"].ToString();
                            u.EmailId = dr["EmailAddress"].ToString();
                            u.DepartmentName = dr["DepartmentName"].ToString();
                            u.UserActive = Convert.ToBoolean(dr["UserActive"]);
                            u.UserLevel = dr["UserLevel"].ToString();

                            u.AuditTrail.CreatedBy = dr["CreatedBy"].ToString();
                            u.AuditTrail.CreatedOn = Convert.ToDateTime(dr["CreatedOn"]);
                            u.AuditTrail.UpdatedBy = dr["UpdatedBy"].ToString();
                            if (dr["UpdatedOn"] != DBNull.Value)
                                u.AuditTrail.UpdatedOn = Convert.ToDateTime(dr["UpdatedOn"]);

                            u.Options = UserOption.Get(_userId);
                            u.Branches = UserBranch.Get(_userId);
                            u.Grades = UserGrade.Get(_userId);
                            u.Companies = UserCompany.Get(_userId);
                            u.Roles = UserRole.Get(_userId);
                            return u;
                        }
                    }
                    else
                        return null;
                }
            }
        }

        internal static bool Save(UserProfile _up)
        {
            string _userId = ServerSession.GetCurrentUser();
            using (DbConnection dbconnection = db.CreateConnection())
            {
                dbconnection.Open();
                DbTransaction transaction = dbconnection.BeginTransaction();
                try
                {
                    #region User Options
                    foreach (UserOption uo in getOptionChanges(_up.Options))
                    {
                        using (DbCommand dbCommanddetail = db.GetStoredProcCommand("SaveUserOption"))
                        {
                            //db.AddInParameter(dbCommanddetail, "UserOptionId", SqlDbType.Int, uo.UserOptionId);
                            db.AddInParameter(dbCommanddetail, "NewUserId", SqlDbType.VarChar, _up.UserId);
                            db.AddInParameter(dbCommanddetail, "OptionId", SqlDbType.SmallInt, uo.OptionId);
                            db.AddInParameter(dbCommanddetail, "AllowAccess", SqlDbType.Bit, uo.AllowAccess);
                            db.AddInParameter(dbCommanddetail, "AllowAdd", SqlDbType.Bit, uo.AllowAdd);
                            db.AddInParameter(dbCommanddetail, "AllowEdit", SqlDbType.Bit, uo.AllowEdit);
                            db.AddInParameter(dbCommanddetail, "UserId", SqlDbType.VarChar, _userId);
                            db.ExecuteNonQuery(dbCommanddetail, transaction);
                        }
                    }
                    #endregion

                    #region User Branch
                    foreach (UserBranch ub in getBranchChanges(_up))
                    {
                        using (DbCommand dbCommanddetail = db.GetStoredProcCommand("SaveUserBranch"))
                        {
                            db.AddInParameter(dbCommanddetail, "UserBranchId", SqlDbType.VarChar, ub.UserBranchId);
                            db.AddInParameter(dbCommanddetail, "NewUserId", SqlDbType.VarChar, _up.UserId);
                            db.AddInParameter(dbCommanddetail, "BranchCode", SqlDbType.VarChar, ub.BranchCode);
                            db.AddInParameter(dbCommanddetail, "Selected", SqlDbType.Bit, ub.Selected);
                            db.AddInParameter(dbCommanddetail, "UserId", SqlDbType.VarChar, _userId);
                            db.ExecuteNonQuery(dbCommanddetail, transaction);
                        }
                    }
                    #endregion

                    #region User Grade
                    foreach (UserGrade ug in getGradeChanges(_up))
                    {
                        using (DbCommand dbCommanddetail = db.GetStoredProcCommand("SaveUserGrade"))
                        {
                            db.AddInParameter(dbCommanddetail, "UserGradeId", SqlDbType.Int, ug.UserGradeId);
                            db.AddInParameter(dbCommanddetail, "NewUserId", SqlDbType.VarChar, _up.UserId);
                            db.AddInParameter(dbCommanddetail, "GradeId", SqlDbType.SmallInt, ug.GradeId);
                            db.AddInParameter(dbCommanddetail, "Selected", SqlDbType.Bit, ug.Selected);
                            db.AddInParameter(dbCommanddetail, "UserId", SqlDbType.VarChar, _userId);
                            db.ExecuteNonQuery(dbCommanddetail, transaction);
                        }
                    }
                    #endregion

                    #region User Role
                    foreach (UserRole ur in getRoleChanges(_up))
                    {
                        using (DbCommand dbCommanddetail = db.GetStoredProcCommand("SaveUserRole"))
                        {
                            db.AddInParameter(dbCommanddetail, "UserRoleId", SqlDbType.Int, ur.UserRoleId);
                            db.AddInParameter(dbCommanddetail, "NewUserId", SqlDbType.VarChar, _up.UserId);
                            db.AddInParameter(dbCommanddetail, "RoleId", SqlDbType.SmallInt, ur.RoleId);
                            db.AddInParameter(dbCommanddetail, "Selected", SqlDbType.Bit, ur.Selected);
                            db.AddInParameter(dbCommanddetail, "UserId", SqlDbType.VarChar, _userId);
                            db.ExecuteNonQuery(dbCommanddetail, transaction);
                        }
                    }
                    #endregion

                    #region User Company
                    foreach (UserCompany uc in getCompanyChanges(_up))
                    {
                        using (DbCommand dbCommanddetail = db.GetStoredProcCommand("SaveUserCompany"))
                        {
                            db.AddInParameter(dbCommanddetail, "UserCompanyId", SqlDbType.Int, uc.UserCompanyId);
                            db.AddInParameter(dbCommanddetail, "NewUserId", SqlDbType.VarChar, _up.UserId);
                            db.AddInParameter(dbCommanddetail, "CompanyId", SqlDbType.SmallInt, uc.CompanyId);
                            db.AddInParameter(dbCommanddetail, "Selected", SqlDbType.Bit, uc.Selected);
                            db.AddInParameter(dbCommanddetail, "UserId", SqlDbType.VarChar, _userId);
                            db.ExecuteNonQuery(dbCommanddetail, transaction);
                        }
                    }
                    #endregion

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
        #endregion
        #region private methods
        private static IEnumerable<UserOption> getOptionChanges(List<UserOption> _uo)
        {
            return _uo.Where(x => x.Edit);
        }

        private static IEnumerable<UserBranch> getBranchChanges(UserProfile u)
        {
            return u.Branches.Where(a => (a.Selected && !a.UserBranchId.HasValue) || (!a.Selected && a.UserBranchId.HasValue));
        }

        private static IEnumerable<UserGrade> getGradeChanges(UserProfile u)
        {
            return u.Grades.Where(a => (a.Selected && !a.UserGradeId.HasValue) || (!a.Selected && a.UserGradeId.HasValue));
        }

        private static IEnumerable<UserRole> getRoleChanges(UserProfile u)
        {
            return u.Roles.Where(a => (a.Selected && !a.UserRoleId.HasValue) || (!a.Selected && a.UserRoleId.HasValue));
        }

        private static IEnumerable<UserCompany> getCompanyChanges(UserProfile u)
        {
            return u.Companies.Where(a => (a.Selected && !a.UserCompanyId.HasValue) || (!a.Selected && a.UserCompanyId.HasValue));
        }
        #endregion

        #region dispose method
        public void Dispose()
        {
        }
        #endregion
    }
}
