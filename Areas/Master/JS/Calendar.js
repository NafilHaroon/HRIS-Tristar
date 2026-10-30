baseURL = "/HRIS/Master/Calendar/";
agGrid.initialiseAgGridWithAngular1(angular);
var app = angular.module('HRIS', ['ui.bootstrap', 'agGrid', 'ngMaterial']); 
let session = "";

$(document).ready(function () {
    $("#btnEdit").click(function () {
        session.FormMode = "Edit";
        setFormSession(session);
    });
});

app.controller('CalendarController', function ($scope, $rootScope, $http, $uibModal, $mdToast) {
    //#region form loading events
    $scope.formLoaded = function () {
        $http({
            method: "GET",
            url: "/HRIS/Home/GetAccessRight",
            dataType: "JSON",
            params: { _optionName: "Calendar" }
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
            $scope.lstPeriod = result.data.lstPeriod;
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
        $scope.Calendar.Details = getCalendarDataFromGrid();
        $http({
            method: "POST",
            url: baseURL + "Save",
            data: $scope.Calendar
        }).then(result => {
            $scope.errors = [];
            if (!result.data.success) {
                $scope.errors = getArray(result.data.message);
                showErrorToast();
            }
            else {
                showSuccessToast(result.data.message);
                session.FormMode = result.data.formMode;
                clearForm();
                setGridData();
                setFormSession(session);                
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
    $scope.getCalendarById = function (_periodId) {
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
                $scope.Calendar = result.data.calendar;
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
        $scope.Calendar = {};
        $scope.errors = [];
    }
    //#endregion

    //#region date functions
    function maskDates() {
        if ($scope.Calendar.Details != null) {
            for (i = 0; i < $scope.Calendar.Details.length; i++) {
                $scope.Calendar.Details[i]["CalendarDate"] = formatDate($scope.Calendar.Details[i]["CalendarDate"]);
            }
        }
    }
    //#endregion

    //#region grid setup
    //#region Calendar Grid Definition & functions
    var colCalendar = [
        {
            headerName: "Date", field: "CalendarDate", editable:false,
            width: 140
        },
        {
            headerName: "Day", field: "WeekDay", editable: false,
            width: 120
        },       
        {
            headerName: 'Holiday', field: 'Holiday', width: 90, editable: false,
            cellRenderer: params => {
                if (params.value) { return "<input type='checkbox' checked />";}
                else { return "<input type='checkbox'/>"; }
            },
            cellEditor: "agilityCheckBoxEditor"
        },
        {
            headerName: "Reason", field: "Reason",
            width: 300
        }
    ];

    $scope.grdOptCalendar = {
        headerHeight: 25,
        rowHeight: 32,
        animateRows: true,
        defaultColDef: {
            editable: allowEdit.bind(this)
        },
        columnDefs: colCalendar,
        rowData: [],
        rowSelection: 'multiple',
        enableSorting: true,     
        singleClickEdit: true,
        stopEditingWhenGridLosesFocus: true,
        getRowNodeId: function (data) { return data.CalendarDate; },
        onCellClicked: function (event) {
            if (event.colDef.field == "Holiday") {
                var RowData = $scope.grdOptCalendar.api.getSelectedRows(); 
                if (!event.data.Holiday) {
                    rowNode = $scope.grdOptCalendar.api.getRowNode(RowData[0].CalendarDate);
                       rowNode.setDataValue('Holiday', true);
                }
                else {
                    rowNode = $scope.grdOptCalendar.api.getRowNode(RowData[0].CalendarDate);
                      rowNode.setDataValue('Holiday', false);
                }
            }  
        },
        onCellValueChanged: function (params) {
            if (!params.data.Add)
                params.data.Edit = true;
        },
        overlayLoadingTemplate: '<span class="ag-overlay-loading-center">Please wait while we are fetching requested records from database</span>',
        overlayNoRowsTemplate: '<span style="padding: 10px; border: 2px solid #444; background: lightgoldenrodyellow; color:red;">No rows available to display here</span>',
        components: {
            agilityCheckBoxEditor: getCellCheckBox()
        }
    };

    function getCalendarDataFromGrid() {
        let rowData = [];
        $scope.grdOptCalendar.api.forEachNode(node => rowData.push(node.data));
        return rowData;
    }
    //#endregion       

    //#region general functions
    function setGridData() {
        if ($scope.Calendar != []) {
            $scope.grdOptCalendar.api.setRowData($scope.Calendar.Details);
            setGridDeleteFilter($scope.grdOptCalendar.api);
        }
        else { $scope.grdOptCalendar.api.setRowData([]); }
    }
     //#endregion
    //#endregion
});