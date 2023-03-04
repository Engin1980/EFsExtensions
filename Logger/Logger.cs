using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ELogging
{
  public static class Logger
  {
    private class ActionInfo
    {
      private static int nextId = 1;
      public ActionInfo(Action<LogItem> action, List<LogRule> rules, object? owner)
      {
        Rules = rules;
        Action = action;
        Owner = owner;
        lock (typeof(ActionInfo))
        {
          this.Id = nextId++;
        }
      }

      public object? Owner { get; private set; }
      public List<LogRule> Rules { get; private set; }
      public Action<LogItem> Action { get; private set; }
      public int Id { get; private set; }

      public LogRule? TryGetFirstRule(string senderName)
      {
        LogRule? ret = this.Rules.FirstOrDefault(q => q.IsPatternMatch(senderName));
        return ret;
      }
    }

    private static readonly Dictionary<object, string> customSenderNames = new();
    private static readonly Dictionary<object, NewLogHandler> senders = new();
    private static readonly List<ActionInfo> actions = new();

    public static NewLogHandler RegisterSender(object sender, string? customSenderName = null)
    {
      if (sender == null) throw new ArgumentNullException(nameof(sender));
      if (customSenderName != null)
        customSenderNames[sender] = customSenderName;
      void handler(LogLevel e, string m) => ProcessMessage(e, sender, m);
      senders[sender] = handler;
      return handler;
    }

    public static void UnregisterSender(object sender)
    {
      if (senders.ContainsKey(sender))
        senders.Remove(sender);
      if (customSenderNames.ContainsKey(sender))
        customSenderNames.Remove(sender);
    }

    public static int RegisterLogAction(Action<LogItem> action, List<LogRule> rules, object? owner = null)
    {
      ActionInfo ai = new ActionInfo(action, rules, owner);
      actions.Add(ai);
      return ai.Id;
    }

    public static void UnregisterLogAction(object owner)
    {
      actions
        .Where(q => owner.Equals(q.Owner))
        .ToList()
        .ForEach(q => actions.Remove(q));
    }

    public static void UnregisterLogAction(int id)
    {
      actions
        .Where(q => q.Id == id)
        .ToList()
        .ForEach(q => actions.Remove(q));
    }

    public static void Log(object sender, LogLevel level, string message)
    {
      ProcessMessage(level, sender, message);
    }

    private static void ProcessMessage(LogLevel level, object sender, string message)
    {
      if (sender == null) throw new ArgumentNullException(nameof(sender));
      string senderName = ResolveSenderName(sender);
      foreach (var actionInfo in actions)
      {
        LogRule? rule = actionInfo.TryGetFirstRule(senderName);
        if (rule == null) continue;
        if (rule.IsLogLevelMatch(level) == false) continue;

        LogItem item = new LogItem(sender, senderName, level, message);
        actionInfo.Action.Invoke(item);
      }
    }

    private static string ResolveSenderName(object sender)
    {
      string ret = customSenderNames.ContainsKey(sender)
        ? customSenderNames[sender]
        : sender.ToString() ?? sender.GetType().Name;
      if (sender is LogIdAble lia)
        ret += " {{" + lia.LogId + "}}";
      return ret;
    }
  }
}
