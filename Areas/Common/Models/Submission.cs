namespace HRIS.Areas.Common.Models
{
    public class Submission
    {
        public int FormId { get; set; }
        public short StateId { get; set; }
        public string Owner { get; set; }
        public string UserId { get; set; }
        public string Comments { get; set; }
        public string RemarksColumn { get; set; }
        public string Remarks { get; set; }
        public bool Rejected { get; set; }
        public bool Approved { get; set; }
        public bool Completed { get; set; }

        public Submission()
        {
            Rejected = false;
            Approved = false;
        }
    }
}
