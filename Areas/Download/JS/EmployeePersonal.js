baseURL = "/HRIS/Download/EmployeePersonal/";
agGrid.initialiseAgGridWithAngular1(angular);
var app = angular.module('HRIS', ['ui.bootstrap', 'agGrid', 'ngMaterial']); 
let session = "";

app.controller('EmployeePersonalController', function ($scope, $rootScope, $http, $uibModal, $mdToast) {
    //#region form loading events
    $scope.formLoaded = function () {
        hideFormLoading();
    };   

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
                $scope.EmpPersonal = result.data.empPersonal;
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
            fileName: "EmployeePersonal.csv"
        };
        $scope.grdOptEmployeePersonal.api.exportDataAsCsv(params);
    };
    //#endregion

    //#region local function
    function clearForm() {
        $scope.EmpPersonal = {};
        $scope.errors = [];
    }

    function maskDates() {
        if ($scope.EmpPersonal.Details != null) {
            for (i = 0; i < $scope.EmpPersonal.Details.length; i++) {
                $scope.EmpPersonal.Details[i]["BirthDate"] = formatDate($scope.EmpPersonal.Details[i]["BirthDate"]);
            }
        }
    }
    //#endregion

    //#region grid setup
    //#region WIP Salary Grid Definition & functions
    var colEmployeeSalary = [
        { headerName: "Employee Code", field: "EmployeeCode", width: 125 },
        { headerName: "Employee Name", field: "EmployeeName", width: 200 },
        { headerName: "Branch", field: "BranchName", width: 150 },
        { headerName: "Dept", field: "DepartmentName", width: 110 },
        { headerName: "Designation", field: "Designation", width: 150 },
        { headerName: "Grade", field: "Grade", width: 80 },
        { headerName: "Head Name", field: "HeadName", width: 200 },
        { headerName: "CNIC", field: "CNIC", width: 200 },
        {
            headerName: "Birth Date", field: "BirthDate", width: 120, type: 'dateColumn', filter: "agDateColumnFilter",
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
        { headerName: "Gender", field: "Gender", width: 60 },
        { headerName: "Cell#", field: "CellNo", width: 120 },
        { headerName: "Email", field: "Email", width: 120 },
        { headerName: "Family#", field: "FamilyNo", width: 80 },
        { headerName: "SS M", field: "SSMember", width: 60 },
        { headerName: "ESS#", field: "ESSNo", width: 80 },
        { headerName: "EOBI#", field: "EOBINo", width: 80 },
        { headerName: "Life Ins.", field: "LifeInsuranceCost", width: 100, filter: 'agNumberColumnFilter' },
        { headerName: "Medical", field: "MedicalCost", width: 100, filter: 'agNumberColumnFilter'  },
        { headerName: "Marital Status", field: "MaritalStatus", width: 100 },
        { headerName: "MoP", field: "PaymentMode", width: 80 },
        { headerName: "Bank", field: "BankName", width: 100 },
        { headerName: "Bank A/c #", field: "BankAccountNo", width: 120 },
        { headerName: "Supervisor Code", field: "SupervisorCode", width: 120 },
        { headerName: "Supervisor Name", field: "SupervisorName", width: 200 },
        { headerName: "Supervisor ESS", field: "SupervisorESS", width: 120 },
        {
            headerName: "Worker Type", field: "WorkerTypeName", width: 150
        },
        {
            headerName: "Address", field: "Address", width: 350
        }
    ];

    $scope.grdOptEmployeePersonal = {
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
        if ($scope.EmpPersonal != []) {
            $scope.grdOptEmployeePersonal.api.setRowData($scope.EmpPersonal.Details);
        }
        else { $scope.grdOptEmployeePersonal.api.setRowData([]); }
    }
    //#endregion
    //#endregion
});