using System;

namespace ServiceLibrary.Attributes
{
    public class PreRequiredPrivilegeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PreRequiredPrivilegeAttribute"/> class.
        /// </summary>
        public PreRequiredPrivilegeAttribute(int preRequiredPrivilege)
        {
            PreRequiredPrivilege = preRequiredPrivilege;
        }

        /// <summary>
        /// Gets the pre required privilege.
        /// </summary>
        public int PreRequiredPrivilege { get; private set; }
    }
}
