using System;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace ImageCompressor
{
    public class ImageResize
    {
        private const double MaxHeight = 1080;
        private readonly long _quality; //90, 60, 40
        private double _imgWidth;
        private double _imgHeight;

        public ImageResize(long q)
        {
            _quality = q;
        }

        public void Resize(string imgName)
        {
            try
            {
                byte[] byteArr;
                ImageFormat imgFrmt;

                using (var imgDetails = Image.FromFile(imgName))
                {
                    byteArr = File.ReadAllBytes(imgName);
                    imgFrmt = imgDetails.RawFormat;
                }

                var resizedImage = AlterImage(byteArr, imgFrmt);

                if (resizedImage != null) { File.WriteAllBytes(imgName, resizedImage); }
            }

            catch {  }

        }

        private byte[] AlterImage(byte[] imgBytes, ImageFormat format)
        {
            try
            {
                using (var ogStream = new MemoryStream(imgBytes))
                using (var ogImg = Image.FromStream(ogStream))
                {
                    // if height is greater than our established maximum then resize
                    if (ogImg.Height > MaxHeight)
                    {
                        var scaleFactor = ogImg.Height / MaxHeight;
                        _imgWidth = (ogImg.Width / scaleFactor);
                        _imgHeight = (ogImg.Height / scaleFactor);
                    }
                    else
                    {
                        _imgHeight = ogImg.Height;
                        _imgWidth = ogImg.Width;
                    }

                    using (var resizedImg = new Bitmap((int)_imgWidth, (int)_imgHeight))
                    {
                        using (var graphics = Graphics.FromImage(resizedImg))
                        {
                            graphics.CompositingMode = CompositingMode.SourceCopy;
                            graphics.CompositingQuality = CompositingQuality.HighSpeed;
                            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            graphics.SmoothingMode = SmoothingMode.HighQuality;
                            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            graphics.DrawImage(ogImg, 0, 0, (int)_imgWidth, (int)_imgHeight);
                        }

                        using (var ms = new MemoryStream())
                        {
                            var imgCodec = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == format.Guid);
                            var codecParms = new EncoderParameters(1) { Param = { [0] = new EncoderParameter(Encoder.Quality, _quality) } };

                            resizedImg.Save(ms, imgCodec, codecParms);

                            return ms.ToArray();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /*Deprecated but possible useful in the future
        private void FindRatio(double w, double h)
        {
            var scaleFactor = h / MaxHeight;
            _imgWidth = (w / scaleFactor); _imgHeight = (h / scaleFactor);
        }
        */
    }
}
