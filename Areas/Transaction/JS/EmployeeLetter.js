baseURL = "/HRIS/Transaction/EmployeeLetter/";
agGrid.initialiseAgGridWithAngular1(angular);
var app = angular.module('HRIS', ['ui.bootstrap', 'agGrid', 'ngMaterial', 'ui.tinymce']); 
let session = new Object();

$(document).ready(function () {
    $("#btnEdit").click(function () {
        session.FormMode = "Edit";
        setFormSession(session);
        $('#ddEmployee').prop('disabled', true);
        $('#ddAllEmployee').prop('disabled', true);
        $('#ddTemplate').prop('disabled', true);

        if (document.getElementById("btnSubmit") != null)
            document.getElementById("btnSubmit").disabled = true;
        if (document.getElementById("btnApprove") != null)
            document.getElementById("btnApprove").disabled = true;
        if (document.getElementById("btnReturn") != null)
            document.getElementById("btnReturn").disabled = true;
        if (document.getElementById("btnReject") != null)
            document.getElementById("btnReject").disabled = true;
        if (document.getElementById("btnSubmitCEO") != null)
            document.getElementById("btnSubmitCEO").disabled = true;
        if (document.getElementById('hdnCompleted').value == "true" ||
            document.getElementById('ftrCreatedBy').value != document.getElementById('hdnOwner').value) {
            document.getElementById("btnSave").disabled = true;
        }
        //$('#btnSave').prop('disabled', true);
    });
});

