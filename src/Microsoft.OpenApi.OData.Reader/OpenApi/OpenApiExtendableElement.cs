//---------------------------------------------------------------------
// <copyright file="IOpenApiExtendable.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.OData.OpenAPI
{
    internal abstract class OpenApiExtendableElement : OpenApiElement, IOpenApiExtensible
    {
        private IList<OpenApiExtension> _extensions;

        /// <summary>
        /// This object MAY be extended with Specification Extensions.
        /// </summary>
        public IList<OpenApiExtension> Extensions
        {
            get
            {
                return _extensions;
            }
        }

        /// <summary>
        /// Add a specification extension into tag.
        /// </summary>
        /// <param name="item">The specification extension.</param>
        public void Add(OpenApiExtension item)
        {
            if (item == null)
            {
                return;
            }

            if (_extensions == null)
            {
                _extensions = new List<OpenApiExtension>();
            }

            _extensions.Add(item);
        }

        /// <summary>
        /// Write specification extensions object.
        /// </summary>
        /// <param name="writer">The Open API Writer.</param>
        public override void Write(IOpenApiWriter writer)
        {
            if (writer == null)
            {
                throw Error.ArgumentNull("writer");
            }

            // specification extensions
            writer.WriteDictionary(Extensions);
        }
    }
}
