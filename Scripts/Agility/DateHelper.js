//$.datepicker.setDefaults({
//    dateFormat: 'dd-M-y',
//    currentText: 'Now',
//    changeYear: true,
//    changeMonth: true,
//    gotoCurrent: true,
//    showAnim: 'blind',
//    autoSize: true,
//    constrainInput: true,
//    highlightWeek: true,
//    maxDate: '+0D',
//    showOn: "both",
//    buttonImageOnly: true,
//    buttonImage: "../../Content/Icon/calendar.gif",
//    buttonText: "Calendar",
//    //format: "dd-MM-yy",
//    //clearBtn: true,
//    //todayHighlight: true,
//    //autoclose: true,
//    //maxViewMode: 0,
//    //minViewMode: 0
//});

$.extend($.fn.datepicker.defaults,
    {
        startDate: '-3d',
        format: 'dd.M.yyyy',
        endDate: '+3M',
        todayHighlight: true,
        autoclose: true,
        useCurrent: true,
        clearBtn: true,
        forceParse: false
    }
);
