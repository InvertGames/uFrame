

public interface IJsonSerializable
{
    void Deserialize(JSONNode node);

    JSONNode Serialize();
}