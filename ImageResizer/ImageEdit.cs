using System;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;


namespace ImageResizer
{
    class ImageEdit
    {
        private int choice;
        private Size newSize;
        private long newPerc;
        
        public ImageEdit(int option, Size nSize)
        {
            choice = option;
            newSize = nSize;
        }

        public ImageEdit(int option, long nPerc)
        {
            choice = option;
            newPerc = nPerc;
        }

        public ImageEdit(int option, long nPerc, Size nSize)
        {
            choice = option;
            newPerc = nPerc;
            newSize = nSize;
        }

        public void Execute(string img)
        {
            try
            {
                byte[] byteArr;
                byte[] newImg;
                ImageFormat imgFrmt;

                //Check if file has a valid extension
                bool validext = IsValidExt(img);
                if (!validext) { return; }

                //Read the file into a byte array and get format
                using (Image imgDetails = Image.FromFile(img))
                { byteArr = File.ReadAllBytes(img); imgFrmt = imgDetails.RawFormat; }
                
                switch (choice)
                {
                    case 1:
                        newImg = ReRes(byteArr);
                        if (newImg == null) { break; }
                        else { File.WriteAllBytes(img, newImg); }
                        break;
                    case 2:
                        newImg = Compress(byteArr, imgFrmt);
                        if (newImg == null) { break; }
                        else { File.WriteAllBytes(img, newImg); }
                        break;
                    case 3:
                        byte[] oldImg = Compress(byteArr, imgFrmt);
                        newImg = ReRes(oldImg);
                        if (newImg == null) { break; }
                        else { File.WriteAllBytes(img, newImg); }
                        break;
                }
            }
            catch (Exception ex)
            {
               System.Windows.MessageBox.Show(ex.ToString());
            }
        }
        
        private byte[] ReRes(byte[] imgBytes)
        {
            try
            {
                //Lots of possible leakage so put using statements everywhere
                using (MemoryStream ogStream = new MemoryStream(imgBytes))
                using (Image ogImg = Image.FromStream(ogStream))
                {
                    int ogWidth = ogImg.Width;
                    int ogHeight = ogImg.Height;

                    //Don't upsize the photos
                    if (ogWidth < newSize.Width || ogHeight < newSize.Height) { return null; }

                    //Get the new ratio
                    decimal newH = newSize.Width * (decimal.Divide(ogHeight, ogWidth));

                    using (Bitmap resizedImg = new Bitmap((int)newSize.Width, (int)newH))
                    {
                        //Adjust the dimensions
                        using (Graphics graphics = Graphics.FromImage((Image)resizedImg))
                        {
                            graphics.InterpolationMode = InterpolationMode.Default;
                            graphics.DrawImage(ogImg, 0, 0, newSize.Width, (int)newH);
                        }

                        using (MemoryStream ms = new MemoryStream())
                        {
                            resizedImg.Save(ms, ImageFormat.Jpeg);

                            //Return the MemoryStream as a byte array
                            return ms.ToArray();
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
        }

        private byte[] Compress(byte[] imgBytes, ImageFormat format)
        {
            try
            {
                //Compress the image
                using (MemoryStream mStream = new MemoryStream(imgBytes))
                using (MemoryStream ms = new MemoryStream())
                {
                    Bitmap resizedImg = new Bitmap(mStream);
                    ImageCodecInfo imgCodec = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == format.Guid);
                    EncoderParameters codecParms = new EncoderParameters(1);

                    codecParms.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, newPerc);

                    resizedImg.Save(ms, imgCodec, codecParms);

                    //Return the MemoryStream as a byte array
                    return ms.ToArray();
                }
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
        }

        private bool IsValidExt(String fName)
        {
            String[] validFileExts = { ".jpg", ".jpeg", ".bmp", ".png" };

            string fExt = Path.GetExtension(fName).ToLower();

            foreach (String ext in validFileExts)
            {
                if (fExt == ext)
                {
                    return true;
                }
            }

            return false;

        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }
}
