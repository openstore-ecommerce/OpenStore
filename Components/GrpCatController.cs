using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows.Forms.VisualStyles;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;

namespace Nevoweb.DNN.NBrightBuy.Components
{
    public class GrpCatController
    {
        private NBrightBuyController _objCtrl;

        private String _lang = "";
        private int _portalId = -1;
        public List<GroupCategoryData> GrpCategoryList;
        public List<GroupCategoryData> CategoryList;
        public List<NBrightInfo> GroupList;
        public Dictionary<String, String> GroupsDictionary; 

        public GrpCatController(String lang,Boolean debugMode = false)
        {
            if (PortalSettings.Current != null)
            {
                _portalId = PortalSettings.Current.PortalId;
                Load(lang, debugMode);
            }
        }

        public GrpCatController(String lang,int portalId, Boolean debugMode = false)
        {
            _portalId = portalId;
            Load(lang, debugMode);
        }

        #region "base methods"

        public void Reload()
        {
            ClearCache();
            Load(_lang);
        }

        public void ClearCache()
        {
            foreach (var lang in DnnUtils.GetCultureCodeList(_portalId))
            {
                var strCacheKey = "GrpList_" + lang + "_" + _portalId;
                CacheUtils.RemoveCache(strCacheKey);
                strCacheKey = "GroupsDictionary_" + lang + "_" + _portalId;
                CacheUtils.RemoveCache(strCacheKey);
                strCacheKey = "GrpCategoryList_" + lang + "_" + _portalId;
                CacheUtils.RemoveCache(strCacheKey);
                strCacheKey = "CategoryList_" + lang + "_" + _portalId;
                CacheUtils.RemoveCache(strCacheKey);                
            }
        }

        public GroupCategoryData GetGrpCategory(int categoryid)
        {
            var lenum = from i in GrpCategoryList where i.categoryid == categoryid select i;
            var l = lenum.ToList();
            return l.Any() ? l[0] : null;
        }

        public GroupCategoryData GetCategory(int categoryid)
        {
            var lenum = from i in CategoryList where i.categoryid == categoryid select i;
            var l = lenum.ToList();
            
            return l.Any() ? l[0] : null;
        }

        public GroupCategoryData GetCategoryByRef(int portalId, String catref)
        {
            // the catref might not be in the CategoryList because the language has changed but the url is still displaying the old catref langauge
            // try and find it. NASTY!!!! incorrect langyage url left after langauge change!!
            var catLang = _objCtrl.GetByGuidKey(portalId, -1, "CATEGORYLANG", catref);
            if (catLang != null)
            {
                return GetCategory(catLang.ParentItemId);
            }
            return null;
        }

        public List<GroupCategoryData> GetGrpCategories(int parentcategoryid, string groupref = "")
        {
            IEnumerable<GroupCategoryData> lenum;
            if (groupref == "" || groupref == "cat")
                lenum = from i in GrpCategoryList where i.parentcatid == parentcategoryid select i;
            else
                lenum = from i in GrpCategoryList select i; // if we're getting group categories, the parentitemid is set to the group itemid + we only have the 1 level.

            if (groupref != "")
            {
                var lenum2 = from i2 in lenum where i2.grouptyperef == groupref select i2;
                return lenum2.ToList();
            }
            return lenum.ToList();
        }

        public List<GroupCategoryData> GetCategories(int parentcategoryid)
        {
            var lenum = from i in CategoryList where i.parentcatid == parentcategoryid select i;
            var l = lenum.ToList();
            return l;
        }

        public List<GroupCategoryData> GetVisibleCategories(int parentcategoryid)
        {
            var lenum = from i in CategoryList where i.parentcatid == parentcategoryid & i.ishidden == false select i;
            var l = lenum.ToList();
            return l;
        }

        public List<GroupCategoryData> GetVisibleCategoriesWithUrl(int parentcategoryid, int tabid)
        {
            var l = GetVisibleCategories(parentcategoryid);
            foreach (var c in l)
            {
                c.url = GetCategoryUrl(c, tabid);
            }
            return l;
        }

        public List<GroupCategoryData> GetCategoriesWithUrl(int parentcategoryid, int tabid)
        {
            var l = GetCategories(parentcategoryid);
            foreach (var c in l)
            {
                c.url = GetCategoryUrl(c, tabid);
            }
            return l;
        }

