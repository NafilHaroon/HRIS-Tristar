using System;

namespace HRIS.Areas.Common.Models
{
    public class FormList
    {
        #region public properties
        public string FormId { get; set; }
        public string FormCode { get; set; }
        public short WorkFlowId { get; set; }
        public string WorkFlowName { get; set; }
        public string StateName { get; set; }
        public string Sender { get; set; }
        public string Recipient { get; set; }
        public string SentOn { get; set; }
        public decimal? DocumentValue { get; set; }
        public string Description { get; set; }
        public string Comments { get; set; }
        public string Controller { get; set; }
        #endregion

        #region constructor
        public FormList(string _formId, string _formCode, short _workFlowId, string _workFlowName, string _stateName,
                string _sender, string _recipient, string _sentOn, object _value, string _description, string _comments,
                string _controller)
        {
            FormId = _formId;
            FormCode = _formCode;
            WorkFlowId = _workFlowId;
            WorkFlowName = _workFlowName;
            StateName = _stateName;
            Sender = _sender;
            Recipient = _recipient;
            SentOn = _sentOn;
            if (_value != DBNull.Value)
                DocumentValue = Convert.ToDecimal(_value);
            Description = _description;
            Comments = _comments;
            Controller = _controller;
         }
        #endregion
    }
}