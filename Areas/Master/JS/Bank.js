baseURL = "/HRIS/Master/Bank/";
agGrid.initialiseAgGridWithAngular1(angular);
var app = angular.module('HRIS', ['ui.bootstrap', 'agGrid', 'ngMaterial']); 
let session = "";

app.directive('numbersOnly', function () {
    return {
        require: 'ngModel',
        link: function (scope, element, attr, ngModelCtrl) {
            function fromUser(text) {
                if (text) {
                    var transformedInput = text.replace(/[^0-9]/g, '');

                    if (transformedInput !== text) {
                        ngModelCtrl.$setViewValue(transformedInput);
                        ngModelCtrl.$render();
                    }
                    return transformedInput;
                }
                return undefined;
            }
            ngModelCtrl.$parsers.push(fromUser);
        }
    };
});

$(document).ready(function () {
    $("#btnEdit").click(function () {
        session.FormMode = "Edit";
        setFormSession(session);
        $('#txtBankName').focus();
    });
});

app.controller('BankController', function ($scope, $rootScope, $http, $uibModal, $mdToast) {
    //#region form loading events
    $scope.formLoaded = function () {
        $http({
            method: "GET",
            url: "/HRIS/Home/GetAccessRight",
            dataType: "JSON",
            params: { _optionName: "Bank" }
        }).then(result => {
            session = result.data;
            setFormSession(session);
        },
        errorMsg => {
            showErrorToast(errorMsg);
        });
    };
    //#endregion

    //#region toolbar actions
    $scope.tbAdd = function () {
        $http({
            method: "Get",
            url: baseURL + "Add"
        }).then(response => {
            $scope.Bank = response.data.bank;
            $scope.Footer = response.data.bank.AuditTrail;
            session.FormMode = response.data.formMode;
            setFormSession(session);
            maskDates();
            $('#txtBankName').focus();
        }, errorMsg => {
            showErrorToast(errorMsg);
        });
    };

    $scope.tbRecall = function () {
        session.FormMode = "Recall";
        setFormSession(session);
        $('#txtBankId').focus();
    };

    $scope.tbSearch = function () {
        $scope.formTitle = "Search & Select Bank";
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
                $scope.getBankById($rootScope.SelectedRow.BankId);
            }, function () {
                
            });
        }
        catch (exception) { showErrorToast(exception); }
    };

    $scope.tbSave = function () {
        $http({
            method: "POST",
            url: baseURL + "Save",
            data: $scope.Bank
        }).then(result => {
            $scope.errors = [];
            if (!result.data.success) {
                $scope.errors = getArray(result.data.message);
                showErrorToast();
            }
            else {
                showSuccessToast(result.data.message);
                session.FormMode = result.data.formMode;
                clearForm();
                setFormSession(session);
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
    $scope.getBankById = function (res) {
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
                $scope.Bank = result.data.bank;
                $scope.footer = $scope.Bank.AuditTrail;
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
        $scope.Bank = {};
        $scope.footer = {};
        $scope.errors = [];
    }
    //#endregion

    //#region date functions
    function maskDates() {
        if ($scope.Bank.AuditTrail != null)
            $scope.Bank.AuditTrail.CreatedOn = formatDateTime($scope.Bank.AuditTrail.CreatedOn);
        if ($scope.Bank.AuditTrail.UpdatedOn != null)
            $scope.Bank.AuditTrail.UpdatedOn = formatDateTime($scope.Bank.AuditTrail.UpdatedOn);
    }
    //#endregion
});