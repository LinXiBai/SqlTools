using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CoreToolkit.Data
{
    /// <summary>
    /// 轴参数仓库
    /// </summary>
    public class AxisParameterRepository : RepositoryBase<AxisParameter>
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="db">数据库上下文</param>
        public AxisParameterRepository(SqliteDbContext db) : base(db)
        {
        }

        /// <summary>
        /// 根据卡号获取轴参数列表
        /// </summary>
        /// <param name="cardId">卡号</param>
        /// <returns>轴参数列表</returns>
        public List<AxisParameter> GetByCardId(int cardId)
        {
            string sql = "SELECT * FROM AxisParameters WHERE 卡号 = @CardId";
            return _db.Query<AxisParameter>(sql, new { CardId = cardId }).ToList();
        }

        /// <summary>
        /// 根据卡号和轴号获取轴参数
        /// </summary>
        /// <param name="cardId">卡号</param>
        /// <param name="axisId">轴号</param>
        /// <returns>轴参数</returns>
        public AxisParameter GetByCardAndAxis(int cardId, int axisId)
        {
            string sql = "SELECT * FROM AxisParameters WHERE 卡号 = @CardId AND 轴号 = @AxisId";
            return _db.QuerySingleOrDefault<AxisParameter>(sql, new { CardId = cardId, AxisId = axisId });
        }

        /// <summary>
        /// 根据轴名称获取轴参数
        /// </summary>
        /// <param name="axisName">轴名称</param>
        /// <returns>轴参数</returns>
        public AxisParameter GetByAxisName(string axisName)
        {
            string sql = "SELECT * FROM AxisParameters WHERE 轴名称 = @AxisName";
            return _db.QuerySingleOrDefault<AxisParameter>(sql, new { AxisName = axisName });
        }

        /// <summary>
        /// 获取所有轴参数
        /// </summary>
        /// <returns>轴参数列表</returns>
        public new List<AxisParameter> GetAll()
        {
            string sql = "SELECT * FROM AxisParameters ORDER BY 卡号, 轴号";
            return _db.Query<AxisParameter>(sql).ToList();
        }
    }
}
