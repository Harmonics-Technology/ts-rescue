using Stripe;
using System.Threading.Tasks;
using TimesheetBE.Services.ConnectedServices.Stripe.Resource;

namespace TimesheetBE.Services.ConnectedServices.Stripe
{
    public class StripeService : IStripeService
    {
        private readonly TokenService _tokenService;
        private readonly CustomerService _customerService;
        private readonly ChargeService _chargeService;
        private readonly CardService _cardService;
        public StripeService(TokenService tokenService, CustomerService customerService, ChargeService chargeService, CardService cardService)
        {
            _tokenService = tokenService;
            _customerService = customerService;
            _chargeService = chargeService;
            _cardService = cardService;
        }

        public async Task<Customer> CreateCustomer(CreateCustomerResource resource)
        {
            var customerOptions = new CustomerCreateOptions
            {
                Email = resource.Email,
                Name = resource.Name
            };
            var customer = await _customerService.CreateAsync(customerOptions);

            return customer;
        }

        public async Task<Card> CreateCard(string stripeCustomerId, CreateCardResource model)
        {
            var tokenOptions = new TokenCreateOptions
            {
                Card = new TokenCardOptions
                {
                    Name = model.Name,
                    Number = model.Number,
                    ExpYear = model.ExpiryYear,
                    ExpMonth = model.ExpiryMonth,
                    Cvc = model.Cvc,
                    AddressZip = model.PostalCode,
                    AddressCountry = model.Country
                }
            };
            var token = await _tokenService.CreateAsync(tokenOptions, null);

            var option = new CardCreateOptions { Source = token.Id };

            var card = await _cardService.CreateAsync(stripeCustomerId, option);
            return card;
        }

        public async Task<Customer> UpdateCustomerDefaultCard(string stripeCustomerId, string cardId)
        {
            var options = new CustomerUpdateOptions { DefaultSource = cardId };

            var customer = _customerService.Update(stripeCustomerId, options);
            return customer;
        }

        public async Task<Customer> UpdateCustomerDetail(string stripeCustomerId, string name, string email)
        {
            var options = new CustomerUpdateOptions { Name = name, Email = email };

            var customer = _customerService.Update(stripeCustomerId, options);
            return customer;
        }

        public async Task<StripeList<Card>> GetClientCards(string stripeCustomerId, int limit)
        {
            var options = new CardListOptions { Limit = limit };

            var cards = await _cardService.ListAsync(stripeCustomerId, options);
            return cards;
        }

        public async Task<bool> DeleteCard(string customerId, string cardId)
        {
            var card = await _cardService.DeleteAsync(customerId, cardId);
            if (card.Deleted == true) return true;
            return false;
        }
    }
}
