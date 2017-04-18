using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Herms.Data.Extensions
{
    public static class SqlConnectionExtensions
    {
        public static int ExecuteCommand(this SqlConnection connection, string commandText)
        {
            using (var command = new SqlCommand(commandText, connection))
            {
                return command.ExecuteNonQuery();
            }
        }

        public static int ExecuteParameterizedCommand(this SqlConnection connection, string commandText,
            Dictionary<string, object> parameters)
        {
            using (var command = new SqlCommand(commandText, connection))
            {
                foreach (var key in parameters.Keys)
                {
                    command.Parameters.AddWithValue(key, parameters[key]);
                }
                return command.ExecuteNonQuery();
            }
        }

        public static List<T> Query<T>(this SqlConnection connection, string query, Func<SqlDataReader, T> mapFunc)
            where T : class
        {
            using (var command = new SqlCommand(query, connection))
            {
                return Query(command, mapFunc);
            }
        }

        public static T QuerySingle<T>(this SqlConnection connection, string query, Func<SqlDataReader, T> mapFunc)
            where T : class
        {
            using (var command = new SqlCommand(query, connection))
            {
                return QuerySingle(command, mapFunc);
            }
        }

        public static T QuerySingleWithParameters<T>(this SqlConnection connection, string query,
            Dictionary<string, object> parameters, Func<SqlDataReader, T> mapFunc)
            where T : class
        {
            using (var command = new SqlCommand(query, connection))
            {
                AddParametersToCommand(parameters, command);
                return QuerySingle(command, mapFunc);
            }
        }

        public static Dictionary<TKey, TValue> QueryToDictionary<TKey, TValue>(this SqlConnection connection,
            string query,
            Func<SqlDataReader, TKey> idFunc, Func<SqlDataReader, TValue> valueFunc)
        {
            using (var command = new SqlCommand(query, connection))
            {
                return QueryToDictionary(command, idFunc, valueFunc);
            }
        }

        public static List<T> QueryWithParameters<T>(this SqlConnection connection, string query,
            Dictionary<string, object> parameters, Func<SqlDataReader, T> mapFunc)
            where T : class
        {
            using (var command = new SqlCommand(query, connection))
            {
                AddParametersToCommand(parameters, command);
                return Query(command, mapFunc);
            }
        }

        public static List<T> QueryWithParametersAndInClause<T, TIn>(this SqlConnection connection, string query,
            Dictionary<string, object> parameters, Tuple<string, IEnumerable<TIn>> inClause, Func<SqlDataReader, T> mapFunc)
            where T : class
        {
            using (var command = new SqlCommand(query, connection))
            {
                AddParametersToCommand(parameters, command);
                AddInClause(inClause, command);
                return Query(command, mapFunc);
            }
        }

        private static void AddInClause<T>(Tuple<string, IEnumerable<T>> inClause, SqlCommand command)
        {
            command.AddArrayParameters(inClause.Item1, inClause.Item2);
        }

        private static void AddParametersToCommand(Dictionary<string, object> parameters, SqlCommand command)
        {
            foreach (var key in parameters.Keys)
            {
                var value = parameters[key];
                command.Parameters.AddWithValue(key, value);
            }
        }

        private static List<T> Query<T>(SqlCommand command, Func<SqlDataReader, T> mapFunc) where T : class
        {
            using (var reader = command.ExecuteReader())
            {
                return ReadList(reader, mapFunc);
            }
        }

        private static T QuerySingle<T>(SqlCommand command, Func<SqlDataReader, T> mapFunc) where T : class
        {
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                    return mapFunc(reader);
            }
            return null;
        }

        private static Dictionary<TKey, TValue> QueryToDictionary<TKey, TValue>(SqlCommand command,
            Func<SqlDataReader, TKey> idFunct, Func<SqlDataReader, TValue> valueFunc)
        {
            using (var reader = command.ExecuteReader())
            {
                return ReadDictionary(reader, idFunct, valueFunc);
            }
        }

        private static Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>(SqlDataReader reader,
            Func<SqlDataReader, TKey> idFunc, Func<SqlDataReader, TValue> valueFunc)
        {
            var list = new Dictionary<TKey, TValue>();
            while (reader.Read())
            {
                var key = idFunc(reader);
                var value = valueFunc(reader);
                list.Add(key, value);
            }
            return list;
        }

        private static List<T> ReadList<T>(SqlDataReader reader, Func<SqlDataReader, T> mapFunc) where T : class
        {
            var list = new List<T>();
            while (reader.Read())
            {
                var item = mapFunc(reader);
                list.Add(item);
            }
            return list;
        }
    }
}