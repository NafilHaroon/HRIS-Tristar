baseURL = "/HRIS/Master/SalaryHead/";
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
        document.getElementById('ddHeadType').disabled = true;
        $('#txtHeadName').focus();
    });
});

app.controller('SalaryHeadController', function ($scope, $rootScope, $http, $uibModal, $mdToast) {
    //#region form loading events
    $scope.formLoaded = function () {
        $http({
            method: "GET",
            url: "/HRIS/Home/GetAccessRight",
            dataType: "JSON",
            params: { _optionName: "Salary Head" }
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
            $scope.lstHeadType = result.data.lstHeadType;
            $scope.lstWorkerType = result.data.lstWorkerType;
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
            $scope.SalaryHead = response.data.salaryHead;
            $scope.Footer = $scope.SalaryHead.AuditTrail;
            session.FormMode = response.data.formMode;
            setFormSession(session);
            setGridData();
            maskDates();
            $('#txtHeadName').focus();
        }, errorMsg => {
            showErrorToast(errorMsg);
        });
    };

    $scope.tbRecall = function () {
        session.FormMode = "Recall";
        setFormSession(session);
        $('#txtHeadId').focus();
    };

    $scope.tbSearch = function () {
        $scope.formTitle = "Search & Select Salary Head";
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
                $scope.getSalaryHeadById($rootScope.SelectedRow.HeadId);
            }, function () {

            });
        }
        catch (exception) { showErrorToast(exception); }
    };

    $scope.tbSave = function () {
        $scope.SalaryHead.Details = getWorkerTypeDataFromGrid();
        $http({
            method: "POST",
            url: baseURL + "Save",
            data: $scope.SalaryHead
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
                setGridData();
            }
        }, errorMsg => {
            showErrorToast(errorMsg);
        });
    };

    $scope.tbUndo = function () {
        clearForm();
        setGridData();
        session.FormMode = "Initialize";
        setFormSession(session);
    };

    $scope.tbExit = function () {
        $scope = null;
        window.location.href = '/HRIS/Common/Common/Index';
    };
    //#endregion

    //#region form functions
    $scope.getSalaryHeadById = function (res) {
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
                $scope.SalaryHead = result.data.salaryHead;
                $scope.footer = $scope.SalaryHead.AuditTrail;
                session.FormMode = result.data.formMode;
                setFormSession(session);
                setGridData();
                maskDates();
            }
        }, errorMsg => {
            showErrorToast(errorMsg);
        });
    };
    //#endregion

    //#region local function
    function clearForm() {
        $scope.SalaryHead = {};
        $scope.footer = {};
        $scope.errors = [];
    }
    //#endregion

    //#region date functions
    function maskDates() {
        if ($scope.SalaryHead.AuditTrail != null) {
            $scope.SalaryHead.AuditTrail.CreatedOn = formatDateTime($scope.SalaryHead.AuditTrail.CreatedOn);
            if ($scope.SalaryHead.AuditTrail.UpdatedOn != null) {
                $scope.SalaryHead.AuditTrail.UpdatedOn = formatDateTime($scope.SalaryHead.AuditTrail.UpdatedOn);
            }
        }
    }
    //#endregion

    //#region grid setup
    //#region Work Type Grid Definition & functions
    var colWorkerType = [
        {
            headerName: "Worker Type", field: "WorkerTypeId", cellEditor: "agilitySelectEditor",
            cellEditorParams: { source: 'WorkerType', class: "agGridSelect tb130" },
            valueFormatter: getWorkerTypeName, width: 130
        },
        {
            headerName: "Debit A/C", field: "DebitAccount", type: "numericColumn",
            valueParser: numberValueParser, width: 120
        },
        {
            headerName: "Credit A/C", field: "CreditAccount", type: "numericColumn",
            valueParser: numberValueParser, width: 120
        },
        { headerName: "Add", field: "Add", hide: true, suppressToolPanel: true },
        { headerName: "Edit", field: "Edit", hide: true, suppressToolPanel: true },
        { headerName: "Delete", field: "Delete", hide: true, suppressToolPanel: true }
    ];    

    $scope.grdOptWorkerType = {
        headerHeight: 25,
        rowHeight: 32,
        animateRows: true,
        defaultColDef: {
            editable: allowEdit.bind(this)
        },
        columnDefs: colWorkerType,
        rowData: [],
        rowSelection: 'single',
        rowDeselection: true,
        enableColResize: true,
        enableSorting: true,
        singleClickEdit: true,       
        onCellValueChanged: function (params) {
            if (!params.data.Add)
                params.data.Edit = true;
        },
        overlayLoadingTemplate: '<span class="ag-overlay-loading-center">Please wait while we are fetching requested records from database</span>',
        overlayNoRowsTemplate: '<span style="padding: 10px; border: 2px solid #444; background: lightgoldenrodyellow; color:red;">No rows available to display here</span>',
        components: {
            agilitySelectEditor: getAgilitySelect()
        }
    };

    function getWorkerTypeDataFromGrid() {
        let rowData = [];
        $scope.grdOptWorkerType.api.forEachNode(node => rowData.push(node.data));
        return rowData;
    }

    $scope.onAddWorkerType = function () {
        try {
            var res = $scope.grdOptWorkerType.api.updateRowData({
                add: [{
                    WorkerTypeId: null,  DebitAccount: null, CreditAccount: null, Add: true, Edit: false, Delete: false
                }]
            });
            $scope.grdOptWorkerType.api.startEditingCell({ rowIndex: res.add[0].rowIndex, colKey: "WorkerTypeId" });
        }
        catch (exception) { showErrorToast('Add Work Type: ' + exception); }
    };

    $scope.onDeleteWorkerType = function () {
        try {
            if ($scope.grdOptWorkerType.api.getSelectedRows().length > 0) {
                if (confirm("Are you sure you want to Delete selected row?")) {
                    $scope.grdOptWorkerType.api.getSelectedRows().forEach(x => x.Delete = true);
                    $scope.grdOptWorkerType.api.getFilterInstance('Delete').onFilterChanged();
                }
            }
            else
                showErrorToast("No Row selected to apply Delete operation. Please select row and then hit Delete button!");
        }
        catch (exception) { showErrorToast('Delete Work Type: ' + exception); }
    };
  
    //#endregion       

    //#region general functions
    function setGridData() {
        if ($scope.SalaryHead != []) {
            $scope.grdOptWorkerType.api.setRowData($scope.SalaryHead.Details);
            setGridDeleteFilter($scope.grdOptWorkerType.api);
        }
        else { $scope.grdOptWorkerType.api.setRowData([]); }        
    }

    $scope.sum = function (items, prop) {
        return items.reduce(function (a, b) {
            return a + b[prop];
        }, 0);
    };
    //#endregion
    //#endregion
});