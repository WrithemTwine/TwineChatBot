using StreamerBotLib.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class OverlayServices : IDatabaseTableMeta
    {
        public System.Int32 Id => (System.Int32)Values["Id"];
        public System.Boolean IsEnabled => (System.Boolean)Values["IsEnabled"];
        public System.Int32 Duration => (System.Int32)Values["Duration"];
        public StreamerBotLib.Overlay.Enums.OverlayTypes OverlayType => (StreamerBotLib.Overlay.Enums.OverlayTypes)Values["OverlayType"];
        public System.String OverlayAction => (System.String)Values["OverlayAction"];
        public System.String UserName => (System.String)Values["UserName"];
        public System.Boolean UseChatMsg => (System.Boolean)Values["UseChatMsg"];
        public System.String Message => (System.String)Values["Message"];
        public System.String ImageFile => (System.String)Values["ImageFile"];
        public System.String MediaFile => (System.String)Values["MediaFile"];

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
                                          (System.Int32)Values["Id"],
                                          (System.Boolean)Values["IsEnabled"],
                                          (System.Int32)Values["Duration"],
                                          (StreamerBotLib.Overlay.Enums.OverlayTypes)Values["OverlayType"],
                                          (System.String)Values["OverlayAction"],
                                          (System.String)Values["UserName"],
                                          (System.Boolean)Values["UseChatMsg"],
                                          (System.String)Values["Message"],
                                          (System.String)Values["ImageFile"],
                                          (System.String)Values["MediaFile"]
);
        }
        public void CopyUpdates(Models.OverlayServices modelData)
        {
            if (modelData.Id != Id)
            {
                modelData.Id = Id;
            }

            if (modelData.IsEnabled != IsEnabled)
            {
                modelData.IsEnabled = IsEnabled;
            }

            if (modelData.Duration != Duration)
            {
                modelData.Duration = Duration;
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

