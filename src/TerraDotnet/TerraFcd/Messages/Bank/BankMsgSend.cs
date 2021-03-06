using System.Text.Json.Serialization;

namespace TerraDotnet.TerraFcd.Messages.Bank;

public record BankMsgSend : IMsg
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("value")]
    public BankMsgSendValue Value { get; set; }
}