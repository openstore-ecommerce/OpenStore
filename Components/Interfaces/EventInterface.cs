
using System.Linq;
using DotNetNuke.Entities.Portals;
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;


using System.Runtime.Remoting;
using NBrightDNN;

namespace Nevoweb.DNN.NBrightBuy.Components.Interfaces
{


	public abstract class EventInterface
	{

        public abstract NBrightInfo ValidateCartBefore(NBrightInfo cartInfo);
        public abstract NBrightInfo ValidateCartAfter(NBrightInfo cartInfo);
        public abstract NBrightInfo ValidateCartItemBefore(NBrightInfo cartItemInfo);
        public abstract NBrightInfo ValidateCartItemAfter(NBrightInfo cartItemInfo);

        public abstract NBrightInfo AfterCartSave(NBrightInfo nbrightInfo);
        public abstract NBrightInfo AfterCategorySave(NBrightInfo nbrightInfo);
        public abstract NBrightInfo AfterProductSave(NBrightInfo nbrightInfo);
        public abstract NBrightInfo AfterSavePurchaseData(NBrightInfo nbrightInfo);
        public abstract NBrightInfo BeforeOrderStatusChange(NBrightInfo nbrightInfo);
        public abstract NBrightInfo AfterOrderStatusChange(NBrightInfo nbrightInfo);
        public abstract NBrightInfo BeforePaymentOK(NBrightInfo nbrightInfo);
        public abstract NBrightInfo AfterPaymentOK(NBrightInfo nbrightInfo);
        public abstract NBrightInfo BeforePaymentFail(NBrightInfo nbrightInfo);
        public abstract NBrightInfo AfterPaymentFail(NBrightInfo nbrightInfo);
        public abstract NBrightInfo BeforeSendEmail(NBrightInfo nbrightInfo, String emailsubjectrexkey);
        public abstract NBrightInfo AfterSendEmail(NBrightInfo nbrightInfo, String emailsubjectrexkey);

	}

}

