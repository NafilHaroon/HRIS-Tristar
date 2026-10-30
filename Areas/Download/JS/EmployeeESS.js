baseURL = "/HRIS/Download/EmployeeESS/";
agGrid.initialiseAgGridWithAngular1(angular);
var app = angular.module('HRIS', ['ui.bootstrap', 'agGrid', 'ngMaterial']);

app.controller('EmployeeESSController', function ($scope, $rootScope, $http, $uibModal, $mdToast) {
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
                $scope.EmpESS = result.data.empESS;
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
            fileName: "EmployeeESS.csv"
        };
        $scope.grdOptEmployeeESS.api.exportDataAsCsv(params);
    };
    //#endregion

    //#region local function
    function clearForm() {
        $scope.EmpESS = {};
        $scope.errors = [];
    }
    //#endregion

    //#region grid setup
    //#region WIP ESS Grid Definition & functions
    var colEmployeeESS = [
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
            headerName: "Marital Status", field: "MaritalStatus", width: 100
        },
        {
            headerName: "NTN", field: "NTN", width: 100
        },
        {
            headerName: "ESS #", field: "ESSNo", width: 100
        },
        {
            headerName: "Gender", field: "Gender", width: 100
        },
        {
            headerName: "EOBI #", field: "EOBINo", width: 100
        },
        {
            headerName: "SS Member?", field: "SSMember", width: 80
        }
    ];

    $scope.grdOptEmployeeESS = {
        headerHeight: 35,
        rowHeight: 32,
        animateRows: true,
        defaultColDef: {
            editable: false
        },
        columnDefs: colEmployeeESS,
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
        if ($scope.EmpESS != []) {
            $scope.grdOptEmployeeESS.api.setRowData($scope.EmpESS.Details);
        }
        else { $scope.grdOptEmployeeESS.api.setRowData([]); }
    }
    //#endregion
    //#endregion
});