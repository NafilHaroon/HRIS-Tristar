baseURL = "/HRIS/Master/LetterTemplate/";
agGrid.initialiseAgGridWithAngular1(angular);
var app = angular.module('HRIS', ['ui.bootstrap', 'agGrid', 'ngMaterial', 'ui.tinymce']); 
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

app.controller('TemplateController', function ($scope, $rootScope, $http, $uibModal, $mdToast) {
    //#region form loading events
    $scope.formLoaded = function () {
        $http({
            method: "GET",
            url: "/HRIS/Home/GetAccessRight",
            dataType: "JSON",
            params: { _optionName: "Letter Template" }
        }).then(result => {
            session = result.data;
            setFormSession(session);
            loadDropDown();
        },
        errorMsg => {
            showErrorToast(errorMsg);
        });
    };

    function loadDropDown() {
        try {
            $http.get(baseURL + 'GetLookups').then(result => {
                $scope.lstDocumentType = result.data.lstDocumentType;
            }, errorMsg => {
                showErrorToast(errorMsg);
            });
        }
        catch (exception) { showErrorToast('loadDropDown: ' + exception.message); }
    }
    //#endregion

    //#region toolbar actions
    $scope.tbAdd = function () {
        $http({
            method: "Get",
            url: baseURL + "Add"
        }).then(response => {
            $scope.Template = response.data.template;
            $scope.Footer = response.data.template.AuditTrail;
            session.FormMode = response.data.formMode;
            setFormSession(session);
            maskDates();
            $('#txtTemplateName').focus();
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
        $scope.formTitle = "Search & Select Letter Template";
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

    $scope.tbEdit = function () {
        session.FormMode = "Edit";
        setFormSession(session);
        $('#txtTemplateName').focus();
    }

    $scope.tbSave = function () {
        $http({
            method: "POST",
            url: baseURL + "Save",
            data: $scope.Template
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
        clearForm();
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
                $scope.Template = result.data.template;
                $scope.footer = $scope.Template.AuditTrail;
                session.FormMode = result.data.formMode;
                setFormSession(session);
                maskDates();
            }
        }, errorMsg => {
            showErrorToast(errorMsg);
        });
    };

    $scope.tinymceOptions = {
        menubar: false,
        statusbar: false,

        branding: false,
        contextmenu: "spellchecker",
        resize: false,

        /* theme of the editor */
        theme: "silver",
        skin: "oxide-dark",

        /* width and height of the editor */
        width: "80vw",
        height: "60vh",

        /* display statusbar */
        statubar: true,

        browser_spellcheck: true,

        ///* plugin */
        plugins: [
            "advlist image lists charmap preview hr anchor pagebreak",
            "searchreplace visualblocks visualchars insertdatetime nonbreaking",
            "table directionality emoticons template paste"
        ],

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

    $scope.showVariables = function () {
        try {
            var modalScope = $scope.$new();
            var modalInstance = $uibModal.open({
                animation: true,
                templateUrl: '/HRIS/Helper/Pages/LetterLegend.html',
                windowClass: 'show',
                backdropClass: 'show',
                size: "lg",
                scope: modalScope
            });
            modalScope.modalInstance = modalInstance;

            modalInstance.result.then(function () {}, function () {
                $uibModal.close('save');
            });
        }
        catch (exception) { showErrorToast(exception); }
    };
    //#endregion

    //#region local function
    function clearForm() {
        $scope.Template = {};
        $scope.footer = {};
        $scope.errors = [];
    }
    //#endregion

    //#region date functions
    function maskDates() {
        if ($scope.Template.AuditTrail != null)
            $scope.Template.AuditTrail.CreatedOn = formatDateTime($scope.Template.AuditTrail.CreatedOn);
        if ($scope.Template.AuditTrail.UpdatedOn != null)
            $scope.Template.AuditTrail.UpdatedOn = formatDateTime($scope.Template.AuditTrail.UpdatedOn);
    }
    //#endregion
});