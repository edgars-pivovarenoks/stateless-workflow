using System;
using System.Linq;
using System.Reflection;

namespace Stateless.Workflow
{
    /// <summary>
    /// Dynamic workflow state object wrapper for Workflow engine consumption. Object must implement public property of TStatus.
    /// </summary>
    /// <typeparam name="TStatus">The type of the status.</typeparam>
    /// <seealso cref="Stateless.Workflow.IStateObject{TStatus}" />
    public class DynamicStateObject<TStatus> : IStateObject<TStatus>
        where TStatus : struct
    {
        private readonly object _originalObject;
        private Func<TStatus> _getStatus; private Action<TStatus> _setStatus;
        private Func<string> _getVersion; private Action<string> _setVersion;

        // Error message templates
        private const string ImplementsStatusMessage = "Workflow engine is expecting that passed object of type '{0}' has implementation of property of type '{statusType.Name}'. Please check that you are passing right status object or that property implemented, is public and is not static.";
        private const string CanReadWriteStatusMessage = "Object of type '{objType.Name}' property '{0}' implementation is either Read or Write only. Please ensure property supports both get() and set() methods.";
        private const string IsPublicStatusProperty = "Object of type '{0}' property '{1}' implementation has either non-public get() or set(). Please ensure properties both get() and set() methods are public.";
        private const string InterfaceImplementsVersionMessage = "This is Statless.Workflow internal exception. IStateObject<TStatus> doesn't implement Workflow Version property of type string.";
        private const string ImplementsVersionMessage = "Object of type '{0}' doesn't contain '{1}' implementation, that is required to hold Workflow Version.";
        private const string CanReadWriteVersionMessage = "Object of type '{0}' property '{}' implementation is either Read or Write only. Please ensure property supports both get() and set() methods.";
        private const string IsPublicVersionMessage = "Object of type '{0}' property '{1}' implementation has either non-public get() or set(). Please ensure properties both get() and set() methods are public.";

        /// <summary>
        /// Gets or sets the underlying object status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public TStatus Status { get { return _getStatus(); } set { _setStatus(value); } }

        /// <summary>
        /// Gets or sets the workflow version key.
        /// </summary>
        /// <value>
        /// The workflow version key.
        /// </value>
        public string WorkflowVersionKey { get { return _getVersion(); } set { _setVersion(value); } }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicStateObject{TStatus}"/> class.
        /// </summary>
        /// <param name="originalObject">The original object.</param>
        /// <exception cref="InvalidCastException">
        /// Unable to create StateObject object. Workflow engine is expecting object that type '{objType.Name}' has implementation of property of type '{statusType.Name}'.
        /// or
        /// Object of type '{objType.Name}' property '{statusProperty.Name}'
        /// or
        /// Object of type '{objType.Name}' property '{statusProperty.Name}'
        /// </exception>
        public DynamicStateObject(object originalObject)
        {
            // Encapsulate original
            _originalObject = originalObject
                .ThrowIfNull("originalObject");

            // Read metadata
            var objType = _originalObject.GetType();
            var properties = objType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // -----------------------------
            // -- Wrap status property
            // -----------------------------
            var statusProperty = properties
                .FirstOrDefault(p => p.PropertyType.IsAssignableFrom(typeof(TStatus)));

            if (statusProperty == null)
                throw new NotImplementedException(ImplementsStatusMessage.With(objType.Name));
            if (!statusProperty.CanWrite || !statusProperty.CanRead)
                throw new NotImplementedException(CanReadWriteStatusMessage.With(statusProperty.Name));
            if (statusProperty.GetGetMethod(false) == null || statusProperty.GetSetMethod(false) == null)
                throw new NotImplementedException(IsPublicStatusProperty.With(objType.Name, statusProperty.Name));

            // -----------------------------
            // -- Wrap version key property
            // -----------------------------
            var interfaceVersionProperty = typeof(IStateObject<TStatus>)
                .GetProperties()
                .FirstOrDefault(p => p.PropertyType.IsAssignableFrom(typeof(string)) && p.Name.Contains("Version"));

            if (interfaceVersionProperty == null)
                throw new NotImplementedException(InterfaceImplementsVersionMessage);

            var supportedVersionKeyName = new[] { interfaceVersionProperty.Name.ToLower(),
                "version", "versionkey", "workflowversion", "workflowversionkey" };

            var versionProperty = properties
                .SingleOrDefault(p => p.PropertyType.IsAssignableFrom(typeof(string))
                    && supportedVersionKeyName.Contains(p.Name.ToLower()));
            if (versionProperty == null)
                throw new NotImplementedException(ImplementsVersionMessage.With(objType.Name, interfaceVersionProperty.Name));
            if (!versionProperty.CanWrite || !versionProperty.CanRead)
                throw new NotImplementedException(CanReadWriteVersionMessage.With(objType.Name, versionProperty.Name));
            if (versionProperty.GetGetMethod(false) == null || versionProperty.GetSetMethod(false) == null)
                throw new NotImplementedException(IsPublicVersionMessage.With(objType.Name, versionProperty.Name));

            // Setup read/write delegates
            SetStatusDelegates(statusProperty);
            SetVersionDelegates(versionProperty);
        }

        // Done to avoid "implicitly captured closure"
        private void SetStatusDelegates(PropertyInfo p)
        {
            _getStatus = () => (TStatus)p.GetValue(_originalObject, null);
            _setStatus = s => p.SetValue(_originalObject, s);
        }
        private void SetVersionDelegates(PropertyInfo p)
        {
            _getVersion = () => (string)p.GetValue(_originalObject, null);
            _setVersion = s => p.SetValue(_originalObject, s);
        }
    }
}
