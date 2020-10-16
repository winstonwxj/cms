﻿using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SSCMS.Services
{
    public partial interface IParseManager
    {
        Task ParseTemplateContentAsync(StringBuilder parsedBuilder);

        Task<string> ParseTemplatePreviewAsync(string template);

        Task ParseInnerContentAsync(StringBuilder builder);

        string StlEncrypt(string stlElement);

        Task ReplacePageElementsInContentPageAsync(StringBuilder parsedBuilder, List<string> labelList, int currentPageIndex, int pageCount);

        Task ReplacePageElementsInChannelPageAsync(StringBuilder parsedBuilder, List<string> labelList, int currentPageIndex, int pageCount, int totalNum);

        Task ReplacePageElementsInSearchPageAsync(StringBuilder parsedBuilder, List<string> labelList, string ajaxDivId, int currentPageIndex, int pageCount, int totalNum);

        Task ReplacePageElementsInDynamicPageAsync(StringBuilder parsedBuilder, List<string> labelList, int currentPageIndex, int pageCount, int totalNum, bool isPageRefresh, string ajaxDivId);
    }
}
