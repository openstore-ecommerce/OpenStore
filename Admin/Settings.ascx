<%@ Control language="C#" Inherits="Nevoweb.DNN.NBrightBuy.Admin.Settings" AutoEventWireup="true"  Codebehind="Settings.ascx.cs" %>
<asp:PlaceHolder ID="notifymsg" runat="server"></asp:PlaceHolder>
<asp:Repeater ID="rpDataH" runat="server" OnItemCommand="CtrlItemCommand" ></asp:Repeater>
<asp:Repeater ID="rpData" runat="server" OnItemCommand="CtrlItemCommand" ></asp:Repeater>
<asp:Repeater ID="rpDataF" runat="server" OnItemCommand="CtrlItemCommand" ></asp:Repeater>
<asp:PlaceHolder ID="phData" runat="server"></asp:PlaceHolder>

