/// LEGACY CODE - WILL BE REMOVED IN FUTURE
/// -------------------------------------------------------------------------

var nbxajaxrtn = 'input[id*="nbxajaxrtn"]';

function nbxajaxaction(selector)
{		 
    $.ajaxSetup({ cache: false });       
	$(selector).click(function () {
		 var cmd = $(this).attr("cmd");		
		 if (cmd===undefined)
		 {
			$(nbxajaxrtn).val('ERROR - cannot find cmd attribute in button: ' + selector);
		 }
		 else {

		     alert(cmd);

		     var request = $.ajax({
		         type: "POST",
		         url: cmd,
		         cache: false
		     });

		     request.done(function (data) {
		         if (data != 'noaction') {
		             $(nbxajaxrtn).val(data).trigger('change');
		         }

		     });

		     request.fail(function (jqXHR, textStatus) {
		         alert("Request failed: " + textStatus);
		     });

		 }		
		 
	});
}

function nbxbuttonview(selector)
{
	if (selector===undefined) selector = nbxajaxrtn;	
	
	// START: Do itemlist buttons
	$('a[id*="nbxItemListAdd"]').each(function(idx, item) {	
		var n = $(selector).val().indexOf("," + item.target + ",");
		if (n >= 0)
		{		
			jQuery('#' + item.id).hide();
			jQuery('#' + item.id.replace('nbxItemListAdd','nbxItemListRemove')).show();
		}
		else
		{
			jQuery('#' + item.id).show();
			jQuery('#' + item.id.replace('nbxItemListAdd','nbxItemListRemove')).hide();
		}		
	});
	var ary = $(selector).val().split(",");
	if (ary.length > 1) 
		$(".nbxItemListCount").html(ary.length - 2);
	else
		$(".nbxItemListCount").html("0");
	// END: Do itemlist buttons
	
}


$(document).ready(function() {

$(nbxajaxrtn).change(function() {	
	nbxbuttonview();
});

});
