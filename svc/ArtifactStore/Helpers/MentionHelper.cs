using HtmlLibrary;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ServiceLibrary.Repositories.InstanceSettings;

namespace ArtifactStore.Helpers
{
    internal class MentionHelper : IMentionValidator
    {
        private readonly IUsersRepository _usersRepository;
        private readonly IInstanceSettingsRepository _instanceSettingsRepository;
        private readonly IArtifactPermissionsRepository _permissionsRepository;
        private readonly MentionProcessor _mentionProcessor;

        public MentionHelper() : this(new SqlUsersRepository(), new SqlInstanceSettingsRepository(), new SqlArtifactPermissionsRepository())
        {
        }

        internal MentionHelper(IUsersRepository usersRepository,
            IInstanceSettingsRepository instanceSettingsRepository,
            IArtifactPermissionsRepository permissionsRepository)
        {
            _usersRepository = usersRepository;
            _instanceSettingsRepository = instanceSettingsRepository;
            _permissionsRepository = permissionsRepository;
            _mentionProcessor = new MentionProcessor(this);
        }

        public async Task<bool> IsEmailBlocked(string email)
        {
            var emailSettings = await GetInstanceEmailSettings();
            var user = await GetUserByEmail(email);
            if ((user != null) && ((user.IsGuest && !user.IsEnabled) || (!CheckUsersEmailDomain(email, user.IsEnabled, user.IsGuest, emailSettings))))
            {
                return true;
            }
            return false;
        }

        public async Task<bool> AreEmailDiscussionsEnabled(int projectId)
        {
            var emailSettings = await GetInstanceEmailSettings();
            if (!emailSettings.EnableEmailReplies)
            {
                return false;
            }

            var permissions = await _permissionsRepository.GetProjectPermissions(projectId);
            return permissions.HasFlag(ProjectPermissions.AreEmailRepliesEnabled);
        }

        public async Task<string> ProcessComment(string comment, bool areEmailDiscussionsEnabled)
        {
            return await _mentionProcessor.ProcessComment(comment, areEmailDiscussionsEnabled);
        }

        private IDictionary<string, UserInfo> _usersByEmail;

        private async Task<UserInfo> GetUserByEmail(string email)
        {
            UserInfo user;
            if (_usersByEmail == null)
            {
                _usersByEmail = new Dictionary<string, UserInfo>();
            }
            if (!_usersByEmail.TryGetValue(email, out user))
            {
                user = (await _usersRepository.GetUsersByEmail(email, true)).FirstOrDefault();
                _usersByEmail.Add(email, user);
            }
            return user;
        }

        /// <summary>
        /// Checks the user's email domain.
        ///
        /// Returns true if email domains are enabled and if a guest user has an acceptable domain.
        ///
        /// Note that permissible email domains are defined within
        ///     Main > Manage > Instance Administration > Email Settings > Edit Settings
        /// </summary>
        internal bool CheckUsersEmailDomain(string email, bool isUserEnabled, bool isGuest, EmailSettings emailSettings)
        {
            if (!isGuest)
                return true;

            if (!isUserEnabled)
                return false;

            if (emailSettings == null)
                return false;

            if (!emailSettings.EnableAllUsers)
                return false;

            if (!emailSettings.EnableDomains)
                return true;

            var domains = emailSettings.Domains
                .Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => string.Format(CultureInfo.CurrentCulture, "@{0}", s))
                .ToArray();

            return domains.Any(email.EndsWith);
        }

        private EmailSettings _instanceEmailSettings;
        internal async Task<EmailSettings> GetInstanceEmailSettings()
        {
            if (_instanceEmailSettings == null)
            {
                _instanceEmailSettings = await _instanceSettingsRepository.GetEmailSettings();
            }
            return _instanceEmailSettings;
        }
    }
}