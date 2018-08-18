$(document).ready(function () {

    $('.selectlang').unbind("click");
    $(".selectlang").click(function () {
        $('.editlanguage').hide();
        $('.processing').show();
        $("#p1_nextlang").val($(this).attr("editlang"));
        if ($("#p1_razortemplate").val() == 'Admin_ProductDetail.cshtml') {
            product_admin_save('product_admin_save');
        } else {
            product_admin_search();
        }
    });

    $(document).on("nbxgetcompleted", Admin_product_nbxgetCompleted); // assign a completed event for the ajax calls

    // start load all ajax data, continued by js in product.js file
    $('.processing').show();

    $('#p1_razortemplate').val('Admin_ProductList.cshtml');
    nbxget('product_admin_getlist', '#selectparams_Product_Admin', '#datadisplay');

    $('#product_admin_cmdSave').unbind("click");
    $('#product_admin_cmdSave').click(function () {
        $('.editlanguage').hide();
        $('.processing').show();
        product_admin_save('product_admin_save');
    });

    $('#product_admin_cmdSaveExit').unbind("click");
    $('#product_admin_cmdSaveExit').click(function () {
        product_admin_save('product_admin_saveexit');
    });

    $('#product_admin_cmdSaveAs').unbind("click");
    $('#product_admin_cmdSaveAs').click(function () {
        product_admin_save('product_admin_saveas');
    });

    $('#product_admin_cmdReturn').unbind('click');
    $('#product_admin_cmdReturn').click(function () {
        $('#p1_selecteditemid').val('');       
        $('#p1_razortemplate').val('Admin_ProductList.cshtml');
        nbxget('product_admin_getlist', '#selectparams_Product_Admin', '#datadisplay');
    });

    $('#product_admin_cmdDelete').unbind('click');
    $('#product_admin_cmdDelete').click(function () {
        if (confirm($('#confirmdeletemsg').text())) {
            $('.processing').show();
            nbxget('product_admin_delete', '#selectparams_Product_Admin');
        }
    });

    $('#product_admin_cmdAddNew').unbind('click');
    $('#product_admin_cmdAddNew').click(function () {
        $('.processing').show();
        $('#p1_selecteditemid').val('');
        $('#p1_razortemplate').val('Admin_ProductDetail.cshtml');
        nbxget('product_admin_addnew', '#selectparams_Product_Admin', '#datadisplay');
    });


});


