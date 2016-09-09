﻿using System;
using System.Collections.Generic;
using System.Text;

namespace vtortola.RedisClient.ProcedureDebugger
{
    public sealed class Parameter
    {
        public String Name { get; private set; }
        public String[] Values { get; private set; }
        public Parameter(String name, String[] values)
        {
            Name = name;
            Values = values;
        }
    }

    public sealed class SessionModel
    {
        public String FileName { get; private set; }
        public String Procedure { get; private set; }
        public String CliCommands { get; private set; }
        public Boolean SyncMode { get; private set; }
        public Dictionary<String, String[]> Parameters { get; private set; }

        private SessionModel()
        {
            Parameters = new Dictionary<String, String[]>();
        }

        public static SessionModel Parse(params String[] args)
        {
            var session = new SessionModel();
            var redisCliCommands = new StringBuilder();

            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                switch(arg)
                {
                    case "--file":
                        session.FileName = args[++i];
                        break;

                    case "--procedure":
                        session.Procedure = args[++i];
                        break;

                    case "--eval":
                        throw new InvalidOperationException("--eval command is forbidden, use --file and --procedure");

                    case "-h":
                    case "-p":
                    case "-s":
                    case "-a":
                        redisCliCommands.Append(arg);
                        redisCliCommands.Append(' ');
                        redisCliCommands.Append(args[++i]);
                        redisCliCommands.Append(' ');
                        break;

                    case "--sync-mode":
                        session.SyncMode = true;
                        break;

                    default:
                        if (arg.StartsWith("--@"))
                        {
                            arg = arg.Substring(3, arg.Length - 3);
                            session.Parameters.Add(arg, ParseValues(args[++i]));
                        }
                        else
                        {
                            throw new InvalidOperationException("invalid part: " + arg);
                        }
                        break;
                }

            }
            session.CliCommands = redisCliCommands.ToString();
            Validate(session);
            return session;
        }

        static void Validate(SessionModel session)
        {
            if (String.IsNullOrWhiteSpace(session.FileName))
                throw new InvalidOperationException("--file is mandatory");
            if (String.IsNullOrWhiteSpace(session.Procedure))
                throw new InvalidOperationException("--procedure is mandatory");
        }

        static String[] ParseValues(String valuesString)
        {
            var index = 0;
            while (valuesString[index] == ' ')
                index++;

            if(valuesString[index] == '[')
            {
                return ParseArray(valuesString, index);
            }
            else
            {
                return new String[] { valuesString };
            }
        }

        internal static String[] ParseArray(String valuesString, Int32 index)
        {
            var list = new List<String>();
            String current = String.Empty;
            Char? context = null;
            for (; index < valuesString.Length; index++)
            {
                var c = valuesString[index];
                if (c == '"' || c == '\'')
                {
                    if (context.HasValue)
                    {
                        if (context.Value == c)
                        {
                            context = null;
                            continue;
                        }
                    }
                    else
                    {
                        context = c;
                        continue;
                    }
                }
                else if (!context.HasValue)
                {
                    if (c == ']' || c == ',')
                    {
                        list.Add(current);
                        current = String.Empty;
                        continue;
                    }
                    else if(c== '[' || c == ' ')
                    {
                        continue;
                    }
                }

                current += c;
            }

            if (!String.IsNullOrWhiteSpace(current))
                throw new InvalidOperationException("Array parameter ended unexpectedly: " + valuesString);

            return list.ToArray();
        }
    }
}
