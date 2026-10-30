baseURL = "/HRIS/Common/User/";

app.controller("ChgPwdController", function ($scope, $uibModal, $http, $window, $mdToast) {
    $scope.chgPwdFormLoaded = function () {
        $scope.errors = [];
        $scope.CP = new Object();
        $scope.CP.OldPassword = null; $scope.CP.NewPassword = null; $scope.CP.ConfirmPassword = null;
        $('#oldPassword').focus();
    };

    $scope.pwdNonCompliant = function () {
        return $scope.CP.OldPassword == null ||
            $scope.CP.NewPassword == null ||
            $scope.CP.OldPassword == $scope.CP.NewPassword ||
            $scope.CP.NewPassword != $scope.CP.ConfirmPassword ||
            $scope.CP.NewPassword.length < 8;
    }

    $scope.tbChgPwd = function (res) {
        $http({
            method: "POST",
            url: baseURL + "ApplyNewPassword",
            data: $scope.CP
        }).then(result => {
            $scope.errors = [];
            if (!result.data.success) {
                $scope.errors = getArray(result.data.message);
                showErrorToast(result.data.message);
            }
            else {
                $scope.errors = [];
                showSuccessToast(result.data.message);
                $scope.modalInstance.close('ok');
            }
        }, errorMsg => {
            showErrorToast(errorMsg);
        });
    };
});


function getArray(_data) {
    return _data.constructor == Array ? _data : [_data];
}
