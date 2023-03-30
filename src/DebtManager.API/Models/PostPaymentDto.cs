using DebtManager.API.Configuration.Extensions;

namespace DebtManager.API.Models
{
    public class PostPaymentDto
    {
        public IEnumerable<Guid> ChargeIds { get; set; }
        public IEnumerable<string> Usernames { get; set; }
        public bool PreApprovedPayments { get; set; }

        public ModelValidationResult ValidateModel()
        {
            if (!ChargeIds.HasItems() && !Usernames.HasItems())
            {
                return new ModelValidationResult
                {
                    Success = false,
                    Message = $"{nameof(ChargeIds)} or {nameof(Usernames)} cannot be empty."
                };
            }

            if (ChargeIds.HasItems())
            {
                var invalidGuids = ChargeIds.Where(x => x == Guid.Empty);

                if (invalidGuids.Any())
                {
                    return new ModelValidationResult
                    {
                        Success = false,
                        Message = $"{nameof(ChargeIds)}: Empty Guids are not allowed."
                    };
                }
            }

            if (Usernames.HasItems())
            {
                var invalidStrings = Usernames.Where(x => string.IsNullOrWhiteSpace(x));

                if (invalidStrings.Any())
                {
                    return new ModelValidationResult
                    {
                        Success = false,
                        Message = $"{nameof(Usernames)}: Empty Strings are not allowed."
                    };
                }
            }

            return new ModelValidationResult
            {
                Success = true,
                Message = string.Empty
            };
        }
    }
}
