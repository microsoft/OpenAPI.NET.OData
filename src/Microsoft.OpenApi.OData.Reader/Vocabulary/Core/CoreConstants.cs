// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -----------------------------------------------------------

namespace Microsoft.OpenApi.OData.Vocabulary.Core
{
    /// <summary>
    /// Constants for the Core vocabulary
    /// </summary>
    internal class CoreConstants
    {
        /// <summary>
        /// Org.OData.Core.V1.Links
        /// </summary>
        public const string Links = "Org.OData.Core.V1.Links";

        /// <summary>
        /// Org.OData.Core.V1.Revisions
        /// </summary>
        public const string Revisions = "Org.OData.Core.V1.Revisions";

        /// <summary>
        /// Link relation types.
        /// </summary>
        public static class LinkRel
        {
            /// <summary>
            ///  Identifies external documentation for a GET operation on an entity.
            /// </summary>
            public const string ReadByKey = "https://graph.microsoft.com/rels/docs/get";

            /// <summary>
            /// Identifies external documentation for a GET operation on an entity set.
            /// </summary>
            public const string List = "https://graph.microsoft.com/rels/docs/list";

            /// <summary>
            /// Identifies external documentation for a POST operation.
            /// </summary>
            public const string Create = "https://graph.microsoft.com/rels/docs/create";

            /// <summary>
            ///  Identifies external documentation for a PATCH operation.
            /// </summary>
            public const string Update = "https://graph.microsoft.com/rels/docs/update";

            /// <summary>
            /// Identifies external documentation for a DELETE operation.
            /// </summary>
            public const string Delete = "https://graph.microsoft.com/rels/docs/delete";

            /// <summary>
            /// Identifies external documentation for a function.
            /// </summary>
            public const string Function = "https://graph.microsoft.com/rels/docs/function";

            /// <summary>
            /// Identifies external documentation for an action.
            /// </summary>
            public const string Action = "https://graph.microsoft.com/rels/docs/action";
        }

        public const string ExternalDocsDescription = "Find more info here";
    }
}
