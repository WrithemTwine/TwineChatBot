using Microsoft.Win32;

using StreamerBotLib.Models.Events;
using StreamerBotLib.Static;
using StreamerBotLib.Systems.Overlay.Models;
using StreamerBotLib.Systems.Overlay.Static;

using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StreamerBotLib.GUI.Data
{
    /// <summary>
    /// Interaction logic for OverlayServiceBulk.xaml
    /// </summary>
    public partial class OverlayServiceBulk : Window, INotifyPropertyChanged
    {
        private OverlayActionType OverlayActionType { get; set; }

        public OverlayServiceBulk(OverlayActionType overlayActionType)
        {
            InitializeComponent();
            OverlayActionType = overlayActionType;

            DataContext = OverlayActionType;
        }

        public event EventHandler<OverlayBulkAddEventArgs> OverlayServicesBulkAddData;
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region File Operations

        private void FileBrowser_TextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TextBox saveMediaPath = sender as TextBox;

            OpenFileDialog pickFile = new()
            {
                Multiselect = true,
                CheckFileExists = true,
                DereferenceLinks = true,
                Title = "Select media file (picture/video) for Overlay event! (needs to be viewable in a webpage)",
                InitialDirectory = Directory.Exists(OptionFlags.MediaOverlayMRUPathSelect) ? OptionFlags.MediaOverlayMRUPathSelect : Directory.GetCurrentDirectory()
            };


            if (pickFile.ShowDialog() == true)
            {
                saveMediaPath.Text = string.Join("; ", pickFile.FileNames);
                OptionFlags.MediaOverlayMRUPathSelect = Path.GetDirectoryName(pickFile.FileNames[0]);
            }
        }

        /// <summary>
        /// Copy the file to a subfolder named with the OverlayType.
        /// </summary>
        /// <param name="FileName">Path and file from which to copy.</param>
        /// <param name="OverlayType">The name of the overlaytype, which becomes the subfolder name to hold the file in the current application folder.</param>
        private static string FileCopy(string FileName, string OverlayType)
        {
            string resultfile;
            string CopyFile = Path.Combine(PublicConstants.BaseOverlayPath, OverlayType, Path.GetFileName(FileName)).Replace("_", " ").Replace(" ", ""); // replace '_' to prevent issues with converting class object to string to class object

            if (FileName == "")
            {
                resultfile = null;
            }
            else if (!File.Exists(CopyFile) && File.Exists(FileName))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(CopyFile));
                File.Copy(FileName, CopyFile, false);
                resultfile = CopyFile;
            }
            else if (!File.Exists(FileName))
            {
                resultfile = FileName;
            }
            else
            {
                resultfile = CopyFile;
            }
            return resultfile == null ? "" : Path.GetRelativePath(Directory.GetCurrentDirectory(), resultfile);
        }

        private void BrowseImage_Click(object sender, RoutedEventArgs e)
        {
            TextBox saveMediaPath = ImageFileTextBox;

            OpenFileDialog pickFile = new()
            {
                Multiselect = true,
                CheckFileExists = true,
                DereferenceLinks = true,
                Title = "Select media file (picture/video) for Overlay event! (needs to be viewable in a webpage)",
                InitialDirectory = Directory.Exists(OptionFlags.MediaOverlayMRUPathSelect) ? OptionFlags.MediaOverlayMRUPathSelect : Directory.GetCurrentDirectory()
            };


            if (pickFile.ShowDialog() == true)
            {
                saveMediaPath.Text = string.Join("; ", pickFile.FileNames);
                OptionFlags.MediaOverlayMRUPathSelect = Path.GetDirectoryName(pickFile.FileNames[0]);
            }

            OnPropertyChanged(nameof(OverlayActionType.ImageFile));
        }

        private void BrowseMedia_Click(object sender, RoutedEventArgs e)
        {
            TextBox saveMediaPath = MediaFileTextBox;

            OpenFileDialog pickFile = new()
            {
                Multiselect = true,
                CheckFileExists = true,
                DereferenceLinks = true,
                Title = "Select media file (picture/video) for Overlay event! (needs to be viewable in a webpage)",
                InitialDirectory = Directory.Exists(OptionFlags.MediaOverlayMRUPathSelect) ? OptionFlags.MediaOverlayMRUPathSelect : Directory.GetCurrentDirectory()
            };


            if (pickFile.ShowDialog() == true)
            {
                saveMediaPath.Text = string.Join("; ", pickFile.FileNames);
                OptionFlags.MediaOverlayMRUPathSelect = Path.GetDirectoryName(pickFile.FileNames[0]);
            }

            OnPropertyChanged(nameof(OverlayActionType.MediaFile));
        }


        #endregion

        private void CreateRecords_Click(object sender, RoutedEventArgs e)
        {
            bool UseSameImage = UseSameImageCheckBox.IsChecked == true;

            string[] images = ImageFileTextBox.Text.Split("; ");
            string[] media = MediaFileTextBox.Text.Split("; ");

            List<OverlayActionType> bulk = [];

            bool imagesAdded = false;

            foreach (string m in media)
            {
                OverlayActionType curr = OverlayActionType.Copy();
                string image = "";
                if (UseSameImage && !imagesAdded)
                {
                    image = images.Length > 0 ? images[0] : "";

                    curr.ImageFile = FileCopy(image, curr.OverlayType.ToString());
                    imagesAdded = true;
                }
                else if (images.Length > 0 && !imagesAdded)
                {
                    imagesAdded = true;
                    curr.MediaFile = "";
                    foreach (string i in images)
                    {
                        curr.ImageFile = FileCopy(i, curr.OverlayType.ToString());

                        bulk.Add(curr);
                    }
                }
                curr.MediaFile = FileCopy(m, curr.OverlayType.ToString());
                bulk.Add(curr);
            }

            OverlayServicesBulkAddData?.Invoke(this, new(bulk));
            Close();
        }
    }
}
