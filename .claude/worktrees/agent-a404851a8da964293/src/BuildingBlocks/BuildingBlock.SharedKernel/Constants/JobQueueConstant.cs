namespace NovaCore.BuildingBlock.SharedKernel.Constants;

public static class JobQueueConstant
{
    public const string DEFAULT = "default";
    public const string MAIL = "mail";
    public const string JOB_SCHEDULE = "job_schedule";

    public readonly static string[] SUPPORTED_JOB_QUEUE = [DEFAULT, MAIL];
}