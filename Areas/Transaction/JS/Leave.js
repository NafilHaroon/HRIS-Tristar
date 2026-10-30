baseURL = "/HRIS/Transaction/Leave/";
agGrid.initialiseAgGridWithAngular1(angular);
var app = angular.module('HRIS', ['ui.bootstrap', 'agGrid', 'ngMaterial']);
let session = "";

app.controller('LeaveController', function ($scope, $rootScope, $http, $uibModal, $mdToast) {
    //#region form loading events
    $scope.formLoaded = function () {
        $http({
            method: "GET",
            url: "/HRIS/Home/GetAccessRight",
            dataType: "JSON",
            params: { _optionName: "Leave Entry" }
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
            $scope.lstEmployee = result.data.lstEmployee;
            $scope.lstLeaveType = result.data.lstLeaveType;
        }, errorMsg => {
            showErrorToast(errorMsg.message);
        });
    }
    //#endregion

    //#region toolbar actions
    $scope.tbAdd = function () {
        $http({
            method: "Get",
            url: baseURL + "Add"
        }).then(response => {
            $scope.Leave = response.data.leave;
            $scope.Footer = $scope.Leave.AuditTrail;
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
        $('#txtReferenceId').focus();
    };

    $scope.tbSave = function () {
        $http({
            method: "POST",
            url: baseURL + "Save",
            data: $scope.Leave
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
            showErrorToast(errorMsg.message);
        });
    };

    $scope.tbSearch = function () {
        $scope.formTitle = "Search & Select Leave Entries";
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
                $scope.getLeaveById($rootScope.SelectedRow.DetailId);
            }, function () {

            });
        }
        catch (exception) { showErrorToast(exception); }
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
    $scope.getLeaveById = function (_detailId) {
        $http({
            type: "Get",
            url: baseURL + "Get",
            dataType: "JSON",
            params: { _keyId: _detailId }
        }).then(result => {
            clearForm();
            if (!result.data.success) {
                $scope.errors = getArray(result.data.message);
                showErrorToast(result.data.message);
            }
            else {
                $scope.Leave = result.data.leave;
                session.FormMode = result.data.formMode;
                setFormSession(session);
                maskDates();
            }
        }, errorMsg => {
            showErrorToast(errorMsg.message);
        });
    };
    //#endregion

    //#region local function
    function clearForm() {
        $scope.Leave = {};
        $scope.errors = [];
    }    
    //#endregion

    //#region date functions
    function setDateParameters() {
        $('#dpFrom').datepicker({
            startDate: '-30d'
        }).on('changeDate', function (e) {
            $scope.Leave.DateFrom = e.format();
            $scope.$apply();
        });

        $('#dpTo').datepicker({
            startDate: '-30d'
        }).on('changeDate', function (e) {
            $scope.Leave.DateTo = e.format();
            $scope.$apply();
        });
    }

    function maskDates() {
        $scope.Leave.DateFrom = formatDate($scope.Leave.DateFrom);
        $scope.Leave.DateTo = formatDate($scope.Leave.DateTo);
        if ($scope.Leave.AuditTrail != null)
            $scope.Leave.AuditTrail.CreatedOn = formatDateTime($scope.Leave.AuditTrail.CreatedOn);
    }
    //#endregion
});