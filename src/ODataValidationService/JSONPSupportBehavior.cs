// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.ValidationService
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.Text;
    using System.Xml;

    /// <summary>Simply apply this attribute to a DataService-derived class to get JSONP support in that service</summary>
    [AttributeUsage(AttributeTargets.Class)]
    [SuppressMessage("Microsoft.Naming", "CA1709: Correct the casing of 'JSONP'", Justification = "JSONP is protocol name")]
    public sealed class JSONPSupportBehaviorAttribute : Attribute, IServiceBehavior
    {
        #region IServiceBehavior Members

        /// <summary>Add the binding parameters</summary>
        /// <param name="serviceDescription">ServiceDescription</param>
        /// <param name="serviceHostBase">ServiceHostBase</param>
        /// <param name="endpoints">Collection of endpoints</param>
        /// <param name="bindingParameters">Collection of binding parameters</param>
        void IServiceBehavior.AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
        }

        /// <summary>Provides the ability to change run-time property values or insert custom extension objects such as error handlers, message or parameter interceptors, security extensions, and other custom extension objects.</summary>
        /// <param name="serviceDescription">ServiceDescription</param>
        /// <param name="serviceHostBase">ServiceHostBase</param>
        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            if (serviceHostBase != null)
            {
                foreach (ChannelDispatcher cd in serviceHostBase.ChannelDispatchers)
                {
                    foreach (EndpointDispatcher ed in cd.Endpoints)
                    {
                        ed.DispatchRuntime.MessageInspectors.Add(new JSONPSupportInspector());
                    }
                }
            }
        }

        /// <summary>Provides the ability to inspect the service host and the service description to confirm that the service can run successfully. </summary>
        /// <param name="serviceDescription">ServiceDescription</param>
        /// <param name="serviceHostBase">ServiceHostBase</param>
        void IServiceBehavior.Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

        #endregion
    }

    /// <summary>Modification of inbound and outbound messages</summary>
    internal class JSONPSupportInspector : IDispatchMessageInspector
    {
        /// <summary>
        /// Assume utf-8, note that Data Services supports charset negotation, so this needs to be more
        /// sophisticated (and per-request) if clients will use multiple charsets
        /// </summary>
        private static Encoding encoding = Encoding.UTF8;

        #region IDispatchMessageInspector Members

        /// <summary>Modification of inbound messages</summary>
        /// <param name="request">Message</param>
        /// <param name="channel">IClientChannel</param>
        /// <param name="instanceContext">InstanceContext</param>
        /// <returns>object</returns>
        public object AfterReceiveRequest(ref System.ServiceModel.Channels.Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            if (request != null && request.Properties.ContainsKey("UriTemplateMatchResults"))
            {
                HttpRequestMessageProperty httpmsg = (HttpRequestMessageProperty)request.Properties[HttpRequestMessageProperty.Name];
                UriTemplateMatch match = (UriTemplateMatch)request.Properties["UriTemplateMatchResults"];

                string format = match.QueryParameters["$format"];
                if ("json".Equals(format, StringComparison.OrdinalIgnoreCase))
                {
                    // strip out $format from the query options to avoid an error
                    // due to use of a reserved option (starts with "$")
                    match.QueryParameters.Remove("$format");

                    // replace the Accept header so that the Data Services runtime 
                    // assumes the client asked for a JSON representation
                    httpmsg.Headers["Accept"] = "application/json";

                    string callback = match.QueryParameters["$callback"];
                    if (!string.IsNullOrEmpty(callback))
                    {
                        match.QueryParameters.Remove("$callback");
                        return callback;
                    }
                }
            }

            return null;
        }

        /// <summary>Modification of outbound messages</summary>
        /// <param name="reply">Message</param>
        /// <param name="correlationState">object</param>
        [SuppressMessage("Microsoft.Performance", "CA1800: Cache casting of 'correlationState'", Justification = "api is expecting object as parameter")]
        public void BeforeSendReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
        {
            if (reply == null)
            {
                throw new ArgumentNullException("reply");
            }

            if (correlationState != null && correlationState is string)
            {
                // if we have a JSONP callback then buffer the response, wrap it with the callback call and then re-create the response message
                string callback = (string)correlationState;

                XmlDictionaryReader reader = reply.GetReaderAtBodyContents();
                reader.ReadStartElement();
                string content = JSONPSupportInspector.encoding.GetString(reader.ReadContentAsBase64());

                content = callback + "(" + content + ");";

                using (Message newreply = Message.CreateMessage(MessageVersion.None, "", new Writer(content)))
                {
                    newreply.Properties.CopyProperties(reply.Properties);

                    reply = newreply;

                    // change response content type to text/javascript if the JSON (only done when wrapped in a callback)
                    var replyProperties = (HttpResponseMessageProperty)reply.Properties[HttpResponseMessageProperty.Name];
                    replyProperties.Headers["Content-Type"] = replyProperties.Headers["Content-Type"].Replace("application/json", "text/javascript");
                }
            }
        }

        #endregion

        /// <summary>Writer of the message body</summary>
        private class Writer : BodyWriter
        {
            /// <summary>content</summary>
            private string content;

            /// <summary>Constructor</summary>
            /// <param name="content">content</param>
            public Writer(string content) : base(false)
            {
                this.content = content;
            }

            /// <summary>Override of the OnWriteBodyContents</summary>
            /// <param name="writer">XmlDictionaryWriter</param>
            protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
            {
                if (writer != null)
                {
                    writer.WriteStartElement("Binary");
                    byte[] buffer = JSONPSupportInspector.encoding.GetBytes(this.content);
                    writer.WriteBase64(buffer, 0, buffer.Length);
                    writer.WriteEndElement();
                }
            }
        }
    }
}
