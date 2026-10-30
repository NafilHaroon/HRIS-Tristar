baseURL = "/HRIS/Process/BulkSupervisor/";
agGrid.initialiseAgGridWithAngular1(angular);
var app = angular.module('HRIS', ['ui.bootstrap', 'agGrid', 'ngMaterial']);
let session = "";

$(document).ready(function () {
    $('#ddBranch').focus();
});

app.controller('BulkSupervisorController', function ($scope, $rootScope, $http, $uibModal, $mdToast) {
    //#region form loading events
    $scope.formLoaded = function () {
        //$http({
        //    method: "GET",
        //    url: "/HRIS/Home/GetAccessRight",
        //    dataType: "JSON",
        //    params: { _optionName: "Bulk Supervisor Update" }
        //}).then(result => {
        //    session = result.data;
        //    setFormSession(session);
        //},
        //    errorMsg => {
        //        showErrorToast(errorMsg);
        //    });
        loadDropDown();
        hideFormLoading();
        $scope.ChangeSupervisor = { BranchCode: null, DepartmentCode: null, SupervisorId: null, NewSupervisorId: null};
    };

    function loadDropDown() {
        $http.get(baseURL + 'GetLookups').then(result => {
            $scope.lstBranch = result.data.lstBranch;
            $scope.lstDepartment = result.data.lstDepartment;
            $scope.lstSupervisor = result.data.lstSupervisor;
            $scope.lstEmployee = result.data.lstEmployee;
        }, errorMsg => {
            showErrorToast(errorMsg);
        });
    }
    //#endregion

    //#region toolbar actions
    $scope.tbSave = function (res) {
        showWritingMsg();
        $http({
            url: baseURL + "Save",
            data: $scope.ChangeSupervisor,
            method: "post"
        }).then(result => {
            hideWritingMsg();
            $scope.errors = [];
            if (!result.data.success) {
                $scope.errors = getArray(result.data.message);
                showErrorToast();
            }
            else {
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