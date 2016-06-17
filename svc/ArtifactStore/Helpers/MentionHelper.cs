using HtmlLibrary;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ArtifactStore.Helpers
{
    internal class MentionHelper : IMentionValidator
    {

        internal readonly IUsersRepository UsersRepository;

        internal readonly IInstanceSettingsRepository InstanceSettingsRepository;

        private readonly MentionProcessor _mentionProcessor;

        public MentionHelper() : this(new SqlUsersRepository(), new SqlInstanceSettingsRepository())
        {
        }

        internal MentionHelper(IUsersRepository usersRepository, IInstanceSettingsRepository instanceSettingsRepository)
        {
            UsersRepository = usersRepository;
            InstanceSettingsRepository = instanceSettingsRepository;
            _mentionProcessor = new MentionProcessor(this);
        }

        //public bool AreEmailDiscussionsEnabled(int itemId)
        //{
        //    if (!GetInstanceEmailSettings().EnableEmailReplies)
        //    {
        //        return false;
        //    }

        //    //var projectId = _dataAccess.ProjectService.GetItemById(itemId, VersionableState.Removed).NodeProjectId.GetValueOrDefault();
        //    //return AreEmailDiscussionsEnabledForProject((int)projectId);
        //    return false;
        //}

        private EmailSettings _instanceEmailSettings;
        public async Task<bool> IsEmailBlocked(string email)
        {
            if (_instanceEmailSettings == null)
            {
                _instanceEmailSettings = await InstanceSettingsRepository.GetEmailSettings();
            }

            var user = (await UsersRepository.GetUsersByEmail(email, true)).FirstOrDefault();
            if ((user != null) && ((user.IsGuest && !user.IsEnabled) || (!CheckUsersEmailDomain(email, user.IsEnabled, user.IsGuest, _instanceEmailSettings))))
            {
                return true;
            }
            return false;
        }

        public async Task<string> ProcessComment(string comment, int itemId)
        {
            return await _mentionProcessor.ProcessComment(comment, false);
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
    }
}