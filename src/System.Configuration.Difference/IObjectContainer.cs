using System;
using System.Configuration.Difference.Metadata;

namespace System.Configuration.Difference {
    
    /// <summary>
    /// Manager object instance in a container.
    /// </summary>
    public interface IObjectContainer {

        /// <summary>
        /// Return object id/reference type.
        /// </summary>
        IObjectType ObjectReferenceType { get; }

        /// <summary>
        /// Get a object from object id.
        /// </summary>
        /// <param name="oid">object id</param>
        /// <param name="obj">return a object equals oid,if not find return null.</param>
        /// <returns>if find it then return true,else return false.</returns>
        bool TryGet(object oid, out object obj);

        /// <summary>
        /// Register a exist object to container.
        /// </summary>
        /// <param name="oid">object id</param>
        /// <param name="obj">exist object instance.</param>
        void Register(object oid, object obj);
    }
}
