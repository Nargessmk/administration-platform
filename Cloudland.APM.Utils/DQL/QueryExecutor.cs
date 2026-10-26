using System.Data;
using Dapper;

namespace Cloudland.APM.Utils.DQL;

public class QueryExecutor(IDbConnection connection)
{
    public IEnumerable<dynamic> Execute(string sql)
    {
        return connection.Query(sql);
    }
}