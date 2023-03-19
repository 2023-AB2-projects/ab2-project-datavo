﻿using Server.Parser.Actions;
using Server.Parser.Commands;
using Server.Parser.DDL;
using System.Text.RegularExpressions;
using Server.Parser.Utils;
using Server.Server.Requests.Controllers.Parser;

namespace Server.Parser.Utils
{
    internal class RequestMapper
    {
        private static readonly Dictionary<string, Type> _commands = new()
        {
            { Patterns.CreateDatabase, typeof(CreateDatabase) },
            { Patterns.DropDatabase, typeof(DropDatabase) },
            { Patterns.CreateTable, typeof(CreateTable) },
            { Patterns.DropTable, typeof(DropTable) },
        };
        private static readonly KeyValuePair<string, Type> _goCommand = new(Patterns.Go, typeof(Go));

        public static List<Queue<IDbAction>> ToRunnables(ParseRequest request)
        {
            List<Queue<IDbAction>> runnables = new();
            Queue<IDbAction> actions = new();

            var rawSqlCode = request.Data.Replace(";", "");
            int lineCount = 0;

        REPEAT:
            while (!string.IsNullOrEmpty(rawSqlCode.Trim()))
            {
                if (MatchCommand(_goCommand, ref rawSqlCode, ref lineCount) != null)
                {
                    runnables.Add(actions);
                    actions.Clear();
                    continue;
                }

                foreach (KeyValuePair<string, Type> command in _commands)
                {
                    var action = MatchCommand(command, ref rawSqlCode, ref lineCount);

                    if (action != null)
                    {
                        actions.Enqueue(action);
                        goto REPEAT;
                    }
                }

                throw new Exception($"Invalid Keyword: {FirstKeyWord(rawSqlCode)} at line: {lineCount}!");
            }

            if (actions.Count != 0)
            {
                runnables.Add(actions);
            }

            return runnables;
        }

        private static IDbAction? MatchCommand(KeyValuePair<string, Type> command, ref string rawSqlCode, ref int lineCount)
        {
            Match match = Regex.Match(rawSqlCode, command.Key, RegexOptions.IgnoreCase | RegexOptions.Multiline);

            if (match.Success)
            {
                lineCount += Regex.Split(match.Value, "\r\n|\r|\n").Length;
                rawSqlCode = rawSqlCode.Substring(match.Index + match.Length);
                return (IDbAction)Activator.CreateInstance(command.Value, match);
            }

            return null;
        }

        private static string FirstKeyWord(string rawSqlCode)
        {
            return rawSqlCode.Trim().Split(" |\t").FirstOrDefault() ?? string.Empty;
        }
    }
}
