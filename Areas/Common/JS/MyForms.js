baseURL = "/HRIS/Common/MyForms/";
agGrid.initialiseAgGridWithAngular1(angular);
var app = angular.module('HRIS', ['ui.bootstrap', 'agGrid', 'ngMaterial']);
let session = "";

app.controller('MyFormsController', function ($scope, $rootScope, $http, $uibModal, $mdToast) {
    //#region form loading events
    $scope.formLoaded = function () {
        $scope.getMyForms(0, -1);
        hideFormLoading();
    };
    //#endregion

    //#region toolbar actions
    $scope.tbExit = function () {
        $scope = null;
        window.location.href = '/HRIS/Common/Common/Index';
    };

    $scope.tbHistory = function (_workFlowId, _formId, _workFlowName) {
        $scope.formId = _formId;
        $scope.workFlowId = _workFlowId;
        $scope.formTitle = "History Of " + _workFlowName + " Request # " + _formId;
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
    //#endregion

    //#region form functions
    $scope.getMyForms = function (forms, workFlowId) {
        showFormLoading();
        $http({
            type: "Get",
            url: baseURL + "FormList",
            dataType: "JSON",
            params: { _forms: forms, _workflowId: workFlowId }
        }).then(result => {
            clearForm();
            if (!result.data.success) {
                showErrorToast(result.data.message);
            }
            else {
                $scope.MyForms = result.data.myForm;
                setGridData();
            }
            hideFormLoading();
        }, errorMsg => {
                showErrorToast(errorMsg.message);
                hideFormLoading();
        });
    };
    //#endregion

    //#region local function
    function clearForm() {
        $scope.MyForms = {};
        $scope.errors = [];
    }
    //#endregion

    //#region grid setup
    //#region form Grid Definition & functions
    var colForms = [
        {
            headerName: "Work Flow", field: "WorkFlowName", width: 140,
            cellStyle: {
                backgroundColor: 'lightgoldenrodyellow',
                color: 'darkgoldenrod',
                fontWeight: 'bold'
            },
        },
        {
            headerName: "Form #", field: "FormCode", width: 120,
            cellRenderer: function (params) {
                return '<a href="' + params.data.Controller + '?_formId=' + params.data.FormId + '" title="Click to open this form">' + params.value + '</a>'
            },
            cellStyle: {
                textDecoration: 'underline'
            }
        },
        { headerName: "State", field: "StateName", width: 180 },
        { headerName: "Sender", field: "Sender", width: 100 },
        { headerName: "Recipient", field: "Recipient", width: 100 },
        { headerName: "Sent On", field: "SentOn", width: 150 },
        {
            headerName: "Amount", field: "DocumentValue", type: "numericColumn", filter: "agNumberColumnFilter",
            valueFormatter: formatNumbers, valueParser: numberValueParser, width: 120
        },
        { headerName: "Description", field: "Description", width: 190 },
        { headerName: "Comments", field: "Comments", width: 180 },
        {
            headerName: "History", field: "FormId", width: 40, filter: false, 
            cellRenderer: function (params) {
                return '<button ng-click="tbHistory(' + params.data.WorkFlowId + ', ' + params.value + ', \'' + params.data.WorkFlowName + '\')" class="history-tiny" title="Click to view the history"></button>'
            }},
        { headerName: 'WFId', field: 'WorkFlowId', hide: true, suppressToolPanel: true },
        { headerName: 'C', field: 'Controller', hide: true, suppressToolPanel: true },
        { headerName: 'TK', field: 'TrackingKey', hide: true, suppressToolPanel: true }
    ];

    $scope.grdForms = {
        headerHeight: 25,
        rowHeight: 32,
        animateRows: true,
        defaultColDef: {
            editable: false
        },
        columnDefs: colForms,
        rowData: [],
        rowSelection: 'single',
        rowDeselection: true,
        floatingFilter: true,
        enableColResize: true,
        enableSorting: true,
        suppressCellSelection: true,
        angularCompileRows: true,
        getRowStyle: function (params) {
            if (params.node.rowPinned) {
                return { 'font-weight': 'bold', 'color': 'blue' };
            }
        },
        onCellClicked: function (event) {
            var RowData = $scope.grdForms.api.getSelectedNodes();
            if (event.colDef.field == "Close") {
                RowData[0].setDataValue('Close', !event.data.Close);
            }
        },
        overlayLoadingTemplate: '<span class="ag-overlay-loading-center">Please wait while we are fetching requested records from database</span>',
        overlayNoRowsTemplate: '<span style="padding: 10px; border: 2px solid #444; background: lightgoldenrodyellow; color:red;">No rows available to display here</span>'
    };
    //#endregion       

    //#region general functions
    function setGridData() {
        if ($scope.MyForms != []) {
            $scope.grdForms.api.setRowData($scope.MyForms.Detail);
        }
        else { $scope.grdForms.api.setRowData([]); }
    }
    //#endregion
    //#endregion
});