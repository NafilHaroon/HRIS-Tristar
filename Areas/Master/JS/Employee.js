baseURL = "/HRIS/Master/Employee/";
agGrid.initialiseAgGridWithAngular1(angular);
var app = angular.module('HRIS', ['ui.bootstrap', 'agGrid', 'ngMaterial']); 
let session = "";

$(document).ready(function () {
    $("#btnEdit").click(function () {
        session.FormMode = "Edit";
        setFormSession(session);
        if (document.getElementById('dpJoining').value != "") {
            document.getElementById('dpJoining').disabled = true;
        }
        if (document.getElementById('dpConfirmationDue').value != "") {
            document.getElementById('dpConfirmationDue').disabled = true;
        }
        if (document.getElementById('dpConfirmation').value != "") {
            document.getElementById('chkConfirmed').disabled = true;
            document.getElementById('dpConfirmation').disabled = true;
        }
        if (document.getElementById('txtEmployeeCode').value != "") {
            document.getElementById('ddHiringType').disabled = true;
        }
        $('#txtEmployeeName').focus();
    });
});

app.controller('EmployeeController', function ($scope, $rootScope, $http, $uibModal, $mdToast) {
    //#region form loading events
    $scope.formLoaded = function () {
        $http({
            method: "GET",
            url: "/HRIS/Home/GetAccessRight",
            dataType: "JSON",
            params: { _optionName: "Employee" }
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
            if (result.data.success) {
                $scope.lstBranch = result.data.lstBranch;
                $scope.lstDepartment = result.data.lstDepartment;
                $scope.lstSupervisor = result.data.lstSupervisor;
                $scope.lstReplacement = result.data.lstReplacement;
                $scope.lstStatus = result.data.lstEmployeeStatus;
                $scope.lstJobNature = result.data.lstJobNature;
                $scope.lstMaritalStatus = result.data.lstMaritalStatus;
                $scope.lstGrade = result.data.lstGrade;
                $scope.lstQualification = result.data.lstQualification;
                $scope.lstPaymentMode = result.data.lstPaymentMode;
                $scope.lstBank = result.data.lstBank;
                $scope.lstEntitlementType = result.data.lstEntitlementType;
                $scope.lstRelation = result.data.lstRelation;
                $scope.lstMedicalPlan = result.data.lstMedicalPlan;
                $scope.lstSalaryHead = result.data.lstSalaryHead;
                $scope.lstAllocTemplate = result.data.lstAllocTemplate;
                $scope.lstWorkerType = result.data.lstWorkerType;
                $scope.lstSalutation = result.data.lstSalutation;
                $scope.lstSubCategory = result.data.lstSubCategory;
                $scope.lstGender = [{
                    GenderId: 1,
                    GenderName: 'Male'
                }, {
                    GenderId: 2,
                    GenderName: 'Female'
                }];

                $scope.lstHiringType = [{
                    HiringTypeId: 1,
                    HiringTypeName: 'Additional'
                }, {
                    HiringTypeId: 2,
                    HiringTypeName: 'Replacement - Separation'
                }, {
                    HiringTypeId: 3,
                    HiringTypeName: 'Replacement - Promotion'
                }, {
                    HiringTypeId: 4,
                    HiringTypeName: 'Additional - ERF'
                }, {
                    HiringTypeId: 5,
                    HiringTypeName: 'Additional - Feasibility/Project'
               },{
                    HiringTypeId: 6,
                    HiringTypeName: 'Additional - Other'
               }];
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
    $scope.tbAdd = function () {
        $http({
            method: "Get",
            url: baseURL + "Add"
        }).then(response => {
            $scope.Employee = response.data.employee;
            $scope.Footer = response.data.employee.AuditTrail;
            session.FormMode = response.data.formMode;
            setFormSession(session);
            setGridData();
            maskDates();
            $('#txtEmployeeName').focus();
        }, errorMsg => {
            showErrorToast(errorMsg);
        });
    };

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

    $scope.tbSave = function () {
        showWritingMsg();
        $scope.Employee.Salary = getSalaryDataFromGrid();
        $scope.Employee.CostAllocations = getCostAllocationDataFromGrid();
        $scope.Employee.Family = getFamilyDataFromGrid();
        $scope.Employee.Entitlements = getEntitlementDataFromGrid();
        $http({
            method: "POST",
            url: baseURL + "Save",
            data: $scope.Employee
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
                $scope.Employee = {};
                setGridData();
                setFormSession(session);
            }
        }, errorMsg => {
            hideWritingMsg();
            showErrorToast(errorMsg);
        });
    };

    $scope.tbUndo = function () {
        try {
            $scope.Employee = {};
            $scope.footer = {};
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

    $scope.getEmployeeByCode = function (res) {
        $http({
            method: "GET",
            url: baseURL + "Get",
            dataType: "JSON",
            params: { _keyId: res }
        }).then(result => {
            $scope.errors = {};
            $scope.Employee = {};
            if (!result.data.success) {
                $scope.errors = getArray(result.data.message);
                showErrorToast(result.data.message);
            }
            else {
                if (result.data.data.HiringTypeId == 2)
                    $scope.lstReplacement = result.data.lstReplacement;
                $scope.Employee = result.data.data;
                $scope.footer = $scope.Employee.AuditTrail;
                session.FormMode = result.data.formMode;
                maskDates();
                setGridData();
                setFormSession(session);
                document.getElementById("btnEdit").disabled = false;//edit is enable to let user scroll on grids but save will remain disabled if edit is not allowed
            }
        }, errorMsg => {
            showErrorToast(errorMsg);
        });
    };
    //#endregion

    //#region grid setup
    //#region Salary Grid
    var colSalary = [
        {
            headerName: "HeadId", field: "HeadId", cellEditor: "agilitySelectEditor",
            cellEditorParams: { source: 'SalaryHead', class: "agGridSelect tb250" },
            valueFormatter: getSalaryHeadName, width: 250
        },
        {
            headerName: "Amount", field: "Amount", type: "numericColumn",
            valueFormatter: formatNumbers, valueParser: numberValueParser, width: 120
        },
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
            var res = $scope.grdOptSalary.api.updateRowData({ add: [{ HeadId: null, Amount: 0, Add: true, Edit: false, Delete: false }] });
            $scope.grdOptSalary.api.startEditingCell({ rowIndex: res.add[0].rowIndex, colKey: "HeadId" });
        }
        catch (exception) { showErrorToast('Add Salary Head: ' + exception); }
    };

    $scope.onDeleteSalary = function () {
        try {
            if ($scope.grdOptSalary.api.getSelectedRows().length > 0) {
                $scope.grdOptSalary.api.getSelectedRows().filter(y => !y.SystemCalculated).forEach(x => x.Delete = true);
                $scope.grdOptSalary.api.getFilterInstance('Delete').onFilterChanged();
                setSalaryFooter();
            }
        }
        catch (exception) { showErrorToast('Delete Salary Head: ' + exception); }
    };

    function setSalaryFooter() {
        try {
            let _total = 0;
            if ($scope.grdOptSalary.api.getDisplayedRowCount() > 0) {
                $scope.grdOptSalary.api.forEachNode(n => _total +=
                    n.data.HeadId != undefined && n.data.HeadId != -1 && n.data.HeadId != null && n.data.HeadId != "" && !n.data.Delete ?
                        n.data.Amount * $scope.lstSalaryHead.find(sh => { return sh.HeadId == n.data.HeadId; })["Symbol"] : 0);
            }
            var _footer = [{ HeadId: -1, Amount: _total }];
            $scope.grdOptSalary.api.setPinnedBottomRowData(_footer);
        }
        catch (exception) { showErrorToast('Salary Footer Method:' + exception); }
    }
    //#endregion

    //#region Cost Allocation Grid
    var colCostAllocation = [
        {
            headerName: "Template", field: "TemplateId", cellEditor: "agilitySelectEditor",
            cellEditorParams: { source: 'Template', class: "agGridSelect tb130" },
            valueFormatter: getAllocTemplateName, width: 130
        },
        {
            headerName: "Branch", field: "BranchCode", cellEditor: "agilitySelectEditor",
            cellEditorParams: { source: 'Branch', class: "agGridSelect tb170" },
            valueFormatter: getBranchName, width: 170
        },
        {
            headerName: "Rate (%)", field: "Rate", type: "numericColumn",
            valueFormatter: formatNumbers, valueParser: numberValueParser, width: 100
        },
        { headerName: "Add", field: "Add", hide: true, suppressToolPanel: true },
        { headerName: "Edit", field: "Edit", hide: true, suppressToolPanel: true },
        { headerName: "Delete", field: "Delete", hide: true, suppressToolPanel: true }
    ];

    $scope.grdOptCostAlloc = {
        headerHeight: 25,
        rowHeight: 32,
        animateRows: true,
        defaultColDef: {
            editable: allowEdit.bind(this)
        },
        columnDefs: colCostAllocation,
        rowData: [],
        rowSelection: 'single',
        rowDeselection: true,
        enableColResize: true,
        enableSorting: true,
        singleClickEdit: true,
        stopEditingWhenGridLosesFocus: true,
        onGridReady: (grid) => {
            setCostAllocationFooter();
        },
        getRowStyle: function (params) {
            if (params.node.rowPinned) {
                return { 'font-weight': 'bold', 'color': 'blue' };
            }
        },
        onCellValueChanged: function (params) {
            if (!params.data.Add)
                params.data.Edit = true;
            if (params.colDef.field == "Rate")
                setCostAllocationFooter();
        },
        onGridSizeChanged: () => {
            $scope.grdOptCostAlloc.api.sizeColumnsToFit();
        },
        overlayLoadingTemplate: '<span class="ag-overlay-loading-center">Please wait while we are fetching requested records from database</span>',
        overlayNoRowsTemplate: '<span style="padding: 10px; border: 2px solid #444; background: lightgoldenrodyellow; color:red;">No rows available to display here</span>',
        components: {
            agilitySelectEditor: getAgilitySelect()
        }
    };

    function getCostAllocationDataFromGrid() {
        let rowData = [];
        $scope.grdOptCostAlloc.api.forEachNode(node => rowData.push(node.data));
        return rowData;
    }

    $scope.onAddCostAllocation = function () {
        try {
            var res = $scope.grdOptCostAlloc.api.updateRowData({ add: [{ TemplateId: null, BranchCode: null, Rate: 0, Add: true, Edit: false, Delete: false }] });
            $scope.grdOptCostAlloc.api.startEditingCell({ rowIndex: res.add[0].rowIndex, colKey: "TemplateId" });
        }
        catch (exception) { showErrorToast('Add Cost Allocation: ' + exception); }
    };

    $scope.onDeleteCostAllocation = function () {
        try {
            if ($scope.grdOptCostAlloc.api.getSelectedRows().length > 0) {
                $scope.grdOptCostAlloc.api.getSelectedRows().forEach(x => x.Delete = true);
                $scope.grdOptCostAlloc.api.getFilterInstance('Delete').onFilterChanged();
                setCostAllocationFooter();
            }
        }
        catch (exception) { showErrorToast('Delete Cost Allocation: ' + exception); }
    };

    function setCostAllocationFooter() {
        try {
            let _total = 0;
            $scope.grdOptCostAlloc.api.forEachNode(n => _total +=
                n.data.TemplateId != undefined && n.data.TemplateId != -1 && !n.data.Delete ?
                    n.data.Rate : 0);
            var _footer = [{ TemplateId: -1, BranchCode: null, Rate: _total }];
            $scope.grdOptCostAlloc.api.setPinnedBottomRowData(_footer);
        }
        catch (exception) { showErrorToast('Cost Allocation Footer Method:' + exception); }
    }
    //#endregion

    //#region Family Grid
    var colFamily = [
        {
            headerName: "Relation", field: "RelationId", cellEditor: "agilitySelectEditor",
            cellEditorParams: { source: 'Relation', class: "agGridSelect tb130" },
            valueFormatter: getRelationName, width: 130
        },
        {
            headerName: "Member Name", field: "MemberName", width: 250
        },
        {
            headerName: 'Medical Covered?', field: 'MedicalCovered', width: 145,
            cellRenderer: params => {
                if (params.value) {
                    return "<input type='checkbox' checked />";
                }
                else {
                    return "<input type='checkbox'/>";
                }
            },
            cellEditor: "agilityCheckBoxEditor"
        },
        {
            headerName: "Birth Date", field: "DoB", width: 125,
            cellEditor: 'datePicker',
            cellEditorParams: { minDate: '-90Y', maxDate: '+0d' }
        },
        {
            headerName: 'Emergency Ctc?', field: 'EmergencyContact', width: 145,
            cellRenderer: params => {
                if (params.value) {
                    return "<input type='checkbox' checked />";
                }
                else {
                    return "<input type='checkbox'/>";
                }
            },
            cellEditor: "agilityCheckBoxEditor"
        },
        {
            headerName: "Contact #", field: "ContactNo", cellEditor: "agTextCellEditor",
            width: 150
        },
        { headerName: "Add", field: "Add", hide: true, suppressToolPanel: true },
        { headerName: "Edit", field: "Edit", hide: true, suppressToolPanel: true },
        { headerName: "Delete", field: "Delete", hide: true, suppressToolPanel: true }
    ];

    $scope.grdOptFamily = {
        headerHeight: 25,
        rowHeight: 32,
        animateRows: true,
        defaultColDef: {
            editable: allowEdit.bind(this)
        },
        columnDefs: colFamily,
        rowData: [],
        rowSelection: 'single',
        rowDeselection: true,
        enableColResize: true,
        enableSorting: true,
        singleClickEdit: true,
        //stopEditingWhenGridLosesFocus: true,
        onCellValueChanged: function (params) {
            if (!params.data.Add)
                params.data.Edit = true;
        },
        overlayLoadingTemplate: '<span class="ag-overlay-loading-center">Please wait while we are fetching requested records from database</span>',
        overlayNoRowsTemplate: '<span style="padding: 10px; border: 2px solid #444; background: lightgoldenrodyellow; color:red;">No rows available to display here</span>',
        components: {
            agilitySelectEditor: getAgilitySelect(),
            datePicker: getDatePicker(),
            agilityCheckBoxEditor: getCellCheckBox()
        }
    };

    function getFamilyDataFromGrid() {
        let rowData = [];
        $scope.grdOptFamily.api.forEachNode(node => rowData.push(node.data));
        return rowData;
    }

    $scope.onAddFamily = function () {
        try {
            var res = $scope.grdOptFamily.api.updateRowData({
                add: [{
                    RelationId: null, MemberName: null, MedicalCovered: true, DoB: "",
                    EmergencyContact: false, ContactNo: null, Add: true, Edit: false, Delete: false
                }]
            });
            $scope.grdOptFamily.api.startEditingCell({ rowIndex: res.add[0].rowIndex, colKey: "RelationId" });
        }
        catch (exception) { showErrorToast('Add Family Member: ' + exception); }
    };

    $scope.onDeleteFamily = function () {
        try {
            if ($scope.grdOptFamily.api.getSelectedRows().length > 0) {
                if (confirm("Are you sure you want to Delete selected row?")) {
                    $scope.grdOptFamily.api.getSelectedRows().forEach(x => x.Delete = true);
                    $scope.grdOptFamily.api.getFilterInstance('Delete').onFilterChanged();
                }
            }
            else
                showErrorToast("No Row selected to apply Delete operation. Please select row and then hit Delete button!");
        }
        catch (exception) { showErrorToast('Delete Family Member: ' + exception); }
    };
    //#endregion

    //#region Entitlement Grid
    var colEntitlement = [
        {
            headerName: "Entitlement Type", field: "EntitlementTypeId", cellEditor: "agilitySelectEditor",
            cellEditorParams: { source: 'EntitlementType', class: "agGridSelect tb130" },
            valueFormatter: getEntitlementTypeName, width: 130
        },
        {
            headerName: "Reg/Ser #", field: "RegistrationNo", width: 120
        },
        {
            headerName: "Model", field: "Model", width: 100
        },
        {
            headerName: "Base Value", field: "BaseValue", type: "numericColumn",
            valueFormatter: formatNumbers, valueParser: numberValueParser, width: 100
        },
        {
            headerName: "Limit", field: "Limit", type: "numericColumn",
            valueFormatter: formatNumbers, valueParser: numberValueParser, width: 100
        },
        {
            headerName: "Issue Date", field: "IssueDate", width: 105,
            cellEditor: 'datePicker',
            cellEditorParams: { minDate: '-5Y', maxDate: '+0d' }
        },
        {
            headerName: "Expiry Date", field: "ExpiryDate", width: 105,
            cellEditor: 'datePicker',
            cellEditorParams: { minDate: '-1Y', maxDate: '+10Y' }
        },
        {
            headerName: "Reminder Date", field: "ReminderDate", width: 105,
            cellEditor: 'datePicker',
            cellEditorParams: { minDate: '-0d', maxDate: '+10Y' }
        },
        {
            headerName: "Disposal Date", field: "DisposalDate", width: 105,
            cellEditor: 'datePicker',
            cellEditorParams: { minDate: '-5Y', maxDate: '+0d' }
        },
        //{
        //    headerName: 'Consumable?', field: 'Consumable', width: 105,
        //    cellRenderer: params => {
        //        return `<input type='checkbox' ${params.value ? 'checked' : ''} />`;
        //    },
        //    cellEditor: "agilityCheckBoxEditor"
        //},
        {
            headerName: 'Active?', field: 'Active', width: 85,
            cellRenderer: params => {
                if (params.value) {
                    return "<input type='checkbox' checked />";
                }
                else {
                    return "<input type='checkbox'/>";
                }
            },
            cellEditor: "agilityCheckBoxEditor"
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
        //onCellClicked: function (event) {
        //    var RowData = $scope.grdOptEntitlement.api.getSelectedRows();
        //    if (event.colDef.field == "Consumable") {
        //        if (!event.data.Consumable) {
        //            rowNode = $scope.grdOptEntitlement.api.getRowNode(RowData[0].EntitlementTypeId);
        //            rowNode.setDataValue('Consumable', true);
        //        }
        //        else {
        //            rowNode = $scope.grdOptEntitlement.api.getRowNode(RowData[0].EntitlementTypeId);
        //            rowNode.setDataValue('Consumable', false);
        //        }
        //    }
        //},           
        onCellValueChanged: function (params) {
            if (!params.data.Add)
                params.data.Edit = true;
        },
        overlayLoadingTemplate: '<span class="ag-overlay-loading-center">Please wait while we are fetching requested records from database</span>',
        overlayNoRowsTemplate: '<span style="padding: 10px; border: 2px solid #444; background: lightgoldenrodyellow; color:red;">No rows available to display here</span>',
        components: {
            agilitySelectEditor: getAgilitySelect(),
            datePicker: getDatePicker(),
            agilityCheckBoxEditor: getCellCheckBox()
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
                    EntitlementTypeId: null, RegistrationNo: null, Model: null, BaseValue: 0, Limit: 0,
                    IssueDate: "", ExpiryDate: "", ReminderDate: "", DisposalDate: "",
                    //Consumable: false, 
                    Active: true, Add: true, Edit: false, Delete: false
                }]
            });
            $scope.grdOptEntitlement.api.startEditingCell({ rowIndex: res.add[0].rowIndex, colKey: "EntitlementTypeId" });
        }
        catch (exception) { showErrorToast('Add Employee Entitlement: ' + exception); }
    };

    $scope.onDeleteEntitlement = function () {
        try {
            if ($scope.grdOptEntitlement.api.getSelectedRows().length > 0) {
                if (confirm("Are you sure you want to Delete selected row?")) {
                    $scope.grdOptEntitlement.api.getSelectedRows().forEach(x => x.Delete = true);
                    $scope.grdOptEntitlement.api.getFilterInstance('Delete').onFilterChanged();
                }
            }
            else
                showErrorToast("No Row selected to apply Delete operation. Please select row and then hit Delete button!");
        }
        catch (exception) { showErrorToast('Delete Employee Entitlement: ' + exception); }
    };
    //#endregion

    //#region general functions
    function setGridData() {
        try {
            if ($scope.Employee != []) {
                $scope.grdOptSalary.api.setRowData($scope.Employee.Salary);
                $scope.grdOptCostAlloc.api.setRowData($scope.Employee.CostAllocations);
                $scope.grdOptFamily.api.setRowData($scope.Employee.Family);
                $scope.grdOptEntitlement.api.setRowData($scope.Employee.Entitlements);
                setGridDeleteFilter($scope.grdOptSalary.api);
                setGridDeleteFilter($scope.grdOptCostAlloc.api);
                setGridDeleteFilter($scope.grdOptFamily.api);
                setGridDeleteFilter($scope.grdOptEntitlement.api);
            }
            else {
                $scope.grdOptSalary.api.setRowData([]);
                $scope.grdOptCostAlloc.api.setRowData([]);
                $scope.grdOptFamily.api.setRowData([]);
                $scope.grdOptEntitlement.api.setRowData([]);
            }
            setSalaryFooter();
            setCostAllocationFooter();
        }
        catch (exception) { showErrorToast(exception); }
    }
    //#endregion

    //#region date functions
    function setDateParameters() {
        $('#dpJoining').datepicker({
            startDate: '-90Y',
            endDate: '+30d'
        }).on('changeDate', function (e) {
            $scope.Employee.JoiningDate = e.format();
            $scope.$apply();
        });

        $('#dpConfirmationDue').datepicker({
            startDate: '-90Y',
            endDate: '+180d',
            defaultViewDate: new Date().setDate(new Date().getDate() + 180)
        }).on('changeDate', function (e) {
            $scope.Employee.ConfirmationDueDate = e.format();
            $scope.$apply();
        });

        $('#dpConfirmation').datepicker({
            startDate: '-60Y'
        }).on('changeDate', function (e) {
            $scope.Employee.ConfirmationDate = e.format();
            $scope.$apply();
        });

        $('#dpSeparation').datepicker({
            startDate: '-30Y',
            endDate: '+30d'
        }).on('changeDate', function (e) {
            $scope.Employee.SeparationDate = e.format();//dt;
            $scope.$apply();
        });

        $('#dpBirth').datepicker({
            startDate: '-70y',
            enddate: '-18y',
            defaultViewDate: new Date(new Date().getFullYear() - 18, new Date().getMonth(), new Date().getDate())
        }).on('changeDate', function (e) {
            $scope.Employee.BirthDate = e.format();
            $scope.$apply();
        });

        $('#dpWorkingSince').datepicker({
            startDate: '-60y'
        }).on('changeDate', function (e) {
            $scope.Employee.EmpPersonalInfo.WorkingSince = e.format();//dt;
            $scope.$apply();
        });
    }

    function maskDates() {
        $scope.Employee.JoiningDate = formatDate($scope.Employee.JoiningDate);
        $scope.Employee.ConfirmationDueDate = formatDate($scope.Employee.ConfirmationDueDate);
        $scope.Employee.EmpPersonalInfo.BirthDate = formatDate($scope.Employee.EmpPersonalInfo.BirthDate);
        $scope.Employee.EmpPersonalInfo.WorkingSince = formatDate($scope.Employee.EmpPersonalInfo.WorkingSince);

        if ($scope.Employee.ConfirmationDate != null) {
            $scope.Employee.ConfirmationDate = formatDate($scope.Employee.ConfirmationDate);
        }

        if ($scope.Employee.SeparationDate != null) {
            $scope.Employee.SeparationDate = formatDate($scope.Employee.SeparationDate);
        }

        if ($scope.Employee.Family != null) {
            for (i = 0; i < $scope.Employee.Family.length; i++) {
                $scope.Employee.Family[i]["DoB"] = formatDate($scope.Employee.Family[i]["DoB"]);
            }
        }

        if ($scope.Employee.Entitlements != null) {
            for (i = 0; i < $scope.Employee.Entitlements.length; i++) {
                $scope.Employee.Entitlements[i]["IssueDate"] = formatDate($scope.Employee.Entitlements[i]["IssueDate"]);
                $scope.Employee.Entitlements[i]["ExpiryDate"] = formatDate($scope.Employee.Entitlements[i]["ExpiryDate"]);
                $scope.Employee.Entitlements[i]["ReminderDate"] = formatDate($scope.Employee.Entitlements[i]["ReminderDate"]);
                $scope.Employee.Entitlements[i]["DisposalDate"] = formatDate($scope.Employee.Entitlements[i]["DisposalDate"]);
            }
        }
        if ($scope.Employee.Attachments != null) {
            for (i = 0; i < $scope.Employee.Attachments.length; i++) {
                if ($scope.Employee.Attachments[i]["ExpiryDate"] != null)
                    $scope.Employee.Attachments[i]["ExpiryDate"] = formatDate($scope.Employee.Attachments[i]["ExpiryDate"]);
            }
        }

        if ($scope.Employee.AuditTrail != null) {
            if ($scope.Employee.AuditTrail.CreatedOn != null) {
                $scope.Employee.AuditTrail.CreatedOn = formatDateTime($scope.Employee.AuditTrail.CreatedOn);
            }
            if ($scope.Employee.AuditTrail.UpdatedOn != null) {
                $scope.Employee.AuditTrail.UpdatedOn = formatDateTime($scope.Employee.AuditTrail.UpdatedOn);
            }
        }
    }
    //#endregion
    //#endregion
});