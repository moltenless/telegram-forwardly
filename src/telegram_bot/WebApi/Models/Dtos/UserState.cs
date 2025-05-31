namespace TelegramForwardly.WebApi.Models.Dtos
{
    public enum UserState
    {
        Idle,                     
        AwaitingPhoneNumber,      
        AwaitingApiId,            
        AwaitingApiHash,          
        AwaitingVerificationCode, 
        AwaitingPassword,         
        AwaitingEnableAllChats,   
        AwaitingChats,            
        AwaitingKeywords,         
        AwaitingForumGroup,      
        AwaitingGroupingType,    
        AwaitingEnableLoggingTopic
    }
}
