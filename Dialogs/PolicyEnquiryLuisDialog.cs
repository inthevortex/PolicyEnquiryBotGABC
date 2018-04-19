using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using static PolicyEnquiryLuisBot.SSMLHelper;

namespace PolicyEnquiryLuisBot
{
    [Serializable]
    public class PolicyEnquiryLuisDialog : LuisDialog<object>
    {
        private PolicyServices policyServices = new PolicyServices();

        public PolicyEnquiryLuisDialog(bool spellCheck = false, string spellCheckAPIKey = "") : base(new LuisService(new LuisModelAttribute(
            ConfigurationManager.AppSettings["LuisAppId"],
            ConfigurationManager.AppSettings["LuisAPIKey"],
            domain: ConfigurationManager.AppSettings["LuisAPIHostName"])
            { SpellCheck = spellCheck, BingSpellCheckSubscriptionKey = spellCheckAPIKey } ))
        {
        }

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
            var message = context.MakeMessage();
            message.Text = "Sorry, I did not understand that. Can you be more precise?";
            message.Speak = Speak("Sorry, I did not understand that. Can you be more precise?");
            message.InputHint = InputHints.ExpectingInput;

            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("Welcome")]
        public async Task WelcomeIntent(IDialogContext context, LuisResult result)
        {
            var card = new HeroCard()
            {
                Title = "Hi! I am Polen.",
                Subtitle = "Nice to meet you. How may I help you?",
                Text = "I am answer some quick questions rearding your policy.",
                Buttons = new List<CardAction>()
                {
                    new CardAction(ActionTypes.ImBack, "Policy Information", value: "policy details"),
                    new CardAction(ActionTypes.ImBack, "Premium Information", value: "premium information"),
                    new CardAction(ActionTypes.ImBack, "Policy Status", value: "policy status"),
                    new CardAction(ActionTypes.ImBack, "Last Payment Details", value: "last payment details")
                },
                Tap = new CardAction(ActionTypes.ImBack, "Welcome", value: "hi")
            };
            var message = context.MakeMessage();

            message.Attachments = new List<Attachment>()
            {
                card.ToAttachment()
            };
            message.Speak = Speak("Hi! I am Polen. Nice to meet you. How may I help you? I can answer quick questions regarding your policy.");
            message.InputHint = InputHints.ExpectingInput;

            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("Cancel")]
        public async Task CancelIntent(IDialogContext context, LuisResult result)
        {
            var message = context.MakeMessage();
            message.Text = "I hope I was able to help you out. See you soon. Have a nice day!";
            message.Speak = Speak("I hope I was able to help you out. See you soon. Have a nice day!");
            message.InputHint = InputHints.IgnoringInput;

            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("PolicyStatus")]
        public async Task PolicyStatusIntent(IDialogContext context, LuisResult result)
        {
            var card = new HeroCard("Policy Information", "Policy Status", string.Format("Your policy is {0}", policyServices.PolicyStatus()), tap: new CardAction(ActionTypes.ImBack, "Welcome", value: "Hi"));
            var message = context.MakeMessage();

            message.Attachments = new List<Attachment>()
            {
                card.ToAttachment()
            };
            message.Speak = Speak(string.Format("Your policy is {0}", policyServices.PolicyStatus()));
            message.InputHint = InputHints.AcceptingInput;

            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("PolicyDetails")]
        public async Task PolicyDetailsIntent(IDialogContext context, LuisResult result)
        {
            var details = policyServices.PolicyDetails();
            EntityRecommendation entity;
            var card = new HeroCard("Policy Information", "Policy Details", tap: new CardAction(ActionTypes.ImBack, "Welcome", value: "Hi"));
            var message = context.MakeMessage();

            if (result.TryFindEntity("PolicyDetail", out entity) || !result.Entities.Any())
            {
                card.Text = "What all details do you want to know about the policy?";
                card.Buttons = new List<CardAction>()
                {
                    // new CardAction(ActionTypes.ImBack, "All Details", displayText: string.Format("Your policy details are: \nStart Date: {0}\nEnd Date: {1}\nContact Address: {2}\nBilling Address: {3}\nPhone Number: {4}\nAuto Renewal: {5}", details[0], details[1], details[2], details[3], details[4], details[5])),
                    new CardAction(ActionTypes.ImBack, "Start Date", value: "start date"),
                    new CardAction(ActionTypes.ImBack, "End Date", value: "end date"),
                    new CardAction(ActionTypes.ImBack, "Contact Address", value: "contact address"),
                    new CardAction(ActionTypes.ImBack, "Billing Address", value: "billing address"),
                    new CardAction(ActionTypes.ImBack, "Phone Number", value: "phone number"),
                    new CardAction(ActionTypes.ImBack, "Auto Renewal", value: "auto renewal")
                };
                message.Speak = Speak("What all details do you want to know about the policy?");
            }
            else if (result.TryFindEntity("StartDate", out entity))
            {
                card.Text = string.Format("Your policy start date is {0}", details[0]);
                card.Subtitle = "Policy Start Date";
                message.Speak = Speak(string.Format("Your policy start date is {0}", details[0]));
            }
            else if (result.TryFindEntity("EndDate", out entity))
            {
                card.Text = string.Format("Your policy end date is {0}", details[1]);
                card.Subtitle = "Policy End Date";
                message.Speak = Speak(string.Format("Your policy end date is {0}", details[1]));
            }
            else if (result.TryFindEntity("ContactAddress", out entity))
            {
                card.Text = string.Format("Your policy contact address is {0}", details[2]);
                card.Subtitle = "Contact Address";
                message.Speak = Speak(string.Format("Your policy contact address is {0}", details[2]));
            }
            else if (result.TryFindEntity("BillingAddress", out entity))
            {
                card.Text = string.Format("Your policy billing address is {0}", details[3]);
                card.Subtitle = "Billing Address";
                message.Speak = Speak(string.Format("Your policy billing address is {0}", details[3]));
            }
            else if (result.TryFindEntity("PhoneNumber", out entity))
            {
                card.Text = string.Format("Your policy phone number is {0}", details[4]);
                card.Subtitle = "Phone Number";
                message.Speak = Speak(string.Format("Your policy phone number is {0}", details[4]));
            }
            else if (result.TryFindEntity("AutoRenewal", out entity))
            {
                card.Text = string.Format("Your policy auto renewal state is {0}", details[5]);
                card.Subtitle = "Auto Renewal";
                message.Speak = Speak(string.Format("Your policy auto renewal state is {0}", details[5]));
            }

            message.Attachments = new List<Attachment>()
            {
                card.ToAttachment()
            };
            message.InputHint = InputHints.AcceptingInput;

            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("PremiumDueDetails")]
        public async Task PremiumDueDetailsIntent(IDialogContext context, LuisResult result)
        {
            var details = policyServices.PremiumDetails();
            EntityRecommendation entity;
            var card = new HeroCard("Premium Information", tap: new CardAction(ActionTypes.ImBack, "Welcome", value: "Hi"));
            var message = context.MakeMessage();

            if (result.TryFindEntity("PremiumDetails", out entity) || !result.Entities.Any())
            {
                card.Text = "What all premium details do you need?";
                card.Buttons = new List<CardAction>()
                {
                    // new CardAction(ActionTypes.ImBack, "All Details", displayText: string.Format("Your premium details are: \nPremium Due Date: {0}\nPremium Amount: {1}", details[0], details[1])),
                    new CardAction(ActionTypes.ImBack, "Due Date", value: "premium due date"),
                    new CardAction(ActionTypes.ImBack, "Amount", value: "premium amount")
                };
                message.Speak = Speak("What all premium details do you need?");
            }
            else if (result.TryFindEntity("PremiumDueDate", out entity))
            {
                card.Text = string.Format("Your premium due date is {0}", details[0]);
                card.Subtitle = "Premium Due Date";
                message.Speak = Speak(string.Format("Your premium due date is {0}", details[0]));
            }
            else if (result.TryFindEntity("PremiumAmount", out entity))
            {
                card.Text = string.Format("Your premium amount is {0}", details[1]);
                card.Subtitle = "Premium Amount";
                message.Speak = Speak(string.Format("Your premium amount is {0}", details[1]));
            }

            message.Attachments = new List<Attachment>()
            {
                card.ToAttachment()
            };
            message.InputHint = InputHints.AcceptingInput;

            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("LastPaymentDetails")]
        public async Task LastPaymentDetailsIntent(IDialogContext context, LuisResult result)
        {
            var details = policyServices.LastPaymentDetails();
            EntityRecommendation entity;
            var card = new HeroCard("Payment Details", tap: new CardAction(ActionTypes.ImBack, "Welcome", value: "Hi"));
            var message = context.MakeMessage();

            if (result.TryFindEntity("LastPaymentDetail", out entity) || !result.Entities.Any())
            {
                card.Text = "What details do you need about your last payment?";
                card.Buttons = new List<CardAction>()
                {
                    // new CardAction(ActionTypes.ImBack, "All Details", displayText: string.Format("Your last payment details are: \nLast Payment Date: {0}\nLast Payment Status: {1}", details[0], details[1])),
                    new CardAction(ActionTypes.ImBack, "Last Payment Date", value: "when did i pay the premium"),
                    new CardAction(ActionTypes.ImBack, "Last Payment Status", value: "my last payment status"),
                    new CardAction(ActionTypes.ImBack, "Last Payment Amount", value: "last payment amount")
                };
                message.Speak = Speak("What details do you need about your last payment?");
            }
            else if (result.TryFindEntity("LastPaymentDate", out entity))
            {
                card.Text = string.Format("Your last payment date is {0}", details[0]);
                card.Subtitle = "Last Payment Date";
                message.Speak = Speak(string.Format("Your last payment date is {0}", details[0]));
            }
            else if (result.TryFindEntity("LastPaymentStatus", out entity))
            {
                card.Text = string.Format("Your last payment status is {0}", details[1]);
                card.Subtitle = "Last Payment Status";
                message.Speak = Speak(string.Format("Your last payment status is {0}", details[1]));
            }
            else if (result.TryFindEntity("LastPaymentAmount", out entity))
            {
                card.Text = string.Format("Your last payment amount is {0}", details[2]);
                card.Subtitle = "Last Payment Amount";
                message.Speak = Speak(string.Format("Your last payment amount is {0}", details[2]));
            }

            message.Attachments = new List<Attachment>()
            {
                card.ToAttachment()
            };
            message.InputHint = InputHints.AcceptingInput;

            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }
    }
}