function Admin_product_nbxgetCompleted(e) {


    $('#datadisplay').children().find('.sortelementUp').unbind("click")
    $('#datadisplay').children().find('.sortelementUp').click(function () { moveUp($(this).parent()); });
    $('#datadisplay').children().find('.sortelementDown').unbind("click")
    $('#datadisplay').children().find('.sortelementDown').click(function () { moveDown($(this).parent()); });
    $('#datadisplay').children().find('.sortelementLeft').unbind("click")
    $('#datadisplay').children().find('.sortelementLeft').click(function () { moveLeft($(this).parent()); });
    $('#datadisplay').children().find('.sortelementRight').unbind("click")
    $('#datadisplay').children().find('.sortelementRight').click(function () { moveRight($(this).parent()); });


    if (e.cmd == 'product_admin_addnew') {
        $('#p1_selecteditemid').val($('#itemid').val()); // move the itemid into the selecteditemid, so page knows what itemid is being edited
        productdetail();
    }

    if (e.cmd == 'product_admin_delete' || e.cmd == 'product_admin_moveproductadmin') {
        $('#p1_selecteditemid').val('');
        $('#p1_razortemplate').val('Admin_ProductList.cshtml');
        $('#p1_moveproductid').val('');
        nbxget('product_admin_getlist', '#selectparams_Product_Admin', '#datadisplay');
    }

    if (e.cmd == 'product_admin_save') {
        $('.processing').show();
        nbxget('product_admin_getdetail', '#selectparams_Product_Admin', '#datadisplay');// relist after save
    }

    if (e.cmd == 'product_admin_saveexit') {
        $('#p1_selecteditemid').val(''); 
        $('#p1_razortemplate').val('Admin_ProductList.cshtml');
        nbxget('product_admin_getlist', '#selectparams_Product_Admin', '#datadisplay');
    }
    if (e.cmd == 'product_admin_saveas') {
        $('#p1_selecteditemid').val('');
        $('#p1_razortemplate').val('Admin_ProductList.cshtml');
        nbxget('product_admin_getlist', '#selectparams_Product_Admin', '#datadisplay');
    }

    if (e.cmd == 'product_admin_selectlang') {
        $('#p1_editlang').val($('#p1_nextlang').val()); // alter lang after, so we get correct data record
        nbxget('product_admin_getdata', '#selectparams_Product_Admin', '#datadisplay'); // do ajax call to get edit form
    }

    if (e.cmd == 'product_admin_updateboolean') {
        $('.processing').hide();
    }    

    if (e.cmd == 'product_admin_getlist') {

        $('.processing').hide();

        // empty recycle bin, so we don;t keep any elements in there from a detail deleted that was not saved.
        $('#recyclebin').empty();

        $('#product_admin_cmdSearch').unbind("click");
        $('#product_admin_cmdSearch').click(function () {
            product_admin_search();
        });

        $('#product_admin_cmdReset').unbind("click");
        $('#product_admin_cmdReset').click(function () {
            $('.processing').show();
            $('#p1_pagenumber').val('1');
            $('#p1_searchtext').val('');
            $('#p1_searchcategory').val('');
            $('#p1_searchproperty').val('');
            $('#p1_searchhidden').val('');
            $('#p1_searchvisible').val('');
            $('#p1_searchenabled').val('');
            $('#p1_searchdisabled').val('');
            $('#p1_selecteditemid').val('');
            nbxget('product_admin_getlist', '#selectparams_Product_Admin', '#datadisplay');
        });

        $('.product_admin_cmdDelete').unbind("click");
        $('.product_admin_cmdDelete').click(function () {
            if (confirm($('#confirmdeletemsg').text())) {
                $('.editlanguage').hide();
                $('.processing').show();
                $('#p1_selecteditemid').val($(this).attr('itemid'));
                nbxget('product_admin_delete', '#selectparams_Product_Admin');
            }
        });

        $('.cmdPg').unbind("click");
        $('.cmdPg').click(function () {
            $('.processing').show();
            $('#p1_pagenumber').val($(this).attr('pagenumber'));
            product_admin_search();
        });

        // Move products
        $(".selectmove").hide();
        $(".selectcancel").hide();
        $(".selectrecord").hide();

        var catlistcount = 0;
        if ($('#p1_searchcategory').val() != '') {
            catlistcount = ($('#p1_searchcategory').val().split(',').length - 1)
        }
        if ($('#p1_searchproperty').val() != '') {
            catlistcount = catlistcount + ($('#p1_searchproperty').val().split(',').length - 1)
        }
        if (catlistcount == 1) {
            if ($('#p1_moveproductid').val() != '') {
                $('.selectmove').show();
            }
            else {
                $('.selectrecord').show();
            }
        }
        else {
            $('.selectrecord').hide();
        }

        $('.selectrecord').unbind("click");
        $(".selectrecord").click(function () {
            $(".selectrecord").hide();
            $(".selectmove").show();
            $(".selectmove[itemid='" + $(this).attr("itemid") + "']").hide();
            $(".selectcancel[itemid='" + $(this).attr("itemid") + "']").show();
            $("#p1_moveproductid").val($(this).attr("itemid"));

            var selectid = $(this).attr("itemid");
            $(".selectmove").each(function (index) {
                if ($(this).attr("parentlist").indexOf(selectid + ";") > -1) $(this).hide();
            });

            $('.searchcategorydiv').hide();
            $('.searchpropertydiv').hide();

        });

        $('.selectcancel').unbind("click");
        $(".selectcancel").click(function () {
            $(".selectmove").hide();
            $(".selectcancel").hide();
            $(".selectrecord").show();
            $('.searchcategorydiv').show();
            $('.searchpropertydiv').show();
            $('#p1_moveproductid').val('');
        });

        $('.selectmove').unbind("click");
        $(".selectmove").click(function () {
            $(".selectmove").hide();
            $(".selectcancel").hide();
            $(".selectrecord").show();
            $("#p1_movetoproductid").val($(this).attr("itemid"));
            $('.searchcategorydiv').show();
            $('.searchpropertydiv').show();

            var propertylist = $(".searchpropertydiv").dropdownCheckbox("checked");
            var selectproperty = '';
            jQuery.each(propertylist, function (index, item) {
                $('#p1_searchproperty').val(item.id + ',');
            });

            var catlist = $(".searchcategorydiv").dropdownCheckbox("checked");
            var selectcatid = '';
            jQuery.each(catlist, function (index, item) {
                $('#p1_searchcategory').val(item.id + ',');
            });

            $('.processing').show();
            nbxget('product_admin_moveproductadmin', '#selectparams_Product_Admin');
        });

        $('.updateboolean').unbind("click");
        $('.updateboolean').click(function () {
            $('.processing').show();
            $('#p1_selecteditemid').val($(this).attr('itemid'));
            $('#p1_xpath').val($(this).attr('xpath'));
            if ($(this).hasClass("fa-check-circle")) {
                $(this).addClass('fa-circle').removeClass('fa-check-circle');
            } else {
                $(this).addClass('fa-check-circle').removeClass('fa-circle');
            }
            nbxget('product_admin_updateboolean', '#selectparams_Product_Admin');
        });

        $('#searchvisible').unbind("click");
        $('#searchvisible').click(function () {
            if (!$('#searchvisible').is(":checked")) {
                $('#searchhidden').prop('checked', true);
            }
        });

        $('#searchhidden').unbind("click");
        $('#searchhidden').click(function () {
            if (!$('#searchhidden').is(":checked")) {
                $('#searchvisible').prop('checked', true);
            }
        });

        $('#searchdisabled').unbind("click");
        $('#searchdisabled').click(function () {
            if (!$('#searchdisabled').is(":checked")) {
                $('#searchenabled').prop('checked', true);
            }
        });

        $('#searchenabled').unbind("click");
        $('#searchenabled').click(function () {
            if (!$('#searchenabled').is(":checked")) {
                $('#searchdisabled').prop('checked', true);
            }
        });


    }

    if (e.cmd == 'product_admin_getdetail' || e.cmd == 'product_admin_addproductmodels') {
        productdetail();
    }

    if (e.cmd == 'product_admin_updateproductdocs') {
        initDocDisplay();
    }

    if (e.cmd == 'product_admin_updateproductimages') {
        initImgDisplay();
    }

    if (e.cmd == 'product_admin_populatecategorylist' || e.cmd == 'product_admin_addproperty' || e.cmd == 'product_admin_removeproperty') {
        initPropertyDisplay();
    }
    if (e.cmd == 'product_admin_addproductcategory' || e.cmd == 'product_admin_removeproductcategory' || e.cmd == 'product_admin_setdefaultcategory') {
        initCategoryDisplay();
    }
    if (e.cmd == 'product_admin_removerelated') {
        initRelatedDisplay();
    }
    if (e.cmd == 'product_admin_removeproductclient') {
        initRelatedDisplay();
    }
    if (e.cmd == 'product_admin_addproductoptionvalues' || e.cmd == 'product_admin_addproductoptions') {
        initOptionDisplay();
    }
    if (e.cmd == 'product_admin_addproductmodels') {
        initModelDisplay();
    }

    // ---------------------------------------------------------------------------
    // check if we are displaying a list or the detail and do processing.
    // ---------------------------------------------------------------------------

    if (($('#p1_selecteditemid').val() != '') || (e.cmd == 'product_admin_addnew')) {

        if (e.cmd == 'product_admin_getproductselectlist'
            || e.cmd == 'product_admin_addrelatedproduct'
            || e.cmd == 'product_admin_addproductclient'
            || e.cmd == 'product_admin_getclientselectlist') {
            $('.processing').hide();
            product_admin_NoButtons();
        }
        else {
            product_admin_DetailButtons();
        }

        // $('.processing').hide();

    } else {
        //PROCESS LIST
        product_admin_ListButtons();
        $('.product_admin_cmdEdit').unbind('click');
        $('.product_admin_cmdEdit').click(function () {
            $('.processing').show();
            $('#p1_razortemplate').val('Admin_ProductDetail.cshtml');
            $('#p1_selecteditemid').val($(this).attr("itemid")); // assign the sleected itemid, so the server knows what item is being edited
            nbxget('product_admin_getdetail', '#selectparams_Product_Admin', '#datadisplay'); // do ajax call to get edit form
        });
        $('.processing').hide();
    }


}


    // ---------------------------------------------------------------------------
    // FUNCTIONS
    // ---------------------------------------------------------------------------

