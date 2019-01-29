using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace IC.DL
{
    public class ImageResize
    {
        private const double MaxHeight = 1080;
        private readonly long _quality; //90, 60, 40
        private double _imgWidth;
        private double _imgHeight;
        private OperationResult messageList = new OperationResult();

        public ImageResize(long q)
        {
            _quality = q;
        }

        public OperationResult Resize(string imgName)
        {
            try
            {
                if (!IsValidExt(imgName))
                {
                    messageList.Success = false;
                    messageList.AddMessage($"{imgName} is not a valid image file");
                }

                var byteArr = ImageToByteArray(imgName, out ImageFormat imgFrmt);

                var resizedImage = AlterImage(byteArr, imgFrmt);

                if (resizedImage != null) File.WriteAllBytes(imgName, resizedImage);

                return messageList;
            }

            catch (FileNotFoundException)
            {
                messageList.Success = false;
                messageList.AddMessage($"File not found: {imgName}");
                return messageList;
            }
        }
        
        private byte[] AlterImage(byte[] imgBytes, ImageFormat format)
        {
               using (var ogStream = new MemoryStream(imgBytes))
                using (var ogImg = Image.FromStream(ogStream))
                {
                    
                    // if height is greater than our established maximum then resize
                    CalculateNewSize(ogImg);

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

                            if (ms.Length != 0) return ms.ToArray();

                            messageList.Success = false;
                            messageList.AddMessage("MemoryStream is empty");

                            return ms.ToArray();
                        }
                    }
                }
        }

        private byte[] ImageToByteArray(string imgName, out ImageFormat imgFrmt)
        {
            byte[] byteArr;

            using (var imgDetails = Image.FromFile(imgName))
            {
                byteArr = File.ReadAllBytes(imgName);
                imgFrmt = imgDetails.RawFormat;
            }
            if (byteArr.Length != 0) return byteArr;                                            // Successful creation

            messageList.Success = false;
            messageList.AddMessage("Image has no size");

            return byteArr;
        }

        private void CalculateNewSize(Image img)
        {
            
            if (img.Height == 0)                                                                // If image has no size return
            {
                messageList.Success = false;
                messageList.AddMessage("Image has no size.");
            }

            if (img.Height > MaxHeight)
            {
                var scaleFactor = img.Height / MaxHeight;
                _imgWidth = (img.Width / scaleFactor);
                _imgHeight = (img.Height / scaleFactor);

                messageList.Success = true;
                messageList.AddMessage($"Image resized, scale factor = {scaleFactor}");
            }
            else
            {
                _imgHeight = img.Height;
                _imgWidth = img.Width;

                messageList.Success = true;
                messageList.AddMessage($"Image below maximum size, no rescaling");
            }
        }

        private bool IsValidExt(string fName)
        {
            string[] validFileExts = { ".jpg", ".jpeg", ".bmp", ".png" };

            var extension = Path.GetExtension(fName);

            if (extension == null) return false;

            var fExt = extension.ToLower();

            return validFileExts.Any(ext => fExt == ext);
        }
    }
}
