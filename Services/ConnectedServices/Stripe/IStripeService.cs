using Stripe;
using System.Collections.Generic;
using System.Threading.Tasks;
using TimesheetBE.Services.ConnectedServices.Stripe.Resource;
using TimesheetBE.Utilities;

namespace TimesheetBE.Services.ConnectedServices.Stripe
{
    public interface IStripeService
    {
        Task<StandardResponse<string>> CreateCustomer(CreateCustomerResource resource);
        Task<CardView> CreateCard(string stripeCustomerId, CreateCardResource model);
        Task<bool> UpdateCustomerDefaultCard(string stripeCustomerId, string cardId);
        Task<CustomerView> UpdateCustomerDetail(string stripeCustomerId, string name, string email);
        Task<List<CardView>> GetClientCards(string stripeCustomerId, int limit);
        Task<bool> DeleteCard(string customerId, string cardId);
        Task<bool> CreateCharge(string customerId, CreateChargeResource resource);
    }
}
