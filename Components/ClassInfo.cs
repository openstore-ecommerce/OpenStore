using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using System.Xml;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;

namespace Nevoweb.DNN.NBrightBuy.Components
{
    /// <summary>
    /// Class to hold transient stock data
    /// </summary>
    public class ModelTransData
    {
        public String modelid { get; set; }
        public int orderid { get; set; }
        public Double qty { get; set; }
        public DateTime setdate { get; set; }
    }

    /// <summary>
    /// Class to hold Category data, so we can use linq and help speed up access from the memory var CategoryList
    /// </summary>
    public class GroupCategoryData
    {
        public int categoryid { get; set; }
        public string categoryref { get; set; }
        public string categoryrefGUIDKey { get; set; }        
        public string grouptyperef { get; set; }
        public string attributecode { get; set; }
        public string groupname { get; set; }
        public bool archived { get; set; }
        public bool ishidden { get; set; }
        public int parentcatid { get; set; }
        public Double recordsortorder { get; set; }
        public string imageurl { get; set; }
        public string categoryname { get; set; }
        public string categorydesc { get; set; }
        public string seoname { get; set; }
        public string metadescription { get; set; }
        public string metakeywords { get; set; }
        public string seopagetitle { get; set; }
        public string breadcrumb { get; set; }
        public int depth { get; set; }
        public bool disabled { get; set; }
        public int entrycount { get; set; }
        public string url { get; set; }
        public string message { get; set; }
        public string propertyref { get; set; }
        public bool isdefault { get; set; }
        public bool isvisible
        {
            get
            {
                if (archived) return false;
                if (ishidden) return false;
                return true;
            }
        }

        public List<int> Parents { get; set; }

        public GroupCategoryData()
        {
            Parents = new List<int>();
            isdefault = false;
        }

    }

   
    public enum ModuleEventCodes { none, displaycategoryheader, displaycategorybody, displaycategoryfooter, displayentryheader, displayentrybody, displayentryfooter, displayheader, displaybody, displayfooter, selectsearch, selectheader, selectbody, selectfooter, selectedheader, selectedbody, selectedfooter, editheader, editbody, editlang, editfooter, editlistsearch, editlistheader, editlistbody, editlistfooter, email, emailsubject, emailclient, emailreturnmsg, jsinsert, exportxsl };

    public enum DataStorageType { Cookie,SessionMemory,Database };

    public enum NotifyCode { ok,fail,warning,error,log};

    public enum EventActions { ValidateCartBefore, ValidateCartAfter, ValidateCartItemBefore, ValidateCartItemAfter, AfterCartSave, AfterCategorySave, AfterProductSave, AfterSavePurchaseData, BeforeOrderStatusChange, AfterOrderStatusChange, BeforePaymentOK, AfterPaymentOK, BeforePaymentFail, AfterPaymentFail, BeforeSendEmail, AfterSendEmail };

    public enum OrderStatus
    {
        /// 010       ,020             ,030      ,040       ,050                 ,060                ,070              ,120               ,080    ,090    ,100      ,110
        /// Incomplete,Waiting for Bank,Cancelled,Payment OK,Payment Not Verified,Waiting for Payment,Waiting for Stock,Being Manufactured,Waiting,Shipped,Completed,Archived
        Incomplete = 010,
        WaitingForBank = 020,
        Cancelled = 030,
        PaymentOk = 040,
        PaymentNotVerified = 050,
        WaitingForPayment = 060,
        WaitingForStock = 070,
        Waiting = 080,
        Shipped = 090,
        Completed = 100,
        Archived = 110,
        BeingManufactured = 120,
    }


