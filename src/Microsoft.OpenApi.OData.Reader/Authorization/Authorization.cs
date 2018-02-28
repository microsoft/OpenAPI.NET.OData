// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.OpenApi.OData.Authorization
{
    /// <summary>
    /// Constant values for Authorization Vocabulary
    /// </summary>
    internal abstract class Authorization
    {
        public string Name { get; set; }

        public string Description { get; set; }
    }

    internal class ApiKey : Authorization
    {
        public string KeyName { get; set; }

        public KeyLocation Location { get; set; }
    }

    internal enum KeyLocation
    {
        Header,

        QueryOption,

        Cookie
    }
}