using System;
using System.Collections.Generic;
using System.Linq;
namespace Stateless.Workflow
{
    /// <summary>
    /// Configuration of tasks, roles and action restrictions.
    /// </summary>
    /// <typeparam name="TCriteria">The type of the criteria.</typeparam>
    /// <typeparam name="TCause">The type of the cause.</typeparam>
    public class RestrictionList<TCriteria, TCause>
        where TCriteria : struct
        where TCause : struct
    {
        /// <summary>
        /// The restrictions
        /// </summary>
        private readonly Dictionary<int, Restriction<TCriteria, TCause>> _restrictions;
        /// <summary>
        /// Gets the cause map.
        /// </summary>
        /// <value>
        /// The cause map.
        /// </value>
        public Dictionary<Type, TCause> CauseMap { get; private set; }
        /// <summary>
        /// The get current criteria
        /// </summary>
        private Func<TCriteria> _getCurrentCriteria;
        /// <summary>
        /// Gets the get current criteria.
        /// </summary>
        /// <value>
        /// The get current criteria.
        /// </value>
        /// <exception cref="InvalidOperationException">Please call SetStateAccessor() in build phase and set state accessor delagate.</exception>
        private Func<TCriteria> GetCurrentCriteria
        {
            get
            {
                if (_getCurrentCriteria == null)
                    throw new InvalidOperationException("Please call SetStateAccessor() in build phase and set state accessor delagate.");
                return
                    _getCurrentCriteria;
            }
        }
        /// <summary>
        /// Sets the state accessor.
        /// </summary>
        /// <param name="stateAccessor">The state accessor.</param>
        /// <returns></returns>
        public RestrictionList<TCriteria, TCause> SetStateAccessor(Func<TCriteria> stateAccessor) { _getCurrentCriteria = stateAccessor; return this; }
        /// <summary>
        /// Sets the cause map.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <returns></returns>
        public RestrictionList<TCriteria, TCause> SetCauseMap(Dictionary<Type, TCause> map)
        {
            CauseMap = map;
            return
                this;
        }
        /// <summary>
        /// Maps the causes.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <returns></returns>
        public RestrictionList<TCriteria, TCause> MapCauses(Dictionary<Type, TCause> map)
        {
            CauseMap = map;
            return
                this;
        }
        /// <summary>
        /// Maps the causes.
        /// </summary>
        /// <param name="configure">The configure.</param>
        /// <returns></returns>
        public RestrictionList<TCriteria, TCause> MapCauses(Action<Dictionary<Type, TCause>> configure)
        {
            CauseMap = new Dictionary<Type, TCause>();
            configure(CauseMap);
            return
                this;
        }
        /// <summary>
        /// Gets the criterias for cause.
        /// </summary>
        /// <param name="cause">The cause.</param>
        /// <returns></returns>
        public ICollection<TCriteria> GetCriteriasForCause(TCause cause)
        {
            return _restrictions.Values
                .Where(r => r.Cause.Equals(cause))
                .SelectMany(r => r.WhiteList)
                .ToList();

        }
        /// <summary>
        /// Initializes a new instance of the <see cref="RestrictionList{TCriteria, TCause}"/> class.
        /// </summary>
        public RestrictionList()
        {
            _restrictions = new Dictionary<int, Restriction<TCriteria, TCause>>();
        }
        /// <summary>
        /// Allows the specified causes.
        /// </summary>
        /// <param name="causes">The causes.</param>
        /// <returns></returns>
        public RestrictionBuilder<TCriteria, TCause> Allow(params TCause[] causes)
        {
            return
                new RestrictionBuilder<TCriteria, TCause>(this, causes);

            /*
            var restriction = new Restriction<TCriteria, TCause>() { Restrictions = this, Cause = cause };

            _restrictions
                .Add(Convert.ToInt32(cause), restriction);

            return
                restriction;
            */
        }
        /// <summary>
        /// Allows the cause.
        /// </summary>
        /// <param name="cause">The cause.</param>
        /// <returns></returns>
        internal Restriction<TCriteria, TCause> AllowCause(TCause cause)
        {
            Restriction<TCriteria, TCause> restriction;

            if (_restrictions.TryGetValue(Convert.ToInt32(cause), out restriction))
                return
                    restriction;

            restriction = new Restriction<TCriteria, TCause>()
            {
                Restrictions = this,
                Cause = cause
            };

            _restrictions
                .Add(Convert.ToInt32(cause), restriction);

            return
                restriction;
        }
        /// <summary>
        /// Determines whether the specified type is allowed.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if the specified type is allowed; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="InvalidOperationException">Object to cause mapping for object type '{type.Name}'</exception>
        public bool IsAllowed(Type type)
        {
            int cause;
            if (CauseMap != null)
            {
                TCause causeEnum;
                if (!CauseMap.TryGetValue(type, out causeEnum))
                    throw new InvalidOperationException($"Object to cause mapping for object type '{type.Name}' is not configured. Please verify MapCauses() method call.");
                cause = Convert.ToInt32(causeEnum);
            }
            else
                cause = CausesAttribute.GetCause(type);

            return
                IsAllowed(cause);
        }
        /// <summary>
        /// Determines whether the specified cause is allowed.
        /// </summary>
        /// <param name="cause">The cause.</param>
        /// <returns>
        ///   <c>true</c> if the specified cause is allowed; otherwise, <c>false</c>.
        /// </returns>
        public bool IsAllowed(TCause cause)
        {
            return IsAllowed(Convert.ToInt32(cause));
        }
        /// <summary>
        /// Determines whether the specified cause is allowed.
        /// </summary>
        /// <param name="cause">The cause.</param>
        /// <returns>
        ///   <c>true</c> if the specified cause is allowed; otherwise, <c>false</c>.
        /// </returns>
        public bool IsAllowed(int cause)
        {
            return _restrictions.ContainsKey(cause)
                && _restrictions[cause]
                    .IsAllowed(GetCurrentCriteria());
        }

        /// <summary>
        /// Gets the allowed causes.
        /// </summary>
        /// <returns></returns>
        public ICollection<TCause> GetAllowedCauses()
        {
            return
                GetAllowedCauses(GetCurrentCriteria());

        }
        /// <summary>
        /// Gets the allowed causes.
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        /// <returns></returns>
        public ICollection<TCause> GetAllowedCauses(TCriteria criteria)
        {
            return _restrictions.Values
                .Where(restriciton => restriciton
                    .WhiteList.Contains(criteria))
                .Select(r => r.Cause)
                .ToList();
        }
    }
}
