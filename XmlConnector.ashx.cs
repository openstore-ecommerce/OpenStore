using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Text;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Xml;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using Microsoft.ApplicationBlocks.Data;
using Microsoft.SqlServer.Server;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;
using DataProvider = DotNetNuke.Data.DataProvider;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Exceptions;
using NBrightBuy.render;
using NBrightCore.images;
using Nevoweb.DNN.NBrightBuy.Components.Address;
using Nevoweb.DNN.NBrightBuy.Components.Cart;
using Nevoweb.DNN.NBrightBuy.Components.Clients;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;
using Nevoweb.DNN.NBrightBuy.Components.Orders;
using Nevoweb.DNN.NBrightBuy.Components.Payments;
using Nevoweb.DNN.NBrightBuy.Components.Products;
using Nevoweb.DNN.NBrightBuy.Components.Category;
using Nevoweb.DNN.NBrightBuy.Components.ItemLists;
using Nevoweb.DNN.NBrightBuy.Components.Plugins;
using RazorEngine.Compilation.ImpromptuInterface;

namespace Nevoweb.DNN.NBrightBuy
{
    /// <summary>
    /// Summary description for XMLconnector
    /// </summary>
    public class XmlConnector : IHttpHandler
    {
        private readonly JavaScriptSerializer _js = new JavaScriptSerializer();
        private String _editlang = "";