        public GroupCategoryData GetGrpCategoryByRef(string categoryref)
        {
            var lenum = from i in GrpCategoryList where i.categoryref == categoryref select i;
            var l = lenum.ToList();
            if (l.Count == 0) return null;
            return l[0];
        }

        public List<GroupCategoryData> GetSubCategoryList(List<GroupCategoryData> catList, int categoryid, int lvl = 0)
        {
            if (lvl > 50) return catList; // stop possible infinate loop

            var subcats = from i in GrpCategoryList where i.parentcatid == categoryid select i;
            foreach (var c in subcats)
            {
                catList.Add(c);
                GetSubCategoryList(catList, c.categoryid, lvl + 1);
            }
            return catList;
        }

        #endregion

        #region "Special methods"


        public string GetCategoryUrl(GroupCategoryData groupCategoryInfo, int tabid,String categorylangauge = "")
        {
            // if diabled return a javascript void
            if (groupCategoryInfo.disabled) return "javascript:void(0)";

            //set a default url
            var url = "";

            // get friendly url if possible
                if (groupCategoryInfo.categoryname != "")
                {
                    var tab = CBO.FillObject<DotNetNuke.Entities.Tabs.TabInfo>(DotNetNuke.Data.DataProvider.Instance().GetTab(tabid));
                    if (tab != null)
                    {
                        var newBaseName = groupCategoryInfo.seoname;
                        newBaseName = Utils.UrlFriendly(newBaseName);
                        var ctrl = Utils.RequestParam(HttpContext.Current, "ctrl");
                        if (ctrl != "") ctrl = "&ctrl=" + ctrl;
                        var language = categorylangauge;
                        if (language == "") language = Utils.GetCurrentCulture();
                        // check if we are calling from BO with a ctrl param
                        url = DotNetNuke.Services.Url.FriendlyUrl.FriendlyUrlProvider.Instance().FriendlyUrl(tab, "~/Default.aspx?TabId=" + tab.TabID.ToString("") + "&catid=" + groupCategoryInfo.categoryid.ToString("") + ctrl + "&language=" + language, newBaseName + ".aspx");

                        url = url.Replace("[catid]/", ""); // remove the injection token from the url, if still there. (Should be removed redirected to new page)
            
                    }
                }
            return url;
        }

        /// <summary>
        /// Get the default category for the product 
        /// </summary>
        /// <param name="productid"></param>
        /// <param name="lang">default is current culture</param>
        /// <returns></returns>
        public int GetDefaultCatId(int productid,String lang = "")
        {
            var dcat = GetDefaultCategory(productid,lang);
            if (dcat == null) return 0;
            return dcat.categoryid;
        }

        /// <summary>
        /// Get default category data from product
        /// </summary>
        /// <param name="productid"></param>
        /// <param name="lang">default is current culture</param>
        /// <returns></returns>
        public GroupCategoryData GetDefaultCategory(int productid, String lang = "")
        {
            if (lang == "") lang = Utils.GetCurrentCulture();
            var prodData = ProductUtils.GetProductData(productid, lang);
            return prodData.GetDefaultCategory();
        }

        public GroupCategoryData GetCurrentCategoryData(int portalId, System.Web.HttpRequest request, int entryId = 0, Dictionary<string, string> settings = null, String targetModuleKey = "")
        {
            var defcatid = 0;

            var categoryid = NBrightBuyUtils.GetCategoryIdFromUrl(portalId, request);

            // always use the catid in url if we have no target module (use GetGrpCategory to include properties being listed)
            if ((categoryid > 0) && targetModuleKey == "") return GetGrpCategory(categoryid);

            if (targetModuleKey != "")
            {
                var navigationdata = new NavigationData(portalId, targetModuleKey);
                if (Utils.IsNumeric(navigationdata.CategoryId) && navigationdata.FilterMode) defcatid = Convert.ToInt32(navigationdata.CategoryId);
                // always use the catid in url if we have no navigation categoryid for the target module.
                if ((categoryid > 0) && defcatid == 0) return GetGrpCategory(categoryid);
            }
 
            // if we have no catid in url, make sure we have any possible entryid
            if (entryId == 0) entryId = NBrightBuyUtils.GetEntryIdFromUrl(portalId, request);

            // use the first/default category the product has
            if (entryId > 0) return GetDefaultCategory(entryId);

            // get any default set in the settings
            if (defcatid == 0)
            {
                if (settings != null && settings["defaultcatid"] != null)
                {
                    var setcatid = settings["defaultcatid"];
                    if (Utils.IsNumeric(setcatid)) defcatid = Convert.ToInt32(setcatid);
                }
            }

            return GetGrpCategory(defcatid);

        }

