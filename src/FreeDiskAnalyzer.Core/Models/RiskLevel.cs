namespace FreeDiskAnalyzer.Core.Models;

/// <summary>
/// How much a user might regret a given deletion, not how dangerous it is
/// to the operating system (everything offered for deletion in this app is
/// already scoped to be safe for the system, see PathSafetyGuard). Low:
/// nothing meaningful is lost (empty folder, regenerable cache, a
/// guaranteed-redundant duplicate). Medium: minor inconvenience (signed out
/// of sites). High: something genuinely gone that some people want to keep
/// (browsing history).
/// </summary>
public enum RiskLevel
{
    Low,
    Medium,
    High
}
