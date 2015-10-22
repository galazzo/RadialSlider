using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
 
using System.Windows.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Core;

namespace Galazzo
{
    public sealed partial class RadialSliderEventArgs : Windows.UI.Xaml.RoutedEventArgs
    {
        public double Value { get; set; }
    }

    public sealed partial class RadialSliderItem
    {
        public string Value { get; set; }
        public BitmapImage Icon { get; set; }

        public RadialSliderItem()
        {
            this.Value = "";
            this.Icon = null;
        }

    }
    public enum ValuesDirectionModeTypes
    {
        Clockwise,
        Counterclockwise
    }

    public enum IndicatorDirectionModeTypes
    {
        SameWay,
        OppositeWay
    }

    public enum DisplayValueModeTypes
    {
        Raw,
        FromRanges
    }

    public enum RangeValuesRotationModeTypes
    {
        Linear,
        Perpendicular
    }

    public sealed partial class RadialSlider : UserControl
    {

        private CoreDispatcher dispatcher = Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher;

        #region events
        public event EventHandler<RadialSliderEventArgs> Changed;
        public event EventHandler<RoutedEventArgs> ChangeStarted;
        public event EventHandler<RoutedEventArgs> ChangeCompleted;                
        private RadialSliderEventArgs eventArgs = new RadialSliderEventArgs();
        #endregion
        

        #region private variables
        private double _value = 0;
        // private Thickness originalPosition;
        //  private double IndicatorWidth = 40;
        // private double IndicatortHeight = 40;
        private int m_layoutPosition = 0;
        private int FirstRadius = 290;
        private double ScreenWidth = 400;
        private double ScreenHeight = 400;        
        private double _start_angle = 60;
        private double _end_angle = 360;

        static private int InnerRadius = 100;
        static private int CircleThickness = 100;
        static private int ThicknessArcHeight = 400;
        static private int InnerBorderHeight = ThicknessArcHeight - (CircleThickness * 2);

        private List<RadialSliderItem> _ranges = null;
        #endregion

