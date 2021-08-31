using NBrightCore.common;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;
using System;
using System.Collections.Generic;

namespace Nevoweb.DNN.NBrightBuy.Providers.PromoProvider
{
    public class GroupPromoScheudler : Components.Interfaces.SchedulerInterface
    {
        public override string DoWork(int portalId)
        {
            return PromoUtils.CalcGroupPromo(portalId);
        }
    }

    public class MultiBuyPromoScheudler : Components.Interfaces.SchedulerInterface
    {
        public override string DoWork(int portalId)
        {
            return PromoUtils.CalcMultiBuyPromo(portalId);
        }
    }

    public class CalcPromo : Components.Interfaces.PromoInterface
    {

        public override string ProviderKey { get; set; }

        public override NBrightInfo CalculatePromotion(int portalId, NBrightInfo cartInfo)
        {
            // loop through cart items
            var rtncartInfo = (NBrightInfo)cartInfo.Clone();
            try
            {

                var cartData = new CartData(cartInfo.PortalId);
                var cartList = cartData.GetCartItemList();

                foreach (var cartItemInfo in cartList)
                {
                    cartInfo.SetXmlPropertyDouble("genxml/items/genxml[./itemcode = '" + cartItemInfo.GetXmlProperty("genxml/itemcode") + "']/promodiscount", 0); // remove any existing discount
                    if (cartItemInfo.GetXmlProperty("genxml/productxml/genxml/hidden/promotype") == "PROMOMULTIBUY")
                    {

                        var promoid = cartItemInfo.GetXmlPropertyInt("genxml/productxml/genxml/hidden/promoid");
                        var objCtrl = new NBrightBuyController();
                        var promoData = objCtrl.GetData(promoid);
                        if (promoData != null)
                        {
                            //NOTE: WE need to process disabld promotions so they can be removed from cart

                            var buyqty = promoData.GetXmlPropertyInt("genxml/textbox/buyqty");
                            var validfrom = promoData.GetXmlProperty("genxml/textbox/validfrom");
                            var validuntil = promoData.GetXmlProperty("genxml/textbox/validuntil");
                            var propbuygroupid = promoData.GetXmlProperty("genxml/dropdownlist/propbuy");
                            var propapplygroupid = promoData.GetXmlProperty("genxml/dropdownlist/propapply");
                            var amounttype = promoData.GetXmlProperty("genxml/radiobuttonlist/amounttype");
                            var amount = promoData.GetXmlPropertyDouble("genxml/textbox/amount");

                            // Applied discount to this single cart item
                            if (!promoData.GetXmlPropertyBool("genxml/checkbox/disabled") && cartItemInfo.GetXmlPropertyInt("genxml/qty") >= buyqty && Utils.IsDate(validfrom) && Utils.IsDate(validuntil)) // check we have correct qty to activate promo
                            {
                                var dteF = Convert.ToDateTime(validfrom).Date;
                                var dteU = Convert.ToDateTime(validuntil).Date;
                                if (DateTime.Now.Date >= dteF && DateTime.Now.Date <= dteU)
                                {
                                    // calc discount amount

                                    var cartqty = cartItemInfo.GetXmlPropertyDouble("genxml/qty");
                                    var qtycount = cartqty;
                                    var unitcost = cartItemInfo.GetXmlPropertyDouble("genxml/basecost");
                                    double discountamt = 0;
                                    while (qtycount > buyqty)
                                    {
                                        if (amounttype == "1")
                                        {
                                            discountamt += (unitcost - amount);
                                        }
                                        else
                                        {
                                            discountamt += ((unitcost/100)*amount);
                                        }
                                        if (discountamt < 0) discountamt = 0;

                                        qtycount = (qtycount - (buyqty + 1)); // +1 so we allow for discount 1 in basket.
                                    }

                                    cartInfo.SetXmlPropertyDouble("genxml/items/genxml[./itemcode = '" + cartItemInfo.GetXmlProperty("genxml/itemcode") + "']/promodiscount", discountamt);

                                }
                            }
                        }
                    }
                }

                return cartInfo;
            }
            catch (Exception ex)
            {
                var x = ex.ToString();
                return rtncartInfo;
            }
        }

    }


    public class PromoEvents : Components.Interfaces.EventInterface
    {
        public override NBrightInfo ValidateCartBefore(NBrightInfo cartInfo)
        {
            return cartInfo;
        }

        public override NBrightInfo ValidateCartAfter(NBrightInfo cartInfo)
        {
            return cartInfo;
        }

        public override NBrightInfo ValidateCartItemBefore(NBrightInfo cartItemInfo)
        {
            return cartItemInfo;
        }

