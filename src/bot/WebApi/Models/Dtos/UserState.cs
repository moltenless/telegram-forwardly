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
        AwaitingSessionString, 
        AwaitingEnableAllChats,   
        AwaitingChats,            
        AwaitingKeywords,         
        AwaitingForumGroup,      
        AwaitingGroupingType,
        AwaitingDeleteConfirmation,
        AwaitingRemoveChats,
        AwaitingRemoveKeywords,
        AwaitingThresholdCharsCount
    }
}
