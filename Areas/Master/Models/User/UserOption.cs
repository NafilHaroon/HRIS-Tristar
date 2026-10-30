using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class UserOption : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        //public short? UserOptionId { get; set; }
        public short OptionId { get; set; }
        public string OptionName { get; set; }
        public bool AllowAccess { get; set; }
        public bool AllowAdd { get; set; }
        public bool AllowEdit { get; set; }
        //public bool Add { get; set; }
        public bool Edit { get; set; }
        //public bool Delete { get; set; }

        #endregion

        #region constructors
        public UserOption()
        {

        }

        public UserOption(short _optionId, string _optionName, bool _allowAccess, bool _allowAdd, bool _allowEdit)
        {
            OptionId = _optionId;
            OptionName = _optionName;
            AllowAccess = _allowAccess;
            AllowAdd = _allowAdd;
            AllowEdit = _allowEdit;
            Edit = false;
        }
        #endregion

        #region intenral methods
        internal static List<UserOption> Get(string _userId)
        {
            List<UserOption> lstOption = new List<UserOption>();
            try
            {
                using (DbCommand dbCommandDetail = db.GetStoredProcCommand("getUserOptionsById"))
                {
                    db.AddInParameter(dbCommandDetail, "UserId", SqlDbType.VarChar, _userId);
                    using (DataSet ds = db.ExecuteDataSet(dbCommandDetail))
                    {
                        if (ds.Tables.Count > 0)
                        {
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                lstOption.Add(new UserOption(
                                    Convert.ToInt16(dr["OptionId"]),
                                    dr["OptionName"].ToString(),
                                    Convert.ToBoolean(dr["IsVisible"]),
                                    Convert.ToBoolean(dr["CanAdd"]),
                                    Convert.ToBoolean(dr["CanEdit"])));
                            }
                        }
                    }
                }
                return lstOption;
            }
            catch (Exception)
            { throw; }
        }
        #endregion

        #region IDisposable Member
        public void Dispose()
        {
        }
        #endregion
    }
}
