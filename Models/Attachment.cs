using HRIS.Areas.Common.Models;
using HRIS.Areas.Master.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Runtime.Serialization;
using static HRIS.Helper.AgilityEnums;

namespace HRIS.Models
{
    public class Attachment : IDisposable
    {
        #region private properties
        private static readonly SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public int AttachmentId { get; set; }
        [DataMember(Order = 0)]
        [Display(Name = "Attachment Type")]
        public AttachmentType TypeOfAttachment { get; set; }
        [DataMember(Order = 1)]
        [Display(Name = "Attachment Name")]
        public string AttachmentName { get; set; }
        public string AttachmentContentType { get; set; }
        public byte[] AttachmentContent { get; set; }
        public bool Edit { get; set; } = false;
        public bool Delete { get; set; } = false;
        #endregion

        #region constructor
        public Attachment()
        {

        }

        //used by Angular screens
        //public Attachment(int _attachmentId, AttachmentType _attachmentType, string _attachmentName)
        //{
        //    AttachmentId = _attachmentId;
        //    TypeOfAttachment = _attachmentType;
        //    AttachmentName = _attachmentName;
        //    IsNew = false;
        //    IsDeleted = false;
        //}

        ////used by angular for downloading specific attachment
        //public Attachment(string _attachmentName, string _attachmentContentType, byte[] _attachmentContent)
        //{
        //    AttachmentName = _attachmentName;
        //    AttachmentContentType = _attachmentContentType;
        //    AttachmentContent = _attachmentContent;
        //}
        #endregion

