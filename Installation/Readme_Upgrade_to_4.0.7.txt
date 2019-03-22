Upgrading Open Store to 4.0.7
-----------------------------

In most situation no change will be neeeded, unless you use a bespoke ajax template system.

The changes in v4.0.7 are due to search robots sometimes failing to index products, due to the ajax call not being follow.

IF YOU DO NOT USE AJAX THERE IS NO CHANGE.

- The main change is that on the first render, all products for the list are rendered as part of the module, and not an ajax call.  This makes the list standard html for the first load.

- The initial loadproducts() function has been disabled in all ajax templates.

- In the example theme "ClassicAjax" some templates have been changed:

"ProductDisplayAjax.cshtml", Calls the list directly.
"ProductDisplayAjaxList.cshtml", has had the paging display added to the bottom of the template.
"ProductDisplayAjaxList_paging.cshtml", has been removed and the code placed in the "ProductDisplayAjaxList.cshtml" template.

- The paging has had the ajax call replaced with a standard href, to ensure the search engine follows the link.  This can be replaced by simply adding the "ajaxpager" class to the page link in the "ProductDisplayAjaxList.cshtml" template. 

- The first template called on page reload is "ProductDisplayAjax.cshtml", if you have a portal level template remember to have this file at that level and make sure the template call paths are correct.

NOTE: At the momnet it is unsure if the Category Menu is followed if the ajax is activated.
We believe it should be followed. We are currently running tests to see the impact of the ajax menu.  However, but to ensure indexing of the products a normal link category should be in place, somewhere on the site, so that search crawlers can follow all links.

