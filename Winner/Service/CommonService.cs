using Naver.SearchAd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winner
{
    class CommonService
    {
        public static SQLite SQL = new SQLite();

        internal static string GetRandomKeyword()
        {
            List<Keyword> keywords = SQL.SelectAllKeyword();
            return keywords.ElementAt(CommonUtils.GetRandomValue(0, keywords.Count - 1)).keyword;
        }
    }
}
