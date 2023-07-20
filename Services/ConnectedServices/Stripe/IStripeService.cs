using Stripe;
using System.Threading.Tasks;
using TimesheetBE.Services.ConnectedServices.Stripe.Resource;

namespace TimesheetBE.Services.ConnectedServices.Stripe
{
    public interface IStripeService
    {
        Task<Customer> CreateCustomer(CreateCustomerResource resource);
        Task<Card> CreateCard(string stripeCustomerId, CreateCardResource model);
        Task<Customer> UpdateCustomerDefaultCard(string stripeCustomerId, string cardId);
        Task<Customer> UpdateCustomerDetail(string stripeCustomerId, string name, string email);
        Task<StripeList<Card>> GetClientCards(string stripeCustomerId, int limit);
        Task<bool> DeleteCard(string customerId, string cardId);
    }
}
