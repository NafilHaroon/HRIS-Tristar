$.datepicker.setDefaults({
    changeMonth: true,
    changeYear: true,
    showButtonPanel: false,
    dateFormat: 'M-y',
    minDate: '-7Y',
    maxDate: '+0D',
    onClose: function (dateText, inst) {
        var month = $("#ui-datepicker-div .ui-datepicker-month :selected").val();
        var year = $("#ui-datepicker-div .ui-datepicker-year :selected").val();
        $(this).datepicker('setDate', new Date(year, month, 1));
    },
    beforeShow: function (input, inst) {
        var datestr;
        if ((datestr = $(this).val()).length > 0) {
            year = '20' + datestr.substring(datestr.length - 2, datestr.length);
            month = jQuery.inArray(datestr.substring(0, datestr.length - 3), $(this).datepicker('option', 'monthNamesShort'));
            $(this).datepicker('option', 'defaultDate', new Date(year, month, 1));
            $(this).datepicker('setDate', new Date(year, month, 1));
        }
    }
});