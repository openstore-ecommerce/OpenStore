using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightCore.render;

namespace Nevoweb.DNN.NBrightBuy.Components
{
    /// <summary>
    /// Syntax class to keep logic and syntax simple for Razor menu functions
    /// </summary>
    public class CatMenuRazorBuilder
    {

        private readonly GrpCatController _catGrpCtrl;
        private readonly List<string> _razorTemplateName;
        private readonly int _currentCatId;
        private readonly string _lang;
        private readonly string _theme;
        private readonly string _controlPath;


        public CatMenuRazorBuilder(string razorTemplate,string controlPath,string theme, int currentCatId,string lang)
        {
            _lang = lang;
            _catGrpCtrl = new GrpCatController(_lang);
            _razorTemplateName = GetMenuTemplates(razorTemplate,controlPath,theme, lang);
            _currentCatId = currentCatId;
            _controlPath = controlPath;
            _theme = theme;
        }

        public string GetTreeCatList(int displaylevels = 20, int parentid = 0, int tabid = 0,string identClass = "nbrightbuy_catmenu",string styleClass = "",string activeClass = "active")
        {
            if (tabid == 0) tabid = PortalSettings.Current.ActiveTab.TabID;
            if (parentid < 0) parentid = 0;
            var rtnList = "";
            var strCacheKey = "NBrightBuy_GetTreeCatListRazor" + PortalSettings.Current.PortalId + "*" + displaylevels + "*" + parentid + "*" + _lang + "*" + _currentCatId.ToString("");
            var objCache = NBrightBuyUtils.GetModCache(strCacheKey);
            if (objCache == null | StoreSettings.Current.DebugMode)
            {
                rtnList = BuildTreeCatList(rtnList, 0, parentid, "cat", tabid, displaylevels,identClass,styleClass,activeClass);
                //remove emprty <ul> elements
                rtnList = rtnList.Replace("<ul></ul>", "");
                NBrightBuyUtils.SetModCache(-1, strCacheKey, rtnList);
            }
            else
            {
                rtnList = (string)objCache;
            }
            return rtnList;
        }


        private String BuildTreeCatList(String rtnList, int level, int parentid, string groupref, int tabid, int displaylevels = 50, String identClass = "nbrightbuy_catmenu", String styleClass = "", String activeClass = "active")
        {
            if (level > displaylevels) return rtnList; // stop infinate loop

            // header
            if (level == 0)
                rtnList += "<ul class='" + identClass + " " + styleClass + "'>";
            else
                rtnList += "<ul>";

            var activeCat = _catGrpCtrl.GetCategory(_currentCatId);
            if (activeCat == null) activeCat = new GroupCategoryData();
            var depth = 0;
            var levelList = _catGrpCtrl.GetGrpCategories(parentid, "cat"); // force this to always categories
            foreach (GroupCategoryData grpcat in levelList)
            {
                if (grpcat.isvisible)
                {
                    // update cat info
                    grpcat.url = _catGrpCtrl.GetCategoryUrl(grpcat, tabid);
                    grpcat.depth = level; //make base 1, to pick up the

                    var openClass = "";
                    if (activeCat.Parents.Contains(grpcat.categoryid) || grpcat.categoryid == _currentCatId) openClass = " open ";

                    if (_currentCatId == grpcat.categoryid)
                        rtnList += "<li class='" + activeClass + openClass + "'>";
                    else
                    {
                        if (openClass == "")
                            rtnList += "<li>";
                        else 
                            rtnList += "<li class='" + openClass + "'>";
                    }

                    //body 
                    if (_razorTemplateName.Count > grpcat.depth) depth = grpcat.depth;
                    rtnList += NBrightBuyUtils.RazorTemplRender(_razorTemplateName[depth],-1,"",grpcat,_controlPath,_theme,_lang,StoreSettings.Current.Settings());

                    rtnList = BuildTreeCatList(rtnList, level + 1, grpcat.categoryid, groupref, tabid, displaylevels);

                    rtnList += "</li>";

                }
            }

            //footer
            rtnList += "</ul>";

            return rtnList;
        }

