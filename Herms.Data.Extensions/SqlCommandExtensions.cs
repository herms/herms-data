using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Herms.Data.Extensions
{
    public static class SqlCommandExtensions
    {
        /// <summary>
        /// Found at: http://stackoverflow.com/a/2377651
        /// Created by: http://stackoverflow.com/users/320/brian
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmd"></param>
        /// <param name="paramNameRoot"></param>
        /// <param name="values"></param>
        /// <param name="start"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static SqlParameter[] AddArrayParameters<T>(this SqlCommand cmd, string paramNameRoot,
            IEnumerable<T> values,
            int start = 1, string separator = ", ")
        {
            /* An array cannot be simply added as a parameter to a SqlCommand so we need to loop through things and add it manually. 
             * Each item in the array will end up being it's own SqlParameter so the return value for this must be used as part of the
             * IN statement in the CommandText.
             */
            var parameters = new List<SqlParameter>();
            var parameterNames = new List<string>();
            var paramNbr = start;
            foreach (var value in values)
            {
                var paramName = string.Format("@{0}{1}", paramNameRoot, paramNbr++);
                parameterNames.Add(paramName);
                parameters.Add(cmd.Parameters.AddWithValue(paramName, value));
            }

            cmd.CommandText = cmd.CommandText.Replace("{" + paramNameRoot + "}", string.Join(separator, parameterNames));

            return parameters.ToArray();
        }
    }
}