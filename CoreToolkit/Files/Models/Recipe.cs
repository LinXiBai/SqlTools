using System;
using System.Collections.Generic;

namespace CoreToolkit.Files.Models
{
    /// <summary>
    /// 设备配方（Recipe）模型
    /// </summary>
    public class Recipe
    {
        public string RecipeName { get; set; }
        public string ProductCode { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string Version { get; set; } = "1.0";
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }
}
