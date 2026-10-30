using HRIS.Helper;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.Serialization;

namespace HRIS.Areas.Common.Models
{
    [DataContract]
    public class FormHistory
    {
        #region private properties
        private static readonly SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        [DataMember(Order=0)]
        public string Sender { get; set; }
        [DataMember(Order = 1)]
        public string Receiver { get; set; }
        [DataMember(Order = 2)]
        public string DocumentStatus { get; set; }
        [DataMember(Order = 3)]
        public string ActivityDate { get; set; }
        [DataMember(Order = 4)]
        public string Remarks { get; set; }
        #endregion

        #region constructor
        public FormHistory()
        {
            
        }

        public FormHistory(string _fromUser, string _toUser, string _documentStatus,
            string _activityDate, string _remarks)
        {
            Sender = _fromUser;
            Receiver = _toUser;
            DocumentStatus = _documentStatus;
            ActivityDate = _activityDate;
            Remarks = _remarks;
        }
        #endregion

        #region public methods
        public static List<FormHistory> Get(int _formId, AgilityEnums.WorkFlow _workFlow)
        {
            List<FormHistory> lstFH = new List<FormHistory>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetForm_History"))
            {
                db.AddInParameter(dbCommand, "FormId", SqlDbType.Int, _formId);
                db.AddInParameter(dbCommand, "WorkFlowId", SqlDbType.TinyInt, _workFlow);
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            lstFH.Add(new FormHistory(
                                dr["Sender"].ToString(),
                                dr["Receiver"].ToString(),
                                dr["StateName"].ToString(),
                                dr["SentOn"].ToString(),
                                dr["SubmissionComments"].ToString()));
                        }
                    }
                }
            }
            return lstFH;
        }
        #endregion
    }
}
