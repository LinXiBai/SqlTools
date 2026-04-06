using System;
using System.IO;
using System.Threading;
using CoreToolkit.Common.Helpers;
using CoreToolkit.Files.Models;

namespace CoreToolkit.Files.Helpers
{
    /// <summary>
    /// 配方文件管理器（基于 JSON）
    /// </summary>
    public class RecipeManager
    {
        private readonly string _recipeDirectory;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public RecipeManager(string recipeDirectory)
        {
            _recipeDirectory = recipeDirectory;
            if (!Directory.Exists(_recipeDirectory))
                Directory.CreateDirectory(_recipeDirectory);
        }

        private string ValidateRecipeName(string recipeName)
        {
            if (string.IsNullOrWhiteSpace(recipeName))
                throw new ArgumentException("配方名称不能为空", nameof(recipeName));

            // 检查非法字符
            char[] invalidChars = Path.GetInvalidFileNameChars();
            foreach (char c in invalidChars)
            {
                if (recipeName.Contains(c.ToString()))
                    throw new ArgumentException($"配方名称包含非法字符: {c}", nameof(recipeName));
            }

            // 只使用文件名，防止路径注入
            return Path.GetFileName(recipeName);
        }

        public string GetRecipePath(string recipeName)
        {
            string safeName = ValidateRecipeName(recipeName);
            return Path.Combine(_recipeDirectory, $"{safeName}.json");
        }

        public void Save(Recipe recipe)
        {
            if (recipe == null) throw new ArgumentNullException(nameof(recipe));

            _lock.EnterWriteLock();
            try
            {
                recipe.UpdatedAt = DateTime.Now;
                string json = JsonHelper.Serialize(recipe);
                File.WriteAllText(GetRecipePath(recipe.RecipeName), json);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public Recipe Load(string recipeName)
        {
            _lock.EnterReadLock();
            try
            {
                string path = GetRecipePath(recipeName);
                if (!File.Exists(path)) return null;
                string json = File.ReadAllText(path);
                return JsonHelper.Deserialize<Recipe>(json);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public string[] GetRecipeNames()
        {
            _lock.EnterReadLock();
            try
            {
                var files = Directory.GetFiles(_recipeDirectory, "*.json");
                for (int i = 0; i < files.Length; i++)
                    files[i] = Path.GetFileNameWithoutExtension(files[i]);
                return files;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Delete(string recipeName)
        {
            _lock.EnterWriteLock();
            try
            {
                string path = GetRecipePath(recipeName);
                if (File.Exists(path))
                    File.Delete(path);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool Exists(string recipeName)
        {
            _lock.EnterReadLock();
            try
            {
                return File.Exists(GetRecipePath(recipeName));
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }
}
