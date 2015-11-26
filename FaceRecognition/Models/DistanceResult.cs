using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace FaceRecognition.Models
{
    public class DistanceResult
    {
        [Key]
        public int distanceResultId { get; set; }
        public int employeeId { get; set; }
        public string label { get; set; }
        public string photoPath { get; set; }

        public virtual Employee employee { get; set; }
    }
}