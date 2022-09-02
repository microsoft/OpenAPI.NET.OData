namespace Microsoft.OpenApi.OData.Vocabulary.Core
{

    /// <summary>
    /// Custom link relation type keys
    /// </summary>
    public enum LinkRelKey
    {
        /// <summary>
        /// Identifies external documentation for a GET operation on an entity.
        /// </summary>
        ReadByKey,

        /// <summary>
        /// Identifies external documentation for a GET operation on an entity set.
        /// </summary>
        List,

        /// <summary>
        /// Identifies external documentation for a POST operation.
        /// </summary>
        Create,

        /// <summary>
        /// Identifies external documentation for a PATCH operation.
        /// </summary>
        Update,

        /// <summary>
        /// Identifies external documentation for a DELETE operation.
        /// </summary>
        Delete,

        /// <summary>
        /// Identifies external documentation for a function.
        /// </summary>
        Function,

        /// <summary>
        /// Identifies external documentation for an action.
        /// </summary>
        Action
    }
}
