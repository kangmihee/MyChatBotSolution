using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChatBotApplication.Models
{
    [Serializable]
    public class ReservationModel
    {
        public string MemberName { get; set; }
        public string DesignerName { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string Telephone { get; set; }
        public string Sample2 { get; set; }
        public string Sample3 { get; set; }
        public string Sample4 { get; set; }
        public string Menu { get; internal set; }
    }
}