        public NBrightInfo GetCurrentCategoryInfo(int portalId, System.Web.HttpRequest request, int entryId = 0)
        {

            var defcatid = 0;
            var qrycatid = Utils.RequestQueryStringParam(request, "catid");
            if (Utils.IsNumeric(entryId) && entryId > 0) defcatid = GetDefaultCatId(entryId);

            if (defcatid == 0 && Utils.IsNumeric(qrycatid)) defcatid = Convert.ToInt32(qrycatid);

            var objCtrl = new NBrightBuyController();
            return objCtrl.GetData(defcatid,"CATEGORYLANG");
        }

        public List<GroupCategoryData> GetTreeCategoryList(List<GroupCategoryData> rtnList, int level, int parentid, string groupref,string breadcrumbseparator)
        {
            if (level > 20) return rtnList; // stop infinate loop

            var levelList = GetGrpCategories(parentid, groupref);
            foreach (GroupCategoryData tInfo in levelList)
            {
                var nInfo = tInfo;
                nInfo.breadcrumb = GetBreadCrumb(nInfo.categoryid, 50, breadcrumbseparator,false);
                nInfo.depth = level;
                rtnList.Add(nInfo);
                if (groupref == "" || groupref == "cat") GetTreeCategoryList(rtnList, level + 1, tInfo.categoryid, groupref, breadcrumbseparator);
            }

            return rtnList;
        }

        public List<GroupCategoryData> GetTreePropertyList(string breadcrumbseparator)
        {
            var rtnList = new List<GroupCategoryData>();
            foreach (var grp in GroupList)
            {
                if (grp.GUIDKey != "cat")
                {
                    var levelList = GetGrpCategories(0, grp.GUIDKey);
                    foreach (GroupCategoryData tInfo in levelList)
                    {
                        var nInfo = tInfo;
                        nInfo.breadcrumb = GetBreadCrumb(nInfo.categoryid, 50, breadcrumbseparator, false);
                        nInfo.depth = 0;
                        rtnList.Add(nInfo);
                    }                    
                }
            }
            return rtnList;
        }

        /// <summary>
        /// Select categories linked to product, by groupref
        /// </summary>
        /// <param name="productid"></param>
        /// <param name="groupref">groupref for select, "" = all, "cat"= Category only, "!cat" = all non-category, "{groupref}"=this group only</param>
        /// <param name="cascade">get all cascade records to get all parent categories</param>
        /// <returns></returns>
        public List<GroupCategoryData> GetProductCategories(int productid, String groupref = "", Boolean cascade = false)
        {
            var objCtrl = new NBrightBuyController();
            var catxrefList = objCtrl.GetList(-1, -1, "CATXREF", " and NB1.[ParentItemId] = " + productid);

            if (cascade)
            {
                var catcascadeList = objCtrl.GetList(-1, -1, "CATCASCADE", " and NB1.[ParentItemId] = " + productid);
                foreach (var c in catcascadeList)
                {
                    catxrefList.Add(c);
                }                
            }


            var notcat = "";
            if (groupref == "!cat")
            {
                groupref = "";
                notcat = "cat";
            }

            var joinItems = (from d1 in GrpCategoryList
                             join d2 in catxrefList on d1.categoryid equals d2.XrefItemId
                             where (d1.grouptyperef == groupref || groupref == "") && d1.grouptyperef != notcat
                             select d1).OrderBy(d1 => d1.grouptyperef).ThenBy(d1 => d1.breadcrumb).ToList<GroupCategoryData>();
            return joinItems;
        }

        #endregion

        #region "breadcrumbs"

