using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using NBrightDNN;
using NBrightDNN.controls;
using NBrightCore.common;
using System.Xml;
using Nevoweb.DNN.NBrightBuy.Components;

namespace Nevoweb.DNN.NBrightBuy.Base
{
    public class NBrightBuyFrontOfficeBase : NBrightBuyBase
	{
        private string _controlPath = "";

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            _controlPath = ControlPath;

            if (ModSettings.Settings().ContainsKey("themefolder") && ModSettings.Settings()["themefolder"] != "")
            {
                ThemeFolder = ModSettings.Settings()["themefolder"];
            }
            if (ThemeFolder == "") ThemeFolder = StoreSettings.Current.ThemeFolder;

            if (ModSettings.Settings().ContainsKey("razortemplate") && ModSettings.Settings()["razortemplate"] != "")
            {
                RazorTemplate = ModSettings.Settings()["razortemplate"];
            }
            if (RazorTemplate == "" && ModSettings.Settings().ContainsKey("razorlisttemplate") && ModSettings.Settings()["razorlisttemplate"] != "")
            {
                RazorTemplate = ModSettings.Settings()["razorlisttemplate"];
            }
            if (RazorTemplate == "" && ModSettings.Settings().ContainsKey("razordetailtemplate") && ModSettings.Settings()["razordetailtemplate"] != "")
            {
                RazorTemplate = ModSettings.Settings()["razordetailtemplate"];
            }

            if (ModSettings != null)
            {
                // check if we're using a provider controlpath for the templates.
                var providercontrolpath = ModSettings.Get("providercontrolpath");
                if (providercontrolpath != "")
                {
                    _controlPath = "/DesktopModules/NBright/" + providercontrolpath + "/";
                }
            }


            // insert page header text
            NBrightBuyUtils.RazorIncludePageHeader(ModuleId, Page, "FrontOfficePageHeader.cshtml", _controlPath, ThemeFolder, ModSettings.Settings());

            // insert text in body.
            NBrightBuyUtils.RazorIncludePageBody(ModuleId, Page, "FrontOfficePageBody.cshtml", _controlPath, ThemeFolder, ModSettings.Settings());


            if (ModuleContext.Configuration != null)
            {
                if (String.IsNullOrEmpty(RazorTemplate)) RazorTemplate = ModuleConfiguration.DesktopModule.ModuleName + ".cshtml";

                // insert page header text
                NBrightBuyUtils.RazorIncludePageHeader(ModuleId, Page, Path.GetFileNameWithoutExtension(RazorTemplate) + "_head" + Path.GetExtension(RazorTemplate), _controlPath, ThemeFolder, ModSettings.Settings());
                // insert page body text for template name. 
                NBrightBuyUtils.RazorIncludePageBody(ModuleId, Page, Path.GetFileNameWithoutExtension(RazorTemplate) + "_pageinject" + Path.GetExtension(RazorTemplate), _controlPath, ThemeFolder, ModSettings.Settings());
            }
            var strOut = "<span class='container_" + ThemeFolder + "_" + RazorTemplate + "'>";

            Controls.AddAt(0, new LiteralControl("<div class='container_" + ThemeFolder.ToLower().Replace(" ","_") + "_" + RazorTemplate.ToLower().Replace(".cshtml","").Replace(" ", "_") + "'>"));
            Controls.AddAt(Controls.Count, new LiteralControl("</div>"));

        }

	}
}
