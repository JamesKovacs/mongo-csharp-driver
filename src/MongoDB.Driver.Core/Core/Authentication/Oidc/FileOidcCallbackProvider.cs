/* Copyright 2010-present MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.IO;
#if !NETSTANDARD2_1_OR_GREATER
using System.Text;
#endif
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Authentication.Oidc
{
    internal class FileOidcCallbackProvider : IOidcCallbackProvider
    {
        #region static
        public static FileOidcCallbackProvider CreateFromEnvironmentVariable(string environmentVariableName, IEnvironmentVariableProvider environmentVariableProvider)
        {
            var tokenPath = Ensure.IsNotNull(environmentVariableProvider, nameof(environmentVariableProvider)).GetEnvironmentVariable(environmentVariableName);
            return new FileOidcCallbackProvider(tokenPath);
        }
        #endregion

        private readonly string _path;

        public FileOidcCallbackProvider(string path)
        {
            _path = Ensure.IsNotNullOrEmpty(path, nameof(path));
        }

        public OidcCallbackResponse GetResponse(OidcCallbackParameters parameters, CancellationToken cancellationToken)
        {
            var accessToken = File.ReadAllText(_path);
            return CreateOidcCallbackResponse(accessToken);
        }

        public async Task<OidcCallbackResponse> GetResponseAsync(OidcCallbackParameters parameters, CancellationToken cancellationToken)
        {
#if NETSTANDARD2_1_OR_GREATER
            var accessToken = await File.ReadAllTextAsync(_path, cancellationToken).ConfigureAwait(false);
#else
            using var streamReader = new StreamReader(_path, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            var accessToken = await streamReader.ReadToEndAsync().ConfigureAwait(false); // no support for cancellationToken
#endif
            return CreateOidcCallbackResponse(accessToken);
        }

        private static OidcCallbackResponse CreateOidcCallbackResponse(string accessToken)
            => new OidcCallbackResponse(accessToken, expiresIn: null);
    }
}
