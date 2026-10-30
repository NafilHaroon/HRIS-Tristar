baseURL = "/HRIS/Master/Company/";
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
        $('#txtCompanyName').focus();
    });

    $('#imgLogo').click(function () { $('#btnLogo').trigger('click'); });
});

app.controller('CompanyController', function ($scope, $rootScope, $http, $uibModal, $mdToast) {
    //#region form loading events
    $scope.formLoaded = function () {
        $http({
            method: "GET",
            url: "/HRIS/Home/GetAccessRight",
            dataType: "JSON",
            params: { _optionName: "Company" }
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
            $scope.lstPFAccount = result.data.lstPFAccount;
            $scope.lstBankAccount = result.data.lstBankAccount;
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
            $scope.Company = response.data.company;
            $scope.Footer = $scope.Company.AuditTrail;
            session.FormMode = response.data.formMode;
            setFormSession(session);
            maskDates();
            $('#txtCompanyName').focus();
        }, errorMsg => {
            showErrorToast(errorMsg);
        });
    };

    $scope.tbRecall = function () {
        session.FormMode = "Recall";
        setFormSession(session);
        $('#txtCompanyId').focus();
    };

    $scope.tbSearch = function () {
        $scope.formTitle = "Search & Select Company";
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
                $scope.getCompanyById($rootScope.SelectedRow.CompanyId);
            }, function () {

            });
        }
        catch (exception) { showErrorToast(exception); }
    };

    $scope.tbSave = function () {
        $http({
            method: "POST",
            url: baseURL + "Save",
            data: $scope.Company
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
    $scope.getCompanyById = function (res) {
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
                $scope.Company = result.data.company;
                $scope.footer = $scope.Company.AuditTrail;
                session.FormMode = result.data.formMode;
                setFormSession(session);
                maskDates();
            }
        }, errorMsg => {
            showErrorToast(errorMsg);
        });
    };

    $scope.imageUpload = function (event) {
        if (event.target.files.length > 0) {
            var reader = new FileReader();
            reader.onload = $scope.updateImage;
            reader.readAsDataURL(event.target.files[0]);
        }
    };

    $scope.updateImage = function (e) {
        $scope.$apply(function () {
            $scope.Company.CompanyLogo = e.target.result.split(',').pop();
        });
    };
    //#endregion

    //#region local function
    function clearForm() {
        $scope.Company = {};
        $scope.footer = {};
        $scope.errors = [];
        document.getElementById('imgLogo').setAttribute("src", "");
    }
    //#endregion

    //#region date functions
    function maskDates() {
        if ($scope.Company.AuditTrail != null)
            $scope.Company.AuditTrail.CreatedOn = formatDateTime($scope.Company.AuditTrail.CreatedOn);
        if ($scope.Company.AuditTrail.UpdatedOn != null)
            $scope.Company.AuditTrail.UpdatedOn = formatDateTime($scope.Company.AuditTrail.UpdatedOn);
    }
    //#endregion
});