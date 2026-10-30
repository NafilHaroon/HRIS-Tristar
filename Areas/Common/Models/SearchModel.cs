using System.Data;

namespace HRIS.Areas.Common.Models
{
    public class SearchModel
    {
        #region public properties
        public string ControllerPath { get; set; }
        public DataTable dtSearch { get; set; }
        public string SearchData { get; set; }
        public string SearchSelection { get; set; }
        public string SearchBy { get; set; }
        #endregion

        #region constructor
        public SearchModel()
        {

        }

        public SearchModel(DataTable _dtSearch, string _controllerPath)
        {
            dtSearch = _dtSearch;
            ControllerPath = _controllerPath;
        }
        #endregion
    }
}