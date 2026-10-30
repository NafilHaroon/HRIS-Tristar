baseURL = "/HRIS/Master/Config/";
agGrid.initialiseAgGridWithAngular1(angular);
var app = angular.module('HRIS', ['ui.bootstrap', 'agGrid', 'ngMaterial']); 
let session = "";

app.controller('ConfigController', function ($scope, $rootScope, $http, $uibModal, $mdToast) {
    //#region form loading events
    $scope.formLoaded = function () {
        $http({
            method: "GET",
            url: "/HRIS/Home/GetAccessRight",
            dataType: "JSON",
            params: { _optionName: "Configuration" }
        }).then(result => {
            session = result.data;
            setFormSession(session);
        },
            errorMsg => {
                showErrorToast(errorMsg);
            });
        loadDropDown();
        getConfig();
        hideFormLoading();
    };

    function loadDropDown() {
        $http.get(baseURL + 'GetLookups').then(result => {
            $scope.lstLoan = result.data.lstLoan;
            $scope.lstAbsent = result.data.lstAbsent;
        }, errorMsg => {
            showErrorToast(errorMsg);
        });
    }
    //#endregion

    //#region toolbar actions
    $scope.tbSave = function (res) {
        $http({
            method: "POST",
            url: baseURL + "Save",
            data: $scope.Config
        }).then(result => {
            $scope.errors = [];
            if (!result.data.success) {
                $scope.errors = getArray(result.data.message);
                showErrorToast();
            }
            else {
                showSuccessToast(result.data.message);
            }
            }, errorMsg => {
                showErrorToast(errorMsg);
        });
    };

    $scope.tbExit = function () {
        $scope = null;
        window.location.href = '/HRIS/Common/Common/Index';
    };

    function getConfig() {
        $http.get(baseURL + 'Get').then(result => {
            $scope.Config = result.data.config;
        }, errorMsg => {
            showErrorToast(errorMsg);
        });
    }
});