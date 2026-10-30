using System.Web.Mvc;

namespace HRIS.Areas.Common.Models
{
    public class RecipientModel
    {
        public SelectList RecipientList { get; set; }
        public string[] SelectedRecipient { get; set; }
        public string SubmissionComment { get; set; }
        public string DocumentStatus { get; set; }
        public int FormId { get; set; }
        public short StateId { get; set; }
        public string ControllerPath { get; set; }
        public string StateName { get; set; }
    }
}