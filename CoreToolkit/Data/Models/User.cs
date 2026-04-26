namespace CoreToolkit.Data
{
    /// <summary>
    /// 示例实体：用户
    /// </summary>
    public class User : EntityBase
    {
        private string _userName;
        private string _email;
        private int _age;
        private bool _isActive = true;
        private string _phone;
        private string _address;

        [Field("用户名", "基本信息", ControlType.String)]
        public string UserName
        {
            get { return _userName; }
            set { SetProperty(ref _userName, value); }
        }

        [Field("邮箱", "联系信息", ControlType.String)]
        public string Email
        {
            get { return _email; }
            set { SetProperty(ref _email, value); }
        }

        [Field("年龄", "基本信息", ControlType.Numeric)]
        public int Age
        {
            get { return _age; }
            set { SetProperty(ref _age, value); }
        }

        [Field("是否活跃", "状态信息", ControlType.Bool)]
        public bool IsActive
        {
            get { return _isActive; }
            set { SetProperty(ref _isActive, value); }
        }

        [Field("电话", "联系信息", ControlType.String)]
        public string Phone
        {
            get { return _phone; }
            set { SetProperty(ref _phone, value); }
        }

        [Field("地址", "联系信息", ControlType.String)]
        public string Address
        {
            get { return _address; }
            set { SetProperty(ref _address, value); }
        }
    }
}
