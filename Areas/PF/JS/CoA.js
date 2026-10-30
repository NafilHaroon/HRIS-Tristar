baseURL = "/HRIS/PF/CoA/";
agGrid.initialiseAgGridWithAngular1(angular);
var app = angular.module('HRIS', ['ui.bootstrap', 'agGrid', 'ngMaterial']);
let session = "";

$(document).ready(function () {
    $("#btnEdit").click(function () {
        session.FormMode = "Edit";
        setFormSession(session);
        document.getElementById("ddAccountType").disabled = true;
        document.getElementById("txtOpeningBalance").disabled = true;
        $('#txtAccountName').focus();
    });
});

app.controller('CoAController', function ($scope, $rootScope, $http, $uibModal, $mdToast) {
    //#region form loading events
    $scope.formLoaded = function () {
        $http({
            method: "GET",
            url: "/HRIS/Home/GetAccessRight",
            dataType: "JSON",
            params: { _optionName: "Chart of Accounts" }
        }).then(result => {
            session = result.data;
            setFormSession(session);
        },
            errorMsg => {
                showErrorToast(errorMsg);
            });
        loadDropDown();
    };

    function loadDropDown() {
        $http.get(baseURL + 'GetLookups').then(result => {
            $scope.lstAccountType = result.data.lstAccountType;
        }, errorMsg => {
            showErrorToast('Main dropdown loading function failed with following reasons\n' + errorMsg);
        });
    }   
    //#endregion

    //#region toolbar actions
    $scope.tbAdd = function () {
        $http({
            method: "Get",
            url: baseURL + "Add"
        }).then(response => {
            $scope.CoA = response.data.CoA;
            $scope.Footer = response.data.CoA.AuditTrail;
            session.FormMode = response.data.formMode;
            setFormSession(session);
            maskDates();
            $('#txtAccountName').focus();
        }, errorMsg => {
            showErrorToast(errorMsg);
        });
    };

    $scope.tbRecall = function () {
        session.FormMode = "Recall";
        setFormSession(session);
        $('#txtAccountCode').focus();
    };

    $scope.tbSearch = function () {
        $scope.formTitle = "Search & Select CoA";
        try {
            var modalScope = $scope.$new();
            var modalInstance = $uibModal.open({
                animation: true,
                templateUrl: '/HRIS/Helper/Pages/Search.html',
                windowClass: 'show',
                backdropClass: 'show',
                size: "lg",
                scope: modalScope
            });
            modalScope.modalInstance = modalInstance;

            modalInstance.result.then(function () {
                $scope.getCoAById($rootScope.SelectedRow.AccountCode);
            }, function () {

            });
        }
        catch (exception) { showErrorToast(exception); }
    };

    $scope.tbSave = function () {
        showWritingMsg();
        $http({
            method: "POST",
            url: baseURL + "Save",
            data: $scope.CoA
        }).then(result => {
            hideWritingMsg();
            $scope.errors = [];
            if (!result.data.success) {
                $scope.errors = getArray(result.data.message);
                showErrorToast();
            }
            else {
                showSuccessToast(result.data.message);
                session.FormMode = result.data.formMode;
                setFormSession(session);
                clearForm();
            }
        }, errorMsg => {
            hideWritingMsg();
            showErrorToast(errorMsg);
        });
    };

    $scope.tbUndo = function () {
        clearForm();
        session.FormMode = "Initialize";
        setFormSession(session);
    };

    $scope.tbExit = function () {
        $scope = null;
        window.location.href = '/HRIS/Common/Common/Index';
    };
    
    $scope.getCoAById = function (res) {
        $http({
            type: "Get",
            url: baseURL + "Get",
            dataType: "JSON",
            params: { _keyId: res }
        }).then(result => {
            clearForm();
            if (!result.data.success) {
                $scope.errors = getArray(result.data.message);
                showErrorToast();
            }
            else {
                $scope.CoA = result.data.CoA;
                $scope.footer = $scope.CoA.AuditTrail;
                session.FormMode = result.data.formMode;
                maskDates();
                setFormSession(session);
            }
        }, errorMsg => {
            showErrorToast(errorMsg);
        });
    };
    //#endregion

    //#region local function
    function clearForm() {
        $scope.CoA = {};
        $scope.footer = {};
        $scope.errors = [];
    }
    //#endregion

    //#region date functions
    function maskDates() {
        if ($scope.CoA.AuditTrail != null) {
            $scope.CoA.AuditTrail.CreatedOn = formatDateTime($scope.CoA.AuditTrail.CreatedOn);
            if ($scope.CoA.AuditTrail.UpdatedOn != null) {
                $scope.CoA.AuditTrail.UpdatedOn = formatDateTime($scope.CoA.AuditTrail.UpdatedOn);
            }
        }
    }
    //#endregion 
});