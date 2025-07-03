using GradProject.Models.Entities;  // <-- brings in your LabStatus enum


public class ProgressDto 
{
    public string? LabName  { get; set; } // Made nullable
    public DateTime Date   { get; set; }
    public int    Score    { get; set; }
    public LabStatus Status   { get; set; }
}
