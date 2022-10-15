using Newtonsoft.Json.Bson;

public interface IGameWorldMainThreadInteractions
{
    public void Player_Moved(object sender, ChunkCoord args);
    public void Start();
    public void Update();
}

