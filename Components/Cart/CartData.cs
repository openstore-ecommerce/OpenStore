using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
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

namespace Nevoweb.DNN.NBrightBuy.Components
{
    public class CartData: PurchaseData
    {
        private int _cartId;
        private string _cookieName;
        private HttpCookie _cookie;

        public Boolean IsCartEmpty()
        {
            var l = GetCartItemList();
            return !l.Any();
        }

        public CartData(int portalId, string nameAppendix = "",String cartid = "")
        {
            _cookieName = "NBrightBuyCart" + "*" + portalId.ToString("") + "*" + nameAppendix;
            Exists = false;
            PortalId = portalId;
            _cartId = GetCartId(cartid);
        }


        /// <summary>
        /// Save cart
        /// </summary>
        public void Save(Boolean debugMode = false)
        {
            Save(debugMode,false);
        }

        /// <summary>
        /// Save cart
        /// </summary>
        /// <param name="debugMode"></param>
        /// <param name="removeZeroQtyItems">Sometimes with Stock activated we don't want to remove zero items from basket until final process on checkout</param>
        public void Save(Boolean debugMode, Boolean removeZeroQtyItems)
        {
            //save cart so any added items are included
            _cartId = base.SavePurchaseData();
            ValidateCart(removeZeroQtyItems);
            if (StoreSettings.Current.DebugModeFileOut) OutputDebugFile("debug_currentcart.xml");
            SaveCartId();
            Exists = true;

            NBrightBuyUtils.ProcessEventProvider(EventActions.AfterCartSave, PurchaseInfo);

        }

        public OrderData ConvertToOrder(Boolean debugMode = false)
        {
            var itemList = GetCartItemList();
            if (IsValidated() && itemList.Count > 0)
            {
                PurchaseTypeCode = "ORDER";
                if (base.PurchaseInfo.GetXmlProperty("genxml/createddate") == "") base.PurchaseInfo.SetXmlProperty("genxml/createddate", DateTime.Now.ToString("O"), TypeCode.DateTime);
                if (base.PurchaseInfo.GetXmlProperty("genxml/ordernumber") == "") base.PurchaseInfo.SetXmlProperty("genxml/ordernumber", StoreSettings.Current.Get("orderprefix") + DateTime.Today.Year.ToString("").Substring(2, 2) + DateTime.Today.Month.ToString("00") + DateTime.Today.Day.ToString("00") + _cartId);

                Save();
                var orderPortalId = PortalId;
                if (StoreSettings.Current.GetBool("shareorders")) orderPortalId = -1;
                var ordData = new OrderData(orderPortalId, base.PurchaseInfo.ItemID);
                if (orderPortalId == -1)
                {
                    // shared order, so save the orginal portal
                    ordData.PurchaseInfo.SetXmlProperty("genxml/createdportalid", PortalId.ToString(""));
                }

                ordData.OrderStatus = "010";
                if (ordData.EditMode == "") // don't update if we are in edit mode, we dont; want manager email to be altered.
                {
                    // if the client has updated the email address, link this back to DNN profile. (We assume they alway place there current email address on th order.)
                    //var objUser = UserController.GetUserById(PortalSettings.Current.PortalId, ordData.UserId);
                    //if (objUser != null && objUser.Email != ordData.EmailAddress)
                    //{
                    //    var clientData = new ClientData(PortalId, ordData.UserId);
                    //    clientData.UpdateEmail(ordData.EmailAddress);
                    //}
                    // ++++++ Assumption Wrong!!! never assume!!  ++++++++++
                    // This also leads to problem is using the email form the username, 
                    //   people don;t understand why their login isn't working when they haven't specificlly change the email address on the site!!


                    var addrData = new AddressData(ordData.UserId.ToString());
                    var billAddr = ordData.GetBillingAddress();
                    var selectedbilladdrIdx = billAddr.GetXmlProperty("genxml/dropdownlist/selectaddress");
                    if (!Utils.IsNumeric(selectedbilladdrIdx)) selectedbilladdrIdx = "-1";
                    addrData.AddAddress(billAddr, Convert.ToInt32(selectedbilladdrIdx));
                    var shipAddr = ordData.GetShippingAddress();
                    var selectedShipaddrIdx = shipAddr.GetXmlProperty("genxml/dropdownlist/selectshipaddress");
                    if (!Utils.IsNumeric(selectedShipaddrIdx)) selectedShipaddrIdx = "-1";
                    addrData.AddAddress(shipAddr, Convert.ToInt32(selectedShipaddrIdx));
                }
                ordData.Save();

                if (StoreSettings.Current.DebugModeFileOut) OutputDebugFile("debug_convertedcart.xml");
                Exists = false;
                return ordData;
            }
            return null;
        }



