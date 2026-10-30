using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Common.Models
{
    public class Menu : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short Id { get; set; }
        public string Name { get; set; }
        public short? ParentId { get; set; }
        public string OptionSubGroup { get; set; }
        public short SortOrder { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public short? WorkFlowTypeId { get; set; }
        #endregion

        #region constructor
        public Menu()
        {
        }

        public Menu(short _id, string _name, object _parentId, string _optionSubGroup, short _sortOrder, string _controller, string _action,
             object _workflowTypeId)
        {
            try
            {
                Id = _id;
                Name = _name;
                if (_parentId != DBNull.Value)
                    ParentId = Convert.ToInt16(_parentId);
                OptionSubGroup = _optionSubGroup;
                SortOrder = _sortOrder;
                Controller = _controller;
                Action = _action;
                if (_workflowTypeId != DBNull.Value)
                    WorkFlowTypeId = Convert.ToInt16(_workflowTypeId);
            }
            catch (Exception)
            { throw; }
        }
        #endregion

        #region internal methods
        internal static List<Menu> Get()
        {
            List<Menu> lstMenu = new List<Menu>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetMenuByUserId"))
            {
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                using (DataTable dt = db.ExecuteDataSet(dbCommand).Tables[0])
                {
                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            lstMenu.Add(new Menu(
                                Convert.ToInt16(dr["OptionId"]),
                                dr["OptionName"].ToString(),
                                dr["ParentOptionId"],
                                dr["OptionSubGroup"].ToString(),
                                Convert.ToInt16(dr["OptionSequence"]),
                                dr["Controller"].ToString(),
                                dr["Action"].ToString(),
                                dr["WorkFlowId"]));
                        }
                    }
                }
            }
            return lstMenu;
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