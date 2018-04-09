// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.OpenApi.OData.Common
{
    public enum PathType
    {
        EntitySet,
        Entity,
        Singleton,
        Operation,
        OperationImport,
        NavigationProperty
    }

    /// <summary>
    /// Utilities methods
    /// </summary>
    public class ODataPath : IEnumerable<ODataSegment>
    {
        private PathType? _pathType;

        /// <summary>
        /// The segments that make up this path.
        /// </summary>
        private IList<ODataSegment> _segments;

        public ODataPath(IEnumerable<ODataSegment> segments)
        {
            _segments = segments.ToList();
            if (_segments.Any(s => s == null))
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
        public IList<ODataSegment> Segments => _segments;

        /// <summary>
        /// Pop the last segment.
        /// </summary>
        /// <returns>The pop last segment.</returns>
        public ODataSegment Pop()
        {
            if (!_segments.Any())
            {
                throw Error.ArgumentNull("Pop");
            }

            ODataSegment segment = _segments.Last();
            _segments.RemoveAt(_segments.Count - 1);
            return segment;
        }

        /// <summary>
        /// Push a segment to the last.
        /// </summary>
        /// <param name="segment">The pushed segment.</param>
        /// <returns>The whole path object.</returns>
        public ODataPath Push(ODataSegment segment)
        {
            if (_segments == null)
            {
                _segments = new List<ODataSegment>();
            }

            _segments.Add(segment);
            return this;
        }

        /// <summary>
        /// Gets the first segment in the path. Returns null if the path is empty.
        /// </summary>
        public ODataSegment FirstSegment
        {
            get
            {
                return this._segments.Count == 0 ? null : this._segments[0];
            }
        }

        /// <summary>
        /// Get the last segment in the path. Returns null if the path is empty.
        /// </summary>
        public ODataSegment LastSegment
        {
            get
            {
                return this._segments.Count == 0 ? null : this._segments[this._segments.Count - 1];
            }
        }

        /// <summary>
        /// Get the number of segments in this path.
        /// </summary>
        public int Count
        {
            get { return this._segments.Count; }
        }

        /// <summary>
        /// Get the segments enumerator
        /// </summary>
        /// <returns>The segments enumerator</returns>
        public IEnumerator<ODataSegment> GetEnumerator()
        {
            return this._segments.GetEnumerator();
        }

        /// <summary>
        /// get the segments enumerator
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
            return new ODataPath(_segments);
        }

        /// <summary>
        /// Output the path string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            return "/" + String.Join("/", _segments);
        }

        public PathType PathType
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
            if (_segments.Any(c => c is ODataNavigationPropertySegment))
            {
                _pathType = PathType.NavigationProperty;
                return;
            }
            else if (_segments.Any(c => c is ODataOperationImportSegment))
            {
                _pathType = PathType.OperationImport;
                return;
            }
            else if (_segments.Any(c => c is ODataOperationSegment))
            {
                _pathType = PathType.Operation;
                return;
            }

            if (_segments.Count == 1)
            {
                ODataNavigationSourceSegment segment = _segments[0] as ODataNavigationSourceSegment;
                if (segment == null)
                {
                    throw Error.ArgumentNull("segment");
                }

                if (segment.NavigationSource is IEdmSingleton)
                {
                    _pathType = PathType.Singleton;
                }
                else
                {
                    _pathType = PathType.EntitySet;
                }
            }
            else
            {
                if (_segments.Count != 2)
                {
                    throw Error.ArgumentNull("entity");
                }

                _pathType = PathType.Entity;
            }
        }
    }
}