        public void ProcessRequest(HttpContext context)
        {
            #region "Initialize"

            var strOut = "** No Action **";
            var sendResponse = true;

            var paramCmd = Utils.RequestQueryStringParam(context, "cmd");
            var itemId = Utils.RequestQueryStringParam(context, "itemid");
            var ctlType = Utils.RequestQueryStringParam(context, "ctltype");
            var idXref = Utils.RequestQueryStringParam(context, "idxref");
            var xpathpdf = Utils.RequestQueryStringParam(context, "pdf");
            var xpathref = Utils.RequestQueryStringParam(context, "pdfref");
            var lang = Utils.RequestQueryStringParam(context, "lang");
            var language = Utils.RequestQueryStringParam(context, "language");
            var moduleId = Utils.RequestQueryStringParam(context, "mid");
            var moduleKey = Utils.RequestQueryStringParam(context, "mkey");
            var parentid = Utils.RequestQueryStringParam(context, "parentid");
            var entryid = Utils.RequestQueryStringParam(context, "entryid");
            var entryxid = Utils.RequestQueryStringParam(context, "entryxid");
            var catid = Utils.RequestQueryStringParam(context, "catid");
            var catxid = Utils.RequestQueryStringParam(context, "catxid");
            var templatePrefix = Utils.RequestQueryStringParam(context, "tprefix");
            var value = Utils.RequestQueryStringParam(context, "value");
            var itemListName = Utils.RequestQueryStringParam(context, "listname");
            if (itemListName == "") itemListName = "ItemList";
            if (itemListName == "*") itemListName = "ItemList";

            #region "setup language"

            // because we are using a webservice the system current thread culture might not be set correctly,
            NBrightBuyUtils.SetContextLangauge(context);
            var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
            _editlang = NBrightBuyUtils.GetEditLang(ajaxInfo, Utils.GetCurrentCulture());

            #endregion

            Logging.Debug($"XmlConnector called with: paramCmd='{paramCmd}', itemId='{itemId}', itemListName='{itemListName}'");

            #endregion

            try
            {

                #region "Do processing of command"

                if (paramCmd.StartsWith("client."))
                {
                    strOut = ClientFunctions.ProcessCommand(paramCmd, context);
                }
                else if (paramCmd.StartsWith("orderadmin_"))
                {
                    strOut = OrderFunctions.ProcessCommand(paramCmd, context);
                }
                else if (paramCmd.StartsWith("payment_"))
                {
                    strOut = PaymentFunctions.ProcessCommand(paramCmd, context);
                }
                else if (paramCmd.StartsWith("product_"))
                {
                    var productFunctions = new ProductFunctions();
                    strOut = productFunctions.ProcessCommand(paramCmd, context, _editlang);
                }
                else if (paramCmd.StartsWith("category_"))
                {
                    var categoryFunctions = new CategoryFunctions();
                    strOut = categoryFunctions.ProcessCommand(paramCmd, context, _editlang);
                }
                else if (paramCmd.StartsWith("property_"))
                {
                    var propertyFunctions = new PropertyFunctions();
                    strOut = propertyFunctions.ProcessCommand(paramCmd, context, _editlang);
                }                
                else if (paramCmd.StartsWith("itemlist_"))
                {
                    strOut = ItemListsFunctions.ProcessCommand(paramCmd, context);
                }
                else if (paramCmd.StartsWith("addressadmin_"))
                {
                    strOut = AddressAdminFunctions.ProcessCommand(paramCmd, context);
                }
                else if (paramCmd.StartsWith("plugins_"))
                {
                    strOut = PluginFunctions.ProcessCommand(paramCmd, context);
                }
                else if (paramCmd.StartsWith("cart_"))
                {
                    strOut = CartFunctions.ProcessCommand(paramCmd, context);
                }
                else
                {
                    switch (paramCmd)
                    {
                        case "test":
                            strOut = "<root>" + UserController.Instance.GetCurrentUserInfo().Username + "</root>";
                            break;
                        case "setdata":
                            break;
                        case "deldata":
                            break;
                        case "getdata":
                            strOut = GetReturnData(context);
                            break;
                        case "fileupload":
                            if (NBrightBuyUtils.CheckRights())
                            {
                                strOut = FileUpload(context);
                            }
                            break;
                        case "fileclientupload":
                            if (StoreSettings.Current.GetBool("allowupload"))
                            {
                                strOut = FileUpload(context, itemId);
                            }
                            break;
                        case "docdownload":
                            strOut = DownloadSystemFile(paramCmd, context);
                            sendResponse = false;
                            break;
                        case "printproduct":
                            break;
                        case "renderpostdata":
                            strOut = RenderPostData(context);
                            break;
                        case "getsettings":
                            strOut = GetSettings(context);
                            break;
                        case "savesettings":
                            if (NBrightBuyUtils.CheckRights()) strOut = SaveSettings(context);
                            break;
                        case "updateprofile":
                            strOut = UpdateProfile(context);
                            break;
                        case "dosearch":
                            strOut = DoSearch(context);
                            break;
                        case "resetsearch":
                            strOut = ResetSearch(context);
                            break;
                        case "orderby":
                            strOut = DoOrderBy(context);
                            break;
                        case "renderthemefolders":
                            strOut = RenderThemeFolders(context);
                            break;

                    }
                }

                if (strOut == "** No Action **")
                {
                    var ajaxprovider = ajaxInfo.GetXmlProperty("genxml/hidden/ajaxprovider");
                    if (ajaxprovider == "")
                    {
                        ajaxprovider = Utils.RequestQueryStringParam(context, "ajaxprovider");
                    }

                    var pluginData = new PluginData(PortalSettings.Current.PortalId);
                    var provList = pluginData.GetAjaxProviders();
                    if (ajaxprovider != "")
                    {
                        strOut = "Ajax Provider not found: " + ajaxprovider;
                        if (provList.ContainsKey(ajaxprovider))
                        {
                            var ajaxprov = AjaxInterface.Instance(ajaxprovider);
                            if (ajaxprov != null)
                            {
                                strOut = ajaxprov.ProcessCommand(paramCmd, context, _editlang);
                            }
                        }
                    }
                    else
                    {
                        foreach (var d in provList)
                        {
                            if (paramCmd.ToLower().StartsWith(d.Key.ToLower() + "_") || paramCmd.ToLower().StartsWith("cmd" + d.Key.ToLower() + "_") )
                            {
                                var ajaxprov = AjaxInterface.Instance(d.Key);
                                if (ajaxprov != null)
                                {
                                    strOut = ajaxprov.ProcessCommand(paramCmd, context, _editlang);
                                }
                            }
                        }
                    }
                }

                #endregion

            }
            catch (Exception ex)
            {
                strOut = ex.ToString();
                Logging.LogException(ex);
                //Exceptions.LogException(ex);
            }


            #region "return results"

            if (sendResponse)
            {
                //send back xml as plain text
                context.Response.Clear();
                context.Response.ContentType = "text/plain";
                context.Response.Write(strOut);
                context.Response.End();
            }

            #endregion

        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        private static string lockobjectDownloadSystemFile = "lockit";
        private string DownloadSystemFile(string paramCmd, HttpContext context)
        {
            var strOut = "";
            lock (lockobjectDownloadSystemFile)
            {
                var fname = Utils.RequestQueryStringParam(context, "filename");
                var filekey = Utils.RequestQueryStringParam(context, "key");
                if (filekey != "")
                {
                    var uData = new UserData();
                    if (uData.HasPurchasedDocByKey(filekey)) fname = uData.GetPurchasedFileName(filekey);
                    fname = StoreSettings.Current.FolderDocuments + "/" + fname;
                }
                if (fname != "")
                {
                    strOut = fname; // return this is error.
                    var downloadname = Utils.RequestQueryStringParam(context, "downloadname");
                    var fpath = HttpContext.Current.Server.MapPath(fname);
                    if (downloadname == "") downloadname = Path.GetFileName(fname);
                    try
                    {
                        if (fpath.ToLower().Contains("\\secure"))
                        {
                            if (NBrightBuyUtils.CheckManagerRights())
                            {
                                Utils.ForceDocDownload(fpath, downloadname, context.Response);
                            }
                        }
                        else
                        {
                            Utils.ForceDocDownload(fpath, downloadname, context.Response);
                        }
                    }
                    catch (Exception ex)
                    {
                        // ignore, robots can cause error on thread abort.
                        //Exceptions.LogException(ex);
                        Logging.Debug($"XmlConnector.ProcessRequest exception for {paramCmd} which is ignored because bots tend to cause these on thread abort: {ex.Message}.");
                    }
                }
            }
            return strOut;
        }


        #region "fileupload"

        private void UpdateProductImages(HttpContext context)
        {
            //get uploaded params
            var settings = NBrightBuyUtils.GetAjaxDictionary(context);
            if (!settings.ContainsKey("itemid")) settings.Add("itemid", "");
            var productitemid = settings["itemid"];
            var imguploadlist = settings["imguploadlist"];

            if (Utils.IsNumeric(productitemid))
            {
                var imgs = imguploadlist.Split(',');
                foreach (var img in imgs)
                {
                    if (ImgUtils.IsImageFile(Path.GetExtension(img)) && img != "")
                    {
                        string fullName = StoreSettings.Current.FolderTempMapPath + "\\" + img;
                        if (File.Exists(fullName))
                        {
                            var imgResize = StoreSettings.Current.GetInt(StoreSettingKeys.productimageresize);
                            if (imgResize == 0) imgResize = 800;
                            var imagepath = ResizeImage(fullName, imgResize);
                            var imageurl = StoreSettings.Current.FolderImages.TrimEnd('/') + "/" + Path.GetFileName(imagepath);
                            AddNewImage(Convert.ToInt32(productitemid), imageurl, imagepath);                                                    
                        }
                    }
                }
                // clear any cache for the product.
                ProductUtils.RemoveProductDataCache(PortalSettings.Current.PortalId, Convert.ToInt32(productitemid));

                var cachekey = "AjaxProductImgs*" + productitemid;
                Utils.RemoveCache(cachekey);

            }
        }

        private String ResizeImage(String fullName, int imgSize = 640)
        {
            if (ImgUtils.IsImageFile(Path.GetExtension(fullName)))
            {
                var extension = Path.GetExtension(fullName);
                var newImageFileName = StoreSettings.Current.FolderImagesMapPath.TrimEnd(Convert.ToChar("\\")) + "\\" + Utils.GetUniqueKey() + extension;
                if (extension != null && extension.ToLower() == ".png")
                {
                    newImageFileName = ImgUtils.ResizeImageToPng(fullName, newImageFileName, imgSize);
                }
                else
                {
                    newImageFileName = ImgUtils.ResizeImageToJpg(fullName, newImageFileName, imgSize);
                }
                Utils.DeleteSysFile(fullName);

                return newImageFileName;

            }
            return "";
        }


        private void AddNewImage(int itemId,String imageurl, String imagepath)
        {
            var objCtrl = new NBrightBuyController();
            var dataRecord = objCtrl.Get(itemId);
            if (dataRecord != null)
            {
                var strXml = "<genxml><imgs><genxml><hidden><imagepath>" + imagepath + "</imagepath><imageurl>" + imageurl + "</imageurl></hidden></genxml></imgs></genxml>";
                if (dataRecord.XMLDoc.SelectSingleNode("genxml/imgs") == null)
                {
                    dataRecord.AddXmlNode(strXml, "genxml/imgs", "genxml");
                }
                else
                {
                    dataRecord.AddXmlNode(strXml, "genxml/imgs/genxml", "genxml/imgs");
                }
                objCtrl.Update(dataRecord);
            }
        }


        private string FileUpload(HttpContext context, string itemid = "")
        {
            try
            {

                var strOut = "";
                switch (context.Request.HttpMethod)
                {
                    case "HEAD":
                    case "GET":
                        break;
                    case "POST":
                    case "PUT":
                        strOut = UploadFile(context, itemid);
                        break;
                    case "DELETE":
                        break;
                    case "OPTIONS":
                        break;

                    default:
                        context.Response.ClearHeaders();
                        context.Response.StatusCode = 405;
                        break;
                }

                return strOut;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
            
        }

        // Upload file to the server
        private String UploadFile(HttpContext context, string itemid = "")
        {
            var statuses = new List<FilesStatus>();
            var headers = context.Request.Headers;

            if (string.IsNullOrEmpty(headers["X-File-Name"]))
            {
                return UploadWholeFile(context, statuses, itemid);
            }
            else
            {
                return UploadPartialFile(headers["X-File-Name"], context, statuses, itemid);
            }
        }

        // Upload partial file
        private String UploadPartialFile(string fileName, HttpContext context, List<FilesStatus> statuses, string itemid = "")
        {
            Regex fexpr = new Regex(StoreSettings.Current.Get("fileregexpr"));
            if (fexpr.Match(fileName.ToLower()).Success)
            {

                if (itemid != "") itemid += "_";
                if (context.Request.Files.Count != 1) throw new HttpRequestValidationException("Attempt to upload chunked file containing more than one fragment per request");
                var inputStream = context.Request.Files[0].InputStream;
                var fn = DnnUtils.Encrypt(fileName, StoreSettings.Current.Get("adminpin"));
                var fullName = StoreSettings.Current.FolderTempMapPath + "\\" + fn;

                using (var fs = new FileStream(fullName, FileMode.Append, FileAccess.Write))
                {
                    var buffer = new byte[1024];

                    var l = inputStream.Read(buffer, 0, 1024);
                    while (l > 0)
                    {
                        fs.Write(buffer, 0, l);
                        l = inputStream.Read(buffer, 0, 1024);
                    }
                    fs.Flush();
                    fs.Close();
                }
                statuses.Add(new FilesStatus(new FileInfo(fullName)));
            }
            return "";
        }

        // Upload entire file
        private String UploadWholeFile(HttpContext context, List<FilesStatus> statuses, string itemid = "")
        {
            if (itemid != "") itemid += "_";
            for (int i = 0; i < context.Request.Files.Count; i++)
            {
                var file = context.Request.Files[i];
                Regex fexpr = new Regex(StoreSettings.Current.Get("fileregexpr"));
                if (fexpr.Match(file.FileName.ToLower()).Success)
                {
                    var fn = DnnUtils.Encrypt(file.FileName, StoreSettings.Current.Get("adminpin"));
                    foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                    {
                        fn = fn.Replace(c, '_');
                    }
                    file.SaveAs(StoreSettings.Current.FolderTempMapPath + "\\" + fn);
                    statuses.Add(new FilesStatus(Path.GetFileName(fn), file.ContentLength));
                }
            }
            return "";
        }

        private void WriteJsonIframeSafe(HttpContext context, List<FilesStatus> statuses)
        {
            context.Response.AddHeader("Vary", "Accept");
            try
            {
                if (context.Request["HTTP_ACCEPT"].Contains("application/json"))
                    context.Response.ContentType = "application/json";
                else
                    context.Response.ContentType = "text/plain";
            }
            catch
            {
                context.Response.ContentType = "text/plain";
            }

            var jsonObj = _js.Serialize(statuses.ToArray());
            context.Response.Write(jsonObj);
        }



        #endregion

        #region "SQL Data return"

        private string GetReturnData(HttpContext context)
        {
            try
            {

                var strOut = "";

                var strIn = HttpUtility.UrlDecode(Utils.RequestParam(context, "inputxml"));
                var xmlData = GenXmlFunctions.GetGenXmlByAjax(strIn, "");
                var objInfo = new NBrightInfo();

                objInfo.ItemID = -1;
                objInfo.TypeCode = "AJAXDATA";
                objInfo.XMLData = xmlData;
                var settings = objInfo.ToDictionary();

                var themeFolder = StoreSettings.Current.ThemeFolder;
                if (settings.ContainsKey("themefolder")) themeFolder = settings["themefolder"];
                var templCtrl = NBrightBuyUtils.GetTemplateGetter(themeFolder);

                if (!settings.ContainsKey("portalid")) settings.Add("portalid", PortalSettings.Current.PortalId.ToString("")); // aways make sure we have portalid in settings
                var objCtrl = new NBrightBuyController();

                // run SQL and template to return html
                if (settings.ContainsKey("sqltpl") && settings.ContainsKey("xsltpl"))
                {
                    var strSql = templCtrl.GetTemplateData(settings["sqltpl"], Utils.GetCurrentCulture(), true, true, true, StoreSettings.Current.Settings());
                    var xslTemp = templCtrl.GetTemplateData(settings["xsltpl"], Utils.GetCurrentCulture(), true, true, true, StoreSettings.Current.Settings());

                    // replace any settings tokens (This is used to place the form data into the SQL)
                    strSql = Utils.ReplaceSettingTokens(strSql, settings);
                    strSql = Utils.ReplaceUrlTokens(strSql);

                    strSql = GenXmlFunctions.StripSqlCommands(strSql); // don't allow anything to update through here.

                    strOut = objCtrl.GetSqlxml(strSql);
                    if (!strOut.StartsWith("<root>")) strOut = "<root>" + strOut + "</root>"; // always wrap with root node.
                    strOut = XslUtils.XslTransInMemory(strOut, xslTemp);
                }

                return strOut;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }


        #endregion

        #region "Front Office Actions"


        /// <summary>
        /// This token used the ajax posted context data to render the razor template specified in "carttemplate"
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private String RenderPostData(HttpContext context)
        {
            var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
            var carttemplate = ajaxInfo.GetXmlProperty("genxml/hidden/carttemplate");
            var theme = ajaxInfo.GetXmlProperty("genxml/hidden/carttheme");
            var lang = ajaxInfo.GetXmlProperty("genxml/hidden/lang");
            var controlpath = ajaxInfo.GetXmlProperty("genxml/hidden/controlpath");
            if (controlpath == "") controlpath = "/DesktopModules/NBright/NBrightBuy";
            var razorTempl = "";
            if (carttemplate != "")
            {
                if (lang == "") lang = Utils.GetCurrentCulture();
                razorTempl = NBrightBuyUtils.RazorTemplRender(carttemplate, 0, "", ajaxInfo, controlpath, theme, lang, StoreSettings.Current.Settings());
            }
            return razorTempl;
        }

        /// <summary>
        /// This is used to render a new theme dropdownlist when a change to the control path has been made.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private String RenderThemeFolders(HttpContext context)
        {
            var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
            var uilang = ajaxInfo.GetXmlProperty("genxml/hidden/uilang");
            var razorTempl = "";
            if (uilang == "") uilang = Utils.GetCurrentCulture();
            razorTempl = NBrightBuyUtils.RazorTemplRender("ThemeFolderSelect.cshtml", 0, "", ajaxInfo, "/DesktopModules/NBright/NBrightBuy", "config", uilang, StoreSettings.Current.Settings());
            return razorTempl;
        }

        private string UpdateProfile(HttpContext context)
        {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context, true);
                var profileData = new ProfileData();
                profileData.UpdateProfileAjax(ajaxInfo.XMLData, StoreSettings.Current.DebugMode);

                return "OK";
        }

        private string ResetSearch(HttpContext context)
        {
                // take all input and created a SQL select with data and save for processing on search list.
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context, true);
                var navData = new NavigationData(ajaxInfo.PortalId, ajaxInfo.GetXmlProperty("genxml/hidden/modulekey"));
                navData.Delete();

                return "RESET";
        }

        private string DoSearch(HttpContext context)
        {
                // take all input and created a SQL select with data and save for processing on search list.
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context, true);
                var tagList = new List<string>();
                var nodList = ajaxInfo.XMLDoc.SelectNodes("genxml/hidden/*");
                foreach (XmlNode nod in nodList)
                {
                    tagList.Add(nod.InnerText);
                }
                var navData = new NavigationData(ajaxInfo.PortalId, ajaxInfo.GetXmlProperty("genxml/hidden/modulekey"));
                navData.Build(ajaxInfo.XMLData,tagList);
                navData.Mode = ajaxInfo.GetXmlProperty("genxml/hidden/navigationmode").ToLower();
                navData.CategoryId = ajaxInfo.GetXmlPropertyInt("genxml/hidden/categoryid");
                if (ajaxInfo.GetXmlProperty("genxml/hidden/pagenumber") != "") navData.PageNumber = ajaxInfo.GetXmlProperty("genxml/hidden/pagenumber");
                if (ajaxInfo.GetXmlProperty("genxml/hidden/pagesize") != "") navData.PageSize = ajaxInfo.GetXmlProperty("genxml/hidden/pagesize");
                if (ajaxInfo.GetXmlProperty("genxml/hidden/pagename") != "") navData.PageName = ajaxInfo.GetXmlProperty("genxml/hidden/pagename");
                if (ajaxInfo.GetXmlProperty("genxml/hidden/pagemoduleid") != "") navData.PageModuleId  = ajaxInfo.GetXmlProperty("genxml/hidden/pagemoduleid");
                navData.SearchFormData = ajaxInfo.XMLData;
                navData.Save();

                return "OK";
        }

