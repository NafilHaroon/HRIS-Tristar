baseURL = "/HRIS/Master/TaxSetup/";
agGrid.initialiseAgGridWithAngular1(angular);
var app = angular.module('HRIS', ['ui.bootstrap', 'agGrid', 'ngMaterial']);
let session = "";

$(document).ready(function () {
    $("#btnEdit").click(function () {
        session.FormMode = "Edit";
        setFormSession(session);
    });
});

app.controller('TaxSetupController', function ($scope, $rootScope, $http, $uibModal, $mdToast) {
    //#region form loading events
    $scope.formLoaded = function () {
        $http({
            method: "GET",
            url: "/HRIS/Home/GetAccessRight",
            dataType: "JSON",
            params: { _optionName: "Tax Setup" }
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
            $scope.lstTaxYear = result.data.lstTaxYear;
        }, errorMsg => {
            showErrorToast(errorMsg);
        });
    }
    //#endregion

    //#region toolbar actions
    $scope.tbRecall = function () {
        session.FormMode = "Recall";
        setFormSession(session);
        $('#ddPeriod').focus();
    };

    $scope.tbSave = function () {
        $scope.TaxSetup.Details = getTaxSetupDataFromGrid();
        $http({
            method: "POST",
            url: baseURL + "Save",
            data: $scope.TaxSetup
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
    $scope.getTaxSetupByYear = function (_periodId) {
        $http({
            type: "Get",
            url: baseURL + "Get",
            dataType: "JSON",
            params: { _keyId: _periodId }
        }).then(result => {
            clearForm();
            if (!result.data.success) {
                $scope.errors = getArray(result.data.message);
                showErrorToast(result.data.message);
            }
            else {
                $scope.TaxSetup = result.data.TaxSetup;
                session.FormMode = result.data.formMode;
                setFormSession(session);
                maskDates();
                setGridData();
            }
        }, errorMsg => {
            showErrorToast(errorMsg);
        });
    };
    //#endregion

    //#region local function
    function clearForm() {
        $scope.TaxSetup = {};
        $scope.errors = [];
    }
    //#endregion

    //#region date functions
    function maskDates() {
        if ($scope.TaxSetup.AuditTrail != null) {
            $scope.TaxSetup.AuditTrail.CreatedOn = formatDateTime($scope.TaxSetup.AuditTrail.CreatedOn);
            if ($scope.TaxSetup.AuditTrail.UpdatedOn != null) {
                $scope.TaxSetup.AuditTrail.UpdatedOn = formatDateTime($scope.TaxSetup.AuditTrail.UpdatedOn);
            }
        }
    }
    //#endregion

    //#region grid setup
    //#region TaxSetup Grid Definition & functions
    var colTaxSetup = [
        {
            headerName: "Slab From", field: "IncomeFrom", type: "numericColumn",
            valueFormatter: formatNumbers, valueParser: numberValueParser, width: 140
        },
        {
            headerName: "Slab To", field: "IncomeTo", type: "numericColumn",
            valueFormatter: formatNumbers, valueParser: numberValueParser, width: 140
        },
        {
            headerName: "Base Tax", field: "BaseTax", type: "numericColumn",
            valueFormatter: formatNumbers, valueParser: numberValueParser, width: 140
        },
        {
            headerName: "Tax Rate", field: "TaxRate", type: "numericColumn",
            valueFormatter: formatNumbers, valueParser: numberValueParser, width: 100
        },
        { headerName: "DetailId", field: "DetailId", hide: true, suppressToolPanel: true },
        { headerName: "Add", field: "Add", hide: true, suppressToolPanel: true },
        { headerName: "Edit", field: "Edit", hide: true, suppressToolPanel: true },
        { headerName: "Delete", field: "Delete", hide: true, suppressToolPanel: true }
    ];

    $scope.grdOptTaxSetup = {
        headerHeight: 25,
        rowHeight: 32,
        animateRows: true,
        defaultColDef: {
            editable: true
        },
        columnDefs: colTaxSetup,
        rowData: [],
        rowSelection: 'single',
        rowDeselection: true,
        enableColResize: true,
        enableSorting: true,
        singleClickEdit: true,
        stopEditingWhenGridLosesFocus: true,
        onCellValueChanged: function (params) {
            if (!params.data.Add)
                params.data.Edit = true;
        },
        overlayLoadingTemplate: '<span class="ag-overlay-loading-center">Please wait while we are fetching requested records from database</span>',
        overlayNoRowsTemplate: '<span style="padding: 10px; border: 2px solid #444; background: lightgoldenrodyellow; color:red;">No rows available to display here</span>'
    };

    function getTaxSetupDataFromGrid() {
        let rowData = [];
        $scope.grdOptTaxSetup.api.forEachNode(node => rowData.push(node.data));
        return rowData;
    }

    $scope.onAddTaxSlab = function () {
        try {
            var res = $scope.grdOptTaxSetup.api.updateRowData({
                add: [{
                    DetailId: -1, IncomeFrom: 0, IncomeTo: 0, BaseTax: 0, TaxRate: 0, Add: true, Edit: false, Delete: false
                }]
            });
            $scope.grdOptTaxSetup.api.startEditingCell({ rowIndex: res.add[0].rowIndex, colKey: "IncomeFrom" });
        }
        catch (exception) { showErrorToast('Add Tax Slab: ' + exception); }
    };

    $scope.onDeleteTaxSlab = function () {
        try {
            if ($scope.grdOptTaxSetup.api.getSelectedRows().length > 0) {
                if (confirm("Are you sure you want to Delete selected row?")) {
                    $scope.grdOptTaxSetup.api.getSelectedRows().forEach(x => x.Delete = true);
                    $scope.grdOptTaxSetup.api.getFilterInstance('Delete').onFilterChanged();
                }
            }
            else
                showErrorToast("No Row selected to apply Delete operation. Please select row and then hit Delete button!");
        }
        catch (exception) { showErrorToast('Delete Tax Slab: ' + exception); }
    };
    //#endregion       

    //#region general functions
    function setGridData() {
        if ($scope.TaxSetup != []) {
            $scope.grdOptTaxSetup.api.setRowData($scope.TaxSetup.Details);
            setGridDeleteFilter($scope.grdOptTaxSetup.api);
        }
        else { $scope.grdOptTaxSetup.api.setRowData([]); }
    }
     //#endregion
    //#endregion
});