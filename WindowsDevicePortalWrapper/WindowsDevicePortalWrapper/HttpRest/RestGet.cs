﻿//----------------------------------------------------------------------------------------------
// <copyright file="RestGet.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsDevicePortal
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Runtime.Serialization.Json;
    using System.Threading.Tasks;

    /// <content>
    /// HTTP GET Wrapper
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// Submits the http get request to the specified uri.
        /// </summary>
        /// <param name="uri">The uri to which the get request will be issued.</param>
        /// /// <param name="validateCertificate">Whether the certificate should be validated.</param>
        /// <returns>Response data as a stream.</returns>
        private async Task<Stream> Get(Uri uri, bool validateCertificate = true)
        {
            MemoryStream dataStream = null;

            WebRequestHandler handler = new WebRequestHandler();
            handler.UseDefaultCredentials = false;
            handler.Credentials = deviceConnection.Credentials;
            if (validateCertificate)
            {
                handler.ServerCertificateValidationCallback = ServerCertificateValidation;
            }
            else
            {
                handler.ServerCertificateValidationCallback = ServerCertificateNonValidation;
            }

            using (HttpClient client = new HttpClient(handler))
            {
                Task<HttpResponseMessage> getTask = client.GetAsync(uri);
                await getTask.ConfigureAwait(false);
                getTask.Wait();

                using (HttpResponseMessage response = getTask.Result)
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new DevicePortalException(response);
                    }

                    using (HttpContent content = response.Content)
                    {
                        dataStream = new MemoryStream();

                        Task copyTask = content.CopyToAsync(dataStream);
                        await copyTask.ConfigureAwait(false);
                        copyTask.Wait();

                        // Ensure we return with the stream pointed at the origin.
                        dataStream.Position = 0;
                    }
                }
            }

            return dataStream;
        }

        /// <summary>
        /// Calls the specified API with the provided payload.
        /// </summary>
        /// <typeparam name="T">Return type for the GET call</typeparam>
        /// <param name="apiPath">The relative portion of the uri path that specifies the API to call.</param>
        /// <param name="payload">The query string portion of the uri path that provides the parameterized data.</param>
        /// <returns>An object of the specified type containing the data returned by the request.</returns>
        private async Task<T> Get<T>(
            string apiPath,
            string payload = null) where T : new()
        {
            T data = default(T);
            
            Uri uri = Utilities.BuildEndpoint(
                deviceConnection.Connection,
                apiPath, 
                payload);

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));

            using (Stream dataStream = await this.Get(uri))
            {
                if (dataStream != null)
                {
                    object response = serializer.ReadObject(dataStream);
                    data = (T)response;
                }
            }

            return data;
        }
    }
}
