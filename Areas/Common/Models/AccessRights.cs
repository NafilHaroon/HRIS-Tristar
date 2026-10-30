using System;
using System.Data;

namespace HRIS.Areas.Common.Models
{
    public class AccessRights : IDisposable
    {
        #region public properties
        public string AllowAdd { get; set; }
        public string AllowEdit { get; set; }
        public string AllowDelete { get; set; }
        public string FormMode { get; set; }
        #endregion

        #region constructor
        public AccessRights(DataRow dr, string _formMode = "None")
        {
            AllowAdd = dr["canAdd"].ToString();
            AllowEdit = dr["canEdit"].ToString();
            AllowDelete = dr["canDelete"].ToString();
            FormMode = _formMode;
        }

        public AccessRights()
        {
            AllowAdd = "False"; AllowEdit = "False"; AllowDelete = "False"; FormMode = "None";
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