app.controller('LetterController', function ($scope, $rootScope, $http, $uibModal, $mdToast) {
    //#region form loading events
    $scope.formLoaded = function () {
        var _letterId = document.getElementById('vbLetterId').value;
        if (_letterId) {
            $scope.MyForm = true;
            $scope.ReviewMode = true;
            setSalaryReviewColumns();
            $scope.getLetterByNo(null, _letterId);
            //this is done to freeze form toolbar if data loading was unsucessful for any reason
            if (!document.getElementById('txtLetterNo').value) {
                setFormControls("Initialize", false, false, false);
            }
        }
        else {
            $scope.MyForm = false;
            $scope.ReviewMode = false;
            $http({
                method: "GET",
                url: "/HRIS/Home/GetAccessRight",
                dataType: "JSON",
                params: { _optionName: "Employee Letter" }
            }).then(result => {
                session = result.data;
                setFormSession(session);
            },
                errorMsg => {
                    showErrorToast(errorMsg);
                });
            session.FormMode = "Initialize";
            enableHistory(session.FormMode);
            loadDropDown();
        }
    };

    function loadDropDown() {
        try {
            $http.get(baseURL + 'GetLookups').then(result => {
                $scope.lstAllEmployee = result.data.lstAllEmployee;              
                $scope.lstLetterTemplate = result.data.lstLetterTemplate;   
                $scope.lstEmployee = $scope.lstAllEmployee.filter(obj => obj.StatusId == 1);
                $scope.lstSalaryHead = result.data.lstSalaryHead;
            }, errorMsg => {
                showErrorToast(errorMsg);
            });
        }
        catch (exception) { showErrorToast('loadDropDown: ' + exception.message); }
    }
    //#endregion

    //#region toolbar actions
    $scope.tbAdd = function () {
       try {
            $http({
                method: "Get",
                url: baseURL + "Add"
            }).then(response => {
                $scope.Letter = response.data.employeeLetter;
                $scope.footer = response.data.employeeLetter.AuditTrail;
                session.FormMode = response.data.formMode;
                setFormSession(session);
                maskDates();               
                $('#ddTemplate').focus();
            }, errorMsg => {
                showErrorToast(errorMsg);
            });
        }
        catch (exception) { showErrorToast(exception.message); }
    };

    $scope.tbRecall = function () {
        session.FormMode = "Recall";
        setFormSession(session);
        $('#txtLetterNo').focus();
    };

    $scope.tbSearch = function () {
        $scope.formTitle = "Search & Select Employee Letter";
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
                $scope.getLetterByNo($rootScope.SelectedRow.LetterNo);
            }, function () {
                
            });
        }
        catch (exception) { showErrorToast(exception); }
    };

    $scope.tbSave = function () {
        //if ($scope.Letter.LetterNo != null) {
        //    showErrorToast("No further changes are allowed on this document");
        //    return;
        //}
        $scope.Letter.Salary = getSalaryDataFromGrid();
        $http({
            method: "POST",
            url: baseURL + "Save",
            data: $scope.Letter
        }).then(result => {
            $scope.errors = [];
            if (!result.data.success) {
                $scope.errors = getArray(result.data.message);
                showErrorToast();
            }
            else {
                $scope.Letter = result.data.employeeLetter;
                session.FormMode = result.data.formMode;
                setFormSession(session);
                enableHistory(session.FormMode);
                enableSubmit();
                showSuccessToast(result.data.message);
                //clearForm();
                //setGridData();
            }
        }, errorMsg => {
            showErrorToast(errorMsg);
        });
    };

    $scope.tbCancel = function (res) {
          $http({
            method: "POST",
            url: baseURL + "Cancel",
            params: { _letterNo: res }
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

    $scope.tbLoad = function () {
        if ($scope.Letter.Employee.EmployeeId == null || $scope.Letter.TemplateId == null) {
            showErrorToast("Please select valid Employee & Template before hitting Load button");
            return;
        }

        $http({
            type: "Get",
            url: baseURL + "New",
            dataType: "JSON",
            params: {
                _employeeId: $scope.Letter.Employee.EmployeeId, _templateId: $scope.Letter.TemplateId}
        }).then(result => {
            //clearForm();
            $scope.errors = [];
            if (!result.data.success) {
                $scope.errors = getArray(result.data.message);
                showErrorToast(result.data.message);
            }
            else {
                //_letterId = $scope.Letter.LetterId;
                //_letterNo = $scope.Letter.LetterNo;
                //_footer = $scope.Letter.AuditTrail;
                //_stateId = $scope.Letter.StateId;
                //_stateName = $scope.Letter.StateName;
                $scope.Letter = result.data.employeeLetter;
                //$scope.Letter.LetterId = _letterId;
                //$scope.Letter.LetterNo = _letterNo;
                //$scope.Letter.AuditTrail = _footer;
                //$scope.Letter.StateId = _stateId;
                //$scope.Letter.StateName = _stateName;
                //$scope.footer = $scope.Letter.AuditTrail;
                setGridData();
                maskDates();
            }
        }, errorMsg => {
            showErrorToast(errorMsg);
        });
    };

    $scope.tbClear = function () {
        $scope.Letter.Loaded = false;
        $scope.Letter.ImpactsPayroll = false;
        $scope.Letter.Salary = {};
        $scope.Letter.TemplateVersion = null;
        $scope.Letter.LetterContent = null;
    };

    $scope.tbHistory = function () {
        $scope.formId = $scope.Letter.LetterId;
        $scope.workFlowId = 3;
        $scope.formTitle = "History Of Employee Letter # " + $scope.Letter.LetterNo;
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

    //#region form functions
    $scope.getLetterByNo = function (letterNo, letterId) {
        $http({
            type: "Get",
            url: baseURL + "Get",
            dataType: "JSON",
            params: { _letterNo: letterNo, _letterId: letterId }
        }).then(result => {
            clearForm();
            if (!result.data.success) {
                $scope.errors = getArray(result.data.message);
                showErrorToast(result.data.message);
            }
            else {
                $scope.Letter = result.data.employeeLetter;
                $scope.footer = $scope.Letter.AuditTrail;
                session.FormMode = result.data.formMode;
                setFormSession(session);
                enableHistory(session.FormMode);
                enableSubmit();
                setGridData();
                maskDates();
            }
        }, errorMsg => {
            showErrorToast(errorMsg);
        });
    };

    $scope.templateChanged = function (res) {
        obj = $scope.lstLetterTemplate.find(sh => { return sh.TemplateId == res; });
        $scope.Letter.IncludeSepEmp = obj["IncludeSepEmp"];
        //$scope.Letter.ImpactsPayroll = obj["ImpactsPayroll"];
        if (!$scope.Letter.IncludeSepEmp && $scope.Letter.Employee) {
            if ($scope.lstEmployee.find(e => { return e.EmployeeId == $scope.Letter.Employee.EmployeeId }) == undefined)
                $scope.Letter.Employee = [];
        }
    };

    $scope.tinymceOptions = {
        menubar: false,
        statusbar: false,

        branding: false,
        contextmenu: "spellchecker",
        resize: false,

        /* theme of the editor */
        theme: "silver",
        skin: "agility",

        /* width and height of the editor */
        width: "70vw",
        height: "60vh",

        /* display statusbar */
        statubar: true,

        ///* plugin */
        plugins: [
            "advlist image lists charmap preview hr anchor pagebreak",
            "searchreplace visualblocks visualchars insertdatetime nonbreaking",
            "table directionality emoticons template paste"
        ],
        paste_webkit_styles: "color font-size",
        paste_retain_style_properties: "all",
        paste_strip_class_attributes: "none",
        browser_spellcheck: true,
//        extended_valid_elements: "a[name|href|target|title|onclick],img[class|src|border=0|alt|title|hspace|vspace|width|height|align|onmouseover|onmouseout|name],table[style|class|border=2|width|cellspacing|cellpadding|bgcolor],colgroup,col[style|width],tbody,tr[style|class],td[style|class|colspan|rowspan|width|height],hr[class|width|size|noshade],font[face|size|color|style],span[class|align|style]",

        /* toolbar */
        toolbar: "undo redo | styleselect | sizeselect fontselect fontsizeselect | bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link image | preview media | forecolor backcolor emoticons | table | searchreplace",
        fontsize_formats: "8pt 10pt 12pt 14pt 16pt 18pt 20pt 24pt 30pt 36pt",

        content_style: ".mce-content-body {font-size:14pt;font-family:Arial,sans-serif;}",

        init_instance_callback: function (editor) {
            editor.on('keyDown', function (e) {
                if (9 == e.keyCode) { // tab pressed
                    editor.execCommand('mceInsertContent', false, '&emsp;&emsp;'); // inserts tab
                    event.preventDefault();
                    return false;
                }
            });
        }
    };

    function showSubmission() {
        switch ($scope.StateId) {
            case 2:
                $scope.formTitle = "Submitted For Approval";
                break;
            case 3:
                $scope.formTitle = "Approve Employee Letter";
                break;
            case 4:
                $scope.formTitle = "Reject Employee Letter";
                break;
            case 5:
                $scope.formTitle = "Return Employee Letter";
                break;
            case 99:
                $scope.formTitle = "Submitted for CEO Approval";
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

    //#region grid setup
    //#region salary grid
    var colSalary = [
        {
            headerName: "Salary Head", field: "HeadId", cellEditor: "agilitySelectEditor",
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
        angularCompileRows: true,
        onGridReady: (grid) => {
            setSalaryFooter();
        },
        getRowStyle: function (params) {
            if (params.node.rowPinned) {
                return { 'font-weight': 'bold', 'color': 'white', 'background-color': 'gray' };
            }
            //else if (params.node.data.SystemCalculated) {
            //    return { 'color': 'darkgray', 'font-style': 'italic' };
            //}
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
                $scope.grdOptSalary.api.getSelectedRows().forEach(x => { x.Delete = true; x.Add = false; });
                $scope.grdOptSalary.api.getFilterInstance('Delete').onFilterChanged();
                setSalaryFooter();
            }
        }
        catch (exception) { showErrorToast('Delete Salary Head: ' + exception); }
    };

    function setGridData() {
        try {
            if (!angular.equals($scope.Letter, {})) {  
                $scope.grdOptSalary.api.setRowData($scope.Letter.Salary);
                setGridDeleteFilter($scope.grdOptSalary.api);
            }
            else {
                $scope.grdOptSalary.api.setRowData([]);
            }
            setSalaryFooter();
        }
        catch (exception) { showErrorToast(exception); }
    }

    function setSalaryFooter() {
        try {
            let _total = 0;
            if ($scope.grdOptSalary.api.getDisplayedRowCount() > 0) {
                $scope.grdOptSalary.api.forEachNode(n => _total +=
                    n.data.HeadId != undefined && n.data.HeadId != -1 && n.data.HeadId != null && n.data.HeadId != "" && !n.data.Delete ?
                        n.data.Amount : 0);
            }
            var _footer = [{ HeadId: -1, Amount: _total }];
            $scope.grdOptSalary.api.setPinnedBottomRowData(_footer);
        }
        catch (exception) { showErrorToast('Salary Footer Method:' + exception); }
    }

    function setSalaryReviewColumns() {
        var colSalary = [
            {
                headerName: "Salary Head", field: "HeadName", width: 250, editable: false
            },
            {
                headerName: "Amount", field: "Amount", type: ["numericColumn"], editable: false,
                valueFormatter: formatNumbers, valueParser: numberValueParser, width: 120
            },
        ];
        $scope.grdOptSalary.columnDefs = colSalary;
    }
    //#endregion
    //#endregion

    //#region local function
    function clearForm() {
        $scope.Letter = {};
        $scope.footer = {};
        $scope.errors = [];
        session.FormMode = "Initialize";
        enableHistory(session.FormMode);
    }
    //#endregion

    //#region date functions
    function maskDates() {
        if ($scope.Letter.AuditTrail != null)
            $scope.Letter.AuditTrail.CreatedOn = formatDateTime($scope.Letter.AuditTrail.CreatedOn);
        if ($scope.Letter.AuditTrail.UpdatedOn != null)
            $scope.Letter.AuditTrail.UpdatedOn = formatDateTime($scope.Letter.AuditTrail.UpdatedOn);
    }
    //#endregion
});