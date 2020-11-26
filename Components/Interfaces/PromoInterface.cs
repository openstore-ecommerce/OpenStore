
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


	public abstract class PromoInterface
	{

		#region "Shared/Static Methods"

		// singleton reference to the instantiated object 

        //private static ShippingInterface objProvider = null;
        public static Dictionary<String, PromoInterface> ProviderList; 

        // constructor
        static PromoInterface()
		{
			CreateProvider();
		}

		// dynamically create provider
		private static void CreateProvider()
		{

            ProviderList = new Dictionary<string, PromoInterface>();

            var pluginData = new PluginData(PortalSettings.Current.PortalId);
            var l = pluginData.GetPromoProviders();

            foreach (var p in l)
            {
                    var prov = p.Value;
                    ObjectHandle handle = null;
                    handle = Activator.CreateInstance(prov.GetXmlProperty("genxml/textbox/assembly"), prov.GetXmlProperty("genxml/textbox/namespaceclass"));
                    var objProvider = (PromoInterface)handle.Unwrap();
                    var ctrlkey = prov.GetXmlProperty("genxml/textbox/ctrl");
                    var lp = 1;
                    while (ProviderList.ContainsKey(ctrlkey))
                    {
                        ctrlkey = ctrlkey + lp.ToString("");
                        lp += 1;
                    }
                    objProvider.ProviderKey = ctrlkey;
                    ProviderList.Add(ctrlkey, objProvider);
                if (!ProviderList.ContainsKey(ctrlkey)) ProviderList.Add(ctrlkey, objProvider);
            }

        }


		// return the provider
        public static PromoInterface Instance(String ctrlkey = "")
		{
            if (ProviderList.ContainsKey(ctrlkey)) return ProviderList[ctrlkey];
            if (ProviderList.Count > 0) return ProviderList.Values.First();
            return null;
		}

		#endregion

        public abstract String ProviderKey { get; set; }

        public abstract NBrightInfo CalculatePromotion(int portalId, NBrightInfo cartInfo);


    }

}

