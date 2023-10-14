using FreeSql.DataAnnotations;
using PiPlayer.Models.Enums;

namespace PiPlayer.Models.Entities
{
    [Table(Name = "sys_media")]
    [Index("index_{TableName}_" + nameof(FileName), nameof(FileName), false)]
    public class Media : IBaseEntity
    {
        [Column(StringLength = 255, IsNullable = false)]
        public string FileName { get; set; }

        [Column(IsIgnore = true)]
        public string FileUrl { get; set; }

        [Column(StringLength = 255, IsNullable = false)]
        public string FileOldName { get; set; }

        [Column(StringLength = 1000, IsNullable = false)]
        public string Path { get; set; }

        [Column(StringLength = 300)]
        public string LogoUrl { get; set; }

        public int? Duration { get; set; } = 0;

        [Column(StringLength = 30, IsNullable = true)]
        public string Extension { get; set; }

        public FileType FileType { get; set; }

        public int Height { get; set; } = 0;

        public int Width { get; set; } = 0;

        public float Size { get; set; }

        [Column(StringLength = 1000, IsNullable = true)]
        public string Remark { get; set; }
    }
}
