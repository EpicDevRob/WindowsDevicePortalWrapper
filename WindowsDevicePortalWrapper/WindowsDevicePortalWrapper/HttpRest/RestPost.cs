﻿//----------------------------------------------------------------------------------------------
// <copyright file="RestPost.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsDevicePortal
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    /// <content>
    /// HTTP POST Wrapper
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// Submits the http post request to the specified uri.
        /// </summary>
        /// <param name="uri">The uri to which the post request will be issued.</param>
        /// <returns>Task tracking the completion of the POST request</returns>
        private async Task Post(Uri uri)
        {
            WebRequestHandler handler = new WebRequestHandler();
            handler.UseDefaultCredentials = false;
            handler.Credentials = deviceConnection.Credentials;
            handler.ServerCertificateValidationCallback = ServerCertificateValidation;

            using (HttpClient client = new HttpClient(handler))
            {
                Task<HttpResponseMessage> postTask = client.PostAsync(uri, null);
                await postTask.ConfigureAwait(false);
                postTask.Wait();

                using (HttpResponseMessage response = postTask.Result)
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new DevicePortalException(response);
                    }
                }
            }
        }

        /// <summary>
        /// Calls the specified API with the provided payload.
        /// </summary>
        /// <param name="apiPath">The relative portion of the uri path that specifies the API to call.</param>
        /// <param name="payload">The query string portion of the uri path that provides the parameterized data.</param>
        /// <returns>Task tracking the POST completion.</returns>
        private async Task Post(
            string apiPath,
            string payload = null)
        {
            Uri uri = Utilities.BuildEndpoint(
                deviceConnection.Connection,
                apiPath, 
                payload);

            await this.Post(uri);
        }
    }
}
