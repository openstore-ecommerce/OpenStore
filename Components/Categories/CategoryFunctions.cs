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
using Nevoweb.DNN.NBrightBuy.Components.Products;

namespace Nevoweb.DNN.NBrightBuy.Components.Category
{
    public class CategoryFunctions
    {
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
            var strOut = "CATEGORY - ERROR!! - No Security rights or function command.";
            var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
            EntityTypeCode = ajaxInfo.GetXmlProperty("genxml/hidden/entitytypecode");
            if (EntityTypeCode == "") EntityTypeCode = "CATEGORY"; // default to category
            EditLangCurrent = NBrightBuyUtils.GetEditLang(ajaxInfo,Utils.GetCurrentCulture());

            if (!paramCmd.ToLower().Contains("save"))
            {
                // pickup nextlang, indicates if we are changing languages. (Don't use if saving data, only for getting next language.)
                EditLangCurrent = NBrightBuyUtils.GetNextLang(ajaxInfo, EditLangCurrent);
            }

            if (PluginUtils.CheckPluginSecurity(PortalSettings.Current.PortalId, "categories"))
            {
                switch (paramCmd)
                {
                    case "category_admin_getlist":
                        strOut = CategoryAdminList(context, "", EditLangCurrent);
                        break;
                    case "category_admin_getdetail":
                        strOut = CategoryAdminDetail(context, 0, EditLangCurrent);
                        break;
                    case "category_admin_addnew":
                        strOut = CategoryAdminAddNew(context);
                        break;
                    case "category_admin_savelist":
                        strOut = CategoryAdminSaveList(context);
                        break;
                    case "category_admin_save":
                        strOut = CategorySave(context, EditLangCurrent);
                        break;
                    case "category_admin_saveexit":
                        strOut = CategorySave(context, EditLangCurrent);
                        break;
                    case "category_admin_movecategory":
                        strOut = MoveCategoryAdmin(context);
                        break;
                    case "category_admin_delete":
                        strOut = DeleteCategory(context);
                        break;
                    case "category_updateimages":
                        strOut = UpdateCategoryImages(context, EditLangCurrent);
                        break;
                    case "category_getproductselectlist":
                        var productFunctions = new ProductFunctions();
                        strOut = productFunctions.ProductAdminList(context, true, EditLangCurrent, "", true);
                        break;
                    case "category_selectchangehidden":
                        strOut = CategoryHidden(context);
                        break;
                    case "category_selectcatxref":
                        strOut = SelectCatXref(context, EditLangCurrent);
                        break;
                    case "category_deletecatxref":
                        strOut = DeleteCatXref(context);
                        break;
                    case "category_deleteallcatxref":
                        strOut = DeleteAllCatXref(context, EditLangCurrent);
                        break;
                    case "category_copyallcatxref":
                        strOut = CopyAllCatXref(context);
                        break;
                    case "category_moveallcatxref":
                        strOut = CopyAllCatXref(context, true);
                        break;
                    case "category_cattaxupdate":
                        strOut = CatTaxUpdate(context, EditLangCurrent);
                        break;
                    case "category_addgroupfilter":
                        strOut = AddGroupFilter(context, EditLangCurrent);
                        break;
                    case "category_removegroupfilter":
                        strOut = RemoveGroupFilter(context, EditLangCurrent);
                        break;
                    case "category_categorygroupfilter":
                        strOut = CategoryGroupFilters(context, EditLangCurrent);
                        break;
                }
            }

            switch (paramCmd)
            {
                case "category_categoryproductlist":
                    strOut = GetCategoryProductList(context, EditLangCurrent);
                    break;
                case "category_removeimage":
                    strOut = RemoveCategoryImage(context, EditLangCurrent);
                    break;
                case "category_displayproductselect":
                    strOut = CategoryProductSelect(context, EditLangCurrent);
                    break;
            }

            return strOut;
        }


