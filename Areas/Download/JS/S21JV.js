baseURL = "/HRIS/Download/S21JV/";
agGrid.initialiseAgGridWithAngular1(angular);
var app = angular.module('HRIS', ['ui.bootstrap', 'agGrid', 'ngMaterial']); 
let session = "";

app.controller('S21JVController', function ($scope, $rootScope, $http, $uibModal, $mdToast) {
    //#region form loading events
    $scope.formLoaded = function () {
        //$http({
        //    method: "GET",
        //    url: "/HRIS/Home/GetAccessRight",
        //    dataType: "JSON",
        //    params: { _optionName: "S21 JV" }
        //}).then(result => {
        //    session = result.data;
        //    session.formMode = "Recall";
        //    setFormSession(session);
        //},
        //errorMsg => {
        //    showErrorToast(errorMsg);
        //});
        loadDropDown();
        hideFormLoading();
    };   

    function loadDropDown() {
        $http.get(baseURL + 'GetLookups').then(result => {
            $scope.lstPeriod = result.data.lstPeriod;
        }, errorMsg => {
            showErrorToast(errorMsg);
        });
    }
    //#endregion

    //#region toolbar actions
    $scope.tbExit = function () {
        $scope = null;
        window.location.href = '/HRIS/Common/Common/Index';
    };

    $scope.tbExtract = function () {
        if (document.getElementById('ddPeriod').value == '') {
            showErrorToast("Please select Salary Period before hitting Download Button!");
            return;
        }
        period = $('#ddPeriod').val().replace('number:', '');
        $http({
            type: "Get",
            url: baseURL + "Get",
            params: { _periodId: period },
            dataType: "JSON"
        }).then(result => {
            clearForm();
            if (!result.data.success) {
                showErrorToast(result.data.message);
            }
            else {
                $scope.S21JV = result.data.s21JV;
                setGridData();
            }
        }, errorMsg => {
            showErrorToast(result);
        });
    };

    $scope.tbExport = function () {
        if ($scope.grdOptS21JV.api.getDisplayedRowCount() == 0) {
            showErrorToast("There is nothing available to Export!");
            return;
        }
        var params = {
            skipHeader: false,
            allColumns: true,
            skipFooters: true,
            skipGroups: true,
            fileName: "S21JV.csv"
        };
        $scope.grdOptS21JV.api.exportDataAsCsv(params);
    };
    //#endregion

    //#region local function
    function clearForm() {
        $scope.S21JV = {};
        $scope.errors = [];
    }
    //#endregion

    //#region grid setup
    //#region WIP Deduction Grid Definition & functions
    var colS21JV = [
        {
            headerName: "AccountCode", field: "AccountCode", width: 125
        },
        {
            headerName: "Narration", field: "Narration", width: 450
        },
        {
            headerName: "Salary", field: "Salary", width: 150,
            type: "numericColumn", valueFormatter: formatNumbers, valueParser: numberValueParser 
        },
        {
            headerName: "Qty", field: "Qty", width: 110
        },
        {
            headerName: "Period", field: "Period", width: 100
        }
    ];

    $scope.grdOptS21JV = {
        headerHeight: 35,
        rowHeight: 32,
        animateRows: true,
        defaultColDef: {
            editable: false
        },
        columnDefs: colS21JV,
        rowData: [],
        rowSelection: 'single',
        rowDeselection: true,
        floatingFilter: true,
        enableColResize: true,
        enableSorting: true,
        singleClickEdit: false,
        suppressCellSelection: false,
        onGridReady: (grid) => {
        },
        overlayLoadingTemplate: '<span class="ag-overlay-loading-center">Please wait while we are fetching requested records from database</span>',
        overlayNoRowsTemplate: '<span style="padding: 10px; border: 2px solid #444; background: lightgoldenrodyellow; color:red;">No rows available to display here</span>'
    };    
    //#endregion       

    //#region general functions
    function setGridData() {
        if ($scope.S21JV != []) {
            $scope.grdOptS21JV.api.setRowData($scope.S21JV.Details);
        }
        else { $scope.grdOptS21JV.api.setRowData([]); }
    }
    //#endregion
    //#endregion
});