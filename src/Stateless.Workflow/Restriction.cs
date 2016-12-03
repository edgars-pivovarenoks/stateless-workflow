using System.Collections.Generic;
using System.Linq;

namespace Stateless.Workflow
{
    /// <summary>
    /// Links particular cause and check criteria
    /// </summary>
    /// <typeparam name="TCriteria">The type of the criteria.</typeparam>
    /// <typeparam name="TCause">The type of the cause.</typeparam>
    public class Restriction<TCriteria, TCause>
        where TCause : struct
        where TCriteria : struct
    {
        /// <summary>
        /// Gets the white list.
        /// </summary>
        /// <value>
        /// The white list.
        /// </value>
        public List<TCriteria> WhiteList { get; private set; }
        /// <summary>
        /// Gets or sets the cause.
        /// </summary>
        /// <value>
        /// The cause.
        /// </value>
        public TCause Cause { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="Restriction{TCriteria, TCause}"/> class.
        /// </summary>
        public Restriction()
        {
            WhiteList = new List<TCriteria>();
        }
        /// <summary>
        /// Determines whether the specified criteria is allowed.
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        /// <returns>
        ///   <c>true</c> if the specified criteria is allowed; otherwise, <c>false</c>.
        /// </returns>
        public bool IsAllowed(TCriteria criteria)
        {
            return WhiteList
                .Contains(criteria);
        }
        /// <summary>
        /// The restrictions
        /// </summary>
        public RestrictionList<TCriteria, TCause> Restrictions;
        /// <summary>
        /// To the specified criterias.
        /// </summary>
        /// <param name="criterias">The criterias.</param>
        /// <returns></returns>
        public Restriction<TCriteria, TCause> To(params TCriteria[] criterias)
        {
            if (criterias == null || criterias.Length == 0)
                return this;

            var newCriterias = criterias
                    .Except(WhiteList)
                    // .Where(c => !WhiteList.Any(w => w.Equals(c)))
                    .ToList();

            if (newCriterias.Any())
                WhiteList
                    .AddRange(newCriterias);

            return
                this;
        }

        /// <summary>
        /// Builds this instance.
        /// </summary>
        /// <returns></returns>
        public RestrictionList<TCriteria, TCause> Build()
        {
            return
                Restrictions;
        }
        /*
        public Restriction<TCriteria, TCause> Restrict(TCause cause)
        {
            return Restrictions
                .RestrictCause(cause);
        }
        */
    }
}
