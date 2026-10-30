baseURL = "/HRIS/Process/LeaveUpload/";
agGrid.initialiseAgGridWithAngular1(angular);
var app = angular.module('HRIS', ['ui.bootstrap', 'agGrid', 'ngMaterial']);
let session = "";

app.controller('LeaveUploadController', function ($scope, $rootScope, $http, $uibModal, $mdToast) {
    //#region form loading events    
    $scope.formLoaded = function () {  
        $http({
            method: "GET",
            url: "/HRIS/Home/GetAccessRight",
            dataType: "JSON",
            params: { _optionName: "Leave Upload" }
        }).then(result => {
            session = result.data;
            setFormSession(session);
        },
            errorMsg => {
                showErrorToast(errorMsg);
            });
        hideFormLoading();
        setDateParameters();
    };
    //#endregion

    //#region toolbar actions
    $scope.files=[];
    $scope.tbSave = function () {
        if (validateFile() == true) {
            $http({
                url: baseURL + "Save",
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
                    formData.append("FirstDay", $('#dtFirstDay').val());
                    return formData;

                },
                //data: { files: $scope.files }
            }).then(result => {
                $scope.errors = [];
                if (!result.data.success) {
                    $scope.errors = getArray(result.data.message);
                    showErrorToast();
                }
                else {
                    showSuccessToast(result.data.message);
                    $scope.LeaveUpload = result.data.LeaveUpload;
                    maskDates();
                    setGridData();
                }
                clearForm();
            }, errorMsg => {
                alert(errorMsg);
            });
        }
    };


    //$scope.tbSave = function (res) {
    //    if (validateFile()) {
    //        //File Upload//
    //        var formData = new FormData();
    //        var totalFiles = document.getElementById("fuAttachment").files.length;

    //        for (var i = 0; i < totalFiles; i++) {
    //            var file = document.getElementById("fuAttachment").files[i];
    //            formData.append("UploadedFile", file);
    //        }
    //        formData.append("DFirstDay", $('#dtFirstDay').val());
    //        //File Upload//

    //        $http({
    //            method: "POST",
    //            url: baseURL + "Save",
    //            headers: { "Content-Type": undefined },
    //            transformRequest: function (data) {
    //                return formData;//NOTICE THIS RETURN WHICH WAS MISSING
    //            },
    //        }).then(result => {
    //            $scope.errors = [];
    //            if (!result.data.success) {
    //                $scope.errors = getArray(result.data.message);
    //                showErrorToast();
    //            }
    //            else {
    //                showSuccessToast(result.data.message);
    //                $scope.LeaveUpload = result.data.LeaveUpload;
    //                //clearForm();
    //                setGridData();
    //                //maskDates();
    //                setFormControls(result.data.session);
    //            }
    //        }, errorMsg => {
    //            showErrorToast(errorMsg);
    //        });
    //    }
    //};

    $scope.tbExit = function () {
        $scope = null;
        window.location.href = '/HRIS/Common/Common/Index';
    };
    //#endregion   

    //#region local function
    function clearForm() {
        $('#fuAttachment').val("");
    }
    //#endregion

    //#region date functions 
    function setDateParameters() {
        $('#dtFirstDay').datepicker({
            startDate: '-30d'
        });
    }

    function maskDates() {
        if ($scope.LeaveUpload.Details != null) {
            for (i = 0; i < $scope.LeaveUpload.Details.length; i++) {
                $scope.LeaveUpload.Details[i]["LeaveDate"] = formatDate($scope.LeaveUpload.Details[i]["LeaveDate"]);
            }
        }
    }
    //#endregion

    //#region grid setup
    //#region Leave  Grid Definition & functions
    var colLUpload = [
        {
            headerName: "EmployeeCode", field: "EmployeeCode", editable: false,
            width: 120
        },
        {
            headerName: "Date", field: "LeaveDate", editable: false,
            width: 120
        },
        {
            headerName: "Status", field: "Status",  editable: false,
            width: 500
        },
    ];

    $scope.grdOptLUpload = {
        headerHeight: 25,
        rowHeight: 32,
        animateRows: true,
        //defaultColDef: {
        //    editable: allowEdit.bind(this)
        //},
        columnDefs: colLUpload,
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
        if ($scope.LeaveUpload != []) {
            $scope.grdOptLUpload.api.setRowData($scope.LeaveUpload.Details);
        }
        else { $scope.grdOptLUpload.api.setRowData([]); }
    }

    $scope.tbExport = function () {
        var params = {
            skipHeader: false,
            allColumns: true,
            skipFooters: true,
            skipGroups: true,
            fileName: "LeaveData.csv"
        };
        $scope.grdOptLUpload.api.exportDataAsCsv(params);
    }
    //#endregion
    //#endregion

    //#region general functions 
    function validateFile() {

        var fu = document.getElementById("fuAttachment");
        var fdate = document.getElementById("dtFirstDay");
        if (fu.value == "") {
            showErrorToast("Please select valid file to be uploaded before hitting upload button");
            return false;
        }
        else if (fdate.value == "") {
            showErrorToast("Please select valid First Day to be uploaded before hitting upload button");
            return false;
        }
        else {
            var validExts = new Array(".xlsx", ".xls");
            var fileExt = fu.value.substring(fu.value.lastIndexOf('.'));
            if (validExts.indexOf(fileExt) < 0 && fileExt != "") {
                showErrorToast("Invalid file selected, valid files are of " +
                    validExts.toString() + " types.");
                $(fu).val("");
                return false;
            }
            else {
                return true;
            }
        }
    };
    //#endregion
});