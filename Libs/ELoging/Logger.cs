using ELogging.Model;
using ESystem.Asserting;
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
    public class Logger
  {
    private record NameInfo(string Name, bool AddId);

    #region Private Classes

    private class ActionInfo
    {
      #region Private Fields

      private static int nextId = 1;

      #endregion Private Fields

      #region Public Properties

      public Action<LogItem> Action { get; private set; }

      public int Id { get; private set; }

      public object? Owner { get; private set; }

      public List<LogRule> Rules { get; private set; }

      #endregion Public Properties

      #region Public Constructors

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

      #endregion Public Constructors

      #region Public Methods

      public LogRule? TryGetFirstRule(string senderName)
      {
        LogRule? ret = this.Rules.FirstOrDefault(q => q.IsPatternMatch(senderName));
        return ret;
      }

      #endregion Public Methods
    }

    private class ObjectIdManager
    {
      #region Private Fields

      private readonly Dictionary<object, int> inner = new();
      private int nextId = 1;

      #endregion Private Fields

      #region Public Indexers

      public int this[object obj]
      {
        get
        {
          if (inner.ContainsKey(obj) == false)
          {
            inner[obj] = nextId++;
          }
          return inner[obj];
        }
      }

      internal void TryRemove(object sender)
      {
        if (inner.ContainsKey(sender))
          inner.Remove(sender);
      }

      #endregion Public Indexers
    }

    #endregion Private Classes

    #region Private Fields

    private static readonly List<ActionInfo> actions = new();
    private static readonly ObjectIdManager senderIds = new();
    private static readonly Dictionary<object, NameInfo> senderNames = new();
    #endregion Private Fields

    #region Public Properties

    public static bool UseFullTypeNames { get; set; } = true;

    #endregion Public Properties

    #region Public Methods

    private readonly object sender;

    private Logger(object sender)
    {
      EAssert.Argument.IsNotNull(sender, nameof(sender));
      this.sender = sender;
    }

    public static Logger Create(object sender)
    {
      Logger ret = new(sender);
      return ret;
    }

    public static Logger CreateChild(object sender, string customSenderName, object parent, char nameSeparator = '.', bool addObjectId = false)
    {
      RegisterChildSenderName(sender, customSenderName, parent, nameSeparator, addObjectId);
      var ret = Create(sender);
      return ret;
    }

    public static Logger Create(object sender, string customSenderName, bool addObjectId = false)
    {
      RegisterSenderName(sender, customSenderName, addObjectId);
      var ret = Create(sender);
      return ret;
    }
    public static string GetSenderName(object sender)
    {
      string ret = ResolveSenderName(sender);
      return ret;
    }

    public static void Log(object sender, LogLevel level, string message)
    {
      ProcessMessage(level, sender, message);
    }

    public static void RegisterChildSenderName(object sender, string customSenderName, object parent, char nameSeparator = '.', bool addObjectId = false)
    {
      EAssert.Argument.IsNotNull(customSenderName, nameof(customSenderName));
      EAssert.Argument.IsNotNull(parent, nameof(parent));
      string parentName = ResolveSenderName(parent);
      RegisterSenderName(sender, $"{parentName}{nameSeparator}{customSenderName}", addObjectId);
    }

    public static int RegisterLogAction(Action<LogItem> action, List<LogRule> rules, object? owner = null)
    {
      ActionInfo ai = new(action, rules, owner);
      actions.Add(ai);
      return ai.Id;
    }

    public static void RegisterSenderName<T>(string customSenderName, bool addObjectId = false)
    {
      EAssert.Argument.IsNonEmptyString(customSenderName, nameof(customSenderName));
      senderNames[typeof(T)] = new(customSenderName, addObjectId);
    }

    public static void RegisterSenderName(object sender, string customSenderName, bool addObjectId = false)
    {
      EAssert.Argument.IsNonEmptyString(customSenderName, nameof(customSenderName));
      senderNames[sender] = new(customSenderName, addObjectId);
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

    public static void UnregisterSenderName(object sender)
    {
      if (senderNames.ContainsKey(sender))
        senderNames.Remove(sender);
    }

    public static void UnregisterSenderType<T>()
    {
      if (senderNames.ContainsKey(typeof(T)))
        senderNames.Remove(typeof(T));
    }

    public void Invoke(LogLevel level, string message)
    {
      this.Log(level, message);
    }
    public void Log(LogLevel level, string message)
    {
      Logger.ProcessMessage(level, this.sender, message);
    }
    #endregion Public Methods

    #region Private Methods

    private static void ProcessMessage(LogLevel level, object sender, string message)
    {
      EAssert.Argument.IsNotNull(sender, nameof(sender));
      EAssert.Argument.IsNonEmptyString(message, nameof(message));

      string senderName = ResolveSenderName(sender);

      foreach (var actionInfo in actions)
      {
        LogRule? rule = actionInfo.TryGetFirstRule(senderName);
        if (rule == null) continue;
        if (rule.IsLogLevelMatch(level) == false) continue;

        LogItem item = new(sender, senderName, level, message);
        actionInfo.Action.Invoke(item);
      }
    }

    private static string ResolveSenderName(object sender)
    {
      NameInfo ni =
        (senderNames.ContainsKey(sender)) ? senderNames[sender] :
        (senderNames.ContainsKey(sender.GetType())) ? senderNames[sender.GetType()] :
        new NameInfo(UseFullTypeNames
          ? sender.GetType().FullName ?? sender.GetType().Name
          : sender.GetType().Name,
          true);

      StringBuilder ret = new(ni.Name);
      if (ni.AddId)
        ret.Append(" {{").Append(senderIds[sender]).Append("}}");

      return ret.ToString();
    }

    public static void UnregisterSender(object sender)
    {
      senderIds.TryRemove(sender);
      if (senderNames.ContainsKey(sender))
        senderNames.Remove(sender);
    }

    #endregion Private Methods
  }
}
