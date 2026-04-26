using System;
using System.ComponentModel;

namespace CoreToolkit.Data
{
    /// <summary>
    /// 所有实体的抽象基类
    /// </summary>
    public abstract class EntityBase : INotifyPropertyChanged
    {
        private long _id;
        private DateTime _createdAt = DateTime.Now;
        private DateTime _updatedAt = DateTime.Now;

        /// <summary>
        /// 主键，默认自增
        /// </summary>
        [Field("ID", "系统信息", ControlType.Numeric, true)]
        public long Id
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Field("创建时间", "系统信息", ControlType.None, true)]
        public DateTime CreatedAt
        {
            get { return _createdAt; }
            set { SetProperty(ref _createdAt, value); }
        }

        /// <summary>
        /// 更新时间
        /// </summary>
        [Field("更新时间", "系统信息", ControlType.None, true)]
        public DateTime UpdatedAt
        {
            get { return _updatedAt; }
            set { SetProperty(ref _updatedAt, value); }
        }

        #region INotifyPropertyChanged 实现

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value))
                return false;

            T oldValue = field;
            field = value;
            OnPropertyChanged(propertyName);
            
            // 输出属性变化词条
            OutputPropertyChangeEntry(propertyName, oldValue, value);
            
            return true;
        }

        /// <summary>
        /// 输出属性变化词条
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="oldValue">旧值</param>
        /// <param name="newValue">新值</param>
        protected virtual void OutputPropertyChangeEntry(string propertyName, object oldValue, object newValue)
        {
            string entityType = GetType().Name;
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string oldValueStr = oldValue?.ToString() ?? "null";
            string newValueStr = newValue?.ToString() ?? "null";
            
            string entry = $"[{timestamp}] [{entityType}] 属性 '{propertyName}' 变化: {oldValueStr} → {newValueStr}";
            Console.WriteLine(entry);
        }

        #endregion
    }
}
