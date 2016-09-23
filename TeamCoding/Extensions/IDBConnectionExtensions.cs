using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.Extensions
{
    public static class IDBConnectionExtensions
    {
        public static int ExecuteWithLogging(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?), [CallerFilePath] string filePath = null, [CallerMemberName] string memberName = null)
        {
            if(cnn == null)
            {
                TeamCodingPackage.Current.Logger.WriteError("Tried to run sql with no connection", null, filePath, memberName);
                return -1;
            }
            try
            {
                return Dapper.SqlMapper.Execute(cnn, sql, param, transaction, commandTimeout, commandType);
            }
            catch(Exception ex)
            {
                TeamCodingPackage.Current.Logger.WriteError(ex, filePath, memberName);
                return -1;
            }
        }
    }
}
