/* Copyright 2013-present MongoDB Inc.
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
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver.Core.Authentication;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Configuration
{
    /// <summary>
    /// Represents settings for a connection.
    /// </summary>
    public class ConnectionSettings
    {
        // fields
        private readonly string _applicationName;
#pragma warning disable CS0618 // Type or member is obsolete
        private readonly IReadOnlyList<IAuthenticatorFactory> _authenticatorFactories;
#pragma warning restore CS0618 // Use auto property
        private readonly IReadOnlyList<CompressorConfiguration> _compressors;
        private readonly LibraryInfo _libraryInfo;
        private readonly bool _loadBalanced;
        private readonly TimeSpan _maxIdleTime;
        private readonly TimeSpan _maxLifeTime;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionSettings" /> class.
        /// </summary>
        /// <param name="authenticatorFactories">The authenticator factories.</param>
        /// <param name="compressors">The compressors.</param>
        /// <param name="libraryInfo">The library information.</param>
        /// <param name="loadBalanced">Whether the load balanced mode is enabled.</param>
        /// <param name="maxIdleTime">The maximum idle time.</param>
        /// <param name="maxLifeTime">The maximum life time.</param>
        /// <param name="applicationName">The application name.</param>
        public ConnectionSettings(
#pragma warning disable CS0618 // Type or member is obsolete
            Optional<IEnumerable<IAuthenticatorFactory>> authenticatorFactories = default,
#pragma warning restore CS0618 // Type or member is obsolete
            Optional<IEnumerable<CompressorConfiguration>> compressors = default(Optional<IEnumerable<CompressorConfiguration>>),
            Optional<LibraryInfo> libraryInfo = default,
            Optional<bool> loadBalanced = default,
            Optional<TimeSpan> maxIdleTime = default(Optional<TimeSpan>),
            Optional<TimeSpan> maxLifeTime = default(Optional<TimeSpan>),
            Optional<string> applicationName = default(Optional<string>))
        {
#pragma warning disable CS0618 // Type or member is obsolete
            _authenticatorFactories = Ensure.IsNotNull(authenticatorFactories.WithDefault(Enumerable.Empty<IAuthenticatorFactory>()), nameof(authenticatorFactories)).ToList().AsReadOnly();
#pragma warning restore CS0618 // Type or member is obsolete
            _compressors = Ensure.IsNotNull(compressors.WithDefault(Enumerable.Empty<CompressorConfiguration>()), nameof(compressors)).ToList();
            _libraryInfo = libraryInfo.WithDefault(null);
            _loadBalanced = loadBalanced.WithDefault(false);
            _maxIdleTime = Ensure.IsGreaterThanZero(maxIdleTime.WithDefault(TimeSpan.FromMinutes(10)), "maxIdleTime");
            _maxLifeTime = Ensure.IsGreaterThanZero(maxLifeTime.WithDefault(TimeSpan.FromMinutes(30)), "maxLifeTime");
            _applicationName = ApplicationNameHelper.EnsureApplicationNameIsValid(applicationName.WithDefault(null), nameof(applicationName));
        }

        // properties
        /// <summary>
        /// Gets the name of the application.
        /// </summary>
        /// <value>
        /// The name of the application.
        /// </value>
        public string ApplicationName
        {
            get { return _applicationName; }
        }

        /// <summary>
        /// Gets the authenticator factories.
        /// </summary>
        /// <value>
        /// The authenticator factories.
        /// </value>
#pragma warning disable CS0618 // Type or member is obsolete
        public IReadOnlyList<IAuthenticatorFactory> AuthenticatorFactories
        {
            get { return _authenticatorFactories; }
        }
#pragma warning restore CS0618 // Type or member is obsolete
        /// <summary>
        /// Gets the compressors.
        /// </summary>
        /// <value>
        /// The compressors.
        /// </value>
        public IReadOnlyList<CompressorConfiguration> Compressors
        {
            get { return _compressors; }
        }

        /// <summary>
        /// Information about a library using the .NET driver.
        /// </summary>
        public LibraryInfo LibraryInfo
        {
            get { return _libraryInfo; }
        }

        /// <summary>
        /// Whether the load balanced mode is enabled.
        /// </summary>
        public bool LoadBalanced
        {
            get { return _loadBalanced; }
        }

        /// <summary>
        /// Gets the maximum idle time.
        /// </summary>
        /// <value>
        /// The maximum idle time.
        /// </value>
        public TimeSpan MaxIdleTime
        {
            get { return _maxIdleTime; }
        }

        /// <summary>
        /// Gets the maximum life time.
        /// </summary>
        /// <value>
        /// The maximum life time.
        /// </value>
        public TimeSpan MaxLifeTime
        {
            get { return _maxLifeTime; }
        }

        // methods
        /// <summary>
        /// Returns a new ConnectionSettings instance with some settings changed.
        /// </summary>
        /// <param name="authenticatorFactories">The authenticator factories.</param>
        /// <param name="compressors">The compressors.</param>
        /// <param name="libraryInfo">The library information.</param>
        /// <param name="loadBalanced">Whether the load balanced mode is enabled.</param>
        /// <param name="maxIdleTime">The maximum idle time.</param>
        /// <param name="maxLifeTime">The maximum life time.</param>
        /// <param name="applicationName">The application name.</param>
        /// <returns>A new ConnectionSettings instance.</returns>
        public ConnectionSettings With(
#pragma warning disable CS0618 // Type or member is obsolete
            Optional<IEnumerable<IAuthenticatorFactory>> authenticatorFactories = default,
#pragma warning restore CS0618 // Type or member is obsolete
            Optional<IEnumerable<CompressorConfiguration>> compressors = default(Optional<IEnumerable<CompressorConfiguration>>),
            Optional<LibraryInfo> libraryInfo = default,
            Optional<bool> loadBalanced = default,
            Optional<TimeSpan> maxIdleTime = default(Optional<TimeSpan>),
            Optional<TimeSpan> maxLifeTime = default(Optional<TimeSpan>),
            Optional<string> applicationName = default(Optional<string>))
        {
            return new ConnectionSettings(
                authenticatorFactories: Optional.Enumerable(authenticatorFactories.WithDefault(_authenticatorFactories)),
                compressors: Optional.Enumerable(compressors.WithDefault(_compressors)),
                libraryInfo: libraryInfo.WithDefault(_libraryInfo),
                loadBalanced: loadBalanced.WithDefault(_loadBalanced),
                maxIdleTime: maxIdleTime.WithDefault(_maxIdleTime),
                maxLifeTime: maxLifeTime.WithDefault(_maxLifeTime),
                applicationName: applicationName.WithDefault(_applicationName));
        }
    }
}
