namespace VetCare.Api.Contracts.System;

public sealed record ApiInfoResponse(
    string Name,
    string Version,
    string Status,
    string Documentation,
    string Health
);
