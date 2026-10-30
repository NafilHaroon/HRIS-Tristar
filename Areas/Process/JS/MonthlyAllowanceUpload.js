baseURL = "/HRIS/Process/MonthlyAllowanceUpload/";
agGrid.initialiseAgGridWithAngular1(angular);
var app = angular.module('HRIS', ['ui.bootstrap', 'agGrid', 'ngMaterial']);
let session = "";

app.controller('MonthlyAllowanceUploadController', function ($scope, $rootScope, $http, $uibModal, $mdToast) {
    //#region toolbar actions
    $scope.formLoaded = function () {
        //$http({
        //    method: "GET",
        //    url: "/HRIS/Home/GetAccessRight",
        //    dataType: "JSON",
        //    params: { _optionName: "Monthly Allowances Upload" }
        //}).then(result => {
        //    session = result.data;
        //    setFormSession(session);
        //},
        //    errorMsg => {
        //        showErrorToast(errorMsg);
        //    });
        hideFormLoading();
    };

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
                    $scope.MonthlyAllowanceUpload = result.data.mau;
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
            fileName: "MonthlyAllowanceUpload.csv"
        };
        $scope.grdOptMAUpload.api.exportDataAsCsv(params);
    };

    $scope.tbExit = function () {
        $scope = null;
        window.location.href = '/HRIS/Common/Common/Index';
    };
    //#endregion   

    //#region grid setup
    //#region Attendance  Grid Definition & functions
    var colMAUpload = [
        {
            headerName: "Employee Code", field: "EmployeeCode",
            width: 120
        },
        {
            headerName: "Head Id", field: "HeadId", width: 70
        },
        {
            headerName: "Amount", field: "Amount", width: 100, type: "numericColumn"
        },
        {
            headerName: "Status", field: "Status", width: 1000
        }
    ];

    $scope.grdOptMAUpload = {
        headerHeight: 25,
        rowHeight: 32,
        editable: false,
        animateRows: true,
        columnDefs: colMAUpload,
        rowData: [],
        rowSelection: 'single',
        rowDeselection: true,
        enableColResize: true,
        enableSorting: true,
        overlayLoadingTemplate: '<span class="ag-overlay-loading-center">Please wait while we are fetching requested records from database</span>',
        overlayNoRowsTemplate: '<span style="padding: 10px; border: 2px solid #444; background: lightgoldenrodyellow; color:red;">No rows available to display here</span>',
    };
    //#endregion       

    //#region general functions
    function setGridData() {
        if ($scope.MonthlyAllowanceUpload != []) {
            $scope.grdOptMAUpload.api.setRowData($scope.MonthlyAllowanceUpload);
        }
        else { $scope.grdOptMAUpload.api.setRowData([]); }
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