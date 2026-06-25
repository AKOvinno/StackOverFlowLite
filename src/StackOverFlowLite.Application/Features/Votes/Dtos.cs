using StackOverflowLite.Domain.Enums;

namespace StackOverflowLite.Application.Features.Votes;

public record VoteRequestDto(VoteType VoteType);
public record VoteResultDto(int NewScore);