        public string GetTreePropertyList(int displaylevels = 20, int parentid = 0, int tabid = 0, String identClass = "nbrightbuy_catmenu", String styleClass = "", String activeClass = "active")
        {
            if (tabid == 0) tabid = PortalSettings.Current.ActiveTab.TabID;
            var rtnList = "";
            var strCacheKey = "NBrightBuy_GetTreePropertyListRazor" + PortalSettings.Current.PortalId + "*" + displaylevels + "*" + parentid + "*" + _lang + "*" + _currentCatId.ToString("");
            var objCache = NBrightBuyUtils.GetModCache(strCacheKey);
            if (objCache == null | StoreSettings.Current.DebugMode)
            {
                rtnList = BuildTreePropertyList(rtnList, 0, parentid, "", tabid, displaylevels, identClass, styleClass, activeClass);
                //remove emprty <ul> elements
                rtnList = rtnList.Replace("<ul></ul>", "");
                NBrightBuyUtils.SetModCache(-1, strCacheKey, rtnList);
            }
            else
            {
                rtnList = (string)objCache;
            }
            return rtnList;
        }

        private String BuildTreePropertyList(String rtnList, int level, int parentid, string groupref, int tabid, int displaylevels = 50, String identClass = "nbrightbuy_catmenu", String styleClass = "", String activeClass = "active")
        {
            if (level > displaylevels) return rtnList; // stop infinate loop

            // header
            if (level == 0)
                rtnList += "<ul class='" + identClass + " " + styleClass + "'>";
            else
                rtnList += "<ul>";

            var activeCat = _catGrpCtrl.GetCategory(_currentCatId);
            if (activeCat == null) activeCat = new GroupCategoryData();
            var depth = 0;
            var levelList = _catGrpCtrl.GetGrpCategories(parentid, ""); // force this to always categories
            foreach (GroupCategoryData grpcat in levelList)
            {
                if (grpcat.isvisible)
                {
                    // update cat info
                    grpcat.url = _catGrpCtrl.GetCategoryUrl(grpcat, tabid);
                    grpcat.depth = level; //make base 1, to pick up the

                    var openClass = "";
                    if (activeCat.Parents.Contains(grpcat.categoryid) || grpcat.categoryid == _currentCatId) openClass = " open ";

                    if (_currentCatId == grpcat.categoryid)
                        rtnList += "<li class='" + activeClass + openClass + "'>";
                    else
                    {
                        if (openClass == "")
                            rtnList += "<li>";
                        else
                            rtnList += "<li class='" + openClass + "'>";
                    }

                    //body 
                    if (_razorTemplateName.Count > grpcat.depth) depth = grpcat.depth;
                    rtnList += NBrightBuyUtils.RazorTemplRender(_razorTemplateName[depth], -1, "", grpcat, _controlPath, _theme, _lang, StoreSettings.Current.Settings());

                    rtnList = BuildTreeCatList(rtnList, level + 1, grpcat.categoryid, groupref, tabid, displaylevels);

                    rtnList += "</li>";

                }
            }

            //footer
            rtnList += "</ul>";

            return rtnList;
        }

        public String GetDrillDownMenu(int parentid, int tabid, String itemClass = "")
        {

            var rtnList = "";
            var levelList = _catGrpCtrl.GetGrpCategories(parentid, ""); // force this to always categories
            foreach (GroupCategoryData grpcat in levelList)
            {
                if (grpcat.isvisible)
                {
                    // update cat info
                    grpcat.url = _catGrpCtrl.GetCategoryUrl(grpcat, tabid);
                    grpcat.depth = 0; 

                    rtnList += "<div class='" + itemClass + "'>";

                    rtnList += NBrightBuyUtils.RazorTemplRender(_razorTemplateName[0], -1, "", grpcat, _controlPath, _theme, _lang, StoreSettings.Current.Settings());

                    rtnList += "</div>";

                }
            }


            return rtnList;
        }



        private List<string> GetMenuTemplates(string basetempl,string controlPath,string theme,string lang)
        {
            var templ = "";
            var rtnL = new List<string>();
            rtnL.Add(basetempl);

            var lp = 1;
            templ = NBrightBuyUtils.GetRazorTemplateData(basetempl.Replace(".cshtml", lp.ToString("") + ".cshtml"), controlPath, theme, lang);
            while (!String.IsNullOrEmpty(templ))
            {
                rtnL.Add(basetempl.Replace(".cshtml", lp.ToString("") + ".cshtml"));
                lp += 1;
                templ = NBrightBuyUtils.GetRazorTemplateData(basetempl.Replace(".cshtml", lp.ToString("") + ".cshtml"), controlPath, theme, lang);
                if (lp > 99)
                {
                    templ += "POSSIBLE INFINATE LOOP: CatMenuRazorBuilder.cs, GenMenuTemplate";
                    rtnL.Add(templ);
                    break;
                }
            }

            return rtnL;
        }


    }
}
