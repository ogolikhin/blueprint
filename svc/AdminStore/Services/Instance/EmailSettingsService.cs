using System;
using System.Threading.Tasks;
using AdminStore.Helpers;
using AdminStore.Models.Emails;
using AdminStore.Repositories;
using AdminStore.Models;
using AdminStore.Services.Email;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Helpers.Security;
using ServiceLibrary.Helpers.Validators;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.InstanceSettings;
using ServiceLibrary.Services;

namespace AdminStore.Services.Instance
{
    public class EmailSettingsService : IEmailSettingsService
    {
        private readonly PrivilegesManager _privilegesManager;
        private readonly IUserRepository _userRepository;
        private readonly IEmailHelper _emailHelper;
        private readonly IWebsiteAddressService _websiteAddressService;
        private readonly IInstanceSettingsRepository _instanceSettingsRepository;
        private readonly IIncomingEmailService _incomingEmailService;

        private const string TestEmailSubject = "Blueprint Test Email";

        public EmailSettingsService() : this(new PrivilegesManager(new SqlPrivilegesRepository()),
                                             new SqlUserRepository(),
                                             new EmailHelper(),
                                             new WebsiteAddressService(),
                                             new SqlInstanceSettingsRepository(),
                                             new IncomingEmailService())
        {
        }

        public EmailSettingsService(PrivilegesManager privilegesManager, IUserRepository userRepository, IEmailHelper emailHelper, IWebsiteAddressService websiteAddressService, IInstanceSettingsRepository instanceSettingsRepository, IIncomingEmailService incomingEmailService)
        {
            _privilegesManager = privilegesManager;
            _userRepository = userRepository;
            _emailHelper = emailHelper;
            _websiteAddressService = websiteAddressService;
            _instanceSettingsRepository = instanceSettingsRepository;
            _incomingEmailService = incomingEmailService;
        }

        public async Task<EmailSettingsDto> GetEmailSettingsAsync(int userId)
        {
            await _privilegesManager.Demand(userId, InstanceAdminPrivileges.ViewInstanceSettings);

            EmailSettings settings = await _instanceSettingsRepository.GetEmailSettings();

            return (EmailSettingsDto) settings;
        }

        public async Task UpdateEmailSettingsAsync(int userId, EmailSettingsDto emailSettingsDto)
        {
            if (emailSettingsDto.Incoming == null)
            {
                throw new BadRequestException("Incoming cannot be null.", ErrorCodes.OutOfRangeParameter);
            }

            if (emailSettingsDto.Outgoing == null)
            {
                throw new BadRequestException("Outgoing cannot be null.", ErrorCodes.OutOfRangeParameter);
            }

            await _privilegesManager.Demand(userId, InstanceAdminPrivileges.ManageInstanceSettings);

            if (!emailSettingsDto.EnableEmailNotifications && emailSettingsDto.EnableDiscussions)
            {
                throw new BadRequestException("Cannot enable discussions without enabling email notifications", ErrorCodes.CannotEnableDiscussions);
            }

            if (emailSettingsDto.EnableDiscussions)
            {
                VerifyIncomingSettings(emailSettingsDto.Incoming);
            }

            if (emailSettingsDto.EnableReviewNotifications || emailSettingsDto.EnableEmailNotifications)
            {
                VerifyOutgoingSettings(emailSettingsDto.Outgoing);
            }

            var emailSettings = await _instanceSettingsRepository.GetEmailSettings();

            UpdateEmailSettings(emailSettings, emailSettingsDto);

            await _instanceSettingsRepository.UpdateEmailSettingsAsync(emailSettings);
        }

        private void UpdateEmailSettings(EmailSettings emailSettings, EmailSettingsDto emailSettingsDto)
        {
            //Notification settings
            emailSettings.EnableNotifications = emailSettingsDto.EnableReviewNotifications;
            emailSettings.EnableEmailReplies = emailSettingsDto.EnableDiscussions;
            emailSettings.EnableEmailDiscussion = emailSettingsDto.EnableEmailNotifications;

            //Incoming settings
            emailSettings.IncomingServerType = (int)emailSettingsDto.Incoming.ServerType;
            emailSettings.IncomingHostName = emailSettingsDto.Incoming.ServerAddress;
            emailSettings.IncomingPort = emailSettingsDto.Incoming.Port;
            emailSettings.IncomingEnableSSL = emailSettingsDto.Incoming.EnableSsl;
            emailSettings.IncomingUserName = emailSettingsDto.Incoming.AccountUsername;

            if (emailSettingsDto.Incoming.IsPasswordDirty)
            {
                emailSettings.IncomingPassword = SystemEncryptions.EncryptForSilverLight(emailSettingsDto.Incoming.AccountPassword);
            }

            //Outgoing settings
            emailSettings.HostName = emailSettingsDto.Outgoing.ServerAddress;
            emailSettings.Port = emailSettingsDto.Outgoing.Port;
            emailSettings.EnableSSL = emailSettingsDto.Outgoing.EnableSsl;
            emailSettings.SenderEmailAddress = emailSettingsDto.Outgoing.AccountEmailAddress;
            emailSettings.Authenticated = emailSettingsDto.Outgoing.AuthenticatedSmtp;
            emailSettings.UserName = emailSettingsDto.Outgoing.AccountUsername;

            if (emailSettingsDto.Outgoing.IsPasswordDirty)
            {
                emailSettings.Password = SystemEncryptions.EncryptForSilverLight(emailSettingsDto.Outgoing.AccountPassword);
            }
        }

