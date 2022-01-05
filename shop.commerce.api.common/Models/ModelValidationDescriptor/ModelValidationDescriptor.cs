namespace shop.commerce.api.common.Models
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// this class is used to describe the model validation state
    /// </summary>
    public class ModelValidationDescriptor
    {
        /// <summary>
        /// list of model properties errors
        /// </summary>
        public IEnumerable<PropertyErrorDescriptor> ValidationErrors { get; }

        /// <summary>
        /// default constructor
        /// </summary>
        public ModelValidationDescriptor(IEnumerable<PropertyErrorDescriptor> propertyErrors)
        {
            ValidationErrors = new List<PropertyErrorDescriptor>(propertyErrors);
        }

        /// <summary>
        /// get the result instant of the model validations
        /// this will get the first error and  return it as a result object
        /// </summary>
        /// <returns>the result instant</returns>
        public Result ToResult()
        {
            // get the error
            var error = ValidationErrors.FirstOrDefault();
            if (error is null)
                return Result.ResultSuccess();

            // get property error
            var propError = error.Errors.FirstOrDefault();

            // build failed result
            return Result.Failed(code: propError.Code, message: propError.Message);
        }
    }
}
