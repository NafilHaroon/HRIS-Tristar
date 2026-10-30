//#region AG Grid Controls
function getAgilitySelect() {
    function AgilitySelect() { }
    AgilitySelect.prototype.init = function (params) {
        try {
            var parentScope = angular.element($("#ngForm")).scope();
            this.dropDown = document.createElement('select');
            this.dropDown.className = params.class;
            switch (params.source) {
                case "SalaryHead":
                    parentScope.lstSalaryHead.forEach(x => {
                        if (!x.SystemCalculated) {
                            var opt = document.createElement('option');
                            opt.value = x.HeadId;
                            opt.innerHTML = x.HeadName;
                            this.dropDown.appendChild(opt);
                        }
                    });
                    break;
                case "Template":
                    parentScope.lstAllocTemplate.forEach(x => {
                        var opt = document.createElement('option');
                        opt.value = x.TemplateId;
                        opt.innerHTML = x.TemplateName;
                        this.dropDown.appendChild(opt);
                    });
                    break;
                case "Branch":
                    parentScope.lstBranch.forEach(x => {
                        var opt = document.createElement('option');
                        opt.value = x.BranchCode;
                        opt.innerHTML = x.BranchName;
                        this.dropDown.appendChild(opt);
                    });
                    break;
                case "Relation":
                    parentScope.lstRelation.forEach(x => {
                        var opt = document.createElement('option');
                        opt.value = x.RelationId;
                        opt.innerHTML = x.RelationName;
                        this.dropDown.appendChild(opt);
                    });
                    break;
                case "EntitlementType":
                    parentScope.lstEntitlementType.forEach(x => {
                        var opt = document.createElement('option');
                        opt.value = x.EntitlementTypeId;
                        opt.innerHTML = x.EntitlementTypeName;
                        this.dropDown.appendChild(opt);
                    });
                    break;
                case "LeaveType":
                    parentScope.lstLeaveType.forEach(x => {
                        var opt = document.createElement('option');
                        opt.value = x.LeaveTypeId;
                        opt.innerHTML = x.LeaveTypeName;
                        this.dropDown.appendChild(opt);
                    });
                    break;
                case "Account":
                    parentScope.lstAccount.forEach(x => {
                        var opt = document.createElement('option');
                        opt.value = x.AccountId;
                        opt.innerHTML = x.AccountName;
                        this.dropDown.appendChild(opt);
                    });
                    break;
                //case "JvCompanyAccount":
                //    parentScope.lstCompanyAccount.forEach(x => {
                //        var opt = document.createElement('option');
                //        opt.value = x.AccountId;
                //        opt.innerHTML = x.AccountName;
                //        this.dropDown.appendChild(opt);
                //    });
                //    break;
                case "Department":
                    parentScope.lstDepartment.forEach(x => {
                        var opt = document.createElement('option');
                        opt.value = x.DepartmentCode;
                        opt.innerHTML = x.DepartmentName;
                        this.dropDown.appendChild(opt);
                    });
                    break;
                case "WorkerType":
                    parentScope.lstWorkerType.forEach(x => {
                        var opt = document.createElement('option');
                        opt.value = x.WorkerTypeId;
                        opt.innerHTML = x.WorkerTypeName;
                        this.dropDown.appendChild(opt);
                    });
                    break;
                case "Company":
                    parentScope.lstCompany.forEach(x => {
                        var opt = document.createElement('option');
                        opt.value = x.CompanyId;
                        opt.innerHTML = x.CompanyName;
                        this.dropDown.appendChild(opt);
                    });
                    break;
                case "DocumentType":
                    parentScope.lstDocumentType.forEach(x => {
                        var opt = document.createElement('option');
                        opt.value = x.DocumentTypeId;
                        opt.innerHTML = x.DocumentTypeName;
                        this.dropDown.appendChild(opt);
                    });
                    break;
                case "AttachmentType":
                    parentScope.lstAttachmentType.forEach(x => {
                        var opt = document.createElement('option');
                        opt.value = x.TypeId;
                        opt.innerHTML = x.TypeName;
                        this.dropDown.appendChild(opt);
                    });
                    break;
            }
            this.dropDown.value = params.value;
        }
        catch (exception) { showErrorToast('Agility agGrid Select init: ' + exception); }
    };

    AgilitySelect.prototype.getGui = function () {
        return this.dropDown;
    };

    AgilitySelect.prototype.afterGuiAttached = function () {
        if (this.dropDown) this.dropDown.focus();
    };

    AgilitySelect.prototype.getValue = function () {
        return this.dropDown.value;
    };

    AgilitySelect.prototype.isPopup = function () {
        return true;
    };

    return AgilitySelect;
}

