namespace Stateless.Workflow
{
    /// <summary>
    /// Helper class to construct restriction lists
    /// </summary>
    /// <typeparam name="TCriteria">The type of the criteria.</typeparam>
    /// <typeparam name="TCause">The type of the cause.</typeparam>
    public class RestrictionBuilder<TCriteria, TCause>
        where TCause : struct
        where TCriteria : struct
    {
        /// <summary>
        /// The restriction list
        /// </summary>
        private readonly RestrictionList<TCriteria, TCause> _restrictionList;
        /// <summary>
        /// The causes
        /// </summary>
        private readonly TCause[] _causes;
        /// <summary>
        /// Initializes a new instance of the <see cref="RestrictionBuilder{TCriteria, TCause}"/> class.
        /// </summary>
        /// <param name="restrictionList">The restriction list.</param>
        /// <param name="causes">The causes.</param>
        public RestrictionBuilder(RestrictionList<TCriteria, TCause> restrictionList, TCause[] causes)
        {
            _restrictionList = restrictionList;
            _causes = causes;
        }
        /// <summary>
        /// Whens the specified criterias.
        /// </summary>
        /// <param name="criterias">The criterias.</param>
        /// <returns></returns>
        public RestrictionList<TCriteria, TCause> When(params TCriteria[] criterias)
        {
            foreach (var cause in _causes)
                _restrictionList.AllowCause(cause)
                    .To(criterias);

            return
                _restrictionList;
        }
    }
}
