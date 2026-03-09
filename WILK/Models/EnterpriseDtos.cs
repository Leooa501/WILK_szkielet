namespace WILK.Models
{
    // Data transfer objects for enterprise database operations

    // Component DTOs
    public record ComponentDto
    {
        public int Id { get; init; }
        public int RId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
        public int Quantity { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    // Reservation DTOs
    public record ReservationDto
    {
        public int Id { get; init; }
        public DateTime Date { get; init; }
        public string ShiftTime { get; init; } = string.Empty;
        public string Machine { get; init; } = string.Empty;
        public string Location { get; init; } = string.Empty;
        public string Items { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
        public string Comments { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public record ReservationSummaryDto
    {
        public int Id { get; init; }
        public DateTime Date { get; init; }
        public string ShiftTime { get; init; } = string.Empty;
        public string Machine { get; init; } = string.Empty;
        public string Location { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
    }

    public record CreateReservationRequest
    {
        public DateTime Date { get; init; }
        public string? ShiftTime { get; init; }
        public string? Machine { get; init; }
        public string? Location { get; init; }
        public string? Items { get; init; }
        public string? Type { get; init; }
        public string? Comments { get; init; }
    }

    public record UpdateReservationRequest
    {
        public int Id { get; init; }
        public DateTime Date { get; init; }
        public string? ShiftTime { get; init; }
        public string? Machine { get; init; }
        public string? Location { get; init; }
        public string? Items { get; init; }
        public string? Type { get; init; }
        public string? Comments { get; init; }
    }

    // Alternative DTOs
    public record AlternativeDto
    {
        public int ReservationId { get; init; }
        public string ComponentName { get; init; } = string.Empty;
        public string AlternativeName { get; init; } = string.Empty;
        public string Location { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public record CreateAlternativeRequest
    {
        public int ReservationId { get; init; }
        public string? ComponentName { get; init; }
        public string? AlternativeName { get; init; }
        public string? Location { get; init; }
    }

    public record UpdateAlternativeRequest
    {
        public int ReservationId { get; init; }
        public string? ComponentName { get; init; }
        public string? AlternativeName { get; init; }
        public string? Location { get; init; }
    }

    // WarmUp DTOs
    public record WarmUpDto
    {
        public int ReservationId { get; init; }
        public string ComponentName { get; init; } = string.Empty;
        public string WarmUpName { get; init; } = string.Empty;
        public string Location { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public record CreateWarmUpRequest
    {
        public int ReservationId { get; init; }
        public string? ComponentName { get; init; }
        public string? WarmUpName { get; init; }
        public string? Location { get; init; }
    }

    public record UpdateWarmUpRequest
    {
        public int ReservationId { get; init; }
        public string? ComponentName { get; init; }
        public string? WarmUpName { get; init; }
        public string? Location { get; init; }
    }

    // Location DTOs
    public record LocationUsageDto
    {
        public string Location { get; init; } = string.Empty;
        public int UsageCount { get; init; }
        public DateTime FirstUsed { get; init; }
        public DateTime LastUsed { get; init; }
    }

    public record LocationDetailDto
    {
        public string Location { get; init; } = string.Empty;
        public int TotalReservations { get; init; }
        public DateTime? FirstReservation { get; init; }
        public DateTime? LastReservation { get; init; }
        public int DaysUsed { get; init; }
        public List<string> ContainerRange { get; init; } = new();
        public bool IsConfigured { get; init; }
    }

    // Result pattern for safe database operation returns
    public class DatabaseResult<T>
    {
        public bool IsSuccess { get; init; }
        public T? Data { get; init; }
        public string? ErrorMessage { get; init; }
        public Exception? Exception { get; init; }

        private DatabaseResult() { }

        public static DatabaseResult<T> Success(T data) => new() { IsSuccess = true, Data = data };
        
        public static DatabaseResult<T> Failure(string errorMessage, Exception? exception = null) => 
            new() { IsSuccess = false, ErrorMessage = errorMessage, Exception = exception };
    }

    public class DatabaseResult
    {
        public bool IsSuccess { get; init; }
        public string? ErrorMessage { get; init; }
        public Exception? Exception { get; init; }

        private DatabaseResult() { }

        public static DatabaseResult Success() => new() { IsSuccess = true };
        
        public static DatabaseResult Failure(string errorMessage, Exception? exception = null) => 
            new() { IsSuccess = false, ErrorMessage = errorMessage, Exception = exception };
    }
}