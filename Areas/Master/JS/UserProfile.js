baseURL = "/HRIS/Master/UserProfile/";
agGrid.initialiseAgGridWithAngular1(angular);
var app = angular.module('HRIS', ['ui.bootstrap', 'agGrid', 'ngMaterial']);
let session = new Object();

$(document).ready(function () {
    $("#btnEdit").click(function () {
        session.FormMode = "Edit";
        setFormSession(session);
        document.getElementById("chkActive").disabled = true;
        $('#tblOption').focus();
    });
});

app.controller('UserProfileController', function ($scope, $rootScope, $http, $uibModal, $mdToast) {
    //#region form loading events
    $scope.formLoaded = function () {
        $http({
            method: "GET",
            url: "/HRIS/Home/GetAccessRight",
            dataType: "JSON",
            params: { _optionName: "User Management" }
        }).then(result => {
            session = result.data;
            setFormSession(session);
        },
        errorMsg => {
            showErrorToast(errorMsg);
        });
    };
    //#endregion

    //#region toolbar actions
    $scope.tbRecall = function () {
        session.FormMode = "Recall";
        setFormSession(session);
        $('#txtUserId').focus();
    };

    $scope.tbSearch = function () {
        $scope.formTitle = "Search & Select User Profile";
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
                $scope.getUserProfileById($rootScope.SelectedRow.UserId);
            }, function () {

            });
        }
        catch (exception) { showErrorToast(exception); }
    };

    $scope.tbSave = function (res) {
        $scope.UserProfile.Options = getUseroptionDataFromGrid();
        $scope.UserProfile.Branches = getUserBranchDataFromGrid();
        $scope.UserProfile.Companies = getUserCompanyDataFromGrid();
        $scope.UserProfile.Grades = getUserGradeDataFromGrid();
        $scope.UserProfile.Roles = getUserRoleDataFromGrid();
        $http({
            method: "POST",
            url: baseURL + "Save",
            data: $scope.UserProfile
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
    //#endregion

    //#region form functions
    $scope.getUserProfileById = function (res) {
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
                $scope.UserProfile = result.data.UserProfile;
                $scope.footer = $scope.UserProfile.AuditTrail;
                session.FormMode = result.data.formMode;
                setFormSession(session);
                setGridData();
                maskDates();
            }
        }, errorMsg => {
            showErrorToast(result);
        });
    };
    //#endregion

    //#region local function
    function clearForm() {
        $scope.UserProfile = {};
        $scope.footer = {};
        $scope.errors = [];
    }
    //#endregion

    //#region date functions
    function maskDates() {
        if ($scope.UserProfile.AuditTrail != null) {
            $scope.UserProfile.AuditTrail.CreatedOn = formatDateTime($scope.UserProfile.AuditTrail.CreatedOn);
            if ($scope.UserProfile.AuditTrail.UpdatedOn != null) {
                $scope.UserProfile.AuditTrail.UpdatedOn = formatDateTime($scope.UserProfile.AuditTrail.UpdatedOn);
            }
        }
    }
    //#endregion

    //#region grid setup
    //#region User Option Grid Definition & functions
    var colUserOption = [
        {
             headerName: 'Access', field: 'AllowAccess', width: 90,
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
            headerName: "Option Name", field: "OptionName", editable: false, width: 240
        },
        {
            headerName: 'Add', field: 'AllowAdd', width: 90,
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
            headerName: 'Edit', field: 'AllowEdit', width: 90,
            cellRenderer: params => {
                if (params.value) {
                    return "<input type='checkbox' checked />";
                }
                else {
                    return "<input type='checkbox'/>";
                }
            },
            cellEditor: "agilityCheckBoxEditor"
        }
    ];


    $scope.grdOptOption = {
        headerHeight: 25,
        rowHeight: 32,
        animateRows: true,
        defaultColDef: {
            editable: allowEdit.bind(this)
        },
        columnDefs: colUserOption,
        rowData: [],
        rowSelection: 'multiple',
        enableSorting: true,
        getRowNodeId: function (data) { return data.OptionId; },
        onCellClicked: function (event) {
            var RowData = $scope.grdOptOption.api.getSelectedRows(); 
            if (event.colDef.field == "AllowAccess") {
                if (!event.data.AllowAccess) {
                    rowNode = $scope.grdOptOption.api.getRowNode(RowData[0].OptionId);
                    rowNode.setDataValue('AllowAccess', true);
                }
                else {
                    rowNode = $scope.grdOptOption.api.getRowNode(RowData[0].OptionId);
                    rowNode.setDataValue('AllowAccess', false);
                }
            }           
            if (event.colDef.field == "AllowAdd") {
                if (!event.data.AllowAdd) {
                    rowNode = $scope.grdOptOption.api.getRowNode(RowData[0].OptionId);
                    rowNode.setDataValue('AllowAdd', true);
                }
                else {
                    rowNode = $scope.grdOptOption.api.getRowNode(RowData[0].OptionId);
                    rowNode.setDataValue('AllowAdd', false);
                }
            }           
            if (event.colDef.field == "AllowEdit") {
                if (!event.data.AllowEdit) {
                    rowNode = $scope.grdOptOption.api.getRowNode(RowData[0].OptionId);
                    rowNode.setDataValue('AllowEdit', true);
                }
                else {
                    rowNode = $scope.grdOptOption.api.getRowNode(RowData[0].OptionId);
                    rowNode.setDataValue('AllowEdit', false);
                }
            }           
        },
        onCellValueChanged: function (params) {
            if (!params.data.Add)
                params.data.Edit = true;
        },
        overlayLoadingTemplate: '<span class="ag-overlay-loading-center">Please wait while we are fetching requested records from database</span>',
        overlayNoRowsTemplate: '<span style="padding: 10px; border: 2px solid #444; background: lightgoldenrodyellow; color:red;">No rows available to display here</span>',
        components: {
            agilityCheckBoxEditor: getCellCheckBox()
        }
    };

    function getUseroptionDataFromGrid() {
        let rowData = [];
        $scope.grdOptOption.api.forEachNode(node => rowData.push(node.data));
        return rowData;
    }

    //#endregion       

    //#region User Branch Grid Definition & functions
    var colUserBranch = [
        {           
            headerName: 'S', field: 'Selected', width: 70, editable: false,
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
            headerName: "Branch Name", field: "BranchName", editable: false, width: 300
        }
    ];
    
    $scope.grdOptBranch = {
        headerHeight: 25,
        rowHeight: 32,
        animateRows: true,
        defaultColDef: {
            editable: allowEdit.bind(this)
        },
        columnDefs: colUserBranch,
        rowData: [],
        rowSelection: 'multiple',
        enableSorting: true,
        getRowNodeId: function (data) { return data.BranchCode; },
        onCellClicked: function (event) {
            if (event.colDef.field == "Selected") {
                var RowData = $scope.grdOptBranch.api.getSelectedRows(); 
                if (!event.data.Selected) {
                    rowNode = $scope.grdOptBranch.api.getRowNode(RowData[0].BranchCode);
                    rowNode.setDataValue('Selected', true);

                }
                else {
                    rowNode = $scope.grdOptBranch.api.getRowNode(RowData[0].BranchCode);
                    rowNode.setDataValue('Selected', false);
                }
            }
        },

        overlayLoadingTemplate: '<span class="ag-overlay-loading-center">Please wait while we are fetching requested records from database</span>',
        overlayNoRowsTemplate: '<span style="padding: 10px; border: 2px solid #444; background: lightgoldenrodyellow; color:red;">No rows available to display here</span>',
        components: {
            agilityCheckBoxEditor: getCellCheckBox()
        }
        
    };

    function getUserBranchDataFromGrid() {
        let rowData = [];
        $scope.grdOptBranch.api.forEachNode(node => rowData.push(node.data));
        return rowData;
    }

    //#endregion       

    //#region User Company Grid Definition & functions
    var colUserCompany = [
        {
            headerName: 'S', field: 'Selected', width: 70, editable: false,
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
            headerName: "Company Name", field: "CompanyName", editable: false, width: 300
        }
    ];

    $scope.grdOptCompany = {
        headerHeight: 25,
        rowHeight: 32,
        animateRows: true,
        defaultColDef: {
            editable: allowEdit.bind(this)
        },
        columnDefs: colUserCompany,
        rowData: [],
        rowSelection: 'multiple', 
        enableSorting: true,
        getRowNodeId: function (data) { return data.CompanyId; },
        onCellClicked: function (event) {
            var RowData = $scope.grdOptCompany.api.getSelectedRows(); 
            if (event.colDef.field == "Selected") {
                if (!event.data.Selected) {
                    rowNode = $scope.grdOptCompany.api.getRowNode(RowData[0].CompanyId);
                    rowNode.setDataValue('Selected', true);
                }
                else {
                    rowNode = $scope.grdOptCompany.api.getRowNode(RowData[0].CompanyId);
                    rowNode.setDataValue('Selected', false);
                }
            }
        },
       

        overlayLoadingTemplate: '<span class="ag-overlay-loading-center">Please wait while we are fetching requested records from database</span>',
        overlayNoRowsTemplate: '<span style="padding: 10px; border: 2px solid #444; background: lightgoldenrodyellow; color:red;">No rows available to display here</span>',
        components: {
            agilityCheckBoxEditor: getCellCheckBox()
        }
    };

    function getUserCompanyDataFromGrid() {
        let rowData = [];
        $scope.grdOptCompany.api.forEachNode(node => rowData.push(node.data));
        return rowData;
    }

    //#endregion       

    //#region User Role Grid Definition & functions
    var colUserRole = [
        {
            headerName: 'S', field: 'Selected', width: 70, editable: false,
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
            headerName: "Role Name", field: "RoleName", editable: false, width: 300
        }
    ];

    $scope.grdOptRole = {
        headerHeight: 25,
        rowHeight: 32,
        animateRows: true,
        defaultColDef: {
            editable: allowEdit.bind(this)
        },
        columnDefs: colUserRole,
        rowData: [],
        rowSelection: 'multiple',
        enableSorting: true,
        getRowNodeId: function (data) { return data.RoleId; },
        onCellClicked: function (event) {
            var RowData = $scope.grdOptRole.api.getSelectedRows(); 
            if (event.colDef.field == "Selected") {
                if (!event.data.Selected) {
                    rowNode = $scope.grdOptRole.api.getRowNode(RowData[0].RoleId);
                    rowNode.setDataValue('Selected', true);
                }
                else {
                    rowNode = $scope.grdOptRole.api.getRowNode(RowData[0].RoleId);
                    rowNode.setDataValue('Selected', false);
                }
            }
           
        },
        //onCellValueChanged: function (params) {
        //    if (!params.data.Add)
        //        params.data.Edit = true;
        //},
        overlayLoadingTemplate: '<span class="ag-overlay-loading-center">Please wait while we are fetching requested records from database</span>',
        overlayNoRowsTemplate: '<span style="padding: 10px; border: 2px solid #444; background: lightgoldenrodyellow; color:red;">No rows available to display here</span>',
        components: {
            agilityCheckBoxEditor: getCellCheckBox()
        }
    };

    function getUserRoleDataFromGrid() {
        let rowData = [];
        $scope.grdOptRole.api.forEachNode(node => rowData.push(node.data));
        return rowData;
    }

    //#endregion       

    //#region User Grade Grid Definition & functions
    var colUserGrade = [
        {
            headerName: 'S', field: 'Selected', width: 70, editable: false,
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
            headerName: "Grade Name", field: "GradeName", editable: false, width: 300
        }
    ];

    $scope.grdOptGrade = {
        headerHeight: 25,
        rowHeight: 32,
        animateRows: true,
        defaultColDef: {
            editable: allowEdit.bind(this)
        },
        columnDefs: colUserGrade,
        rowData: [],
        rowSelection: 'multiple',
        enableSorting: true,
        getRowNodeId: function (data) { return data.GradeId; },
        onCellClicked: function (event) {
            var RowData = $scope.grdOptGrade.api.getSelectedRows();
            if (event.colDef.field == "Selected") {
                if (!event.data.Selected) {
                    rowNode = $scope.grdOptGrade.api.getRowNode(RowData[0].GradeId);
                    rowNode.setDataValue('Selected', true);
                }
                else {
                    rowNode = $scope.grdOptGrade.api.getRowNode(RowData[0].GradeId);
                    rowNode.setDataValue('Selected', false);
                }
            }

        },
        //onCellValueChanged: function (params) {
        //    if (!params.data.Add)
        //        params.data.Edit = true;
        //},
        overlayLoadingTemplate: '<span class="ag-overlay-loading-center">Please wait while we are fetching requested records from database</span>',
        overlayNoRowsTemplate: '<span style="padding: 10px; border: 2px solid #444; background: lightgoldenrodyellow; color:red;">No rows available to display here</span>',
        components: {
            agilityCheckBoxEditor: getCellCheckBox()
        }
    };

    function getUserGradeDataFromGrid() {
        let rowData = [];
        $scope.grdOptGrade.api.forEachNode(node => rowData.push(node.data));
        return rowData;
    }

    //#endregion       
    
    //#region general functions
    function setGridData() {
        if ($scope.UserProfile != []) {
            $scope.grdOptOption.api.setRowData($scope.UserProfile.Options);
            $scope.grdOptBranch.api.setRowData($scope.UserProfile.Branches);
            $scope.grdOptCompany.api.setRowData($scope.UserProfile.Companies);
            $scope.grdOptGrade.api.setRowData($scope.UserProfile.Grades);
            $scope.grdOptRole.api.setRowData($scope.UserProfile.Roles);
        }
        else {
            $scope.grdOptOption.api.setRowData([]);
            $scope.grdOptBranch.api.setRowData([]);
            $scope.grdCompany.api.setRowData([]);
            $scope.grdOptGrade.api.setRowData([]);
            $scope.grdOptRole.api.setRowData([]);
        }
    }

    $scope.sum = function (items, prop) {
        return items.reduce(function (a, b) {
            return a + b[prop];
        }, 0);
    };
    //#endregion
    //#endregion
});