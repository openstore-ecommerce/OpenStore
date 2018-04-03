using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Reflection;
using DotNetNuke;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;

namespace Nevoweb.DNN.NBrightBuy.Components
{

        /// <summary>
        /// Summary description for RequestFormWrapper.
        /// </summary>
        public abstract class RequestFormWrapper
        {
            protected RequestFormWrapper()
            {
            }

            protected RequestFormWrapper(NameValueCollection requestForm)
            {
                LoadRequestForm(requestForm);
            }


            /// <summary>
            /// Parses the Request Form parameters and sets properties, if they exist,
            /// in the derived object.
            /// </summary>
            /// <param name="requestForm"></param>
            public void LoadRequestForm(NameValueCollection requestForm)
            {
                // Iterate thru all properties for this type
                PropertyInfo[] propertyList = this.GetType().GetProperties();
                foreach (PropertyInfo property in propertyList)
                {
                    // Do we have a value for this property?
                    string val = requestForm[property.Name];
                    if (val != null)
                    {
                        object objValue = null;

                        try
                        {
                            // Cast to the appropriate type
                            switch (property.PropertyType.Name)
                            {
                                case "String":
                                    objValue = (object)val;
                                    break;
                                case "Int32":
                                    objValue = (object)Convert.ToInt32(val);
                                    break;
                                case "Boolean":
                                    objValue = (object)Convert.ToBoolean(val);
                                    break;
                                case "Decimal":
                                    objValue = (object)Convert.ToDecimal(val);
                                    break;
                            }
                        }
                        catch
                        {
                            //Cast failed - Skip this property
                        }

                        // Set the value
                        if (objValue != null)
                        {
                            property.SetValue(this, objValue, null);
                        }
                    }
                }
            }

        }

}
