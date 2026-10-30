var HRISApp = angular.module('HRIS', ['ngRoute']);

HRISApp.config(['$routeProvider', '$locationProvider', function ($routeProvider, $locationProvider) {

    $routeProvider.when("/home", {
        templateUrl: "/Templates/HtmlPage.html" //Load a HTML template  
        })
        .when("/details", {
            templateUrl: "/Home/TestRedirect",  //Redirect to action  
            //controller: "TestCntrl"
        })
        //.when("/contact", {
        //    template = "<h2>Contact</h2><br/><div>Marimuthu Karuppaiah<br/>India</div>"   //Render custom design  
        //})
        .otherwise({
            redirectTo: "/home"
        });
    $locationProvider.hashPrefix('');
}]);   