// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Common;

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
        public ODataSegment Pop()
        {
            if (!Segments.Any())
            {
                throw Error.InvalidOperation("Pop a segment is invalid. The segments in the path is empty.");
            }

            ODataSegment segment = Segments.Last();
            Segments.RemoveAt(Segments.Count - 1);
            return segment;
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
        public ODataSegment FirstSegment
        {
            get
            {
                return this.Segments.Count == 0 ? null : this.Segments[0];
            }
        }

        /// <summary>
        /// Get the last segment in the path. Returns null if the path is empty.
        /// </summary>
        public ODataSegment LastSegment
        {
            get
            {
                return this.Segments.Count == 0 ? null : this.Segments[this.Segments.Count - 1];
            }
        }

        /// <summary>
        /// Get the number of segments in this path.
        /// </summary>
        public int Count
        {
            get { return this.Segments.Count; }
        }

        /// <summary>
        /// Get the segments enumerator
        /// </summary>
        /// <returns>The segments enumerator</returns>
        public IEnumerator<ODataSegment> GetEnumerator()
        {
            return this.Segments.GetEnumerator();
        }

        /// <summary>
        /// Get the segments enumerator
        /// </summary>
        /// <returns>The segments enumerator.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Clone a new ODataPath object.
        /// </summary>
        /// <returns>The new ODataPath.</returns>
        public ODataPath Clone()
        {
            return new ODataPath(Segments);
        }

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