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
    private List<ImageItem> ImageList { get; set; } = new List<ImageItem>();
    private int CurrentIndex { get; set; } = -1;
    private int Count { get; set; } = 0;
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
        Count = ImageList.Count;
        if (Count > 0)
        {
            CurrentIndex = 0;
        }
    }
    public int GetCount()
    {
        return Count;
    }
    public Bitmap GetCurrentImage()
    {
        return BitmapImageToBitmap(ImageList[CurrentIndex].Image); 
    }
    public String GetCurrentFileName()
    {
        return ImageList[CurrentIndex].FileName;
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
    public ICogImage ConvertBitmapToCogImage(Bitmap bitmap)
    {
        return new CogImage24PlanarColor(bitmap);
    }
}
