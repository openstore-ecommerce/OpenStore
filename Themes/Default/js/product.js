
$(document).ready(function () {

    $(document).on("nbxproductgetcompleted", AjaxView_GetList_nbxproductgetCompleted); // assign a completed event for the ajax calls

    $('.nav-tabs > li > a').click(function (event) {
        event.preventDefault();//stop browser to take action for clicked anchor

        //get displaying tab content jQuery selector
        var active_tab_selector = $('.nav-tabs > li.tab-active > a').attr('href');

        //find actived navigation and remove 'active' css
        var actived_nav = $('.nav-tabs > li.tab-active');
        actived_nav.removeClass('tab-active');

        //add 'active' css into clicked navigation
        $(this).parents('li').addClass('tab-active');

        //hide displaying tab content
        $(active_tab_selector).removeClass('tab-active');
        $(active_tab_selector).addClass('tab-hide');

        //show target tab content
        var target_tab_selector = $(this).attr('href');
        $(target_tab_selector).removeClass('tab-hide');
        $(target_tab_selector).addClass('tab-active');
    });


    // *****************************************************************************
    // *******  Events for product detail (not ajax)
    // *****************************************************************************
    $(".quantity").keydown(function (e) {
        if (e.keyCode === 8 || e.keyCode <= 46) return; // Allow: backspace, delete.
        if ((e.keyCode >= 35 && e.keyCode <= 39)) return; // Allow: home, end, left, right
        // Ensure that it is a number and stop the keypress
        if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) e.preventDefault();
    });

    $('.qtyminus').click(function () {
        if (parseInt($('.quantity').val()) > 1)
            $('.quantity').val(parseInt($('.quantity').val()) - 1);
        else
            $('.quantity').val('1');
    });

    $('.qtyplus').click(function () {
        $('.quantity').val(parseInt($('.quantity').val()) + 1);
        if ($('.quantity').val() === 'NaN') $('.quantity').val('1');
    });

    addtobasketclick();

    $('.processingproduct').hide();

});


// *****************************************************************************
// *******  Ajax Completed actions
// *****************************************************************************


