using System;
using System.Data;

namespace HRIS.Areas.Common.Models
{
    public class Footer
    {
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; } = DateTime.Now;
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }

        public Footer()
        {
            
        }

        public Footer(string _createdBy, object _createdOn, string _updatedBy, object _updatedOn)
        {
            CreatedBy = _createdBy;
            if (_createdOn != DBNull.Value)
                CreatedOn = Convert.ToDateTime(_createdOn);
            UpdatedBy = _updatedBy;
            if (_updatedOn != DBNull.Value)
                UpdatedOn = Convert.ToDateTime(_updatedOn);
        }

        public Footer(DataRow dr, bool _inludeUpdate = true)
        {
            CreatedBy = dr["CreatedBy"].ToString();
            CreatedOn = Convert.ToDateTime(dr["CreatedOn"]);
            if (_inludeUpdate)
            {
                UpdatedBy = dr["UpdatedBy"].ToString();
                if (dr["UpdatedOn"] != DBNull.Value)
                    UpdatedOn = Convert.ToDateTime(dr["UpdatedOn"]);
            }
        }
    }
}