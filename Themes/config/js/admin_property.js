$(document).ready(function() {

    $('.selectlang').unbind("click");
    $(".selectlang").click(function() {
        $('.actionbuttonwrapper').hide();
        $('.editlanguage').hide();
        $('.processing').show();
        if ($("#razortemplate").val() == 'Admin_PropertyDetail.cshtml') {
            //move data to update postback field
            $("#nextlang").val($(this).attr("editlang"));
            nbxget('property_admin_save', '#propertydatasection', '#actionreturn');
        } else {
            // Need to save list
            $("#nextlang").val('');
            $("#editlang").val($(this).attr("editlang"));
            $('#razortemplate').val('Admin_PropertyList.cshtml');
            nbxget('property_admin_getlist', '#propertyadminsearch', '#datadisplay');
        }
    });

    $(document).on("nbxgetcompleted", Admin_property_nbxgetCompleted); // assign a completed event for the ajax calls

        // start load all ajax data, continued by js in product.js file
        $('.processing').show();

        
        $('#selectedgroup').val($('#groupsel').val());
        $('#razortemplate').val('Admin_PropertyList.cshtml');
        nbxget('property_admin_getlist', '#propertyadminsearch', '#datadisplay');


        var $container = $('#selectlistwrapper').masonry({
            columnWidth: 120, // List item width - Can also use CSS width of first list item
            itemSelector: '.productlistitem',
            gutter: 6, // Set horizontal gap and include in calculations. Also used in CSS for vertical gap
            isOriginLeft: true, // Build from right to left if false
            isOriginTop: true // Build from bottom to top if false
        });

        // initialize Masonry after all images have loaded - Webkit needs this when image containers don't have a fixed height
        $container.imagesLoaded(function () {
            $container.masonry();
        });


        // function to do actions after an ajax call has been made.
        function Admin_property_nbxgetCompleted(e) {

            $('.actionbuttonwrapper').show();
            $('.editlanguage').show();


            //NBS - Tooltips
            $('[data-toggle="tooltip"]').tooltip({
                animation: 'true',
                placement: 'auto top',
                viewport: { selector: '#content', padding: 0 },
                delay: { show: 100, hide: 200 }
            });

            $('#productAdmin_cmdAddNew').unbind("click");
            $('#productAdmin_cmdAddNew').click(function () {
                $('.processing').show();
                $('#razortemplate').val('Admin_ProductDetail.cshtml');
                nbxget('property_adminaddnew', '#productadminsearch', '#datadisplay');
            });

            if (e.cmd == 'property_categoryproductlist'
                || e.cmd == 'property_selectchangehidden'
                || e.cmd == 'property_copyallcatxref'
                || e.cmd == 'property_moveallcatxref'
                || e.cmd == 'property_cattaxupdate'
                || e.cmd == 'property_admin_savelist') {
                $('.processing').hide();
            };

            if (e.cmd == 'property_admin_getlist'
                || e.cmd == 'property_admin_delete'
                || e.cmd == 'property_admin_movecategory'
                || e.cmd == 'property_admin_addnew') {
            
                $('.processing').hide();

                $("#propertyAdmin_cmdAddNew").show();
                $("#propertyAdmin_cmdSaveList").show();

                $("#propertyAdmin_cmdSaveExit").hide();
                $("#propertyAdmin_cmdSave").hide();
                $("#propertyAdmin_cmdSaveAs").hide();
                $("#propertyAdmin_cmdDelete").hide();
                $("#propertyAdmin_cmdReturn").hide();

                // Move products
                $(".selectmove").hide();
                $(".selectcancel").hide();
                $(".selectrecord").show();
                $(".savebutton").hide();


                $('#groupsel').unbind("change");
                $("#groupsel").change(function () {
                    $('.processing').show();
                    $('#selectedgroup').val($(this).val());
                    nbxget('property_admin_getlist', '#propertyadminsearch', '#datadisplay');
                });

                $('.selectrecord').unbind("click");
                $(".selectrecord").click(function() {
                    $(".selectrecord").hide();
                    $(".selectmove").show();
                    $(".selectmove[itemid='" + $(this).attr("itemid") + "']").hide();
                    $(".selectcancel[itemid='" + $(this).attr("itemid") + "']").show();
                    $("#movecatid").val($(this).attr("itemid"));

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
                    $("#movetocatid").val($(this).attr("itemid"));
                    nbxget('property_admin_movecategory', '#propertyadminsearch', '#datadisplay');
                });

                // editbutton created by list, so needs to be assigned on each render of list.
                $('.propertyAdmin_cmdEdit').unbind("click");
                $('.propertyAdmin_cmdEdit').click(function () {
                    $('.processing').show();
                    $('.groupselpanel').hide();
                    $('#razortemplate').val('Admin_PropertyDetail.cshtml');
                    $('#selectedcatid').val($(this).attr('itemid'));
                    nbxget('property_admin_getdetail', '#propertyadminsearch', '#datadisplay');
                });


                $('.selectchangehidden').unbind("click");
                $('.selectchangehidden').click(function () {
                    $('.processing').show();
                    $('#selectedcatid').val($(this).attr('itemid'));
                    if ($(this).hasClass("fa-check-circle")) {
                        $(this).addClass('fa-circle').removeClass('fa-check-circle');
                    } else {
                        $(this).addClass('fa-check-circle').removeClass('fa-circle');
                    }
                    nbxget('property_selectchangehidden', '#propertyadminsearch');
                });

                $('#propertyAdmin_cmdAddNew').unbind("click");
                $("#propertyAdmin_cmdAddNew").click(function () {
                    $('.processing').show();
                    nbxget('property_admin_addnew', '#propertyadminsearch', '#datadisplay');
                });

                $('.propertyAdmin_cmdDelete').unbind("click");
                $(".propertyAdmin_cmdDelete").click(function () {
                    if (confirm($('#confirmdeletemsg').text())) {
                        $('.processing').show();
                        $('#selectedcatid').val($(this).attr('itemid'));
                        nbxget('property_admin_delete', '#propertyadminsearch', '#datadisplay');
                    }
                });


                $('#propertyAdmin_cmdSaveList').unbind("click");
                $("#propertyAdmin_cmdSaveList").click(function () {
                    $('.processing').show();
                    nbxget('property_admin_savelist', '.propertyfields', '', '.propertyitemfields');
                });

                $('.categorynametextbox').unbind("change");
                $(".categorynametextbox").change(function () {
                    $('#isdirty_' + $(this).attr('lp')).val('True');
                });
                $('#propertyref').unbind("change");
                $("#propertyref").change(function () {
                    $('#isdirty_' + $(this).attr('lp')).val('True');
                });

            }


            if (e.cmd == 'property_displayproductselect') {
                setupbackoffice(); // run JS to deal with standard BO functions like accordian.   NOTE: Select2 can only be assigned 1 time.
            };

            if (e.cmd == 'property_admin_getdetail'
                || e.cmd == 'property_removeimage'
                || e.cmd == 'property_updateimages') {

                setupbackoffice(); // run JS to deal with standard BO functions like accordian.   NOTE: Select2 can only be assigned 1 time.

                $('.actionbuttonwrapper').show();        

                $("#propertyAdmin_cmdAddNew").hide();
                $("#propertyAdmin_cmdSaveList").hide();

                $("#propertyAdmin_cmdSaveExit").show();
                $("#propertyAdmin_cmdSave").show();
                $("#propertyAdmin_cmdDelete").show();
                $("#propertyAdmin_cmdReturn").show();

                $('#datadisplay').children().find('.sortelementUp').click(function () { moveUp($(this).parent()); });
                $('#datadisplay').children().find('.sortelementDown').click(function () { moveDown($(this).parent()); });


                // ---------------------------------------------------------------------------
                // FILE UPLOAD
                // ---------------------------------------------------------------------------
                var filecount = 0;
                var filesdone = 0;
                $(function () {
                    'use strict';
                    var url = '/DesktopModules/NBright/NBrightBuy/XmlConnector.ashx?cmd=fileupload';
                    $('#fileupload').unbind('fileupload');
                    $('#fileupload').fileupload({
                        url: url,
                        maxFileSize: 5000000,
                        acceptFileTypes: /(\.|\/)(gif|jpe?g|png)$/i,
                        dataType: 'json'
                    }).prop('disabled', !$.support.fileInput).parent().addClass($.support.fileInput ? undefined : 'disabled')
                        .bind('fileuploadprogressall', function (e, data) {
                            var progress = parseInt(data.loaded / data.total * 100, 10);
                            $('#progress .progress-bar').css('width', progress + '%');
                        })
                        .bind('fileuploadadd', function (e, data) {
                            $.each(data.files, function (index, file) {
                                $('input[id*="imguploadlist"]').val($('input[id*="imguploadlist"]').val() + file.name + ',');
                                filesdone = filesdone + 1;
                            });
                        })
                        .bind('fileuploadchange', function (e, data) {
                            filecount = data.files.length;
                            $('.processing').show();
                        })
                        .bind('fileuploaddrop', function (e, data) {
                            filecount = data.files.length;
                            $('.processing').show();
                        })
                        .bind('fileuploadstop', function (e) {
                            if (filesdone == filecount) {
                                nbxget('property_updateimages', '#propertyadminsearch', '#datadisplay'); // load images
                                filesdone = 0;
                                $('input[id*="imguploadlist"]').val('');
                                $('.processing').hide();
                                $('#progress .progress-bar').css('width', '0');
                            }
                        });
                });

                $('#removeimage').unbind("click");
                $('#removeimage').click(function () {
                    $('.processing').show();
                    nbxget('property_removeimage', '#propertyadminsearch', '#datadisplay');
                });
            

                // ---------------------------------------------------------------------------
                // ACTION BUTTONS
                // ---------------------------------------------------------------------------
                $('#propertyAdmin_cmdReturn').unbind("click");
                $('#propertyAdmin_cmdReturn').click(function () {
                    $('.processing').show();
                    $('.groupselpanel').show();
                    $('#selectedcatid').val('');
                    $('#razortemplate').val('Admin_PropertyList.cshtml');
                    nbxget('property_admin_getlist', '#propertyadminsearch', '#datadisplay');
                });
            
                $('#propertyAdmin_cmdSave').unbind("click");
                $('#propertyAdmin_cmdSave').click(function () {
                    $('.processing').show();
                    nbxget('property_admin_save', '#propertydatasection');
                });

                $('#propertyAdmin_cmdSaveExit').unbind("click");
                $('#propertyAdmin_cmdSaveExit').click(function () {
                    $('.processing').show();
                    nbxget('property_admin_saveexit', '#propertydatasection');
                });


                $('#propertyAdmin_cmdDelete').unbind("click");
                $('#propertyAdmin_cmdDelete').click(function () {
                    if (confirm($('#confirmdeletemsg').text())) {
                        $('.processing').show();
                        nbxget('property_admin_delete', '#propertyadminsearch', '#datadisplay');
                    }
                });



                // ---------------------------------------------------------------------------
                // Product Select
                // ---------------------------------------------------------------------------
                $('#productselect').unbind();
                $('#productselect').click(function () {
                    $('.processing').show();
                    $('#razortemplate').val('Admin_PropertyProductSelect.cshtml');
                    nbxget('property_displayproductselect', '#propertyadminsearch', '#datadisplay');
                });


                // remove single product
                $('.removeproduct').unbind();
                $('.removeproduct').click(function () {
                    $('#selectproductid').val($(this).attr('itemid'));
                    $('.productid' + $(this).attr('itemid')).hide();
                    nbxget('property_deletecatxref', '#propertyadminsearch');
                });

                $('#removeall').unbind();
                $('#removeall').click(function () {
                    if (confirm($('#confirmmsg').html())) {
                        $('#productlist').hide();
                        nbxget('property_deleteallcatxref', '#propertyadminsearch');
                    }
                });

                $('select[id*="selectcatid"]').change(function () {
                    $('input[id*="newcatid"]').val($(this).val());
                });


                $('.processing').hide();

            }

            if (e.cmd == 'property_admin_save') {
                $('.processing').show();
                $('#editlang').val($('#nextlang').val());
                $("#nextlang").val('');
                $('#razortemplate').val('Admin_PropertyDetail.cshtml');
                nbxget('property_admin_getdetail', '#propertyadminsearch', '#datadisplay');
            }
            if (e.cmd == 'property_admin_saveexit'
                || e.cmd == 'property_admin_delete') {
                $('.processing').show();
                $('.groupselpanel').show();
                $('#selectedcatid').val('');
                $('#razortemplate').val('Admin_PropertyList.cshtml');
                nbxget('property_admin_getlist', '#propertyadminsearch', '#datadisplay');
            }

            // ---------------------------------------------------------------------------
            // Product Select
            // ---------------------------------------------------------------------------
            if (e.cmd == 'property_getproductselectlist') {
                $('.processing').hide();

                $("#propertyAdmin_cmdSaveExit").hide();
                $("#propertyAdmin_cmdSave").hide();
                $("#propertyAdmin_cmdDelete").hide();
                $("#propertyAdmin_cmdReturn").hide();

                $('#returnfromselect').click(function () {
                    $("#searchtext").val('');
                    $("#searchcategory").val('');
                    $('#razortemplate').val('Admin_PropertyDetail.cshtml');
                    nbxget('property_admin_getdetail', '#propertyadminsearch', '#datadisplay');
                    $('#datadisplay').show();
                });

                $('#selectsearch').unbind("click");
                $('#selectsearch').click(function () {
                    $('.processing').show();
                    $('#pagenumber').val('1');
                    $('#searchtext').val($('#txtproductselectsearch').val());
                    $('#searchcategory').val($('#ddlsearchcategory').val());
                    nbxget('property_getproductselectlist', '#propertyadminsearch', '#productselectlist');
                });

                $('#selectreset').unbind("click");
                $('#selectreset').click(function () {
                    $('.processing').show();
                    $('#pagenumber').val('1');
                    $('#searchtext').val('');
                    $("#searchcategory").val('');
                    $('#txtproductselectsearch').val('');
                    nbxget('property_getproductselectlist', '#propertyadminsearch', '#productselectlist');
                });

                $('.cmdPg').unbind("click");
                $('.cmdPg').click(function () {
                    $('.processing').show();
                    $('#pagenumber').val($(this).attr('pagenumber'));
                    nbxget('property_getproductselectlist', '#propertyadminsearch', '#productselectlist');
                });

                // select product
                $('.selectproduct').unbind();
                $('.selectproduct').click(function () {
                    $('.selectproductid' + $(this).attr('itemid')).hide();
                    $('#selectproductid').val($(this).attr('itemid'));
                    nbxget('property_selectcatxref', '#propertyadminsearch');
                });

            };

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

        // ---------------------------------------------------------------------------

    });

