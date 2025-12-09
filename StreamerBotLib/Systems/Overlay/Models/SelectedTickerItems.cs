using Microsoft.Win32;

using StreamerBotLib.Static;
using StreamerBotLib.Systems.Overlay.Enums;
using StreamerBotLib.Systems.Overlay.Static;

using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StreamerBotLib.Systems.Overlay.Models
{
    [DebuggerDisplay("IsSelected={IsSelected}, Overlay={OverlayTickerItem}")]
    public class SelectedTickerItem : INotifyPropertyChanged
    {
        private const string DefaultIconPath = "Click to select icon. Will copy to resulting path.";

        private string icon;
        private ImageBrush showIcon;

        public bool IsSelected { get; set; } = false;
        public string OverlayTickerItem { get; set; }
        public string Icon
        {
            get
            {
                return icon;
            }

            set
            {
                icon = value;
                string showIconUri = PublicConstants.BaseTickerPath + '/' + (icon == DefaultIconPath ? PublicConstants.DefaultTickerIcons[OverlayTickerItem] : icon);

                if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), showIconUri)))
                {
                    BitmapImage bitmap = new();

                    bitmap.BeginInit();
                    bitmap.UriSource = new(Path.Combine(Directory.GetCurrentDirectory(), showIconUri));
                    bitmap.EndInit();

                    ShowIcon = new ImageBrush(bitmap);
                }

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Icon)));
            }
        }

        public ImageBrush ShowIcon
        {
            get
            {
                return showIcon;
            }
            set
            {
                showIcon = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowIcon)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public SelectedTickerItem(bool isSelected, string overlayTickerItem)
        {
            IsSelected = isSelected;
            OverlayTickerItem = overlayTickerItem;
            SetIcon();
        }

        public void ProcessIcon()
        {
            OpenFileDialog pickFile = new()
            {
                Multiselect = false,
                CheckFileExists = true,
                DereferenceLinks = true,
                Title = $"Select image file to show as {OverlayTickerItem} icon (must be webpage ready).",
                DefaultExt = "jpg|jpeg|png|apng|gif|bmp|svg",
                InitialDirectory = Directory.Exists(OptionFlags.MediaOverlayMRUPathSelect) ? OptionFlags.MediaOverlayMRUPathSelect : Directory.GetCurrentDirectory()
            };

            if (pickFile.ShowDialog() == true)
            {
                string selectedpath = pickFile.FileName;
                File.Copy(selectedpath,
                    $"{Path.Combine(PublicConstants.BaseTickerPath, PublicConstants.BaseTickerIconPath, OverlayTickerItem)}{Path.GetExtension(selectedpath)}", true);
                Icon = $"{PublicConstants.BaseTickerIconPath}/{OverlayTickerItem}{Path.GetExtension(selectedpath)}";

                SetIcon(Icon);
            }
        }

        public void SetIcon()
        {
            /*

            <img class="tickername" alt="Example Image" />

            CSS:
            ------------
            tickername {
                content: url('https://example.com/image.jpg');
            }
            -----------
            */
            if (OptionFlags.MediaOverlayTickerIcons)
            {
                if (!Directory.Exists($"{Path.Combine(PublicConstants.BaseTickerPath, PublicConstants.BaseTickerIconPath)}"))
                {
                    Directory.CreateDirectory($"{Path.Combine(PublicConstants.BaseTickerPath, PublicConstants.BaseTickerIconPath)}");
                }

                string iconstyle = Path.Combine(PublicConstants.BaseTickerPath, PublicConstants.BaseTickerIconPath, PublicConstants.TickerIconsStyle);

                bool found = false;

                if (File.Exists(iconstyle))
                {
                    StreamReader reader = new(iconstyle);
                    string iconrow = reader.ReadLine();
                    while (iconrow != null && !iconrow.Contains(OverlayTickerItem) && !reader.EndOfStream)
                    {
                        iconrow = reader.ReadLine();
                    }

                    found = iconrow.Contains(OverlayTickerItem);

                    if (found)
                    {
                        iconrow = reader.ReadLine(); //     content: url('https://example.com/image.jpg');
                        Icon = iconrow.Replace("content: url('", "").Replace("');", "").Replace('/', '\\').Trim();
                    }
                }

                // no .css file or style not in the .css
                if (!found)
                {
                    Icon = DefaultIconPath;
                }
            }
        }

        private void SetIcon(string IconFile)
        {
            /*

            <img class="tickername" alt="Example Image" />

            CSS:
            ------------
            tickername {
                content: url('https://example.com/image.jpg');
            }
            -----------
            */

            string iconstyle = PublicConstants.BaseTickerPath + "/" + PublicConstants.BaseTickerIconPath + "/" + PublicConstants.TickerIconsStyle;

            if (!File.Exists(iconstyle)) // if no file, write the image and content URL. if file does exist, it may or may not contain existing data to replace
            {
                StreamWriter writer = new(iconstyle);
                writer.WriteLine($"#{OverlayTickerItem} {{");
                writer.WriteLine($"     content: url('{IconFile}');");
                writer.WriteLine($"     float: left;");
                writer.WriteLine($"}}");
                writer.WriteLine("");
            } 
            else
            {
                string StyleFile;
                using (StreamReader reader = new(iconstyle))
                {
                    StyleFile = reader.ReadToEnd();
                }

                StreamWriter writer = new(iconstyle,false);

                if (!StyleFile.Contains(OverlayTickerItem)) // no existing style content
                {
                    writer.Write(StyleFile);
                    writer.WriteLine($"#{OverlayTickerItem} {{");
                    writer.WriteLine($"     content: url('{IconFile}');");
                    writer.WriteLine($"     float: left;");
                    writer.WriteLine($"}}");
                    writer.WriteLine("");
                } 
                else // replace existing style content
                {
                    List<string> rows = [.. StyleFile.Split('\n')];
                    int idx = rows.FindIndex(r => r.Contains(OverlayTickerItem));
                    rows[idx + 1] = $"     content: url('{IconFile}');";

                    foreach(string r in rows)
                    {
                        writer.WriteLine(r);
                    }
                }
            }

        }
    }

    public class SelectedTickerItems
    {
        public List<SelectedTickerItem> TickerItems { get; private set; }

        public SelectedTickerItems()
        {
            TickerItems = [.. from string ticker in Enum.GetNames<OverlayTickerItem>()
                              select new SelectedTickerItem(new List<string>(OptionFlags.MediaOverlayTickerSelected).Contains(ticker), ticker)
                          ];
        }

        public void SaveSelections()
        {
            OptionFlags.MediaOverlayTickerSelected =
             [.. (from SelectedTickerItem ticker in TickerItems
              where ticker.IsSelected
              select ticker.OverlayTickerItem)];

        }

        public IEnumerable<SelectedTickerItem> GetSelectedTickers()
        {
            return from ticker in TickerItems
                   where ticker.IsSelected
                   select ticker;
        }

    }
}
