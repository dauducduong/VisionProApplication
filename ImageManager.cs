using Cognex.VisionPro;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

public class ImageItem
{
    public BitmapImage Image { get; set; }
    public string FileName { get; set; }

    public ImageItem(BitmapImage image, string fileName)
    {
        Image = image;
        FileName = fileName;
    }
}

public class ImageManager
{
    public List<ImageItem> ImageList { get; private set; } = new List<ImageItem>();
    private int CurrentIndex { get; set; } = -1;
    public void LoadImagesToMemory(string folderPath)
    {
        ImageList.Clear(); // Xóa danh sách cũ
        CurrentIndex = -1;

        string[] imageExtensions = { ".png", ".jpg", ".jpeg", ".bmp", ".gif" };
        string[] files = Directory.GetFiles(folderPath);

        foreach (string file in files)
        {
            if (Array.Exists(imageExtensions, ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
            {
                BitmapImage bitmap = new BitmapImage(new Uri(file));
                string fileName = Path.GetFileName(file);

                ImageList.Add(new ImageItem(bitmap, fileName));
            }
        }
        if (ImageList.Count > 0)
        {
            CurrentIndex = 0;
        }
    }
    public int GetCurrentIndex()
    {
        return CurrentIndex;
    }
    public int SetNextIndex()
    {
        if (CurrentIndex == ImageList.Count - 1)
        {
            return CurrentIndex;
        }
        else
        {
            CurrentIndex++;
            return CurrentIndex;
        }
        
    }

    public int SetPrevIndex()
    {
        if (CurrentIndex == 0)
        {
            return CurrentIndex;
        }
        else
        {
            CurrentIndex--;
            return CurrentIndex;
        }
    }

    private static Bitmap BitmapImageToBitmap(BitmapImage bitmapImage)
    {
        using (MemoryStream outStream = new MemoryStream())
        {
            BitmapEncoder encoder = new BmpBitmapEncoder(); // Chuyển về BMP
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
            encoder.Save(outStream);
            using (Bitmap bitmap = new Bitmap(outStream))
            {
                return new Bitmap(bitmap); // Tạo bản sao Bitmap để tránh lỗi giải phóng bộ nhớ
            }
        }
    }
    public ICogImage ConvertBitmapToCogImage(BitmapImage bitmapImage)
    {
        Bitmap bitmap = BitmapImageToBitmap(bitmapImage);
        return new CogImage24PlanarColor(bitmap);
    }
}
