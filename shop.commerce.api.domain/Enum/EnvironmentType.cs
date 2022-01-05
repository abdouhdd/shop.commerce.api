using System.ComponentModel;

namespace shop.commerce.api.domain.Enum
{
    public enum EnvironmentType
    {
        /// <summary>
        /// Development Environment
        /// </summary>
        [Description("Development")]
        Development = 1,

        /// <summary>
        /// Test Environment
        /// </summary>
        [Description("Test")]
        Test = 2,

        /// <summary>
        /// Production Environment
        /// </summary>
        [Description("Production")]
        Production = 3,
    }
}
