using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using NBrightCore.common;
using NBrightDNN;

namespace Nevoweb.DNN.NBrightBuy.Components.ItemLists
{
    public static class ItemListsFunctions
    {

        private static string _entityTypeCode;
        private static string _entityTypeCodeLang;
        private static string _themeFolder;
        private static string _templatename;

        public static string ProcessCommand(string paramCmd, HttpContext context)
        {
            var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
            _entityTypeCode = ajaxInfo.GetXmlProperty("genxml/hidden/entitytypecode");
            _entityTypeCodeLang = ajaxInfo.GetXmlProperty("genxml/hidden/entitytypecodelang");
            if (_entityTypeCode == "") _entityTypeCode = "PRD";
            if (_entityTypeCodeLang == "") _entityTypeCodeLang = "PRDLANG";
            _themeFolder = ajaxInfo.GetXmlProperty("genxml/hidden/themefolder");
            _templatename = ajaxInfo.GetXmlProperty("genxml/hidden/templatename");
            if (_templatename == "") _templatename = "favoriteslist";
            if (_themeFolder == "") _themeFolder = "ClassicAjax";
            var itemId = ajaxInfo.GetXmlProperty("genxml/hidden/shopitemid");
            var itemlistname = ajaxInfo.GetXmlProperty("genxml/hidden/shoplistname");

            var strOut = "ORDER - ERROR!! - No Security rights for current user!";

            var cw = new ItemListData(PortalSettings.Current.PortalId, UserController.Instance.GetCurrentUserInfo().UserID);

            switch (paramCmd)
            {
                case "itemlist_add":
                    if (Utils.IsNumeric(itemId))
                    {
                        cw.Add(itemlistname, itemId);
                        strOut = cw.products;
                    }
                    break;
                case "itemlist_remove":
                    if (Utils.IsNumeric(itemId))
                    {
                        cw.Remove(itemlistname, itemId);
                        strOut = cw.products;
                    }
                    break;
                case "itemlist_delete":
                    cw.DeleteList(itemlistname);
                    strOut = "deleted";
                    break;
                case "itemlist_productlist":
                    strOut = GetProductItemListHtml(cw, itemlistname);
                    break;
                case "itemlist_getpopup":
                    strOut = GetProductItemListPopup(cw, itemlistname);
                    break;
            }

            return strOut;
        }


        public static List<NBrightInfo> GetProductItemList(ItemListData itemListData,string listkey = "",string entityTypeCode = "PRD", string entityTypeCodeLang = "PRDLANG")
        {
            var strOut = "";
            var strFilter = "";
            var rtnList = new List<NBrightInfo>();
            if (itemListData.Exists && itemListData.ItemCount > 0)
            {
                var ModCtrl = new NBrightBuyController();
                if (listkey == "")
                {
                    foreach (var i in itemListData.listnames)
                    {
                        var itemlist = itemListData.GetItemList(i.Key);
                        if (itemlist.Count > 0)
                        {
                            strFilter = " and (";
                            foreach (var i2 in itemlist)
                            {
                                strFilter += " NB1.itemid = '" + i2 + "' or";
                            }
                            strFilter = strFilter.Substring(0, (strFilter.Length - 3)) + ") ";
                            strFilter += " and (NB3.Visible = 1) ";
                            var l = ModCtrl.GetDataList(PortalSettings.Current.PortalId, -1, entityTypeCode,
                                entityTypeCodeLang, Utils.GetCurrentCulture(), strFilter, "", true);
                            foreach (var n in l)
                            {
                                n.SetXmlProperty("genxml/listkey", i.Key);
                                rtnList.Add(n);
                            }
                        }
                    }
                }
                else
                {
                    var itemlist = itemListData.GetItemList(listkey);
                    if (itemlist.Count > 0)
                    {
                        strFilter = " and (";
                        foreach (var i in itemlist)
                        {
                            strFilter += " NB1.itemid = '" + i + "' or";
                        }
                        strFilter = strFilter.Substring(0, (strFilter.Length - 3)) + ") ";
                        strFilter += " and (NB3.Visible = 1) ";
                        var l = ModCtrl.GetDataList(PortalSettings.Current.PortalId, -1, _entityTypeCode,
                            _entityTypeCodeLang, Utils.GetCurrentCulture(), strFilter, "", true);
                        foreach (var n in l)
                        {
                            n.SetXmlProperty("genxml/listkey", listkey);
                            rtnList.Add(n);
                        }
                    }
                }
            }

            return rtnList;
        }

        public static string GetProductItemListHtml(ItemListData itemListData, string listkey = "", string entityTypeCode = "PRD", string entityTypeCodeLang = "PRDLANG")
        {
            var strOut = "";
            var rtnList = GetProductItemList(itemListData,listkey, entityTypeCode,  entityTypeCodeLang);
            var modelsetings = StoreSettings.Current.Settings();
            if (!modelsetings.ContainsKey("listkeys")) modelsetings.Add("listkeys", itemListData.listkeys);
            strOut = NBrightBuyUtils.RazorTemplRenderList(_templatename, -1, "", rtnList, "/DesktopModules/NBright/NBrightBuy", _themeFolder, Utils.GetCurrentCulture(), modelsetings);

            return strOut;
        }

        public static string GetProductItemListPopup(ItemListData itemListData, string listkey = "", string entityTypeCode = "PRD", string entityTypeCodeLang = "PRDLANG")
        {
            var ajaxInfo = new NBrightInfo(true);
            ajaxInfo.SetXmlProperty("genxml/products", itemListData.products);
            ajaxInfo.SetXmlProperty("genxml/listkeys", itemListData.listkeys);
            ajaxInfo.SetXmlProperty("genxml/list", "");
            foreach (var l in itemListData.listnames)
            {
                ajaxInfo.SetXmlProperty("genxml/list/" + l.Key, l.Value);
            }
            ajaxInfo.SetXmlProperty("genxml/productsinlist", "");
            foreach (var l in itemListData.productsInList)
            {
                ajaxInfo.SetXmlProperty("genxml/productsinlist/" + l.Key, l.Value);
            }

            var modelsetings = StoreSettings.Current.Settings();
            if (!modelsetings.ContainsKey("listkeys")) modelsetings.Add("listkeys", itemListData.listkeys);
            var strOut = NBrightBuyUtils.RazorTemplRender("ItemListPopupBody.cshtml", -1, "", ajaxInfo, "/DesktopModules/NBright/NBrightBuy", _themeFolder, Utils.GetCurrentCulture(), modelsetings);

            return strOut;
        }


    }

}
