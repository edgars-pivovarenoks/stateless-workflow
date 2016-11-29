using System;
using System.Linq;
namespace Stateless.Workflow
{

    /// <summary>
    /// Declaratively link DTO to workflow Task to streamline restrictions control
    /// </summary>
    /// <seealso cref="System.Attribute" />
    public class CausesAttribute : Attribute
    {
        /// <summary>
        /// Gets the cause.
        /// </summary>
        /// <value>
        /// The cause.
        /// </value>
        public int Cause { get; private set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="CausesAttribute"/> class.
        /// </summary>
        /// <param name="restriction">The restriction.</param>
        public CausesAttribute(int restriction)
        {
            Cause = restriction;
        }
        /// <summary>
        /// Gets the cause.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Type [{typeName}]</exception>
        public static int GetCause(Type type)
        {
            var typeName = type.Name;
            var causeAttribute = type
                .GetCustomAttributes(typeof(CausesAttribute), true).FirstOrDefault(a => a is CausesAttribute)
                as CausesAttribute;

            if (causeAttribute != null)
                return (int)causeAttribute.Cause;

            throw
                new NotImplementedException($"Type [{typeName}] should have [Cause] attribute to participate in restrictions validation.");
        }
    }
    
}
