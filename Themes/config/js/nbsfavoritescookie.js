    //---------------------------------------------------------------
    //----- functions required to deal with favorites cookie --------
    //---------------------------------------------------------------


function AddToFavCookie(itemid, name) {
    var favlist = Cookies.get(name);
    if (favlist == null || favlist == 'undefined') favlist = "";
    var favarray = favlist.split('*');
    var a = favarray.indexOf(itemid);
    if (a == -1) {
        favlist += itemid + "*";
        Cookies.set(name, favlist, { expires: 365, path: '/' });
    }
};

    function RemoveToFavCookie(itemid, name) {
        var favlist = Cookies.get(name);
        if (favlist != null && favlist != 'undefined') {
            var favarray = favlist.split('*');
            var a = favarray.indexOf(itemid);
            if (a >= 0) {
                favarray.splice(a, 1);
                favlist = "";
                for (var i = 0; i < favarray.length; i++) {
                    if (favarray[i] != '') favlist += favarray[i] + '*';
                }
                Cookies.set(name, favlist, { expires: 365, path: '/' });
            }
        }
    };


    function showhidebuttons(name) {
        var favlist = Cookies.get(name);
        if (favlist != null && favlist != 'undefined') {
            var favarray = favlist.split('*');
            $('.wishlistadd').each(function (idx, item) {
                var itemid = $(this).attr("itemid");
                var n = favarray.indexOf(itemid);
                if (n >= 0) {
                    $(this).hide();
                    $('.wishlistremove[itemid="' + itemid + '"]').show();
                }
                else {
                    $(this).show();
                    $('.wishlistremove[itemid="' + itemid + '"]').hide();
                }
            });

            if (favarray.length > 1)
                $("." + name + "count").html(favarray.length - 1);
            else
                $("." + name + "count").html("0");
        }

    }



    $(document).ready(function () {

        $('.wishlistadd').click(function () {
            var name = $(this).attr('listname');
            AddToFavCookie($(this).attr('itemid'), name);
            showhidebuttons(name);
        });

        $('.wishlistremove').click(function () {
            var name = $(this).attr('listname');
            RemoveToFavCookie($(this).attr('itemid'), name);
            showhidebuttons(name);
        });

        $('.wishlistremoveall').click(function () {
            var name = $(this).attr('listname');
            Cookies.remove(name, { path: '/' });
            location.reload(true);
        });

    });