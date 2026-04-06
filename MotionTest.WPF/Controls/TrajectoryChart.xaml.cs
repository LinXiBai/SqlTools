using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using CoreToolkit.StateMachine.Models;
using Microsoft.Win32;

namespace MotionTest.WPF.Controls
{
    /// <summary>
    /// 轨迹图表控件
    /// 显示轴运动轨迹的位置-时间、速度-时间、位置-位置图
    /// </summary>
    public partial class TrajectoryChart : UserControl
    {
        #region 依赖属性

        public TrajectoryRecord Record
        {
            get { return (TrajectoryRecord)GetValue(RecordProperty); }
            set { SetValue(RecordProperty, value); }
        }

        public static readonly DependencyProperty RecordProperty =
            DependencyProperty.Register("Record", typeof(TrajectoryRecord), typeof(TrajectoryChart),
                new PropertyMetadata(null, OnRecordChanged));

        private static void OnRecordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var chart = d as TrajectoryChart;
            chart?.UpdateChart();
        }

        #endregion

        #region 字段

        private readonly Brush[] _axisColors = new Brush[]
        {
            Brushes.Red,
            Brushes.Blue,
            Brushes.Green,
            Brushes.Orange,
            Brushes.Purple,
            Brushes.Brown,
            Brushes.Cyan,
            Brushes.Magenta
        };

        private readonly List<bool> _axisVisibility = new List<bool> { true, true, false, false };
        private DisplayMode _displayMode = DisplayMode.PositionTime;
        
        private double _zoomX = 1.0;
        private double _zoomY = 1.0;
        private Point _panOffset = new Point(0, 0);
        private bool _isPanning = false;
        private Point _lastMousePos;

        private const double MarginLeft = 60;
        private const double MarginBottom = 40;
        private const double MarginTop = 20;
        private const double MarginRight = 20;

        #endregion

        #region 枚举

        public enum DisplayMode
        {
            PositionTime,    // 位置-时间
            VelocityTime,    // 速度-时间
            PositionPosition, // 位置-位置(X-Y)
            Combined         // 综合显示
        }

        #endregion

        public TrajectoryChart()
        {
            InitializeComponent();
            Loaded += (s, e) => UpdateChart();
            SizeChanged += (s, e) => UpdateChart();
        }

        #region 更新图表

        private void UpdateChart()
        {
            chartCanvas.Children.Clear();
            legendPanel.Children.Clear();

            if (Record?.Points == null || Record.Points.Count == 0)
            {
                txtStatus.Text = "无数据";
                return;
            }

            txtStatus.Text = $"记录: {Record.Name}";
            txtDataInfo.Text = $"点数: {Record.Points.Count}, 时长: {Record.Points.Last().ElapsedMs:F1}ms";

            var actualWidth = Math.Max(chartCanvas.ActualWidth, 100);
            var actualHeight = Math.Max(chartCanvas.ActualHeight, 100);
            var plotWidth = actualWidth - MarginLeft - MarginRight;
            var plotHeight = actualHeight - MarginBottom - MarginTop;

            switch (_displayMode)
            {
                case DisplayMode.PositionTime:
                    DrawPositionTimeChart(plotWidth, plotHeight);
                    break;
                case DisplayMode.VelocityTime:
                    DrawVelocityTimeChart(plotWidth, plotHeight);
                    break;
                case DisplayMode.PositionPosition:
                    DrawPositionPositionChart(plotWidth, plotHeight);
                    break;
                case DisplayMode.Combined:
                    DrawCombinedChart(plotWidth, plotHeight);
                    break;
            }

            UpdateLegend();
        }

        #endregion

        #region 绘制图表

