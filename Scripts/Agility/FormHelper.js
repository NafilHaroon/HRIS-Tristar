function setFormControls(formMode, allowAdd, allowEdit, allowDelete) {
    try {
        //#region this block is there to convert string values to boolean
        allowAdd = allowAdd != "False" && allowAdd != "false";
        allowEdit = allowEdit != "False" && allowEdit != "false";
        allowDelete = allowDelete != "False" && allowDelete != "false";
        //#endregion end of special block

        if (formMode == "Edit" || formMode == "Add") {
            $("#divMain input,select,textarea,label,button,div[class*='tox-editor-container'],a").prop("disabled", false);
            $('#divMain md-switch').removeAttr('disabled');
            $("#divMain input[type=text],select:enabled")[0].disabled = true;
            $("#divMain select[accesskey='permanentReadOnly']").prop("disabled", true);
            $("#divMain div[class*='ag-root-wrapper']").removeClass('ag-disabled');
            $("#FooterButtons button").prop("disabled", true);
            if (document.getElementById('ui-tinymce-1') != undefined) {
                if (tinyMCE.editors.length > 0)
                    tinymce.activeEditor.setMode('design');
            }
        }
        else {
            $("#divMain input,select,textarea,label,button,div[class*='tox-editor-container'],a[accesskey!='tabPanel']").prop("disabled", true);
            $('#divMain md-switch').attr('disabled', "");
            $("#divMain div[class*='ag-root-wrapper']").addClass('ag-disabled');
            $("#FooterButtons button").prop("disabled", false);
            if (formMode == "Recall") {
                $('#divMain input[type="text"],select:disabled')[0].disabled = false;
            }
            if (document.getElementById('ui-tinymce-1') != undefined) {
                if (tinyMCE.editors.length > 0)
                    tinymce.activeEditor.setMode('readonly');
            }
        }

        if (document.getElementById("btnAdd") != null)
            document.getElementById("btnAdd").disabled = !allowAdd || formMode != "Initialize";
        if (document.getElementById("btnEdit") != null)
            document.getElementById("btnEdit").disabled = !allowEdit || formMode != "ReadOnly";
        if (document.getElementById("btnDelete") != null)
            document.getElementById("btnDelete").disabled = !allowDelete || formMode != "ReadOnly";

        if (document.getElementById("btnSave") != null)
            document.getElementById("btnSave").disabled = (formMode != "Edit" || !allowEdit) && formMode != "Add";

        if (formMode != "Initialize") {
            if (document.getElementById("btnFind") != null)
                document.getElementById("btnFind").disabled = true;
        }
        else {
            if (document.getElementById("btnFind") != null)
                document.getElementById("btnFind").disabled = false;
        }

        if (document.getElementById("btnUndo") != null)
            document.getElementById("btnUndo").disabled = formMode == "Initialize" || formMode == "Review";
        if (document.getElementById("btnRecall") != null)
            document.getElementById("btnRecall").disabled = formMode != "Initialize";
        if (document.getElementById("btnRecall") != null)
            document.getElementById("btnExit").disabled = formMode != "Initialize" && formMode != "Review";

        if (formMode == "Edit" || formMode == "Add" || formMode == "Recall") {
            $(':input(input[type="text"],select):not([class~="dateField"]):not([type="checkbox"]):enabled:visible:not([readonly]):first').focus();
        }
    }
    catch (exception) { showErrorToast(exception); }
    finally { hideFormLoading(); }
}

function setFormSession(session) {
    setFormControls(session.FormMode, session.AllowAdd, session.AllowEdit, session.AllowDelete);
}

function showWaitDialog() {
    try {
        $("#dialog-wait-small").dialog({
            autoOpen: false,
            closeOnEscape: false,
            resizable: false,
            modal: true,
            title: "Requested operation is in progress...",
            height: $(window).height() * 0.4,
            width: $(window).width() * 0.3,
            draggable: false,
            overlapEmbedObjects: true,
            zindex: 1000,
            open: function (event, ui) {
                $(".ui-dialog-titlebar-close").hide();
                $(".ui-dialog-titlebar").hide();
            }
        });

        $("#dialog-wait-small").dialog('open');
    }
    catch (exception) { showErrorToast(exception); }
}

