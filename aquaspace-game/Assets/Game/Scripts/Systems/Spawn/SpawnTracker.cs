public class SpawnTracker
{
    private int fishCount;
    private int trashCount;

    public bool CanSpawn(string tag, int max)
    {
        return GetCount(tag) < max;
    }

    public void Register(string tag)
    {
        if (tag == "Fish") fishCount++;
        else if (tag == "Trash") trashCount++;
    }

    public void Unregister(string tag)
    {
        if (tag == "Fish") fishCount--;
        else if (tag == "Trash") trashCount--;
    }

    public int GetCount(string tag)
    {
        return tag == "Fish" ? fishCount : trashCount;
    }
}