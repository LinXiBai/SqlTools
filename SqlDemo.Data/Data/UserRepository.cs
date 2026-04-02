using System.Collections.Generic;

namespace SqlDemo.Data
{
    /// <summary>
    /// 用户仓储：针对用户业务的扩展查询方法
    /// </summary>
    public class UserRepository : RepositoryBase<User>
    {
        public UserRepository(SqliteDbContext db) : base(db) { }

        /// <summary>
        /// 按用户名模糊查询
        /// </summary>
        public IEnumerable<User> SearchByName(string keyword)
        {
            string whereClause = "UserName LIKE @Keyword";
            return Query(whereClause, new { Keyword = $"%{keyword}%" });
        }

        /// <summary>
        /// 获取所有活跃用户
        /// </summary>
        public IEnumerable<User> GetActiveUsers()
        {
            string whereClause = "IsActive = 1";
            return Query(whereClause);
        }
    }
}