function getDatePicker() {
    function DateEditor() { }

    // gets called once before the renderer is used
    DateEditor.prototype.init = function (params) {
        // create the cell
        this.eInput = document.createElement('input');
        this.eInput.value = params.value;
        $(this.eInput).datepicker({
            startDate: params.minDate !== undefined ? params.minDate : '-10Y',
            endDate: params.maxDate !== undefined ? params.maxDate : '+10Y'
        });
    };

    // gets called once when grid ready to insert the element
    DateEditor.prototype.getGui = function () {
        return this.eInput;
    };

    // focus and select can be done after the gui is attached
    DateEditor.prototype.afterGuiAttached = function () {
        this.eInput.focus();
        this.eInput.select();
    };

    // returns the new value after editing
    DateEditor.prototype.getValue = function () { return this.eInput.value; };

    // any cleanup we need to be done here
    DateEditor.prototype.destroy = function () { };

    // if true, then this editor will appear in a popup
    DateEditor.prototype.isPopup = function () {
        return false;
    };
    return DateEditor;
}

function getTimePicker() {
    function TimeEditor() { }

    // gets called once before the renderer is used
    TimeEditor.prototype.init = function (params) {
        // create the cell
        this.eInput = document.createElement('input');
        this.eInput.value = params.value;
        $(this.eInput).timepicker({
            timeformat: "hh:mm"
        });
    };

    // gets called once when grid ready to insert the element
    TimeEditor.prototype.getGui = function () {
        return this.eInput;
    };

    // focus and select can be done after the gui is attached
    TimeEditor.prototype.afterGuiAttached = function () {
        this.eInput.focus();
        this.eInput.select();
    };

    // returns the new value after editing
    TimeEditor.prototype.getValue = function () { return this.eInput.value; };

    // any cleanup we need to be done here
    TimeEditor.prototype.destroy = function () { };

    // if true, then this editor will appear in a popup
    TimeEditor.prototype.isPopup = function () {
        return false;
    };
    return TimeEditor;
}

function getCellCheckBox() {
    function chkBox() { }

    // gets called once before the renderer is used
    chkBox.prototype.init = function (params) {
        // create the cell
        this.eInput = document.createElement('input');
        this.eInput.type = "checkbox";
        this.eInput.checked = params.value;
    };

    // gets called once when grid ready to insert the element
    chkBox.prototype.getGui = function () {
        return this.eInput;
    };

    // focus and select can be done after the gui is attached
    chkBox.prototype.afterGuiAttached = function () {
        this.eInput.focus();
        this.eInput.select();
    };

    // returns the new value after editing
    chkBox.prototype.getValue = function () { return this.eInput.checked; };

    // any cleanup we need to be done here
    chkBox.prototype.destroy = function () { };

    // if true, then this editor will appear in a popup
    chkBox.prototype.isPopup = function () {
        return false;
    };
    return chkBox;
}

function getCellButton() {
    function btn() { }
    btn.prototype.init = function (params) {
        this.eGui = document.createElement('div');
        this.eGui.innerHTML = '<span class="my-css-class"><button class="btn-simple">Browse</button><span class="my-value"></span></span>';
        this.eButton = this.eGui.querySelector('.btn-simple');
        this.eValue = this.eGui.querySelector('.my-value');

        // set value into cell
        this.eValue.innerHTML = params.valueFormatted ? params.valueFormatted : params.value;

        // add event listener to button
        this.eventListener = function () {
            console.log('file upload requested!!');
        };
        this.eButton.addEventListener('click', this.eventListener);
    };

    // gets called once when grid ready to insert the element
    btn.prototype.getGui = function () { return this.eGui; };

    // gets called whenever the user gets the cell to refresh
    MyCellRenderer.prototype.refresh = function (params) {
        this.eValue.innerHTML = params.valueFormatted ? params.valueFormatted : params.value;
        return true;
    };

    // gets called when the cell is removed from the grid
    MyCellRenderer.prototype.destroy = function () {
        this.eButton.removeEventListener('click', this.eventListener);
    };
    return btn;
}
//#endregion

//#region helper functions
function allowEdit(params) {
    return !params.node.isRowPinned();
}

function allowSalaryEdit(params) {
    return !params.node.isRowPinned() && !params.node.data.SystemCalculated;
}

function formatNumbers(x) {
    if (x.value !== undefined && x.value != null) {
        var parts = x.value.toString().split(".");
        parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ",");
        return parts.join(".");
    }
}

function numberValueParser(params) {
    if (params != undefined && params != null)
        return Number(params.newValue);
}

function sum(items, prop) {
    return items.reduce(function (a, b) {
        return a + b[prop];
    }, 0);
}

function setGridDeleteFilter(gridApi) {
    try {
        gridApi.setFilterModel({ Delete: { filter: 'false', filterType: 'text', type: 'equals' } });
    }
    catch (exception) { showErrorToast('setGridDeleteFilter: ' + exception); }
}
//#endregion

//#region Lookup Names
function getSalaryHeadName(x) {
    try {
        if (x.value != undefined && x.value != "" && x.value != null) {
            if (x.value == -1) {
                return 'Total Salary';
            }
            else {
                var parentScope = angular.element($("#ngForm")).scope();
                return parentScope.lstSalaryHead.find(sh => { return sh.HeadId == x.value; }).HeadName;
            }
        }
        else
            return '---Select Salary Head---';
    }
    catch (exception) { showErrorToast('getSalaryHeadName: ' + exception); }
}

