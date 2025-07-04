﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Generator;
using Microsoft.OpenApi.OData.Operation;
using Microsoft.OpenApi.OData.PathItem;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;
using Microsoft.OpenApi.OData.Vocabulary.Core;

namespace Microsoft.OpenApi.OData.Edm
{
    /// <summary>
    /// Context information for the <see cref="IEdmModel"/>, configuration, etc.
    /// </summary>
    internal class ODataContext
    {
        private IEnumerable<ODataPath>? _allPaths;
        private readonly IODataPathProvider _pathProvider;

        /// <summary>
        /// Initializes a new instance of <see cref="ODataContext"/> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        public ODataContext(IEdmModel model)
            : this(model, new OpenApiConvertSettings())
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ODataContext"/> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="settings">The convert setting.</param>
        public ODataContext(IEdmModel model, OpenApiConvertSettings settings)
        {
            Model = model ?? throw Error.ArgumentNull(nameof(model));
            Settings = settings ?? throw Error.ArgumentNull(nameof(settings));

            EdmSpatialTypeVisitor visitor = new EdmSpatialTypeVisitor();
            visitor.Visit(model);
            IsSpatialTypeUsed = visitor.IsSpatialTypeUsed;

            OperationHandlerProvider = new OperationHandlerProvider();
            PathItemHandlerProvider = new PathItemHandlerProvider();

            // If no path provider, use the default path provider.
            _pathProvider = settings.PathProvider ?? new ODataPathProvider();

            if (settings.EnableKeyAsSegment != null)
            {
                // We have the global setting, use the global setting
                KeyAsSegment = settings.EnableKeyAsSegment.Value;
            }
            else
            {
                KeyAsSegment = false;
                if (model.EntityContainer != null)
                {
                    var keyAsSegment = model.GetBoolean(model.EntityContainer, CapabilitiesConstants.KeyAsSegmentSupported);
                    if (keyAsSegment != null)
                    {
                        KeyAsSegment = keyAsSegment.Value;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the path item handler provider.
        /// </summary>
        public IPathItemHandlerProvider PathItemHandlerProvider { get; }

        /// <summary>
        /// Gets the operation handler provider.
        /// </summary>
        public IOperationHandlerProvider OperationHandlerProvider { get; }

        /// <summary>
        /// Gets the Edm model.
        /// </summary>
        public IEdmModel Model { get; }

        /// <summary>
        /// Gets the Entity Container.
        /// </summary>
        public IEdmEntityContainer EntityContainer
        {
            get
            {
                return Model.EntityContainer;
            }
        }

        /// <summary>
        /// Gets the <see cref="ODataPath"/>s.
        /// </summary>
        public IEnumerable<ODataPath> AllPaths
        {
            get
            {
                if (_allPaths == null)
                {
                    _allPaths = LoadAllODataPaths().ToList();
                }

                return _allPaths;
            }
        }

        /// <summary>
        /// Gets the boolean value indicating to support key as segment.
        /// </summary>
        public bool KeyAsSegment { get; }

        /// <summary>
        /// Gets the value indicating the Edm spatial type used.
        /// </summary>
        public bool IsSpatialTypeUsed { get; private set; }

        /// <summary>
        /// Gets the convert settings.
        /// </summary>
        public OpenApiConvertSettings Settings { get; }

        /// <summary>
        /// Gets all tags.
        /// </summary>
        public HashSet<OpenApiTag>? Tags { get; private set; }

        /// <summary>
        /// Append tag.
        /// </summary>
        /// <param name="tagItem">The tag item.</param>
        internal void AppendTag(OpenApiTag tagItem)
        {
            Tags ??= [];

            if (tagItem.Name is not null && FindTagByName(tagItem.Name) is not null)
            {
                return;
            }

            Tags.Add(tagItem);
        }

        /// <summary>
        /// Find tag by name.
        /// </summary>
        /// <param name="name">The name to lookup the tag.</param>
        /// <returns></returns>
        internal OpenApiTag? FindTagByName(string name)
        {
            Utils.CheckArgumentNullOrEmpty(name, nameof(name));
            return Tags?.FirstOrDefault(t => StringComparer.Ordinal.Equals(t.Name, name));
        }

        /// <summary>
        /// Sets the extension for the existing tag, or create a new tag with the extension.
        /// </summary>
        /// <param name="tagName">The tag name to lookup.</param>
        /// <param name="extensionName">The extension name.</param>
        /// <param name="extensionValue">The extension value to set.</param>
        /// <param name="initialValueFactory">The tag default value factory.</param>
        internal void AddExtensionToTag(string tagName, string extensionName, JsonNodeExtension extensionValue, Func<OpenApiTag> initialValueFactory)
        {
            Utils.CheckArgumentNullOrEmpty(tagName, nameof(tagName));
            Utils.CheckArgumentNullOrEmpty(extensionName, nameof(extensionName));
            Utils.CheckArgumentNull(extensionValue, nameof(extensionValue));
            Utils.CheckArgumentNull(initialValueFactory, nameof(initialValueFactory));

            if (FindTagByName(tagName) is {} foundTag)
            {
                foundTag.Extensions ??= new Dictionary<string, IOpenApiExtension>();
                foundTag.Extensions.TryAdd(extensionName, extensionValue);
            }
            else
            {
                var tag = initialValueFactory();
                tag.Extensions ??= new Dictionary<string, IOpenApiExtension>();
                tag.Extensions.TryAdd(extensionName, extensionValue);
                AppendTag(tag);
            }
        }

        /// <summary>
        /// Gets all OData paths
        /// </summary>
        /// <returns>All acceptable OData path.</returns>
        private IEnumerable<ODataPath> LoadAllODataPaths()
        {
            IEnumerable<ODataPath> allPaths = _pathProvider.GetPaths(Model, Settings);
            foreach (var path in allPaths)
            {
                if ((path.Kind == ODataPathKind.Operation && !Settings.EnableOperationPath) ||
                    (path.Kind == ODataPathKind.OperationImport && !Settings.EnableOperationImportPath) ||
                    ((path.Kind == ODataPathKind.NavigationProperty || path.Kind == ODataPathKind.ComplexProperty) && !Settings.EnableNavigationPropertyPath))
                {
                    continue;
                }

                yield return path;
            }
        }
        internal IEnumerable<RevisionRecord> GetDeprecationInformations(IEdmVocabularyAnnotatable annotable)
        {
            return annotable == null ?
                Enumerable.Empty<RevisionRecord>() :
                    (Model?.GetCollection<RevisionRecord>(annotable, CoreConstants.Revisions)?.Where(x => x.Kind == RevisionKind.Deprecated) ?? 
                    Enumerable.Empty<RevisionRecord>());
        }
    }
}
