

$(document).ready(function () {
    // start load all ajax data, continued by js in product.js file


    $(document).on("nbxgetcompleted", Payment_nbxgetCompleted); // assign a completed event for the ajax calls

    // function to do actions after an ajax call has been made.
    function Payment_nbxgetCompleted(e) {

        $('.processing').hide();

        if (e.cmd == 'payment_getlist') {

            $('.processing').show();
            $('#razortemplate').val('Payment.cshtml');
            //nbxget('payment_getlist', '#Paymentsearch', '#datadisplay');

        }

    };

});