        public String GetBreadCrumb(int categoryid, int shortLength, string separator, bool aslist)
        {
            return GetBreadCrumb(categoryid, shortLength, separator, aslist, false);
        }
        /// <summary>
        /// Get category breadcrumb, using controller langauge
        /// </summary>
        /// <param name="categoryid"></param>
        /// <param name="shortLength">0 = unlimited</param>
        /// <param name="separator"></param>
        /// <param name="aslist">if true brings html list as return string</param>
        /// <param name="useSEO">if true uses the SEO Name if entered</param>
        /// <returns></returns>
        public String GetBreadCrumb(int categoryid, int shortLength, string separator, bool aslist, bool useSEO)
        {
            var breadCrumb = "";
            var checkDic = new Dictionary<int, int>();
            while (true)
            {
                if (checkDic.ContainsKey(categoryid)) break; // jump out if we get data giving an infinate loop
                int itemid1 = categoryid;
                var lenum = from i in CategoryList where i.categoryid == itemid1 select i;
                var l = lenum.ToList();
                if (l.Any())
                {
                    var crumbText = l.First().categoryname;
                    if (useSEO && l.First().seoname != "") crumbText = l.First().seoname;
                    if (crumbText != null)
                    {
                        if (shortLength > 0)
                        {
                            if (crumbText.Length > (shortLength + 1)) crumbText = crumbText.Substring(0, shortLength) + ".";
                        }

                        var strOut = "";
                        if (aslist)
                            strOut = "<li>" + separator + crumbText + "</li>" + breadCrumb;
                        else
                            strOut = separator + crumbText + breadCrumb;

                        checkDic.Add(categoryid, categoryid);
                        categoryid = l.First().parentcatid;
                        breadCrumb = strOut;
                        continue;
                    }
                }
                if (breadCrumb.StartsWith(separator)) breadCrumb = breadCrumb.Substring(separator.Length);
                if (aslist && breadCrumb != "") breadCrumb = "<ul class='crumbs'>" + breadCrumb + "</ul>";
                return breadCrumb;
            }
            return "";
        }
        public String GetBreadCrumbWithLinks(int categoryid, int tabId, int shortLength, string separator, bool aslist)
        {
            return GetBreadCrumbWithLinks(categoryid, tabId, shortLength, separator, aslist, false);
        }
        public String GetBreadCrumbWithLinks(int categoryid, int tabId, int shortLength, string separator, bool aslist, bool useSEO)
        {
            return GetBreadCrumbWithLinks(categoryid, tabId, shortLength, separator, aslist, false, useSEO);
        }
        public String GetBreadCrumbWithLinks(int categoryid, int tabId, int shortLength, string separator, bool aslist, bool useSEO, bool ajax = false)
        {
            var breadCrumb = "";
            var checkDic = new Dictionary<int, int>();
            while (true)
            {
                if (checkDic.ContainsKey(categoryid)) break; // jump out if we get data giving an infinate loop
                int itemid1 = categoryid;
                var lenum = from i in CategoryList where i.categoryid == itemid1 select i;
                var l = lenum.ToList();
                if (l.Any())
                {
                    var crumbText = l.First().categoryname;
                    if (useSEO && l.First() != null && l.First().seoname != "") crumbText = l.First().seoname;
                    if (crumbText != null)
                    {
                        if (shortLength > 0)
                        {
                            if (crumbText.Length > (shortLength + 1)) crumbText = crumbText.Substring(0, shortLength) + ".";
                        }

                        var strOut = "";
                        if (ajax)
                        {
                            var catid = l.First().categoryid;
                            if (aslist)
                                strOut = "<li>" + separator + "<a href='" + GetCategoryUrl(l.First(), tabId) + "' catid='" + catid + "' class='ajaxcatmenu'>" + crumbText + "</a>" + "</li>" + breadCrumb;
                            else
                                strOut = separator + "<a href='" + GetCategoryUrl(l.First(), tabId) + "' catid='" + catid + "' class='ajaxcatmenu'>" + crumbText + "</a>" + breadCrumb;
                        }
                        else
                        {
                            if (aslist)
                                strOut = "<li>" + separator + "<a href='" + GetCategoryUrl(l.First(), tabId) + "'>" + crumbText + "</a>" + "</li>" + breadCrumb;
                            else
                                strOut = separator + "<a href='" + GetCategoryUrl(l.First(), tabId) + "'>" + crumbText + "</a>" + breadCrumb;
                        }

                        checkDic.Add(categoryid, categoryid);
                        categoryid = l.First().parentcatid;
                        breadCrumb = strOut;
                        continue;
                    }
                }
                if (breadCrumb.StartsWith(separator)) breadCrumb = breadCrumb.Substring(separator.Length);
                if (aslist) breadCrumb = "<ul class='crumbs'>" + breadCrumb + "</ul>"; 
                return breadCrumb;
            }
            return "";
        }