    public static class StoreSettingKeys
    {
        public static string countrycodelist = "countrycodelist";
        public static string currencysymbol = "currencysymbol";
        public static string currencyculturecode = "currencyculturecode";
        public static string merchantculturecode = "merchantculturecode";
        public static string bonoimageicon = "bonoimageicon";
        public static string alloweditorder = "alloweditorder";
        public static string shareproducts = "shareproducts";
        public static string sharecategories = "sharecategories";
        public static string chkgroupresults = "chkgroupresults";
        public static string productimageresize = "productimageresize";
        public static string cartsteps = "cartsteps";
        public static string carttitle = "carttitle";
        public static string checkouttaxcode = "checkouttaxcode";
        public static string checkoutpromocode = "checkoutpromocode";
        public static string subscribenewsletter = "subscribenewsletter";
        public static string copyorderto = "copyorderto";
        public static string jqueryuijs = "jqueryuijs";
        public static string jqueryuicss = "jqueryuicss";
        public static string fontawesome = "fontawesome";
        public static string orderprefix = "orderprefix";
        public static string enabledealer = "enabledealer";
        public static string adminpin = "adminpin";
        public static string storagetypeclient = "storagetypeclient";
        public static string debugfileout = "debugfileout";
        public static string debug = "debug";
        public static string devoptions = "devoptions";
        public static string pageactions = "pageactions";
        public static string cataloguemode = "cataloguemode";
        public static string emailthemefolder = "emailthemefolder";
        public static string themefolder = "themefolder";
        public static string minimumstocklevel = "minimumstocklevel";
        public static string enablemyaddress = "enablemyaddress";
        public static string addressestab = "addressestab";
        public static string enablemyprofile = "enablemyprofile";
        public static string profiletab = "profiletab";
        public static string enablemyorders = "enablemyorders";
        public static string ordermanagertab = "ordermanagertab";
        public static string exittab = "exittab";
        public static string paymenttab = "paymenttab";
        public static string carttab = "carttab";
        public static string checkouttab = "checkouttab";
        public static string ddldetailtabid = "ddldetailtabid";
        public static string productlisttab = "productlisttab";
        public static string pagesize = "pagesize";
        public static string folderuploads = "folderuploads";
        public static string folderdocs = "folderdocs";
        public static string folderimages = "folderimages";
        public static string checkoutfailmessage = "checkoutfailmessage";
        public static string checkoutsuccessmessage = "checkoutsuccessmessage";
        public static string pickupmessage = "pickupmessage";
        public static string sharingwidget = "sharingwidget";
        public static string facebookpage = "facebookpage";
        public static string twitterpage = "twitterpage";
        public static string emailmessage = "emailmessage";
        public static string emaillogo = "emaillogo";
        public static string storename = "storename";
        public static string bankinginstructions = "bankinginstructions";
        public static string bankaccountnumber = "bankaccountnumber";
        public static string bankaccountname = "bankaccountname";
        public static string bankname = "bankname";
        public static string storetaxnumber = "storetaxnumber";
        public static string storephone = "storephone";
        public static string storecountry = "storecountry";
        public static string emaillogourl = "emaillogourl";
        public static string storepostcode = "storepostcode";
        public static string storeregion = "storeregion";
        public static string storecity = "storecity";
        public static string storeaddressline2 = "storeaddressline2";
        public static string storeaddressline1 = "storeaddressline1";
        public static string storepostbox = "storepostbox";
        public static string storecompany = "storecompany";
        public static string storeattention = "storeattention";
        public static string salesemail = "salesemail";
        public static string supportemail = "supportemail";
        public static string manageremail = "manageremail";
        public static string adminemail = "adminemail";
        public static string imagesizes = "imagesizes";
    }

    public class FilesStatus
    {
        public const string HandlerPath = "/";

        public string group { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public int size { get; set; }
        public string progress { get; set; }
        public string url { get; set; }
        public string thumbnail_url { get; set; }
        public string delete_url { get; set; }
        public string delete_type { get; set; }
        public string error { get; set; }

        public FilesStatus() { }

        public FilesStatus(System.IO.FileInfo fileInfo) { SetValues(fileInfo.Name, (int)fileInfo.Length); }

        public FilesStatus(string fileName, int fileLength) { SetValues(fileName, fileLength); }

        private void SetValues(string fileName, int fileLength)
        {
            name = fileName;
            type = "image/png";
            size = fileLength;
            progress = "1.0";
            url = HandlerPath + "FileTransferHandler.ashx?f=" + fileName;
            thumbnail_url = HandlerPath + "Thumbnail.ashx?f=" + fileName;
            delete_url = HandlerPath + "FileTransferHandler.ashx?f=" + fileName;
            delete_type = "DELETE";
        }
    }

}