function AjaxView_GetList_nbxproductgetCompleted(e) {

    $('.ajaxcatmenu').unbind("click");
    $('.ajaxcatmenu').click(function (event) {
        event.preventDefault();

        $(".active").removeClass("active");
        $(this).parent().addClass("active");

        var catid = $(this).attr("catid");
        if (typeof catid !== 'undefined') {
            $('#catid').val(catid);
            $('#filter_catid').val(catid);
        }

        clearPagerVals();

        // add to browser bar and history
        var stateObj = { catid: $(this).attr("catid") };
        history.pushState(stateObj, "Title", $(this).attr("href"));

        var moduleid = $("#moduleid").val();
        setPagerVals(moduleid);

        loadProducts(moduleid);
        loadFilters(moduleid);
    });

    $('.ajaxpager').unbind("click");
    $('.ajaxpager').click(function (event) {
        event.preventDefault();

        var pagenumber = $(this).attr("pagenumber");
        if (typeof pagenumber === 'undefined') {
            pagenumber = "1";
        }
        $("#page").val(pagenumber);

        clearPagerVals();

        var moduleid = this.id.replace("ajaxpager", "");
        setPagerVals(moduleid);

        loadProducts(moduleid);
    });

    if (e.cmd === 'cart_addtobasket') {
        $('.processingminicart').show();
        $('input[id*="optionfilelist"]').val(''); // clear any clientupload file names
        $('#carttemplate').val('MiniCart.cshtml');
        nbxproductget('cart_renderminicart', '#productajaxview', '.container_classicajax_minicart'); // Reload Cart
        $('.addedtobasket').delay(1000).fadeOut('fast');
    }

    if (e.cmd === 'cart_renderminicart') {
        $('.processingminicart').hide();
    }

    if (e.cmd === 'product_ajaxview_getlist') {
        var moduleid = $("#moduleid").val();
        $('.sortorderselect').unbind("change");
        $('.sortorderselect').change(function () {
            $('#orderby').val($(this).val());
            loadProductList(moduleid);
        });
        loadFilters(moduleid);
        $('.processingproductajax').hide();
    }

    if (e.cmd === 'product_ajaxview_getfilters') {
        // swith moduleid param back to list after change for filter
        $('#moduleid').val($('#list_moduleid').val());

        // propertyFilterClicked method is in AjaxDisplayProductList_head
        $(".nbs-ajaxfilter input[type='checkbox']").change(propertyFilterClicked);

        if (typeof $.cookie('shopredirectflag') !== 'undefined') {
            var shopredirectflag = $.cookie("shopredirectflag");
            if (shopredirectflag !== 0) {
                $.cookie("shopredirectflag", 0);
                $('#shopitemid').val(shopredirectflag);
                $(".shoppinglistadd[itemid='" + $('#shopitemid').val() + "']").click();
            }
        }
        $("html, body").animate({ scrollTop: 0 }, 200);

        addtobasketclick();
        $('.processingfilter').hide();
    }

    if (e.cmd === 'product_ajaxview_getlistfilter') {
        addtobasketclick();
        $('.processingfilter').hide();
    }

    if (e.cmd === 'itemlist_add') {
        $('#shoppinglistadd-' + $('#shopitemid').val()).hide();
        $('#shoppinglistremove-' + $('#shopitemid').val()).show();
        $('#shopitemid').val('');
        $('.processinglist').hide();
    }

    if (e.cmd === 'itemlist_remove') {
        $('#shoppinglistadd-' + $('#shopitemid').val()).show();
        $('#shoppinglistremove-' + $('#shopitemid').val()).hide();
        $('#shopitemid').val('');
        $('.processinglist').hide();
    }

    if (e.cmd === 'itemlist_getpopup') {
        $('.processinglist').hide();
    }

    // Shopping List Popup
    $('.shoppinglistadd').unbind("click");
    $('.shoppinglistadd').click(function () {
        $('#shopitemid').val($(this).attr('itemid'));
        $(".shoppinglistadd").colorbox({
            inline: true, maxWidth: "230px", maxHeight: "250px", fixed: true,
            onClosed: function () {
                $('#shopitemid').val('');
            }
        });
    });

    // Wish List events
    $('.wishlistadd').unbind("click");
    $('.wishlistadd').click(function () {
        $('#shoplistname').val($('.shoplistselect').val());
        nbxproductget('itemlist_add', '#productajaxview'); //apply serverside
        $('#shoppinglistpopup').colorbox.close();
    });

    $('.shoppinglistremove').unbind("click");
    $('.shoppinglistremove').click(function () {
        $('#shopitemid').val($(this).attr('itemid'));
        $('.productline' + $(this).attr('itemid')).hide();
        nbxproductget('itemlist_remove', '#productajaxview'); //apply serverside
    });

    $('.wishlistremoveall').unbind("click");
    $('.wishlistremoveall').click(function () {
        nbxproductget('itemlist_delete', '#productajaxview'); //apply serverside
    });

    $('.shoplistselect').unbind("change");
    $('.shoplistselect').change(function () {
        if ($(this).val() === '-1') {
            $('.newlistdiv').show();
            $('.wishlistadd').hide();
            $('.shoplistselect').hide();
        } else {
            $('#shoplistname').val($(this).val());
            $('.newlistdiv').hide();
            $('.wishlistadd').show();
            $('.shoplistselect').show();
        }
    });

    $('.cancelnewlist').unbind("click");
    $('.cancelnewlist').click(function () {
        $('.newlistdiv').hide();
        $('.wishlistadd').show();
        $('.shoplistselect').show();
        $('.shoplistselect>option:eq(0)').attr('selected', true);
    });

    $('.addnewlist').unbind("click");
    $('.addnewlist').click(function () {
        var newlisttext = $('.newlisttext').val();
        //$('.shoplistselect').append('<option value="' + newlisttext + '" selected="selected">' + newlisttext + '</option>');
        $('.shoplistselect').append(new Option(newlisttext, newlisttext, false, true));
        $('.newlistdiv').hide();
        $('.wishlistadd').show();
        $('.shoplistselect').show();
    });

    $('.redirecttologin').unbind("click");
    $('.redirecttologin').click(function () {
        $.cookie("shopredirectflag", $('#shopitemid').val());
        window.location.replace('/default.aspx?tabid=' + $('#logintab').val() + '&returnurl=' + location.protocol + '//' + location.host + location.pathname);
    });
    $('.cancellogin').unbind("click");
    $('.cancellogin').click(function () {
        $.cookie("shopredirectflag", 0);
        $('#shoppinglistpopup').colorbox.close();
    });

}



