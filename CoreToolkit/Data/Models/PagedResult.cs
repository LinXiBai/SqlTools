using System.Collections.Generic;

namespace CoreToolkit.Data.Models
{
    /// <summary>
    /// 分页查询结果
    /// </summary>
    /// <typeparam name="T">数据项类型</typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// 数据项列表
        /// </summary>
        public IEnumerable<T> Items { get; set; }

        /// <summary>
        /// 总记录数
        /// </summary>
        public long TotalCount { get; set; }

        /// <summary>
        /// 当前页索引（从0开始）
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 每页大小
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages => PageSize > 0 ? (int)((TotalCount + PageSize - 1) / PageSize) : 0;

        /// <summary>
        /// 是否有上一页
        /// </summary>
        public bool HasPreviousPage => PageIndex > 0;

        /// <summary>
        /// 是否有下一页
        /// </summary>
        public bool HasNextPage => PageIndex + 1 < TotalPages;
    }
}
