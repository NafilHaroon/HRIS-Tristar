using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class Allocations : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short TemplateId { get; set; }
        public string TemplateName { get; set; }
        public bool IsActive { get; set; }
        #endregion

        #region constructor
        public Allocations()
        {
        }

        //public Allocations(short _templateId, string _templateName, bool _isActive = true)
        //{
        //    TemplateId = _templateId;
        //    TemplateName = _templateName;
        //    IsActive = _isActive;
        //}
        #endregion

        #region internal methods
        internal static List<Allocations> Get(bool _isActive)
        {
            List<Allocations> lstAT = new List<Allocations>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetAllocationTemplates"))
            {
                db.AddInParameter(dbCommand, "ActiveOnly", SqlDbType.Bit, _isActive);
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            lstAT.Add(new Allocations
                            {
                                TemplateId = Convert.ToInt16(dr["TemplateId"]),
                                TemplateName = dr["TemplateName"].ToString(),
                                IsActive = Convert.ToBoolean(dr["IsActive"])
                            });
                        }
                    }
                }
            }
            return lstAT;
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