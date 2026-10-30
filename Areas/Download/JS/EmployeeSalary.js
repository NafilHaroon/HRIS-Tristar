baseURL = "/HRIS/Download/EmployeeSalary/";
agGrid.initialiseAgGridWithAngular1(angular);
var app = angular.module('HRIS', ['ui.bootstrap', 'agGrid', 'ngMaterial']);//
let session = "";

app.controller('EmployeeSalaryController', function ($scope, $rootScope, $http, $uibModal, $mdToast) {
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
            //params: { _employeeCode: "", _branchCode: "ALL", _departmentCode: "ALL", _gradeId: -1},
            dataType: "JSON"
        }).then(result => {
            clearForm();
            if (!result.data.success) {
                showErrorToast(result.data.message);
            }
            else {
                $scope.EmpSalary = result.data.empSalary;
                maskDates();
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
            fileName: "EmployeeSalary.csv"
        };
        $scope.grdOptEmployeeSalary.api.exportDataAsCsv(params);
    };
    //#endregion

    //#region local function
    function clearForm() {
        $scope.EmpSalary = {};
        $scope.errors = [];
    }

    function maskDates() {
        if ($scope.EmpSalary.Details != null) {
            for (i = 0; i < $scope.EmpSalary.Details.length; i++) {
                $scope.EmpSalary.Details[i]["JoiningDate"] = formatDate($scope.EmpSalary.Details[i]["JoiningDate"]);
            }
        }
    }
    //#endregion

    //#region grid setup
    //#region WIP Salary Grid Definition & functions
    var colEmployeeSalary = [
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
            headerName: "Grade", field: "Grade", width: 80
        },
        {
            headerName: "Joining Date", field: "JoiningDate", width: 120, type: 'dateColumn', filter: "agDateColumnFilter",
            filterParams: {

                // provide comparator function
                comparator: function (filterLocalDateAtMidnight, cellValue) {
                    var dateAsString = cellValue;
                    if (dateAsString == null) return 0;

                    // In the example application, dates are stored as dd/mm/yyyy
                    // We create a Date object for comparison against the filter date
                    var dateParts = dateAsString.split(".");
                    var day = Number(dateParts[0]);
                    var month = monthNameToNum(dateParts[1]);
                    var year = Number(dateParts[2]);
                    var cellDate = new Date(year, month, day);

                    // Now that both parameters are Date objects, we can compare
                    if (cellDate < filterLocalDateAtMidnight) {
                        return -1;
                    } else if (cellDate > filterLocalDateAtMidnight) {
                        return 1;
                    } else {
                        return 0;
                    }
                }
            }
        },
        {
            headerName: "Qualification", field: "Qualification", width: 100
        },
        {
            headerName: "Gross Salary", field: "GrossSalary", type: "numericColumn", filter: "agNumberColumnFilter",
            valueFormatter: formatNumbers, valueParser: numberValueParser, width: 100
        },
        {
            headerName: "Car Allow.", field: "CarAllowance", type: "numericColumn", filter: "agNumberColumnFilter",
            valueFormatter: formatNumbers, valueParser: numberValueParser, width: 100
        },
        {
            headerName: "Fuel Allow.", field: "FuelAllowance", type: "numericColumn", filter: "agNumberColumnFilter",
            valueFormatter: formatNumbers, valueParser: numberValueParser, width: 100
        },
        {
            headerName: "Mobile Allow.", field: "MobileAllowance", type: "numericColumn", filter: "agNumberColumnFilter",
            valueFormatter: formatNumbers, valueParser: numberValueParser, width: 100
        },
        {
            headerName: "CM Allow.", field: "CarMaintAllowance", type: "numericColumn", filter: "agNumberColumnFilter",
            valueFormatter: formatNumbers, valueParser: numberValueParser, width: 100
        },
        {
            headerName: "Other Allow.", field: "OtherAllowance", type: "numericColumn", filter: "agNumberColumnFilter",
            valueFormatter: formatNumbers, valueParser: numberValueParser, width: 100
        }
    ];

    $scope.grdOptEmployeeSalary = {
        headerHeight: 35,
        rowHeight: 32,
        animateRows: true,
        defaultColDef: {
            editable: false
        },
        columnDefs: colEmployeeSalary,
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
        if ($scope.EmpSalary != []) {
            $scope.grdOptEmployeeSalary.api.setRowData($scope.EmpSalary.Details);
        }
        else { $scope.grdOptEmployeeSalary.api.setRowData([]); }
    }
    //#endregion
    //#endregion
});