function getAllocTemplateName(x) {
    try {
        if (x.value != undefined && x.value != "" && x.value != null) {
            if (x.value == -1) {
                return 'Total Allocation';
            }
            else {
                var parentScope = angular.element($("#ngForm")).scope();
                return parentScope.lstAllocTemplate.find(t => { return t.TemplateId == x.value; }).TemplateName;
            }
        }
        else
            return '---Select Allocation Template---';
    }
    catch (exception) { showErrorToast('getAllocTemplateName: ' + exception); }
}

function getBranchName(x) {
    try {
        if (x.value != undefined && x.value != "" && x.value != null) {
            {
                var parentScope = angular.element($("#ngForm")).scope();
                return parentScope.lstBranch.find(b => { return b.BranchCode == x.value; }).BranchName;
            }
        }
        else { return '---Select Branch---'; }
    }
    catch (exception) { showErrorToast('getBranchName: ' + exception); }
}

function getRelationName(x) {
    try {
        if (x.value != undefined && x.value != "" && x.value != null) {
            var parentScope = angular.element($("#ngForm")).scope();
            return parentScope.lstRelation.find(r => { return r.RelationId == x.value; }).RelationName;
        }
        else
            return '---Select Relation---';
    }
    catch (exception) { showErrorToast('getRelationName: ' + exception); }
}

function getEntitlementTypeName(x) {
    try {
        if (x.value != undefined && x.value != "" && x.value != null) {
            var parentScope = angular.element($("#ngForm")).scope();
            return parentScope.lstEntitlementType.find(r => { return r.EntitlementTypeId == x.value; }).EntitlementTypeName;
        }
        else
            return '---Select Entitlement Type---';
    }
    catch (exception) { showErrorToast('getEntitlementTypeName: ' + exception); }
}

function getLeaveTypeName(x) {
    try {
        if (x.value != undefined && x.value != "" && x.value != null) {
            var parentScope = angular.element($("#ngForm")).scope();
            return parentScope.lstLeaveType.find(r => { return r.LeaveTypeId == x.value; }).LeaveTypeName;
        }
        else
            return '---Select Leave Type---';
    }
    catch (exception) { showErrorToast('getLeaveTypeName: ' + exception); }
}

function getAccountName(x) {
    try {
        if (x.value != undefined && x.value != "" && x.value != null) {
            if (x.value == -1) {
                return 'Total';
            }
            else {
                var parentScope = angular.element($("#ngForm")).scope();
                return parentScope.lstAccount.find(t => { return t.AccountId == x.value; }).AccountName;
            }
        }
        else
            return '---Select Account---';
    }
    catch (exception) { showErrorToast('getAccountName: ' + exception); }
}

function getDepartmentName(x) {
    try {
        if (x.value != undefined && x.value != "" && x.value != null) {
            {
                var parentScope = angular.element($("#ngForm")).scope();
                return parentScope.lstDepartment.find(d => { return d.DepartmentCode == x.value; }).DepartmentName;
            }
        }
        else
            return '---Select Department---';
    }
    catch (exception) { showErrorToast('getDepartmentName: ' + exception); }
}

function getWorkerTypeName(x) {
    try {
        if (x.value != undefined && x.value != "" && x.value != null) {
            var parentScope = angular.element($("#ngForm")).scope();
            return parentScope.lstWorkerType.find(r => { return r.WorkerTypeId == x.value; }).WorkerTypeName;
        }
        else
            return '---Select Worker Type---';
    }
    catch (exception) { showErrorToast('getWorkerTypeName: ' + exception); }
}

function getCompanyName(x) {
    try {
        if (x.value != undefined && x.value != "" && x.value != null) {
            if (x.value === -1) {
                return '';
            }
            else {
                var parentScope = angular.element($("#ngForm")).scope();
                return parentScope.lstCompany.find(t => { return t.CompanyId == x.value; }).CompanyName;
            }
        }
        else
            return '---Select Company---';
    }
    catch (exception) { showErrorToast('getCompanyName: ' + exception); }
}

function getDocumentTypeName (x) {
    try {
        if (x.value != undefined && x.value != "" && x.value != null) {
            var parentScope = angular.element($("#ngForm")).scope();
            return parentScope.lstDocumentType.find(dt => { return dt.DocumentTypeId == x.value; }).DocumentTypeName;
        }
        else
            return '---Select Document Type---';
    }
    catch (exception) { showErrorToast('getDocumentTypeName: ' + exception); }
}

function getAttachmentTypeName(x) {
    try {
        if (x.value != undefined && x.value != "" && x.value != null) {
            var parentScope = angular.element($("#ngForm")).scope();
            return parentScope.lstAttachmentType.find(dt => { return dt.TypeId == x.value; }).TypeName;
        }
        else
            return '---Select Attachment Type---';
    }
    catch (exception) { showErrorToast('getAttachmentTypeName: ' + exception); }
}
//#endregion