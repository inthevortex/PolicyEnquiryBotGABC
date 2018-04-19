using System;

namespace PolicyEnquiryLuisBot
{
    [Serializable]
    public class PolicyServices
    {
        public string PolicyStatus()
        {
            return "Active";
        }

        public string[] PolicyDetails()
        {
            return new string[] { "10-10-2017", "10-10-2018", "123, 6th St., Melbourne, FL - 32904", "4, Goldfield Rd., Honolulu, HI - 96815", "5555444488", "True" };
        }

        public string[] PremiumDetails()
        {
            return new string[] { "03-10-2018", "$23" };
        }

        public string[] LastPaymentDetails()
        {
            return new string[] { "03-10-2018", "Paid", "$20" };
        }
    }
}