function product_admin_DetailButtons() {
    $('.editlanguage').show();
    $('#product_admin_cmdSave').show();
    $('#product_admin_cmdSaveExit').show();
    $('#product_admin_cmdSaveAs').show();
    $('#product_admin_cmdDelete').show();
    $('#product_admin_cmdReturn').show();
    $('#product_admin_cmdAddNew').hide();
    $('input[datatype="date"]').datepicker(); // assign datepicker to any ajax loaded fields
}

function product_admin_ListButtons() {
        $('.editlanguage').show();
    $('#product_admin_cmdSave').hide();
    $('#product_admin_cmdSaveExit').hide();
    $('#product_admin_cmdSaveAs').hide();
    $('#product_admin_cmdDelete').hide();
    $('#product_admin_cmdReturn').hide();
    $('#product_admin_cmdAddNew').show();
}

function product_admin_NoButtons() {
        $('.editlanguage').hide();
    $('#product_admin_cmdSave').hide();
    $('#product_admin_cmdSaveExit').hide();
    $('#product_admin_cmdSaveAs').hide();
    $('#product_admin_cmdDelete').hide();
    $('#product_admin_cmdReturn').hide();
    $('#product_admin_cmdAddNew').hide();
    }


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

function moveLeft(item) {
    var prev = item.prev();
    if (prev.length == 0)
        return;
    prev.css('z-index', 999).css('position', 'relative').animate({ left: item.width() }, 250);
    item.css('z-index', 1000).css('position', 'relative').animate({ left: '-' + prev.width() }, 300, function () {
        prev.css('z-index', '').css('left', '').css('position', '');
        item.css('z-index', '').css('left', '').css('position', '');
        item.insertBefore(prev);
    });
}

