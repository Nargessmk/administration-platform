namespace Cloudland.APM.DTO.NodeObjects;

public enum ProcessStepActionType
{
    شروع = 0,
    عملیات = 1,
    شرط = 2,
    پایان = 3,
    خالی = 4,
    SendTask = 5,
    ReceiveTask = 6,
    ScriptTask = 7,
    UserTask = 8,
    ServiceTask = 9,
    BusinessRuleTask = 10,
    ManualTask = 11
}