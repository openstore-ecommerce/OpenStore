// --- Copyright (c) notice NevoWeb ---
//  Copyright (c) 2014 SARL NevoWeb.  www.nevoweb.com. The MIT License (MIT).
// Author: D.C.Lee
// ------------------------------------------------------------------------
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ------------------------------------------------------------------------
// This copyright notice may NOT be removed, obscured or modified without written consent from the author.
// --- End copyright notice --- 

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Users;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using NBrightDNN.render;
using Nevoweb.DNN.NBrightBuy.Base;
using Nevoweb.DNN.NBrightBuy.Components;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;
using RazorEngine;
using DataProvider = DotNetNuke.Data.DataProvider;

namespace Nevoweb.DNN.NBrightBuy
{

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ViewNBrightGen class displays the content
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class ProductRazorView : NBrightBuyFrontOfficeBase
    {

        private String _eid = "";
        private String _ename = "";
        private String _catid = "";
        private String _catname = "";
        private String _modkey = "";
        private String _pagemid = "";
        private String _pagenum = "1";
        private String _pagesize = "";
        private String _templD = "";
        private Boolean _displayentrypage = false;
        private String _orderbyindex = "";
        private NavigationData _navigationdata;
        public String EntityTypeCode = "PRD";
        public String EntityTypeCodeLang = "PRDLANG";
        private String _itemListName = "";
        private String _print = "";
        private String _printtemplate = "";
        private String _guidkey = "";
        private string _controlPath = "";

        #region Event Handlers

        override protected void OnInit(EventArgs e)
        {
            _eid = Utils.RequestQueryStringParam(Context, "eid");
            _print = Utils.RequestParam(Context, "print");
            _printtemplate = Utils.RequestParam(Context, "template");
            _controlPath = ControlPath;

            EnablePaging = true;
            
            base.OnInit(e);

            if (ModSettings != null)
            {
                // check if we're using a typcode for the data.
                var modentitytypecode = ModSettings.Get("entitytypecode");
                if (modentitytypecode != "")
                {
                    EntityTypeCode = modentitytypecode;
                    EntityTypeCodeLang = modentitytypecode + "LANG";
                }
                // check if we're using a provider controlpath for the templates.
                var providercontrolpath = ModSettings.Get("providercontrolpath");
                if (providercontrolpath != "")
                {
                    _controlPath = "/DesktopModules/NBright/" + providercontrolpath + "/";
                }
            }


            // if guidkey entered instead of eid, find it using the guid and assign to _eid
            _guidkey = Utils.RequestQueryStringParam(Context, "guidkey");
            if (_guidkey == "") _guidkey = Utils.RequestQueryStringParam(Context, "ref");

            Logging.Info($"ProductRazorView requested with: eid={_eid}, guidkey={_guidkey}.");

            if (_eid== "" && _guidkey != "")
            {
                var guidData = ModCtrl.GetByGuidKey(PortalId, -1, EntityTypeCode, _guidkey);
                if (guidData != null)
                    _eid = guidData.ItemID.ToString("D");
                else
                    _eid = "0";
            }

            // if we want to print we need to open the browser with a startup script, this points to a Printview.aspx. (Must go after the ModSettings has been init.)
            if (_print != "") Page.ClientScript.RegisterStartupScript(this.GetType(), "printproduct", "window.open('" + StoreSettings.NBrightBuyPath() + "/PrintView.aspx?itemid=" + _eid + "&printcode=" + _print + "&template=" + _printtemplate + "&theme=" + ModSettings.Get("themefolder") + "','_blank');", true);

            if (ModuleKey == "")  // if we don't have module setting jump out
            {
                var lit = new Literal();
                lit.Text = "NO MODULE SETTINGS";
                phData.Controls.Add(lit);
                return;
            }

            _navigationdata = new NavigationData(PortalId, ModuleKey);

            // Pass in a template specifying the token to create a friendly url for paging. 
            // (NOTE: we need this in NBS becuase the edit product from list return url will copy the page number and hence paging will not work after editing if we don;t do this)
            CtrlPaging.HrefLinkTemplate = "[<tag type='valueof' databind='PreText' />][<tag type='if' databind='Text' testvalue='' display='{OFF}' />][<tag type='hrefpagelink' moduleid='" + ModuleId.ToString("") + "' />][<tag type='endif' />][<tag type='valueof' databind='PostText' />]";
            CtrlPaging.UseListDisplay = true;
            try
            {

                _catid = Utils.RequestQueryStringParam(Context, "catid");
                _catname = Utils.RequestQueryStringParam(Context, "catref");

                #region "set templates based on entry id (eid) from url"

                _ename = Utils.RequestQueryStringParam(Context, "entry");
                _modkey = Utils.RequestQueryStringParam(Context, "modkey");
                _pagemid = Utils.RequestQueryStringParam(Context, "pagemid");
                _pagenum = Utils.RequestQueryStringParam(Context, "page");
                _pagesize = Utils.RequestQueryStringParam(Context, "pagesize");
                _orderbyindex = Utils.RequestQueryStringParam(Context, "orderby");

                // see if we need to display the entry page.
                if ((_modkey == ModuleKey | _modkey == "") && (_eid != "" | _ename != "")) _displayentrypage = true;

                // if we have entry detail display, but no catd, get the default one.
                if (_displayentrypage && _catid == "" && Utils.IsNumeric(_eid))
                {
                    var prdData = ProductUtils.GetProductData(Convert.ToInt32(_eid),Utils.GetCurrentCulture(), true, EntityTypeCode);
                    var defcat = prdData.GetDefaultCategory();
                    if (defcat != null) _catid = defcat.categoryid.ToString("");
                }

                if (ModSettings.Get("listonly").ToLower() == "true") _displayentrypage = false;

                // get template codes
                if (_displayentrypage)
                {
                    _templD = ModSettings.Get("razordetailtemplate");
                    if (_templD == "") _templD = ModSettings.Get("txtdisplayentrybody"); // legacy name
                }
                else
                {
                    _templD = ModSettings.Get("razorlisttemplate");
                    if (_templD == "") _templD = ModSettings.Get("txtdisplaybody"); // legacy name
                }

                #endregion

            }
            catch (Exception exc)
            {
                // remove any cookie which might store SQL in error.
                _navigationdata.Delete();
                DisplayProductError(null, exc.ToString());
            }

        }

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                if (Page.IsPostBack == false)
                {
                    // do razor code
                    RazorPageLoad();
                }
            }
            catch (Exception exc) //Module failed to load
            {
                //display the error on the template (don;t want to log it here, prefer to deal with errors directly.)
                var l = new Literal();
                l.Text = exc.ToString();
                phData.Controls.Add(l);
                // remove any nav data which might store SQL in error.
                _navigationdata.Delete();
            }
        }

        private void RazorPageLoad()
        {
            NBrightInfo objCat = null;

            if (_templD.Trim() != "")  // if we don;t have a template, don't do anything
            {

                if (_displayentrypage)
                {
                    // get correct itemid, based on eid given
                    _eid = GetEntryIdFromName(_eid);
                    RazorDisplayDataEntry(_eid);

                }
                else
                {
                    // Get meta data from template

                    var metaTokens = NBrightBuyUtils.RazorPreProcessTempl(_templD, _controlPath, ModSettings.ThemeFolder, Utils.GetCurrentCulture(), ModSettings.Settings(), ModuleId.ToString());

                    #region "Order BY"

                    ////////////////////////////////////////////
                    // get ORDERBY SORT 
                    ////////////////////////////////////////////
                    if (_orderbyindex != "") // if we have orderby set in url, find the meta tags
                    {
                        if (metaTokens.ContainsKey("orderby" + _orderbyindex))
                        {
                            if (metaTokens["orderby" + _orderbyindex].Contains("{") || metaTokens["orderby" + _orderbyindex].ToLower().Contains("order by"))
                            {
                                _navigationdata.OrderBy = metaTokens["orderby" + _orderbyindex];
                                _navigationdata.OrderByIdx = _orderbyindex;
                            }
                            else
                            {
                                _navigationdata.OrderBy = " Order by " + metaTokens["orderby" + _orderbyindex];
                                _navigationdata.OrderByIdx = _orderbyindex;
                            }
                            _navigationdata.Save();
                        }
                    }
                    else
                    {
                        if (String.IsNullOrEmpty(_navigationdata.OrderBy) && metaTokens.ContainsKey("orderby"))
                        {
                            if (metaTokens["orderby"].Contains("{") || metaTokens["orderby"].ToLower().Contains("order by"))
                            {
                                _navigationdata.OrderBy = metaTokens["orderby"];
                            }
                            else
                            {
                                _navigationdata.OrderBy = " Order by " + metaTokens["orderby"];
                            }
                            _navigationdata.OrderByIdx = "";
                            _navigationdata.Save();
                        }
                    }


                    #endregion

                    #region "Get Paging setup"

                    //See if we have a pagesize, uses the "searchpagesize" tag token.
                    // : This can be overwritten by the cookie value if we need user selection of pagesize.
                    CtrlPaging.Visible = false;

                    #region "Get pagesize, from best place"

                    var pageSize = 0;
                    if (Utils.IsNumeric(ModSettings.Get("pagesize"))) pageSize = Convert.ToInt32(ModSettings.Get("pagesize"));
                    // overwrite default module pagesize , if we have a pagesize control in the template
                    if (metaTokens.ContainsKey("selectpagesize") && Utils.IsNumeric(_navigationdata.PageSize))
                    {
                        pageSize = Convert.ToInt32(_navigationdata.PageSize);
                    }
                    //check for url param page size
                    if (Utils.IsNumeric(_pagesize) && (_pagemid == "" | _pagemid == ModuleId.ToString(CultureInfo.InvariantCulture))) pageSize = Convert.ToInt32(_pagesize);
                    if (pageSize == 0)
                    {
                        var strPgSize = "";
                        if (metaTokens.ContainsKey("searchpagesize")) strPgSize = metaTokens["searchpagesize"];
                        if (metaTokens.ContainsKey("pagesize") && strPgSize == "") strPgSize = metaTokens["pagesize"];
                        if (Utils.IsNumeric(strPgSize)) pageSize = Convert.ToInt32(strPgSize);
                    }
                    if (pageSize > 0) CtrlPaging.Visible = true;
                    _navigationdata.PageSize = pageSize.ToString("");

                    #endregion

                    var pageNumber = 1;
                    //check for url param paging
                    if (Utils.IsNumeric(_pagenum) && (_pagemid == "" | _pagemid == ModuleId.ToString(CultureInfo.InvariantCulture)))
                    {
                        pageNumber = Convert.ToInt32(_pagenum);
                    }

                    //Get returnlimt from module settings
                    var returnlimit = 0;
                    var strreturnlimit = ModSettings.Get("returnlimit");
                    if (Utils.IsNumeric(strreturnlimit)) returnlimit = Convert.ToInt32(strreturnlimit);

                    #endregion

                    #region "Get filter setup"

                    // check the display header to see if we have a sqlfilter defined.
                    var strFilter = "";
                    var sqlTemplateFilter = "";
                    if (metaTokens.ContainsKey("sqlfilter")) sqlTemplateFilter = GenXmlFunctions.StripSqlCommands(metaTokens["sqlfilter"]);

                    if (_navigationdata.HasCriteria)
                    {
                        var paramcatid = Utils.RequestQueryStringParam(Context, "catid");
                        if (Utils.IsNumeric(paramcatid))
                        {
                            if (_navigationdata.CategoryId != Convert.ToInt32(paramcatid)) // filter mode DOES NOT persist catid (stop confusion when user selects a category)
                            {
                                _navigationdata.ResetSearch();
                            }
                        }

                        // if navdata is not deleted then get filter from navdata, created by productsearch module.
                        strFilter = _navigationdata.Criteria;
                        if (!strFilter.Contains(sqlTemplateFilter)) strFilter += " " + sqlTemplateFilter;

                        if (_navigationdata.Mode.ToLower() == "s") _navigationdata.ResetSearch(); // single search so clear after
                    }
                    else
                    {
                        // reset search if category selected 
                        // NOTE: keeping search across categories is VERY confusing for cleint, although it works logically.
                        _navigationdata.ResetSearch();
                        strFilter = sqlTemplateFilter;
                    }

                    #endregion

                    #region "Get Category select setup"

                    var objQual = DotNetNuke.Data.DataProvider.Instance().ObjectQualifier;
                    var dbOwner = DataProvider.Instance().DatabaseOwner;

                    //get default catid.
                    var catseo = _catid;
                    var defcatid = ModSettings.Get("defaultcatid");
                    if (defcatid == "") defcatid = "0";
                    if (Utils.IsNumeric(defcatid) && Convert.ToInt32(defcatid) > 0)
                    {
                        // if we have no filter use the default category
                        if (_catid == "" && strFilter.Trim() == "") _catid = defcatid;
                    }
                    else
                    {
                        defcatid = ModSettings.Get("defaultpropertyid");
                        if (defcatid == "") defcatid = "0";
                        if (Utils.IsNumeric(defcatid))
                        {
                            // if we have no filter use the default category
                            if (_catid == "" && strFilter.Trim() == "") _catid = defcatid;
                        }
                    }

                    // If we have a static list,then always display the default category
                    if (ModSettings.Get("staticlist") == "True")
                    {
                        if (catseo == "") catseo = _catid;
                        _catid = defcatid;
                        if (ModSettings.Get("chkcascaderesults").ToLower() == "true")
                            strFilter = strFilter + " and NB1.[ItemId] in (select parentitemid from " + dbOwner + "[" + objQual + "NBrightBuy] where (typecode = 'CATCASCADE' or typecode = 'CATXREF') and XrefItemId = " + _catid + ") ";
                        else
                            strFilter = strFilter + " and NB1.[ItemId] in (select parentitemid from " + dbOwner + "[" + objQual + "NBrightBuy] where typecode = 'CATXREF' and XrefItemId = " + _catid + ") ";

                        if (ModSettings.Get("caturlfilter") == "True" && catseo != "" && catseo != _catid)
                        {
                            // add aditional filter for catid filter on url (catseo holds catid from url)
                            if (ModSettings.Get("chkcascaderesults").ToLower() == "true")
                                strFilter = strFilter + " and NB1.[ItemId] in (select parentitemid from " + dbOwner + "[" + objQual + "NBrightBuy] where (typecode = 'CATCASCADE' or typecode = 'CATXREF') and XrefItemId = " + catseo + ") ";
                            else
                                strFilter = strFilter + " and NB1.[ItemId] in (select parentitemid from " + dbOwner + "[" + objQual + "NBrightBuy] where typecode = 'CATXREF' and XrefItemId = " + catseo + ") ";
                        }
                        // do special custom sort in each cateogry, this passes the catid to the SQL SPROC, whcih process the '{bycategoryproduct}' and orders by product/category seq. 
                        if (_navigationdata.OrderBy.Contains("{bycategoryproduct}")) _navigationdata.OrderBy = "{bycategoryproduct}" + _catid;
                    }
                    else
                    {
                        #region "use url to get category to display"

                        //check if we are display categories 
                        // get category list data
                        if (_catname != "") // if catname passed in url, calculate what the catid is
                        {
                            objCat = ModCtrl.GetByGuidKey(PortalId, ModuleId, "CATEGORYLANG", _catname);
                            if (objCat == null)
                            {
                                // check it's not just a single language
                                objCat = ModCtrl.GetByGuidKey(PortalId, ModuleId, "CATEGORY", _catname);
                                if (objCat != null) _catid = objCat.ItemID.ToString("");
                            }
                            else
                            {
                                _catid = objCat.ParentItemId.ToString("");
                                if (!String.IsNullOrEmpty(objCat.GUIDKey) && Utils.IsNumeric(_catid) && objCat.Lang != Utils.GetCurrentCulture())
                                {
                                    // do a 301 redirect to correct url for the langauge (If the langauge is changed on the product list, we need to make sure we have the correct catref for the langauge)
                                    var catGrpCtrl = new GrpCatController(Utils.GetCurrentCulture());
                                    var activeCat = catGrpCtrl.GetCategory(Convert.ToInt32(_catid));
                                    if (activeCat != null)
                                    {
                                        var redirecturl = "";
                                        if (Utils.IsNumeric(_eid))
                                        {
                                            var prdData = ProductUtils.GetProductData(Convert.ToInt32(_eid), Utils.GetCurrentCulture(), true, EntityTypeCode);
                                            redirecturl = NBrightBuyUtils.GetEntryUrl(PortalId, _eid, _modkey, prdData.SEOName, TabId.ToString(), "", activeCat.categoryrefGUIDKey);
                                        }
                                        else
                                        {
                                            redirecturl = catGrpCtrl.GetCategoryUrl(activeCat, TabId, Utils.GetCurrentCulture());
                                        }

                                        try
                                        {
                                            if (redirecturl != "")
                                            {
                                                Response.Redirect(redirecturl, false);
                                                Response.StatusCode = (int) System.Net.HttpStatusCode.MovedPermanently;
                                                Response.End();
                                            }
                                        }
                                        catch (Exception)
                                        {
                                            // catch err
                                        }
                                    }
                                }
                            }
                            // We have a category selected (in url), so overwrite categoryid navigationdata.
                            // This allows the return to the same category after a returning from a entry view.
                            if (Utils.IsNumeric(_catid)) _navigationdata.CategoryId = Convert.ToInt32(_catid);
                            catseo = _catid;
                            _navigationdata.ResetSearch();
                            strFilter = "";
                        }

                        if (Utils.IsNumeric(_catid))
                        {

                            if (ModSettings.Get("chkcascaderesults").ToLower() == "true")
                                strFilter = strFilter + " and NB1.[ItemId] in (select parentitemid from " + dbOwner + "[" + objQual + "NBrightBuy] where (typecode = 'CATCASCADE' or typecode = 'CATXREF') and XrefItemId = " + _catid + ") ";
                            else
                                strFilter = strFilter + " and NB1.[ItemId] in (select parentitemid from " + dbOwner + "[" + objQual + "NBrightBuy] where typecode = 'CATXREF' and XrefItemId = " + _catid + ") ";

                            if (Utils.IsNumeric(catseo))
                            {
                                var objSEOCat = ModCtrl.GetData(Convert.ToInt32(catseo), "CATEGORYLANG", Utils.GetCurrentCulture());
                                if (objSEOCat != null && _eid == "") // we may have a detail page and listonly module, in which can we need the product detail as page title
                                {
                                    //Page Title
                                    var seoname = objSEOCat.GetXmlProperty("genxml/lang/genxml/textbox/txtseoname");
                                    if (seoname == "") seoname = objSEOCat.GetXmlProperty("genxml/lang/genxml/textbox/txtcategoryname");

                                    var newBaseTitle = objSEOCat.GetXmlProperty("genxml/lang/genxml/textbox/txtseopagetitle");
                                    if (newBaseTitle == "") newBaseTitle = objSEOCat.GetXmlProperty("genxml/lang/genxml/textbox/txtseoname");
                                    if (newBaseTitle == "") newBaseTitle = objSEOCat.GetXmlProperty("genxml/lang/genxml/textbox/txtcategoryname");
                                    if (newBaseTitle != "") BasePage.Title = newBaseTitle;
                                    //Page KeyWords
                                    var newBaseKeyWords = objSEOCat.GetXmlProperty("genxml/lang/genxml/textbox/txtmetakeywords");
                                    if (newBaseKeyWords != "") BasePage.KeyWords = newBaseKeyWords;
                                    //Page Description
                                    var newBaseDescription = objSEOCat.GetXmlProperty("genxml/lang/genxml/textbox/txtmetadescription");
                                    if (newBaseDescription == "") newBaseDescription = objSEOCat.GetXmlProperty("genxml/lang/genxml/textbox/txtcategorydesc");
                                    if (newBaseDescription != "") BasePage.Description = newBaseDescription;


                                    // Remove canonical link for list.  The Open URL Rewriter (OUR) will create a url that is different to the default SEO url in NBS. 
                                    // So to stop clashes it's been disable by default.  The requirment for a canonical link on a category list is more ticking the box than of being any SEO help (might even be causing confusion to Search Engines). 
                                    // ** If your a SEO nutcases (or SEO companies pushing for it) then you can uncomment the code below, and you can implement the Open URL Rewriter and canonical link.

                                    //if (PortalSettings.HomeTabId == TabId)
                                    //    PageIncludes.IncludeCanonicalLink(Page, Globals.AddHTTP(PortalSettings.PortalAlias.HTTPAlias)); //home page always default of site.
                                    //else
                                    //{
                                    //    PageIncludes.IncludeCanonicalLink(Page, NBrightBuyUtils.GetListUrl(PortalId, TabId, objSEOCat.ItemID, seoname, Utils.GetCurrentCulture()));
                                    //    // Code required for OUR (if used, test to ensure it works correctly!!)
                                    //    //PageIncludes.IncludeCanonicalLink(Page, NBrightBuyUtils.GetListUrl(PortalId, TabId, objSEOCat.ItemID, "", Utils.GetCurrentCulture()));
                                    //}
                                }
                            }

                            // do special custom sort in each cateogry, this passes the catid to the SQL SPROC, whcih process the '{bycategoryproduct}' and orders by product/category seq. 
                            if (_navigationdata.OrderBy.Contains("{bycategoryproduct}")) _navigationdata.OrderBy = "{bycategoryproduct}" +  _catid; 

                        }
                        else
                        {
                            if (!_navigationdata.FilterMode) _navigationdata.CategoryId = 0; // filter mode persist catid
                            if (_navigationdata.OrderBy.Contains("{bycategoryproduct}")) _navigationdata.OrderBy = " Order by ModifiedDate DESC  ";
                        }

                        #endregion
                    }

                    // This allows the return to the same category after a returning from a entry view. + Gives support for current category in razor tokens
                    if (Utils.IsNumeric(_catid)) _navigationdata.CategoryId = Convert.ToInt32(_catid);

                    #endregion

                    #region "Apply provider product filter"

                    // Special filtering can be done, by using the ProductFilter interface.
                    var productfilterkey = "";
                    if (metaTokens.ContainsKey("providerfilterkey")) productfilterkey = metaTokens["providerfilterkey"];
                    if (productfilterkey != "")
                    {
                        var provfilter = FilterInterface.Instance(productfilterkey);
                        if (provfilter != null) strFilter = provfilter.GetFilter(strFilter, _navigationdata, ModSettings, Context);
                    }

                    #endregion

                    #region "itemlists (wishlist)"

                    // if we have a itemListName field then get the itemlist cookie.
                    if (ModSettings.Get("displaytype") == "2") // displaytype 2 = "selected list"
                    {
                        var cw = new ItemListData(PortalId, UserController.Instance.GetCurrentUserInfo().UserID);
                        if (cw.Exists && cw.ItemCount > 0)
                        {
                            strFilter = " and (";
                            foreach (var i in cw.GetItemList())
                            {
                                strFilter += " NB1.itemid = '" + i + "' or";
                            }
                            strFilter = strFilter.Substring(0, (strFilter.Length - 3)) + ") ";
                                // remove the last "or"                    
                        }
                        else
                        {
                            //no data in list so select false itemid to stop anything displaying
                            strFilter += " and (NB1.itemid = '-1') ";
                        }
                    }

                    #endregion

                    // insert page header text
                    NBrightBuyUtils.RazorIncludePageHeader(ModuleId, Page, Path.GetFileNameWithoutExtension(_templD) + "_head" + Path.GetExtension(_templD), _controlPath, ModSettings.ThemeFolder, ModSettings.Settings());

                    // save navigation data
                    _navigationdata.PageModuleId = Utils.RequestParam(Context, "pagemid");
                    _navigationdata.PageNumber = Utils.RequestParam(Context, "page");
                    if (Utils.IsNumeric(_catid)) _navigationdata.PageName = NBrightBuyUtils.GetCurrentPageName(Convert.ToInt32(_catid));

                    // save the last active modulekey to a cookie, so it can be used by the "NBrightBuyUtils.GetReturnUrl" function
                    NBrightCore.common.Cookie.SetCookieValue(PortalId, "NBrigthBuyLastActive", "ModuleKey", ModuleKey, 1);


                    if (strFilter.Trim() == "")
                    {
                        // if at this point we have no filter, then assume we're using urlrewriter and a 404 url has been entered.
                        // rather than display all visible products in a list with no default.
                        // redirect to the product display function, so we can display a 404 and product not found.
                        RazorDisplayDataEntry(_eid);
                    }
                    else
                    {

                        strFilter += " and (NB3.Visible = 1) "; // get only visible products

                        var recordCount = ModCtrl.GetDataListCount(PortalId, ModuleId, EntityTypeCode, strFilter, EntityTypeCodeLang, Utils.GetCurrentCulture(), DebugMode);

                        _navigationdata.RecordCount = recordCount.ToString("");
                        _navigationdata.Save();

                        if (returnlimit > 0 && returnlimit < recordCount) recordCount = returnlimit;

                        // **** check if we already have the template cached, if so no need for DB call or razor call ****
                        // get same cachekey used for DB return, and use for razor.
                        var razorcachekey = ModCtrl.GetDataListCacheKey(PortalId, ModuleId, EntityTypeCode, EntityTypeCodeLang, Utils.GetCurrentCulture(), strFilter, _navigationdata.OrderBy, DebugMode, "", returnlimit, pageNumber, pageSize, recordCount);
                        var cachekey = "NBrightBuyRazorOutput" + _templD + "*" + razorcachekey + PortalId.ToString();
                        var strOut = (String) NBrightBuyUtils.GetModCache(cachekey);
                        if (strOut == null || StoreSettings.Current.DebugMode)
                        {
                            var l = ModCtrl.GetDataList(PortalId, ModuleId, EntityTypeCode, EntityTypeCodeLang, Utils.GetCurrentCulture(), strFilter, _navigationdata.OrderBy, DebugMode, "", returnlimit, pageNumber, pageSize, recordCount);
                            strOut = NBrightBuyUtils.RazorTemplRenderList(_templD, ModuleId, razorcachekey, l, _controlPath, ModSettings.ThemeFolder, Utils.GetCurrentCulture(), ModSettings.Settings());
                        }

                        var lit = new Literal();
                        lit.Text = strOut;
                        phData.Controls.Add(lit);

                        if (_navigationdata.SingleSearchMode) _navigationdata.ResetSearch();

                        if (pageSize > 0)
                        {
                            CtrlPaging.PageSize = pageSize;
                            CtrlPaging.CurrentPage = pageNumber;
                            CtrlPaging.TotalRecords = recordCount;
                            CtrlPaging.BindPageLinks();
                        }
                    }

                }
            }

        }

        #endregion


        #region "Methods"

        private String GetEntryIdFromName(String entryId)
        {
            // get correct itemid, based on eid given
            if (_ename != "")
            {
                var o = ModCtrl.GetByGuidKey(PortalId, ModuleId, EntityTypeCodeLang, _ename);
                if (o == null)
                {
                    o = ModCtrl.GetByGuidKey(PortalId, ModuleId, EntityTypeCode, _ename);
                    if (o != null)
                    {
                        entryId = o.ItemID.ToString("");
                    }
                }
                else
                {
                    entryId = o.ParentItemId.ToString("");
                }
            }
            return entryId;
        }

        private void RazorDisplayDataEntry(String entryId)
        {
            var productData = new ProductData();
            if (Utils.IsNumeric(entryId))
            {
                productData = ProductUtils.GetProductData(Convert.ToInt32(entryId), Utils.GetCurrentCulture(), true, EntityTypeCode);
            }

            if (productData.Exists && (productData.Info.PortalId == -1 || productData.Info.PortalId == PortalId))
            {

                if (PortalSettings.HomeTabId == TabId)
                    PageIncludes.IncludeCanonicalLink(Page, Globals.AddHTTP(PortalSettings.PortalAlias.HTTPAlias)); //home page always default of site.
                else
                    PageIncludes.IncludeCanonicalLink(Page, NBrightBuyUtils.GetEntryUrl(PortalId, _eid, "", productData.SEOName, TabId.ToString("")));

                // overwrite SEO data
                if (productData.SEOName != "")
                    BasePage.Title = productData.SEOTitle;
                else
                    BasePage.Title = productData.ProductName;

                if (productData.SEODescription != "") BasePage.Description = productData.SEODescription;

                // if debug , output the xml used.
                if (DebugMode) productData.Info.XMLDoc.Save(PortalSettings.HomeDirectoryMapPath + "debug_entry.xml");
                // insert page header text
                NBrightBuyUtils.RazorIncludePageHeader(ModuleId, Page, Path.GetFileNameWithoutExtension(_templD) + "_seohead" + Path.GetExtension(_templD), _controlPath, ModSettings.ThemeFolder, ModSettings.Settings(), productData);

                #region "do razor template"

                var strOut = NBrightBuyUtils.RazorTemplRender(_templD, ModuleId, "productdetailrazor" + ModuleId.ToString() + "*" + entryId, productData, _controlPath, ModSettings.ThemeFolder, Utils.GetCurrentCulture(), ModSettings.Settings());
                var lit = new Literal();
                lit.Text = strOut;
                phData.Controls.Add(lit);

                #endregion
            }
            else
            {
                DisplayProductError(productData, "");
            }

        }

        private void DisplayProductError(ProductData productData, String msg)
        {
            var strOut = msg;
            if (productData != null)
            {
                strOut = NBrightBuyUtils.RazorTemplRender("ProductNotFound.cshtml", ModuleId, "", productData, _controlPath, ModSettings.ThemeFolder, Utils.GetCurrentCulture(), ModSettings.Settings());
            }
            var lit = new Literal();
            lit.Text = strOut;
            phData.Controls.Add(lit);
            if (StoreSettings.Current.SettingsInfo.GetXmlPropertyBool("genxml/checkbox/activate404"))
            {
                Response.StatusCode = 404;
            }
        }

        #endregion



    }

}
