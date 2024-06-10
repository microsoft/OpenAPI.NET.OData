// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Properties;

namespace Microsoft.OpenApi.OData.Edm
{
    /// <summary>
    /// Describes an OData path.
    /// </summary>
    public class ODataPath : IEnumerable<ODataSegment>, IComparable<ODataPath>
    {
        private ODataPathKind? _pathKind;
        private string _defaultPathItemName;

        /// <summary>
        /// Initializes a new instance of <see cref="ODataPath"/> class.
        /// </summary>
        /// <param name="segments">The segments.</param>
        public ODataPath(IEnumerable<ODataSegment> segments)
        {
            Segments = segments.ToList();
            if (Segments.Any(s => s == null))
            {
                throw Error.ArgumentNull("segments");
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="ODataPath"/> containing the given segments.
        /// </summary>
        /// <param name="segments">The segments that make up the path.</param>
        /// <exception cref="ArgumentNullException">Throws if input segments is null.</exception>
        public ODataPath(params ODataSegment[] segments)
            : this((IEnumerable<ODataSegment>)segments)
        {
        }

        /// <summary>
        /// Gets/Sets the support HttpMethods
        /// </summary>
        public ISet<string> HttpMethods { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets/Sets the path template for this path.
        /// If it is set, it will be used to generate the path item.
        /// </summary>
        public string PathTemplate { get; set; }

        /// <summary>
        /// Gets the segments.
        /// </summary>
        public IList<ODataSegment> Segments { get; private set; }

        /// <summary>
        /// Gets the path kind.
        /// </summary>
        public virtual ODataPathKind Kind
        {
            get
            {
                if (_pathKind == null)
                {
                    _pathKind = CalcPathType();
                }

                return _pathKind.Value;
            }
        }

        /// <summary>
        /// Gets the first segment in the path. Returns null if the path is empty.
        /// </summary>
        public ODataSegment FirstSegment => Segments.Count == 0 ? null : Segments[0];

        /// <summary>
        /// Get the last segment in the path. Returns null if the path is empty.
        /// </summary>
        public ODataSegment LastSegment => Segments.Count == 0 ? null : this.Segments[Segments.Count - 1];

        /// <summary>
        /// Get the number of segments in this path.
        /// </summary>
        public int Count => this.Segments.Count;

        /// <summary>
        /// Get the segments enumerator
        /// </summary>
        /// <returns>The segments enumerator</returns>
        public IEnumerator<ODataSegment> GetEnumerator() => Segments.GetEnumerator();

        /// <summary>
        /// Get the segments enumerator
        /// </summary>
        /// <returns>The segments enumerator.</returns>
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        /// <summary>
        /// Clone a new ODataPath object.
        /// </summary>
        /// <returns>The new ODataPath.</returns>
        public ODataPath Clone() => new ODataPath(Segments);

        /// <summary>
        /// Get the segment count.
        /// </summary>
        /// <param name="keySegmentAsDepth">A bool value indicating whether to count key segment or not.</param>
        /// <returns>The count.</returns>
        public int GetCount(bool keySegmentAsDepth)
        {
            return Segments.Count(c => keySegmentAsDepth || c is not ODataKeySegment);
        }

        /// <summary>
        /// Gets the default path item name.
        /// </summary>
        /// <returns>The string.</returns>
        public string GetPathItemName()
        {
            if (_defaultPathItemName != null)
            {
                return _defaultPathItemName;
            }

            _defaultPathItemName = GetPathItemName(new OpenApiConvertSettings());
            return _defaultPathItemName;
        }

        /// <summary>
        /// Gets the path item name.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>The string.</returns>
        public string GetPathItemName(OpenApiConvertSettings settings)
        {
            Utils.CheckArgumentNull(settings, nameof(settings));

            // From Open API spec, parameter name is case sensitive, so don't use the IgnoreCase HashSet.
            HashSet<string> parameters = new();
            StringBuilder sb = new();

            if (!string.IsNullOrWhiteSpace(settings.PathPrefix))
            {
                sb.Append("/");
                sb.Append(settings.PathPrefix);
            }

            foreach (var segment in Segments)
            {
                string pathItemName = segment.GetPathItemName(settings, parameters);

                if (segment is ODataKeySegment keySegment &&
                    (settings.EnableKeyAsSegment == null || !settings.EnableKeyAsSegment.Value || keySegment.IsAlternateKey))
                {
                    sb.Append("(");
                    sb.Append(pathItemName);
                    sb.Append(")");
                }
                else // other segments
                {
                    if (segment.Kind == ODataSegmentKind.Operation)
                    {
                        ODataOperationSegment operation = (ODataOperationSegment)segment;
                        if (operation.IsEscapedFunction && settings.EnableUriEscapeFunctionCall)
                        {
                            sb.Append(":/");
                            sb.Append(pathItemName);
                            continue;
                        }
                    }

                    sb.Append("/");
                    sb.Append(pathItemName);
                }
            }

            return sb.ToString();
        }

        internal bool SupportHttpMethod(string method)
        {
            // If the Httpmethods is empty, let it go
            if (HttpMethods.Count == 0)
            {
                return true;
            }

            return HttpMethods.Contains(method);
        }

        /// <summary>
        /// Push a segment to the last.
        /// </summary>
        /// <param name="segment">The pushed segment.</param>
        /// <returns>The whole path object.</returns>
        internal ODataPath Push(ODataSegment segment)
        {
            if (Segments == null)
            {
                Segments = new List<ODataSegment>();
            }

            _pathKind = null;
            _defaultPathItemName = null;
            Segments.Add(segment);
            return this;
        }

        /// <summary>
        /// Pop the last segment.
        /// </summary>
        /// <returns>The pop last segment.</returns>
        internal ODataPath Pop()
        {
            if (!Segments.Any())
            {
                throw Error.InvalidOperation(SRResource.ODataPathPopInvalid);
            }

            _pathKind = null;
            _defaultPathItemName = null;
            Segments.RemoveAt(Segments.Count - 1);
            return this;
        }

        internal IDictionary<ODataSegment, IDictionary<string, string>> CalculateParameterMapping(OpenApiConvertSettings settings)
        {
            IDictionary<ODataSegment, IDictionary<string, string>> parameterMapping = new Dictionary<ODataSegment, IDictionary<string, string>>();

            // From Open API spec, parameter name is case sensitive, so don't use the IgnoreCase HashSet.
            HashSet<string> parameters = new HashSet<string>();

            foreach (var segment in Segments)
            {
                // So far, only care about the key segment and operation segment
                if (segment.Kind == ODataSegmentKind.Key)
                {
                    ODataKeySegment keySegment = (ODataKeySegment)segment;
                    parameterMapping[keySegment] = keySegment.GetKeyNameMapping(settings, parameters);
                }
                else if (segment.Kind == ODataSegmentKind.Operation)
                {
                    ODataOperationSegment operationSegment = (ODataOperationSegment)segment;
                    parameterMapping[operationSegment] = operationSegment.GetNameMapping(settings, parameters);
                }
            }

            return parameterMapping;
        }

        /// <summary>
        /// Get string representation of the Edm Target Path for annotations
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <returns>The string representation of the Edm target path.</returns>
        internal string GetTargetPath(IEdmModel model)
        {
            Utils.CheckArgumentNull(model, nameof(model));

            var targetPath = new StringBuilder(model.EntityContainer.FullName());

            bool skipLastSegment = LastSegment is ODataRefSegment 
                || LastSegment is ODataDollarCountSegment
                || LastSegment is ODataStreamContentSegment;
            foreach (var segment in Segments.Where(segment => segment is not ODataKeySegment
                && !(skipLastSegment && segment == LastSegment)))
            {
                targetPath.Append($"/{segment.Identifier}");
            }
            return targetPath.ToString();
        }

        /// <summary>
        /// Output the path string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            if (PathTemplate != null)
            {
                return PathTemplate;
            }

            return "/" + String.Join("/", Segments.Select(e => e.Kind));
        }

        /// <summary>
        /// Compare between two ODataPath using its path item name.
        /// </summary>
        /// <param name="other">The compare to ODataPath.</param>
        /// <returns>true/false</returns>
        public int CompareTo(ODataPath other)
        {
            return GetPathItemName().CompareTo(other.GetPathItemName());
        }

        private ODataPathKind CalcPathType()
        {
            if (Segments.Count == 1 && Segments.First().Kind == ODataSegmentKind.Metadata)
            {
                return ODataPathKind.Metadata;
            }
            else if (Segments.Last().Kind == ODataSegmentKind.DollarCount)
            {
                return ODataPathKind.DollarCount;
            }
            else if (Segments.Last().Kind == ODataSegmentKind.TypeCast)
            {
                return ODataPathKind.TypeCast;
            }
            else if (Segments.Last().Kind == ODataSegmentKind.ComplexProperty)
            {
                return ODataPathKind.ComplexProperty;
            }
            else if (Segments.Any(c => c.Kind == ODataSegmentKind.StreamProperty || c.Kind == ODataSegmentKind.StreamContent))
            {
                return ODataPathKind.MediaEntity;
            }
            else if (Segments.Any(c => c.Kind == ODataSegmentKind.Ref))
            {
                return ODataPathKind.Ref;
            }
            else if (Segments.Any(c => c.Kind == ODataSegmentKind.OperationImport))
            {
                return ODataPathKind.OperationImport;
            }
            else if (Segments.Last().Kind == ODataSegmentKind.Operation)
            {
                return ODataPathKind.Operation;
            }
            else if (Segments.Any(c => c.Kind == ODataSegmentKind.NavigationProperty))
            {
                return ODataPathKind.NavigationProperty;
            }
            else if (Segments.Count == 1 && Segments[0] is ODataNavigationSourceSegment segment)
            {
                if (segment.NavigationSource is IEdmSingleton)
                {
                    return ODataPathKind.Singleton;
                }
                else
                {
                    return ODataPathKind.EntitySet;
                }
            }
            else if (Segments.Count == 2 && Segments.Last().Kind == ODataSegmentKind.Key)
            {
                return ODataPathKind.Entity;
            }

            return ODataPathKind.Unknown;
        }

        /// <summary>
        /// Provides a suffix for the operation id based on the operation path.
        /// </summary>
        /// <param name="settings">The settings.</param>
        ///<returns>The suffix.</returns>
        public string GetPathHash(OpenApiConvertSettings settings) =>
            LastSegment.GetPathHash(settings, this);
    }
}