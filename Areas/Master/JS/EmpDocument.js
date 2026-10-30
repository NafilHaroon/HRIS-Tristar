baseURL = "/HRIS/Master/EmpDocument/";
agGrid.initialiseAgGridWithAngular1(angular);
var app = angular.module('HRIS', ['ui.bootstrap', 'agGrid', 'ngMaterial']); 
let session = "";

$(document).ready(function () {
    $("#btnEdit").click(function () {
        session.FormMode = "Edit";
        setFormSession(session);
    });
});

app.controller('EmpDocumentController', function ($scope, $rootScope, $http, $uibModal, $mdToast) {
    //#region form loading events
    $scope.formLoaded = function () {
        $http({
            method: "GET",
            url: "/HRIS/Home/GetAccessRight",
            dataType: "JSON",
            params: { _optionName: "Documents" }
        }).then(result => {
            session = result.data;
            setFormSession(session);
        },
            errorMsg => {
                showErrorToast(errorMsg);
            });
        LoadDropDown();
        setDateParameters();
    };

    function LoadDropDown() {
        $http.get(baseURL + 'GetLookups').then(result => {
            if (result.data.success) {
                $scope.lstDocumentType = result.data.lstDocumentType;
            }
            else {
                showErrorToast('Load dropdown failed with: ' + result.data.errorMsg);
            }
        }, errorMsg => {
            showErrorToast('Main dropdown loading function failed with following reasons\n' + errorMsg);
        });
    }
    //#endregion

    //#region toolbar actions
    $scope.tbRecall = function () {
        session.FormMode = "Recall";
        setFormSession(session);
        $('#txtEmployeeCode').focus();
    };

    $scope.tbSearch = function () {
        $scope.formTitle = "Search & Select Employee";
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
                $scope.getEmployeeByCode($rootScope.SelectedRow.EmployeeCode);
            }, function () {
            });
        }
        catch (exception) { showErrorToast(exception); }
    };

    $scope.getEmployeeByCode = function (res) {
        $http({
            method: "GET",
            url: baseURL + "Get",
            dataType: "JSON",
            params: { _keyId: res }
        }).then(result => {
            $scope.errors = {};
            $scope.EmpDocument = {};
            if (!result.data.success) {
                $scope.errors = getArray(result.data.message);
                showErrorToast(result.data.message);
            }
            else {
                $scope.EmpDocument = result.data.data;
                session.FormMode = result.data.formMode;
                setFormSession(session);
                maskDates();
                setGridData();
            }
        }, errorMsg => {
            showErrorToast(errorMsg);
        });
    };

    //to be verified later
    $scope.tbSave = function () {
        showWritingMsg();
        $scope.EmpDocument.Details = getDataFromGrid();
        $http({
            method: "POST",
            url: baseURL + "Save",
            data: $scope.EmpDocument
        }).then(result => {
            hideWritingMsg();
            $scope.errors = {};
            if (!result.data.success) {
                $scope.errors = getArray(result.data.message);
                showErrorToast();
            }
            else {
                showSuccessToast(result.data.message);
                session.FormMode = result.data.formMode;
                setFormSession(session);
                $scope.EmpDocument = {};
                setGridData();
            }
        }, errorMsg => {
            hideWritingMsg();
            showErrorToast(errorMsg);
        });
    };

    $scope.tbUndo = function () {
        try {
            $scope.EmpDocument = {};
            //$scope.footer = {};
            $scope.errors = {};
            setGridData();
            session.FormMode = "Initialize";
            setFormSession(session);
        }
        catch (exception) {
            showErrorToast(exception);
        }
    };

    $scope.tbExit = function () {
        $scope = null;
        window.location.href = '/HRIS/Common/Common/Index';
    };
    //#endregion

    //#region grid setup
    //#region Document Grid  
    var colDocument = [
        { headerName: "DocumentId", field: "DocumentId", hide: true, suppressToolPanel: true },
        {
            headerName: "Document Type", field: "DocTypeId", cellEditor: "agilitySelectEditor",
            cellEditorParams: { source: 'DocumentType', class: "agGridSelect tb250" },
            valueFormatter: getDocumentTypeName, width: 250
        },
        {
            headerName: "Document Name", field: "DocName", width: 525, editable: false,
            cellRenderer: params => {
                return "<a class='template' ng-href='View?_documentId=" + params.data.DocumentId + "' target='_blank' title='Click to view or download the document'>" + params.value + "</a>";
            }
        },
        {
            headerName: "Expiry Date", field: "ExpiryDate", width: 125,
            cellEditor: 'datePicker',
            cellEditorParams: { minDate: '+0d', maxDate: '+90Y' }
        },
        {
            headerName: "Reminder", field: "ReminderDays", type: "numericColumn",
            valueFormatter: formatNumbers, valueParser: numberValueParser, width: 100
        },
        {
            headerName: "Delete", width: 60, editable: false, field: "DocumentId",
            cellRenderer: params => {
                return "<button class='delete' title='Click to delete this row' ng-click='onDeleteDocument(" + params.value + ")'/>";
            }
        },
        { headerName: "Add", field: "Add", hide: true, suppressToolPanel: true },
        { headerName: "Edit", field: "Edit", hide: true, suppressToolPanel: true },
        { headerName: "Delete", field: "Delete", hide: true, suppressToolPanel: true }
    ];
   
    $scope.grdOptDocument = {
        headerHeight: 25,
        rowHeight: 32,
        animateRows: true,
        defaultColDef: {
            editable: true
        },
        columnDefs: colDocument,
        rowData: [],
        rowSelection: 'single',
        rowDeselection: true,
        enableColResize: true,
        enableSorting: true,
        singleClickEdit: true,
        angularCompileRows: true,
        onCellValueChanged: function (params) {
            if (!params.data.Add)
                params.data.Edit = true;
        },
        overlayLoadingTemplate: '<span class="ag-overlay-loading-center">Please wait while we are fetching requested records from database</span>',
        overlayNoRowsTemplate: '<span style="padding: 10px; border: 2px solid #444; background: lightgoldenrodyellow; color:red;">No rows available to display here</span>',
        components: {
            agilitySelectEditor: getAgilitySelect(),
            datePicker: getDatePicker()
        }
    };

    function getDataFromGrid() {
        let rowData = [];
        $scope.grdOptDocument.api.forEachNode(node => rowData.push(node.data));
        return rowData;
    }

    $scope.onAddDocument = function () {
        try {
            $scope.EmpDocument.AddNew = true;
        }
        catch (exception) { showErrorToast('Add Document: ' + exception); }
    };

    $scope.onUndoDocument = function () {
        try {
            $scope.EmpDocument.AddNew = false;
        }
        catch (exception) { showErrorToast('Add Document: ' + exception); }
    };

    $scope.onUpload = function () {
        try {
            if (validateFile() == true) {
                $http({
                    url: baseURL + "Upload",
                    method: "POST",
                    params: {
                        _employeeCode: $scope.EmpDocument.EmployeeCode,
                        _docTypeId: $scope.EmpDocument.DocTypeId,
                        _expiryDate: $scope.EmpDocument.ExpiryDate,
                        _reminderDays: $scope.EmpDocument.ReminderDays
                    },
                    headers: { "Content-Type": undefined },
                    transformRequest: function (data) {
                        var formData = new FormData();
                        var totalFiles = document.getElementById("fuAttachment").files.length;
                        for (var i = 0; i < totalFiles; i++) {
                            formData.append("files[" + i + "]", document.getElementById("fuAttachment").files[i]);//files);
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
                        $scope.EmpDocument = result.data.data;
                        $('#fuAttachment').val("");
                        maskDates();
                        setGridData();
                    }
                }, errorMsg => {
                    showErrorToast(errorMsg);
                });
            }
        }
        catch (exception) { showErrorToast(exception); }
    };

    $scope.onDeleteDocument = function (_keyId) {
        try {
            if ($scope.grdOptDocument.api.getSelectedRows().length > 0) {
                $scope.grdOptDocument.api.forEachNodeAfterFilter(function (rowNode, index) {
                    if (rowNode.data.DocumentId == _keyId) {
                        rowNode.data.Delete = true;
                    }
                });
                //$scope.grdOptDocument.api.getSelectedRows().forEach(x => x.Delete = true);
                //$scope.grdOptDocument.api.getFilterInstance('Delete').onFilterChanged();
                $scope.grdOptDocument.api.getFilterInstance('Delete').onFilterChanged();
            }
        }
        catch (exception) { showErrorToast('Delete Document: ' + exception); }
    };

    $scope.onViewDocument = function (_keyId) {
        try {
            $http({
                method: "POST",
                url: baseURL + "View",
                data: { _documentId: _keyId },
                headers: {
                    'Content-type': 'application/json'
                },
                responseType: 'arraybuffer'
            }).then(result => {
                // Get the filename from the x-filename header or default to "download.bin"
                var filename = headers['x-filename'] || _keyId.toString();
                // Determine the content type from the header or default to "application/octet-stream"
                var _contentType = headers['content-type'] || octetStreamMime;
                var blob = new Blob([result.data], { type: _contentType });
                saveAs(blob, filename);
            }, errorMsg => {
                showErrorToast(errorMsg);
            });
        }
        catch (exception) { showErrorToast('View Document: ' + exception);}
    };
    //#endregion

    //#region general functions
    function setGridData() {
        try {
            if ($scope.EmpDocument != []) {
                $scope.grdOptDocument.api.setRowData($scope.EmpDocument.Details);
                setGridDeleteFilter($scope.grdOptDocument.api);
            }
            else {
                $scope.grdOptDocument.api.setRowData([]);
            }
        }
        catch (exception) { showErrorToast(exception); }
    }

    function setDateParameters() {
        $('#dpExpiry').datepicker({
            startDate: '+0d',
            endDate: '+90Y'
        }).on('changeDate', function (e) {
            $scope.EmpDocument.ExpiryDate = e.format();
            $scope.$apply();
        });
    }

    function maskDates() {
        if ($scope.EmpDocument.Details != null) {
            for (i = 0; i < $scope.EmpDocument.Details.length; i++) {
                if ($scope.EmpDocument.Details[i]["ExpiryDate"] != null)
                    $scope.EmpDocument.Details[i]["ExpiryDate"] = formatDate($scope.EmpDocument.Details[i]["ExpiryDate"]);
            }
        }
    }

    function validateFile() {
        var fu = document.getElementById("fuAttachment");
        if (fu.value === "") {
            showErrorToast("Please select valid file to be uploaded before hitting the upload button");
            return false;
        }
        else if (document.getElementById("ddDocumentType").value == "")
        {
            showErrorToast("Please select valid Document Type before hitting the upload button");
            return false;
        }
        else {
            var validExts = new Array(".pdf", ".jpg", ".png", ".bmp", ".tiff");
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
    //#endregion
});
