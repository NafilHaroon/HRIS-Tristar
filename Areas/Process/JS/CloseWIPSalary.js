baseURL = "/HRIS/Process/CloseWIPSalary/";
agGrid.initialiseAgGridWithAngular1(angular);
var app = angular.module('HRIS', ['ui.bootstrap', 'agGrid', 'ngMaterial']);
let session = "";

app.controller('CloseWIPSalaryController', function ($scope, $rootScope, $http, $uibModal, $mdToast) {
    //#region form loading events
    $scope.formLoaded = function () {
        //$http({
        //    method: "GET",
        //    url: "/HRIS/Home/GetAccessRight",
        //    dataType: "JSON",
        //    params: { _optionName: "Verify & Close Processed Salary" }
        //}).then(result => {
        //    session = result.data;
        //    setFormSession(session);
        //},
        //    errorMsg => {
        //        showErrorToast(errorMsg);
        //    });
        getOpenWIPSalary();
        hideFormLoading();
    };   
    //#endregion

    //#region toolbar actions
    $scope.tbSave = function () {
        if (!confirm("You are about to close the salary for selected staff. You will no longer be able to modify " +
            "any WIP salary component afterwards. Are you sure you want to proceed?")) {
            //e.preventDefault();
            return false;
        }
        showWritingMsg();
        $scope.WIPSalary.Details = getCloseWIPSalaryDataFromGrid();
        $http({
            method: "POST",
            url: baseURL + "Save",
            data: $scope.WIPSalary
        }).then(result => {
            hideWritingMsg();
            $scope.errors = [];
            if (!result.data.success) {
                $scope.errors = getArray(result.data.message);
                showErrorToast();
            }
            else {
                showSuccessToast(result.data.message);
                getOpenWIPSalary();
            }
        }, errorMsg => {
            hideWritingMsg();
            showErrorToast(errorMsg);
        });
    };     

    $scope.tbExit = function () {
        $scope = null;
        window.location.href = '/HRIS/Common/Common/Index';
    };
       
    //#endregion

    //#region form functions
    function getOpenWIPSalary() {
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
                $scope.WIPSalary = result.data.wipSalary;
                setGridData();
            }
            }, errorMsg => {
                showErrorToast(result);
        });
    }
    //#endregion

    //#region local function
    function clearForm() {
        $scope.WIPSalary = {};
        $scope.errors = [];
    }
    //#endregion

    //#region grid setup
    //#region WIP Salary Grid Definition & functions
    var colCloseWIPSalary = [
        {
            headerName: 'V.', field: 'Close', width: 40,
            cellRenderer: params => {
                if (params.data.NetSalary.indexOf('(') == 0) {
                    return "<input type='checkbox' disabled title='Can`t verify -ve salary payout'/>";
                }
                else if (params.value) {
                    return "<input type='checkbox' checked title='uncheck to mark this salary as not verified'/>";
                }
                else {
                    return "<input type='checkbox' title='check to mark this salary as verified'/>";
                }
            },
            cellEditor: "agilityCheckBoxEditor"
        },
        {
            headerName: "Employee", field: "EmployeeName", width: 150
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
            headerName: "Net Salary", field: "NetSalary", type: "numericColumn", 
            valueFormatter: formatNumbers, valueParser: numberValueParser, width: 120, 
            cellStyle: {
                backgroundColor: '#aaffaa', // light green
                color: 'blue'
            }
        },
        {
            headerName: "Gross", field: "GrossSalary", type: "numericColumn",
            valueFormatter: formatNumbers, valueParser: numberValueParser, width: 100
        },
        {
            headerName: "Allowances", field: "Allowance", type: "numericColumn",
            valueFormatter: formatNumbers, valueParser: numberValueParser, width: 100
        },
        {
            headerName: "Personal Ded.", field: "PersonalDeduction", type: "numericColumn",
            valueFormatter: formatNumbers, valueParser: numberValueParser, width: 100
        },
        {
            headerName: "Absent Ded.", field: "AbsentDeduction", type: "numericColumn",
            valueFormatter: formatNumbers, valueParser: numberValueParser, width: 100
        },
        {
            headerName: "Other Ded.", field: "OtherDeduction", type: "numericColumn",
            valueFormatter: formatNumbers, valueParser: numberValueParser, width: 100
        },
        {
            headerName: "Unpaid Days", field: "UnpaidDays", type: "numericColumn", width: 80
        },
        {
            headerName: "Leave", field: "LeaveDays", type: "numericColumn", width: 80
        }
    ];

    $scope.grdOptCloseWIPSalary = {
        headerHeight: 25,
        rowHeight: 32,
        animateRows: true,
        defaultColDef: {
            editable: false
        },
        columnDefs: colCloseWIPSalary,
        rowData: [],
        rowSelection: 'single',
        rowDeselection: true,
        floatingFilter: true,
        enableColResize: true,
        enableSorting: true,
        singleClickEdit: true,
        suppressCellSelection: true,
        onGridReady: (grid) => {
        },
        getRowStyle: function (params) {
            if (params.node.rowPinned) {
                return { 'font-weight': 'bold', 'color': 'blue' };
            }
        },
        onCellValueChanged: function (params) {
            if (!params.data.Add)
                params.data.Edit = false;
        },
        onCellClicked: function (event) {
            var RowData = $scope.grdOptCloseWIPSalary.api.getSelectedNodes();
            if (event.colDef.field == "Close") {
                RowData[0].setDataValue('Close', !event.data.Close);
                //if (!event.data.Close) {
                    //rowNode = $scope.grdOptCloseWIPSalary.api.getRowNode(RowData[0].SalaryId);
                    //rowNode.setDataValue('Close', true);
                  //  RowData[0].setDataValue('Close', true);
                //}
                //else {
                    //rowNode = $scope.grdOptCloseWIPSalary.api.getRowNode(RowData[0].SalaryId);
                  //  RowData[0].setDataValue('Close', false);
                //}
            }
        },
        overlayLoadingTemplate: '<span class="ag-overlay-loading-center">Please wait while we are fetching requested records from database</span>',
        overlayNoRowsTemplate: '<span style="padding: 10px; border: 2px solid #444; background: lightgoldenrodyellow; color:red;">No rows available to display here</span>',
        components: {
            agilityCheckBoxEditor: getCellCheckBox()
        }
    };    

    function getCloseWIPSalaryDataFromGrid() {
        let rowData = [];
        $scope.grdOptCloseWIPSalary.api.forEachNode(node => rowData.push(node.data));
        return rowData;
    }

    //#endregion       

    //#region general functions
    function setGridData() {
        if ($scope.WIPSalary != []) {
            $scope.grdOptCloseWIPSalary.api.setRowData($scope.WIPSalary.Details);
        }
        else { $scope.grdOptCloseWIPSalary.api.setRowData([]); }
    }
    //#endregion
    //#endregion
});