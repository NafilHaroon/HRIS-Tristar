using HRIS.Helper;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Common.Models
{
    public class MyForm
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public AgilityEnums.Forms ViewType { get; set; }
        public List<FormList> Detail { get; set; }
        #endregion

        #region constructor
        public MyForm()
        {
            ViewType = AgilityEnums.Forms.Current;
            Detail = new List<FormList>();
        }
        #endregion

        #region public methods
        public static List<FormList> ActiveForms(AgilityEnums.WorkFlow _workFlowId)
        {
            return get(_workFlowId, "GetForms_Active");
        }

        public static List<FormList> SentForms(AgilityEnums.WorkFlow _workFlowId)
        {
            return get(_workFlowId, "GetForms_Sent");
        }

        public static List<FormList> CompletedForms(AgilityEnums.WorkFlow _workFlowId)
        {
            return get(_workFlowId, "GetForms_Completed");
        }
        #endregion

        #region private methods
        private static List<FormList> get(AgilityEnums.WorkFlow _workFlowId, string _spName)
        {
            List<FormList> lstFL = new List<FormList>();
            try
            {
                using (DbCommand dbCommand = db.GetStoredProcCommand(_spName))
                {
                    db.AddInParameter(dbCommand, "WorkFlowId", SqlDbType.SmallInt, (short)_workFlowId);
                    db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, ServerSession.GetCurrentCompany());
                    db.AddInParameter(dbCommand, "Owner", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                    using (DataSet ds = db.ExecuteDataSet(dbCommand))
                    {
                        if (ds.Tables.Count > 0)
                        {
                            using (DataTable dt = ds.Tables[0])
                            {
                                foreach (DataRow dr in dt.Rows)
                                {
                                    lstFL.Add(new FormList
                                    (
                                        dr["FormId"].ToString(),
                                        dr["FormCode"].ToString(),
                                        Convert.ToInt16(dr["WorkFlowId"]),
                                        dr["WorkFlowName"].ToString(),
                                        dr["StateName"].ToString(),
                                        dr["Sender"].ToString(),
                                        dr["Recipient"].ToString(),
                                        dr["SentOn"].ToString(),
                                        dr["Amount"],
                                        dr["Description"].ToString(),
                                        dr["SubmissionComments"].ToString(),
                                        dr["Controller"].ToString()));
                                }
                            }
                        }
                    }
                }
                return lstFL;
            }
            catch (Exception)
            { throw; }
        }
        #endregion
    }
}