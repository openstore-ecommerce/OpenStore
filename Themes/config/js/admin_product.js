$(document).ready(function() {

    $('.selectlang').unbind("click");
    $(".selectlang").click(function() {
        $('.actionbuttonwrapper').hide();
        $('.editlanguage').hide();
        $('.processing').show();
        $("#nextlang").val($(this).attr("editlang"));
        if ($("#razortemplate").val() == 'Admin_ProductDetail.cshtml') {
            //move data to update postback field
            $('#xmlupdatemodeldata').val($.fn.genxmlajaxitems('#productmodels', '.modelitem'));
            $('#xmlupdateoptiondata').val($.fn.genxmlajaxitems('#productoptions', '.optionitem'));
            $('#xmlupdateoptionvaluesdata').val($.fn.genxmlajaxitems('#productoptionvalues', '.optionvalueitem'));
            $('#xmlupdateproductimages').val($.fn.genxmlajaxitems('#productimages', '.imageitem'));
            $('#xmlupdateproductdocs').val($.fn.genxmlajaxitems('#productdocs', '.docitem'));
            nbxget('product_admin_save', '#productdatasection', '#actionreturn');
        } else {
            nbxget('product_admin_getlist', '#productadminsearch', '#datadisplay');
        }
    });

    $(document).on("nbxgetcompleted", Admin_product_nbxgetCompleted); // assign a completed event for the ajax calls

    // start load all ajax data, continued by js in product.js file
    $('.processing').show();

    $('#razortemplate').val('Admin_ProductList.cshtml');
    nbxget('product_admin_getlist', '#productadminsearch', '#datadisplay');

    // function to do actions after an ajax call has been made.
    function Admin_product_nbxgetCompleted(e) {

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

        $('#productAdmin_cmdAddNew').unbind("click");
        $('#productAdmin_cmdAddNew').click(function () {
            $('.processing').show();
            $('#razortemplate').val('Admin_ProductDetail.cshtml');
            nbxget('product_adminaddnew', '#productadminsearch', '#datadisplay');
        });

        if (e.cmd == 'product_selectchangedisable' || e.cmd == 'product_selectchangehidden') {
            $('.processing').hide();
        };

        if (e.cmd == 'product_admin_getlist') {

            $('.processing').hide();

            $('.productsearchpanel').show();

            $("#productAdmin_cmdSaveExit").hide();
            $("#productAdmin_cmdSave").hide();
            $("#productAdmin_cmdSaveAs").hide();
            $("#productAdmin_cmdDelete").hide();
            $("#productAdmin_cmdReturn").hide();
            $("#productAdmin_cmdAddNew").show();

            // Move products
            $(".selectmove").hide();
            $(".selectcancel").hide();
            $(".selectrecord").hide();
            $(".savebutton").hide();

            if ($('#searchcategory').val() != '') {
                $(".selectrecord").show();
            }

        $("#ddllistsearchcategory").unbind("change");
            $("#ddllistsearchcategory").change(function() {
                $('#searchcategory').val($("#ddllistsearchcategory").val());
                nbxget('product_admin_getlist', '#productadminsearch', '#datadisplay');
            });

            $("#chkcascaderesults").unbind("change");
            $("#chkcascaderesults").change(function() {
                if ($("#chkcascaderesults").is(':checked')) {
                    $('#cascade').val("True");
                } else {
                    $('#cascade').val("False");
                }
                nbxget('product_admin_getlist', '#productadminsearch', '#datadisplay');
            });


            $('.selectrecord').unbind("click");
            $(".selectrecord").click(function() {
                $(".selectrecord").hide();
                $(".selectmove").show();
                $(".selectmove[itemid='" + $(this).attr("itemid") + "']").hide();
                $(".selectcancel[itemid='" + $(this).attr("itemid") + "']").show();
                $("#moveproductid").val($(this).attr("itemid"));

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
                $("#movetoproductid").val($(this).attr("itemid"));
                nbxget('product_moveproductadmin', '#productadminsearch', '#datadisplay');
            });

            $('#productAdmin_searchtext').val($('#searchtext').val());

            // editbutton created by list, so needs to be assigned on each render of list.
            $('.productAdmin_cmdEdit').unbind("click");
            $('.productAdmin_cmdEdit').click(function() {
                $('.processing').show();
                $('#razortemplate').val('Admin_ProductDetail.cshtml');
                $('#selecteditemid').val($(this).attr('itemid'));
                nbxget('product_admin_getdetail', '#productadminsearch', '#datadisplay');
            });


            $('.selectchangedisable').unbind("click");
            $('.selectchangedisable').click(function () {
                $('.processing').show();
                $('#selecteditemid').val($(this).attr('itemid'));
                if ($(this).hasClass("fa-check-circle")) {
                    $(this).addClass('fa-circle').removeClass('fa-check-circle');
                } else {
                    $(this).addClass('fa-check-circle').removeClass('fa-circle');
                }
                nbxget('product_selectchangedisable', '#productadminsearch');
            });

            $('.selectchangehidden').unbind("click");
            $('.selectchangehidden').click(function () {
                $('.processing').show();
                $('#selecteditemid').val($(this).attr('itemid'));
                if ($(this).hasClass("fa-check-circle")) {
                    $(this).addClass('fa-circle').removeClass('fa-check-circle');
                } else {
                    $(this).addClass('fa-check-circle').removeClass('fa-circle');
                }
                nbxget('product_selectchangehidden', '#productadminsearch');
            });


            $('.cmdPg').unbind("click");
            $('.cmdPg').click(function() {
                $('.processing').show();
                $('#pagenumber').val($(this).attr('pagenumber'));
                nbxget('product_admin_getlist', '#productadminsearch', '#datadisplay');
            });

            $('#productAdmin_cmdSearch').unbind("click");
            $('#productAdmin_cmdSearch').click(function() {
                $('.processing').show();
                $('#pagenumber').val('1');
                $('#searchtext').val($('#productAdmin_searchtext').val());

                nbxget('product_admin_getlist', '#productadminsearch', '#datadisplay');
            });

            $('#productAdmin_cmdReset').unbind("click");
            $('#productAdmin_cmdReset').click(function() {
                $('.processing').show();
                $('#pagenumber').val('1');
                $('#searchtext').val('');
                $("#searchcategory").val('');
                $("#ddllistsearchcategory").get(0).selectedIndex = 0;;

                nbxget('product_admin_getlist', '#productadminsearch', '#datadisplay');
            });

            $('.productAdmin_cmdDelete').unbind("click");
            $('.productAdmin_cmdDelete').click(function () {
                if (confirm($('#confirmdeletemsg').text())) {
                    $('.actionbuttonwrapper').hide();
                    $('.editlanguage').hide();
                    $('.processing').show();
                    $('#selecteditemid').val($(this).attr('itemid'));
                    nbxget('product_admin_delete', '#productadminsearch', '#actionreturn');
                }
            });

        }

        if (e.cmd == 'product_admin_delete') {
            $('.processing').show();
            $('#razortemplate').val('Admin_ProductList.cshtml');
            $('#selecteditemid').val('');
            nbxget('product_admin_getlist', '#productadminsearch', '#datadisplay');
        }

        if (e.cmd == 'product_admin_save') {
            $("#editlang").val($("#nextlang").val());
            $("#editlanguage").val($("#nextlang").val());
            nbxget('product_admin_getdetail', '#productadminsearch', '#datadisplay');
        };

        if (e.cmd == 'product_admin_saveexit' || e.cmd == 'product_admin_saveas') {
            $("#editlang").val($("#nextlang").val());
            $("#editlanguage").val($("#nextlang").val());
            $('#razortemplate').val('Admin_ProductList.cshtml');
            $('#selecteditemid').val('');
            nbxget('product_admin_getlist', '#productadminsearch', '#datadisplay');
        };

        if (e.cmd == 'product_moveproductadmin') {
            $('#selecteditemid').val('');
            $('#razortemplate').val('Admin_ProductList.cshtml');
            nbxget('product_admin_getlist', '#productadminsearch', '#datadisplay');
        };
        
        if (e.cmd == 'product_getproductselectlist') {
            $('.processing').hide();
        };

        if (e.cmd == 'product_admin_getdetail'
            || e.cmd == 'product_addproductmodels'
            || e.cmd == 'product_addproductoptions'
            || e.cmd == 'product_addproductoptionvalues'
            || e.cmd == 'product_updateproductdocs'
            || e.cmd == 'product_addproductcategory'
            || e.cmd == 'product_setdefaultcategory'
            || e.cmd == 'product_removeproductcategory'
            || e.cmd == 'product_populatecategorylist'
            || e.cmd == 'product_removeproperty'
            || e.cmd == 'product_addproperty'
            || e.cmd == 'product_addrelated'
            || e.cmd == 'product_removerelated'
            || e.cmd == 'product_adminaddnew'
            || e.cmd == 'product_updateproductimages') {

            // Copy the productid into the selecteditemid (for Add New Product)
            $('#selecteditemid').val($('#itemid').val());

            $('.actionbuttonwrapper').show();

            $('.processing').hide();

            $('.productsearchpanel').hide();
            
            $("#productAdmin_cmdSaveExit").show();
            $("#productAdmin_cmdSave").show();
            $("#productAdmin_cmdSaveAs").show();
            $("#productAdmin_cmdDelete").show();
            $("#productAdmin_cmdReturn").show();
            $("#productAdmin_cmdAddNew").hide();

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
                            $('#razortemplate').val('Admin_ProductImages.cshtml');
                            nbxget('product_updateproductimages', '#productadminsearch', '#productimages'); // load images
                            filesdone = 0;
                            $('input[id*="imguploadlist"]').val('');
                            $('.processing').hide();
                            $('#progress .progress-bar').css('width', '0');
                        }
                    });
            });
            var filecount2 = 0;
            var filesdone2 = 0;
            $(function () {
                'use strict';
                var url = '/DesktopModules/NBright/NBrightBuy/XmlConnector.ashx?cmd=fileupload';
                $('#cmdSaveDoc').unbind();
                $('#cmdSaveDoc').fileupload({
                    url: url,
                    maxFileSize: 5000000,
                    acceptFileTypes: /(\.|\/)(png)$/i,
                    dataType: 'json'
                }).prop('disabled', !$.support.fileInput).parent().addClass($.support.fileInput ? undefined : 'disabled')
                    .bind('fileuploadprogressall', function (e, data) {
                        var progress = parseInt(data.loaded / data.total * 100, 10);
                        $('#progress .progress-bar').css('width', progress + '%');
                    })
                    .bind('fileuploadadd', function (e, data) {
                        $.each(data.files, function (index, file) {
                            $('input[id*="docuploadlist"]').val($('input[id*="docuploadlist"]').val() + file.name + ',');
                            filesdone2 = filesdone2 + 1;
                        });
                    })
                    .bind('fileuploadchange', function (e, data) {
                        filecount2 = data.files.length;
                        $('.processing').show();
                    })
                    .bind('fileuploaddrop', function (e, data) {
                        filecount2 = data.files.length;
                        $('.processing').show();
                    })
                    .bind('fileuploadstop', function (e) {
                        if (filesdone2 == filecount2) {
                            $('#razortemplate').val('Admin_ProductDocs.cshtml');
                            nbxget('product_updateproductdocs', '#productadminsearch', '#productdocs'); // load images
                            filesdone2 = 0;
                            $('input[id*="docuploadlist"]').val('');
                            $('.processing').hide();
                            $('#progress .progress-bar').css('width', '0');
                        }
                    });
            });


            // ---------------------------------------------------------------------------
            // ACTION BUTTONS
            // ---------------------------------------------------------------------------
            $('#productAdmin_cmdReturn').unbind("click");
            $('#productAdmin_cmdReturn').click(function () {
                $('.processing').show();
                $('#razortemplate').val('Admin_ProductList.cshtml');
                $('#selecteditemid').val('');
                nbxget('product_admin_getlist', '#productadminsearch', '#datadisplay');
            });
            
            $('#productAdmin_cmdSave').unbind("click");
            $('#productAdmin_cmdSave').click(function () {
                $('.actionbuttonwrapper').hide();
                $('.editlanguage').hide();
                $('.processing').show();
                //move data to update postback field
                $('#xmlupdatemodeldata').val($.fn.genxmlajaxitems('#productmodels', '.modelitem'));
                $('#xmlupdateoptiondata').val($.fn.genxmlajaxitems('#productoptions', '.optionitem'));
                $('#xmlupdateoptionvaluesdata').val($.fn.genxmlajaxitems('#productoptionvalues', '.optionvalueitem'));
                $('#xmlupdateproductimages').val($.fn.genxmlajaxitems('#productimages', '.imageitem'));
                $('#xmlupdateproductdocs').val($.fn.genxmlajaxitems('#productdocs', '.docitem'));
                $('#razortemplate').val('Admin_ProductDetail.cshtml');
                nbxget('product_admin_save', '#productdatasection', '#actionreturn');
            });

            $('#productAdmin_cmdSaveExit').unbind("click");
            $('#productAdmin_cmdSaveExit').click(function () {
                $('.actionbuttonwrapper').hide();
                $('.editlanguage').hide();
                $('.processing').show();
                //move data to update postback field
                $('#xmlupdatemodeldata').val($.fn.genxmlajaxitems('#productmodels', '.modelitem'));
                $('#xmlupdateoptiondata').val($.fn.genxmlajaxitems('#productoptions', '.optionitem'));
                $('#xmlupdateoptionvaluesdata').val($.fn.genxmlajaxitems('#productoptionvalues', '.optionvalueitem'));
                $('#xmlupdateproductimages').val($.fn.genxmlajaxitems('#productimages', '.imageitem'));
                $('#xmlupdateproductdocs').val($.fn.genxmlajaxitems('#productdocs', '.docitem'));
                nbxget('product_admin_saveexit', '#productdatasection', '#actionreturn');
            });

            $('#productAdmin_cmdSaveAs').unbind("click");
            $('#productAdmin_cmdSaveAs').click(function () {
                $('.actionbuttonwrapper').hide();
                $('.editlanguage').hide();
                $('.processing').show();
                //move data to update postback field
                $('#xmlupdatemodeldata').val($.fn.genxmlajaxitems('#productmodels', '.modelitem'));
                $('#xmlupdateoptiondata').val($.fn.genxmlajaxitems('#productoptions', '.optionitem'));
                $('#xmlupdateoptionvaluesdata').val($.fn.genxmlajaxitems('#productoptionvalues', '.optionvalueitem'));
                $('#xmlupdateproductimages').val($.fn.genxmlajaxitems('#productimages', '.imageitem'));
                $('#xmlupdateproductdocs').val($.fn.genxmlajaxitems('#productdocs', '.docitem'));
                nbxget('product_admin_saveas', '#productdatasection', '#actionreturn');
            });


            $('#productAdmin_cmdDelete').unbind("click");
            $('#productAdmin_cmdDelete').click(function () {
                if (confirm($('#confirmdeletemsg').text())) {
                    $('.actionbuttonwrapper').hide();
                    $('.editlanguage').hide();
                    $('.processing').show();
                    nbxget('product_admin_delete', '#productadminsearch', '#actionreturn');
                }
            });



            // ---------------------------------------------------------------------------
            // STOCK CONTROL
            // ---------------------------------------------------------------------------
            $('.chkstockon:not(:checked)').each(function (index) {
                $(this).parent().parent().next().hide();
            });

            $('.selectrecord').unbind("click");
            $('.chkstockon').click(function () {
                if ($(this).is(":checked")) {
                    $(this).parent().parent().next().show();
                } else {
                    $(this).parent().parent().next().hide();
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
                $('#addqty').val($('#txtaddmodelqty').val());
                $('#razortemplate').val('Admin_ProductDetail.cshtml');
                nbxget('product_addproductmodels', '#productadminsearch', '#datadisplay'); // load models
            });

            $('#undomodel').unbind("click");
            $('#undomodel').click(function() {
                 undoremove('.modelitem', '#productmodels');
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

            // ---------------------------------------------------------------------------
            // OPTIONS
            // ---------------------------------------------------------------------------

            //Add options
            $('#addopt').unbind("click");
            $('#addopt').click(function () {
                $('.processing').show();
                $('#addqty').val($('#txtaddoptqty').val());
                $('#razortemplate').val("Admin_ProductOptions.cshtml");                
                nbxget('product_addproductoptions', '#productadminsearch', '#productoptions');
            });

            $('#undooption').unbind("click");
            $('#undooption').click(function () {
                undoremove('.optionitem', '#productoptions');
            });

            $('.removeoption').unbind("click");
            $('.removeoption').click(function () {
                removeelement($(this).parent().parent().parent().parent());
                if ($(this).parent().parent().parent().parent().hasClass('selected')) {
                    $('#productoptionvalues').hide();
                    $(this).parent().parent().parent().parent().removeClass('selected');
                }
            });

            $('.selectoption').unbind("click");
            $('.selectoption').click(function () {
                $('#selectedoptionid').val($(this).attr('itemid'));
                $(this).parent().parent().parent().parent().parent().children().removeClass('selected');
                $(this).parent().parent().parent().parent().addClass('selected');
                showoptionvalues();
            });

            //Add optionvalues
            $('#addoptvalues').unbind("click");
            $('#addoptvalues').click(function () {
                $('.processing').show();
                $('#addqty').val($('#txtaddoptvalueqty').val());   
                $('#razortemplate').val("Admin_ProductOptionValues.cshtml");
                nbxget('product_addproductoptionvalues', '#productadminsearch', '#productoptionvalues');
            });

            $('.removeoptionvalue').unbind("click");
            $('.removeoptionvalue').click(function () {
                 removeelement($(this).parent().parent().parent().parent());
            });

            $('#undooptionvalue').unbind("click");
            $('#undooptionvalue').click(function () {
                 undoremove('.optionvalueitem', '#productoptionvalues');
            });

            //trigger select option, to display correct option values
            if ($('#selectedoptionid').val() == '') {
                $('.selectoption').last().trigger('click');
            } else {
                var findstr = ".selectoption input[itemid='" + $('#selectedoptionid').val() + "']";
                if ($('.selectoption').find(findstr).length > 0) {
                    $('.selectoption').find(findstr).trigger('click');
                } else {
                    $('.selectoption').last().trigger('click');
                }

            }


            // ---------------------------------------------------------------------------
            // IMAGES
            // ---------------------------------------------------------------------------

            $('.removeimage').unbind("click");
            $('.removeimage').click(function () {
                removeelement($(this).parent().parent().parent().parent());
            });

            $('#undoimage').unbind("click");
            $('#undoimage').click(function () {
                 undoremove('.imageitem', '#productimages');
            });

            // ---------------------------------------------------------------------------
            // DOCS
            // ---------------------------------------------------------------------------

            $('.removedoc').unbind();
            $('.removedoc').click(function() {
                 removeelement($(this).parent().parent().parent().parent());
            });

            $('#undodoc').unbind();
            $('#undodoc').click(function () {
                 undoremove('.docitem', '#productdocs');
            });

            // ---------------------------------------------------------------------------
            // CATEGORY
            // ---------------------------------------------------------------------------
            $('.selectcategory').unbind();
            $('.selectcategory').click(function () {
                $('.processing').show();
                $('#selectedcatid').val($(this).val());
                $('#razortemplate').val("Admin_ProductCategories.cshtml");
                if ($(this).val() != null) nbxget('product_addproductcategory', '#productadminsearch', '#productcategories'); // load 
            });
            $('.removecategory').unbind();
            $('.removecategory').click(function () {
                $('.processing').show();
                $('#selecteditemid').val($(this).attr('itemid'));
                $('#selectedcatid').val($(this).attr('categoryid'));
                $('#razortemplate').val("Admin_ProductCategories.cshtml");
                nbxget('product_removeproductcategory', '#productadminsearch', '#productcategories'); // load             
            });
            // set default category
            $('.defaultcategory').unbind('click');
            $('.defaultcategory').click(function () {
                $('.processing').show();
                $('#selecteditemid').val($(this).attr('itemid'));
                $('#selectedcatid').val($(this).attr('categoryid'));
                $('#razortemplate').val("Admin_ProductCategories.cshtml");
                nbxget('product_setdefaultcategory', '#productadminsearch', '#productcategories'); // load             
            });


            // ---------------------------------------------------------------------------
            // PROPERTIES
            // ---------------------------------------------------------------------------

            $('.selectgrouptype').unbind('click');
            $('.selectgrouptype').click(function () {
                $('.processing').show();
                $('#selectedgroupref').val($(this).val());
                $('#razortemplate').val("Admin_ProductPropertySelect.cshtml");
                if ($(this).val() != null) nbxget('product_populatecategorylist', '#productadminsearch', '#groupcategorylist'); // load 
            });

            $('.removegroupcategory').unbind('click');
            $('.removegroupcategory').click(function () {
                $('.processing').show();
                $('#selectedcatid').val($(this).attr('categoryid'));
                $('#razortemplate').val("Admin_ProductProperties.cshtml");
                nbxget('product_removeproperty', '#productadminsearch', '#productgroupcategories'); // load             
            });

            $('.selectproperty').unbind('click');
            $('.selectproperty').click(function () {
                $('#selectedcatid').val($(this).val());
                $('.processing').show();
                $('#razortemplate').val("Admin_ProductProperties.cshtml");
                nbxget('product_addproperty', '#productadminsearch', '#productgroupcategories'); // load 
            });

            // ---------------------------------------------------------------------------
            // RELATED
            // ---------------------------------------------------------------------------

            $('.productAdmin_cmdEdit').unbind("click");
            $('.productAdmin_cmdEdit').click(function () {
                $('.processing').show();
                $('#razortemplate').val('Admin_ProductDetail.cshtml');
                $('#selecteditemid').val($(this).attr('itemid'));
                nbxget('product_admin_getdetail', '#productadminsearch', '#datadisplay');
            });


            $('.removerelated').unbind('click');
            $('.removerelated').click(function () {
                $('#selectedrelatedid').val($(this).attr('productid'));
                $('#razortemplate').val('Admin_ProductRelated.cshtml');
                nbxget('product_removerelated', '#productadminsearch', '#productrelated'); // load releated
            });

            $('#productselect').click(function () {
                $('#pagesize').val('60');
                $('#razortemplate').val('Admin_ProductSelectList.cshtml');
                nbxget('product_getproductselectlist', '#productadminsearch', '#productselectlist');
                $('#productdatasection').hide();
                $('#productselectsection').show();
            });

            $('#returnfromselect').click(function () {
                $('#pagesize').val('20');
                $('#productdatasection').show();
                $('#productselectsection').hide();
                $("#searchtext").val('');
                $("#searchcategory").val('');
                $('#razortemplate').val('Admin_ProductDetail.cshtml');
                nbxget('product_admin_getdetail', '#productadminsearch', '#datadisplay');
                $('#datadisplay').show();
            });

            $('#productselectlist').change(function () {
                //Do paging
                $('.cmdPg').unbind();
                $('.cmdPg').click(function () {
                    $('input[id*="pagenumber"]').val($(this).attr("pagenumber"));
                    nbxget('product_getproductselectlist', '#productadminsearch', '#productselectlist');
                });
                // select product
                $('.selectproduct').unbind();
                $('.selectproduct').click(function() {
                    $('.selectproductid' + $(this).attr('itemid')).hide();
                    $('#selectedrelatedid').val($(this).attr('itemid'));
                    nbxget('product_addrelatedproduct', '#productadminsearch', '#productrelated'); // load releated
                });

            });

            $("#ddlsearchcategory").unbind("change");
            $("#ddlsearchcategory").change(function () {
                $('#searchcategory').val($(this).val());
                nbxget('product_getproductselectlist', '#productadminsearch', '#productselectlist');
            });

            $('#txtproductselectsearch').val($('#searchtext').val());

            $('#selectsearch').unbind("click");
            $('#selectsearch').click(function () {
                $('.processing').show();
                $('#pagenumber').val('1');
                $('#searchtext').val($('#txtproductselectsearch').val());
                nbxget('product_getproductselectlist', '#productadminsearch', '#productselectlist');
            });

            $('#selectreset').unbind("click");
            $('#selectreset').click(function () {
                $('.processing').show();
                $('#pagenumber').val('1');
                $('#txtproductselectsearch').val('');
                $('#searchtext').val('');
                $("#ddlsearchcategory").val('');
                nbxget('product_getproductselectlist', '#productadminsearch', '#productselectlist');
            });

            // ---------------------------------------------------------------------------
            // CLIENTS
            // ---------------------------------------------------------------------------

            $('#clientselectlist').unbind("change");
            $('#clientselectlist').change(function () {
                // select product
                $('.selectclient').unbind();
                $('.selectclient').click(function () {
                    $('.selectuserid' + $(this).attr('itemid')).hide();
                    $('input[id*="selecteduserid"]').val($(this).attr('itemid'));
                    $('#razortemplate').val('Admin_ProductClientSelect.cshtml');
                    nbxget('product_addproductclient', '#productadminsearch', '#productclients'); // load releated
                });
            });

            $('#clientlistsearch').unbind("click");
            $('#clientlistsearch').click(function () {
                $('#searchtext').val($('#txtclientsearch').val());
                $('#razortemplate').val('Admin_ProductClientSelect.cshtml');
                nbxget('product_getclientselectlist', '#productadminsearch', '#clientselectlist');
            });

            $('#clientselect').unbind("click");
            $('#clientselect').click(function () {
                $(this).hide();
                $('#productdatasection').hide();
                $('#clientselectsection').show();
            });

            $('#returnfromclientselect').unbind("click");
            $('#returnfromclientselect').click(function () {
                $('#clientselect').show();
                $("input[id*='searchtext']").val('');
                $('#razortemplate').val('Admin_ProductClients.cshtml');
                nbxget('product_productclients', '#productadminsearch', '#productclients');
                $('#clientselectsection').hide();
                $('#productdatasection').show();
            });

            $('#productclients').unbind("click");
            $('#productclients').change(function () {
                $('.removeclient').click(function () {
                    $('input[id*="selecteduserid"]').val($(this).attr('itemid'));
                    $('#razortemplate').val('Admin_ProductClients.cshtml');
                    nbxget('product_removeproductclient', '#productadminsearch', '#productclients');
                });
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
        if ($(elementtoberemoved).hasClass('optionitem')) $('#undooption').show();
        if ($(elementtoberemoved).hasClass('optionvalueitem')) $('#undooptionvalue').show();
        if ($(elementtoberemoved).hasClass('imageitem')) $('#undoimage').show();
        if ($(elementtoberemoved).hasClass('docitem')) $('#undodoc').show();
        if ($(elementtoberemoved).hasClass('categoryitem')) $('#undocategory').show();
        if ($(elementtoberemoved).hasClass('relateditem')) $('#undorelated').show();
        if ($(elementtoberemoved).hasClass('clientitem')) $('#undoclient').show();
    }
    function undoremove(itemselector, destinationselector) {
        if ($('#recyclebin').length > 0) {
            $(destinationselector).append($('#recyclebin').find(itemselector).last());
        }
        if ($('#recyclebin').children(itemselector).length == 0) {
            if (itemselector == '.modelitem') $('#undomodel').hide();
            if (itemselector == '.optionitem') $('#undooption').hide();
            if (itemselector == '.optionvalueitem') $('#undooptionvalue').hide();
            if (itemselector == '.imageitem') $('#undoimage').hide();
            if (itemselector == '.docitem') $('#undodoc').hide();
            if (itemselector == '.categoryitem') $('#undocategory').hide();
            if (itemselector == '.relateditem') $('#undorelated').hide();
            if (itemselector == '.clientitem') $('#undoclient').hide();
        }
    }

    function showoptionvalues() {
        $('#productoptionvalues').children().hide();
        if ($('#productoptions').children('.selected').first().find('input[id*="optionid"]').length > 0) {
            $('#productoptionvalues').children('.' + $('#productoptions').children('.selected').first().find('input[id*="optionid"]').val()).show();
            $('#productoptionvalues').show();
        }
        $('#optionvaluecontrol').show();
    }


    // ---------------------------------------------------------------------------

});

