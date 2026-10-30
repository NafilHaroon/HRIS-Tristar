baseURL = "/HRIS/Master/Company/";
var app = angular.module('HRIS', []);//"ui.bootstrap"

app.controller('MainLayout', function ($scope, $http) {
    $scope.formLoaded = function () {
        //$http({
        //    method: "GET",
        //    url: "/HRIS/Home/GetAccessRight",
        //    dataType: "JSON",
        //    params: { _optionName: "Company" }
        //}).then(result => {
        //    session = result.data;
        //    setFormSession(session);
        //},
        //errorMsg => {
        //    showErrorToast(errorMsg);
        //});
        loadDefaultCompanyandLogo();
        loadCompanyList();
    };

    function loadDefaultCompanyandLogo() {
        try {
            $http({
                type: "Get",
                url: baseURL + "GetUserDefaultCompany",
                dataType: "JSON"//,
                //params: { _companyId: 6 }
            }).then(result => {
                $('#imgCompanyLogo').attr("src", "data:image/png;base64," + result.data.usercompany.Logo);
                $scope.UserCompanyId = result.data.usercompany.CompanyId;
                }, errorMsg => {
                    showErrorToast(errorMsg);
            });
        }
        catch (exception) { showErrorToast('loadDefaultCompanyLogo: ' + exception); }
    }

    function loadCompanyList() {
        try {
            $http({
                type: "Get",
                url: baseURL + "GetUserCompanies",
                dataType: "JSON"
            }).then(result => {
                $scope.lstCompany = result.data;
                }, errorMsg => {
                    showErrorToast(errorMsg);
            });
        }
        catch (exception) { showErrorToast('loadCompanyList: ' + exception); }
    }

    $scope.toggleCompany = function () {
        var img = document.getElementById('imgCompanyLogo');
        img.style.display == "block" ? img.style.display = "none" : img.style.display = "block";
        var dropdown = document.getElementById('divCompanySelection');
        dropdown.style.display == "block" ? dropdown.style.display = "none" : dropdown.style.display = "block";
    };

    $scope.saveSelection = function () {
        $http({
            url: baseURL + "SetUserDefaultCompany",// + $scope.UserCompanyId,
            data: { _companyId: $scope.UserCompanyId },
            method: "post"
        }).then(result => {
            loadDefaultCompanyandLogo();
            $scope.toggleCompany();
            }, errorMsg => {
                showErrorToast(errorMsg);
        });
    };

    $scope.discardSelection = function () {
        $scope.toggleCompany();
    };
});