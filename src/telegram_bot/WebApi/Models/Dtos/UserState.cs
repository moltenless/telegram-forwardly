using System.Text.Json.Serialization;

namespace TelegramForwardly.WebApi.Models.Dtos
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
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
