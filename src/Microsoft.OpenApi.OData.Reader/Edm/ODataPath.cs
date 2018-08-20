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
        private ODataPathType? _pathType;

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
        /// Pop the last segment.
        /// </summary>
        /// <returns>The pop last segment.</returns>
        public ODataPath Pop()
        {
            if (!Segments.Any())
            {
                throw Error.InvalidOperation(SRResource.ODataPathPopInvalid);
            }

            Segments.RemoveAt(Segments.Count - 1);
            return this;
        }

        /// <summary>
        /// Push a segment to the last.
        /// </summary>
        /// <param name="segment">The pushed segment.</param>
        /// <returns>The whole path object.</returns>
        public ODataPath Push(ODataSegment segment)
        {
            if (Segments == null)
            {
                Segments = new List<ODataSegment>();
            }

            Segments.Add(segment);
            return this;
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
        /// Gets the path item name.
        /// </summary>
        /// <param name="keyAsSegment">A bool value indicating whether to output key as segment.</param>
        /// <returns>The string.</returns>
        public string GetPathItemName(bool keyAsSegment)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var segment in Segments)
            {
                ODataKeySegment keySegmnet = segment as ODataKeySegment;
                if (keySegmnet != null)
                {
                    // So far, the behaviour for composite keys in key as segment is un-defined.
                    // So, for composite keys, we will use () at any time.
                    if (keyAsSegment && !keySegmnet.HasCompositeKeys)
                    {
                        sb.Append("/");
                        sb.Append(keySegmnet.ToString());
                    }
                    else
                    {
                        sb.Append("(");
                        sb.Append(segment);
                        sb.Append(")");
                    }
                }
                else // other segments
                {
                    sb.Append("/");
                    sb.Append(segment);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Output the path string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            return "/" + String.Join("/", Segments);
        }

        internal ODataPathType PathType
        {
            get
            {
                if (_pathType == null)
                {
                    CalcPathType();
                }

                return _pathType.Value;
            }
        }

        private void CalcPathType()
        {
            if (Segments.Any(c => c is ODataNavigationPropertySegment))
            {
                _pathType = ODataPathType.NavigationProperty;
                return;
            }
            else if (Segments.Any(c => c is ODataOperationImportSegment))
            {
                _pathType = ODataPathType.OperationImport;
                return;
            }
            else if (Segments.Any(c => c is ODataOperationSegment))
            {
                _pathType = ODataPathType.Operation;
                return;
            }

            if (Segments.Count == 1)
            {
                ODataNavigationSourceSegment segment = Segments[0] as ODataNavigationSourceSegment;
                if (segment == null)
                {
                    throw Error.ArgumentNull("segment");
                }

                if (segment.NavigationSource is IEdmSingleton)
                {
                    _pathType = ODataPathType.Singleton;
                }
                else
                {
                    _pathType = ODataPathType.EntitySet;
                }
            }
            else
            {
                if (Segments.Count != 2)
                {
                    throw Error.InvalidOperation("Calc the path type wrong!");
                }

                _pathType = ODataPathType.Entity;
            }
        }
    }
}