        #region Properties
        public int RoundToIhDecimal { get; set; }
        public double DivideValuesBy { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public string ValueToDisplay { get; set; }        
        public Brush BackgroundCircle
        {
            get
            {
                return ThicknessArc.Stroke;
            }

            set
            {
                LayoutRoot.Background = value;

            }
        }
        public ImageSource IndicatorImageSource
        {
            get
            {
                return Indicator.Source;
            }

            set
            {
                Indicator.Source = value;
            }
        }
        public double StartAngle
        {
            get
            {
                return _start_angle;
            }

            set
            {
                _start_angle = value;
                // AngleWideRanges = (_end_angle - _start_angle) % 360;
                RenderArc();
            }
        }
        public double EndAngle
        {
            get
            {
                return _end_angle;
            }

            set
            {
                _end_angle = value;
                //AngleWideRanges = (_end_angle - _start_angle) % 360;
                RenderArc();
            }
        }
        public IndicatorDirectionModeTypes IndicatorDirectionMode { get; set; }
        public ValuesDirectionModeTypes ValuesDirectionMode { get; set; }
        public double InnerBorderMargin
        {
            get; set;
        }
        public double OuterBorderMargin
        {
            get; set;
        }
        public bool IsValueShowed
        {
            get
            {
                return (ValueText.Visibility == Visibility.Visible);
            }

            set
            {
                ValueText.Visibility = (value == true) ? Visibility.Visible : Visibility.Collapsed;
            }
        }        
        
        public DisplayValueModeTypes DisplayValueMode
        {
            get; set;
        }
        public RangeValuesRotationModeTypes RangeValuesRotation
        {
            get; set;
        }
        #endregion


        public RadialSlider()
        {
            this.InitializeComponent();
                        
            Min = 0;
            Max = 100;

            DivideValuesBy = 1;
            RoundToIhDecimal = 0;

            ScreenWidth = 400; // (int)this.ActualWidth; 
            ScreenHeight = 400; // (int)this.ActualHeight; 
            FirstRadius = 300; // (int)((ScreenHeight - InnerRadius) / 2);

            System.Diagnostics.Debug.WriteLine("ScreenWidth:" + ScreenWidth);
            System.Diagnostics.Debug.WriteLine("ScreenHeight:" + ScreenHeight);

            _ranges = new List<RadialSliderItem>();
            IndicatorDirectionMode = IndicatorDirectionModeTypes.SameWay;
            ValuesDirectionMode = ValuesDirectionModeTypes.Counterclockwise;

            InnerBorderMargin = 0;

            IsValueShowed = true;
            DisplayValueMode = DisplayValueModeTypes.Raw;

            RangeValuesRotation = RangeValuesRotationModeTypes.Perpendicular;
        }
                
        
        private void RenderArc()
        {
            if (FirstRadius <= 0 || ScreenHeight <= 0 || ScreenWidth <= 0) return;

            double __end_angle = ((_end_angle - _start_angle) == 360.0) ? _end_angle - 0.01 : _end_angle;

            double begin = _start_angle * (Math.PI / 180);
            double end = __end_angle * (Math.PI / 180);
            double radius = (FirstRadius - (InnerRadius / 4));

            if (radius <= 0) return;

            
            //double startX = (LayoutRoot.ActualWidth - ScreenWidth) + (ScreenWidth /2) + (radius * Math.Cos(begin));
            double startX = (ScreenWidth / 2) + (radius * Math.Cos(begin));
            double startY = (ScreenHeight / 2) - (radius * Math.Sin(begin));

            //double endX = (LayoutRoot.ActualWidth - ScreenWidth) + (ScreenWidth /2) + (radius * Math.Cos(end));
            double endX = (ScreenWidth / 2) + (radius * Math.Cos(end));
            double endY = (ScreenHeight / 2) - (radius * Math.Sin(end));

            PathGeometry outerGeometry = new PathGeometry();
            PathGeometry innerBorderGeometry = new PathGeometry();
            
            PathFigure outerFigure = new PathFigure();
            outerFigure.StartPoint = new Point(startX, startY);

            var outerArc = new ArcSegment();
            outerArc.Point = new Point(endX, endY);
            outerArc.Size = new Size(radius, radius);
            outerArc.RotationAngle = 0;
            outerArc.IsLargeArc = (((__end_angle - _start_angle)%360) > 180.0);
            outerArc.SweepDirection = SweepDirection.Counterclockwise;
            
            outerFigure.Segments.Add(outerArc);
            outerGeometry.Figures.Add(outerFigure);

            #region inner border
            radius = (FirstRadius - (InnerRadius/2) - InnerBorderMargin);

            startX = (ScreenWidth / 2) + (radius * Math.Cos(begin));
            startY = (ScreenHeight / 2) - (radius * Math.Sin(begin));

            endX = (ScreenWidth / 2) + (radius * Math.Cos(end));
            endY = (ScreenHeight / 2) - (radius * Math.Sin(end));

            PathFigure innerBorderFigure = new PathFigure();
            innerBorderFigure.StartPoint = new Point(startX, startY);

            var innerBorderArc = new ArcSegment();
            innerBorderArc.Point = new Point(endX, endY);
            innerBorderArc.Size = new Size(radius, radius);
            innerBorderArc.RotationAngle = 0;
            innerBorderArc.IsLargeArc = (((__end_angle - _start_angle) % 360) > 180.0);
            innerBorderArc.SweepDirection = SweepDirection.Counterclockwise;

            innerBorderFigure.Segments.Add(innerBorderArc);
            innerBorderGeometry.Figures.Add(innerBorderFigure);
            #endregion

            #region outer border
            PathGeometry outerBorderGeometry = new PathGeometry();
            radius = (FirstRadius +  OuterBorderMargin);

            startX = (ScreenWidth / 2) + (radius * Math.Cos(begin));
            startY = (ScreenHeight / 2) - (radius * Math.Sin(begin));

            endX = (ScreenWidth / 2) + (radius * Math.Cos(end));
            endY = (ScreenHeight / 2) - (radius * Math.Sin(end));

            PathFigure outerBorderFigure = new PathFigure();
            outerBorderFigure.StartPoint = new Point(startX, startY);

            var outerBorderArc = new ArcSegment();
            outerBorderArc.Point = new Point(endX, endY);
            outerBorderArc.Size = new Size(radius, radius);
            outerBorderArc.RotationAngle = 0;
            outerBorderArc.IsLargeArc = (((__end_angle - _start_angle) % 360) > 180.0);
            outerBorderArc.SweepDirection = SweepDirection.Counterclockwise;

            outerBorderFigure.Segments.Add(outerBorderArc);
            outerBorderGeometry.Figures.Add(outerBorderFigure);
            #endregion

            ThicknessArc.Data = outerGeometry;
            InnerBorder.Data = innerBorderGeometry;
            OuterBorder.Data = outerBorderGeometry;
        }

        private void RenderRanges()
        {
            if(_ranges!= null)
            {
                double _ScreenWidth = ScreenWidth;// Window.Current.Bounds.Width / 2; // Application.Current.Host.Content.ActualWidth / 2;
                double _ScreenHeight = ScreenHeight; // Width;

                double X = 0.0;
                double Y = 0.0;

                double angle = 0.0;
                double radians = 0.0;

                double AngleWideRanges = ((_end_angle - _start_angle) == 360) ? 360 : (_end_angle - _start_angle) % 360;

               // LayoutRoot.Children.Clear(); RenderArc();

                for (int i = 0; i < _ranges.Count; i++)
                {
                    if (_ranges[i].Icon == null)
                    {
                        Windows.UI.Xaml.Shapes.Line line = new Windows.UI.Xaml.Shapes.Line();

                        TextBlock element = new TextBlock();
                        element.FontSize = 16;
                        element.Text = _ranges[i].Value;
                        element.HorizontalAlignment = HorizontalAlignment.Center; element.VerticalAlignment = VerticalAlignment.Center;

                        element.RenderTransformOrigin = new Point(0.5, 0.5);

                        angle = _start_angle + ((_ranges.Count == 1) ? 0 : ((i * (AngleWideRanges / (_ranges.Count - 1)))));
                        radians = angle * (Math.PI / 180);

                        X = +(((ScreenWidth - InnerRadius - 20) / 2) * Math.Cos(radians)) - ((element.ActualWidth) / 2);
                        Y = -(((ScreenWidth - InnerRadius - 20) / 2) * Math.Sin(radians)) - ((element.ActualHeight) / 2);

                        line.Stroke = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 0, 0));

                        line.X1 = (ScreenWidth / 2) + (((FirstRadius + 0) / 2) * Math.Cos(radians));
                        line.Y1 = (ScreenHeight / 2) - (((FirstRadius + 0) / 2) * Math.Sin(radians));
                        line.X2 = (ScreenWidth / 2) + (((FirstRadius + InnerRadius) / 2) * Math.Cos(radians));
                        line.Y2 = (ScreenHeight / 2) - (((FirstRadius + InnerRadius) / 2) * Math.Sin(radians));
                        LayoutRoot.Children.Add(line);

                        double angle_compensation = 0;
                        switch (RangeValuesRotation)
                        {
                            case RangeValuesRotationModeTypes.Perpendicular: angle_compensation = (((angle % 360) > 180) && ((angle % 360) < 360)) ? 270 : 90; break;
                            case RangeValuesRotationModeTypes.Linear: angle_compensation = (((angle % 360) > 270) || ((angle % 350) < 90)) ? 0 : 180; break;
                            default: break;
                        }
                        element.RenderTransform = new CompositeTransform() { Rotation = angle_compensation - angle, TranslateX = X, TranslateY = Y };
                        LayoutRoot.Children.Add(element);
                    }
                    else
                    {
                        Image element = new Image();
                        element.Source = _ranges[i].Icon;
                        element.Width = 20;
                        element.Height = 20;
                        element.HorizontalAlignment = HorizontalAlignment.Left;
                        element.VerticalAlignment = VerticalAlignment.Top;

                        element.Margin = new Thickness(-(element.Width / 2), -(element.Height / 2), 0, 0);

                        // LayoutRoot.Children.Add(element);

                        element.RenderTransformOrigin = new Point(0.5, 0.5);

                        angle = (_ranges.Count == 1) ? 0 : (-(AngleWideRanges / 2) + (i * (AngleWideRanges / (_ranges.Count - 1))));
                        radians = angle * (Math.PI / 180);
                        X = (ScreenWidth - element.ActualWidth / 2) - ((FirstRadius + 10 + element.ActualHeight / 2) * Math.Cos(radians));
                        Y = ((ScreenHeight / 2) - element.ActualHeight / 2) - ((FirstRadius + 10 + element.ActualHeight / 2) * Math.Sin(radians));

                        element.RenderTransform = new CompositeTransform() { Rotation = 270 + angle, TranslateX = X, TranslateY = Y };
                    }

                }
                Focus(FocusState.Pointer);
            }
        }

