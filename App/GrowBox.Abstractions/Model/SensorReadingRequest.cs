namespace GrowBox.Abstractions.Model;

public record SensorReadingRequest(string[] Types, Guid GrowBoxId, DateTime? From, DateTime? To);