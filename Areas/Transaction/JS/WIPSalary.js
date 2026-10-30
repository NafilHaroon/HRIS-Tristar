baseURL = "/HRIS/Transaction/WIPSalary/";
agGrid.initialiseAgGridWithAngular1(angular);
var app = angular.module('HRIS', ['ui.bootstrap', 'agGrid', 'ngMaterial']);
let session = "";

$(document).ready(function () {
    $("#btnEdit").click(function () {
        session.FormMode = "Edit";
        setFormSession(session);
        if(document.getElementById('hdVerified').value == 'true')
            $('#btnSave')[0].disabled = true;
        $('#txtUnpaidDays').focus();
    });

    $("#dialog-delete").dialog({
        autoOpen: false,
        resizable: false,
        height: 250,
        width: 400,
        modal: true,
        class: "DeleteAlert",
        dialogClass: "DeleteAlert",
        show: { effect: 'bounce' },
        title: "Confirm Delete Action",
        buttons: [
            {
                id: "btnDelete",
                text: "Delete",
                width: 120,
                class: "delete",
                click: function () {
                    $(this).dialog('close');
                    angular.element($("#ngForm")).scope().DeleteWIP();
                }
            },
            {
                id: "btnCancel",
                text: "Go Back!",
                width: 120,
                click: function () {
                    $(this).dialog('close');
                }
            }
        ],
        open: function () {
            //this special part is just done because delete and cancel were appearing in this form for unknown reason
            $(this).siblings('.ui-dialog-buttonpane').find('button:eq(1)').prop('disabled', false);
            $(this).siblings('.ui-dialog-buttonpane').find('button:eq(0)').prop('disabled', false);
            //end of this special part
            $(this).siblings('.ui-dialog-buttonpane').find('button:eq(1)').focus();
        }
    });
});