function moveRight(item) {
    var next = item.next();
    if (next.length == 0)
        return;
    next.css('z-index', 999).css('position', 'relative').animate({ left: '-' + item.width() }, 250);
    item.css('z-index', 1000).css('position', 'relative').animate({ left: next.width() }, 300, function () {
        next.css('z-index', '').css('left', '').css('position', '');
        item.css('z-index', '').css('left', '').css('position', '');
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

    function initImgFileUpload() {
        // ---------------------------------------------------------------------------
        // IMG FILE UPLOAD
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
                        $('input[id*="p1_imguploadlist"]').val($('input[id*="p1_imguploadlist"]').val() + file.name + ',');
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
                        $('#p1_razortemplate').val('Admin_ProductImages.cshtml');
                        nbxget('product_admin_updateproductimages', '#selectparams_Product_Admin', '#productimages'); // load images
                        filesdone = 0;
                        $('input[id*="p1_imguploadlist"]').val('');
                        $('.processing').hide();
                        $('#progress .progress-bar').css('width', '0');
                    }
                });
        });

    }

    function initDocFileUpload() {
        // ---------------------------------------------------------------------------
        // DOC FILE UPLOAD
        // ---------------------------------------------------------------------------
        var filecount2 = 0;
        var filesdone2 = 0;
        $(function () {
            'use strict';
            var url = '/DesktopModules/NBright/NBrightBuy/XmlConnector.ashx?cmd=fileupload';
            $('#cmdSaveDoc').unbind();
            $('#cmdSaveDoc').fileupload({
                url: url,
                maxFileSize: 5000000,
                acceptFileTypes: /(\.|\/)(pdf)$/i,
                dataType: 'json'
            }).prop('disabled', !$.support.fileInput).parent().addClass($.support.fileInput ? undefined : 'disabled')
                .bind('fileuploadprogressall', function (e, data) {
                    var progress = parseInt(data.loaded / data.total * 100, 10);
                    $('#progress .progress-bar').css('width', progress + '%');
                })
                .bind('fileuploadadd', function (e, data) {
                    $.each(data.files, function (index, file) {
                        $('input[id*="p1_docuploadlist"]').val($('input[id*="p1_docuploadlist"]').val() + file.name + ',');
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
                        $('#p1_razortemplate').val('Admin_ProductDocs.cshtml');
                        nbxget('product_admin_updateproductdocs', '#selectparams_Product_Admin', '#productdocs'); // load images
                        filesdone2 = 0;
                        $('input[id*="p1_docuploadlist"]').val('');
                        $('.processing').hide();
                        $('#progress .progress-bar').css('width', '0');
                    }
                });
        });

    }

function initImgDisplay() {

    $('#p1_razortemplate').val('Admin_ProductDetail.cshtml');

        $('.removeimage').unbind("click");
        $('.removeimage').click(function () {
            removeelement($(this).parent().parent().parent().parent());
        });

        $('#undoimage').unbind("click");
        $('#undoimage').click(function () {
            undoremove('.imageitem', '#productimages');
        });
    }

function initDocDisplay() {
    $('#p1_razortemplate').val('Admin_ProductDetail.cshtml');

        $('.removedoc').unbind();
        $('.removedoc').click(function () {
            removeelement($(this).parent().parent().parent().parent());
        });

        $('#undodoc').unbind();
        $('#undodoc').click(function () {
            undoremove('.docitem', '#productdocs');
        });
    }

    function initPropertyDisplay() {

        $('.selectgrouptype').unbind('click');
        $('.selectgrouptype').click(function () {
            $('.processing').show();
            $('#p1_selectedgroupref').val($(this).val());
            $('#p1_razortemplate').val("Admin_ProductPropertySelect.cshtml");
            if ($(this).val() != null) nbxget('product_admin_populatecategorylist', '#selectparams_Product_Admin', '#groupcategorylist'); // load 
        });

        $('.removegroupcategory').unbind('click');
        $('.removegroupcategory').click(function () {
            $('.processing').show();
            $('#p1_selectedcatid').val($(this).attr('categoryid'));
            $('#p1_razortemplate').val("Admin_ProductProperties.cshtml");
            nbxget('product_admin_removeproperty', '#selectparams_Product_Admin', '#productgroupcategories'); // load             
        });

        $('.selectproperty').unbind('click');
        $('.selectproperty').click(function () {
            $('#p1_selectedcatid').val($(this).val());
            $('.processing').show();
            $('#p1_razortemplate').val("Admin_ProductProperties.cshtml");
            nbxget('product_admin_addproperty', '#selectparams_Product_Admin', '#productgroupcategories'); // load 
        });
        $('.processing').hide();
    }

    function initCategoryDisplay() {
        $('.selectcategory').unbind();
        $('.selectcategory').click(function () {
            $('.processing').show();
            $('#p1_selectedcatid').val($(this).val());
            $('#p1_razortemplate').val("Admin_ProductCategories.cshtml");
            if ($(this).val() != null) nbxget('product_admin_addproductcategory', '#selectparams_Product_Admin', '#productcategories'); // load 
        });
        $('.removecategory').unbind();
        $('.removecategory').click(function () {
            $('.processing').show();
            $('#p1_selecteditemid').val($(this).attr('itemid'));
            $('#p1_selectedcatid').val($(this).attr('categoryid'));
            $('#p1_razortemplate').val("Admin_ProductCategories.cshtml");
            nbxget('product_admin_removeproductcategory', '#selectparams_Product_Admin', '#productcategories'); // load             
        });
        // set default category
        $('.defaultcategory').unbind('click');
        $('.defaultcategory').click(function () {
            $('.processing').show();
            $('#p1_selecteditemid').val($(this).attr('itemid'));
            $('#p1_selectedcatid').val($(this).attr('categoryid'));
            $('#p1_razortemplate').val("Admin_ProductCategories.cshtml");
            nbxget('product_admin_setdefaultcategory', '#selectparams_Product_Admin', '#productcategories'); // load             
        });
        $('.processing').hide();
    }

    function initRelatedDisplay() {
        $('.removerelated').unbind('click');
        $('.removerelated').click(function () {
            $('#p1_selectedrelatedid').val($(this).attr('productid'));
            $("#p1_razortemplate").val('Admin_ProductRelated.cshtml');
            nbxget('product_admin_removerelated', '#selectparams_Product_Admin', '#productrelated'); // load releated
        });

        $('#productselect').unbind('click');
        $('#productselect').click(function () {
            $('.processing').show();
            $('#p1_accordianactive').val($("#accordion").accordion("option", "active"));
            $('#p1_pagesize').val('60');
            $("#p1_razortemplate").val('Admin_ProductSelectList.cshtml');
            nbxget('product_admin_getproductselectlist', '#selectparams_Product_Admin', '#productselectlist');
            $('#productdatasection').hide();
            $('#productselectsection').show();
        });

        $('#returnfromselect').unbind('click');
        $('#returnfromselect').click(function () {
            $('#p1_pagesize').val('20');
            $('#productdatasection').show();
            $('#productselectsection').hide();
            $("#p1_searchtext").val('');
            $("#p1_searchcategory").val('');
            $('#p1_razortemplate').val("Admin_ProductDetail.cshtml");
            nbxget('product_admin_getdetail', '#selectparams_Product_Admin', '#datadisplay');
            $('#datadisplay').show();
        });

        $("#productselectlist").unbind("change");
        $('#productselectlist').change(function () {
            //Do paging
            $('.cmdPg').unbind();
            $('.cmdPg').click(function () {
                $('#p1_pagenumber').val($(this).attr("pagenumber"));
                nbxget('product_admin_getproductselectlist', '#selectparams_Product_Admin', '#productselectlist');
            });
            // select product
            $('.selectproduct').unbind();
            $('.selectproduct').click(function () {
                $('.selectproductid' + $(this).attr('itemid')).hide();
                $('#p1_selectedrelatedid').val($(this).attr('itemid'));
                nbxget('product_admin_addrelatedproduct', '#selectparams_Product_Admin', '#productrelated'); // load releated
            });

        });

        $("#ddlsearchcategory").unbind("change");
        $("#ddlsearchcategory").change(function () {
            $('#p1_searchcategory').val($(this).val());
            $("#p1_razortemplate").val('Admin_ProductSelectList.cshtml');
            nbxget('product_admin_getproductselectlist', '#selectparams_Product_Admin', '#productselectlist');
        });

        $('#txtproductselectsearch').val($('#searchtext').val());

        $('#selectsearch').unbind("click");
        $('#selectsearch').click(function () {
            $('.processing').show();
            $('#p1_pagenumber').val('1');
            $('#p1_searchtextrelated').val($('#txtproductselectsearch').val());
            $("#p1_razortemplate").val('Admin_ProductSelectList.cshtml');
            nbxget('product_admin_getproductselectlist', '#selectparams_Product_Admin', '#productselectlist');
        });

        $('#selectreset').unbind("click");
        $('#selectreset').click(function () {
            $('.processing').show();
            $('#p1_pagenumber').val('1');
            $('#p1_searchtextrelated').val('');
            $("#p1_searchcategory").val('');
            $("#ddlsearchcategory").val('');
            $('#txtproductselectsearch').val('');
            $("#p1_razortemplate").val('Admin_ProductSelectList.cshtml');
            nbxget('product_admin_getproductselectlist', '#selectparams_Product_Admin', '#productselectlist');
        });

        $('.processing').hide();

    }

    function initClientDisplay() {

        $('#clientselectlist').unbind("change");
        $('#clientselectlist').change(function () {
            // select product
            $('.selectclient').unbind();
            $('.selectclient').click(function () {
                $('.selectuserid' + $(this).attr('itemid')).hide();
                $('input[id*="selecteduserid"]').val($(this).attr('itemid'));
                $('#p1_razortemplate').val('Admin_ProductClientSelect.cshtml');
                 nbxget('product_admin_addproductclient', '#selectparams_Product_Admin', '#productclients'); // load releated
            });
        });

        $('#clientlistsearch').unbind("click");
        $('#clientlistsearch').click(function () {
            $('#p1_searchtext').val($('#txtclientsearch').val());
            $('#p1_razortemplate').val('Admin_ProductClientSelect.cshtml');
            nbxget('product_admin_getclientselectlist', '#selectparams_Product_Admin', '#clientselectlist');
        });

        $('#clientselect').unbind("click");
        $('#clientselect').click(function () {
            $('#p1_accordianactive').val($("#accordion").accordion("option", "active"));
            $(this).hide();
            $('#productdatasection').hide();
            $('#clientselectsection').show();
            product_admin_NoButtons();
        });

        $('#returnfromclientselect').unbind("click");
        $('#returnfromclientselect').click(function () {
            $('#clientselect').show();
            $('#p1_searchtext').val('');
            $("#p1_razortemplate").val('Admin_ProductClients.cshtml');
            nbxget('product_admin_productclients', '#selectparams_Product_Admin', '#productclients');
            $('#clientselectsection').hide();
            $('#productdatasection').show();
        });

        $('#productclients').unbind("click");
        $('#productclients').change(function () {
            $('.removeclient').click(function () {
                $('#p1_searchtext').val($(this).attr('itemid'));
                $("#p1_razortemplate").val('Admin_ProductClients.cshtml');
                nbxget('product_admin_removeproductclient', '#selectparams_Product_Admin', '#productclients');
            });
        });


    }

function initOptionDisplay() {
    //Add options
    $('#addopt').unbind("click");
    $('#addopt').click(function () {
        $('.processing').show();
        $('#p1_addqty').val($('#txtaddoptqty').val());
        $('#p1_razortemplate').val("Admin_ProductOptions.cshtml");
        nbxget('product_admin_addproductoptions', '#selectparams_Product_Admin', '#productoptions');
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
        $('#p1_selectedoptionid').val($(this).attr('itemid'));
        $(this).parent().parent().parent().parent().parent().children().removeClass('selected');
        $(this).parent().parent().parent().parent().addClass('selected');
        showoptionvalues();
    });

    //Add optionvalues
    $('#addoptvalues').unbind("click");
    $('#addoptvalues').click(function () {
        $('.processing').show();
        $('#p1_addqty').val($('#txtaddoptvalueqty').val());
        $('#p1_razortemplate').val("Admin_ProductOptionValues.cshtml");
        nbxget('product_admin_addproductoptionvalues', '#selectparams_Product_Admin', '#productoptionvalues');
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
    if ($('#p1_selectedoptionid').val() == '') {
        $('.selectoption').last().trigger('click');
    } else {
        if ($('.selectoption[itemid=' + $('#p1_selectedoptionid').val() + ']').length > 0) {
            $('.selectoption[itemid=' + $('#p1_selectedoptionid').val() + ']').trigger('click');
        } else {
            $('.selectoption').last().trigger('click');
        }

    }

    $('.processing').hide();

}

function initModelDisplay() {

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

    $('.removemodel').unbind("click");
    $('.removemodel').click(function () { removeelement($(this).parent().parent().parent().parent()); });

    $('input[id*="availabledate"]').datepicker();

    //Add models
    $('#addmodels').unbind("click");
    $('#addmodels').click(function () {
        $('.processing').show();
        $('#p1_addqty').val($('#txtaddmodelqty').val());
        $('#p1_razortemplate').val('Admin_ProductDetail.cshtml');
        nbxget('product_admin_addproductmodels', '#selectparams_Product_Admin', '#datadisplay'); // load models
    });

    $('#undomodel').unbind("click");
    $('#undomodel').click(function () {
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

    $('.processing').hide();

}

function product_admin_save(ajaxaction) {
    $('.processing').show();
    product_admin_NoButtons();

    $('#p1_razortemplate').val('Admin_ProductDetail.cshtml');

    //move data to update postback field
    $('#xmlupdatemodeldata').val($.fn.genxmlajaxitems('#productmodels', '.modelitem'));
    $('#xmlupdateoptiondata').val($.fn.genxmlajaxitems('#productoptions', '.optionitem'));
    $('#xmlupdateoptionvaluesdata').val($.fn.genxmlajaxitems('#productoptionvalues', '.optionvalueitem'));
    $('#xmlupdateproductimages').val($.fn.genxmlajaxitems('#productimages', '.imageitem'));
    $('#xmlupdateproductdocs').val($.fn.genxmlajaxitems('#productdocs', '.docitem'));
    nbxget(ajaxaction, '#productdatasection');

}

    function product_admin_search() {
        $('.processing').show();
        product_admin_NoButtons();

        $('#p1_razortemplate').val('Admin_ProductList.cshtml');
        $('#p1_selecteditemid').val('');
        $('#p1_searchtext').val($('#searchtext').val());

        $('#p1_moveproductid').val('');
        $('#p1_movetoproductid').val('');

        if ($(".searchcategorydiv").length) {
            var catlist = $(".searchcategorydiv").dropdownCheckbox("checked");
            var selectcatid = '';
            jQuery.each(catlist, function (index, item) {
                selectcatid += item.id + ',';
            });
            $('#p1_searchcategory').val(selectcatid);
        }

        if ($(".searchpropertydiv").length) {
            var propertylist = $(".searchpropertydiv").dropdownCheckbox("checked");
            var selectproperty = '';
            jQuery.each(propertylist, function (index, item) {
                selectproperty += item.id + ',';
            });
            $('#p1_searchproperty').val(selectproperty);
        }
        $('#p1_cascade').val('False');
        if ($('#cascade').prop('checked')) {
            $('#p1_cascade').val('True');
        }
        $('#p1_searchhidden').val('False');
        if ($('#searchhidden').prop('checked')) {
            $('#p1_searchhidden').val('True');
        }
        $('#p1_searchvisible').val('False');
        if ($('#searchvisible').prop('checked')) {
            $('#p1_searchvisible').val('True');
        }
        $('#p1_searchenabled').val('False');
        if ($('#searchenabled').prop('checked')) {
            $('#p1_searchenabled').val('True');
        }
        $('#p1_searchdisabled').val('False');
        if ($('#searchdisabled').prop('checked')) {
            $('#p1_searchdisabled').val('True');
        }

    nbxget('product_admin_getlist', '#selectparams_Product_Admin', '#datadisplay');

}

function productdetail()
{

    setupbackoffice();
    $("#accordion").accordion("option", "active", parseInt($('#p1_accordianactive').val()));

    $('#accordion').unbind('click');
    $('#accordion').click(function () {
        if ($("#accordion").accordion("option", "active") != false) {
            $('#p1_accordianactive').val($("#accordion").accordion("option", "active"));
        }
        else {
            $('#p1_accordianactive').val('0');
        }
    });

    $('input[datatype=date]').datepicker();

    initDocFileUpload();
    initImgFileUpload();
    initImgDisplay();
    initDocDisplay();
    initCategoryDisplay();
    initPropertyDisplay();
    initRelatedDisplay();
    initClientDisplay();
    initOptionDisplay();
    initModelDisplay();

    $('.processing').hide();


}

    // ---------------------------------------------------------------------------


