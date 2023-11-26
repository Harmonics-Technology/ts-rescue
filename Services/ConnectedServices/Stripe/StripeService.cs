using Stripe;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TimesheetBE.Services.ConnectedServices.Stripe.Resource;
using TimesheetBE.Utilities;

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

        public async Task<StandardResponse<string>> CreateCustomer(CreateCustomerResource resource)
        {
            try
            {
                var customerOptions = new CustomerCreateOptions
                {
                    Email = resource.Email,
                    Name = resource.Name
                };
                var customer = await _customerService.CreateAsync(customerOptions);

                if (string.IsNullOrEmpty(customer.Id)) return null;

                return StandardResponse<string>.Ok(customer.Id);
            }
            catch(Exception ex)
            {
                return StandardResponse<string>.Error(ex.Message);
            }
            
        }

        private async Task<CustomerView> GetDefaultCard(string id)
        {
            var customer = await _customerService.GetAsync(id);
            if (customer == null) return null;

            var response = new CustomerView { Name = customer.Name, Email = customer.Email, DefaultSource = customer.DefaultSourceId };
            return response;
        }

        public async Task<CardView> CreateCard(string stripeCustomerId, CreateCardResource model)
        {
            //var tokenOptions = new TokenCreateOptions
            //{
            //    Card = new TokenCardOptions
            //    {
            //        Name = model.Name,
            //        Number = model.Number,
            //        ExpYear = model.ExpiryYear,
            //        ExpMonth = model.ExpiryMonth,
            //        Cvc = model.Cvc,
            //        AddressZip = model.PostalCode,
            //        AddressCountry = model.Country
            //    }
            //};
            //var token = await _tokenService.CreateAsync(tokenOptions);

            //var option = new CardCreateOptions { Source = token.Id };

            var option = new CardCreateOptions
            {
                Source = "tok_visa",
            };

            var card = await _cardService.CreateAsync(stripeCustomerId, option);

            if(card == null) return null;

            var customer = await GetDefaultCard(card.CustomerId);

            var userCard = new CardView { Id = card.Id, CustomerId = card.CustomerId, CustomerName = customer.Name, 
                CustomerEmail = customer.Email, Brand = card.Brand, LastFourDigit = card.Last4, IsDefaultCard = card.Id == customer.DefaultSource ? true : false };
  
            return userCard;
        }

        public async Task<bool> UpdateCustomerDefaultCard(string stripeCustomerId, string cardId)
        {
            var options = new CustomerUpdateOptions { DefaultSource = cardId };

            var customer = _customerService.Update(stripeCustomerId, options);

            if(customer == null) return false;
            return true;
        }

        public async Task<CustomerView> UpdateCustomerDetail(string stripeCustomerId, string name, string email)
        {
            var options = new CustomerUpdateOptions { Name = name, Email = email };

            var customer = _customerService.Update(stripeCustomerId, options);
            var customerView = new CustomerView { Name = customer.Name, Email = customer.Email, DefaultSource = null };
            return customerView;
        }

        public async Task<List<CardView>> GetClientCards(string stripeCustomerId, int limit)
        {
            var options = new CardListOptions { Limit = limit };

            var cards = await _cardService.ListAsync(stripeCustomerId, options);

            if(cards == null) return null;

            var cardList = new List<CardView>();
            foreach (var card in cards)
            {
                var customer = await GetDefaultCard(card.CustomerId);

                cardList.Add(new CardView { Id = card.Id, CustomerId = card.CustomerId, CustomerName = customer.Name, 
                    CustomerEmail = customer.Email, Brand = card.Brand, LastFourDigit = card.Last4, IsDefaultCard = card.Id == customer.DefaultSource ? true : false });
            }
            return cardList;
        }

        public async Task<bool> DeleteCard(string customerId, string cardId)
        {
            var card = await _cardService.DeleteAsync(customerId, cardId);
            if (card.Deleted == true) return true;
            return false;
        }
        public async Task<bool> CreateCharge(string customerId, CreateChargeResource resource)
        {
            var chargeOptions = new ChargeCreateOptions
            {
                Customer = customerId,
                Currency = resource.Currency,
                Amount = resource.Amount,
                ReceiptEmail = resource.ReceiptEmail,
                Source = resource.CardId,
                Description = resource.Description
            };

            var charge = await _chargeService.CreateAsync(chargeOptions);
            if(charge == null) return false;

            return true;
        }
    }
}