        #endregion

        #region "private methods"

        private void Load(String lang, Boolean debugMode = false)
        {
            _objCtrl = new NBrightBuyController();
            _lang = lang;

            var strCacheKey = "GrpList_" + lang + "_" + _portalId;
            GroupList = (List<NBrightInfo>)CacheUtils.GetCache(strCacheKey);
            if (GroupList == null || debugMode)
            {
                // get groups
                GroupList = NBrightBuyUtils.GetCategoryGroups(_portalId, _lang, true);
                CacheUtils.SetCache(strCacheKey, GroupList);
            }

            strCacheKey = "GroupsDictionary_" + lang + "_" + _portalId;
            GroupsDictionary = (Dictionary<string,string>)CacheUtils.GetCache(strCacheKey);
            if (GroupsDictionary == null || debugMode)
            {
                GroupsDictionary = new Dictionary<String, String>();
                foreach (var g in GroupList)
                {
                    if (!GroupsDictionary.ContainsKey(g.GetXmlProperty("genxml/textbox/groupref"))) GroupsDictionary.Add(g.GetXmlProperty("genxml/textbox/groupref"), g.GetXmlProperty("genxml/lang/genxml/textbox/groupname"));
                }
                CacheUtils.SetCache(strCacheKey, GroupsDictionary);
            }

            // build group category list
            strCacheKey = "GrpCategoryList_" + lang + "_" + _portalId;
            GrpCategoryList = (List<GroupCategoryData>)CacheUtils.GetCache(strCacheKey);
            if (GrpCategoryList == null || debugMode)
            {
                GrpCategoryList = GetGrpCatListFromDatabase(lang);
                CacheUtils.SetCache(strCacheKey, GrpCategoryList);
            }

            // build cateogry list for navigation from group category list
            strCacheKey = "CategoryList_" + lang + "_" + _portalId;
            CategoryList = (List<GroupCategoryData>)CacheUtils.GetCache(strCacheKey);
            if (CategoryList == null || debugMode)
            {
                var lenum = from i in GrpCategoryList where i.grouptyperef == "cat" select i;
                CategoryList = lenum.ToList();
                CacheUtils.SetCache(strCacheKey, CategoryList);
            }

            // add breadcrumb (needs both GrpCategoryList and CategoryList )
            //[TODO: fix this catch 22 for list dependancy]
            foreach (var grpcat in CategoryList)
            {
                grpcat.breadcrumb = GetBreadCrumb(grpcat.categoryid, 200, ">", false);
            }


        }

        private NBrightInfo GetLangData(List<NBrightInfo> langList,int categoryid)
        {
            var lenum = from i in langList where i.ParentItemId == categoryid select i;
            var l = lenum.ToList();
            return l.Any() ? l[0] : null;
        }

        private int GetEntryCount(List<NBrightInfo> xrefList, int categoryid)
        {
            var lenum = from i in xrefList where i.XrefItemId == categoryid select i;
            return lenum.Count();
        }


        private List<NBrightInfo> GetParentList(List<NBrightInfo> catList, int categoryid)
        {
            var rtnList = new List<NBrightInfo>();
            var startCat = from i in catList where i.ItemID == categoryid select i;
            if (startCat.Any())
            {
                categoryid = startCat.ToList()[0].ParentItemId;
                var c = 1;
                while (true)
                {
                    var l = from i in catList where i.ItemID == categoryid select i;
                    if (l.Any())
                    {
                        rtnList.Add(l.ToList()[0]);
                        categoryid = rtnList.Last().ParentItemId;
                    }
                    else
                        break;
                    c += 1;
                    if (c > 50) break; //stop possible infinate loop
                }
            }
            return rtnList;
        }

