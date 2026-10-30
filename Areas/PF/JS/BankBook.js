baseURL = "/HRIS/PF/BankBook/";
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

app.controller('BankBookController', function ($scope, $rootScope, $http, $uibModal, $mdToast) {
    //#region form loading events
    $scope.formLoaded = function () {
        $http({
            method: "GET",
            url: "/HRIS/Home/GetAccessRight",
            dataType: "JSON",
            params: { _optionName: "Bank Book" }
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
                $scope.lstBankAccount = result.data.lstBankAccount;
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
            $scope.BankBook = response.data.bb;
            $scope.Footer = response.data.bb.AuditTrail;
            session.FormMode = response.data.formMode;
            setFormSession(session);
            maskDates();
            $('#ddBankAccount').focus();
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
        $scope.formTitle = "Search & Select Bank Vouchers";
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
                $scope.getBBVoucherByNo($rootScope.SelectedRow.VoucherNo);
            }, function () {

            });
        }
        catch (exception) { showErrorToast(exception); }
    };

    $scope.tbSave = function () {
        showWritingMsg();
        $scope.BankBook.Details = getBBDataFromGrid();
        $http({
            method: "POST",
            url: baseURL + "Save",
            data: $scope.BankBook
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
    
    $scope.getBBVoucherByNo = function (res) {
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
                $scope.BankBook = result.data.bb;
                $scope.footer = $scope.BankBook.AuditTrail;
                session.FormMode = result.data.formMode;
                setGridData();
                maskDates();
                setFormSession(session);
            }
        }, errorMsg => {
            showErrorToast(result);
        });
    };
    //#endregion

    //#region local function
    function clearForm() {
        $scope.BankBook = {};
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
            $scope.BankBook.VoucherDate = e.format();
            $scope.$apply();
        });

        $('#dpCheque').datepicker({
            startDate: '-150d',
            endDate: '+180d'
        }).on('changeDate', function (e) {
            $scope.BankBook.ChequeDate = e.format();//dt;
            $scope.$apply();
        });      
    }

    function maskDates() {
        $scope.BankBook.VoucherDate = formatDate($scope.BankBook.VoucherDate);
        $scope.BankBook.ChequeDate = formatDate($scope.BankBook.ChequeDate);

        if ($scope.BankBook.AuditTrail != null) {
            $scope.BankBook.AuditTrail.CreatedOn = formatDateTime($scope.BankBook.AuditTrail.CreatedOn);
        }
    }
    //#endregion

    //#region grid setup
    //#region Bank Book Grid Definition & functions
    var colBankBook = [
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

    $scope.grdOptBankBook = {
        headerHeight: 25,
        rowHeight: 32,
        animateRows: true,
        defaultColDef: {
            editable: allowEdit.bind(this)
        },
        columnDefs: colBankBook,
        rowData: [],
        rowSelection: 'single',
        rowDeselection: true,
        enableColResize: true,
        enableSorting: true,
        singleClickEdit: true,
        onGridReady: (grid) => {
            setBankBookFooter();
        },
        getRowStyle: function (params) {
            if (params.node.rowPinned) {
                return { 'font-weight': 'bold', 'color': 'blue' };
            }
        },
        onCellValueChanged: function (params) {
            if (params.colDef.field === "Debit" || params.colDef.field === "Credit")
                setBankBookFooter();
        },
        overlayLoadingTemplate: '<span class="ag-overlay-loading-center">Please wait while we are fetching requested records from database</span>',
        overlayNoRowsTemplate: '<span style="padding: 10px; border: 2px solid #444; background: lightgoldenrodyellow; color:red;">No rows available to display here</span>',
        components: {
            agilitySelectEditor: getAgilitySelect()
        }
    };

    function getBBDataFromGrid() {
        let rowData = [];
        $scope.grdOptBankBook.api.forEachNode(node => rowData.push(node.data));
        return rowData;
    }

    $scope.onAddLine = function () {
        try {
            var res = $scope.grdOptBankBook.api.updateRowData({
                add: [{
                    AccountId: null, Debit: 0, Credit: 0, Description: null, Delete: false
                }]
            });
            $scope.grdOptBankBook.api.startEditingCell({ rowIndex: res.add[0].rowIndex, colKey: "AccountId" });
        }
        catch (exception) { showErrorToast('Add Line: ' + exception); }
    };

    $scope.onDeleteLine = function () {
        try {
            if ($scope.grdOptBankBook.api.getSelectedRows().length > 0) {
                if (confirm("Are you sure you want to Delete selected row?")) {
                    $scope.grdOptBankBook.api.getSelectedRows().forEach(x => x.Delete = true);
                    $scope.grdOptBankBook.api.getFilterInstance('Delete').onFilterChanged();
                    setBankBookFooter();
                }
            }
            else
                showErrorToast("No Row selected to apply Delete operation. Please select row and then hit Delete button!");
        }
        catch (exception) { showErrorToast('Delete Line Item: ' + exception); }
    };

    function setBankBookFooter() {
        try {
            let _debit = 0, _credit=0;
            $scope.grdOptBankBook.api.forEachNode(n => _debit +=
                n.data.AccountId !== undefined && n.data.AccountId !== -1 ?
                    n.data.Debit : 0);
            $scope.grdOptBankBook.api.forEachNode(n => _credit +=
                n.data.AccountId !== undefined && n.data.AccountId !== -1 ?
                n.data.Credit : 0);
            var _footer = [{ AccountId: -1, Description: null, Debit: _debit, Credit: _credit }];            
            $scope.grdOptBankBook.api.setPinnedBottomRowData(_footer);

            var _balance = parseFloat(_debit) - parseFloat(_credit); var _result;
            if (_balance != 0)
                _result = (_balance < 0 ? "Debit with " : "Credit with ") + formatNum(Math.abs(_balance));
            else
                _result = "";
            $("#lblBalance").val(_result);
        }
        catch (exception) { showErrorToast('Calculate Total: ' + exception); }
    }
    //#endregion       

    //#region general functions
    function setGridData() {
        if ($scope.BankBook !== []) {
            $scope.grdOptBankBook.api.setRowData($scope.BankBook.Details);
            setGridDeleteFilter($scope.grdOptBankBook.api);
        }
        else { $scope.grdOptBankBook.api.setRowData([]); }
        setBankBookFooter();
    }
    //#endregion
    //#endregion
});

///this function is mentioned here as having formatnumbers in gridhelper and formlayout were creating conflict and issue
function formatNum(x) {
    var parts = x.toString().split(".");
    parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ",");
    return parts.join(".");
}