﻿using SSCMS.Enums;

namespace SSCMS.Models
{
    public class TemplateSummary
    {
        public int Id { get; set; }
        public string TemplateName { get; set; }
        public TemplateType TemplateType { get; set; }
        public bool DefaultTemplate{ get; set; }
    }
}
