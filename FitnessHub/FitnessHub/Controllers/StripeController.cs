using FitnessHub.Data.Classes;
using FitnessHub.Data.Entities;
using FitnessHub.Data.Entities.History;
using FitnessHub.Data.Entities.Users;
using FitnessHub.Data.HelperClasses;
using FitnessHub.Data.Repositories;
using FitnessHub.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace FitnessHub.Controllers
{
    public class StripeController : Controller
    {
        private readonly StripeSettings _stripeSettings;
        private readonly IMembershipRepository _membershipRepository;
        private readonly IMembershipDetailsRepository _membershipDetailsRepository;
        private readonly IUserHelper _userHelper;
        private readonly IClientMembershipHistoryRepository _clientMembershipHistoryRepository;
        private readonly IMailHelper _mailHelper;
        private readonly AppSettings _settings;

        public StripeController(IOptions<StripeSettings> stripeSettings, IMembershipRepository membershipRepository, IMembershipDetailsRepository membershipDetailsRepository, IUserHelper userHelper, IClientMembershipHistoryRepository clientMembershipHistoryRepository, IOptions<AppSettings> settings, IMailHelper mailHelper)
        {
            _stripeSettings = stripeSettings.Value;
            _membershipRepository = membershipRepository;
            _membershipDetailsRepository = membershipDetailsRepository;
            _userHelper = userHelper;
            _clientMembershipHistoryRepository = clientMembershipHistoryRepository;
            _mailHelper = mailHelper;
            _settings = settings.Value;
        }

        //public IActionResult Index()
        //{
        //    return View();
        //}

        public IActionResult MembershipCheckoutSession(string userId, string userEmail, string membershipName, int membershipId, string membershipDescription, decimal amount)
        {
            var currency = "usd";
            var successUrl = _settings.Url + $@"Memberships/MyMembership/{userId}";
            var cancelUrl = _settings.Url + $@"Memberships/MyMembership/{userId}";
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;

            //var customerService = new CustomerService();

            //var customer = customerService.Create(new CustomerCreateOptions
            //{
            //    Email = null
            //});

            var options = new SessionCreateOptions
            {
                CustomerEmail = userEmail,
                PaymentMethodTypes = new List<string>
                {
                    "card"
                },

                InvoiceCreation = new SessionInvoiceCreationOptions
                {
                    Enabled = true,
                },

                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = currency,
                            UnitAmount = Convert.ToInt32(amount) * 100,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"{membershipName}",
                                Description = membershipDescription,
                            }
                        },

                        Quantity = 1,
                    }
                },
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                //Customer = customer.Id,
                Metadata = new Dictionary<string, string>
                    {
                        { "membershipId", membershipId.ToString() },
                        { "userId", userId }
                    }
            };

            var service = new SessionService();
            var session = service.Create(options);

            Task.Delay(3000);

            return Redirect(session.Url);
        }

        [HttpPost("Stripe/Webhook")]
        public async Task<IActionResult> Webhook()
        {
            var json = await new StreamReader(Request.Body).ReadToEndAsync();
            var stripeSignature = Request.Headers["Stripe-Signature"];

            try
            {
                // Verify the event signature
                var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, _stripeSettings.WebhookSecret);

                // Handle the event type
                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Session;

                    // Check if session contains the necessary metadata
                    var membershipId = session.Metadata["membershipId"];
                    var userId = session.Metadata["userId"];

                    if (string.IsNullOrEmpty(membershipId) || string.IsNullOrEmpty(userId))
                    {
                        // Return bad request if metadata is missing
                        return BadRequest("Missing metadata");
                    }

                    // Look up the membership and client by IDs
                    var membership = await _membershipRepository.GetByIdTrackAsync(int.Parse(membershipId));

                    if (membership == null)
                    {
                        return BadRequest("Membership not found");
                    }

                    Client client = await _userHelper.GetUserByIdAsync(userId) as Client;

                    if (client == null)
                    {
                        return BadRequest("Client not found");
                    }

                    if (client.MembershipDetailsId != null)
                    {
                        var activeMembership = await _membershipDetailsRepository.GetByIdTrackAsync(client.MembershipDetailsId.Value);

                        if (activeMembership != null && activeMembership.Status == false)
                        {
                            // This session is to renew membership
                            activeMembership.Status = true;
                            activeMembership.DateRenewal = DateTime.UtcNow.AddMonths(12);

                            await _membershipDetailsRepository.UpdateAsync(activeMembership);

                            var record = await _clientMembershipHistoryRepository.GetByIdAsync(activeMembership.Id);

                            record.Status = true;
                            record.DateRenewal = activeMembership.DateRenewal;

                            await _clientMembershipHistoryRepository.UpdateAsync(record);

                            string classesUrl = Url.Action("AvailableClasses", "ClientClasses");
                            string body = _mailHelper.GetEmailTemplate($"Membership Sign Up", @$"Hey, {client.FirstName}, you have signed up for the membership plan <span style=""font-weight: bold"">{membership.Name}</span>. Enjoy it!", "Check our available classes");
                            Response response = await _mailHelper.SendEmailAsync(client.Email, "Membership sign up", body, null, null);
                        }
                        else
                        {
                            return BadRequest("Unhandled event type");
                        }
                    }
                    else
                    {
                        // This session is to assign a membership for the first time
                        MembershipDetails membershipDetails = new()
                        {
                            Membership = membership,
                            DateRenewal = DateTime.UtcNow.AddMonths(12),
                            SignUpDate = DateTime.UtcNow
                        };

                        client.MembershipDetails = membershipDetails;

                        await _membershipDetailsRepository.CreateAsync(membershipDetails);
                        await _userHelper.UpdateUserAsync(client);

                        var record = new ClientMembershipHistory
                        {
                            Id = membershipDetails.Id,
                            DateRenewal = membershipDetails.DateRenewal,
                            MembershipHistoryId = membership.Id,
                            UserId = client.Id,
                            SignUpDate = membershipDetails.SignUpDate
                        };

                        await _clientMembershipHistoryRepository.CreateAsync(record);
                    }
                }
                else
                {
                    return BadRequest("Unhandled event type");
                }

                return Ok();
            }
            catch (StripeException e)
            {
                return BadRequest($"Stripe error: {e.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        //public IActionResult Success()
        //{
        //    return View();
        //}

        //public IActionResult Cancel()
        //{
        //    return View();
        //}
    }
}
