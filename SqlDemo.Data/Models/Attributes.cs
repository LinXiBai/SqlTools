using System;

namespace SqlDemo.Data
{
    /// <summary>
    /// 控件类型枚举
    /// </summary>
    public enum ControlType
    {
        None,
        String,
        Numeric,
        Bool,
        ComboBox
    }

    /// <summary>
    /// 字段属性特性：用于定义字段的显示名称、分类、控件类型等
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class FieldAttribute : Attribute
    {
        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 分类名称
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// 控件类型
        /// </summary>
        public ControlType ControlType { get; set; } = ControlType.None;

        /// <summary>
        /// 是否隐藏
        /// </summary>
        public bool IsHidden { get; set; } = false;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="displayName">显示名称</param>
        /// <param name="category">分类名称</param>
        /// <param name="controlType">控件类型</param>
        /// <param name="isHidden">是否隐藏</param>
        public FieldAttribute(string displayName, string category = "基本信息", ControlType controlType = ControlType.None, bool isHidden = false)
        {
            DisplayName = displayName;
            Category = category;
            ControlType = controlType;
            IsHidden = isHidden;
        }
    }
}