        private List<GroupCategoryData> GetGrpCatListFromDatabase(String lang = "")
        {
            // this process seems to be creating an error on DB connection after the cache is release.
            //[TODO: re-write this code to stop DB conection failure. For now Module level caching has been increased to 2 days to stop this processing re-running (Seems OK on 29/08/2017)]

            var objCtrl = new NBrightBuyController();
            const string strOrderBy = " order by [XMLData].value('(genxml/hidden/recordsortorder)[1]','decimal(10,2)') ";
            var grpcatList = new List<GroupCategoryData>();

            var l = new List<NBrightInfo>();
            var lg = new List<NBrightInfo>();
            var lx = new List<NBrightInfo>();
            var lx2 = new List<NBrightInfo>();

            // if we get an error, assume the DB connection lock and rerun to get detail. (unsure why it does this???)
            try
            {
                l = objCtrl.GetList(_portalId, -1, "CATEGORY", "", strOrderBy, 0, 0, 0, 0, "", "");
            }
            catch
            {
                l = objCtrl.GetList(_portalId, -1, "CATEGORY", "", strOrderBy, 0, 0, 0, 0, "", "");
            }
            try
            {
                lg = objCtrl.GetList(_portalId, -1, "CATEGORYLANG", "and NB1.lang = '" + lang + "'", "", 0, 0, 0, 0, "", "");
            }
            catch
            {
                lg = objCtrl.GetList(_portalId, -1, "CATEGORYLANG", "and NB1.lang = '" + lang + "'", "", 0, 0, 0, 0, "", "");
            }

            try
            {
                lx = objCtrl.GetList(_portalId, -1, "CATCASCADE", "", "", 0, 0, 0, 0, "", "");
            }
            catch
            {
                lx = objCtrl.GetList(_portalId, -1, "CATCASCADE", "", "", 0, 0, 0, 0, "", "");
            }

            try
            {
                lx2 = objCtrl.GetList(_portalId, -1, "CATXREF", "", "", 0, 0, 0, 0, "", "");
            }
            catch
            {
                lx2 = objCtrl.GetList(_portalId, -1, "CATXREF", "", "", 0, 0, 0, 0, "", "");
            }

            lx.AddRange(lx2);
            foreach (var i in l)
            {
                var grpcat = new GroupCategoryData();
                grpcat.categoryid = i.ItemID;
                grpcat.recordsortorder = i.GetXmlPropertyDouble("genxml/hidden/recordsortorder");
                grpcat.imageurl = i.GetXmlProperty("genxml/hidden/imageurl");
                grpcat.categoryref = i.GetXmlProperty("genxml/textbox/txtcategoryref");
                grpcat.propertyref = i.GetXmlProperty("genxml/textbox/propertyref");
                grpcat.archived = i.GetXmlPropertyBool("genxml/checkbox/chkarchived");
                grpcat.ishidden = i.GetXmlPropertyBool("genxml/checkbox/chkishidden");
                grpcat.disabled = i.GetXmlPropertyBool("genxml/checkbox/chkdisable");
                grpcat.grouptyperef = i.GetXmlProperty("genxml/dropdownlist/ddlgrouptype");
                grpcat.attributecode = i.GetXmlProperty("genxml/dropdownlist/ddlattrcode");
                grpcat.parentcatid = i.ParentItemId;
                grpcat.entrycount = GetEntryCount(lx, grpcat.categoryid);
                if (GroupsDictionary.ContainsKey(grpcat.grouptyperef)) grpcat.groupname = GroupsDictionary[grpcat.grouptyperef];

                // get the language data
                var langItem =  GetLangData(lg,grpcat.categoryid);
                if (langItem != null)
                {
                    grpcat.categoryname = langItem.GetXmlProperty("genxml/textbox/txtcategoryname");
                    grpcat.categorydesc = langItem.GetXmlProperty("genxml/textbox/txtcategorydesc");
                    grpcat.seoname = langItem.GetXmlProperty("genxml/textbox/txtseoname");
                    if (grpcat.seoname == "") grpcat.seoname = langItem.GetXmlProperty("genxml/textbox/txtcategoryname");
                    grpcat.metadescription = langItem.GetXmlProperty("genxml/textbox/txtmetadescription");
                    grpcat.metakeywords = langItem.GetXmlProperty("genxml/textbox/txtmetakeywords");
                    grpcat.seopagetitle = langItem.GetXmlProperty("genxml/textbox/txtseopagetitle");
                    grpcat.message = langItem.GetXmlProperty("genxml/edt/message");
                    grpcat.categoryrefGUIDKey = langItem.GUIDKey;
                }

                //get parents
                var p = GetParentList(l,grpcat.categoryid);
                foreach (var pi in p)
                    grpcat.Parents.Add(pi.ItemID);

                grpcatList.Add(grpcat);
            }

            // we don;t have the depth number at this point, so use recussive call to calc it.
            CalcCategoryDepthList(grpcatList, 0, 0);

            return grpcatList;

        }

