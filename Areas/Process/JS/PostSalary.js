baseURL = "/HRIS/Process/PostSalary/";
agGrid.initialiseAgGridWithAngular1(angular);
var app = angular.module('HRIS', ['ui.bootstrap', 'agGrid', 'ngMaterial']);
let session = "";

app.controller('PostSalaryController', function ($scope, $rootScope, $http, $uibModal, $mdToast) {
    //#region Form loading events
    $scope.formLoaded = function () {
        //$http({
        //    method: "GET",
        //    url: "/HRIS/Home/GetAccessRight",
        //    dataType: "JSON",
        //    params: { _optionName: "Post Salary" }
        //}).then(result => {
        //    session = result.data;
        //    setFormSession(session);
        //},
        //    errorMsg => {
        //        showErrorToast(errorMsg);
        //    });
        hideFormLoading();
    };
    //#endregion

    //#region toolbar actions
    $scope.tbPost = function (res) {
        showWritingMsg();
        $http({
            method: "POST",
            url: baseURL + "Post"
        }).then(result => {
            $scope.errors = [];
            hideWritingMsg();
            if (!result.data.success) {
                $scope.errors = getArray(result.data.message);
                showErrorToast();
            }
            else {
                document.getElementById('btnPost').disabled = true;
                showSuccessToast(result.data.message);
            }
        }, errorMsg => {
            hideWritingMsg();
            showErrorToast(errorMsg);
        });
    };

    $scope.tbExit = function () {
        $scope = null;
        window.location.href = '/HRIS/Common/Common/Index';
    };
    //#endregion
});