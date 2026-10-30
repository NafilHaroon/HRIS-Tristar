app.controller('FormSubmissionController', function ($scope, $http) {
    try {
        $http({
            type: "Get",
            url: baseURL + "FormSubmission",
            params: { _formId: $scope.formId, _stateId: $scope.StateId }
        }).then(result => {
            if (!result.data.success) {
                alert(result.data.message);
                $scope.modalInstance.close('ok');
            }
            else {
                $scope.RecipientModel = result.data.RecipientModel;
                if (result.data.RecipientModel.RecipientList.length === 1)
                    $scope.RecipientModel.SelectedRecipient = result.data.RecipientModel.RecipientList[0].Value;
                document.getElementById('lstRecipient').focus();
            }
        }, errorMsg => {
            alert(errorMsg);
        });
    }
    catch (exception) {
        alert(exception.Message);
    }

    $scope.tbCancel = function () {
        $scope.RecipientModel = {};  
        $scope.modalInstance.close('ok');
    };

    $scope.tbSubmit = function () {
        if ($scope.RecipientModel.SelectedRecipient == null) {
            alert("Please Select Submission user to proceed this form .");
        }
        else {
            $http({
                method: "Get",
                url: baseURL + "FormSubmitted",
                params: {
                    _formId: $scope.RecipientModel.FormId, _stateId: $scope.RecipientModel.StateId,
                    _comments: $scope.RecipientModel.SubmissionComment,
                    _owner: $scope.RecipientModel.SelectedRecipient
                }
            }).then(result => {
                $scope.errors = [];
                if (!result.data.success) {
                    alert(result.data.message);
                }
                else {
                    alert(result.data.message);
                    $scope.RecipientModel = {};
                    if ($scope.MyForm) {
                        window.location.href = '/HRIS/Common/MyForms/DataForm';
                    }
                    else {
                        window.location.href = '/HRIS/Common/Common/Index';
                    }
                }
            }, errorMsg => {
                alert(errorMsg);
            });
        }
    };
});


