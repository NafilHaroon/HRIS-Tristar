baseURL = "/HRIS/Process/EmailSetup/";
agGrid.initialiseAgGridWithAngular1(angular);
var app = angular.module('HRIS', ["ui.bootstrap", "agGrid"]);
let session = "";

$(document).ready(function () {
    $("#btnEdit").click(function () {
        session.FormMode = "Edit";
        setFormSession(session);
        document.getElementById('ddBranch').disabled = true;
        document.getElementById('ddCompany').disabled = true;
       // $('#txtHeadName').focus();
    });
});

app.controller('EmailSetupController', function ($scope, $rootScope, $http, $uibModal) {
    //#region form loading events
    $scope.formLoaded = function () {
        $http({
            method: "GET",
            url: "/HRIS/Home/GetAccessRight",
            dataType: "JSON",
            params: { _optionName: "Cash Salary Email Setup" }
        }).then(result => {
            session = result.data;
            setFormSession(session);
        },
            errorMsg => {
                showErrorToast(errorMsg);
            });
        loadDropDown();
        getEmailSetupDetail();
    };

    function loadDropDown() {
        $http.get(baseURL + 'GetLookups').then(result => {
            $scope.lstBranch = result.data.lstBranch;
            $scope.lstCompany = result.data.lstCompany;
        }, errorMsg => {
            showErrorToast(errorMsg);
        });
    }
    //#endregion

    //#region toolbar actions
    $scope.tbAdd = function () {
        $http({
            method: "Get",
            url: baseURL + "Add"
        }).then(response => {
            $scope.EmailSetup = response.data.EmailSetup;
            session.FormMode = response.data.formMode;
            setFormSession(session);
        }, errorMsg => {
            showErrorToast(errorMsg);
        });
    }; 

    $scope.tbSave = function () {
        $scope.EmailSetup.Details = getEmailSetupDataFromGrid();
        $http({
            method: "POST",
            url: baseURL + "Save",
            data: $scope.EmailSetup
        }).then(result => {
            $scope.errors = [];
            if (!result.data.success) {
                $scope.errors = [result.data.message];
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
    //#endregion

    //#region form functions
    function getEmailSetupDetail() {
        $http({
            type: "Get",
            url: baseURL + "Get",
            dataType: "JSON"
        }).then(result => {
            clearForm();
            if (!result.data.success) {
                $scope.errors = [result.data.message];
                showErrorToast();
            }
            else {
                showSuccessToast();
                session.FormMode = result.data.formMode;
                setFormSession(session);
                clearForm();
                setGridData();
            }
        }, errorMsg => {
            showErrorToast(result);
        });
    }
    //#endregion

    //#region local function
    function clearForm() {
        $scope.EmailSetup = {};
        $scope.errors = [];
    }
    //#endregion

    //#region grid setup
    //#region Work Type Grid Definition & functions
    var colEmailSetup = [
        {
            headerName: "Branch", field: "BranchCode", cellEditor: "agilitySelectEditor",
            cellEditorParams: { source: 'Branch', class: "agGridSelect tb130" },
            valueFormatter: getBranchName, width: 130
        },
        {
            headerName: "Company", field: "CompanyId", cellEditor: "agilitySelectEditor",
            cellEditorParams: { source: 'Company', class: "agGridSelect tb130" },
            valueFormatter: getCompanyName, width: 130
        },
        {
            headerName: "ContactName", field: "ContactName", type: "TextColumn",
            width: 200
        },
        {
            headerName: "EmailTo", field: "EmailTo", type: "TextColumn",
            width: 200
        },
        {
            headerName: "EmailCC", field: "EmailCC", type: "TextColumn",
            width: 200
        },
        {
            headerName: "EmailBCC", field: "EmailBCC", type: "TextColumn",
            width: 200
        },
        { headerName: "Add", field: "Add", hide: true, suppressToolPanel: true },
        { headerName: "Edit", field: "Edit", hide: true, suppressToolPanel: true },
        { headerName: "Delete", field: "Delete", hide: true, suppressToolPanel: true },
    ];
        

    $scope.grdOptEmailSetup = {
        headerHeight: 25,
        rowHeight: 32,
        animateRows: true,
        defaultColDef: {
            editable: allowEdit.bind(this)
        },
        columnDefs: colEmailSetup,
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
                params.data.Edit = true;           
        },
        overlayLoadingTemplate: '<span class="ag-overlay-loading-center">Please wait while we are fetching requested records from database</span>',
        overlayNoRowsTemplate: '<span style="padding: 10px; border: 2px solid #444; background: lightgoldenrodyellow; color:red;">No rows available to display here</span>',
        components: {
            agilitySelectEditor: getAgilitySelect()
        }
    };

    function getEmailSetupDataFromGrid() {
        let rowData = [];
        $scope.grdOptEmailSetup.api.forEachNode(node => rowData.push(node.data));
        return rowData;
    }

    $scope.onAddEmailSetup = function () {
        try {
            var res = $scope.grdOptEmailSetup.api.updateRowData({
                add: [{
                    BranchCode: null, CompanyId: -1, ContactName: null, EmailTo: null, EmailCC: null, EmailBCC: null,
                    Add: true, Edit: false, Delete: false
                }]
            });
            $scope.grdOptEmailSetup.api.startEditingCell({ rowIndex: res.add[0].rowIndex, colKey: "BranchCode" });
        }
        catch (exception) { showErrorToast('Add Email Setup: ' + exception); }
    };

    $scope.onDeleteEmailSetup = function () {
        try {
            if ($scope.grdOptEmailSetup.api.getSelectedRows().length > 0) {
                if (confirm("Are you sure you want to Delete selected row?")) {
                    $scope.grdOptEmailSetup.api.getSelectedRows().forEach(x => x.Delete = true);
                    $scope.grdOptEmailSetup.api.getFilterInstance('Delete').onFilterChanged();                    
                }
            }
            else
                showErrorToast("No Row selected to apply Delete operation. Please select row and then hit Delete button!");
        }
        catch (exception) { showErrorToast('Delete Email Setup: ' + exception); }
    };
    //#endregion       

    //#region general functions
    function setGridData() {
        if ($scope.EmailSetup != []) {
            $scope.grdOptEmailSetup.api.setRowData($scope.EmailSetup.Details);
            setGridDeleteFilter($scope.grdOptEmailSetup.api);
        }
        else { $scope.grdOptEmailSetup.api.setRowData([]); }
    }
 
    //#endregion
    //#endregion
});