        public int LayoutPosition
        {
            get
            {
                return m_layoutPosition;
            }

            set
            {
                m_layoutPosition = value;
                int CircleThickness_2 = CircleThickness / 2;

                Width = ScreenHeight + (value * CircleThickness);
                ScreenHeight = Width;

                ThicknessArc.Height = ThicknessArcHeight + (value * CircleThickness * 2); ThicknessArc.Width = ThicknessArc.Height;
                InnerBorder.Height = InnerBorderHeight + (value * CircleThickness * 2); InnerBorder.Width = InnerBorder.Height;

                ThicknessArc.Margin = new Thickness(0, -CircleThickness_2 - (value * CircleThickness), 0, 0);
                InnerBorder.Margin = new Thickness(100, CircleThickness_2 - (value * CircleThickness), 0, 0);

                InnerRadius = (int)(InnerBorder.Height / 2); // (InnerBorder.Height / 2) + (value * CircleThickness * 2);


                Canvas.SetZIndex(this.LayoutRoot, 100 - value);
                //Canvas.SetZIndex(this.ContentPanelCanvas, 1000 - (value * 10));
                Canvas.SetZIndex(this.Indicator, 10000 - (value * 100));
                UpdateLayout();

                // System.Diagnostics.Debug.WriteLine(this.Name + " Layout - ZIndex: " + Canvas.GetZIndex(this.LayoutRoot));
                //System.Diagnostics.Debug.WriteLine(this.Name + " ContentPanelCanvas - ZIndex: " + Canvas.GetZIndex(this.ContentPanelCanvas));
                //// System.Diagnostics.Debug.WriteLine(this.Name + " Indicator - ZIndex: " + Canvas.GetZIndex(this.Indicator));
                // System.Diagnostics.Debug.WriteLine(this.Name + " ThicknessArc - ZIndex: " + Canvas.GetZIndex(this.ThicknessArc));
                // System.Diagnostics.Debug.WriteLine(this.Name + " InnerBorder - ZIndex: " + Canvas.GetZIndex(this.InnerBorder));
                // System.Diagnostics.Debug.WriteLine(this.Name + " ValueText - ZIndex: " + Canvas.GetZIndex(this.ValueText));

            }
        }

