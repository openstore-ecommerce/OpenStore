
$(document).ready(function () {

    $('#cmdDeleteCart').click(function () {
        var msg = $('#cmdClearCart').val();
        if (confirm(msg)) {
            $('.processing').show();
            nbxget('cart_clearcart', '#productajaxview');
        }
    });

    $('#cmdRecalcCart').click(function () {
        $('.processing').show();
        $('#cmdGoCheckout').show();
        nbxget('cart_recalculatecart', '.cartdatarow', '', '.quantitycolumn');
    });

    $('#cmdGoCheckout').click(function () {
        $('.processing').show();
        nbxget('cart_redirecttocheckout', '.cartdatarow', '', '.quantitycolumn');
    });

    $(document).on("nbxgetcompleted", Cart_nbxgetCompleted); // assign a completed event for the ajax calls

    // function to do actions after an ajax call has been made.
    function Cart_nbxgetCompleted(e) {

        $('.processing').hide();

        if (e.cmd == 'cart_rendercartlist') {

            $('.removeitem').unbind();
            $('.removeitem').click(function() {
                $('.processing').show();
                $('#itemcode').val($(this).attr('itemcode'));
                nbxget('cart_removefromcart', '#productajaxview');
            });

            $('.processing').hide();

            // if we have a cartempty element hide the action buttons
            if ($('#cartempty').text() != '') {
                $('#cartdetails').hide();
            } else {
                $('#cartdetails').show();
            }


            $(".quantity").keydown(function(e) {
                if (e.keyCode == 8 || e.keyCode <= 46) return; // Allow: backspace, delete.
                if ((e.keyCode >= 35 && e.keyCode <= 39)) return; // Allow: home, end, left, right
                // Ensure that it is a number and stop the keypress
                if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) e.preventDefault();
            });

            $('.qtyminus').unbind();
            $('.qtyminus').click(function() {
                var oldqty = $('.itemcode' + $(this).attr('itemcode')).val();
                var newQty = parseInt(oldqty, 10);
                if (isNaN(newQty)) {
                    newQty = 2;
                }
                if (newQty >= 1) {
                    --newQty;
                    $('.itemcode' + $(this).attr('itemcode')).val(newQty);
                }
            });
            $('.qtyplus').unbind();
            $('.qtyplus').click(function() {
                var oldqty = $('.itemcode' + $(this).attr('itemcode')).val();
                var newQty = parseInt(oldqty, 10);
                if (isNaN(newQty)) {
                    newQty = 0;
                }
                ++newQty;
                $('.itemcode' + $(this).attr('itemcode')).val(newQty);
            });

        }

        if (e.cmd == 'cart_recalculatecart' || e.cmd == 'cart_removefromcart' || e.cmd == 'cart_clearcart') {
            $('#carttemplate').val('FullCartList.cshtml');
            $('.processing').show();
            nbxget('cart_rendercartlist', '#productajaxview', '#checkoutitemlist');
        }
        
        if (e.cmd == 'cart_redirecttocheckout') {
            $('.processing').show();
            var redirecturl = $('#checkouturl').val();
            window.location.href = redirecturl + '?cartstep=2';
        }

    }


});
