using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdminStore.Helpers;
using AdminStore.Models.Emails;
using AdminStore.Repositories;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

namespace AdminStore.Services.Instance
{
    public class EmailSettingsService : IEmailSettingsService
    {
        private readonly PrivilegesManager _privilegesManager;
        private readonly IUserRepository _userRepository;
        private readonly IEmailHelper _emailHelper;

        public EmailSettingsService() : this(new PrivilegesManager(new SqlPrivilegesRepository()),
                                             new SqlUserRepository(),
                                             new EmailHelper())
        {
        }

        public EmailSettingsService(PrivilegesManager privilegesManager, IUserRepository userRepository, IEmailHelper emailHelper)
        {
            _privilegesManager = privilegesManager;
            _userRepository = userRepository;
            _emailHelper = emailHelper;
        }

        public async Task SendTestEmailAsync(int userId, EmailOutgoingSettings outgoingSettings)
        {
            await _privilegesManager.Demand(userId, InstanceAdminPrivileges.ManageInstanceSettings);

            VerifyOutgoingSettings(outgoingSettings);

            var currentUser = await _userRepository.GetUserAsync(userId);

            if (String.IsNullOrWhiteSpace(currentUser.Email))
            {
                throw new ConflictException("Your user profile does not include an email address. Please add one to receive a test email.", ErrorCodes.UserHasNoEmail);
            }

            _emailHelper.Initialize(new TestEmailConfigInstanceSettings(outgoingSettings, currentUser.Email));

            _emailHelper.SendEmail(currentUser.Email, "Blueprint Test Email",
                @"A test email was requested from the Blueprint Instance Administration Console.<br/><br/>This email was sent to you as a registered Blueprint user from Website");
        }

        private void VerifyOutgoingSettings(EmailOutgoingSettings outgoingSettings)
        {
            if (String.IsNullOrWhiteSpace(outgoingSettings.ServerAddress))
            {
                throw new BadRequestException("Please enter a mail server.", ErrorCodes.EmptyMailServer);
            }

            if (outgoingSettings.Port < 1 || outgoingSettings.Port > 65535)
            {
                throw new BadRequestException("Ensure the port number is between 1 and 65535.", ErrorCodes.PortOutOfRange);
            }

            if (outgoingSettings.AuthenticatedSmtp)
            {
                if (String.IsNullOrWhiteSpace(outgoingSettings.AuthenticatedSmtpUsername))
                {
                    throw new BadRequestException("Please enter the SMTP administrator username.", ErrorCodes.EmptySmtpAdministratorUsername);
                }

                if (String.IsNullOrWhiteSpace(outgoingSettings.AuthenticatedSmtpPassword))
                {
                    throw new BadRequestException("Please enter the SMTP administrator password.", ErrorCodes.EmptySmtpAdministratorPassword);
                }
            }
        }
    }
}
