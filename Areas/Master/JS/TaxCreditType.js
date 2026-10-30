baseURL = "/HRIS/Master/TaxCreditType/";
agGrid.initialiseAgGridWithAngular1(angular);
var app = angular.module('HRIS', ['ui.bootstrap', 'agGrid', 'ngMaterial']);
let session = "";

$(document).ready(function () {
    $("#btnEdit").click(function () {
        session.FormMode = "Edit";
        setFormSession(session);
        $('#txtTaxCreditTypeName').focus();
    });
});

app.controller('TaxCreditTypeController', function ($scope, $rootScope, $http, $uibModal, $mdToast) {
    //#region form loading events
    $scope.formLoaded = function () {
        $http({
            method: "GET",
            url: "/HRIS/Home/GetAccessRight",
            dataType: "JSON",
            params: { _optionName: "Tax Credit Type" }
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
        try {
            $http.get(baseURL + 'GetLookups').then(result => {
                $scope.lstCreditRule = result.data.lstCreditRule;
            }, errorMsg => {
                showErrorToast(errorMsg);
            });
        }
        catch (exception) { showErrorToast('Load Drop Down:' + exception.message); }
    }
    //#endregion

    //#region toolbar actions
    $scope.tbAdd = function () {
        $http({
            method: "Get",
            url: baseURL + "Add"
        }).then(response => {
            $scope.TaxCreditType = response.data.creditType;
            $scope.Footer = response.data.creditType.AuditTrail;
            session.FormMode = response.data.formMode;
            setFormSession(session);
            maskDates();
            $('#txtTaxCreditTypeName').focus();
        }, errorMsg => {
            showErrorToast(errorMsg);
        });
    };

    $scope.tbRecall = function () {
        session.FormMode = "Recall";
        setFormSession(session);
        $('#txtTaxCreditTypeId').focus();
    };

    $scope.tbSearch = function () {
        $scope.formTitle = "Search & Select Tax Credit Type";
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
                $scope.getTaxCreditTypeById($rootScope.SelectedRow.TaxCreditTypeId);
            }, function () {

            });
        }
        catch (exception) { showErrorToast(exception); }
    };

    $scope.tbSave = function (res) {
        $http({
            method: "POST",
            url: baseURL + "Save",
            data: $scope.TaxCreditType
        }).then(result => {
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
    //#endregion

    //#region form functions
    $scope.getTaxCreditTypeById = function (res) {
        $http({
            type: "Get",
            url: baseURL + "Get",
            dataType: "JSON",
            params: { _keyId: res }
        }).then(result => {
            clearForm();
            if (!result.data.success) {
                $scope.errors = getArray(result.data.message);
                showErrorToast(result.data.message);
            }
            else {
                $scope.TaxCreditType = result.data.creditType;
                $scope.footer = $scope.TaxCreditType.AuditTrail;
                session.FormMode = result.data.formMode;
                setFormSession(session);
                maskDates();
            }
        }, errorMsg => {
            showErrorToast(errorMsg);
        });
    };
    //#endregion

    //#region local function
    function clearForm() {
        $scope.TaxCreditType = {};
        $scope.footer = {};
        $scope.errors = [];
    }
    //#endregion

    //#region date functions
    function maskDates() {
        if ($scope.TaxCreditType.AuditTrail != null) {
            $scope.TaxCreditType.AuditTrail.CreatedOn = formatDateTime($scope.TaxCreditType.AuditTrail.CreatedOn);
            if ($scope.TaxCreditType.AuditTrail.UpdatedOn != null) {
                $scope.TaxCreditType.AuditTrail.UpdatedOn = formatDateTime($scope.TaxCreditType.AuditTrail.UpdatedOn);
            }
        }
    }
    //#endregion   
});