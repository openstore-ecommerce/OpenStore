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

namespace Nevoweb.DNN.NBrightBuy.Components.Orders
{
    public static class OrderFunctions
    {

        public static string ProcessCommand(string paramCmd, HttpContext context)
        {
            var strOut = "ORDER - ERROR!! - No Security rights for current user!";

            switch (paramCmd)
            {

                case "orderadmin_getlist":
                    strOut = OrderAdminList(context);
                    break;
                case "orderadmin_getdetail":
                    strOut = OrderAdminDetail(context);
                    break;
                case "orderadmin_reorder":
                    strOut = OrderAdminReOrder(context);
                    break;
                case "orderadmin_edit":
                    strOut = OrderAdminEdit(context);
                    break;
                case "orderadmin_save":
                    strOut = OrderAdminSave(context);
                    break;
                case "orderadmin_removeinvoice":
                    strOut = OrderAdminRemoveInvoice(context);
                    break;
                case "orderadmin_sendemail":
                    strOut = OrderAdminEmail(context);
                    break;
                case "orderadmin_getexport":
                    strOut = OrderAdminExport(context);
                    break;
            }

            return strOut;
        }

        #region "Order Admin Methods"

        private static String OrderAdminList(HttpContext context)
        {
            try
            {
                if (UserController.Instance.GetCurrentUserInfo().UserID > 0)
                {
                    var settings = NBrightBuyUtils.GetAjaxDictionary(context);
                    return GetOrderListData(settings);
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        private static String OrderAdminExport(HttpContext context)
        {
            try
            {
                if (UserController.Instance.GetCurrentUserInfo().UserID > 0)
                {
                    var settings = NBrightBuyUtils.GetAjaxDictionary(context);
                    return GetOrderListData(settings,false,true);
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        private static String OrderAdminDetail(HttpContext context)
        {
            try
            {
                if (UserController.Instance.GetCurrentUserInfo().UserID > 0)
                {
                    var settings = NBrightBuyUtils.GetAjaxDictionary(context);
                    return GetOrderDetailData(settings);
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        private static String OrderAdminReOrder(HttpContext context)
        {
            try
            {
                if (UserController.Instance.GetCurrentUserInfo().UserID > 0)
                {
                    var settings = NBrightBuyUtils.GetAjaxDictionary(context);
                    if (!settings.ContainsKey("selecteditemid")) settings.Add("selecteditemid", "");
                    var selecteditemid = settings["selecteditemid"];
                    if (Utils.IsNumeric(selecteditemid))
                    {
                        var orderData = new OrderData(PortalSettings.Current.PortalId, Convert.ToInt32(selecteditemid));
                        if (orderData.UserId == UserController.Instance.GetCurrentUserInfo().UserID || NBrightBuyUtils.CheckRights())
                        {
                            orderData.CopyToCart(false);
                        }
                    }
                    return "";
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        private static String OrderAdminEdit(HttpContext context)
        {
            try
            {
                if (UserController.Instance.GetCurrentUserInfo().UserID > 0)
                {
                    var settings = NBrightBuyUtils.GetAjaxDictionary(context);
                    if (!settings.ContainsKey("selecteditemid")) settings.Add("selecteditemid", "");
                    var selecteditemid = settings["selecteditemid"];
                    if (Utils.IsNumeric(selecteditemid))
                    {
                        var orderData = new OrderData(PortalSettings.Current.PortalId, Convert.ToInt32(selecteditemid));
                        if (NBrightBuyUtils.CheckRights())
                        {
                            orderData.ConvertToCart(false);
                        }
                    }
                    return "";
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        private static String OrderAdminEmail(HttpContext context)
        {
            try
            {
                if (UserController.Instance.GetCurrentUserInfo().UserID > 0)
                {
                    //get uploaded params
                    var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                    NBrightBuyUtils.SendOrderEmail(ajaxInfo.GetXmlProperty("genxml/hidden/emailtype"), ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid"), ajaxInfo.GetXmlProperty("genxml/hidden/emailsubject"), "", ajaxInfo.GetXmlProperty("genxml/hidden/emailmessage"), true);
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }


        }


        private static String OrderAdminSave(HttpContext context)
        {
            try
            {
                if (NBrightBuyUtils.CheckManagerRights())
                {
                    var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                    var itemId = ajaxInfo.GetXmlPropertyInt("genxml/hidden/itemid");
                    if (itemId > 0)
                    {
                        var ordData = new OrderData(itemId);
                        if (ordData != null)
                        {
                            var newStatusOrder = ajaxInfo.GetXmlProperty("genxml/dropdownlist/orderstatus");
                            if (ordData.OrderStatus != newStatusOrder)
                            {
                                ordData.OrderStatus = newStatusOrder;
                            }

                            ordData.PurchaseInfo.SetXmlProperty("genxml/textbox/shippingdate", ajaxInfo.GetXmlProperty("genxml/textbox/shippingdate"), TypeCode.DateTime);
                            ordData.PurchaseInfo.SetXmlProperty("genxml/textbox/trackingcode", ajaxInfo.GetXmlProperty("genxml/textbox/trackingcode"));

                            // do audit notes
                            if (ajaxInfo.GetXmlProperty("genxml/textbox/auditnotes") != "")
                            {
                                ordData.AddAuditMessage(ajaxInfo.GetXmlProperty("genxml/textbox/auditnotes"), "notes", UserController.Instance.GetCurrentUserInfo().Username, "False");
                            }

                            // save relitive path also
                            if (ajaxInfo.GetXmlProperty("genxml/hidden/optionfilelist") != "")
                            {
                                var fname = Path.GetFileName(ajaxInfo.GetXmlProperty("genxml/hidden/optionfilelist"));

                                if (File.Exists(StoreSettings.Current.FolderTempMapPath.TrimEnd('\\') + "\\" + fname))
                                {
                                    var newfname = "secure" + Utils.GetUniqueKey();
                                    // save relitive path also
                                    if (File.Exists(ordData.PurchaseInfo.GetXmlProperty("genxml/hidden/invoicefilepath")))
                                    {
                                        File.Delete(StoreSettings.Current.FolderUploadsMapPath.TrimEnd('\\') + "\\" + newfname);
                                    }

                                    File.Copy(StoreSettings.Current.FolderTempMapPath.TrimEnd('\\') + "\\" + fname, StoreSettings.Current.FolderUploadsMapPath.TrimEnd('\\') + "\\" + newfname);
                                    File.Delete(StoreSettings.Current.FolderTempMapPath.TrimEnd('\\') + "\\" + fname);

                                    ordData.PurchaseInfo.SetXmlProperty("genxml/hidden/invoicefilepath", StoreSettings.Current.FolderUploadsMapPath.TrimEnd('\\') + "\\" + newfname);
                                    ordData.PurchaseInfo.SetXmlProperty("genxml/hidden/invoicefilename", newfname);
                                    ordData.PurchaseInfo.SetXmlProperty("genxml/hidden/invoiceuploadname", fname);
                                    ordData.PurchaseInfo.SetXmlProperty("genxml/hidden/invoicefileext", Path.GetExtension(fname));
                                    ordData.PurchaseInfo.SetXmlProperty("genxml/hidden/invoicefilerelpath", StoreSettings.Current.FolderUploads + "/" + newfname);
                                    ordData.PurchaseInfo.SetXmlProperty("genxml/hidden/invoicedownloadname", "NBS" + ordData.OrderNumber + Path.GetExtension(fname));
                                }
                            }



                            ordData.Save();
                        }
                    }

                    return "";
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }


        }


        private static String OrderAdminRemoveInvoice(HttpContext context)
        {
            try
            {
                if (NBrightBuyUtils.CheckManagerRights())
                {
                    var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                    var itemId = ajaxInfo.GetXmlPropertyInt("genxml/hidden/itemid");
                    if (itemId > 0)
                    {
                        var ordData = new OrderData(itemId);
                        if (ordData != null)
                        {

                            // save relitive path also
                            if (File.Exists(ordData.PurchaseInfo.GetXmlProperty("genxml/hidden/invoicefilepath")))
                            {
                                File.Delete(ordData.PurchaseInfo.GetXmlProperty("genxml/hidden/invoicefilepath"));
                            }


                            ordData.PurchaseInfo.SetXmlProperty("genxml/hidden/invoicefilepath", "");
                            ordData.PurchaseInfo.SetXmlProperty("genxml/hidden/invoicefilename", "");
                            ordData.PurchaseInfo.SetXmlProperty("genxml/hidden/invoicefileext", "");
                            ordData.PurchaseInfo.SetXmlProperty("genxml/hidden/invoicefilerelpath", "");
                            ordData.PurchaseInfo.SetXmlProperty("genxml/hidden/invoicedownloadname", "");
                            ordData.AddAuditMessage(NBrightBuyUtils.ResourceKey("OrderAdmin.cmdDeleteInvoice"), "invremove", UserController.Instance.GetCurrentUserInfo().Username, "False");

                            ordData.Save();
                        }
                    }

                    return "";
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }


        }


        #endregion

        private static String GetOrderDetailData(Dictionary<String, String> settings, bool paging = true)
        {
            var strOut = "";

            if (!settings.ContainsKey("themefolder")) settings.Add("themefolder", "");
            if (!settings.ContainsKey("razortemplate")) settings.Add("razortemplate", "");
            if (!settings.ContainsKey("portalid")) settings.Add("portalid", PortalSettings.Current.PortalId.ToString("")); // aways make sure we have portalid in settings
            if (!settings.ContainsKey("selecteditemid")) settings.Add("selecteditemid", "");

            var themeFolder = settings["themefolder"];
            var selecteditemid = settings["selecteditemid"];
            var razortemplate = settings["razortemplate"];
            var portalId = Convert.ToInt32(settings["portalid"]);

            var passSettings = settings;
            foreach (var s in StoreSettings.Current.Settings()) // copy store setting, otherwise we get a byRef assignement
            {
                if (passSettings.ContainsKey(s.Key))
                    passSettings[s.Key] = s.Value;
                else
                    passSettings.Add(s.Key, s.Value);
            }

            if (!Utils.IsNumeric(selecteditemid)) return "";

            if (themeFolder == "")
            {
                themeFolder = StoreSettings.Current.ThemeFolder;
                if (settings.ContainsKey("themefolder")) themeFolder = settings["themefolder"];
            }

            var ordData = new OrderData(portalId, Convert.ToInt32(selecteditemid));

            // check for user or manager.
            if (UserController.Instance.GetCurrentUserInfo().UserID != ordData.UserId)
            {
                if (!NBrightBuyUtils.CheckRights())
                {
                    return "";
                }
            }

            strOut = NBrightBuyUtils.RazorTemplRender(razortemplate, 0, "", ordData, "/DesktopModules/NBright/NBrightBuy", themeFolder, Utils.GetCurrentCulture(), passSettings);


            return strOut;
        }

        private static String GetOrderListData(Dictionary<String, String> settings, bool paging = true, bool csv = false)
        {
            if (UserController.Instance.GetCurrentUserInfo().UserID <= 0) return "";

            var strOut = "";

            if (!settings.ContainsKey("selecteduserid")) settings.Add("selecteduserid", "");

            if (!settings.ContainsKey("themefolder")) settings.Add("themefolder", "");
            if (!settings.ContainsKey("userid")) settings.Add("userid", "-1");
            if (!settings.ContainsKey("razortemplate")) settings.Add("razortemplate", "");
            if (!settings.ContainsKey("returnlimit")) settings.Add("returnlimit", "0");
            if (!settings.ContainsKey("pagenumber")) settings.Add("pagenumber", "0");
            if (!settings.ContainsKey("pagesize")) settings.Add("pagesize", "0");
            if (!settings.ContainsKey("searchtext")) settings.Add("searchtext", "");
            if (!settings.ContainsKey("dtesearchdatefrom")) settings.Add("dtesearchdatefrom", "");
            if (!settings.ContainsKey("dtesearchdateto")) settings.Add("dtesearchdateto", "");
            if (!settings.ContainsKey("searchorderstatus")) settings.Add("searchorderstatus", "");
            if (!settings.ContainsKey("portalid")) settings.Add("portalid", PortalSettings.Current.PortalId.ToString("")); // aways make sure we have portalid in settings

            if (!Utils.IsNumeric(settings["userid"])) settings["pagenumber"] = "1";
            if (!Utils.IsNumeric(settings["pagenumber"])) settings["pagenumber"] = "1";
            if (!Utils.IsNumeric(settings["pagesize"])) settings["pagesize"] = "20";
            if (!Utils.IsNumeric(settings["returnlimit"])) settings["returnlimit"] = "50";

            var themeFolder = settings["themefolder"];
            var razortemplate = settings["razortemplate"];
            var returnLimit = Convert.ToInt32(settings["returnlimit"]);
            var pageNumber = Convert.ToInt32(settings["pagenumber"]);
            var pageSize = Convert.ToInt32(settings["pagesize"]);
            var portalId = Convert.ToInt32(settings["portalid"]);
            var userid = settings["userid"];
            var selecteduserid = settings["selecteduserid"];

            var searchText = settings["searchtext"];
            var searchdatefrom = settings["dtesearchdatefrom"];
            var searchdateto = settings["dtesearchdateto"];
            var searchorderstatus = settings["searchorderstatus"];

            var filter = "";
            if (searchText != "")
            {
                filter += " and (    (([xmldata].value('(genxml/billaddress/genxml/textbox/firstname)[1]', 'nvarchar(max)') like '%" + searchText + "%' collate sql_latin1_general_cp1_ci_ai ))";
                filter += " or (([xmldata].value('(genxml/billaddress/genxml/textbox/lastname)[1]', 'nvarchar(max)') like '%" + searchText + "%' collate sql_latin1_general_cp1_ci_ai ))";
                filter += " or (([xmldata].value('(genxml/billaddress/genxml/textbox/unit)[1]', 'nvarchar(max)') like '%" + searchText + "%' collate sql_latin1_general_cp1_ci_ai ))";
                filter += " or (([xmldata].value('(genxml/billaddress/genxml/textbox/street)[1]', 'nvarchar(max)') like '%" + searchText + "%' collate sql_latin1_general_cp1_ci_ai ))";
                filter += " or (([xmldata].value('(genxml/billaddress/genxml/textbox/postalcode)[1]', 'nvarchar(max)') like '%" + searchText + "%' collate sql_latin1_general_cp1_ci_ai ))";
                filter += " or (([xmldata].value('(genxml/billaddress/genxml/textbox/email)[1]', 'nvarchar(max)') like '%" + searchText + "%' collate sql_latin1_general_cp1_ci_ai ))";
                filter += " or (([xmldata].value('(genxml/shipaddress/genxml/textbox/firstname)[1]', 'nvarchar(max)') like '%" + searchText + "%' collate sql_latin1_general_cp1_ci_ai ))";
                filter += " or (([xmldata].value('(genxml/shipaddress/genxml/textbox/lastname)[1]', 'nvarchar(max)') like '%" + searchText + "%' collate sql_latin1_general_cp1_ci_ai ))";
                filter += " or (([xmldata].value('(genxml/shipaddress/genxml/textbox/unit)[1]', 'nvarchar(max)') like '%" + searchText + "%' collate sql_latin1_general_cp1_ci_ai ))";
                filter += " or (([xmldata].value('(genxml/shipaddress/genxml/textbox/street)[1]', 'nvarchar(max)') like '%" + searchText + "%' collate sql_latin1_general_cp1_ci_ai ))";
                filter += " or (([xmldata].value('(genxml/shipaddress/genxml/textbox/postalcode)[1]', 'nvarchar(max)') like '%" + searchText + "%' collate sql_latin1_general_cp1_ci_ai ))";
                filter += " or (([xmldata].value('(genxml/shipaddress/genxml/textbox/email)[1]', 'nvarchar(max)') like '%" + searchText + "%' collate sql_latin1_general_cp1_ci_ai ))";
                filter += " or (([xmldata].value('(genxml/productrefs)[1]', 'nvarchar(max)') like '%" + searchText + "%' collate sql_latin1_general_cp1_ci_ai ))";
                filter += " or (([xmldata].value('(genxml/ordernumber)[1]', 'nvarchar(max)') like '%" + searchText + "%' collate sql_latin1_general_cp1_ci_ai ))  ) ";
            }

            if (Utils.IsNumeric(selecteduserid))
            {
                filter += " and (NB1.UserId = " + selecteduserid + ")   ";
            }

            if (searchdateto != "" && searchdatefrom != "")
            {
                filter += " and  ( ([xmldata].value('(genxml/createddate)[1]', 'datetime') >= convert(datetime,'" + searchdatefrom + "') ) and ([xmldata].value('(genxml/createddate)[1]', 'datetime') <= convert(datetime,'" + searchdateto + "') ) )  ";
            }
            if (searchdateto == "" && searchdatefrom != "")
            {
                filter += " and  ([xmldata].value('(genxml/createddate)[1]', 'datetime') >= convert(datetime,'" + searchdatefrom + "') ) ";
            }
            if (searchdateto != "" && searchdatefrom == "")
            {
                filter += " and ([xmldata].value('(genxml/createddate)[1]', 'datetime') <= convert(datetime,'" + searchdateto + "') ) ";
            }

            if (searchorderstatus != "")
            {
                filter += " and ([xmldata].value('(genxml/dropdownlist/orderstatus)[1]', 'nvarchar(max)') = '" + searchorderstatus + "')   ";
            }

            // check for user or manager.
            if (!NBrightBuyUtils.CheckRights())
            {
                filter += " and ( userid = " + UserController.Instance.GetCurrentUserInfo().UserID + ")   ";
            }

            var recordCount = 0;

            if (themeFolder == "")
            {
                themeFolder = StoreSettings.Current.ThemeFolder;
                if (settings.ContainsKey("themefolder")) themeFolder = settings["themefolder"];
            }

            var objCtrl = new NBrightBuyController();

            if (paging) // get record count for paging
            {
                if (pageNumber == 0) pageNumber = 1;
                if (pageSize == 0) pageSize = 20;

                // get only entity type required
                recordCount = objCtrl.GetListCount(PortalSettings.Current.PortalId, -1, "ORDER", filter);

            }

            var orderby = "   order by [XMLData].value('(genxml/createddate)[1]','datetime') DESC, ModifiedDate DESC  ";
            var list = objCtrl.GetList(portalId, -1, "ORDER", filter, orderby, 0, pageNumber, pageSize, recordCount);

            var passSettings = settings;
            foreach (var s in StoreSettings.Current.Settings()) // copy store setting, otherwise we get a byRef assignement
            {
                if (passSettings.ContainsKey(s.Key))
                    passSettings[s.Key] = s.Value;
                else
                    passSettings.Add(s.Key, s.Value);
            }

            if (csv)
            {
                var securekey = Utils.GetUniqueKey(24);
                strOut = NBrightBuyUtils.RazorTemplRenderList("ordercsv.cshtml", 0, "", list, "/DesktopModules/NBright/NBrightBuy", "config", Utils.GetCurrentCulture(), passSettings);
                Utils.SaveFile(StoreSettings.Current.FolderTempMapPath + "/secure" + securekey, strOut);
                strOut = StoreSettings.Current.FolderTemp + "/secure" + securekey;
            }
            else
            {
                strOut = NBrightBuyUtils.RazorTemplRenderList(razortemplate, 0, "", list, "/DesktopModules/NBright/NBrightBuy", themeFolder, Utils.GetCurrentCulture(), passSettings);
            }

            // add paging if needed
            if (paging && (recordCount > pageSize))
            {
                var pg = new NBrightCore.controls.PagingCtrl();
                strOut += pg.RenderPager(recordCount, pageSize, pageNumber);
            }

            return strOut;
        }


    }
}
