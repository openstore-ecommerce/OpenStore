$(document).ready(function() {

    $('.selectlang').unbind("click");
    $(".selectlang").click(function() {
        $('.actionbuttonwrapper').hide();
        $('.editlanguage').hide();
        $('.processing').show();
        if ($("#razortemplate").val() == 'Admin_CategoryDetail.cshtml') {
            //move data to update postback field
            $("#nextlang").val($(this).attr("editlang"));
            nbxget('category_admin_save', '#categorydatasection', '#actionreturn');
        } else {
            // Need to save list
            $("#editlang").val($(this).attr("editlang"));
            $('#razortemplate').val('Admin_CategoryList.cshtml');
            nbxget('category_admin_getlist', '#categoryadminsearch', '#datadisplay');
        }
    });

    $(document).on("nbxgetcompleted", Admin_category_nbxgetCompleted); // assign a completed event for the ajax calls

    // start load all ajax data, continued by js in product.js file
    $('.processing').show();

    $('#razortemplate').val('Admin_CategoryList.cshtml');
    nbxget('category_admin_getlist', '#categoryadminsearch', '#datadisplay');


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
    function Admin_category_nbxgetCompleted(e) {

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
            nbxget('product_adminaddnew', '#productadminsearch', '#datadisplay');
        });

        if (e.cmd == 'category_categoryproductlist'
            || e.cmd == 'category_selectchangehidden'
            || e.cmd == 'category_copyallcatxref'
            || e.cmd == 'category_moveallcatxref'
            || e.cmd == 'category_admin_savelist'
            || e.cmd == 'category_admin_movecategory'
            || e.cmd == 'category_cattaxupdate') {
            $('.processing').hide();
        };

        if (e.cmd == 'category_admin_getlist'
            || e.cmd == 'category_admin_delete'
            || e.cmd == 'category_admin_movecategory'
            || e.cmd == 'category_admin_addnew') {
            
            $('.processing').hide();

            $("#categoryAdmin_cmdAddNew").show();
            $("#categoryAdmin_cmdSaveList").show();

            $("#categoryAdmin_cmdSaveExit").hide();
            $("#categoryAdmin_cmdSave").hide();
            $("#categoryAdmin_cmdSaveAs").hide();
            $("#categoryAdmin_cmdDelete").hide();
            $("#categoryAdmin_cmdReturn").hide();

            // Move categories
            $(".selectmove").hide();
            $(".selectcancel").hide();
            $(".selectrecord").show();
            $(".savebutton").hide();

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
                $('.processing').show();
                nbxget('category_admin_movecategory', '#categoryadminsearch', '#datadisplay');
            });

            // editbutton created by list, so needs to be assigned on each render of list.
            $('.categoryAdmin_cmdEdit').unbind("click");
            $('.categoryAdmin_cmdEdit').click(function () {
                $('.processing').show();
                $('#razortemplate').val('Admin_CategoryDetail.cshtml');
                $('#selectedcatid').val($(this).attr('itemid'));
                nbxget('category_admin_getdetail', '#categoryadminsearch', '#datadisplay');
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
                nbxget('category_selectchangehidden', '#categoryadminsearch');
            });

            $('.cmdopen').unbind("click");
            $(".cmdopen").click(function () {
                $('.processing').show();
                $('#selectedcatid').val($(this).attr('itemid'));
                if ($('#selectedcatid').val() != $('#returncatid').val()) {
                    $('#returncatid').val($('#selectedcatid').val() + "," + $('#returncatid').val());
                }
                $('#razortemplate').val('Admin_CategoryList.cshtml');
                nbxget('category_admin_getlist', '#categoryadminsearch', '#datadisplay');
            });

            $('.cmdreturn').unbind("click");
            $(".cmdreturn").click(function () {
                $('.processing').show();
                var array = $('#returncatid').val().split(',');
                $('#selectedcatid').val(array[1]);
                $('#returncatid').val($('#returncatid').val().replace(array[0] + ",", ""));
                $('#razortemplate').val('Admin_CategoryList.cshtml');
                nbxget('category_admin_getlist', '#categoryadminsearch', '#datadisplay');
            });

            $('#categoryAdmin_cmdAddNew').unbind("click");
            $("#categoryAdmin_cmdAddNew").click(function () {
                $('.processing').show();
                nbxget('category_admin_addnew', '#categoryadminsearch', '#datadisplay');
            });

            $('.categoryAdmin_cmdDelete').unbind("click");
            $(".categoryAdmin_cmdDelete").click(function () {
                if (confirm($('#confirmdeletemsg').text())) {
                    $('.processing').show();
                    $('#selectedcatid').val($(this).attr('itemid'));
                    nbxget('category_admin_delete', '#categoryadminsearch', '#datadisplay');
                }
            });


            $('#categoryAdmin_cmdSaveList').unbind("click");
            $("#categoryAdmin_cmdSaveList").click(function () {
                $('.processing').show();
                nbxget('category_admin_savelist', '.categoryfields', '', '.categoryitemfields');
            });

            $('.categorynametextbox').unbind("change");
            $(".categorynametextbox").change(function () {
                $('#isdirty_' + $(this).attr('lp')).val('True');
            });
            
        }


        if (e.cmd == 'category_displayproductselect') {
            setupbackoffice(); // run JS to deal with standard BO functions like accordian.   NOTE: Select2 can only be assigned 1 time.
        };

        if (e.cmd == 'category_admin_getdetail'
            || e.cmd == 'category_removeimage'
            || e.cmd == 'category_updateimages') {            

            setupbackoffice(); // run JS to deal with standard BO functions like accordian.   NOTE: Select2 can only be assigned 1 time.

            $('.actionbuttonwrapper').show();        

            $("#categoryAdmin_cmdAddNew").hide();
            $("#categoryAdmin_cmdSaveList").hide();

            $("#categoryAdmin_cmdSaveExit").show();
            $("#categoryAdmin_cmdSave").show();
            $("#categoryAdmin_cmdDelete").show();
            $("#categoryAdmin_cmdReturn").show();

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
                            nbxget('category_updateimages', '#categoryadminsearch', '#datadisplay'); // load images
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
                nbxget('category_removeimage', '#categoryadminsearch', '#datadisplay');
            });
            

            // ---------------------------------------------------------------------------
            // ACTION BUTTONS
            // ---------------------------------------------------------------------------
            $('#categoryAdmin_cmdReturn').unbind("click");
            $('#categoryAdmin_cmdReturn').click(function () {
                $('.processing').show();
                var array = $('#returncatid').val().split(',');
                $('#selectedcatid').val(array[0]);
                $('#razortemplate').val('Admin_CategoryList.cshtml');
                nbxget('category_admin_getlist', '#categoryadminsearch', '#datadisplay');
            });
            
            $('#categoryAdmin_cmdSave').unbind("click");
            $('#categoryAdmin_cmdSave').click(function () {
                $('.processing').show();
                nbxget('category_admin_save', '#categorydatasection');
            });

            $('#categoryAdmin_cmdSaveExit').unbind("click");
            $('#categoryAdmin_cmdSaveExit').click(function () {
                $('.processing').show();
                nbxget('category_admin_saveexit', '#categorydatasection');
            });


            $('#categoryAdmin_cmdDelete').unbind("click");
            $('#categoryAdmin_cmdDelete').click(function () {
                if (confirm($('#confirmdeletemsg').text())) {
                    $('.processing').show();
                    nbxget('category_admin_delete', '#categoryadminsearch', '#datadisplay');                    
                }
            });



            // ---------------------------------------------------------------------------
            // Product Select
            // ---------------------------------------------------------------------------
            $('#productselect').unbind();
            $('#productselect').click(function () {
                $('.processing').show();
                $('#razortemplate').val('Admin_CategoryProductSelect.cshtml');
                nbxget('category_displayproductselect', '#categoryadminsearch', '#datadisplay');
            });


            // remove single product
            $('.removeproduct').unbind();
            $('.removeproduct').click(function () {
                $('#selectproductid').val($(this).attr('itemid'));
                $('.productid' + $(this).attr('itemid')).hide();
                nbxget('category_deletecatxref', '#categoryadminsearch');
            });

            $('#removeall').unbind();
            $('#removeall').click(function () {
                if (confirm($('#confirmmsg').html())) {
                    $('#productlist').hide();
                    nbxget('category_deleteallcatxref', '#categoryadminsearch');
                }
            });

            $('select[id*="selectcatid"]').change(function () {
                $('input[id*="newcatid"]').val($(this).val());
            });

            $('#copyto').unbind();
            $('#copyto').click(function () {
                if (confirm($('#confirmmsg').html())) {
                    $('.processing').show();
                    $('#selectproductid').val($(this).attr('itemid'));
                    nbxget('category_copyallcatxref', '#categoryadminsearch', '#nbsnotify');
                }
            });

            $('#moveto').unbind();
            $('#moveto').click(function () {
                if (confirm($('#confirmmsg').html())) {
                    $('.processing').show();
                    $('#productlist').hide();
                    nbxget('category_moveallcatxref', '#categoryadminsearch', '#nbsnotify');
                }
            });


            $('#producttax').unbind();
            $('#producttax').click(function () {
                $('#selecttaxrate').val($('select[id*=taxrate]').val());
                $('.processing').show();
                nbxget('category_cattaxupdate', '#categoryadminsearch', '#nbsnotify');
            });

            // ---------------------------------------------------------------------------
            // Group Filters
            // ---------------------------------------------------------------------------
            $('#selectgrouptype').unbind();
            $('#selectgrouptype').click(function () {
                $('.processing').show();
                $('#selectedgroupref').val($(this).val());
                if ($(this).val() != null) nbxget('category_addgroupfilter', '#categoryadminsearch', '.filtergroupsection');
            });

            $('.removegroupcategory').unbind();
            $('.removegroupcategory').click(function () {
                $('.processing').show();
                $('#selectedgroupid').val($(this).attr("groupid"));
                nbxget('category_removegroupfilter', '#categoryadminsearch', '.filtergroupsection');
            });

            $('.processing').hide();

        }
        if (e.cmd == 'category_removegroupfilter'
            || e.cmd == 'category_addgroupfilter') {

            $('.removegroupcategory').unbind();
            $('.removegroupcategory').click(function () {
                $('.processing').show();
                $('#selectedgroupid').val($(this).attr("groupid"));
                nbxget('category_removegroupfilter', '#categoryadminsearch', '.filtergroupsection');
            });

            $('.processing').hide();
        }

        if (e.cmd == 'category_admin_save') {
            $('.processing').show();
            $('#editlang').val($('#nextlang').val());
            $('#razortemplate').val('Admin_CategoryDetail.cshtml');
            nbxget('category_admin_getdetail', '#categoryadminsearch', '#datadisplay');
        }
        if (e.cmd == 'category_admin_saveexit'
            || e.cmd == 'category_admin_delete') {
            $('.processing').show();
            var array = $('#returncatid').val().split(',');
            $('#selectedcatid').val(array[0]);
            $('#razortemplate').val('Admin_CategoryList.cshtml');
            nbxget('category_admin_getlist', '#categoryadminsearch', '#datadisplay');
        }

        // ---------------------------------------------------------------------------
        // Product Select
        // ---------------------------------------------------------------------------
        if (e.cmd == 'category_getproductselectlist') {
            $('.processing').hide();

            $("#categoryAdmin_cmdSaveExit").hide();
            $("#categoryAdmin_cmdSave").hide();
            $("#categoryAdmin_cmdDelete").hide();
            $("#categoryAdmin_cmdReturn").hide();

            $('#returnfromselect').click(function () {
                $("#searchtext").val('');
                $("#searchcategory").val('');
                $('#razortemplate').val('Admin_CategoryDetail.cshtml');
                nbxget('category_admin_getdetail', '#categoryadminsearch', '#datadisplay');
                $('#datadisplay').show();
            });

            $('#selectsearch').unbind("click");
            $('#selectsearch').click(function () {
                $('.processing').show();
                $('#pagenumber').val('1');
                $('#searchtext').val($('#txtproductselectsearch').val());
                $('#searchcategory').val($('#ddlsearchcategory').val());
                nbxget('category_getproductselectlist', '#categoryadminsearch', '#productselectlist');
            });

            $('#selectreset').unbind("click");
            $('#selectreset').click(function () {
                $('.processing').show();
                $('#pagenumber').val('1');
                $('#searchtext').val('');
                $("#searchcategory").val('');
                $('#txtproductselectsearch').val('');
                nbxget('category_getproductselectlist', '#categoryadminsearch', '#productselectlist');
            });

            $('.cmdPg').unbind("click");
            $('.cmdPg').click(function () {
                $('.processing').show();
                $('#pagenumber').val($(this).attr('pagenumber'));
                nbxget('category_getproductselectlist', '#categoryadminsearch', '#productselectlist');
            });

            // select product
            $('.selectproduct').unbind();
            $('.selectproduct').click(function () {
                $('.selectproductid' + $(this).attr('itemid')).hide();
                $('#selectproductid').val($(this).attr('itemid'));
                nbxget('category_selectcatxref', '#categoryadminsearch'); 
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

