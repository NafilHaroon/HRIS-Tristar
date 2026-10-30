using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace HRIS.Areas.Common.Models
{
    public class ColumnDefinition
    {
        #region public properties
        public string headerName;
        public string field;
        #endregion

        #region constructor
        public ColumnDefinition(string _headerName, string _field)
        {
            headerName = _headerName;
            field = _field;
        }
        #endregion

        #region internal methods
        internal static List<ColumnDefinition> getColDef<T>(T dataClass)
        {
            try
            {
                List<ColumnDefinition> _cd = new List<ColumnDefinition>();
                PropertyInfo[] propT = typeof(T).GetProperties().Where(
                    p => p.GetCustomAttributes(typeof(BrowsableAttribute), false).Count() == 0 ||
                    ((BrowsableAttribute)p.GetCustomAttributes(typeof(BrowsableAttribute), false)[0]).Browsable).ToArray();

                foreach (PropertyInfo pi in propT)
                {
                    string colName = pi.Name, colCaption = pi.Name;
                    if (pi.GetCustomAttributes(typeof(DataMemberAttribute), false).Any())
                        colCaption = pi.GetCustomAttributes(typeof(DataMemberAttribute), false).Cast<DataMemberAttribute>().FirstOrDefault().Name;
                    _cd.Add(new ColumnDefinition(colCaption, colName));
                }
                return _cd;
            }
            catch (Exception)
            { throw; }
        }
        #endregion
    }
}