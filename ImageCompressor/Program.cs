using System;
using System.IO;
using System.Reflection;

namespace ImageCompressor
{
    public class Program
    {
        private static long _quality;

        public static void Main(string[] args)
        {
            // Lines for testing
            //var img = @"C:\temp\1.jpg";
            //_quality = 90L;
            
            try
            {
                // Get executable location and name
                var name = Assembly.GetEntryAssembly().Location;
                var fileName = Path.GetFileNameWithoutExtension(name);

                if (!ValidateExeName(fileName))
                {
                    Console.WriteLine("Error with application name");
                }
                else
                {
                    var resizer = new ImageResize(_quality);

                    foreach (var img in args)
                    {
                        Console.WriteLine($"Attempting to resize {img}");
                        var result = resizer.Resize(img);

                        if (result.Success == false)
                        {
                            foreach (var msg in result.MessageList)
                            {
                                if(result.Success == false) Console.WriteLine(msg);
                                return;
                            }
                        }

                        Console.WriteLine($"Successfully resized {img}");
                    }
                }

            }
            catch (Exception ex) { Console.WriteLine(ex); }

            Console.WriteLine("Press any key to close...");
            Console.Read();
        }
        
        private static bool ValidateExeName(string fName)
        {
           if (fName.Length != 16 || fName.Substring(0, 15) != "ImageCompressor")
            {
                Console.WriteLine("Problem with ImageCompressor filename");
                return false;
            }
            
            switch (fName.Substring(15).ToUpper())
            {
                case "L":
                    _quality = 40L;
                    break;
                case "M":
                    _quality = 60L;
                    break;
                case "H":
                    _quality = 90L;
                    break;
                default:
                    return false;
            }

            return true;
        }
        
    }
}
