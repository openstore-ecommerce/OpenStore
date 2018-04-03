$(document).ready(function() {

    $(document).on("nbxgetcompleted", Admin_plugins_nbxgetCompleted); // assign a completed event for the ajax calls

    // start load all ajax data, continued by js in plugins.js file
    $('.processing').show();

    $('#razortemplate').val('Admin_PluginsList.cshtml');
    nbxget('plugins_admin_getlist', '#pluginsadminsearch', '#datadisplay');

    // function to do actions after an ajax call has been made.
    function Admin_plugins_nbxgetCompleted(e) {

        $('.actionbuttonwrapper').show();
        $('.editlanguage').show();

        setupbackoffice(); // run JS to deal with standard BO functions like accordian.


        //NBS - Tooltips
        $('[data-toggle="tooltip"]').tooltip({
            animation: 'true',
            placement: 'auto top',
            viewport: { selector: '#content', padding: 0 },
            delay: { show: 100, hide: 200 }
        });

        $('#pluginsAdmin_cmdAddNew').unbind("click");
        $('#pluginsAdmin_cmdAddNew').click(function () {
            $('.processing').show();
            $('#razortemplate').val('Admin_pluginsDetail.cshtml');
            nbxget('plugins_adminaddnew', '#pluginsadminsearch', '#datadisplay');
        });

        if (e.cmd == 'plugins_admin_getlist' || e.cmd == 'plugins_admin_delete') {

            $('.processing').hide();

            $('.pluginssearchpanel').show();

            $("#pluginsAdmin_cmdSave").hide();
            $("#pluginsAdmin_cmdDelete").hide();
            $("#pluginsAdmin_cmdReturn").hide();
            $("#pluginsAdmin_cmdAddNew").show();

            // Move pluginss
            $(".selectmove").hide();
            $(".selectcancel").hide();
            $(".selectrecord").hide();
            $(".savebutton").hide();

            if ($('#searchcategory').val() != '') {
                $(".selectrecord").show();
            }


            $('.selectrecord').unbind("click");
            $(".selectrecord").click(function() {
                $(".selectrecord").hide();
                $(".selectmove").show();
                $(".selectmove[itemid='" + $(this).attr("itemid") + "']").hide();
                $(".selectcancel[itemid='" + $(this).attr("itemid") + "']").show();
                $("#movepluginsid").val($(this).attr("itemid"));

                var selectid = $(this).attr("itemid");
                $(".selectmove").each(function(index) {
                    if ($(this).attr("parentlist").indexOf(selectid + ";") > -1) $(this).hide();
                });

            });

            $('.selectcancel').unbind("click");
            $(".selectcancel").click(function() {
                $(".selectmove").hide();
                $(".selectcancel").hide();
                $(".selectrecord").show();
            });

            $('.selectmove').unbind("click");
            $(".selectmove").click(function() {
                $(".selectmove").hide();
                $(".selectcancel").hide();
                $(".selectrecord").show();
                $("#movetopluginsid").val($(this).attr("itemid"));
                nbxget('plugins_movepluginsadmin', '#pluginsadminsearch', '#datadisplay');
            });

            // editbutton created by list, so needs to be assigned on each render of list.
            $('.pluginsAdmin_cmdEdit').unbind("click");
            $('.pluginsAdmin_cmdEdit').click(function() {
                $('.processing').show();
                $('#razortemplate').val('Admin_PluginsDetail.cshtml');
                $('#selecteditemid').val($(this).attr('itemid'));
                nbxget('plugins_admin_getdetail', '#pluginsadminsearch', '#datadisplay');
            });


            $('.selectchangeactive').unbind("click");
            $('.selectchangeactive').click(function () {
                $('.processing').show();
                $('#selecteditemid').val($(this).attr('itemid'));
                if ($(this).hasClass("fa-check-circle")) {
                    $(this).addClass('fa-circle').removeClass('fa-check-circle');
                } else {
                    $(this).addClass('fa-check-circle').removeClass('fa-circle');
                }
                nbxget('plugins_selectchangeactive', '#pluginsadminsearch');
            });

        }

        if (e.cmd == 'plugins_admin_save') {
            $("#editlang").val($("#nextlang").val());
            $("#editlanguage").val($("#nextlang").val());
            nbxget('plugins_admin_getdetail', '#pluginsadminsearch', '#datadisplay');
        };


        if (e.cmd == 'plugins_movepluginsadmin') {
            $('#razortemplate').val('Admin_pluginsList.cshtml');
            $('#selecteditemid').val('');
            nbxget('plugins_admin_getlist', '#pluginsadminsearch', '#datadisplay');
        };
        
        if (e.cmd == 'plugins_getpluginsselectlist') {
            $('.processing').hide();
        };

        if (e.cmd == 'plugins_admin_getdetail'
            || e.cmd == 'plugins_addpluginsmodels'
            || e.cmd == 'plugins_adminaddnew') {

            // Copy the pluginsid into the selecteditemid (for Add New plugins)
            $('#selecteditemid').val($('#itemid').val());

            $('.actionbuttonwrapper').show();

            $('.processing').hide();

            $('.pluginssearchpanel').hide();
            
            $("#pluginsAdmin_cmdSave").show();
            $("#pluginsAdmin_cmdDelete").show();
            $("#pluginsAdmin_cmdReturn").show();
            $("#pluginsAdmin_cmdAddNew").hide();

            $('#datadisplay').children().find('.sortelementUp').click(function () { moveUp($(this).parent()); });
            $('#datadisplay').children().find('.sortelementDown').click(function () { moveDown($(this).parent()); });


            // ---------------------------------------------------------------------------
            // ACTION BUTTONS
            // ---------------------------------------------------------------------------
            $('#pluginsAdmin_cmdReturn').unbind("click");
            $('#pluginsAdmin_cmdReturn').click(function () {
                $('.processing').show();
                $('#razortemplate').val('Admin_pluginsList.cshtml');
                $('#selecteditemid').val('');
                nbxget('plugins_admin_getlist', '#pluginsadminsearch', '#datadisplay');
            });
            
            $('#pluginsAdmin_cmdSave').unbind("click");
            $('#pluginsAdmin_cmdSave').click(function () {
                $('.actionbuttonwrapper').hide();
                $('.editlanguage').hide();
                $('.processing').show();
                //move data to update postback field
                $('#xmlupdatemodeldata').val($.fn.genxmlajaxitems('#pluginsmodels', '.modelitem'));
                nbxget('plugins_admin_save', '#pluginsdatasection', '#actionreturn');
            });

            $('#pluginsAdmin_cmdDelete').unbind("click");
            $('#pluginsAdmin_cmdDelete').click(function () {
                if (confirm($('#confirmresetmsg').text())) {
                    $('.actionbuttonwrapper').hide();
                    $('.editlanguage').hide();
                    $('.processing').show();
                    $('#razortemplate').val('Admin_pluginsList.cshtml');
                    $('#selecteditemid').val('');
                    nbxget('plugins_admin_delete', '#pluginsdatasection', '#datadisplay');
                }
            });


            // ---------------------------------------------------------------------------
            // MODELS
            // ---------------------------------------------------------------------------
            $('.removemodel').unbind("click");
            $('.removemodel').click(function () { removeelement($(this).parent().parent().parent().parent()); });

            $('input[id*="availabledate"]').datepicker();

            //Add models
            $('#addmodels').unbind("click");
            $('#addmodels').click(function () {
                $('.processing').show();
                nbxget('plugins_addpluginsmodels', '#pluginsadminsearch', '#datadisplay'); // load models
            });

            $('#undomodel').unbind("click");
            $('#undomodel').click(function() {
                 undoremove('.modelitem', '#pluginsmodels');
            });
            $('.chkdisabledealer:checked').each(function (index) {
                $(this).prev().attr("disabled", "disabled");;
                $(this).parent().next().find('.dealersale').attr("disabled", "disabled");
            });
            $('.chkdisabledealer').unbind("change");
            $('.chkdisabledealer').change(function () {
                if ($(this).is(":checked")) {
                    $(this).prev().attr("disabled", "disabled");
                    $(this).parent().next().find('.dealersale').attr("disabled", "disabled");
                    $(this).prev().val(0);
                    $(this).parent().next().find('.dealersale').val(0);
                } else {
                    $(this).prev().removeAttr("disabled");
                    $(this).parent().next().find('.dealersale').removeAttr("disabled");
                }
            });

            $('.chkdisablesale:checked').each(function (index) {
                $(this).prev().attr("disabled", "disabled");;
            });
            $('.chkdisablesale').unbind("change");
            $('.chkdisablesale').change(function () {
                if ($(this).is(":checked")) {
                    $(this).prev().attr("disabled", "disabled");;
                    $(this).prev().val(0);
                } else {
                    $(this).prev().removeAttr("disabled");
                }
            });

    


        }

    };

    // ---------------------------------------------------------------------------
    // FUNCTIONS
    // ---------------------------------------------------------------------------
    function moveUp(item) {
        var prev = item.prev();
        if (prev.length == 0)
            return;
        prev.css('z-index', 999).css('position', 'relative').animate({ top: item.height() }, 250);
        item.css('z-index', 1000).css('position', 'relative').animate({ top: '-' + prev.height() }, 300, function () {
            prev.css('z-index', '').css('top', '').css('position', '');
            item.css('z-index', '').css('top', '').css('position', '');
            item.insertBefore(prev);
        });
    }
    function moveDown(item) {
        var next = item.next();
        if (next.length == 0)
            return;
        next.css('z-index', 999).css('position', 'relative').animate({ top: '-' + item.height() }, 250);
        item.css('z-index', 1000).css('position', 'relative').animate({ top: next.height() }, 300, function () {
            next.css('z-index', '').css('top', '').css('position', '');
            item.css('z-index', '').css('top', '').css('position', '');
            item.insertAfter(next);
        });
    }

    function removeelement(elementtoberemoved) {
        if ($('#recyclebin').length > 0) {
            $('#recyclebin').append($(elementtoberemoved));
        } else { $(elementtoberemoved).remove(); }
        if ($(elementtoberemoved).hasClass('modelitem')) $('#undomodel').show();
    }
    function undoremove(itemselector, destinationselector) {
        if ($('#recyclebin').length > 0) {
            $(destinationselector).append($('#recyclebin').find(itemselector).last());
        }
        if ($('#recyclebin').children(itemselector).length == 0) {
            if (itemselector == '.modelitem') $('#undomodel').hide();
        }
    }


    // ---------------------------------------------------------------------------

});

