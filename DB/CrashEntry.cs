using System;
using System.ComponentModel.DataAnnotations;

namespace vpn_crash.DB
{
    public class CrashEntry
    {
        [Key]
        public ulong MessageId { get; set; }
        public DateTime Date { get; set; }
        public string User { get; set; }
    }

}