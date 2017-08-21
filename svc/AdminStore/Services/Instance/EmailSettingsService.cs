using System.Threading.Tasks;
using AdminStore.Helpers;
using AdminStore.Models.Emails;
using AdminStore.Repositories;
using AdminStore.Models;
using AdminStore.Services.Email;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Helpers.Security;
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

            //TODO: Body and Subject need to be localized, this is temporary
            string blueprintUrl = "http://www.blueprintsys.net/";

            string body = $"A test email was requested from the Blueprint Instance Administration Console.<br/><br/>This email was sent to you as a registered <a href='{blueprintUrl}'>{blueprintUrl}</a> user from {_websiteAddressService.GetWebsiteAddress()}";
            string subject = "Blueprint Test Email";

            _emailHelper.SendEmail(currentUser.Email, subject, body);
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

            if (outgoingSettings.AuthenticatedSmtp)
            {
                if (string.IsNullOrWhiteSpace(outgoingSettings.AuthenticatedSmtpUsername))
                {
                    throw new BadRequestException("Please enter the SMTP administrator username.", ErrorCodes.EmptySmtpAdministratorUsername);
                }

                if (string.IsNullOrWhiteSpace(outgoingSettings.AuthenticatedSmtpPassword))
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
                ClientType = EmailClientType.Imap,
                AccountUsername = incomingSettings.AccountUsername,
                AccountPassword = incomingSettings.AccountPassword,
                EnableSsl = incomingSettings.EnableSsl
            };

            if (incomingSettings.IsPasswordDirty)
            {
                var emailSettings = await _instanceSettingsRepository.GetEmailSettings();

                emailClientConfig.AccountPassword = SystemEncryptions.DecryptFromSilverlight(emailSettings.IncomingPassword);
            }

            return emailClientConfig;
        }
    }
}
