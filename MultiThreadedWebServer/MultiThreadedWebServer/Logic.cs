using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace MultiThreadedWebServer
{
	internal class Logic
	{
        public static void ConvertToGifThreadPool(string imagePath, int index)
        {
            var image1 = Image.Load<Rgba32>(imagePath);
            var image2 = Image.Load<Rgba32>(imagePath);
            var image3 = Image.Load<Rgba32>(imagePath);
            var image4 = Image.Load<Rgba32>(imagePath);
            var image5 = Image.Load<Rgba32>(imagePath);
            var image6 = Image.Load<Rgba32>(imagePath);

            int width, height;

            width = image1.Width;
            height = image1.Height;

            using var gif = new Image<Rgba32>(width, height);

            var waitHandles = new WaitHandle[]
            {
                ThreadPoolAction(image1, () => EditImage(image1, 0, 20, 0)),
                ThreadPoolAction(image2, () => EditImage(image2, 20, 0, 0)),
                ThreadPoolAction(image3, () => EditImage(image3, 0, 0, 20)),
                ThreadPoolAction(image4, () => EditImage(image4, 20, 20, 0)),
                ThreadPoolAction(image5, () => EditImage(image5, 0, 20, 20)),
                ThreadPoolAction(image6, () => EditImage(image6, 20, 0, 20))
            };

            WaitHandle.WaitAll(waitHandles);

            lock (gif)
            {
                gif.Frames.AddFrame(image1.Frames.RootFrame);
                gif.Frames.AddFrame(image2.Frames.RootFrame);
                gif.Frames.AddFrame(image3.Frames.RootFrame);
                gif.Frames.AddFrame(image4.Frames.RootFrame);
                gif.Frames.AddFrame(image5.Frames.RootFrame);
                gif.Frames.AddFrame(image6.Frames.RootFrame);
                gif.Save($"../../../output{index}.gif");
            }
        }

        public static void EditImage(Image<Rgba32> image, byte red, byte green, byte blue)
        {
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Rgba32 pixel = image[x, y];
                    if (red != 0)
                        pixel.R = red;
                    if (green != 0)
                        pixel.G = green;
                    if (blue != 0)
                        pixel.B = blue;
                    image[x, y] = pixel;
                }
            }
        }

        private static WaitHandle ThreadPoolAction(Image<Rgba32> image, Action action)
        {
            var handle = new ManualResetEvent(false);
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
				{
                    Console.WriteLine(ex.ToString());
				}
                finally
                {
                    handle.Set();
                }
            });
            return handle;
        }

    }
}
