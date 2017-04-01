using System.Collections.Generic;

namespace ElasticCommon.Models
{
    public class SearchRequest
    {
        public int PageSize { get; set; }

        public int CurrentPage { get; set; }

        public double MinScore { get; set; }

        public string Query { get; set; }

        // dictionary of field => [value1, value2, ... valueN]
        //
        // Best fields for filters:
        //
        // tagGradeLevel  (numeric -1, 0, 1 ... 12)
        // tagResources
        // tagLanguage
        // tagEla
        // tagThemes
        // tagSubjectsPrefixed
        // tagSubjects
        // tagSpeciality
        // tagStrategy
        //
        // and you can still filter on tmplTags but the matching for that field
        public Dictionary<string, List<string>> Filters { get; set; }

        public bool IsSortBySmily { get; set; }
    }
}