        #region internal methods
        internal static List<Attachment> Get(short _workFlowId, int _formId)
        {
            List<Attachment> lstATT = new List<Attachment>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetAttachmentsById"))
            {
                db.AddInParameter(dbCommand, "WorkFlowId", SqlDbType.TinyInt, _workFlowId);
                db.AddInParameter(dbCommand, "FormId", SqlDbType.Int, _formId);
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            lstATT.Add(new Attachment
                            {
                                TypeOfAttachment = new AttachmentType
                                {
                                    AttachmentTypeId = Convert.ToInt16(dr["AttachmentTypeId"]),
                                    AttachmentTypeName = dr["AttachmentTypeName"].ToString()
                                },
                                AttachmentName = dr["FileName"].ToString(),
                                AttachmentContentType = dr["FileContentType"].ToString(),
                                AttachmentContent = (byte[])(dr["FileContent"]),
                                Edit = false
                            });
                        }
                    }
                }
            }
            return lstATT;
        }

        //internal static bool Save(IEnumerable<Attachment> _wfAttachments,
        //    int _formId, WorkFlow _workFlowId, DbTransaction _transaction)
        //{
        //    try
        //    {
        //        foreach (Attachment _att in _wfAttachments)
        //        {
        //            using (DbCommand dbCommanddetail = db.GetStoredProcCommand("SaveWorkFlowAttachment"))
        //            {
        //                db.AddInParameter(dbCommanddetail, "WorkFlowId", SqlDbType.SmallInt, _workFlowId);
        //                db.AddInParameter(dbCommanddetail, "FormId", SqlDbType.Int, _formId);
        //                db.AddInParameter(dbCommanddetail, "AttachmentTypeId", SqlDbType.SmallInt, _att.TypeOfAttachment.AttachmentTypeId);
        //                db.AddInParameter(dbCommanddetail, "FileName", SqlDbType.VarChar, _att.AttachmentName);
        //                db.AddInParameter(dbCommanddetail, "FileContentType", SqlDbType.VarChar, _att.AttachmentContentType);
        //                db.AddInParameter(dbCommanddetail, "FileContent", SqlDbType.VarBinary, _att.AttachmentContent);
        //                db.AddInParameter(dbCommanddetail, "IsNew", SqlDbType.Bit, _att.IsNew);
        //                db.AddInParameter(dbCommanddetail, "userid", SqlDbType.VarChar, ServerSession.GetCurrentUser());
        //                db.ExecuteNonQuery(dbCommanddetail, _transaction);
        //            }
        //        }
        //        return true;
        //    }
        //    catch (Exception)
        //    { throw; }
        //}

        /// <summary>
        /// Gets attachment list against form without loading contents stream
        /// </summary>
        /// <param name="_formId">Form #</param>
        /// <returns></returns>
        internal static List<Attachment> GetWOStream(int _formId, WorkFlow _wfId)
        {
            try
            {
                List<Attachment> lstAtt = new List<Attachment>();
                using (DbCommand dbCommand = db.GetStoredProcCommand("GetAttachmentWOStreamById"))
                {
                    db.AddInParameter(dbCommand, "FormId", SqlDbType.Int, _formId);
                    db.AddInParameter(dbCommand, "WorkFlowId", SqlDbType.Int, _wfId);
                    using (DataSet ds = db.ExecuteDataSet(dbCommand))
                    {
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                lstAtt.Add(new Attachment
                                {
                                    AttachmentId = Convert.ToInt32(dr["AttachmentId"]),
                                    TypeOfAttachment = new AttachmentType
                                    {
                                        AttachmentTypeId = Convert.ToInt16(dr["AttachmentTypeId"]),
                                        AttachmentTypeName = dr["AttachmentTypeName"].ToString()
                                    },
                                    AttachmentName = dr["FileName"].ToString()
                                });
                            }
                        }
                    }
                }
                return lstAtt;
            }
            catch (Exception)
            { throw; }
        }

        /// <summary>
        /// Get Attachment stream for specific attachment when user click view button
        /// </summary>
        /// <param name="_attachmentId"></param>
        /// <returns></returns>
        internal static Attachment GetStream(int _attachmentId)
        {
            try
            {
                using (DbCommand dbCommand = db.GetStoredProcCommand("GetAttachmentStreamById"))
                {
                    db.AddInParameter(dbCommand, "AttachmentId", SqlDbType.Int, _attachmentId);
                    db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                    using (DataSet ds = db.ExecuteDataSet(dbCommand))
                    {
                        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                        {
                            DataRow dr = ds.Tables[0].Rows[0];
                            return new Attachment
                            {
                                AttachmentName = dr["FileName"].ToString(),
                                AttachmentContentType = dr["FileContentType"].ToString(),
                                AttachmentContent = (byte[])(dr["FileContent"])
                            };
                        }
                    }
                }
                return null;
            }
            catch (Exception)
            { throw; }
        }

        //deletes documents marked as logically deleted on form, Remove name is used because Delete is conflicting with field name otherwise
        internal static bool Remove(int _formId, List<Attachment> _details, DbTransaction _transaction)
        {
            try
            {
                foreach (Attachment _att in _details.Where(x => x.Delete))
                {
                    using (DbCommand dbCommand = db.GetStoredProcCommand("DeleteWorkFlowAttachment"))
                    {
                        db.AddInParameter(dbCommand, "AttachmentId", SqlDbType.VarChar, _att.AttachmentId);
                        db.AddInParameter(dbCommand, "FormId", SqlDbType.Int, _formId);
                        db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                        db.ExecuteNonQuery(dbCommand, _transaction);
                    }
                }
                return true;
            }
            catch (Exception)
            { throw; }
        }

        //uploads new document - single document at a time
        internal static bool Upload(int _formId, WorkFlow _workFlowId, short _typeId, string _attName, string _attType, byte[] _content)
        {
            try
            {
                using (DbCommand dbCommand = db.GetStoredProcCommand("SaveWorkFlowAttachment"))
                {
                    db.AddInParameter(dbCommand, "WorkFlowId", SqlDbType.SmallInt, _workFlowId);
                    db.AddInParameter(dbCommand, "FormId", SqlDbType.Int, _formId);
                    db.AddInParameter(dbCommand, "AttachmentTypeId", SqlDbType.SmallInt, _typeId);
                    db.AddInParameter(dbCommand, "FileName", SqlDbType.VarChar, _attName);
                    db.AddInParameter(dbCommand, "FileContentType", SqlDbType.VarChar, _attType);
                    db.AddInParameter(dbCommand, "FileContent", SqlDbType.VarBinary, _content);
                    db.AddInParameter(dbCommand, "IsNew", SqlDbType.Bit, true);
                    db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                    db.ExecuteNonQuery(dbCommand);
                }
                return true;
            }
            catch (Exception)
            { throw; }
        }

        internal static IEnumerable<Attachment> GetChanges(List<Attachment> _supportingDox)
        {
            return _supportingDox.Where(x => x.Edit || (x.Delete && !x.Edit));
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
