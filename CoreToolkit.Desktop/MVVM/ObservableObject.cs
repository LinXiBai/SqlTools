using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CoreToolkit.Desktop.MVVM
{
    /// <summary>
    /// 可观察对象基类，提供属性变更通知（支持变更前/后通知及批量通知）
    /// </summary>
    public abstract class ObservableObject : INotifyPropertyChanged, INotifyPropertyChanging
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;

        /// <summary>
        /// 触发 PropertyChanging 事件（变更前）
        /// </summary>
        protected virtual void OnPropertyChanging([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanging;
            handler?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        }

        /// <summary>
        /// 触发 PropertyChanged 事件（变更后）
        /// </summary>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 批量触发多个属性的 PropertyChanged 事件
        /// </summary>
        protected virtual void OnPropertyChanged(params string[] propertyNames)
        {
            var handler = PropertyChanged;
            if (handler == null) return;

            foreach (var name in propertyNames)
            {
                handler.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        /// <summary>
        /// 通知所有绑定属性已变更（空属性名表示全部）
        /// </summary>
        protected void NotifyAllPropertiesChanged()
        {
            OnPropertyChanged(string.Empty);
        }

        /// <summary>
        /// 设置属性值并自动通知变更（含变更前/后通知）
        /// </summary>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            OnPropertyChanging(propertyName);
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// 设置属性值，变更后执行回调
        /// </summary>
        protected bool SetProperty<T>(ref T field, T value, Action onChanged, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            OnPropertyChanging(propertyName);
            field = value;
            OnPropertyChanged(propertyName);
            onChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// 设置属性值，变更前/后均执行回调
        /// </summary>
        protected bool SetProperty<T>(ref T field, T value, Action onChanging, Action onChanged, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            onChanging?.Invoke();
            OnPropertyChanging(propertyName);
            field = value;
            OnPropertyChanged(propertyName);
            onChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// 设置属性值，使用自定义比较器
        /// </summary>
        protected bool SetProperty<T>(ref T field, T value, IEqualityComparer<T> comparer, [CallerMemberName] string propertyName = null)
        {
            if (comparer?.Equals(field, value) ?? EqualityComparer<T>.Default.Equals(field, value))
                return false;

            OnPropertyChanging(propertyName);
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
