using EFEntityEntryTesting.Enums;

using Microsoft.EntityFrameworkCore;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace EFEntityEntryTesting.EF
{
    [PrimaryKey(nameof(UserId), nameof(Platform))]
    [DebuggerDisplay("UserId={UserId}, UserName={UserName}, LastSeen={LastDateSeen}")]
    public class Users(DateTime firstDateSeen = default,
    DateTime currLoginDate = default,
                       DateTime lastDateSeen = default,
                       string userId = null,
                       string userName = null,
                       Platform platform = Platform.Default)
 : UserBase(userId, platform)
    {
        public string UserName { get; set; } = userName;
        public DateTime FirstDateSeen { get; set; } = firstDateSeen;
        public DateTime CurrLoginDate { get; set; } = currLoginDate;
        public DateTime LastDateSeen { get; set; } = lastDateSeen;

        public ICollection<Currency> Currency { get; } = [];
        public UserStats? UserStats { get; set; }

    }
}
