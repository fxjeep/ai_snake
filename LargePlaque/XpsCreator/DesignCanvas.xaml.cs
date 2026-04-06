using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace XpsCreator;

public partial class DesignCanvas : UserControl
{
    private FrameworkElement? _selectedElement;
    private Point _mouseClickPos;
    private double _leftPos;
    private double _topPos;

    public event Action<string, double, double>? ElementMoved;
    public event Action<string, double, double>? ElementResized;

    public DesignCanvas()
    {
        InitializeComponent();
        this.Loaded += DesignCanvas_Loaded;
    }

    private void DesignCanvas_Loaded(object sender, RoutedEventArgs e)
    {
        // Initial setup
    }

    public void UpdateBackground(string fileName, double widthCm, double heightCm)
    {
        string fullPath = Path.GetFullPath(fileName);
        if (string.IsNullOrEmpty(fileName) || !File.Exists(fullPath))
        {
            BackgroundImage.Source = null;
            return;
        }

        try
        {
            var bitmap = new BitmapImage(new Uri(fullPath, UriKind.Absolute));
            BackgroundImage.Source = bitmap;

            double widthPx = UnitConverter.ToPx(widthCm);
            double heightPx = UnitConverter.ToPx(heightCm);

            BackgroundImage.Width = widthPx;
            BackgroundImage.Height = heightPx;

            // Center horizontally, Top vertically
            Canvas.SetLeft(BackgroundImage, (this.ActualWidth - widthPx) / 2);
            Canvas.SetTop(BackgroundImage, 0);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading background: {ex.Message}");
        }
    }

    public void SetSideBoxVisible(bool visible)
    {
        SideBoxControl.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
    }

    public void SetMainBoxVisible(bool visible)
    {
        MainBoxControl.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
    }

    public void SetSampleTexts(string mainText, string sideText, double mainMaxFontSize, double sideMaxFontSize)
    {
        MainBoxCanvas.Children.Clear();
        SideBoxCanvas.Children.Clear();

        if (!string.IsNullOrWhiteSpace(mainText))
        {
            var lines = mainText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var textConfig = new TextPrintBoxConfigure
            {
                Left = 0, Top = 0, Width = MainBoxControl.ActualWidth, Height = MainBoxControl.ActualHeight,
                ColumnAlign = EnumColumnAlign.Center, ColumnGap = 20
            };
            VerticalTextRenderer.RenderText(MainBoxCanvas, lines, textConfig, mainMaxFontSize, true);
        }

        if (!string.IsNullOrWhiteSpace(sideText) && SideBoxControl.Visibility == Visibility.Visible)
        {
            var lines = sideText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var textConfig = new TextPrintBoxConfigure
            {
                Left = 0, Top = 0, Width = SideBoxControl.ActualWidth, Height = SideBoxControl.ActualHeight,
                ColumnAlign = EnumColumnAlign.Center, ColumnGap = 5
            };
            VerticalTextRenderer.RenderText(SideBoxCanvas, lines, textConfig, sideMaxFontSize, false);
        }
    }

    public void SetScale(double scale)
    {
        CanvasScaleTransform.ScaleX = scale;
        CanvasScaleTransform.ScaleY = scale;
    }

    public void UpdateStamp(string fileName, double widthCm, double heightCm)
    {
        string fullPath = Path.GetFullPath(fileName);
        if (string.IsNullOrEmpty(fileName) || !File.Exists(fullPath))
        {
            StampControl.Visibility = Visibility.Collapsed;
            return;
        }

        try
        {
            var bitmap = new BitmapImage(new Uri(fullPath, UriKind.Absolute));
            StampImage.Source = bitmap;
            StampControl.Width = UnitConverter.ToPx(widthCm);
            StampControl.Height = UnitConverter.ToPx(heightCm);
            StampControl.Visibility = Visibility.Visible;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading stamp: {ex.Message}");
        }
    }

    private void Element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _selectedElement = (FrameworkElement)sender;
        if (_selectedElement.Parent is Grid grid && grid.Parent is ContentControl cc)
        {
            _selectedElement = cc;
        }

        _mouseClickPos = e.GetPosition(MainCanvas);
        _leftPos = Canvas.GetLeft(_selectedElement);
        _topPos = Canvas.GetTop(_selectedElement);

