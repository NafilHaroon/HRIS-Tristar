baseURL = "/HRIS/Master/DocumentType/";
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
        $('#txtDocumentTypeName').focus();
    });
});

app.controller('DocumentTypeController', function ($scope, $rootScope, $http, $uibModal, $mdToast) {
    //#region form loading events
    $scope.formLoaded = function () {
        $http({
            method: "GET",
            url: "/HRIS/Home/GetAccessRight",
            dataType: "JSON",
            params: { _optionName: "Document Type" }
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
            $scope.DocumentType = response.data.documentType;
            $scope.Footer = response.data.documentType.AuditTrail;
            session.FormMode = response.data.formMode;
            setFormSession(session);
            maskDates();
            $('#txtDocumentTypeName').focus();
        }, errorMsg => {
            showErrorToast(errorMsg);
        });
    };

    $scope.tbRecall = function () {
        session.FormMode = "Recall";
        setFormSession(session);
        $('#txtDocumentTypeId').focus();
    };

    $scope.tbSearch = function () {
        $scope.formTitle = "Search & Select Document Type";
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
                $scope.getDocumentTypeById($rootScope.SelectedRow.DocumentTypeId);
            }, function () {

            });
        }
        catch (exception) { showErrorToast(exception); }
    };

    $scope.tbSave = function (res) {
        $http({
            method: "POST",
            url: baseURL + "Save",
            data: $scope.DocumentType
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
    $scope.getDocumentTypeById = function (res) {
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
                $scope.DocumentType = result.data.documentType;
                $scope.footer = $scope.DocumentType.AuditTrail;
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
        $scope.DocumentType = {};
        $scope.footer = {};
        $scope.errors = [];
    }
    //#endregion

    //#region date functions
    function maskDates() {
        if ($scope.DocumentType.AuditTrail != null) {
            $scope.DocumentType.AuditTrail.CreatedOn = formatDateTime($scope.DocumentType.AuditTrail.CreatedOn);
            if ($scope.DocumentType.AuditTrail.UpdatedOn != null) {
                $scope.DocumentType.AuditTrail.UpdatedOn = formatDateTime($scope.DocumentType.AuditTrail.UpdatedOn);
            }
        }
    }
    //#endregion   
});