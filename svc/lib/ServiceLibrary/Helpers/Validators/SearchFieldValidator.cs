using ServiceLibrary.Exceptions;

namespace ServiceLibrary.Helpers.Validators
{
    public class SearchFieldValidator
    {
        private const int MaxSearchLength = 250;

        public static void Validate(string search)
        {
            if (string.IsNullOrEmpty(search))
            {
                return;
            }

            search = search.Trim();

            if (search.Length > MaxSearchLength)
            {
                throw new BadRequestException(ErrorMessages.SearchFieldLimitation, ErrorCodes.BadRequest);
            }
        }
    }
}
