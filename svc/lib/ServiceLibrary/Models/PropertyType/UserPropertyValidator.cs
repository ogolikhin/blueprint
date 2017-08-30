using System.Linq;
using ServiceLibrary.Helpers;
using ServiceLibrary.Helpers.Validators;

namespace ServiceLibrary.Models.PropertyType
{
    public class UserPropertyValidator : PropertyValidator<UserPropertyType>
    {
        protected override PropertySetResult Validate(PropertyLite property, UserPropertyType propertyType, IValidationContext validationContext)
        {
            var isValid = property.UsersAndGroups.All(ug => IsUserOrGroupValid(ug, validationContext));
            if (!isValid)
            {
                return new PropertySetResult(property.PropertyTypeId, ErrorCodes.InvalidArtifactProperty, "User or group id does not exist");
            }
            return null;
        }

        protected override bool IsPropertyValueEmpty(PropertyLite property, UserPropertyType propertyType)
        {
            return !property.UsersAndGroups.Any();
        }

        private static bool IsUserOrGroupValid(UserGroup userOrGroup, IValidationContext context)
        {
            return userOrGroup.IsGroup.GetValueOrDefault(false)
                ? context.Groups.Any(u => u.GroupId == userOrGroup.Id)
                : context.Users.Any(u => u.UserId == userOrGroup.Id);
        }

    }
}