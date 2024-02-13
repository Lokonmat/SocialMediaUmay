using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.IO;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App.Views.OgretmenPages.Paylasimlar
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ResimKesmeSayfasi : ContentPage
    {
        // Burda yaptığım işlemler hakkında en ufak bir bilgim yok allaha emanet gidiyoruz hadi hayırlısı.
        public event EventHandler<byte[]> ImageCropped;
        private SKBitmap originalBitmap;
        private SKPoint lastTouchPoint;
        private SKRect cropRect;
        private bool isCropping;
        private float cropRectWidth = 900;
        private float cropRectHeight = 900;
        public ResimKesmeSayfasi()
        {
            InitializeComponent();
        }
        public void SetImageSize(string imagePath, float width, float height)
        {
            try
            {
                originalBitmap = SKBitmap.Decode(imagePath);
                cropRectWidth = 900;
                cropRectHeight = 900;

                // Kırpmayı yeniden hesapla ve görüntüle
                CalculateInitialCrop();
                canvasView.InvalidateSurface();
            }
            catch (Exception ex)
            {
                // Hata durumunda işlemler
                Console.WriteLine("Hata: " + ex.Message);
            }
        }
        private void CalculateInitialCrop()
        {
            // İlk kırpmayı belirle, örneğin ortadan kırpa veya özel bir mantık uygula
            float centerX = originalBitmap.Width / 2;
            float centerY = originalBitmap.Height / 2;
            float left = centerX - (cropRectWidth / 2);
            float top = centerY - (cropRectHeight / 2);
            float right = left + cropRectWidth;
            float bottom = top + cropRectHeight;
            cropRect = new SKRect(left, top, right, bottom);
        }
        private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            if (originalBitmap != null)
            {
                // Aspectfit olarak resmi çiz
                SKRect destRect = CalculateAspectFitRect(info, originalBitmap);
                canvas.DrawBitmap(originalBitmap, destRect);
            }
        }

        private SKRect CalculateAspectFitRect(SKImageInfo canvasInfo, SKBitmap bitmap)
        {
            float widthRatio = canvasInfo.Width / (float)bitmap.Width;
            float heightRatio = canvasInfo.Height / (float)bitmap.Height;
            float minRatio = Math.Min(widthRatio, heightRatio);

            float newWidth = bitmap.Width * minRatio;
            float newHeight = bitmap.Height * minRatio;

            float left = (canvasInfo.Width - newWidth) / 2;
            float top = (canvasInfo.Height - newHeight) / 2;

            return new SKRect(left, top, left + newWidth, top + newHeight);
        }
        private void OnCanvasViewTouch(object sender, SKTouchEventArgs args)
        {
            if (originalBitmap == null)
                return;

            switch (args.ActionType)
            {
                case SKTouchAction.Pressed:
                    lastTouchPoint = args.Location;
                    isCropping = cropRect.Contains(args.Location);
                    break;

                case SKTouchAction.Moved:
                    if (isCropping)
                    {
                        float deltaX = args.Location.X - lastTouchPoint.X;
                        float deltaY = args.Location.Y - lastTouchPoint.Y;
                        cropRect.Offset(deltaX, deltaY);
                        lastTouchPoint = args.Location;
                        canvasView.InvalidateSurface();
                    }
                    break;

                case SKTouchAction.Released:
                    isCropping = false;
                    break;
            }

            args.Handled = true;
        }
        private void OnCropClicked(object sender, EventArgs e)
        {
            if (originalBitmap != null)
            {
                // Orijinal bitmap'i kullan
                SKBitmap visibleBitmap = originalBitmap.Copy();

                // Görüntüyü paylaş
                MemoryStream stream = new MemoryStream();
                visibleBitmap.Encode(stream, SKEncodedImageFormat.Png, 100);
                stream.Position = 0;
                ImageCropped?.Invoke(this, stream.ToArray());
            }

            // Sayfaya geri dön
            Navigation.PopModalAsync();
        }
        private void OnRotateClicked(object sender, EventArgs e)
        {
            if (originalBitmap != null)
            {
                // Mevcut bitmap'i döndür
                originalBitmap = RotateBitmap(originalBitmap, 90);

                // Kırpmayı yeniden hesapla ve görüntüle
                CalculateInitialCrop();
                canvasView.InvalidateSurface();
            }
        }
        private SKBitmap RotateBitmap(SKBitmap bitmap, double degrees)
        {
            using (var surface = SKSurface.Create(new SKImageInfo(bitmap.Height, bitmap.Width)))
            {
                var canvas = surface.Canvas;
                canvas.Translate(bitmap.Height, 0);
                canvas.RotateDegrees((float)degrees);
                canvas.DrawBitmap(bitmap, 0, 0);

                surface.Canvas.Flush();

                var rotatedBitmap = surface.Snapshot();
                return SKBitmap.FromImage(rotatedBitmap);
            }
        }
    }
}