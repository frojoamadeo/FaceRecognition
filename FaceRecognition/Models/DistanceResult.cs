using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FaceRecognition.Models
{
    public class DistanceResult
    {
        Guid DistanceResultId { get; set; }
        Guid EmployeeId { get; set; }
        string Label { get; set; }


        public virtual Employee employee { get; set; }
    }
}