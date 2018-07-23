
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


	public abstract class EntityTypeInterface
	{

        public static string providerKey = "";

        #region "Shared/Static Methods"

        // constructor
        static EntityTypeInterface()
		{
			CreateProvider();
		}

        //private static ShippingInterface objProvider = null;
        private static Dictionary<String, EntityTypeInterface> _providerList; 

        // dynamically create provider
        private static void CreateProvider()
        {

            _providerList = new Dictionary<string, EntityTypeInterface>();

            var pluginData = new PluginData(PortalSettings.Current.PortalId);
            var l = pluginData.GetEntityTypeProviders(false);

            foreach (var p in l)
            {
                var prov = p.Value;
                ObjectHandle handle = null;
                handle = Activator.CreateInstance(prov.GetXmlProperty("genxml/textbox/assembly"),
                prov.GetXmlProperty("genxml/textbox/namespaceclass"));
                var objProvider = (EntityTypeInterface)handle.Unwrap();
                var ctrlkey = prov.GetXmlProperty("genxml/textbox/ctrl");
                if (!_providerList.ContainsKey(ctrlkey))
                {
                    _providerList.Add(ctrlkey, objProvider);
                }
            }

        }

        // return the provider
        public static EntityTypeInterface Instance(String ctrlkey)
        {
            providerKey = ctrlkey;
            if (_providerList.ContainsKey(ctrlkey)) return _providerList[ctrlkey];
            if (_providerList.Count > 0) return _providerList.Values.First();
            return null;
        }

		#endregion

	    public abstract String GetEntityTypeCode();
        public abstract String GetEntityTypeCodeLang();

	}

}

