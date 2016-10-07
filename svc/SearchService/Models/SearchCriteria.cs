﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SearchService.Models
{

    public class SearchCriteria : ISearchCriteria
    {
        [Required]
        public string Query { get; set; }

        [Required]
        public IEnumerable<int> ProjectIds { get; set; }

        public IEnumerable<int> ItemTypeIds { get; set; }
    }
}