using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace dotRant
{
    partial class IrcConnection
    {
        #region Setup
        static readonly Dictionary<string, MethodInfo> _handlerInfos = new Dictionary<string, MethodInfo>();
        static readonly Dictionary<string, Func<IrcConnection, string, string, string[], Task>> _handlers = new Dictionary<string, Func<IrcConnection, string, string, string[], Task>>();
        static readonly MethodInfo _defaultHandler;

        static IrcConnection()
        {
            PopulateHandlerInfos();
            _defaultHandler = _handlerInfos["$DEFAULT$"];
        }

        static Task HandleCommand(IrcConnection conn, string prefix, string command, string[] args)
        {
            Func<IrcConnection, string, string, string[], Task> handler;
            if (!_handlers.TryGetValue(command, out handler))
            {
                // handler not yet registered
                MethodInfo mi;
                if (!_handlerInfos.TryGetValue(command, out mi))
                    mi = _defaultHandler;

                ParameterExpression pInstance = Expression.Parameter(typeof(IrcConnection), "instance"),
                    pPrefix = Expression.Parameter(typeof(string), "prefix"),
                    pCmd = Expression.Parameter(typeof(string), "cmd"),
                    pArgs = Expression.Parameter(typeof(string[]), "args");
                ParameterExpression[] all = new[] { pInstance, pPrefix, pCmd, pArgs };

                var lambda = Expression.Lambda<Func<IrcConnection, string, string, string[], Task>>(
                    Expression.Call(
                        pInstance,
                        mi,
                        pPrefix,
                        pCmd,
                        pArgs
                    ),
                    all
                );

                handler = _handlers[command] = lambda.Compile();
            }
            return handler(conn, prefix, command, args);
        }

        static void PopulateHandlerInfos()
        {
            _handlerInfos.Clear();
            foreach (var mi in typeof(IrcConnection).GetRuntimeMethods())
            {
                var attr = mi.GetCustomAttribute<IrcCommandAttribute>();
                if (attr != null)
                {
                    _handlerInfos.Add(attr.Command, mi);
                }
            }
        }
        #endregion

        [IrcCommand("$DEFAULT$")]
        async Task DefaultHandler(string prefix, string command, string[] args)
        {
            // Unknown command. Do nothing.
        }

        [IrcCommand("PING")]
        async Task HandlePing(string prefix, string command, string[] args)
        {
            SendCommand("PONG", args[0]);
        }
    }
}
