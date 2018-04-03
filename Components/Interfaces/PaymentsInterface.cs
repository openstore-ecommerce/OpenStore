
using System.Linq;
using System.Security.Policy;
using System.Web;
using DotNetNuke.Entities.Portals;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using NBrightDNN;

namespace Nevoweb.DNN.NBrightBuy.Components.Interfaces
{


	public abstract class PaymentsInterface
	{

		#region "Shared/Static Methods"

		// singleton reference to the instantiated object 

        //private static ShippingInterface objProvider = null;
        private static Dictionary<String, PaymentsInterface> _providerList; 
        // constructor
        static PaymentsInterface()
		{
			CreateProvider();
		}

		// dynamically create provider
		private static void CreateProvider()
		{

			string providerName = null;

            _providerList = new Dictionary<string, PaymentsInterface>();

            var pluginData = new PluginData(PortalSettings.Current.PortalId);
		    var l = pluginData.GetPaymentProviders(false);

		    foreach (var p in l)
		    {
		        var prov = p.Value;
		        ObjectHandle handle = null;
		        handle = Activator.CreateInstance(prov.GetXmlProperty("genxml/textbox/assembly"),
		            prov.GetXmlProperty("genxml/textbox/namespaceclass"));
		        var objProvider = (PaymentsInterface) handle.Unwrap();
		        var ctrlkey = prov.GetXmlProperty("genxml/textbox/ctrl");
		        if (!_providerList.ContainsKey(ctrlkey))
		        {
		            objProvider.Paymentskey = ctrlkey;
		            _providerList.Add(ctrlkey, objProvider);
		        }
		    }

		}


		// return the provider
        public static PaymentsInterface Instance(String ctrlkey)
		{
            if (_providerList.ContainsKey(ctrlkey)) return _providerList[ctrlkey];
            if (_providerList.Count > 0) return _providerList.Values.First();
            return null;
		}

		#endregion

        public abstract String Paymentskey { get; set; }

        public abstract String GetTemplate(NBrightInfo cartInfo);

        public abstract String RedirectForPayment(OrderData orderData);

        public abstract String ProcessPaymentReturn(HttpContext context);


    }

}

