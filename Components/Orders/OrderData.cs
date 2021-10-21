using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;
using Nevoweb.DNN.NBrightBuy.Components.Orders;

namespace Nevoweb.DNN.NBrightBuy.Components
{
    public class OrderData : PurchaseData
    {

        public string payselectionXml { get; set; }

        public OrderData(int entryid)
        {
            PurchaseTypeCode = "ORDER";
            PopulatePurchaseData(entryid);
            PortalId = PurchaseInfo.PortalId;
        }

        /// <summary>
        /// Save order and turn off edit mode.
        /// </summary>
        /// <returns></returns>
        public int Save()
        {
            base.EditMode = "E"; // set edit mode so user id is not 
            var i = SavePurchaseData();
            TurnOffEditMode();
            return i;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalId">Left to ensure backward compatiblity</param>
        /// <param name="entryid"></param>
        public OrderData(int portalId, int entryid)
        {
            PurchaseTypeCode = "ORDER";
            PortalId = portalId;
            PopulatePurchaseData(entryid);
        }

        public void ConvertToCart(Boolean debugMode = false, String storageType = "Cookie", string nameAppendix = "")
        {
            // only magers and editors allowed to edit orders
            if (UserController.Instance.GetCurrentUserInfo().IsInRole(StoreSettings.ManagerRole) ||
                UserController.Instance.GetCurrentUserInfo().IsInRole(StoreSettings.EditorRole) ||
                UserController.Instance.GetCurrentUserInfo().IsInRole("Administrators")) 
            {
                AddAuditMessage("EDIT ORDER","sys",UserController.Instance.GetCurrentUserInfo().Username,"False");
                PurchaseTypeCode = "CART";
                EditMode = "E";
                var cartId = base.SavePurchaseData();
                var cartData = new CartData(PortalId, "", cartId.ToString("")); //create the client record (cookie)
                cartData.PurchaseInfo.SetXmlProperty("genxml/currentcartstage", "cartlist"); // make sure we start edit at cart stage.               
                cartData.Save();
                if (StoreSettings.Current.DebugModeFileOut) OutputDebugFile("debug_convertedorder.xml");
            }
        }


        /// <summary>
        /// Order status
        /// 010       ,020             ,030      ,040       ,050                 ,060                ,070              ,120               ,080    ,090    ,100      ,110
        /// Incomplete,Waiting for Bank,Cancelled,Payment OK,Payment Not Verified,Waiting for Payment,Waiting for Stock,Being Manufactured,Waiting,Shipped,Completed,Archived
        /// </summary>
        public String OrderStatus { 
            get
            {
                return PurchaseInfo.GetXmlProperty("genxml/dropdownlist/orderstatus");
            } 
            set
            {
                NBrightBuyUtils.ProcessEventProvider(EventActions.BeforeOrderStatusChange, PurchaseInfo);

                if (PurchaseInfo.GUIDKey != value) AddAuditStatusChange(value, UserController.Instance.GetCurrentUserInfo().Username);
                PurchaseInfo.SetXmlProperty("genxml/dropdownlist/orderstatus", value);
                PurchaseInfo.GUIDKey = value;

                SavePurchaseData();

                NBrightBuyUtils.ProcessEventProvider(EventActions.AfterOrderStatusChange, PurchaseInfo);

            }  
        }

        public String ShippedDate
        {
            get
            {
                return PurchaseInfo.GetXmlProperty("genxml/textbox/shippingdate");
            }
            set
            {
                PurchaseInfo.SetXmlProperty("genxml/textbox/shippingdate", value, TypeCode.DateTime);
            }
        }
        public String OrderPlacedDate
        {
            get
            {
                return PurchaseInfo.GetXmlProperty("genxml/textbox/orderplaceddate");
            }
            set
            {
                PurchaseInfo.SetXmlProperty("genxml/textbox/orderplaceddate", value, TypeCode.DateTime);
            }
        }
        public String TrackingCode
        {
            get
            {
                return PurchaseInfo.GetXmlProperty("genxml/textbox/trackingcode");
            }
            set
            {
                PurchaseInfo.SetXmlProperty("genxml/textbox/trackingcode", value);
            }
        }

        public String InvoiceFilePath
        {
            get
            {
                return PurchaseInfo.GetXmlProperty("genxml/hidden/invoicefilepath");
            }
            set
            {
                PurchaseInfo.SetXmlProperty("genxml/hidden/invoicefilepath", value);
            }
        }
        public String InvoiceFileName
        {
            get
            {
                return PurchaseInfo.GetXmlProperty("genxml/hidden/invoicefilename");
            }
            set
            {
                PurchaseInfo.SetXmlProperty("genxml/hidden/invoicefilename", value);
            }
        }
        public String InvoiceFileExt
        {
            get
            {
                return PurchaseInfo.GetXmlProperty("genxml/hidden/invoicefileext");
            }
            set
            {
                PurchaseInfo.SetXmlProperty("genxml/hidden/invoicefileext", value);
            }
        }
        public String InvoiceDownloadName
        {
            get
            {
                return PurchaseInfo.GetXmlProperty("genxml/hidden/invoicedownloadname");
            }
            set
            {
                PurchaseInfo.SetXmlProperty("genxml/hidden/invoicedownloadname", value);
            }
        }

        public String OrderNumber
        {
            get
            {
                return PurchaseInfo.GetXmlProperty("genxml/ordernumber");
            }
            set
            {
                PurchaseInfo.SetXmlProperty("genxml/ordernumber", value);
            }
        }

        public String CreatedDate
        {
            get
            {
                return PurchaseInfo.GetXmlProperty("genxml/createddate");
            }
            set
            {
                PurchaseInfo.SetXmlProperty("genxml/createddate", value,TypeCode.DateTime);
            }
        }

        /// <summary>
        /// Save the internal key for to identify whcih payment provider is processing the order
        /// </summary>
        public String PaymentProviderKey
        {
            get
            {
                return PurchaseInfo.GetXmlProperty("genxml/paymentproviderkey");
            }
            set
            {
                PurchaseInfo.SetXmlProperty("genxml/paymentproviderkey", value);
            }
        }

        /// <summary>
        /// A payment passkey can be link to the order for security
        /// </summary>
        public String PaymentPassKey
        {
            get
            {
                return PurchaseInfo.GetXmlProperty("genxml/paymentpasskey");
            }
            set
            {
                PurchaseInfo.SetXmlProperty("genxml/paymentpasskey", value);
            }
        }

        public void PaymentOk(String orderStatus = "040", Boolean sendEmails = true)
        {
            PaymentOk(orderStatus, sendEmails, false);
        }

        public void PaymentOk(String orderStatus, Boolean sendEmails, Boolean forceStatusChange)
        {
            NBrightBuyUtils.ProcessEventProvider(EventActions.BeforePaymentOK, PurchaseInfo);

            if (!PurchaseInfo.GetXmlPropertyBool("genxml/stopprocess"))
            {

                // only process this on waiting for bank, incomplete or cancelled.  Cancel might be sent back from bank if client fails on first payment try.
                if (IsNotPaid() || forceStatusChange)
                {
                    var discountprov = DiscountCodeInterface.Instance();
                    if (discountprov != null)
                    {
                        PurchaseInfo = discountprov.UpdatePercentUsage(PortalId, UserId, PurchaseInfo);
                        PurchaseInfo = discountprov.UpdateVoucherAmount(PortalId, UserId, PurchaseInfo);
                    }

                    PurchaseTypeCode = "ORDER";
                    CreatedDate = DateTime.Now.ToString("O");
                    ApplyModelTransQty();
                    OrderStatus = orderStatus;
                    SavePurchaseData();

                    // Send emails
                    if (sendEmails)
                    {
                        NBrightBuyUtils.SendOrderEmail("OrderCreatedClient", PurchaseInfo.ItemID, "ordercreatedemailsubject");
                    }

                }
            }
            NBrightBuyUtils.ProcessEventProvider(EventActions.AfterPaymentOK, PurchaseInfo);
        }

        public void PaymentFail(String orderStatus = "030")
        {
            NBrightBuyUtils.ProcessEventProvider(EventActions.BeforePaymentFail, PurchaseInfo);

            if (!PurchaseInfo.GetXmlPropertyBool("genxml/stopprocess"))
            {
                // only move back to cart, if we've not processed payment already.
                if (IsNotPaid())
                {

                    // If the client returns to the website before payment is accepted, 
                    // then we may get a CART but no ORDER. So once it has moved to ORDER we cannot return to CART.
                    // In theory this should never happen, but in reality, this has happened.
                    //PurchaseTypeCode = "CART";                  

                    // make a new cart and copy the order data to it.
                    OrderFunctions.CopyToCart(this);

                    PurchaseTypeCode = "ORDER"; // Make sure this order is moved to an order.
                    CreatedDate = DateTime.Now.ToString("O");
                    ReleaseModelTransQty();
                    if (orderStatus != "030")
                    {
                        OrderStatus = orderStatus;
                        AddAuditStatusChange(orderStatus, UserController.Instance.GetCurrentUserInfo().Username);
                    }
                    AddAuditStatusChange("030", UserController.Instance.GetCurrentUserInfo().Username);
                    OrderStatus = "030";
                    SavePurchaseData();

                }
            }

            NBrightBuyUtils.ProcessEventProvider(EventActions.AfterPaymentFail, PurchaseInfo);
        }

    }
}
