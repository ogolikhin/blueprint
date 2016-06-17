using HtmlLibrary;
using ServiceLibrary.Repositories;
using System;
using System.Globalization;
using System.Linq;

namespace ArtifactStore.Helpers
{
    internal class MentionHelper
    {

        internal readonly IUsersRepository UsersRepository;

        private readonly MentionProcessor _mentionProcessor;

        public MentionHelper() : this(new SqlUsersRepository())
        {
        }

        internal MentionHelper(IUsersRepository usersRepository)
        {
            UsersRepository = usersRepository;
            _mentionProcessor = new MentionProcessor();
        }

        private DEmailSettings _instanceEmailSettings;
        public DEmailSettings GetInstanceEmailSettings()
        {
            if (_instanceEmailSettings == null)
            {
                //Call to database
                _instanceEmailSettings = new DEmailSettings();
            }
            return _instanceEmailSettings;
        }

        public bool AreEmailDiscussionsEnabled(int itemId)
        {
            if (!GetInstanceEmailSettings().EnableEmailReplies)
            {
                return false;
            }

            //var projectId = _dataAccess.ProjectService.GetItemById(itemId, VersionableState.Removed).NodeProjectId.GetValueOrDefault();
            //return AreEmailDiscussionsEnabledForProject((int)projectId);
            return false;
        }

        public bool IsEmailBlocked(string email)
        {
            var instanceEmailSettings = GetInstanceEmailSettings();
            var user = UsersRepository.GetUsersByEmail(email, true).Result.SingleOrDefault();
            if ((user != null) && ((user.IsGuest && !user.IsEnabled) || (!CheckUsersEmailDomain(email, user.IsEnabled, user.IsGuest, instanceEmailSettings))))
            {
                return true;
            }
            return false;
        }

        public string ProcessComment(string comment, int itemId)
        {
            return _mentionProcessor.ProcessComment(comment, false, IsEmailBlocked);
        }

        /// <summary>
        /// Checks the user's email domain. 
        /// 
        /// Returns true if email domains are enabled and if a guest user has an acceptable domain.
        /// 
        /// Note that permissible email domains are defined within 
        ///     Main > Manage > Instance Administration > Email Settings > Edit Settings
        /// </summary>
        internal bool CheckUsersEmailDomain(string email, bool isUserEnabled, bool isGuest, DEmailSettings emailSettings)
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

    //[DataContract(Name = "DEmailSettings", Namespace = "http://schemas.datacontract.org/2004/07/BluePrintSys.RC.Data.AccessAPI.Model.Serializable")]
    internal class DEmailSettings
    {
        public string Domains { get; set; }
        public bool EnableAllUsers { get; set; }
        public bool EnableDomains { get; set; }
        public bool EnableEmailReplies { get; set; }
    }
}