        private void CalcCategoryDepthList(List<GroupCategoryData> grpCatList, int level, int parentid)
        {
            if (level < 50) // stop any possiblity of infinite loop
            {
                var lenum = from i in grpCatList where i.parentcatid == parentid select i;
                foreach (GroupCategoryData tInfo in lenum)
                {
                    tInfo.depth = level;
                    CalcCategoryDepthList(grpCatList, level + 1, tInfo.categoryid);
                }
            }
        }

        private void AddCatCascadeRecord(int categoryid,int productid)
        {
            var strGuid = categoryid.ToString("") + "x" + productid.ToString("");
            var nbi = _objCtrl.GetByGuidKey(_portalId, -1, "CATCASCADE", strGuid);
            if (nbi == null)
            {
                nbi = new NBrightInfo();
                nbi.ItemID = -1;
                nbi.PortalId = _portalId;
                nbi.ModuleId = -1;
                nbi.TypeCode = "CATCASCADE";
                nbi.XrefItemId = categoryid;
                nbi.ParentItemId = productid;
                nbi.XMLData = null;
                nbi.TextData = null;
                nbi.Lang = null;
                nbi.GUIDKey = strGuid;
                _objCtrl.Update(nbi);
            }
        }

        #endregion

        #region "indexing"

        /// <summary>
        /// Reindex catcascade records for category and all parent categories 
        /// </summary>
        /// <param name="categoryid"></param>
        public void ReIndexCascade(int categoryid)
        {
            if (categoryid > 0)
            {
                ReIndexSingleCascade(categoryid);
                var cat = GetCategory(categoryid);
                if (cat != null)
                {
                    foreach (var p in cat.Parents)
                    {
                        ReIndexSingleCascade(p);
                    }
                }
            }
        }

        /// <summary>
        /// Rebuild the CATCASCADE index records for a single category
        /// </summary>
        /// <param name="categoryid"></param>
        private void ReIndexSingleCascade(int categoryid)
        {
            if (categoryid > 0)
            {
                //get all category product ids from catxref sub category records.
                var xrefList = new List<NBrightInfo>();
                var prodItemIdList = xrefList.Select(r => r.ParentItemId).ToList();
                var catList = new List<GroupCategoryData>();
                var subCats = GetSubCategoryList(catList, categoryid);
                foreach (var c in subCats)
                {
                    xrefList = _objCtrl.GetList(_portalId, -1, "CATXREF", " and xrefitemid = " + c.categoryid.ToString(""));
                    prodItemIdList.AddRange(xrefList.Select(r => r.ParentItemId));
                }
                //Get the current catascade records
                xrefList = _objCtrl.GetList(_portalId, -1, "CATCASCADE", " and xrefitemid = " + categoryid.ToString(""));
                var casacdeProdItemIdList = xrefList.Select(r => r.ParentItemId).ToList();

                //Update the catcascade records.
                foreach (var prodId in prodItemIdList)
                {
                    AddCatCascadeRecord(categoryid, prodId);
                    casacdeProdItemIdList.RemoveAll(i => i == prodId);
                }
                //remove any cascade records that no longer exists
                foreach (var productid in casacdeProdItemIdList)
                {
                    var strGuid = categoryid.ToString("") + "x" + productid.ToString("");
                    var nbi = _objCtrl.GetByGuidKey(_portalId, -1, "CATCASCADE", strGuid);
                    if (nbi != null) _objCtrl.Delete(nbi.ItemID);
                }
            }
        }


        #endregion

    }
}