        /// <summary>
        /// Set to true if cart exists
        /// </summary>
        public bool Exists { get; private set; }

        #region "private methods/functions"

        /// <summary>
        /// Get CartID from cookie or session
        /// </summary>
        /// <returns></returns>
        private int GetCartId(String cartId = "")
        {
            if (!Utils.IsNumeric(cartId))
            {
                NBrightInfo nbi = null;
                var userid = UserController.Instance.GetCurrentUserInfo().UserID;
                if (userid > 0)
                {
                    // get any cart linked to a user.
                    var modCtrl = new NBrightBuyController();
                    nbi = modCtrl.GetByGuidKey(PortalSettings.Current.PortalId, -1, "CART", userid.ToString(""), userid.ToString(""),true);
                    if (nbi != null) cartId = nbi.ItemID.ToString("");
                }
                if (nbi == null)
                {
                    _cookie = HttpContext.Current.Request.Cookies[_cookieName];
                    if (_cookie == null)
                    {
                        _cookie = new HttpCookie(_cookieName);
                        _cookie.SameSite = SameSiteMode.Lax;
                    }
                    else
                    {
                        if (_cookie["cartId"] != null) cartId = _cookie["cartId"];
                    }
                }
                if (!Utils.IsNumeric(cartId)) cartId = "-1";
            }
            else
            {
                _cartId = Convert.ToInt32(cartId);
            }


            //populate cart data
            var rtnid = PopulatePurchaseData(Convert.ToInt32(cartId));
            if (PurchaseTypeCode == "CART") return rtnid;

            // this class has only rights to edit CART items, so reset cartid to new cart Item.           
            PurchaseTypeCode = "CART";
            var rtnId = PopulatePurchaseData(-1);
            SaveCartId(); //created from order, so save cartid for client.
            return rtnId;
        }

        private void SaveCartId()
        {
            //save cartid for client
            _cookie = HttpContext.Current.Request.Cookies[_cookieName];
            if (_cookie == null) _cookie = new HttpCookie(_cookieName);
            _cookie.SameSite = SameSiteMode.Lax;
            _cookie["cartId"] = _cartId.ToString("");
            _cookie.Expires = DateTime.Now.AddYears(1);
            HttpContext.Current.Response.Cookies.Add(_cookie);
        }

        #endregion

        #region "Stock control"

        public int GetQtyOfModelInCart(String modelid)
        {
            var itemList = GetCartItemList();
            return itemList.Where(c => c.GetXmlProperty("genxml/modelid") == modelid).Sum(c => c.GetXmlPropertyInt("genxml/qty"));
        }

        #endregion

        #region "cart validation and calculation"

