using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class HFMSubCategory : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short SubCategoryId { get; set; }
        public string SubCategoryName { get; set; }
        #endregion

        #region constructor
        public HFMSubCategory()
        {
        }

        public HFMSubCategory(short _subCategoryId, string _subCategoryName)
        {
            SubCategoryId = _subCategoryId;
            SubCategoryName = _subCategoryName;

        }
        #endregion

        #region internal methods
        internal static List<HFMSubCategory> Get()
        {
            return get();
        }
        #endregion

        #region private methods
        /// <summary>
        /// return list of HFM Sub Categories
        /// </summary>
        /// <returns></returns>
        private static List<HFMSubCategory> get()
        {
            List<HFMSubCategory> lstSC = new List<HFMSubCategory>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetHFMSubCategories"))
            {
                using (DataTable dt = db.ExecuteDataSet(dbCommand).Tables[0])
                {
                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            lstSC.Add(new HFMSubCategory(
                                Convert.ToInt16(dr["SubCategoryId"]),
                                dr["SubCategoryName"].ToString()));
                        }
                    }
                }
            }
            return lstSC;
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