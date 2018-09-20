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
    public class ODataPath : IEnumerable<ODataSegment>
    {
        private ODataPathKind? _pathKind;

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
            return Segments.Count(c => keySegmentAsDepth ? true : !(c is ODataKeySegment));
        }

        /// <summary>
        /// Gets the default path item name.
        /// </summary>
        /// <returns>The string.</returns>
        public string GetPathItemName()
        {
            return GetPathItemName(new OpenApiConvertSettings());
        }

        /// <summary>
        /// Gets the path item name.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>The string.</returns>
        public string GetPathItemName(OpenApiConvertSettings settings)
        {
            Utils.CheckArgumentNull(settings, nameof(settings));

            StringBuilder sb = new StringBuilder();
            foreach (var segment in Segments)
            {
                string pathItemName = segment.GetPathItemName(settings);

                if (segment.Kind == ODataSegmentKind.Key &&
                    (settings.EnableKeyAsSegment == null || !settings.EnableKeyAsSegment.Value))
                {
                    sb.Append("(");
                    sb.Append(pathItemName);
                    sb.Append(")");
                }
                else // other segments
                {
                    sb.Append("/");
                    sb.Append(pathItemName);
                }
            }

            return sb.ToString();
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
            Segments.RemoveAt(Segments.Count - 1);
            return this;
        }

        /// <summary>
        /// Output the path string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            return "/" + String.Join("/", Segments.Select(e => e.Kind));
        }

        private ODataPathKind CalcPathType()
        {
            if (Segments.Any(c => c.Kind == ODataSegmentKind.OperationImport))
            {
                return ODataPathKind.OperationImport;
            }
            else if (Segments.Any(c => c.Kind == ODataSegmentKind.Operation))
            {
                return ODataPathKind.Operation;
            }
            else if (Segments.Any(c => c.Kind == ODataSegmentKind.NavigationProperty))
            {
                return ODataPathKind.NavigationProperty;
            }

            if (Segments.Count == 1)
            {
                ODataNavigationSourceSegment segment = Segments[0] as ODataNavigationSourceSegment;
                if (segment != null)
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
            }
            else if (Segments.Count == 2 && Segments.Last().Kind == ODataSegmentKind.Key)
            {
                return ODataPathKind.Entity;
            }

            return ODataPathKind.Unknown;
        }
    }
}