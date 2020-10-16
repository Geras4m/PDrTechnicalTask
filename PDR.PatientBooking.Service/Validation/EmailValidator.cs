using System.ComponentModel.DataAnnotations;

namespace PDR.PatientBooking.Service.Validation
{
    public static class EmailValidator
    {
        public static bool IsValidEmail(string email)
        {
            if (new EmailAddressAttribute().IsValid(email))
                return true;

            return false;
        }
    }
}
