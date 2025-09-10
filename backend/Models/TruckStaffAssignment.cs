

using System; 
using System.Collections.Generic; 
namespace backend.Models;
 public partial class TruckStaffAssignment { 
    public int AssignmentId { get; set; } 
    public int TruckId { get; set; } 
    public int StaffId { get; set; } 
    public string Role { get; set; } = null!; 
    public DateOnly DateAssigned { get; set; } 
    public virtual Truck Truck { get; set; } = null!; }