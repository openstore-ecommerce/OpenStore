// NBright JQuery plugin to generate ajax post of input fields - v1.0.1
(function ($) {

    // Usage: var values = $.fn.genxmlajax(selectordiv);
    // selectordiv: The div selector which encapsulates the controls for which data will be passed to the server.
    $.fn.genxmlajax = function (selectordiv) {
        return getgenxml(selectordiv);
    };

    $.fn.genxmlajaxitems = function (selectordiv, selectoritemdiv) {
        return getgenxmlitems(selectordiv, selectoritemdiv);
    };

    $.fn.popupformlist = function (selformdiv, sellistdiv, selpopupbutton, ajaxbutton, cmdupdate, width) {

        $(selformdiv).dialog({
            autoOpen: false,
            width: width,
            buttons: {
                "Ok": function () {

                    $(selformdiv).trigger('beforeupdate');

                    // get all the inputs into an array.
                    var values = getgenxml(selformdiv);
                    var request = $.ajax({ type: "POST",
                        url: cmdupdate,
                        data: { inputxml: escape(values) }
                    });

                    $(this).dialog("close");

                    request.done(function (data) {
                        $(selformdiv).trigger('afterupdate');
                        displayList(data, ajaxbutton, sellistdiv);
                    });

                    request.fail(function (jqXHR, textStatus) {
                        alert("Request failed: " + textStatus);
                    });

                },
                "Cancel": function () {
                    $(this).dialog("close");
                }
            }
        });

        // Dialog Link	
        $(selpopupbutton).click(function () {
            $(selformdiv).dialog('open');
            return false;
        });

    };

    $.fn.initlist = function (sellistdiv, ajaxbutton, cmdget) {
        $.ajaxSetup({ cache: false });
        $.get(cmdget, function (data) {
            displayList(data, ajaxbutton, sellistdiv);
        });
    };

    function displayList(data, ajaxbutton, sellistdiv) {
        $(sellistdiv).html(data);
        $(ajaxbutton).click(function () {
            var cmd = $(this).attr("cmd");
            $.get(cmd, function (data) {
                displayList(data, ajaxbutton, sellistdiv);
            });
        });
    };

    function getgenxmlitems(selectordiv, selectoritemdiv) {
        // get each item div into xml format.
        var values = "<root>";

        var $inputs = $(selectordiv).children(':input');
        $inputs.each(function () {
            values += getctrlxml($(this));
        });

        var $selects = $(selectordiv).children(' select');
        $selects.each(function () {
            strID = $(this).attr("id");
            nam = strID.split('_');
            var shortID = nam[nam.length - 1];
            var lp = 1;
            while (shortID.length < 4 && nam.length > lp) {
                lp++;
                shortID = nam[nam.length - lp];
            }

			var updAttr = $(this).attr("update");
			var strUpdate = '';
			if (updAttr != undefined) strUpdate = 'upd="' + updAttr + '"';

            values += '<f t="dd" ' + strUpdate + ' id="' + shortID + '" val="' + $(this).val() + '"><![CDATA[' + $('#' + strID + ' option:selected').text() + ']]></f>';
        });

        $(selectordiv).children(selectoritemdiv).each(function () {
            values += '<root>';
            var $iteminputs = $(this).find(':input');
            $iteminputs.each(function () {
                values += getctrlxml($(this));
            });

            var $itemselects = $(this).find(' select');
            $itemselects.each(function () {
                strID = $(this).attr("id");
                nam = strID.split('_');
                var shortID = nam[nam.length - 1];
                var lp = 1;
                while (shortID.length < 4 && nam.length > lp) {
                    lp++;
                    shortID = nam[nam.length - lp];
                }
				
				var updAttr = $(this).attr("update");
				var strUpdate = '';
				if (updAttr != undefined) strUpdate = 'upd="' + updAttr + '"';

                values += '<f t="dd" ' + strUpdate + ' id="' + shortID + '" val="' + $(this).val() + '"><![CDATA[' + $('#' + strID + ' option:selected').text() + ']]></f>';
            });

            values += '</root>';
        });

        values += '</root>';
        return values;
    };

    function getgenxml(selectordiv) {

        // get all the inputs into an array.
        var values = "<root>";

        var $inputs = $(selectordiv + ' :input');
        $inputs.each(function () {
            values += getctrlxml($(this));
        });

        var $selects = $(selectordiv + ' select');
        $selects.each(function () {
            strID = $(this).attr("id");
            nam = strID.split('_');
            var shortID = nam[nam.length - 1];
            var lp = 1;
            while (shortID.length < 4 && nam.length > lp) {
                lp++;
                shortID = nam[nam.length - lp];
            }
			var updAttr = $(this).attr("update");
			var strUpdate = '';
			if (updAttr != undefined) strUpdate = 'upd="' + updAttr + '"';

            values += '<f t="dd" ' + strUpdate + ' id="' + shortID + '" val="' + $(this).val() + '"><![CDATA[' + $('#' + strID + ' option:selected').text() + ']]></f>';
        });

        values += '</root>';

        return values;

    };


    function getctrlxml(element) {

        var values = "";
        var strID = element.attr("id");
        if (strID != undefined) {
			var parentflag = false;
	        var updAttr = element.attr("update");
			var strUpdate = '';
			if (updAttr != undefined)
				strUpdate = 'upd="' + updAttr + '"';
			else
			{
				if ($(element).parent() != undefined)
				{
					updAttr = $(element).parent().attr("update");
					if (updAttr != undefined) strUpdate = 'upd="' + updAttr + '"';
					parentflag = true;
				}
			}

            var nam = strID.split('_');
            var shortID = nam[nam.length - 1];
            var lp = 1;
            while (shortID.length < 4 && nam.length > lp) {
                lp++;
                shortID = nam[nam.length - lp];
            }
            if (element.attr("type") == 'radio') {
                values += '<f t="rb" ' + strUpdate + ' id="' + shortID + '" val="' + element.attr("value") + '"><![CDATA[' + element.is(':checked') + ']]></f>';
            } else if (element.attr("type") == 'checkbox') {
				var typecode = 'cb';
				if (parentflag) typecode = 'cbl';
					values += '<f t="' + typecode + '" ' + strUpdate + ' id="' + shortID + '" for="' + $('label[for=' + strID + ']').text() + '" val="' + element.attr("value") + '">' + element.is(':checked') + '</f>';
            } else if (element.attr("type") == 'text' || element.attr("type") == 'date' || element.attr("type") == 'email' || element.attr("type") == 'url' || element.attr("type") == 'number' || element.attr("type") == 'search') {
                if (element.attr("datatype") === undefined) {
                    values += '<f t="txt" ' + strUpdate + ' id="' + shortID + '"><![CDATA[' + element.val() + ']]></f>';
                } else {
                    values += '<f t="txt" ' + strUpdate + ' id="' + shortID + '" dt="' + element.attr("datatype") + '"><![CDATA[' + element.val() + ']]></f>';
                }
            } else if (element.attr("type") == 'hidden') {

                if (element.attr("datatype") == 'coded') {
                    var coded = '';
                    var str = element.val();
                    for (var i = 0; i < str.length; i++) {
                        coded = coded + str.charCodeAt(i) + '.';
                    }
                    values += '<f t="hid" ' + strUpdate + ' id="' + shortID + '" dt="' + element.attr("datatype") + '">' + coded + '</f>';
                } else {
                    values += '<f t="hid" ' + strUpdate + ' id="' + shortID + '"><![CDATA[' + element.val() + ']]></f>';
                }


            } else {
                values += '<f ' + strUpdate + ' id="' + shortID + '"><![CDATA[' + element.val() + ']]></f>';
            }
        }

        return values;

    };



})(jQuery);


