using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace ImageResizer
{
    public class fileInfo
    {
        public string FName { get; set; }
        public string FSize { get; set; }
    }

    public partial class MainWindow : Window
    {
        private ImageEdit _resize;
        //private String[] fNames;
        private string[] _imgs;
        private System.Drawing.Size _newRes;
        private long _percentAmt;
        private delegate void PBarUpdate(System.Windows.DependencyProperty dp, object value);

        public MainWindow()
        {
            InitializeComponent();
        }

        private void lvFiles_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                lvFiles.Items.Clear();
                var fNames = (string[])e.Data.GetData(DataFormats.FileDrop);
                _imgs = new string[fNames.Length];
                var i = 0;

                foreach (var img in fNames)
                {
                    var name = Path.GetFileName(img);
                    var fSize = new FileInfo(img).Length;
                    var fileSize = GetFileSizeInBytes(fSize);

                    lvFiles.Items.Add(new fileInfo { FName = name, FSize = fileSize });
                    _imgs[i] = img;
                    i++;
                }
            }

            
        }
        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            lvFiles.Items.Clear();

            var dlg = new OpenFileDialog
            {
                DefaultExt = ".jpg",
                Filter = "JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif|PNG Files (*.png)|*.png",
                Multiselect = true
            };


            var result = dlg.ShowDialog();

            if (result != true) return;

            var fNames = dlg.FileNames;
            _imgs = new string[fNames.Length];
            var i = 0;

            foreach (var img in fNames)
            {
                var name = Path.GetFileName(img);
                var fSize = new FileInfo(img).Length;
                var fileSize = GetFileSizeInBytes(fSize);

                lvFiles.Items.Add(new fileInfo { FName = name, FSize = fileSize });
                _imgs[i] = img;
                i++;
            }
        }
        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            lvFiles.Items.Remove(lvFiles.SelectedItem);
        }
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            lvFiles.Items.Clear();
            lvAfter.Items.Clear();
        }
        private void btnResize_Click(object sender, RoutedEventArgs e)
        {
            
            lvFiles.IsEnabled = false;

            pBar.Value = 0;
            pBar.Maximum = lvFiles.Items.Count;
            var update = new PBarUpdate(pBar.SetValue);

            double value = 0;

            if (!AreFiles()) { return; }

            if (!IsResolution() && !IsPercentage()) { return; }

            if (!IsPercentage() && IsResolution())
            {
                _resize = new ImageEdit(1, _newRes);
            }

            if (IsPercentage() && !IsResolution())
            {
                _resize = new ImageEdit(2, _percentAmt);
            }

            if (IsPercentage() && IsResolution())
            {
                _resize = new ImageEdit(3, _percentAmt, _newRes);
            }

            try
            {
                foreach (var img in _imgs)
                {
                    value += 1;
                    lblStatus.Content = "Processing file " + value + " of " + lvFiles.Items.Count;
                    _resize.Execute(img);

                    Dispatcher.Invoke(update, System.Windows.Threading.DispatcherPriority.Background, System.Windows.Controls.Primitives.RangeBase.ValueProperty, value);
                }

                LvAfterPopulate();

                lvFiles.IsEnabled = true;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private bool AreFiles()
        {
            return lvFiles.Items.Count > 0;
        }

        private System.Drawing.Size GetUserSize()
        {
            System.Drawing.Size newSize;
            switch (cboSizes.SelectedIndex)
            {
                case 0:
                    newSize = new System.Drawing.Size(640, 480);
                    break;
                case 1:
                    newSize = new System.Drawing.Size(800, 600);
                    break;
                case 2:
                    newSize = new System.Drawing.Size(1280, 720);
                    break;
                default:
                    newSize = new System.Drawing.Size(1920, 1280);
                    break;
            }

            return newSize;
        }

        private bool IsResolution()
        {
            if (cboSizes.SelectedIndex == -1)
            {
                return false;
            }

            _newRes = GetUserSize();
            return true;
        }
        private bool IsPercentage()
        {
            if (txtPerc.Text != "")
            {
                int newPercentage = int.Parse(txtPerc.Text);
                _percentAmt = Convert.ToInt64(newPercentage);

                return true;
            }
            
            return false;
        }
        private string GetFileSizeInBytes(long totalBytes)
        {
            if (totalBytes >= 1073741824)
            {
                var fileSize = decimal.Divide(totalBytes, 1073741824);
                return $"{fileSize:##.##} GB";
            }
            if (totalBytes >= 1048576)
            {
                var fileSize = decimal.Divide(totalBytes, 1048576);
                return $"{fileSize:##.##} MB";
            }
            if (totalBytes >= 1024)
            {
                var fileSize = decimal.Divide(totalBytes, 1024);
                return $"{fileSize:##.##} KB";
            }
            else
            {
                var fileSize = totalBytes;
                return $"{fileSize:##.##} Bytes";
            }
        }
        private void LvAfterPopulate()
        {
            lvAfter.Items.Clear();
            
            foreach (var img in _imgs)
            {
                var name = Path.GetFileName(img);
                var fSize = new FileInfo(img).Length;
                var fileSize = GetFileSizeInBytes(fSize);

                lvAfter.Items.Add(new fileInfo { FName = name, FSize = fileSize });
            }
        }
    }
}