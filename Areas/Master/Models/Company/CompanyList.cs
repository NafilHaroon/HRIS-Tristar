using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Runtime.Serialization;

namespace HRIS.Areas.Master.Models
{
    public class CompanyList : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        [DataMember(Order = 0, Name = "Company Id")]
        public short CompanyId { get; set; }
        //[DataMember(Order = 1, Name = "Company Code")]
        //public string CompanyCode { get; set; }
        [DataMember(Order = 1, Name = "Company Name")]
        public string CompanyName { get; set; }
        [DataMember(Order = 2)]
        public bool IsActive { get; set; }
        //[Browsable(false)]
        //public byte[] CompanyLogo { get; set; }
        #endregion

        #region constructor
        public CompanyList()
        {
        }

        public CompanyList(short _companyId, string _companyName, //string _companyCode, object _companyLogo, 
            bool _isActive = true)
        {
            CompanyId = _companyId;
            CompanyName = _companyName;
            //CompanyCode = _companyCode;
            //if (_companyLogo != DBNull.Value)
            //    CompanyLogo = (byte[])_companyLogo;
            IsActive = _isActive;

        }
        #endregion

        #region internal methods
        internal static List<CompanyList> Get(bool _activeOnly)
        {
            List<CompanyList> lstCompany = new List<CompanyList>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetCompanies"))
            {
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                db.AddInParameter(dbCommand, "ActiveOnly", SqlDbType.Bit, _activeOnly);
                using (DataTable dt = db.ExecuteDataSet(dbCommand).Tables[0])
                {
                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            lstCompany.Add(new CompanyList(
                                Convert.ToInt16(dr["CompanyId"]),
                                dr["CompanyName"].ToString(),
                                //dr["CompanyLogo"],
                                //dr["CompanyCode"].ToString(),
                                Convert.ToBoolean(dr["IsActive"])));
                        }
                    }
                }
            }
            return lstCompany;
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