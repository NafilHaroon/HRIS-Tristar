app.controller('SearchController', function ($scope, $rootScope, $http) {
    function valueSelected(row) {
        try {
            $rootScope.SelectedRow = row.data;
            $scope.modalInstance.close('ok');
        }
        catch (exception) { showErrorToast(exception.Message); }
    }

    try {
        $http({
            type: "Get",
            url: baseURL + "Search"
        }).then(result => {
            if (!result.data.success) {
                showErrorToast(result.data.message);
                $scope.modalInstance.close('ok');
            }
            else {
                $scope.gridOptions.api.setColumnDefs(result.data.columnDefs);
                $scope.gridOptions.api.setRowData(result.data.rowData);//JSON.parse(JSON.stringify(
                $scope.gridOptions.api.sizeColumnsToFit();
            }
            }, errorMsg => {
                showErrorToast(errorMsg);
        });

        $scope.gridOptions = {
            headerHeight: 25,
            rowHeight: 32,
            //enableFilter: true,
            animateRows: true,
            floatingFilter: true,
            enableSorting: true,
            multiSortKey: 'ctrl',
            rowSelection: 'single',
            rowDeselection: true,
            enableColResize: true,
            suppressCellSelection: true,
            onRowDoubleClicked: valueSelected,
            overlayLoadingTemplate: '<span class="ag-overlay-loading-center">Please wait while we are fetching requested records from database</span>',
            overlayNoRowsTemplate: '<span style="padding: 10px; border: 2px solid #444; background: lightgoldenrodyellow; color:red;">No rows available to display here</span>'
        };
    }
    catch (exception) {
        showErrorToast(exception.Message);
    }
});