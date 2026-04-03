using System.Collections.Generic;
using System.Linq;

namespace SqlDemo.Data
{
    /// <summary>
    /// IO参数仓库
    /// </summary>
    public class IOParameterRepository : RepositoryBase<IOParameter>
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="db">数据库上下文</param>
        public IOParameterRepository(SqliteDbContext db) : base(db)
        {
        }

        /// <summary>
        /// 根据卡号获取IO参数列表
        /// </summary>
        /// <param name="cardId">卡号</param>
        /// <returns>IO参数列表</returns>
        public List<IOParameter> GetByCardId(int cardId)
        {
            string sql = "SELECT * FROM IOParameters WHERE 卡号 = @CardId";
            return _db.Query<IOParameter>(sql, new { CardId = cardId }).ToList();
        }

        /// <summary>
        /// 根据卡号和端口号获取IO参数
        /// </summary>
        /// <param name="cardId">卡号</param>
        /// <param name="portId">端口号</param>
        /// <returns>IO参数</returns>
        public IOParameter GetByCardAndPort(int cardId, int portId)
        {
            string sql = "SELECT * FROM IOParameters WHERE 卡号 = @CardId AND 端口号 = @PortId";
            return _db.QuerySingleOrDefault<IOParameter>(sql, new { CardId = cardId, PortId = portId });
        }

        /// <summary>
        /// 根据输入点获取IO参数
        /// </summary>
        /// <param name="cardId">卡号</param>
        /// <param name="inputPoint">输入点</param>
        /// <returns>IO参数</returns>
        public IOParameter GetByInputPoint(int cardId, int inputPoint)
        {
            string sql = "SELECT * FROM IOParameters WHERE 卡号 = @CardId AND 输入点 = @InputPoint";
            return _db.QuerySingleOrDefault<IOParameter>(sql, new { CardId = cardId, InputPoint = inputPoint });
        }

        /// <summary>
        /// 根据输出点获取IO参数
        /// </summary>
        /// <param name="cardId">卡号</param>
        /// <param name="outputPoint">输出点</param>
        /// <returns>IO参数</returns>
        public IOParameter GetByOutputPoint(int cardId, int outputPoint)
        {
            string sql = "SELECT * FROM IOParameters WHERE 卡号 = @CardId AND 输出点 = @OutputPoint";
            return _db.QuerySingleOrDefault<IOParameter>(sql, new { CardId = cardId, OutputPoint = outputPoint });
        }

        /// <summary>
        /// 获取所有IO参数
        /// </summary>
        /// <returns>IO参数列表</returns>
        public new List<IOParameter> GetAll()
        {
            string sql = "SELECT * FROM IOParameters ORDER BY 卡号, 端口号";
            return _db.Query<IOParameter>(sql).ToList();
        }
    }
}
