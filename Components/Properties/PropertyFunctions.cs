using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Razor;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using NBrightCore.common;
using NBrightCore.images;
using NBrightCore.render;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Admin;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;
using Nevoweb.DNN.NBrightBuy.Components.Category;
using Nevoweb.DNN.NBrightBuy.Components.Products;

namespace Nevoweb.DNN.NBrightBuy.Components.Clients
{
    public class PropertyFunctions
    {
        public string UiLang = "";
        public string EditLangCurrent = "";
        public string EntityTypeCode = "";
        public string TemplateRelPath = "/DesktopModules/NBright/NBrightBuy";

        private  NBrightBuyController _objCtrl = new NBrightBuyController();
        private bool DebugMode => StoreSettings.Current.DebugMode;

        public void ResetTemplateRelPath()
        {
            TemplateRelPath = "/DesktopModules/NBright/NBrightBuy";
        }

        public string ProcessCommand(string paramCmd, HttpContext context, string editlang = "")
        {
            var categoryFunctions = new CategoryFunctions();

            var strOut = "PROPERTY - ERROR!! - No Security rights or function command.";
            var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
            UiLang = ajaxInfo.GetXmlProperty("genxml/hidden/uilang");
            if (UiLang == "") UiLang = EditLangCurrent;
            EntityTypeCode = ajaxInfo.GetXmlProperty("genxml/hidden/entitytypecode");
            if (EntityTypeCode == "") EntityTypeCode = "CATEGORY"; // default to category
            UiLang = NBrightBuyUtils.GetUILang(ajaxInfo);
            EditLangCurrent = editlang;
            if (EditLangCurrent == "") EditLangCurrent = NBrightBuyUtils.GetEditLang(ajaxInfo);

            if (!paramCmd.ToLower().Contains("save"))
            {
                // pickup nextlang, indicates if we are changing languages. (Don't use if saving data, only for getting next language.)
                EditLangCurrent = NBrightBuyUtils.GetNextLang(ajaxInfo, EditLangCurrent);
            }

            if (PluginUtils.CheckPluginSecurity(PortalSettings.Current.PortalId, "propertiesvalue"))
            {
                switch (paramCmd)
                {
                    case "property_admin_getlist":
                        strOut = categoryFunctions.CategoryAdminList(context, "property", EditLangCurrent);
                        break;
                    case "property_admin_getdetail":
                        strOut = categoryFunctions.CategoryAdminDetail(context, 0, EditLangCurrent);
                        break;
                    case "property_admin_addnew":
                        strOut = categoryFunctions.CategoryAdminAddNew(context, "property");
                        break;
                    case "property_admin_savelist":
                        strOut = categoryFunctions.CategoryAdminSaveList(context);
                        break;
                    case "property_admin_save":
                        strOut = categoryFunctions.CategorySave(context, EditLangCurrent);
                        break;
                    case "property_admin_saveexit":
                        strOut = categoryFunctions.CategorySave(context, EditLangCurrent);
                        break;
                    case "property_admin_movecategory":
                        strOut = categoryFunctions.MoveCategoryAdmin(context, "property");
                        break;
                    case "property_admin_delete":
                        strOut = categoryFunctions.DeleteCategory(context, "property");
                        break;
                    case "property_updateimages":
                        strOut = categoryFunctions.UpdateCategoryImages(context, EditLangCurrent);
                        break;
                    case "property_getproductselectlist":
                        var productFunctions = new ProductFunctions();
                        strOut = productFunctions.ProductAdminList(context, true, EditLangCurrent, "", true);
                        break;
                    case "property_selectchangehidden":
                        strOut = categoryFunctions.CategoryHidden(context);
                        break;
                    case "property_selectcatxref":
                        strOut = categoryFunctions.SelectCatXref(context, EditLangCurrent);
                        break;
                    case "property_deletecatxref":
                        strOut = categoryFunctions.DeleteCatXref(context);
                        break;
                    case "property_deleteallcatxref":
                        strOut = categoryFunctions.DeleteAllCatXref(context, EditLangCurrent);
                        break;
                }
            }

            switch (paramCmd)
            {
                case "property_categoryproductlist":
                    strOut = categoryFunctions.GetCategoryProductList(context, EditLangCurrent);
                    break;
                case "property_removeimage":
                    strOut = categoryFunctions.RemoveCategoryImage(context, EditLangCurrent);
                    break;
                case "property_displayproductselect":
                    strOut = categoryFunctions.CategoryProductSelect(context, EditLangCurrent);
                    break;
            }

            return strOut;
        }



    }
}
