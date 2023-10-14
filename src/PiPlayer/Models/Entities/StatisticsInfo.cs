using FreeSql.DataAnnotations;

namespace PiPlayer.Models.Entities
{
    [Table(Name = "sys_statistics_info")]
    public class StatisticsInfo : IBaseEntity
    {
        /// <summary>
        /// 总计播放时长
        /// </summary>
        public long TotalDuration { get; set; }
    }
}
