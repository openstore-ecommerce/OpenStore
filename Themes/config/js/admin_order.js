$(document).ready(function () {
    // start load all ajax data, continued by js in product.js file
    $('.processing').show();

    // if we pass uid as url param, select only that user orders.
    if ($('#useridparam').val() !== '') {
        $('#selecteduserid').val($('#useridparam').val());
    }

    // Check for eid in path & get details, otherwise get list
    var path = window.location.pathname;
    if (path.indexOf("/eid/") > 0) {
        var re = /(\/eid\/)\d+/g;
        var eid = path.match(re) != null ? path.match(re).toString().replace("/eid/", "") : null;
        if (eid != null && !isNaN(eid)) {
            $('#razortemplate').val('Admin_OrdersDetail.cshtml');
            $('#selecteditemid').val(eid);
            nbxget('orderadmin_getdetail', '#orderadminsearch', '#datadisplay');
        }
    }
    else {
        $('#razortemplate').val('Admin_OrdersList.cshtml');
        nbxget('orderadmin_getlist', '#orderadminsearch', '#datadisplay');
    }

    $(document).on("nbxgetcompleted", Admin_Order_nbxgetCompleted); // assign a completed event for the ajax calls

    // function to do actions after an ajax call has been made.
    function Admin_Order_nbxgetCompleted(e) {

        $('.processing').hide();

        //NBS - Tooltips
        $('[data-toggle="tooltip"]').tooltip({
            animation: 'true',
            placement: 'auto top',
            viewport: { selector: '#content', padding: 0 },
            delay: { show: 100, hide: 200 }
        });


        if (e.cmd === 'orderadmin_getlist') {

            $("#OrderAdmin_cmdExport").show();
            $("#OrderAdmin_cmdSave").hide();
            $("#OrderAdmin_cmdReturn").hide();

            $('#OrderAdmin_searchtext').val($('#searchtext').val());
            $('#OrderAdmin_datesearchfrom').val($('#dtesearchdatefrom').val());
            $('#OrderAdmin_datesearchto').val($('#dtesearchdateto').val());
            $('#OrderAdmin_searchorderstatus').val($('#searchorderstatus').val());

            // editbutton created by list, so needs to be assigned on each render of list.
            $('.cmd_vieworder').unbind("click");
            $('.cmd_vieworder').click(function () {
                $('.processing').show();
                $('#razortemplate').val('Admin_OrdersDetail.cshtml');
                $('#selecteditemid').val($(this).attr('itemid'));
                nbxget('orderadmin_getdetail', '#orderadminsearch', '#datadisplay');
            });

            $('.cmd_repeatorder').unbind("click");
            $('.cmd_repeatorder').click(function () {
                $('.processing').show();
                $('#selecteditemid').val($(this).attr('itemid'));
                nbxget('orderadmin_reorder', '#orderadminsearch', '#datadisplay');
            });

            $('.cmdPg').unbind("click");
            $('.cmdPg').click(function () {
                $('.processing').show();
                $('#pagenumber').val($(this).attr('pagenumber'));
                nbxget('orderadmin_getlist', '#orderadminsearch', '#datadisplay');
            });

            $('#OrderAdmin_cmdSearch').unbind("click");
            $('#OrderAdmin_cmdSearch').click(function () {
                $('.processing').show();
                $('#pagenumber').val('1');
                $('#searchtext').val($('#OrderAdmin_searchtext').val());
                $('#dtesearchdatefrom').val($('#OrderAdmin_datesearchfrom').val());
                $('#dtesearchdateto').val($('#OrderAdmin_datesearchto').val());
                $('#searchorderstatus').val($('#OrderAdmin_searchorderstatus').val());
                $('#OrderAdmin_cmdExportDownload').show();
                $('#OrderAdmin_cmdExport').hide();

                nbxget('orderadmin_getlist', '#orderadminsearch', '#datadisplay');
            });

            $('#OrderAdmin_cmdExport').unbind("click");
            $('#OrderAdmin_cmdExport').click(function () {
                $('.processing').show();
                $('#searchtext').val($('#OrderAdmin_searchtext').val());
                $('#dtesearchdatefrom').val($('#OrderAdmin_datesearchfrom').val());
                $('#dtesearchdateto').val($('#OrderAdmin_datesearchto').val());
                $('#searchorderstatus').val($('#OrderAdmin_searchorderstatus').val());
                $('#pagenumber').val('0');
                $('#pagesize').val('0');

                nbxget('orderadmin_getexport', '#orderadminsearch', '#csvfile');
            });

            $('#OrderAdmin_cmdReset').unbind("click");
            $('#OrderAdmin_cmdReset').click(function () {
                $('.processing').show();
                $('#pagenumber').val('1');
                $('#searchtext').val('');
                $('#dtesearchdatefrom').val('');
                $('#dtesearchdateto').val('');
                $('#searchorderstatus').val('');
                $('#OrderAdmin_cmdExportDownload').show();
                $('#OrderAdmin_cmdExport').hide();

                nbxget('orderadmin_getlist', '#orderadminsearch', '#datadisplay');
            });


        }

        if (e.cmd === 'orderadmin_getexport') {
            $('#OrderAdmin_cmdExportDownload').attr('href', '/DesktopModules/NBright/NBrightBuy/XmlConnector.ashx?cmd=docdownload&downloadname=orderlist.csv&filename=' + $('#csvfile').text());
            $('#OrderAdmin_cmdExportDownload').show();
            $('#OrderAdmin_cmdExport').hide();
        }

        if (e.cmd === 'orderadmin_getdetail') {

            $("#OrderAdmin_cmdExport").hide();
            $("#OrderAdmin_cmdSave").show();
            $("#OrderAdmin_cmdReturn").show();

            $('#OrderAdmin_cmdReturn').unbind("click");
            $('#OrderAdmin_cmdReturn').click(function () {
                $('.processing').show();
                $('#razortemplate').val('Admin_OrdersList.cshtml');
                $('#selecteditemid').val('');
                nbxget('orderadmin_getlist', '#orderadminsearch', '#datadisplay');
            });

            $('#OrderAdmin_cmdReOrder').unbind("click");
            $('#OrderAdmin_cmdReOrder').click(function () {
                $('.processing').show();
                $('#selecteditemid').val($(this).attr('itemid'));
                nbxget('orderadmin_reorder', '#orderadminsearch', '#datadisplay');
            });
            $('#OrderAdmin_cmdEditOrder').unbind("click");
            $('#OrderAdmin_cmdEditOrder').click(function () {
                $('.processing').show();
                $('#selecteditemid').val($(this).attr('itemid'));
                nbxget('orderadmin_edit', '#orderadminsearch', '#datadisplay');
            });

            $('.OrderAdmin_cmdSave').unbind("click");
            $('.OrderAdmin_cmdSave').click(function () {
                $('.processing').show();
                nbxget('orderadmin_save', '#orderadmin', '#actionreturn');
            });

            $('#OrderAdmin_cmdDeleteInvoice').unbind("click");
            $('#OrderAdmin_cmdDeleteInvoice').click(function () {
                $('.processing').show();
                nbxget('orderadmin_removeinvoice', '#orderadmin', '#actionreturn');
            });

            $('#OrderAdmin_cmdEmailAmended').unbind("click");
            $('#OrderAdmin_cmdEmailAmended').click(function () {
                if (confirm($('#cmdEmailAmended').val())) {
                    $('.processing').show();
                    $('#emailtype').val("OrderAmended");
                    $('#emailsubject').val("orderamended_emailsubject");
                    $('#emailmessage').val($('#emailmsg').val());
                    nbxget('orderadmin_save', '#orderadmin', '#actionreturn');
                }
            });
            $('#OrderAdmin_cmdEmailValidated').unbind("click");
            $('#OrderAdmin_cmdEmailValidated').click(function () {
                if (confirm($('#cmdEmailValidated').val())) {
                    $('.processing').show();
                    $('#emailtype').val("OrderValidated");
                    $('#emailsubject').val("ordervalidated_emailsubject");
                    $('#emailmessage').val($('#emailmsg').val());
                    nbxget('orderadmin_save', '#orderadmin', '#actionreturn');
                }
            });
            $('#OrderAdmin_cmdEmailShipped').unbind("click");
            $('#OrderAdmin_cmdEmailShipped').click(function () {
                if (confirm($('#cmdEmailShipped').val())) {
                    $('.processing').show();
                    $('#emailtype').val("OrderShipped");
                    $('#emailsubject').val("ordershipped_emailsubject");
                    $('#emailmessage').val($('#emailmsg').val());
                    nbxget('orderadmin_save', '#orderadmin', '#actionreturn');
                }
            });
            $('#OrderAdmin_cmdEmailReceipt').unbind("click");
            $('#OrderAdmin_cmdEmailReceipt').click(function () {
                if (confirm($('#cmdEmailReceipt').val())) {
                    $('.processing').show();
                    $('#emailtype').val("OrderReceipt");
                    $('#emailsubject').val("orderreceipt_emailsubject");
                    $('#emailmessage').val($('#emailmsg').val());
                    nbxget('orderadmin_save', '#orderadmin', '#actionreturn');
                }
            });

            $('#actionreturn').unbind("change");
            $('#actionreturn').change(function () {
                $('.processing').show();
                nbxget('orderadmin_getdetail', '#orderadminsearch', '#datadisplay');
            });


        }

        if (e.cmd === 'orderadmin_removeinvoice') {
            $('.processing').show();
            nbxget('orderadmin_getdetail', '#orderadminsearch', '#datadisplay');
        }

        if (e.cmd === 'orderadmin_save') {
            $('.processing').show();
            if ($('#emailtype').val() !== '') {
                nbxget('orderadmin_sendemail', '#orderadminsearch', '#actionreturn');
            } else {
                nbxget('orderadmin_getdetail', '#orderadminsearch', '#datadisplay');
            }

        }

        if (e.cmd === 'orderadmin_sendemail') {
            $('.processing').hide();
            $('#emailtype').val(''); // clear flag
        }

        if (e.cmd === 'orderadmin_reorder' || e.cmd == 'orderadmin_edit') {
            $('.processing').show();
            window.location = $('#redirecturl').val();
        }

    };

});

