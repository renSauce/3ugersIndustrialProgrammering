namespace SystemLogin.Domain;
/// <summary>
/// Represents a customer bin (a destination) that accepts blocks of a specific color.
/// </summary>
/// <param name="Id">The unique identifier of the customer bin.</param>
/// <param name="CustomerName">The name of the customer associated with the bin.</param>
/// <param name="AcceptedColor">The color of blocks that this bin accepts.</param>
public sealed record CustomerBin(
    string Id,
    string CustomerName,
    BlockColor AcceptedColor
);
