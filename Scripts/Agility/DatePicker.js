$(function () {
    $.validator.addMethod('date',
    function (value, element) {
        if (this.optional(element)) {
            return true;
        }
        var ok = true;
        try {
            $.datepicker.parseDate('dd.M.yyyy', value);
        }
        catch (err) {
            ok = false;
        }
        return ok;
    });
});