        public void ValidateCart(Boolean removeZeroQtyItems = false)
        {
            PurchaseInfo = NBrightBuyUtils.ProcessEventProvider(EventActions.ValidateCartBefore, PurchaseInfo);

            var itemList = GetCartItemList();
            Double subtotalcost = 0;
            Double totaldealerbonus = 0;
            Double totaldiscount = 0;
            Double totalsalediscount = 0;
            Double totalqty = 0;
            Double totalweight = 0;
            Double totalunitcost = 0;

            // Calculate Promotions
            // Calculate first, so if we add items the price is calculated
            if (PromoInterface.Instance() != null)
            {
                var promol = PromoInterface.ProviderList;
                foreach (var p in promol)
                {
                    PurchaseInfo = PromoInterface.Instance(p.Key).CalculatePromotion(PortalId, PurchaseInfo);
                    itemList = GetCartItemList(); // get any new items
                }
            }

            var strXml = "<items>";
            foreach (var info in itemList)
            {
                // check product still exists and remove if deleted, altered or disabled.

                var cartItem = ValidateCartItem(PortalId, UserId, info, removeZeroQtyItems);
                if (cartItem != null)
                {
                    strXml += cartItem.XMLData;
                    totalunitcost += info.GetXmlPropertyDouble("genxml/unitcost");
                    subtotalcost += info.GetXmlPropertyDouble("genxml/totalcost");
                    totaldealerbonus += info.GetXmlPropertyDouble("genxml/totaldealerbonus");
                    totaldiscount += info.GetXmlPropertyDouble("genxml/totaldiscount");
                    totalsalediscount += info.GetXmlPropertyDouble("genxml/salediscount");
                    totalqty += info.GetXmlPropertyDouble("genxml/qty");
                    totalweight += info.GetXmlPropertyDouble("genxml/totalweight"); 
                }
            }
            strXml += "</items>";
            PurchaseInfo.RemoveXmlNode("genxml/items");
            PurchaseInfo.AddXmlNode(strXml, "items", "genxml");
            PopulateItemList(); // put changed items and prices back into base class for saving to DB

            // calculate totals

            var promototaldiscount = (totaldiscount - totalsalediscount);

            PurchaseInfo.SetXmlPropertyDouble("genxml/totalqty", totalqty);
            PurchaseInfo.SetXmlPropertyDouble("genxml/totalweight", totalweight);
            PurchaseInfo.SetXmlPropertyDouble("genxml/totalunitcost", totalunitcost);
            PurchaseInfo.SetXmlPropertyDouble("genxml/subtotalcost", subtotalcost);
            PurchaseInfo.SetXmlPropertyDouble("genxml/subtotal", subtotalcost);
            PurchaseInfo.SetXmlPropertyDouble("genxml/appliedsubtotal", (subtotalcost + totalsalediscount));


            // calc any voucher amounts
            var discountcode = PurchaseInfo.GetXmlProperty("genxml/extrainfo/genxml/textbox/promocode");
            Double voucherDiscount = 0;
            if (DiscountCodeInterface.Instance() != null)
            {
                PurchaseInfo = DiscountCodeInterface.Instance().CalculateVoucherAmount(PortalId, UserId, PurchaseInfo, discountcode);
                voucherDiscount = PurchaseInfo.GetXmlPropertyDouble("genxml/voucherdiscount");
            }
            promototaldiscount += voucherDiscount;
            totaldiscount += voucherDiscount;

            PurchaseInfo.SetXmlPropertyDouble("genxml/applieddiscount", totaldiscount);

            PurchaseInfo.SetXmlPropertyDouble("genxml/totaldealerbonus", totaldealerbonus);

            PurchaseInfo.SetXmlPropertyDouble("genxml/totaldiscount", totaldiscount);
            PurchaseInfo.SetXmlPropertyDouble("genxml/totalsalediscount", totalsalediscount);


            //add shipping
            Double shippingcost = 0;
            Double shippingdealercost = 0;
            var shippingkey = PurchaseInfo.GetXmlProperty("genxml/extrainfo/genxml/radiobuttonlist/shippingprovider");
            var currentcartstage = PurchaseInfo.GetXmlProperty("genxml/currentcartstage");
            if (currentcartstage == "cartaddress" || currentcartstage == "cartsummary") // can only calc shipping on this stage.
            {
                ShippingInterface shipprov = null;
                 shipprov = ShippingInterface.Instance(shippingkey);
                if (shipprov != null)
                {
                    PurchaseInfo = shipprov.CalculateShipping(PurchaseInfo);
                    shippingcost = PurchaseInfo.GetXmlPropertyDouble("genxml/shippingcost");
                    shippingdealercost = PurchaseInfo.GetXmlPropertyDouble("genxml/shippingdealercost");
                }
                PurchaseInfo.SetXmlPropertyDouble("genxml/appliedshipping", AppliedCost(shippingcost, shippingdealercost));
            }
            else
            {
                // clear the provider if not cartshipping stage
                PurchaseInfo.SetXmlProperty("genxml/extrainfo/genxml/radiobuttonlist/shippingprovider", "");
            }


            //add tax
            Double appliedtax = 0;
            var taxproviderkey = PurchaseInfo.GetXmlProperty("genxml/extrainfo/genxml/hidden/taxproviderkey");
            var taxprov = TaxInterface.Instance(taxproviderkey);
            if (taxprov != null)
            {
                PurchaseInfo = taxprov.Calculate(PurchaseInfo);
                appliedtax = PurchaseInfo.GetXmlPropertyDouble("genxml/appliedtax");
            }

            //cart full total
            var total = (subtotalcost + shippingcost + appliedtax) - promototaldiscount;
            if (total < 0) total = 0;
            PurchaseInfo.SetXmlPropertyDouble("genxml/total", total);
            PurchaseInfo.SetXmlPropertyDouble("genxml/appliedtotal", total);

            if (PurchaseInfo.GetXmlProperty("genxml/clientmode") == "True")
            {
                // user not editor, so stop edit mode.
                if (!UserController.Instance.GetCurrentUserInfo().IsInRole("Administrators") && !UserController.Instance.GetCurrentUserInfo().IsInRole(StoreSettings.ManagerRole) && !UserController.Instance.GetCurrentUserInfo().IsInRole(StoreSettings.EditorRole) && !UserController.Instance.GetCurrentUserInfo().IsInRole(StoreSettings.SalesRole)) PurchaseInfo.SetXmlProperty("genxml/clientmode", "False");
            }

            PurchaseInfo = NBrightBuyUtils.ProcessEventProvider(EventActions.ValidateCartAfter, PurchaseInfo);

            SavePurchaseData();
        }

