<%@ Control language="C#" Inherits="Nevoweb.DNN.NBrightBuy.Admin.Import" AutoEventWireup="true"  Codebehind="Import.ascx.cs" %>
<asp:PlaceHolder ID="notifymsg" runat="server"></asp:PlaceHolder>
<asp:Repeater ID="rpData" runat="server" OnItemCommand="CtrlItemCommand" ></asp:Repeater>
<asp:PlaceHolder ID="phData" runat="server"></asp:PlaceHolder>