        private void DrawPositionTimeChart(double plotWidth, double plotHeight)
        {
            // 计算范围
            double minTime = Record.Points.First().ElapsedMs;
            double maxTime = Record.Points.Last().ElapsedMs;
            double timeRange = maxTime - minTime;
            if (timeRange < 0.001) timeRange = 1;

            double minPos = double.MaxValue, maxPos = double.MinValue;
            for (int axis = 0; axis < Record.AxisCount; axis++)
            {
                if (!_axisVisibility[axis]) continue;
                foreach (var point in Record.Points)
                {
                    if (point.Positions != null && axis < point.Positions.Length)
                    {
                        minPos = Math.Min(minPos, point.Positions[axis]);
                        maxPos = Math.Max(maxPos, point.Positions[axis]);
                    }
                }
            }
            double posRange = maxPos - minPos;
            if (posRange < 0.001) posRange = 1;

            // 绘制网格
            DrawGrid(plotWidth, plotHeight, minTime, maxTime, minPos, maxPos, "时间(ms)", "位置");

            // 绘制数据线
            for (int axis = 0; axis < Record.AxisCount; axis++)
            {
                if (!_axisVisibility[axis]) continue;

                var polyline = new Polyline
                {
                    Stroke = _axisColors[axis % _axisColors.Length],
                    StrokeThickness = 1.5,
                    StrokeLineJoin = PenLineJoin.Round
                };

                var points = new PointCollection();
                foreach (var data in Record.Points)
                {
                    if (data.Positions == null || axis >= data.Positions.Length) continue;
                    
                    double x = MarginLeft + (data.ElapsedMs - minTime) / timeRange * plotWidth * _zoomX + _panOffset.X;
                    double y = MarginTop + plotHeight - (data.Positions[axis] - minPos) / posRange * plotHeight * _zoomY + _panOffset.Y;
                    points.Add(new Point(x, y));
                }

                polyline.Points = points;
                chartCanvas.Children.Add(polyline);
            }
        }

        private void DrawVelocityTimeChart(double plotWidth, double plotHeight)
        {
            // 计算范围
            double minTime = Record.Points.First().ElapsedMs;
            double maxTime = Record.Points.Last().ElapsedMs;
            double timeRange = maxTime - minTime;
            if (timeRange < 0.001) timeRange = 1;

            double minVel = double.MaxValue, maxVel = double.MinValue;
            for (int axis = 0; axis < Record.AxisCount; axis++)
            {
                if (!_axisVisibility[axis]) continue;
                foreach (var point in Record.Points)
                {
                    if (point.Velocities != null && axis < point.Velocities.Length)
                    {
                        minVel = Math.Min(minVel, point.Velocities[axis]);
                        maxVel = Math.Max(maxVel, point.Velocities[axis]);
                    }
                }
            }
            double velRange = maxVel - minVel;
            if (velRange < 0.001) velRange = 1;

            // 绘制网格
            DrawGrid(plotWidth, plotHeight, minTime, maxTime, minVel, maxVel, "时间(ms)", "速度");

            // 绘制数据线
            for (int axis = 0; axis < Record.AxisCount; axis++)
            {
                if (!_axisVisibility[axis]) continue;

                var polyline = new Polyline
                {
                    Stroke = _axisColors[axis % _axisColors.Length],
                    StrokeThickness = 1.5,
                    StrokeLineJoin = PenLineJoin.Round
                };

                var points = new PointCollection();
                foreach (var data in Record.Points)
                {
                    if (data.Velocities == null || axis >= data.Velocities.Length) continue;
                    
                    double x = MarginLeft + (data.ElapsedMs - minTime) / timeRange * plotWidth * _zoomX + _panOffset.X;
                    double y = MarginTop + plotHeight - (data.Velocities[axis] - minVel) / velRange * plotHeight * _zoomY + _panOffset.Y;
                    points.Add(new Point(x, y));
                }

                polyline.Points = points;
                chartCanvas.Children.Add(polyline);
            }
        }

