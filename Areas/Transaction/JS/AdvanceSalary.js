baseURL = "/HRIS/Transaction/AdvanceSalary/";
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
        if (document.getElementById('txtStatus').value != 'Open')
            $('#btnSave')[0].disabled = true;
        $('#ddEmployee').focus();
    });
});

app.controller('AdvanceSalaryController', function ($scope, $rootScope, $http, $uibModal, $mdToast) {
    //#region form loading events
    $scope.formLoaded = function () {
        $http({
            method: "GET",
            url: "/HRIS/Home/GetAccessRight",
            dataType: "JSON",
            params: { _optionName: "Advance Salary" }
        }).then(result => {
            session = result.data;
            setFormSession(session);
        },
            errorMsg => {
                showErrorToast(errorMsg);
            });
        loadDropDown();
        setDateParameters();
    };

    function loadDropDown() {
        $http.get(baseURL + 'GetLookups').then(result => {
            $scope.lstEmployee = result.data;
        }, errorMsg => {
            showErrorToast(errorMsg);
        });
    }
    //#endregion

    //#region toolbar actions
    $scope.tbAdd = function () {
        $http({
            method: "Get",
            url: baseURL + "Add"
        }).then(response => {
            $scope.AdvanceSalary = response.data.advanceSalary;
            $scope.Footer = $scope.AdvanceSalary.AuditTrail;
            session.FormMode = response.data.formMode;
            setFormSession(session);
            maskDates();
            $('#ddEmployee').focus();
        }, errorMsg => {
            showErrorToast(errorMsg);
        });
    };

    $scope.tbRecall = function () {
        session.FormMode = "Recall";
        setFormSession(session);
        $('#txtAdvanceId').focus();
    };

    $scope.tbSearch = function () {
        $scope.formTitle = "Search & Select Advance Salary";
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
                $scope.getAdvanceSalaryById($rootScope.SelectedRow.AdvanceId);
            }, function () {

            });
        }
        catch (exception) { showErrorToast(exception); }
    };

    $scope.tbSave = function () {
        $http({
            method: "POST",
            url: baseURL + "Save",
            data: $scope.AdvanceSalary
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
    $scope.getAdvanceSalaryById = function (res) {
        $http({
            type: "Get",
            url: baseURL + "Get",
            dataType: "JSON",
            params: { _keyId: res }
        }).then(result => {
            clearForm();
            if (!result.data.success) {
                showErrorToast(result.data.message);
                showErrorToast(result.data.message);
            }
            else {
                $scope.AdvanceSalary = result.data.advanceSalary;
                $scope.footer = $scope.AdvanceSalary.AuditTrail;
                session.FormMode = result.data.formMode;
                setFormSession(session);
                maskDates();
            }
        }, errorMsg => {
            showErrorToast(result);
        });
    };
    //#endregion

    //#region local function
    function clearForm() {
        $scope.AdvanceSalary = {};
        $scope.footer = {};
        $scope.errors = [];
    }
    //#endregion

    //#region date functions
    function setDateParameters() {
        $('#dpAdvance').datepicker({
            startDate: '-30d'
        }).on('changeDate', function (e) {
            $scope.AdvanceSalary.AdvanceDate = e.format();
            $scope.$apply();
        });
    }

    function maskDates() {
        $scope.AdvanceSalary.AdvanceDate = formatDate($scope.AdvanceSalary.AdvanceDate);
        if ($scope.AdvanceSalary.AuditTrail != null)
            $scope.AdvanceSalary.AuditTrail.CreatedOn = formatDateTime($scope.AdvanceSalary.AuditTrail.CreatedOn);
        if ($scope.AdvanceSalary.AuditTrail.UpdatedOn != null)
            $scope.AdvanceSalary.AuditTrail.UpdatedOn = formatDateTime($scope.AdvanceSalary.AuditTrail.UpdatedOn);
    }
    //#endregion
});