        public String CategoryAdminList(HttpContext context, string editType, string editLangCurrent)
        {
            var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
            var razortemplate = ajaxInfo.GetXmlProperty("genxml/hidden/razortemplate");
            if (razortemplate == "") razortemplate = "Admin_CategoryList.cshtml";
            var themefolder = ajaxInfo.GetXmlProperty("genxml/hidden/themefolder");
            if (themefolder == "") themefolder = "config";

            var catid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedcatid");
            var grpCats = new List<NBrightInfo>();
            if (editType.ToLower() == "property")
            {
                var selgroup = ajaxInfo.GetXmlProperty("genxml/hidden/selectedgroup");
                grpCats = NBrightBuyUtils.GetCatList(catid, selgroup, editLangCurrent);
            }
            else
                grpCats = NBrightBuyUtils.GetCatList(catid, "cat", editLangCurrent);

            var strOut = NBrightBuyUtils.RazorTemplRenderList(razortemplate, 0, "", grpCats, TemplateRelPath, themefolder, Utils.GetCurrentCulture(), StoreSettings.Current.Settings());

            return strOut;
        }

        public String CategoryAdminAddNew(HttpContext context,string editType = "")
        {
            var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
            var categoryData = CategoryUtils.GetCategoryData(-1, EditLangCurrent);
            var selgroup = ajaxInfo.GetXmlProperty("genxml/hidden/selectedgroup");
            if (selgroup == "") selgroup = "cat";
            categoryData.GroupType = selgroup;
            categoryData.DataRecord.SetXmlProperty("genxml/checkbox/chkishidden", "True");
            categoryData.DataRecord.ParentItemId = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedcatid");
            categoryData.DataRecord.SetXmlProperty("genxml/dropdownlist/ddlparentcatid", categoryData.DataRecord.ParentItemId.ToString());
            categoryData.Save();
            NBrightBuyUtils.RemoveModCachePortalWide(PortalSettings.Current.PortalId);
            return CategoryAdminList(context, editType, EditLangCurrent);
        }

        public String DeleteCategory(HttpContext context, string editType = "")
        {
            var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
            var selectedcatid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedcatid");
            var categoryData = CategoryUtils.GetCategoryData(selectedcatid, EditLangCurrent);
            categoryData.Delete();
            NBrightBuyUtils.RemoveModCachePortalWide(PortalSettings.Current.PortalId);
            return CategoryAdminList(context, editType, EditLangCurrent);
        }


        public String CategoryAdminSaveList(HttpContext context)
        {
            var ajaxInfoList = NBrightBuyUtils.GetAjaxInfoList(context);

            foreach (var nbi in ajaxInfoList)
            {
                if (nbi.GetXmlPropertyBool("genxml/hidden/isdirty"))
                {
                    var categoryData = CategoryUtils.GetCategoryData(nbi.GetXmlPropertyInt("genxml/hidden/itemid"), nbi.GetXmlProperty("genxml/hidden/categorylang"));
                    if (categoryData.Exists)
                    {
                        categoryData.Name = nbi.GetXmlProperty("genxml/textbox/txtcategoryname");
                        if (categoryData.DataRecord.GetXmlProperty("genxml/textbox/propertyref") == "")
                        {
                            categoryData.DataRecord.SetXmlProperty("genxml/textbox/propertyref", nbi.GetXmlProperty("genxml/textbox/propertyref"));
                        }
                        categoryData.Save();
                    }
                }
            }
            NBrightBuyUtils.RemoveModCachePortalWide(PortalSettings.Current.PortalId);
            return "";
        }

