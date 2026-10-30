baseURL = "/HRIS/Transaction/Attendance/";
agGrid.initialiseAgGridWithAngular1(angular);
var app = angular.module('HRIS', ['ui.bootstrap', 'agGrid', 'ngMaterial']);
let session = "";

$(document).ready(function () {
    $("#btnEdit").click(function () {
        session.FormMode = "Edit";
        setFormSession(session);
    });
});

app.controller('AttendanceController', function ($scope, $rootScope, $http, $uibModal, $mdToast) {
    //#region form loading events
    $scope.formLoaded = function () {
        $http({
            method: "GET",
            url: "/HRIS/Home/GetAccessRight",
            dataType: "JSON",
            params: { _optionName: "Attendance Upload" }
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
            $scope.lstEmployee = result.data.lstEmployee;
        }, errorMsg => {
            showErrorToast(errorMsg.message);
        });
    }
    //#endregion

    //#region toolbar actions
    $scope.tbRecall = function () {
        session.FormMode = "Recall";
        setFormSession(session);
        $('#ddEmployee').focus();
    };  

    $scope.tbSave = function () {
        $scope.Attendance.Details = getAttendanceDataFromGrid();
        $http({
            method: "POST",
            url: baseURL + "Save",
            data: $scope.Attendance
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
    $scope.getAttendanceById = function (_empId) {
        $http({
            type: "Get",
            url: baseURL + "Get",
            dataType: "JSON",
            params: { _employeeId: _empId }
        }).then(result => {
            clearForm();
            if (!result.data.success) {
                $scope.errors = getArray(result.data.message);
                showErrorToast(result.data.message);
            }
            else {
                $scope.Attendance = result.data.attendance;
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
        $scope.Attendance = {};
        $scope.errors = [];
    }
    //#endregion

    //#region date functions
    function maskDates() {  
        if ($scope.Attendance.Details != null) {
            for (i = 0; i < $scope.Attendance.Details.length; i++) {
                $scope.Attendance.Details[i]["AttendanceDate"] = formatDate($scope.Attendance.Details[i]["AttendanceDate"]);
                $scope.Attendance.Details[i]["TimeIn"] = formatTime($scope.Attendance.Details[i]["TimeIn"]);
                $scope.Attendance.Details[i]["TimeOut"] = formatTime($scope.Attendance.Details[i]["TimeOut"]);
            }
        }
    }
    //#endregion

    //#region grid setup
    //#region Monthly Attendance Grid Definition & functions
    var colAttendance = [
        {
            headerName: "Date", field: "AttendanceDate", width: 130
        },
        {
            headerName: "Time In", field: "TimeIn", width: 100, editable: true,
            cellEditor: 'timePicker'
        },
        {
            headerName: "Time Out", field: "TimeOut", width: 100, editable: true,
            cellEditor: 'timePicker'
        },
        {
            headerName: 'Visit', field: 'OfficialVisit', width: 70,
            cellRenderer: params => {
                if (params.value) {
                    return "<input type='checkbox' checked />";
                }
                else {
                    return "<input type='checkbox'/>";
                }
            }
        },
        { headerName: "Edit", field: "Edit", hide: true, suppressToolPanel: true }
    ];


    $scope.grdOptAttendance = {
        headerHeight: 25,
        rowHeight: 32,
        animateRows: true,
        columnDefs: colAttendance,
        rowData: [],
        rowSelection: 'multiple',
        rowDeselection: true,
        enableColResize: true,
        enableSorting: true,
        singleClickEdit: true,
        onCellClicked: function (event) {
            if (event.colDef.field == "OfficialVisit")
            {
                var RowData = $scope.grdOptAttendance.api.getSelectedNodes();       
                RowData[0].setDataValue('OfficialVisit', !event.data.OfficialVisit);
            }           
        },
        onCellValueChanged: function (params) {
            params.data.Edit = true;
        },
        overlayLoadingTemplate: '<span class="ag-overlay-loading-center">Please wait while we are fetching requested records from database</span>',
        overlayNoRowsTemplate: '<span style="padding: 10px; border: 2px solid #444; background: lightgoldenrodyellow; color:red;">No rows available to display here</span>',
        components: {
            timePicker: getTimePicker(),
        }
    };

    function getAttendanceDataFromGrid() {
        let rowData = [];
        $scope.grdOptAttendance.api.forEachNode(node => rowData.push(node.data));
        return rowData;
    }
    //#endregion       

    //#region general functions
    function setGridData() {
        if ($scope.Attendance != []) {
            $scope.grdOptAttendance.api.setRowData($scope.Attendance.Details);
            setGridDeleteFilter($scope.grdOptAttendance.api);
        }
        else { $scope.grdOptAttendance.api.setRowData([]); }
    }

    $scope.sum = function (items, prop) {
        return items.reduce(function (a, b) {
            return a + b[prop];
        }, 0);
    };
    //#endregion
    //#endregion
});