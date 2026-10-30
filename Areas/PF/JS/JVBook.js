baseURL = "/HRIS/PF/JVBook/";
agGrid.initialiseAgGridWithAngular1(angular);
var app = angular.module('HRIS', ['ui.bootstrap', 'agGrid', 'ngMaterial']);
let session = "";

$(document).ready(function () {
    $("#btnEdit").click(function () {
        session.FormMode = "Edit";
        setFormSession(session);
        document.getElementById("btnSave").disabled = true;
    });
});

app.controller('JVBookController', function ($scope, $rootScope, $http, $uibModal, $mdToast) {
    //#region form loading events
    $scope.formLoaded = function () {
        $http({
            method: "GET",
            url: "/HRIS/Home/GetAccessRight",
            dataType: "JSON",
            params: { _optionName: "JV Book" }
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
            if (result.data.success == true) {
                $scope.lstAccount = result.data.lstAccount;
            }
            else {
                showErrorToast(result.data.errorMsg);
            }
        }, errorMsg => {
            showErrorToast('Main dropdown loading function failed with following reasons\n' + errorMsg);
        });
    }
    //#endregion

    //#region toolbar actions
    $scope.tbAdd = function () {
        $http({
            method: "Get",
            url: baseURL + "Add"
        }).then(response => {
            $scope.JVBook = response.data.jv;
            $scope.Footer = $scope.JVBook.AuditTrail;
            session.FormMode = response.data.formMode;
            setFormSession(session);
            maskDates();
            $('#txtRemarks').focus();
        }, errorMsg => {
            showErrorToast(errorMsg);
        });
    };

    $scope.tbRecall = function () {
        session.FormMode = "Recall";
        setFormSession(session);
        $('#txtVoucherNo').focus();
    };

    $scope.tbSearch = function () {
        $scope.formTitle = "Search & Select Journal Voucher";
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
                $scope.getJVoucherByNo($rootScope.SelectedRow.VoucherNo);
            }, function () {

            });
        }
        catch (exception) { showErrorToast(exception); }
    };

    $scope.tbSave = function () {
        showWritingMsg();
        $scope.JVBook.Details = getJVDataFromGrid();
        $http({
            method: "POST",
            url: baseURL + "Save",
            data: $scope.JVBook
        }).then(result => {
            hideWritingMsg();
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
            }
        }, errorMsg => {
            hideWritingMsg();
            showErrorToast(errorMsg);
        });
    };

    $scope.tbUndo = function () {
        clearForm();
        session.FormMode = "Initialize";
        setFormSession(session);
    };

    $scope.tbExit = function () {
        $scope = null;
        window.location.href = '/HRIS/Common/Common/Index';
    };
    
    $scope.getJVoucherByNo = function (res) {
        if (res != "" && res != null & res != undefined) {
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
                    $scope.JVBook = result.data.jv;
                    $scope.footer = $scope.JVBook.AuditTrail;
                    session.FormMode = result.data.formMode;
                    setGridData();
                    maskDates();
                    setFormSession(session);
                }
            }, errorMsg => {
                showErrorToast(result);
            });
        }
        else { showErrorToast("Please enter valid voucher number before hitting enter!");}
    };
    //#endregion

    //#region local function
    function clearForm() {
        $scope.JVBook = {};
        $scope.footer = {};
        $scope.errors = [];
        setGridData();
    }
    //#endregion

    //#region date functions
    function setDateParameters() {
        $('#dpVoucher').datepicker({
            startDate: '-30d'
        }).on('changeDate', function (e) {
            $scope.JVBook.VoucherDate = e.format();
            $scope.$apply();
        });
    }

    function maskDates() {
        $scope.JVBook.VoucherDate = formatDate($scope.JVBook.VoucherDate);

        if ($scope.JVBook.AuditTrail != null) {
            $scope.JVBook.AuditTrail.CreatedOn = formatDateTime($scope.JVBook.AuditTrail.CreatedOn);
        }
    }
    //#endregion

    //#region grid setup
    //#region Jv Book Grid Definition & functions
    var colJVBook = [
        {
            headerName: "Account", field: "AccountId", cellEditor: "agilitySelectEditor",
            cellEditorParams: { source: 'Account', class: "agGridSelect tb350" },
            valueFormatter: getAccountName, width: 350
        },
        {
            headerName: "Description", field: "Description", cellEditor: "agTextCellEditor",
            width: 200
        },
        {
            headerName: "Debit", field: "Debit", type: "numericColumn",
            valueFormatter: formatNumbers, valueParser: numberValueParser, width: 120
        },
        {
            headerName: "Credit", field: "Credit", type: "numericColumn",
            valueFormatter: formatNumbers, valueParser: numberValueParser, width: 120
        },
        {
            headerName: "Loan #", field: "LoanId", type: "numericColumn", width: 80
        },
        { headerName: "Delete", field: "Delete", hide: true, suppressToolPanel: true }
    ];

    $scope.grdOptJVBook = {
        headerHeight: 25,
        rowHeight: 32,
        animateRows: true,
        defaultColDef: {
            editable: allowEdit.bind(this)
        },
        columnDefs: colJVBook,
        rowData: [],
        rowSelection: 'single',
        rowDeselection: true,
        enableColResize: true,
        enableSorting: true,
        singleClickEdit: true,
        onGridReady: (grid) => {
            setJVBookFooter();
        },
        getRowStyle: function (params) {
            if (params.node.rowPinned) {
                return { 'font-weight': 'bold', 'color': 'blue' };
            }
        },
        onCellValueChanged: function (params) {
            if (params.colDef.field == "Debit" || params.colDef.field == "Credit")
                setJVBookFooter();
        },
        overlayLoadingTemplate: '<span class="ag-overlay-loading-center">Please wait while we are fetching requested records from database</span>',
        overlayNoRowsTemplate: '<span style="padding: 10px; border: 2px solid #444; background: lightgoldenrodyellow; color:red;">No rows available to display here</span>',
        components: {
            agilitySelectEditor: getAgilitySelect()
        }
    };

    function getJVDataFromGrid() {
        let rowData = [];
        $scope.grdOptJVBook.api.forEachNode(node => rowData.push(node.data));
        return rowData;
    }
       
    $scope.onAddLine = function () {
        try {
            var res = $scope.grdOptJVBook.api.updateRowData({
                add: [{
                    AccountId: null, Debit: 0, Credit: 0, Description: null, Delete: false
                }]
            });
            $scope.grdOptJVBook.api.startEditingCell({ rowIndex: res.add[0].rowIndex, colKey: "AccountId" });
        }
        catch (exception) { showErrorToast('Add Line: ' + exception); }
    };

    $scope.onDeleteLine = function () {
        try {
            if ($scope.grdOptJVBook.api.getSelectedRows().length > 0) {
                if (confirm("Are you sure you want to Delete selected row?")) {
                    $scope.grdOptJVBook.api.getSelectedRows().forEach(x => x.Delete = true);
                    $scope.grdOptJVBook.api.getFilterInstance('Delete').onFilterChanged();
                    setJVBookFooter();
                }
            }
            else
                showErrorToast("No Row selected to apply Delete operation. Please select row and then hit Delete button!");
        }
        catch (exception) { showErrorToast('Delete Line Item: ' + exception); }
    };

    function setJVBookFooter() {
        try {
            let _debit = 0, _credit = 0;
            $scope.grdOptJVBook.api.forEachNode(n => _debit +=
                n.data.AccountId != undefined && n.data.AccountId != -1 ?
                    n.data.Debit : 0);
            $scope.grdOptJVBook.api.forEachNode(n => _credit +=
                n.data.AccountId != undefined && n.data.AccountId != -1 ?
                    n.data.Credit : 0);
            var _footer = [{ AccountId: -1, Description: null, Debit: _debit, Credit: _credit }];
            $scope.grdOptJVBook.api.setPinnedBottomRowData(_footer);
        }
        catch (exception) { showErrorToast('Calculate Total: ' + exception); }
    }
    //#endregion       

    //#region general functions
    function setGridData() {
        if ($scope.JVBook != []) {
            $scope.grdOptJVBook.api.setRowData($scope.JVBook.Details);
            setGridDeleteFilter($scope.grdOptJVBook.api);
        }
        else { $scope.grdOptJVBook.api.setRowData([]); }
        setJVBookFooter();
    }

    $scope.sum = function (items, prop) {
        return items.reduce(function (a, b) {
            return a + b[prop];
        }, 0);
    };
    //#endregion
    //#endregion
});