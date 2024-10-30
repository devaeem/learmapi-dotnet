using System.ComponentModel;

namespace LearmApi.Models
{
    public enum Role
    {

        [Description("Admin")]
        Admin,

        [Description("Manager")]
        Manager,

        [Description("User")]
        User,

    }
}
