<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.UI.Skins.Skin" %>
<%@ Register TagPrefix="nbs" TagName="BACKOFFICE" Src="~/DesktopModules/NBright/NBrightBuy/Admin/BackOffice.ascx" %>
<style type="text/css">#ControlBar_ControlPanel,.dnnDragHint,.actionMenu,.dnnActionMenuBorder,.dnnActionMenu{display:none !important}#Form.showControlBar{margin-top:0 !important}.dnnEditState .DnnModule,.DnnModule{opacity:1 !important}.dnnSortable{min-height:0 !important}</style>
<div id="ControlPanel" runat="server" visible="false"></div>
<div id="ContentPane" class="" runat="server"></div>
<nbs:BACKOFFICE id="nbsBackOffice" runat="server" />