namespace shop.commerce.api.common
{
    using shop.commerce.api.common.Models;
    using System;

    /// <summary>
    /// this exception Class is used to define that the validation of a model has been failed
    /// </summary>
    [Serializable]
    public class ModelValidationException : AppException
    {
        /// <summary>
        /// the model validation descriptor instant
        /// </summary>
        public ModelValidationDescriptor Descriptor { get; }

        /// <summary>
        /// create an instant of <see cref="ModelValidationException"/>
        /// </summary>
        /// <param name="descriptor"><see cref="ModelValidationDescriptor"/> instant</param>
        public ModelValidationException(ModelValidationDescriptor descriptor)
            : base("model validation failed", ResultCode.ValidationFailed)
        {
            Descriptor = descriptor;
        }
    }
}
