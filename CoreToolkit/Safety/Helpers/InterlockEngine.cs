using CoreToolkit.Safety.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreToolkit.Safety.Helpers
{
    /// <summary>
    /// 互锁规则引擎
    /// 管理设备安全互锁逻辑
    /// </summary>
    public class InterlockEngine
    {
        private readonly List<InterlockRule> _rules = new List<InterlockRule>();
        private readonly object _lock = new object();

        /// <summary>
        /// 规则触发事件
        /// </summary>
        public event EventHandler<InterlockRule> RuleTriggered;

        /// <summary>
        /// 添加互锁规则
        /// </summary>
        public void AddRule(InterlockRule rule)
        {
            if (rule == null) throw new ArgumentNullException(nameof(rule));
            lock (_lock)
            {
                _rules.Add(rule);
            }
        }

        /// <summary>
        /// 移除规则
        /// </summary>
        public bool RemoveRule(string ruleId)
        {
            lock (_lock)
            {
                var rule = _rules.FirstOrDefault(r => r.Id == ruleId);
                if (rule != null)
                {
                    return _rules.Remove(rule);
                }
                return false;
            }
        }

        /// <summary>
        /// 评估所有规则
        /// </summary>
        public InterlockEvaluationResult EvaluateAll()
        {
            lock (_lock)
            {
                var result = new InterlockEvaluationResult { IsSafe = true };

                foreach (var rule in _rules.Where(r => r.Enabled))
                {
                    if (rule.Evaluate())
                    {
                        result.IsSafe = false;
                        result.TriggeredRules.Add(rule);
                        RuleTriggered?.Invoke(this, rule);
                    }
                }

                // 取最高优先级动作
                if (!result.IsSafe)
                {
                    result.RecommendedAction = GetHighestPriorityAction(result.TriggeredRules);
                    result.BlockReason = string.Join("; ", result.TriggeredRules.Select(r => $"[{r.Name}] {r.Message}"));
                }

                return result;
            }
        }

        /// <summary>
        /// 评估特定动作类型的规则（如运动前只评估BlockMotion规则）
        /// </summary>
        public InterlockEvaluationResult EvaluateBeforeMotion()
        {
            lock (_lock)
            {
                var result = new InterlockEvaluationResult { IsSafe = true };
                var relevantActions = new[] { InterlockAction.BlockMotion, InterlockAction.EmergencyStop, InterlockAction.DecelerateStop };

                foreach (var rule in _rules.Where(r => r.Enabled && relevantActions.Contains(r.Action)))
                {
                    if (rule.Evaluate())
                    {
                        result.IsSafe = false;
                        result.TriggeredRules.Add(rule);
                        RuleTriggered?.Invoke(this, rule);
                    }
                }

                if (!result.IsSafe)
                {
                    result.RecommendedAction = GetHighestPriorityAction(result.TriggeredRules);
                    result.BlockReason = string.Join("; ", result.TriggeredRules.Select(r => $"[{r.Name}] {r.Message}"));
                }

                return result;
            }
        }

        /// <summary>
        /// 获取所有规则
        /// </summary>
        public List<InterlockRule> GetAllRules()
        {
            lock (_lock)
            {
                return _rules.ToList();
            }
        }

        /// <summary>
        /// 清除所有规则
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _rules.Clear();
            }
        }

        /// <summary>
        /// 启用规则
        /// </summary>
        public void EnableRule(string ruleId)
        {
            lock (_lock)
            {
                var rule = _rules.FirstOrDefault(r => r.Id == ruleId);
                if (rule != null) rule.Enabled = true;
            }
        }

        /// <summary>
        /// 禁用规则
        /// </summary>
        public void DisableRule(string ruleId)
        {
            lock (_lock)
            {
                var rule = _rules.FirstOrDefault(r => r.Id == ruleId);
                if (rule != null) rule.Enabled = false;
            }
        }

        /// <summary>
        /// 按优先级排序获取最高动作
        /// </summary>
        private InterlockAction GetHighestPriorityAction(List<InterlockRule> rules)
        {
            if (rules.Count == 0) return InterlockAction.AlarmOnly;

            // 优先级：EmergencyStop > DecelerateStop > BlockMotion > AlarmOnly
            if (rules.Any(r => r.Action == InterlockAction.EmergencyStop))
                return InterlockAction.EmergencyStop;
            if (rules.Any(r => r.Action == InterlockAction.DecelerateStop))
                return InterlockAction.DecelerateStop;
            if (rules.Any(r => r.Action == InterlockAction.BlockMotion))
                return InterlockAction.BlockMotion;

            return InterlockAction.AlarmOnly;
        }
    }

    /// <summary>
    /// 互锁评估结果
    /// </summary>
    public class InterlockEvaluationResult
    {
        /// <summary>
        /// 是否安全（无规则触发）
        /// </summary>
        public bool IsSafe { get; set; }

        /// <summary>
        /// 被触发的规则列表
        /// </summary>
        public List<InterlockRule> TriggeredRules { get; set; } = new List<InterlockRule>();

        /// <summary>
        /// 建议执行的动作
        /// </summary>
        public InterlockAction RecommendedAction { get; set; }

        /// <summary>
        /// 阻止原因描述
        /// </summary>
        public string BlockReason { get; set; }
    }
}