        public String ProductAdminSave(HttpContext context, string editLangCurrent)
        {
            try
            {
                try
                {
                    EditLangCurrent = editLangCurrent;
                    CategorySave(context, EditLangCurrent);
                    return "";
                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }


        public String CategorySave(HttpContext context, string editLangCurrent)
        {
            if (NBrightBuyUtils.CheckManagerRights())
            {
                EditLangCurrent = editLangCurrent;
                var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
                var parentitemid = ajaxInfo.GetXmlPropertyInt("genxml/dropdownlist/ddlparentcatid");
                var catid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/itemid");
                if (catid > 0)
                {
                    if (parentitemid != catid)
                    {
                        var catData = new CategoryData(catid, EditLangCurrent);

                        // check we've not put a category under it's child
                        if (!IsParentInChildren(catData, parentitemid))
                        {
                            var catDirectList = catData.GetDirectArticles();
                            var oldparentitemId = catData.ParentItemId;
                            if (parentitemid != oldparentitemId)
                            {
                                // remove articles for category, so we realign the cascade records.                            
                                foreach (var p in catDirectList)
                                {
                                    var prdData = new ProductData(p.ParentItemId, p.PortalId, p.Lang);
                                    prdData.RemoveCategory(catData.CategoryId);
                                }
                            }

                            catData.Update(ajaxInfo);

                            // the base category ref cannot have language dependant refs, we therefore just use a unique key
                            var catref = catData.DataRecord.GetXmlProperty("genxml/textbox/txtcategoryref");
                            if (catref == "")
                            {
                                if (catData.DataRecord.GUIDKey == "")
                                {
                                    catref = Utils.GetUniqueKey().ToLower();
                                    catData.DataRecord.SetXmlProperty("genxml/textbox/txtcategoryref", catref);
                                    catData.DataRecord.GUIDKey = catref;
                                }
                                else
                                {
                                    catData.DataRecord.SetXmlProperty("genxml/textbox/txtcategoryref", catData.DataRecord.GUIDKey);
                                }
                            }
                            catData.Save();
                            CategoryUtils.ValidateLangaugeRef(PortalSettings.Current.PortalId, catid); // do validate so we update all refs and children refs
                            NBrightBuyUtils.RemoveModCachePortalWide(PortalSettings.Current.PortalId);

                            if (parentitemid != oldparentitemId)
                            {
                                // all all articles for category. so we realign the cascade records.                            
                                foreach (var p in catDirectList)
                                {
                                    var prdData = new ProductData(p.ParentItemId, p.PortalId, p.Lang);
                                    prdData.AddCategory(catData.CategoryId);
                                }
                            }
                        }
                    }
                }
                NBrightBuyUtils.RemoveModCachePortalWide(PortalSettings.Current.PortalId);
            }
            return "";
        }

        private Boolean IsParentInChildren(CategoryData catData, int parentItemId)
        {
            foreach (var ch in catData.GetDirectChildren())
            {
                if (ch.ItemID == parentItemId) return true;
                var catChildData = CategoryUtils.GetCategoryData(ch.ItemID, EditLangCurrent);
                if (IsParentInChildren(catChildData, parentItemId)) return true;
            }
            return false;
        }


        public String MoveCategoryAdmin(HttpContext context,string editType = "")
        {
            var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
            var movecatid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/movecatid");
            var movetocatid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/movetocatid");

            if (movecatid > 0 && movetocatid > 0)
            {
                MoveRecord(movetocatid, movecatid);
            }

            NBrightBuyUtils.RemoveModCachePortalWide(PortalSettings.Current.PortalId);
            return CategoryAdminList(context, editType, EditLangCurrent);
        }

        private void MoveRecord(int movetocatid, int movecatid)
        {
            if (movecatid > 0)
            {
                var movData = CategoryUtils.GetCategoryData(movetocatid, EditLangCurrent);
                var selData = CategoryUtils.GetCategoryData(movecatid, EditLangCurrent);
                if (movData.Exists && selData.Exists)
                {
                    var fromParentItemid = selData.DataRecord.ParentItemId;
                    var toParentItemid = movData.DataRecord.ParentItemId;
                    var reindex = toParentItemid != fromParentItemid;
                    var objGrpCtrl = new GrpCatController(EditLangCurrent);
                    var movGrp = objGrpCtrl.GetGrpCategory(movData.Info.ItemID);
                    if (!movGrp.Parents.Contains(selData.Info.ItemID)) // cannot move a category into itself (i.e. move parent into sub-category)
                    {
                        selData.DataRecord.SetXmlProperty("genxml/dropdownlist/ddlparentcatid", toParentItemid.ToString(""));
                        selData.DataRecord.ParentItemId = toParentItemid;
                        selData.DataRecord.SetXmlProperty("genxml/dropdownlist/ddlgrouptype", movData.DataRecord.GetXmlProperty("genxml/dropdownlist/ddlgrouptype"));
                        var strneworder = movData.DataRecord.GetXmlPropertyDouble("genxml/hidden/recordsortorder");
                        var selorder = selData.DataRecord.GetXmlPropertyDouble("genxml/hidden/recordsortorder");
                        var neworder = Convert.ToDouble(strneworder, CultureInfo.GetCultureInfo("en-US"));
                        if (strneworder < selorder)
                            neworder = neworder - 0.5;
                        else
                            neworder = neworder + 0.5;
                        selData.DataRecord.SetXmlProperty("genxml/hidden/recordsortorder", neworder.ToString(""), TypeCode.Double);
                        var objCtrl = new NBrightBuyController();
                        objCtrl.Update(selData.DataRecord);

                        FixRecordSortOrder(toParentItemid.ToString(""), EditLangCurrent); //reindex all siblings (this is so we get a int on the recordsortorder)
                        FixRecordGroupType(selData.Info.ItemID.ToString(""), selData.DataRecord.GetXmlProperty("genxml/dropdownlist/ddlgrouptype"), EditLangCurrent);

                        if (reindex)
                        {
                            objGrpCtrl.ReIndexCascade(fromParentItemid); // reindex from parent and parents.
                            objGrpCtrl.ReIndexCascade(selData.Info.ItemID); // reindex select and parents
                        }
                        NBrightBuyUtils.RemoveModCachePortalWide(PortalSettings.Current.PortalId);
                    }
                }
            }
        }

        private void FixRecordGroupType(String parentid, String groupType, string editLangCurrent)
        {
            if (Utils.IsNumeric(parentid) && Convert.ToInt32(parentid) > 0)
            {
                EditLangCurrent = editLangCurrent;
                // fix any incorrect sort orders
                var strFilter = " and NB1.ParentItemId = " + parentid + " ";
                var levelList = _objCtrl.GetDataList(PortalSettings.Current.PortalId, -1, "CATEGORY", "CATEGORYLANG", EditLangCurrent, strFilter, " order by [XMLData].value('(genxml/hidden/recordsortorder)[1]','decimal(10,2)') ", true);
                foreach (NBrightInfo catinfo in levelList)
                {
                    var grouptype = catinfo.GetXmlProperty("genxml/dropdownlist/ddlgrouptype");
                    var catData = CategoryUtils.GetCategoryData(catinfo.ItemID, EditLangCurrent);
                    if (grouptype != groupType)
                    {
                        catData.DataRecord.SetXmlProperty("genxml/dropdownlist/ddlgrouptype", groupType);
                        _objCtrl.Update(catData.DataRecord);
                    }
                    FixRecordGroupType(catData.Info.ItemID.ToString(""), groupType, editLangCurrent);
                }
            }
        }

        private void FixRecordSortOrder(String parentid,string editLangCurrent)
        {
            EditLangCurrent = editLangCurrent;
            if (!Utils.IsNumeric(parentid)) parentid = "0";
            // fix any incorrect sort orders
            Double lp = 1;
            var strFilter = " and NB1.ParentItemId = " + parentid + " ";
            var levelList = _objCtrl.GetDataList(PortalSettings.Current.PortalId, -1, "CATEGORY", "CATEGORYLANG", EditLangCurrent, strFilter, " order by [XMLData].value('(genxml/hidden/recordsortorder)[1]','decimal(10,2)') ", true);
            foreach (NBrightInfo catinfo in levelList)
            {
                var recordsortorder = catinfo.GetXmlProperty("genxml/hidden/recordsortorder");
                if (!Utils.IsNumeric(recordsortorder) || Convert.ToDouble(recordsortorder, CultureInfo.GetCultureInfo("en-US")) != lp)
                {
                    var catData = CategoryUtils.GetCategoryData(catinfo.ItemID, EditLangCurrent);
                    catData.DataRecord.SetXmlProperty("genxml/hidden/recordsortorder", lp.ToString(""));
                    _objCtrl.Update(catData.DataRecord);
                }
                lp += 1;
            }
        }


        public string CategoryHidden(HttpContext context)
        {
            try
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var parentitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedcatid");
                if (parentitemid > 0)
                {
                    var catData = CategoryUtils.GetCategoryData(parentitemid, EditLangCurrent);

                    if (catData.DataRecord.GetXmlPropertyBool("genxml/checkbox/chkishidden"))
                    {
                        catData.DataRecord.SetXmlProperty("genxml/checkbox/chkishidden", "False");
                    }
                    else
                    {
                        catData.DataRecord.SetXmlProperty("genxml/checkbox/chkishidden", "True");
                    }
                    catData.Save();
                    // remove save GetData cache
                    NBrightBuyUtils.RemoveModCachePortalWide(PortalSettings.Current.PortalId);
                    return "";
                }
                return "Invalid parentitemid";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }


        public String CategoryAdminDetail(HttpContext context, int catid,string editLangCurrent)
        {
            try
            {
                if (NBrightBuyUtils.CheckManagerRights())
                {
                    EditLangCurrent = editLangCurrent;
                    var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                    var strOut = "";
                    var selecteditemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedcatid");
                    if (catid > 0) selecteditemid = catid;
                    if (Utils.IsNumeric(selecteditemid))
                    {
                        var themeFolder = ajaxInfo.GetXmlProperty("genxml/hidden/themefolder");
                        var razortemplate = ajaxInfo.GetXmlProperty("genxml/hidden/razortemplate");
                        var portalId = PortalSettings.Current.PortalId;

                        var passSettings = ajaxInfo.ToDictionary();
                        foreach (var s in StoreSettings.Current.Settings()) // copy store setting, otherwise we get a byRef assignement
                        {
                            if (passSettings.ContainsKey(s.Key))
                                passSettings[s.Key] = s.Value;
                            else
                                passSettings.Add(s.Key, s.Value);
                        }

                        if (selecteditemid <= 0) return "";

                        if (themeFolder == "") themeFolder = StoreSettings.Current.ThemeFolder;

                        var objCtrl = new NBrightBuyController();
                        var info = objCtrl.GetData(Convert.ToInt32(selecteditemid), EntityTypeCode + "LANG", EditLangCurrent,true);

                        strOut = NBrightBuyUtils.RazorTemplRender(razortemplate, 0, "", info, TemplateRelPath, themeFolder, Utils.GetCurrentCulture(), passSettings);
                    }
                    return strOut;
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public String GetCategoryProductList(HttpContext context,string editLangCurrent)
        {
            try
            {
                EditLangCurrent = editLangCurrent;
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var objQual = DotNetNuke.Data.DataProvider.Instance().ObjectQualifier;
                var dbOwner = DotNetNuke.Data.DataProvider.Instance().DatabaseOwner;

                var strFilter = " and NB1.[ItemId] in (select parentitemid from " + dbOwner + "[" + objQual + "NBrightBuy] where typecode = 'CATXREF' and XrefItemId = {Settings:itemid}) ";

                strFilter = Utils.ReplaceSettingTokens(strFilter, ajaxInfo.ToDictionary());

                ajaxInfo.SetXmlProperty("genxml/hidden/filter", strFilter);
                ajaxInfo.SetXmlProperty("genxml/hidden/razortemplate", "Admin_CategoryProducts.cshtml");
                ajaxInfo.SetXmlProperty("genxml/hidden/themefolder", "config");

                var productFunctions = new ProductFunctions();
                return productFunctions.ProductAdminList(context, true, EditLangCurrent,"",true);

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }


        }

        public String CategoryProductSelect(HttpContext context,string editLangCurrent)
        {
            try
            {
                EditLangCurrent = editLangCurrent;
                var strOut = "";
                if (NBrightBuyUtils.CheckManagerRights())
                {
                    var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                    var selecteditemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedcatid");
                    if (Utils.IsNumeric(selecteditemid))
                    {
                        var themeFolder = ajaxInfo.GetXmlProperty("genxml/hidden/themefolder");
                        var razortemplate = ajaxInfo.GetXmlProperty("genxml/hidden/razortemplate");

                        var passSettings = ajaxInfo.ToDictionary();
                        foreach (var s in StoreSettings.Current.Settings()) // copy store setting, otherwise we get a byRef assignement
                        {
                            if (passSettings.ContainsKey(s.Key))
                                passSettings[s.Key] = s.Value;
                            else
                                passSettings.Add(s.Key, s.Value);
                        }

                        if (selecteditemid <= 0) return "";

                        if (themeFolder == "") themeFolder = StoreSettings.Current.ThemeFolder;

                        var objCtrl = new NBrightBuyController();
                        var info = objCtrl.GetData(Convert.ToInt32(selecteditemid), EntityTypeCode + "LANG", EditLangCurrent, true);

                        strOut = NBrightBuyUtils.RazorTemplRender(razortemplate, 0, "", info, TemplateRelPath, themeFolder, Utils.GetCurrentCulture(), passSettings);
                    }
                }
                return strOut;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }


        }

        #region "fileupload"

        public string UpdateCategoryImages(HttpContext context,string editLangCurrent)
        {
            EditLangCurrent = editLangCurrent;
            //get uploaded params
            var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
            var catitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedcatid");
            var imguploadlist = ajaxInfo.GetXmlProperty("genxml/hidden/imguploadlist");
            var strOut = "";

            if (catitemid > 0)
            {
                var imgs = imguploadlist.Split(',');
                foreach (var img in imgs)
                {
                    if (ImgUtils.IsImageFile(Path.GetExtension(img)) && img != "")
                    {
                        var fn = DnnUtils.Encrypt(img, StoreSettings.Current.Get("adminpin"));
                        foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                        {
                            fn = fn.Replace(c, '_');
                        }
                        var extension = Path.GetExtension(img);
                        var fullName = StoreSettings.Current.FolderTempMapPath.TrimEnd(Convert.ToChar("\\")) + "\\" + fn;
                        if (File.Exists(fullName))
                        {
                            File.Move(fullName, fullName + extension);
                            fullName = fullName + extension;
                            var imgResize = StoreSettings.Current.GetInt(StoreSettingKeys.productimageresize);
                            if (imgResize == 0) imgResize = 800;
                            var productFunctions = new ProductFunctions();
                            var imagepath = productFunctions.ResizeImage(fullName, imgResize);
                            var imageurl = StoreSettings.Current.FolderImages.TrimEnd('/') + "/" + Path.GetFileName(imagepath);
                            AddNewImage(catitemid, imageurl, imagepath, EditLangCurrent);
                        }
                    }
                }
            }
            return CategoryAdminDetail(context, 0, EditLangCurrent);
        }

        private void AddNewImage(int itemId, String imageurl, String imagepath,string editLangCurrent)
        {
            EditLangCurrent = editLangCurrent;
            var catData = new CategoryData(itemId,EditLangCurrent);
            if (catData.Exists)
            {
                catData.DataRecord.SetXmlProperty("genxml/hidden/imageurl", imageurl);
                catData.DataRecord.SetXmlProperty("genxml/hidden/imagepath", imagepath);
                catData.Save();
            }
        }

        public string RemoveCategoryImage(HttpContext context,string editLangCurrent)
        {
            EditLangCurrent = editLangCurrent;
            //get uploaded params
            var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
            var catitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedcatid");

            var catData = new CategoryData(catitemid, EditLangCurrent);
            if (catData.Exists)
            {
                catData.DataRecord.SetXmlProperty("genxml/hidden/imageurl", "");
                catData.DataRecord.SetXmlProperty("genxml/hidden/imagepath", "");
                catData.Save();
            }
            return CategoryAdminDetail(context,0,EditLangCurrent);
        }

        #endregion

        public string SelectCatXref(HttpContext context,string editLangCurrent)
        {
            try
            {
                EditLangCurrent = editLangCurrent;
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var catid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedcatid");
                var selectedproductid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectproductid");
                if (selectedproductid > 0 && catid > 0)
                {
                    var prodData = ProductUtils.GetProductData(selectedproductid, EditLangCurrent, false);
                    prodData.AddCategory(catid);
                }
                else
                    return "Invalid parentitemid or xrefitmeid";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            return "";
        }
        public string DeleteAllCatXref(HttpContext context,string editLangCurrent)
        {
            EditLangCurrent = editLangCurrent;
            var strOut = NBrightBuyUtils.GetResxMessage("general_fail");
            try
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var catid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedcatid");
                if (catid > 0)
                {
                    var catData = new CategoryData(catid, EditLangCurrent);
                    foreach (var cxref in catData.GetAllArticles())
                    {
                        var prdData = new ProductData(cxref.ParentItemId, cxref.PortalId, EditLangCurrent);
                        prdData.RemoveCategory(catid);
                    }
                }
                strOut = NBrightBuyUtils.GetResxMessage();
                DataCache.ClearCache();
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            return strOut;
        }

        public string DeleteCatXref(HttpContext context)
        {
            try
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var catid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedcatid");
                var selectproductid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectproductid");
                if (selectproductid > 0 && catid > 0)
                {
                    var prodData = ProductUtils.GetProductData(selectproductid, EditLangCurrent, false);
                    prodData.RemoveCategory(catid);
                }
                else
                    return "Invalid parentitemid or xrefitemid";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            return "";
        }


        public String CopyAllCatXref(HttpContext context, Boolean moverecords = false)
        {
            var strOut = NBrightBuyUtils.GetResxMessage("general_fail");
            try
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var catid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedcatid");
                var newcatid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/newcatid");

                if (newcatid > 0 && catid > 0 && catid != newcatid)
                {

                    NBrightBuyUtils.CopyAllCatXref(catid, Convert.ToInt32(newcatid), moverecords);

                    strOut = NBrightBuyUtils.GetResxMessage();
                    DataCache.ClearCache();
                }

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
            return strOut;
        }

        public string CatTaxUpdate(HttpContext context,string editLangCurrent)
        {
            try
            {
                EditLangCurrent = editLangCurrent;
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var catid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedcatid");
                var taxrate = ajaxInfo.GetXmlProperty("genxml/hidden/selecttaxrate");
                if (catid > 0)
                {
                    var catData = new CategoryData(catid, EditLangCurrent);
                    foreach (var cxref in catData.GetAllArticles())
                    {
                        var strXml = "<genxml><models>";
                        var prdData = new ProductData(cxref.ParentItemId, cxref.PortalId, EditLangCurrent);
                        foreach (var mod in prdData.Models)
                        {
                            mod.SetXmlProperty("genxml/dropdownlist/taxrate", taxrate);
                            strXml += mod.XMLData;
                        }
                        strXml += "</models></genxml>";
                        prdData.DataRecord.ReplaceXmlNode(strXml, "genxml/models", "genxml");
                        prdData.Save();
                    }
                }
                else
                    return "Invalid catid";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            return "";
        }


        public string AddGroupFilter(HttpContext context,string editLangCurrent)
        {
            try
            {
                EditLangCurrent = editLangCurrent;
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var catid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedcatid");
                var groupref = ajaxInfo.GetXmlProperty("genxml/hidden/selectedgroupref");
                if (catid > 0 && groupref != "")
                {
                    var grp = _objCtrl.GetByGuidKey(PortalSettings.Current.PortalId, -1, "GROUP", groupref);
                    if (grp != null)
                    {
                        var catData = new CategoryData(catid, EditLangCurrent);
                        catData.AddFilterGroup(grp.ItemID);
                    }
                }
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            return CategoryGroupFilters(context, EditLangCurrent);
        }
        public string RemoveGroupFilter(HttpContext context, string editLangCurrent)
        {
            try
            {
                EditLangCurrent = editLangCurrent;
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var catid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedcatid");
                var groupid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedgroupid");
                if (catid > 0)
                {
                    var catData = new CategoryData(catid, EditLangCurrent);
                    catData.RemoveFilterGroup(groupid);
                }
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            return CategoryGroupFilters(context, EditLangCurrent);
        }

        public String CategoryGroupFilters(HttpContext context,string editLangCurrent)
        {
            try
            {
                if (NBrightBuyUtils.CheckManagerRights())
                {
                    EditLangCurrent = editLangCurrent;
                    var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                    var strOut = "";
                    var catid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedcatid");
                    if (catid > 0)
                    {
                        var themeFolder = "config";
                        var razortemplate = "Admin_CategoryFilterGroups.cshtml";

                        var passSettings = ajaxInfo.ToDictionary();
                        foreach (var s in StoreSettings.Current.Settings()) // copy store setting, otherwise we get a byRef assignement
                        {
                            if (passSettings.ContainsKey(s.Key))
                                passSettings[s.Key] = s.Value;
                            else
                                passSettings.Add(s.Key, s.Value);
                        }

                        var objCtrl = new NBrightBuyController();
                        var info = objCtrl.GetData(catid, EntityTypeCode + "LANG", EditLangCurrent, true);

                        strOut = NBrightBuyUtils.RazorTemplRender(razortemplate, 0, "", info, TemplateRelPath, themeFolder, Utils.GetCurrentCulture(), passSettings);
                    }
                    return strOut;
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }


    }
}
