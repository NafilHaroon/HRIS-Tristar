using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.Serialization;

namespace HRIS.Areas.Master.Models
{
    public class LetterTemplates : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        [DataMember(Order = 0, Name = "Template Id")]
        public short TemplateId { get; set; }
        [DataMember(Order = 1, Name = "Template Name")]
        public string TemplateName { get; set; }
        [DataMember(Order =2, Name ="Letter Prefix")]
        public string LetterPrefix { get; set; }
        [DataMember(Order = 3)]
        public bool IsActive { get; set; }

        public bool IncludeSepEmp { get; set; }
        public bool ImpactsPayroll { get; set; }
        #endregion

        #region constructor
        public LetterTemplates()
        {
        }

        //public LetterTemplates(short _templateId, string _templateName, string _letterPrefix,
        //    bool _includeSepEmp, bool _impactPayroll, bool _isActive = true)
        //{
        //    TemplateId = _templateId;
        //    TemplateName = _templateName;
        //    LetterPrefix = _letterPrefix;
        //    IsActive = _isActive;
        //    IncludeSepEmp = _includeSepEmp;
        //    ImpactsPayroll = _impactPayroll;
        //}
        #endregion

        #region internal methods
        internal static List<LetterTemplates> Get(bool _activeOnly)
        {
            List<LetterTemplates> templates = new List<LetterTemplates>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetLetterTemplates"))
            {
                db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, ServerSession.GetCurrentCompany());
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                db.AddInParameter(dbCommand, "ActiveOnly", SqlDbType.Bit, _activeOnly);
                using (DataTable dt = db.ExecuteDataSet(dbCommand).Tables[0])
                {
                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            templates.Add(new LetterTemplates
                            {
                                TemplateId = Convert.ToInt16(dr["TemplateId"]),
                                TemplateName = dr["TemplateName"].ToString(),
                                LetterPrefix = dr["LetterPrefix"].ToString(),
                                IncludeSepEmp = Convert.ToBoolean(dr["IncludeSeparatedEmployees"]),
                                ImpactsPayroll = Convert.ToBoolean(dr["ImpactsPayroll"]),
                                IsActive = Convert.ToBoolean(dr["IsActive"])
                            });
                        }
                    }
                }
            }
            return templates;
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