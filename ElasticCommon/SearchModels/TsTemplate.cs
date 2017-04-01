using System;
using System.Collections.Generic;
using ElasticCommon.Converter;
using Nest;
using Newtonsoft.Json;

namespace ElasticCommon.SearchModels
{
    [ElasticsearchType(IdProperty = "id", Name = "ts_template")]
    public class TsTemplate
    {
        public TsTemplate()
        {
            this.TmplTags = new List<string>();
            this.TmplTypes = new List<string>();
            this.TmplCcss = new List<string>();
            this.TagGradeLevel = new List<int>();
            this.TagLanguage = new List<string>();
            this.TagResources = new List<string>();
            this.TagEla = new List<string>();
            this.TagThemes = new List<string>();
            this.TagSubjectsPrefixed = new List<string>();
            this.TagSubjects = new List<string>();
            this.TagSpeciality = new List<string>();
            this.TagStrategy = new List<string>();
        }


        [Keyword(Name = "id")]
        public string Id { get; set; }


        [Text(Name = "title", Analyzer = "en_base_analysis")]
        public string Title { get; set; }


        [Text(Name = "desc", Analyzer = "en_base_analysis")]
        public string Desc { get; set; }


        [Keyword(Name = "tmplUri", Normalizer = "base_normalizer")]
        public string TmplUri { get; set; }


        [Text(Name = "by", Analyzer = "en_base_analysis")]
        public string By { get; set; }


        [Keyword(Name = "byUri", Normalizer = "base_normalizer")]
        public string ByUri { get; set; }


        [Text(Name = "schlDist", Analyzer = "en_base_analysis")]
        public string SchlDist { get; set; }


        [Keyword(Name = "afmcCode", Normalizer = "base_normalizer")]
        public string AfmcCode { get; set; }

        [Text(Name = "tmplTags", Analyzer = "en_base_analysis")]
        public List<string> TmplTags { get; set; }

        [Text(Name = "tmplCcss", Analyzer = "en_base_analysis")]
        public List<string> TmplCcss { get; set; }


        [Text(Name = "tmplTypes", Analyzer = "en_base_analysis")]
        public List<string> TmplTypes { get; set; }


        [Keyword(Name = "insTmplId", Normalizer = "base_normalizer")]
        public string InsTmplId { get; set; }


        [Text(Name = "insAuthor", Analyzer = "en_base_analysis")]
        public string InsAuthor { get; set; }

        [Text(Name = "spelldata", Analyzer = "en_base_analysis")]
        public string SpellData { get; set; }


        [Keyword(Name = "insAfmcCode", Normalizer = "base_normalizer")]
        public string InsAfmcCode { get; set; }


        [Keyword(Name = "insAutherUrl", Normalizer = "base_normalizer")]
        public string InsAutherUrl { get; set; }


        [Keyword(Name = "authorId", Normalizer = "base_normalizer")]
        public string AuthorId { get; set; }


        [Keyword(Name = "tmplCode", Normalizer = "base_normalizer")]
        public string TmplCode { get; set; }


        [Keyword(Name = "isFeatured")]
        public string IsFeatured { get; set; }

        [Boolean(Name = "deleted")]
        [JsonConverter(typeof(BoolConverter))]
        public bool Deleted { get; set; }

        [Date(Name = "lstDt")]
        public DateTimeOffset? LstDt { get; set; }

        [Number(NumberType.Integer, Coerce = true, Name = "downloadCnt")]
        public int DownloadCnt { get; set; }

        [Number(NumberType.Integer, Coerce = true, Name = "clonedCnt")]
        public int ClonedCnt { get; set; }

        [Number(NumberType.Integer, Coerce = true, Name = "smileyCnt")]
        public int SmileyCnt { get; set; }

        [Keyword(Name = "escTitle")]
        public string EscTitle { get; set; }

        public double Score { get; set; }

        [Boolean(Name = "isUploaded")]
        public bool IsUploaded { get; set; }


        [Boolean(Name = "isLandscape")]
        public bool IsLandscape { get; set; }

        [Keyword(Name = "tagGradeLevel", Normalizer = "base_normalizer")]
        public List<int> TagGradeLevel { get; set; }

        [Keyword(Name = "tagResources", Normalizer = "base_normalizer")]
        public List<string> TagResources { get; set; }

        [Keyword(Name = "tagLanguage", Normalizer = "base_normalizer")]
        public List<string> TagLanguage { get; set; }

        [Keyword(Name = "tagEla", Normalizer = "base_normalizer")]
        public List<string> TagEla { get; set; }

        [Keyword(Name = "tagThemes", Normalizer = "base_normalizer")]
        public List<string> TagThemes { get; set; }

        [Keyword(Name = "tagSubjectsPrefixed", Normalizer = "base_normalizer")]
        public List<string> TagSubjectsPrefixed { get; set; }

        [Keyword(Name = "tagSubjects", Normalizer = "base_normalizer")]
        public List<string> TagSubjects { get; set; }

        [Keyword(Name = "tagSpeciality", Normalizer = "base_normalizer")]
        public List<string> TagSpeciality { get; set; }

        [Keyword(Name = "tagStrategy", Normalizer = "base_normalizer")]
        public List<string> TagStrategy { get; set; }
    }
}