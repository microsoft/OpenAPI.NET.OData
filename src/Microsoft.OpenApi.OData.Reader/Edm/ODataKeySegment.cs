// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;

namespace Microsoft.OpenApi.OData.Edm
{
    /// <summary>
    /// The key segment.
    /// </summary>
    public class ODataKeySegment : ODataSegment
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataKeySegment"/> class.
        /// </summary>
        /// <param name="entityType">The entity type contains the keys.</param>
        public ODataKeySegment(IEdmEntityType entityType)
        {
            EntityType = entityType ?? throw Error.ArgumentNull(nameof(entityType));
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ODataKeySegment"/> class.
        /// </summary>
        /// <param name="entityType">The entity type contains the keys.</param>
        /// <param name="keyMappings">The key/template mappings.</param>
        public ODataKeySegment(IEdmEntityType entityType, IDictionary<string, string> keyMappings)
        {
            EntityType = entityType ?? throw Error.ArgumentNull(nameof(entityType));
            KeyMappings = keyMappings ?? throw Error.ArgumentNull(nameof(keyMappings));
        }

        /// <summary>
        /// Is true if key segment is alternate key
        /// </summary>
        public bool IsAlternateKey { get; set; } = false;

        /// <summary>
        /// Gets the key/template mappings.
        /// </summary>
        public IDictionary<string, string> KeyMappings { get; }

        /// <inheritdoc />
        public override IEdmEntityType EntityType { get; }

        /// <inheritdoc />
        public override ODataSegmentKind Kind => ODataSegmentKind.Key;

        /// <inheritdoc />
        public override string Identifier
        {
            get
            {
                IList<string> keys = IsAlternateKey ? 
                    KeyMappings.Values.ToList() : 
                    EntityType.Key().Select(static x => x.Name).ToList();

                return string.Join(",", keys);
            }
        }

        /// <inheritdoc />
		public override IEnumerable<IEdmVocabularyAnnotatable> GetAnnotables()
		{
			return Enumerable.Empty<IEdmVocabularyAnnotatable>();
		}

		/// <inheritdoc />
		public override string GetPathItemName(OpenApiConvertSettings settings, HashSet<string> parameters)
        {
            Utils.CheckArgumentNull(settings, nameof(settings));

            // Use the output key/template mapping
            if (KeyMappings != null)
            {
                if (KeyMappings.Count == 1 && !IsAlternateKey)
                {
                    var key = KeyMappings.First();
                    return $"{{{key.Value}}}";
                }
                else
                {
                    return string.Join(",", KeyMappings.Select(x => x.Key + "='{" + x.Value + "}'"));
                }
            }

            IList<IEdmStructuralProperty> keys = EntityType.Key().ToList();
            if (keys.Count() == 1)
            {
                string keyName = keys.First().Name;

                if (settings.PrefixEntityTypeNameBeforeKey)
                {
                    string name = Utils.GetUniqueName(EntityType.Name + "-" + keyName, parameters);
                    return "{" + name + "}";
                }
                else
                {
                    string name = Utils.GetUniqueName(keyName, parameters);
                    return "{" + name + "}";
                }
            }
            else
            {
                IList<string> keyStrings = new List<string>();
                foreach (var keyProperty in keys)
                {
                    string name = Utils.GetUniqueName(keyProperty.Name, parameters);
                    var quote = keyProperty.Type.Definition.ShouldPathParameterBeQuoted(settings) ? "'" : string.Empty;
                    keyStrings.Add($"{keyProperty.Name}={quote}{{{name}}}{quote}");
                }

                return string.Join(",", keyStrings);
            }
        }

        internal IDictionary<string, string> GetKeyNameMapping(OpenApiConvertSettings settings, HashSet<string> parameters)
        {
            // Use the output key/template mapping
            IDictionary<string, string> keyNamesMapping = new Dictionary<string, string>();
            if (KeyMappings != null)
            {
                foreach (var keyName in KeyMappings)
                {
                    keyNamesMapping[keyName.Key] = keyName.Value;
                }

                return keyNamesMapping;
            }

            IList<IEdmStructuralProperty> keys = EntityType.Key().ToList();
            if (keys.Count() == 1)
            {
                string keyName = keys.First().Name;

                if (settings.PrefixEntityTypeNameBeforeKey)
                {
                    string name = Utils.GetUniqueName(EntityType.Name + "-" + keyName, parameters);
                    keyNamesMapping[keyName] = name;
                }
                else
                {
                    string name = Utils.GetUniqueName(keyName, parameters);
                    keyNamesMapping[keyName] = name;
                }
            }
            else
            {
                foreach (var keyProperty in keys)
                {
                    string name = Utils.GetUniqueName(keyProperty.Name, parameters);
                    keyNamesMapping[keyProperty.Name] = name;
                }
            }

            return keyNamesMapping;
        }
    }
}