app.controller('WIPSalaryController', function ($scope, $rootScope, $http, $uibModal, $mdToast) {
    //#region form loading events
    $scope.formLoaded = function () {
        $http({
            method: "GET",
            url: "/HRIS/Home/GetAccessRight",
            dataType: "JSON",
            params: { _optionName: "Salary Processing" }
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
            $scope.lstEntitlementType = result.data.lstConsumable;
            $scope.lstSalaryHead = result.data.lstSalaryHead;
        }, errorMsg => {
            showErrorToast(errorMsg);
        });
    }
    //#endregion

    //#region toolbar actions
    $scope.tbRecall = function () {
        session.FormMode = "Recall";
        setFormSession(session);
        $('#ddEmployee').focus();
    };

    $scope.tbSearch = function () {
        $scope.formTitle = "Search & Select Employee for Salary processing";
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
                $scope.getWIPSalaryById($rootScope.SelectedRow.EmployeeId);
            }, function () {

            });
        }
        catch (exception) { showErrorToast(exception); }
    };

    $scope.tbSave = function () {
        $scope.WIPSalary.Salary = getSalaryDataFromGrid();
        $scope.WIPSalary.Entitlement = getEntitlementDataFromGrid();
        $http({
            method: "post",
            url: baseURL + "Save",
            data: $scope.WIPSalary
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

    $scope.getWIPSalaryById = function (_empId) {
        $http({
            type: "Get",
            url: baseURL + "Get",
            dataType: "JSON",
            params: { _keyId: _empId }
        }).then(result => {
            clearForm();
            if (!result.data.success) {
                $scope.errors = getArray(result.data.message);
                showErrorToast(result.data.message);
            }
            else {
                $scope.WIPSalary = result.data.wipSalary;
                $scope.footer = $scope.WIPSalary.AuditTrail;
                session.FormMode = result.data.formMode;
                maskDates();
                setGridData();
                setFormSession(session);
                document.getElementById("btnEdit").disabled = false;//edit is enable to let user scroll on grids but save will remain disabled if edit is not allowed
            }
        }, errorMsg => {
            showErrorToast(result);
        });
    };

    $scope.DeleteWIP = function () {
        try {
            var e = document.getElementById("ddEmployee");
            var _empId = e.options[e.selectedIndex].value.replace('number:', '');
            $http({
                type: "Get",
                url: baseURL + "Delete",
                dataType: "JSON",
                params: { _keyId: _empId }
            }).then(result => {
                if (!result.data.success) {
                    showErrorToast(result.data.message);
                }
                else {
                    showSuccessToast(result.data.message);
                    session.FormMode = result.data.formMode;
                    clearForm();
                    setGridData();
                    setFormSession(session);
                }
            }, errorMsg => {
                showErrorToast(result);
            });
        }
        catch (exception) { showErrorToast(exception.message); }
    };
    //#endregion

    //#region local function
    function clearForm() {
        $scope.WIPSalary = {};
        $scope.footer = {};
        $scope.errors = [];
    }

    function setGridData() {
        try {
            if (!angular.equals($scope.WIPSalary, {})) {  // (!jQuery.isEmptyObject($scope.WIPSalary){ // != []) {
                $scope.grdOptSalary.api.setRowData($scope.WIPSalary.Salary);
                $scope.grdOptEntitlement.api.setRowData($scope.WIPSalary.Consumable);
                setGridDeleteFilter($scope.grdOptSalary.api);
                setGridDeleteFilter($scope.grdOptEntitlement.api);
            }
            else {
                $scope.grdOptSalary.api.setRowData([]);
                $scope.grdOptEntitlement.api.setRowData([]);
            }
            setSalaryFooter();
        }
        catch (exception) { showErrorToast(exception); }
    }
    //#endregion

    //#region date functions
    function maskDates() {
        if ($scope.WIPSalary.AuditTrail != null) {
            $scope.WIPSalary.AuditTrail.CreatedOn = formatDateTime($scope.WIPSalary.AuditTrail.CreatedOn);
            if ($scope.WIPSalary.AuditTrail.UpdatedOn != null) {
                $scope.WIPSalary.AuditTrail.UpdatedOn = formatDateTime($scope.WIPSalary.AuditTrail.UpdatedOn);
            }
        }
    }
    //#endregion

    //#region grid setup
    //#region Salary Grid Definition & functions
    var colSalary = [
        {
            headerName: "Head", field: "HeadId", cellEditor: "agilitySelectEditor",
            cellEditorParams: { source: 'SalaryHead', class: "agGridSelect tb250" },
            valueFormatter: getSalaryHeadName, width: 250
        },
        {
            headerName: "Amount", field: "Amount", type: "numericColumn",
            valueFormatter: formatNumbers, valueParser: numberValueParser, width: 100
        },
        {
            headerName: "Remarks", field: "Remarks", width: 250
        },
        { headerName: "Auto", field: "SystemCalculated", hide: true, suppressToolPanel: true },
        { headerName: "Add", field: "Add", hide: true, suppressToolPanel: true },
        { headerName: "Edit", field: "Edit", hide: true, suppressToolPanel: true },
        { headerName: "Delete", field: "Delete", hide: true, suppressToolPanel: true }
    ];

    $scope.grdOptSalary = {
        headerHeight: 25,
        rowHeight: 32,
        animateRows: true,
        defaultColDef: {
            editable: allowSalaryEdit.bind(this)
        },
        columnDefs: colSalary,
        rowData: [],
        rowSelection: 'single',
        rowDeselection: true,
        enableColResize: true,
        enableSorting: true,
        singleClickEdit: true,
        stopEditingWhenGridLosesFocus: true,
        getRowNodeId: function (data) { return data.LoanId == null ? data.HeadId : data.LoanId; },
        onGridReady: (grid) => {
            setSalaryFooter();
        },
        getRowStyle: function (params) {
            if (params.node.rowPinned) {
                return { 'font-weight': 'bold', 'color': 'white', 'background-color': 'gray' };
            }
            else if (params.node.data.SystemCalculated) {
                return { 'color': 'darkgray', 'font-style': 'italic' };
            }
        },
        onCellValueChanged: function (params) {
            if (!params.data.Add)
                params.data.Edit = true;
            setSalaryFooter();
        },
        overlayLoadingTemplate: '<span class="ag-overlay-loading-center">Please wait while we are fetching requested records from database</span>',
        overlayNoRowsTemplate: '<span style="padding: 10px; border: 2px solid #444; background: lightgoldenrodyellow; color:red;">No rows available to display here</span>',
        components: {
            agilitySelectEditor: getAgilitySelect()
        }
    };

    function getSalaryDataFromGrid() {
        let rowData = [];
        $scope.grdOptSalary.api.forEachNode(node => rowData.push(node.data));
        return rowData;
    }

    $scope.onAddSalary = function () {
        try {
            var res = $scope.grdOptSalary.api.updateRowData({
                add: [{
                    HeadId: null, Amount: 0, Remarks: null,  SystemCalculated: false, Add: true, Edit: false, Delete: false
                }]
            });
            $scope.grdOptSalary.api.startEditingCell({ rowIndex: res.add[0].rowIndex, colKey: "HeadId" });
        }
        catch (exception) { showErrorToast('Add Salary Head: ' + exception); }
    };

    $scope.onDeleteSalary = function () {
        try {
            if ($scope.grdOptSalary.api.getSelectedRows().length > 0) {
                if (confirm("Are you sure you want to Delete selected row?")) {
                    $scope.grdOptSalary.api.getSelectedRows().forEach(x => x.Delete = true);
                    $scope.grdOptSalary.api.getFilterInstance('Delete').onFilterChanged();
                    setSalaryFooter();
                }
            }
            else
                showErrorToast("No Row selected to apply Delete operation. Please select row and then hit Delete button!");
        }
        catch (exception) { showErrorToast('Delete Salary Head: ' + exception); }
    };

    function setSalaryFooter() {
        try {
            let _total = 0;
            if ($scope.grdOptSalary.api.getDisplayedRowCount() > 0) {
                $scope.grdOptSalary.api.forEachNode(n => _total +=
                    n.data.HeadId != undefined && n.data.HeadId != -1 && n.data.HeadId != "" && !n.data.Delete ?
                        n.data.Amount * $scope.lstSalaryHead.find(sh => { return sh.HeadId == n.data.HeadId; })["Symbol"] : 0);
            }
            var _footer = [{ HeadId: -1, Amount: _total }];
            $scope.grdOptSalary.api.setPinnedBottomRowData(_footer);
        }
        catch (exception) { showErrorToast('Salary Footer Method:' + exception); }
    }

    $scope.CalculateSalary = function () {
        if ($scope.WIPSalary.UnpaidDays > $scope.WIPSalary.SalaryDays) {
            showErrorToast("Absent days can't be more than total days in the month!");
            $scope.WIPSalary.UnpaidDays = 0;
        }
        else {
            $scope.WIPSalary.PaidDays = $scope.WIPSalary.SalaryDays - $scope.WIPSalary.UnpaidDays;
            var _absentId = $scope.WIPSalary.AbsentId;
            /*if ($scope.WIPSalary.UnpaidDays > 0) {*/
            if ($scope.grdOptSalary.api.getDisplayedRowCount() > 0) {
                var _deduction = 0;
                $scope.grdOptSalary.api.forEachNode(s => _deduction +=
                    s.data.HeadId != "" && s.data.HeadId != _absentId && !s.data.Delete && $scope.lstSalaryHead.find(sh => sh.HeadId == s.data.HeadId)["DeductForAbsent"] ?
                        s.data.Amount / $scope.WIPSalary.SalaryDays * 1.0 * $scope.WIPSalary.UnpaidDays : 0);
                _deduction = Math.round(_deduction);
                var rowNode;
                rowNode = $scope.grdOptSalary.api.getRowNode(_absentId);
                if (rowNode != undefined) {
                    if (_deduction == 0) {
                        rowNode.setDataValue('Amount', _deduction);
                        rowNode.setDataValue('Delete', true);
                    }
                    else {
                        rowNode.setDataValue('Amount', _deduction);
                        rowNode.setDataValue('SystemCalculated', true);
                        if (!rowNode.data.Add) {
                            rowNode.setDataValue('Edit', true);
                            rowNode.setDataValue('Add', false);
                            rowNode.setDataValue('Delete', false);
                        }
                    }
                }
                else {
                    $scope.grdOptSalary.api.updateRowData({
                        add: [{
                            DetailId: null, HeadId: _absentId, Amount: _deduction,
                            Remarks: null, SystemCalculated: true,
                            Add: true, Edit: false, Delete: false
                        }]
                    });
                }
            }
        }
    };
    //#endregion

    //#region Entitlement Grid Definition & functions
    var colEntitlement = [
        {
            headerName: "Entitlement Type", field: "EntitlementTypeId", cellEditor: "agilitySelectEditor",
            cellEditorParams: { source: 'EntitlementType', class: "agGridSelect tb170" },
            valueFormatter: getEntitlementTypeName, width: 170
        },
        {
            headerName: "Limit", field: "Limit", type: "numericColumn",
            valueFormatter: formatNumbers, valueParser: numberValueParser, width: 100
        },       
        {
            headerName: "Remarks", field: "Remarks", width: 200
        },
        { headerName: "Add", field: "Add", hide: true, suppressToolPanel: true },
        { headerName: "Edit", field: "Edit", hide: true, suppressToolPanel: true },
        { headerName: "Delete", field: "Delete", hide: true, suppressToolPanel: true }
    ];

    $scope.grdOptEntitlement = {
        headerHeight: 25,
        rowHeight: 32,
        animateRows: true,
        defaultColDef: {
            editable: allowEdit.bind(this)
        },
        columnDefs: colEntitlement,
        rowData: [],
        rowSelection: 'single',
        rowDeselection: true,
        enableColResize: true,
        enableSorting: true,
        singleClickEdit: true,
        stopEditingWhenGridLosesFocus: true,
        getRowStyle: function (params) {
            if (params.node.rowPinned) {
                return { 'font-weight': 'bold', 'color': 'blue' };
            }
        },
        onCellValueChanged: function (params) {
            if (!params.data.Add)
                params.data.Edit = true;
          
        },
        overlayLoadingTemplate: '<span class="ag-overlay-loading-center">Please wait while we are fetching requested records from database</span>',
        overlayNoRowsTemplate: '<span style="padding: 10px; border: 2px solid #444; background: lightgoldenrodyellow; color:red;">No rows available to display here</span>',
        components: {
            agilitySelectEditor: getAgilitySelect()
        }
    };

    function getEntitlementDataFromGrid() {
        let rowData = [];
        $scope.grdOptEntitlement.api.forEachNode(node => rowData.push(node.data));
        return rowData;
    }

    $scope.onAddEntitlement = function () {
        try {
            var res = $scope.grdOptEntitlement.api.updateRowData({
                add: [{
                    EntitlementTypeId: null, Limit: 0, Add: true, Edit: false, Delete: false
                }]
            });
            $scope.grdOptEntitlement.api.startEditingCell({ rowIndex: res.add[0].rowIndex, colKey: "EntitlementTypeId" });
        }
        catch (exception) { showErrorToast('Add Entitlement: ' + exception); }
    };

    $scope.onDeleteEntitlement = function () {
        try {
            if ($scope.grdOptEntitlement.api.getSelectedRows().length > 0) {
                $scope.grdOptEntitlement.api.getSelectedRows().forEach(x => x.Delete = true);
                $scope.grdOptEntitlement.api.getFilterInstance('Delete').onFilterChanged();
            
            }
        }
        catch (exception) { showErrorToast('Delete Entitlement: ' + exception); }
    };
    //#endregion
    //#endregion
});