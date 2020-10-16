using System;
using SSCMS.Enums;

namespace SSCMS.Dto
{
    public class CreateTaskLog
    {
        public CreateTaskLog(int id, CreateType createType, int siteId, int channelId, int contentId, int fileTemplateId, int specialId, string taskName, string timeSpan, bool isSuccess, string errorMessage, DateTime addDate)
        {
            Id = id;
            CreateType = createType;
            SiteId = siteId;
            ChannelId = channelId;
            ContentId = contentId;
            FileTemplateId = fileTemplateId;
            SpecialId = specialId;
            TaskName = taskName;
            TimeSpan = timeSpan;
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            AddDate = addDate;
        }

        public int Id { get; set; }

        public CreateType CreateType { get; set; }

        public int SiteId { get; set; }

        public int ChannelId { get; set; }

        public int ContentId { get; set; }

        public int FileTemplateId { get; set; }

        public int SpecialId { get; set; }

        public string TaskName { get; set; }

        public string TimeSpan { get; set; }

        public bool IsSuccess { get; set; }

        public string ErrorMessage { get; set; }

        public DateTime AddDate { get; set; }
    }
}
