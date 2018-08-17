
function nbxget(cmd, selformdiv, target, selformitemdiv, appendreturn)
{
    $.ajaxSetup({ cache: false });

    var cmdupdate = '/DesktopModules/NBright/NBrightBuy/XmlConnector.ashx?cmd=' + cmd;
    var values = '';
    if (selformitemdiv == null) {
        values = $.fn.genxmlajax(selformdiv);        
    }
    else {
        values = $.fn.genxmlajaxitems(selformdiv, selformitemdiv);
    }
    var request = $.ajax({ type: "POST",
		url: cmdupdate,
		cache: false,
        timeout: 90000,
		data: { inputxml: encodeURI(values) }		
	});

	request.done(function (data) {
	    if (data != 'noaction') {
	        if (target != '' && target != null) {
	            if (appendreturn == null) {
	                $(target).children().remove();
	                $(target).html(data).trigger('change');
	            } else
	                $(target).append(data).trigger('change');
	        }

	        $.event.trigger({
	            type: "nbxgetcompleted",
	            cmd: cmd
	        });
	    }

	});

	request.fail(function (jqXHR, textStatus) {
	    $('#loader').hide('');
        alert("Request failed: " + textStatus + " : " + cmd);
	});
}

	function nbxcheckifnull(selector,searchtoken)
	{ // check if the value is empty, if so do NOT build the SQL.  Add it to disabledlist token.
		var disabledlist = $("input[id*='disabledsearchtokens']").val();
		if ($(selector).val() == '')
			disabledlist = disabledlist.replace(';' + searchtoken,'') + ';' + searchtoken;
		else
			disabledlist = disabledlist.replace(';' + searchtoken,'');
		$("input[id*='disabledsearchtokens']").val(disabledlist);
	}

	function nbxcheckifselected(selector,searchtoken)
	{ // check if checboxlist is selected. If not, do not build the SQL.  Add it to disabledlist token.
		var disabledlist = $("input[id*='disabledsearchtokens']").val();
		var sel = false;
		$(selector).each(function() { 
			if (this.checked)
			{	
				sel = true; 
				return false;
			}
        });	
		if (sel)
			disabledlist = disabledlist.replace(';' + searchtoken,'');
		else
			disabledlist = disabledlist.replace(';' + searchtoken,'') + ';' + searchtoken;
		$("input[id*='disabledsearchtokens']").val(disabledlist);
	}

	function nbxposturl(cmd, params) {
	    $.ajaxSetup({ cache: false });

	    var cmdupdate = '/DesktopModules/NBright/NBrightBuy/XmlConnector.ashx?cmd=' + cmd + "&" + params;
	    var request = $.ajax({
	        type: "POST",
	        url: cmdupdate,
	        cache: false,
	        timeout: 90000
	    });

	    request.done(function (data) {
	        if (data != 'noaction') {
	            $.event.trigger({
	                type: "nbxgetcompleted",
	                cmd: cmd
	            });
	        }
	    });

	    request.fail(function (jqXHR, textStatus) {
	        $('#loader').hide('');
	        alert("Request failed: " + textStatus);
	    });
	}



	function nbxproductget(cmd, selformdiv, target, selformitemdiv, appendreturn) {
	    $.ajaxSetup({ cache: false });

	    var cmdupdate = '/DesktopModules/NBright/NBrightBuy/XmlConnector.ashx?cmd=' + cmd;
	    var values = '';
	    if (selformitemdiv == null) {
	        values = $.fn.genxmlajax(selformdiv);
	    }
	    else {
	        values = $.fn.genxmlajaxitems(selformdiv, selformitemdiv);
	    }
	    var request = $.ajax({
	        type: "POST",
	        url: cmdupdate,
	        cache: false,
	        timeout: 90000,
	        data: { inputxml: encodeURI(values) }
	    });

	    request.done(function (data) {
	        if (data != 'noaction') {
	            if (target != '' && target != null) {
	                if (appendreturn == null) {
	                    $(target).children().remove();
	                    $(target).html(data).trigger('change');
	                } else
	                    $(target).append(data).trigger('change');
	            }

	            $.event.trigger({
	                type: "nbxproductgetcompleted",
	                cmd: cmd
	            });
	        }

	    });

	    request.fail(function (jqXHR, textStatus) {
	        $('#loader').hide('');
           // alert("Request failed: " + textStatus + " : " + cmd);
	    });
	}