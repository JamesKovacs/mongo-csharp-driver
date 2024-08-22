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
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents target server versions when translation Expressions to MQL.
    /// </summary>
    /// <note>Only major/minor versions are represented.</note>
    public enum TargetServerVersion
    {
        /// <summary>
        /// Server version 2.6.
        /// </summary>
        Server26 = 2,

        /// <summary>
        /// Server version 3.0.
        /// </summary>
        Server30 = 3,

        /// <summary>
        /// Server version 3.2.
        /// </summary>
        Server32 = 4,

        /// <summary>
        /// Server version 2.6.
        /// </summary>
        Server34 = 5,

        /// <summary>
        /// Server version 3.6.
        /// </summary>
        Server36 = 6,

        /// <summary>
        /// Server version 4.0.
        /// </summary>
        Server40 = 7,

        /// <summary>
        /// Server version 4.2.
        /// </summary>
        Server42 = 8,

        /// <summary>
        /// Server version 4.4.
        /// </summary>
        Server44 = 9,

        /// <summary>
        /// Server version 4.7.
        /// </summary>
        Server47 = 10,

        /// <summary>
        /// Server version 4.8.
        /// </summary>
        Server48 = 11,

        /// <summary>
        /// Server version 4.9.
        /// </summary>
        Server49 = 12,

        /// <summary>
        /// Server version 5.0.
        /// </summary>
        Server50 = 13,

        /// <summary>
        /// Server version 5.1.
        /// </summary>
        Server51 = 14,

        /// <summary>
        /// Server version 5.2.
        /// </summary>
        Server52 = 15,

        /// <summary>
        /// Server version 5.3.
        /// </summary>
        Server53 = 16,

        /// <summary>
        /// Server version 6.0.
        /// </summary>
        Server60 = 17,

        /// <summary>
        /// Server version 6.1.
        /// </summary>
        Server61 = 18,

        /// <summary>
        /// Server version 6.2.
        /// </summary>
        Server62 = 19,

        /// <summary>
        /// Server version 6.3.
        /// </summary>
        Server63 = 20,

        /// <summary>
        /// Server version 7.0.
        /// </summary>
        Server70 = 21,

        /// <summary>
        /// Server version 7.1.
        /// </summary>
        Server71 = 22,

        /// <summary>
        /// Server version 7.2.
        /// </summary>
        Server72 = 23,

        /// <summary>
        /// Server version 7.3.
        /// </summary>
        Server73 = 24,

        /// <summary>
        /// Server version 8.0.
        /// </summary>
        Server80 = 25

        // note: when adding new enum values update the ToWireVersion method below
    }

    internal static class ServerVersionExtensions
    {
        public static int ToWireVersion(this TargetServerVersion? serverVersion)
        {
            return serverVersion switch
            {
                null => WireVersion.Server40,
                TargetServerVersion.Server26 => WireVersion.Server26,
                TargetServerVersion.Server30 => WireVersion.Server30,
                TargetServerVersion.Server32 => WireVersion.Server32,
                TargetServerVersion.Server34 => WireVersion.Server34,
                TargetServerVersion.Server36 => WireVersion.Server36,
                TargetServerVersion.Server40 => WireVersion.Server40,
                TargetServerVersion.Server42 => WireVersion.Server42,
                TargetServerVersion.Server44 => WireVersion.Server44,
                TargetServerVersion.Server47 => WireVersion.Server47,
                TargetServerVersion.Server48 => WireVersion.Server48,
                TargetServerVersion.Server49 => WireVersion.Server49,
                TargetServerVersion.Server50 => WireVersion.Server50,
                TargetServerVersion.Server51 => WireVersion.Server51,
                TargetServerVersion.Server52 => WireVersion.Server52,
                TargetServerVersion.Server53 => WireVersion.Server53,
                TargetServerVersion.Server60 => WireVersion.Server60,
                TargetServerVersion.Server61 => WireVersion.Server61,
                TargetServerVersion.Server62 => WireVersion.Server62,
                TargetServerVersion.Server63 => WireVersion.Server63,
                TargetServerVersion.Server70 => WireVersion.Server70,
                TargetServerVersion.Server71 => WireVersion.Server71,
                TargetServerVersion.Server72 => WireVersion.Server72,
                TargetServerVersion.Server73 => WireVersion.Server73,
                TargetServerVersion.Server80 => WireVersion.Server80,
                _ => throw new ArgumentException($"Invalid server version: {serverVersion}.", nameof(serverVersion))
            };
        }
    }
}
