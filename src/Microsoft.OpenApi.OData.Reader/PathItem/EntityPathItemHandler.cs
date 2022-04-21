// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.OpenApi.OData.PathItem
{
    /// <summary>
    /// Path item handler for single Entity.
    /// </summary>
    internal class EntityPathItemHandler : EntitySetPathItemHandler
    {
        /// <inheritdoc/>
        protected override ODataPathKind HandleKind => ODataPathKind.Entity;

        /// <inheritdoc/>
        protected override void SetOperations(OpenApiPathItem item)
        {
            ReadRestrictionsType read = Context.Model.GetRecord<ReadRestrictionsType>(EntitySet);
            if (read == null ||
               (read.ReadByKeyRestrictions == null && read.IsReadable) ||
               (read.ReadByKeyRestrictions != null && read.ReadByKeyRestrictions.IsReadable))
            {
                // If we don't have Read by key read restriction, we should check the set read restrction.
                AddOperation(item, OperationType.Get);
            }

            UpdateRestrictionsType update = Context.Model.GetRecord<UpdateRestrictionsType>(EntitySet);
            if (update == null || update.IsUpdatable)
            {
                if (update != null && update.IsUpdateMethodPut)
                {
                    AddOperation(item, OperationType.Put);
                }
                else
                {
                    AddOperation(item, OperationType.Patch);
                }
            }

            DeleteRestrictionsType delete = Context.Model.GetRecord<DeleteRestrictionsType>(EntitySet);
            if (delete == null || delete.IsDeletable)
            {
                AddOperation(item, OperationType.Delete);
            }
        }

        protected override void SetExtensions(OpenApiPathItem pathItem)
        {
            base.SetExtensions(pathItem);

            // Retrieve custom attributes, if present
            Dictionary<string, string> atrributesValueMap = 
                Context.Model.GetCustomXMLAtrributesValueMapping(EntitySet.EntityType(), Context.Settings.CustomXMLAttributesMapping);

            if (atrributesValueMap?.Any() ?? false)
            {
                foreach (var item in atrributesValueMap)
                {
                    if (!pathItem.Extensions.ContainsKey(item.Key))
                    {
                        pathItem.Extensions.Add(item.Key, new OpenApiString(item.Value));
                    }                    
                }
            }
        }
    }
}
