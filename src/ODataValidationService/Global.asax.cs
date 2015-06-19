// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.ValidationService
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.ServiceModel.Activation;
    using System.Web;
    using System.Web.Routing;
    using System.Web.Security;
    using System.Web.SessionState;
    
    /// <summary>TBD</summary>
    public class Global : System.Web.HttpApplication
    {
        /// <summary>Initializes a new instance of the ServiceRoute class</summary>
        [SuppressMessage("Microsoft.Performance", "CA1811: Should have upstream public or protected callers", Justification = "called by private event handler")]
        private static void RegisterRoutes()
        {
            DataServiceHostFactory factory = new DataServiceHostFactory();
            RouteTable.Routes.Add(
                new ServiceRoute(ODataValidator.ServiceName, factory, typeof(ODataValidator)));
        }

        /// <summary>Called when the first resource (such as a page) in an ASP.NET application is requested.</summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        [SuppressMessage("Microsoft.Performance", "CA1811: Should have upstream public or protected callers", Justification = "event handler")]
        private void Application_Start(object sender, EventArgs e)
        {
            Global.RegisterRoutes();
        }
    }
}
