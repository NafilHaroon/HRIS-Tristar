baseURL = "/HRIS/Process/AttendanceUpload/";
agGrid.initialiseAgGridWithAngular1(angular);
var app = angular.module('HRIS', ['ui.bootstrap', 'agGrid', 'ngMaterial']);
let session = "";

app.controller('AttendanceUploadController', function ($scope, $rootScope, $http, $uibModal, $mdToast) {
    //#region form loading events
    $scope.formLoaded = function () {
        //$http({
        //    method: "GET",
        //    url: "/HRIS/Home/GetAccessRight",
        //    dataType: "JSON",
        //    params: { _optionName: "Attendance Upload" }
        //}).then(result => {
        //    session = result.data;
        //    setFormSession(session);
        //},
        //    errorMsg => {
        //        showErrorToast(errorMsg);
        //    });
        hideFormLoading();
    };
    //#endregion

    //#region toolbar actions
    $scope.tbUpload = function () {
        $scope.files = [];
        if (validateFile() == true) {
            $http({
                url: baseURL + "Upload",
                method: "POST",
                headers: { "Content-Type": undefined },
                transformRequest: function (data) {
                    var formData = new FormData();
                    var totalFiles = document.getElementById("fuAttachment").files.length;
                    for (var i = 0; i < totalFiles; i++) {
                        files = document.getElementById("fuAttachment").files[i];
                        formData.append("files[" + i + "]", files);
                        $scope.files.push(files);
                    }
                    return formData;
                }
            }).then(result => {
                $scope.errors = [];
                if (!result.data.success) {
                    $scope.errors = getArray(result.data.message);
                    showErrorToast();
                }
                else {
                    showSuccessToast("File processed successfully, please refer grid for line level results!");
                    $scope.AttendanceUpload = result.data.au;
                    maskDates();
                    setGridData();
                }
                $('#fuAttachment').val("");
            }, errorMsg => {
                showErrorToast(errorMsg);
            });
        }
    };

    $scope.tbExport = function () {
        var params = {
            skipHeader: false,
            allColumns: true,
            skipFooters: true,
            skipGroups: true,
            fileName: "AttendanceData.csv"
        };
        $scope.grdOptAttUpload.api.exportDataAsCsv(params);
    };

    $scope.tbExit = function () {
        $scope = null;
        window.location.href = '/HRIS/Common/Common/Index';
    };
    //#endregion   

    //#region date functions 
    function maskDates() {
        if ($scope.AttendanceUpload != null) {
            for (i = 0; i < $scope.AttendanceUpload.length; i++) {
                $scope.AttendanceUpload[i]["AttendanceDate"] = formatDate($scope.AttendanceUpload[i]["AttendanceDate"]);
                $scope.AttendanceUpload[i]["AttendanceTime"] = formatTime($scope.AttendanceUpload[i]["AttendanceTime"]);
            }
        }
    }
    //#endregion

    //#region grid setup
    //#region Attendance Grid Definition & functions
    var colAttUpload = [
        {
            headerName: "Employee Code", field: "EmployeeCode",  editable: false,
            width: 140
        },
        {
            headerName: "Date", field: "AttendanceDate",  editable: false,
            width: 120
        },
        {
            headerName: "Time", field: "AttendanceTime", editable: false,
            width: 120
        },
        {
            headerName: "Status", field: "Status", editable: false,
            width: 350
        }
    ];

    $scope.grdOptAttUpload = {
        headerHeight: 25,
        rowHeight: 32,
        animateRows: true,
        //defaultColDef: {
        //    editable: allowEdit.bind(this)
        //},
        columnDefs: colAttUpload,
        rowData: [],
        rowSelection: 'single',
        rowDeselection: true,
        enableColResize: true,
        enableSorting: true,
        singleClickEdit: true,
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
        overlayLoadingTemplate: '<span class="ag-overlay-loading-center">Please wait while we are fetching requested records from database</span>',
        overlayNoRowsTemplate: '<span style="padding: 10px; border: 2px solid #444; background: lightgoldenrodyellow; color:red;">No rows available to display here</span>',
       
    }; 
    //#endregion       

    //#region general functions
    function setGridData() {
        if ($scope.AttendanceUpload != []) {
        $scope.grdOptAttUpload.api.setRowData($scope.AttendanceUpload);
        }
        else { $scope.grdOptAttUpload.api.setRowData([]); }
    }
    //#endregion
    //#endregion

    //#region general functions 
    function validateFile() {     
        var fu = document.getElementById("fuAttachment");
        if (fu.value === "") {
            showErrorToast("Please select valid file to be uploaded before hitting the upload button");
            return false;
        }
        else {
            var validExts = new Array(".xlsx", ".xls");
            var fileExt = fu.value.substring(fu.value.lastIndexOf('.'));
            if (validExts.indexOf(fileExt) < 0 && fileExt !== "") {
                showErrorToast("Invalid file selected, valid files are of " + validExts.toString() + " types.");
                $(fu).val("");
                return false;
            }
            else { return true; }
        }
    }
    //#endregion
});