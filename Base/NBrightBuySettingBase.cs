using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI.WebControls;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;

namespace Nevoweb.DNN.NBrightBuy.Base
{

    public class NBrightBuySettingBase : ModuleSettingsBase
	{

		public NBrightBuyController ModCtrl;
        public string CtrlTypeCode;
        public string CtrlPluginPath;
        public ModSettings ModSettings;

		protected Repeater RpData;

		protected override void OnInit(EventArgs e)
		{

            //Remove any cache for the module, we don't want any cache in/after the BO editing.
            NBrightBuyUtils.RemoveModCache(ModuleId);

            // add required controls.
            RpData = new Repeater();
            RpData.ItemCommand += CtrlItemCommand;

			this.Controls.Add(RpData);

			//set default Data controller
            ModCtrl = new NBrightBuyController();


            #region "Get all Settings for module"
            //get Model Level Settings
            ModSettings = new ModSettings(ModuleId, Settings);

            #endregion

		    var strTemplate = "";
            if (!String.IsNullOrEmpty(CtrlPluginPath))
		    {
                //search plugin path for template
                strTemplate = NBrightBuyUtils.GetTemplateData(CtrlTypeCode + "_Settings.html", CtrlPluginPath, "config", ModSettings.Settings());
		    }
            if (strTemplate == "")
		    {
                // add themefolder to settings, incase module has independant theme.
                strTemplate = ModCtrl.GetTemplateData(ModSettings, CtrlTypeCode + "_Settings.html", Utils.GetCurrentCulture(), StoreSettings.Current.DebugMode);
		    }
            if (strTemplate != "") RpData.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(strTemplate, ModSettings.Settings(), PortalSettings.HomeDirectory);		        

            //add template provider to NBright Templating
            NBrightCore.providers.GenXProviderManager.AddProvider("NBrightBuy,Nevoweb.DNN.NBrightBuy.render.GenXmlTemplateExt");
            var pInfo = ModCtrl.GetByGuidKey(PortalId, -1, "PROVIDERS", "NBrightTempalteProviders");
            if (pInfo != null)
            {
                NBrightCore.providers.GenXProviderManager.AddProvider(pInfo.XMLDoc);
            }


			base.OnInit(e);

		}


		#region "Page Load"


		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (Page.IsPostBack == false)
			{
				PageLoad();
			}
		}

		private void PageLoad()
		{
			try
			{
                // load the notificationmessage if with have a placeholder control to display it.
                var ctrlMsg = this.FindControl("notifymsg");
                if (ctrlMsg != null)
                {
                    var msg = NBrightBuyUtils.GetNotfiyMessage(ModuleId);
                    var l = new Literal { Text = msg };
                    ctrlMsg.Controls.Add(l);
                }

                if (CtrlTypeCode != "")
                {
                    // display the detail
                    var l = new List<NBrightInfo>();
                    var moduleSettings = NBrightBuyUtils.GetSettings(PortalId, ModuleId, CtrlTypeCode, false);
                    moduleSettings = EventBeforeRender(moduleSettings);
                    l.Add(moduleSettings);
                    RpData.DataSource = l;
                    RpData.DataBind();

                    //if new setting, save theme and reload, so we get theme settings displayed.
                    if (moduleSettings.ItemID == -1)
                    {
                        UpdateData();
                        Response.Redirect(Request.Url.AbsoluteUri,true);
                    }
                }
			}
			catch (Exception exc) //Module failed to load
			{
				Exceptions.ProcessModuleLoadException(this, exc);
			}
		}

		#endregion


        #region  "Events "

        protected void CtrlItemCommand(object source, RepeaterCommandEventArgs e)
        {

            switch (e.CommandName.ToLower())
            {
                case "save":
                    UpdateData();
                    break;
            }

        }

        public virtual NBrightInfo EventBeforeRender(NBrightInfo objInfo)
        {
            return objInfo;
        }

        public virtual NBrightInfo EventBeforeUpdate(Repeater rpData, NBrightInfo objInfo)
        {
            return objInfo;
        }

        public virtual void EventAfterUpdate(Repeater rpData, NBrightInfo objInfo)
        {

        }


        #endregion

		#region "presistance methods"

        protected void UpdateData()
		{

            if (CtrlTypeCode != "")
            {
                // read any existing data or create new.
                var objInfo = NBrightBuyUtils.GetSettings(PortalId, ModuleId, CtrlTypeCode, false);

                // populate changed data
                objInfo.ModifiedDate = DateTime.Now;

                //rebuild xml
                objInfo.XMLData = GenXmlFunctions.GetGenXml(RpData);

                objInfo = EventBeforeUpdate(RpData, objInfo);

                objInfo.ItemID = ModCtrl.Update(objInfo);


                EventAfterUpdate(RpData, objInfo);

                // clear any store level cache, might be overkill to clear ALL Store cache, 
                // but editing of settings should only happen when changes are being made.
                NBrightBuyUtils.RemoveModCache(-1);
                NBrightBuyUtils.RemoveModCache(ModuleId);

            }
        }

		#endregion



    }

}
