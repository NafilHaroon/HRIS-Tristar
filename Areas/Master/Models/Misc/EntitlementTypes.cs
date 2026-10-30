using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class EntitlementTypes : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short EntitlementTypeId { get; set; }
        public string EntitlementTypeName { get; set; }
        #endregion

        #region constructor
        public EntitlementTypes()
        {
        }

        //public EntitlementType(short _entitlementTypeId, string _entitlementTypeName)
        //{
        //    EntitlementTypeId = _entitlementTypeId;
        //    EntitlementTypeName = _entitlementTypeName;

        //}
        #endregion

        #region internal methods
        /// <summary>
        /// returns list of entitlement types
        /// </summary>
        /// <returns></returns>
        internal static List<EntitlementTypes> Get()
        {
            return get();
        }

        /// <summary>
        /// returns list of Consumable Entitlement Types only
        /// </summary>
        /// <returns></returns>
        internal static List<EntitlementTypes> GetConsumables()
        {
            return get(true);
        }
        #endregion

        #region private methods
        private static List<EntitlementTypes> get(bool _consumableOnly = false)
        {
            List<EntitlementTypes> types = new List<EntitlementTypes>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetEntitlementTypes"))
            {
                db.AddInParameter(dbCommand, "ConsumableOnly", SqlDbType.Bit, _consumableOnly);
                using (DataTable dt = db.ExecuteDataSet(dbCommand).Tables[0])
                {
                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            types.Add(new EntitlementTypes
                            {
                                EntitlementTypeId = Convert.ToInt16(dr["EntitlementTypeId"]),
                                EntitlementTypeName = dr["EntitlementTypeName"].ToString()
                            });
                        }
                    }
                }
            }
            return types;
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