        _selectedElement.CaptureMouse();
        _selectedElement.MouseMove += Element_MouseMove;
        _selectedElement.MouseLeftButtonUp += Element_MouseLeftButtonUp;

        // Position resize handle
        UpdateResizeThumb();

        e.Handled = true;
    }

    private void Element_MouseMove(object sender, MouseEventArgs e)
    {
        if (_selectedElement != null)
        {
            Point currentPos = e.GetPosition(MainCanvas);
            double newLeft = _leftPos + (currentPos.X - _mouseClickPos.X);
            double newTop = _topPos + (currentPos.Y - _mouseClickPos.Y);

            Canvas.SetLeft(_selectedElement, newLeft);
            Canvas.SetTop(_selectedElement, newTop);

            string? name = null;
            if (_selectedElement == StampControl) name = "Stamp";
            else if (_selectedElement == MainBoxControl) name = "Main";
            else if (_selectedElement == SideBoxControl) name = "Side";
            if (name != null)
                ElementMoved?.Invoke(name, newLeft, newTop);

            UpdateResizeThumb();
        }
    }

    private void Element_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_selectedElement != null)
        {
            _selectedElement.ReleaseMouseCapture();
            _selectedElement.MouseMove -= Element_MouseMove;
            _selectedElement.MouseLeftButtonUp -= Element_MouseLeftButtonUp;
        }
    }

    private void UpdateResizeThumb()
    {
        if (_selectedElement == null) return;

        AdornerCanvas.Visibility = Visibility.Visible;
        double left = Canvas.GetLeft(_selectedElement) + _selectedElement.ActualWidth - 4;
        double top = Canvas.GetTop(_selectedElement) + _selectedElement.ActualHeight - 4;

        Canvas.SetLeft(ResizeThumb, left);
        Canvas.SetTop(ResizeThumb, top);
    }

    private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        if (_selectedElement != null)
        {
            double newWidth = _selectedElement.ActualWidth + e.HorizontalChange;
            double newHeight = _selectedElement.ActualHeight + e.VerticalChange;

            if (newWidth > 10) _selectedElement.Width = newWidth;
            if (newHeight > 10) _selectedElement.Height = newHeight;

            string? name = null;
            if (_selectedElement == MainBoxControl) name = "Main";
            else if (_selectedElement == SideBoxControl) name = "Side";
            else if (_selectedElement == StampControl) name = "Stamp";
            if (name != null)
                ElementResized?.Invoke(name, _selectedElement.Width, _selectedElement.Height);

            UpdateResizeThumb();
        }
    }

    private void ResizeThumb_MouseEnter(object sender, MouseEventArgs e) { Cursor = Cursors.SizeNWSE; }
    private void ResizeThumb_MouseLeave(object sender, MouseEventArgs e) { Cursor = Cursors.Arrow; }

    public void GetElementValues(out Rect main, out Rect side, out Rect stamp)
    {
        main = new Rect(Canvas.GetLeft(MainBoxControl), Canvas.GetTop(MainBoxControl), MainBoxControl.ActualWidth, MainBoxControl.ActualHeight);
        side = new Rect(Canvas.GetLeft(SideBoxControl), Canvas.GetTop(SideBoxControl), SideBoxControl.ActualWidth, SideBoxControl.ActualHeight);
        stamp = new Rect(Canvas.GetLeft(StampControl), Canvas.GetTop(StampControl), StampControl.ActualWidth, StampControl.ActualHeight);
    }

    public void SetElementValues(Rect main, Rect side, Rect stamp)
    {
        Canvas.SetLeft(MainBoxControl, main.Left);
        Canvas.SetTop(MainBoxControl, main.Top);
        MainBoxControl.Width = main.Width;
        MainBoxControl.Height = main.Height;

        Canvas.SetLeft(SideBoxControl, side.Left);
        Canvas.SetTop(SideBoxControl, side.Top);
        SideBoxControl.Width = side.Width;
        SideBoxControl.Height = side.Height;

        Canvas.SetLeft(StampControl, stamp.Left);
        Canvas.SetTop(StampControl, stamp.Top);
        StampControl.Width = stamp.Width;
        StampControl.Height = stamp.Height;

        UpdateResizeThumb();
    }
}
