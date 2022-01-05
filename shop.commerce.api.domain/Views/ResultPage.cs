using System;
using System.Collections.Generic;
using System.Linq;

namespace shop.commerce.api.domain.Views
{
    public class ResultPage<T>
    {
        public int TotalPage { get; set; }
        public int PageNumber { get; set; }
        public int TotalCount { get; set; }
        public IEnumerable<T> List { get; set; }

        public static ResultPage<T> PageData(IEnumerable<T> list, int pageNumber, int rowsOfPage, int totalCount, Func<T, bool> filter = null)
        {
            int totalpages = 0;
            if (totalCount > 0)
            {
                rowsOfPage = rowsOfPage > 0 ? rowsOfPage : 10;
                rowsOfPage = rowsOfPage > totalCount ? totalCount : rowsOfPage;
                totalpages = (totalCount % rowsOfPage) == 0 ? totalCount / rowsOfPage : (totalCount/ rowsOfPage) + 1;
                if (pageNumber > totalpages)
                {
                    pageNumber = totalpages;
                }
            }
            int skip = (pageNumber - 1) * rowsOfPage;
            if (filter != null)
            {
                var data = new ResultPage<T>
                {
                    PageNumber = pageNumber,
                    TotalPage = totalpages,
                    TotalCount = totalCount,
                    List = list.Where(filter).Skip(skip).Take(rowsOfPage).ToList()
                };
                return data;
            }
            else
            {
                var data = new ResultPage<T>
                {
                    PageNumber = pageNumber,
                    TotalPage = totalpages,
                    TotalCount = totalCount,
                    List = list.Skip(skip).Take(rowsOfPage).ToList()
                };
                return data;
            }
        }
    }

}
