using System.Threading.Tasks;
using AdminStore.Helpers;
using AdminStore.Models.Emails;
using AdminStore.Repositories;
using AdminStore.Models;
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

        private const string TestEmailSubject = "Blueprint Test Email";

        public EmailSettingsService() : this(new PrivilegesManager(new SqlPrivilegesRepository()),
                                             new SqlUserRepository(),
                                             new EmailHelper(),
                                             new WebsiteAddressService(),
                                             new SqlInstanceSettingsRepository())
        {
        }

        public EmailSettingsService(PrivilegesManager privilegesManager, IUserRepository userRepository, IEmailHelper emailHelper, IWebsiteAddressService websiteAddressService, IInstanceSettingsRepository instanceSettingsRepository)
        {
            _privilegesManager = privilegesManager;
            _userRepository = userRepository;
            _emailHelper = emailHelper;
            _websiteAddressService = websiteAddressService;
            _instanceSettingsRepository = instanceSettingsRepository;
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

            _emailHelper.SendEmail(currentUser.Email, TestEmailSubject, body);
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
                throw new BadRequestException("Please enter a mail server.", ErrorCodes.EmptyMailServer);
            }

            if (outgoingSettings.Port < 1 || outgoingSettings.Port > 65535)
            {
                throw new BadRequestException("Ensure the port number is between 1 and 65535.", ErrorCodes.PortOutOfRange);
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
    }
}
