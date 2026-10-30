baseURL = "/HRIS/Master/Allocation/";
agGrid.initialiseAgGridWithAngular1(angular);
var app = angular.module('HRIS', ['ui.bootstrap', 'agGrid', 'ngMaterial']); 
let session = "";

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

$(document).ready(function () {
    $("#btnEdit").click(function () {
        session.FormMode = "Edit";
        setFormSession(session);
        $('#txtTemplateDesc').focus();
    });
});

app.controller('AllocationController', function ($scope, $rootScope, $http, $uibModal, $mdToast) {
    //#region form loading events
    $scope.formLoaded = function () {
        $http({
            method: "GET",
            url: "/HRIS/Home/GetAccessRight",
            dataType: "JSON",
            params: { _optionName: "Allocation Template" }
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
            $scope.lstDepartment = result.data.lstDepartment;
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
            $scope.Allocation = response.data.allocation;
            $scope.Footer = $scope.Allocation.AuditTrail;
            session.FormMode = response.data.formMode;
            setFormSession(session);
            maskDates();
            $('#txtTemplateDesc').focus();
        }, errorMsg => {
            showErrorToast(errorMsg);
        });
    };

    $scope.tbRecall = function () {
        session.FormMode = "Recall";
        setFormSession(session);
        $('#txtTemplateId').focus();
    };

    $scope.tbSearch = function () {
        $scope.formTitle = "Search & Select Cost Template";
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
                $scope.getTemplateById($rootScope.SelectedRow.TemplateId);
            }, function () {

            });
        }
        catch (exception) { showErrorToast(exception); }
    };

    $scope.tbSave = function () {
        $scope.Allocation.Details = getAllocationDataFromGrid();
        $http({
            method: "POST",
            url: baseURL + "Save",
            data: $scope.Allocation
        }).then(result => {
            $scope.errors = [];
            if (!result.data.success) {
                $scope.errors = getArray(result.data.message);
                showErrorToast();
            }
            else {
                showSuccessToast(result.data.message);
                session.FormMode = result.data.formMode;
                clearForm();
                setGridData();
                setFormSession(session);
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
    $scope.getTemplateById = function (res) {

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
                $scope.Allocation = result.data.allocation;
                $scope.footer = $scope.Allocation.AuditTrail;
                session.FormMode = result.data.formMode;
                setFormSession(session);
                setGridData();
                maskDates();
            }
        }, errorMsg => {
            showErrorToast(errorMsg);
        });
    };
    //#endregion

    //#region local function
    function clearForm() {
        $scope.Allocation = {};
        $scope.footer = {};
        $scope.errors = [];
    }
    //#endregion

    //#region date functions
    function maskDates() {
        if ($scope.Allocation.AuditTrail != null) {
            $scope.Allocation.AuditTrail.CreatedOn = formatDateTime($scope.Allocation.AuditTrail.CreatedOn);
            if ($scope.Allocation.AuditTrail.UpdatedOn != null) {
                $scope.Allocation.AuditTrail.UpdatedOn = formatDateTime($scope.Allocation.AuditTrail.UpdatedOn);
            }
        }
    }
    //#endregion

    //#region grid setup
    //#region Template Grid Definition & functions
    var colAllocation = [
        {
            headerName: "Department", field: "DepartmentCode", cellEditor: "agilitySelectEditor",
            cellEditorParams: { source: 'Department', class: "agGridSelect tb270" },
            valueFormatter: getDepartmentName, width: 270
        },
        {
            headerName: "Rate (%)", field: "Rate", type: "numericColumn",
            valueFormatter: formatNumbers, valueParser: numberValueParser, width: 100
        },
        { headerName: "Add", field: "Add", hide: true, suppressToolPanel: true },
        { headerName: "Edit", field: "Edit", hide: true, suppressToolPanel: true },
        { headerName: "Delete", field: "Delete", hide: true, suppressToolPanel: true }
    ];

    $scope.grdOptAllocation = {
        headerHeight: 25,
        rowHeight: 32,
        animateRows: true,
        defaultColDef: {
            editable: allowEdit.bind(this)
        },
        columnDefs: colAllocation,
        rowData: [],
        rowSelection: 'single',
        rowDeselection: true,
        enableColResize: true,
        enableSorting: true,
        singleClickEdit: true,
        onGridReady: (grid) => {
            setAllocationFooter();
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
                setAllocationFooter();
        },
        overlayLoadingTemplate: '<span class="ag-overlay-loading-center">Please wait while we are fetching requested records from database</span>',
        overlayNoRowsTemplate: '<span style="padding: 10px; border: 2px solid #444; background: lightgoldenrodyellow; color:red;">No rows available to display here</span>',
        components: {
            agilitySelectEditor: getAgilitySelect()
        }
    };

    function getAllocationDataFromGrid() {
        let rowData = [];
        $scope.grdOptAllocation.api.forEachNode(node => rowData.push(node.data));
        return rowData;
    }

    $scope.onAddAllocation = function () {
        try {
            var res = $scope.grdOptAllocation.api.updateRowData({
                add: [{
                    DepartmentCode: null, Rate: 0, Add: true, Edit: false, Delete: false
                }]
            });
            $scope.grdOptAllocation.api.startEditingCell({ rowIndex: res.add[0].rowIndex, colKey: "DepartmentCode" });
        }
        catch (exception) { showErrorToast('Add Allocation: ' + exception); }
    };

    $scope.onDeleteAllocation = function () {
        try {
            if ($scope.grdOptAllocation.api.getSelectedRows().length > 0) {
                $scope.grdOptAllocation.api.getSelectedRows().forEach(x => x.Delete = true);
                $scope.grdOptAllocation.api.getFilterInstance('Delete').onFilterChanged();
                setAllocationFooter();
            }
        }
        catch (exception) { showErrorToast('Delete Allocation: ' + exception); }
    };

    function setAllocationFooter() {
        try {
            let _rate = 0;
            $scope.grdOptAllocation.api.forEachNode(n => _rate +=
                n.data.DepartmentCode != undefined && n.data.DepartmentCode != null && !n.data.Delete ?
                    n.data.Rate : 0);
            var _footer = [{ DepartmentCode: null, Rate: _rate }];
            $scope.grdOptAllocation.api.setPinnedBottomRowData(_footer);
        }
        catch (exception) { showErrorToast('Allocation Footer Method:' + exception); }
    }
    //#endregion       

    //#region general functions
    function setGridData() {
        if ($scope.Allocation != []) {
            $scope.grdOptAllocation.api.setRowData($scope.Allocation.Details);
            setGridDeleteFilter($scope.grdOptAllocation.api);
        }
        else { $scope.grdOptAllocation.api.setRowData([]); }
        setAllocationFooter();
    }

    //$scope.sum = function (items, prop) {
    //    return items.reduce(function (a, b) {
    //        return a + b[prop];
    //    }, 0);
    //};
    //#endregion
     //#endregion


});