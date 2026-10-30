using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class UserCompany :IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
       // public short SerialNo { get; set; }
        public short? UserCompanyId { get; set; }
        public short CompanyId { get; set; }
        public string CompanyName { get; set; }
        public bool Selected { get; set; }
        #endregion

        #region constructor
        public UserCompany()
        {

        }

        internal UserCompany(//short _serialNo, 
            object _UserCompanyId, short _CompanyId, string _CompanyName, bool _selected)
        {
            //SerialNo = _serialNo;
            if (_UserCompanyId != DBNull.Value)
                UserCompanyId = Convert.ToInt16(_UserCompanyId);
            CompanyId = _CompanyId;
            CompanyName = _CompanyName;
            Selected = _selected;
        }
        #endregion

        #region internal methods
        internal static List<UserCompany> Get(string _userId)
        {
            List<UserCompany> lstCompany = new List<UserCompany>();
            try
            {
                using (DbCommand dbCommandDetail = db.GetStoredProcCommand("GetUserCompaniesbyId"))
                {
                    db.AddInParameter(dbCommandDetail, "UserId", SqlDbType.VarChar, _userId);
                    using (DataSet ds = db.ExecuteDataSet(dbCommandDetail))
                    {
                        if (ds.Tables.Count > 0)
                        {
                            //short _serialNo = 1;
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                lstCompany.Add(new UserCompany(
                                    //_serialNo++,
                                    dr["UserCompanyId"],
                                    Convert.ToInt16(dr["CompanyId"]),
                                    dr["CompanyName"].ToString(),
                                    Convert.ToBoolean(dr["Selected"])));
                            }
                        }
                    }
                }
                return lstCompany;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region dispose method
        public void Dispose()
        {
        }
        #endregion
    }
}
