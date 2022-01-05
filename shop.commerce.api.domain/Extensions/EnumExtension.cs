using shop.commerce.api.domain.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace shop.commerce.api.domain.Extensions
{
    public class EnumExtension
    {
        public static bool EnumValid(Type typeEnum, int enumValue)
        {
            bool valid = false;
            Array values = System.Enum.GetValues(typeEnum);
            foreach (int value in values)
            {
                if (enumValue == value)
                {
                    valid = true;
                    break;
                }
            }
            return valid;
        }

        public static List<SelectView> ValuesEnum(Type enumType)
        {
            List<SelectView> paymentMethods = new List<SelectView>();

            Array valuesPaymentMethod = System.Enum.GetValues(enumType);
            foreach (int value in valuesPaymentMethod)
            {
                var memberInfos = enumType.GetMember(System.Enum.GetName(enumType, value));
                var enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == enumType);
                var valueAttributes =
                      enumValueMemberInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
                var description = ((DescriptionAttribute)valueAttributes[0]).Description;

                paymentMethods.Add(new SelectView
                {
                    Id = value,
                    Name = description
                });
            }
            return paymentMethods;
        }
    }
}