        private string DoOrderBy(HttpContext context)
        {
                // take all input and created a SQL select with data and save for processing on search list.
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context, true);
                var navData = new NavigationData(ajaxInfo.PortalId, ajaxInfo.GetXmlProperty("genxml/hidden/modulekey"));
                navData.OrderByIdx = ajaxInfo.GetXmlProperty("genxml/hidden/orderbyidx");
                navData.OrderBy = " order by " + ajaxInfo.GetXmlProperty("genxml/hidden/orderby" + navData.OrderByIdx);
                navData.Save();

                return "OK";
        }


        #endregion

        #region "Settings"

        private String GetSettings(HttpContext context, bool clearCache = false)
        {
            try
            {
                var strOut = "";
                //get uploaded params
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);

                var moduleid = ajaxInfo.GetXmlProperty("genxml/hidden/moduleid");
                var razortemplate = ajaxInfo.GetXmlProperty("genxml/hidden/razortemplate");
                var themefolder = ajaxInfo.GetXmlProperty("genxml/dropdownlist/themefolder");
                var controlpath = ajaxInfo.GetXmlProperty("genxml/hidden/controlpath");
                if (controlpath == "") controlpath = "/DesktopModules/NBright/NBrightBuy";
                if (razortemplate == "") return ""; // assume no settings requirted
                if (moduleid == "") moduleid = "-1";

                // do edit field data if a itemid has been selected
                var obj = NBrightBuyUtils.GetSettings(PortalSettings.Current.PortalId, Convert.ToInt32(moduleid));
                obj.ModuleId = Convert.ToInt32(moduleid); // assign for new records
                strOut = NBrightBuyUtils.RazorTemplRender(razortemplate, obj.ModuleId, "settings", obj, controlpath, themefolder, Utils.GetCurrentCulture(), null);

                return strOut;

            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                return ex.ToString();
            }

        }

        private String SaveSettings(HttpContext context)
        {
            try
            {
                var objCtrl = new NBrightBuyController();

                //get uploaded params
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);

                var moduleid = ajaxInfo.GetXmlProperty("genxml/hidden/moduleid");
                if (Utils.IsNumeric(moduleid))
                {
                    // get DB record
                    var nbi = NBrightBuyUtils.GetSettings(PortalSettings.Current.PortalId, Convert.ToInt32(moduleid)); 
                    if (nbi.ModuleId == 0) // new setting record
                    {
                        nbi = CreateSettingsInfo(moduleid, nbi);
                    }
                    // get data passed back by ajax
                    var strIn = HttpUtility.UrlDecode(Utils.RequestParam(context, "inputxml"));
                    // update record with ajax data
                    nbi.UpdateAjax(strIn);


                    if (nbi.GetXmlProperty("genxml/hidden/modref") == "") nbi.SetXmlProperty("genxml/hidden/modref", Utils.GetUniqueKey(10));
                    if (nbi.TextData == "") nbi.TextData = "NBrightBuy";
                    objCtrl.Update(nbi);
                    NBrightBuyUtils.RemoveModCachePortalWide(PortalSettings.Current.PortalId);  // make sure all new settings are used.
                }
                return "";

            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                return ex.ToString();
            }

        }

        private NBrightInfo CreateSettingsInfo(String moduleid, NBrightInfo nbi)
        {

            var objCtrl = new NBrightBuyController();
            nbi = objCtrl.GetByType(PortalSettings.Current.PortalId, Convert.ToInt32(moduleid), "SETTINGS");
            if (nbi == null)
            {
                nbi = new NBrightInfo(true); // populate empty XML so we can update nodes.
                nbi.GUIDKey = "";
                nbi.PortalId = PortalSettings.Current.PortalId;
                nbi.ModuleId = Convert.ToInt32(moduleid);
                nbi.TypeCode = "SETTINGS";
                nbi.Lang = "";
            }
            //rebuild xml
            nbi.ModuleId = Convert.ToInt32(moduleid);
            nbi.GUIDKey = Utils.GetUniqueKey(10);
            nbi.SetXmlProperty("genxml/hidden/modref", nbi.GUIDKey);
            return nbi;
        }


        #endregion

    }
}