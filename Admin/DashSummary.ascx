<%@ Control language="C#" Inherits="Nevoweb.DNN.NBrightBuy.Admin.DashSummary" AutoEventWireup="true"  Codebehind="DashSummary.ascx.cs" %>
<asp:Repeater ID="rpDash" runat="server" OnItemCommand="CtrlItemCommand" ></asp:Repeater>
<asp:Repeater ID="rpDataH" runat="server" OnItemCommand="CtrlItemCommand" ></asp:Repeater>
<asp:Repeater ID="rpData" runat="server" OnItemCommand="CtrlItemCommand" ></asp:Repeater>
<asp:Repeater ID="rpDataF" runat="server" OnItemCommand="CtrlItemCommand" ></asp:Repeater>
<asp:PlaceHolder ID="phData" runat="server"></asp:PlaceHolder>


