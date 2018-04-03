

$(document).ready(function () {
    // start load all ajax data, continued by js in product.js file
    $('.processing').show();
    $('#razortemplate').val('AddressAdminList.cshtml');
    nbxget('addressadmin_getlist', '#addressadminsearch', '#datadisplay');

    $(document).on("nbxgetcompleted", AddressAdmin_nbxgetCompleted); // assign a completed event for the ajax calls

    // function to do actions after an ajax call has been made.
    function AddressAdmin_nbxgetCompleted(e) {

        $('.processing').hide();

        if (e.cmd == 'addressadmin_getlist' || e.cmd == 'addressadmin_saveaddress') {

            $('.AddressAdmin_cmdEdit').unbind("click");
            $('.AddressAdmin_cmdEdit').click(function () {
                $('.processing').show();
                $('#razortemplate').val('AddressAdminDetail.cshtml');
                $('#selectedindex').val($(this).attr("index"));                
                nbxget('addressadmin_editaddress', '#addressadminsearch', '#datadisplay');
            });

            $('.AddressAdmin_cmdDel').unbind("click");
            $('.AddressAdmin_cmdDel').click(function () {
                $('.processing').show();
                $('#selectedindex').val($(this).attr("index"));
                nbxget('addressadmin_deleteaddress', '#addressadminsearch', '#datadisplay');
            });

            $('.AddressAdmin_cmdAdd').unbind("click");
            $('.AddressAdmin_cmdAdd').click(function () {
                $('.processing').show();
                nbxget('addressadmin_newaddress', '#addressadminsearch', '#datadisplay');
            });
           
        }


        if (e.cmd == 'addressadmin_editaddress' || e.cmd == 'addressadmin_add') {

            $('.AddressAdmin_cmdCancel').unbind("click");
            $('.AddressAdmin_cmdCancel').click(function () {
                $('.processing').show();
                $('#razortemplate').val('AddressAdminList.cshtml');
                nbxget('addressadmin_getlist', '#addressadminsearch', '#datadisplay');
            });

            $('.AddressAdmin_cmdSave').unbind("click");
            $('.AddressAdmin_cmdSave').click(function () {
                $('.processing').show();
                $('#addrindex').val($(this).attr("index"));
                nbxget('addressadmin_saveaddress', '#addressdata', '#datadisplay');
            });

            $('#country').unbind();
            $('#country').change(function () {
                $('.checkoutbillformregiondiv').hide();
                nbxget('renderpostdata', '.checkoutbillformcountrydiv', '.checkoutbillformregiondiv');
            });

            $('.checkoutbillformregiondiv').unbind();
            $('.checkoutbillformregiondiv').change(function () {
                if ($('#billregion').val() != '') {
                    $('#billaddress_region').val($('#billregion').val());
                    $('#billregion').val('');
                }
                $('.checkoutbillformregiondiv').show();
            });
        }

        if (e.cmd == 'addressadmin_saveaddress' || e.cmd == 'addressadmin_newaddress' || e.cmd == 'addressadmin_deleteaddress') {
            $('.processing').show();
            $('#razortemplate').val('AddressAdminList.cshtml');
            nbxget('addressadmin_getlist', '#addressadminsearch', '#datadisplay');
        }
    };

});

