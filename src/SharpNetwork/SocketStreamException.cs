using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace SharpNetwork
{
    /// <summary>
    /// An exception being thrown when a socket's connection is aborted.
    /// </summary>
    [Serializable]
    public class SocketStreamException : Exception
    {
        /// <summary>
        /// The reason for aborting socket's connection.
        /// </summary>
        public SocketStreamError Error { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:SharpNetwork.SocketStreamException" /> class.
        /// </summary>
        /// <param name="error">The reason for aborting socket's connection.</param>
        public SocketStreamException(SocketStreamError error) : this(error, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:SharpNetwork.SocketStreamException" /> class.
        /// </summary>
        /// <param name="error">The reason for aborting socket's connection.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified. </param>
        public SocketStreamException(SocketStreamError error, Exception innerException) : base(null, innerException)
        {
            Error = error;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:SharpNetwork.SocketStreamException" /> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown. </param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination. </param>
        protected SocketStreamException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Error = (SocketStreamError)info.GetInt32(nameof(Error));
        }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown. </param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination. </param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue(nameof(Error), (int)Error);

            base.GetObjectData(info, context);
        }
    }
}