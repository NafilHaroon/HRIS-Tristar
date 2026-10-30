baseURL = "/HRIS/Download/EmployeeDeduction/";
agGrid.initialiseAgGridWithAngular1(angular);
var app = angular.module('HRIS', ['ui.bootstrap', 'agGrid', 'ngMaterial']);
let session = "";

app.controller('EmployeeDeductionController', function ($scope, $rootScope, $http, $uibModal, $mdToast) {
    //#region form loading events
    $scope.formLoaded = function () {
        hideFormLoading();
    };   
    //#endregion

    //#region toolbar actions
    $scope.tbExit = function () {
        $scope = null;
        window.location.href = '/HRIS/Common/Common/Index';
    };

    $scope.tbExtract = function () {
        $http({
            type: "Get",
            url: baseURL + "Get",
            dataType: "JSON"
        }).then(result => {
            clearForm();
            if (!result.data.success) {
                showErrorToast(result.data.message);
            }
            else {
                $scope.EmpDeduction = result.data.empDeduction;
                setGridData();
            }
        }, errorMsg => {
            showErrorToast(result);
        });
    };

    $scope.tbExport = function () {
        var params = {
            skipHeader: false,
            allColumns: true,
            skipFooters: true,
            skipGroups: true,
            fileName: "EmployeeDeduction.csv"
        };
        $scope.grdOptEmployeeDeduction.api.exportDataAsCsv(params);
    };
    //#endregion

    //#region local function
    function clearForm() {
        $scope.EmpDeduction = {};
        $scope.errors = [];
    }
    //#endregion

    //#region grid setup
    //#region WIP Deduction Grid Definition & functions
    var colEmployeeDeduction = [
        {
            headerName: "Employee Code", field: "EmployeeCode", width: 125
        },
        {
            headerName: "Employee Name", field: "EmployeeName", width: 200
        },
        {
            headerName: "Branch", field: "BranchName", width: 150
        },
        {
            headerName: "Dept", field: "DepartmentName", width: 110
        },
        {
            headerName: "Designation", field: "Designation", width: 150
        },
        {
            headerName: "D E D U C T I O N S",
            children: [
                {
                headerName: "Lunch", field: "Lunch", type: "numericColumn", filter: "agNumberColumnFilter",
                valueFormatter: formatNumbers, valueParser: numberValueParser, width: 100
                },
                {
                    headerName: "PF", field: "PF", type: "numericColumn", filter: "agNumberColumnFilter",
                    valueFormatter: formatNumbers, valueParser: numberValueParser, width: 100
                },
                {
                    headerName: "EOBI", field: "EOBI", type: "numericColumn", filter: "agNumberColumnFilter",
                    valueFormatter: formatNumbers, valueParser: numberValueParser, width: 100
                },
                {
                    headerName: "Income Tax", field: "ITax", type: "numericColumn", filter: "agNumberColumnFilter",
                    valueFormatter: formatNumbers, valueParser: numberValueParser, width: 100
                },
                {
                    headerName: "Transport", field: "Transportation", type: "numericColumn", filter: "agNumberColumnFilter",
                    valueFormatter: formatNumbers, valueParser: numberValueParser, width: 100
                },
                {
                    headerName: "Other Deductions", field: "OtherDeductions", type: "numericColumn", filter: "agNumberColumnFilter",
                    valueFormatter: formatNumbers, valueParser: numberValueParser, width: 100
                }]
        }
    ];

    $scope.grdOptEmployeeDeduction = {
        headerHeight: 35,
        rowHeight: 32,
        animateRows: true,
        defaultColDef: {
            editable: false
        },
        columnDefs: colEmployeeDeduction,
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
        if ($scope.EmpDeduction != []) {
            $scope.grdOptEmployeeDeduction.api.setRowData($scope.EmpDeduction.Details);
        }
        else { $scope.grdOptEmployeeDeduction.api.setRowData([]); }
    }
    //#endregion
    //#endregion
});