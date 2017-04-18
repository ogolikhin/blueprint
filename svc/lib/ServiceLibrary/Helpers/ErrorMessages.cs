﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLibrary.Helpers
{
    public static  class ErrorMessages
    {
        public static readonly string LoginRequired = "The \"Login\" field is required.";
        public static readonly string DisplayNameRequired = "The \"Display name\" field is required.";
        public static readonly string FirstNameRequired = "The \"First name\" field is required.";
        public static readonly string LastNameRequired = "The \"Last name\" field is required.";
        public static readonly string LoginNameUnique = "The login name must be unique in the instance.";
        public static readonly string SessionIsEmpty = "The session is empty.";
        public static readonly string UserDoesNotHavePermissions = "The user does not have permissions.";
        public static readonly string LoginFieldLimitation = "The length of the \"Login\" field must be between 4 and 256 characters.";
        public static readonly string DisplayNameFieldLimitation = "The length of the \"Display name\" field must be between 2 and 255 characters.";
        public static readonly string FirstNameFieldLimitation = "The length of the \"First name\" field must be between 2 and 255 characters.";
        public static readonly string LastNameFieldLimitation = "The length of the \"Last name\" field must be between 2 and 255 characters.";
        public static readonly string EmailFieldLimitation = "The length of the \"Email\" field must be between 4 and 255 characters.";
        public static readonly string TitleFieldLimitation = "The length of the \"Title\" field must be between 2 and 255 characters.";
        public static readonly string DepartmentFieldLimitation = "The length of the \"Department\" field must be between 1 and 255 characters.";
        public static readonly string UserModelIsEmpty = "The user model is empty.";
    }
}
