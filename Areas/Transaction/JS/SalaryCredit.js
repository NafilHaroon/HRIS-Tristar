baseURL = "/HRIS/Transaction/SalaryCredit/";
agGrid.initialiseAgGridWithAngular1(angular);
var app = angular.module('HRIS', ['ui.bootstrap', 'agGrid', 'ngMaterial']);
let session = "";

$(document).ready(function () {
    $("#btnEdit").click(function () {
        session.FormMode = "Edit";
        setFormSession(session);
        if (document.getElementById('hdnPrinted').value == 'true') 
            $('#btnSave')[0].disabled = true;
        disableFilters();
        $('#dpLetter').focus();
    });
});

function disableFilters() {
    document.getElementById('ddBank').disabled = true;
    document.getElementById('ddPeriod').disabled = true;
    document.getElementById('btnLoad').disabled = true;
}

app.controller('SalaryCreditController', function ($scope, $rootScope, $http, $uibModal, $mdToast) {
    //#region form loading events
    $scope.formLoaded = function () {
        $http({
            method: "GET",
            url: "/HRIS/Home/GetAccessRight",
            dataType: "JSON",
            params: { _optionName: "Salary Credit Letter" }
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
            $scope.lstPeriod = result.data.lstPeriod;
            $scope.lstBank = result.data.lstBank;
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
            $scope.SalaryLetter = response.data.salaryLetter;
            $scope.Footer = response.data.salaryLetter.AuditTrail;
            session.FormMode = response.data.formMode;
            setFormSession(session);
            maskDates();
            $('#dpLetter').focus();
        }, errorMsg => {
            showErrorToast(errorMsg);
        });
    };

    $scope.tbRecall = function () {
        session.FormMode = "Recall";
        setFormSession(session);
        $('#txtLetterId').focus();
    };

    $scope.tbSearch = function () {
        $scope.formTitle = "Search & Select Salary Credit Letter";
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
                $scope.getSalaryLetterById($rootScope.SelectedRow.LetterId);
            }, function () {

            });
        }
        catch (exception) { showErrorToast(exception); }
    };

    $scope.tbSave = function () {
        $scope.SalaryLetter.Details = getSalaryLetterDataFromGrid();
        $http({
            method: "POST",
            url: baseURL + "Save",
            data: $scope.SalaryLetter
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

    $scope.tbLoad = function () {
        try {
            if (!isPreRequisiteMeet()) {
                showErrorToast('Please select valid Bank & Salary Period before hitting load button!');
                return false;
            }
            $http({
                method: "Get",
                url: baseURL + "GetPendingSalaries",
                dataType: "JSON",
                params: { _bankId: $scope.SalaryLetter.BankId, _periodId: $scope.SalaryLetter.PeriodId }
            }).then(result => {
                $scope.errors = [];
                if (!result.data.success) {
                    $scope.errors = getArray(result.data.message);
                    showErrorToast();
                }
                else {
                    $scope.SalaryLetter = result.data.salaryLetter;
                    $scope.footer = $scope.SalaryLetter.AuditTrail;
                    session.FormMode = result.data.formMode;
                    setFormSession(session);
                    disableFilters();
                    maskDates();
                    setGridData();
                }
            }, errorMsg => {
                showErrorToast(errorMsg);
            });
        }
        catch (exception) { showErrorToast(exception.message); }
    };
    //#endregion

    //#region form functions
    $scope.getSalaryLetterById = function (res) {
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
                $scope.SalaryLetter = result.data.salaryLetter;
                $scope.footer = $scope.SalaryLetter.AuditTrail;
                session.FormMode = result.data.formMode;
                setFormSession(session);
                maskDates();
                setGridData();
            }
        }, errorMsg => {
            showErrorToast(result);
        });
    };
    //#endregion

    //#region local function
    function clearForm() {
        $scope.SalaryLetter = {};
        $scope.footer = {};
        $scope.errors = [];
    }

    function isPreRequisiteMeet() {
        if (document.getElementById("ddBank").value == "" ||
            document.getElementById("ddPeriod").value == "")
            return false;
        else
            return true;
    }

    //#endregion

    //#region date functions
    function setDateParameters() {

        $('#dpLetter').datepicker({
            startDate: '-30d'
        }).on('changeDate', function (e) {
            $scope.SalaryLetter.LetterDate = e.format();
            $scope.$apply();
        });
    }

    function maskDates() {
        $scope.SalaryLetter.LetterDate = formatDate($scope.SalaryLetter.LetterDate);

        if ($scope.SalaryLetter.AuditTrail != null) {
            $scope.SalaryLetter.AuditTrail.CreatedOn = formatDateTime($scope.SalaryLetter.AuditTrail.CreatedOn);
            if ($scope.SalaryLetter.AuditTrail.UpdatedOn != null) {
                $scope.SalaryLetter.AuditTrail.UpdatedOn = formatDateTime($scope.SalaryLetter.AuditTrail.UpdatedOn);
            }
        }
    }
    //#endregion

    //#region grid setup
    //#region Salary Letter Grid Definition & functions
    var colSalaryLetter = [
        {
            field: 'Selected', hide: true
        },
        {
            headerName: "Name", field: "EmployeeName", width: 300,
            headerCheckboxSelection: true,
            headerCheckboxSelectionFilteredOnly: true, checkboxSelection: true
        },
        {
            headerName: "Branch", field: "BranchName",  width: 200
        },
        {
            headerName: "Department", field: "DepartmentName", width: 150
        },
        {
            headerName: "Grade", field: "GradeNo", width: 90
        },
        {
            headerName: "A/C #", field: "AccountNo", width: 150
        },
        {
            headerName: "Amount", field: "NetAmount", type: "numericColumn",
            valueFormatter: formatNumbers, valueParser: numberValueParser, width: 120,
            cellStyle: {
                backgroundColor: '#aaffaa', 
                color: 'blue'
            }
        }
    ];
    
    $scope.grdOptSalaryLetter = {
        headerHeight: 25,
        rowHeight: 32,
        animateRows: true,
        defaultColDef: {
            editable: false
        },
        columnDefs: colSalaryLetter,
        rowData: [],
        suppressRowClickSelection: true,
        rowSelection: 'multiple',
        floatingFilter: true,
        enableColResize: true,
        enableSorting: true,
        rowDeselection: true,
        getRowNodeId: function (data) { return data.EmployeeId; },
        onCellClicked: function (event) {     
            var RowData = $scope.grdOptSalaryLetter.api.getSelectedRows(); 
            if (event.colDef.field == "Selected") {                
                if (!event.data.Selected) {
                    rowNode = $scope.grdOptSalaryLetter.api.getRowNode(RowData[0].EmployeeId);
                    rowNode.setDataValue('Selected', true);
                }
                else {
                    rowNode = $scope.grdOptSalaryLetter.api.getRowNode(RowData[0].EmployeeId);
                    rowNode.setDataValue('Selected', false);
                }
            }            
        },
        onRowSelected: function (row) {
            row.data.Selected = row.node.selected;
        },
        getRowStyle: function (params) {
            if (params.node.rowPinned) {
                return { 'font-weight': 'bold', 'color': 'blue' };
            }
        },
        overlayLoadingTemplate: '<span class="ag-overlay-loading-center">Please wait while we are fetching requested records from database</span>',
        overlayNoRowsTemplate: '<span style="padding: 10px; border: 2px solid #444; background: lightgoldenrodyellow; color:red;">No rows available to display here</span>'//,
    };

    function getSalaryLetterDataFromGrid() {
        let rowData = [];
        $scope.grdOptSalaryLetter.api.forEachNode(node => rowData.push(node.data));
        return rowData;
    }
    //#endregion       

    //#region general functions
    function setGridData() {
        if ($scope.SalaryLetter != []) {
            $scope.grdOptSalaryLetter.api.setRowData($scope.SalaryLetter.Details);
            $scope.grdOptSalaryLetter.api.forEachNode(node => node.setSelected(node.data.Selected));
        }
        else { $scope.grdOptSalaryLetter.api.setRowData([]); }
    }   
    //#endregion
    //#endregion
});