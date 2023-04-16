﻿using System;
using System.Collections.Generic;

namespace TimesheetBE.Models.ViewModels
{
    public class UsersShiftView
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; }
        public int TotalHours { get; set; }
        public List<ShiftView> Shift { get; set; }
    }
}