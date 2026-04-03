using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using SqlDemo.Data;

namespace MotionTest.WPF
{
    public partial class DatabaseManager : Window
    {
        private SqliteDbContext _dbContext;
        private AxisParameterRepository _axisRepo;
        private IOParameterRepository _ioRepo;
        private AxisParameter _currentAxis;
        private IOParameter _currentIO;

        // 轴名称列表
        private readonly string[] _axisNames = {
            "芯片取料X轴",
            "芯片取料Y轴",
            "芯片取料Z轴",
            "芯片取料F轴",
            "热沉取料X轴",
            "热沉取料Y轴",
            "热沉取料Z轴",
            "热沉取料F轴"
        };

        // IO名称随机生成
        private readonly string[] _ioNamePrefixes = {
            "限位", "传感器", "按钮", "指示灯", "继电器", "电磁阀", "电机", "泵"
        };

        public DatabaseManager()
        {
            InitializeComponent();
            InitializeDatabase();
            InitializeTabs();
            InitializeAxes();
            InitializeIOs();
        }

        private void InitializeDatabase()
        {
            try
            {
                // 使用绝对路径创建数据库文件
                string dbPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "motion.db");
                _dbContext = new SqliteDbContext(dbPath);
                _dbContext.InitDatabase(); // 初始化数据库表结构
                _axisRepo = new AxisParameterRepository(_dbContext);
                _ioRepo = new IOParameterRepository(_dbContext);
                txtStatus.Text = $"数据库初始化成功，路径: {dbPath}";
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"数据库初始化失败: {ex.Message}";
            }
        }

        private void InitializeTabs()
        {
            tabControl.SelectionChanged += (s, e) =>
            {
                if (tabControl.SelectedIndex == 0)
                {
                    gridAxis.Visibility = Visibility.Visible;
                    gridIO.Visibility = Visibility.Collapsed;
                    LoadAxisList();
                }
                else
                {
                    gridAxis.Visibility = Visibility.Collapsed;
                    gridIO.Visibility = Visibility.Visible;
                    LoadIOList();
                }
            };
        }

        private void InitializeAxes()
        {
            try
            {
                txtStatus.Text = "开始初始化轴参数...";
                // 检查是否已有轴参数
                var existingAxes = _axisRepo.GetAll();
                txtStatus.Text = $"现有轴数量: {existingAxes.Count}";
                if (existingAxes.Count == 0)
                {
                    txtStatus.Text = "添加8个轴参数...";
                    // 添加8个轴
                    for (int i = 0; i < _axisNames.Length; i++)
                    {
                        var axis = new AxisParameter
                        {
                            轴名称 = _axisNames[i],
                            轴号 = i,
                            卡号 = 0,
                            轴类型 = 0,
                            脉冲当量 = 1000,
                            脉冲当量分母 = 1,
                            运动低速 = 100,
                            运动高速 = 1000,
                            加速度 = 500,
                            减速度 = 500,
                            加加速度 = 100,
                            减减速度 = 100,
                            回原模式 = 0,
                            回原方向 = 0,
                            原点高速 = 500,
                            原点低速 = 100,
                            原点加速度 = 200,
                            原点减速度 = 200,
                            原点偏移 = 0,
                            正向软极限 = 10000,
                            负向软极限 = -10000,
                            使能IO = i
                        };
                        _axisRepo.Insert(axis);
                        txtStatus.Text = $"添加轴: {_axisNames[i]}";
                    }
                    txtStatus.Text = "轴参数初始化成功";
                }
                LoadAxisList();
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"轴参数初始化失败: {ex.Message}\n{ex.StackTrace}";
            }
        }

        private void InitializeIOs()
        {
            try
            {
                txtStatus.Text = "开始初始化IO参数...";
                // 检查是否已有IO参数
                var existingIOs = _ioRepo.GetAll();
                txtStatus.Text = $"现有IO数量: {existingIOs.Count}";
                if (existingIOs.Count == 0)
                {
                    txtStatus.Text = "添加16x16 IO参数...";
                    // 添加16x16 IO
                    var random = new Random();
                    int count = 0;
                    for (int port = 0; port < 16; port++)
                    {
                        for (int point = 0; point < 16; point++)
                        {
                            var io = new IOParameter
                            {
                                卡号 = 0,
                                端口号 = port,
                                输入点 = point,
                                输入名称 = $"{_ioNamePrefixes[random.Next(_ioNamePrefixes.Length)]}输入{port}-{point}",
                                输出点 = point,
                                输出名称 = $"{_ioNamePrefixes[random.Next(_ioNamePrefixes.Length)]}输出{port}-{point}"
                            };
                            _ioRepo.Insert(io);
                            count++;
                            if (count % 10 == 0)
                            {
                                txtStatus.Text = $"已添加 {count} 个IO参数";
                            }
                        }
                    }
                    txtStatus.Text = "IO参数初始化成功";
                }
                LoadIOList();
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"IO参数初始化失败: {ex.Message}\n{ex.StackTrace}";
            }
        }

