function isNumberKey(evt) {
    var charCode = (evt.which) ? evt.which : event.keyCode;
    if ((charCode < 48 || charCode > 57)
        && charCode != 8 && charCode != 13 && charCode != 44) {
        return false;
    }
    else {
        return true;
    }
}

function isDecimalKey(evt, value) {
    var charCode = (evt.which) ? evt.which : event.keyCode;
    if ((charCode < 48 || charCode > 57)
        && charCode != 8 && charCode != 13 && charCode != 44 && charCode != 46) {
        return false;
    }
    else
      
        return true;
}


 function check (e, value) {
    if (!e.target.validity.valid) {
        e.target.value = value.substring(0, value.length - 1);
        return false;
    }
    var idx = value.indexOf('.');
    if (idx >= 0) {
        if (value.length - idx > 3) {
            e.target.value = value.substring(0, value.length - 1);
            return false;
        }
    }
    
    return true;
 }

 function Decimal2Places(evt, value) {
     var v = parseFloat(value);
     evt.target.value = (isNaN(v)) ? '' : v.toFixed(2);
     return true;
 }





 function date(e, value) {
     
     try {
         $.datepicker.parseDate('dd-M-y', value);
     }
     catch (err) {
         e.target.value = "";
         return false;
     }
     //e.target.text = value;
     return true;
 }







 function time(e, value) {

     try {
         $.datepicker.parseDate('hh:mm tt', value);
     }
     catch (err) {
         e.target.value = "";
         return false;
     }
     //e.target.text = value;
     return true;
 }


//function isPercentageKey(evt) {
//    var charCode = (evt.which) ? evt.which : event.keyCode;
//    if ((charCode < 48 || charCode > 57)
//         && charCode != 8 && charCode != 13 && charCode != 44 && charCode != 46) {
//        return false;        
//    }

//        var str = String.fromCharCode(charCode);
//        if (str > 100) {
//            return false;
//        }     
    
//return true;
//}




function isText(evt) {
    //$("#user").keypress(function (event) {
        var ew = event.which;
        if (ew == 32)
            return true;
        if (65 <= ew && ew <= 90)
            return true;
        if (97 <= ew && ew <= 122)
            return true;
        return false;
   // });
}

function phonenumber(e, value)
{
    var check = "/^(?([0-9]{3}))?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$/";
    if (value.value.match(check))
    {
        return true;
    }
    else
    {
        e.target.value = "";
        return false;
    }
}