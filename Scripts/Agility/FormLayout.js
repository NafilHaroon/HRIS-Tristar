$(document).ready(function () {
    $("input:text").focus(function () { $(this).select(); });

    $(".openDialog").on("click", function (e) {
        e.preventDefault();

        if ($(e.target).attr('disabled') == 'disabled') 
        {
            return;
        }

        if ($(this).attr("data-dialog-id") == "dlgReject")
        {
            if (!confirm("You are about to Reject this request permanently, Once rejected, no action could be taken on this request. Are you sure you want to proceed with Rejection?"))
                return;
        }

        var waitDialog = $("#dialog-wait-small").dialog({
            autoOpen: false,
            modal: true,
            height: $(window).height() * 0.6,
            width: $(window).width() * 0.45,
            open: function (event, ui) {
                $(this).closest(".ui-dialog").find(".ui-dialog-titlebar:first").hide();
            },
        });

        $("<div></div>")
            .addClass("dialog")
            .attr("id", $(this)
            .attr("data-dialog-id"))
            .appendTo("body")
            .dialog({
                title: $(this).attr("data-dialog-title"),
                close: function () { $(this).remove(); },
                modal: true,
                resizable: false,
                height: $(window).height() * 0.58,
                width: $(window).width() * 0.42,
                left: 0,
                open: function (event, ui) {
                    $(waitDialog).dialog('open');
                },
            })
            .load(this.href, function (responseText, statusText, errorMsg) {
                waitDialog.dialog('close');
                if (statusText == "error") {
                    var tempDiv = document.createElement('div');
                    tempDiv.innerHTML = errorMsg.responseText;
                    showErrorToast(tempDiv.childNodes[1].innerText);
                    $(this).dialog('close');
                }
            });
    });

    $("#dialog-wait-large").dialog({
        autoOpen: false,
        resizable: false,
        height: $(window).height() * 0.3,
        width: $(window).width() * 0.3,
        title: "Appreciate your patience",
        show: { effect: 'drop', direction: "up" },
        modal: true,
        draggable: false,
        overlapEmbedObjects: true,
        zindex: 1000,
        closeOnEscape: false,
        open: function (event, ui) {
            $(".ui-dialog-titlebar-close").hide(),
            $(".ui-dialog-titlebar").hide();
        }
    });
});

function deleteRequest(_deleteId) {
    var waitForDelete = $("#dialog-wait-small").dialog({
        autoOpen: false,
        closeOnEscape: false,
        modal: true,
        title: "Requested operation is in progress...",
        height: $(window).height() * 0.4,
        width: $(window).width() * 0.3,
        open: function (event, ui) {
            $(".ui-dialog-titlebar-close").hide();
        }
    });

    $.ajax({
        url: 'Delete',
        type: 'Post',
        data: { _keyId: _deleteId },
        beforeSend: function () {
            $(waitForDelete).dialog('open');
        },
        success: function (result) {
             $(waitForDelete).dialog('close');
            if (result.success) {
                window.location.href = 'DataForm'; //'@Url.Action("DataForm")';
            }
            else
            {
                showErrorToast(result.errorMsg);
            }
        },
        error: function (xhr, ajaxOptions, thrownError) {
            $(waitForDelete).dialog('close');
            var tempDiv = document.createElement('div');
            tempDiv.innerHTML = xhr.responseText;
            showErrorToast("The requested operation failed with following error text:\n\n" + tempDiv.childNodes[1].innerHTML);
        }
    })
}

function formatNumbers(x) {
    var parts = x.toString().split(".");
    parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ",");
    return parts.join(".");
}

function removeCommas(x) {
    while (x.search(",") > 0) {
        x = x.replace(',', '');
    }
    return x;
    //return x.replace(",", "");
}

function formatDate(_date) {
    try {
        if (_date != null) {
            var date = new Date(Number(_date.replace("/Date(", "").replace(")/", "")));
            var monthNames = ["Jan", "Feb", "Mar", "Apr", "May", "Jun",
                "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
            var DD = ("0" + date.getDate()).slice(-2);
            var DM = monthNames[date.getMonth()];
            var DY = date.getFullYear().toString();//.substr(2, 2)
            var formattedDate = DD + "." + DM + "." + DY;
            return formattedDate;
        }
        else { return _date; }
    }
    catch (exception) { showErrorToast('error formating date: ' + exception); }
}

function formatDate2(_date) {
    try {
        if (_date != null) {
            date = new Date(_date);
            var monthNames = ["Jan", "Feb", "Mar", "Apr", "May", "Jun",
                "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
            var DD = ("0" + date.getDate()).slice(-2);
            var DM = monthNames[date.getMonth()];
            var DY = date.getFullYear().toString();//.substr(2, 2)
            var formattedDate = DD + "." + DM + "." + DY;
            return formattedDate;
        }
        else { return _date; }
    }
    catch (exception) { showErrorToast('error formating date 2: ' + exception); }
}

function formatTime(_time) {
    try {
        if (_time != null) {
            var replaced = _time.replace("/Date(", "").replace(")/", "");
            var _date = new Date(Number(replaced));
            var hours = _date.getHours();
            var minutes = _date.getMinutes();
            var ampm = hours >= 12 ? 'pm' : 'am';
            hours = hours % 12;
            hours = hours ? hours : 12; // the hour '0' should be '12'
            minutes = minutes < 10 ? '0' + minutes : minutes;
            var strTime = hours + ':' + minutes + ' ' + ampm;
            return strTime;
        }
        else { return _time; }
    }
    catch (exception) { showErrorToast('error formating time: ' + exception); }
}

function formatDateTime(_dateTime) {
    var dateTime = new Date(Number(_dateTime.replace("/Date(", "").replace(")/", "")));
    var monthNames = ["Jan", "Feb", "Mar", "Apr", "May", "Jun",
     "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
    var DD = ("0" + dateTime.getDate()).slice(-2);
    var DM = monthNames[dateTime.getMonth()];
    var DY = dateTime.getFullYear().toString();
    var hours = dateTime.getHours();
    var minutes = dateTime.getMinutes();
    var sec = dateTime.getSeconds();
    hours = hours < 10 ? '0' + hours : hours;
    minutes = minutes < 10 ? '0' + minutes : minutes;
    sec = sec < 10 ? '0' + sec : sec;
    return DD + "." + DM + "." + DY + ' ' + hours + ':' + minutes + ':' + sec;
}

function getArray(_data) {
    return _data.constructor == Array ? _data : [_data];
}

function hasError() {
    var scope = angular.element($("#ngForm")).scope();
    return scope.errors != undefined && scope.errors.length > 0;
}

function monthNameToNum(monthname) {
    var months = [
        'Jan', 'Feb', 'Mar', 'Apr', 'May',
        'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'
    ];
    var month = months.indexOf(monthname);
    return month ? month + 1 : 0;
}