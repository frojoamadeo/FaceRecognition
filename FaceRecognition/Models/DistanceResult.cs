using System;
using System.Collections.Generic;

namespace FaceRecognition.Models
{
    public partial class DistanceResult
    {
        public int distanceResultId { get; set; }
        public int employeeId { get; set; }
        //public int? label { get; set; }
        public double? distance { get; set; }
        public string photoName { get; set; }
        public string algorithm { get; set; }
        public virtual Employee Employee { get; set; }
    }
}
