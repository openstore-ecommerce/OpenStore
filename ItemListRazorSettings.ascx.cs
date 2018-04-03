using System;
using System.Web.UI.WebControls;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
using NBrightCore.common;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Base;
using Nevoweb.DNN.NBrightBuy.Components;

namespace Nevoweb.DNN.NBrightBuy
{

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The Settings class manages Module Settings
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class ItemListRazorSettings : ModuleSettingsBase
    {

        // Expose SettingsTemplate so it can be overwritten by plugin using this module to display articles.
        public String SettingsTemplate = "";

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            // insert page header text
            NBrightBuyUtils.RazorIncludePageHeader(ModuleId, Page, "settingspageheader.cshtml", ControlPath, "config", StoreSettings.Current.Settings());
        }

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
                var obj = NBrightBuyUtils.GetSettings(PortalId,ModuleId);
                obj.ModuleId = base.ModuleId; // need to pass the moduleid here, becuase it doesn;t exists in url for settings and on new settings it needs it.

                if (String.IsNullOrEmpty(SettingsTemplate)) SettingsTemplate = ModuleConfiguration.DesktopModule.ModuleName + "settings.cshtml"; // default to name of module

                var strOut = NBrightBuyUtils.RazorTemplRender(SettingsTemplate, ModuleId, "", obj, ControlPath, "config", Utils.GetCurrentCulture(), StoreSettings.Current.Settings());
                var lit = new Literal();
                lit.Text = strOut;
                phData.Controls.Add(lit);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }


    }

}