        public async Task SendTestEmailAsync(int userId, EmailOutgoingSettings outgoingSettings)
        {
            await _privilegesManager.Demand(userId, InstanceAdminPrivileges.ManageInstanceSettings);

            VerifyOutgoingSettings(outgoingSettings);

            var currentUser = await _userRepository.GetUserAsync(userId);

            if (string.IsNullOrWhiteSpace(currentUser.Email))
            {
                throw new ConflictException("Your user profile does not include an email address. Please add one to receive a test email.", ErrorCodes.UserHasNoEmail);
            }

            var config = await GetEmailConfigAsync(outgoingSettings, currentUser);

            _emailHelper.Initialize(config);

            string body = EmailTemplateHelper.GetSendTestEmailTemplate(_websiteAddressService.GetWebsiteAddress());

            try
            {
                _emailHelper.SendEmail(currentUser.Email, TestEmailSubject, body);
            }
            catch (EmailException ex)
            {
                throw new BadRequestException(ex.Message, ex.ErrorCode);
            }
        }

        private async Task<IEmailConfigInstanceSettings> GetEmailConfigAsync(EmailOutgoingSettings outgoingSettings, User currentUser)
        {
            var config = new TestEmailConfigInstanceSettings(outgoingSettings, currentUser.Email);

            if (!outgoingSettings.AuthenticatedSmtp)
            {
                config.UserName = string.Empty;
                config.Password = string.Empty;
            }
            else if (!outgoingSettings.IsPasswordDirty)
            {
                var emailSettings = await _instanceSettingsRepository.GetEmailSettings();

                config.Password = SystemEncryptions.DecryptFromSilverlight(emailSettings.Password);
            }

            return config;
        }

        private void VerifyOutgoingSettings(EmailOutgoingSettings outgoingSettings)
        {
            if (string.IsNullOrWhiteSpace(outgoingSettings.ServerAddress))
            {
                throw new BadRequestException("Please enter a mail server.", ErrorCodes.OutgoingEmptyMailServer);
            }

            if (outgoingSettings.Port < 1 || outgoingSettings.Port > 65535)
            {
                throw new BadRequestException("Ensure the port number is between 1 and 65535.", ErrorCodes.OutgoingPortOutOfRange);
            }

            if (string.IsNullOrWhiteSpace(outgoingSettings.AccountEmailAddress))
            {
                throw new BadRequestException("Please enter the system email account address.", ErrorCodes.EmptyEmailAddress);
            }

            if (!EmailValidator.IsEmailAddress(outgoingSettings.AccountEmailAddress))
            {
                throw new BadRequestException("The system email account address is not in a valid format.", ErrorCodes.InvalidEmailAddress);
            }

            if (outgoingSettings.AuthenticatedSmtp)
            {
                if (string.IsNullOrWhiteSpace(outgoingSettings.AccountUsername))
                {
                    throw new BadRequestException("Please enter the SMTP administrator username.", ErrorCodes.EmptySmtpAdministratorUsername);
                }

                if (string.IsNullOrWhiteSpace(outgoingSettings.AccountPassword))
                {
                    throw new BadRequestException("Please enter the SMTP administrator password.", ErrorCodes.EmptySmtpAdministratorPassword);
                }
            }
        }

        public async Task TestIncomingEmailConnectionAsync(int userId, EmailIncomingSettings incomingSettings)
        {
            await _privilegesManager.Demand(userId, InstanceAdminPrivileges.ManageInstanceSettings);

            VerifyIncomingSettings(incomingSettings);

            var emailClientConfig = await GetEmailClientConfig(incomingSettings);

            _incomingEmailService.TryConnect(emailClientConfig);
        }

        private void VerifyIncomingSettings(EmailIncomingSettings incomingSettings)
        {
            if (string.IsNullOrWhiteSpace(incomingSettings.ServerAddress))
            {
                throw new BadRequestException("Please enter a mail server.", ErrorCodes.IncomingEmptyMailServer);
            }

            if (incomingSettings.Port < 1 || incomingSettings.Port > 65535)
            {
                throw new BadRequestException("Ensure the port number is between 1 and 65535.", ErrorCodes.IncomingPortOutOfRange);
            }

            if (string.IsNullOrWhiteSpace(incomingSettings.AccountUsername))
            {
                throw new BadRequestException("Please enter the system email account username.", ErrorCodes.EmptyEmailUsername);
            }

            if (string.IsNullOrWhiteSpace(incomingSettings.AccountPassword))
            {
                throw new BadRequestException("Please enter the system email account username.", ErrorCodes.EmptyEmailPassword);
            }
        }

        private async Task<EmailClientConfig> GetEmailClientConfig(EmailIncomingSettings incomingSettings)
        {
            var emailClientConfig = new EmailClientConfig()
            {
                ServerAddress = incomingSettings.ServerAddress,
                Port = incomingSettings.Port,
                ClientType = incomingSettings.ServerType,
                AccountUsername = incomingSettings.AccountUsername,
                AccountPassword = incomingSettings.AccountPassword,
                EnableSsl = incomingSettings.EnableSsl
            };

            if (!incomingSettings.IsPasswordDirty)
            {
                var emailSettings = await _instanceSettingsRepository.GetEmailSettings();

                emailClientConfig.AccountPassword = SystemEncryptions.DecryptFromSilverlight(emailSettings.IncomingPassword);
            }

            return emailClientConfig;
        }
    }
}
