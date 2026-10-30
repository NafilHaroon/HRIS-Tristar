baseURL = "/HRIS/Master/Qualification/";
agGrid.initialiseAgGridWithAngular1(angular);
var app = angular.module('HRIS', ['ui.bootstrap', 'agGrid', 'ngMaterial']);
let session = "";

$(document).ready(function () {
    $("#btnEdit").click(function () {
        session.FormMode = "Edit";
        setFormSession(session);
        $('#txtQualificationName').focus();
    });
});

app.controller('QualificationController', function ($scope, $rootScope, $http, $uibModal, $mdToast) {
    //#region form loading events
    $scope.formLoaded = function () {
        $http({
            method: "GET",
            url: "/HRIS/Home/GetAccessRight",
            dataType: "JSON",
            params: { _optionName: "Qualification" }
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
            $scope.Qualification = response.data.qualification;
            $scope.Footer = response.data.qualification.AuditTrail;
            session.FormMode = response.data.formMode;
            maskDates();
            setFormSession(session);
            $('#txtQualificationName').focus();
        }, errorMsg => {
            showErrorToast(errorMsg);
        });
    };

    $scope.tbRecall = function () {
        session.FormMode = "Recall";
        setFormSession(session);
        $('#txtQualificationId').focus();
    };

    $scope.tbSearch = function () {
        $scope.formTitle = "Search & Select Qualification";
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
                $scope.getQualificationById($rootScope.SelectedRow.QualificationId);
            }, function () {
                
            });
        }
        catch (exception) { showErrorToast(exception); }
    };

    $scope.tbSave = function () {
        $http({
            method: "POST",
            url: baseURL + "Save",
            data: $scope.Qualification
        }).then(result => {
            $scope.errors = [];
            if (!result.data.success) {
                $scope.errors = getArray(result.data.message);
                showErrorToast(result.data.message);
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

    $scope.getQualificationById = function (res) {
        $http({
            type: "Get",
            url: baseURL + "Get",
            dataType: "JSON",
            params: { _keyId: res }
        }).then(result => {
            clearForm();
            if (!result.data.success) {
                $scope.errors = getArray(result.data.message);
            }
            else {
                $scope.Qualification = result.data.qualification;
                $scope.footer = $scope.Qualification.AuditTrail;
                session.FormMode = result.data.formMode;
                maskDates();
                setFormSession(session);
            }
        }, errorMsg => {
            showErrorToast(result);
        });
    };
    //#endregion

    //#region local function
    function clearForm() {
        $scope.Qualification = {};
        $scope.footer = {};
        $scope.errors = [];
    }
    //#endregion

    //#region date functions
    function maskDates() {
        if ($scope.Qualification.AuditTrail != null)
            $scope.Qualification.AuditTrail.CreatedOn = formatDateTime($scope.Qualification.AuditTrail.CreatedOn);
        if ($scope.Qualification.AuditTrail.UpdatedOn != null)
            $scope.Qualification.AuditTrail.UpdatedOn = formatDateTime($scope.Qualification.AuditTrail.UpdatedOn);
    }
    //#endregion
});