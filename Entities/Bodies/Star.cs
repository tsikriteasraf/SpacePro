﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Bodies
{
    public class Star
    {
        public Star()
        {
            BodyImages = new HashSet<BodyImage>();
        }
        public int StarId { get; set; }
        [Required]
        public string Name { get; set; }
        [Display(Name = "Mass")]
        public decimal? MassValue { get; set; }
        [Display(Name = "Volume")]
        public decimal? VolValue { get; set; }
        public string Dimension { get; set; }
        public string DiscoveredBy { get; set; }
        public DateTime? DiscoveryDate { get; set; }

        //Navigation Properties
        public ICollection<BodyImage> BodyImages { get; set; }

    }
}
