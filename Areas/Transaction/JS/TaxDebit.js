baseURL = "/HRIS/Transaction/TaxDebit/";
agGrid.initialiseAgGridWithAngular1(angular);
var app = angular.module('HRIS', ['ui.bootstrap', 'agGrid', 'ngMaterial']);
let session = new Object();

app.directive('numbersOnly', function () {
    return {
        require: 'ngModel',
        link: function (scope, element, attr, ngModelCtrl) {
            function fromUser(text) {
                if (text) {
                    var transformedInput = text.replace(/[^0-9]/g, '');

                    if (transformedInput !== text) {
                        ngModelCtrl.$setViewValue(transformedInput);
                        ngModelCtrl.$render();
                    }
                    return transformedInput;
                }
                return undefined;
            }
            ngModelCtrl.$parsers.push(fromUser);
        }
    };
});

app.controller('TaxDebitController', function ($scope, $rootScope, $http, $uibModal, $mdToast) {
    //#region form loading events
    $scope.formLoaded = function () {
        var _debitId = document.getElementById('hdnDebitId').value;
        if (_debitId > 0) {
            loadAttachmentType();
            $scope.getTaxDebitById(_debitId); //,true
            $scope.MyForm = true;
            $scope.ReviewMode = true;
        }
        else {
            $scope.MyForm = false;
            $scope.ReviewMode = false;
            $http({
                method: "GET",
                url: "/HRIS/Home/GetAccessRight",
                dataType: "JSON",
                params: { _optionName: "Tax Debit" }
            }).then(result => {
                session = result.data;
                setFormSession(session);
            },
                errorMsg => {
                    showErrorToast(errorMsg);
                });
            session.FormMode = "Initialize";
            historyVisibility();
            loadDropDown();
        }
    };

    function loadDropDown() {
        try {
            $http.get(baseURL + 'GetLookups').then(result => {
                $scope.lstEmployee = result.data.lstEmployee;
                $scope.lstTaxDebitType = result.data.lstTaxDebitType;
                $scope.lstAttachmentType = result.data.lstAttachmentType;
            }, errorMsg => {
                showErrorToast(errorMsg);
            });
        }
        catch (exception) { showErrorToast('Load Drop Down:' + exception.message);}
    }

    function loadAttachmentType() {
        try {
            $http.get(baseURL + 'GetAttachmentTypes').then(result => {
                $scope.lstAttachmentType = result.data.lstAttachmentType;
            }, errorMsg => {
                showErrorToast(errorMsg);
            });
        }
        catch (exception) { showErrorToast('Load Attachment Types:' + exception.message); }
    }
    //#endregion

    //#region toolbar actions
    $scope.tbAdd = function () {
        $http({
            method: "Get",
            url: baseURL + "Add"
        }).then(response => {
            $scope.TaxDebit = response.data.taxDebit;
            $scope.Footer = $scope.TaxDebit.AuditTrail;
            session.FormMode = response.data.formMode;
            setFormSession(session);
            maskDates();
            $('#ddEmployee').focus();
        }, errorMsg => {
            showErrorToast(errorMsg);
        });
    };

    $scope.tbRecall = function () {
        session.FormMode = "Recall";
        setFormSession(session);
        $('#txtDebitId').focus();
    };

    $scope.tbSearch = function () {
        $scope.formTitle = "Search & Select Tax Debits";
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
                $scope.getTaxDebitById($rootScope.SelectedRow.TaxDebitId);
            }, function () {

            });
        }
        catch (exception) { showErrorToast(exception); }
    };

    $scope.tbEdit = function () {
        session.FormMode = "Edit";
        setFormSession(session);
        if (document.getElementById("btnSubmit") != null)
            document.getElementById("btnSubmit").disabled = true;
        if (document.getElementById("btnApprove") != null)
            document.getElementById("btnApprove").disabled = true;
        if (document.getElementById("btnReturn") != null)
            document.getElementById("btnReturn").disabled = true;
        if (document.getElementById("btnReject") != null)
            document.getElementById("btnReject").disabled = true;
        if (document.getElementById('hdnCompleted').value == "true" ||
            document.getElementById('ftrCreatedBy').value != document.getElementById('hdnOwner').value) {
            document.getElementById("btnSave").disabled = true;
        }
        else {
            $('#ddEmployee').focus();
        }
    }

    $scope.tbSave = function () {
        $scope.TaxDebit.Attachments = getDataFromGrid();
        $http({
            method: "POST",
            url: baseURL + "Save",
            data: $scope.TaxDebit
        }).then(result => {
            $scope.errors = [];
            if (!result.data.success) {
                $scope.errors = getArray(result.data.message);
                showErrorToast();
            }
            else {
                $scope.TaxDebit = result.data.taxDebit;
                session.FormMode = result.data.formMode;
                setFormSession(session);
                historyVisibility();
                if (document.getElementById("btnSubmit") != null) {
                    document.getElementById("btnSubmit").disabled = false;
                }
                if ($scope.TaxDebit.Owner == currentUser() && $scope.TaxDebit.Owner == $scope.TaxDebit.AuditTrail.CreatedBy) {
                    $("#divAttachment").find("input, select, textarea, button").prop("disabled", false);
                }
                showSuccessToast(result.data.message);
                setGridData();
            }
        }, errorMsg => {
            showErrorToast(errorMsg);
        });
    };

    $scope.tbUndo = function () {
        if ($scope.MyForm) {
            $scope = null;
            window.location.href = '/HRIS/Common/MyForms/DataForm';
        }
        else {
            clearForm();
            session.FormMode = "Initialize";
            setGridData();
            setFormSession(session);
        }
    };

    $scope.tbExit = function () {
        $scope = null;
        window.location.href = '/HRIS/Common/Common/Index';
    };

    $scope.tbHistory = function () {
        $scope.formId = $scope.TaxDebit.TaxDebitId;
        $scope.workFlowId = 2;
        $scope.formTitle = "History Of Tax Debit Request # " + $scope.TaxDebit.TaxDebitId;
        var modalScope = $scope.$new();
        var modalInstance = $uibModal.open({
            animation: true,
            templateUrl: '/HRIS/Helper/Pages/History.html',
            windowClass: 'show',
            backdropClass: 'show',
            size: "md",
            scope: modalScope
        });
        modalScope.modalInstance = modalInstance;
    };

    $scope.tbSubmission = function (res, _stateId) {
        $scope.formId = res;
        $scope.StateId = _stateId;
        if (_stateId == 4) {
            showYesNoToast("You are about to Reject this request permanently, Once rejected, no action could be taken on this request. Are you sure you want to proceed with Rejection?",
                function (result) {
                    if (result) { showSubmission(); }
                })
        }
        else { showSubmission(); }
    };
    //#endregion

    //#region Attachment Grid  
    var colAttachment = [
        { headerName: "AttachmentId", field: "AttachmentId", hide: true, suppressToolPanel: true },
        {
            headerName: "Attachment Type", field: "TypeOfAttachment.AttachmentTypeId", cellEditor: "agilitySelectEditor",
            cellEditorParams: { source: 'AttachmentType', class: "agGridSelect tb200" },
            valueFormatter: getAttachmentTypeName, width: 200
        },
        {
            headerName: "Attachment Name", field: "AttachmentName", width: 420, editable: false,
            cellRenderer: params => {
                return "<a class='Template' ng-href='View?_attachmentId=" + params.data.AttachmentId + "' target='_blank' title='Click to view or download the attachment'>" + params.value + "</a>";
            }
        },
        {
            headerName: "Delete", width: 60, editable: false, field: "AttachmentId",
            cellRenderer: params => {
                return "<button class='delete' title='Click to delete this row' ng-click='onDeleteAttachment(" + params.value + ")'/>";
            }
        },
        { headerName: "Add", field: "Add", hide: true, suppressToolPanel: true },
        { headerName: "Edit", field: "Edit", hide: true, suppressToolPanel: true },
        { headerName: "Delete", field: "Delete", hide: true, suppressToolPanel: true }
    ];

    $scope.grdAttachment = {
        headerHeight: 25,
        rowHeight: 32,
        animateRows: true,
        defaultColDef: {
            editable: true
        },
        columnDefs: colAttachment,
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
        $scope.grdAttachment.api.forEachNode(node => rowData.push(node.data));
        return rowData;
    }

    $scope.onAddAttachment = function () {
        try {
            $scope.TaxDebit.AddNew = true;
        }
        catch (exception) { showErrorToast('Add Attachment: ' + exception); }
    };

    $scope.onUndoAttachment = function () {
        try {
            $scope.TaxDebit.AddNew = false;
        }
        catch (exception) { showErrorToast('Undo Add Attachment: ' + exception); }
    };

    $scope.onUpload = function () {
        try {
            if (validateFile() == true) {
                $http({
                    url: baseURL + "Upload",
                    method: "POST",
                    params: {
                        _debitId: $scope.TaxDebit.TaxDebitId,
                        _attTypeId: $scope.TaxDebit.AttachmentTypeId
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
                        $scope.TaxDebit.Attachments = result.data.attachments;
                        $scope.TaxDebit.AddNew = false;
                        $('#fuAttachment').val("");
                        setGridData();
                    }
                }, errorMsg => {
                    showErrorToast(errorMsg);
                });
            }
        }
        catch (exception) { showErrorToast(exception); }
    };

    $scope.onDeleteAttachment = function (_keyId) {
        if (!document.getElementById('btnEdit').disabled) {
            showErrorToast('Delete function will only work after you switch form to Edit mode!');
            return;
        }
        if (!$scope.TaxDebit.Completed && $scope.TaxDebit.Owner == $scope.TaxDebit.AuditTrail.CreatedBy && $scope.TaxDebit.Owner == currentUser()) {
            try {
                if (confirm('Are you sure you want to delete selected row?, ok to proceed, cancel to go back!')) {
                    if ($scope.grdAttachment.api.getSelectedRows().length > 0) {
                        $scope.grdAttachment.api.forEachNodeAfterFilter(function (rowNode, index) {
                            if (rowNode.data.AttachmentId == _keyId) {
                                rowNode.data.Delete = true;
                            }
                        });
                        $scope.grdAttachment.api.getFilterInstance('Delete').onFilterChanged();
                    }
                }
            }
            catch (exception) { showErrorToast('Delete Attachment: ' + exception); }
        }
    };

    $scope.onViewAttachment = function (_keyId) {
        try {
            $http({
                method: "POST",
                url: baseURL + "View",
                data: { _attachmentId: _keyId },
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
        catch (exception) { showErrorToast('View Attachment: ' + exception); }
    };

    function setGridData() {
        try {
            if ($scope.TaxDebit != []) {
                $scope.grdAttachment.api.setRowData($scope.TaxDebit.Attachments);
                setGridDeleteFilter($scope.grdAttachment.api);
            }
            else {
                $scope.grdAttachment.api.setRowData([]);
            }
        }
        catch (exception) { showErrorToast(exception); }
    }

    function validateFile() {
        var fu = document.getElementById("fuAttachment");
        if (fu.value === "") {
            showErrorToast("Please select valid file to be uploaded before hitting the upload button");
            return false;
        }
        else if (document.getElementById("ddAttachmentType").value == "") {
            showErrorToast("Please select valid Attachment Type before hitting the upload button");
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

    //#region form functions
    $scope.getTaxDebitById = function (res) {
        $http({
            type: "Get",
            url: baseURL + "Get",
            dataType: "JSON",
            params: { _keyId: res }
        }).then(result => {
            clearForm();
            if (!result.data.success) {
                showErrorToast(result.data.message);
            }
            else {
                $scope.TaxDebit = result.data.taxDebit;
                $scope.footer = $scope.TaxDebit.AuditTrail;
                session.FormMode = result.data.formMode;
                setGridData();
                setFormSession(session);
                historyVisibility();
                maskDates();
                if ($scope.TaxDebit.Owner == currentUser() && $scope.TaxDebit.Owner == $scope.TaxDebit.AuditTrail.CreatedBy) {
                    $("#divAttachment").find("input, select, textarea, button").prop("disabled", false);
                }
                $("#divAttachment").find("div[class*='ag-root-wrapper']").removeClass('ag-disabled');
            }
        }, errorMsg => {
            showErrorToast(errorMsg.message);
        });
    };
    //#endregion

    //#region local function
    function clearForm() {
        $scope.TaxDebit = {};
        $scope.footer = {};
        $scope.errors = [];
        session.FormMode = "Initialize";
        historyVisibility();
    }

    function historyVisibility() {
        if (session.FormMode == "ReadOnly" || session.FormMode == "Review") {
            document.getElementById('btnHistory').style.visibility = 'visible'; // show
            document.getElementById("btnHistory").disabled = false;
        }
        else {
            document.getElementById('btnHistory').style.visibility = 'hidden'; // hide  
            document.getElementById("btnHistory").disabled = true;
        }
    }

    function showSubmission() {
        switch ($scope.StateId) {
            case 2:
                $scope.formTitle = "Submitted For Approval";
                break;
            case 3:
                $scope.formTitle = "Approve Tax Debit Request";
                break;
            case 4:
                $scope.formTitle = "Reject Tax Debit Request";
                break;
            case 5:
                $scope.formTitle = "Return Tax Debit Request";
                break;
        }

        var modalScope = $scope.$new();
        var modalInstance = $uibModal.open({
            animation: true,
            templateUrl: '/HRIS/Helper/Pages/FormSubmission.html',
            windowClass: 'show',
            backdropClass: 'show',
            size: "md",
            scope: modalScope
        });
        modalScope.modalInstance = modalInstance;
    };
    //#endregion

    //#region date functions
    function maskDates() {
        if ($scope.TaxDebit.AuditTrail != null)
            $scope.TaxDebit.AuditTrail.CreatedOn = formatDateTime($scope.TaxDebit.AuditTrail.CreatedOn);
        if ($scope.TaxDebit.AuditTrail.UpdatedOn != null)
            $scope.TaxDebit.AuditTrail.UpdatedOn = formatDateTime($scope.TaxDebit.AuditTrail.UpdatedOn);
    }
    //#endregion
});