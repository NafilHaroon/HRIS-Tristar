namespace HRIS.Helper
{
    public class AgilityEnums
    {
        public const string myFormPath = "../Common/MyForms";
        public const string mainMenuPath = "../Common/Common/Index";
        public const string formMode_Add = "Add";
        public const string formMode_Edit = "Edit";
        public const string formMode_Initialize = "Initialize";
        public const string formMode_Recall = "Recall";
        public const string formMode_ReadOnly = "ReadOnly";
        public const string formMode_Review = "Review";
        public const string Save_Success = "Save operation completed successfully";
        public const string Save_Access_Denied = "Sorry, you don't have required access to Save this Entry";

        public enum UserValidation { Successful = 1, InvalidUserId = 2, InvalidPassword = 3, ForcePasswordChange = 4, UserIdDisabled = 5, }

        public enum TaxCreditState { New = 1, SubmittedForApproval = 2, Approved = 3, Rejected = 4, Returned = 5}
        public enum TaxDebitState { New = 1, SubmittedForApproval = 2, Approved = 3, Rejected = 4, Returned = 5 }
        public enum EmployeeLetterState { New = 1, SubmittedForApproval = 2, Approved = 3, Rejected = 4, Returned = 5, SubmittedForCEOApproval=99 }

        public enum WorkFlow { All = -1, TaxCredit = 1, TaxDebit = 2, EmployeeLetter = 3 }
        public enum Forms : short { Current = 0, Sent = 1, Closed = 2, };

        public enum CalledForm : short { MyForm = 2, Menu = 3 };

        public enum UserRole : short { None = 0, HR_Staff = 1, HoD_HR = 2, CEO = 3 }
    }
}