function hideWaitDialog() {
    $("#dialog-wait-small").dialog('close');
}

function showFormLoading() {
    //working code disabled for debugging
    $('#dialog-load-small').dialog({
        appendTo: "#divBody",
        autoOpen: false,
        resizable: false,
        modal: true,
        draggable: false,
        overlapEmbedObjects: true,
        closeOnEscape: false,
        height: $(window).height() * 0.4,
        width: $(window).width() * 0.2,
        top: $(window).height() * 0.6 / 2,
        show: { effect: 'fade', direction: "in" },
        hide: { effect: 'drop', direction: "down" },
        //zindex: 1000,
        close: function () { $(this).remove(); },
        open: function (event, ui) {
            $(".ui-dialog-titlebar-close").hide(),
            $(".ui-dialog-titlebar").hide();
        }
    });

    $('#dialog-load-small').dialog('open');
}

function hideFormLoading() {
    $('#dialog-load-small').dialog('close');
}

function showSuccessToast(msg) {
    try {
        var toastService = angular.element($("#ngForm")).injector().get('$mdToast');
        if (msg == undefined)
            msg = "Save operation completed successfully!";
        toastService.show(
            toastService.simple()
                .textContent(msg)
                .parent(angular.element(document.body))
                .highlightAction(true)
                .position('bottom right')
                .hideDelay(3000)
                .theme('success-toast'));
    }
    catch (ex) { alert(ex.message); }
}

function showErrorToast(msg) {
    try {
        if (msg == undefined)
            msg = "Some validation errors occurred that prevented requested operation!";
        var toastService = angular.element($("#ngForm")).injector().get('$mdToast');
        toastService.show(
            toastService.simple()
                .textContent(msg)
                .parent(angular.element(document.body))
                .highlightAction(true)
                .position('top right')
                .hideDelay(3000)
                .theme('error-toast'));
    }
    catch (ex) {
        alert(ex.message);
    }
}

function showYesNoToast(msg, callBack) {
    try {
        var toastService = angular.element($("#ngForm")).injector().get('$mdToast');
        if (msg == undefined)
            msg = "Are you sure you want to proceed?";
        toastService.show(
            toastService.simple()
                .textContent(msg)
                .parent(angular.element(document.body))
                .highlightAction(true)
                .position('bottom right')
                .actionKey('y')
                .actionHint('Press Control-"Y" key combination to Proceed')
                .dismissHint('Press Escape key to dismiss this message.')
                .highlightAction(true)
                .highlightClass('md-accent')
                .theme('yesno-toast')
                .action('Proceed')
                .hideDelay(0)).then(function (response) {
                    if (response === 'ok') {
                        callBack(true);
                    }
                    else { callBack(false); }
                });
    }
    catch (ex) { alert(ex.message); }
}

function showWritingMsg() {
    $('#dialog-writing-small').dialog({
        appendTo: "#divBody",
        autoOpen: false,
        resizable: false,
        modal: true,
        draggable: false,
        overlapEmbedObjects: true,
        closeOnEscape: false,
        height: $(window).height() * 0.4,
        width: $(window).width() * 0.2,
        top: $(window).height() * 0.6 / 2,
        show: { effect: 'fade', direction: "in" },
        hide: { effect: 'drop', direction: "down" },
        //zindex: 1000,
        close: function () { $(this).remove(); },
        open: function (event, ui) {
            $(".ui-dialog-titlebar-close").hide(),
                $(".ui-dialog-titlebar").hide();
        }
    });

    $('#dialog-writing-small').dialog('open');
}

function hideWritingMsg() {
    $('#dialog-writing-small').dialog('close');
}

function enableSubmit() {
    if (document.getElementById("btnSubmit") != null) {
        document.getElementById("btnSubmit").disabled = false;
    }
}

function enableHistory(formMode) {
    if (formMode == "ReadOnly" || formMode == "Review") {
        document.getElementById('btnHistory').style.visibility = 'visible'; // show
        document.getElementById("btnHistory").disabled = false;
    }
    else {
        document.getElementById('btnHistory').style.visibility = 'hidden'; // hide  
        document.getElementById("btnHistory").disabled = true;
    }
}

function currentUser() {
    return $("#hdnSession").data('value');
}