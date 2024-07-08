using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Overlay.Enums;

using System.ComponentModel.DataAnnotations.Schema;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(Id))]
    [Index(nameof(Id), nameof(OverlayType), nameof(OverlayAction))]
    public class OverlayServices(int id = 0,
                                 bool isEnabled = false,
                                 int duration = 0,
                                 OverlayTypes overlayType = default,
                                 string overlayAction = null,
                                 string userName = null,
                                 bool useChatMsg = false,
                                 string message = null,
                                 string imageFile = null,
                                 string mediaFile = null) : EntityBase
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } = id;
        public bool IsEnabled { get; set; } = isEnabled;
        public int Duration { get; set; } = duration;
        public OverlayTypes OverlayType { get; set; } = overlayType;
        public string OverlayAction { get; set; } = overlayAction;
        public string UserName { get; set; } = userName;
        public bool UseChatMsg { get; set; } = useChatMsg;
        public string Message { get; set; } = message;
        public string ImageFile { get; set; } = imageFile;
        public string MediaFile { get; set; } = mediaFile;
    }
}
