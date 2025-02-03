using StreamerBotLib.Enums;
using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Overlay.Enums;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class OverlayServices : IDatabaseTableMeta
    {
        public System.Int32 Id { get => (System.Int32)Values["Id"]; set => Values["Id"] = value; }
        public System.Boolean IsEnabled { get => (System.Boolean)Values["IsEnabled"]; set => Values["IsEnabled"] = value; }
        public System.Int32 Duration { get => (System.Int32)Values["Duration"]; set => Values["Duration"] = value; }
        public StreamerBotLib.Overlay.Enums.OverlayTypes OverlayType { get => (StreamerBotLib.Overlay.Enums.OverlayTypes)Values["OverlayType"]; set => Values["OverlayType"] = value; }
        public System.String OverlayAction { get => (System.String)Values["OverlayAction"]; set => Values["OverlayAction"] = value; }
        public System.String UserName { get => (System.String)Values["UserName"]; set => Values["UserName"] = value; }
        public System.Boolean UseChatMsg { get => (System.Boolean)Values["UseChatMsg"]; set => Values["UseChatMsg"] = value; }
        public System.String Message { get => (System.String)Values["Message"]; set => Values["Message"] = value; }
        public System.String ImageFile { get => (System.String)Values["ImageFile"]; set => Values["ImageFile"] = value; }
        public System.String MediaFile { get => (System.String)Values["MediaFile"]; set => Values["MediaFile"] = value; }

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "OverlayServices";

        public OverlayServices(Models.OverlayServices tableData)
        {
            Values = new()
            {
                 { "Id", tableData.Id },
                 { "IsEnabled", tableData.IsEnabled },
                 { "Duration", tableData.Duration },
                 { "OverlayType", tableData.OverlayType },
                 { "OverlayAction", tableData.OverlayAction },
                 { "UserName", tableData.UserName },
                 { "UseChatMsg", tableData.UseChatMsg },
                 { "Message", tableData.Message },
                 { "ImageFile", tableData.ImageFile },
                 { "MediaFile", tableData.MediaFile }
            };
        }
        public Dictionary<string, Type> Meta => new()
        {
              { "Id", typeof(System.Int32) },
              { "IsEnabled", typeof(System.Boolean) },
              { "Duration", typeof(System.Int32) },
              { "OverlayType", typeof(StreamerBotLib.Overlay.Enums.OverlayTypes) },
              { "OverlayAction", typeof(System.String) },
              { "UserName", typeof(System.String) },
              { "UseChatMsg", typeof(System.Boolean) },
              { "Message", typeof(System.String) },
              { "ImageFile", typeof(System.String) },
              { "MediaFile", typeof(System.String) }
        };
        public object GetModelEntity()
        {
            return new Models.OverlayServices(
            isEnabled: IsEnabled, 
            overlayType: OverlayType, 
            overlayAction: OverlayAction, 
            userName: UserName, 
            useChatMsg: UseChatMsg, 
            message: Message, 
            imageFile: ImageFile, 
            mediaFile: MediaFile
        );
        }
        public void CopyUpdates(Models.OverlayServices modelData)
        {
          if (modelData.IsEnabled != IsEnabled)
            {
                modelData.IsEnabled = IsEnabled;
            }

          if (modelData.OverlayType != OverlayType)
            {
                modelData.OverlayType = OverlayType;
            }

          if (modelData.OverlayAction != OverlayAction)
            {
                modelData.OverlayAction = OverlayAction;
            }

          if (modelData.UserName != UserName)
            {
                modelData.UserName = UserName;
            }

          if (modelData.UseChatMsg != UseChatMsg)
            {
                modelData.UseChatMsg = UseChatMsg;
            }

          if (modelData.Message != Message)
            {
                modelData.Message = Message;
            }

          if (modelData.ImageFile != ImageFile)
            {
                modelData.ImageFile = ImageFile;
            }

          if (modelData.MediaFile != MediaFile)
            {
                modelData.MediaFile = MediaFile;
            }

        }
    }
}