// *****************************************************************************
// *******  Browser Functions
// *****************************************************************************


// call ajax based on browser history
window.onpopstate = function (e) {
    var currentState = history.state;
    if (currentState) {
        $('#catid').val(currentState.catid);
        $('#filter_catid').val(currentState.catid);
        loadProductList();
    }
};



// *****************************************************************************
// *******  Functions
// *****************************************************************************

function pageSizeChanged() {
    var moduleid = this.id.replace("pagesizedropdown", "");
    $("#page").val($("#ajaxproducts" + moduleid + " .NBrightSelectPg a").attr("pagenumber"));
    setPagerVals(moduleid);
    loadProducts(moduleid);
}

function sortOrderChanged() {
    var moduleid = this.id.replace("sortorderselect", "");
    $("#page").val($("#ajaxproducts" + moduleid + " .NBrightSelectPg a").attr("pagenumber"));
    setPagerVals(moduleid);
    loadProducts(moduleid);
}

function propertyFilterClicked() {
    var list = "";
    $(".nbs-ajaxfilter input[type='checkbox']:checked").each(function () {
        list += $(this).val() + ",";
    });
    $("#propertyfilter").val(list);
    loadProductListFilter();
}

function loadProducts(moduleid) {
    if (typeof moduleid !== 'undefined') moduleid = $('#moduleid').val();
    if ($("#propertyfilter").val() !== "") {
        loadProductListFilter(moduleid);
    }
    else {
        loadProductList(moduleid);
    }
}

function loadProductList(moduleid) {
    if (typeof moduleid !== 'undefined') moduleid = $('#moduleid').val();
    if ($('#ajaxlist').val() === 'True') {
        $('.processingproductajax').show();
        nbxproductget('product_ajaxview_getlist', '#productajaxview', '#ajaxproducts' + moduleid);
    } else {
        addtobasketclick();
    }
}

function loadProductListFilter(moduleid) {
    if (typeof moduleid === 'undefined') moduleid = $('#moduleid').val();
    $('.processingfilter').show();
    nbxproductget('product_ajaxview_getlistfilter', '#productajaxview', '#ajaxproducts' + moduleid);
}

function loadItemListPopup() {
    $('.processinglist').show();
    nbxproductget('itemlist_getpopup', '#productajaxview', '#shoppinglistpopup');
}

function loadFilters() {
    $('.processingfilter').show();
    $("#propertyfiltertypeinside").val($("#filter_propertyfiltertypeinside").val());
    $("#propertyfiltertypeoutside").val($("#filter_propertyfiltertypeoutside").val());
    $("#moduleid").val($("#filter_moduleid").val());
    nbxproductget('product_ajaxview_getfilters', '#productajaxview', '#ajaxfilter');
}

function IsInFavorites(productid) {
    var productitemlist = $.cookie("NBSShoppingList");
    if (typeof productitemlist !== 'undefined') {
        if (productitemlist.indexOf(productid + '*') !== -1) {
            return true;
        }
    }
    return false;
}

function showImage(imgPath, imgAlt) {
    var imgMain = document.getElementById('mainimage');
    imgMain.src = imgPath;
    imgMain.alt = imgAlt;
}

function setPagerVals(moduleid) {
    $("#moduleid").val(moduleid);
    $("#pagesize").val($("#pagesizedropdown" + moduleid).val());    
    $("#orderby").val($("#sortorderselect" + moduleid).val());
}

function clearPagerVals() {
    $('#catref').val('');
    $('#filter_catref').val('');
    $('#searchtext').val('');
}

function addtobasketclick() {
    //Form validation 
    var form = $("#Form");
    form.validate();
    $('.addtobasket').unbind("click");
    $('.addtobasket').click(function () {
        if (form.valid()) {
            $('.processingminicart').show();
            if (parseInt($('.quantity').val()) < 1) $('.quantity').val('1');
            nbxproductget('cart_addtobasket', '.entryid' + $(this).attr('itemid'), '#minicartdatareturn'); // Reload Cart
            $('.addedtobasket').delay(10).fadeIn('fast');
        }
    });

}