        private NBrightInfo ValidateCartItem(int portalId, int userId, NBrightInfo cartItemInfo,Boolean removeZeroQtyItems = false)
        {
            #region "get cart values"
            cartItemInfo = NBrightBuyUtils.ProcessEventProvider(EventActions.ValidateCartItemBefore, cartItemInfo);

            var modelid = cartItemInfo.GetXmlProperty("genxml/modelid");
            var prdid = cartItemInfo.GetXmlPropertyInt("genxml/productid");
            var qty = cartItemInfo.GetXmlPropertyDouble("genxml/qty");
            var lang = cartItemInfo.GetXmlProperty("genxml/lang");

            if (removeZeroQtyItems && qty == 0) return null; // Remove zero qty item

            if (lang == "") lang = Utils.GetCurrentCulture();
            var prd = ProductUtils.GetProductData(prdid, lang);
            if (!prd.Exists || prd.Disabled) return null; //Invalid product remove from cart

            // update product xml data on cart (product may have change via plugin so always replace)
            cartItemInfo.RemoveXmlNode("genxml/productxml");
            // add entitytype for validation methods
            prd.Info.SetXmlProperty("genxml/entitytypecode", prd.Info.TypeCode);
            cartItemInfo.AddSingleNode("productxml", prd.Info.XMLData, "genxml");

            var prdModel = prd.GetModel(modelid);
            if (prdModel == null) return null; // Invalid Model remove from cart
            // check if dealer (for tax calc)
            if (NBrightBuyUtils.IsDealer())
                cartItemInfo.SetXmlProperty("genxml/isdealer", "True");
            else
                cartItemInfo.SetXmlProperty("genxml/isdealer", "False");

            // check for price change
            var unitcost = prdModel.GetXmlPropertyDouble("genxml/textbox/txtunitcost");
            var dealercost = prdModel.GetXmlPropertyDouble("genxml/textbox/txtdealercost");
            var saleprice = prdModel.GetXmlPropertyDouble("genxml/textbox/txtsaleprice");
            var dealersalecost = prdModel.GetXmlPropertyDouble("genxml/textbox/txtdealersale");

            // use dynamic price is flagged in cartitem.
            if (cartItemInfo.GetXmlPropertyBool("genxml/dynamicpriceflag") && cartItemInfo.GetXmlProperty("genxml/dynamicprice") != "")
            {
                unitcost = cartItemInfo.GetXmlPropertyDouble("genxml/dynamicprice");
                dealercost = cartItemInfo.GetXmlPropertyDouble("genxml/dynamicprice");
                saleprice = cartItemInfo.GetXmlPropertyDouble("genxml/dynamicprice");
                dealersalecost = cartItemInfo.GetXmlPropertyDouble("genxml/dynamicprice");
            }

            // always make sale price best price
            if (unitcost > 0 && (unitcost < saleprice || saleprice <= 0)) saleprice = unitcost;
            // if we have a promoprice use it as saleprice (This is passed in via events provider like "Multi-Buy promotions")
            if (cartItemInfo.GetXmlPropertyDouble("genxml/promoprice") > 0 && cartItemInfo.GetXmlPropertyDouble("genxml/promoprice") < saleprice)
            {
                saleprice = cartItemInfo.GetXmlPropertyDouble("genxml/promoprice");
            }

            // always assign the dealercost the best price.
            if (dealersalecost > 0 && dealersalecost < dealercost) dealercost = dealersalecost;
            if (saleprice > 0 && (saleprice < dealercost || dealercost <= 0)) dealercost = saleprice;
            if (unitcost > 0 && (unitcost < dealercost || dealercost <= 0)) dealercost = unitcost;



            // calc sell price
            // sellcost = the calculated cost of the item.  
            var sellcost = unitcost;
            if (saleprice > 0 && saleprice < sellcost) sellcost = saleprice;
            if (NBrightBuyUtils.IsDealer())
            {
                if (dealercost > 0 && dealercost < sellcost) sellcost = dealercost;
            }
            // --------------------------------------------
            #endregion

            if (prdModel != null)
            {
                #region "Stock Control"
                var stockon = prdModel.GetXmlPropertyBool("genxml/checkbox/chkstockon");
                var stocklevel = prdModel.GetXmlPropertyDouble("genxml/textbox/txtqtyremaining");
                var minStock = prdModel.GetXmlPropertyDouble("genxml/textbox/txtqtyminstock");
                if (minStock == 0) minStock = StoreSettings.Current.GetInt("minimumstocklevel");
                var maxStock = prdModel.GetXmlPropertyDouble("genxml/textbox/txtqtystockset");
                var weight = prdModel.GetXmlPropertyDouble("genxml/textbox/weight");
                if (stockon)
                {
                    stocklevel = stocklevel - minStock;
                    stocklevel = stocklevel - prd.GetModelTransQty(modelid, _cartId);
                    if (stocklevel < qty)
                    {
                        qty = stocklevel;
                        if (qty <= 0)
                        {
                            qty = 0;
                            cartItemInfo.SetXmlProperty("genxml/validatecode", "OUTOFSTOCK");
                        }
                        else
                        {
                            cartItemInfo.SetXmlProperty("genxml/validatecode", "STOCKADJ");
                        }
                        base.SetValidated(false);
                        cartItemInfo.SetXmlPropertyDouble("genxml/qty", qty.ToString(""));
                    }
                }
                #endregion

                #region "Addtional options costs"
                Double additionalCosts = 0;
                var optNods = cartItemInfo.XMLDoc.SelectNodes("genxml/options/*");
                if (optNods != null)
                {
                    var lp = 1;
                    foreach (XmlNode nod in optNods)
                    {
                        var optid = nod.SelectSingleNode("optid");
                        if (optid != null)
                        {
                            var optvalueid = nod.SelectSingleNode("optvalueid");
                            if (optvalueid != null && optvalueid.InnerText != "False")
                            {
                                XmlNode  optvalcostnod;
                                if (optvalueid.InnerText == "True")
                                    optvalcostnod = cartItemInfo.XMLDoc.SelectSingleNode("genxml/productxml/genxml/optionvalues[@optionid='" + optid.InnerText + "']/genxml/textbox/txtaddedcost");
                                else
                                    optvalcostnod = cartItemInfo.XMLDoc.SelectSingleNode("genxml/productxml/genxml/optionvalues/genxml[./hidden/optionvalueid='" + optvalueid.InnerText + "']/textbox/txtaddedcost");

                                if (optvalcostnod != null)
                                {
                                    var optvalcost = optvalcostnod.InnerText;
                                    if (Utils.IsNumeric(optvalcost))
                                    {
                                        cartItemInfo.SetXmlPropertyDouble("genxml/options/option[" + lp + "]/optvalcost", optvalcost);
                                        var optvaltotal = Convert.ToDouble(optvalcost, CultureInfo.GetCultureInfo("en-US"))*qty;
                                        cartItemInfo.SetXmlPropertyDouble("genxml/options/option[" + lp + "]/optvaltotal", optvaltotal);
                                        additionalCosts += optvaltotal;
                                    }
                                }
                                else
                                {
                                    cartItemInfo.SetXmlPropertyDouble("genxml/options/option[" + lp + "]/optvalcost", "0");
                                    cartItemInfo.SetXmlPropertyDouble("genxml/options/option[" + lp + "]/optvaltotal", "0");
                                }
                            }
                        }
                        lp += 1;
                    }
                }

                if (qty > 0)  // can't devide by zero
                {                   
                    unitcost += (additionalCosts / qty);
                    if (dealercost > 0) dealercost += (additionalCosts / qty); // zero turns off
                    if (saleprice > 0) saleprice += (additionalCosts / qty); // zero turns off
                    sellcost += (additionalCosts / qty);
                }
                #endregion

                var totalcost = qty * unitcost;
                var totalsellcost = qty * sellcost;
                var totalweight = weight * qty;

                cartItemInfo.SetXmlPropertyDouble("genxml/unitcost", unitcost);
                cartItemInfo.SetXmlPropertyDouble("genxml/dealercost", dealercost);
                cartItemInfo.SetXmlPropertyDouble("genxml/saleprice", saleprice);

                cartItemInfo.SetXmlPropertyDouble("genxml/totalweight", totalweight.ToString(""));
                cartItemInfo.SetXmlPropertyDouble("genxml/totalcost", totalcost);
                cartItemInfo.SetXmlPropertyDouble("genxml/totaldealerbonus", (totalcost - (qty * dealercost)));

                Double salediscount = 0;
                Double discountcodeamt = 0;
                Double totaldiscount = 0;

                //add update genxml/discountcodeamt
                var discountcode = PurchaseInfo.GetXmlProperty("genxml/extrainfo/genxml/textbox/promocode");
                if (discountcode != "") 
                {
                    cartItemInfo = DiscountCodeInterface.UpdateItemPercentDiscountCode(PortalId, UserId, cartItemInfo, discountcode);
                    discountcodeamt = cartItemInfo.GetXmlPropertyDouble("genxml/discountcodeamt");
                    if (discountcodeamt > 0) PurchaseInfo.SetXmlProperty("genxml/discountprocessed", "False");
                }

                if (NBrightBuyUtils.IsDealer())
                {
                    salediscount = (unitcost - dealercost);
                }
                else
                {
                    salediscount = (unitcost - saleprice);
                }
                totaldiscount = (salediscount * qty) + discountcodeamt; // add on any promo code amount
                if (totaldiscount < 0) totaldiscount = 0;

                // if we have a promodiscount use it
                if (cartItemInfo.GetXmlPropertyDouble("genxml/promodiscount") > 0)
                {
                    totaldiscount = cartItemInfo.GetXmlPropertyDouble("genxml/promodiscount");
                    totalcost = totalcost - totaldiscount;
                    if (totalcost < 0) totalcost = 0;
                    cartItemInfo.SetXmlPropertyDouble("genxml/appliedtotalcost", totalcost);
                }


                cartItemInfo.SetXmlPropertyDouble("genxml/totaldiscount", totaldiscount);

                // if product is on sale then we need to display the sale price in the cart, and any discount codes don;t show at this cart item level, only on the order total.
                cartItemInfo.SetXmlPropertyDouble("genxml/appliedtotalcost", totalsellcost); 
                cartItemInfo.SetXmlPropertyDouble("genxml/appliedcost", sellcost); 


                // calc tax for item
                var taxproviderkey = PurchaseInfo.GetXmlProperty("genxml/hidden/taxproviderkey");
                var taxprov = TaxInterface.Instance(taxproviderkey);
                if (taxprov != null)
                {
                    var nbi = (NBrightInfo)cartItemInfo.Clone();
                    cartItemInfo.SetXmlPropertyDouble("genxml/taxcost", taxprov.CalculateItemTax(nbi));
                }

            }


            cartItemInfo = NBrightBuyUtils.ProcessEventProvider(EventActions.ValidateCartItemAfter, cartItemInfo);

            return cartItemInfo;
        }

        private Double AppliedCost(Double cost, Double dealercost)
        {
            if (cost < 0) cost = 0;
            if (dealercost < 0) dealercost = 0;
            var rtncost = cost;
            if (NBrightBuyUtils.IsDealer())
            {
                if (dealercost > 0 && dealercost < cost) rtncost = dealercost;
            }
            return rtncost;
        }


        #endregion

    }
}
