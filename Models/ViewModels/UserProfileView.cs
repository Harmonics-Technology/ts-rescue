using System;
using System.Collections.Generic;

namespace TimesheetBE.Models.ViewModels {
    public class UserProfileView {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public string ProfilePictureUrl { get; set; }
    }
}
