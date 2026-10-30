baseURL = "/HRIS/Common/User/";
var app = angular.module('HRIS', ['ui.bootstrap', 'ngMaterial']);

$(document).ready(function () {
    $('#txtUserId').focus();
});

app.controller("UserController", function ($scope, $uibModal, $http, $window, $mdToast) {
    $scope.login = function () {
        var userId = $scope.UserProfile.UserId;
        var passWord = $scope.UserProfile.Password;

        var isValid = validParameters(userId, passWord, "Login");
        if (isValid) {
            $.ajax({
                url: baseURL + 'ValidateUser',
                type: "GET",
                data: $scope.UserProfile,
                beforeSend: function () {
                    $("#dialog-alert").dialog('open');
                },
                success: function (result) {
                    if (result.success == true) {
                        if (result.url.indexOf("ChangePassword") > -1) {
                            $("#dialog-alert").dialog('close');
                            $scope.changePassword();
                        }
                        else {
                            window.location.href = result.url;
                        }
                    }
                    else
                        showErrorToast(result.errorMsg);
                },
                error: function (xhr, ajaxOptions, thrownError) {
                    var tempDiv = document.createElement('div');
                    tempDiv.innerHTML = xhr.responseText;
                    showErrorToast(tempDiv.childNodes[1].innerHTML);
                    $('#txtUserId').focus();
                },
                complete: function () {
                    $("#dialog-alert").dialog('close');
                    $('#txtUserId').focus();
                }
            });
        }
    };

    $scope.SendPassword = function () {
        var userId = document.getElementById("txtUserId").value;
        var isValid = validParameters(userId, "", "Forgot");
        if (isValid) {
            $.ajax({
                url: baseURL + "SendPassword",
                type: "GET",
                data: { _userId: userId },
                beforeSend: function () {
                    $("#dialog-alert").dialog('open');
                },
                success: function (result) {
                    showSuccessToast("Password was successfully sent to email Id configured on selected User Profile!");
                },
                error: function (xhr, ajaxOptions, thrownError) {
                    var tempDiv = document.createElement('div');
                    tempDiv.innerHTML = xhr.responseText;
                    showErrorToast(tempDiv.childNodes[1].innerHTML);
                    $('#txtUserId').focus();
                },
                complete: function () {
                    $("#dialog-alert").dialog('close');
                }
            });
        }
    };

    $("#dialog-alert").dialog({
        autoOpen: false,
        resizable: false,
        height: $(window).height() * 0.6,
        width: $(window).width() * 0.35,
        title: "Appreciate your patience",
        show: { effect: 'drop', direction: "up" },
        modal: true,
        draggable: false,
        overlapEmbedObjects: true,
        zindex: 1000,
        open: function (event, ui) {
            $(".ui-dialog-titlebar-close").hide(),
            $(".ui-dialog-titlebar").hide();
        }
    });

    $scope.changePassword = function () {
        $scope.formTitle = "Its time to Change your Password";
        try {
            var modalScope = $scope.$new();
            var modalInstance = $uibModal.open({
                animation: true,
                templateUrl: '/HRIS/Helper/Pages/ChangePassword.html',
                windowClass: 'show',
                backdropClass: 'show',
                keyboard: true,
                size: "md",
                scope: modalScope
            });
            modalScope.modalInstance = modalInstance;

            modalInstance.result.then(function () {
                $('#txtUserId').focus();
            }, function () {

            });
        }
        catch (exception) { showErrorToast(exception); }
    };
});

function validParameters(userId, password, processName) {
    if (processName == "Login" && (userId.trim() == "" || password.trim() == "")) {
        showErrorToast("Please enter valid User Id & Password before proceeding with Login process!");
        $('#txtUserId').focus();
        return false;
    }
    else if (processName == "Forgot" && userId.trim() == "") {
        showErrorToast("Please enter User Id before using Forgot Password option!");
        $('#txtUserId').focus();
        return false;
    }
    else
        return true;
}