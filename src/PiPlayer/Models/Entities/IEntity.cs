﻿using FreeSql.DataAnnotations;
using System.ComponentModel;

namespace PiPlayer.Models.Entities
{
    public interface IEntity
    {

    }


    public abstract class IEntity<TKey> : IEntity where TKey : struct
    {
        /// <summary>
        /// Id
        /// </summary>
        [Description("Id")]
        [Column(IsPrimary = true, Position = 1)]
        public virtual TKey Id { get; set; }
    }
}
