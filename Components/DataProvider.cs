using System.Collections.Generic;
using System.Data;
using System;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Framework.Providers;
using NBrightDNN;

namespace Nevoweb.DNN.NBrightBuy.Components
{

    public abstract class NBrigthBuyDataCtrlInterface : DataCtrlInterface
    {
        public abstract List<NBrightInfo> GetListCustom(int portalId, int moduleId, string SPROCname, int pageNumber = 0, string lang = "", string extraParam = "");
    }

    public abstract class DataCtrlInterfaceNBrightBuy : NBrigthBuyDataCtrlInterface
    {
        public abstract List<NBrightInfo> GetDnnUserProductClient(int portalId, int productid);
        public abstract List<NBrightInfo> GetDnnUsers(int portalId, string sqlSearchFilter = "", int returnLimit = 0, int pageNumber = 0, int pageSize = 0, int recordCount = 0);
        public abstract int GetDnnUsersCount(int portalId, string sqlSearchFilter = "");
        public abstract NBrightInfo GetData(int itemId);
        public abstract NBrightInfo GetDataLang(int parentItemId, string lang);
        public abstract List<PropertyByProductInfo> GetPropertyListByProduct(int PortalId, int ModuleId, string TypeCode, string SQLSearchFilter, string SQLOrderBy, int ReturnLimit, int PageNumber, int PageSize, int RecordCount, string TypeCodeLang, string lang);
		public abstract List<PropertyByProductInfo> GetPropertyListByProductIds(string itemIds);
	}
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// An abstract class for the data access layer
    /// </summary>
    /// -----------------------------------------------------------------------------
    public abstract class DataProvider
	{

		#region Shared/Static Methods

		private static DataProvider provider;

		// return the provider
		public static DataProvider Instance()
		{
			if (provider == null)
			{
                const string assembly = "Nevoweb.DNN.NBrightBuy.Components.SqlDataprovider.SqlDataprovider,NBrightBuy";
				Type objectType = Type.GetType(assembly, true, true);

				provider = (DataProvider)Activator.CreateInstance(objectType);
				DataCache.SetCache(objectType.FullName, provider);
			}

			return provider;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Not returning class state information")]
		public static IDbConnection GetConnection()
		{
			const string providerType = "data";
			ProviderConfiguration _providerConfiguration = ProviderConfiguration.GetProviderConfiguration(providerType);

			Provider objProvider = ((Provider)_providerConfiguration.Providers[_providerConfiguration.DefaultProvider]);
			string _connectionString;
			if (!String.IsNullOrEmpty(objProvider.Attributes["connectionStringName"]) && !String.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings[objProvider.Attributes["connectionStringName"]]))
			{
				_connectionString = System.Configuration.ConfigurationManager.AppSettings[objProvider.Attributes["connectionStringName"]];
			}
			else
			{
				_connectionString = objProvider.Attributes["connectionString"];
			}

			IDbConnection newConnection = new System.Data.SqlClient.SqlConnection();
			newConnection.ConnectionString = _connectionString.ToString();
			newConnection.Open();
			return newConnection;
		}

		#endregion


		#region "NBrightBuy Abstract Methods"

        public abstract IDataReader GetListCustom(int portalId, int moduleId, string SPROCname, int pageNumber = 0, string lang = "", string extraParam = "");

        public abstract IDataReader GetList(int PortalId, int ModuleId, string TypeCode, string SQLSearchFilter, string SQLOrderBy, int ReturnLimit, int PageNumber, int PageSize, int RecordCount, string TypeCodeLang, string lang);
        public abstract IDataReader GetPropertyListByProduct(int PortalId, int ModuleId, string TypeCode, string SQLSearchFilter, string SQLOrderBy, int ReturnLimit, int PageNumber, int PageSize, int RecordCount, string TypeCodeLang, string lang);
		public abstract IDataReader GetPropertyListByProductIds(string itemIds);
		public abstract int GetListCount(int PortalId, int ModuleId, string TypeCode, string SQLSearchFilter, string TypeCodeLang, string lang);
		public abstract IDataReader GetListCountWithProperties(int PortalId, int ModuleId, string TypeCode, string SQLSearchFilter, string TypeCodeLang, string lang);
		public abstract IDataReader Get(int ItemID, string TypeCodeLang, string lang);
        public abstract int Update(int ItemId, int PortalId, int ModuleId, String TypeCode, String XMLData, String GUIDKey, DateTime ModifiedDate, String TextData, int XrefItemId, int ParentItemId, int UserId, string lang);
		public abstract void Delete(int ItemID);
		public abstract void CleanData();
        public abstract string ExecSql(string commandText);
        public abstract string GetSqlxml(string commandText);
        public abstract IDataReader GetDnnUsers(int portalId, string sqlSearchFilter = "", int returnLimit = 0, int pageNumber = 0, int pageSize = 0, int recordCount = 0);
        public abstract IDataReader GetDnnUserProductClient(int portalId, int productid);
        public abstract int GetDnnUsersCount(int portalId, string sqlSearchFilter = "");
        public abstract IDataReader GetData(int itemId);
        public abstract IDataReader GetDataLang(int parentItemId, string lang);

		#endregion


	}

}