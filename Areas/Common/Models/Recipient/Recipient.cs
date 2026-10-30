using HRIS.Helper;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Common.Models
{
    public class Recipient
    {
        #region prviate properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public string RecipientId { get; set; }
        public string RecipientName { get; set; }
        public List<Recipient> RecipientList { get; set; }
        #endregion

        #region constructor
        public Recipient()
        {
        }

        public Recipient(string _recipientId, string _recipientName)
        {
            RecipientId = _recipientId;
            RecipientName = _recipientName;
        }
        #endregion

        #region public methods
        public static List<Recipient> GetFormCreator(AgilityEnums.WorkFlow _workFlowId, int _formId)
        {
            return get("GetFormCreator", _workFlowId, _formId);
        }

        public static List<Recipient> GetDirectManager()
        {
            List<Recipient> lstRcpt = new List<Recipient>();
            try
            {
                using (DbCommand dbCommandDetail = db.GetStoredProcCommand("GetDirectManager"))
                {
                    db.AddInParameter(dbCommandDetail, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                    using (IDataReader dr = db.ExecuteReader(dbCommandDetail))
                    {
                        while (dr.Read())
                        {
                            lstRcpt.Add(new Recipient(
                                dr.GetString(dr.GetOrdinal("UserId")),
                                dr.GetString(dr.GetOrdinal("UserName"))));
                        }
                    }
                    return lstRcpt;
                }
            }
            catch (Exception)
            { throw; }
        }

        public static List<Recipient> GetLetterRecipient(int _letterId, 
            AgilityEnums.EmployeeLetterState _stateId, out AgilityEnums.EmployeeLetterState _nextStateId)
        {
            List<Recipient> lstRcpt = new List<Recipient>();
            try
            {
                _nextStateId = _stateId;
                using (DbCommand dbCommandDetail = db.GetStoredProcCommand("GetEmployeeLetterRecipients"))
                {
                    db.AddInParameter(dbCommandDetail, "LetterId", SqlDbType.Int, _letterId);
                    db.AddInParameter(dbCommandDetail, "StateId", SqlDbType.SmallInt, _stateId);
                    db.AddOutParameter(dbCommandDetail, "NextStateId", SqlDbType.SmallInt, 16);
                    using (DataSet ds = db.ExecuteDataSet(dbCommandDetail))
                    {
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            _nextStateId = (AgilityEnums.EmployeeLetterState)Convert.ToInt16(dbCommandDetail.Parameters["@NextStateId"].Value);
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                lstRcpt.Add(new Recipient(
                                    dr["UserId"].ToString(),
                                    dr["UserName"].ToString()));
                            }
                        }
                    }
                }
                return lstRcpt;
            }
            catch (Exception)
            { throw; }
        }
        #endregion

        #region private methods
        private static List<Recipient> get(string _spName, AgilityEnums.WorkFlow _workFlowId, int _formId)
        {
            List<Recipient> lstRcpt = new List<Recipient>();
            try
            {
                using (DbCommand dbCommandDetail = db.GetStoredProcCommand(_spName))
                {
                    db.AddInParameter(dbCommandDetail, "WorkFlowId", SqlDbType.SmallInt, _workFlowId);
                    db.AddInParameter(dbCommandDetail, "FormId", SqlDbType.Int, _formId);
                    using (IDataReader dr = db.ExecuteReader(dbCommandDetail))
                    {
                        while (dr.Read())
                        {
                            lstRcpt.Add(new Recipient(
                                dr.GetString(dr.GetOrdinal("UserId")),
                                dr.GetString(dr.GetOrdinal("UserName"))));
                        }
                    }
                    return lstRcpt;
                }
            }
            catch (Exception)
            { throw; }
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