        public double Value
        {
            get
            {
                return _value;
            }

            set
            {
                if (_ranges != null && (_ranges.Count > 0))
                {
                    _value = value;
                    MoveIndicatorTo(_value);

                    /*System.Diagnostics.Debug.WriteLine("value " + _value);
                    System.Diagnostics.Debug.WriteLine("(Math.Round(_value) - Min) " + (Math.Round(_value) - Min));
                    System.Diagnostics.Debug.WriteLine("(Max-Min) " + (Max - Min));*/

                    try {
                        int rounded_value = (int)(((Math.Round(_value) - Min) / (Max - Min)) * _ranges.Count); if (rounded_value > (_ranges.Count - 1)) rounded_value = _ranges.Count - 1;

                        //System.Diagnostics.Debug.WriteLine("rounded_value:" + rounded_value + " of " + _value);

                        if (_ranges[rounded_value].Icon != null)
                        {
                            ValueText.Visibility = Visibility.Collapsed;
                            //ValueImage.Visibility = System.Windows.Visibility.Visible;
                            //ValueImage.Source = _ranges[rounded_value].Icon;
                        }
                        else
                        {
                            ValueText.Visibility = Visibility.Visible;
                            //ValueImage.Visibility = System.Windows.Visibility.Collapsed;

                            ValueText.Text = String.IsNullOrEmpty(ValueToDisplay) ? ((DisplayValueMode == DisplayValueModeTypes.Raw) ? Math.Round(_value, RoundToIhDecimal).ToString() : _ranges[rounded_value].Value) : ValueToDisplay;
                            ValueText.Margin = new Thickness(((ScreenWidth / 2) - (ValueText.ActualWidth / 2)), ((ScreenHeight / 2) - (ValueText.ActualHeight / 2)), 0, 0);                            
                        }
                        eventArgs.Value = _value;
                        OnChanged(eventArgs);
                    } catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Value exception: " + ex.Message);
                    }

                }
            }
        }

        public IEnumerable<RadialSliderItem> Ranges
        {
            get
            {
                return _ranges;
            }

            set
            {
                _ranges = null; GC.Collect();
                _ranges = value.ToList();

                RenderRanges();
            }

        }
                
        private void Indicator_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            Point position = e.GetCurrentPoint(this.LayoutRoot).Position;
            //System.Diagnostics.Debug.WriteLine(this.Name+": "+position.X + " " + position.Y);

            double Y = ((ScreenHeight / 2) - position.Y);
            double X = ((ScreenWidth / 2) - position.X);

            double radians = Math.Atan2(Y, X);
            double degree = Math.Floor(radians * (180 / Math.PI));

            degree = 180 - degree;

            double __end_angle = ((_end_angle - _start_angle) == 360.0) ? _end_angle - 0.01 : _end_angle;

            double AngleWideRanges = (__end_angle - _start_angle) % 360;           
            double ratio = (degree - _start_angle) / AngleWideRanges;
           

            //double _Value = (IndicatorDirectionMode==IndicatorDirection.SameWay) ? (Min + ratio * (Max - Min)) : (Max - ratio * (Max - Min));
            double _Value = (ValuesDirectionMode == ValuesDirectionModeTypes.Counterclockwise) ? (Min + ratio * (Max - Min)) : (Max - (Min + ratio * (Max - Min)));

            //System.Diagnostics.Debug.WriteLine("degree: " + degree);
           // System.Diagnostics.Debug.WriteLine("_Value:" + _Value);

            if ((degree >= _start_angle) && ( degree <= __end_angle) && Value != _Value)
            {
                //Indicator.RenderTransform = new CompositeTransform() {TranslateX = X, TranslateY = Y };
                //Indicator.Margin = new Thickness(X, Y, 0, 0);
                Value = _Value / DivideValuesBy;

               // System.Diagnostics.Debug.WriteLine("Value:" + Value);

                eventArgs.Value = Value;
                OnChanged(eventArgs);
            }
        }

        private void MoveIndicatorTo(double value)
        {
            value = (ValuesDirectionMode == ValuesDirectionModeTypes.Counterclockwise) ? value : Max - value;

            double __end_angle = ((_end_angle - _start_angle) == 360.0) ? _end_angle - 0.01 : _end_angle;

            double AngleWideRanges = (__end_angle - _start_angle) % 360;
            //System.Diagnostics.Debug.WriteLine("MoveIndicatorTo:" + value);
            double ratio = (value - Min) / (Max - Min);         
            double degree =  (ratio * AngleWideRanges) + _start_angle;
           // System.Diagnostics.Debug.WriteLine("AngleWideRanges:" + AngleWideRanges+" degree:" + degree);
            double radians = degree * (Math.PI / 180);
            double awidth = Indicator.ActualWidth / 2; if (awidth <= 0) awidth = 40;
            double aheight = Indicator.ActualHeight / 2; if (aheight <= 0) aheight = 40;
            
            if (value <= Max && value >= Min)
            {
                double X = (ScreenWidth / 2) + (FirstRadius * Math.Cos(radians));
                double Y = (ScreenWidth / 2) - (FirstRadius * Math.Sin(radians));

                /*System.Diagnostics.Debug.WriteLine("\n");
                System.Diagnostics.Debug.WriteLine("X:" + (FirstRadius * Math.Cos(radians)));
                System.Diagnostics.Debug.WriteLine("Y:" + (FirstRadius * Math.Sin(radians)));
                System.Diagnostics.Debug.WriteLine("ratio:" + ratio);
                System.Diagnostics.Debug.WriteLine("radians:" + radians);
                System.Diagnostics.Debug.WriteLine("FirstRadius:" + FirstRadius);
                System.Diagnostics.Debug.WriteLine("MoveIndicatorTo (" + X+","+Y+") ["+ ScreenWidth + ","+ ScreenHeight + "]");*/
                Indicator.Margin = new Thickness(X - awidth, Y - aheight, 0, 0);
            }

        }

        private void OnChanged(RadialSliderEventArgs e)
        {
            EventHandler<RadialSliderEventArgs> handler = Changed;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void Indicator_GotFocus(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Focus");
        }
                
        private void Indicator_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Indicator_MouseLeftButtonDown");
            StartAngle = 45;
        }

        private void Indicator_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Indicator_ManipulationStarted");
            EventHandler<RoutedEventArgs> handler = ChangeStarted;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void Indicator_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Indicator_ManipulationCompleted");
            EventHandler<RoutedEventArgs> handler = ChangeCompleted;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public bool IsVisible()
        {
            return (Visibility == Visibility.Visible);
        }

        public async void Show()
        {
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Visibility = Visibility.Visible;
            });
        }

        public async void Close()
        {
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Visibility = Visibility.Collapsed;
            });
        }

        public void Rotate(double? from, double? to)
        {
            Duration Time_duration = new Duration(TimeSpan.FromSeconds(1));
            Storyboard storyboard = new Storyboard();
            storyboard.Duration = Time_duration;
            DoubleAnimation doubleAnimation = new DoubleAnimation();
            doubleAnimation.Duration = Time_duration;
            storyboard.Children.Add(doubleAnimation);
            RotateTransform rotateTransform = new RotateTransform();
            Storyboard.SetTarget(doubleAnimation, rotateTransform);
            //Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("Angle"));
            Storyboard.SetTargetProperty(doubleAnimation, "Angle");
            doubleAnimation.From = from;
            doubleAnimation.To = to;

            //LayoutRoot.RenderTransform = rotateTransform;
            ValueText.RenderTransform = rotateTransform;
            Indicator.RenderTransform = rotateTransform;
            //ValueImage.RenderTransform = rotateTransform;

            storyboard.Begin();
        }

        /*public void RotateLaytout(double angle)
        {
            RotateTransform rotateTransform = new RotateTransform();
            rotateTransform.Angle = angle;
            LayoutRoot.RenderTransform = rotateTransform;
        }*/

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine(e.NewSize.Width+";"+e.NewSize.Height);
            
            double side = Math.Min(e.NewSize.Width, e.NewSize.Height);
            double thickness = (side - InnerRadius) ;

            LayoutRoot.Width = side;
            LayoutRoot.Height = side;
            ScreenWidth = side;
            ScreenHeight = side;
            FirstRadius = (int)(side / 2);

           // ThicknessArc.Width = side; ThicknessArc.Height = side; ThicknessArc.StrokeThickness = InnerRadius/2;            
          //  InnerBorder.Width = thickness; InnerBorder.Height = thickness;

            ValueText.Margin = new Thickness( (side/2) - (ValueText.ActualWidth/2), (side/2) - (ValueText.ActualHeight/2), 0,0 );

            MoveIndicatorTo(_value);
            RenderArc();
           // RenderRanges();
        }
    }
}