        private void LoadAxisList()
        {
            try
            {
                var axes = _axisRepo.GetAll();
                cmbAxis.Items.Clear();
                foreach (var axis in axes)
                {
                    cmbAxis.Items.Add($"{axis.轴名称} (轴号: {axis.轴号})");
                }
                if (axes.Count > 0)
                {
                    cmbAxis.SelectedIndex = 0;
                    LoadAxisParameters(axes[0]);
                }
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"加载轴列表失败: {ex.Message}";
            }
        }

        private void LoadIOList()
        {
            try
            {
                var ios = _ioRepo.GetAll();
                cmbIO.Items.Clear();
                foreach (var io in ios)
                {
                    cmbIO.Items.Add($"端口{io.端口号} (输入: {io.输入点}, 输出: {io.输出点})");
                }
                if (ios.Count > 0)
                {
                    cmbIO.SelectedIndex = 0;
                    LoadIOParameters(ios[0]);
                }
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"加载IO列表失败: {ex.Message}";
            }
        }

        private void LoadAxisParameters(AxisParameter axis)
        {
            _currentAxis = axis;

            // 加载到DataGrid
            var properties = axis.GetType().GetProperties()
                .Where(p => p.Name != "Id" && p.Name != "CreatedAt" && p.Name != "UpdatedAt")
                .Select(p => new
                {
                    Path = p.Name,
                    Value = p.GetValue(axis)?.ToString() ?? ""
                }).ToList();

            dgAxisParams.ItemsSource = properties;

            // 加载到详细编辑面板
            stackAxisParams.Children.Clear();
            foreach (var prop in axis.GetType().GetProperties())
            {
                if (prop.Name == "Id" || prop.Name == "CreatedAt" || prop.Name == "UpdatedAt")
                    continue;

                var stackPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };
                var label = new Label { Content = prop.Name, Width = 150 };
                var textBox = new TextBox { Width = 200, Text = prop.GetValue(axis)?.ToString() ?? "" };
                textBox.Tag = prop.Name;
                stackPanel.Children.Add(label);
                stackPanel.Children.Add(textBox);
                stackAxisParams.Children.Add(stackPanel);
            }
        }

        private void LoadIOParameters(IOParameter io)
        {
            _currentIO = io;

            // 加载到DataGrid
            var properties = io.GetType().GetProperties()
                .Where(p => p.Name != "Id" && p.Name != "CreatedAt" && p.Name != "UpdatedAt")
                .Select(p => new
                {
                    Path = p.Name,
                    Value = p.GetValue(io)?.ToString() ?? ""
                }).ToList();

            dgIOParams.ItemsSource = properties;

            // 加载到详细编辑面板
            stackIOParams.Children.Clear();
            foreach (var prop in io.GetType().GetProperties())
            {
                if (prop.Name == "Id" || prop.Name == "CreatedAt" || prop.Name == "UpdatedAt")
                    continue;

                var stackPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };
                var label = new Label { Content = prop.Name, Width = 150 };
                var textBox = new TextBox { Width = 200, Text = prop.GetValue(io)?.ToString() ?? "" };
                textBox.Tag = prop.Name;
                stackPanel.Children.Add(label);
                stackPanel.Children.Add(textBox);
                stackIOParams.Children.Add(stackPanel);
            }
        }

        private void btnAddAxis_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newAxis = new AxisParameter
                {
                    轴名称 = "新轴",
                    轴号 = _axisRepo.GetAll().Count,
                    卡号 = 0,
                    轴类型 = 0,
                    脉冲当量 = 1000,
                    脉冲当量分母 = 1,
                    运动低速 = 100,
                    运动高速 = 1000,
                    加速度 = 500,
                    减速度 = 500,
                    加加速度 = 100,
                    减减速度 = 100,
                    回原模式 = 0,
                    回原方向 = 0,
                    原点高速 = 500,
                    原点低速 = 100,
                    原点加速度 = 200,
                    原点减速度 = 200,
                    原点偏移 = 0,
                    正向软极限 = 10000,
                    负向软极限 = -10000,
                    使能IO = 0
                };
                _axisRepo.Insert(newAxis);
                LoadAxisList();
                txtStatus.Text = "轴添加成功";
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"添加轴失败: {ex.Message}";
            }
        }

        private void btnDeleteAxis_Click(object sender, RoutedEventArgs e)
        {
            if (_currentAxis == null)
            {
                txtStatus.Text = "请选择要删除的轴";
                return;
            }

            try
            {
                _axisRepo.Delete(_currentAxis.Id);
                LoadAxisList();
                txtStatus.Text = "轴删除成功";
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"删除轴失败: {ex.Message}";
            }
        }

        private void btnSaveAxis_Click(object sender, RoutedEventArgs e)
        {
            if (_currentAxis == null)
            {
                txtStatus.Text = "请选择要保存的轴";
                return;
            }

            try
            {
                foreach (var child in stackAxisParams.Children)
                {
                    if (child is StackPanel stackPanel)
                    {
                        var textBox = stackPanel.Children[1] as TextBox;
                        if (textBox != null && textBox.Tag is string propertyName)
                        {
                            var property = _currentAxis.GetType().GetProperty(propertyName);
                            if (property != null)
                            {
                                var value = textBox.Text;
                                if (property.PropertyType == typeof(int))
                                {
                                    if (int.TryParse(value, out int intValue))
                                        property.SetValue(_currentAxis, intValue);
                                }
                                else if (property.PropertyType == typeof(double))
                                {
                                    if (double.TryParse(value, out double doubleValue))
                                        property.SetValue(_currentAxis, doubleValue);
                                }
                                else if (property.PropertyType == typeof(string))
                                {
                                    property.SetValue(_currentAxis, value);
                                }
                            }
                        }
                    }
                }

                _axisRepo.Update(_currentAxis);
                // 保存后重新加载轴列表，确保下拉框中的选项更新
                LoadAxisList();
                txtStatus.Text = "轴参数保存成功";
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"保存轴参数失败: {ex.Message}";
            }
        }

        private void btnAddIO_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var existingIOs = _ioRepo.GetAll();
                var newIO = new IOParameter
                {
                    卡号 = 0,
                    端口号 = existingIOs.Count / 16,
                    输入点 = existingIOs.Count % 16,
                    输入名称 = "新输入",
                    输出点 = existingIOs.Count % 16,
                    输出名称 = "新输出"
                };
                _ioRepo.Insert(newIO);
                LoadIOList();
                txtStatus.Text = "IO添加成功";
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"添加IO失败: {ex.Message}";
            }
        }

        private void btnDeleteIO_Click(object sender, RoutedEventArgs e)
        {
            if (_currentIO == null)
            {
                txtStatus.Text = "请选择要删除的IO";
                return;
            }

            try
            {
                _ioRepo.Delete(_currentIO.Id);
                LoadIOList();
                txtStatus.Text = "IO删除成功";
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"删除IO失败: {ex.Message}";
            }
        }

        private void btnSaveIO_Click(object sender, RoutedEventArgs e)
        {
            if (_currentIO == null)
            {
                txtStatus.Text = "请选择要保存的IO";
                return;
            }

            try
            {
                foreach (var child in stackIOParams.Children)
                {
                    if (child is StackPanel stackPanel)
                    {
                        var textBox = stackPanel.Children[1] as TextBox;
                        if (textBox != null && textBox.Tag is string propertyName)
                        {
                            var property = _currentIO.GetType().GetProperty(propertyName);
                            if (property != null)
                            {
                                var value = textBox.Text;
                                if (property.PropertyType == typeof(int))
                                {
                                    if (int.TryParse(value, out int intValue))
                                        property.SetValue(_currentIO, intValue);
                                }
                                else if (property.PropertyType == typeof(string))
                                {
                                    property.SetValue(_currentIO, value);
                                }
                            }
                        }
                    }
                }

                _ioRepo.Update(_currentIO);
                // 保存后重新加载IO列表，确保下拉框中的选项更新
                LoadIOList();
                txtStatus.Text = "IO参数保存成功";
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"保存IO参数失败: {ex.Message}";
            }
        }

        private void cmbAxis_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbAxis.SelectedIndex >= 0)
            {
                var axes = _axisRepo.GetAll();
                if (axes.Count > cmbAxis.SelectedIndex)
                {
                    LoadAxisParameters(axes[cmbAxis.SelectedIndex]);
                }
            }
        }

        private void cmbIO_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbIO.SelectedIndex >= 0)
            {
                var ios = _ioRepo.GetAll();
                if (ios.Count > cmbIO.SelectedIndex)
                {
                    LoadIOParameters(ios[cmbIO.SelectedIndex]);
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _dbContext?.Dispose();
            base.OnClosing(e);
        }
    }
}
