/*
*************************************************************************
DC EMV
Open Source EMV
Copyright (C) 2018  Vicente Da Silva

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see http://www.gnu.org/licenses/
*************************************************************************
*/
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Collections.Generic;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using System;

namespace DCEMV.DemoServer.Components
{
    // This class is used by the application to send Email and SMS
    // when you turn on two-factor authentication in ASP.NET Identity.
    // For more details see this link http://go.microsoft.com/fwlink/?LinkID=532713
    public class AuthMessageSender : IEmailSender, ISmsSender
    {
        private readonly ILogger<AuthMessageSender> _logger;
        private const string emailKey = "";
        private const string smsKey = "";
        private const string smsAuthToken = "";
        private const string smsFromNumber = "";

        public AuthMessageSender(ILogger<AuthMessageSender> logger)
        {
            _logger = logger;
        }
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                _logger.LogInformation("Will Send Email: {email}, Subject: {subject}, Message: {message}", email, subject, message);

#if EnableSendGrid
                SendGridMessage msg = new SendGridMessage();

                msg.SetFrom(new EmailAddress("email@payloola.com", "Payloola"));

                List<EmailAddress> recipients = new List<EmailAddress>{new EmailAddress(email)};
                msg.AddTos(recipients);

                msg.SetSubject(subject);

                msg.AddContent(MimeType.Text, message);

                //var apiKey = System.Environment.GetEnvironmentVariable("SENDGRID_APIKEY");
                SendGridClient client = new SendGridClient(emailKey);
                Response response = await client.SendEmailAsync(msg);
                if (response.StatusCode != System.Net.HttpStatusCode.OK && response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    _logger.LogError("Could not send email:" + response.StatusCode);
                    throw new Exception("Could not send email:" + response.StatusCode);
                }
#endif
            }
            catch (Exception ex)
            {
                _logger.LogError("Could not send email:" + ex.Message);
                throw ex;
            }
        }

        public async Task SendSmsAsync(string number, string message)
        {
            try
            {
                number = number.TrimStart('0');
                number = "+27" + number;

#if EnableTwilio
                MessageResource mr;
                TwilioClient.Init(smsKey, smsAuthToken);
                mr = await MessageResource.CreateAsync(
                    to: new PhoneNumber(number),
                    from: new PhoneNumber(smsFromNumber),
                    body: message);
#endif

                _logger.LogInformation("SMS: {number}, Message: {message}", number, message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Could not send sms:" + ex.Message);
                throw ex;
            }
        }
    }
}
