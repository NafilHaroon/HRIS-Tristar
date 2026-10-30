baseURL = "/HRIS/Process/GenerateSalary/";
agGrid.initialiseAgGridWithAngular1(angular);
var app = angular.module('HRIS', ['ui.bootstrap', 'agGrid', 'ngMaterial']);
let session = "";

app.controller('GenerateSalaryController', function ($scope, $rootScope, $http, $uibModal, $mdToast) {
    //#region form loading events
    $scope.formLoaded = function () {
        //$http({
        //    method: "GET",
        //    url: "/HRIS/Home/GetAccessRight",
        //    dataType: "JSON",
        //    params: { _optionName: "Generate Salary" }
        //}).then(result => {
        //    session = result.data;
        //    setFormSession(session);
        //},
        //    errorMsg => {
        //        showErrorToast(errorMsg);
        //    });
        hideFormLoading();
        $scope.OverWrite = false;
    };
    //#endregion

    //#region toolbar actions
    $scope.tbGenerate = function () {
        showWritingMsg();
        $http({
            url: baseURL + "Generate",
            data: { _overwrite: $scope.OverWrite },//$scope.Overwrite
            method: "post"
        }).then(result => {
            $scope.errors = [];
            hideWritingMsg();
            if (!result.data.success) {
                $scope.errors = getArray(result.data.message);
                showErrorToast();
            }
            else {
                document.getElementById('btnGenerate').disabled = true;
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