        public override NBrightInfo ValidateCartItemAfter(NBrightInfo cartItemInfo)
        {
            return cartItemInfo;
        }

        public override NBrightInfo AfterCartSave(NBrightInfo nbrightInfo)
        {
            return nbrightInfo;
        }

        public override NBrightInfo AfterCategorySave(NBrightInfo nbrightInfo)
        {
            return nbrightInfo;
        }

        public override NBrightInfo AfterProductSave(NBrightInfo nbrightInfo)
        {
            var promoid = nbrightInfo.GetXmlPropertyInt("genxml/hidden/promoid"); // legacy promo flag
            if (nbrightInfo.GetXmlPropertyBool("genxml/hidden/promoflag") || promoid > 0)
            {
                var prdData = ProductUtils.GetProductData(nbrightInfo.ItemID, nbrightInfo.PortalId, nbrightInfo.Lang);
                // loop on models to get all promoid at model level.
                var modelpromoids = new List<int>();
                if (promoid > 0) modelpromoids.Add(promoid);
                var lp = 1;
                foreach (var m in prdData.Models)
                {
                    var modelPromoId = prdData.Info.GetXmlPropertyInt("genxml/promo/salepriceid" + lp);
                    if (modelPromoId > 0 && !modelpromoids.Contains(modelPromoId)) modelpromoids.Add(modelPromoId);
                    modelPromoId = prdData.Info.GetXmlPropertyInt("genxml/promo/dealercostid" + lp);
                    if (modelPromoId > 0 && !modelpromoids.Contains(modelPromoId)) modelpromoids.Add(modelPromoId);
                    modelPromoId = prdData.Info.GetXmlPropertyInt("genxml/promo/dealersaleid" + lp);
                    if (modelPromoId > 0 && !modelpromoids.Contains(modelPromoId)) modelpromoids.Add(modelPromoId);
                    lp += 1;
                }

                // multiple promotions, remove from each model.
                foreach (var mpid in modelpromoids)
                {
                    var objCtrl = new NBrightBuyController();
                    var promoData = objCtrl.GetData(mpid);

                    var catgroupid = promoData.GetXmlPropertyInt("genxml/dropdownlist/catgroupid");
                    var propgroupid = promoData.GetXmlPropertyInt("genxml/dropdownlist/propgroupid");
                    var propbuygroupid = promoData.GetXmlPropertyInt("genxml/dropdownlist/propbuy");
                    var propapplygroupid = promoData.GetXmlPropertyInt("genxml/dropdownlist/propapply");

                    var removepromo = true;
                    foreach (var c in prdData.GetCategories(nbrightInfo.PortalId))
                    {
                        if (c.categoryid == catgroupid) removepromo = false;
                        if (c.categoryid == propgroupid) removepromo = false;
                        if (c.categoryid == propbuygroupid) removepromo = false;
                        if (c.categoryid == propapplygroupid) removepromo = false;
                    }

                    if (removepromo)
                    {
                        PromoUtils.RemoveProductPromoData(nbrightInfo.PortalId, nbrightInfo.ItemID, mpid);
                        ProductUtils.RemoveProductDataCache(nbrightInfo.PortalId, nbrightInfo.ItemID);
                    }

                }
            }

            return nbrightInfo;
        }

        public override NBrightInfo AfterSavePurchaseData(NBrightInfo nbrightInfo)
        {
            return nbrightInfo;
        }

        public override NBrightInfo BeforeOrderStatusChange(NBrightInfo nbrightInfo)
        {
            return nbrightInfo;
        }

        public override NBrightInfo AfterOrderStatusChange(NBrightInfo nbrightInfo)
        {
            return nbrightInfo;
        }

        public override NBrightInfo BeforePaymentOK(NBrightInfo nbrightInfo)
        {
            return nbrightInfo;
        }

        public override NBrightInfo AfterPaymentOK(NBrightInfo nbrightInfo)
        {
            return nbrightInfo;
        }

        public override NBrightInfo BeforePaymentFail(NBrightInfo nbrightInfo)
        {
            return nbrightInfo;
        }

        public override NBrightInfo AfterPaymentFail(NBrightInfo nbrightInfo)
        {
            return nbrightInfo;
        }

        public override NBrightInfo BeforeSendEmail(NBrightInfo nbrightInfo, string emailsubjectrexkey)
        {
            return nbrightInfo;
        }

        public override NBrightInfo AfterSendEmail(NBrightInfo nbrightInfo, string emailsubjectrexkey)
        {
            return nbrightInfo;
        }
    }
}
