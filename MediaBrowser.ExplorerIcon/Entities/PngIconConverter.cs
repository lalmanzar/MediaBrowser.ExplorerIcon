using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace MediaBrowser.ExplorerIcon.Entities
{
    internal class PngIconConverter
    {
        /* input image with width = height is suggested to get the best result */
        /* png support in icon was introduced in Windows Vista */

        public static bool Convert(Image input_bit, Stream output_stream, int size, bool keepAspectRatio = true)
        {
            if (input_bit == null) return false;
            int width, height;
            if (keepAspectRatio)
            {
                if (input_bit.Width > input_bit.Height)
                {
                    width = size;
                    height = (int) (((float) input_bit.Height)/input_bit.Width*size);
                }
                else
                {
                    width = (int) (((float) input_bit.Width)/input_bit.Height*size);
                    height = size;
                }
            }
            else
            {
                width = height = size;
            }
            var newBit = new Bitmap(input_bit, new Size(width, height));
            {
                // save the resized png into a memory stream for future use
                var memData = new MemoryStream();
                newBit.Save(memData, ImageFormat.Png);

                var iconWriter = new BinaryWriter(output_stream);
                if (output_stream == null) return false;
                // 0-1 reserved, 0
                iconWriter.Write((byte) 0);
                iconWriter.Write((byte) 0);

                // 2-3 image type, 1 = icon, 2 = cursor
                iconWriter.Write((short) 1);

                // 4-5 number of images
                iconWriter.Write((short) 1);

                // image entry 1
                // 0 image width
                iconWriter.Write((byte) width);
                // 1 image height
                iconWriter.Write((byte) height);

                // 2 number of colors
                iconWriter.Write((byte) 0);

                // 3 reserved
                iconWriter.Write((byte) 0);

                // 4-5 color planes
                iconWriter.Write((short) 0);

                // 6-7 bits per pixel
                iconWriter.Write((short) 32);

                // 8-11 size of image data
                iconWriter.Write((int) memData.Length);

                // 12-15 offset of image data
                iconWriter.Write((6 + 16));

                // write image data
                // png data must contain the whole png data file
                iconWriter.Write(memData.ToArray());

                iconWriter.Flush();

                return true;
            }
        }

        public static bool Convert(Image inputImage, string outputIcon, int size, bool keepAspectRatio = true)
        {
            bool result;
            using (var outputStream = new FileStream(outputIcon, FileMode.OpenOrCreate))
            {
                result = Convert(inputImage, outputStream, size, keepAspectRatio);
                outputStream.Close();
            }
            return result;
        }
    }
}