        private void DrawPositionPositionChart(double plotWidth, double plotHeight)
        {
            // X-Y位置图，需要至少2个可见轴
            int xAxis = -1, yAxis = -1;
            for (int i = 0; i < Record.AxisCount && i < _axisVisibility.Count; i++)
            {
                if (_axisVisibility[i])
                {
                    if (xAxis == -1) xAxis = i;
                    else if (yAxis == -1) { yAxis = i; break; }
                }
            }

            if (xAxis == -1 || yAxis == -1)
            {
                var text = new TextBlock
                {
                    Text = "需要至少2个可见轴",
                    Foreground = Brushes.Gray,
                    FontSize = 14,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Canvas.SetLeft(text, plotWidth / 2);
                Canvas.SetTop(text, plotHeight / 2);
                chartCanvas.Children.Add(text);
                return;
            }

            // 计算范围
            double minX = double.MaxValue, maxX = double.MinValue;
            double minY = double.MaxValue, maxY = double.MinValue;

            foreach (var point in Record.Points)
            {
                if (point.Positions == null) continue;
                if (xAxis < point.Positions.Length)
                {
                    minX = Math.Min(minX, point.Positions[xAxis]);
                    maxX = Math.Max(maxX, point.Positions[xAxis]);
                }
                if (yAxis < point.Positions.Length)
                {
                    minY = Math.Min(minY, point.Positions[yAxis]);
                    maxY = Math.Max(maxY, point.Positions[yAxis]);
                }
            }

            double xRange = maxX - minX;
            double yRange = maxY - minY;
            if (xRange < 0.001) xRange = 1;
            if (yRange < 0.001) yRange = 1;

            // 绘制网格
            DrawGrid(plotWidth, plotHeight, minX, maxX, minY, maxY, 
                $"轴{xAxis}位置", $"轴{yAxis}位置");

            // 绘制轨迹
            var polyline = new Polyline
            {
                Stroke = _axisColors[0],
                StrokeThickness = 1.5,
                StrokeLineJoin = PenLineJoin.Round
            };

            var points = new PointCollection();
            foreach (var data in Record.Points)
            {
                if (data.Positions == null || xAxis >= data.Positions.Length || yAxis >= data.Positions.Length) continue;
                
                double x = MarginLeft + (data.Positions[xAxis] - minX) / xRange * plotWidth * _zoomX + _panOffset.X;
                double y = MarginTop + plotHeight - (data.Positions[yAxis] - minY) / yRange * plotHeight * _zoomY + _panOffset.Y;
                points.Add(new Point(x, y));
            }

            polyline.Points = points;
            chartCanvas.Children.Add(polyline);

            // 标记起点和终点
            if (Record.Points.Count > 0)
            {
                var first = Record.Points.First();
                var last = Record.Points.Last();

                if (first.Positions != null && xAxis < first.Positions.Length && yAxis < first.Positions.Length)
                {
                    AddMarker(MarginLeft + (first.Positions[xAxis] - minX) / xRange * plotWidth * _zoomX + _panOffset.X,
                             MarginTop + plotHeight - (first.Positions[yAxis] - minY) / yRange * plotHeight * _zoomY + _panOffset.Y,
                             Brushes.Green, "S");
                }

                if (last.Positions != null && xAxis < last.Positions.Length && yAxis < last.Positions.Length)
                {
                    AddMarker(MarginLeft + (last.Positions[xAxis] - minX) / xRange * plotWidth * _zoomX + _panOffset.X,
                             MarginTop + plotHeight - (last.Positions[yAxis] - minY) / yRange * plotHeight * _zoomY + _panOffset.Y,
                             Brushes.Red, "E");
                }
            }
        }

        private void DrawCombinedChart(double plotWidth, double plotHeight)
        {
            // 简化为位置-时间图
            DrawPositionTimeChart(plotWidth, plotHeight);
        }

        private void DrawGrid(double plotWidth, double plotHeight, 
            double minX, double maxX, double minY, double maxY, 
            string xLabel, string yLabel)
        {
            // 绘制坐标轴
            var xAxis = new Line
            {
                X1 = MarginLeft,
                Y1 = MarginTop + plotHeight,
                X2 = MarginLeft + plotWidth,
                Y2 = MarginTop + plotHeight,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            chartCanvas.Children.Add(xAxis);

            var yAxis = new Line
            {
                X1 = MarginLeft,
                Y1 = MarginTop,
                X2 = MarginLeft,
                Y2 = MarginTop + plotHeight,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            chartCanvas.Children.Add(yAxis);

            // 绘制网格线
            int gridLines = 5;
            for (int i = 0; i <= gridLines; i++)
            {
                double y = MarginTop + plotHeight * i / gridLines;
                
                var gridLine = new Line
                {
                    X1 = MarginLeft,
                    Y1 = y,
                    X2 = MarginLeft + plotWidth,
                    Y2 = y,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 0.5,
                    StrokeDashArray = new DoubleCollection { 4, 2 }
                };
                chartCanvas.Children.Add(gridLine);

                // Y轴标签
                double value = minY + (maxY - minY) * (1 - (double)i / gridLines);
                var label = new TextBlock
                {
                    Text = FormatValue(value),
                    FontSize = 9,
                    Foreground = Brushes.Gray
                };
                Canvas.SetLeft(label, 5);
                Canvas.SetTop(label, y - 8);
                chartCanvas.Children.Add(label);
            }

            for (int i = 0; i <= gridLines; i++)
            {
                double x = MarginLeft + plotWidth * i / gridLines;
                
                var gridLine = new Line
                {
                    X1 = x,
                    Y1 = MarginTop,
                    X2 = x,
                    Y2 = MarginTop + plotHeight,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 0.5,
                    StrokeDashArray = new DoubleCollection { 4, 2 }
                };
                chartCanvas.Children.Add(gridLine);

                // X轴标签
                double value = minX + (maxX - minX) * i / gridLines;
                var label = new TextBlock
                {
                    Text = FormatValue(value),
                    FontSize = 9,
                    Foreground = Brushes.Gray
                };
                Canvas.SetLeft(label, x - 15);
                Canvas.SetTop(label, MarginTop + plotHeight + 5);
                chartCanvas.Children.Add(label);
            }

            // 轴标签
            var xAxisLabel = new TextBlock
            {
                Text = xLabel,
                FontSize = 10,
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold
            };
            Canvas.SetLeft(xAxisLabel, MarginLeft + plotWidth / 2 - 20);
            Canvas.SetTop(xAxisLabel, MarginTop + plotHeight + 20);
            chartCanvas.Children.Add(xAxisLabel);

            var yAxisLabel = new TextBlock
            {
                Text = yLabel,
                FontSize = 10,
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold,
                LayoutTransform = new RotateTransform(-90)
            };
            Canvas.SetLeft(yAxisLabel, 5);
            Canvas.SetTop(yAxisLabel, MarginTop + plotHeight / 2);
            chartCanvas.Children.Add(yAxisLabel);
        }

        private void AddMarker(double x, double y, Brush color, string text)
        {
            var ellipse = new Ellipse
            {
                Width = 12,
                Height = 12,
                Fill = color,
                Stroke = Brushes.White,
                StrokeThickness = 2
            };
            Canvas.SetLeft(ellipse, x - 6);
            Canvas.SetTop(ellipse, y - 6);
            chartCanvas.Children.Add(ellipse);

            var label = new TextBlock
            {
                Text = text,
                FontSize = 8,
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold
            };
            Canvas.SetLeft(label, x - 4);
            Canvas.SetTop(label, y - 5);
            chartCanvas.Children.Add(label);
        }

        private void UpdateLegend()
        {
            legendPanel.Children.Clear();

            for (int i = 0; i < Record?.AxisCount; i++)
            {
                if (i >= _axisVisibility.Count) continue;

                var item = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(2),
                    Opacity = _axisVisibility[i] ? 1.0 : 0.3
                };

                var colorBox = new Rectangle
                {
                    Width = 12,
                    Height = 12,
                    Fill = _axisColors[i % _axisColors.Length],
                    Stroke = Brushes.Gray,
                    StrokeThickness = 0.5,
                    Margin = new Thickness(0, 0, 5, 0)
                };

                var name = Record.AxisNames != null && i < Record.AxisNames.Length 
                    ? Record.AxisNames[i] 
                    : $"轴{i}";

                var text = new TextBlock
                {
                    Text = name,
                    FontSize = 10,
                    VerticalAlignment = VerticalAlignment.Center
                };

                item.Children.Add(colorBox);
                item.Children.Add(text);
                legendPanel.Children.Add(item);
            }
        }

        private string FormatValue(double value)
        {
            if (Math.Abs(value) >= 10000)
                return value.ToString("F0", CultureInfo.InvariantCulture);
            if (Math.Abs(value) >= 100)
                return value.ToString("F1", CultureInfo.InvariantCulture);
            if (Math.Abs(value) >= 1)
                return value.ToString("F2", CultureInfo.InvariantCulture);
            return value.ToString("F4", CultureInfo.InvariantCulture);
        }

        #endregion

        #region 事件处理

        private void cmbDisplayMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _displayMode = (DisplayMode)cmbDisplayMode.SelectedIndex;
            UpdateChart();
        }

        private void AxisVisibilityChanged(object sender, RoutedEventArgs e)
        {
            var chk = sender as CheckBox;
            if (chk == null) return;

            int axisIndex = -1;
            if (chk == chkShowAxis0) axisIndex = 0;
            else if (chk == chkShowAxis1) axisIndex = 1;
            else if (chk == chkShowAxis2) axisIndex = 2;
            else if (chk == chkShowAxis3) axisIndex = 3;

            if (axisIndex >= 0 && axisIndex < _axisVisibility.Count)
            {
                _axisVisibility[axisIndex] = chk.IsChecked == true;
                UpdateChart();
            }
        }

        private void btnZoomIn_Click(object sender, RoutedEventArgs e)
        {
            _zoomX *= 1.2;
            _zoomY *= 1.2;
            UpdateChart();
        }

        private void btnZoomOut_Click(object sender, RoutedEventArgs e)
        {
            _zoomX /= 1.2;
            _zoomY /= 1.2;
            UpdateChart();
        }

        private void btnResetZoom_Click(object sender, RoutedEventArgs e)
        {
            _zoomX = 1.0;
            _zoomY = 1.0;
            _panOffset = new Point(0, 0);
            UpdateChart();
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            ExportToCsv();
        }

        private void chartCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                _zoomX *= 1.1;
                _zoomY *= 1.1;
            }
            else
            {
                _zoomX /= 1.1;
                _zoomY /= 1.1;
            }
            UpdateChart();
            e.Handled = true;
        }

        private void chartCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isPanning = true;
            _lastMousePos = e.GetPosition(chartCanvas);
            chartCanvas.CaptureMouse();
        }

        private void chartCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isPanning = false;
            chartCanvas.ReleaseMouseCapture();
        }

        private void chartCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isPanning)
            {
                var pos = e.GetPosition(chartCanvas);
                _panOffset.X += pos.X - _lastMousePos.X;
                _panOffset.Y += pos.Y - _lastMousePos.Y;
                _lastMousePos = pos;
                UpdateChart();
            }

            // 显示光标位置信息
            UpdateCursorInfo(e.GetPosition(chartCanvas));
        }

        private void UpdateCursorInfo(Point mousePos)
        {
            if (Record?.Points == null || Record.Points.Count == 0) return;

            // 简单的光标位置显示
            txtCursorInfo.Text = $"光标: ({mousePos.X:F0}, {mousePos.Y:F0})";
        }

        private void ExportToCsv()
        {
            if (Record == null) return;

            var dialog = new SaveFileDialog
            {
                FileName = $"{Record.Name}.csv",
                Filter = "CSV文件 (*.csv)|*.csv|所有文件 (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var csv = RecordToCsv(Record);
                    File.WriteAllText(dialog.FileName, csv);
                    MessageBox.Show("导出成功!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"导出失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private string RecordToCsv(TrajectoryRecord record)
        {
            var lines = new List<string>();
            
            // 表头
            var headers = new List<string> { "Time(ms)" };
            for (int i = 0; i < record.AxisCount; i++)
            {
                var name = record.AxisNames != null && i < record.AxisNames.Length 
                    ? record.AxisNames[i] : $"Axis{i}";
                headers.Add($"{name}_Pos");
                headers.Add($"{name}_Vel");
                headers.Add($"{name}_Status");
            }
            lines.Add(string.Join(",", headers));

            // 数据
            foreach (var point in record.Points)
            {
                var values = new List<string> { point.ElapsedMs.ToString("F2", CultureInfo.InvariantCulture) };
                for (int i = 0; i < record.AxisCount; i++)
                {
                    if (point.Positions != null && i < point.Positions.Length)
                        values.Add(point.Positions[i].ToString("F4", CultureInfo.InvariantCulture));
                    else
                        values.Add("");

                    if (point.Velocities != null && i < point.Velocities.Length)
                        values.Add(point.Velocities[i].ToString("F4", CultureInfo.InvariantCulture));
                    else
                        values.Add("");

                    if (point.Statuses != null && i < point.Statuses.Length)
                        values.Add(point.Statuses[i].ToString());
                    else
                        values.Add("");
                }
                lines.Add(string.Join(",", values));
            }

            return string.Join("\n", lines);
        }

        #endregion
    }
}
